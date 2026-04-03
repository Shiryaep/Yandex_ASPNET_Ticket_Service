using Microsoft.AspNetCore.Mvc;
using Yandex_ASPNET_Ticket_Service.Models;
using Yandex_ASPNET_Ticket_Service.Models.DTO;
using Yandex_ASPNET_Ticket_Service.Services.BookingServices;
using Yandex_ASPNET_Ticket_Service.Services.EventServices;

namespace Yandex_ASPNET_Ticket_Service.Controllers;

/// <summary> Events Controller process /events requests </summary> 
[ApiController]
[Route("api/[controller]")]
public class EventsController(IEventService _eventService, IBookingService _bookingService) : ControllerBase
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

    [HttpPost("{id:Guid}/book")]
    [ProducesResponseType(typeof(Booking), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Post(Guid id)
    {
        Event? @event = _eventService.GetEvent(id);
        if (@event == null) return NotFound(new { message = $"Event with id [{id}] not found" });
        else
        {
            var booking = _bookingService.CreateBookingAsync(id).Result;

            var response = new BookingResponseDto
            {
                Id = booking.Id,
                EventId = booking.EventId,
                Status = booking.Status,
                CreatedAt = booking.CreatedAt,
                ProcessedAt = booking.ProcessedAt
            };

            return AcceptedAtAction(actionName: "GetBooking", controllerName: "Bookings", routeValues: new { bookingId = booking.Id }, value: booking);
        }
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
}