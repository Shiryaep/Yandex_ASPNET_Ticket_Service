using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Yandex_ASPNET_Ticket_Service.DataAccess;
using Yandex_ASPNET_Ticket_Service.Models;
using Yandex_ASPNET_Ticket_Service.Models.DTO;
using Yandex_ASPNET_Ticket_Service.Models.Exceptions;

namespace Yandex_ASPNET_Ticket_Service.Services.EventServices;

/// <summary> Service for events manipulation </summary>
public class EventService(AppDbContext context) : IEventService
{
    private readonly AppDbContext _context = context;

    /// <summary> Return all created events as list using filters</summary>
    public async Task<PaginatedResult<EventInfoDto>> GetAllEventsAsync(string? title = null,
        DateTime? from = null, DateTime? to = null,
        int page = AppConstants.DefaultPage,
        int pageSize = AppConstants.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Events.AsQueryable();

        if (from.HasValue)
            query = query.Where(e => e.StartAt >= from.Value);

        if (to.HasValue)
            query = query.Where(e => e.StartAt <= to.Value);

        if (!string.IsNullOrWhiteSpace(title))
            query = query.Where(e => e.Title.ToLower().Contains(title.ToLower()));

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<EventInfoDto>
        {
            Items = items.Select(ToInfo).ToArray(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <summary> Return event by ID </summary>
    public async Task<EventInfoDto> GetEventByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var @event = await _context.Events.FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new NotFoundException("Event not found");

        return ToInfo(@event);
    }

    /// <summary> Add new event to events List</summary>
    public async Task<EventInfoDto> CreateEventAsync(CreateEventDto createEvent, CancellationToken cancellationToken = default)
    {
        Event @event = Event.Create(createEvent.Title, createEvent.Description, createEvent.StartAt, createEvent.EndAt, createEvent.TotalSeats);
        await _context.Events.AddAsync(@event, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return ToInfo(@event);
    }

    /// <summary> Update existing event by ID </summary>
    public async Task<EventInfoDto> UpdateEventAsync(Guid id, UpdateEventDto updateEvent, CancellationToken cancellationToken = default)
    {
        var @event = await _context.Events.FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new NotFoundException("Event not found");
        @event.Update(updateEvent.Title, updateEvent.Description, updateEvent.StartAt, updateEvent.EndAt);
        await _context.SaveChangesAsync(cancellationToken);

        return ToInfo(@event);
    }

    public async Task<bool> DeleteEventAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var @event = await _context.Events.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (@event == null)
            return false;

        _context.Events.Remove(@event);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public static EventInfoDto ToInfo(Event @event) => new()
    {
        Id = @event.Id,
        Title = @event.Title,
        Description = @event.Description,
        StartAt = @event.StartAt,
        EndAt = @event.EndAt,
        TotalSeats = @event.TotalSeats,
        AvailableSeats = @event.AvailableSeats
    };
}