using Microsoft.EntityFrameworkCore;
using TimeIsNow.Application.Interfaces.Repositories;
using TimeIsNow.Infrastructure.Data;
using TimeIsNow.Domain.Entities;

namespace TimeIsNow.Infrastructure.Repositories;

public sealed class CategoryModuleRepository(TimeIsNowDbContext context) : ICategoryModuleRepository
{
    public async Task<IReadOnlyList<GlobalModule>> GetGlobalModulesAsync(CancellationToken cancellationToken = default) =>
        await context.GlobalModules
            .AsNoTracking()
            .OrderBy(m => m.SortOrder)
            .ThenBy(m => m.ModuleKey)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CategoryModule>> GetModulesByCategoryIdAsync(int categoryId, CancellationToken cancellationToken = default) =>
        await context.CategoryModules
            .AsNoTracking()
            .Where(m => m.CategoryId == categoryId)
            .OrderBy(m => m.SortOrder)
            .ThenBy(m => m.ModuleKey)
            .ToListAsync(cancellationToken);

    public void AddGlobal(GlobalModule module) => context.GlobalModules.Add(module);

    public void AddCategory(CategoryModule module) => context.CategoryModules.Add(module);

    public void RemoveGlobalRange(IEnumerable<GlobalModule> modules) =>
        context.GlobalModules.RemoveRange(modules);

    public void RemoveCategoryRange(IEnumerable<CategoryModule> modules) =>
        context.CategoryModules.RemoveRange(modules);
}
