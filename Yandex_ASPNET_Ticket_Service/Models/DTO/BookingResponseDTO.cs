namespace Yandex_ASPNET_Ticket_Service.Models.DTO;

/// <summary>
/// Data Transfer Object for booking response
/// </summary>
public class BookingResponseDto
{
    /// <summary>
    /// Unique identifier of the booking
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identifier of the event associated with the booking
    /// </summary>
    public Guid EventId { get; set; }

    /// <summary>
    /// Current status of the booking
    /// </summary>
    public BookingStatus Status { get; set; }

    /// <summary>
    /// Date and time when the booking was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date and time when the booking was processed (confirmed or rejected)
    /// </summary>
    public DateTime? ProcessedAt { get; set; }
}