using InvoiceAssistant.Application.Common;
using InvoiceAssistant.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using System.Text.Json;

namespace InvoiceAssistant.Api.Middleware;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> _logger) : IExceptionHandler
{

    public async ValueTask<bool> TryHandleAsync(
             HttpContext httpContext,
             Exception exception,
             CancellationToken cancellationToken)
    {
        (int statusCode, string message) = exception switch
        {
            NotFoundException notFoundEx => ((int)HttpStatusCode.NotFound, notFoundEx.Message),
            ValidationException validationEx => ((int)HttpStatusCode.BadRequest, validationEx.Message),

            _ => LogAndReturnGenericError()
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        var response = new ApiResponse<string>
        {
            Success = false,
            Message = message,
            Data = null
        };

        var json = JsonSerializer.Serialize(response);
        await httpContext.Response.WriteAsync(json, cancellationToken);

        return true;

        // Local function for default case to log and return default values
        (int, string) LogAndReturnGenericError()
        {
            _logger.LogError(exception, "Unhandled exception occurred.");
            return ((int)HttpStatusCode.InternalServerError, "An unexpected error occurred. Please try again later.");
        }
    }
}
