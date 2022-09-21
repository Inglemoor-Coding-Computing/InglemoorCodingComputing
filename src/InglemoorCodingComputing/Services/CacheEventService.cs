namespace InglemoorCodingComputing.Services;

using System.Collections.Concurrent;

public sealed class CacheEventService : ICacheEventService
{
    private bool _clearingInProgress;

    public event Action<Action<string, bool>>? CacheClearing;
    
    public IReadOnlyDictionary<string, bool>? ClearCache()
    {
        if (_clearingInProgress)
            return null;

        _clearingInProgress = true;
        try
        {
            ConcurrentDictionary<string, bool> results = new();
            CacheClearing?.Invoke((source, success) => results[source] = success);
            return results;
        }
        finally
        {
            _clearingInProgress = false;
        }
    }
}
