using System.ComponentModel.DataAnnotations;

namespace SPG_Fachtheorie.Aufgabe1.Commands
{
    public record AddressCmd(
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Invalid street.")]
        string Street,
        [StringLength(6, MinimumLength = 1, ErrorMessage = "Invalid zip.")]
        string Zip,
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Invalid city.")]
        string City
    );
}
