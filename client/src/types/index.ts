export type Priority = 'Low' | 'Medium' | 'High'
export type RecurrenceType = 'None' | 'Daily' | 'Weekly' | 'Monthly'
export type ScheduleStatus = 'Planned' | 'Done' | 'Skipped' | 'Cancelled'
export type FinanceType = 'Expense' | 'Income'
export type GoalStatus = 'Active' | 'Achieved' | 'Abandoned'
export type HabitFrequency = 'Daily' | 'Weekly'

export interface Category {
  id: number
  name: string
  description: string | null
  color: string
  icon: string | null
  sortOrder: number
  isArchived: boolean
  modules: string[]
  createdAt: string
  updatedAt: string
}

export interface ModuleSet {
  modules: string[]
}

export interface Profile {
  id: number
  displayName: string
  currency: string
  weekStartsOn: number
  theme: string
  createdAt: string
  updatedAt: string
}

export interface Todo {
  id: number
  categoryId: number
  title: string
  description: string | null
  priority: Priority
  dueDate: string | null
  reminderAt: string | null
  recurrenceType: RecurrenceType
  recurrenceInterval: number | null
  isDone: boolean
  completedAt: string | null
  parentTodoId: number | null
  tags: string[]
  subTasks: Todo[]
  createdAt: string
  updatedAt: string
}

export interface DailyEntry {
  id: number
  categoryId: number
  date: string
  content: string
  durationMinutes: number | null
  mood: number | null
  energy: number | null
  createdAt: string
  updatedAt: string
}

export interface JournalEntry {
  id: number
  categoryId: number
  date: string
  title: string | null
  content: string
  mood: number | null
  energy: number | null
  tags: string[]
  createdAt: string
  updatedAt: string
}

export interface ScheduleItem {
  id: number
  categoryId: number
  title: string
  notes: string | null
  scheduledAt: string
  endAt: string | null
  status: ScheduleStatus
  reminderAt: string | null
  goalId: number | null
  createdAt: string
  updatedAt: string
}

export interface FinanceEntry {
  id: number
  categoryId: number
  type: FinanceType
  amount: number
  date: string
  description: string | null
  recurrenceType: RecurrenceType
  createdAt: string
  updatedAt: string
}

export interface Goal {
  id: number
  categoryId: number
  title: string
  description: string | null
  targetDate: string | null
  status: GoalStatus
  progressPercent: number
  createdAt: string
  updatedAt: string
}

export interface Habit {
  id: number
  categoryId: number
  title: string
  description: string | null
  frequency: HabitFrequency
  isActive: boolean
  createdAt: string
  updatedAt: string
}

export interface HabitLog {
  id: number
  habitId: number
  date: string
  isCompleted: boolean
  note: string | null
  createdAt: string
}

export interface Note {
  id: number
  categoryId: number
  title: string
  content: string
  isPinned: boolean
  tags: string[]
  createdAt: string
  updatedAt: string
}

export interface Tag {
  id: number
  name: string
  createdAt: string
}

export interface TodayTodo {
  id: number
  categoryId: number
  categoryName: string
  title: string
  priority: Priority
  dueDate: string | null
}

export interface TodaySchedule {
  id: number
  categoryId: number
  categoryName: string
  title: string
  scheduledAt: string
  endAt: string | null
  status: ScheduleStatus
}

export interface TodayEntry {
  id: number
  categoryId: number
  categoryName: string
  content: string
  durationMinutes: number | null
}

export interface TodayHabit {
  habitId: number
  categoryId: number
  title: string
  frequency: HabitFrequency
  isLogged: boolean
}

export interface TodayResponse {
  date: string
  overdueTodos: TodayTodo[]
  dueTodayTodos: TodayTodo[]
  scheduleToday: TodaySchedule[]
  entriesToday: TodayEntry[]
  habitsToday: TodayHabit[]
}

export interface SummaryCategoryFinance {
  categoryId: number
  categoryName: string
  income: number
  expense: number
}

export interface SummaryCategoryCount {
  categoryId: number
  categoryName: string
  openTodoCount: number
}

export interface SummaryHabitStreak {
  habitId: number
  title: string
  categoryId: number
  currentStreak: number
}

export interface SummaryResponse {
  month: string
  income: number
  expense: number
  balance: number
  financeByCategory: SummaryCategoryFinance[]
  openTodosByCategory: SummaryCategoryCount[]
  activeHabits: SummaryHabitStreak[]
}

export interface BriefingStats {
  todosCompleted: number
  todosCreated: number
  todosOverdue: number
  habitLogsCompleted: number
  habitOpportunities: number
  bestStreak: number
  bestStreakHabit: string | null
  income: number
  expense: number
  balance: number
  topExpenseCategory: string | null
  topExpenseAmount: number
  journalEntries: number
  dailyEntries: number
  scheduleDone: number
  averageMood: number | null
}

export interface WeeklyBriefing {
  weekStart: string
  weekEnd: string
  stats: BriefingStats
  narrative: string
  narrativeSource: 'template' | 'ai'
  narrativeProvider: string | null
}

export interface ApiEnvelope<T> {
  message: string
  status: string
  statusCode: number
  data: T
}

export interface AuthUser {
  id: number
  email: string
  displayName: string
}

export interface AuthResponse {
  accessToken: string
  accessTokenExpiresAt: string
  user: AuthUser
}
