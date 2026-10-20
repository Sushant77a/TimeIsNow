namespace TimeIsNow.Application.Features.Dashboard;

public sealed record TodayResponse
{
    public DateOnly Date { get; init; }
    public List<TodayTodoDto> OverdueTodos { get; init; } = [];
    public List<TodayTodoDto> DueTodayTodos { get; init; } = [];
    public List<TodayScheduleDto> ScheduleToday { get; init; } = [];
    public List<TodayEntryDto> EntriesToday { get; init; } = [];
    public List<TodayHabitDto> HabitsToday { get; init; } = [];
}

public sealed record TodayTodoDto
{
    public int Id { get; init; }
    public int CategoryId { get; init; }
    public required string CategoryName { get; init; }
    public required string Title { get; init; }
    public PriorityDto Priority { get; init; }
    public DateTime? DueDate { get; init; }
}

public sealed record TodayScheduleDto
{
    public int Id { get; init; }
    public int CategoryId { get; init; }
    public required string CategoryName { get; init; }
    public required string Title { get; init; }
    public DateTime ScheduledAt { get; init; }
    public DateTime? EndAt { get; init; }
    public ScheduleStatusDto Status { get; init; }
}

public sealed record TodayEntryDto
{
    public int Id { get; init; }
    public int CategoryId { get; init; }
    public required string CategoryName { get; init; }
    public required string Content { get; init; }
    public int? DurationMinutes { get; init; }
}

public sealed record TodayHabitDto
{
    public int HabitId { get; init; }
    public int CategoryId { get; init; }
    public required string Title { get; init; }
    public HabitFrequencyDto Frequency { get; init; }
    public bool IsLogged { get; init; }
}

public sealed record SummaryResponse
{
    public required string Month { get; init; }
    public decimal Income { get; init; }
    public decimal Expense { get; init; }
    public decimal Balance { get; init; }
    public List<SummaryCategoryFinanceDto> FinanceByCategory { get; init; } = [];
    public List<SummaryCategoryCountDto> OpenTodosByCategory { get; init; } = [];
    public List<SummaryHabitStreakDto> ActiveHabits { get; init; } = [];
}

public sealed record SummaryCategoryFinanceDto
{
    public int CategoryId { get; init; }
    public required string CategoryName { get; init; }
    public decimal Income { get; init; }
    public decimal Expense { get; init; }
}

public sealed record SummaryCategoryCountDto
{
    public int CategoryId { get; init; }
    public required string CategoryName { get; init; }
    public int OpenTodoCount { get; init; }
}

public sealed record SummaryHabitStreakDto
{
    public int HabitId { get; init; }
    public required string Title { get; init; }
    public int CategoryId { get; init; }
    public int CurrentStreak { get; init; }
}

public enum PriorityDto { Low = 0, Medium = 1, High = 2 }
public enum ScheduleStatusDto { Planned = 0, Done = 1, Skipped = 2, Cancelled = 3 }
public enum HabitFrequencyDto { Daily = 0, Weekly = 1 }
