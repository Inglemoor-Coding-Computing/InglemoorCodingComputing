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

    public Task CreateAsync(StaticPage page) =>
        _container.CreateItemAsync(page, partitionKey: new(page.Id.ToString()));

    public Task DeleteAsync(Guid id) =>
        _container.DeleteItemAsync<StaticPage>(id.ToString(), new(id.ToString()));

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

    public async Task<StaticPage> ReadAsync(Guid id) =>
        (await _container.ReadItemAsync<StaticPage>(id.ToString(), new(id.ToString()))).Resource;

    public Task UpdateAsync(StaticPage page) =>
        _container.ReplaceItemAsync(page, page.Id.ToString(), new(page.Id.ToString()));
}
