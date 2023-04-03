namespace InglemoorCodingComputing.Models;

public record Banner
{
    public string Html { get; init; } = string.Empty;
    public string? Style { get; init; }
}