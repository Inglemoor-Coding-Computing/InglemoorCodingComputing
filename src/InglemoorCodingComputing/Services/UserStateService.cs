namespace InglemoorCodingComputing.Services;

using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

public sealed class UserStateService : IUserStateService, IDisposable
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly IUserService _userService;
    private Task<AppUser?> current;

    public UserStateService(AuthenticationStateProvider authenticationStateProvider, IUserService userService)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _userService = userService;
        _authenticationStateProvider.AuthenticationStateChanged += AuthChanged;
        current = Task.Run(async () =>
        {
            var auth = await _authenticationStateProvider.GetAuthenticationStateAsync();

            if (auth.User.Identity?.IsAuthenticated is not true)
                return null;
            else
            {
                if (Guid.TryParse(auth.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
                    return await _userService.TryReadUserAsync(id);
                return null;
            }
        });
    }

    public event EventHandler<AppUser?>? Updated;

    public Task<AppUser?> CurrentAsync() =>
        current;

    public void Update()
    {
        current = Task.Run(async () =>
        {
            var auth = await _authenticationStateProvider.GetAuthenticationStateAsync();

            if (auth.User.Identity?.IsAuthenticated is not true)
                return null;
            else
            {
                if (Guid.TryParse(auth.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
                    return await _userService.TryReadUserAsync(id);
                return null;
            }
        });
    }

    public void Dispose() =>
        _authenticationStateProvider.AuthenticationStateChanged -= AuthChanged;

    private async void AuthChanged(Task<AuthenticationState> authState)
    {
        var auth = await authState;
        if (auth.User.Identity?.IsAuthenticated is not true)
            current = Task.FromResult<AppUser?>(null);
        else
        {
            if (Guid.TryParse(auth.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
                current = _userService.TryReadUserAsync(id);
            else
                current = Task.FromResult<AppUser?>(null);
        }
        Updated?.Invoke(this, await current);
    }
}
