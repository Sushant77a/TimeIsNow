import { cloneElement, isValidElement, useEffect, useId, type ReactElement, type ReactNode } from 'react'
import { Annoyed, Frown, Laugh, Meh, Smile, X, Zap, type LucideIcon } from 'lucide-react'
import type { Category } from '../types'

export function Spinner({ label }: { label?: string }) {
  return (
    <div className="flex flex-col items-center justify-center gap-3 py-12">
      <div className="h-6 w-6 animate-spin rounded-full border-2 border-accent-400/30 border-t-accent-400 shadow-e1" />
      {label && <p className="text-xs text-gray-500">{label}</p>}
    </div>
  )
}

export function ErrorBanner({ message }: { message: string }) {
  return (
    <div
      role="alert"
      className="mb-4 flex items-start gap-2 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700"
    >
      <span className="mt-1.5 h-1.5 w-1.5 shrink-0 rounded-full bg-red-500" />
      {message}
    </div>
  )
}

export function EmptyState({ message, icon: Icon }: { message: string; icon?: LucideIcon }) {
  return (
    <div className="flex flex-col items-center justify-center gap-3 py-14 text-center">
      {Icon && (
        <span className="flex h-11 w-11 items-center justify-center rounded-full border border-hairline bg-hairline text-gray-500">
          <Icon size={20} strokeWidth={1.75} />
        </span>
      )}
      <p className="text-sm text-gray-400">{message}</p>
    </div>
  )
}

export function PageHeader({
  title,
  subtitle,
  actions,
}: {
  title: string
  subtitle?: string
  actions?: ReactNode
}) {
  return (
    <div className="mb-6 flex flex-wrap items-end justify-between gap-3">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight text-gray-900">{title}</h1>
        {subtitle && <p className="mt-0.5 text-sm text-gray-500">{subtitle}</p>}
      </div>
      {actions && <div className="flex flex-wrap items-center gap-2">{actions}</div>}
    </div>
  )
}

