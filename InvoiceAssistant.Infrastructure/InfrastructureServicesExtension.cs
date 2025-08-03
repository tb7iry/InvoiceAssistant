using InvoiceAssistant.Application.Contracts;
using InvoiceAssistant.Infrastructure.Db;
using InvoiceAssistant.Infrastructure.LLM;
using InvoiceAssistant.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;


namespace InvoiceAssistant.Infrastructure;

public static class InfrastructureServicesExtension
{
    public static IServiceCollection ConfigureInfrastructureServices(this IServiceCollection services, string connectionString)
    {

        services.AddDbContext<InvoiceAssistantDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        services.AddHttpClient<ILLMClient, OllamaClient>();

        return services;
    }
}
