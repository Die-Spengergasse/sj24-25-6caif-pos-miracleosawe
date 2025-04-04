using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using SPG_Fachtheorie.Aufgabe1.Commands;
using SPG_Fachtheorie.Aufgabe1.Infrastructure;
using SPG_Fachtheorie.Aufgabe1.Model;
using SPG_Fachtheorie.Aufgabe1.Services;
using SPG_Fachtheorie.Aufgabe3.Commands;
using SPG_Fachtheorie.Aufgabe3.Dtos;

namespace SPG_Fachtheorie.Aufgabe3.Controllers
{
    [Route("api/[controller]")]  // [controller] bedeutet: das Wort vor Controller
    [ApiController]              // Soll von ASP gemappt werden
    public class EmployeesController : ControllerBase
    {
        private readonly EmployeeService _service;

        public EmployeesController(EmployeeService service)
        {
            _service = service;
        }

        /// <summary>
        /// GET /api/employees oder             --> type = null
        /// GET /api/employees?type=Manager     --> type = Manager
        /// GET /api/employees?type=manager     --> type = manager
        /// GET /api/employees?type=            --> type = ""
        /// </summary>
        [HttpGet]
        public ActionResult<List<EmployeeDto>> GetAllEmployees([FromQuery] string? type)
        {
            var employees = _service.Employees
                .Where(e => string.IsNullOrEmpty(type)
                    ? true : e.Type.ToLower() == type.ToLower())
                .Select(e => new EmployeeDto(
                    e.RegistrationNumber, e.FirstName, e.LastName,
                    $"{e.FirstName} {e.LastName}", e.Type))
                .ToList();    //  // [{...}, {...}, ... ]
            return Ok(employees);
        }

        /// <summary>
        /// GET /api/employee/1001
        /// </summary>
        [HttpGet("{registrationNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<EmployeeDetailDto> GetEmployee(int registrationNumber)
        {
            var employees = _service.Employees
                .Where(e => e.RegistrationNumber == registrationNumber)
                .Select(e => new EmployeeDetailDto(
                    e.RegistrationNumber,
                    e.FirstName, e.LastName,
                    $"{e.FirstName} {e.LastName}",
                    e.Address, e.Type))
                .AsNoTracking()
                .FirstOrDefault();  // { .... }
            if (employees is null) { return NotFound(); }
            return Ok(employees);
        }

        ///
        /// POST /api/employee/manager
        [HttpPost("manager")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult AddManager(NewManagerCommand cmd)
        {
            try
            {
                var manager = _service.AddManager(cmd);
                return CreatedAtAction(nameof(AddManager), new { manager.RegistrationNumber });
            }
            catch (EmployeeServiceException e)
            {
                return Problem(e.Message, statusCode: 400);
            }
        }

        /// POST /api/employee/cashier
        [HttpPost("cashier")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult AddCashier(NewCashierCommand cmd)
        {
            try
            {
                var cashier = _service.AddCashier(cmd);
                return CreatedAtAction(nameof(AddCashier), new { cashier.RegistrationNumber });
            }
            catch (EmployeeServiceException e)
            {
                return Problem(e.Message, statusCode: 400);
            }
        }

        [HttpDelete("{registrationNumber}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult DeleteEmployee(int registrationNumber) =>
            CallServiceMethod(() => _service.DeleteEmployee(registrationNumber), NoContent());


        /// <summary>
        /// PUT /api/manager/{registrationNumber}
        /// </summary>

        [HttpPut("/api/manager/{registrationNumber}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateManager(
            int registrationNumber, [FromBody] UpdateManagerCommand cmd)
        {
            if (registrationNumber != cmd.RegistrationNumber)
                return Problem("Invalid registration number", statusCode: 400);
            return CallServiceMethod(() => _service.UpdateManager(cmd), NoContent());
        }

        /// <summary>
        /// PATCH /api/employees/{registrationNumber}/address
        /// </summary>
        [HttpPatch("{registrationNumber}/address")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateAddress(
            int registrationNumber, [FromBody] UpdateAddressCommand cmd) =>
            CallServiceMethod(() => _service.UpdateAddress(registrationNumber, cmd), NoContent());

        private IActionResult CallServiceMethod(Action action, IActionResult successResult)
        {
            try
            {
                action();
                return successResult;
            }
            catch (EmployeeServiceException e) when (e.IsNotFoundError == true)
            {
                return Problem(e.Message, statusCode: 404);
            }
            catch (EmployeeServiceException e)
            {
                return Problem(e.Message, statusCode: 400);
            }
        }
    }
}
