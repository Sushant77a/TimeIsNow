namespace TimeIsNow.Domain.Entities;

using TimeIsNow.Domain.Enums;

public class JournalTag
{
    public int JournalEntryId { get; set; }
    public int TagId { get; set; }

    public JournalEntry JournalEntry { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
