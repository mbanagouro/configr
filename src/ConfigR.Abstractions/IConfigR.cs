using System.Threading.Tasks;

namespace ConfigR.Abstractions;

public interface IConfigR
{
    T Get<T>() where T : new();
    Task<T> GetAsync<T>() where T : new();

    void Save<T>(T config);
    Task SaveAsync<T>(T config);
}
