using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SPG_Fachtheorie.Aufgabe1.Commands
{
    public record NewCashierCommand(
        [Range(1000,9999,ErrorMessage = "Invalid RegistrationNumber")]
        int RegistrationNumber,
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Invalid firstname.")]
        string FirstName,
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Invalid lastname.")]
        string LastName,
        DateOnly Birthdate,
        [Range(0, 1_000_000)]
        decimal? Salary,
        AddressCmd? Address,
        string JobSpezialisation
    ) : IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (JobSpezialisation != "Feinkost" && JobSpezialisation != "Kassa")
                yield return new ValidationResult(
                    "Invalid Job Spezialisation", new[] { nameof(JobSpezialisation) });
            if (FirstName.Length + LastName.Length < 3)
                yield return new ValidationResult(
                    "Name is too short.", new[] { nameof(FirstName), nameof(LastName) });
        }
    }
}
