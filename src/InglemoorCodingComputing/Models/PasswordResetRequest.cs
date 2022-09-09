namespace InglemoorCodingComputing.Models;

public class PasswordResetRequest
{
    [Required(AllowEmptyStrings = false)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
