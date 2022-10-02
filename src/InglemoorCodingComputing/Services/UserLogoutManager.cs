namespace InglemoorCodingComputing.Services;

public sealed class UserLogoutManager
{
    public event Action<Guid, bool>? UserLoggedOut;
    public void Logout(Guid user, bool force = false) =>
        UserLoggedOut?.Invoke(user, force);
}
