using System.ComponentModel.DataAnnotations;

namespace Yandex_ASPNET_Ticket_Service.Models.DTO;

public class CreateEventDto
{
    [Required] public string? Title { get; set; }
    public string? Description { get; set; }
    [Required] public DateTime? StartAt { get; set; } = null;
    [Required] public DateTime? EndAt { get; set; } = null;
    [Required] public int TotalSeats { get; set; }
}