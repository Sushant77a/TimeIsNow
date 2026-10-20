namespace TimeIsNow.Domain.Entities;

using TimeIsNow.Domain.Enums;

public class Goal : BaseEntity, ICategoryEntity
{
    public int CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly? TargetDate { get; set; }
    public GoalStatus Status { get; set; } = GoalStatus.Active;
    public byte ProgressPercent { get; set; }

    public Category Category { get; set; } = null!;
    public ICollection<ScheduleItem> ScheduleItems { get; set; } = new List<ScheduleItem>();
}
