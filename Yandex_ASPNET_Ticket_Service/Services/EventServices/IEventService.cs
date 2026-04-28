using Yandex_ASPNET_Ticket_Service.Models;
using Yandex_ASPNET_Ticket_Service.Models.DTO;

namespace Yandex_ASPNET_Ticket_Service.Services.EventServices;

/// <summary> Service Interface for events manipulation </summary>
public interface IEventService
{
    /// <summary> Return all created events as list </summary>
    public PaginatedResult<Event> GetEvents(string? title, 
        DateTime? from, DateTime? to,
        int page, int pageSize);

    /// <summary> Return event by ID </summary>
    public Event? GetEvent(Guid id);

    /// <summary> Add new event to events List</summary>
    public EventInfoDto AddEvent(CreateEventDto @event);

    /// <summary> Replace existing event by ID </summary>
    public void UpdateEvent(Guid id, Event @event);

    /// <summary> Remove event from events by ID </summary>
    public void DeleteEvent(Guid id);
}