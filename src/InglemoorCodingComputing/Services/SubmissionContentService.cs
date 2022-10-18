namespace InglemoorCodingComputing.Services;

using System.Text.Json;

public class SubmissionContentService : ISubmissionContentService
{
    public readonly Container _container;
    private readonly ICacheService<SubmissionContentService> _cacheService;

    public SubmissionContentService(IConfiguration configuration, CosmosClient cosmosClient, ICacheService<SubmissionContentService> cacheService)
    {
        _container = cosmosClient.GetContainer(configuration["Cosmos:DatabaseName"], configuration["Cosmos:SubmissionContentContainer"]);
        _cacheService = cacheService;
    }
    
    public async Task<bool> TryCreateAsync(SubmissionContent assignment)
    {
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

    public async Task<SubmissionContent?> TryReadAsync(Guid id)
    {
        try
        {
            using (var stream = _cacheService.TryRead(id.ToString()))
            {
                if (stream is not null)
                    return await JsonSerializer.DeserializeAsync<SubmissionContent>(stream);
            }

            var res = (await _container.ReadItemAsync<SubmissionContent>(id.ToString(), new(id.ToString()))).Resource;
            using var ws = _cacheService.Add(id.ToString());
            await JsonSerializer.SerializeAsync(ws, res);
            return res;
        }
        catch
        {
            return null;
        }
    }
}
