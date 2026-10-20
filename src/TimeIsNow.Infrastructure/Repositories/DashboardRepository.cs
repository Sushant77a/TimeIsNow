using Microsoft.EntityFrameworkCore;
using TimeIsNow.Application.Features.Briefing;
using TimeIsNow.Application.Features.Dashboard;
using TimeIsNow.Application.Interfaces.Repositories;
using TimeIsNow.Infrastructure.Data;
using TimeIsNow.Domain.Enums;

namespace TimeIsNow.Infrastructure.Repositories;

public sealed class DashboardRepository(TimeIsNowDbContext context) : IDashboardRepository
{
    private sealed record TodoRow(int Id, int CategoryId, string CategoryName, string Title, Priority Priority, DateTime? DueDate);
    private sealed record ScheduleRow(int Id, int CategoryId, string CategoryName, string Title, DateTime ScheduledAt, DateTime? EndAt, ScheduleStatus Status);
    private sealed record EntryRow(int Id, int CategoryId, string CategoryName, string Content, int? DurationMinutes);
    private sealed record HabitRow(int Id, int CategoryId, string Title, HabitFrequency Frequency, bool IsLogged);

    public async Task<TodayResponse> GetTodayAsync(DateOnly date, CancellationToken cancellationToken = default)
    {
        var startOfDay = date.ToDateTime(TimeOnly.MinValue);
        var endOfDay = date.ToDateTime(TimeOnly.MaxValue);

        var overdueTodos = await context.Todos
            .Where(todo => !todo.IsDeleted && !todo.IsDone && todo.ParentTodoId == null
                && todo.DueDate != null && todo.DueDate < startOfDay)
            .OrderBy(todo => todo.DueDate)
            .Select(todo => new TodoRow(todo.Id, todo.CategoryId, todo.Category.Name, todo.Title, todo.Priority, todo.DueDate))
            .ToListAsync(cancellationToken);

        var dueTodayTodos = await context.Todos
            .Where(todo => !todo.IsDeleted && !todo.IsDone && todo.ParentTodoId == null
                && todo.DueDate != null && todo.DueDate >= startOfDay && todo.DueDate <= endOfDay)
            .OrderBy(todo => todo.DueDate)
            .Select(todo => new TodoRow(todo.Id, todo.CategoryId, todo.Category.Name, todo.Title, todo.Priority, todo.DueDate))
            .ToListAsync(cancellationToken);

        var scheduleToday = await context.ScheduleItems
            .Where(item => !item.IsDeleted
                && item.ScheduledAt >= startOfDay && item.ScheduledAt <= endOfDay
                && item.Status == ScheduleStatus.Planned)
            .OrderBy(item => item.ScheduledAt)
            .Select(item => new ScheduleRow(item.Id, item.CategoryId, item.Category.Name, item.Title, item.ScheduledAt, item.EndAt, item.Status))
            .ToListAsync(cancellationToken);

        var entriesToday = await context.DailyEntries
            .Where(entry => !entry.IsDeleted && entry.Date == date)
            .OrderBy(entry => entry.Id)
            .Select(entry => new EntryRow(entry.Id, entry.CategoryId, entry.Category.Name, entry.Content, entry.DurationMinutes))
            .ToListAsync(cancellationToken);

        var habits = await context.Habits
            .Where(habit => !habit.IsDeleted && habit.IsActive)
            .OrderBy(habit => habit.Title)
            .Select(habit => new HabitRow(
                habit.Id,
                habit.CategoryId,
                habit.Title,
                habit.Frequency,
                habit.Logs.Any(log => log.Date == date && log.IsCompleted)))
            .ToListAsync(cancellationToken);

        return new TodayResponse
        {
            Date = date,
            OverdueTodos = [.. overdueTodos.Select(todo => new TodayTodoDto
            {
                Id = todo.Id,
                CategoryId = todo.CategoryId,
                CategoryName = todo.CategoryName,
                Title = todo.Title,
                Priority = (PriorityDto)todo.Priority,
                DueDate = todo.DueDate,
            })],
            DueTodayTodos = [.. dueTodayTodos.Select(todo => new TodayTodoDto
            {
                Id = todo.Id,
                CategoryId = todo.CategoryId,
                CategoryName = todo.CategoryName,
                Title = todo.Title,
                Priority = (PriorityDto)todo.Priority,
                DueDate = todo.DueDate,
            })],
            ScheduleToday = [.. scheduleToday.Select(item => new TodayScheduleDto
            {
                Id = item.Id,
                CategoryId = item.CategoryId,
                CategoryName = item.CategoryName,
                Title = item.Title,
                ScheduledAt = item.ScheduledAt,
                EndAt = item.EndAt,
                Status = (ScheduleStatusDto)item.Status,
            })],
            EntriesToday = [.. entriesToday.Select(entry => new TodayEntryDto
            {
                Id = entry.Id,
                CategoryId = entry.CategoryId,
                CategoryName = entry.CategoryName,
                Content = entry.Content,
                DurationMinutes = entry.DurationMinutes,
            })],
            HabitsToday = [.. habits.Select(habit => new TodayHabitDto
            {
                HabitId = habit.Id,
                CategoryId = habit.CategoryId,
                Title = habit.Title,
                Frequency = (HabitFrequencyDto)habit.Frequency,
                IsLogged = habit.IsLogged,
            })],
        };
    }

