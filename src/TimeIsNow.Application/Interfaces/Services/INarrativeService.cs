using TimeIsNow.Application.Features.Briefing;

namespace TimeIsNow.Application.Interfaces.Services;

public interface INarrativeService
{
    Task<NarrativeResult> SummarizeAsync(
        BriefingNarrativeInput input,
        CancellationToken cancellationToken = default);
}
