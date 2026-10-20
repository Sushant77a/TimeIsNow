namespace TimeIsNow.Domain.Entities;

using TimeIsNow.Domain.Enums;

public class DailyEntry : BaseEntity, ICategoryEntity
{
    public int CategoryId { get; set; }
    public DateOnly Date { get; set; }
    public string Content { get; set; } = string.Empty;
    public int? DurationMinutes { get; set; }
    public byte? Mood { get; set; }
    public byte? Energy { get; set; }

    public Category Category { get; set; } = null!;
}
