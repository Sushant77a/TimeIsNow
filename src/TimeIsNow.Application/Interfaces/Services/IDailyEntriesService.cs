using TimeIsNow.Application.Features.DailyEntries;

namespace TimeIsNow.Application.Interfaces.Services;

public interface IDailyEntriesService
{
    Task<IReadOnlyList<DailyEntryDto>> GetListAsync(DailyEntryListQuery query, CancellationToken cancellationToken = default);
    Task<DailyEntryDto> CreateAsync(DailyEntryRequest request, CancellationToken cancellationToken = default);
    Task<DailyEntryDto> UpdateAsync(int id, DailyEntryRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
