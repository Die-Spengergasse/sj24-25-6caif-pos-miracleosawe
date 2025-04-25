using Microsoft.EntityFrameworkCore;
using SPG_Fachtheorie.Aufgabe1.Commands;
using SPG_Fachtheorie.Aufgabe1.Infrastructure;
using SPG_Fachtheorie.Aufgabe1.Model;
using SPG_Fachtheorie.Aufgabe1.Services;
using System;
using System.Linq;
using Xunit;

namespace SPG_Fachtheorie.Aufgabe1.Test
{
    public class PaymentServiceTests
    {
        private AppointmentContext GetEmptyDbContext()
        {
            var options = new DbContextOptionsBuilder()
                .UseSqlite(@"Data Source=cash.db") // In-Memory wäre besser für Tests, aber behalte Konsistenz
                .Options;

            var db = new AppointmentContext(options);
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            return db;
        }

        [Theory]
        [InlineData(-1, 1001, "Cash", "Invalid cashdesk")] // Ungültige Kasse
        [InlineData(999, 1001, "Cash", "Invalid cashdesk")] // Ungültige Kasse (existiert nicht)
        [InlineData(1, -1, "Cash", "Invalid employee")] // Ungültiger Mitarbeiter
        [InlineData(1, 9999, "Cash", "Invalid employee")] // Ungültiger Mitarbeiter (existiert nicht)
        [InlineData(1, 1001, "CreditCard", "Insufficient rights to create a credit card payment.")] // Kassierer darf keine Kreditkartenzahlung anlegen
        [InlineData(2, 1001, "Cash", "Open payment for cashdesk.")] // Bereits offene Zahlung für diese Kasse
        public void CreatePaymentExceptionsTest(int cashDeskNumber, int employeeRegistrationNumber, string paymentType, string expectedErrorMessage)
        {
            // ARRANGE
            using var db = GetEmptyDbContext();
            // Seed Data für abhängige Entitäten
            var cashier = new Cashier(1001, "FN", "LN", new DateOnly(2000, 1, 1), 2000, null, "JobSpec");
            var manager = new Manager(1002, "FNM", "LNM", new DateOnly(1990, 1, 1), null, null, "SUV");
            var cashDesk1 = new CashDesk(1);
            var cashDesk2 = new CashDesk(2);
            db.Employees.AddRange(cashier, manager);
            db.CashDesks.AddRange(cashDesk1, cashDesk2);
            // Seed Data für "Open payment for cashdesk" Testfall
            var existingPayment = new Payment(cashDesk2, DateTime.UtcNow, cashier, PaymentType.Cash);
            db.Payments.Add(existingPayment);
            db.SaveChanges();
            db.ChangeTracker.Clear(); // Wichtig, um Tracking zu lösen vor dem eigentlichen Test

            var service = new PaymentService(db);
            var cmd = new NewPaymentCommand(cashDeskNumber, paymentType, employeeRegistrationNumber);

            // ACT & ASSERT
            var ex = Assert.Throws<PaymentServiceException>(() => service.CreatePayment(cmd));
            Assert.Equal(expectedErrorMessage, ex.Message);
        }

