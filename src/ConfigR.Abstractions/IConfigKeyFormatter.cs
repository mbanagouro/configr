namespace ConfigR.Abstractions;

public interface IConfigKeyFormatter
{
    string GetKey(Type configType, string propertyName);
    string Normalize(string key);
}
