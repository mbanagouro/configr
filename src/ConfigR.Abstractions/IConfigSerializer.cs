namespace ConfigR.Abstractions;

/// <summary>
/// Defines the contract for serializing and deserializing configuration values.
/// </summary>
public interface IConfigSerializer
{
    /// <summary>
    /// Serializes an object to its string representation.
    /// </summary>
    /// <param name="value">The value to serialize.</param>
    /// <returns>The serialized string representation.</returns>
    string Serialize(object? value);

    /// <summary>
    /// Deserializes a string to an object of the specified type.
    /// </summary>
    /// <param name="serializedValue">The serialized string value.</param>
    /// <param name="targetType">The target type to deserialize to.</param>
    /// <returns>The deserialized object.</returns>
    object? Deserialize(string serializedValue, Type targetType);
}
