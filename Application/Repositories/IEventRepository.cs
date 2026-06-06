using Domain;

namespace Application.Repositories;

public interface IEventRepository
{
    public Task<Event?> GetEventByIdAsync(Guid id, CancellationToken cancellationToken = default);

    public Task AddEventAsync(Event @event, CancellationToken cancellationToken = default);

    public void DeleteEventAsync(Event @event);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default);

    public IQueryable<Event> GetEventsAsQuery();
}