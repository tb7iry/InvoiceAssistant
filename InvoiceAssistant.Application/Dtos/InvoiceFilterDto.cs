
namespace InvoiceAssistant.Application.Dtos;

public class InvoiceFilterDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string InvoiceNumber { get; set; }
    public string ClientName { get; set; }
    public decimal? MinTotalAmount { get; set; }
    public decimal? MaxTotalAmount { get; set; }

}
