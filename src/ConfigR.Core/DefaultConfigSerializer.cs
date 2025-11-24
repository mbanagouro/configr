using ConfigR.Abstractions;
using System.Text.Json;

namespace ConfigR.Core;

public sealed class DefaultConfigSerializer : IConfigSerializer
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    public string Serialize(object? value)
    {
        if (value is null)
        {
            return string.Empty;
        }

        var type = value.GetType();
        return JsonSerializer.Serialize(value, type, Options);
    }

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
