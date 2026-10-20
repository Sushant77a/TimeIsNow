import type { ReactNode } from 'react'
import { Hourglass } from 'lucide-react'

export function AuthShell({ subtitle, children }: { subtitle: string; children: ReactNode }) {
  return (
    <div className="flex min-h-screen items-center justify-center px-4">
      <div className="tin-pop w-full max-w-sm rounded-2xl border border-hairline-strong bg-surface/90 p-8 shadow-e3 backdrop-blur-xl">
        <div className="mb-6 flex items-center gap-3">
          <span className="flex h-10 w-10 items-center justify-center rounded-xl bg-accent-50 text-accent ring-1 ring-inset ring-hairline-strong">
            <Hourglass size={19} />
          </span>
          <div>
            <h1 className="text-lg font-bold leading-tight tracking-tight">
              <span className="text-ink">
                Time is Now
              </span>
            </h1>
            <p className="text-xs leading-tight text-gray-500">{subtitle}</p>
          </div>
        </div>
        {children}
      </div>
    </div>
  )
}
