import { createContext, useContext, useState, type ReactNode } from 'react'
import type { Category } from '../types'

interface CategoryContextValue {
  categories: Category[]
  setCategories: (categories: Category[]) => void
  selectedCategoryId: number | null
  selectCategory: (id: number | null) => void
  selectedCategory: Category | null
  allModules: string[]
  setAllModules: (modules: string[]) => void
}

const CategoryContext = createContext<CategoryContextValue | null>(null)

export function CategoryProvider({ children }: { children: ReactNode }) {
  const [categories, setCategories] = useState<Category[]>([])
  const [selectedCategoryId, setSelectedCategoryId] = useState<number | null>(null)
  const [allModules, setAllModules] = useState<string[]>([])

  return (
    <CategoryContext.Provider
      value={{
        categories,
        setCategories,
        selectedCategoryId,
        selectCategory: setSelectedCategoryId,
        selectedCategory: categories.find((c) => c.id === selectedCategoryId) ?? null,
        allModules,
        setAllModules,
      }}
    >
      {children}
    </CategoryContext.Provider>
  )
}

export function useCategories() {
  const context = useContext(CategoryContext)
  if (!context) throw new Error('useCategories must be used within CategoryProvider')
  return context
}
