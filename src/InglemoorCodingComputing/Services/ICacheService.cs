namespace InglemoorCodingComputing.Services;

/// <summary>
/// Persists data for future retrieval. 
/// </summary>
/// <typeparam name="TPartition">Class name to partition the content into.</typeparam>
public interface ICacheService<TPartition>
{
    /// <summary>
    /// Removes an item from the cache.
    /// </summary>
    /// <param name="key"></param>
    void Delete(string key);

    /// <summary>
    /// Attempts to find the item in the cache.
    /// </summary>
    /// <param name="key"></param>
    /// <returns>The content as a stream, or null if not found.</returns>
    Stream? TryRead(string key);

    /// <summary>
    /// Adds or replaces item in cache.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="content"></param>
    /// <returns>Stream to write data to</returns>
    Stream Add(string key);
}
