namespace InglemoorCodingComputing.Services;

public class PublishedStaticContentEditContext : IStaticContentEditContext
{
    private readonly IStaticPageService _staticPageService;
    private readonly string _routing;
    private readonly string? _area;

    public PublishedStaticContentEditContext(IStaticPageService staticPageService, string routing, string? area)
    {
        _staticPageService = staticPageService;
        _routing = routing;
        _area = area;
    }

    public bool AutoSave => false;

    public async Task<StaticPage> InitialPageAsync() => await _staticPageService.FindAsync(_routing, _area) ?? new StaticPage()
    {
        Area = _area,
        Path = _routing,
        Id = Guid.NewGuid(),
        Live = true,
        Creation = DateTime.UtcNow,
        Type = StaticPageType.Markdown
    };

    private readonly SemaphoreSlim semaphore = new(1, 1);
    private async Task SaveAsync(StaticPage page)
    {
        await semaphore.WaitAsync();
        try
        {
            if (await _staticPageService.FindAsync(page.Id) is not null)
                await _staticPageService.UpdateAsync(page);
            else
                await _staticPageService.CreateAsync(page);
        }
        finally
        {
            semaphore.Release();
        }
    }

    public Action<StaticPage> Saved => page => _ = SaveAsync(page);
}
