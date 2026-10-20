namespace TimeIsNow.Domain.Entities;

using TimeIsNow.Domain.Enums;

public class GlobalModule : IUserOwned
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public ModuleKey ModuleKey { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
