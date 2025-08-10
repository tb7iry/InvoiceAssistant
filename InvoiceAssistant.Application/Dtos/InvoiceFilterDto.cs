
namespace InvoiceAssistant.Application.Dtos;

public sealed record InvoiceFilterDto
{
    public DateTimeOffset? StartDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
    public string? InvoiceNumber { get; init; }
    public string? Customer { get; init; }
    public string? Salesperson { get; init; }
    public string? Status { get; init; }
    public string? Currency { get; init; }
    public decimal? MinAmount { get; init; }
    public decimal? MaxAmount { get; init; }
    public string? Branch { get; init; }
    public string? ItemSku { get; init; }
    public int? TopN { get; init; }
    public bool? OverdueOnly { get; init; }
}
