using TimeIsNow.Domain.Entities;

namespace TimeIsNow.Application.Interfaces.Repositories;

public interface IJournalRepository
{
    Task<IReadOnlyList<JournalEntry>> GetListAsync(
        int? categoryId, DateOnly? from, DateOnly? to, string? search,
        CancellationToken cancellationToken = default);

    Task<JournalEntry?> FindByIdAsync(int id, CancellationToken cancellationToken = default);
    void Add(JournalEntry entry);
    void Remove(JournalEntry entry);
}
