namespace InglemoorCodingComputing.Models;

public record UrlAssociation(string Shortened, string Original)
{
    public string Id => Shortened;
}
