
namespace InvoiceAssistant.Application.Contracts;

public interface ILLMClient
{
    Task<string> AskAsync(string prompt, string model = "mistral");
}
