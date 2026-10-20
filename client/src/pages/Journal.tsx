import { useState } from 'react'
import { journalApi } from '../api/resources'
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
  TagInput,
  TagPills,
  buttonPrimary,
  buttonSecondary,
  cardClass,
  inputClass,
  moodFaces,
} from '../components/ui'
import { BookOpen, Pencil, Plus, Search, Trash2, Zap } from '../components/icons'
import type { JournalEntry } from '../types'
import { formatDate, moodLabel, todayIso } from '../lib/format'

interface JournalForm {
  categoryId: number | null
  date: string
  title: string
  content: string
  mood: string
  energy: string
  tags: string
}

const emptyForm: JournalForm = {
  categoryId: null,
  date: todayIso(),
  title: '',
  content: '',
  mood: '',
  energy: '',
  tags: '',
}

export default function Journal() {
  const { categories, selectedCategoryId } = useCategories()
  const [search, setSearch] = useState('')
  const [editing, setEditing] = useState<JournalEntry | null>(null)
  const [creating, setCreating] = useState(false)
  const [form, setForm] = useState<JournalForm>(emptyForm)
  const [saving, setSaving] = useState(false)
  const [actionError, setActionError] = useState<string | null>(null)

  const { data: entries, error, loading, reload } = useAsync(
    () =>
      journalApi.getList({
        categoryId: selectedCategoryId ?? undefined,
        search: search || undefined,
      }),
    [selectedCategoryId, search],
  )

  function openCreate() {
    setForm({ ...emptyForm, categoryId: selectedCategoryId })
    setEditing(null)
    setCreating(true)
  }

  function openEdit(entry: JournalEntry) {
    setForm({
      categoryId: entry.categoryId,
      date: entry.date.slice(0, 10),
      title: entry.title ?? '',
      content: entry.content,
      mood: entry.mood ? String(entry.mood) : '',
      energy: entry.energy ? String(entry.energy) : '',
      tags: entry.tags.join(', '),
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
      title: form.title || null,
      content: form.content,
      mood: form.mood ? Number(form.mood) : null,
      energy: form.energy ? Number(form.energy) : null,
      tags: form.tags.split(',').map((t) => t.trim()).filter(Boolean),
    }
    try {
      if (editing) {
        await journalApi.update(editing.id, body)
      } else {
        await journalApi.create(body)
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
    if (!confirm('Delete this journal entry?')) return
    await journalApi.remove(id)
    reload()
  }

  return (
    <div>
      <PageHeader
        title="Journal"
        actions={
          <button onClick={openCreate} className={buttonPrimary}>
            <Plus size={15} /> New entry
          </button>
        }
      />
      <div className="relative mb-4 w-64">
        <Search size={15} className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-gray-500" />
        <input
          placeholder="Search journal…"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className={`${inputClass} pl-9`}
        />
      </div>
      {actionError && <ErrorBanner message={actionError} />}
      {error && <ErrorBanner message={error} />}
      {loading && <Spinner />}
      {!loading && (entries?.length ?? 0) === 0 && <EmptyState icon={BookOpen} message="No journal entries yet." />}

      {entries?.map((entry) => {
        const MoodFace = entry.mood ? moodFaces[entry.mood - 1] : null
        return (
          <div key={entry.id} className={`mb-3 ${cardClass} px-4 py-3`}>
            <div className="flex items-start justify-between gap-3">
              <div className="min-w-0 flex-1">
                <div className="flex flex-wrap items-center gap-2 text-xs text-gray-400">
                  <span className="font-medium text-gray-600">{formatDate(entry.date)}</span>
                  {MoodFace && (
                    <span className="flex items-center gap-1" title={`Mood ${entry.mood}/5`}>
                      <MoodFace size={14} /> {moodLabel(entry.mood)}
                    </span>
                  )}
                  {entry.energy && (
                    <span className="flex items-center gap-1" title={`Energy ${entry.energy}/5`}>
                      <Zap size={14} className="text-amber-500" /> {entry.energy}/5
                    </span>
                  )}
                  <CategoryDot category={categories.find((c) => c.id === entry.categoryId)} />
                </div>
                {entry.title && <h3 className="mt-1 text-sm font-semibold text-gray-900">{entry.title}</h3>}
                <p className="mt-1 whitespace-pre-wrap text-sm text-gray-600">{entry.content}</p>
                {entry.tags.length > 0 && (
                  <div className="mt-2">
                    <TagPills tags={entry.tags} />
                  </div>
                )}
              </div>
              <div className="flex shrink-0 gap-0.5">
                <IconButton icon={Pencil} label="Edit entry" onClick={() => openEdit(entry)} />
                <IconButton icon={Trash2} label="Delete entry" tone="danger" onClick={() => remove(entry.id)} />
              </div>
            </div>
          </div>
        )
      })}

      {(creating || editing) && (
        <Modal title={editing ? 'Edit entry' : 'New journal entry'} onClose={() => { setCreating(false); setEditing(null) }}>
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
          <Field label="Title (optional)">
            <input
              className={inputClass}
              value={form.title}
              onChange={(e) => setForm({ ...form, title: e.target.value })}
            />
          </Field>
          <Field label="Content">
            <textarea
              className={inputClass}
              rows={6}
              value={form.content}
              onChange={(e) => setForm({ ...form, content: e.target.value })}
              autoFocus
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
          <Field label="Tags">
            <TagInput
              value={form.tags.split(',').map((t) => t.trim()).filter(Boolean)}
              onChange={(tags) => setForm({ ...form, tags: tags.join(', ') })}
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
