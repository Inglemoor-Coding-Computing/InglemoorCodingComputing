namespace InglemoorCodingComputing.Models;

public record UserAuth(Guid Id, string Username, bool IsAdmin, string? VerificationToken, bool Verified, Argon2idHash Hash);
