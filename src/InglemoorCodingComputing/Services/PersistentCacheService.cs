namespace InglemoorCodingComputing.Services;

using System.Security.Cryptography;

/// <summary>
/// Cache storing content as files locally.
/// </summary>
/// <typeparam name="TPartition"></typeparam>
public class PersistentCacheService<TPartition> : ICacheService<TPartition>
{
    private readonly string directory = 
        $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/inglemoorccc/cache/{typeof(TPartition).FullName}/";

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
        var path = GetPath(key);
        if (File.Exists(path))
            File.Delete(path);
        return File.OpenWrite(path);
    }

    public void Delete(string key)
    {
        var path = GetPath(key);
        if (File.Exists(path))
            File.Delete(path);
    }

    public Stream? TryRead(string key)
    {
        var path = GetPath(key);
        return File.Exists(path) ? File.OpenRead(path) : null;
    }
}
