namespace InglemoorCodingComputing.Services;

public class StaticResourceService : IStaticResourceService
{
    private readonly BlobContainerClient _blobContainerClient;
    private readonly ICacheService<StaticResourceService> _cacheService;

    public StaticResourceService(BlobServiceClient blobServiceClient, ICacheService<StaticResourceService> cacheService)
    {
        _blobContainerClient = blobServiceClient.GetBlobContainerClient("static");
        _cacheService = cacheService;
    }

    public async Task<Stream?> DownloadAsync(string file)
    {
        // Check if file was cached:
        if (_cacheService.TryRead(file) is Stream stream)
            return stream;

        var blobClient = _blobContainerClient.GetBlobClient(file);
        if (blobClient.Exists())
        {
            using (var writeSteam = _cacheService.Add(file))
                await blobClient.DownloadToAsync(writeSteam);
            return await DownloadAsync(file);
        }
        return null;
    }

    public async Task UploadAsync(string file, Stream content)
    {
        await DeleteAsync(file);
        await _blobContainerClient.UploadBlobAsync(file, content);
        _ = DownloadAsync(file); // Cache it again.
    }

    public async Task DeleteAsync(string file)
    {
        _cacheService.Delete(file);
        await _blobContainerClient.DeleteBlobIfExistsAsync(file);
    }

    public IAsyncEnumerable<string> GetFileNamesAsync() =>
        _blobContainerClient.GetBlobsAsync().SelectAwait(x => ValueTask.FromResult(x.Name));
}
