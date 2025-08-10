namespace InvoiceAssistant.Domain.Entites;

public class Invoice
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; }
    public string ClientName { get; set; }
    public DateTimeOffset IssueDate { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public string Status { get; set; } = "Unpaid";
    public string Currency { get; set; } = "EGP";
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; } = 0.00m;
    public string? Salesperson { get; set; }
    public string Branch { get; set; } = "Main Branch";
    public List<InvoiceDetail> InvoiceDetails { get; set; }
}
