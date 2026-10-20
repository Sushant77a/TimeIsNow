using TimeIsNow.Application.Common.Constants;
using TimeIsNow.Application.Common.Exceptions;
using TimeIsNow.Application.Features.Finance;
using TimeIsNow.Application.Interfaces.Repositories;
using TimeIsNow.Application.Interfaces.Services;
using TimeIsNow.Domain.Entities;

namespace TimeIsNow.Application.Services;

public sealed class FinanceService(
    IFinanceRepository finance,
    IUnitOfWork unitOfWork) : IFinanceService
{
    public async Task<IReadOnlyList<FinanceEntryDto>> GetListAsync(FinanceListQuery query, CancellationToken cancellationToken = default)
    {
        var results = await finance.GetListAsync(
            query.CategoryId, query.From, query.To, query.Type, cancellationToken);
        return [.. results.Select(ToDto)];
    }

    public async Task<FinanceEntryDto> CreateAsync(FinanceEntryRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var entry = new FinanceEntry
        {
            CategoryId = request.CategoryId,
            Type = request.Type,
            Amount = request.Amount,
            Date = request.Date,
            Description = request.Description,
            RecurrenceType = request.RecurrenceType,
            CreatedAt = now,
            UpdatedAt = now,
        };

        finance.Add(entry);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(entry);
    }

    public async Task<FinanceEntryDto> UpdateAsync(int id, FinanceEntryRequest request, CancellationToken cancellationToken = default)
    {
        var entry = await FindRequiredAsync(id, cancellationToken);
        entry.CategoryId = request.CategoryId;
        entry.Type = request.Type;
        entry.Amount = request.Amount;
        entry.Date = request.Date;
        entry.Description = request.Description;
        entry.RecurrenceType = request.RecurrenceType;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(entry);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entry = await FindRequiredAsync(id, cancellationToken);
        finance.Remove(entry);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<FinanceEntry> FindRequiredAsync(int id, CancellationToken cancellationToken) =>
        await finance.FindByIdAsync(id, cancellationToken)
            ?? throw new NotFound404Exception(ApiMessages.FinanceEntryNotFound);

    private static FinanceEntryDto ToDto(FinanceEntry entry) => new()
    {
        Id = entry.Id,
        CategoryId = entry.CategoryId,
        Type = entry.Type,
        Amount = entry.Amount,
        Date = entry.Date,
        Description = entry.Description,
        RecurrenceType = entry.RecurrenceType,
        CreatedAt = entry.CreatedAt,
        UpdatedAt = entry.UpdatedAt,
    };
}
