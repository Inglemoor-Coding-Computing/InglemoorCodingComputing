namespace InglemoorCodingComputing.Services;

public interface IUserService
{
    Task<bool> TryCreateUserAsync(AppUser user);
    Task<AppUser?> TryReadUserAsync(Guid id);
    Task<bool> TryUpdateUserAsync(AppUser user);
    Task<bool> TryDeleteUserAsync(Guid id);

    IAsyncEnumerable<AppUser> ReadAllUsers();
}
