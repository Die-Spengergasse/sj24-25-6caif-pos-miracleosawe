using System;
using System.ComponentModel.DataAnnotations;

namespace SPG_Fachtheorie.Aufgabe1.Commands
{
    // Die Command Klasse speichert den Payload,
    // den uns der Client schickt.
    public record NewManagerCommand(
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
        [RegularExpression(@"^[A-Z][A-Za-z0-9 ]+$", ErrorMessage = "Invalid car type.")]
        string CarType
    );
}
