namespace TimeIsNow.Domain.Entities;

using TimeIsNow.Domain.Enums;

public class Practice : BaseEntity, ICategoryEntity
{
    public int CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Overview { get; set; }
    public DateOnly StartDate { get; set; }
    public int DurationDays { get; set; } = 21;
    public int SessionsPerDay { get; set; } = 1;
    public PracticeStatus Status { get; set; } = PracticeStatus.Active;
    public string PlanSource { get; set; } = "template";

    public Category Category { get; set; } = null!;
    public ICollection<PracticeSession> Sessions { get; set; } = new List<PracticeSession>();
    public ICollection<PracticeLog> Logs { get; set; } = new List<PracticeLog>();
}
