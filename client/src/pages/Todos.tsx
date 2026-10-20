import { useState } from 'react'
import { todosApi } from '../api/resources'
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
  TagInput,
  TagPills,
  buttonPrimary,
  buttonSecondary,
  cardClass,
  inputClass,
} from '../components/ui'
import { ListTodo, Pencil, Plus, Trash2 } from '../components/icons'
import type { Priority, Todo } from '../types'
import { formatDateTime, priorityClasses } from '../lib/format'

interface TodoForm {
  categoryId: number | null
  title: string
  description: string
  priority: Priority
  dueDate: string
  tags: string
}

const emptyForm: TodoForm = {
  categoryId: null,
  title: '',
  description: '',
  priority: 'Medium',
  dueDate: '',
  tags: '',
}

function toRequest(form: TodoForm, isDone = false) {
  return {
    categoryId: form.categoryId!,
    title: form.title,
    description: form.description || null,
    priority: form.priority,
    dueDate: form.dueDate ? new Date(form.dueDate).toISOString() : null,
    isDone,
    tags: form.tags.split(',').map((t) => t.trim()).filter(Boolean),
  }
}

export default function Todos() {
  const { categories, selectedCategoryId } = useCategories()
  const [isDone, setIsDone] = useState<boolean | undefined>(false)
  const [editing, setEditing] = useState<Todo | null>(null)
  const [creating, setCreating] = useState(false)
  const [form, setForm] = useState<TodoForm>(emptyForm)
  const [saving, setSaving] = useState(false)
  const [actionError, setActionError] = useState<string | null>(null)

  const { data: todos, error, loading, reload } = useAsync(
    () => todosApi.getList({ categoryId: selectedCategoryId ?? undefined, isDone }),
    [selectedCategoryId, isDone],
  )

  function openCreate() {
    setForm({ ...emptyForm, categoryId: selectedCategoryId })
    setEditing(null)
    setCreating(true)
  }

  function openEdit(todo: Todo) {
    setForm({
      categoryId: todo.categoryId,
      title: todo.title,
      description: todo.description ?? '',
      priority: todo.priority,
      dueDate: todo.dueDate ? todo.dueDate.slice(0, 16) : '',
      tags: todo.tags.join(', '),
    })
    setEditing(todo)
    setCreating(false)
  }

  async function save() {
    if (!form.title.trim() || !form.categoryId) return
    setSaving(true)
    setActionError(null)
    try {
      if (editing) {
        await todosApi.update(editing.id, toRequest(form, editing.isDone))
      } else {
        await todosApi.create(toRequest(form))
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

  async function toggleDone(todo: Todo) {
    await todosApi.update(todo.id, {
      categoryId: todo.categoryId,
      title: todo.title,
      description: todo.description,
      priority: todo.priority,
      dueDate: todo.dueDate,
      reminderAt: todo.reminderAt,
      recurrenceType: todo.recurrenceType,
      recurrenceInterval: todo.recurrenceInterval,
      isDone: !todo.isDone,
      parentTodoId: todo.parentTodoId,
      tags: todo.tags,
    })
    reload()
  }

  async function remove(id: number) {
    if (!confirm('Delete this todo (and its subtasks)?')) return
    await todosApi.remove(id)
    reload()
  }

  function renderTodo(todo: Todo, nested = false) {
    return (
      <div key={todo.id}>
        <div
          className={`mb-2 flex items-start gap-3 ${cardClass} px-4 py-3 ${
            nested ? 'ml-8' : ''
          } ${todo.isDone ? 'opacity-60' : ''}`}
        >
          <input
            type="checkbox"
            checked={todo.isDone}
            onChange={() => toggleDone(todo)}
            className="mt-1 h-4 w-4 rounded-sm border-gray-300"
          />
          <div className="min-w-0 flex-1">
            <div className="flex flex-wrap items-center gap-2">
              <span className={`text-sm font-medium ${todo.isDone ? 'line-through text-gray-400' : 'text-gray-900'}`}>
                {todo.title}
              </span>
              <Badge text={todo.priority} className={priorityClasses(todo.priority)} />
              <CategoryDot category={categories.find((c) => c.id === todo.categoryId)} />
              {todo.dueDate && (
                <span className="text-xs text-gray-400">due {formatDateTime(todo.dueDate)}</span>
              )}
            </div>
            {todo.description && (
              <p className="mt-1 text-sm text-gray-500">{todo.description}</p>
            )}
            <div className="mt-1.5">
              <TagPills tags={todo.tags} />
            </div>
          </div>
          <div className="flex shrink-0 gap-0.5">
            <IconButton icon={Pencil} label="Edit todo" onClick={() => openEdit(todo)} />
            <IconButton icon={Trash2} label="Delete todo" tone="danger" onClick={() => remove(todo.id)} />
          </div>
        </div>
        {todo.subTasks.filter((sub) => !sub.isDone || !todo.isDone).map((sub) => renderTodo(sub, true))}
      </div>
    )
  }

  const roots = (todos ?? []).filter((todo) => todo.parentTodoId === null)

  return (
    <div>
      <PageHeader
        title="Todos"
        actions={
          <>
            <select
              value={isDone === undefined ? 'all' : isDone ? 'done' : 'open'}
              onChange={(e) =>
                setIsDone(e.target.value === 'all' ? undefined : e.target.value === 'done')
              }
              className={`${inputClass} w-auto`}
            >
              <option value="open">Open</option>
              <option value="done">Done</option>
              <option value="all">All</option>
            </select>
            <button onClick={openCreate} className={buttonPrimary}>
              <Plus size={15} /> New todo
            </button>
          </>
        }
      />
      {actionError && <ErrorBanner message={actionError} />}
      {error && <ErrorBanner message={error} />}
      {loading && <Spinner />}
      {!loading && roots.length === 0 && <EmptyState icon={ListTodo} message="No todos yet." />}
      {roots.map((todo) => renderTodo(todo))}

      {(creating || editing) && (
        <Modal title={editing ? 'Edit todo' : 'New todo'} onClose={() => { setCreating(false); setEditing(null) }}>
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
            <Field label="Priority">
              <select
                className={inputClass}
                value={form.priority}
                onChange={(e) => setForm({ ...form, priority: e.target.value as Priority })}
              >
                <option>Low</option>
                <option>Medium</option>
                <option>High</option>
              </select>
            </Field>
            <Field label="Due date">
              <input
                type="datetime-local"
                className={inputClass}
                value={form.dueDate}
                onChange={(e) => setForm({ ...form, dueDate: e.target.value })}
              />
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
          <Field label="Tags">
            <TagInput
              value={form.tags.split(',').map((t) => t.trim()).filter(Boolean)}
              onChange={(tags) => setForm({ ...form, tags: tags.join(', ') })}
            />
          </Field>
          <div className="mt-4 flex justify-end gap-2">
            <button
              className={buttonSecondary}
              onClick={() => { setCreating(false); setEditing(null) }}
            >
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
