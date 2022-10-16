namespace InglemoorCodingComputing.Services;

public interface IPublishedStaticContentEditContextProvider
{
    PublishedStaticContentEditContext GetContentEditContext(string routing, string? area = null);
}
