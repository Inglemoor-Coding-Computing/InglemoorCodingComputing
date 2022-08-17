namespace InglemoorCodingComputing.Models;

public class RegisterRequest
{
    [Required(AllowEmptyStrings = false)]
    public string FirstName { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string LastName { get; set; } = string.Empty;

    [Range(1000000, 9999999, ErrorMessage = "Student number must be seven digits.")]
    [Required]
    public int StudentNumber { get; set; }

    [Range(9, 12, ErrorMessage = "Grade level must be between 9 and 12")]
    [Required]
    public int GradeLevel { get; set; }

    [DataType(DataType.Password)]
    [MinLength(8, ErrorMessage="Password must be at least 8 characters")]
    [Required]
    public string Password { get; set; } = string.Empty;
}
