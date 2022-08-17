namespace InglemoorCodingComputing.Services;

public interface IMeetingsService
{
    Task CreateAsync(Meeting meeting);
    Task<Meeting> ReadAsync(Guid id);
    Task UpdateAsync(Meeting meeting);
    Task DeleteAsync(Guid id);
    Task<Meeting?> NextAsync(DateTime dateTime);
    IAsyncEnumerable<Meeting> GetMeetingsAsync(int year);
    event EventHandler? Changed;
}
