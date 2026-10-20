using System.ComponentModel.DataAnnotations;
using TimeIsNow.Domain.Enums;

namespace TimeIsNow.Application.Features.Habits;

public sealed record HabitRequest
{
    private string _title = null!;

    [Required]
    public int CategoryId { get; init; }

    [Required]
    [MaxLength(200)]
    public required string Title
    {
        get => _title;
        init => _title = value?.Trim()!;
    }

    public string? Description { get; init; }

    public HabitFrequency Frequency { get; init; } = HabitFrequency.Daily;

    public bool IsActive { get; init; } = true;
}

public sealed record HabitListQuery
{
    public int? CategoryId { get; init; }
    public bool? Active { get; init; }
}

public sealed record HabitLogRequest
{
    [Required]
    public DateOnly Date { get; init; }

    public bool IsCompleted { get; init; } = true;

    [MaxLength(500)]
    public string? Note { get; init; }
}

public sealed record HabitLogListQuery
{
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; }
}
