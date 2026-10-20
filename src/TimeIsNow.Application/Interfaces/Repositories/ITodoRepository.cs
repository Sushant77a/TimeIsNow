using TimeIsNow.Domain.Entities;

namespace TimeIsNow.Application.Interfaces.Repositories;

public interface ITodoRepository
{
    Task<IReadOnlyList<Todo>> GetListAsync(
        int? categoryId, bool? isDone, DateTime? dueBefore, bool includeSubtasks,
        CancellationToken cancellationToken = default);

    Task<Todo?> FindByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Todo>> GetSubTasksAsync(int parentTodoId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Todo>> GetDescendantsAsync(int parentTodoId, CancellationToken cancellationToken = default);
    void Add(Todo todo);
    void Remove(Todo todo);
}
