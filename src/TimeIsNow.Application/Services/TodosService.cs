using TimeIsNow.Application.Common.Constants;
using TimeIsNow.Application.Common.Exceptions;
using TimeIsNow.Application.Features.Todos;
using TimeIsNow.Application.Interfaces.Repositories;
using TimeIsNow.Application.Interfaces.Services;
using TimeIsNow.Domain.Entities;
using TimeIsNow.Domain.Enums;

namespace TimeIsNow.Application.Services;

public sealed class TodosService(
    ITodoRepository todos,
    ITagRepository tags,
    IUnitOfWork unitOfWork) : ITodosService
{
    public async Task<IReadOnlyList<TodoDto>> GetListAsync(TodoListQuery query, CancellationToken cancellationToken = default)
    {
        var results = await todos.GetListAsync(
            query.CategoryId, query.IsDone, query.DueBefore, query.IncludeSubtasks, cancellationToken);
        return [.. results.Select(ToDto)];
    }

    public async Task<TodoDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var todo = await FindRequiredAsync(id, cancellationToken);
        return ToDto(todo);
    }

    public async Task<TodoDto> CreateAsync(TodoRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var todo = new Todo
        {
            CategoryId = request.CategoryId,
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            DueDate = request.DueDate,
            ReminderAt = request.ReminderAt,
            RecurrenceType = request.RecurrenceType,
            RecurrenceInterval = request.RecurrenceInterval,
            ParentTodoId = request.ParentTodoId,
            IsDone = request.IsDone,
            CompletedAt = request.IsDone ? now : null,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await ApplyTagsAsync(todo, request.Tags, cancellationToken);

        todos.Add(todo);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(todo);
    }

    public async Task<TodoDto> UpdateAsync(int id, TodoRequest request, CancellationToken cancellationToken = default)
    {
        var todo = await FindRequiredAsync(id, cancellationToken);
        todo.CategoryId = request.CategoryId;
        todo.Title = request.Title;
        todo.Description = request.Description;
        todo.Priority = request.Priority;
        todo.DueDate = request.DueDate;
        todo.ReminderAt = request.ReminderAt;
        todo.RecurrenceType = request.RecurrenceType;
        todo.RecurrenceInterval = request.RecurrenceInterval;
        todo.ParentTodoId = request.ParentTodoId;
        todo.IsDone = request.IsDone;
        todo.CompletedAt = request.IsDone
            ? todo.CompletedAt ?? DateTime.UtcNow
            : null;

        await ApplyTagsAsync(todo, request.Tags, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(todo);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var todo = await FindRequiredAsync(id, cancellationToken);

        var descendants = await todos.GetDescendantsAsync(id, cancellationToken);
        foreach (var descendant in descendants)
            descendant.IsDeleted = true;

        todos.Remove(todo);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task ApplyTagsAsync(Todo todo, List<string>? tagNames, CancellationToken cancellationToken)
    {
        if (tagNames is null)
            return;

        var resolved = await tags.EnsureManyAsync(tagNames, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        todo.Tags.Clear();
        foreach (var tag in resolved)
            todo.Tags.Add(new TodoTag { Todo = todo, Tag = tag });
    }

    private async Task<Todo> FindRequiredAsync(int id, CancellationToken cancellationToken) =>
        await todos.FindByIdAsync(id, cancellationToken)
            ?? throw new NotFound404Exception(ApiMessages.TodoNotFound);

    private static TodoDto ToDto(Todo todo) => new()
    {
        Id = todo.Id,
        CategoryId = todo.CategoryId,
        Title = todo.Title,
        Description = todo.Description,
        Priority = todo.Priority,
        DueDate = todo.DueDate,
        ReminderAt = todo.ReminderAt,
        RecurrenceType = todo.RecurrenceType,
        RecurrenceInterval = todo.RecurrenceInterval,
        IsDone = todo.IsDone,
        CompletedAt = todo.CompletedAt,
        ParentTodoId = todo.ParentTodoId,
        Tags = [.. todo.Tags.Select(tag => tag.Tag.Name)],
        SubTasks = [.. todo.SubTasks.Where(sub => !sub.IsDeleted).OrderBy(sub => sub.Id).Select(ToDto)],
        CreatedAt = todo.CreatedAt,
        UpdatedAt = todo.UpdatedAt,
    };
}
