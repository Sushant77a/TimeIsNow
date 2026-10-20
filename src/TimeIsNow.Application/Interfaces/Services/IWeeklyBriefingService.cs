using TimeIsNow.Application.Features.Briefing;

namespace TimeIsNow.Application.Interfaces.Services;

public interface IWeeklyBriefingService
{
    Task<WeeklyBriefingResponse> GetWeeklyBriefingAsync(
        DateOnly? weekStart,
        CancellationToken cancellationToken = default);
}
