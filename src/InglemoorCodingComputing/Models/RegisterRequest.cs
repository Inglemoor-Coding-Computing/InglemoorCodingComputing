namespace InglemoorCodingComputing.Models;

using System.ComponentModel.DataAnnotations;

public class RegisterRequest
{
    [Required(AllowEmptyStrings = false)]
    public string FirstName { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Range(9, 12, ErrorMessage = "Grade level must be between 9 and 12")]
    [Required]
    public int GradeLevel { get; set; }

    [DataType(DataType.Password)]
    [MinLength(8, ErrorMessage="Password must be at least 8 characters")]
    [Required]
    public string Password { get; set; } = string.Empty;
}
