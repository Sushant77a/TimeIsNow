using TimeIsNow.Application.Features.Practices;

namespace TimeIsNow.Application.Interfaces.Services;

public interface IPracticesService
{
    Task<IReadOnlyList<PracticeDto>> GetListAsync(PracticeListQuery query, CancellationToken cancellationToken = default);
    Task<PracticeDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PracticeDto> CreateAsync(PracticeUpsertRequest request, CancellationToken cancellationToken = default);
    Task<PracticeDto> UpdateAsync(int id, PracticeUpsertRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PracticeLogDto>> GetLogsAsync(int practiceId, PracticeLogListQuery query, CancellationToken cancellationToken = default);
    Task<PracticeLogDto> UpsertLogAsync(int practiceId, PracticeLogRequest request, CancellationToken cancellationToken = default);
    Task DeleteLogAsync(int logId, CancellationToken cancellationToken = default);
}
