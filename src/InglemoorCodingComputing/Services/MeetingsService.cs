namespace InglemoorCodingComputing.Services;

using System.Collections.Concurrent;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

public sealed class MeetingsService : IMeetingsService, IDisposable
{
    private readonly ICacheEventService _cacheEventService;
    private readonly Container _container;

    private readonly ConcurrentDictionary<int, ConcurrentDictionary<Guid, Meeting>> _cache = new();

    public event EventHandler? Changed;
    private void OnChange() => Changed?.Invoke(this, new());
    public MeetingsService(IConfiguration configuration, CosmosClient cosmosClient, ICacheEventService cacheEventService)
    {
        _cacheEventService = cacheEventService;
        _cacheEventService.CacheClearing += OnCacheClearing;
        _container = cosmosClient.GetContainer(configuration["Cosmos:DatabaseName"], configuration["Cosmos:MeetingsContainer"]);
    }

    private void OnCacheClearing(Action<string, bool> report)
    {
        _cache.Clear();
        report(nameof(MeetingsService), true);
    }

    public void Dispose() =>
        _cacheEventService.CacheClearing -= OnCacheClearing;

    public async Task CreateAsync(Meeting meeting)
    {
        await _container.CreateItemAsync(meeting, partitionKey: new(meeting.Id.ToString()));
        _cache[meeting.Year][meeting.Id] = meeting;
        OnChange();
    }

    public async Task DeleteAsync(Guid id)
    {
        foreach (var year in _cache)
            year.Value.TryRemove(id, out _);
        await _container.DeleteItemAsync<Meeting>(id.ToString(), new(id.ToString()));
        OnChange();
    }

    public async IAsyncEnumerable<Meeting> GetMeetingsAsync(int year)
    {
        if (!_cache.TryGetValue(year, out var _))
        {
            _cache[year] = new();
            var iterator = _container.GetItemLinqQueryable<Meeting>().Where(x => x.Year == year).ToFeedIterator();
            while (iterator.HasMoreResults)
            {
                foreach (var x in await iterator.ReadNextAsync())
                    _cache[year][x.Id] = x;
            }
        }
        foreach (var meeting in _cache[year])
            yield return meeting.Value;
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

    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
    public async Task TakeAttendanceAsync(Guid meeting, Guid user)
    {
        await _semaphoreSlim.WaitAsync();
        try
        {
            var m = await ReadAsync(meeting);
            await UpdateAsync(m with { RegisteredAttendees = m.RegisteredAttendees.Append(user).ToList() });
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public async Task<Meeting> ReadAsync(Guid id) =>
        (await _container.ReadItemAsync<Meeting>(id.ToString(), new(id.ToString()))).Resource;

    public async Task UpdateAsync(Meeting meeting)
    {
        await _container.ReplaceItemAsync(meeting, meeting.Id.ToString(), new(meeting.Id.ToString()));
        _cache[meeting.Year][meeting.Id] = meeting;
        OnChange();
    }
}
