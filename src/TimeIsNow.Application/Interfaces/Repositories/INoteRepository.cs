using TimeIsNow.Domain.Entities;

namespace TimeIsNow.Application.Interfaces.Repositories;

public interface INoteRepository
{
    Task<IReadOnlyList<Note>> GetListAsync(
        int? categoryId, string? search, string? tag, bool? pinned,
        CancellationToken cancellationToken = default);

    Task<Note?> FindByIdAsync(int id, CancellationToken cancellationToken = default);
    void Add(Note note);
    void Remove(Note note);
}
