

using InvoiceAssistant.Application.Dtos;

namespace InvoiceAssistant.Application.Contracts;

public interface IChatbotService
{
    Task<string> AskQuestionAsync(QuestionDto userQuestion);
}
