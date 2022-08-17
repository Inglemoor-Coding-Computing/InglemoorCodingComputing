namespace InglemoorCodingComputing.Models;

public class AdminGrant
{
    [Range(1000000, 9999999, ErrorMessage = "Student number must be seven digits.")]
    public int StudentNumber { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string Key { get; set; } = string.Empty;
}
