using Application.DTO;
using Application.Services.EventServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Presentation.Controllers;

/// <summary>
/// Controller for managing events and event-related bookings
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class EventsController(IEventService eventService) : ControllerBase
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
    [AllowAnonymous]
    public async Task<ActionResult<PaginatedResult<EventInfoDto>>> GetAllEventsAsync(
        string? title = null,
        DateTime? from = null,
        DateTime? to = null,
        int page = 1,
        int pageSize = 10)
    {
        return await eventService.GetAllEventsAsync(title, from, to, page, pageSize);
    }

    /// <summary>
    /// Retrieves a specific event by its identifier
    /// </summary>
    /// <param name="id">Event identifier</param>
    /// <returns>The event if found; otherwise 404 Not Found</returns>
    [HttpGet("{id:Guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<EventInfoDto>> GetEventById(Guid id)
    {
        var eventItem = await eventService.GetEventByIdAsync(id);
        return eventItem;
    }

    /// <summary>
    /// Creates a new event
    /// </summary>
    /// <param name="event">Event data</param>
    /// <returns>201 Created with the created event</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Post([FromBody] CreateEventDto @event)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var created = await eventService.CreateEventAsync(@event);
        return CreatedAtAction(nameof(GetEventById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Replaces an existing event by its identifier
    /// </summary>
    /// <param name="id">Event identifier</param>
    /// <param name="event">Updated event data</param>
    /// <returns>204 No Content if successful; 400 Bad Request if validation fails</returns>
    [HttpPut("{id:Guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Put(Guid id, [FromBody] UpdateEventDto @event)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await eventService.UpdateEventAsync(id, @event);
        return new NoContentResult();
    }

    /// <summary>
    /// Deletes an event by its identifier
    /// </summary>
    /// <param name="id">Event identifier</param>
    /// <returns>200 OK if successful</returns>
    [HttpDelete("{id:Guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await eventService.DeleteEventAsync(id);
        return new OkResult();
    }
}