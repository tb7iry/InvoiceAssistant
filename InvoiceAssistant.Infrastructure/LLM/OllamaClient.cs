using InvoiceAssistant.Application.Contracts;
using System.Text;
using System.Text.Json;

public class OllamaClient : ILLMClient
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    private const string GeneratePath = "/api/generate";

    public OllamaClient(HttpClient httpClient)
    {
        _http = httpClient;
    }

    public async Task<string> AskAsync(
        string prompt,
        string model = "llama3.1",
        double? temperature = null,
        int? maxTokens = null,
        CancellationToken ct = default)
    {
        var options = new Dictionary<string, object>();

        if (temperature.HasValue)
            options["temperature"] = temperature.Value;

        if (maxTokens.HasValue)
            options["num_predict"] = maxTokens.Value;

        string format = "json";

        var requestBody = new
        {
            model,
            prompt,
            stream = false,
            options = options.Count > 0 ? options : null,
            format 
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, GeneratePath)
        {
            Content = new StringContent(JsonSerializer.Serialize(requestBody, _jsonOpts), Encoding.UTF8, "application/json")
        };

        using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        var payload = await JsonSerializer.DeserializeAsync<OllamaResponse>(
            await response.Content.ReadAsStreamAsync(ct),
            _jsonOpts,
            ct
        );

        if (payload is null || string.IsNullOrWhiteSpace(payload.response))
            return "[No answer from model]";

        return payload.response.Trim();
    }

    private sealed class OllamaResponse
    {
        public string response { get; set; } = "";
        public bool done { get; set; }
    }
}
