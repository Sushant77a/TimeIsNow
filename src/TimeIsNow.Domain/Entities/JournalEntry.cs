namespace TimeIsNow.Domain.Entities;

using TimeIsNow.Domain.Enums;

public class JournalEntry : BaseEntity, ICategoryEntity
{
    public int CategoryId { get; set; }
    public DateOnly Date { get; set; }
    public string? Title { get; set; }
    public string Content { get; set; } = string.Empty;
    public byte? Mood { get; set; }
    public byte? Energy { get; set; }

    public Category Category { get; set; } = null!;
    public ICollection<JournalTag> Tags { get; set; } = new List<JournalTag>();
}
