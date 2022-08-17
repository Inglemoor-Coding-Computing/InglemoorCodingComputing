namespace InglemoorCodingComputing.Services;

public interface IUserService
{
    Task CreateUser(AppUser user);
    Task<AppUser> ReadUser(Guid id);
    Task UpdateUser(AppUser user);
    Task DeleteUser(Guid id);
}
