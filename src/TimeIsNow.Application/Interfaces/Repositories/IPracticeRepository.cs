using TimeIsNow.Domain.Entities;
using TimeIsNow.Domain.Enums;

namespace TimeIsNow.Application.Interfaces.Repositories;

public interface IPracticeRepository
{
    Task<IReadOnlyList<Practice>> GetListAsync(
        int? categoryId, PracticeStatus? status,
        CancellationToken cancellationToken = default);

    Task<Practice?> FindByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PracticeSession>> GetSessionsAsync(
        int practiceId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PracticeSession>> GetSessionsForPracticesAsync(
        IReadOnlyList<int> practiceIds, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PracticeLog>> GetLogsAsync(
        int practiceId, DateOnly? from, DateOnly? to,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PracticeLog>> GetLogsForPracticesAsync(
        IReadOnlyList<int> practiceIds, CancellationToken cancellationToken = default);

    Task<PracticeLog?> FindLogAsync(
        int practiceId, DateOnly date, int sessionOrder,
        CancellationToken cancellationToken = default);

    Task<PracticeLog?> FindLogByIdAsync(int id, CancellationToken cancellationToken = default);

    void Add(Practice practice);
    void Remove(Practice practice);
    void AddSession(PracticeSession session);
    void RemoveSession(PracticeSession session);
    void AddLog(PracticeLog log);
    void RemoveLog(PracticeLog log);
}
