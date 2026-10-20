namespace TimeIsNow.Domain.Entities;

using TimeIsNow.Domain.Enums;

public class Tag : IUserOwned
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public ICollection<NoteTag> NoteTags { get; set; } = new List<NoteTag>();
    public ICollection<TodoTag> TodoTags { get; set; } = new List<TodoTag>();
    public ICollection<JournalTag> JournalTags { get; set; } = new List<JournalTag>();
}
