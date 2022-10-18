namespace InglemoorCodingComputing.Models;

public record SubmissionContent
{
    public Guid Id { get; init; }
    public SubmissionType Type { get; init; }
    public string Content { get; init; } = string.Empty;
}
