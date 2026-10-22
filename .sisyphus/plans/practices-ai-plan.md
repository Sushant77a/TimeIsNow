# Practices — AI-generated practice plans with daily logs and graphs

## Goal

You say "I want to start chakra balancing practices" → the app generates a plan
(overview + N sessions per day, each with instructions and duration) → the plan
lives under a new **Practices** sidebar module with its own daily log → you tick
each session off after doing it IRL → a clean chart shows how the practice is
going.

Follows the existing Habits vertical end-to-end (entity → DbContext → repo →
service → controller → React page) and reuses the existing AI chat client used by
the weekly briefing (`Ai:Provider/ApiKey/Model` secrets are already set).

## Domain (`src/TimeIsNow.Domain`)

`Enums/Enums.cs` — add:

```csharp
public enum PracticeStatus : byte { Active = 0, Paused = 1, Completed = 2, Abandoned = 3 }
public enum ModuleKey : byte { … Practices = 10 }   // append only
```

New entities:

| File | Shape |
|---|---|
| `Entities/Practice.cs` | `BaseEntity, ICategoryEntity` — `Title` (200), `Overview?`, `StartDate` (`DateOnly`), `DurationDays` (int), `SessionsPerDay` (int), `Status` (`PracticeStatus`), `PlanSource` ("ai"/"template", 20); nav `Sessions`, `Logs` |
| `Entities/PracticeSession.cs` | `BaseEntity` — `PracticeId` FK cascade, `Order` (1..SessionsPerDay), `Name` (200), `TimeOfDay?` (30: Morning/Midday/Evening/Anytime), `DurationMinutes?`, `Instructions` (required); unique `(PracticeId, Order)` |
| `Entities/PracticeLog.cs` | `IUserOwned` (same style as `HabitLog`) — `PracticeId` FK cascade, `Date` (`DateOnly`), `SessionOrder` (int), `IsCompleted`, `Note?` (500), `CompletedAt?`, `CreatedAt`; unique `(PracticeId, Date, SessionOrder)` |

Sessions are *templates repeated every day*; the log records
`(practice, date, sessionOrder)` so editing/deleting a template never corrupts history.

## DbContext + migration (`src/TimeIsNow.Infrastructure`)

- `Data/TimeIsNowDbContext.cs`: 3 `DbSet`s; fluent config for `Practice`
  (`ConfigureBase` + `ConfigureCategoryRef`, index `(CategoryId, Status)`),
  `PracticeSession` (ConfigureBase, cascade, unique `(PracticeId, Order)`),
  `PracticeLog` (unique `(PracticeId, Date, SessionOrder)`, `UserId` index,
  `CreatedAt` default). `DateOnly` conversion + `UserId` global filters are
  already handled generically.
- `dotnet ef migrations add AddPractices` → `dotnet ef database update`.

## Application (`src/TimeIsNow.Application`)

- `Common/Constants/ApiMessages.cs` — Practices/PracticeLogs messages (same wording style as Habits).
- `Common/Constants/ModuleKeys.cs` — `[ModuleKey.Practices] = "practices"`.
- `Features/Practices/PracticesRequests.cs`
  - `PracticeListQuery { CategoryId?, Status? }`
  - `PracticeSessionInput { Order, Name, TimeOfDay?, DurationMinutes?, Instructions }`
  - `PracticeUpsertRequest { CategoryId, Title, Overview?, StartDate, DurationDays (1–365), SessionsPerDay (1–6), Status?, Sessions (≥1) }`
  - `PracticeLogRequest { Date, SessionOrder, IsCompleted = true, Note? }`
  - `PracticeLogListQuery { From?, To? }`
  - `PracticePlanRequest { Prompt (required), CategoryId?, StartDate?, SessionsPerDay?, Days? }`
- `Features/Practices/PracticesResponses.cs`
  - `PracticeDto` (incl. `Sessions` + `Stats`), `PracticeSessionDto`, `PracticeLogDto`
  - `PracticeStatsDto { TodayCompleted, TodayTotal, TotalCompleted, ExpectedSessions, CompletionRate, CurrentStreak, BestStreak, LastLogDate?, DayNumber }`
    - `ExpectedSessions = clamp(today − StartDate + 1, 0, DurationDays) × SessionsPerDay`
    - `CurrentStreak`/`BestStreak` = consecutive *perfect* days (all sessions done)
  - `PracticePlanDraftDto { Title, Overview, SessionsPerDay, DurationDays, Sessions[], Source ("ai"|"template"), Provider? }`
- `Interfaces/Services/IPracticesService.cs`, `IPracticePlanGenerator.cs`
- `Interfaces/Repositories/IPracticeRepository.cs`
- `Services/PracticesService.cs` — list (with stats), get, create, update (rewrites session templates), delete (removes sessions + logs), logs get/upsert/delete; validates `Sessions.Count ≤ SessionsPerDay` and re-numbers `Order`.
- `Services/TemplatePlanGenerator.cs` — deterministic fallback: title-cases the prompt, 3 sessions (Morning/Midday/Evening, 10 min) with generic-but-real instructions, 21 days. Also the fallback when the AI call fails.
- `DependencyInjectionExtensions.cs` — register `IPracticesService`, `TemplatePlanGenerator`.

## Infrastructure (`src/TimeIsNow.Infrastructure`)

