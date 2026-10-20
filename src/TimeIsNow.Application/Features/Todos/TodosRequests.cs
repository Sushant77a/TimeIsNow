using System.ComponentModel.DataAnnotations;
using TimeIsNow.Domain.Enums;

namespace TimeIsNow.Application.Features.Todos;

public sealed record TodoRequest
{
    private string _title = null!;

    [Required]
    public int CategoryId { get; init; }

    [Required]
    [MaxLength(300)]
    public required string Title
    {
        get => _title;
        init => _title = value?.Trim()!;
    }

    public string? Description { get; init; }

    public Priority Priority { get; init; } = Priority.Medium;

    public DateTime? DueDate { get; init; }

    public DateTime? ReminderAt { get; init; }

    public RecurrenceType RecurrenceType { get; init; } = RecurrenceType.None;

    [Range(1, 365)]
    public int? RecurrenceInterval { get; init; }

    public bool IsDone { get; init; }

    public int? ParentTodoId { get; init; }

    public List<string>? Tags { get; init; }
}

public sealed record TodoListQuery
{
    public int? CategoryId { get; init; }
    public bool? IsDone { get; init; }
    public DateTime? DueBefore { get; init; }
    public bool IncludeSubtasks { get; init; }
}
