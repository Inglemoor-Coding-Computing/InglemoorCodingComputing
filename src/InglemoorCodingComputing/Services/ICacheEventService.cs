namespace InglemoorCodingComputing.Services;

/// <summary>
/// Sends an event a site admin requests the server cache to be cleared.
/// </summary>
public interface ICacheEventService
{
    /// <summary>
    /// Invoke event.
    /// </summary>
    void ClearCache();

    /// <summary>
    /// Event invoked when cache is being cleared.
    /// </summary>
    event Action CacheClearing;
}
