namespace TimeIsNow.Domain.Entities;

using TimeIsNow.Domain.Enums;

public class FinanceEntry : BaseEntity, ICategoryEntity
{
    public int CategoryId { get; set; }
    public FinanceType Type { get; set; }
    public decimal Amount { get; set; }
    public DateOnly Date { get; set; }
    public string? Description { get; set; }
    public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.None;

    public Category Category { get; set; } = null!;
}
