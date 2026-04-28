using System;
using System.ComponentModel.DataAnnotations;

namespace Yandex_ASPNET_Ticket_Service.Models;

/// <summary> Event entity </summary> 
public class Event
{
    /// <summary> Event unique ID </summary> 
    [Required]
    public Guid Id { get; set; }

    /// <summary> Event title </summary> 
    [Required(ErrorMessage = "There is no Title!")]
    public string? Title { get; set; }

    /// <summary> Event description </summary> 
    public string? Description { get; set; }

    /// <summary> Event start date and time </summary> 
    [Required(ErrorMessage = "There is no Start At!")]
    public DateTime? StartAt { get; set; } = null;

    /// <summary> Event finish date and time </summary> 
    [Required(ErrorMessage = "There is no End At!")]
    [ValidateEndAtLaterThanStartAt(ErrorMessage = "EndAt must be later than StartAt.")]
    public DateTime? EndAt { get; set; } = null;

    /// <summary> Event total seats for reservation </summary> 
    [Required(ErrorMessage = "You missed TotalSeats count in Event")]
    public int TotalSeats { get; set; }

    private int? _availableSeats;
    /// <summary> Event currently available seats for reservation </summary> 
    public int AvailableSeats
    {
        get => _availableSeats ?? TotalSeats;
        set => _availableSeats = value;
    }

    /// <summary>
    /// Check Available seats and try to reserve them
    /// </summary>
    /// <param name="count">Seats to reserve</param>
    /// <returns>True if there are avalible seats and false if no</returns>
    public bool TryReserveSeats(int count = 1)
    {
        if(AvailableSeats - count >= 0)
        {
            AvailableSeats -= count;
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Release reserved seats
    /// </summary>
    /// <param name="count">Seats to release</param>
    public void ReleaseSeats(int count = 1)
    {
        if(TotalSeats >= AvailableSeats + count)
        {
            AvailableSeats += count;
        }
    }
}

/// <summary>
/// Validates that the EndAt datetime is later than the StartAt datetime for an event
/// </summary>
public class ValidateEndAtLaterThanStartAt : ValidationAttribute
{
    /// <summary>
    /// Validates that EndAt is later than StartAt
    /// </summary>
    /// <param name="value">The value of the EndAt property being validated</param>
    /// <param name="validationContext">Validation context providing access to the object instance</param>
    /// <returns>ValidationResult.Success if valid; otherwise an error message</returns>
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return new ValidationResult("EndAt is required.");

        var endAt = (DateTime)value;
        var startAtProperty = validationContext.ObjectInstance.GetType()
            .GetProperty("StartAt");
        if (startAtProperty == null)
            return new ValidationResult("StartAt property not found.");

        var startAtValue = startAtProperty.GetValue(validationContext.ObjectInstance);
        if (startAtValue == null)
            return new ValidationResult("StartAt is required.");

        var startAt = (DateTime)startAtValue;

        if (endAt <= startAt)
            return new ValidationResult(ErrorMessage);

        return ValidationResult.Success!;
    }
}
