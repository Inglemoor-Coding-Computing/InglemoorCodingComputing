using System;

namespace InglemoorCodingComputing.Services;

using System.Security.Cryptography;

public class StaticResourceService : IStaticResourceService
{
    public static byte[] GetHash(string inputString)
    {
        using var algorithm = SHA256.Create();
        return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
    }

    public static string GetHashString(string inputString)
    {
        StringBuilder sb = new();
        foreach (var b in GetHash(inputString))
            sb.Append(b.ToString("X2"));

        return sb.ToString();
    }

    public static string GetPath(string file)
    {
        var dir = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/inglemoorccc/cache/resources/";
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, file);
        return path;
    }

    private readonly BlobContainerClient _blobContainerClient;

    public StaticResourceService(BlobServiceClient blobServiceClient)
    {
        _blobContainerClient = blobServiceClient.GetBlobContainerClient("static");
    }

    public async Task<Stream?> DownloadAsync(string file)
    {
        // Check if file was cached:
        var path = GetPath(file);
        if (File.Exists(path))
            return File.OpenRead(path);

        var blobClient = _blobContainerClient.GetBlobClient(file);
        if (blobClient.Exists())
        {
            await blobClient.DownloadToAsync(path);
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

    public Task DeleteAsync(string file)
    {
        // Check if file was cached:
        var path = GetPath(file);
        if (File.Exists(path))
            File.Delete(path);
        return _blobContainerClient.DeleteBlobIfExistsAsync(file);
    }

    public IAsyncEnumerable<string> GetFileNamesAsync() =>
        _blobContainerClient.GetBlobsAsync().SelectAwait(x => ValueTask.FromResult(x.Name));
}
