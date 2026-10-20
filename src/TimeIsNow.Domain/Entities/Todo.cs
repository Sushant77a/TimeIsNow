namespace TimeIsNow.Domain.Entities;

using TimeIsNow.Domain.Enums;

public class Todo : BaseEntity, ICategoryEntity
{
    public int CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Priority Priority { get; set; } = Priority.Medium;
    public DateTime? DueDate { get; set; }
    public DateTime? ReminderAt { get; set; }
    public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.None;
    public int? RecurrenceInterval { get; set; }
    public bool IsDone { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? ParentTodoId { get; set; }

    public Category Category { get; set; } = null!;
    public Todo? Parent { get; set; }
    public ICollection<Todo> SubTasks { get; set; } = new List<Todo>();
    public ICollection<TodoTag> Tags { get; set; } = new List<TodoTag>();
}
