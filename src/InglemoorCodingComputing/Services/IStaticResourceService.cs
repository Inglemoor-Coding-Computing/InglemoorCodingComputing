namespace InglemoorCodingComputing.Services;

public interface IStaticResourceService
{
    Task<Stream?> DownloadAsync(string file);
    Task UploadAsync(string name, Stream content);
    Task DeleteAsync(string file);
    IAsyncEnumerable<string> GetFileNamesAsync();
}
