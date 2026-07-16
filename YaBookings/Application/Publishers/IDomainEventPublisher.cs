namespace YaBookings.Application.Publishers;

public interface IDomainEventPublisher
{
    Task PublishAsync<T>(string topic, T @event, string key, CancellationToken cancellationToken = default);
}
