namespace TimeIsNow.Application.Features.Briefing;

public sealed record WeeklyBriefingResponse
{
    public DateOnly WeekStart { get; init; }
    public DateOnly WeekEnd { get; init; }
    public BriefingStatsDto Stats { get; init; } = new();
    public required string Narrative { get; init; }
    public required string NarrativeSource { get; init; }
    public string? NarrativeProvider { get; init; }
}

public sealed record BriefingStatsDto
{
    public int TodosCompleted { get; init; }
    public int TodosCreated { get; init; }
    public int TodosOverdue { get; init; }
    public int HabitLogsCompleted { get; init; }
    public int HabitOpportunities { get; init; }
    public int BestStreak { get; init; }
    public string? BestStreakHabit { get; init; }
    public decimal Income { get; init; }
    public decimal Expense { get; init; }
    public decimal Balance { get; init; }
    public string? TopExpenseCategory { get; init; }
    public decimal TopExpenseAmount { get; init; }
    public int JournalEntries { get; init; }
    public int DailyEntries { get; init; }
    public int ScheduleDone { get; init; }
    public double? AverageMood { get; init; }
}

public sealed record BriefingNarrativeInput(
    DateOnly WeekStart,
    DateOnly WeekEnd,
    BriefingStatsDto Stats);

public sealed record NarrativeResult(string Text, string Source, string? Provider = null);

public static class NarrativeSources
{
    public const string Template = "template";
    public const string Ai = "ai";
}
