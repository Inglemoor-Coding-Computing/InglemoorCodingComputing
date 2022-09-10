namespace InglemoorCodingComputing.Services;

public interface IUserStateService
{
    Task<AppUser?> CurrentAsync();

    void Update();

    event EventHandler<AppUser?>? Updated;
}
