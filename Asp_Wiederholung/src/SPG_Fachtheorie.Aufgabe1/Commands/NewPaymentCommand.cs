using SPG_Fachtheorie.Aufgabe1.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SPG_Fachtheorie.Aufgabe1.Commands
{
    public record NewPaymentCommand(
        [Range(1, int.MaxValue, ErrorMessage = "Invalid cashdesk number." )]
        int CashDeskNumber,
        DateTime PaymentDateTime,
        string PaymentType,
        [Range(1, int.MaxValue, ErrorMessage = "Invalid EmployeeRegistrationNumber." )]
        int EmployeeRegistrationNumber) : IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PaymentDateTime > DateTime.Now.AddMinutes(1))
                yield return new ValidationResult("Invalid payment date",
                    new string[] { nameof(PaymentDateTime) });
            if (!Enum.TryParse<PaymentType>(PaymentType, out var _))
            {
                yield return new ValidationResult("Invalid payment type",
                    new string[] { nameof(PaymentType) });
            }
        }
    }
}