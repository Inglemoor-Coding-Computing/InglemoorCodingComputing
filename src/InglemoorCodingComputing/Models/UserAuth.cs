namespace InglemoorCodingComputing.Models;

public record UserAuth(Guid Id, string Email, bool IsAdmin, Argon2idHash? Hash)
{
    public string? GoogleId { get; init; }
    public IReadOnlyList<LoginAttempt> LoginAttempts { get; init; } = Array.Empty<LoginAttempt>();
    public DateTime? SecurityTimeStamp { get; init; }
}

