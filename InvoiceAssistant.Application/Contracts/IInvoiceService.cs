using InvoiceAssistant.Application.Common;
using InvoiceAssistant.Application.Dtos;

namespace InvoiceAssistant.Application.Contracts;

public interface IInvoiceService
{
    Task<ApiResponse<IEnumerable<InvoiceDto>>> GetAllAsync();
    Task<ApiResponse<InvoiceDto>> GetByIdAsync(object id);
    Task<ApiResponse<InvoiceDto>> AddAsync(InvoiceDto invoice);
    Task<ApiResponse<InvoiceDto>> UpdateAsync(InvoiceDto invoice);
    Task<ApiResponse<PaginatedList<InvoiceListViewDto>>> GetAllPaginatedAsync(int pageIndex, int pageSize);
    Task<List<InvoiceDto>> GetInvoicesAsync(InvoiceFilterDto filter , CancellationToken ct = default);
}
