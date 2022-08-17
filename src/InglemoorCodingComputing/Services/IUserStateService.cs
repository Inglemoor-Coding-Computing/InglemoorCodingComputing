namespace InglemoorCodingComputing.Services;

public interface IUserStateService
{
    Task<AppUser?> CurrentAsync();

    event EventHandler<AppUser?>? Updated;
}
