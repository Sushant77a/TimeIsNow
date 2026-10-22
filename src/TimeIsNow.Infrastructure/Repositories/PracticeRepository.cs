using Microsoft.EntityFrameworkCore;
using TimeIsNow.Application.Interfaces.Repositories;
using TimeIsNow.Domain.Entities;
using TimeIsNow.Domain.Enums;
using TimeIsNow.Infrastructure.Data;

namespace TimeIsNow.Infrastructure.Repositories;

public sealed class PracticeRepository(TimeIsNowDbContext context) : IPracticeRepository
{
    public async Task<IReadOnlyList<Practice>> GetListAsync(
        int? categoryId, PracticeStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = context.Practices
            .Where(practice => !practice.IsDeleted);

        if (categoryId.HasValue)
            query = query.Where(practice => practice.CategoryId == categoryId.Value);

        if (status.HasValue)
            query = query.Where(practice => practice.Status == status.Value);

        return await query
            .OrderBy(practice => practice.Title)
            .ToListAsync(cancellationToken);
    }

    public Task<Practice?> FindByIdAsync(int id, CancellationToken cancellationToken = default) =>
        context.Practices.FirstOrDefaultAsync(
            practice => practice.Id == id && !practice.IsDeleted, cancellationToken);

    public async Task<IReadOnlyList<PracticeSession>> GetSessionsAsync(
        int practiceId, CancellationToken cancellationToken = default)
    {
        return await context.PracticeSessions
            .Where(session => session.PracticeId == practiceId && !session.IsDeleted)
            .OrderBy(session => session.Order)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PracticeSession>> GetSessionsForPracticesAsync(
        IReadOnlyList<int> practiceIds, CancellationToken cancellationToken = default)
    {
        if (practiceIds.Count == 0)
            return [];

        return await context.PracticeSessions
            .Where(session => practiceIds.Contains(session.PracticeId) && !session.IsDeleted)
            .OrderBy(session => session.PracticeId)
            .ThenBy(session => session.Order)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PracticeLog>> GetLogsAsync(
        int practiceId, DateOnly? from, DateOnly? to,
        CancellationToken cancellationToken = default)
    {
        var query = context.PracticeLogs
            .Where(log => log.PracticeId == practiceId);

        if (from.HasValue)
            query = query.Where(log => log.Date >= from.Value);

        if (to.HasValue)
            query = query.Where(log => log.Date <= to.Value);

        return await query
            .OrderByDescending(log => log.Date)
            .ThenBy(log => log.SessionOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PracticeLog>> GetLogsForPracticesAsync(
        IReadOnlyList<int> practiceIds, CancellationToken cancellationToken = default)
    {
        if (practiceIds.Count == 0)
            return [];

        return await context.PracticeLogs
            .Where(log => practiceIds.Contains(log.PracticeId))
            .OrderBy(log => log.Date)
            .ThenBy(log => log.SessionOrder)
            .ToListAsync(cancellationToken);
    }

    public Task<PracticeLog?> FindLogAsync(
        int practiceId, DateOnly date, int sessionOrder,
        CancellationToken cancellationToken = default) =>
        context.PracticeLogs.FirstOrDefaultAsync(
            log => log.PracticeId == practiceId && log.Date == date && log.SessionOrder == sessionOrder,
            cancellationToken);

    public Task<PracticeLog?> FindLogByIdAsync(int id, CancellationToken cancellationToken = default) =>
        context.PracticeLogs.FirstOrDefaultAsync(log => log.Id == id, cancellationToken);

    public void Add(Practice practice) => context.Practices.Add(practice);

    public void Remove(Practice practice) => practice.IsDeleted = true;

    public void AddSession(PracticeSession session) => context.PracticeSessions.Add(session);

    public void RemoveSession(PracticeSession session) => session.IsDeleted = true;

    public void AddLog(PracticeLog log) => context.PracticeLogs.Add(log);

    public void RemoveLog(PracticeLog log) => context.PracticeLogs.Remove(log);
}
