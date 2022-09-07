namespace InglemoorCodingComputing.Models;

public record UserAuth(Guid Id, string Email, bool IsAdmin, Argon2idHash Hash, string? ResetToken);
