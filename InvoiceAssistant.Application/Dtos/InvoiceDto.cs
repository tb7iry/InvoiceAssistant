

namespace InvoiceAssistant.Application.Dtos;

public class InvoiceDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; }
    public string ClientName { get; set; }
    public DateTimeOffset IssueDate { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public string Status { get; set; } = "Unpaid";
    public string Currency { get; set; } = "EGP";
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public List<InvoiceDetailDto> InvoiceDetails { get; set; }
}
public class InvoiceListViewDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; }
    public string ClientName { get; set; }
    public DateTimeOffset IssueDate { get; set; }
    public decimal TotalAmount { get; set; }
}
