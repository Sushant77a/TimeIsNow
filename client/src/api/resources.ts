import { api } from './client'
import type {
  AuthResponse,
  AuthUser,
  Category,
  DailyEntry,
  FinanceEntry,
  Goal,
  Habit,
  HabitLog,
  JournalEntry,
  ModuleSet,
  Note,
  Practice,
  PracticeLog,
  PracticePlanDraft,
  PracticeStatus,
  PracticeUpsert,
  Profile,
  ScheduleItem,
  SummaryResponse,
  Tag,
  Todo,
  TodayResponse,
  WeeklyBriefing,
} from '../types'

export const authApi = {
  register: (body: { email: string; password: string; displayName: string }) =>
    api.post<AuthResponse>('/api/auth/register', body),
  login: (body: { email: string; password: string }) =>
    api.post<AuthResponse>('/api/auth/login', body),
  logout: () => api.post<void>('/api/auth/logout'),
  me: () => api.get<AuthUser>('/api/auth/me'),
}

export const categoriesApi = {
  getAll: (includeArchived = false) =>
    api.get<Category[]>('/api/categories', { includeArchived }),
  create: (body: Partial<Category>) => api.post<Category>('/api/categories', body),
  update: (id: number, body: Partial<Category>) =>
    api.put<Category>(`/api/categories/${id}`, body),
  remove: (id: number) => api.delete(`/api/categories/${id}`),
}

export const modulesApi = {
  getAllScope: () => api.get<ModuleSet>('/api/modules/all'),
  updateAllScope: (modules: string[]) =>
    api.put<ModuleSet>('/api/modules/all', { modules }),
  updateCategoryScope: (categoryId: number, modules: string[]) =>
    api.put<ModuleSet>(`/api/modules/category/${categoryId}`, { modules }),
}

export const profileApi = {
  get: () => api.get<Profile>('/api/profile'),
  update: (body: {
    displayName: string
    currency: string
    weekStartsOn: number
    theme: string
  }) => api.put<Profile>('/api/profile', body),
}

export const todosApi = {
  getList: (params: { categoryId?: number; isDone?: boolean } = {}) =>
    api.get<Todo[]>('/api/todos', params),
  getById: (id: number) => api.get<Todo>(`/api/todos/${id}`),
  create: (body: Partial<Todo>) => api.post<Todo>('/api/todos', body),
  update: (id: number, body: Partial<Todo>) =>
    api.put<Todo>(`/api/todos/${id}`, body),
  remove: (id: number) => api.delete(`/api/todos/${id}`),
}

export const dailyEntriesApi = {
  getList: (params: { categoryId?: number; from?: string; to?: string } = {}) =>
    api.get<DailyEntry[]>('/api/daily-entries', params),
  create: (body: Partial<DailyEntry>) =>
    api.post<DailyEntry>('/api/daily-entries', body),
  update: (id: number, body: Partial<DailyEntry>) =>
    api.put<DailyEntry>(`/api/daily-entries/${id}`, body),
  remove: (id: number) => api.delete(`/api/daily-entries/${id}`),
}

export const journalApi = {
  getList: (params: { categoryId?: number; from?: string; to?: string; search?: string } = {}) =>
    api.get<JournalEntry[]>('/api/journal', params),
  getById: (id: number) => api.get<JournalEntry>(`/api/journal/${id}`),
  create: (body: Partial<JournalEntry>) => api.post<JournalEntry>('/api/journal', body),
  update: (id: number, body: Partial<JournalEntry>) =>
    api.put<JournalEntry>(`/api/journal/${id}`, body),
  remove: (id: number) => api.delete(`/api/journal/${id}`),
}

export const scheduleApi = {
  getList: (params: { categoryId?: number; from?: string; to?: string; status?: string } = {}) =>
    api.get<ScheduleItem[]>('/api/schedule', params),
  create: (body: Partial<ScheduleItem>) => api.post<ScheduleItem>('/api/schedule', body),
  update: (id: number, body: Partial<ScheduleItem>) =>
    api.put<ScheduleItem>(`/api/schedule/${id}`, body),
  remove: (id: number) => api.delete(`/api/schedule/${id}`),
}

