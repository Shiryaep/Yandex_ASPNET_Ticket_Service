using Confluent.Kafka;
using System.Text.Json;
using YaBookings.Application.Publishers;

namespace YaBookings.Infrastructure.Publishers;

public class KafkaEventPublisher : IDomainEventPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;

    public KafkaEventPublisher(IProducer<string, string> producer)
    {
        _producer = producer;
    }

    public async Task PublishAsync<T>(string topic, T @event, string key, CancellationToken cancellationToken = default)
    {
        var jsonValue = JsonSerializer.Serialize(@event);

        var message = new Message<string, string>
        {
            Key = key,
            Value = jsonValue
        };

        await _producer.ProduceAsync(topic, message, cancellationToken);
    }

    public void Dispose()
    {
        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();
    }
}