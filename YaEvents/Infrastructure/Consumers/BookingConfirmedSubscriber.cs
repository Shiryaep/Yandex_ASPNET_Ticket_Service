using Application.Repositories;
using Confluent.Kafka;
using Infrastructure.HostedServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using YaContracts;

namespace Infrastructure.Consumers;

/// <summary>
/// Фоновый сервис, который подписывается на топик booking-confirmed
/// и обрабатывает события подтверждения бронирования.
/// </summary>
public class BookingConfirmedSubscriber : BackgroundService
{
    private readonly ILogger<BookingConfirmedSubscriber> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly KafkaConsumerSettings _settings;
    private readonly IConsumer<string, string> _consumer;

    public BookingConfirmedSubscriber(
        ILogger<BookingConfirmedSubscriber> logger,
        IServiceProvider serviceProvider,
        IOptions<KafkaConsumerSettings> settings,
        IConsumer<string, string> consumer)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _settings = settings.Value;
        _consumer = consumer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Подписчик BookingConfirmed запущен. Топик: {TopicName}, Группа: {ConsumerGroup}",
            _settings.Topics.BookingConfirmed, _settings.ConsumerGroup);

        _consumer.Subscribe(_settings.Topics.BookingConfirmed);

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

        try
        {
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

            _logger.LogInformation("Успешно обновлено количество мест для события {EventId}. Новое значение: {NewSeats}",
                @event.EventId, domainEvent.AvailableSeats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке события BookingConfirmed для EventId={EventId}", @event.EventId);
        }
    }

    public override void Dispose()
    {
        _consumer?.Dispose();
        base.Dispose();
    }
}