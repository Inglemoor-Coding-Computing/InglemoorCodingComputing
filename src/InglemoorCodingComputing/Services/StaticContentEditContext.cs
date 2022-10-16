namespace InglemoorCodingComputing.Services;

public class StaticContentEditContext : IStaticContentEditContext
{
    public bool AutoSave { get; init; }

    public StaticPage InitialPage { get; init; } = new()
    {
        Id = Guid.NewGuid(),
        Live = true,
        Creation = DateTime.UtcNow,
        Type = StaticPageType.Markdown
    };

    public Task<StaticPage> InitialPageAsync() => Task.FromResult(InitialPage);

    public Action<StaticPage> Saved { get; init; } = _ => { };
}
