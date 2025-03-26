using System;
using System.Collections.Generic;

namespace SPG_Fachtheorie.Aufgabe1.Model
{
    public class Payment
    {
#pragma warning disable CS8618 
        protected Payment() { }
#pragma warning restore CS8618 
        public Payment(CashDesk cashDesk, DateTime paymentDateTime, Employee employee, PaymentType paymentType)
        {
            CashDesk = cashDesk;
            PaymentDateTime = paymentDateTime;
            Employee = employee;
            PaymentType = paymentType;
        }

        public int Id { get; set; }
        public CashDesk CashDesk { get; set; }
        public DateTime PaymentDateTime { get; set; }
        public Employee Employee { get; set; }
        public PaymentType PaymentType { get; set; }
        public List<PaymentItem> PaymentItems { get; } = new();
    }
}