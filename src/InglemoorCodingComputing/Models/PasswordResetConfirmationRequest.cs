namespace InglemoorCodingComputing.Models;

public class PasswordResetConfirmationRequest
{
    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
}
