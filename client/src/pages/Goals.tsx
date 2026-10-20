import { useState } from 'react'
import { goalsApi } from '../api/resources'
import { useCategories } from '../context/CategoryContext'
import { useAsync } from '../hooks/useAsync'
import {
  Badge,
  CategoryDot,
  CategorySelect,
  EmptyState,
  ErrorBanner,
  Field,
  IconButton,
  Modal,
  PageHeader,
  Spinner,
  buttonPrimary,
  buttonSecondary,
  cardClass,
  inputClass,
} from '../components/ui'
import { Pencil, Plus, Target, Trash2, Trophy } from '../components/icons'
import { statusClasses } from '../lib/format'
import type { Goal, GoalStatus } from '../types'
import { formatDate } from '../lib/format'

function ProgressRing({ percent }: { percent: number }) {
  const clamped = Math.max(0, Math.min(100, percent))
  const r = 18
  const circumference = 2 * Math.PI * r
  const offset = circumference * (1 - clamped / 100)
  return (
    <div className="relative h-12 w-12 shrink-0">
      <svg viewBox="0 0 44 44" className="h-12 w-12 -rotate-90">
        <circle cx="22" cy="22" r={r} fill="none" stroke="#2b2e35" strokeWidth="4" />
        <circle
          cx="22"
          cy="22"
          r={r}
          fill="none"
          stroke="var(--color-accent)"
          strokeWidth="4"
          strokeLinecap="round"
          strokeDasharray={circumference}
          strokeDashoffset={offset}
        />
      </svg>
      <span className="absolute inset-0 flex items-center justify-center text-[10px] font-semibold text-gray-800">
        {clamped}%
      </span>
    </div>
  )
}

interface GoalForm {
  categoryId: number | null
  title: string
  description: string
  targetDate: string
  progressPercent: string
  status: GoalStatus
}

const emptyForm: GoalForm = {
  categoryId: null,
  title: '',
  description: '',
  targetDate: '',
  progressPercent: '0',
  status: 'Active',
}

