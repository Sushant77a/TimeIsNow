namespace TimeIsNow.Domain.Entities;

using TimeIsNow.Domain.Enums;

public abstract class BaseEntity : IUserOwned
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
}
