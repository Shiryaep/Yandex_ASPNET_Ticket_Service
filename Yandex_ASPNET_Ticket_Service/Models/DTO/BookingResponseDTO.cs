namespace Yandex_ASPNET_Ticket_Service.Models.DTO;

public class BookingResponseDto
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}