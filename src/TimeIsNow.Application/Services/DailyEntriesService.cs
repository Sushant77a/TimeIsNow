using TimeIsNow.Application.Common.Constants;
using TimeIsNow.Application.Common.Exceptions;
using TimeIsNow.Application.Features.DailyEntries;
using TimeIsNow.Application.Interfaces.Repositories;
using TimeIsNow.Application.Interfaces.Services;
using TimeIsNow.Domain.Entities;

namespace TimeIsNow.Application.Services;

public sealed class DailyEntriesService(
    IDailyEntryRepository entries,
    IUnitOfWork unitOfWork) : IDailyEntriesService
{
    public async Task<IReadOnlyList<DailyEntryDto>> GetListAsync(DailyEntryListQuery query, CancellationToken cancellationToken = default)
    {
        var results = await entries.GetListAsync(query.CategoryId, query.From, query.To, cancellationToken);
        return [.. results.Select(ToDto)];
    }

    public async Task<DailyEntryDto> CreateAsync(DailyEntryRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var entry = new DailyEntry
        {
            CategoryId = request.CategoryId,
            Date = request.Date,
            Content = request.Content,
            DurationMinutes = request.DurationMinutes,
            Mood = request.Mood,
            Energy = request.Energy,
            CreatedAt = now,
            UpdatedAt = now,
        };

        entries.Add(entry);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(entry);
    }

    public async Task<DailyEntryDto> UpdateAsync(int id, DailyEntryRequest request, CancellationToken cancellationToken = default)
    {
        var entry = await FindRequiredAsync(id, cancellationToken);
        entry.CategoryId = request.CategoryId;
        entry.Date = request.Date;
        entry.Content = request.Content;
        entry.DurationMinutes = request.DurationMinutes;
        entry.Mood = request.Mood;
        entry.Energy = request.Energy;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(entry);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entry = await FindRequiredAsync(id, cancellationToken);
        entries.Remove(entry);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<DailyEntry> FindRequiredAsync(int id, CancellationToken cancellationToken) =>
        await entries.FindByIdAsync(id, cancellationToken)
            ?? throw new NotFound404Exception(ApiMessages.DailyEntryNotFound);

    private static DailyEntryDto ToDto(DailyEntry entry) => new()
    {
        Id = entry.Id,
        CategoryId = entry.CategoryId,
        Date = entry.Date,
        Content = entry.Content,
        DurationMinutes = entry.DurationMinutes,
        Mood = entry.Mood,
        Energy = entry.Energy,
        CreatedAt = entry.CreatedAt,
        UpdatedAt = entry.UpdatedAt,
    };
}
