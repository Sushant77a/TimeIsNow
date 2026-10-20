using TimeIsNow.Domain.Enums;

namespace TimeIsNow.Application.Features.Todos;

public sealed record TodoDto
{
    public int Id { get; init; }
    public int CategoryId { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public Priority Priority { get; init; }
    public DateTime? DueDate { get; init; }
    public DateTime? ReminderAt { get; init; }
    public RecurrenceType RecurrenceType { get; init; }
    public int? RecurrenceInterval { get; init; }
    public bool IsDone { get; init; }
    public DateTime? CompletedAt { get; init; }
    public int? ParentTodoId { get; init; }
    public List<string> Tags { get; init; } = [];
    public List<TodoDto> SubTasks { get; init; } = [];
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
