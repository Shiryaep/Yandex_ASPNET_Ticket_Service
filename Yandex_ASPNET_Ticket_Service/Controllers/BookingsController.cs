using Microsoft.AspNetCore.Mvc;
using Yandex_ASPNET_Ticket_Service.Models.DTO;
using Yandex_ASPNET_Ticket_Service.Services.BookingServices;

/// <summary>
/// Controller for managing booking operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BookingsController(IBookingService _bookingService) : ControllerBase
{
    /// <summary>
    /// Retrieves a booking by its identifier
    /// </summary>
    /// <param name="bookingId">Unique identifier of the booking</param>
    /// <returns>200 OK with booking details if found; otherwise 404 Not Found</returns>
    [HttpGet("{bookingId}")]
    [ProducesResponseType(typeof(BookingResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBooking(Guid bookingId)
    {
        var booking = await _bookingService.GetBookingByIdAsync(bookingId);

        if (booking == null)
        {
            return NotFound(new { message = $"Booking with id [{bookingId}] not found" });
        }

        var response = new BookingResponseDto
        {
            Id = booking.Id,
            EventId = booking.EventId,
            Status = booking.Status,
            CreatedAt = booking.CreatedAt,
            ProcessedAt = booking.ProcessedAt
        };
        
        return Ok(response);
    }
}
