namespace InglemoorCodingComputing.Models;

public record StaticPage
{
    public Guid Id { get; init; }
    public bool Live { get; set; }
    public bool Authorized { get; set; }
    public string Path { get; init; } = string.Empty;
    public string Rendered { get; init; } = string.Empty;
    public string Raw { get; init; } = string.Empty;
    public StaticPageType Type { get; init; }
}
