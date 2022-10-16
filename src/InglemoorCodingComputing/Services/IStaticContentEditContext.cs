namespace InglemoorCodingComputing.Services;

public interface IStaticContentEditContext
{
    /// <summary>
    /// Automatically triggers the Saved callback.
    /// </summary>
    bool AutoSave { get; }

    /// <summary>
    /// The first page to populate the editor with.
    /// </summary>
    Task<StaticPage> InitialPageAsync();

    /// <summary>
    /// Callback when saved.
    /// </summary>
    Action<StaticPage> Saved { get; }
}
