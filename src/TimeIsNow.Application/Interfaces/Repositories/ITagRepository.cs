using TimeIsNow.Domain.Entities;

namespace TimeIsNow.Application.Interfaces.Repositories;

public interface ITagRepository
{
    Task<IReadOnlyList<Tag>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Tag?> FindByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Tag>> EnsureManyAsync(IReadOnlyList<string> names, CancellationToken cancellationToken = default);
    void Add(Tag tag);
    void Remove(Tag tag);
}
