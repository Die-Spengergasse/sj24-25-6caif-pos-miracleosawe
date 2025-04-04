using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPG_Fachtheorie.Aufgabe1.Commands;
using SPG_Fachtheorie.Aufgabe1.Infrastructure;
using SPG_Fachtheorie.Aufgabe1.Model;
using SPG_Fachtheorie.Aufgabe3.Commands;
using SPG_Fachtheorie.Aufgabe3.Dtos;

namespace SPG_Fachtheorie.Aufgabe3.Controllers
{
    [Route("api/[controller]")]  // --> api/payments
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly AppointmentContext _db;

        public PaymentsController(AppointmentContext db)
        {
            _db = db;
        }

        /// <summary>
        /// GET /api/payments
        /// GET /api/payments?cashDesk=1
        /// GET /api/payments?dateFrom=2024-05-13
        /// GET /api/payments?dateFrom=2024-05-13&cashDesk=1
        /// </summary>
        [HttpGet]
        public ActionResult<List<PaymentDto>> GetAllPayments(
            [FromQuery] int? cashDesk,
            [FromQuery] DateTime? dateFrom)
        {
            var payments = _db.Payments
                .Where(p => (!cashDesk.HasValue || p.CashDesk.Number == cashDesk.Value)
                         && (!dateFrom.HasValue || p.PaymentDateTime >= dateFrom.Value))
                .Select(p => new PaymentDto(
                    p.Id, p.Employee.FirstName, p.Employee.LastName,
                    p.PaymentDateTime,
                    p.CashDesk.Number, p.PaymentType.ToString(),
                    p.PaymentItems.Sum(pi => pi.Price)))
                .ToList();
            return Ok(payments);
        }

        /// <summary>
        /// GET /api/payments/{id}
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public ActionResult<PaymentDetailDto> GetPaymentById(int id)
        {
            var payment = _db.Payments
                .Where(p => p.Id == id)
                .Select(p => new PaymentDetailDto(
                    p.Id, p.Employee.FirstName, p.Employee.LastName,
                    p.CashDesk.Number, p.PaymentType.ToString(),
                    p.PaymentItems
                        .Select(pi => new PaymentItemDto(
                            pi.ArticleName, pi.Amount, pi.Price))
                        .ToList()))
                .FirstOrDefault();
            if (payment is null) return NotFound();
            return Ok(payment);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult AddPayment([FromBody] NewPaymentCommand cmd)
        {
            // Löse die foreign keys auf.
            var cashDesk = _db.CashDesks
                .FirstOrDefault(c => c.Number == cmd.CashDeskNumber);
            if (cashDesk is null) return Problem("Invalid cashdesk", statusCode: 400);
            var employee = _db.Employees
                .FirstOrDefault(e => e.RegistrationNumber == cmd.EmployeeRegistrationNumber);
            if (employee is null) return Problem("Invalid employee", statusCode: 400);
            // Erzeuge die Modelklasse
            var paymentType = Enum.Parse<PaymentType>(cmd.PaymentType);
            var payment = new Payment(
                cashDesk, cmd.PaymentDateTime, employee, paymentType);
            _db.Payments.Add(payment);
            try
            {
                // Führe das INSERT INTO durch.
                _db.SaveChanges();
            }
            catch (DbUpdateException e)
            {
                return Problem(e.InnerException?.Message ?? e.Message, statusCode: 400);
            }
            return CreatedAtAction(nameof(AddPayment), new { payment.Id });
        }

        /// <summary>
        /// DELETE /api/payments/{id}?deleteItems=true|false
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult DeletePayment(int id, [FromQuery] bool deleteItems)
        {
            var payment = _db.Payments.FirstOrDefault(p => p.Id == id);
            if (payment is null)
                //return Problem("Payment not found", statusCode: 404);
                return NoContent();

            var paymentItems = _db.PaymentItems
                .Where(p => p.Payment.Id == id)
                .ToList();

            if (deleteItems)
            {
                try
                {
                    _db.PaymentItems.RemoveRange(paymentItems);
                    _db.SaveChanges();
                }
                catch (DbUpdateException e)
                {
                    return Problem(
                        e.InnerException?.Message ?? e.Message,
                        statusCode: 400);
                }
            }
            else
            {
                if (paymentItems.Any())
                    return Problem("Payment has payment items.", statusCode: 400);
            }
            try
            {
                _db.Payments.Remove(payment);
                _db.SaveChanges();
            }
            catch (DbUpdateException e)
            {
                return Problem(
                    e.InnerException?.Message ?? e.Message, statusCode: 400);
            }
            return NoContent();
        }

        [HttpPut("/api/paymentItems/{id}")]
        public IActionResult UpdatePayment(int id, [FromBody] UpdatePaymentItemCommand cmd)
        {
            if (cmd.Id != id)
                return Problem("Invalid payment item id", statusCode: 400);

            var paymentItem = _db.PaymentItems.FirstOrDefault(p => p.Id == cmd.Id);
            if (paymentItem is null) return Problem("Payment item Item not found", statusCode: 404);

            var payment = _db.Payments.FirstOrDefault(p => p.Id == cmd.PaymentId);
            if (payment is null) return Problem("Payment Item not found", statusCode: 404);

            if (paymentItem.LastUpdated != cmd.LastUpdated)
                return Problem("Payment item has changed", statusCode: 400);

            paymentItem.ArticleName = cmd.ArticleName;
            paymentItem.Price = cmd.Price;
            paymentItem.Payment = payment;
            paymentItem.LastUpdated = DateTime.UtcNow;
            try
            {
                _db.SaveChanges();
            }
            catch (DbUpdateException e)
            {
                return Problem(e.InnerException?.Message ?? e.Message, statusCode: 400);
            }
            return NoContent();
        }

        [HttpPatch("{id}")]
        public IActionResult UpdateConfirmed(int id, UpdateConfirmedCommand cmd)
        {
            var payment = _db.Payments.FirstOrDefault(p => p.Id == id);
            if (payment is null) return Problem("Payment not found", statusCode: 404);

            if (payment.Confirmed.HasValue)
                return Problem("Payment already confirmed", statusCode: 400);
            payment.Confirmed = cmd.Confirmed;
            try
            {
                _db.SaveChanges();
            }
            catch (DbUpdateException e)
            {
                return Problem(e.InnerException?.Message ?? e.Message, statusCode: 400);
            }
            return NoContent();
        }
    }
}
