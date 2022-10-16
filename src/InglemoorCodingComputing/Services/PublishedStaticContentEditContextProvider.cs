namespace InglemoorCodingComputing.Services;

public class PublishedStaticContentEditContextProvider : IPublishedStaticContentEditContextProvider
{
    private readonly IStaticPageService _staticPageService;

    public PublishedStaticContentEditContextProvider(IStaticPageService staticPageService) =>
        _staticPageService = staticPageService;

    public PublishedStaticContentEditContext GetContentEditContext(string routing, string? area = null) =>
        new(_staticPageService, routing, area);
}
