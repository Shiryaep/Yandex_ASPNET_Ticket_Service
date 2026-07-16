using YaContracts;
using YaEvents.Domain;

namespace YaEvents.Application.Repositories;

public interface IEventRepository
{
    public Task<Event?> GetEventByIdAsync(Guid id,
        CancellationToken cancellationToken = default);

    public Task<List<Event>> GetTopEventsAsync(CancellationToken cancellationToken = default);

    public Task AddEventAsync(Event @event,
        CancellationToken cancellationToken = default);

    public void DeleteEventAsync(Event @event);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default);

    public IQueryable<Event> GetEventsAsQuery();

    public IQueryable<Event> GetFilteredQuery(string? title = null,
        DateTime? from = null, DateTime? to = null);

    public Task<int> GetCountOfQuery(IQueryable<Event> query,
        CancellationToken cancellationToken = default);

    public Task<List<Event>> GetPaginatedItemsOfQuery(IQueryable<Event> query,
        int page = Constants.DefaultPage,
        int pageSize = Constants.DefaultPageSize,
        CancellationToken cancellationToken = default);
}