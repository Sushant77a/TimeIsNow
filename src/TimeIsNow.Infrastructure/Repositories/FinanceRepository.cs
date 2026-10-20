using Microsoft.EntityFrameworkCore;
using TimeIsNow.Application.Interfaces.Repositories;
using TimeIsNow.Infrastructure.Data;
using TimeIsNow.Domain.Entities;
using TimeIsNow.Domain.Enums;

namespace TimeIsNow.Infrastructure.Repositories;

public sealed class FinanceRepository(TimeIsNowDbContext context) : IFinanceRepository
{
    public async Task<IReadOnlyList<FinanceEntry>> GetListAsync(
        int? categoryId, DateOnly? from, DateOnly? to, FinanceType? type,
        CancellationToken cancellationToken = default)
    {
        var query = context.FinanceEntries
            .Where(entry => !entry.IsDeleted);

        if (categoryId.HasValue)
            query = query.Where(entry => entry.CategoryId == categoryId.Value);

        if (from.HasValue)
            query = query.Where(entry => entry.Date >= from.Value);

        if (to.HasValue)
            query = query.Where(entry => entry.Date <= to.Value);

        if (type.HasValue)
            query = query.Where(entry => entry.Type == type.Value);

        return await query
            .OrderByDescending(entry => entry.Date)
            .ThenByDescending(entry => entry.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<FinanceEntry?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        context.FinanceEntries.FirstOrDefaultAsync(entry => entry.Id == id && !entry.IsDeleted, cancellationToken);

    public void Add(FinanceEntry entry) => context.FinanceEntries.Add(entry);

    public void Remove(FinanceEntry entry) => entry.IsDeleted = true;
}
