using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using TimeIsNow.Application.Common.Exceptions;
using TimeIsNow.Application.Features.Habits;
using TimeIsNow.Application.Features.Journal;
using TimeIsNow.Application.Features.Practices;
using TimeIsNow.Application.Features.Todos;
using TimeIsNow.Application.Interfaces.Services;
using TimeIsNow.Domain.Enums;

namespace TimeIsNow.API.Mcp;

[McpServerToolType]
public sealed class TimeIsNowMcpTools(
    IDashboardService dashboardService,
    IWeeklyBriefingService briefingService,
    ITodosService todosService,
    IHabitsService habitsService,
    IPracticesService practicesService,
    IJournalService journalService,
    ICategoriesService categoriesService)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
    };

    [McpServerTool(Name = "get_weekly_briefing")]
    [Description("Returns the AI-written weekly briefing with stats: todos, habit check-ins, money, writing, streaks, and mood for a given week.")]
    public async Task<string> GetWeeklyBriefing(
        [Description("Week start (Monday) in yyyy-MM-dd. Omit for the current week.")] string? weekStart = null,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(() => briefingService.GetWeeklyBriefingAsync(
            ParseOptionalDate(weekStart, nameof(weekStart)), cancellationToken));
    }

    [McpServerTool(Name = "get_today")]
    [Description("Returns today's dashboard: overdue and due-today todos, habit logs for the day, schedule events, and monthly finance summary.")]
    public async Task<string> GetToday(
        [Description("Date in yyyy-MM-dd. Omit for today.")] string? date = null,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(() => dashboardService.GetTodayAsync(
            ParseOptionalDate(date, nameof(date)), cancellationToken));
    }

    [McpServerTool(Name = "list_categories")]
    [Description("Lists the user's categories (e.g. Personal, Study, Work, Leisure). Category IDs are needed to create todos or journal entries.")]
    public async Task<string> ListCategories(CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(() => categoriesService.GetAllAsync(
            includeArchived: false, cancellationToken));
    }

    [McpServerTool(Name = "list_todos")]
    [Description("Lists todos. Filters: completed (true/false), dueBefore (todos due before this yyyy-MM-dd date). Includes subtasks.")]
    public async Task<string> ListTodos(
        [Description("true = only completed, false = only open. Omit for all.")] bool? completed = null,
        [Description("Only todos due before this yyyy-MM-dd date.")] string? dueBefore = null,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(() =>
        {
            var query = new TodoListQuery
            {
                IsDone = completed,
                DueBefore = dueBefore is null
                    ? null
                    : ParseDate(dueBefore, nameof(dueBefore)).ToDateTime(TimeOnly.MinValue),
                IncludeSubtasks = true,
            };
            return todosService.GetListAsync(query, cancellationToken);
        });
    }

    [McpServerTool(Name = "create_todo")]
    [Description("Creates a todo. Only title is required; if categoryId is omitted the first category is used. Priority: low, medium (default), high, urgent.")]
    public async Task<string> CreateTodo(
        [Description("Todo title, max 300 characters.")] string title,
        [Description("Optional details.")] string? description = null,
        [Description("Category ID from list_categories. Omit to use the default (first) category.")] int? categoryId = null,
        [Description("low | medium | high | urgent. Defaults to medium.")] string priority = "medium",
        [Description("Due date in yyyy-MM-dd. Omit for none.")] string? dueDate = null,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("title must not be empty.", nameof(title));

            var request = new TodoRequest
            {
                CategoryId = await ResolveCategoryIdAsync(categoryId, cancellationToken),
                Title = title.Trim(),
                Description = description,
                Priority = ParseEnum<Priority>(priority, nameof(priority)),
                DueDate = dueDate is null
                    ? null
                    : ParseDate(dueDate, nameof(dueDate)).ToDateTime(TimeOnly.MaxValue),
            };
            return await todosService.CreateAsync(request, cancellationToken);
        });
    }

    [McpServerTool(Name = "list_habits")]
    [Description("Lists habits with their IDs, frequency, and current streaks. Use a habit ID with log_habit.")]
    public async Task<string> ListHabits(
        [Description("true = only active habits (default). false = include inactive.")] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(() => habitsService.GetListAsync(
            new HabitListQuery { Active = activeOnly ? true : null }, cancellationToken));
    }

    [McpServerTool(Name = "log_habit")]
    [Description("Records (or updates) a habit check-in for a date. Idempotent per habit+date: logging twice just updates the entry.")]
    public async Task<string> LogHabit(
        [Description("Habit ID from list_habits.")] int habitId,
        [Description("Date in yyyy-MM-dd. Defaults to today.")] string? date = null,
        [Description("true = completed (default), false = missed/skipped.")] bool completed = true,
        [Description("Optional note, max 500 characters.")] string? note = null,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(() =>
        {
            var request = new HabitLogRequest
            {
                Date = ParseDate(
                    date ?? DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd"),
                    nameof(date)),
                IsCompleted = completed,
                Note = note,
            };
            return habitsService.UpsertLogAsync(habitId, request, cancellationToken);
        });
    }

    [McpServerTool(Name = "list_journal_entries")]
    [Description("Lists journal entries, newest first, with date, title, content, mood, and energy.")]
    public async Task<string> ListJournalEntries(
        [Description("Only entries on/after this yyyy-MM-dd date.")] string? from = null,
        [Description("Only entries on/before this yyyy-MM-dd date.")] string? to = null,
        [Description("Free-text search over title and content.")] string? search = null,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(() =>
        {
            var query = new JournalListQuery
            {
                From = ParseOptionalDate(from, nameof(from)),
                To = ParseOptionalDate(to, nameof(to)),
                Search = search,
            };
            return journalService.GetListAsync(query, cancellationToken);
        });
    }

    [McpServerTool(Name = "add_journal_entry")]
    [Description("Writes a journal entry. Content is required; mood and energy are optional ratings from 1 (lowest) to 5 (highest).")]
    public async Task<string> AddJournalEntry(
        [Description("Entry text.")] string content,
        [Description("Optional short title, max 300 characters.")] string? title = null,
        [Description("Entry date in yyyy-MM-dd. Defaults to today.")] string? date = null,
        [Description("Category ID from list_categories. Omit to use the default (first) category.")] int? categoryId = null,
        [Description("Mood rating 1-5. Optional.")] int? mood = null,
        [Description("Energy rating 1-5. Optional.")] int? energy = null,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("content must not be empty.", nameof(content));
            ValidateRange(mood, nameof(mood));
            ValidateRange(energy, nameof(energy));

            var request = new JournalEntryRequest
            {
                CategoryId = await ResolveCategoryIdAsync(categoryId, cancellationToken),
                Date = ParseDate(
                    date ?? DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd"),
                    nameof(date)),
                Title = title,
                Content = content.Trim(),
                Mood = (byte?)mood,
                Energy = (byte?)energy,
            };
            return await journalService.CreateAsync(request, cancellationToken);
        });
    }

    [McpServerTool(Name = "list_practices")]
    [Description("Lists your practice plans (for example chakra balancing) with their sessions, today's progress, streaks, and completion rate. Use a practice ID with log_practice.")]
    public async Task<string> ListPractices(
        [Description("active (default), paused, completed, abandoned, or all.")] string status = "active",
        [Description("Only practices in this category ID. Omit for all categories.")] int? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(() =>
        {
            var query = new PracticeListQuery
            {
                CategoryId = categoryId,
                Status = status.Equals("all", StringComparison.OrdinalIgnoreCase)
                    ? null
                    : ParseEnum<PracticeStatus>(status, nameof(status)),
            };
            return practicesService.GetListAsync(query, cancellationToken);
        });
    }

    [McpServerTool(Name = "log_practice")]
    [Description("Records (or clears) one practice session for a date. Idempotent per practice+date+session: calling twice just updates the entry.")]
    public async Task<string> LogPractice(
        [Description("Practice ID from list_practices.")] int practiceId,
        [Description("Session number within the day, starting at 1.")] int sessionOrder,
        [Description("Date in yyyy-MM-dd. Defaults to today.")] string? date = null,
        [Description("true = completed (default), false = clear the entry.")] bool completed = true,
        [Description("Optional note, max 500 characters.")] string? note = null,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(() =>
        {
            if (sessionOrder < 1)
                throw new ArgumentException("sessionOrder must be 1 or greater.", nameof(sessionOrder));

            var request = new PracticeLogRequest
            {
                Date = ParseDate(
                    date ?? DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd"),
                    nameof(date)),
                SessionOrder = sessionOrder,
                IsCompleted = completed,
                Note = note,
            };
            return practicesService.UpsertLogAsync(practiceId, request, cancellationToken);
        });
    }

    private static async Task<string> ExecuteAsync<T>(Func<Task<T>> action)
    {
        try
        {
            return Json(await action());
        }
        catch (McpException)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (ApiException ex)
        {
            throw new McpException(ex.Message);
        }
        catch (ArgumentException ex)
        {
            throw new McpException(ex.Message);
        }
    }

    private async Task<int> ResolveCategoryIdAsync(int? categoryId, CancellationToken cancellationToken)
    {
        if (categoryId is > 0)
            return categoryId.Value;

        var all = await categoriesService.GetAllAsync(includeArchived: false, cancellationToken);
        var first = all.FirstOrDefault()
            ?? throw new InvalidOperationException(
                "No categories exist yet. Ask the user to create a category first.");
        return first.Id;
    }

    private static void ValidateRange(int? value, string paramName)
    {
        if (value is not null && value is < 1 or > 5)
            throw new ArgumentException($"{paramName} must be between 1 and 5, got {value}.", paramName);
    }

    private static DateOnly ParseDate(string value, string paramName) =>
        DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            ? parsed
            : throw new ArgumentException(
                $"{paramName} must be a valid date in yyyy-MM-dd format, got '{value}'.", paramName);

    private static DateOnly? ParseOptionalDate(string? value, string paramName) =>
        value is null ? null : ParseDate(value, paramName);

    private static T ParseEnum<T>(string value, string paramName)
        where T : struct, Enum =>
        Enum.TryParse<T>(value, ignoreCase: true, out var parsed)
            ? parsed
            : throw new ArgumentException(
                $"{paramName} must be one of: {string.Join(", ", Enum.GetNames<T>())}. Got '{value}'.", paramName);

    private static string Json(object value) => JsonSerializer.Serialize(value, JsonOptions);
}
