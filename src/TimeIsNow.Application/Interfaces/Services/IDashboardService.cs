using TimeIsNow.Application.Features.Dashboard;

namespace TimeIsNow.Application.Interfaces.Services;

public interface IDashboardService
{
    Task<TodayResponse> GetTodayAsync(DateOnly? date, CancellationToken cancellationToken = default);
    Task<SummaryResponse> GetSummaryAsync(int? year, int? month, CancellationToken cancellationToken = default);
}
