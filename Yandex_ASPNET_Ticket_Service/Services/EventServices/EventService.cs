using Microsoft.EntityFrameworkCore;
using Yandex_ASPNET_Ticket_Service.Models;
using Yandex_ASPNET_Ticket_Service.Models.DTO;
using Yandex_ASPNET_Ticket_Service.Models.Exceptions;
using Yandex_ASPNET_Ticket_Service.Repositories;

namespace Yandex_ASPNET_Ticket_Service.Services.EventServices;

/// <summary> Service for events manipulation </summary>
public class EventService(IEventRepository eventRepository) : IEventService
{
    private readonly IEventRepository _eventRepository = eventRepository;

    /// <summary> Return all created events as list using filters</summary>
    public async Task<PaginatedResult<EventInfoDto>> GetAllEventsAsync(string? title = null,
        DateTime? from = null, DateTime? to = null,
        int page = AppConstants.DefaultPage,
        int pageSize = AppConstants.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _eventRepository.GetEventsAsQuery();

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
        var @event = await _eventRepository.GetEventByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Event not found");

        return ToInfo(@event);
    }

    /// <summary> Add new event to events List</summary>
    public async Task<EventInfoDto> CreateEventAsync(CreateEventDto createEvent, CancellationToken cancellationToken = default)
    {
        Event @event = Event.Create(createEvent.Title, createEvent.Description, createEvent.StartAt, createEvent.EndAt, createEvent.TotalSeats);
        await _eventRepository.AddEventAsync(@event, cancellationToken);
        await _eventRepository.SaveChangesAsync(cancellationToken);
        return ToInfo(@event);
    }

    /// <summary> Update existing event by ID </summary>
    public async Task<EventInfoDto> UpdateEventAsync(Guid id, UpdateEventDto updateEvent, CancellationToken cancellationToken = default)
    {
        var @event = await _eventRepository.GetEventByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Event not found");
        @event.Update(updateEvent.Title, updateEvent.Description, updateEvent.StartAt, updateEvent.EndAt);
        await _eventRepository.SaveChangesAsync(cancellationToken);

        return ToInfo(@event);
    }

    public async Task<bool> DeleteEventAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var @event = await _eventRepository.GetEventByIdAsync(id, cancellationToken);
        if (@event == null)
            return false;

        _eventRepository.DeleteEventAsync(@event);
        await _eventRepository.SaveChangesAsync(cancellationToken);
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