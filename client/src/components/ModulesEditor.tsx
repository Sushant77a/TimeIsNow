import { useEffect, useState } from 'react'
import { Check } from 'lucide-react'
import { modulesApi } from '../api/resources'
import { useCategories } from '../context/CategoryContext'
import { cardClass, ErrorBanner } from './ui'
import { navItems } from './icons'
import { modulesForAllScope, modulesForCategory } from '../lib/modules'

const editableNav = navItems.filter((item) => item.key !== 'settings')

function ModuleToggles({
  selected,
  onToggle,
}: {
  selected: string[]
  onToggle: (key: string) => void
}) {
  return (
    <div className="flex flex-wrap gap-1.5">
      {editableNav.map((item) => {
        const active = selected.includes(item.key)
        return (
          <button
            key={item.key}
            type="button"
            data-testid="module-toggle"
            data-module={item.key}
            aria-pressed={active}
            onClick={() => onToggle(item.key)}
            className={`inline-flex items-center gap-1.5 rounded-lg border px-2.5 py-1.5 text-xs font-medium transition ${
              active
                ? 'border-accent-400/40 bg-accent-500/15 text-accent-300'
                : 'border-hairline-strong text-gray-500 hover:border-hairline-strong hover:text-gray-300'
            }`}
          >
            <item.Icon size={14} />
            {item.label}
            {active && <Check size={12} className="text-accent-300" />}
          </button>
        )
      })}
    </div>
  )
}

export default function ModulesEditor() {
  const { categories, allModules, setAllModules, setCategories } = useCategories()
  const [scopeModules, setScopeModules] = useState<Record<number, string[]>>({})
  const [savingKey, setSavingKey] = useState<string | null>(null)
  const [savedKey, setSavedKey] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const next: Record<number, string[]> = {}
    for (const category of categories) {
      next[category.id] = modulesForCategory(category)
    }
    setScopeModules(next)
  }, [categories])

  function toggleGlobal(key: string) {
    const current = modulesForAllScope(allModules)
    const next = current.includes(key)
      ? current.filter((k) => k !== key)
      : [...current, key]
    setAllModules(next)
    void save('global', () => modulesApi.updateAllScope(next))
  }

  function toggleCategory(categoryId: number, key: string) {
    const current = scopeModules[categoryId] ?? []
    const next = current.includes(key)
      ? current.filter((k) => k !== key)
      : [...current, key]
    setScopeModules((prev) => ({ ...prev, [categoryId]: next }))
    void save(`cat-${categoryId}`, async () => {
      await modulesApi.updateCategoryScope(categoryId, next)
      setCategories(
        categories.map((c) => (c.id === categoryId ? { ...c, modules: next } : c)),
      )
    })
  }

  async function save(key: string, op: () => Promise<unknown>) {
    setSavingKey(key)
    setSavedKey(null)
    setError(null)
    try {
      await op()
      setSavedKey(key)
      window.setTimeout(() => setSavedKey((k) => (k === key ? null : k)), 1800)
    } catch (err) {
      setError((err as Error).message)
    } finally {
      setSavingKey(null)
    }
  }

  return (
    <div className="mt-6">
      <div className="mb-3">
        <h2 className="text-lg font-semibold text-gray-900">Navigation modules</h2>
        <p className="mt-0.5 text-sm text-gray-500">
          Choose which modules appear in the sidebar for each scope.
        </p>
      </div>
      {error && <ErrorBanner message={error} />}
      <div className={`${cardClass} divide-y divide-hairline`}>
        <div className="p-4" data-testid="modules-all">
          <div className="mb-2.5 flex items-center gap-2">
            <span className="text-sm font-medium text-gray-800">All</span>
            {savingKey === 'global' && <span className="text-xs text-gray-500">Saving…</span>}
            {savedKey === 'global' && (
              <span className="flex items-center gap-1 text-xs text-emerald-600">
                <Check size={13} /> Saved
              </span>
            )}
          </div>
          <ModuleToggles selected={modulesForAllScope(allModules)} onToggle={toggleGlobal} />
        </div>
        {categories.map((category) => (
          <div key={category.id} className="p-4" data-testid="modules-category" data-category={category.name}>
            <div className="mb-2.5 flex items-center gap-2">
              <span
                className="h-2.5 w-2.5 rounded-full"
                style={{ backgroundColor: category.color }}
              />
              <span className="text-sm font-medium text-gray-800">{category.name}</span>
              {savingKey === `cat-${category.id}` && (
                <span className="text-xs text-gray-500">Saving…</span>
              )}
              {savedKey === `cat-${category.id}` && (
                <span className="flex items-center gap-1 text-xs text-emerald-600">
                  <Check size={13} /> Saved
                </span>
              )}
            </div>
            <ModuleToggles
              selected={scopeModules[category.id] ?? modulesForCategory(category)}
              onToggle={(key) => toggleCategory(category.id, key)}
            />
          </div>
        ))}
      </div>
    </div>
  )
}
