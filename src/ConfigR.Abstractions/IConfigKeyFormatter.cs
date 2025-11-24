namespace ConfigR.Abstractions;

/// <summary>
/// Defines the contract for formatting configuration keys.
/// </summary>
public interface IConfigKeyFormatter
{
    /// <summary>
    /// Gets the formatted configuration key for a given type and property name.
    /// </summary>
    /// <param name="configType">The configuration type.</param>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The formatted configuration key.</returns>
    string GetKey(Type configType, string propertyName);

    /// <summary>
    /// Normalizes a configuration key.
    /// </summary>
    /// <param name="key">The key to normalize.</param>
    /// <returns>The normalized key.</returns>
    string Normalize(string key);
}
