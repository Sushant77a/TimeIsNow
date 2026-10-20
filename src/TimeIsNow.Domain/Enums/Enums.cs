namespace TimeIsNow.Domain.Enums;

public enum Priority : byte
{
    Low = 0,
    Medium = 1,
    High = 2
}

public enum RecurrenceType : byte
{
    None = 0,
    Daily = 1,
    Weekly = 2,
    Monthly = 3
}

public enum ScheduleStatus : byte
{
    Planned = 0,
    Done = 1,
    Skipped = 2,
    Cancelled = 3
}

public enum FinanceType : byte
{
    Expense = 0,
    Income = 1
}

public enum GoalStatus : byte
{
    Active = 0,
    Achieved = 1,
    Abandoned = 2
}

public enum HabitFrequency : byte
{
    Daily = 0,
    Weekly = 1
}

public enum ScopeKind : byte
{
    All = 0,
    Category = 1
}

public enum ModuleKey : byte
{
    Dashboard = 0,
    Todos = 1,
    Habits = 2,
    DailyEntries = 3,
    Journal = 4,
    Schedule = 5,
    Finance = 6,
    Goals = 7,
    Notes = 8,
    Settings = 9
}
