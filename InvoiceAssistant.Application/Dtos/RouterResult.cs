using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace InvoiceAssistant.Application.Dtos;

public sealed class RouterResult
{
    [JsonPropertyName("function")]
    public string? Function { get; set; }

    [JsonPropertyName("params")]
    public Dictionary<string, string>? Params { get; set; }

    [JsonPropertyName("missing")]
    public List<string>? Missing { get; set; }

    [JsonPropertyName("confidence")]
    public double? Confidence { get; set; }

    [JsonPropertyName("clarification")]
    public string? Clarification { get; set; }
}
