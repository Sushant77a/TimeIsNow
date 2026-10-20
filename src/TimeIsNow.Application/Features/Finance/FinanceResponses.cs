using TimeIsNow.Domain.Enums;

namespace TimeIsNow.Application.Features.Finance;

public sealed record FinanceEntryDto
{
    public int Id { get; init; }
    public int CategoryId { get; init; }
    public FinanceType Type { get; init; }
    public decimal Amount { get; init; }
    public DateOnly Date { get; init; }
    public string? Description { get; init; }
    public RecurrenceType RecurrenceType { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
