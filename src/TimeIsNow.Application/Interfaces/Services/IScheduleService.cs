using TimeIsNow.Application.Features.Schedule;

namespace TimeIsNow.Application.Interfaces.Services;

public interface IScheduleService
{
    Task<IReadOnlyList<ScheduleItemDto>> GetListAsync(ScheduleListQuery query, CancellationToken cancellationToken = default);
    Task<ScheduleItemDto> CreateAsync(ScheduleItemRequest request, CancellationToken cancellationToken = default);
    Task<ScheduleItemDto> UpdateAsync(int id, ScheduleItemRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
