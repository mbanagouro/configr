using ConfigR.Abstractions;
using System.Text.Json;

namespace ConfigR.Core;

/// <summary>
/// Default implementation of configuration serialization using JSON.
/// </summary>
public sealed class DefaultConfigSerializer : IConfigSerializer
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    /// <summary>
    /// Serializes an object to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to serialize.</param>
    /// <returns>An empty string if value is null; otherwise, the JSON serialized representation.</returns>
    public string Serialize(object? value)
    {
        if (value is null)
        {
            return string.Empty;
        }

        var type = value.GetType();
        return JsonSerializer.Serialize(value, type, Options);
    }

    /// <summary>
    /// Deserializes a JSON string to an object of the specified type.
    /// </summary>
    /// <param name="serializedValue">The serialized JSON string.</param>
    /// <param name="targetType">The target type to deserialize to.</param>
    /// <returns>
    /// If <paramref name="serializedValue"/> is null or empty, returns the default value for the target type.
    /// Otherwise, returns the deserialized object.
    /// </returns>
    public object? Deserialize(string serializedValue, Type targetType)
    {
        if (string.IsNullOrEmpty(serializedValue))
        {
            if (targetType.IsValueType)
            {
                return Activator.CreateInstance(targetType);
            }

            return null;
        }

        return JsonSerializer.Deserialize(serializedValue, targetType, Options);
    }
}
