namespace InglemoorCodingComputing.Services;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

public class UserService : IUserService
{
    private readonly Container _container;
    public UserService(IConfiguration configuration, CosmosClient cosmosClient)
    {
        _container = cosmosClient.GetContainer(configuration["Cosmos:DatabaseName"], configuration["Cosmos:UserContainer"]);
    }

    public Task CreateUser(AppUser user) =>
        _container.CreateItemAsync(user, partitionKey: new(user.Id.ToString()));

    public async Task<AppUser> ReadUser(Guid id) =>
        (await _container.ReadItemAsync<AppUser>(id.ToString(), new(id.ToString()))).Resource;

    public Task UpdateUser(AppUser user) =>
        _container.ReplaceItemAsync(user, user.Id.ToString(), new(user.Id.ToString()));

    public async Task DeleteUser(Guid id)
    {
        var user = await ReadUser(id);
        await UpdateUser(user with { DeletedDate = DateTime.UtcNow });
    }

    public async IAsyncEnumerable<AppUser> ReadAllUsers()
    {
        var iterator = _container.GetItemLinqQueryable<AppUser>().ToFeedIterator();
        while (iterator.HasMoreResults)
        {
            foreach (var item in await iterator.ReadNextAsync())
                yield return item;
        }
    }
}
