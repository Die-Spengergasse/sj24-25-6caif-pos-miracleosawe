using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace SPG_Fachtheorie.Aufgabe1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PaymentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // PUT: api/paymentItems/1
        [HttpPut("paymentItems/{id}")]
        public IActionResult UpdatePaymentItem(int id, [FromBody] PaymentItemUpdateDto paymentItemDto)
        {
            var paymentItem = _context.PaymentItems.FirstOrDefault(pi => pi.Id == id);
            if (paymentItem == null)
            {
                return NotFound();
            }

            paymentItem.Update(paymentItemDto.ArticleName, paymentItemDto.Amount, paymentItemDto.Price);
            _context.SaveChanges();

            return NoContent();
        }
    }

    // DTO für die Aktualisierung von PaymentItems
    public class PaymentItemUpdateDto
    {
        public int Id { get; set; }
        public string ArticleName { get; set; }
        public int Amount { get; set; }
        public decimal Price { get; set; }
        public int PaymentId { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}
