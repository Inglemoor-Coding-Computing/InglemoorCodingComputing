namespace InglemoorCodingComputing.Models;

public class Submission
{
    public Guid Id { get; init; }
    public Guid CreatedBy { get; init; }
    public Guid? PreviousVersion { get; init; }
    public DateTime Creation { get; init; }
    public DateTime? Deletion { get; init; }
    public bool Deleted => Deletion.HasValue;
}
