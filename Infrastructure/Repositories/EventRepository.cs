using Application.Repositories;
using Domain;
using Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class EventRepository(AppDbContext db) : IEventRepository
{
    private readonly AppDbContext _db = db;

    public Task<Event?> GetEventByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _db.Events.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
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
}