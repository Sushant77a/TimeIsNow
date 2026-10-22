import { useMemo, useState } from 'react'
import type { Practice, PracticeLog, PracticeStats } from '../types'

export interface DayCell {
  date: string
  completed: number
}

export function buildDayCells(logs: PracticeLog[], days: number): DayCell[] {
  const byDate = new Map<string, number>()
  for (const log of logs) {
    if (!log.isCompleted) continue
    byDate.set(log.date, (byDate.get(log.date) ?? 0) + 1)
  }

  const cells: DayCell[] = []
  const today = new Date()
  for (let i = days - 1; i >= 0; i--) {
    const d = new Date(today)
    d.setDate(today.getDate() - i)
    const date = d.toISOString().slice(0, 10)
    cells.push({ date, completed: byDate.get(date) ?? 0 })
  }
  return cells
}

function fmtShort(date: string) {
  return new Date(date + 'T00:00:00').toLocaleDateString(undefined, {
    month: 'short',
    day: 'numeric',
  })
}

interface ChartProps {
  practice: Practice
  logs: PracticeLog[]
  stats: PracticeStats
}

const RANGES = [14, 30, 90]

export default function PracticeChart({ practice, logs, stats }: ChartProps) {
  const [range, setRange] = useState(14)
  const perDay = Math.max(practice.sessionsPerDay, 1)

  const cells = useMemo(() => buildDayCells(logs, range), [logs, range])

  const width = 720
  const height = 180
  const padLeft = 8
  const padRight = 8
  const padTop = 12
  const padBottom = 24
  const plotW = width - padLeft - padRight
  const plotH = height - padTop - padBottom
  const slot = plotW / cells.length
  const barW = Math.max(2, Math.min(slot * 0.7, 22))

  const total = cells.reduce((sum, cell) => sum + Math.min(cell.completed, perDay), 0)
  const expected = cells.length * perDay
  const rate = expected > 0 ? total / expected : 0

  const linePoints = cells.map((_, i) => {
    const expectedSoFar = (i + 1) * perDay
    const completedSoFar = cells
      .slice(0, i + 1)
      .reduce((sum, cell) => sum + Math.min(cell.completed, perDay), 0)
    const rateSoFar = expectedSoFar > 0 ? completedSoFar / expectedSoFar : 0
    const x = padLeft + slot * i + slot / 2
    const y = padTop + plotH - rateSoFar * plotH
    return `${x.toFixed(1)},${y.toFixed(1)}`
  })

  const gridLines = [0, 0.5, 1]

  return (
    <div className="mt-4 rounded-xl border border-hairline bg-canvas/40 p-3" data-testid="practice-chart">
      <div className="mb-2 flex flex-wrap items-center justify-between gap-2">
        <div className="flex flex-wrap items-center gap-1.5 text-xs">
          <Chip label="Streak" value={`${stats.currentStreak}d`} accent="text-amber-600" />
          <Chip label="Best" value={`${stats.bestStreak}d`} accent="text-emerald-600" />
          <Chip label="Done" value={`${stats.totalCompleted}`} />
          <Chip label="Rate" value={`${Math.round(stats.completionRate * 100)}%`} accent="text-accent-400" />
          <Chip label="Day" value={`${stats.dayNumber}/${practice.durationDays}`} />
        </div>
        <div className="flex items-center gap-1">
          {RANGES.map((r) => (
            <button
              key={r}
              type="button"
              data-testid={`practice-range-${r}`}
              onClick={() => setRange(r)}
              className={`rounded-md px-2 py-0.5 text-xs font-medium transition ${
                range === r
                  ? 'bg-accent-500/15 text-accent-300'
                  : 'text-gray-500 hover:bg-hairline hover:text-gray-700'
              }`}
            >
              {r}d
            </button>
          ))}
        </div>
      </div>

      <div className="flex items-center gap-3 text-[10px] text-gray-500">
        <span className="flex items-center gap-1">
          <span className="h-2 w-2 rounded-sm bg-accent-400/70" /> sessions
        </span>
        <span className="flex items-center gap-1">
          <span className="h-0.5 w-3 rounded-full bg-emerald-500" /> cumulative rate
        </span>
      </div>

      <svg viewBox={`0 0 ${width} ${height}`} className="mt-1 w-full" role="img" aria-label="Practice progress">
        {gridLines.map((g) => (
          <line
            key={g}
            x1={padLeft}
            x2={width - padRight}
            y1={padTop + plotH - g * plotH}
            y2={padTop + plotH - g * plotH}
            className="stroke-hairline"
            strokeWidth={1}
          />
        ))}

        {cells.map((cell, i) => {
          const filled = Math.min(cell.completed, perDay)
          const h = perDay > 0 ? (filled / perDay) * plotH : 0
          const x = padLeft + slot * i + (slot - barW) / 2
          const y = padTop + plotH - h
          return (
            <rect
              key={cell.date}
              x={x}
              y={y}
              width={barW}
              height={Math.max(h, filled > 0 ? 2 : 0)}
              rx={1.5}
              className={filled >= perDay ? 'fill-emerald-500' : 'fill-accent-400/70'}
            >
              <title>{`${fmtShort(cell.date)}: ${cell.completed}/${perDay}`}</title>
            </rect>
          )
        })}

        <polyline
          points={linePoints.join(' ')}
          fill="none"
          className="stroke-emerald-500"
          strokeWidth={1.75}
          strokeLinejoin="round"
        />

        <text x={padLeft} y={height - 6} className="fill-gray-400 text-[9px]">
          {fmtShort(cells[0]?.date ?? '')}
        </text>
        <text x={width - padRight} y={height - 6} textAnchor="end" className="fill-gray-400 text-[9px]">
          today
        </text>
      </svg>

      <div className="mt-2">
        <p className="mb-1 text-[10px] uppercase tracking-wide text-gray-500">
          Heatmap ({Math.round(rate * 100)}% of the last {range} days)
        </p>
        <div className="flex flex-wrap gap-[3px]">
          {cells.map((cell) => {
            const filled = Math.min(cell.completed, perDay)
            const tone =
              filled === 0
                ? 'bg-hairline'
                : filled >= perDay
                  ? 'bg-emerald-500'
                  : 'bg-accent-400/50'
            return (
              <span
                key={cell.date}
                title={`${fmtShort(cell.date)}: ${cell.completed}/${perDay}`}
                className={`h-3.5 w-3.5 rounded-sm ${tone}`}
              />
            )
          })}
        </div>
      </div>
    </div>
  )
}

function Chip({ label, value, accent }: { label: string; value: string; accent?: string }) {
  return (
    <span className="inline-flex items-center gap-1 rounded-md border border-hairline px-2 py-0.5">
      <span className="text-gray-500">{label}</span>
      <span className={`font-semibold ${accent ?? 'text-gray-800'}`}>{value}</span>
    </span>
  )
}