export default function Goals() {
  const { categories, selectedCategoryId } = useCategories()
  const [statusFilter, setStatusFilter] = useState<string>('')
  const [editing, setEditing] = useState<Goal | null>(null)
  const [creating, setCreating] = useState(false)
  const [form, setForm] = useState<GoalForm>(emptyForm)
  const [saving, setSaving] = useState(false)
  const [actionError, setActionError] = useState<string | null>(null)

  const { data: goals, error, loading, reload } = useAsync(
    () =>
      goalsApi.getList({
        categoryId: selectedCategoryId ?? undefined,
        status: statusFilter || undefined,
      }),
    [selectedCategoryId, statusFilter],
  )

  function openCreate() {
    setForm({ ...emptyForm, categoryId: selectedCategoryId })
    setEditing(null)
    setCreating(true)
  }

  function openEdit(goal: Goal) {
    setForm({
      categoryId: goal.categoryId,
      title: goal.title,
      description: goal.description ?? '',
      targetDate: goal.targetDate ?? '',
      progressPercent: String(goal.progressPercent),
      status: goal.status,
    })
    setEditing(goal)
    setCreating(false)
  }

  async function save() {
    if (!form.title.trim() || !form.categoryId) return
    setSaving(true)
    setActionError(null)
    const body = {
      categoryId: form.categoryId,
      title: form.title,
      description: form.description || null,
      targetDate: form.targetDate || null,
      progressPercent: Number(form.progressPercent),
      status: form.status,
    }
    try {
      if (editing) {
        await goalsApi.update(editing.id, body)
      } else {
        await goalsApi.create(body)
      }
      setCreating(false)
      setEditing(null)
      reload()
    } catch (err) {
      setActionError((err as Error).message)
    } finally {
      setSaving(false)
    }
  }

  async function setStatus(goal: Goal, status: GoalStatus) {
    await goalsApi.update(goal.id, {
      categoryId: goal.categoryId,
      title: goal.title,
      description: goal.description,
      targetDate: goal.targetDate,
      progressPercent: goal.progressPercent,
      status,
    })
    reload()
  }

  async function remove(id: number) {
    if (!confirm('Delete this goal?')) return
    await goalsApi.remove(id)
    reload()
  }

  return (
    <div>
      <PageHeader
        title="Goals"
        actions={
          <>
            <select
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)}
              className={`${inputClass} w-auto`}
            >
              <option value="">All statuses</option>
              <option value="Active">Active</option>
              <option value="Achieved">Achieved</option>
              <option value="Abandoned">Abandoned</option>
            </select>
            <button onClick={openCreate} className={buttonPrimary}>
              <Plus size={15} /> New goal
            </button>
          </>
        }
      />
      {actionError && <ErrorBanner message={actionError} />}
      {error && <ErrorBanner message={error} />}
      {loading && <Spinner />}
      {!loading && (goals?.length ?? 0) === 0 && <EmptyState icon={Target} message="No goals yet." />}

      {goals?.map((goal) => (
        <div key={goal.id} className={`mb-3 ${cardClass} px-4 py-3`}>
          <div className="flex items-start justify-between gap-3">
            <div className="min-w-0 flex-1">
              <div className="flex flex-wrap items-center gap-2">
                <span className="text-sm font-medium text-gray-900">{goal.title}</span>
                <Badge text={goal.status} className={statusClasses(goal.status)} />
                <CategoryDot category={categories.find((c) => c.id === goal.categoryId)} />
                {goal.targetDate && (
                  <span className="text-xs text-gray-400">target {formatDate(goal.targetDate)}</span>
                )}
              </div>
              {goal.description && <p className="mt-1 text-sm text-gray-500">{goal.description}</p>}
            </div>
            <ProgressRing percent={goal.progressPercent} />
            <div className="flex shrink-0 items-center gap-0.5">
              {goal.status === 'Active' && (
                <IconButton icon={Trophy} label="Mark achieved" tone="success" onClick={() => setStatus(goal, 'Achieved')} />
              )}
              <IconButton icon={Pencil} label="Edit goal" onClick={() => openEdit(goal)} />
              <IconButton icon={Trash2} label="Delete goal" tone="danger" onClick={() => remove(goal.id)} />
            </div>
          </div>
        </div>
      ))}

      {(creating || editing) && (
        <Modal title={editing ? 'Edit goal' : 'New goal'} onClose={() => { setCreating(false); setEditing(null) }}>
          <Field label="Title">
            <input
              className={inputClass}
              value={form.title}
              onChange={(e) => setForm({ ...form, title: e.target.value })}
              autoFocus
            />
          </Field>
          <Field label="Category">
            <CategorySelect
              categories={categories}
              value={form.categoryId}
              onChange={(id) => setForm({ ...form, categoryId: id })}
              allowAll={false}
            />
          </Field>
          <div className="grid grid-cols-3 gap-3">
            <Field label="Target date">
              <input
                type="date"
                className={inputClass}
                value={form.targetDate}
                onChange={(e) => setForm({ ...form, targetDate: e.target.value })}
              />
            </Field>
            <Field label="Progress %">
              <input
                type="number"
                min={0}
                max={100}
                className={inputClass}
                value={form.progressPercent}
                onChange={(e) => setForm({ ...form, progressPercent: e.target.value })}
              />
            </Field>
            <Field label="Status">
              <select
                className={inputClass}
                value={form.status}
                onChange={(e) => setForm({ ...form, status: e.target.value as GoalStatus })}
              >
                <option>Active</option>
                <option>Achieved</option>
                <option>Abandoned</option>
              </select>
            </Field>
          </div>
          <Field label="Description">
            <textarea
              className={inputClass}
              rows={3}
              value={form.description}
              onChange={(e) => setForm({ ...form, description: e.target.value })}
            />
          </Field>
          <div className="flex justify-end gap-2">
            <button className={buttonSecondary} onClick={() => { setCreating(false); setEditing(null) }}>
              Cancel
            </button>
            <button className={buttonPrimary} onClick={save} disabled={saving || !form.title.trim() || !form.categoryId}>
              {saving ? 'Saving…' : 'Save'}
            </button>
          </div>
        </Modal>
      )}
    </div>
  )
}
