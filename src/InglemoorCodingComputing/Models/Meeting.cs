namespace InglemoorCodingComputing.Models;

public record Meeting
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public DateOnly Date { get; init; }

    public int Year => AppUser.ToAcademicYear(Date.ToDateTime(new()));

    public TimeOnly StartTime { get; init; }

    public TimeOnly EndTime { get; init; }

    public DateTime End =>
        Date.ToDateTime(EndTime);

    public DateTime CreatedAt { get; init; }

    public Guid? CreatedBy { get; init; }

    public string Details { get; init; } = string.Empty;

    public IReadOnlyList<Guid> RegisteredAttendees { get; init; } = Array.Empty<Guid>();
    
    public IReadOnlyList<AttendanceModel> NonRegisteredAttendees { get; init; } = Array.Empty<AttendanceModel>();
}