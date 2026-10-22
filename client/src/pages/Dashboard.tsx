import type { ReactNode } from 'react'
import { useNavigate } from 'react-router-dom'
import { dashboardApi, habitsApi, practicesApi } from '../api/resources'
import { useAsync } from '../hooks/useAsync'
import { Badge, EmptyState, ErrorBanner, PageHeader, Spinner, cardClass } from '../components/ui'
import {
  CalendarClock,
  Check,
  Flame,
  ListTodo,
  Scale,
  Sparkles,
  type LucideIcon,
} from '../components/icons'
import { formatDate, formatTime, monthKey, priorityClasses, statusClasses } from '../lib/format'
import type { SummaryResponse, TodayResponse, WeeklyBriefing } from '../types'

function Card({
  title,
  icon: Icon,
  children,
  action,
}: {
  title: string
  icon?: LucideIcon
  children: ReactNode
  action?: ReactNode
}) {
  return (
    <div className={`${cardClass} p-4`}>
      <div className="mb-3 flex items-center justify-between">
        <h2 className="flex items-center gap-2 text-sm font-semibold text-gray-700">
          {Icon && <Icon size={15} className="text-gray-500" />}
          {title}
        </h2>
        {action}
      </div>
      {children}
    </div>
  )
}

function StatStrip({
  today,
  summary,
}: {
  today: TodayResponse | null
  summary: SummaryResponse | null
}) {
  const habitsDone = today?.habitsToday.filter((h) => h.isLogged).length ?? 0
  const habitsTotal = today?.habitsToday.length ?? 0
  const openTodos = summary?.openTodosByCategory.reduce((sum, c) => sum + c.openTodoCount, 0) ?? 0
  const balance = summary?.balance ?? 0

  const stats: {
    label: string
    value: string
    sub: string
    accent: string
    icon: LucideIcon
  }[] = [
    {
      label: 'Balance',
      value: summary ? balance.toFixed(0) : '-',
      sub: summary?.month ?? '',
      accent: balance >= 0 ? 'text-emerald-600' : 'text-red-600',
      icon: Scale,
    },
    {
      label: 'Open todos',
      value: summary ? String(openTodos) : '-',
      sub: 'this month',
      accent: 'text-accent-400',
      icon: ListTodo,
    },
    {
      label: 'Habits today',
      value: `${habitsDone}/${habitsTotal}`,
      sub: 'logged',
      accent: 'text-amber-600',
      icon: Flame,
    },
    {
      label: 'Schedule',
      value: String(today?.scheduleToday.length ?? 0),
      sub: 'events today',
      accent: 'text-sky-700',
      icon: CalendarClock,
    },
  ]

  return (
    <div className="mb-6 grid grid-cols-2 gap-3 lg:grid-cols-4">
      {stats.map((stat) => (
        <div key={stat.label} className={`${cardClass} px-4 py-3`}>
          <div className="flex items-center justify-between">
            <p className="text-xs font-medium uppercase tracking-wide text-gray-500">{stat.label}</p>
            <stat.icon size={15} className="text-gray-500/70" />
          </div>
          <p className={`mt-1 text-2xl font-semibold tracking-tight ${stat.accent}`}>
            {stat.value}
          </p>
          <p className="text-[11px] text-gray-500">{stat.sub}</p>
        </div>
      ))}
    </div>
  )
}

