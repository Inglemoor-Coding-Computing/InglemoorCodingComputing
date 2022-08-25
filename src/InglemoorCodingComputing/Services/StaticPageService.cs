namespace InglemoorCodingComputing.Services;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

public class StaticPageService : IStaticPageService
{
    private readonly Container _container;

    public StaticPageService(IConfiguration configuration, CosmosClient cosmosClient)
    {
        _container = cosmosClient.GetContainer(configuration["Cosmos:DatabaseName"], configuration["Cosmos:StaticPagesContainer"]);
    }

    public async Task CreateAsync(StaticPage page)
    {
        await _container.CreateItemAsync(page, partitionKey: new(page.Id.ToString()));
        Changed?.Invoke();
    }

    public async Task DeleteAsync(Guid id)
    {
        await _container.DeleteItemAsync<StaticPage>(id.ToString(), new(id.ToString()));
        Changed?.Invoke();
    }

    public async Task<StaticPage?> FindAsync(string path)
    {
        var iterator = _container.GetItemLinqQueryable<StaticPage>().Where(x => x.Path == path).ToFeedIterator();
        List<StaticPage> pages = new();
        while (iterator.HasMoreResults)
        {
            foreach (var item in await iterator.ReadNextAsync())
                pages.Add(item);
        }
        return pages.FirstOrDefault();
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
