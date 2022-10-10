namespace InglemoorCodingComputing.Models;

public record AppUser
{
    public Guid Id { get; init; }

    public string Email { get; init; } = string.Empty;

    public int? StudentNumber =>
        Email.EndsWith("@apps.nsd.org") &&
        Email.Length == 20 &&
        int.TryParse(Email.Split('@')[0], out var sid)
        ? sid
        : null;

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public int GraduationYear { get; init; }

    public DateTime CreatedDate { get; init; }

    public DateTime? DeletedDate { get; init; }

    public IReadOnlyList<Guid> Groups { get; init; } = Array.Empty<Guid>();

    public static int AcademicYear => ToAcademicYear(DateTime.UtcNow);

    public bool RegistrationIncomplete => GraduationYear == -1;

    public static int ToAcademicYear(DateTime dt) =>
        new DateTime(dt.Year, 8, 1) > dt
        ? dt.Year
        : dt.Year + 1;

    public static int CalenderYear(int academicYear, int month, int day) =>
        new DateTime(DateTime.UtcNow.Year, 8, 1) > new DateTime(DateTime.UtcNow.Year, month, day)
        ? academicYear
        : academicYear - 1;

    public static int GradeLevelToGraduationYear(int grade) =>
        12 - grade + AcademicYear;

    public static int GraduationYearToGradeLevel(int year) =>
        12 + AcademicYear - year;
}