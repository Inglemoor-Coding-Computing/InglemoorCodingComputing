namespace InglemoorCodingComputing.Services;

/// <summary>
/// Finds and creates shortened urls.
/// </summary>
public interface IURLShortenerService
{
    /// <summary>
    /// Find the expanded form of a shortened url token
    /// </summary>
    /// <param name="shortened">the shortened url to expand.</param>
    /// <returns>the url that may be redirected to</returns>
    Task<string?> FindExpandedURLAsync(string shortened);

    /// <summary>
    /// Create a shortened url.
    /// </summary>
    /// <param name="original">Original url that needs shortening</param>
    /// <param name="shortened">the new shortened portion of the url.</param>
    /// <returns>true if successful, false otherwise</returns>
    Task<bool> CreateAsync(string original, string shortened);

    /// <summary>
    /// Deletes a shortened url.
    /// </summary>
    /// <param name="shortened">the new shortened portion of the url.</param>
    Task DeleteAsync(string shortened);

    /// <summary>
    /// Finds all the url associations.
    /// </summary>
    IAsyncEnumerable<UrlAssociation> ReadAllAsync();
}
