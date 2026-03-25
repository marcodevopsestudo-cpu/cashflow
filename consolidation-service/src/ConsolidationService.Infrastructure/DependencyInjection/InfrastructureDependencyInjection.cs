using ConsolidationService.Application.Abstractions;
using ConsolidationService.Infrastructure.Data;
using ConsolidationService.Infrastructure.Persistence;
using ConsolidationService.Infrastructure.Resilience;
using ConsolidationService.Infrastructure.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConsolidationService.Infrastructure.DependencyInjection;

/// <summary>
/// Provides dependency injection registration for infrastructure-layer services.
/// </summary>
public static class InfrastructureDependencyInjection
{
    /// <summary>
    /// Registers infrastructure services in the dependency injection container.
    /// </summary>
    /// <param name="services">
    /// The service collection used to register infrastructure dependencies.
    /// </param>
    /// <param name="configuration">
    /// The application configuration used to bind infrastructure settings.
    /// </param>
    /// <returns>
    /// The same <see cref="IServiceCollection"/> instance so that additional calls can be chained.
    /// </returns>
    /// <remarks>
    /// This method registers PostgreSQL configuration, connection factory, repositories,
    /// and resilience services required by the infrastructure layer.
    /// </remarks>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
        configuration.GetConnectionString("Postgres") ??
        configuration["ConnectionStrings__Postgres"] ??
        configuration["ConnectionStrings:Postgres"] ??
        configuration["Values:ConnectionStrings__Postgres"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                InfrastructureMessageCatalog.PostgresConnectionStringNotFound);
        }

        services.AddSingleton<NpgsqlConnectionFactory>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IDailyBalanceRepository, DailyBalanceRepository>();
        services.AddScoped<IDailyBatchRepository, DailyBatchRepository>();
        services.AddScoped<ITransactionProcessingErrorRepository, TransactionProcessingErrorRepository>();
        services.AddScoped<IRetryPolicy, ExponentialBackoffRetryPolicy>();

        return services;
    }
}
