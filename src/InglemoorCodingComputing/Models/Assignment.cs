namespace InglemoorCodingComputing.Models;

public record Assignment
{
    public string Title { get; init; } = string.Empty;
    public Guid Id { get; set; } = Guid.NewGuid();
    public bool Published { get; init; }
    public Guid CreatedBy { get; init; }
    public DateTime Creation { get; init; }
    public DateTime? Deletion { get; init; }
    public bool Deleted => Deletion.HasValue;
    public IReadOnlyList<Submission> Submissions { get; init; } = Array.Empty<Submission>();
}
