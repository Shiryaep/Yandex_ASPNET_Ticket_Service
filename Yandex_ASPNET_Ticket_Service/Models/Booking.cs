using System;
using System.ComponentModel.DataAnnotations;

namespace Yandex_ASPNET_Ticket_Service.Models;

/// <summary> Booking entity </summary> 
///<remarks> Booking constuctor </remarks>
public class Booking(Guid eventID)
{
    /// <summary> Booking unique ID </summary>
    [Required]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary> ID of the Event that Booking refer to </summary>
    [Required]
    public Guid EventId { get; set; } = eventID;

    ///<summary> Current Booking Status </summary>
    [Required]
    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    ///<summary> Date and Time of Booking creation </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    ///<summary> Date and Time of Booking solution </summary>
    public DateTime? ProcessedAt { get; set; }
}