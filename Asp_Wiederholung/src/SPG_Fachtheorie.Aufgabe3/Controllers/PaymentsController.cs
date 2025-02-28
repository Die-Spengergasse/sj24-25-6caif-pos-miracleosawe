using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SPG_Fachtheorie.Aufgabe3.Dtos;

namespace SPG_Fachtheorie.Aufgabe3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<PaymentDto>> GetPayments()
        {
            var payments = new List<PaymentDto>
            {
                new PaymentDto
                {
                    Id = 1,
                    EmployeeFirstName = "Max",
                    EmployeeLastName = "Mustermann",
                    CashDeskNumber = 101,
                    PaymentType = "CreditCard",
                    TotalAmount = 123.45m
                },
                new PaymentDto
                {
                    Id = 2,
                    EmployeeFirstName = "Erika",
                    EmployeeLastName = "Musterfrau",
                    CashDeskNumber = 102,
                    PaymentType = "Cash",
                    TotalAmount = 67.89m
                }
            };

            return Ok(payments);


        }

        [HttpGet("{id}")]
        public ActionResult<PaymentDetailDto> GetPaymentById(int id)
        {
            // Here would be the logic to fetch the specific payment from the database
            // This is a mock example
            var payment = new PaymentDetailDto
            {
                Id = id,
                EmployeeFirstName = "Max",
                EmployeeLastName = "Mustermann",
                CashDeskNumber = 101,
                PaymentType = "CreditCard",
                PaymentItems = new List<PaymentItemDto>
                {
                    new PaymentItemDto
                    {
                        ArticleName = "Laptop",
                        Amount = 1,
                        Price = 999.99m
                    },
                    new PaymentItemDto
                    {
                        ArticleName = "Mouse",
                        Amount = 2,
                         Price = 25.50m
                    }
                }
            };

            if (payment == null)
            {
                return NotFound();
            }

            return Ok(payment);
        }
    }
}
