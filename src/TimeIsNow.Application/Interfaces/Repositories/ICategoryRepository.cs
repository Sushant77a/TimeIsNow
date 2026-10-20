using TimeIsNow.Domain.Entities;

namespace TimeIsNow.Application.Interfaces.Repositories;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetAllAsync(bool includeArchived, CancellationToken cancellationToken = default);
    Task<Category?> FindByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> HasActiveItemsAsync(int categoryId, CancellationToken cancellationToken = default);
    void Add(Category category);
    void Remove(Category category);
}
