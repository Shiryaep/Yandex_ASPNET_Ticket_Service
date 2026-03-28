using Microsoft.AspNetCore.Mvc;
using Yandex_ASPNET_Ticket_Service.EventServices;
using Yandex_ASPNET_Ticket_Service.Models;

namespace Yandex_ASPNET_Ticket_Service.Controllers;

/// <summary> Events Controller process /events requests </summary> 
[ApiController]
[Route("[controller]")]
public class EventsController(IEventService _eventService) : ControllerBase
{
    /// <summary> Return all created events as list </summary> 
    [HttpGet]
    public ActionResult<PaginatedResult<Event>> GetAllEvents(
        string? title = null,
        DateTime? from = null,
        DateTime? to = null,
        int page = 1,
        int pageSize = 10)
    {
        return _eventService.GetEvents(title, from, to, page, pageSize);
    }

    /// <summary> Return event by received ID </summary> 
    [HttpGet("{id:Guid}")]
    public ActionResult<Event> GetEventById(Guid id)
    {
        var eventItem = _eventService.GetEvent(id);
        if (eventItem == null)
        {
            return NotFound();
        }
        return eventItem;
    }

    /// <summary> Add new event to all events </summary> 
    [HttpPost]
    public IActionResult Post([FromBody] Event @event)
    {
        if(!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var created = _eventService.AddEvent(@event);
        return CreatedAtAction(nameof(GetEventById), new { id = created.Id }, created);
    }

    /// <summary> Replace existing event by ID </summary> 
    [HttpPut("{id:Guid}")]
    public IActionResult Put(Guid id, [FromBody] Event @event)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _eventService.UpdateEvent(id, @event);
        return new NoContentResult();
    }

    /// <summary> Remove event from events by ID </summary>
    [HttpDelete("{id:Guid}")]
    public IActionResult Delete(Guid id)
    {
        _eventService.DeleteEvent(id);
        return new OkResult();
    }

    /// <summary> Trigger an exception </summary>
    [HttpGet("throw")]
    public IActionResult Throw()
    {
        throw new ArgumentException("Test exception");
    }
}