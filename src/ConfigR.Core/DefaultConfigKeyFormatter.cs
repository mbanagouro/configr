using ConfigR.Abstractions;
using System.Buffers;

namespace ConfigR.Core;

/// <summary>
/// Default implementation of configuration key formatting with low-allocation optimizations.
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

        // Use ArrayPool to avoid allocations for temporary buffers
        var typeName = configType.Name.AsSpan();
        var propName = propertyName.AsSpan();
        
        // Calculate exact size needed: typeName.Length + 1 (dot) + propName.Length
        var totalLength = typeName.Length + 1 + propName.Length;
        
        // Rent buffer from pool - more efficient than allocating new array
        var rentedArray = ArrayPool<char>.Shared.Rent(totalLength);
        try
        {
            var buffer = rentedArray.AsSpan(0, totalLength);
            
            // Copy type name
            typeName.CopyTo(buffer);
            
            // Add separator
            buffer[typeName.Length] = '.';
            
            // Copy property name
            propName.CopyTo(buffer[(typeName.Length + 1)..]);
            
            // Normalize and return
            return Normalize(buffer);
        }
        finally
        {
            // Always return buffer to pool
            ArrayPool<char>.Shared.Return(rentedArray);
        }
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
        
        return Normalize(key.AsSpan());
    }
    
    /// <summary>
    /// Normalizes a configuration key span by trimming whitespace and converting to lowercase.
    /// </summary>
    /// <param name="key">The key span to normalize.</param>
    /// <returns>The normalized key.</returns>
    private static string Normalize(ReadOnlySpan<char> key)
    {
        // Trim whitespace without allocating
        var trimmed = key.Trim();
        
        if (trimmed.IsEmpty)
        {
            return string.Empty;
        }
        
        // Use stack allocation for small keys (< 128 chars is common)
        Span<char> lowerBuffer = stackalloc char[128];
        
        if (trimmed.Length <= lowerBuffer.Length)
        {
            // Fast path: use stack allocation
            var destination = lowerBuffer[..trimmed.Length];
            
            // ToLowerInvariant with Span avoids allocation
            var written = trimmed.ToLowerInvariant(destination);
            
            return new string(destination[..written]);
        }
        
        // Slow path: use ArrayPool for larger keys
        var rentedArray = ArrayPool<char>.Shared.Rent(trimmed.Length);
        try
        {
            var buffer = rentedArray.AsSpan(0, trimmed.Length);
            var written = trimmed.ToLowerInvariant(buffer);
            
            return new string(buffer[..written]);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(rentedArray);
        }
    }
}
