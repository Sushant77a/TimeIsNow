namespace TimeIsNow.Domain.Entities;

using TimeIsNow.Domain.Enums;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Color { get; set; } = "#4F46E5";
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
    public bool IsArchived { get; set; }

    public ICollection<Todo> Todos { get; set; } = new List<Todo>();
    public ICollection<DailyEntry> DailyEntries { get; set; } = new List<DailyEntry>();
    public ICollection<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();
    public ICollection<ScheduleItem> ScheduleItems { get; set; } = new List<ScheduleItem>();
    public ICollection<FinanceEntry> FinanceEntries { get; set; } = new List<FinanceEntry>();
    public ICollection<Goal> Goals { get; set; } = new List<Goal>();
    public ICollection<Habit> Habits { get; set; } = new List<Habit>();
    public ICollection<Note> Notes { get; set; } = new List<Note>();
    public ICollection<CategoryModule> Modules { get; set; } = new List<CategoryModule>();
}
