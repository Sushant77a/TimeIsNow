using TimeIsNow.Application.Features.Finance;

namespace TimeIsNow.Application.Interfaces.Services;

public interface IFinanceService
{
    Task<IReadOnlyList<FinanceEntryDto>> GetListAsync(FinanceListQuery query, CancellationToken cancellationToken = default);
    Task<FinanceEntryDto> CreateAsync(FinanceEntryRequest request, CancellationToken cancellationToken = default);
    Task<FinanceEntryDto> UpdateAsync(int id, FinanceEntryRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
