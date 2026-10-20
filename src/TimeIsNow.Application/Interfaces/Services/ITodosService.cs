using TimeIsNow.Application.Features.Todos;

namespace TimeIsNow.Application.Interfaces.Services;

public interface ITodosService
{
    Task<IReadOnlyList<TodoDto>> GetListAsync(TodoListQuery query, CancellationToken cancellationToken = default);
    Task<TodoDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<TodoDto> CreateAsync(TodoRequest request, CancellationToken cancellationToken = default);
    Task<TodoDto> UpdateAsync(int id, TodoRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
