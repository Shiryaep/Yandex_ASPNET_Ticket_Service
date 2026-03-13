using Microsoft.AspNetCore.Mvc;
using Yandex_ASPNET_Ticket_Service.Models;

namespace Yandex_ASPNET_Ticket_Service.Controllers;

[ApiController]
[Route("[controller]")]

public class EventController(IEventService _eventService) : ControllerBase
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

    [HttpPost]
    public IActionResult Post([FromBody] Event @event)
    {
        _eventService.AddEvent(@event);
        return new CreatedResult();
    }

    [HttpPut("{id:Guid}")]
    public IActionResult Put(Guid id, [FromBody] Event @event)
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