using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

public class UpdateConfirmedCommand : IValidatableObject
{
    [Required]
    public DateTime Confirmed { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var now = DateTime.Now;
        var maxAllowedFutureTime = now.AddMinutes(1);

        if (Confirmed > maxAllowedFutureTime)
        {
            yield return new ValidationResult(
                $"The confirmed date must not be more than 1 minute in the future. Current time: {now}, provided: {Confirmed}",
                new[] { nameof(Confirmed) });
        }
    }
}