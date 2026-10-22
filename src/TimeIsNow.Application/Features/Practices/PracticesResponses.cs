using TimeIsNow.Domain.Enums;

namespace TimeIsNow.Application.Features.Practices;

public sealed record PracticeDto
{
    public int Id { get; init; }
    public int CategoryId { get; init; }
    public required string Title { get; init; }
    public string? Overview { get; init; }
    public DateOnly StartDate { get; init; }
    public int DurationDays { get; init; }
    public int SessionsPerDay { get; init; }
    public PracticeStatus Status { get; init; }
    public required string PlanSource { get; init; }
    public List<PracticeSessionDto> Sessions { get; init; } = [];
    public PracticeStatsDto Stats { get; init; } = new();
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public sealed record PracticeSessionDto
{
    public int Id { get; init; }
    public int PracticeId { get; init; }
    public int Order { get; init; }
    public required string Name { get; init; }
    public string? TimeOfDay { get; init; }
    public int? DurationMinutes { get; init; }
    public required string Instructions { get; init; }
}

public sealed record PracticeLogDto
{
    public int Id { get; init; }
    public int PracticeId { get; init; }
    public DateOnly Date { get; init; }
    public int SessionOrder { get; init; }
    public bool IsCompleted { get; init; }
    public string? Note { get; init; }
    public DateTime? CompletedAt { get; init; }
    public DateTime CreatedAt { get; init; }
}

public sealed record PracticeStatsDto
{
    public int DayNumber { get; init; }
    public int TodayCompleted { get; init; }
    public int TodayTotal { get; init; }
    public int TotalCompleted { get; init; }
    public int ExpectedSessions { get; init; }
    public double CompletionRate { get; init; }
    public int CurrentStreak { get; init; }
    public int BestStreak { get; init; }
    public DateOnly? LastLogDate { get; init; }
}

public sealed record PracticePlanDraftDto
{
    public required string Title { get; init; }
    public string? Overview { get; init; }
    public DateOnly StartDate { get; init; }
    public int SessionsPerDay { get; init; }
    public int DurationDays { get; init; }
    public List<PracticePlanSessionDto> Sessions { get; init; } = [];
    public required string Source { get; init; }
    public string? Provider { get; init; }
}

public sealed record PracticePlanSessionDto
{
    public required string Name { get; init; }
    public string? TimeOfDay { get; init; }
    public int? DurationMinutes { get; init; }
    public required string Instructions { get; init; }
}
