using Microsoft.EntityFrameworkCore;
using YaContracts;
using YaEvents.Application.Repositories;
using YaEvents.Domain;
using YaEvents.Infrastructure.DataAccess;

namespace YaEvents.Infrastructure.Repositories;

public class EventRepository(AppDbContext db) : IEventRepository
{
    private readonly AppDbContext _db = db;

    public Task<Event?> GetEventByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _db.Events.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<List<Event>> GetTopEventsAsync(CancellationToken cancellationToken = default)
    {
        var topEvents = await _db.Events
                        .Where(e => e.TotalSeats > 0 && e.TotalSeats != e.AvailableSeats)
                        .OrderByDescending(e =>
                        (e.TotalSeats - e.AvailableSeats) / (double)e.TotalSeats)
                        .Take(10)
                        .ToListAsync(cancellationToken: cancellationToken);

        return topEvents ?? [];
    }

    public Task AddEventAsync(Event @event, CancellationToken cancellationToken = default)
    {
        return _db.Events.AddAsync(@event, cancellationToken).AsTask();
    }

    public void DeleteEventAsync(Event @event)
    {
        _db.Events.Remove(@event);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _db.SaveChangesAsync(cancellationToken);
    }

    public IQueryable<Event> GetEventsAsQuery()
    {
        return _db.Events.AsQueryable();
    }

    public IQueryable<Event> GetFilteredQuery(string? title = null,
        DateTime? from = null, DateTime? to = null)
    {
        var query = _db.Events.AsQueryable();

        if (from.HasValue)
            query = query.Where(e => e.StartAt >= from.Value);

        if (to.HasValue)
            query = query.Where(e => e.StartAt <= to.Value);

        if (!string.IsNullOrWhiteSpace(title))
            query = query.Where(e => e.Title.ToLower().Contains(title.ToLower()));

        return query;
    }

    public Task<int> GetCountOfQuery(IQueryable<Event> query, CancellationToken cancellationToken = default)
    {
        return query.CountAsync(cancellationToken);
    }

    public Task<List<Event>> GetPaginatedItemsOfQuery(IQueryable<Event> query,
        int page = Constants.DefaultPage,
        int pageSize = Constants.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        return query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }
}