import { useMemo, useState } from 'react'
import { financeApi } from '../api/resources'
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
import {
  Pencil,
  Plus,
  Scale,
  Trash2,
  TrendingDown,
  TrendingUp,
  Wallet,
} from '../components/icons'
import type { FinanceEntry, FinanceType } from '../types'
import { formatDate, todayIso } from '../lib/format'

interface FinanceForm {
  categoryId: number | null
  type: FinanceType
  amount: string
  date: string
  description: string
}

const emptyForm: FinanceForm = {
  categoryId: null,
  type: 'Expense',
  amount: '',
  date: todayIso(),
  description: '',
}

export default function Finance() {
  const { categories, selectedCategoryId } = useCategories()
  const [typeFilter, setTypeFilter] = useState<string>('')
  const [editing, setEditing] = useState<FinanceEntry | null>(null)
  const [creating, setCreating] = useState(false)
  const [form, setForm] = useState<FinanceForm>(emptyForm)
  const [saving, setSaving] = useState(false)
  const [actionError, setActionError] = useState<string | null>(null)

  const { data: entries, error, loading, reload } = useAsync(
    () =>
      financeApi.getList({
        categoryId: selectedCategoryId ?? undefined,
        type: typeFilter || undefined,
      }),
    [selectedCategoryId, typeFilter],
  )

  const totals = useMemo(() => {
    let income = 0
    let expense = 0
    for (const entry of entries ?? []) {
      if (entry.type === 'Income') income += entry.amount
      else expense += entry.amount
    }
    return { income, expense, balance: income - expense }
  }, [entries])

  function openCreate() {
    setForm({ ...emptyForm, categoryId: selectedCategoryId })
    setEditing(null)
    setCreating(true)
  }

  function openEdit(entry: FinanceEntry) {
    setForm({
      categoryId: entry.categoryId,
      type: entry.type,
      amount: String(entry.amount),
      date: entry.date.slice(0, 10),
      description: entry.description ?? '',
    })
    setEditing(entry)
    setCreating(false)
  }

  async function save() {
    if (!form.categoryId || !form.amount) return
    setSaving(true)
    setActionError(null)
    const body = {
      categoryId: form.categoryId,
      type: form.type,
      amount: Number(form.amount),
      date: form.date,
      description: form.description || null,
    }
    try {
      if (editing) {
        await financeApi.update(editing.id, body)
      } else {
        await financeApi.create(body)
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
    await financeApi.remove(id)
    reload()
  }

  return (
    <div>
      <PageHeader
        title="Finance"
        actions={
          <>
            <select
              value={typeFilter}
              onChange={(e) => setTypeFilter(e.target.value)}
              className={`${inputClass} w-auto`}
            >
              <option value="">All types</option>
              <option value="Expense">Expense</option>
              <option value="Income">Income</option>
            </select>
            <button onClick={openCreate} className={buttonPrimary}>
              <Plus size={15} /> New entry
            </button>
          </>
        }
      />
      <div className="mb-4 grid grid-cols-3 gap-3 sm:max-w-xl">
        <div className={`${cardClass} px-4 py-2`}>
          <p className="flex items-center gap-1.5 text-xs text-gray-400">
            <TrendingUp size={13} className="text-emerald-500" /> Income
          </p>
          <p className="text-lg font-semibold text-emerald-600">+{totals.income.toFixed(2)}</p>
        </div>
        <div className={`${cardClass} px-4 py-2`}>
          <p className="flex items-center gap-1.5 text-xs text-gray-400">
            <TrendingDown size={13} className="text-red-500" /> Expense
          </p>
          <p className="text-lg font-semibold text-red-600">−{totals.expense.toFixed(2)}</p>
        </div>
        <div className={`${cardClass} px-4 py-2`}>
          <p className="flex items-center gap-1.5 text-xs text-gray-400">
            <Scale size={13} className="text-accent-400" /> Balance
          </p>
          <p className={`text-lg font-semibold ${totals.balance >= 0 ? 'text-emerald-600' : 'text-red-600'}`}>
            {totals.balance.toFixed(2)}
          </p>
        </div>
      </div>
      {actionError && <ErrorBanner message={actionError} />}
      {error && <ErrorBanner message={error} />}
      {loading && <Spinner />}
      {!loading && (entries?.length ?? 0) === 0 && <EmptyState icon={Wallet} message="No finance entries yet." />}

      {entries?.map((entry) => (
        <div key={entry.id} className={`mb-2 flex items-center gap-3 ${cardClass} px-4 py-3`}>
          <div className="w-24 shrink-0">
            <p className="text-sm text-gray-500">{formatDate(entry.date)}</p>
          </div>
          <Badge
            text={entry.type}
            className={entry.type === 'Income' ? 'bg-emerald-100 text-emerald-700' : 'bg-red-100 text-red-700'}
          />
          <div className="min-w-0 flex-1">
            <p className="truncate text-sm text-gray-800">{entry.description || '-'}</p>
            <CategoryDot category={categories.find((c) => c.id === entry.categoryId)} />
          </div>
          <p className={`shrink-0 text-sm font-semibold ${entry.type === 'Income' ? 'text-emerald-600' : 'text-red-600'}`}>
            {entry.type === 'Income' ? '+' : '−'}{entry.amount.toFixed(2)}
          </p>
          <div className="flex shrink-0 gap-0.5">
            <IconButton icon={Pencil} label="Edit entry" onClick={() => openEdit(entry)} />
            <IconButton icon={Trash2} label="Delete entry" tone="danger" onClick={() => remove(entry.id)} />
          </div>
        </div>
      ))}

      {(creating || editing) && (
        <Modal title={editing ? 'Edit entry' : 'New finance entry'} onClose={() => { setCreating(false); setEditing(null) }}>
          <div className="grid grid-cols-2 gap-3">
            <Field label="Type">
              <select
                className={inputClass}
                value={form.type}
                onChange={(e) => setForm({ ...form, type: e.target.value as FinanceType })}
              >
                <option>Expense</option>
                <option>Income</option>
              </select>
            </Field>
            <Field label="Amount">
              <input
                type="number"
                step="0.01"
                min="0.01"
                className={inputClass}
                value={form.amount}
                onChange={(e) => setForm({ ...form, amount: e.target.value })}
                autoFocus
              />
            </Field>
          </div>
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
          <Field label="Description">
            <input
              className={inputClass}
              value={form.description}
              onChange={(e) => setForm({ ...form, description: e.target.value })}
            />
          </Field>
          <div className="flex justify-end gap-2">
            <button className={buttonSecondary} onClick={() => { setCreating(false); setEditing(null) }}>
              Cancel
            </button>
            <button className={buttonPrimary} onClick={save} disabled={saving || !form.amount || !form.categoryId}>
              {saving ? 'Saving…' : 'Save'}
            </button>
          </div>
        </Modal>
      )}
    </div>
  )
}
