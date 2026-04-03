using Microsoft.AspNetCore.Mvc;
using Yandex_ASPNET_Ticket_Service.Models.DTO;
using Yandex_ASPNET_Ticket_Service.Services.BookingServices;

[ApiController]
[Route("api/[controller]")]
public class BookingsController(IBookingService _bookingService) : ControllerBase
{

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
