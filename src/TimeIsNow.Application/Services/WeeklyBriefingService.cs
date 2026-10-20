using TimeIsNow.Application.Features.Briefing;
using TimeIsNow.Application.Interfaces.Repositories;
using TimeIsNow.Application.Interfaces.Services;

namespace TimeIsNow.Application.Services;

public sealed class WeeklyBriefingService(
    IDashboardRepository dashboard,
    INarrativeService narrative) : IWeeklyBriefingService
{
    public async Task<WeeklyBriefingResponse> GetWeeklyBriefingAsync(
        DateOnly? weekStart,
        CancellationToken cancellationToken = default)
    {
        var start = weekStart ?? StartOfWeek(DateOnly.FromDateTime(DateTime.UtcNow));
        var end = start.AddDays(6);

        var stats = await dashboard.GetWeeklyBriefingStatsAsync(start, end, cancellationToken);
        var result = await narrative.SummarizeAsync(
            new BriefingNarrativeInput(start, end, stats),
            cancellationToken);

        return new WeeklyBriefingResponse
        {
            WeekStart = start,
            WeekEnd = end,
            Stats = stats,
            Narrative = result.Text,
            NarrativeSource = result.Source,
            NarrativeProvider = result.Provider,
        };
    }

    private static DateOnly StartOfWeek(DateOnly date)
    {
        var daysSinceMonday = ((int)date.DayOfWeek + 6) % 7;
        return date.AddDays(-daysSinceMonday);
    }
}
