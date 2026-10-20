import { useEffect } from 'react'
import { NavLink, Outlet } from 'react-router-dom'
import { Hourglass, LogOut, Tag } from 'lucide-react'
import { categoriesApi, modulesApi } from '../api/resources'
import { useAuth } from '../context/AuthContext'
import { useCategories } from '../context/CategoryContext'
import { useAsync } from '../hooks/useAsync'
import { modulesForAllScope, modulesForCategory } from '../lib/modules'
import { navItems } from './icons'

export default function Layout() {
  const {
    categories,
    setCategories,
    selectedCategoryId,
    selectCategory,
    selectedCategory,
    allModules,
    setAllModules,
  } = useCategories()
  const { user, logout } = useAuth()
  const { data, error, loading } = useAsync(() => categoriesApi.getAll(), [])
  const allScope = useAsync(() => modulesApi.getAllScope(), [])

  useEffect(() => {
    if (data) setCategories(data)
  }, [data, setCategories])

  useEffect(() => {
    if (allScope.data) setAllModules(allScope.data.modules)
  }, [allScope.data, setAllModules])

  const initial = (user?.displayName ?? user?.email ?? '?').charAt(0).toUpperCase()

  const activeKeys = selectedCategory
    ? modulesForCategory(selectedCategory)
    : modulesForAllScope(allModules)
  const visibleNav = navItems.filter((item) => item.key === 'settings' || activeKeys.includes(item.key))

  return (
    <div className="flex min-h-screen">
      <aside className="fixed inset-y-0 left-0 z-30 flex w-60 flex-col border-r border-hairline bg-surface/80 backdrop-blur-xl">
        <div className="flex items-center gap-2.5 border-b border-hairline px-4 py-4">
          <span className="flex h-8 w-8 items-center justify-center rounded-lg bg-accent-50 text-accent ring-1 ring-inset ring-hairline-strong">
            <Hourglass size={17} />
          </span>
          <div>
            <h1 className="text-sm font-bold leading-tight tracking-tight">
              <span className="text-ink">
                Time is Now
              </span>
            </h1>
            <p className="text-[11px] leading-tight text-gray-500">personal life dashboard</p>
          </div>
        </div>

        <nav data-testid="sidebar-nav" className="flex-1 overflow-y-auto px-2 py-3">
          {visibleNav.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              end={item.to === '/'}
              className={({ isActive }) =>
                `group relative mb-0.5 flex items-center gap-2.5 rounded-lg px-3 py-2 text-sm font-medium transition ${
                  isActive
                    ? 'bg-accent-500/15 text-accent-300'
                    : 'text-gray-500 hover:bg-hairline hover:text-gray-800'
                }`
              }
            >
              {({ isActive }) => (
                <>
                  {isActive && (
                    <span className="absolute inset-y-1.5 left-0 w-0.5 rounded-full bg-accent-400 " />
                  )}
                  <item.Icon
                    size={17}
                    strokeWidth={2}
                    className={isActive ? 'text-accent-300' : 'text-gray-500 transition group-hover:text-gray-700'}
                  />
                  {item.label}
                </>
              )}
            </NavLink>
          ))}
        </nav>

        <div className="border-t border-hairline px-4 py-3">
          <div className="mb-2 flex items-center gap-2">
            <span className="flex h-7 w-7 shrink-0 items-center justify-center rounded-full bg-accent-500/20 text-xs font-semibold text-accent-300 ring-1 ring-inset ring-hairline-strong">
              {initial}
            </span>
            <div className="min-w-0 flex-1">
              <p className="truncate text-sm font-medium text-gray-800">
                {user?.displayName ?? 'Signed in'}
              </p>
              <p className="truncate text-xs text-gray-500" data-testid="user-email">{user?.email}</p>
            </div>
            <button
              onClick={() => void logout()}
              title="Sign out"
              aria-label="Sign out"
              className="rounded-lg p-1.5 text-gray-500 transition hover:bg-red-500/10 hover:text-red-300"
            >
              <LogOut size={16} />
            </button>
          </div>
        </div>
      </aside>

      <main className="ml-60 flex-1">
        <header className="sticky top-0 z-20 border-b border-hairline bg-canvas/70 backdrop-blur-xl">
          <div className="flex items-center gap-3 px-8 py-2.5">
            <Tag size={15} className="shrink-0 text-gray-500" />
            {loading ? (
              <span className="text-xs text-gray-500">Loading categories…</span>
            ) : error ? (
              <span className="text-xs text-red-400">{error}</span>
            ) : (
              <div className="flex flex-1 items-center gap-1.5 overflow-x-auto">
                <button
                  data-testid="scope-chip"
                  data-scope="all"
                  onClick={() => selectCategory(null)}
                  className={`shrink-0 rounded-full border px-3 py-1 text-xs font-medium transition ${
                    selectedCategoryId === null
                      ? 'border-accent-400/40 bg-accent-500/15 text-accent-300'
                      : 'border-hairline text-gray-500 hover:bg-hairline hover:text-gray-700'
                  }`}
                >
                  All
                </button>
                {categories.map((category) => {
                  const active = selectedCategoryId === category.id
                  return (
                    <button
                      key={category.id}
                      data-testid="scope-chip"
                      data-scope={category.name}
                      onClick={() => selectCategory(category.id)}
                      className={`flex shrink-0 items-center gap-2 rounded-full border px-3 py-1 text-xs font-medium transition ${
                        active
                          ? 'border-accent-400/40 bg-accent-500/15 text-accent-300'
                          : 'border-hairline text-gray-500 hover:bg-hairline hover:text-gray-700'
                      }`}
                    >
                      <span
                        className="h-2 w-2 shrink-0 rounded-full "
                        style={{ backgroundColor: category.color, color: category.color }}
                      />
                      {category.name}
                    </button>
                  )
                })}
              </div>
            )}
          </div>
        </header>
        <div className="overflow-x-hidden px-8 py-6">
          <Outlet />
        </div>
      </main>
    </div>
  )
}
