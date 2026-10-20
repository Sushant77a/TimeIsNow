namespace TimeIsNow.Domain.Entities;

using TimeIsNow.Domain.Enums;

public class CategoryModule : BaseEntity
{
    public int CategoryId { get; set; }
    public ModuleKey ModuleKey { get; set; }
    public int SortOrder { get; set; }

    public Category Category { get; set; } = null!;
}
