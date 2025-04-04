namespace SPG_Fachtheorie.Aufgabe3.Dtos
{
    public record PaymentDetailDto(
        int Id, string EmployeeFirstName, string EmployeeLastName,
        int CashDeskNumber, string PaymentType,
        List<PaymentItemDto> PaymentItems);

    public record PaymentItemDto(
        string ArticleName, int Amount, decimal Price);
}
