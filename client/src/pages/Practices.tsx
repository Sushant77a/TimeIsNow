import { useEffect, useState } from 'react'
import { practicesApi } from '../api/resources'
import { useCategories } from '../context/CategoryContext'
import { useAsync } from '../hooks/useAsync'
import PracticeChart from '../components/PracticeChart'
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
import { Check, Pencil, Plus, Sparkles, Trash2 } from '../components/icons'
import type { Practice, PracticeLog, PracticePlanDraft, PracticeStatus } from '../types'
import { todayIso } from '../lib/format'

const TIME_OF_DAY_OPTIONS = ['', 'Morning', 'Midday', 'Afternoon', 'Evening', 'Night', 'Anytime']

interface DraftSession {
  name: string
  timeOfDay: string
  durationMinutes: string
  instructions: string
}

interface DraftForm {
  categoryId: number | null
  title: string
  overview: string
  startDate: string
  durationDays: number
  sessions: DraftSession[]
}

function makeManualSession(index: number): DraftSession {
  return {
    name: `Session ${index + 1}`,
    timeOfDay: '',
    durationMinutes: '10',
    instructions: '',
  }
}

function emptyDraft(categoryId: number | null): DraftForm {
  return {
    categoryId,
    title: '',
    overview: '',
    startDate: todayIso(),
    durationDays: 21,
    sessions: [makeManualSession(0)],
  }
}

function draftFromPlan(plan: PracticePlanDraft, categoryId: number | null): DraftForm {
  return {
    categoryId,
    title: plan.title,
    overview: plan.overview ?? '',
    startDate: plan.startDate,
    durationDays: plan.durationDays,
    sessions: plan.sessions.map((session) => ({
      name: session.name,
      timeOfDay: session.timeOfDay ?? '',
      durationMinutes: session.durationMinutes ? String(session.durationMinutes) : '',
      instructions: session.instructions,
    })),
  }
}

function statusTone(status: PracticeStatus): string {
  switch (status) {
    case 'Active':
      return 'bg-emerald-100 text-emerald-700'
    case 'Paused':
      return 'bg-amber-100 text-amber-700'
    case 'Completed':
      return 'bg-accent-50 text-accent-700'
    default:
      return 'bg-gray-100 text-gray-600'
  }
}

