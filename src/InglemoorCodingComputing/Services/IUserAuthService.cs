namespace InglemoorCodingComputing.Services;

public interface IUserAuthService
{
    /// <summary>
    /// Authenticates user.
    /// </summary>
    /// <param name="username">student id</param>
    /// <param name="password"></param>
    /// <returns></returns>
    Task<UserAuth?> AuthenticateAsync(string email, string password);

    /// <summary>
    /// Registers user.
    /// </summary>
    /// <param name="username">student id</param>
    /// <param name="password"></param>
    /// <returns></returns>
    Task<UserAuth?> AddUserAsync(string email, string password);

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

    /// <summary>
    /// Verify token
    /// </summary>
    /// <param name="token">Email verification token</param>
    /// <returns></returns>
    Task<bool> VerifyEmailAsync(string token);

    IAsyncEnumerable<UserAuth> GetAdmins();

    bool AdminKeyValid(string key);

    Task<bool> HasAdminAsync(Guid id);

    event EventHandler<Guid>? OnAdminRevoked;
}