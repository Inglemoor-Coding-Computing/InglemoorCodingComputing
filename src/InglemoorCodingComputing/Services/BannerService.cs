using System.Text.Json;

namespace InglemoorCodingComputing.Services;

public class BannerService : IBannerService
{
    private readonly string _filePath; 
    public BannerService()
    {
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "inglemoorccc");
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "banner.json");
    }

    public void RemoveBanner()
    {
        if (File.Exists(_filePath))
            File.Delete(_filePath);
    }

    public void SetBanner(Banner banner)
    {
        var json = JsonSerializer.Serialize(banner);
        File.WriteAllText(_filePath, json);
    }

    public Banner? GetBanner()
    {
        if (!File.Exists(_filePath))
            return null;
        var json = File.ReadAllText(_filePath);
        return JsonSerializer.Deserialize<Banner>(json);
    }
}