export default function Practices() {
  const { categories, selectedCategoryId } = useCategories()
  const [status, setStatus] = useState<PracticeStatus | 'All'>('Active')
  const [expanded, setExpanded] = useState<number | null>(null)
  const [wizardOpen, setWizardOpen] = useState(false)
  const [step, setStep] = useState<'prompt' | 'review'>('prompt')
  const [prompt, setPrompt] = useState('')
  const [promptSessions, setPromptSessions] = useState(3)
  const [promptDays, setPromptDays] = useState(21)
  const [promptCategory, setPromptCategory] = useState<number | null>(null)
  const [planSource, setPlanSource] = useState<'ai' | 'template' | null>(null)
  const [draft, setDraft] = useState<DraftForm | null>(null)
  const [generating, setGenerating] = useState(false)
  const [saving, setSaving] = useState(false)
  const [actionError, setActionError] = useState<string | null>(null)

  const { data, error, loading, reload } = useAsync(
    () =>
      practicesApi.getList({
        categoryId: selectedCategoryId ?? undefined,
        status: status === 'All' ? undefined : status,
      }),
    [selectedCategoryId, status],
  )

  async function toggleSession(practice: Practice, order: number, completed: boolean) {
    await practicesApi.upsertLog(practice.id, {
      date: todayIso(),
      sessionOrder: order,
      isCompleted: !completed,
    })
    reload()
  }

  async function setStatusFor(practice: Practice, next: PracticeStatus) {
    await practicesApi.update(practice.id, {
      categoryId: practice.categoryId,
      title: practice.title,
      overview: practice.overview,
      startDate: practice.startDate,
      durationDays: practice.durationDays,
      status: next,
      sessions: practice.sessions.map((session) => ({
        name: session.name,
        timeOfDay: session.timeOfDay,
        durationMinutes: session.durationMinutes,
        instructions: session.instructions,
      })),
    })
    reload()
  }

  async function remove(id: number) {
    if (!confirm('Delete this practice and its whole log history?')) return
    await practicesApi.remove(id)
    reload()
  }

  function openWizard() {
    setStep('prompt')
    setPrompt('')
    setPromptSessions(3)
    setPromptDays(21)
    setPromptCategory(selectedCategoryId ?? categories[0]?.id ?? null)
    setDraft(null)
    setPlanSource(null)
    setActionError(null)
    setWizardOpen(true)
  }

  async function generatePlan() {
    if (!prompt.trim()) return
    setGenerating(true)
    setActionError(null)
    try {
      const plan = await practicesApi.generatePlan({
        prompt: prompt.trim(),
        categoryId: promptCategory ?? undefined,
        sessionsPerDay: promptSessions,
        days: promptDays,
      })
      setDraft(draftFromPlan(plan, promptCategory))
      setPlanSource(plan.source)
      setStep('review')
    } catch (err) {
      setActionError((err as Error).message)
    } finally {
      setGenerating(false)
    }
  }

  function startManual() {
    setDraft(emptyDraft(promptCategory))
    setPlanSource(null)
    setStep('review')
  }

  function updateSession(index: number, patch: Partial<DraftSession>) {
    if (!draft) return
    setDraft({
      ...draft,
      sessions: draft.sessions.map((session, i) => (i === index ? { ...session, ...patch } : session)),
    })
  }

  function addSession() {
    if (!draft) return
    setDraft({ ...draft, sessions: [...draft.sessions, makeManualSession(draft.sessions.length)] })
  }

  function removeSession(index: number) {
    if (!draft || draft.sessions.length <= 1) return
    setDraft({ ...draft, sessions: draft.sessions.filter((_, i) => i !== index) })
  }

  async function saveDraft() {
    if (!draft || !draft.title.trim() || !draft.categoryId) return
    setSaving(true)
    setActionError(null)
    try {
      await practicesApi.create({
        categoryId: draft.categoryId,
        title: draft.title.trim(),
        overview: draft.overview.trim() || null,
        startDate: draft.startDate,
        durationDays: draft.durationDays,
        status: 'Active',
        sessions: draft.sessions.map((session) => ({
          name: session.name.trim(),
          timeOfDay: session.timeOfDay || null,
          durationMinutes: session.durationMinutes ? Number(session.durationMinutes) : null,
          instructions: session.instructions.trim(),
        })),
      })
      setWizardOpen(false)
      setDraft(null)
      reload()
    } catch (err) {
      setActionError((err as Error).message)
    } finally {
      setSaving(false)
    }
  }

  const practices = data ?? []

  return (
    <div>
      <PageHeader
        title="Practices"
        subtitle="Multi-day practice plans with a session log and progress graph"
        actions={
          <>
            <select
              value={status}
              onChange={(e) => setStatus(e.target.value as PracticeStatus | 'All')}
              className={`${inputClass} w-auto`}
            >
              <option value="Active">Active</option>
              <option value="Paused">Paused</option>
              <option value="Completed">Completed</option>
              <option value="Abandoned">Abandoned</option>
              <option value="All">All</option>
            </select>
            <button onClick={openWizard} className={buttonPrimary}>
              <Plus size={15} /> New practice
            </button>
          </>
        }
      />
      {actionError && !wizardOpen && <ErrorBanner message={actionError} />}
      {error && <ErrorBanner message={error} />}
      {loading && <Spinner />}
      {!loading && practices.length === 0 && (
        <EmptyState
          icon={Sparkles}
          message='No practices yet. Try "chakra balancing" and let AI draft a plan.'
        />
      )}

      {practices.map((practice) => (
        <PracticeCard
          key={practice.id}
          practice={practice}
          categoryName={categories.find((c) => c.id === practice.categoryId)}
          expanded={expanded === practice.id}
          onToggleExpand={() => setExpanded(expanded === practice.id ? null : practice.id)}
          onToggleSession={toggleSession}
          onStatus={setStatusFor}
          onDelete={remove}
        />
      ))}

      {wizardOpen && (
        <Modal
          title={step === 'prompt' ? 'New practice' : 'Review plan'}
          onClose={() => setWizardOpen(false)}
        >
          {actionError && <ErrorBanner message={actionError} />}

          {step === 'prompt' && (
            <>
              <Field label="What do you want to practice?">
                <input
                  className={inputClass}
                  value={prompt}
                  onChange={(e) => setPrompt(e.target.value)}
                  placeholder="e.g. chakra balancing"
                  autoFocus
                />
              </Field>
              <Field label="Category">
                <CategorySelect
                  categories={categories}
                  value={promptCategory}
                  onChange={setPromptCategory}
                  allowAll={false}
                />
              </Field>
              <div className="grid grid-cols-2 gap-3">
                <Field label="Sessions per day">
                  <input
                    type="number"
                    min={1}
                    max={6}
                    className={inputClass}
                    value={promptSessions}
                    onChange={(e) => setPromptSessions(Math.max(1, Math.min(6, Number(e.target.value) || 1)))}
                  />
                </Field>
                <Field label="Plan length (days)">
                  <input
                    type="number"
                    min={1}
                    max={365}
                    className={inputClass}
                    value={promptDays}
                    onChange={(e) => setPromptDays(Math.max(1, Math.min(365, Number(e.target.value) || 1)))}
                  />
                </Field>
              </div>
              <div className="mt-4 flex justify-between gap-2">
                <button className={buttonSecondary} onClick={startManual}>
                  Create manually
                </button>
                <button
                  className={buttonPrimary}
                  onClick={generatePlan}
                  disabled={generating || !prompt.trim()}
                >
                  <Sparkles size={15} /> {generating ? 'Generating…' : 'Generate plan'}
                </button>
              </div>
            </>
          )}

          {step === 'review' && draft && (
            <>
              <div className="mb-3 flex items-center gap-2">
                <Badge
                  text={planSource === 'ai' ? 'AI plan' : planSource === 'template' ? 'Template plan' : 'Manual'}
                  className={planSource ? 'bg-accent-50 text-accent-700' : 'bg-gray-100 text-gray-600'}
                />
                <button
                  className="text-xs font-medium text-accent-400 hover:text-accent-300"
                  onClick={() => setStep('prompt')}
                >
                  Back
                </button>
              </div>
              <Field label="Title">
                <input
                  className={inputClass}
                  value={draft.title}
                  onChange={(e) => setDraft({ ...draft, title: e.target.value })}
                />
              </Field>
              <Field label="Overview">
                <textarea
                  className={inputClass}
                  rows={3}
                  value={draft.overview}
                  onChange={(e) => setDraft({ ...draft, overview: e.target.value })}
                />
              </Field>
              <div className="grid grid-cols-3 gap-3">
                <Field label="Category">
                  <CategorySelect
                    categories={categories}
                    value={draft.categoryId}
                    onChange={(id) => setDraft({ ...draft, categoryId: id })}
                    allowAll={false}
                  />
                </Field>
                <Field label="Start date">
                  <input
                    type="date"
                    className={inputClass}
                    value={draft.startDate}
                    onChange={(e) => setDraft({ ...draft, startDate: e.target.value })}
                  />
                </Field>
                <Field label="Days">
                  <input
                    type="number"
                    min={1}
                    max={365}
                    className={inputClass}
                    value={draft.durationDays}
                    onChange={(e) =>
                      setDraft({ ...draft, durationDays: Math.max(1, Number(e.target.value) || 1) })
                    }
                  />
                </Field>
              </div>

              <p className="mt-3 mb-1 text-xs font-medium text-gray-600">
                Daily sessions ({draft.sessions.length})
              </p>
              <div className="space-y-3">
                {draft.sessions.map((session, index) => (
                  <div key={index} className="rounded-lg border border-hairline bg-canvas/40 p-3">
                    <div className="flex items-center gap-2">
                      <span className="text-xs font-semibold text-gray-500">#{index + 1}</span>
                      <input
                        className={`${inputClass} flex-1`}
                        value={session.name}
                        placeholder="Session name"
                        onChange={(e) => updateSession(index, { name: e.target.value })}
                      />
                      <IconButton
                        icon={Trash2}
                        label="Remove session"
                        tone="danger"
                        onClick={() => removeSession(index)}
                      />
                    </div>
                    <div className="mt-2 grid grid-cols-2 gap-2">
                      <select
                        className={inputClass}
                        value={session.timeOfDay}
                        onChange={(e) => updateSession(index, { timeOfDay: e.target.value })}
                      >
                        {TIME_OF_DAY_OPTIONS.map((option) => (
                          <option key={option} value={option}>
                            {option || 'Any time'}
                          </option>
                        ))}
                      </select>
                      <input
                        type="number"
                        min={1}
                        max={600}
                        className={inputClass}
                        placeholder="Minutes"
                        value={session.durationMinutes}
                        onChange={(e) => updateSession(index, { durationMinutes: e.target.value })}
                      />
                    </div>
                    <textarea
                      className={`${inputClass} mt-2`}
                      rows={3}
                      placeholder="What to do in this session"
                      value={session.instructions}
                      onChange={(e) => updateSession(index, { instructions: e.target.value })}
                    />
                  </div>
                ))}
              </div>

              <button className={`${buttonSecondary} mt-3`} onClick={addSession}>
                <Plus size={14} /> Add session
              </button>

              <div className="mt-4 flex justify-end gap-2">
                <button className={buttonSecondary} onClick={() => setWizardOpen(false)}>
                  Cancel
                </button>
                <button
                  className={buttonPrimary}
                  onClick={saveDraft}
                  disabled={
                    saving ||
                    !draft.title.trim() ||
                    !draft.categoryId ||
                    draft.sessions.some((s) => !s.name.trim() || !s.instructions.trim())
                  }
                >
                  {saving ? 'Saving…' : 'Start practice'}
                </button>
              </div>
            </>
          )}
        </Modal>
      )}
    </div>
  )
}

