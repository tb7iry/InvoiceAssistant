using InvoiceAssistant.Application.Common;
using InvoiceAssistant.Application.Contracts;
using InvoiceAssistant.Application.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceAssistant.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoicesController(IInvoiceService invoiceService) : ControllerBase
    {
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<InvoiceDto>>> GetById(int id)
            => Ok(await invoiceService.GetByIdAsync(id));

        [HttpGet("paginated")]
        public async Task<ActionResult<ApiResponse<PaginatedList<InvoiceDto>>>> GetAllPaginated(
            [FromQuery] int pageIndex = 0, [FromQuery] int pageSize = 10)
            => Ok(await invoiceService.GetAllPaginatedAsync(pageIndex, pageSize));

        [HttpPost]
        public async Task<ActionResult<ApiResponse<InvoiceDto>>> Add([FromBody] InvoiceDto dto)
            => Ok(await invoiceService.AddAsync(dto));

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<InvoiceDto>>> Update(int id, [FromBody] InvoiceDto dto)
            => id != dto.Id
                ? BadRequest(new ApiResponse<InvoiceDto>(null, false, "ID mismatch."))
                : Ok(await invoiceService.UpdateAsync(dto));
    }
}
