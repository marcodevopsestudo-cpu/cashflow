using ConsolidationService.Application.Abstractions;
using ConsolidationService.Application.Behaviors;
using ConsolidationService.Application.Orchestration;
using ConsolidationService.Application.Steps;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ConsolidationService.Application;

/// <summary>
/// Provides dependency injection registration for application-layer services,
/// including MediatR handlers, validators, pipeline behaviors, workflows, and processing steps.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers application-layer services in the dependency injection container.
    /// </summary>
    /// <param name="services">
    /// The service collection used to register application dependencies.
    /// </param>
    /// <returns>
    /// The same <see cref="IServiceCollection"/> instance so that additional calls can be chained.
    /// </returns>
    /// <remarks>
    /// This method registers:
    /// <list type="bullet">
    /// <item><description>MediatR handlers from the current assembly.</description></item>
    /// <item><description>FluentValidation validators from the current assembly.</description></item>
    /// <item><description>The validation pipeline behavior.</description></item>
    /// <item><description>The consolidation workflow orchestration service.</description></item>
    /// <item><description>The individual workflow processing steps.</description></item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddScoped<IConsolidationWorkflow, ConsolidationWorkflow>();

        services.AddScoped<RegisterBatchStep>();
        services.AddScoped<LoadTransactionsStep>();
        services.AddScoped<ValidateTransactionsStep>();
        services.AddScoped<AggregateTransactionsStep>();
        services.AddScoped<UpsertDailyBalanceStep>();
        services.AddScoped<FinalizeBatchStep>();

        return services;
    }
}
