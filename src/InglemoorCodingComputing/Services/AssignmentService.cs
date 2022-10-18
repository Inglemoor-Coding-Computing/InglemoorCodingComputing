namespace InglemoorCodingComputing.Services;

using System.Text.Json;
using Microsoft.Azure.Cosmos.Linq;

public class AssignmentService : IAssignmentService
{
    private readonly Container _container;
    private readonly ICacheService<AssignmentService> _cacheService;
    public AssignmentService(IConfiguration configuration, CosmosClient cosmosClient, ICacheService<AssignmentService> cacheService)
    {
        _container = cosmosClient.GetContainer(configuration["Cosmos:DatabaseName"], configuration["Cosmos:AssignmentsContainer"]);
        _cacheService = cacheService;
    }

    public async IAsyncEnumerable<Assignment> AllAsync()
    {
        var iterator = _container.GetItemLinqQueryable<Assignment>().ToFeedIterator();
        while (iterator.HasMoreResults)
        {
            foreach (var item in await iterator.ReadNextAsync())
                yield return item;
        }
    }

    public async Task<bool> TryCreateAsync(Assignment assignment)
    {
        _cacheService.Delete(assignment.Id.ToString());
        try
        {
            await _container.CreateItemAsync(assignment);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> TryDeleteAsync(Guid id)
    {
        _cacheService.Delete(id.ToString());
        try
        {
            await _container.DeleteItemAsync<Assignment>(id.ToString(), new(id.ToString()));
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<Assignment?> TryReadAsync(Guid id)
    {
        try
        {
            using (var stream = _cacheService.TryRead(id.ToString()))
            {
                if (stream is not null)
                    return await JsonSerializer.DeserializeAsync<Assignment>(stream);
            }

            var res = (await _container.ReadItemAsync<Assignment>(id.ToString(), new(id.ToString()))).Resource;
            using var ws = _cacheService.Add(id.ToString());
            await JsonSerializer.SerializeAsync(ws, res);
            return res;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> TryUpdateAsync(Assignment assignment)
    {
        _cacheService.Delete(assignment.Id.ToString());
        try
        {
            await _container.ReplaceItemAsync<Assignment>(assignment, assignment.Id.ToString());
            return true;
        }
        catch
        {
            return false;
        }
    }
}
