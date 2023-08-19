namespace InglemoorCodingComputing.Services;

public class LocalStaticResourceService : IStaticResourceService
{
    private readonly string _rootPath;
    public LocalStaticResourceService()
    {
        _rootPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "inglemoorccc", "files");
        if (!Directory.Exists(_rootPath))
            Directory.CreateDirectory(_rootPath);
    }

    private string EncodeName(string originalName) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(originalName));

    private string DecodeName(string encodedName) =>
        Encoding.UTF8.GetString(Convert.FromBase64String(encodedName));

    private Stream? Download(string file)
    {
        // They're giving us an arbitrary file name, it could be a directory injection attack so we'll encode it first.
        var encodedName = EncodeName(file);
        var path = Path.Combine(_rootPath, encodedName);
        if (!File.Exists(path))
            return null;
        return File.OpenRead(path);
    }

    private void Delete(string file)
    {
        var encodedName = EncodeName(file);
        var path = Path.Combine(_rootPath, encodedName);
        if (File.Exists(path))
            File.Delete(path);
    }

    public Task<Stream?> DownloadAsync(string file) =>
        Task.FromResult(Download(file));

    public async Task UploadAsync(string name, Stream content)
    {
        var encodedName = EncodeName(name);
        var path = Path.Combine(_rootPath, encodedName);
        using var file = File.OpenWrite(path);
        await content.CopyToAsync(file);
    }

    public Task DeleteAsync(string file)
    {
        Delete(file);
        return Task.CompletedTask;
    }

    public IAsyncEnumerable<string> GetFileNamesAsync()
    {
        var files = Directory.EnumerateFiles(_rootPath);
        return files.Select(x => DecodeName(new FileInfo(x).Name)).ToAsyncEnumerable();
    }
}