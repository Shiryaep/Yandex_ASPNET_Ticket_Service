using System;
using System.ComponentModel.DataAnnotations;

namespace Yandex_ASPNET_Ticket_Service.Models;

/// <summary> Booking entity </summary> 
public class Booking
{
    /// <summary> Booking unique ID </summary>
    [Required]
    public Guid Id { get; set; }

    /// <summary> ID of the Event that Booking refer to </summary>
    [Required]
    public Guid EventId { get; set; }

    ///<summary> Current Booking Status </summary>
    [Required]
    public BookingStatus Status { get; set; }

    ///<summary> Date and Time of Booking creation </summary>
    [Required]
    public DateTime CreatedAt { get; set; }

    ///<summary> Date and Time of Booking solution </summary>
    public DateTime? ProcessedAt { get; set; }

    ///<summary> Booking constuctor </summary>
    public Booking(Guid eventID) 
    {
        Id = Guid.NewGuid();
        EventId = eventID;
        Status = BookingStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }
}
