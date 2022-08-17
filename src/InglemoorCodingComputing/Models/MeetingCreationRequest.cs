namespace InglemoorCodingComputing.Models;

using System.Diagnostics.CodeAnalysis;

public class MeetingCreationRequest
{
    [Required(AllowEmptyStrings=false)]
    public string Name { get; set; } = string.Empty;
    
    public string Details { get; set; } = string.Empty;

    [Required]
    public int? Month { get; set; }
    
    [Required]
    public int? Day { get; set; }

    [Required]
    public string? StartTime { get; set; }
    
    [Required]
    public string? EndTime { get; set; }

    public bool ToMeeting(Guid? createdBy, [MaybeNullWhen(false)] out Meeting meeting)
    {
        try
        {
            DateOnly date = new(AppUser.AcademicYear, Month ?? -1, Day ?? -1);
            var start = TimeOnly.ParseExact(StartTime ?? string.Empty, "t");
            var end = TimeOnly.ParseExact(EndTime ?? string.Empty, "t");
            meeting = new()
            {
                Id = Guid.NewGuid(),
                Name = Name,
                Date = date,
                Details = Details,
                StartTime = start,
                EndTime = end,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };
            return true;
        }
        catch
        {
            meeting = null;
            return false;
        }
    }
}