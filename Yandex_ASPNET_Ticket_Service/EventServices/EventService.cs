using Yandex_ASPNET_Ticket_Service.Models;

namespace Yandex_ASPNET_Ticket_Service.EventServices;

/// <summary> Service for events manipulation </summary> 
public class EventService : IEventService
{
    private List<Event> events = new List<Event>();

    /// <summary> Return all created events as list using filters</summary> 
    public PaginatedResult<Event> GetEvents(string? title = null,
                                DateTime? from = null,
                                DateTime? to = null,
                                int page = 1,
                                int pageSize = 10)
    {
        var eventsLocal = new List<Event>(events);

        if (!string.IsNullOrEmpty(title))
        {
            eventsLocal = eventsLocal.Where(e => e.Title != null &&
            e.Title.Contains(title, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (from.HasValue)
        {
            eventsLocal = eventsLocal.Where(e => e.StartAt >= from.Value)
                .ToList();
        }

        if (to.HasValue)
        {
            eventsLocal = eventsLocal.Where(e => e.EndAt <= to.Value)
                .ToList();
        }

        int allEventsCount = eventsLocal.Count;

        eventsLocal = eventsLocal.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new PaginatedResult<Event>(eventsLocal, allEventsCount, page, pageSize);
    }

    /// <summary> Return event by ID </summary> 
    public Event? GetEvent(Guid id)
    {
        return events.FirstOrDefault(e => e.Id == id);
    }

    /// <summary> Add new event to events List</summary> 
    public Event AddEvent(Event @event)
    {
        events.Add(@event);
        return @event;
    }

    /// <summary> Replace existing event by ID </summary> 
    public void UpdateEvent(Guid id, Event @event)
    {
        var index = events.FindIndex(e => e.Id == id);

        if (index >= 0)
        {
            events[index] = @event;
        }
    }

    /// <summary> Remove event from events by ID </summary> 
    public void DeleteEvent(Guid id)
    {
        var index = events.FindIndex(e => e.Id == id);

        if (index >= 0)
        {
            events.RemoveAt(index);
        }
    }
}