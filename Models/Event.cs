using System;
using System.ComponentModel.DataAnnotations;

namespace Yandex_ASPNET_Ticket_Service.Models;

public class Event
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "There is no Title!")]
    public string Title { get; set; }

    public string Description { get; set; }

    [Required(ErrorMessage = "There is no Start At!")]
    public DateTime? StartAt { get; set; } = null;

    [Required(ErrorMessage = "There is no End At!")]
    [ValidateEndAtLaterThanStartAt(ErrorMessage = "EndAt must be later than StartAt.")]
    public DateTime? EndAt { get; set; } = null;
}


public class ValidateEndAtLaterThanStartAt : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value == null)
            return new ValidationResult("EndAt is required.");

        var endAt = (DateTime)value;
        var startAt = (DateTime)validationContext.ObjectInstance.GetType()
            .GetProperty("StartAt")
            .GetValue(validationContext.ObjectInstance);

        if (endAt <= startAt)
            return new ValidationResult(ErrorMessage);

        return ValidationResult.Success;
    }
}
