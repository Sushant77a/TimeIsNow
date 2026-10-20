using Microsoft.EntityFrameworkCore;
using TimeIsNow.Application.Interfaces.Repositories;
using TimeIsNow.Infrastructure.Data;
using TimeIsNow.Domain.Entities;

namespace TimeIsNow.Infrastructure.Repositories;

public sealed class JournalRepository(TimeIsNowDbContext context) : IJournalRepository
{
    public async Task<IReadOnlyList<JournalEntry>> GetListAsync(
        int? categoryId, DateOnly? from, DateOnly? to, string? search,
        CancellationToken cancellationToken = default)
    {
        var query = context.JournalEntries
            .Include(entry => entry.Tags).ThenInclude(tag => tag.Tag)
            .Where(entry => !entry.IsDeleted);

        if (categoryId.HasValue)
            query = query.Where(entry => entry.CategoryId == categoryId.Value);

        if (from.HasValue)
            query = query.Where(entry => entry.Date >= from.Value);

        if (to.HasValue)
            query = query.Where(entry => entry.Date <= to.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(entry =>
                (entry.Title != null && entry.Title.Contains(search)) ||
                entry.Content.Contains(search));

        return await query
            .OrderByDescending(entry => entry.Date)
            .ThenByDescending(entry => entry.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<JournalEntry?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        context.JournalEntries
            .Include(entry => entry.Tags).ThenInclude(tag => tag.Tag)
            .FirstOrDefaultAsync(entry => entry.Id == id && !entry.IsDeleted, cancellationToken);

    public void Add(JournalEntry entry) => context.JournalEntries.Add(entry);

    public void Remove(JournalEntry entry) => entry.IsDeleted = true;
}
