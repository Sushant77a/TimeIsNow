using TimeIsNow.Domain.Entities;

namespace TimeIsNow.Application.Interfaces.Repositories;

public interface IDailyEntryRepository
{
    Task<IReadOnlyList<DailyEntry>> GetListAsync(
        int? categoryId, DateOnly? from, DateOnly? to,
        CancellationToken cancellationToken = default);

    Task<DailyEntry?> FindByIdAsync(int id, CancellationToken cancellationToken = default);
    void Add(DailyEntry entry);
    void Remove(DailyEntry entry);
}
