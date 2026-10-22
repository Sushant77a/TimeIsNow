using System.ComponentModel.DataAnnotations;
using TimeIsNow.Domain.Enums;

namespace TimeIsNow.Application.Features.Practices;

public sealed record PracticeListQuery
{
    public int? CategoryId { get; init; }
    public PracticeStatus? Status { get; init; }
}

public sealed record PracticeSessionInput
{
    private string _name = null!;
    private string _instructions = null!;

    [Required]
    [MaxLength(200)]
    public required string Name
    {
        get => _name;
        init => _name = value?.Trim()!;
    }

    [MaxLength(30)]
    public string? TimeOfDay { get; init; }

    [Range(1, 600)]
    public int? DurationMinutes { get; init; }

    [Required]
    public required string Instructions
    {
        get => _instructions;
        init => _instructions = value?.Trim()!;
    }
}

public sealed record PracticeUpsertRequest
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

    [MaxLength(2000)]
    public string? Overview { get; init; }

    [Required]
    public DateOnly StartDate { get; init; }

    [Range(1, 365)]
    public int DurationDays { get; init; } = 21;

    public PracticeStatus Status { get; init; } = PracticeStatus.Active;

    [Required]
    [MinLength(1)]
    public required IReadOnlyList<PracticeSessionInput> Sessions { get; init; }
}

public sealed record PracticeLogRequest
{
    [Required]
    public DateOnly Date { get; init; }

    [Range(1, 20)]
    public int SessionOrder { get; init; }

    public bool IsCompleted { get; init; } = true;

    [MaxLength(500)]
    public string? Note { get; init; }
}

public sealed record PracticeLogListQuery
{
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; }
}

public sealed record PracticePlanRequest
{
    private string _prompt = null!;

    [Required]
    [MaxLength(500)]
    public required string Prompt
    {
        get => _prompt;
        init => _prompt = value?.Trim()!;
    }

    public int? CategoryId { get; init; }

    public DateOnly? StartDate { get; init; }

    [Range(1, 6)]
    public int? SessionsPerDay { get; init; }

    [Range(1, 365)]
    public int? Days { get; init; }
}
