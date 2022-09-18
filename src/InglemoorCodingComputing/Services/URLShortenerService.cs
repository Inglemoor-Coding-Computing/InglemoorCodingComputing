namespace InglemoorCodingComputing.Services;

using Microsoft.Azure.Cosmos.Linq;
using System.Collections.Concurrent;

public class URLShortenerService : IURLShortenerService
{
    private readonly Container _container;
    private readonly ConcurrentDictionary<string, string> _cache = new();

    public URLShortenerService(IConfiguration configuration, CosmosClient cosmosClient)
    {
        _container = cosmosClient.GetContainer(configuration["Cosmos:DatabaseName"], configuration["Cosmos:ShortenedURLContainer"]);
    }

    public async Task<bool> CreateAsync(string original, string shortened)
    {
        if (await FindExpandedURLAsync(shortened) is not null)
            return false;
        await _container.CreateItemAsync<UrlAssociation>(new(shortened, original));
        _cache[shortened] = original;
        return true;
    }

    public Task DeleteAsync(string shortened)
    {
        try
        {
            _cache.TryRemove(shortened, out var _);
            return _container.DeleteItemAsync<UrlAssociation>(shortened, new(shortened));
        }
        catch 
        {
            return Task.CompletedTask;
        }
    }

    public async Task<string?> FindExpandedURLAsync(string shortened)
    {
        try
        {
            if (_cache.TryGetValue(shortened, out var orignal)) 
                return orignal;
            _cache[shortened] = (await _container.ReadItemAsync<UrlAssociation>(shortened, new(shortened))).Resource.Original;
            return _cache[shortened];
        }
        catch
        {
            return null;
        }
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
}
