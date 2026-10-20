namespace TimeIsNow.Domain.Entities;

using TimeIsNow.Domain.Enums;

public class ScheduleItem : BaseEntity, ICategoryEntity
{
    public int CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime ScheduledAt { get; set; }
    public DateTime? EndAt { get; set; }
    public ScheduleStatus Status { get; set; } = ScheduleStatus.Planned;
    public DateTime? ReminderAt { get; set; }
    public int? GoalId { get; set; }

    public Category Category { get; set; } = null!;
    public Goal? Goal { get; set; }
}
