import { useEffect, useState } from 'react'
import { profileApi } from '../api/resources'
import { useAsync } from '../hooks/useAsync'
import {
  ErrorBanner,
  Field,
  PageHeader,
  Spinner,
  buttonPrimary,
  cardClass,
  inputClass,
} from '../components/ui'
import { Check } from '../components/icons'
import ModulesEditor from '../components/ModulesEditor'

const dayNames = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday']

export default function Settings() {
  const { data: profile, error, loading } = useAsync(() => profileApi.get(), [])
  const [form, setForm] = useState({ displayName: '', currency: '', weekStartsOn: '1', theme: 'system' })
  const [saving, setSaving] = useState(false)
  const [saved, setSaved] = useState(false)
  const [actionError, setActionError] = useState<string | null>(null)

  useEffect(() => {
    if (profile) {
      setForm({
        displayName: profile.displayName,
        currency: profile.currency,
        weekStartsOn: String(profile.weekStartsOn),
        theme: profile.theme,
      })
    }
  }, [profile])

  async function save() {
    setSaving(true)
    setSaved(false)
    setActionError(null)
    try {
      await profileApi.update({
        displayName: form.displayName,
        currency: form.currency,
        weekStartsOn: Number(form.weekStartsOn),
        theme: form.theme,
      })
      setSaved(true)
    } catch (err) {
      setActionError((err as Error).message)
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className="max-w-2xl">
      <PageHeader title="Settings" />
      {actionError && <ErrorBanner message={actionError} />}
      {error && <ErrorBanner message={error} />}
      {loading && <Spinner />}
      {profile && (
        <div className={`${cardClass} p-6`}>
          <Field label="Display name">
            <input
              className={inputClass}
              value={form.displayName}
              onChange={(e) => setForm({ ...form, displayName: e.target.value })}
            />
          </Field>
          <div className="grid grid-cols-2 gap-3">
            <Field label="Currency">
              <input
                className={inputClass}
                value={form.currency}
                maxLength={10}
                onChange={(e) => setForm({ ...form, currency: e.target.value })}
              />
            </Field>
            <Field label="Week starts on">
              <select
                className={inputClass}
                value={form.weekStartsOn}
                onChange={(e) => setForm({ ...form, weekStartsOn: e.target.value })}
              >
                {dayNames.map((day, index) => (
                  <option key={day} value={index}>{day}</option>
                ))}
              </select>
            </Field>
          </div>
          <Field label="Theme">
            <select
              className={inputClass}
              value={form.theme}
              onChange={(e) => setForm({ ...form, theme: e.target.value })}
            >
              <option value="system">System</option>
              <option value="light">Light</option>
              <option value="dark">Dark</option>
            </select>
          </Field>
          <div className="mt-4 flex items-center gap-3">
            <button className={buttonPrimary} onClick={save} disabled={saving}>
              {saving ? 'Saving…' : 'Save'}
            </button>
            {saved && (
              <span className="flex items-center gap-1.5 text-sm text-emerald-600">
                <Check size={15} /> Saved
              </span>
            )}
          </div>
        </div>
      )}
      <ModulesEditor />
    </div>
  )
}
