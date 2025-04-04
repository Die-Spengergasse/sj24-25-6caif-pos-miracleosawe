using System;
using System.ComponentModel.DataAnnotations;

namespace SPG_Fachtheorie.Aufgabe1.Commands
{
    public record UpdateManagerCommand(
        int RegistrationNumber,
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Invalid firstname.")]
        string FirstName,
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Invalid lastname.")]
        string LastName,
        AddressCmd? Address,
        [RegularExpression(@"^[A-Z][A-Za-z0-9 ]+$", ErrorMessage = "Invalid car type.")]
        string CarType,
        DateTime? LastUpdate
    );
}
