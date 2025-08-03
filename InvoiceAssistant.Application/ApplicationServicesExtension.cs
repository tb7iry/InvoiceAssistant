using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using InvoiceAssistant.Application.Common;
using InvoiceAssistant.Application.Validators;
using Microsoft.AspNetCore.Mvc;
using FluentValidation.AspNetCore;
using InvoiceAssistant.Application.Contracts;
using InvoiceAssistant.Application.Services;

namespace InvoiceAssistant.Application;

public static class ApplicationServicesExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register FluentValidation for all validators in this assembly
        services.AddValidatorsFromAssemblyContaining<InvoiceDtoValidator>();
        services.AddFluentValidationAutoValidation();

        // Configure ModelState error response to use ApiResponse<T>
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState.Values
                    .SelectMany(x => x.Errors)
                    .Select(x => x.ErrorMessage)
                    .ToList();

                var response = new ApiResponse<string>
                {
                    Success = false,
                    Message = string.Join(" | ", errors),
                    Data = null
                };
                return new BadRequestObjectResult(response);
            };
        });

        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IChatbotService, ChatbotService>();

        return services;
    }
}

