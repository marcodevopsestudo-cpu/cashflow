using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
            ?? configuration.GetSetting("ConnectionStrings:Postgres", "ConnectionStrings__Postgres")
            ?? throw new InvalidOperationException(
                "Postgres connection string not found. Configure ConnectionStrings__Postgres.");

        var serviceBusOptions = BuildServiceBusOptions(configuration);

        services.AddOptions<ServiceBusOptions>()
            .Configure(options =>
            {
                options.TopicName = serviceBusOptions.TopicName;
                options.FullyQualifiedNamespace = serviceBusOptions.FullyQualifiedNamespace;
                options.ConnectionString = serviceBusOptions.ConnectionString;
                options.UseManagedIdentity = serviceBusOptions.UseManagedIdentity;
            });

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

    private static ServiceBusOptions BuildServiceBusOptions(IConfiguration configuration)
    {
        var section = configuration.GetSection("ServiceBus");

        var options = new ServiceBusOptions
        {
            TopicName = configuration.GetSetting(
                "ServiceBus:TopicName",
                "ServiceBus__TopicName",
                "TopicName") ?? section[nameof(ServiceBusOptions.TopicName)] ?? string.Empty,
            FullyQualifiedNamespace = configuration.GetSetting(
                "ServiceBus:FullyQualifiedNamespace",
                "ServiceBus__FullyQualifiedNamespace") ?? section[nameof(ServiceBusOptions.FullyQualifiedNamespace)],
            ConnectionString = configuration.GetSetting(
                "ServiceBus:ConnectionString",
                "ServiceBus__ConnectionString") ?? section[nameof(ServiceBusOptions.ConnectionString)],
            UseManagedIdentity = GetBooleanSetting(
                configuration,
                defaultValue: true,
                "ServiceBus:UseManagedIdentity",
                "ServiceBus__UseManagedIdentity")
        };

        return options;
    }

    private static bool GetBooleanSetting(IConfiguration configuration, bool defaultValue, params string[] keys)
    {
        var rawValue = configuration.GetSetting(keys);
        return bool.TryParse(rawValue, out var parsedValue) ? parsedValue : defaultValue;
    }
}
