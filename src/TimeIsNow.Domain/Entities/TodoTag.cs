namespace TimeIsNow.Domain.Entities;

using TimeIsNow.Domain.Enums;

public class TodoTag
{
    public int TodoId { get; set; }
    public int TagId { get; set; }

    public Todo Todo { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
