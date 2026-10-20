import { useState } from 'react'
import { notesApi } from '../api/resources'
import { useCategories } from '../context/CategoryContext'
import { useAsync } from '../hooks/useAsync'
import {
  CategorySelect,
  EmptyState,
  ErrorBanner,
  Field,
  IconButton,
  Modal,
  PageHeader,
  Spinner,
  TagInput,
  TagPills,
  buttonPrimary,
  buttonSecondary,
  cardClass,
  inputClass,
} from '../components/ui'
import { Inbox, Pencil, Pin, Plus, Search, Trash2, X } from '../components/icons'
import type { Note } from '../types'

interface NoteForm {
  categoryId: number | null
  title: string
  content: string
  isPinned: boolean
  tags: string
}

const emptyForm: NoteForm = {
  categoryId: null,
  title: '',
  content: '',
  isPinned: false,
  tags: '',
}

export default function Notes() {
  const { categories, selectedCategoryId } = useCategories()
  const [search, setSearch] = useState('')
  const [tagFilter, setTagFilter] = useState<string | null>(null)
  const [pinnedOnly, setPinnedOnly] = useState(false)
  const [editing, setEditing] = useState<Note | null>(null)
  const [creating, setCreating] = useState(false)
  const [form, setForm] = useState<NoteForm>(emptyForm)
  const [saving, setSaving] = useState(false)
  const [actionError, setActionError] = useState<string | null>(null)

  const { data: notes, error, loading, reload } = useAsync(
    () =>
      notesApi.getList({
        categoryId: selectedCategoryId ?? undefined,
        search: search || undefined,
        tag: tagFilter ?? undefined,
        pinned: pinnedOnly || undefined,
      }),
    [selectedCategoryId, search, tagFilter, pinnedOnly],
  )

  function openCreate() {
    setForm({ ...emptyForm, categoryId: selectedCategoryId })
    setEditing(null)
    setCreating(true)
  }

  function openEdit(note: Note) {
    setForm({
      categoryId: note.categoryId,
      title: note.title,
      content: note.content,
      isPinned: note.isPinned,
      tags: note.tags.join(', '),
    })
    setEditing(note)
    setCreating(false)
  }

  async function save() {
    if (!form.title.trim() || !form.categoryId) return
    setSaving(true)
    setActionError(null)
    const body = {
      categoryId: form.categoryId,
      title: form.title,
      content: form.content,
      isPinned: form.isPinned,
      tags: form.tags.split(',').map((t) => t.trim()).filter(Boolean),
    }
    try {
      if (editing) {
        await notesApi.update(editing.id, body)
      } else {
        await notesApi.create(body)
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
    if (!confirm('Delete this note?')) return
    await notesApi.remove(id)
    reload()
  }

  return (
    <div>
      <PageHeader
        title="Notes"
        actions={
          <button onClick={openCreate} className={buttonPrimary}>
            <Plus size={15} /> New note
          </button>
        }
      />
      <div className="mb-4 flex flex-wrap items-center gap-2">
        <div className="relative w-64">
          <Search size={15} className="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-gray-500" />
          <input
            placeholder="Search notes…"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className={`${inputClass} pl-9`}
          />
        </div>
        <button
          onClick={() => setPinnedOnly(!pinnedOnly)}
          className={`inline-flex items-center gap-1.5 rounded-lg border px-3 py-1.5 text-sm transition ${
            pinnedOnly
              ? 'border-accent-400/40 bg-accent-500/15 text-accent-300'
              : 'border-gray-300 text-gray-600 hover:bg-hairline hover:text-gray-800'
          }`}
        >
          <Pin size={14} className={pinnedOnly ? 'fill-accent-400/40' : ''} /> Pinned
        </button>
        {tagFilter && (
          <button
            onClick={() => setTagFilter(null)}
            className="inline-flex items-center gap-1.5 rounded-lg bg-accent-50 px-3 py-1.5 text-sm text-accent-700 transition hover:bg-accent-100"
          >
            #{tagFilter} <X size={13} />
          </button>
        )}
      </div>
      {actionError && <ErrorBanner message={actionError} />}
      {error && <ErrorBanner message={error} />}
      {loading && <Spinner />}
      {!loading && (notes?.length ?? 0) === 0 && <EmptyState icon={Inbox} message="No notes yet." />}

      <div className="grid grid-cols-1 gap-3 md:grid-cols-2 xl:grid-cols-3">
        {notes?.map((note) => (
          <div
            key={note.id}
            className={`flex flex-col ${cardClass} px-4 py-3 ${
              note.isPinned ? 'border-amber-400/40' : ''
            }`}
          >
            <div className="flex items-start justify-between gap-2">
              <h3 className="flex items-center gap-1.5 text-sm font-semibold text-gray-900">
                {note.isPinned && <Pin size={13} className="shrink-0 text-amber-400" />}
                {note.title}
              </h3>
              <div className="flex shrink-0 gap-0.5">
                <IconButton icon={Pencil} label="Edit note" onClick={() => openEdit(note)} />
                <IconButton icon={Trash2} label="Delete note" tone="danger" onClick={() => remove(note.id)} />
              </div>
            </div>
            <p className="mt-1 line-clamp-4 flex-1 whitespace-pre-wrap text-sm text-gray-600">
              {note.content || <span className="text-gray-300">(empty)</span>}
            </p>
            <div className="mt-2 flex flex-wrap items-center gap-2">
              <TagPills tags={note.tags} />
              {note.tags.length > 0 && (
                <button
                  className="text-xs text-accent-600 hover:underline"
                  onClick={() => setTagFilter(note.tags[0])}
                >
                  filter #{note.tags[0]}
                </button>
              )}
            </div>
          </div>
        ))}
      </div>

      {(creating || editing) && (
        <Modal title={editing ? 'Edit note' : 'New note'} onClose={() => { setCreating(false); setEditing(null) }}>
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
          <Field label="Content">
            <textarea
              className={inputClass}
              rows={6}
              value={form.content}
              onChange={(e) => setForm({ ...form, content: e.target.value })}
            />
          </Field>
          <Field label="Tags">
            <TagInput
              value={form.tags.split(',').map((t) => t.trim()).filter(Boolean)}
              onChange={(tags) => setForm({ ...form, tags: tags.join(', ') })}
            />
          </Field>
          <label className="mb-3 flex items-center gap-2 text-sm text-gray-700">
            <input
              type="checkbox"
              checked={form.isPinned}
              onChange={(e) => setForm({ ...form, isPinned: e.target.checked })}
              className="rounded-sm border-gray-300 text-accent-600 focus:ring-accent-500"
            />
            Pinned
          </label>
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
