namespace InglemoorCodingComputing.Services;

public interface IGroupsService
{
    Task<Group?> TryCreateGroup(string name);
    
    Task<Group?> TryReadGroup(string name);
    
    Task<Group?> TryReadGroup(Guid id);
    
    Task<bool> TryUpdateGroup(Group group);
    
    Task<bool> TryDeleteGroup(Guid id);

    IAsyncEnumerable<Group> AllGroupsAsync();
}
