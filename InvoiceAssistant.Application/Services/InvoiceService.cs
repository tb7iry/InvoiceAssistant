
using Microsoft.EntityFrameworkCore;
using InvoiceAssistant.Application.Common;
using InvoiceAssistant.Application.Contracts;
using InvoiceAssistant.Application.Dtos;
using InvoiceAssistant.Application.Exceptions;
using InvoiceAssistant.Domain.Entites;

namespace InvoiceAssistant.Application.Services;

public class InvoiceService(IGenericRepository<Invoice> invoiceRepo) : IInvoiceService
{
    public async Task<ApiResponse<PaginatedList<InvoiceListViewDto>>> GetAllPaginatedAsync(int pageIndex, int pageSize)
    {
        var paginatedInvoices = await invoiceRepo.GetPaginatedAsync(pageIndex, pageSize);
        var dtoList = paginatedInvoices.Items.Select(MapToListViewDto).ToList();
        var paginatedDto = new PaginatedList<InvoiceListViewDto>(
            dtoList,
            paginatedInvoices.TotalCount,
            paginatedInvoices.PageIndex,
            paginatedInvoices.PageSize
        );
        return new ApiResponse<PaginatedList<InvoiceListViewDto>>(paginatedDto);
    }

    public async Task<ApiResponse<InvoiceDto>> GetByIdAsync(object id)
    {
        var invoice = await invoiceRepo
        .GetQueryable()
        .Where(i => i.Id.Equals(id))
        .Include(i => i.InvoiceDetails)
        .FirstOrDefaultAsync();

        if (invoice is null)
            throw new NotFoundException("Invoice not found.");

        return new ApiResponse<InvoiceDto>(MapToDto(invoice));
    }

    public async Task<ApiResponse<InvoiceDto>> AddAsync(InvoiceDto invoiceDto)
    {
        var invoice = MapToEntity(invoiceDto);
        await invoiceRepo.AddAsync(invoice);

        // Retrieve after insert (to get DB-generated fields and navigation)
        var addedInvoice = await invoiceRepo.GetByIdAsync(invoice.Id);
        return new ApiResponse<InvoiceDto>(MapToDto(addedInvoice), true, "Invoice created successfully.");
    }

    public async Task<ApiResponse<InvoiceDto>> UpdateAsync(InvoiceDto invoiceDto)
    {
        // 1. Fetch the existing invoice, including details
        var invoice = await invoiceRepo
            .GetQueryable()
            .Include(i => i.InvoiceDetails)
            .FirstOrDefaultAsync(i => i.Id == invoiceDto.Id);

        if (invoice is null)
            throw new NotFoundException("Invoice not found.");

        // 2. Update main invoice fields
        invoice.InvoiceNumber = invoiceDto.InvoiceNumber;
        invoice.ClientName = invoiceDto.ClientName;
        invoice.IssueDate = invoiceDto.IssueDate;
        invoice.TotalAmount = invoiceDto.TotalAmount;

        // 3. Update details (add new, update existing, remove missing)
        // Build a dictionary for fast lookup
        var existingDetails = invoice.InvoiceDetails.ToDictionary(d => d.Id);

        var dtoDetailIds = invoiceDto.InvoiceDetails?.Select(d => d.Id).ToHashSet() ?? new HashSet<int>();

        // Update or add details
        foreach (var dtoDetail in invoiceDto.InvoiceDetails ?? new List<InvoiceDetailDto>())
        {
            if (dtoDetail.Id == 0) // New detail (Id==0 or not set)
            {
                invoice.InvoiceDetails.Add(new InvoiceDetail
                {
                    ItemName = dtoDetail.ItemName,
                    Quantity = dtoDetail.Quantity,
                    UnitPrice = dtoDetail.UnitPrice
                });
            }
            else if (existingDetails.TryGetValue(dtoDetail.Id, out var existingDetail))
            {
                existingDetail.ItemName = dtoDetail.ItemName;
                existingDetail.Quantity = dtoDetail.Quantity;
                existingDetail.UnitPrice = dtoDetail.UnitPrice;
            }
        }

        // Remove details not in DTO
        var toRemove = invoice.InvoiceDetails
            .Where(d => !dtoDetailIds.Contains(d.Id))
            .ToList();

        foreach (var removeDetail in toRemove)
            invoice.InvoiceDetails.Remove(removeDetail);

        // 4. Persist changes
        await invoiceRepo.UpdateAsync(invoice);

        // 5. Fetch updated invoice (including details)
        var updatedInvoice = await invoiceRepo
            .GetQueryable()
            .Include(i => i.InvoiceDetails)
            .FirstOrDefaultAsync(i => i.Id == invoiceDto.Id);

        return new ApiResponse<InvoiceDto>(MapToDto(updatedInvoice), true, "Invoice updated successfully.");
    }

    // ---- Mapping Helpers ----

    private static InvoiceDto MapToDto(Invoice invoice)
    {
        return new()
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            ClientName = invoice.ClientName,
            IssueDate = invoice.IssueDate,
            TotalAmount = invoice.TotalAmount,
            InvoiceDetails = invoice.InvoiceDetails?
            .Select(d => new InvoiceDetailDto
            {
                Id = d.Id,
                ItemName = d.ItemName,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice
            }).ToList()
        };
    }
    private static InvoiceListViewDto MapToListViewDto(Invoice invoice)
    {
        return new()
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            ClientName = invoice.ClientName,
            IssueDate = invoice.IssueDate,
            TotalAmount = invoice.TotalAmount
        };
    }

    private static Invoice MapToEntity(InvoiceDto dto)
    {
        return new()
        {
            Id = dto.Id,
            InvoiceNumber = dto.InvoiceNumber,
            ClientName = dto.ClientName,
            IssueDate = dto.IssueDate,
            TotalAmount = dto.TotalAmount,
            InvoiceDetails = dto.InvoiceDetails?
            .Select(d => new InvoiceDetail
            {
                Id = d.Id,
                ItemName = d.ItemName,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice
            }).ToList()
        };
    }

    public async Task<ApiResponse<IEnumerable<InvoiceDto>>> GetAllAsync()
    {
        var invoices = await invoiceRepo.GetAllAsync();
        var invoiceDtos = invoices.Select(MapToDto);
        return new ApiResponse<IEnumerable<InvoiceDto>>(invoiceDtos, true);
    }

    public async Task<List<InvoiceDto>> GetInvoicesAsync(InvoiceFilterDto filter)
    {
        var query = invoiceRepo.GetQueryable();

        if (filter.StartDate.HasValue)
            query = query.Where(i => i.IssueDate >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(i => i.IssueDate <= filter.EndDate.Value);

        if (!string.IsNullOrWhiteSpace(filter.InvoiceNumber))
            query = query.Where(i => i.InvoiceNumber == filter.InvoiceNumber);

        if (!string.IsNullOrWhiteSpace(filter.ClientName))
            query = query.Where(i => i.ClientName.Contains(filter.ClientName));

        if (filter.MinTotalAmount.HasValue)
            query = query.Where(i => i.TotalAmount >= filter.MinTotalAmount.Value);

        if (filter.MaxTotalAmount.HasValue)
            query = query.Where(i => i.TotalAmount <= filter.MaxTotalAmount.Value);

        // Include InvoiceDetails if needed for summaries
        query = query.Include(i => i.InvoiceDetails);

        var invoices = await query.ToListAsync();
        return invoices.Select(MapToDto).ToList();
    }
}
