using ConsolidationService.Application.Abstractions;
using ConsolidationService.Infrastructure.Configuration;
using ConsolidationService.Infrastructure.Data;
using ConsolidationService.Infrastructure.Persistence;
using ConsolidationService.Infrastructure.Resilience;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ConsolidationService.Infrastructure.DependencyInjection;

/// <summary>
/// Registers infrastructure services.
/// </summary>
public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<PostgresOptions>(
            configuration.GetSection(PostgresOptions.SectionName));

        services.AddSingleton<NpgsqlConnectionFactory>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IDailyBalanceRepository, DailyBalanceRepository>();
        services.AddScoped<IDailyBatchRepository, DailyBatchRepository>();
        services.AddScoped<ITransactionProcessingErrorRepository, TransactionProcessingErrorRepository>();

        return services;
    }
}
