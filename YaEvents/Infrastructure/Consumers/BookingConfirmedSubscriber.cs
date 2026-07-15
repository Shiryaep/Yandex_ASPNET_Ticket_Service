using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using YaContracts;
using YaEvents.Application.Repositories;
using YaEvents.Application.Services;
using YaEvents.Domain;
using YaEvents.Infrastructure.Settings;

namespace YaEvents.Infrastructure.Consumers;

/// <summary>
/// Фоновый сервис, который подписывается на топик booking-confirmed
/// и обрабатывает события подтверждения бронирования.
/// </summary>
public class BookingConfirmedSubscriber : BackgroundService
{
    private readonly ILogger<BookingConfirmedSubscriber> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConsumer<string, string> _consumer;
    private readonly KafkaConsumerSettings _settingsKafka;

    public BookingConfirmedSubscriber(
        ILogger<BookingConfirmedSubscriber> logger,
        IServiceProvider serviceProvider,
        IConsumer<string, string> consumer,
        IOptions<KafkaConsumerSettings> settingsKafka)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _consumer = consumer;
        _settingsKafka = settingsKafka.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Подписчик BookingConfirmed запущен. Топик: {TopicName}, Группа: {ConsumerGroup}",
            _settingsKafka.Topics.BookingConfirmed, _settingsKafka.ConsumerGroup);

        _consumer.Subscribe(_settingsKafka.Topics.BookingConfirmed);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(stoppingToken);

                    if (consumeResult?.Message == null)
                    {
                        continue;
                    }

                    _logger.LogDebug("Получено сообщение из топика {Topic}, Partition: {Partition}, Offset: {Offset}",
                        consumeResult.Topic, consumeResult.Partition.Value, consumeResult.Offset.Value);

                    await ProcessMessageAsync(consumeResult.Message, stoppingToken);

                    _consumer.Commit(consumeResult);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Ошибка при чтении сообщения из Kafka: {Error}", ex.Error.Reason);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Подписчик останавливается...");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Неожиданная ошибка при обработке сообщения");
                }
            }
        }
        finally
        {
            _consumer.Close();
            _logger.LogInformation("Подписчик BookingConfirmed остановлен");
        }
    }

    private async Task ProcessMessageAsync(Message<string, string> message, CancellationToken cancellationToken)
    {
        BookingConfirmed? @event;
        try
        {
            @event = JsonSerializer.Deserialize<BookingConfirmed>(message.Value);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Не удалось десериализовать сообщение: {MessageValue}", message.Value);
            return;
        }

        if (@event == null)
        {
            _logger.LogWarning("Получено пустое событие");
            return;
        }

        _logger.LogInformation("Обработка события BookingConfirmed: BookingId={BookingId}, EventId={EventId}, TicketsCount={TicketsCount}",
            @event.BookingId, @event.EventId, @event.SeatsCount);

        using var scope = _serviceProvider.CreateScope();
        var eventsRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();
        var processedBookingsRepository = scope.ServiceProvider.GetRequiredService<IProcessedBookingsRepository>();
        var cacheInvalidator = scope.ServiceProvider.GetRequiredService<ICacheInvalidator>();

        try
        {
            var processedBooking = new ProcessedBooking(
                @event.BookingId,
                @event.EventId,
                @event.SeatsCount,
                @event.ConfirmedAt
            );

            var isNewBooking = await processedBookingsRepository.TryMarkAsProcessedAsync(
                processedBooking,
                cancellationToken);

            if (!isNewBooking)
            {
                _logger.LogWarning(
                    "Дубликат сообщения для BookingId={BookingId}. Пропускаем обработку",
                    @event.BookingId);
                return;
            }

            var domainEvent = await eventsRepository.GetEventByIdAsync(@event.EventId, cancellationToken);

            if (domainEvent == null)
            {
                _logger.LogWarning("Событие с ID {EventId} не найдено в БД. Пропускаем сообщение", @event.EventId);
                return;
            }

            if (domainEvent.AvailableSeats < @event.SeatsCount)
            {
                _logger.LogWarning(
                    "Недостаточно мест для события {EventId}. Доступно: {AvailableSeats}, Запрошено: {SeatsCount}. Пропускаем",
                    @event.EventId, domainEvent.AvailableSeats, @event.SeatsCount);
                return;
            }

            domainEvent.TryReserveSeats(@event.SeatsCount);

            await eventsRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Успешно зарезервировано {SeatsCount} мест для события {EventId}. Осталось: {AvailableSeats}",
                @event.SeatsCount, @event.EventId, domainEvent.AvailableSeats);

            await cacheInvalidator.UpdateEventInCacheAsync(domainEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке BookingId={BookingId}, EventId={EventId}", @event.BookingId, @event.EventId);
        }
    }

    public override void Dispose()
    {
        _consumer?.Dispose();
        base.Dispose();
    }
}