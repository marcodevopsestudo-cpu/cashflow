// using Microsoft.Azure.Functions.Worker;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Hosting;
// using TransactionService.Api.Configuration;
// using TransactionService.Api.Middlewares;
// using TransactionService.Api.Security;
// using TransactionService.Application;
// using TransactionService.Infrastructure;

// var host = new HostBuilder()
//     .ConfigureFunctionsWorkerDefaults(worker =>
//     {
//         worker.UseMiddleware<ExceptionHandlingMiddleware>();
//         worker.UseMiddleware<CorrelationIdMiddleware>();
//         worker.UseMiddleware<IdempotencyKeyMiddleware>();
//         worker.UseMiddleware<EntraAuthorizationMiddleware>();
//     })
//     .ConfigureServices((context, services) =>
//     {
//         services.AddApplication();
//         services.AddInfrastructure(context.Configuration);
//         services.AddApplicationInsightsTelemetryWorkerService();
//         services.ConfigureFunctionsApplicationInsights();
//         services.Configure<EntraAuthorizationOptions>(context.Configuration.GetSection(EntraAuthorizationOptions.SectionName));
//         services.AddSingleton<EntraAuthorizationEvaluator>();
//     })
//     .Build();

// await host.RunAsync();
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TransactionService.Api.Middlewares;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(worker =>
    {
        worker.UseMiddleware<ExceptionHandlingMiddleware>();
        worker.UseMiddleware<CorrelationIdMiddleware>();
        worker.UseMiddleware<IdempotencyKeyMiddleware>();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

await host.RunAsync();
