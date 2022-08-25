namespace InglemoorCodingComputing.Models;

public record StaticPage
{
    public Guid Id { get; init; }
    public string? Title { get; set; }
    public bool Live { get; set; }
    public bool Authorized { get; set; }
    public bool AdminAuthorized { get; set; }
    public string Path { get; init; } = string.Empty;
    public string Rendered { get; init; } = string.Empty;
    public string Raw { get; init; } = string.Empty;
    public StaticPageType Type { get; init; }
}
