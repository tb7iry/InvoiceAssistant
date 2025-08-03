

namespace InvoiceAssistant.Application.Dtos;

public class InvoiceDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; }
    public string ClientName { get; set; }
    public DateTime IssueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public List<InvoiceDetailDto> InvoiceDetails { get; set; }
}
public class InvoiceListViewDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; }
    public string ClientName { get; set; }
    public DateTime IssueDate { get; set; }
    public decimal TotalAmount { get; set; }
}