    public async Task<SummaryResponse> GetSummaryAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        var from = new DateOnly(year, month, 1);
        var to = from.AddMonths(1).AddDays(-1);

        var finance = await context.FinanceEntries
            .Where(entry => !entry.IsDeleted && entry.Date >= from && entry.Date <= to)
            .GroupBy(entry => new { entry.CategoryId, entry.Category.Name, entry.Type })
            .Select(group => new
            {
                group.Key.CategoryId,
                CategoryName = group.Key.Name,
                group.Key.Type,
                Total = group.Sum(entry => entry.Amount),
            })
            .ToListAsync(cancellationToken);

        var income = finance.Where(x => x.Type == FinanceType.Income).Sum(x => x.Total);
        var expense = finance.Where(x => x.Type == FinanceType.Expense).Sum(x => x.Total);

        var financeByCategory = finance
            .GroupBy(x => new { x.CategoryId, x.CategoryName })
            .Select(group => new SummaryCategoryFinanceDto
            {
                CategoryId = group.Key.CategoryId,
                CategoryName = group.Key.CategoryName,
                Income = group.Where(x => x.Type == FinanceType.Income).Sum(x => x.Total),
                Expense = group.Where(x => x.Type == FinanceType.Expense).Sum(x => x.Total),
            })
            .OrderByDescending(x => x.Expense)
            .ToList();

        var openTodos = await context.Todos
            .Where(todo => !todo.IsDeleted && !todo.IsDone)
            .GroupBy(todo => new { todo.CategoryId, todo.Category.Name })
            .Select(group => new SummaryCategoryCountDto
            {
                CategoryId = group.Key.CategoryId,
                CategoryName = group.Key.Name,
                OpenTodoCount = group.Count(),
            })
            .OrderByDescending(x => x.OpenTodoCount)
            .ToListAsync(cancellationToken);

        var activeHabits = await context.Habits
            .Where(habit => !habit.IsDeleted && habit.IsActive && habit.Frequency == HabitFrequency.Daily)
            .Select(habit => new
            {
                habit.Id,
                habit.Title,
                habit.CategoryId,
                Dates = habit.Logs
                    .Where(log => log.IsCompleted && log.Date >= from.AddDays(-90))
                    .Select(log => log.Date)
                    .ToList(),
            })
            .ToListAsync(cancellationToken);

        var streaks = activeHabits
            .Select(habit => new SummaryHabitStreakDto
            {
                HabitId = habit.Id,
                Title = habit.Title,
                CategoryId = habit.CategoryId,
                CurrentStreak = ComputeStreak(habit.Dates),
            })
            .OrderByDescending(x => x.CurrentStreak)
            .ToList();

