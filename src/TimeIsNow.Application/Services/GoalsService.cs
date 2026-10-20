using TimeIsNow.Application.Common.Constants;
using TimeIsNow.Application.Common.Exceptions;
using TimeIsNow.Application.Features.Goals;
using TimeIsNow.Application.Interfaces.Repositories;
using TimeIsNow.Application.Interfaces.Services;
using TimeIsNow.Domain.Entities;
using TimeIsNow.Domain.Enums;

namespace TimeIsNow.Application.Services;

public sealed class GoalsService(
    IGoalRepository goals,
    IUnitOfWork unitOfWork) : IGoalsService
{
    public async Task<IReadOnlyList<GoalDto>> GetListAsync(GoalListQuery query, CancellationToken cancellationToken = default)
    {
        var results = await goals.GetListAsync(query.CategoryId, query.Status, cancellationToken);
        return [.. results.Select(ToDto)];
    }

    public async Task<GoalDto> CreateAsync(GoalRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var goal = new Goal
        {
            CategoryId = request.CategoryId,
            Title = request.Title,
            Description = request.Description,
            TargetDate = request.TargetDate,
            ProgressPercent = request.ProgressPercent,
            Status = request.Status,
            CreatedAt = now,
            UpdatedAt = now,
        };

        goals.Add(goal);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(goal);
    }

    public async Task<GoalDto> UpdateAsync(int id, GoalRequest request, CancellationToken cancellationToken = default)
    {
        var goal = await FindRequiredAsync(id, cancellationToken);
        goal.CategoryId = request.CategoryId;
        goal.Title = request.Title;
        goal.Description = request.Description;
        goal.TargetDate = request.TargetDate;
        goal.ProgressPercent = request.ProgressPercent;
        goal.Status = request.Status;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(goal);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var goal = await FindRequiredAsync(id, cancellationToken);
        goals.Remove(goal);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Goal> FindRequiredAsync(int id, CancellationToken cancellationToken) =>
        await goals.FindByIdAsync(id, cancellationToken)
            ?? throw new NotFound404Exception(ApiMessages.GoalNotFound);

    private static GoalDto ToDto(Goal goal) => new()
    {
        Id = goal.Id,
        CategoryId = goal.CategoryId,
        Title = goal.Title,
        Description = goal.Description,
        TargetDate = goal.TargetDate,
        ProgressPercent = goal.ProgressPercent,
        Status = goal.Status,
        CreatedAt = goal.CreatedAt,
        UpdatedAt = goal.UpdatedAt,
    };
}
