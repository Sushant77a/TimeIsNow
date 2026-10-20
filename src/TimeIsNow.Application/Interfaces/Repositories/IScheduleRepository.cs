using TimeIsNow.Domain.Entities;
using TimeIsNow.Domain.Enums;

namespace TimeIsNow.Application.Interfaces.Repositories;

public interface IScheduleRepository
{
    Task<IReadOnlyList<ScheduleItem>> GetListAsync(
        int? categoryId, DateTime? from, DateTime? to, ScheduleStatus? status,
        CancellationToken cancellationToken = default);

    Task<ScheduleItem?> FindByIdAsync(int id, CancellationToken cancellationToken = default);
    void Add(ScheduleItem item);
    void Remove(ScheduleItem item);
}
