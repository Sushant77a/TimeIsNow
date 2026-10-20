using System.ComponentModel.DataAnnotations;
using TimeIsNow.Domain.Enums;

namespace TimeIsNow.Application.Features.Finance;

public sealed record FinanceEntryRequest
{
    [Required]
    public int CategoryId { get; init; }

    [Required]
    public FinanceType Type { get; init; }

    [Range(typeof(decimal), "0.01", "99999999999999.99")]
    public decimal Amount { get; init; }

    [Required]
    public DateOnly Date { get; init; }

    [MaxLength(500)]
    public string? Description { get; init; }

    public RecurrenceType RecurrenceType { get; init; } = RecurrenceType.None;
}

public sealed record FinanceListQuery
{
    public int? CategoryId { get; init; }
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; }
    public FinanceType? Type { get; init; }
}
