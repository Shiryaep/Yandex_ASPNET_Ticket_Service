using System.ComponentModel.DataAnnotations;
using Yandex_ASPNET_Ticket_Service.Models;
using Yandex_ASPNET_Ticket_Service.Models.DTO;

namespace Yandex_ASPNET_Ticket_Service.Services.EventServices;

/// <summary> Service for events manipulation </summary>
public class EventService : IEventService
{
    private readonly List<Event> _events = [];

    /// <summary> Return all created events as list using filters</summary>
    public PaginatedResult<Event> GetEvents(string? title = null,
                                DateTime? from = null,
                                DateTime? to = null,
                                int page = 1,
                                int pageSize = 10)
    {
        var eventsLocal = new List<Event>(_events);

        if (!string.IsNullOrEmpty(title))
        {
            eventsLocal = [.. eventsLocal.Where(e => e.Title != null &&
            e.Title.Contains(title, StringComparison.OrdinalIgnoreCase))];
        }

        if (from.HasValue)
        {
            eventsLocal = [.. eventsLocal.Where(e => e.StartAt >= from.Value)];
        }

        if (to.HasValue)
        {
            eventsLocal = [.. eventsLocal.Where(e => e.EndAt <= to.Value)];
        }

        int allEventsCount = eventsLocal.Count;

        eventsLocal = [.. eventsLocal.Skip((page - 1) * pageSize).Take(pageSize)];

        return new PaginatedResult<Event>(eventsLocal, allEventsCount, page, pageSize);
    }

    /// <summary> Return event by ID </summary>
    public Event? GetEvent(Guid id)
    {
        return _events.FirstOrDefault(e => e.Id == id);
    }

    /// <summary> Add new event to events List</summary>
    public EventInfoDto AddEvent(CreateEventDto @event)
    {
        if (@event.TotalSeats <= 0) throw new ValidationException();
        Event newEvent = new()
        {
            Id = @event.Id,
            Title = @event.Title,
            Description = @event.Description,
            StartAt = @event.StartAt,
            EndAt = @event.EndAt,
            TotalSeats = @event.TotalSeats,
            AvailableSeats = @event.TotalSeats
        };

        _events.Add(newEvent);

        EventInfoDto eventInfoDto = new()
        {
            Id = newEvent.Id,
            Title = newEvent.Title,
            Description = newEvent.Description,
            StartAt = newEvent.StartAt,
            EndAt = newEvent.EndAt,
            TotalSeats = newEvent.TotalSeats,
            AvailableSeats = newEvent.TotalSeats
        };

        return eventInfoDto;
    }

    /// <summary> Replace existing event by ID </summary>
    public void UpdateEvent(Guid id, Event @event)
    {
        var index = _events.FindIndex(e => e.Id == id);

        if (index >= 0)
        {
            _events[index] = @event;
        }
    }

    /// <summary> Remove event from events by ID </summary>
    public void DeleteEvent(Guid id)
    {
        var index = _events.FindIndex(e => e.Id == id);

        if (index >= 0)
        {
            _events.RemoveAt(index);
        }
    }
}