namespace InglemoorCodingComputing.Services;

using Microsoft.JSInterop;

public class TimeZoneService
{
    private readonly IJSRuntime _jsRuntime;

    private TimeSpan? _userOffset;

    public TimeZoneService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async ValueTask<DateTimeOffset> GetLocalDateTime(DateTimeOffset dateTime)
    {
        if (_userOffset is null)
        {
            int offsetInMinutes = await _jsRuntime.InvokeAsync<int>("getTimezoneOffset");
            _userOffset = TimeSpan.FromMinutes(-offsetInMinutes);
        }

        return dateTime.ToOffset(_userOffset.Value);
    }
}
