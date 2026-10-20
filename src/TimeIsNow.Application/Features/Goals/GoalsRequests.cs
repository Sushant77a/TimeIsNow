using System.ComponentModel.DataAnnotations;
using TimeIsNow.Domain.Enums;

namespace TimeIsNow.Application.Features.Goals;

public sealed record GoalRequest
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

    public string? Description { get; init; }

    public DateOnly? TargetDate { get; init; }

    public GoalStatus Status { get; init; } = GoalStatus.Active;

    [Range(0, 100)]
    public byte ProgressPercent { get; init; }
}

public sealed record GoalListQuery
{
    public int? CategoryId { get; init; }
    public GoalStatus? Status { get; init; }
}