        [Fact]
        public void CreatePaymentSuccessTest()
        {
            // ARRANGE
            using var db = GetEmptyDbContext();
            var cashier = new Cashier(1001, "FN", "LN", new DateOnly(2000, 1, 1), 2000, null, "JobSpec");
            var manager = new Manager(1002, "FNM", "LNM", new DateOnly(1990, 1, 1), null, null, "SUV");
            var cashDesk = new CashDesk(1);
            db.Employees.AddRange(cashier, manager);
            db.CashDesks.Add(cashDesk);
            db.SaveChanges();
            db.ChangeTracker.Clear();

            var service = new PaymentService(db);
            // Erfolgreicher Fall 1: Kassierer mit Bargeld
            var cmdCash = new NewPaymentCommand(1, "Cash", 1001 );
             // Erfolgreicher Fall 2: Manager mit Kreditkarte
            var cmdCreditCard = new NewPaymentCommand(1, "CreditCard", 1002);
            // ACT
            var paymentCash = service.CreatePayment(cmdCash);
            // Für den 2. Test brauchen wir eine neue Kasse oder müssen die erste Zahlung bestätigen
            var cashDesk2 = new CashDesk(2);
            db.CashDesks.Add(cashDesk2);
            db.SaveChanges();
            cmdCreditCard = new NewPaymentCommand(2, "CreditCard", 1002); // Update command mit neuer Kasse
            var paymentCreditCard = service.CreatePayment(cmdCreditCard);


            // ASSERT
            db.ChangeTracker.Clear(); // Wichtig, um frisch aus DB zu laden
            var paymentCashFromDb = db.Payments.Include(p => p.CashDesk).Include(p => p.Employee)
                                     .FirstOrDefault(p => p.Id == paymentCash.Id);
            var paymentCreditCardFromDb = db.Payments.Include(p => p.CashDesk).Include(p => p.Employee)
                                         .FirstOrDefault(p => p.Id == paymentCreditCard.Id);

            Assert.NotNull(paymentCashFromDb);
            Assert.Equal(1, paymentCashFromDb.CashDesk.Number);
            Assert.Equal(1001, paymentCashFromDb.Employee.RegistrationNumber);
            Assert.Equal(PaymentType.Cash, paymentCashFromDb.PaymentType);
            Assert.Null(paymentCashFromDb.Confirmed); // Noch nicht bestätigt

            Assert.NotNull(paymentCreditCardFromDb);
            Assert.Equal(2, paymentCreditCardFromDb.CashDesk.Number);
            Assert.Equal(1002, paymentCreditCardFromDb.Employee.RegistrationNumber);
            Assert.Equal(PaymentType.CreditCard, paymentCreditCardFromDb.PaymentType);
            Assert.Null(paymentCreditCardFromDb.Confirmed); // Noch nicht bestätigt
        }

        [Theory]
        [InlineData(999, true, "Payment not found")]       // Payment existiert nicht
        [InlineData(1, false, "Payment already confirmed")] // Payment bereits bestätigt
        public void ConfirmPaymentExceptionsTest(int paymentId, bool isNotFoundError, string expectedErrorMessage)
        {
            // ARRANGE
            using var db = GetEmptyDbContext();
            var cashier = new Cashier(1001, "FN", "LN", new DateOnly(2000, 1, 1), 2000, null, "JobSpec");
            var cashDesk = new CashDesk(1);
            db.Employees.Add(cashier);
            db.CashDesks.Add(cashDesk);

            var payment = new Payment(cashDesk, DateTime.UtcNow, cashier, PaymentType.Cash);
            if (!isNotFoundError) // Nur hinzufügen, wenn der Testfall nicht "not found" ist
            {
                 if (expectedErrorMessage == "Payment already confirmed")
                 {
                     payment.Confirmed = DateTime.UtcNow.AddMinutes(-5); // Setze Bestätigungsdatum in der Vergangenheit
                 }
                 db.Payments.Add(payment);
            }
            db.SaveChanges();
            db.ChangeTracker.Clear();

            var service = new PaymentService(db);

            // ACT & ASSERT
            var ex = Assert.Throws<PaymentServiceException>(() => service.ConfirmPayment(paymentId));
            Assert.Equal(expectedErrorMessage, ex.Message);
            if (isNotFoundError) {
                 Assert.True(ex.IsNotFoundError);
            }
        }

        [Fact]
        public void ConfirmPaymentSuccessTest()
        {
            // ARRANGE
            using var db = GetEmptyDbContext();
            var cashier = new Cashier(1001, "FN", "LN", new DateOnly(2000, 1, 1), 2000, null, "JobSpec");
            var cashDesk = new CashDesk(1);
            db.Employees.Add(cashier);
            db.CashDesks.Add(cashDesk);
            var payment = new Payment(cashDesk, DateTime.UtcNow.AddMinutes(-10), cashier, PaymentType.Cash);
            db.Payments.Add(payment);
            db.SaveChanges();
            int paymentIdToConfirm = payment.Id;
            db.ChangeTracker.Clear();

            var service = new PaymentService(db);

            // ACT
            service.ConfirmPayment(paymentIdToConfirm);

            // ASSERT
            db.ChangeTracker.Clear();
            var confirmedPayment = db.Payments.Find(paymentIdToConfirm);
            Assert.NotNull(confirmedPayment);
            Assert.NotNull(confirmedPayment.Confirmed);
            Assert.True(confirmedPayment.Confirmed.Value > DateTime.UtcNow.AddMinutes(-1)); // Zeitpunkt sollte aktuell sein
        }

