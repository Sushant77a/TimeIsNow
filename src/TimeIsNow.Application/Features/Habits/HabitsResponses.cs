using TimeIsNow.Domain.Enums;

namespace TimeIsNow.Application.Features.Habits;

public sealed record HabitDto
{
    public int Id { get; init; }
    public int CategoryId { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public HabitFrequency Frequency { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public sealed record HabitLogDto
{
    public int Id { get; init; }
    public int HabitId { get; init; }
    public DateOnly Date { get; init; }
    public bool IsCompleted { get; init; }
    public string? Note { get; init; }
    public DateTime CreatedAt { get; init; }
}
