namespace InglemoorCodingComputing.Services;

public sealed class UserLogoutManager
{
    private readonly IUserAuthService _userAuthService;
    
    public UserLogoutManager(IUserAuthService userAuthService)
    {
        _userAuthService = userAuthService;
    }

    public event Action<Guid, bool>? UserLoggedOut;
    
    public void Logout(Guid user, bool force = false)
    {
        if (force)
        {
            _userAuthService.TryUpdateSecurityStamp(user);    
        }
        UserLoggedOut?.Invoke(user, force);
    }
}
