using System.ComponentModel.DataAnnotations;
using TimeIsNow.Domain.Enums;

namespace TimeIsNow.Application.Features.Schedule;

public sealed record ScheduleItemRequest
{
    private string _title = null!;

    [Required]
    public int CategoryId { get; init; }

    [Required]
    [MaxLength(300)]
    public required string Title
    {
        get => _title;
        init => _title = value?.Trim()!;
    }

    public string? Notes { get; init; }

    [Required]
    public DateTime ScheduledAt { get; init; }

    public DateTime? EndAt { get; init; }

    public ScheduleStatus Status { get; init; } = ScheduleStatus.Planned;

    public DateTime? ReminderAt { get; init; }

    public int? GoalId { get; init; }
}

public sealed record ScheduleListQuery
{
    public int? CategoryId { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
    public ScheduleStatus? Status { get; init; }
}
