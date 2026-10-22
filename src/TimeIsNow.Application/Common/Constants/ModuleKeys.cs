using TimeIsNow.Domain.Enums;

namespace TimeIsNow.Application.Common.Constants;

public static class ModuleKeys
{
    public static readonly IReadOnlyDictionary<ModuleKey, string> Names = new Dictionary<ModuleKey, string>
    {
        [ModuleKey.Dashboard] = "dashboard",
        [ModuleKey.Todos] = "todos",
        [ModuleKey.Habits] = "habits",
        [ModuleKey.DailyEntries] = "entries",
        [ModuleKey.Journal] = "journal",
        [ModuleKey.Schedule] = "schedule",
        [ModuleKey.Finance] = "finance",
        [ModuleKey.Goals] = "goals",
        [ModuleKey.Notes] = "notes",
        [ModuleKey.Settings] = "settings",
        [ModuleKey.Practices] = "practices",
    };

    private static readonly Dictionary<string, ModuleKey> Values =
        Names.ToDictionary(kv => kv.Value, kv => kv.Key, StringComparer.OrdinalIgnoreCase);

    public static string ToName(ModuleKey key) => Names[key];

    public static ModuleKey? FromName(string name) =>
        Values.TryGetValue(name, out var key) ? key : null;

    public static string[] ToNames(IEnumerable<ModuleKey> keys) =>
        [.. keys.Select(ToName)];

    public static ModuleKey[] ParseNames(IEnumerable<string> names) =>
        [.. names
            .Select(FromName)
            .Where(key => key.HasValue)
            .Select(key => key!.Value)
            .Distinct()];
}