        [Theory]
        [InlineData(999, "Artikel A", 1, 10.0, "Payment not found.")]      // Payment existiert nicht
        [InlineData(1, "Artikel B", 2, 5.0, "Payment already confirmed.")] // Payment bereits bestätigt
        public void AddPaymentItemExceptionsTest(int paymentId, string articleName, int amount, decimal price, string expectedErrorMessage)
        {
            // ARRANGE
            using var db = GetEmptyDbContext();
            var cashier = new Cashier(1001, "FN", "LN", new DateOnly(2000, 1, 1), 2000, null, "JobSpec");
            var cashDesk = new CashDesk(1);
            db.Employees.Add(cashier);
            db.CashDesks.Add(cashDesk);

            var payment = new Payment(cashDesk, DateTime.UtcNow, cashier, PaymentType.Cash);
            // Füge Payment nur hinzu, wenn der Testfall nicht "not found" ist.
            if (expectedErrorMessage != "Payment not found.")
            {
                if (expectedErrorMessage == "Payment already confirmed.")
                {
                    payment.Confirmed = DateTime.UtcNow.AddMinutes(-5); // Setze Bestätigungsdatum
                }
                db.Payments.Add(payment);
            }

            db.SaveChanges();
            int actualPaymentId = (expectedErrorMessage != "Payment not found.") ? payment.Id : paymentId;
            db.ChangeTracker.Clear();

            var service = new PaymentService(db);
            var cmd = new NewPaymentItemCommand(articleName,  amount, price, actualPaymentId);

            // ACT & ASSERT
            var ex = Assert.Throws<PaymentServiceException>(() => service.AddPaymentItem(cmd));
            Assert.Equal(expectedErrorMessage, ex.Message);
        }

        [Fact]
        public void AddPaymentItemSuccessTest()
        {
            // ARRANGE
            using var db = GetEmptyDbContext();
            var cashier = new Cashier(1001, "FN", "LN", new DateOnly(2000, 1, 1), 2000, null, "JobSpec");
            var cashDesk = new CashDesk(1);
            db.Employees.Add(cashier);
            db.CashDesks.Add(cashDesk);
            var payment = new Payment(cashDesk, DateTime.UtcNow.AddMinutes(-10), cashier, PaymentType.Cash);
            db.Payments.Add(payment);
            db.SaveChanges();
            int paymentId = payment.Id;
            db.ChangeTracker.Clear();

            var service = new PaymentService(db);
            var cmd = new NewPaymentItemCommand( "Test Artikel", 3, 15.99m, paymentId);

            // ACT
            service.AddPaymentItem(cmd);

            // ASSERT
            db.ChangeTracker.Clear();
            var paymentFromDb = db.Payments.Include(p => p.PaymentItems).FirstOrDefault(p => p.Id == paymentId);
            Assert.NotNull(paymentFromDb);
            Assert.Single(paymentFromDb.PaymentItems); // Es sollte genau ein Item hinzugefügt worden sein
            var item = paymentFromDb.PaymentItems.First();
            Assert.Equal("Test Artikel", item.ArticleName);
            Assert.Equal(3, item.Amount);
            Assert.Equal(15.99m, item.Price);
            Assert.Null(paymentFromDb.Confirmed); // Payment sollte noch nicht bestätigt sein
        }

