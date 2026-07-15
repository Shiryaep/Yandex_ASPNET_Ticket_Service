using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using YaContracts;
using YaEvents.Application.Repositories;
using YaEvents.Application.Services;
using YaEvents.Domain;
using YaEvents.Infrastructure.DataAccess;
using YaEvents.Infrastructure.Settings;

namespace YaEvents.Infrastructure.Repositories;

public class EventRepository(
    AppDbContext db,
    ICacheService cache,
    IOptions<RedisCacheSettings> settings) : IEventRepository
{
    private readonly AppDbContext _db = db;
    private readonly ICacheService _cache = cache;
    private readonly RedisCacheSettings _settings = settings.Value;

    public async Task<Event?> GetEventByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"event:{id}";

        var cachedVal = await _cache.GetAsync<Event>(cacheKey);
        if (cachedVal != null)
            return cachedVal;

        var dbEventVal = await _db.Events.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (dbEventVal != null)
            await _cache.SetAsync<Event>(cacheKey, dbEventVal, TimeSpan.FromSeconds(_settings.EventsGetEventByIdTTLSeconds));

        return dbEventVal;
    }

    public async Task<List<Event>> GetTopEventsAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = "events:top10";

        var cachedVal = await _cache.GetAsync<List<Event>>(cacheKey);
        if (cachedVal != null)
            return cachedVal;

        var topEvents = await _db.Events
                        .Where(e => e.TotalSeats > 0 && e.TotalSeats != e.AvailableSeats)
                        .OrderByDescending(e =>
                        (e.TotalSeats - e.AvailableSeats) / (double)e.TotalSeats)
                        .Take(10)
                        .ToListAsync(cancellationToken: cancellationToken);

        if (topEvents != null)
            await _cache.SetAsync<List<Event>>(cacheKey, topEvents, TimeSpan.FromMinutes(_settings.EventsGetTopEventsTTLMinutes));

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