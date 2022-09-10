namespace InglemoorCodingComputing.Models;

public class AccountFinalization
{
    [Required]
    [Range(9, 12)]
    public int Grade { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string FirstName { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string LastName { get; set; } = string.Empty;
}