export function Badge({ text, className }: { text: string; className?: string }) {
  return (
    <span
      className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${className ?? 'bg-gray-100 text-gray-600'}`}
    >
      {text}
    </span>
  )
}

export function CategoryDot({ category }: { category: Category | undefined }) {
  if (!category) return null
  return (
    <span className="inline-flex items-center gap-1.5 text-xs text-gray-500">
      <span
        className="h-2.5 w-2.5 rounded-full "
        style={{ backgroundColor: category.color, color: category.color }}
      />
      {category.name}
    </span>
  )
}

export function TagPills({ tags }: { tags: string[] }) {
  if (tags.length === 0) return null
  return (
    <span className="flex flex-wrap gap-1">
      {tags.map((tag) => (
        <span
          key={tag}
          className="rounded-sm bg-accent-50 px-1.5 py-0.5 text-xs text-accent-700 ring-1 ring-inset ring-accent-100"
        >
          #{tag}
        </span>
      ))}
    </span>
  )
}

export function Modal({
  title,
  onClose,
  children,
}: {
  title: string
  onClose: () => void
  children: ReactNode
}) {
  useEffect(() => {
    function onKey(event: KeyboardEvent) {
      if (event.key === 'Escape') onClose()
    }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [onClose])

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4 backdrop-blur-sm"
      onClick={onClose}
    >
      <div
        className="tin-pop max-h-[90vh] w-full max-w-lg overflow-y-auto rounded-2xl border border-hairline-strong bg-surface p-6 shadow-e3"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="mb-4 flex items-center justify-between gap-4">
          <h2 className="text-lg font-semibold text-gray-900">{title}</h2>
          <button
            onClick={onClose}
            className="rounded-lg p-1.5 text-gray-400 transition hover:bg-hairline hover:text-gray-200"
            aria-label="Close"
          >
            <X size={18} />
          </button>
        </div>
        {children}
      </div>
    </div>
  )
}

export const cardClass =
  'rounded-xl border border-hairline bg-surface shadow-e1'

export const inputClass =
  'w-full rounded-lg border border-gray-300 bg-gray-50 px-3 py-2 text-sm text-gray-900 placeholder:text-gray-400 transition focus:border-accent-500 focus:outline-none focus:ring-1 focus:ring-accent-500'

export const labelClass = 'mb-1 block text-xs font-medium text-gray-600'

export const buttonPrimary =
  'inline-flex items-center gap-1.5 rounded-lg bg-accent px-3 py-1.5 text-sm font-medium text-accent-ink shadow-e1 transition hover:bg-accent-600 hover:shadow-e2 active:scale-[0.97] disabled:opacity-50 disabled:shadow-none'

export const buttonSecondary =
  'inline-flex items-center gap-1.5 rounded-lg border border-gray-300 px-3 py-1.5 text-sm font-medium text-gray-700 transition hover:border-gray-200 hover:bg-gray-100 hover:text-gray-900 active:scale-[0.97]'

export const buttonDanger =
  'inline-flex items-center gap-1.5 rounded-lg border border-red-200 px-3 py-1.5 text-sm font-medium text-red-600 transition hover:bg-red-50 active:scale-[0.97]'

export const buttonGhost =
  'inline-flex items-center gap-1.5 rounded-lg px-3 py-1.5 text-sm font-medium text-gray-500 transition hover:bg-hairline hover:text-gray-800 active:scale-[0.97]'

export function IconButton({
  icon: Icon,
  label,
  onClick,
  tone = 'default',
}: {
  icon: LucideIcon
  label: string
  onClick?: () => void
  tone?: 'default' | 'danger' | 'success' | 'warning'
}) {
  const tones = {
    default: 'text-gray-400 hover:bg-hairline hover:text-gray-200',
    danger: 'text-gray-400 hover:bg-red-500/10 hover:text-red-500',
    success: 'text-gray-400 hover:bg-emerald-600/10 hover:text-emerald-600',
    warning: 'text-gray-400 hover:bg-amber-600/10 hover:text-amber-600',
  }
  return (
    <button
      type="button"
      onClick={onClick}
      title={label}
      aria-label={label}
      className={`rounded-lg p-1.5 transition active:scale-[0.97] ${tones[tone]}`}
    >
      <Icon size={16} />
    </button>
  )
}

const moodOptions: { value: number; Icon: LucideIcon; label: string }[] = [
  { value: 1, Icon: Frown, label: 'Awful' },
  { value: 2, Icon: Annoyed, label: 'Bad' },
  { value: 3, Icon: Meh, label: 'Okay' },
  { value: 4, Icon: Smile, label: 'Good' },
  { value: 5, Icon: Laugh, label: 'Great' },
]

export const moodFaces: LucideIcon[] = moodOptions.map((o) => o.Icon)

export function MoodPicker({
  value,
  onChange,
}: {
  value: number | null
  onChange: (value: number | null) => void
}) {
  return (
    <div role="group" aria-label="Mood" className="flex items-center gap-1.5">
      {moodOptions.map((option) => {
        const active = value === option.value
        return (
          <button
            key={option.value}
            type="button"
            title={`${option.label} (${option.value}/5)`}
            onClick={() => onChange(active ? null : option.value)}
            className={`flex h-9 w-9 items-center justify-center rounded-lg border transition ${
              active
                ? 'border-accent-400/50 bg-accent-500/15 text-accent-300 shadow-e1'
                : 'border-hairline-strong text-gray-500 hover:border-hairline-strong hover:text-gray-300'
            }`}
          >
            <option.Icon size={18} />
          </button>
        )
      })}
    </div>
  )
}

export function EnergyPicker({
  value,
  onChange,
}: {
  value: number | null
  onChange: (value: number | null) => void
}) {
  return (
    <div role="group" aria-label="Energy" className="flex items-center gap-1">
      {[1, 2, 3, 4, 5].map((n) => {
        const filled = value !== null && n <= value
        return (
          <button
            key={n}
            type="button"
            title={`${n}/5`}
            onClick={() => onChange(value === n ? null : n)}
            className={`rounded-lg p-1.5 transition ${
              filled ? 'text-amber-400' : 'text-gray-600 hover:text-gray-400'
            }`}
          >
            <Zap size={17} className={filled ? 'fill-amber-400/90' : ''} />
          </button>
        )
      })}
    </div>
  )
}

export function CategorySelect({
  id,
  categories,
  value,
  onChange,
  allowAll = true,
}: {
  id?: string
  categories: Category[]
  value: number | null
  onChange: (value: number | null) => void
  allowAll?: boolean
}) {
  return (
    <select
      id={id}
      value={value ?? ''}
      onChange={(e) => onChange(e.target.value === '' ? null : Number(e.target.value))}
      className={`${inputClass} w-auto`}
    >
      {allowAll ? (
        <option value="">All categories</option>
      ) : (
        <option value="" disabled>
          Select category…
        </option>
      )}
      {categories.map((category) => (
        <option key={category.id} value={category.id}>
          {category.name}
        </option>
      ))}
    </select>
  )
}

export function TagInput({
  id,
  value,
  onChange,
}: {
  id?: string
  value: string[]
  onChange: (tags: string[]) => void
}) {
  return (
    <input
      id={id}
      type="text"
      value={value.join(', ')}
      onChange={(e) =>
        onChange(
          e.target.value
            .split(',')
            .map((tag) => tag.trim())
            .filter(Boolean),
        )
      }
      placeholder="tags (comma-separated)"
      className={inputClass}
    />
  )
}

export function Field({ label, children }: { label: string; children: ReactNode }) {
  const id = useId()
  const element = isValidElement(children) ? (children as ReactElement<{ id?: string }>) : null
  const control = element && !element.props.id ? cloneElement(element, { id }) : children

  return (
    <div className="mb-3">
      <label className={labelClass} htmlFor={id}>
        {label}
      </label>
      {control}
    </div>
  )
}
