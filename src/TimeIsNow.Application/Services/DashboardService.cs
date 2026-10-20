using TimeIsNow.Application.Features.Dashboard;
using TimeIsNow.Application.Interfaces.Repositories;
using TimeIsNow.Application.Interfaces.Services;

namespace TimeIsNow.Application.Services;

public sealed class DashboardService(IDashboardRepository dashboard) : IDashboardService
{
    public Task<TodayResponse> GetTodayAsync(DateOnly? date, CancellationToken cancellationToken = default) =>
        dashboard.GetTodayAsync(date ?? DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);

    public Task<SummaryResponse> GetSummaryAsync(int? year, int? month, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow;
        var resolvedYear = year ?? today.Year;
        var resolvedMonth = month ?? today.Month;

        return dashboard.GetSummaryAsync(resolvedYear, resolvedMonth, cancellationToken);
    }
}
