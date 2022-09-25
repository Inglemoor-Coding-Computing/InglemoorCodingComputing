namespace InglemoorCodingComputing.Services;

using Microsoft.Azure.Cosmos.Linq;
using System.Collections.Concurrent;

public sealed class URLShortenerService : IURLShortenerService, IDisposable
{
    private readonly Container _container;
    private readonly ICacheEventService _cacheEventService;
    private readonly ConcurrentDictionary<string, UrlAssociation> _cache = new();

    public URLShortenerService(IConfiguration configuration, CosmosClient cosmosClient, ICacheEventService cacheEventService)
    {
        _container = cosmosClient.GetContainer(configuration["Cosmos:DatabaseName"], configuration["Cosmos:ShortenedURLContainer"]);
        _cacheEventService = cacheEventService;
        _cacheEventService.CacheClearing += OnCacheClearing;
    }

    private void OnCacheClearing(Action<string, bool> addResponse)
    {
        _ = OnUpdateSpecialLinksAsync();
        _cache.Clear();
        addResponse("URLShortenerService (Memory)", true);
    }

    public async Task<bool> CreateAsync(string original, string shortened, bool special = false)
    {
        if (await FindExpandedURLAsync(shortened) is not null)
            return false;
        UrlAssociation association = new(shortened, original, special);
        await _container.CreateItemAsync(association);
        _cache[shortened] = association;
        await OnUpdateSpecialLinksAsync();
        return true;
    }

    public async Task DeleteAsync(string shortened)
    {
        try
        {
            _cache.TryRemove(shortened, out var _);
            await _container.DeleteItemAsync<UrlAssociation>(shortened, new(shortened));
            await OnUpdateSpecialLinksAsync();
        }
        catch { }
    }

    public async Task<string?> FindExpandedURLAsync(string shortened, bool special = false)
    {
        try
        {
            if (_cache.TryGetValue(shortened, out var orignal)) 
                return orignal.Original;
            
            var found = (await _container.ReadItemAsync<UrlAssociation>(shortened, new(shortened))).Resource;
            _cache[shortened] = found;
            if (found.Special || !special)
                return found.Original;
        }
        catch { }
        return null;
    }

    public async IAsyncEnumerable<UrlAssociation> ReadAllAsync()
    {
        var iterator = _container.GetItemLinqQueryable<UrlAssociation>().ToFeedIterator();
        while (iterator.HasMoreResults)
        {
            foreach (var item in await iterator.ReadNextAsync())
                yield return item;
        }
    }

    public async IAsyncEnumerable<string> ReadAllSpecialAsync()
    {
        var iterator = _container.GetItemLinqQueryable<UrlAssociation>().ToFeedIterator();
        while (iterator.HasMoreResults)
        {
            foreach (var item in await iterator.ReadNextAsync())
                yield return item.Shortened;
        }
    }

    private async Task OnUpdateSpecialLinksAsync() =>
        SpecialURLsChanged?.Invoke(await ReadAllSpecialAsync().ToListAsync());

    public event Action<IReadOnlyList<string>>? SpecialURLsChanged;

    public void Dispose() =>
        _cacheEventService.CacheClearing -= OnCacheClearing;
}
