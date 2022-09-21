namespace InglemoorCodingComputing.Services;

public sealed class UserLogoutManager
{
    public event Action<Guid>? UserLoggedOut;
    public void Logout(Guid user) =>
        UserLoggedOut?.Invoke(user);
}
