using InvoiceAssistant.Application.Contracts;
using System.Text;
using System.Text.Json;


namespace InvoiceAssistant.Infrastructure.LLM;

public class OllamaClient(HttpClient httpClient) : ILLMClient
{
    private const string Endpoint = "http://localhost:11434/api/generate";

    public async Task<string> AskAsync(string prompt, string model = "llama3.1")
    {
        var requestBody = new
        {
            model,
            prompt,
            stream = false
        };
        var requestJson = JsonSerializer.Serialize(requestBody);

        using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint)
        {
            Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
        };

        using var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<OllamaResponse>(responseJson);

        return result?.response ?? "[No answer from model]";
    }

    private class OllamaResponse
    {
        public string response { get; set; }
    }
}
