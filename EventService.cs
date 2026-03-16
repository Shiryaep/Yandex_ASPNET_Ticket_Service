using Yandex_ASPNET_Ticket_Service.Models;

namespace Yandex_ASPNET_Ticket_Service;

public class EventService : IEventService
{
    public List<Event> events = new List<Event>();

    public void AddEvent(Event @event)
    {
        events.Add(@event);
    }

    public Event? GetEvent(Guid id)
    {
        return events.FirstOrDefault(e => e.Id == id);
    }

    public List<Event> GetEvents()
    {
        return events;
    }

    public void UpdateEvent(Guid id, Event @event)
    {
        var index = events.FindIndex(e => e.Id == id);

        if (index >= 0)
        {
            events[index] = @event;
        }
    }

    public void DeleteEvent(Guid id)
    {
        var index = events.FindIndex(e => e.Id == id);

        if (index >= 0)
        {
            events.RemoveAt(index);
        }
    }
}