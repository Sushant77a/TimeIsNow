namespace TimeIsNow.Domain.Entities;

public class PracticeLog : IUserOwned
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int PracticeId { get; set; }
    public DateOnly Date { get; set; }
    public int SessionOrder { get; set; }
    public bool IsCompleted { get; set; } = true;
    public string? Note { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public Practice Practice { get; set; } = null!;
}
