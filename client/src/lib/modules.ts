export const DEFAULT_ALL_MODULES = ['dashboard', 'todos', 'habits', 'finance']

const DEFAULT_CATEGORY_MODULES = ['dashboard', 'entries', 'schedule', 'goals', 'notes', 'settings']

const PERSONAL_MODULES = ['dashboard', 'entries', 'journal', 'schedule', 'goals', 'notes', 'settings']

export function defaultModulesForCategory(name: string): string[] {
  if (name.trim().toLowerCase() === 'personal') return PERSONAL_MODULES
  return DEFAULT_CATEGORY_MODULES
}

export function modulesForCategory(category: { name: string; modules?: string[] }): string[] {
  if (category.modules && category.modules.length > 0) return category.modules
  return defaultModulesForCategory(category.name)
}

export function modulesForAllScope(configured: string[]): string[] {
  if (configured.length > 0) return configured
  return DEFAULT_ALL_MODULES
}
