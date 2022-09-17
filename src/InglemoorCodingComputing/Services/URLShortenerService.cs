namespace InglemoorCodingComputing.Services;

using Microsoft.Azure.Cosmos.Linq;

public class URLShortenerService : IURLShortenerService
{
    private readonly Container _container;

    public URLShortenerService(IConfiguration configuration, CosmosClient cosmosClient)
    {
        _container = cosmosClient.GetContainer(configuration["Cosmos:DatabaseName"], configuration["Cosmos:ShortenedURLContainer"]);
    }

    public async Task<bool> CreateAsync(string original, string shortened)
    {
        if (await FindExpandedURLAsync(shortened) is not null)
            return false;
        await _container.CreateItemAsync<UrlAssociation>(new(shortened, original));
        return true;
    }

    public Task DeleteAsync(string shortened)
    {
        try
        {
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
            return (await _container.ReadItemAsync<UrlAssociation>(shortened, new(shortened))).Resource.Original;
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
