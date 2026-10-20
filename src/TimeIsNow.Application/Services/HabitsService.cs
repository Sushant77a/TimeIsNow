using TimeIsNow.Application.Common.Constants;
using TimeIsNow.Application.Common.Exceptions;
using TimeIsNow.Application.Features.Habits;
using TimeIsNow.Application.Interfaces.Repositories;
using TimeIsNow.Application.Interfaces.Services;
using TimeIsNow.Domain.Entities;

namespace TimeIsNow.Application.Services;

public sealed class HabitsService(
    IHabitRepository habits,
    IUnitOfWork unitOfWork) : IHabitsService
{
    public async Task<IReadOnlyList<HabitDto>> GetListAsync(HabitListQuery query, CancellationToken cancellationToken = default)
    {
        var results = await habits.GetListAsync(query.CategoryId, query.Active, cancellationToken);
        return [.. results.Select(ToDto)];
    }

    public async Task<HabitDto> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        ToDto(await FindRequiredAsync(id, cancellationToken));

    public async Task<HabitDto> CreateAsync(HabitRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var habit = new Habit
        {
            CategoryId = request.CategoryId,
            Title = request.Title,
            Description = request.Description,
            Frequency = request.Frequency,
            IsActive = request.IsActive,
            CreatedAt = now,
            UpdatedAt = now,
        };

        habits.Add(habit);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(habit);
    }

    public async Task<HabitDto> UpdateAsync(int id, HabitRequest request, CancellationToken cancellationToken = default)
    {
        var habit = await FindRequiredAsync(id, cancellationToken);
        habit.CategoryId = request.CategoryId;
        habit.Title = request.Title;
        habit.Description = request.Description;
        habit.Frequency = request.Frequency;
        habit.IsActive = request.IsActive;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(habit);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var habit = await FindRequiredAsync(id, cancellationToken);

        var logs = await habits.GetLogsAsync(id, null, null, cancellationToken);
        foreach (var log in logs)
            habits.RemoveLog(log);

        habits.Remove(habit);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<HabitLogDto>> GetLogsAsync(
        int habitId, HabitLogListQuery query, CancellationToken cancellationToken = default)
    {
        await FindRequiredAsync(habitId, cancellationToken);
        var logs = await habits.GetLogsAsync(habitId, query.From, query.To, cancellationToken);
        return [.. logs.Select(ToDto)];
    }

    public async Task<HabitLogDto> UpsertLogAsync(int habitId, HabitLogRequest request, CancellationToken cancellationToken = default)
    {
        await FindRequiredAsync(habitId, cancellationToken);

        var log = await habits.FindLogAsync(habitId, request.Date, cancellationToken);
        if (log is null)
        {
            log = new HabitLog
            {
                HabitId = habitId,
                Date = request.Date,
                IsCompleted = request.IsCompleted,
                Note = request.Note,
                CreatedAt = DateTime.UtcNow,
            };
            habits.AddLog(log);
        }
        else
        {
            log.IsCompleted = request.IsCompleted;
            log.Note = request.Note;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(log);
    }

    public async Task DeleteLogAsync(int habitLogId, CancellationToken cancellationToken = default)
    {
        var log = await habits.FindLogByIdAsync(habitLogId, cancellationToken)
            ?? throw new NotFound404Exception(ApiMessages.HabitLogNotFound);

        habits.RemoveLog(log);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Habit> FindRequiredAsync(int id, CancellationToken cancellationToken) =>
        await habits.FindByIdAsync(id, cancellationToken)
            ?? throw new NotFound404Exception(ApiMessages.HabitNotFound);

    private static HabitDto ToDto(Habit habit) => new()
    {
        Id = habit.Id,
        CategoryId = habit.CategoryId,
        Title = habit.Title,
        Description = habit.Description,
        Frequency = habit.Frequency,
        IsActive = habit.IsActive,
        CreatedAt = habit.CreatedAt,
        UpdatedAt = habit.UpdatedAt,
    };

    private static HabitLogDto ToDto(HabitLog log) => new()
    {
        Id = log.Id,
        HabitId = log.HabitId,
        Date = log.Date,
        IsCompleted = log.IsCompleted,
        Note = log.Note,
        CreatedAt = log.CreatedAt,
    };
}
