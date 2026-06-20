using Application.DTO;
using Application.Services.BookingServices;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;
/// <summary>
/// Controller for managing booking operations
/// </summary>
[ApiController]
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
}