using Microsoft.AspNetCore.Mvc;
using Yandex_ASPNET_Ticket_Service.Models.DTO;
using Yandex_ASPNET_Ticket_Service.Models.Exceptions;
using Yandex_ASPNET_Ticket_Service.Services.BookingServices;

namespace Yandex_ASPNET_Ticket_Service.Controllers;
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
        try
        {
            var booking = await bookingService.GetBookingByIdAsync(bookingId);

            if (booking == null)
            {
                return NotFound(new { message = $"Booking with id [{bookingId}] not found" });
            }

            var response = new BookingInfoDto
            {
                Id = booking.Id,
                EventId = booking.EventId,
                Status = booking.Status,
                CreatedAt = booking.CreatedAt,
                ProcessedAt = booking.ProcessedAt
            };

            return Ok(response);
        }
        catch (NotFoundException)
        {
            return NotFound(new { message = $"Booking with id [{bookingId}] not found" });
        }
    }
}