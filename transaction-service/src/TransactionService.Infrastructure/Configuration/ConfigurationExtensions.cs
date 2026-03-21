using Microsoft.Extensions.Configuration;

namespace TransactionService.Infrastructure.Configuration;

internal static class ConfigurationExtensions
{
    public static string? GetSetting(this IConfiguration configuration, params string[] keys)
    {
        foreach (var key in keys)
        {
            var value = configuration[key];
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }

    public static string GetRequiredSetting(this IConfiguration configuration, string errorMessage, params string[] keys)
        => configuration.GetSetting(keys) ?? throw new InvalidOperationException(errorMessage);
}
