using TimeIsNow.Domain.Entities;

namespace TimeIsNow.Application.Interfaces.Repositories;

public interface ICategoryModuleRepository
{
    Task<IReadOnlyList<GlobalModule>> GetGlobalModulesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CategoryModule>> GetModulesByCategoryIdAsync(int categoryId, CancellationToken cancellationToken = default);
    void AddGlobal(GlobalModule module);
    void AddCategory(CategoryModule module);
    void RemoveGlobalRange(IEnumerable<GlobalModule> modules);
    void RemoveCategoryRange(IEnumerable<CategoryModule> modules);
}
