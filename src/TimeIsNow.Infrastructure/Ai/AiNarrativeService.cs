using Microsoft.Extensions.Logging;
using TimeIsNow.Application.Features.Briefing;
using TimeIsNow.Application.Interfaces.Services;
using TimeIsNow.Application.Services;

namespace TimeIsNow.Infrastructure.Ai;

public sealed class AiNarrativeService(
    AiOptions options,
    IAiChatClient client,
    TemplateNarrativeService template,
    ILogger<AiNarrativeService> logger) : INarrativeService
{
    private const string SystemPrompt =
        "You write a short weekly briefing for a personal life-tracking dashboard. " +
        "Use only the numbers you are given and never invent details. " +
        "Write 3 to 5 short sentences of plain prose. " +
        "No markdown, no headings, no bullet points, no emojis, no em dashes. " +
        "Be factual, warm, and concise.";

    public async Task<NarrativeResult> SummarizeAsync(
        BriefingNarrativeInput input,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var text = await client.CompleteAsync(
                new AiChatRequest(SystemPrompt, BuildPrompt(input)),
                cancellationToken);

            if (string.IsNullOrWhiteSpace(text))
                return await template.SummarizeAsync(input, cancellationToken);

            return new NarrativeResult(text.Trim(), NarrativeSources.Ai, options.Provider);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "AI narrative for provider '{Provider}' failed. Falling back to the template briefing.",
                options.Provider);
            return await template.SummarizeAsync(input, cancellationToken);
        }
    }

    private static string BuildPrompt(BriefingNarrativeInput input)
    {
        var stats = input.Stats;
        var mood = stats.AverageMood.HasValue
            ? stats.AverageMood.Value.ToString("0.0")
            : "n/a";
        var invariant = System.Globalization.CultureInfo.InvariantCulture;

        return
            $"Week: {input.WeekStart:yyyy-MM-dd} to {input.WeekEnd:yyyy-MM-dd}\n" +
            $"Todos completed: {stats.TodosCompleted}, created: {stats.TodosCreated}, overdue: {stats.TodosOverdue}\n" +
            $"Habit check-ins: {stats.HabitLogsCompleted} of {stats.HabitOpportunities}\n" +
            $"Best streak: {stats.BestStreak} days ({stats.BestStreakHabit ?? "none"})\n" +
            $"Income: {stats.Income.ToString(invariant)}, " +
            $"Expense: {stats.Expense.ToString(invariant)}, " +
            $"Balance: {stats.Balance.ToString(invariant)}\n" +
            $"Top expense: {stats.TopExpenseCategory ?? "none"} ({stats.TopExpenseAmount.ToString(invariant)})\n" +
            $"Journal entries: {stats.JournalEntries}, daily notes: {stats.DailyEntries}\n" +
            $"Schedule events completed: {stats.ScheduleDone}\n" +
            $"Average mood: {mood} out of 5";
    }
}
