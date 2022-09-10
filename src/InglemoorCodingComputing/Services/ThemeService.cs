namespace InglemoorCodingComputing.Services;

using Microsoft.JSInterop;

public class ThemeService : IThemeService
{
    private readonly ValueTask<IJSObjectReference> _module;
    private Theme _theme;

    public ThemeService(IJSRuntime jsRuntime)
    {
        _module = jsRuntime.InvokeAsync<IJSObjectReference>("import", "/js/theme-switcher.js");
        _ = GetThemeAsync();
    }

    public Theme Theme { get => _theme; set => _ = SetThemeAsync(value); }

    public event EventHandler<Theme>? OnThemeChanged;

    private async ValueTask GetThemeAsync()
    {
        _theme = Enum.Parse<Theme>(await (await _module).InvokeAsync<string>("getTheme"), true);
        OnThemeChanged?.Invoke(this, _theme);
    }

    private async ValueTask SetThemeAsync(Theme newTheme)
    {
        _theme = newTheme;
        await (await _module).InvokeVoidAsync("setTheme", new[] { newTheme.ToString().ToLower() });
        OnThemeChanged?.Invoke(this, _theme);
    }

}
