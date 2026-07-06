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

    [HttpDelete("{bookingId}")]
    public async Task<IActionResult> CancelBooking(Guid bookingId)
    {
        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub);

        if (userIdClaim == null)
        {
            return BadRequest("User Id not found");
        }

        Guid userId = Guid.Parse(userIdClaim.Value);

        var result = await bookingService.CancelBookingByIdAsync(bookingId, userId);
        return Ok(result);
    }
}