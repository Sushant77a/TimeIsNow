import type { ReactNode } from 'react'
import { BrowserRouter, Navigate, Outlet, Route, Routes, useLocation } from 'react-router-dom'
import Layout from './components/Layout'
import { Spinner } from './components/ui'
import { AuthProvider, useAuth } from './context/AuthContext'
import { CategoryProvider } from './context/CategoryContext'
import Dashboard from './pages/Dashboard'
import Todos from './pages/Todos'
import Habits from './pages/Habits'
import DailyEntries from './pages/DailyEntries'
import Journal from './pages/Journal'
import Schedule from './pages/Schedule'
import Finance from './pages/Finance'
import Goals from './pages/Goals'
import Notes from './pages/Notes'
import Settings from './pages/Settings'
import Login from './pages/Login'
import Register from './pages/Register'

function RequireAuth({ children }: { children: ReactNode }) {
  const { user, loading } = useAuth()
  const location = useLocation()

  if (loading) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <Spinner />
      </div>
    )
  }

  if (!user) {
    return <Navigate to="/login" state={{ from: location }} replace />
  }

  return children
}

function PublicOnly({ children }: { children: ReactNode }) {
  const { user, loading } = useAuth()

  if (loading) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <Spinner />
      </div>
    )
  }

  if (user) {
    return <Navigate to="/" replace />
  }

  return children
}

function ProtectedLayout() {
  return (
    <RequireAuth>
      <CategoryProvider>
        <Outlet />
      </CategoryProvider>
    </RequireAuth>
  )
}

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route
            path="login"
            element={
              <PublicOnly>
                <Login />
              </PublicOnly>
            }
          />
          <Route
            path="register"
            element={
              <PublicOnly>
                <Register />
              </PublicOnly>
            }
          />
          <Route element={<ProtectedLayout />}>
            <Route element={<Layout />}>
              <Route index element={<Dashboard />} />
              <Route path="todos" element={<Todos />} />
              <Route path="habits" element={<Habits />} />
              <Route path="entries" element={<DailyEntries />} />
              <Route path="journal" element={<Journal />} />
              <Route path="schedule" element={<Schedule />} />
              <Route path="finance" element={<Finance />} />
              <Route path="goals" element={<Goals />} />
              <Route path="notes" element={<Notes />} />
              <Route path="settings" element={<Settings />} />
            </Route>
          </Route>
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  )
}
