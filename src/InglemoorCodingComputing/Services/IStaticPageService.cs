namespace InglemoorCodingComputing.Services;

public interface IStaticPageService
{
    Task CreateAsync(StaticPage page);
    Task<StaticPage> ReadAsync(Guid id);
    Task<StaticPage?> FindAsync(string path);
    Task<StaticPage?> FindAsync(string path, string? area);
    Task<StaticPage?> FindAsync(Guid id);
    IAsyncEnumerable<StaticPage> SearchAsync(string? search = null);
    Task UpdateAsync(StaticPage page);
    Task DeleteAsync(Guid id);
    event Action Changed;
    Task<bool> TrySetPublishStatusAsync(string path, bool live, string? area = null);
    Task<bool> TrySetPublishStatusAsync(Guid id, bool live);
}
