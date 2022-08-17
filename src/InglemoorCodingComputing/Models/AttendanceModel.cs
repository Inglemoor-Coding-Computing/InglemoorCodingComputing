namespace InglemoorCodingComputing.Models;

public class AttendanceModel
{
    [Required, Range(1000000, 9999999, ErrorMessage = "Student number must be seven digits.")]
    public int StudentNumber { get; set; }

    [Required, Range(9, 12, ErrorMessage = "Grade level must be between 9 and 12")]
    public int GradeLevel { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string Name { get; set; } = string.Empty;
}