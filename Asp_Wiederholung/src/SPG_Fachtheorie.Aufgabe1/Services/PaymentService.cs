using Microsoft.EntityFrameworkCore;
using SPG_Fachtheorie.Aufgabe1.Commands;
using SPG_Fachtheorie.Aufgabe1.Infrastructure;
using SPG_Fachtheorie.Aufgabe1.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPG_Fachtheorie.Aufgabe1.Services;
public class PaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ICashDeskRepository _cashDeskRepository;
    private readonly IEmployeeRepository _employeeRepository;

    public PaymentService(
        IPaymentRepository paymentRepository,
        ICashDeskRepository cashDeskRepository,
        IEmployeeRepository employeeRepository)
    {
        _paymentRepository = paymentRepository;
        _cashDeskRepository = cashDeskRepository;
        _employeeRepository = employeeRepository;
    }

    public Payment CreatePayment(NewPaymentCommand cmd)
    {
        if (cmd == null)
        {
            throw new ArgumentNullException(nameof(cmd), "Command cannot be null.");
        }

        var cashDesk = _cashDeskRepository.GetByNumber(cmd.CashDeskNumber);
        if (cashDesk == null)
        {
            throw new PaymentServiceException($"CashDesk with number {cmd.CashDeskNumber} not found.");
        }

        var existingPayment = _paymentRepository
            .GetPaymentsByCashDesk(cashDesk.Id)
            .FirstOrDefault(p => p.Confirmed == null);

        if (existingPayment != null)
        {
            throw new PaymentServiceException("An open payment already exists for this cash desk.");
        }

        var employee = _employeeRepository.GetByRegistrationNumber(cmd.EmployeeRegistrationNumber);
        if (employee == null)
        {
            throw new PaymentServiceException($"Employee with registration number {cmd.EmployeeRegistrationNumber} not found.");
        }

        if (!Enum.TryParse<PaymentType>(cmd.PaymentType, true, out var paymentType))
        {
            throw new PaymentServiceException($"Invalid payment type: {cmd.PaymentType}.");
        }

        if (paymentType == PaymentType.CreditCard && employee.Role != EmployeeRole.Manager)
        {
            throw new PaymentServiceException("Only managers can create credit card payments.");
        }

        var payment = new Payment(
            cashDesk,
            DateTime.UtcNow,
            employee,
            paymentType
        );

        _paymentRepository.Add(payment);
        return payment;
    }
}

