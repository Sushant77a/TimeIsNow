namespace TimeIsNow.Domain.Entities;

public class PracticeSession : BaseEntity
{
    public int PracticeId { get; set; }
    public int Order { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? TimeOfDay { get; set; }
    public int? DurationMinutes { get; set; }
    public string Instructions { get; set; } = string.Empty;

    public Practice Practice { get; set; } = null!;
}
