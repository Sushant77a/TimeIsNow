using Microsoft.EntityFrameworkCore;
using TimeIsNow.Application.Interfaces.Repositories;
using TimeIsNow.Infrastructure.Data;
using TimeIsNow.Domain.Entities;

namespace TimeIsNow.Infrastructure.Repositories;

public sealed class CategoryRepository(TimeIsNowDbContext context) : ICategoryRepository
{
    public async Task<IReadOnlyList<Category>> GetAllAsync(bool includeArchived, CancellationToken cancellationToken = default) =>
        await context.Categories
            .Where(category => !category.IsDeleted && (includeArchived || !category.IsArchived))
            .Include(category => category.Modules)
            .OrderBy(category => category.SortOrder)
            .ToListAsync(cancellationToken);

    public Task<Category?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        context.Categories
            .FirstOrDefaultAsync(category => category.Id == id && !category.IsDeleted, cancellationToken);

    public async Task<bool> HasActiveItemsAsync(int categoryId, CancellationToken cancellationToken = default) =>
        await context.Todos.AnyAsync(x => x.CategoryId == categoryId && !x.IsDeleted, cancellationToken)
            || await context.DailyEntries.AnyAsync(x => x.CategoryId == categoryId && !x.IsDeleted, cancellationToken)
            || await context.JournalEntries.AnyAsync(x => x.CategoryId == categoryId && !x.IsDeleted, cancellationToken)
            || await context.ScheduleItems.AnyAsync(x => x.CategoryId == categoryId && !x.IsDeleted, cancellationToken)
            || await context.FinanceEntries.AnyAsync(x => x.CategoryId == categoryId && !x.IsDeleted, cancellationToken)
            || await context.Goals.AnyAsync(x => x.CategoryId == categoryId && !x.IsDeleted, cancellationToken)
            || await context.Habits.AnyAsync(x => x.CategoryId == categoryId && !x.IsDeleted, cancellationToken)
            || await context.Notes.AnyAsync(x => x.CategoryId == categoryId && !x.IsDeleted, cancellationToken);

    public void Add(Category category) => context.Categories.Add(category);

    public void Remove(Category category) => category.IsDeleted = true;
}
