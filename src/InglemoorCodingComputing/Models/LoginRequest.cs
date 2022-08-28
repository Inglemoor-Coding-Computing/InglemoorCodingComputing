namespace InglemoorCodingComputing.Models;

public class LoginRequest
{
    [Required(AllowEmptyStrings = false)]
    public string Email { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string Password { get; set; } = string.Empty;
}