function BriefingCard({ briefing }: { briefing: WeeklyBriefing | null }) {
  if (!briefing) return null

  const { stats } = briefing
  const hasMoney = stats.income !== 0 || stats.expense !== 0
  const chips: { label: string; value: string; accent?: string }[] = [
    { label: 'Todos closed', value: String(stats.todosCompleted) },
    {
      label: 'Overdue',
      value: String(stats.todosOverdue),
      accent: stats.todosOverdue > 0 ? 'text-red-600' : undefined,
    },
    { label: 'Habit check-ins', value: `${stats.habitLogsCompleted}/${stats.habitOpportunities}` },
    {
      label: 'Balance',
      value: hasMoney ? stats.balance.toFixed(0) : '-',
      accent: hasMoney ? (stats.balance >= 0 ? 'text-emerald-600' : 'text-red-600') : undefined,
    },
    { label: 'Writing', value: String(stats.journalEntries + stats.dailyEntries) },
    { label: 'Best streak', value: stats.bestStreak > 0 ? `${stats.bestStreak} days` : '-' },
  ]

  return (
    <div className="mb-6" data-testid="weekly-briefing">
      <Card
        title="Weekly briefing"
        icon={Sparkles}
        action={
          <Badge
            text={briefing.narrativeSource === 'ai' ? 'AI' : 'Template'}
            className={
              briefing.narrativeSource === 'ai'
                ? 'bg-accent text-accent-ink'
                : 'bg-gray-100 text-gray-600'
            }
          />
        }
      >
        <div className="mb-3 grid grid-cols-2 gap-2 sm:grid-cols-3 lg:grid-cols-6">
          {chips.map((chip) => (
            <div key={chip.label} className="rounded-lg border border-hairline bg-gray-100 px-3 py-2">
              <p className="text-[11px] font-medium uppercase tracking-wide text-gray-500">
                {chip.label}
              </p>
              <p className={`mt-0.5 text-lg font-semibold tracking-tight ${chip.accent ?? 'text-gray-800'}`}>
                {chip.value}
              </p>
            </div>
          ))}
        </div>
        <p className="text-sm leading-relaxed text-gray-700">{briefing.narrative}</p>
        <p className="mt-2 text-xs text-gray-500">
          {formatDate(briefing.weekStart)} to {formatDate(briefing.weekEnd)}
          {briefing.narrativeProvider && ` · ${briefing.narrativeProvider}`}
        </p>
      </Card>
    </div>
  )
}

