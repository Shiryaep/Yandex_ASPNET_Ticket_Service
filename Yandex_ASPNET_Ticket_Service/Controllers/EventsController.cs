using Microsoft.AspNetCore.Mvc;
using Yandex_ASPNET_Ticket_Service.Models;
using Yandex_ASPNET_Ticket_Service.Models.DTO;
using Yandex_ASPNET_Ticket_Service.Services.BookingServices;
using Yandex_ASPNET_Ticket_Service.Services.EventServices;

namespace Yandex_ASPNET_Ticket_Service.Controllers;

/// <summary>
/// Controller for managing events and event-related bookings
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EventsController(IEventService _eventService, IBookingService _bookingService) : ControllerBase
{
    /// <summary>
    /// Retrieves all events with optional filtering and pagination
    /// </summary>
    /// <param name="title">Optional title filter (case-insensitive substring match)</param>
    /// <param name="from">Optional start date filter (events starting on or after this date)</param>
    /// <param name="to">Optional end date filter (events ending on or before this date)</param>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 10)</param>
    /// <returns>Paginated list of events matching the filters</returns>
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

    /// <summary>
    /// Retrieves a specific event by its identifier
    /// </summary>
    /// <param name="id">Event identifier</param>
    /// <returns>The event if found; otherwise 404 Not Found</returns>
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

    /// <summary>
    /// Creates a new event
    /// </summary>
    /// <param name="event">Event data</param>
    /// <returns>201 Created with the created event</returns>
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

    /// <summary>
    /// Creates a booking for the specified event
    /// </summary>
    /// <param name="id">Event identifier</param>
    /// <returns>202 Accepted with booking details if event exists; otherwise 404 Not Found</returns>
    [HttpPost("{id:Guid}/book")]
    [ProducesResponseType(typeof(Booking), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Post(Guid id)
    {
        Event? @event = _eventService.GetEvent(id);
        if (@event == null) return NotFound(new { message = $"Event with id [{id}] not found" });
        else
        {
            var booking = await _bookingService.CreateBookingAsync(id);

            var response = new BookingResponseDto
            {
                Id = booking.Id,
                EventId = booking.EventId,
                Status = booking.Status,
                CreatedAt = booking.CreatedAt,
                ProcessedAt = booking.ProcessedAt
            };

            return AcceptedAtAction(actionName: "GetBooking", controllerName: "Bookings", routeValues: new { bookingId = booking.Id }, value: response);
        }
    }

    /// <summary>
    /// Replaces an existing event by its identifier
    /// </summary>
    /// <param name="id">Event identifier</param>
    /// <param name="event">Updated event data</param>
    /// <returns>204 No Content if successful; 400 Bad Request if validation fails</returns>
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

    /// <summary>
    /// Deletes an event by its identifier
    /// </summary>
    /// <param name="id">Event identifier</param>
    /// <returns>200 OK if successful</returns>
    [HttpDelete("{id:Guid}")]
    public IActionResult Delete(Guid id)
    {
        _eventService.DeleteEvent(id);
        return new OkResult();
    }
}