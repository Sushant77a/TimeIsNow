using TimeIsNow.Domain.Entities;

namespace TimeIsNow.Application.Interfaces.Repositories;

public interface IHabitRepository
{
    Task<IReadOnlyList<Habit>> GetListAsync(
        int? categoryId, bool? active,
        CancellationToken cancellationToken = default);

    Task<Habit?> FindByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<HabitLog>> GetLogsAsync(
        int habitId, DateOnly? from, DateOnly? to,
        CancellationToken cancellationToken = default);

    Task<HabitLog?> FindLogAsync(int habitId, DateOnly date, CancellationToken cancellationToken = default);
    Task<HabitLog?> FindLogByIdAsync(int id, CancellationToken cancellationToken = default);
    void Add(Habit habit);
    void Remove(Habit habit);
    void AddLog(HabitLog log);
    void RemoveLog(HabitLog log);
}
