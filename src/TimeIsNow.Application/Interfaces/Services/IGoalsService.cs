using TimeIsNow.Application.Features.Goals;

namespace TimeIsNow.Application.Interfaces.Services;

public interface IGoalsService
{
    Task<IReadOnlyList<GoalDto>> GetListAsync(GoalListQuery query, CancellationToken cancellationToken = default);
    Task<GoalDto> CreateAsync(GoalRequest request, CancellationToken cancellationToken = default);
    Task<GoalDto> UpdateAsync(int id, GoalRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
