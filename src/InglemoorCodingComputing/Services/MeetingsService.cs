namespace InglemoorCodingComputing.Services;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

public class MeetingsService : IMeetingsService
{
    private readonly Container _container;

    public event EventHandler? Changed;
    private OnChange() =>
    public MeetingsService(IConfiguration configuration, CosmosClient cosmosClient)
    {
        _container = cosmosClient.GetContainer(configuration["Cosmos:DatabaseName"], configuration["Cosmos:MeetingsContainer"]);
    }

    public Task CreateAsync(Meeting meeting) =>
        _container.CreateItemAsync(meeting, partitionKey: new(meeting.Id.ToString()));

    public Task DeleteAsync(Guid id) =>
        _container.DeleteItemAsync<Meeting>(id.ToString(), new(id.ToString()));

    public async IAsyncEnumerable<Meeting> GetMeetingsAsync(int year)
    {
        var iterator = _container.GetItemLinqQueryable<Meeting>().Where(x => x.Year == AppUser.AcademicYear).ToFeedIterator();
        while (iterator.HasMoreResults)
        {
            foreach (var x in await iterator.ReadNextAsync())
                yield return x;
        }    
    }

    public async Task<Meeting?> NextAsync(DateTime local)
    {
        Meeting? next = null;
        await foreach (var meeting in GetMeetingsAsync(AppUser.AcademicYear))
        {
            if (meeting.End > local && (meeting.End - local).Days <= 31)
            {
                if (next is null || next.End > meeting.End)
                next = meeting;
            }
        }
        return next;
    }

    public async Task<Meeting> ReadAsync(Guid id) =>
        (await _container.ReadItemAsync<Meeting>(id.ToString(), new(id.ToString()))).Resource;

    public Task UpdateAsync(Meeting meeting) =>
        _container.ReplaceItemAsync(meeting, meeting.Id.ToString(), new(meeting.Id.ToString()));
}
