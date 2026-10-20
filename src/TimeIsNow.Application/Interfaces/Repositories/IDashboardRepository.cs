using TimeIsNow.Application.Features.Briefing;
using TimeIsNow.Application.Features.Dashboard;

namespace TimeIsNow.Application.Interfaces.Repositories;

public interface IDashboardRepository
{
    Task<TodayResponse> GetTodayAsync(DateOnly date, CancellationToken cancellationToken = default);
    Task<SummaryResponse> GetSummaryAsync(int year, int month, CancellationToken cancellationToken = default);
    Task<BriefingStatsDto> GetWeeklyBriefingStatsAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
}
