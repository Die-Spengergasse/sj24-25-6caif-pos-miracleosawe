using Microsoft.EntityFrameworkCore;
using SPG_Fachtheorie.Aufgabe1.Commands;
using SPG_Fachtheorie.Aufgabe1.Infrastructure;
using SPG_Fachtheorie.Aufgabe1.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPG_Fachtheorie.Aufgabe1.Services
{
    public class EmployeeService
    {
        private readonly AppointmentContext _db;

        public EmployeeService(AppointmentContext db)
        {
            _db = db;
        }
        // Stellt einen read-only Zugriff auf die Datenbank als Property bereit.
        public IQueryable<Employee> Employees => _db.Employees.AsQueryable();
        public Employee AddManager(NewManagerCommand cmd)
        {
            // Ist das Alter unter 30? Dann darf maximal 4000 Euro an Salary eingetragen
            // werden.
            if (DateTime.Now.Year - cmd.Birthdate.Year < 30 && cmd.Salary > 4000)
                throw new EmployeeServiceException($"Invalid salary for Employee {cmd.LastName}.");
            if (_db.Managers.Count() > 3)
                throw new EmployeeServiceException($"Only 3 managers are allowed.");
            
            var manager = new Manager(
                cmd.RegistrationNumber, cmd.FirstName, cmd.LastName,
                cmd.Birthdate, cmd.Salary,
                cmd.Address is null ? null : new Address(cmd.Address.Street, cmd.Address.Zip, cmd.Address.City),
                cmd.CarType);
            
            _db.Managers.Add(manager);
            SaveOrThrow();
            return manager;
        }

        public Employee AddCashier(NewCashierCommand cmd)
        {
            var cashier = new Cashier(
                cmd.RegistrationNumber, cmd.FirstName, cmd.LastName,
                cmd.Birthdate, cmd.Salary,
                cmd.Address is null ? null : new Address(cmd.Address.Street, cmd.Address.Zip, cmd.Address.City),
                cmd.JobSpezialisation);

            _db.Cashiers.Add(cashier);
            SaveOrThrow();
            return cashier;
        }

        public void DeleteEmployee(int registrationNumber)
        {
            var paymentItems = _db.PaymentItems
                .Where(p => p.Payment.Employee.RegistrationNumber == registrationNumber)
                .ToList();
            var payments = _db.Payments
                .Where(p => p.Employee.RegistrationNumber == registrationNumber)
                .ToList();
            var employee = _db.Employees
                .FirstOrDefault(e => e.RegistrationNumber == registrationNumber);
            if (employee is null) return;
            try
            {
                _db.PaymentItems.RemoveRange(paymentItems);
                _db.SaveChanges();
                _db.Payments.RemoveRange(payments);
                _db.SaveChanges();
                _db.Employees.Remove(employee);
                _db.SaveChanges();
            }
            catch (DbUpdateException e)
            {
                throw new EmployeeServiceException(e.InnerException?.Message ?? e.Message);
            }
        }

        public void UpdateManager(UpdateManagerCommand cmd)
        {
            var manager = _db.Managers
                .FirstOrDefault(m => m.RegistrationNumber == cmd.RegistrationNumber);
            if (manager is null)
                throw new EmployeeServiceException(
                    "Manager not found.", isNotFoundError: true);
            if (manager.LastUpdate != cmd.LastUpdate)
                throw new EmployeeServiceException("Manager has changed.");
            manager.FirstName = cmd.FirstName;
            manager.LastName = cmd.LastName;

            manager.Address = cmd.Address is not null
                ? new Address(cmd.Address.Street, cmd.Address.Zip, cmd.Address.City)
                : null;

            manager.CarType = cmd.CarType;
            manager.LastUpdate = DateTime.UtcNow;
            SaveOrThrow();
        }

        private void SaveOrThrow()
        {
            try
            {
                _db.SaveChanges();
            }
            catch (DbUpdateException e)
            {
                throw new EmployeeServiceException(e.InnerException?.Message ?? e.Message);
            }
        }

        public void UpdateAddress(int registrationNumber, UpdateAddressCommand cmd)
        {
            var employee = _db.Employees
                            .FirstOrDefault(e => e.RegistrationNumber == registrationNumber);
            if (employee is null)
                throw new EmployeeServiceException( "Employee not found.", isNotFoundError: true);

            employee.Address = new Address(cmd.Street, cmd.Zip, cmd.City);
            SaveOrThrow();
        }
    }
}