        return new SummaryResponse
        {
            Month = $"{year:0000}-{month:00}",
            Income = income,
            Expense = expense,
            Balance = income - expense,
            FinanceByCategory = financeByCategory,
            OpenTodosByCategory = openTodos,
            ActiveHabits = streaks,
        };
    }

    public async Task<BriefingStatsDto> GetWeeklyBriefingStatsAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default)
    {
        var startOfWindow = from.ToDateTime(TimeOnly.MinValue);
        var endOfWindow = to.ToDateTime(TimeOnly.MaxValue);
        var now = DateTime.UtcNow;

        var todosCompleted = await context.Todos.CountAsync(todo =>
            !todo.IsDeleted && todo.IsDone
            && todo.CompletedAt >= startOfWindow && todo.CompletedAt <= endOfWindow, cancellationToken);

        var todosCreated = await context.Todos.CountAsync(todo =>
            !todo.IsDeleted
            && todo.CreatedAt >= startOfWindow && todo.CreatedAt <= endOfWindow, cancellationToken);

        var todosOverdue = await context.Todos.CountAsync(todo =>
            !todo.IsDeleted && !todo.IsDone && todo.ParentTodoId == null
            && todo.DueDate != null && todo.DueDate < now, cancellationToken);

        var habitLogsCompleted = await context.HabitLogs.CountAsync(log =>
            log.IsCompleted && log.Date >= from && log.Date <= to, cancellationToken);

        var activeDailyHabits = await context.Habits.CountAsync(habit =>
            !habit.IsDeleted && habit.IsActive && habit.Frequency == HabitFrequency.Daily, cancellationToken);

        var daysInWindow = to.DayNumber - from.DayNumber + 1;

        var streakCandidates = await context.Habits
            .Where(habit => !habit.IsDeleted && habit.IsActive && habit.Frequency == HabitFrequency.Daily)
            .Select(habit => new
            {
                habit.Title,
                Dates = habit.Logs
                    .Where(log => log.IsCompleted)
                    .Select(log => log.Date)
                    .ToList(),
            })
            .ToListAsync(cancellationToken);

        var bestStreak = streakCandidates
            .Select(habit => new { habit.Title, Streak = ComputeStreak(habit.Dates) })
            .OrderByDescending(x => x.Streak)
            .FirstOrDefault();

        var finance = await context.FinanceEntries
            .Where(entry => !entry.IsDeleted && entry.Date >= from && entry.Date <= to)
            .Select(entry => new { entry.Type, entry.Amount, entry.Category.Name })
            .ToListAsync(cancellationToken);

        var income = finance.Where(x => x.Type == FinanceType.Income).Sum(x => x.Amount);
        var expense = finance.Where(x => x.Type == FinanceType.Expense).Sum(x => x.Amount);

        var topExpense = finance
            .Where(x => x.Type == FinanceType.Expense)
            .GroupBy(x => x.Name)
            .Select(group => new { CategoryName = group.Key, Total = group.Sum(x => x.Amount) })
            .OrderByDescending(x => x.Total)
            .FirstOrDefault();

        var journalEntries = await context.JournalEntries.CountAsync(entry =>
            !entry.IsDeleted && entry.Date >= from && entry.Date <= to, cancellationToken);

        var dailyEntries = await context.DailyEntries.CountAsync(entry =>
            !entry.IsDeleted && entry.Date >= from && entry.Date <= to, cancellationToken);

        var scheduleDone = await context.ScheduleItems.CountAsync(item =>
            !item.IsDeleted && item.Status == ScheduleStatus.Done
            && item.ScheduledAt >= startOfWindow && item.ScheduledAt <= endOfWindow, cancellationToken);

        var moods = await context.DailyEntries
            .Where(entry => !entry.IsDeleted && entry.Date >= from && entry.Date <= to && entry.Mood != null)
            .Select(entry => entry.Mood)
            .ToListAsync(cancellationToken);

        moods.AddRange(await context.JournalEntries
            .Where(entry => !entry.IsDeleted && entry.Date >= from && entry.Date <= to && entry.Mood != null)
            .Select(entry => entry.Mood)
            .ToListAsync(cancellationToken));

        var measuredMoods = moods
            .Where(mood => mood.HasValue)
            .Select(mood => (double)mood!.Value)
            .ToList();

        return new BriefingStatsDto
        {
            TodosCompleted = todosCompleted,
            TodosCreated = todosCreated,
            TodosOverdue = todosOverdue,
            HabitLogsCompleted = habitLogsCompleted,
            HabitOpportunities = activeDailyHabits * daysInWindow,
            BestStreak = bestStreak?.Streak ?? 0,
            BestStreakHabit = bestStreak is { Streak: > 0 } ? bestStreak.Title : null,
            Income = income,
            Expense = expense,
            Balance = income - expense,
            TopExpenseCategory = topExpense?.CategoryName,
            TopExpenseAmount = topExpense?.Total ?? 0m,
            JournalEntries = journalEntries,
            DailyEntries = dailyEntries,
            ScheduleDone = scheduleDone,
            AverageMood = measuredMoods.Count > 0 ? Math.Round(measuredMoods.Average(), 1) : null,
        };
    }

    private static int ComputeStreak(List<DateOnly> completedDates)
    {
        if (completedDates.Count == 0)
            return 0;

        var dates = completedDates.ToHashSet();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var cursor = dates.Contains(today) ? today : today.AddDays(-1);

        if (!dates.Contains(cursor))
            return 0;

        var streak = 0;
        while (dates.Contains(cursor))
        {
            streak++;
            cursor = cursor.AddDays(-1);
        }

        return streak;
    }
}
