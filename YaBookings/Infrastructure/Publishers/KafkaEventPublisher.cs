using Application.Publishers;
using Confluent.Kafka;
using System.Text.Json;

namespace Infrastructure.Publishers;

public class KafkaEventPublisher : IDomainEventPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;

    // Инжектируем готовый продюсер через DI
    public KafkaEventPublisher(IProducer<string, string> producer)
    {
        _producer = producer;
    }

    public async Task PublishAsync<T>(string topic, T @event, string key, CancellationToken cancellationToken = default)
    {
        // 1. Сериализуем событие в JSON
        var jsonValue = JsonSerializer.Serialize(@event);

        // 2. Формируем сообщение. Key = EventId (гарантирует порядок для одного события)
        var message = new Message<string, string>
        {
            Key = key,
            Value = jsonValue
        };

        // 3. Отправляем в Kafka
        // ProduceAsync ждет подтверждения от брокера (зависит от настроек Acks)
        await _producer.ProduceAsync(topic, message, cancellationToken);
    }

    // Освобождаем ресурсы продюсера при остановке приложения
    public void Dispose()
    {
        _producer?.Flush(TimeSpan.FromSeconds(10)); // Даем время отправить оставшиеся сообщения
        _producer?.Dispose();
    }
}