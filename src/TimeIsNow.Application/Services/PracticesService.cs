using TimeIsNow.Application.Common.Constants;
using TimeIsNow.Application.Common.Exceptions;
using TimeIsNow.Application.Features.Practices;
using TimeIsNow.Application.Interfaces.Repositories;
using TimeIsNow.Application.Interfaces.Services;
using TimeIsNow.Domain.Entities;

namespace TimeIsNow.Application.Services;

public sealed class PracticesService(
    IPracticeRepository practices,
    IUnitOfWork unitOfWork) : IPracticesService
{
    public async Task<IReadOnlyList<PracticeDto>> GetListAsync(PracticeListQuery query, CancellationToken cancellationToken = default)
    {
        var results = await practices.GetListAsync(query.CategoryId, query.Status, cancellationToken);
        var ids = results.Select(practice => practice.Id).ToList();
        var sessions = await practices.GetSessionsForPracticesAsync(ids, cancellationToken);
        var logs = await practices.GetLogsForPracticesAsync(ids, cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return [.. results.Select(practice => ToDto(
            practice,
            sessions.Where(session => session.PracticeId == practice.Id),
            logs.Where(log => log.PracticeId == practice.Id),
            today))];
    }

    public async Task<PracticeDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var practice = await FindRequiredAsync(id, cancellationToken);
        var sessions = await practices.GetSessionsAsync(id, cancellationToken);
        var logs = await practices.GetLogsAsync(id, null, null, cancellationToken);

        return ToDto(practice, sessions, logs, DateOnly.FromDateTime(DateTime.UtcNow));
    }

    public async Task<PracticeDto> CreateAsync(PracticeUpsertRequest request, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var sessions = NormalizeSessions(request);

        var practice = new Practice
        {
            CategoryId = request.CategoryId,
            Title = request.Title,
            Overview = request.Overview,
            StartDate = request.StartDate,
            DurationDays = Math.Clamp(request.DurationDays, 1, 365),
            SessionsPerDay = sessions.Count,
            Status = request.Status,
            PlanSource = "manual",
            CreatedAt = now,
            UpdatedAt = now,
        };

        practices.Add(practice);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var (order, name, timeOfDay, durationMinutes, instructions) in sessions)
        {
            practices.AddSession(new PracticeSession
            {
                PracticeId = practice.Id,
                Order = order,
                Name = name,
                TimeOfDay = timeOfDay,
                DurationMinutes = durationMinutes,
                Instructions = instructions,
                CreatedAt = now,
                UpdatedAt = now,
            });
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(practice.Id, cancellationToken);
    }

    public async Task<PracticeDto> UpdateAsync(int id, PracticeUpsertRequest request, CancellationToken cancellationToken = default)
    {
        var practice = await FindRequiredAsync(id, cancellationToken);
        var sessions = NormalizeSessions(request);

        practice.CategoryId = request.CategoryId;
        practice.Title = request.Title;
        practice.Overview = request.Overview;
        practice.StartDate = request.StartDate;
        practice.DurationDays = Math.Clamp(request.DurationDays, 1, 365);
        practice.SessionsPerDay = sessions.Count;
        practice.Status = request.Status;

        var existing = await practices.GetSessionsAsync(id, cancellationToken);
        foreach (var session in existing)
            practices.RemoveSession(session);

        var now = DateTime.UtcNow;
        foreach (var (order, name, timeOfDay, durationMinutes, instructions) in sessions)
        {
            practices.AddSession(new PracticeSession
            {
                PracticeId = practice.Id,
                Order = order,
                Name = name,
                TimeOfDay = timeOfDay,
                DurationMinutes = durationMinutes,
                Instructions = instructions,
                CreatedAt = now,
                UpdatedAt = now,
            });
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var practice = await FindRequiredAsync(id, cancellationToken);

        var logs = await practices.GetLogsAsync(id, null, null, cancellationToken);
        foreach (var log in logs)
            practices.RemoveLog(log);

        var sessions = await practices.GetSessionsAsync(id, cancellationToken);
        foreach (var session in sessions)
            practices.RemoveSession(session);

        practices.Remove(practice);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PracticeLogDto>> GetLogsAsync(
        int practiceId, PracticeLogListQuery query, CancellationToken cancellationToken = default)
    {
        await FindRequiredAsync(practiceId, cancellationToken);
        var logs = await practices.GetLogsAsync(practiceId, query.From, query.To, cancellationToken);
        return [.. logs.Select(ToDto)];
    }

    public async Task<PracticeLogDto> UpsertLogAsync(
        int practiceId, PracticeLogRequest request, CancellationToken cancellationToken = default)
    {
        var practice = await FindRequiredAsync(practiceId, cancellationToken);

        if (request.SessionOrder < 1 || request.SessionOrder > practice.SessionsPerDay)
            throw new Validation400Exception(ApiMessages.PracticeSessionOrderInvalid);

        var log = await practices.FindLogAsync(practiceId, request.Date, request.SessionOrder, cancellationToken);
        if (log is null)
        {
            log = new PracticeLog
            {
                PracticeId = practiceId,
                Date = request.Date,
                SessionOrder = request.SessionOrder,
                IsCompleted = request.IsCompleted,
                Note = request.Note,
                CompletedAt = request.IsCompleted ? DateTime.UtcNow : null,
                CreatedAt = DateTime.UtcNow,
            };
            practices.AddLog(log);
        }
        else
        {
            log.IsCompleted = request.IsCompleted;
            log.Note = request.Note;
            log.CompletedAt = request.IsCompleted ? DateTime.UtcNow : null;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return ToDto(log);
    }

    public async Task DeleteLogAsync(int logId, CancellationToken cancellationToken = default)
    {
        var log = await practices.FindLogByIdAsync(logId, cancellationToken)
            ?? throw new NotFound404Exception(ApiMessages.PracticeLogNotFound);

        practices.RemoveLog(log);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Practice> FindRequiredAsync(int id, CancellationToken cancellationToken) =>
        await practices.FindByIdAsync(id, cancellationToken)
            ?? throw new NotFound404Exception(ApiMessages.PracticeNotFound);

    private static List<(int Order, string Name, string? TimeOfDay, int? DurationMinutes, string Instructions)> NormalizeSessions(
        PracticeUpsertRequest request)
    {
        var sessions = request.Sessions ?? [];
        if (sessions.Count == 0)
            throw new Validation400Exception(ApiMessages.PracticeSessionsRequired);

        return [.. sessions.Select((session, index) => (
            index + 1,
            session.Name.Trim(),
            string.IsNullOrWhiteSpace(session.TimeOfDay) ? null : session.TimeOfDay.Trim(),
            session.DurationMinutes,
            session.Instructions.Trim()))];
    }

    private static PracticeDto ToDto(
        Practice practice,
        IEnumerable<PracticeSession> sessions,
        IEnumerable<PracticeLog> logs,
        DateOnly today)
    {
        List<PracticeSessionDto> sessionList = [.. sessions.OrderBy(session => session.Order).Select(ToDto)];
        List<PracticeLogDto> logList = [.. logs.Select(ToDto)];
        var stats = ComputeStats(practice, logList, today);

        return new PracticeDto
        {
            Id = practice.Id,
            CategoryId = practice.CategoryId,
            Title = practice.Title,
            Overview = practice.Overview,
            StartDate = practice.StartDate,
            DurationDays = practice.DurationDays,
            SessionsPerDay = practice.SessionsPerDay,
            Status = practice.Status,
            PlanSource = practice.PlanSource,
            Sessions = sessionList,
            Stats = stats,
            CreatedAt = practice.CreatedAt,
            UpdatedAt = practice.UpdatedAt,
        };
    }

    private static PracticeStatsDto ComputeStats(Practice practice, IReadOnlyList<PracticeLogDto> logs, DateOnly today)
    {
        var completed = logs.Where(log => log.IsCompleted).ToList();
        var completedByDate = completed
            .GroupBy(log => log.Date)
            .ToDictionary(group => group.Key, group => group.Count());

        var todayCompleted = completedByDate.GetValueOrDefault(today);
        var totalCompleted = completed.Count;

        var dayNumber = (today.DayNumber - practice.StartDate.DayNumber) + 1;
        dayNumber = Math.Clamp(dayNumber, 0, practice.DurationDays);
        var expected = dayNumber * practice.SessionsPerDay;
        var completionRate = expected > 0
            ? Math.Clamp(totalCompleted / (double)expected, 0d, 1d)
            : 0d;

        return new PracticeStatsDto
        {
            DayNumber = dayNumber,
            TodayCompleted = todayCompleted,
            TodayTotal = practice.SessionsPerDay,
            TotalCompleted = totalCompleted,
            ExpectedSessions = expected,
            CompletionRate = Math.Round(completionRate, 4),
            CurrentStreak = ComputeCurrentStreak(completedByDate, practice.SessionsPerDay, today),
            BestStreak = ComputeBestStreak(completedByDate, practice.SessionsPerDay),
            LastLogDate = logs.Count > 0 ? logs.Max(log => log.Date) : null,
        };
    }

    private static int ComputeCurrentStreak(
        IReadOnlyDictionary<DateOnly, int> completedByDate, int sessionsPerDay, DateOnly today)
    {
        var cursor = completedByDate.ContainsKey(today) ? today : today.AddDays(-1);

        var streak = 0;
        while (completedByDate.GetValueOrDefault(cursor) >= sessionsPerDay)
        {
            streak++;
            cursor = cursor.AddDays(-1);
        }

        return streak;
    }

    private static int ComputeBestStreak(IReadOnlyDictionary<DateOnly, int> completedByDate, int sessionsPerDay)
    {
        var perfectDays = completedByDate
            .Where(pair => pair.Value >= sessionsPerDay)
            .Select(pair => pair.Key)
            .OrderBy(date => date)
            .ToList();

        var best = 0;
        var run = 0;
        DateOnly? previous = null;

        foreach (var day in perfectDays)
        {
            run = previous.HasValue && day.DayNumber == previous.Value.DayNumber + 1 ? run + 1 : 1;
            best = Math.Max(best, run);
            previous = day;
        }

        return best;
    }

    private static PracticeSessionDto ToDto(PracticeSession session) => new()
    {
        Id = session.Id,
        PracticeId = session.PracticeId,
        Order = session.Order,
        Name = session.Name,
        TimeOfDay = session.TimeOfDay,
        DurationMinutes = session.DurationMinutes,
        Instructions = session.Instructions,
    };

    private static PracticeLogDto ToDto(PracticeLog log) => new()
    {
        Id = log.Id,
        PracticeId = log.PracticeId,
        Date = log.Date,
        SessionOrder = log.SessionOrder,
        IsCompleted = log.IsCompleted,
        Note = log.Note,
        CompletedAt = log.CompletedAt,
        CreatedAt = log.CreatedAt,
    };
}
