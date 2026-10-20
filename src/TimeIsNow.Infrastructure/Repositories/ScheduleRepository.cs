using Microsoft.EntityFrameworkCore;
using TimeIsNow.Application.Interfaces.Repositories;
using TimeIsNow.Infrastructure.Data;
using TimeIsNow.Domain.Entities;
using TimeIsNow.Domain.Enums;

namespace TimeIsNow.Infrastructure.Repositories;

public sealed class ScheduleRepository(TimeIsNowDbContext context) : IScheduleRepository
{
    public async Task<IReadOnlyList<ScheduleItem>> GetListAsync(
        int? categoryId, DateTime? from, DateTime? to, ScheduleStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = context.ScheduleItems
            .Where(item => !item.IsDeleted);

        if (categoryId.HasValue)
            query = query.Where(item => item.CategoryId == categoryId.Value);

        if (from.HasValue)
            query = query.Where(item => item.ScheduledAt >= from.Value);

        if (to.HasValue)
            query = query.Where(item => item.ScheduledAt <= to.Value);

        if (status.HasValue)
            query = query.Where(item => item.Status == status.Value);

        return await query
            .OrderBy(item => item.ScheduledAt)
            .ToListAsync(cancellationToken);
    }

    public Task<ScheduleItem?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        context.ScheduleItems.FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken);

    public void Add(ScheduleItem item) => context.ScheduleItems.Add(item);

    public void Remove(ScheduleItem item) => item.IsDeleted = true;
}