        [Theory]
        [InlineData(999, false, true, "Payment not found.")]  // Payment existiert nicht
        [InlineData(1, false, false, "Payment has payment items.")] // Payment hat Items, deleteItems=false
        public void DeletePaymentExceptionsTest(int paymentId, bool deleteItems, bool isNotFoundError, string expectedErrorMessage)
        {
            // ARRANGE
            using var db = GetEmptyDbContext();
            var cashier = new Cashier(1001, "FN", "LN", new DateOnly(2000, 1, 1), 2000, null, "JobSpec");
            var cashDesk = new CashDesk(1);
            db.Employees.Add(cashier);
            db.CashDesks.Add(cashDesk);

            int actualPaymentId = paymentId;
            if (!isNotFoundError) // Nur hinzufügen, wenn der Testfall nicht "not found" ist
            {
                var payment = new Payment(cashDesk, DateTime.UtcNow, cashier, PaymentType.Cash);
                db.Payments.Add(payment);
                // Füge Item hinzu für den Fall "Payment has payment items."
                if (expectedErrorMessage == "Payment has payment items.")
                {
                     var item = new PaymentItem("Artikel C", 1, 1.0m, payment);
                     db.PaymentItems.Add(item);
                }
                 db.SaveChanges();
                 actualPaymentId = payment.Id; // Korrekte Id verwenden
            }
            db.ChangeTracker.Clear();

            var service = new PaymentService(db);

            // ACT & ASSERT
            var ex = Assert.Throws<PaymentServiceException>(() => service.DeletePayment(actualPaymentId, deleteItems));
            Assert.Equal(expectedErrorMessage, ex.Message);
             if (isNotFoundError) {
                 Assert.True(ex.IsNotFoundError);
            }
        }

        [Fact]
        public void DeletePaymentWithoutItemsSuccessTest()
        {
            // ARRANGE
            using var db = GetEmptyDbContext();
            var cashier = new Cashier(1001, "FN", "LN", new DateOnly(2000, 1, 1), 2000, null, "JobSpec");
            var cashDesk = new CashDesk(1);
            db.Employees.Add(cashier);
            db.CashDesks.Add(cashDesk);
            var payment = new Payment(cashDesk, DateTime.UtcNow.AddMinutes(-10), cashier, PaymentType.Cash);
            db.Payments.Add(payment);
            db.SaveChanges();
            int paymentIdToDelete = payment.Id;
            db.ChangeTracker.Clear();

            var service = new PaymentService(db);

            // ACT
            service.DeletePayment(paymentIdToDelete, false); // deleteItems = false, da keine Items da sind

            // ASSERT
            db.ChangeTracker.Clear();
            var deletedPayment = db.Payments.Find(paymentIdToDelete);
            Assert.Null(deletedPayment);
        }

         [Fact]
        public void DeletePaymentWithItemsSuccessTest()
        {
            // ARRANGE
            using var db = GetEmptyDbContext();
            var cashier = new Cashier(1001, "FN", "LN", new DateOnly(2000, 1, 1), 2000, null, "JobSpec");
            var cashDesk = new CashDesk(1);
            db.Employees.Add(cashier);
            db.CashDesks.Add(cashDesk);
            var payment = new Payment(cashDesk, DateTime.UtcNow.AddMinutes(-10), cashier, PaymentType.Cash);
            db.Payments.Add(payment);
            // Items hinzufügen
            var item1 = new PaymentItem("Item 1", 1, 10, payment);
            var item2 = new PaymentItem("Item 2", 2, 20, payment);
            db.PaymentItems.AddRange(item1, item2);
            db.SaveChanges();
            int paymentIdToDelete = payment.Id;
            int item1Id = item1.Id;
            int item2Id = item2.Id;
            db.ChangeTracker.Clear();

            var service = new PaymentService(db);

            // ACT
            service.DeletePayment(paymentIdToDelete, true); // deleteItems = true

            // ASSERT
            db.ChangeTracker.Clear();
            var deletedPayment = db.Payments.Find(paymentIdToDelete);
            var deletedItem1 = db.PaymentItems.Find(item1Id);
            var deletedItem2 = db.PaymentItems.Find(item2Id);
            Assert.Null(deletedPayment);
            Assert.Null(deletedItem1); // Items sollten auch gelöscht sein
            Assert.Null(deletedItem2);
        }

        // Hier kommen die Tests hin
    }
} 