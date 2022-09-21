namespace InglemoorCodingComputing.Services;

using System.Text.Json;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

public sealed class StaticPageService : IStaticPageService
{
    private readonly Container _container;
    private readonly ICacheService<StaticPageService> _cacheService;

    public StaticPageService(IConfiguration configuration, CosmosClient cosmosClient, ICacheService<StaticPageService> cacheService)
    {
        _container = cosmosClient.GetContainer(configuration["Cosmos:DatabaseName"], configuration["Cosmos:StaticPagesContainer"]);
        _cacheService = cacheService;
    }

    public async Task CreateAsync(StaticPage page)
    {
        await _container.CreateItemAsync(page, partitionKey: new(page.Id.ToString()));
        Changed?.Invoke();
    }

    public async Task DeleteAsync(Guid id)
    {
        var page = await ReadAsync(id);
        _cacheService.Delete(page.Path);
        await _container.DeleteItemAsync<StaticPage>(id.ToString(), new(id.ToString()));
        Changed?.Invoke();
    }

    public async Task<StaticPage?> FindAsync(string path)
    {
        if (_cacheService.TryRead(path) is Stream stream)
        {
            using var _ = stream;
            return JsonSerializer.Deserialize<StaticPage>(stream);
        }
        
        var iterator = _container.GetItemLinqQueryable<StaticPage>().Where(x => x.Path == path).ToFeedIterator();
        StaticPage? page = null;
        while (iterator.HasMoreResults)
        {
            foreach (var item in await iterator.ReadNextAsync())
                page = item;
        }

        if (page is null)
            return null;

        using var writeStream = _cacheService.Add(path);
        JsonSerializer.Serialize(writeStream, page);

        return page;
    }

    public async Task<StaticPage?> FindAsync(Guid id)
    {
        try
        {
            return await ReadAsync(id);
        }
        catch
        {
            return null;
        }
    }

    public async Task<StaticPage> ReadAsync(Guid id) =>
        (await _container.ReadItemAsync<StaticPage>(id.ToString(), new(id.ToString()))).Resource;

    public async IAsyncEnumerable<StaticPage> SearchAsync(string? search = null)
    {
        var iterator =
            (search is null
            ? _container.GetItemLinqQueryable<StaticPage>()
            : _container.GetItemLinqQueryable<StaticPage>().Where(x => x.Path.Contains(search)))
                .OrderBy(x => x.Path)
                .ToFeedIterator();
        while (iterator.HasMoreResults)
        {
            foreach (var item in await iterator.ReadNextAsync())
                yield return item;
        }
    }

    public async Task UpdateAsync(StaticPage page)
    {
        await _container.ReplaceItemAsync(page, page.Id.ToString(), new(page.Id.ToString()));
        Changed?.Invoke();
    }

    public event Action? Changed;
}
