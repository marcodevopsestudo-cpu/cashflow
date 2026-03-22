using ConsolidationService.Application.Abstractions;
using ConsolidationService.Application.Behaviors;
using ConsolidationService.Application.Orchestration;
using ConsolidationService.Application.Steps;
using ConsolidationService.Application.Telemetry;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ConsolidationService.Application;

/// <summary>
/// Registers application-layer services.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddScoped<IConsolidationWorkflow, ConsolidationWorkflow>();
        services.AddScoped<ITelemetryContextAccessor, TelemetryContextAccessor>();

        services.AddScoped<IConsolidationWorkflowStep, RegisterBatchStep>();
        services.AddScoped<IConsolidationWorkflowStep, LoadTransactionsStep>();
        services.AddScoped<IConsolidationWorkflowStep, AggregateTransactionsStep>();
        services.AddScoped<IConsolidationWorkflowStep, UpsertDailyBalanceStep>();
        services.AddScoped<IConsolidationWorkflowStep, FinalizeBatchStep>();

        return services;
    }
}
