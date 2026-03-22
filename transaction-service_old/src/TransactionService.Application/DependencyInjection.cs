using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TransactionService.Application.Common.Pipeline;

namespace TransactionService.Application;

/// <summary>
/// Registers application services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds application services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        return services;
    }
}
