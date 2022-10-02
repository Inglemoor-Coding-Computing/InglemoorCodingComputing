namespace InglemoorCodingComputing.Services;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

public sealed class UserService : IUserService
{
    private readonly Container _container;
    public UserService(IConfiguration configuration, CosmosClient cosmosClient)
    {
        _container = cosmosClient.GetContainer(configuration["Cosmos:DatabaseName"], configuration["Cosmos:UserContainer"]);
    }

    public async Task<bool> TryCreateUserAsync(AppUser user)
    {
        try
        {
            await _container.CreateItemAsync(user, partitionKey: new(user.Id.ToString()));
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<AppUser?> TryReadUserAsync(Guid id)
    {
        try
        {
            var res = (await _container.ReadItemAsync<AppUser>(id.ToString(), new(id.ToString()))).Resource;
            return res.DeletedDate.HasValue ? null : res;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> TryUpdateUserAsync(AppUser user)
    {
        try
        {
            await _container.ReplaceItemAsync(user, user.Id.ToString(), new(user.Id.ToString()));
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> TryDeleteUserAsync(Guid id)
    {
        var user = await TryReadUserAsync(id);
        if (user is null)
            return false;
        return await TryUpdateUserAsync(user with { DeletedDate = DateTime.UtcNow });
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
