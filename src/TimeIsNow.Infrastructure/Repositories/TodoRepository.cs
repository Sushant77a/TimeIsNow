using Microsoft.EntityFrameworkCore;
using TimeIsNow.Application.Interfaces.Repositories;
using TimeIsNow.Infrastructure.Data;
using TimeIsNow.Domain.Entities;

namespace TimeIsNow.Infrastructure.Repositories;

public sealed class TodoRepository(TimeIsNowDbContext context) : ITodoRepository
{
    public async Task<IReadOnlyList<Todo>> GetListAsync(
        int? categoryId, bool? isDone, DateTime? dueBefore, bool includeSubtasks,
        CancellationToken cancellationToken = default)
    {
        var query = context.Todos
            .Include(todo => todo.Tags).ThenInclude(tag => tag.Tag)
            .Where(todo => !todo.IsDeleted);

        if (categoryId.HasValue)
            query = query.Where(todo => todo.CategoryId == categoryId.Value);

        if (isDone.HasValue)
            query = query.Where(todo => todo.IsDone == isDone.Value);

        if (dueBefore.HasValue)
            query = query.Where(todo => todo.DueDate != null && todo.DueDate <= dueBefore.Value);

        if (!includeSubtasks)
            query = query.Where(todo => todo.ParentTodoId == null);

        return await query
            .OrderBy(todo => todo.IsDone)
            .ThenBy(todo => todo.DueDate)
            .ThenByDescending(todo => todo.Priority)
            .ToListAsync(cancellationToken);
    }

    public Task<Todo?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        context.Todos
            .AsSplitQuery()
            .Include(todo => todo.Tags).ThenInclude(tag => tag.Tag)
            .Include(todo => todo.SubTasks).ThenInclude(sub => sub.Tags).ThenInclude(tag => tag.Tag)
            .FirstOrDefaultAsync(todo => todo.Id == id && !todo.IsDeleted, cancellationToken);

    public async Task<IReadOnlyList<Todo>> GetSubTasksAsync(int parentTodoId, CancellationToken cancellationToken = default) =>
        await context.Todos
            .Where(todo => todo.ParentTodoId == parentTodoId && !todo.IsDeleted)
            .OrderBy(todo => todo.Id)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Todo>> GetDescendantsAsync(int parentTodoId, CancellationToken cancellationToken = default)
    {
        var result = new List<Todo>();
        var frontier = new List<int> { parentTodoId };

        while (frontier.Count > 0)
        {
            var children = await context.Todos
                .Where(todo => todo.ParentTodoId != null && frontier.Contains(todo.ParentTodoId.Value) && !todo.IsDeleted)
                .ToListAsync(cancellationToken);

            if (children.Count == 0)
                break;

            result.AddRange(children);
            frontier = [.. children.Select(child => child.Id)];
        }

        return result;
    }

    public void Add(Todo todo) => context.Todos.Add(todo);

    public void Remove(Todo todo) => todo.IsDeleted = true;
}
