using TimeIsNow.Domain.Entities;
using TimeIsNow.Domain.Enums;

namespace TimeIsNow.Application.Interfaces.Repositories;

public interface IFinanceRepository
{
    Task<IReadOnlyList<FinanceEntry>> GetListAsync(
        int? categoryId, DateOnly? from, DateOnly? to, FinanceType? type,
        CancellationToken cancellationToken = default);

    Task<FinanceEntry?> FindByIdAsync(int id, CancellationToken cancellationToken = default);
    void Add(FinanceEntry entry);
    void Remove(FinanceEntry entry);
}
