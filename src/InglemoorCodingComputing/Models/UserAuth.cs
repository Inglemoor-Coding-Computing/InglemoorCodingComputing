namespace InglemoorCodingComputing.Models;

public record UserAuth(Guid Id, string Email, bool IsAdmin, string? VerificationToken, bool Verified, Argon2idHash Hash);
