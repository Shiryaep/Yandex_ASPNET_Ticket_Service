using System.ComponentModel.DataAnnotations;

namespace Yandex_ASPNET_Ticket_Service.Models.DTO;

/// <summary>
/// Data Transfer Object with event information
/// </summary>
public class EventInfoDto
{
    /// <summary> Event unique ID </summary> 
    [Required] public Guid Id { get; set; }

    /// <summary> Event title </summary> 
    [Required] public string? Title { get; set; }

    /// <summary> Event description </summary> 
    public string? Description { get; set; }

    /// <summary> Event start date and time </summary> 
    [Required] public DateTime? StartAt { get; set; } = null;

    /// <summary> Event finish date and time </summary> 
    [Required] public DateTime? EndAt { get; set; } = null;

    /// <summary> Event total seats for reservation </summary> 
    [Required] public int TotalSeats { get; set; }

    /// <summary> Event currently available seats for reservation </summary> 
    public int AvailableSeats { get; set; }
}