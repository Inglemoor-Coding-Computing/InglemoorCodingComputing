namespace InglemoorCodingComputing.Services;

public interface ISubmissionContentService
{
    Task<bool> TryCreateAsync(SubmissionContent assignment);
    Task<SubmissionContent?> TryReadAsync(Guid id);
}