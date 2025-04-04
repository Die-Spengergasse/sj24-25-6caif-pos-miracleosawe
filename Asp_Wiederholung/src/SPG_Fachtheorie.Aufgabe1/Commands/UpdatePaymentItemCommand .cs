using System;
using System.ComponentModel.DataAnnotations;

namespace SPG_Fachtheorie.Aufgabe3.Commands
{
    /// <summary>
    /// {
    ///  "id": 1,
    ///  "articleName": "Coca Cola 0.5l",
    ///  "amount": 2,
    ///  "price": 1.65,
    ///  "paymentId": 1,
    ///  "lastUpdated": null
    ///}
    /// </summary>
    public record UpdatePaymentItemCommand(
        [Range(1, int.MaxValue)]
        int Id,
        [StringLength(255, MinimumLength = 1)]
        string ArticleName,
        [Range(1, int.MaxValue)]
        int Amount,
        [Range(1, 1_000_000)]
        decimal Price,
        [Range(1, int.MaxValue)]
        int PaymentId,
        DateTime? LastUpdated);
}
