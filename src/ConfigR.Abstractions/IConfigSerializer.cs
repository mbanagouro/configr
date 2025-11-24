namespace ConfigR.Abstractions;

public interface IConfigSerializer
{
    string Serialize(object? value);
    object? Deserialize(string serializedValue, Type targetType);
}
