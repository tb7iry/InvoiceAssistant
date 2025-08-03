

namespace InvoiceAssistant.Application.Common;

public class ApiResponse<T>
{
    public T Data { get; set; }
    public bool Success { get; set; } = true;
    public string Message { get; set; }

    public ApiResponse() { }

    public ApiResponse(T data, bool success = true, string message = null)
    {
        Data = data;
        Success = success;
        Message = message;
    }
}
