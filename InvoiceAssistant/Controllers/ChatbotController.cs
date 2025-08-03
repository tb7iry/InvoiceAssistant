using InvoiceAssistant.Application.Contracts;
using InvoiceAssistant.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceAssistant.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ChatbotController(IChatbotService chatbotService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<string>> Ask([FromBody] QuestionDto userQuestion)
        => Ok(await chatbotService.AskQuestionAsync(userQuestion));
}
