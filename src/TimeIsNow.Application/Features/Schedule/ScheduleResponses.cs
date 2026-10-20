using TimeIsNow.Domain.Enums;

namespace TimeIsNow.Application.Features.Schedule;

public sealed record ScheduleItemDto
{
    public int Id { get; init; }
    public int CategoryId { get; init; }
    public required string Title { get; init; }
    public string? Notes { get; init; }
    public DateTime ScheduledAt { get; init; }
    public DateTime? EndAt { get; init; }
    public ScheduleStatus Status { get; init; }
    public DateTime? ReminderAt { get; init; }
    public int? GoalId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
