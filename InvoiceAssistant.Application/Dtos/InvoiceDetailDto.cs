

namespace InvoiceAssistant.Application.Dtos;

public class InvoiceDetailDto
{
    public int Id { get; set; }
    public string ItemName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
