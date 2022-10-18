namespace InglemoorCodingComputing.Services;

public interface IAssignmentService
{
    Task<bool> TryCreateAsync(Assignment assignment);
    Task<Assignment?> TryReadAsync(Guid id);
    Task<bool> TryUpdateAsync(Assignment assignment);
    Task<bool> TryDeleteAsync(Guid id);
    IAsyncEnumerable<Assignment> AllAsync();
}
