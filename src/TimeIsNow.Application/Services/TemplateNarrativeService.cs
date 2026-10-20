using System.Globalization;
using TimeIsNow.Application.Features.Briefing;
using TimeIsNow.Application.Interfaces.Services;

namespace TimeIsNow.Application.Services;

public sealed class TemplateNarrativeService : INarrativeService
{
    public Task<NarrativeResult> SummarizeAsync(
        BriefingNarrativeInput input,
        CancellationToken cancellationToken = default)
    {
        var stats = input.Stats;
        var sentences = new List<string>
        {
            $"Week of {FormatDate(input.WeekStart)} to {FormatDate(input.WeekEnd)}.",
        };

        sentences.AddRange(TodoSentences(stats));
        sentences.AddRange(HabitSentences(stats));
        sentences.AddRange(FinanceSentences(stats));
        sentences.AddRange(ReflectionSentences(stats));

        if (IsQuiet(stats))
        {
            sentences.Add(stats.HabitOpportunities > 0
                ? "It was a quiet week. Log your habits or write an entry and this picture starts to fill in."
                : "It was a quiet week. Log a habit or write an entry and this picture starts to fill in.");
        }
        else if (stats.TodosOverdue > 0)
        {
            sentences.Add("Clearing the overdue list first would be the fastest win next week.");
        }
        else if (stats.TodosCompleted >= 5)
        {
            sentences.Add("Momentum is on your side. Keep the same pace next week.");
        }

        return Task.FromResult(
            new NarrativeResult(string.Join(' ', sentences), NarrativeSources.Template));
    }

    private static IEnumerable<string> TodoSentences(BriefingStatsDto stats)
    {
        if (stats.TodosCompleted > 0 && stats.TodosCreated > 0)
            yield return $"You closed {stats.TodosCompleted} todos and started {stats.TodosCreated} more.";
        else if (stats.TodosCompleted > 0)
            yield return $"You closed {stats.TodosCompleted} todos.";
        else if (stats.TodosCreated > 0)
            yield return $"You started {stats.TodosCreated} new todos.";

        if (stats.TodosOverdue > 0)
            yield return $"{stats.TodosOverdue} todos are still overdue.";
    }

    private static IEnumerable<string> HabitSentences(BriefingStatsDto stats)
    {
        if (stats.HabitOpportunities > 0)
            yield return $"You logged {stats.HabitLogsCompleted} of {stats.HabitOpportunities} habit check-ins.";

        if (stats.BestStreak > 0 && !string.IsNullOrEmpty(stats.BestStreakHabit))
            yield return $"{stats.BestStreakHabit} is on a {stats.BestStreak}-day streak.";
    }

    private static IEnumerable<string> FinanceSentences(BriefingStatsDto stats)
    {
        if (stats.Income == 0 && stats.Expense == 0)
            yield break;

        var direction = stats.Balance >= 0 ? "ahead" : "behind";
        yield return $"You earned {Money(stats.Income)} and spent {Money(stats.Expense)}, ending the week {Money(Math.Abs(stats.Balance))} {direction}.";

        if (stats.TopExpenseCategory != null && stats.TopExpenseAmount > 0)
            yield return $"{stats.TopExpenseCategory} took the largest share at {Money(stats.TopExpenseAmount)}.";
    }

    private static IEnumerable<string> ReflectionSentences(BriefingStatsDto stats)
    {
        if (stats.ScheduleDone > 0)
            yield return $"You completed {stats.ScheduleDone} scheduled events.";

        var written = new List<string>();
        if (stats.JournalEntries > 0)
            written.Add($"{stats.JournalEntries} journal {(stats.JournalEntries == 1 ? "entry" : "entries")}");
        if (stats.DailyEntries > 0)
            written.Add($"{stats.DailyEntries} daily {(stats.DailyEntries == 1 ? "note" : "notes")}");

        if (written.Count > 0)
            yield return $"You wrote {string.Join(" and ", written)}.";

        if (stats.AverageMood.HasValue)
            yield return $"Average mood was {stats.AverageMood.Value.ToString("0.0", CultureInfo.InvariantCulture)} out of 5.";
    }

    private static bool IsQuiet(BriefingStatsDto stats) =>
        stats.TodosCompleted == 0
        && stats.TodosCreated == 0
        && stats.HabitLogsCompleted == 0
        && stats.Income == 0
        && stats.Expense == 0
        && stats.JournalEntries == 0
        && stats.DailyEntries == 0
        && stats.ScheduleDone == 0;

    private static string FormatDate(DateOnly date) =>
        date.ToString("MMM d", CultureInfo.InvariantCulture);

    private static string Money(decimal value) =>
        value.ToString("0.##", CultureInfo.InvariantCulture);
}
