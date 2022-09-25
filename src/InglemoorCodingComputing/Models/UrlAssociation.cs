namespace InglemoorCodingComputing.Models;

public record UrlAssociation(string Shortened, string Original, bool Special)
{
    public string Id => Shortened;
}