export const financeApi = {
  getList: (params: { categoryId?: number; from?: string; to?: string; type?: string } = {}) =>
    api.get<FinanceEntry[]>('/api/finance', params),
  create: (body: Partial<FinanceEntry>) => api.post<FinanceEntry>('/api/finance', body),
  update: (id: number, body: Partial<FinanceEntry>) =>
    api.put<FinanceEntry>(`/api/finance/${id}`, body),
  remove: (id: number) => api.delete(`/api/finance/${id}`),
}

export const goalsApi = {
  getList: (params: { categoryId?: number; status?: string } = {}) =>
    api.get<Goal[]>('/api/goals', params),
  create: (body: Partial<Goal>) => api.post<Goal>('/api/goals', body),
  update: (id: number, body: Partial<Goal>) => api.put<Goal>(`/api/goals/${id}`, body),
  remove: (id: number) => api.delete(`/api/goals/${id}`),
}

export const habitsApi = {
  getList: (params: { categoryId?: number; active?: boolean } = {}) =>
    api.get<Habit[]>('/api/habits', params),
  getById: (id: number) => api.get<Habit>(`/api/habits/${id}`),
  create: (body: Partial<Habit>) => api.post<Habit>('/api/habits', body),
  update: (id: number, body: Partial<Habit>) => api.put<Habit>(`/api/habits/${id}`, body),
  remove: (id: number) => api.delete(`/api/habits/${id}`),
  getLogs: (habitId: number, params: { from?: string; to?: string } = {}) =>
    api.get<HabitLog[]>(`/api/habits/${habitId}/logs`, params),
  upsertLog: (habitId: number, body: { date: string; isCompleted: boolean; note?: string }) =>
    api.post<HabitLog>(`/api/habits/${habitId}/logs`, body),
  removeLog: (logId: number) => api.delete(`/api/habits/logs/${logId}`),
}

export const practicesApi = {
  getList: (params: { categoryId?: number; status?: PracticeStatus } = {}) =>
    api.get<Practice[]>('/api/practices', params),
  getById: (id: number) => api.get<Practice>(`/api/practices/${id}`),
  create: (body: PracticeUpsert) => api.post<Practice>('/api/practices', body),
  update: (id: number, body: PracticeUpsert) => api.put<Practice>(`/api/practices/${id}`, body),
  remove: (id: number) => api.delete(`/api/practices/${id}`),
  getLogs: (practiceId: number, params: { from?: string; to?: string } = {}) =>
    api.get<PracticeLog[]>(`/api/practices/${practiceId}/logs`, params),
  upsertLog: (
    practiceId: number,
    body: { date: string; sessionOrder: number; isCompleted: boolean; note?: string },
  ) => api.post<PracticeLog>(`/api/practices/${practiceId}/logs`, body),
  removeLog: (logId: number) => api.delete(`/api/practices/logs/${logId}`),
  generatePlan: (body: {
    prompt: string
    categoryId?: number
    startDate?: string
    sessionsPerDay?: number
    days?: number
  }) => api.post<PracticePlanDraft>('/api/practices/plan', body),
}

export const notesApi = {  getList: (params: { categoryId?: number; search?: string; tag?: string; pinned?: boolean } = {}) =>
    api.get<Note[]>('/api/notes', params),
  getById: (id: number) => api.get<Note>(`/api/notes/${id}`),
  create: (body: Partial<Note>) => api.post<Note>('/api/notes', body),
  update: (id: number, body: Partial<Note>) => api.put<Note>(`/api/notes/${id}`, body),
  remove: (id: number) => api.delete(`/api/notes/${id}`),
}

export const tagsApi = {
  getAll: () => api.get<Tag[]>('/api/tags'),
  remove: (id: number) => api.delete(`/api/tags/${id}`),
}

export const dashboardApi = {
  getToday: (date?: string) =>
    api.get<TodayResponse>('/api/dashboard/today', date ? { date } : undefined),
  getSummary: (year?: number, month?: number) =>
    api.get<SummaryResponse>('/api/dashboard/summary', { year, month }),
  getBriefing: (weekStart?: string) =>
    api.get<WeeklyBriefing>('/api/dashboard/briefing', weekStart ? { weekStart } : undefined),
}
