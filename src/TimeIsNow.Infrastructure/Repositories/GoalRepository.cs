using Microsoft.EntityFrameworkCore;
using TimeIsNow.Application.Interfaces.Repositories;
using TimeIsNow.Infrastructure.Data;
using TimeIsNow.Domain.Entities;
using TimeIsNow.Domain.Enums;

namespace TimeIsNow.Infrastructure.Repositories;

public sealed class GoalRepository(TimeIsNowDbContext context) : IGoalRepository
{
    public async Task<IReadOnlyList<Goal>> GetListAsync(
        int? categoryId, GoalStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = context.Goals
            .Where(goal => !goal.IsDeleted);

        if (categoryId.HasValue)
            query = query.Where(goal => goal.CategoryId == categoryId.Value);

        if (status.HasValue)
            query = query.Where(goal => goal.Status == status.Value);

        return await query
            .OrderBy(goal => goal.Status)
            .ThenBy(goal => goal.TargetDate)
            .ToListAsync(cancellationToken);
    }

    public Task<Goal?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        context.Goals.FirstOrDefaultAsync(goal => goal.Id == id && !goal.IsDeleted, cancellationToken);

    public void Add(Goal goal) => context.Goals.Add(goal);

    public void Remove(Goal goal) => goal.IsDeleted = true;
}
