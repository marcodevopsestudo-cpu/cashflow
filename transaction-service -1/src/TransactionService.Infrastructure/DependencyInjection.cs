using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TransactionService.Application.Abstractions.Idempotency;
using TransactionService.Application.Abstractions.Messaging;
using TransactionService.Application.Abstractions.Persistence;
using TransactionService.Infrastructure.Configuration;
using TransactionService.Infrastructure.Idempotency;
using TransactionService.Infrastructure.Messaging;
using TransactionService.Infrastructure.Persistence;
using TransactionService.Infrastructure.Persistence.Repositories;

namespace TransactionService.Infrastructure;

/// <summary>
/// Registers infrastructure services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds infrastructure services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration root.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? configuration["ConnectionStrings:Postgres"]
            ?? throw new InvalidOperationException("ConnectionStrings:Postgres was not configured.");

        services.Configure<ServiceBusOptions>(configuration.GetSection("ServiceBus"));

        services.AddDbContext<TransactionDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IIdempotencyRepository, IdempotencyRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<TransactionDbContext>());
        services.AddSingleton<IIntegrationEventPublisher, AzureServiceBusEventPublisher>();
        services.AddScoped<IOutboxProcessor, OutboxProcessor>();
        services.AddSingleton<IRequestHashService, CreateTransactionRequestHashService>();

        return services;
    }
}
