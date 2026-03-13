using Microsoft.AspNetCore.Mvc;

namespace Yandex_ASPNET_Ticket_Service.Controllers;

[ApiController]
[Route("[controller]")]

public class EventController(IEventService _eventService)
{
    [HttpGet]
    public ActionResult<List<Event>> GetAllEvents()
    {
        return _eventService.GetEvents();
    }

    [HttpGet("{id:Guid}")]
    public ActionResult<Event> GetEventById(Guid id)
    {
        return _eventService.GetEvent(id);
    }

    [HttpPost("{event:Event}")]
    public IActionResult Post(Event @event)
    {
        _eventService.AddEvent(@event);
        return new CreatedResult();
    }

    [HttpPut("{id:Guid}/{event:Event}")]
    public IActionResult Put(Guid id, Event @event)
    {
        _eventService.UpdateEvent(id, @event);
        return new NoContentResult();
    }

    [HttpDelete("{id:Guid}")]
    public IActionResult Delete(Guid id)
    {
        _eventService.DeleteEvent(id);
        return new OkResult();
    }

}