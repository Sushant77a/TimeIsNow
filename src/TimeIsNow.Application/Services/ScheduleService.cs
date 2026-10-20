using TimeIsNow.Application.Common.Constants;
using TimeIsNow.Application.Common.Exceptions;
using TimeIsNow.Application.Features.Schedule;
using TimeIsNow.Application.Interfaces.Repositories;
using TimeIsNow.Application.Interfaces.Services;
using TimeIsNow.Domain.Entities;
using TimeIsNow.Domain.Enums;

namespace TimeIsNow.Application.Services;

public sealed class ScheduleService(
    IScheduleRepository schedule,
    IUnitOfWork unitOfWork) : IScheduleService
{
    public async Task<IReadOnlyList<ScheduleItemDto>> GetListAsync(ScheduleListQuery query, CancellationToken cancellationToken = default)
    {
        var results = await schedule.GetListAsync(
            query.CategoryId, query.From, query.To, query.Status, cancellationToken);
        return [.. results.Select(ToDto)];
    }

    public async Task<ScheduleItemDto> CreateAsync(ScheduleItemRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var item = new ScheduleItem
        {
            CategoryId = request.CategoryId,
            Title = request.Title,
            Notes = request.Notes,
            ScheduledAt = request.ScheduledAt,
            EndAt = request.EndAt,
            Status = request.Status,
            ReminderAt = request.ReminderAt,
            GoalId = request.GoalId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        schedule.Add(item);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(item);
    }

    public async Task<ScheduleItemDto> UpdateAsync(int id, ScheduleItemRequest request, CancellationToken cancellationToken = default)
    {
        var item = await FindRequiredAsync(id, cancellationToken);
        item.CategoryId = request.CategoryId;
        item.Title = request.Title;
        item.Notes = request.Notes;
        item.ScheduledAt = request.ScheduledAt;
        item.EndAt = request.EndAt;
        item.Status = request.Status;
        item.ReminderAt = request.ReminderAt;
        item.GoalId = request.GoalId;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(item);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = await FindRequiredAsync(id, cancellationToken);
        schedule.Remove(item);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<ScheduleItem> FindRequiredAsync(int id, CancellationToken cancellationToken) =>
        await schedule.FindByIdAsync(id, cancellationToken)
            ?? throw new NotFound404Exception(ApiMessages.ScheduleItemNotFound);

    private static ScheduleItemDto ToDto(ScheduleItem item) => new()
    {
        Id = item.Id,
        CategoryId = item.CategoryId,
        Title = item.Title,
        Notes = item.Notes,
        ScheduledAt = item.ScheduledAt,
        EndAt = item.EndAt,
        Status = item.Status,
        ReminderAt = item.ReminderAt,
        GoalId = item.GoalId,
        CreatedAt = item.CreatedAt,
        UpdatedAt = item.UpdatedAt,
    };
}
