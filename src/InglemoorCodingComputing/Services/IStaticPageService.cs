namespace InglemoorCodingComputing.Services;

public interface IStaticPageService
{
    Task CreateAsync(StaticPage page);
    Task<StaticPage> ReadAsync(Guid id);
    Task<StaticPage?> FindAsync(string path);
    Task<StaticPage?> FindAsync(Guid id);
    IAsyncEnumerable<StaticPage> SearchAsync(string? search = null);
    Task UpdateAsync(StaticPage page);
    Task DeleteAsync(Guid id);
    event Action Changed;
}
