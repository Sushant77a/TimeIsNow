namespace TimeIsNow.Domain.Entities;

using TimeIsNow.Domain.Enums;

public class HabitLog : IUserOwned
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int HabitId { get; set; }
    public DateOnly Date { get; set; }
    public bool IsCompleted { get; set; } = true;
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }

    public Habit Habit { get; set; } = null!;
}
