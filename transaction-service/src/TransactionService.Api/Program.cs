using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TransactionService.Api.Common.Middleware;
using TransactionService.Api.Configuration;
using TransactionService.Api.Middlewares;
using TransactionService.Api.Security;
using TransactionService.Application;
using TransactionService.Infrastructure;

var host = new HostBuilder()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
              .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
              .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
              .AddEnvironmentVariables();

    }).ConfigureFunctionsWebApplication(worker =>
    {
        worker.UseMiddleware<ExceptionHandlingMiddleware>();
        worker.UseMiddleware<CorrelationIdMiddleware>();
        worker.UseMiddleware<IdempotencyKeyMiddleware>();
        worker.UseMiddleware<EntraAuthorizationMiddleware>();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddApplication();
        services.AddInfrastructure(context.Configuration);
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.AddOptions<EntraAuthorizationOptions>()
            .Configure(options =>
            {
                var authorizationSection = context.Configuration.GetSection(EntraAuthorizationOptions.SectionName);

                options.Enabled = bool.TryParse(
                    context.Configuration.GetSetting("Authorization:Enabled", "Authorization__Enabled") ?? authorizationSection[nameof(EntraAuthorizationOptions.Enabled)],
                    out var enabled)
                    ? enabled
                    : options.Enabled;

                options.AllowedAppIds = context.Configuration.GetArraySetting(
                    "Authorization:AllowedAppIds",
                    "Authorization__AllowedAppIds") is { Length: > 0 } allowedAppIds
                    ? allowedAppIds
                    : authorizationSection.GetSection(nameof(EntraAuthorizationOptions.AllowedAppIds)).Get<string[]>() ?? [];

                options.RequiredRoles = context.Configuration.GetArraySetting(
                    "Authorization:RequiredRoles",
                    "Authorization__RequiredRoles") is { Length: > 0 } requiredRoles
                    ? requiredRoles
                    : authorizationSection.GetSection(nameof(EntraAuthorizationOptions.RequiredRoles)).Get<string[]>() ?? [];

                options.AllowedAudiences = context.Configuration.GetArraySetting(
                    "Authorization:AllowedAudiences",
                    "Authorization__AllowedAudiences") is { Length: > 0 } allowedAudiences
                    ? allowedAudiences
                    : authorizationSection.GetSection(nameof(EntraAuthorizationOptions.AllowedAudiences)).Get<string[]>() ?? [];

                options.AllowedIssuers = context.Configuration.GetArraySetting(
                    "Authorization:AllowedIssuers",
                    "Authorization__AllowedIssuers") is { Length: > 0 } allowedIssuers
                    ? allowedIssuers
                    : authorizationSection.GetSection(nameof(EntraAuthorizationOptions.AllowedIssuers)).Get<string[]>() ?? [];
            });
        services.AddSingleton<EntraAuthorizationEvaluator>();
    }).Build();

    await host.RunAsync();
