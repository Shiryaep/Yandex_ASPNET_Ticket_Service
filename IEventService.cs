namespace Yandex_ASPNET_Ticket_Service;

public interface IEventService
{
    public List<Event> GetEvents();

    public Event GetEvent(Guid id);

    public void AddEvent(Event @event);

    public void UpdateEvent(Guid id, Event @event);

    public void DeleteEvent(Guid id);
}