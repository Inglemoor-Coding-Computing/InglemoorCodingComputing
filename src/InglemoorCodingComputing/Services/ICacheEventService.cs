namespace InglemoorCodingComputing.Services;

/// <summary>
/// Sends an event a site admin requests the server cache to be cleared.
/// </summary>
public interface ICacheEventService
{
    /// <summary>
    /// Clears server cache.
    /// </summary>
    /// <returns>A dictionary indicating success from various caching mechanisms. Null if a clearing is in progress.</returns>
    IReadOnlyDictionary<string, bool>? ClearCache();


    /// <summary>
    /// Event invoked when cache is being cleared.
    /// Includes a function to add results indicating source and success.
    /// </summary>
    event Action<Action<string, bool>> CacheClearing;
}

    