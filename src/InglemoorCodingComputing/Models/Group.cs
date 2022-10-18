namespace InglemoorCodingComputing.Models;

public record Group(Guid Id, string Name, DateTime Creation)
{
    public IReadOnlyList<Guid> Assignments { get; set; } = Array.Empty<Guid>();
}
