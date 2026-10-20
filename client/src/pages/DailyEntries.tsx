import { useState } from 'react'
import { dailyEntriesApi } from '../api/resources'
import { useCategories } from '../context/CategoryContext'
import { useAsync } from '../hooks/useAsync'
import {
  CategoryDot,
  CategorySelect,
  EmptyState,
  EnergyPicker,
  ErrorBanner,
  Field,
  IconButton,
  Modal,
  MoodPicker,
  PageHeader,
  Spinner,
  buttonPrimary,
  buttonSecondary,
  cardClass,
  inputClass,
  moodFaces,
} from '../components/ui'
import { NotebookPen, Pencil, Plus, Trash2, Zap } from '../components/icons'
import type { DailyEntry } from '../types'
import { formatDate, moodLabel, todayIso } from '../lib/format'

interface EntryForm {
  categoryId: number | null
  date: string
  content: string
  durationMinutes: string
  mood: string
  energy: string
}

const emptyForm: EntryForm = {
  categoryId: null,
  date: todayIso(),
  content: '',
  durationMinutes: '',
  mood: '',
  energy: '',
}

export default function DailyEntries() {
  const { categories, selectedCategoryId } = useCategories()
  const [editing, setEditing] = useState<DailyEntry | null>(null)
  const [creating, setCreating] = useState(false)
  const [form, setForm] = useState<EntryForm>(emptyForm)
  const [saving, setSaving] = useState(false)
  const [actionError, setActionError] = useState<string | null>(null)

  const { data: entries, error, loading, reload } = useAsync(
    () => dailyEntriesApi.getList({ categoryId: selectedCategoryId ?? undefined }),
    [selectedCategoryId],
  )

  function openCreate() {
    setForm({ ...emptyForm, categoryId: selectedCategoryId })
    setEditing(null)
    setCreating(true)
  }

  function openEdit(entry: DailyEntry) {
    setForm({
      categoryId: entry.categoryId,
      date: entry.date.slice(0, 10),
      content: entry.content,
      durationMinutes: entry.durationMinutes ? String(entry.durationMinutes) : '',
      mood: entry.mood ? String(entry.mood) : '',
      energy: entry.energy ? String(entry.energy) : '',
    })
    setEditing(entry)
    setCreating(false)
  }

  async function save() {
    if (!form.content.trim() || !form.categoryId) return
    setSaving(true)
    setActionError(null)
    const body = {
      categoryId: form.categoryId,
      date: form.date,
      content: form.content,
      durationMinutes: form.durationMinutes ? Number(form.durationMinutes) : null,
      mood: form.mood ? Number(form.mood) : null,
      energy: form.energy ? Number(form.energy) : null,
    }
    try {
      if (editing) {
        await dailyEntriesApi.update(editing.id, body)
      } else {
        await dailyEntriesApi.create(body)
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

  async function remove(id: number) {
    if (!confirm('Delete this entry?')) return
    await dailyEntriesApi.remove(id)
    reload()
  }

  return (
    <div>
      <PageHeader
        title="Daily Entries"
        actions={
          <button onClick={openCreate} className={buttonPrimary}>
            <Plus size={15} /> New entry
          </button>
        }
      />
      {actionError && <ErrorBanner message={actionError} />}
      {error && <ErrorBanner message={error} />}
      {loading && <Spinner />}
      {!loading && (entries?.length ?? 0) === 0 && <EmptyState icon={NotebookPen} message="No entries yet." />}

      {entries?.map((entry) => {
        const MoodFace = entry.mood ? moodFaces[entry.mood - 1] : null
        return (
          <div key={entry.id} className={`mb-3 flex items-start gap-3 ${cardClass} px-4 py-3`}>
            <div className="w-28 shrink-0 pt-0.5">
              <p className="text-sm font-medium text-gray-900">{formatDate(entry.date)}</p>
              {entry.durationMinutes && (
                <p className="text-xs text-gray-400">{entry.durationMinutes} min</p>
              )}
            </div>
            <div className="min-w-0 flex-1">
              <p className="whitespace-pre-wrap text-sm text-gray-700">{entry.content}</p>
              <div className="mt-1.5 flex items-center gap-3">
                <CategoryDot category={categories.find((c) => c.id === entry.categoryId)} />
                {MoodFace && (
                  <span className="flex items-center gap-1 text-xs text-gray-500" title={`Mood ${entry.mood}/5`}>
                    <MoodFace size={14} /> {moodLabel(entry.mood)}
                  </span>
                )}
                {entry.energy && (
                  <span className="flex items-center gap-1 text-xs text-gray-500" title={`Energy ${entry.energy}/5`}>
                    <Zap size={14} className="text-amber-500" /> {entry.energy}/5
                  </span>
                )}
              </div>
            </div>
            <div className="flex shrink-0 gap-0.5">
              <IconButton icon={Pencil} label="Edit entry" onClick={() => openEdit(entry)} />
              <IconButton icon={Trash2} label="Delete entry" tone="danger" onClick={() => remove(entry.id)} />
            </div>
          </div>
        )
      })}

      {(creating || editing) && (
        <Modal title={editing ? 'Edit entry' : 'New daily entry'} onClose={() => { setCreating(false); setEditing(null) }}>
          <div className="grid grid-cols-2 gap-3">
            <Field label="Date">
              <input
                type="date"
                className={inputClass}
                value={form.date}
                onChange={(e) => setForm({ ...form, date: e.target.value })}
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
          </div>
          <Field label="What did you do?">
            <textarea
              className={inputClass}
              rows={4}
              value={form.content}
              onChange={(e) => setForm({ ...form, content: e.target.value })}
              autoFocus
            />
          </Field>
          <Field label="Minutes">
            <input
              type="number"
              min={1}
              max={1440}
              className={`${inputClass} w-32`}
              value={form.durationMinutes}
              onChange={(e) => setForm({ ...form, durationMinutes: e.target.value })}
            />
          </Field>
          <Field label="Mood">
            <MoodPicker
              value={form.mood ? Number(form.mood) : null}
              onChange={(value) => setForm({ ...form, mood: value ? String(value) : '' })}
            />
          </Field>
          <Field label="Energy">
            <EnergyPicker
              value={form.energy ? Number(form.energy) : null}
              onChange={(value) => setForm({ ...form, energy: value ? String(value) : '' })}
            />
          </Field>
          <div className="flex justify-end gap-2">
            <button className={buttonSecondary} onClick={() => { setCreating(false); setEditing(null) }}>
              Cancel
            </button>
            <button className={buttonPrimary} onClick={save} disabled={saving || !form.content.trim() || !form.categoryId}>
              {saving ? 'Saving…' : 'Save'}
            </button>
          </div>
        </Modal>
      )}
    </div>
  )
}
