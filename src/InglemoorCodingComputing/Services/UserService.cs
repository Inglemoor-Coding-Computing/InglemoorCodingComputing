namespace InglemoorCodingComputing.Services;

using System.Text.Json;
using Microsoft.Azure.Cosmos.Linq;

public sealed class UserService : IUserService
{
    private readonly Container _container;
    private readonly ICacheService<UserService> _cacheService;

    public UserService(IConfiguration configuration, CosmosClient cosmosClient, ICacheService<UserService> cacheService)
    {
        _cacheService = cacheService;
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
            using (var stream = _cacheService.TryRead(id.ToString()))
            {
                if (stream is not null)
                    return await JsonSerializer.DeserializeAsync<AppUser>(stream);
            }
            var res = (await _container.ReadItemAsync<AppUser>(id.ToString(), new(id.ToString()))).Resource;
            using var ws = _cacheService.Add(id.ToString());
            await JsonSerializer.SerializeAsync(ws, res);
            return res;
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
            _cacheService.Delete(user.Id.ToString());
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
