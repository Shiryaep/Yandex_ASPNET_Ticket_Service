using Yandex_ASPNET_Ticket_Service.Models;

namespace Yandex_ASPNET_Ticket_Service.Repositories;

public interface IEventRepository
{
    public Task<Event?> GetEventByIdAsync(Guid id, CancellationToken cancellationToken = default);

    public Task AddEventAsync(Event @event, CancellationToken cancellationToken = default);

    public void DeleteEventAsync(Event @event);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default);

    public IQueryable<Event> GetEventsAsQuery();
}