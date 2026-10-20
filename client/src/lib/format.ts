export function formatDate(value: string | null | undefined): string {
  if (!value) return '-'
  const date = new Date(value)
  return date.toLocaleDateString(undefined, { day: 'numeric', month: 'short', year: 'numeric' })
}

export function formatDateTime(value: string | null | undefined): string {
  if (!value) return '-'
  const date = new Date(value)
  return date.toLocaleString(undefined, {
    day: 'numeric',
    month: 'short',
    hour: '2-digit',
    minute: '2-digit',
  })
}

export function formatTime(value: string | null | undefined): string {
  if (!value) return '-'
  const date = new Date(value)
  return date.toLocaleTimeString(undefined, { hour: '2-digit', minute: '2-digit' })
}

export function todayIso(): string {
  return new Date().toISOString().slice(0, 10)
}

export function monthKey(): { year: number; month: number } {
  const now = new Date()
  return { year: now.getFullYear(), month: now.getMonth() + 1 }
}

export const moodLabel = (mood: number | null | undefined): string =>
  mood === null || mood === undefined ? '' : (['Awful', 'Bad', 'Okay', 'Good', 'Great'][mood - 1] ?? '')

export function priorityClasses(priority: string): string {
  switch (priority) {
    case 'High':
      return 'bg-red-100 text-red-700'
    case 'Medium':
      return 'bg-amber-100 text-amber-700'
    default:
      return 'bg-sky-100 text-sky-700'
  }
}

export function statusClasses(status: string): string {
  switch (status) {
    case 'Done':
      return 'bg-emerald-100 text-emerald-700'
    case 'Planned':
      return 'bg-sky-100 text-sky-700'
    case 'Skipped':
      return 'bg-amber-100 text-amber-700'
    case 'Cancelled':
      return 'bg-gray-200 text-gray-600'
    case 'Achieved':
      return 'bg-emerald-100 text-emerald-700'
    case 'Abandoned':
      return 'bg-gray-200 text-gray-600'
    default:
      return 'bg-sky-100 text-sky-700'
  }
}
