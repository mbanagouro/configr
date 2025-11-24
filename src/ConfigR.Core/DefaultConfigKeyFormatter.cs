using System;
using ConfigR.Abstractions;

namespace ConfigR.Core;

public sealed class DefaultConfigKeyFormatter : IConfigKeyFormatter
{
    public string GetKey(Type configType, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(configType);

        if (string.IsNullOrWhiteSpace(propertyName))
        {
            throw new ArgumentException("Property name must be provided.", nameof(propertyName));
        }

        var raw = $"{configType.Name}.{propertyName}";
        return Normalize(raw);
    }

    public string Normalize(string key)
    {
        ArgumentNullException.ThrowIfNull(key);

        return key.Trim().ToLowerInvariant();
    }
}
