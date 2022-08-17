namespace InglemoorCodingComputing.Models;

public record AppUser
{
    public Guid Id { get; init; }

    public int StudentNumber { get; init; }

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public int GraduationYear { get; init; }

    public DateTime CreatedDate { get; init; }

    public DateTime? DeletedDate { get; init; }

    public static int AcademicYear =>
        new DateTime(DateTime.UtcNow.Year, 8, 1) > DateTime.UtcNow
        ? DateTime.UtcNow.Year
        : DateTime.UtcNow.Year + 1;

    public static int GradeLevelToGraduationYear(int grade) =>
        12 - grade + AcademicYear;
}