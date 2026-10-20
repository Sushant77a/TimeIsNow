using TimeIsNow.Application.Features.Habits;

namespace TimeIsNow.Application.Interfaces.Services;

public interface IHabitsService
{
    Task<IReadOnlyList<HabitDto>> GetListAsync(HabitListQuery query, CancellationToken cancellationToken = default);
    Task<HabitDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<HabitDto> CreateAsync(HabitRequest request, CancellationToken cancellationToken = default);
    Task<HabitDto> UpdateAsync(int id, HabitRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<HabitLogDto>> GetLogsAsync(int habitId, HabitLogListQuery query, CancellationToken cancellationToken = default);
    Task<HabitLogDto> UpsertLogAsync(int habitId, HabitLogRequest request, CancellationToken cancellationToken = default);
    Task DeleteLogAsync(int habitLogId, CancellationToken cancellationToken = default);
}
