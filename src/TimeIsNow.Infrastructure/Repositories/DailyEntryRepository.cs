using Microsoft.EntityFrameworkCore;
using TimeIsNow.Application.Interfaces.Repositories;
using TimeIsNow.Infrastructure.Data;
using TimeIsNow.Domain.Entities;

namespace TimeIsNow.Infrastructure.Repositories;

public sealed class DailyEntryRepository(TimeIsNowDbContext context) : IDailyEntryRepository
{
    public async Task<IReadOnlyList<DailyEntry>> GetListAsync(
        int? categoryId, DateOnly? from, DateOnly? to,
        CancellationToken cancellationToken = default)
    {
        var query = context.DailyEntries
            .Where(entry => !entry.IsDeleted);

        if (categoryId.HasValue)
            query = query.Where(entry => entry.CategoryId == categoryId.Value);

        if (from.HasValue)
            query = query.Where(entry => entry.Date >= from.Value);

        if (to.HasValue)
            query = query.Where(entry => entry.Date <= to.Value);

        return await query
            .OrderByDescending(entry => entry.Date)
            .ThenBy(entry => entry.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<DailyEntry?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        context.DailyEntries.FirstOrDefaultAsync(entry => entry.Id == id && !entry.IsDeleted, cancellationToken);

    public void Add(DailyEntry entry) => context.DailyEntries.Add(entry);

    public void Remove(DailyEntry entry) => entry.IsDeleted = true;
}
