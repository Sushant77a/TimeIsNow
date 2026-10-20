namespace TimeIsNow.Domain.Entities;

using TimeIsNow.Domain.Enums;

public class Habit : BaseEntity, ICategoryEntity
{
    public int CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public HabitFrequency Frequency { get; set; } = HabitFrequency.Daily;
    public bool IsActive { get; set; } = true;

    public Category Category { get; set; } = null!;
    public ICollection<HabitLog> Logs { get; set; } = new List<HabitLog>();
}
