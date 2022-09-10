namespace InglemoorCodingComputing.Services;

using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

public class UserStateService : IUserStateService, IDisposable
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
                var id = Guid.Parse(auth.User.FindFirstValue(ClaimTypes.NameIdentifier));
                try
                {
                    return await _userService.ReadUser(id);
                }
                catch
                {
                    return null;
                }
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
                var id = Guid.Parse(auth.User.FindFirstValue(ClaimTypes.NameIdentifier));
                try
                {
                    return await _userService.ReadUser(id);
                }
                catch
                {
                    return null;
                }
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
            var id = Guid.Parse(auth.User.FindFirstValue(ClaimTypes.NameIdentifier));
            current = Task.FromResult<AppUser?>(await _userService.ReadUser(id));
        }
        Updated?.Invoke(this, await current);
    }
}
