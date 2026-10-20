namespace TimeIsNow.Domain.Entities;

using TimeIsNow.Domain.Enums;

public class Note : BaseEntity, ICategoryEntity
{
    public int CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsPinned { get; set; }

    public Category Category { get; set; } = null!;
    public ICollection<NoteTag> Tags { get; set; } = new List<NoteTag>();
}
