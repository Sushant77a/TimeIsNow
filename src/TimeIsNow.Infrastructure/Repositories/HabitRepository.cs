using Microsoft.EntityFrameworkCore;
using TimeIsNow.Application.Interfaces.Repositories;
using TimeIsNow.Infrastructure.Data;
using TimeIsNow.Domain.Entities;

namespace TimeIsNow.Infrastructure.Repositories;

public sealed class HabitRepository(TimeIsNowDbContext context) : IHabitRepository
{
    public async Task<IReadOnlyList<Habit>> GetListAsync(
        int? categoryId, bool? active,
        CancellationToken cancellationToken = default)
    {
        var query = context.Habits
            .Where(habit => !habit.IsDeleted);

        if (categoryId.HasValue)
            query = query.Where(habit => habit.CategoryId == categoryId.Value);

        if (active.HasValue)
            query = query.Where(habit => habit.IsActive == active.Value);

        return await query
            .OrderBy(habit => habit.Title)
            .ToListAsync(cancellationToken);
    }

    public Task<Habit?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        context.Habits.FirstOrDefaultAsync(habit => habit.Id == id && !habit.IsDeleted, cancellationToken);

    public async Task<IReadOnlyList<HabitLog>> GetLogsAsync(
        int habitId, DateOnly? from, DateOnly? to,
        CancellationToken cancellationToken = default)
    {
        var query = context.HabitLogs
            .Where(log => log.HabitId == habitId);

        if (from.HasValue)
            query = query.Where(log => log.Date >= from.Value);

        if (to.HasValue)
            query = query.Where(log => log.Date <= to.Value);

        return await query
            .OrderByDescending(log => log.Date)
            .ToListAsync(cancellationToken);
    }

    public Task<HabitLog?> FindLogAsync(int habitId, DateOnly date, CancellationToken cancellationToken = default) =>
        context.HabitLogs.FirstOrDefaultAsync(log => log.HabitId == habitId && log.Date == date, cancellationToken);

    public Task<HabitLog?> FindLogByIdAsync(int id, CancellationToken cancellationToken = default) =>
        context.HabitLogs.FirstOrDefaultAsync(log => log.Id == id, cancellationToken);

    public void Add(Habit habit) => context.Habits.Add(habit);

    public void Remove(Habit habit) => habit.IsDeleted = true;

    public void AddLog(HabitLog log) => context.HabitLogs.Add(log);

    public void RemoveLog(HabitLog log) => context.HabitLogs.Remove(log);
}
