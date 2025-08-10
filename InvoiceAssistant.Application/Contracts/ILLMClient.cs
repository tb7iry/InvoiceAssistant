
namespace InvoiceAssistant.Application.Contracts;

public interface ILLMClient
{
    Task<string> AskAsync(string prompt, 
        string model = "llama3.1", 
        double? temperature = null, 
        int? maxTokens = null, 
        CancellationToken ct = default);
}
