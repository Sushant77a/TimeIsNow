using TimeIsNow.Application.Features.Practices;

namespace TimeIsNow.Application.Interfaces.Services;

public interface IPracticePlanGenerator
{
    Task<PracticePlanDraftDto> GenerateAsync(PracticePlanRequest request, CancellationToken cancellationToken = default);
}