export default function Dashboard() {
  const navigate = useNavigate()
  const { year, month } = monthKey()

  const { data: today, error: todayError, loading: todayLoading, reload } = useAsync(
    () => dashboardApi.getToday(),
    [],
  )
  const { data: summary, error: summaryError, loading: summaryLoading } = useAsync(
    () => dashboardApi.getSummary(year, month),
    [year, month],
  )
  const { data: briefing, error: briefingError } = useAsync(
    () => dashboardApi.getBriefing(),
    [],
  )

  async function toggleHabit(habitId: number, isLogged: boolean) {
    await habitsApi.upsertLog(habitId, {
      date: new Date().toISOString().slice(0, 10),
      isCompleted: !isLogged,
    })
    reload()
  }

  async function togglePractice(practiceId: number, sessionOrder: number, completed: boolean) {
    await practicesApi.upsertLog(practiceId, {
      date: new Date().toISOString().slice(0, 10),
      sessionOrder,
      isCompleted: !completed,
    })
    reload()
  }

  return (
    <div>
      <PageHeader
        title="Dashboard"
        subtitle={formatDate(new Date().toISOString())}
      />
      {todayError && <ErrorBanner message={todayError} />}
      {summaryError && <ErrorBanner message={summaryError} />}
      {briefingError && <ErrorBanner message={briefingError} />}
      {todayLoading && <Spinner />}

      <StatStrip today={today ?? null} summary={summary ?? null} />

      <BriefingCard briefing={briefing ?? null} />

      {today && (
        <div className="mb-6 grid grid-cols-1 gap-4 lg:grid-cols-2">
          <Card
            title={`Overdue (${today.overdueTodos.length})`}
            icon={ListTodo}
            action={
              <button
                onClick={() => navigate('/todos')}
                className="text-xs font-medium text-accent-400 hover:text-accent-300"
              >
                All todos
              </button>
            }
          >
            {today.overdueTodos.length === 0 ? (
              <p className="flex items-center gap-2 text-sm text-emerald-600/90">
                <Check size={15} /> Nothing overdue
              </p>
            ) : (
              today.overdueTodos.map((todo) => (
                <div key={todo.id} className="mb-2 flex items-center gap-2">
                  <Badge text={todo.priority} className={priorityClasses(todo.priority)} />
                  <span className="text-sm text-gray-800">{todo.title}</span>
                  <span className="text-xs text-red-500">{formatDate(todo.dueDate)}</span>
                </div>
              ))
            )}
          </Card>

          <Card title={`Due today (${today.dueTodayTodos.length})`} icon={ListTodo}>
            {today.dueTodayTodos.length === 0 ? (
              <p className="text-sm text-gray-400">Nothing due today.</p>
            ) : (
              today.dueTodayTodos.map((todo) => (
                <div key={todo.id} className="mb-2 flex items-center gap-2">
                  <Badge text={todo.priority} className={priorityClasses(todo.priority)} />
                  <span className="text-sm text-gray-800">{todo.title}</span>
                </div>
              ))
            )}
          </Card>

          <Card
            title={`Habits (${today.habitsToday.filter((h) => h.isLogged).length}/${today.habitsToday.length})`}
            icon={Flame}
          >
            {today.habitsToday.length === 0 ? (
              <p className="text-sm text-gray-400">No active habits.</p>
            ) : (
              today.habitsToday.map((habit) => (
                <label key={habit.habitId} className="mb-2 flex cursor-pointer items-center gap-2">
                  <input
                    type="checkbox"
                    checked={habit.isLogged}
                    onChange={() => toggleHabit(habit.habitId, habit.isLogged)}
                    className="h-4 w-4 rounded-sm border-gray-300"
                  />
                  <span className={`text-sm ${habit.isLogged ? 'text-gray-400 line-through' : 'text-gray-800'}`}>
                    {habit.title}
                  </span>
                  <Badge text={habit.frequency} />
                </label>
              ))
            )}
          </Card>

          <Card
            title={`Practices (${today.practicesToday.filter((p) => p.completedCount >= p.sessionsPerDay).length}/${today.practicesToday.length})`}
            icon={Sparkles}
            action={
              <button
                onClick={() => navigate('/practices')}
                className="text-xs font-medium text-accent-400 hover:text-accent-300"
              >
                All practices
              </button>
            }
          >
            {today.practicesToday.length === 0 ? (
              <p className="text-sm text-gray-400">No active practices.</p>
            ) : (
              today.practicesToday.map((practice) => (
                <div key={practice.practiceId} className="mb-2" data-testid="dashboard-practice">
                  <div className="flex items-center justify-between gap-2">
                    <span className="text-sm text-gray-800">{practice.title}</span>
                    <span className="text-xs text-gray-500">
                      {practice.completedCount}/{practice.sessionsPerDay} today
                    </span>
                  </div>
                  <div className="mt-1 flex gap-1.5">
                    {Array.from({ length: practice.sessionsPerDay }, (_, i) => i + 1).map((order) => {
                      const done = order <= practice.completedCount
                      return (
                        <button
                          key={order}
                          data-testid="dashboard-practice-session"
                          onClick={() => togglePractice(practice.practiceId, order, done)}
                          title={`Session ${order}`}
                          className={`flex h-5 w-5 items-center justify-center rounded-full text-[10px] font-semibold transition ${
                            done ? 'bg-emerald-500 text-white' : 'bg-gray-200 text-gray-500 hover:bg-gray-300'
                          }`}
                        >
                          {done ? <Check size={11} /> : order}
                        </button>
                      )
                    })}
                  </div>
                </div>
              ))
            )}
          </Card>

          <Card title={`Schedule (${today.scheduleToday.length})`} icon={CalendarClock}>
            {today.scheduleToday.length === 0 ? (
              <p className="text-sm text-gray-400">Nothing scheduled today.</p>
            ) : (
              today.scheduleToday.map((item) => (
                <div key={item.id} className="mb-2 flex items-center gap-2">
                  <span className="w-14 text-xs text-gray-400">{formatTime(item.scheduledAt)}</span>
                  <span className="text-sm text-gray-800">{item.title}</span>
                  <Badge text={item.status} className={statusClasses(item.status)} />
                </div>
              ))
            )}
          </Card>
        </div>
      )}

      {today && today.entriesToday.length > 0 && (
        <div className="mb-6">
          <Card title={`Logged today (${today.entriesToday.length})`}>
            {today.entriesToday.map((entry) => (
              <div key={entry.id} className="mb-2 flex items-baseline gap-2">
                <span className="text-xs font-medium text-gray-500">{entry.categoryName}</span>
                <span className="text-sm text-gray-800">{entry.content}</span>
                {entry.durationMinutes && (
                  <span className="text-xs text-gray-400">({entry.durationMinutes} min)</span>
                )}
              </div>
            ))}
          </Card>
        </div>
      )}

      {summaryLoading && <Spinner />}
      {summary && (
        <div className="grid grid-cols-1 gap-4 lg:grid-cols-3">
          <Card title={`This month (${summary.month})`} icon={Scale}>
            <div className="space-y-2">
              <div className="flex justify-between text-sm">
                <span className="text-gray-500">Income</span>
                <span className="font-medium text-emerald-600">+{summary.income.toFixed(2)}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-gray-500">Expense</span>
                <span className="font-medium text-red-600">−{summary.expense.toFixed(2)}</span>
              </div>
              <div className="flex justify-between border-t border-gray-100 pt-2 text-sm">
                <span className="font-medium text-gray-700">Balance</span>
                <span className={`font-semibold ${summary.balance >= 0 ? 'text-emerald-600' : 'text-red-600'}`}>
                  {summary.balance.toFixed(2)}
                </span>
              </div>
            </div>
            {summary.financeByCategory.length > 0 && (
              <div className="mt-3 border-t border-gray-100 pt-3">
                {summary.financeByCategory.map((cat) => (
                  <div key={cat.categoryId} className="mb-1 flex justify-between text-xs">
                    <span className="text-gray-500">{cat.categoryName}</span>
                    <span className="text-gray-700">
                      {cat.income > 0 && <span className="text-emerald-600">+{cat.income.toFixed(0)} </span>}
                      {cat.expense > 0 && <span className="text-red-600">−{cat.expense.toFixed(0)}</span>}
                    </span>
                  </div>
                ))}
              </div>
            )}
          </Card>

          <Card title="Open todos" icon={ListTodo}>
            {summary.openTodosByCategory.length === 0 ? (
              <p className="text-sm text-gray-400">All clear.</p>
            ) : (
              summary.openTodosByCategory.map((cat) => (
                <div key={cat.categoryId} className="mb-1 flex justify-between text-sm">
                  <span className="text-gray-600">{cat.categoryName}</span>
                  <span className="font-medium text-gray-800">{cat.openTodoCount}</span>
                </div>
              ))
            )}
          </Card>

          <Card title="Habit streaks" icon={Flame}>
            {summary.activeHabits.length === 0 ? (
              <p className="text-sm text-gray-400">No daily habits yet.</p>
            ) : (
              summary.activeHabits.map((habit) => (
                <div key={habit.habitId} className="mb-1 flex justify-between text-sm">
                  <span className="text-gray-600">{habit.title}</span>
                  {habit.currentStreak > 0 ? (
                    <span className="flex items-center gap-1 font-medium text-amber-600">
                      <Flame size={14} className="text-amber-500" />
                      {habit.currentStreak}
                    </span>
                  ) : (
                    <span className="text-gray-500">-</span>
                  )}
                </div>
              ))
            )}
          </Card>
        </div>
      )}

      {!todayLoading && today && today.overdueTodos.length === 0 && today.dueTodayTodos.length === 0 &&
        today.habitsToday.length === 0 && today.scheduleToday.length === 0 && (
          <EmptyState message="Your day is wide open. Add a todo, log a habit, or write in your journal." />
        )}
    </div>
  )
}