- `Repositories/PracticeRepository.cs` (mirrors `HabitRepository`, plus sessions/logs).
- `Ai/AiPlanGenerator.cs` — `IPracticePlanGenerator`; strict system prompt ("return ONLY JSON:
  `{ title, overview, sessionsPerDay, durationDays, sessions:[{order,name,timeOfDay,durationMinutes,instructions}] }`,
  2–6 concrete sessions, no markdown"), user prompt carries your request + hints.
  Parse tolerantly (strip ``` fences, first `{`…last `}`), validate/clamp, and on
  *any* failure log a warning and fall back to `TemplatePlanGenerator` — the
  feature never breaks. Reports `Source`/`Provider`.
- `Ai/IAiChatClient.cs` — add optional `MaxTokens` to `AiChatRequest` and honor it
  in `OpenAiChatClient`/`AnthropicChatClient` (fall back to `Ai:MaxTokens`);
  plan generation needs ~2000 tokens so Anthropic's 1024 default can't truncate it.
- `Repositories/DashboardRepository.cs` — add `practicesToday` to `GetTodayAsync`
  (active practices + completed count for the date).
- `Startup/DependencyInjectionExtensions.cs` — register `IPracticeRepository`;
  `IPracticePlanGenerator` → `AiPlanGenerator` when `Ai:ApiKey` is set, else
  `TemplatePlanGenerator`.

## API (`src/TimeIsNow.API`)

- `Controllers/PracticesController.cs` (`api/practices`), same envelope/ conventions as Habits:

| Method | Route | Notes |
|---|---|---|
| GET | `` `?categoryId&status` `` | list with stats |
| GET | `/{id}` | detail (sessions + stats) |
| POST | `` | create from a draft |
| PUT | `/{id}` | update (incl. status Pause/Resume/Complete) |
| DELETE | `/{id}` | cascades sessions + logs |
| GET | `/{id}/logs?from&to` | |
| POST | `/{id}/logs` | upsert by `(date, sessionOrder)` |
| DELETE | `/logs/{logId}` | |
| POST | `/plan` | generate a draft (not persisted), `{ prompt, categoryId?, startDate?, sessionsPerDay?, days? }` |

- `Mcp/TimeIsNowMcpTools.cs` — 2 new tools: `list_practices` (practices + today's progress),
  `log_practice` (`practiceId`, `date`, `sessionOrder`, `completed`, `note`).

## Client (`client/`)

- `src/types/index.ts` — `Practice`, `PracticeSession`, `PracticeLog`, `PracticeStats`,
  `PracticePlanDraft`, `TodayPractice`; `TodayResponse.practicesToday`.
- `src/api/resources.ts` — `practicesApi` (`getList, getById, create, update, remove,
  getLogs, upsertLog, removeLog, generatePlan`).
- `src/components/icons.tsx` — nav item `{ key: 'practices', to: '/practices', label: 'Practices', Icon }`.
- `src/lib/modules.ts` — add `'practices'` to `DEFAULT_ALL_MODULES`,
  `DEFAULT_CATEGORY_MODULES`, `PERSONAL_MODULES` (Settings toggles pick it up
  automatically since they derive from `navItems`).
- `src/App.tsx` — route `practices`.
- `src/pages/Practices.tsx`
  - Header + status filter + **New practice**
  - **New practice modal, 2 steps**
    1. Prompt: "What do you want to practice?" (e.g. *chakra balancing*), category,
       start date, sessions/day, plan length → **Generate plan** (AI) or **Create manually** (template skeleton)
    2. Preview/edit: title, overview, per-session name / time of day / minutes /
       instructions, add & remove sessions, **Regenerate**, **Start practice** (save)
  - Cards: title, status badge, category dot, today's session dots (tap = log/unlog),
     `x/y today`, streak flame, completion %, mini 7-day dots, expand for detail
  - Detail: overview, each session with full instructions + today's checkbox,
     status controls (Pause/Resume/Mark complete), recent log entries, delete
  - Expanded detail hosts the graph panel.
- `src/components/PracticeChart.tsx` — hand-rolled SVG (no new dependency):
  range toggle 14/30/90 days; **bars** = sessions completed per day (vs sessionsPerDay),
  **line** = cumulative completion rate, **heatmap strip** underneath (day cells
  shaded by fraction of sessions done), stat chips (current streak, best streak,
  completion %, total sessions, day X of N).
- `src/pages/Dashboard.tsx` — "Practices today" card: each active practice with
  tappable session dots (logs through `practicesApi.upsertLog`) + link to `/practices`.
- `tests/practices.spec.ts` — E2E: manual builder creates a 2-session practice →
  tick sessions → persists after reload; API contract for `POST /api/practices/plan`
  (`source` ∈ ai|template, ≥1 session); Dashboard card ticks a session.

## Docs

Update `PROJECT.md` (core-concept tree, tables 17→20, API table, MCP tools 9→11,
module keys, roadmap) and `README.md` feature list.

## Order of work

Domain → DbContext → migration → Application → Infrastructure (repo/AI/dashboard) →
Controller + MCP → client (types/api/nav → Practices page → chart → Dashboard card) →
builds, migration, tests, docs.

## Verification

1. `dotnet build TimeIsNow.slnx`
2. `dotnet ef migrations add AddPractices --project src/TimeIsNow.Infrastructure --startup-project src/TimeIsNow.API`
   then `dotnet ef database update …`
3. `npm --prefix client run build` and `npm --prefix client run lint`
4. `npm --prefix client test` (Playwright reuses/starts servers; existing specs must stay green —
   navigation/modules specs are not exhaustive and `mcp.spec` uses `arrayContaining`)
5. Manual: `.\dev.ps1` → Practices → "chakra balancing" → AI plan → tick sessions →
   check chart, heatmap, and the Dashboard card.

## Notes / risks

- AI outage or bad key → automatic template plan (`source: "template"`), so the flow always works.
- If a user already customized their module toggles, `practices` won't appear in
  that scope until enabled in Settings (defaults only apply when nothing is saved).
- Enums are stored as tinyint; `Practices = 10` is append-only so existing rows are unaffected.
