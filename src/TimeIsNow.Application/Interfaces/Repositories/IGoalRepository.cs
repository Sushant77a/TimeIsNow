using TimeIsNow.Domain.Entities;
using TimeIsNow.Domain.Enums;

namespace TimeIsNow.Application.Interfaces.Repositories;

public interface IGoalRepository
{
    Task<IReadOnlyList<Goal>> GetListAsync(
        int? categoryId, GoalStatus? status,
        CancellationToken cancellationToken = default);

    Task<Goal?> FindByIdAsync(int id, CancellationToken cancellationToken = default);
    void Add(Goal goal);
    void Remove(Goal goal);
}
