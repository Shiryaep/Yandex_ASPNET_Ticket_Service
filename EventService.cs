using Yandex_ASPNET_Ticket_Service.Models;

namespace Yandex_ASPNET_Ticket_Service;

/// <summary> Service for events manipulation </summary> 
public class EventService : IEventService
{
    private List<Event> events = new List<Event>();

    /// <summary> Return all created events as list </summary> 
    public List<Event> GetEvents()
    {
        return events;
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