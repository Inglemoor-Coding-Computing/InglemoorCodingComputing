namespace InglemoorCodingComputing.Services;

public interface IBannerService
{
    void RemoveBanner();
    void SetBanner(Banner banner);
    Banner? GetBanner();
}