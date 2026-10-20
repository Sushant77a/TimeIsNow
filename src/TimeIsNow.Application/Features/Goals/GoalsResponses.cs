using TimeIsNow.Domain.Enums;

namespace TimeIsNow.Application.Features.Goals;

public sealed record GoalDto
{
    public int Id { get; init; }
    public int CategoryId { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public DateOnly? TargetDate { get; init; }
    public GoalStatus Status { get; init; }
    public byte ProgressPercent { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