function PracticeCard({
  practice,
  categoryName,
  expanded,
  onToggleExpand,
  onToggleSession,
  onStatus,
  onDelete,
}: {
  practice: Practice
  categoryName: { name: string; color: string } | undefined
  expanded: boolean
  onToggleExpand: () => void
  onToggleSession: (practice: Practice, order: number, completed: boolean) => void
  onStatus: (practice: Practice, status: PracticeStatus) => void
  onDelete: (id: number) => void
}) {
  const [logs, setLogs] = useState<PracticeLog[]>([])
  const [loadingLogs, setLoadingLogs] = useState(false)
  const { stats } = practice
  const today = todayIso()
  const todayLogs = logs.filter((log) => log.date === today && log.isCompleted)
  const completedOrders = new Set(todayLogs.map((log) => log.sessionOrder))

  useEffect(() => {
    if (!expanded) return
    let cancelled = false
    setLoadingLogs(true)
    practicesApi
      .getLogs(practice.id)
      .then((result) => {
        if (!cancelled) setLogs(result)
      })
      .catch(() => {
        if (!cancelled) setLogs([])
      })
      .finally(() => {
        if (!cancelled) setLoadingLogs(false)
      })
    return () => {
      cancelled = true
    }
  }, [expanded, practice.id, practice.stats.totalCompleted])

  return (
    <div className={`mb-3 ${cardClass} px-4 py-3`} data-testid="practice-card">
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <div className="flex flex-wrap items-center gap-2">
            <span className="text-sm font-medium text-gray-900">{practice.title}</span>
            <Badge text={practice.status} className={statusTone(practice.status)} />
            <Badge text={practice.planSource === 'ai' ? 'AI plan' : 'Manual'} />
            <CategoryDot category={categoryName as never} />
          </div>
          <p className="mt-1 text-xs text-gray-500">
            Day {stats.dayNumber}/{practice.durationDays} · {stats.todayCompleted}/{stats.todayTotal} today ·{' '}
            {Math.round(stats.completionRate * 100)}% complete
            {stats.currentStreak > 0 && ` · ${stats.currentStreak}-day streak`}
          </p>
        </div>
        <div className="flex shrink-0 items-center gap-1">
          <button
            onClick={onToggleExpand}
            className={expanded ? buttonSecondary : buttonPrimary}
            data-testid="practice-expand"
          >
            {expanded ? 'Hide log' : 'Open log'}
          </button>
          <IconButton icon={Pencil} label="Pause practice" onClick={() => onStatus(practice, practice.status === 'Paused' ? 'Active' : 'Paused')} />
          <IconButton icon={Trash2} label="Delete practice" tone="danger" onClick={() => onDelete(practice.id)} />
        </div>
      </div>

      <div className="mt-3 flex flex-wrap gap-3">
        {practice.sessions.map((session) => {
          const done = completedOrders.has(session.order)
          return (
            <button
              key={session.id}
              data-testid="practice-session-toggle"
              onClick={() => onToggleSession(practice, session.order, done)}
              title={session.name}
              className={`flex items-center gap-1.5 rounded-lg border px-2.5 py-1 text-xs font-medium transition ${
                done
                  ? 'border-emerald-300 bg-emerald-50 text-emerald-700'
                  : 'border-hairline-strong text-gray-500 hover:border-hairline-strong hover:text-gray-800'
              }`}
            >
              <span className={`flex h-3.5 w-3.5 items-center justify-center rounded-full ${done ? 'bg-emerald-500 text-white' : 'bg-gray-200'}`}>
                {done && <Check size={10} />}
              </span>
              {session.order}. {session.name}
              {session.durationMinutes ? ` (${session.durationMinutes}m)` : ''}
            </button>
          )
        })}
      </div>

      {expanded && (
        <>
          {practice.overview && (
            <p className="mt-3 border-t border-hairline pt-3 text-sm text-gray-600">{practice.overview}</p>
          )}

          <div className="mt-3 space-y-2">
            {practice.sessions.map((session) => (
              <div key={session.id} className="rounded-lg border border-hairline bg-canvas/40 p-3">
                <div className="flex flex-wrap items-center gap-2">
                  <span className="text-xs font-semibold text-gray-700">
                    {session.order}. {session.name}
                  </span>
                  {session.timeOfDay && <Badge text={session.timeOfDay} />}
                  {session.durationMinutes && <Badge text={`${session.durationMinutes} min`} />}
                </div>
                <p className="mt-1 text-sm text-gray-600">{session.instructions}</p>
              </div>
            ))}
          </div>

          {loadingLogs ? (
            <Spinner />
          ) : (
            <PracticeChart practice={practice} logs={logs} stats={stats} />
          )}

          <div className="mt-3 flex flex-wrap items-center justify-between gap-2">
            <div className="flex flex-wrap gap-1.5">
              {(['Active', 'Paused', 'Completed', 'Abandoned'] as PracticeStatus[]).map((next) => (
                <button
                  key={next}
                  onClick={() => onStatus(practice, next)}
                  className={`rounded-full border px-3 py-1 text-xs font-medium transition ${
                    practice.status === next
                      ? 'border-accent-400/40 bg-accent-500/15 text-accent-300'
                      : 'border-hairline text-gray-500 hover:bg-hairline hover:text-gray-700'
                  }`}
                >
                  {next}
                </button>
              ))}
            </div>
            <span className="text-xs text-gray-500">
              Best streak {stats.bestStreak} days · {stats.totalCompleted} sessions logged
            </span>
          </div>
        </>
      )}
    </div>
  )
}
