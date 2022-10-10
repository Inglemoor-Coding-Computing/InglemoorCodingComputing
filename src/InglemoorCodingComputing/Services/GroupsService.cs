namespace InglemoorCodingComputing.Services;

using Microsoft.Azure.Cosmos.Linq;
using System.Collections.Generic;

public class GroupsService : IGroupsService
{
    private readonly ILogger _logger;
    private readonly Container _container;

    public GroupsService(IConfiguration configuration, CosmosClient cosmosClient, ILogger<GroupsService> logger)
    {
        _logger = logger;
        _container = cosmosClient.GetContainer(configuration["Cosmos:DatabaseName"], configuration["Cosmos:GroupsContainer"]);
    }

    public async IAsyncEnumerable<Group> AllGroupsAsync()
    {
        FeedIterator<Group>? iterator = null;
        try
        {
            iterator = _container.GetItemLinqQueryable<Group>().ToFeedIterator();
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to enumerate groups.");
        }
        if (iterator is not null)
        {
            while (iterator.HasMoreResults)
            {
                foreach (var item in await iterator.ReadNextAsync())
                    yield return item;
            }
        }
    }

    public async Task<Group?> TryCreateGroup(string name)
    {
        try
        {
            Group group = new(Guid.NewGuid(), name, DateTime.UtcNow);
            await _container.CreateItemAsync(group);
            return group;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to create group {name}.");
            return null;
        }
    }

    public async Task<bool> TryDeleteGroup(Guid id)
    {
        try
        {
            var id_ = id.ToString();
            await _container.DeleteItemAsync<Group>(id_, new(id_));
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to delete group {id}.");
            return false;
        }
    }

    public async Task<Group?> TryReadGroup(string name)
    {
        try
        {
            var iterator = _container.GetItemLinqQueryable<Group>().Where(x => x.Name == name).ToFeedIterator();
            while (iterator.HasMoreResults)
            {
                foreach (var item in await iterator.ReadNextAsync())
                    return item;
            }
            return null;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to read group {name}.");
            return null;
        }
    }

    public async Task<Group?> TryReadGroup(Guid id)
    {
        try
        {
            var id_ = id.ToString();
            var res = await _container.ReadItemAsync<Group>(id_, new(id_));
            return res.Resource;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to read group {id}.");
            return null;
        }
    }

    public async Task<bool> TryUpdateGroup(Group group)
    {
        try
        {
            await _container.ReplaceItemAsync(group, group.Id.ToString());
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to update group {group.Id}.");
            return false;
        }
    }
}
