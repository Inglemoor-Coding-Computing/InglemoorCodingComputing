namespace InglemoorCodingComputing.Services;

using System.Security.Cryptography;
using System.Collections.Concurrent;

/// <summary>
/// Cache storing content as files locally.
/// </summary>
/// <typeparam name="TPartition"></typeparam>
public sealed class PersistentCacheService<TPartition> : ICacheService<TPartition>, IDisposable
{
    private readonly string directory = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/inglemoorccc/cache/{typeof(TPartition).FullName}/";
    private readonly ConcurrentDictionary<string, object> _locks = new();
    private readonly ICacheEventService _cacheEventService;

    /// <summary>
    /// Creates a new PersistentCacheService.
    /// </summary>
    /// <param name="cacheEventService">Supplies events to initiate clearing of the cache.</param>
    public PersistentCacheService(ICacheEventService cacheEventService)
    {
        _cacheEventService = cacheEventService;
        _cacheEventService.CacheClearing += OnCacheClearing;
    }

    private void OnCacheClearing(Action<string, bool> addResponse)
    {
        try
        {
            DirectoryInfo directoryInfo = new(directory);
            if (directoryInfo.Exists)
                directoryInfo.Delete(true);
            _locks.Clear();
            addResponse($"PersistentCacheService: {typeof(TPartition).Name}", true);
        }
        catch
        {
            addResponse($"PersistentCacheService: {typeof(TPartition).Name}", false);
        }
    }

    private static byte[] GetHash(string inputString)
    {
        using var algorithm = SHA256.Create();
        return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
    }

    private static string GetHashString(string inputString)
    {
        StringBuilder sb = new();
        foreach (var b in GetHash(inputString))
            sb.Append(b.ToString("X2"));
        return sb.ToString();
    }


    private string GetPath(string file)
    {
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);
        return Path.Combine(directory, GetHashString(file));
    }

    public Stream Add(string key)
    {
        lock (_locks.GetOrAdd(key, new object()))
        {
            var path = GetPath(key);
            if (File.Exists(path))
                File.Delete(path);
            return File.OpenWrite(path);
        }
    }

    public void Delete(string key)
    {
        lock (_locks.GetOrAdd(key, new object()))
        {
            var path = GetPath(key);
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    public Stream? TryRead(string key)
    {
        lock (_locks.GetOrAdd(key, new object()))
        {
            var path = GetPath(key);
            return File.Exists(path) ? File.OpenRead(path) : null;
        }
    }

    public void Dispose() => 
        _cacheEventService.CacheClearing -= OnCacheClearing;
}
