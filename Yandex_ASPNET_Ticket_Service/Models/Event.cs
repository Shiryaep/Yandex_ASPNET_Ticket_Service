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
}

/// <summary> Event datetime validator check whether finish later than start of event </summary> 
public class ValidateEndAtLaterThanStartAt : ValidationAttribute
{
    /// <summary> Validate date and time of event </summary> 
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
