using ConfigR.Abstractions;

namespace ConfigR.Core;

/// <summary>
/// Default implementation of configuration key formatting.
/// </summary>
public sealed class DefaultConfigKeyFormatter : IConfigKeyFormatter
{
    /// <summary>
    /// Gets the formatted configuration key for a given type and property name.
    /// </summary>
    /// <param name="configType">The configuration type.</param>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The formatted and normalized configuration key.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configType"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="propertyName"/> is null, empty, or whitespace.</exception>
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

    /// <summary>
    /// Normalizes a configuration key by trimming whitespace and converting to lowercase.
    /// </summary>
    /// <param name="key">The key to normalize.</param>
    /// <returns>The normalized key.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is null.</exception>
    public string Normalize(string key)
    {
        ArgumentNullException.ThrowIfNull(key);

        return key.Trim().ToLowerInvariant();
    }
}
