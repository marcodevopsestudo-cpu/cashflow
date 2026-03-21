using Microsoft.Extensions.Configuration;

namespace TransactionService.Api.Configuration;

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

    public static string[] GetArraySetting(this IConfiguration configuration, params string[] keys)
    {
        foreach (var key in keys)
        {
            var section = configuration.GetSection(key);
            var values = section.Get<string[]>();
            if (values is { Length: > 0 })
            {
                return values;
            }
        }

        return [];
    }
}
