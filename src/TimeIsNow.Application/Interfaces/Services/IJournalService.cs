using TimeIsNow.Application.Features.Journal;

namespace TimeIsNow.Application.Interfaces.Services;

public interface IJournalService
{
    Task<IReadOnlyList<JournalEntryDto>> GetListAsync(JournalListQuery query, CancellationToken cancellationToken = default);
    Task<JournalEntryDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<JournalEntryDto> CreateAsync(JournalEntryRequest request, CancellationToken cancellationToken = default);
    Task<JournalEntryDto> UpdateAsync(int id, JournalEntryRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
