namespace InglemoorCodingComputing.Services;

public interface IUserAuthService
{
    /// <summary>
    /// Authenticates user.
    /// </summary>
    /// <param name="username">student id</param>
    /// <param name="password"></param>
    /// <param name="ipAddress"></param>
    /// <param name="userAgent"></param>
    /// <returns></returns>
    Task<UserAuth?> AuthenticateAsync(string email, string password, string? ipAddress = null, string? userAgent = null);

    /// <summary>
    /// Registers user.
    /// </summary>
    /// <param name="username">student id</param>
    /// <param name="password"></param>
    /// <returns></returns>
    Task<UserAuth?> AddUserAsync(string email, string password);

    Task<UserAuth?> AddGoogleUserAsync(string email, string googleId);

    /// <summary>
    /// Gives a user admin role.
    /// </summary>
    /// <param name="username"></param>
    /// <param name="key">secure admin key</param>
    /// <returns></returns>
    Task<bool> GrantAdminAsync(string email, string key);

    /// <summary>
    /// Removes a user admin role.
    /// </summary>
    /// <param name="username"></param>
    /// <param name="key">secure admin key</param>
    /// <returns></returns>
    Task<bool> RevokeAdminAsync(string email, string key);

    IAsyncEnumerable<UserAuth> GetAdmins();

    bool AdminKeyValid(string key);

    Task<bool> HasAdminAsync(Guid id);

    Task<UserAuth?> UserWithEmail(string email);

    Task<UserAuth?> AuthenticateWithGoogleIdAsync(string id, string? ipAddress = null, string? userAgent = null);

    Task ChangePasswordAsync(UserAuth userAuth, string password);

    event EventHandler<Guid>? OnAdminRevoked;

    Task<bool> TryDeleteUserAsync(Guid id);

    Task<UserAuth?> TryReadUserAsync(Guid id);

    Task<bool> TryUpdateSecurityStamp(Guid id);
}