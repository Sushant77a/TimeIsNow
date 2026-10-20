import { useState } from 'react'
import { scheduleApi } from '../api/resources'
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
import { CalendarDays, Check, Pencil, Plus, SkipForward, Trash2 } from '../components/icons'
import { statusClasses } from '../lib/format'
import type { ScheduleItem, ScheduleStatus } from '../types'
import { formatDateTime, todayIso } from '../lib/format'

interface ScheduleForm {
  categoryId: number | null
  title: string
  notes: string
  scheduledAt: string
  endAt: string
  status: ScheduleStatus
}

const emptyForm: ScheduleForm = {
  categoryId: null,
  title: '',
  notes: '',
  scheduledAt: `${todayIso()}T09:00`,
  endAt: '',
  status: 'Planned',
}

export default function Schedule() {
  const { categories, selectedCategoryId } = useCategories()
  const [statusFilter, setStatusFilter] = useState<string>('')
  const [editing, setEditing] = useState<ScheduleItem | null>(null)
  const [creating, setCreating] = useState(false)
  const [form, setForm] = useState<ScheduleForm>(emptyForm)
  const [saving, setSaving] = useState(false)
  const [actionError, setActionError] = useState<string | null>(null)

  const { data: items, error, loading, reload } = useAsync(
    () =>
      scheduleApi.getList({
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

  function openEdit(item: ScheduleItem) {
    setForm({
      categoryId: item.categoryId,
      title: item.title,
      notes: item.notes ?? '',
      scheduledAt: item.scheduledAt.slice(0, 16),
      endAt: item.endAt ? item.endAt.slice(0, 16) : '',
      status: item.status,
    })
    setEditing(item)
    setCreating(false)
  }

  async function save() {
    if (!form.title.trim() || !form.categoryId) return
    setSaving(true)
    setActionError(null)
    const body = {
      categoryId: form.categoryId,
      title: form.title,
      notes: form.notes || null,
      scheduledAt: new Date(form.scheduledAt).toISOString(),
      endAt: form.endAt ? new Date(form.endAt).toISOString() : null,
      status: form.status,
    }
    try {
      if (editing) {
        await scheduleApi.update(editing.id, body)
      } else {
        await scheduleApi.create(body)
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

  async function setStatus(item: ScheduleItem, status: ScheduleStatus) {
    await scheduleApi.update(item.id, {
      categoryId: item.categoryId,
      title: item.title,
      notes: item.notes,
      scheduledAt: item.scheduledAt,
      endAt: item.endAt,
      status,
    })
    reload()
  }

  async function remove(id: number) {
    if (!confirm('Delete this schedule item?')) return
    await scheduleApi.remove(id)
    reload()
  }

  return (
    <div>
      <PageHeader
        title="Schedule"
        actions={
          <>
            <select
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)}
              className={`${inputClass} w-auto`}
            >
              <option value="">All statuses</option>
              <option value="Planned">Planned</option>
              <option value="Done">Done</option>
              <option value="Skipped">Skipped</option>
              <option value="Cancelled">Cancelled</option>
            </select>
            <button onClick={openCreate} className={buttonPrimary}>
              <Plus size={15} /> New item
            </button>
          </>
        }
      />
      {actionError && <ErrorBanner message={actionError} />}
      {error && <ErrorBanner message={error} />}
      {loading && <Spinner />}
      {!loading && (items?.length ?? 0) === 0 && <EmptyState icon={CalendarDays} message="Nothing scheduled." />}

      {items?.map((item) => (
        <div key={item.id} className={`mb-3 flex items-start gap-3 ${cardClass} px-4 py-3`}>
          <div className="w-36 shrink-0 pt-0.5">
            <p className="text-sm font-medium text-gray-900">{formatDateTime(item.scheduledAt)}</p>
            {item.endAt && <p className="text-xs text-gray-400">→ {formatDateTime(item.endAt)}</p>}
          </div>
          <div className="min-w-0 flex-1">
            <div className="flex flex-wrap items-center gap-2">
              <span className={`text-sm font-medium ${item.status === 'Cancelled' ? 'text-gray-400 line-through' : 'text-gray-900'}`}>
                {item.title}
              </span>
              <Badge text={item.status} className={statusClasses(item.status)} />
              <CategoryDot category={categories.find((c) => c.id === item.categoryId)} />
            </div>
            {item.notes && <p className="mt-1 text-sm text-gray-500">{item.notes}</p>}
          </div>
          <div className="flex shrink-0 items-center gap-0.5">
            {item.status === 'Planned' && (
              <>
                <IconButton icon={Check} label="Mark done" tone="success" onClick={() => setStatus(item, 'Done')} />
                <IconButton icon={SkipForward} label="Skip" tone="warning" onClick={() => setStatus(item, 'Skipped')} />
              </>
            )}
            <IconButton icon={Pencil} label="Edit item" onClick={() => openEdit(item)} />
            <IconButton icon={Trash2} label="Delete item" tone="danger" onClick={() => remove(item.id)} />
          </div>
        </div>
      ))}

      {(creating || editing) && (
        <Modal title={editing ? 'Edit item' : 'New schedule item'} onClose={() => { setCreating(false); setEditing(null) }}>
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
            <Field label="Starts at">
              <input
                type="datetime-local"
                className={inputClass}
                value={form.scheduledAt}
                onChange={(e) => setForm({ ...form, scheduledAt: e.target.value })}
              />
            </Field>
            <Field label="Ends at (optional)">
              <input
                type="datetime-local"
                className={inputClass}
                value={form.endAt}
                onChange={(e) => setForm({ ...form, endAt: e.target.value })}
              />
            </Field>
          </div>
          <Field label="Status">
            <select
              className={inputClass}
              value={form.status}
              onChange={(e) => setForm({ ...form, status: e.target.value as ScheduleStatus })}
            >
              <option>Planned</option>
              <option>Done</option>
              <option>Skipped</option>
              <option>Cancelled</option>
            </select>
          </Field>
          <Field label="Notes">
            <textarea
              className={inputClass}
              rows={2}
              value={form.notes}
              onChange={(e) => setForm({ ...form, notes: e.target.value })}
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
