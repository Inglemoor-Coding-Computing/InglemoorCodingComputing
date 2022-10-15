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
        await _container.CreateItemAsync(page with { Creation = DateTime.UtcNow }, partitionKey: new(page.Id.ToString()));
        Changed?.Invoke();
    }

    public async Task DeleteAsync(Guid id)
    {
        var page = await ReadAsync(id);
        _cacheService.Delete(page.Path);
        var id_ = id.ToString();

        await _container.ReplaceItemAsync(page with { Deletion = DateTime.UtcNow }, id_);

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
            {
                if (!item.Deletion.HasValue && item.Area is null)
                    page = item;
            }
        }

        if (page is null)
            return null;

        using var writeStream = _cacheService.Add(path);
        JsonSerializer.Serialize(writeStream, page);

        return page;
    }

    public async Task<StaticPage?> FindAsync(string path, string area)
    {
        var cachePath = "AREA-PREFIX 🙅" + area + "AREA-SEPARATOR 🙅" + path;
        if (_cacheService.TryRead(cachePath) is Stream stream)
        {
            using var _ = stream;
            return JsonSerializer.Deserialize<StaticPage>(stream);
        }

        var iterator = _container.GetItemLinqQueryable<StaticPage>().Where(x => x.Path == path && x.Area == area).ToFeedIterator();
        StaticPage? page = null;
        while (iterator.HasMoreResults)
        {
            foreach (var item in await iterator.ReadNextAsync())
            {
                if (!item.Deletion.HasValue)
                    page = item;
            }
        }

        if (page is null)
            return null;

        using var writeStream = _cacheService.Add(cachePath);
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
            {
                if (!item.Deletion.HasValue)
                    yield return item;
            }
        }
    }

    public async Task UpdateAsync(StaticPage page)
    {
        _cacheService.Delete(page.Path);
        var oldId = page.Id;
        var newId = Guid.NewGuid();

        await _container.CreateItemAsync(page with { Creation = DateTime.UtcNow, Id = newId, PreviousVersion = oldId});
        await DeleteAsync(oldId);
    }

    public async Task<bool> TrySetPublishStatusAsync(string path, bool live)
    {
        var page = await FindAsync(path);
        if (page is null) 
            return false;
        _cacheService.Delete(path);

        await _container.ReplaceItemAsync(page with { Live = live }, page.Id.ToString());
        Changed?.Invoke();
        return true;
    }

    public event Action? Changed;
}
