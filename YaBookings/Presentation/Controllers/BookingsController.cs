using Application.DTO;
using Application.Services.BookingServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Presentation.Controllers;
/// <summary>
/// Controller for managing booking operations
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class BookingsController(IBookingService bookingService) : ControllerBase
{
    /// <summary>
    /// Retrieves a booking by its identifier
    /// </summary>
    /// <param name="bookingId">Unique identifier of the booking</param>
    /// <returns>200 OK with booking details if found; otherwise 404 Not Found</returns>
    [HttpGet("{bookingId}")]
    [ProducesResponseType(typeof(BookingInfoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBooking(Guid bookingId)
    {
        var booking = await bookingService.GetBookingByIdAsync(bookingId);
        return Ok(booking);
    }

    /// <summary>
    /// Creates a booking for the specified event
    /// </summary>
    /// <param name="id">Event identifier</param>
    /// <returns>202 Accepted with booking details if event exists; otherwise 404 Not Found</returns>
    [HttpPost("{eventId:Guid}/book")]
    [Authorize]
    //[ProducesResponseType(typeof(BookingInfoDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Post(Guid eventId)
    {
        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub);

        if (userIdClaim == null)
        {
            return BadRequest("User Id not found");
        }

        Guid userId = Guid.Parse(userIdClaim.Value);

        // ТУТ НАДО ПУБЛИКОВАТЬ СОБИТИЕ О ТОМ, ЧТО БУКИНГ СОЗДАН И ЕГО НАДО ОБРАБОТАТЬ
        //var booking = await bookingService.CreateBookingAsync(eventId, userId);

        //return AcceptedAtAction(actionName: "GetBooking", controllerName: "Bookings", routeValues: new { bookingId = booking.Id }, value: booking);
        return NoContent();
    }

    [HttpDelete("{bookingId}")]
    public async Task<IActionResult> CancelBooking(Guid bookingId)
    {
        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub);

        if (userIdClaim == null)
        {
            return BadRequest("User Id not found");
        }

        Guid userId = Guid.Parse(userIdClaim.Value);

        await bookingService.CancelBookingByIdAsync(bookingId, userId);
        return NoContent();
    }
}