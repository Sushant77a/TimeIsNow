namespace TimeIsNow.Domain.Entities;

using TimeIsNow.Domain.Enums;

public class UserProfile : IUserOwned
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Currency { get; set; } = "USD";
    public byte WeekStartsOn { get; set; }
    public string Theme { get; set; } = "system";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
