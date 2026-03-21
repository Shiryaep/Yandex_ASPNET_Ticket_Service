using Yandex_ASPNET_Ticket_Service.Models;

namespace Yandex_ASPNET_Ticket_Service;

/// <summary> Service Interface for events manipulation </summary> 
public interface IEventService
{
    /// <summary> Return all created events as list </summary> 
    public List<Event> GetEvents();

    /// <summary> Return event by ID </summary> 
    public Event? GetEvent(Guid id);

    /// <summary> Add new event to events List</summary> 
    public Event AddEvent(Event @event);

    /// <summary> Replace existing event by ID </summary> 
    public void UpdateEvent(Guid id, Event @event);

    /// <summary> Remove event from events by ID </summary> 
    public void DeleteEvent(Guid id);
}