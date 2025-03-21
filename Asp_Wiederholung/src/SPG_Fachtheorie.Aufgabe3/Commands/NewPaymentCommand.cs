using System.ComponentModel.DataAnnotations;

namespace SPG_Fachtheorie.Aufgabe3.Commands
{
    public class NewPaymentCommand
    {
        public int CashDeskNumber { get; set; }

        [Required]
        public DateTime PaymentDateTime { get; set; }

        [Required]
        public string PaymentType { get; set; }

        public int EmployeeRegistrationNumber { get; set; }

        public bool IsPaymentDateTimeValid()
        {
            return PaymentDateTime <= DateTime.Now.AddMinutes(1);
        }

    }
}