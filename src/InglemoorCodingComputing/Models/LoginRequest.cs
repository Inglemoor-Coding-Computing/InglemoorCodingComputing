namespace InglemoorCodingComputing.Models;

public class LoginRequest
{
    [Range(1000000, 9999999, ErrorMessage = "Student number must be seven digits.")]
    [Required]
    public int StudentNumber { get; set; }

    [Required]
    public string Password { get; set; } = string.Empty;
}
