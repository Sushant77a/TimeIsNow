import { useState } from 'react'
import { habitsApi } from '../api/resources'
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
import { Check, Pencil, Plus, Repeat, Trash2 } from '../components/icons'
import type { Habit, HabitFrequency } from '../types'
import { todayIso } from '../lib/format'

function lastNDates(n: number): string[] {
  const dates: string[] = []
  const now = new Date()
  for (let i = n - 1; i >= 0; i--) {
    const d = new Date(now)
    d.setDate(now.getDate() - i)
    dates.push(d.toISOString().slice(0, 10))
  }
  return dates
}

interface HabitForm {
  categoryId: number | null
  title: string
  description: string
  frequency: HabitFrequency
  isActive: boolean
}

const emptyForm: HabitForm = {
  categoryId: null,
  title: '',
  description: '',
  frequency: 'Daily',
  isActive: true,
}

export default function Habits() {
  const { categories, selectedCategoryId } = useCategories()
  const [active, setActive] = useState<boolean | undefined>(true)
  const [editing, setEditing] = useState<Habit | null>(null)
  const [creating, setCreating] = useState(false)
  const [form, setForm] = useState<HabitForm>(emptyForm)
  const [saving, setSaving] = useState(false)
  const [actionError, setActionError] = useState<string | null>(null)

  const week = lastNDates(7)

  const { data: habits, error, loading, reload } = useAsync(
    async () => {
      const list = await habitsApi.getList({
        categoryId: selectedCategoryId ?? undefined,
        active,
      })
      const logs = await Promise.all(
        list.map((habit) =>
          habitsApi.getLogs(habit.id, { from: week[0], to: week[6] }).catch(() => []),
        ),
      )
      return list.map((habit, i) => ({
        habit,
        loggedDates: new Set(logs[i].filter((log) => log.isCompleted).map((log) => log.date)),
      }))
    },
    [selectedCategoryId, active],
  )

  function openCreate() {
    setForm({ ...emptyForm, categoryId: selectedCategoryId })
    setEditing(null)
    setCreating(true)
  }

  function openEdit(habit: Habit) {
    setForm({
      categoryId: habit.categoryId,
      title: habit.title,
      description: habit.description ?? '',
      frequency: habit.frequency,
      isActive: habit.isActive,
    })
    setEditing(habit)
    setCreating(false)
  }

  async function save() {
    if (!form.title.trim() || !form.categoryId) return
    setSaving(true)
    setActionError(null)
    try {
      const body = { ...form, categoryId: form.categoryId ?? undefined }
      if (editing) {
        await habitsApi.update(editing.id, body)
      } else {
        await habitsApi.create(body)
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

  async function toggleToday(habit: Habit, logged: boolean) {
    await habitsApi.upsertLog(habit.id, { date: todayIso(), isCompleted: !logged })
    reload()
  }

  async function remove(id: number) {
    if (!confirm('Delete this habit and all its logs?')) return
    await habitsApi.remove(id)
    reload()
  }

  return (
    <div>
      <PageHeader
        title="Habits"
        actions={
          <>
            <select
              value={active === undefined ? 'all' : active ? 'active' : 'inactive'}
              onChange={(e) =>
                setActive(e.target.value === 'all' ? undefined : e.target.value === 'active')
              }
              className={`${inputClass} w-auto`}
            >
              <option value="active">Active</option>
              <option value="inactive">Inactive</option>
              <option value="all">All</option>
            </select>
            <button onClick={openCreate} className={buttonPrimary}>
              <Plus size={15} /> New habit
            </button>
          </>
        }
      />
      {actionError && <ErrorBanner message={actionError} />}
      {error && <ErrorBanner message={error} />}
      {loading && <Spinner />}
      {!loading && (habits?.length ?? 0) === 0 && <EmptyState icon={Repeat} message="No habits yet." />}

      {habits?.map(({ habit, loggedDates }) => (
        <div key={habit.id} className={`mb-3 ${cardClass} px-4 py-3`}>
          <div className="flex items-start justify-between gap-3">
            <div className="min-w-0">
              <div className="flex flex-wrap items-center gap-2">
                <span className="text-sm font-medium text-gray-900">{habit.title}</span>
                <Badge text={habit.frequency} />
                {!habit.isActive && <Badge text="Inactive" className="bg-gray-100 text-gray-500" />}
                <CategoryDot category={categories.find((c) => c.id === habit.categoryId)} />
              </div>
              {habit.description && <p className="mt-1 text-sm text-gray-500">{habit.description}</p>}
            </div>
            <div className="flex shrink-0 items-center gap-1">
              <button
                onClick={() => toggleToday(habit, loggedDates.has(todayIso()))}
                className={loggedDates.has(todayIso()) ? buttonSecondary : buttonPrimary}
              >
                {loggedDates.has(todayIso()) ? (
                  <>
                    <Check size={15} /> Done today
                  </>
                ) : (
                  'Log today'
                )}
              </button>
              <IconButton icon={Pencil} label="Edit habit" onClick={() => openEdit(habit)} />
              <IconButton icon={Trash2} label="Delete habit" tone="danger" onClick={() => remove(habit.id)} />
            </div>
          </div>
          <div className="mt-3 flex gap-1.5">
            {week.map((date) => (
              <div key={date} className="flex flex-col items-center">
                <span className="text-[10px] text-gray-400">
                  {new Date(date + 'T00:00:00').toLocaleDateString(undefined, { weekday: 'narrow' })}
                </span>
                <span
                  className={`mt-0.5 h-4 w-4 rounded-full ${
                    loggedDates.has(date) ? 'bg-emerald-500' : 'bg-gray-200'
                  }`}
                  title={date}
                />
              </div>
            ))}
          </div>
        </div>
      ))}

      {(creating || editing) && (
        <Modal title={editing ? 'Edit habit' : 'New habit'} onClose={() => { setCreating(false); setEditing(null) }}>
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
          <div className="grid grid-cols-2 gap-3">
            <Field label="Frequency">
              <select
                className={inputClass}
                value={form.frequency}
                onChange={(e) => setForm({ ...form, frequency: e.target.value as HabitFrequency })}
              >
                <option>Daily</option>
                <option>Weekly</option>
              </select>
            </Field>
            <Field label="Active">
              <select
                className={inputClass}
                value={form.isActive ? 'yes' : 'no'}
                onChange={(e) => setForm({ ...form, isActive: e.target.value === 'yes' })}
              >
                <option value="yes">Yes</option>
                <option value="no">No</option>
              </select>
            </Field>
          </div>
          <Field label="Description">
            <textarea
              className={inputClass}
              rows={2}
              value={form.description}
              onChange={(e) => setForm({ ...form, description: e.target.value })}
            />
          </Field>
          <div className="mt-4 flex justify-end gap-2">
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
