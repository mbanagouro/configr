namespace ConfigR.Abstractions;

/// <summary>
/// Defines the contract for reading and writing strongly-typed configuration.
/// </summary>
public interface IConfigR
{
    /// <summary>
    /// Gets a strongly-typed configuration instance synchronously.
    /// </summary>
    /// <typeparam name="T">The configuration type.</typeparam>
    /// <returns>A new instance of the configuration type with populated values.</returns>
    T Get<T>() where T : new();

    /// <summary>
    /// Gets a strongly-typed configuration instance asynchronously.
    /// </summary>
    /// <typeparam name="T">The configuration type.</typeparam>
    /// <returns>A task that returns a new instance of the configuration type with populated values.</returns>
    Task<T> GetAsync<T>() where T : new();

    /// <summary>
    /// Saves a strongly-typed configuration instance synchronously.
    /// </summary>
    /// <typeparam name="T">The configuration type.</typeparam>
    /// <param name="config">The configuration instance to save.</param>
    void Save<T>(T config);

    /// <summary>
    /// Saves a strongly-typed configuration instance asynchronously.
    /// </summary>
    /// <typeparam name="T">The configuration type.</typeparam>
    /// <param name="config">The configuration instance to save.</param>
    /// <returns>A task representing the asynchronous save operation.</returns>
    Task SaveAsync<T>(T config);
}
