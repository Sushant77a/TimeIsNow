# Time is Now

> A personal life dashboard and assistant — organized by life domains, everything scoped to a category.

## Vision

One place to track and run your life, split into **categories** (Personal, Study, Work, Leisure, and any you create). Every feature lives *inside* a category, so your todos, work logs, journal, schedule, finances, and notes never mix between life areas.

## Core concept

```
Category (Personal | Study | Work | Leisure | custom…)
   │
   ├── Todos            tasks, priorities, due dates, reminders, recurrence, subtasks
   ├── Daily Entries    work-done log per day (duration + mood + energy)
   ├── Journal          reflections with mood/energy
   ├── Schedule         future things — events, deadlines, plans (link to goals)
   ├── Finances         income/expenses per life area (recurring supported)
   ├── Goals            long-term targets with progress
   ├── Habits           daily/weekly habits with per-day logs (streaks)
   └── Notes            pinned freeform notes with tags
```

## Tech stack

| Layer | Choice |
|---|---|
| API | .NET 10 Web API, controllers, clean architecture (Domain / Application / Infrastructure / API) |
| ORM | EF Core 10 + **Oracle MySQL provider** (`MySql.EntityFrameworkCore` 10.x) |
| Database | MySQL 8 (local service, port 3306) |
| Client | React 19 + TypeScript + Vite, Tailwind CSS v4 |
| Auth | JWT access token (15 min, in-memory) + rotating httpOnly refresh cookie (7 days); multi-user, per-user data scoping |
| Dev setup | Vite dev server (5173) proxies `/api` → API (5099) |

## Project layout

```
project_timeisnow/
├── PROJECT.md
├── TimeIsNow.slnx
└── src/
    ├── TimeIsNow.Domain/          ← entities + enums (no dependencies)
    │   └── Entities/              ←   BaseEntity (+IUserOwned UserId), User, RefreshToken, …
    ├── TimeIsNow.Application/     ← DTOs, service interfaces, repos interfaces,
    │   ├── Common/                ←   ApiResponse envelope, ApiException (401/404/409), ApiMessages
    │   ├── Features/{X}/          ←   {X}Requests.cs + {X}Responses.cs per feature
    │   ├── Interfaces/            ←   I{X}Service, I{X}Repository, IUnitOfWork, Security/ (ICurrentUser, ITokenService, IPasswordHasher)
    │   └── Services/              ←   13 services (business logic + mapping), incl. AuthService
    ├── TimeIsNow.Infrastructure/
    │   ├── Data/                  ← TimeIsNowDbContext (fluent config, global user filters, UserId/UpdatedAt stamping)
    │   ├── Repositories/          ← 14 EF repositories
    │   ├── Security/              ← JwtOptions, JwtTokenService, PasswordHasher (PBKDF2)
    │   └── Startup/               ← AddInfrastructure DI extension
    └── TimeIsNow.API/
        ├── Controllers/           ← 13 controllers (incl. AuthController)
        ├── Mcp/                   ← TimeIsNowMcpTools (9 MCP tools at POST /mcp)
        ├── Security/              ← HttpContextCurrentUser (ICurrentUser impl)
        ├── Middlewares/           ← ApiExceptionHandler (exception → envelope)
        ├── Extensions/            ← ModelValidationExtensions, CorsExtensions
        └── Program.cs             ← auth/scheme, fallback policy (all endpoints require auth)
```

## Database — `timeisnow`

Created via EF Core migration `InitialCreate` (in `TimeIsNow.Infrastructure/Migrations`). Connection string lives in **user secrets** (never in code):

```
dotnet user-secrets set "ConnectionStrings:TimeIsNow" "server=localhost;port=3306;database=timeisnow;user=root;password=..."
```

JWT signing key also lives in user secrets:

```
dotnet user-secrets set "Jwt:SecretKey" "<random 48-byte base64>"
```

### Tables (17)

| Table | Purpose | Key fields |
|---|---|---|
| `User` | accounts | Email (**unique**), PasswordHash (PBKDF2), DisplayName |
| `RefreshToken` | rotating refresh sessions | TokenHash (**unique**, SHA-256), ExpiresAt, RevokedAt, ReplacedByToken → User cascade |
| `UserProfile` | per-user settings (one row per user) | DisplayName, Currency, WeekStartsOn, Theme — **unique UserId** |
| `Category` | life domains (seeded per user: Personal, Study, Work, Leisure) | Name, Color (#hex), Icon, SortOrder, **IsArchived** |
| `Todo` | tasks | Title, Priority, DueDate, ReminderAt, RecurrenceType/Interval, IsDone, CompletedAt, **ParentTodoId** (subtasks) |
| `DailyEntry` | work done per day | Date, Content, DurationMinutes, Mood(1–5), Energy(1–5) |
| `JournalEntry` | journaling | Date, Title, Content, Mood(1–5), Energy(1–5) |
| `ScheduleItem` | future/planned things | ScheduledAt, EndAt, Status (Planned/Done/Skipped/Cancelled), ReminderAt, GoalId |
| `FinanceEntry` | money in/out per area | Type (Expense/Income), Amount decimal(18,2), Date, RecurrenceType |
| `Goal` | long-term targets | TargetDate, Status (Active/Achieved/Abandoned), ProgressPercent |
| `Habit` | habit definitions | Frequency (Daily/Weekly), IsActive |
| `HabitLog` | one row per habit-day | Date, IsCompleted, Note — **unique (HabitId, Date)** |
| `Tag` | normalized tags (per user) | Name — **unique (UserId, Name)** |
| `Note` | freeform notes | Title, Content, IsPinned |
| `NoteTag` / `TodoTag` / `JournalTag` | many-to-many joins | composite PK (entityId, TagId) |

### Schema conventions

- **Every data table** carries `UserId` (multi-user scoping) plus `CategoryId` FK → `Category` (`ON DELETE CASCADE`) where applicable, plus `CreatedAt`, `UpdatedAt`, `IsDeleted`.
- **User scoping**: `UserId` is stamped automatically on insert (`SaveChanges` override via `IUserOwned`); all queries get an **EF global query filter** (`UserId == current user`) applied through the DbContext's `CurrentUserId`. Tag join tables filter transitively through their parent.
- **Soft delete everywhere**: queries filter `IsDeleted == false`. Category uses `IsArchived` to retire without losing data; deleting a category with items returns **409**.
- **Timestamps**: `CreatedAt` DB-managed (`DEFAULT CURRENT_TIMESTAMP(6)`); `UpdatedAt` stamped by the API on every save (`SaveChanges` override) since the Oracle provider doesn't emit `ON UPDATE CURRENT_TIMESTAMP`.
- **Enums** stored as `tinyint unsigned`, serialized as **strings in JSON** (`"High"`, `"Expense"`).
- **`DateOnly` columns**: Oracle's provider can't materialize `DateOnly`, so the DbContext converts it to `DateTime` at the provider boundary while keeping the column type `date`.
- **CHECK constraints**: `Amount > 0`, `Mood/Energy BETWEEN 1 AND 5` (DailyEntry + JournalEntry), `ProgressPercent BETWEEN 0 AND 100`.
- **Delete behaviors**: Category/Habit/Todo → children cascade; Goal → ScheduleItem is `SET NULL`; tag joins cascade both ways; RefreshToken → User cascades.
- **Indexes**: all FKs; hot paths `(CategoryId, IsDone)`, `(CategoryId, Date)` (entries/finance), `(CategoryId, ScheduledAt)`, `(HabitId, Date)` unique, `(UserId, Name)` unique on Tag, `UserId` on all owned tables, `IsDeleted` on all soft-deletable tables.

## Authentication

- **Access token**: JWT (HMAC-SHA256), 15 min, returned in the login/register body, held **in memory** in the client, sent as `Authorization: Bearer`. Claims: `sub` (user id), `email`, `name`.
- **Refresh token**: opaque 64-byte random value delivered as an **httpOnly cookie** `tin_refresh` (`Path=/api/auth`, `SameSite=Lax`, `Secure` in https, 7 days). Only its **SHA-256 hash** is stored (`RefreshToken.TokenHash`).
- **Rotation**: each refresh issues a new access + refresh pair and revokes the old refresh row (`ReplacedByToken`). Re-use of a revoked/expired token revokes the whole family.
- **Client**: `client.ts` keeps the access token in a module variable and, on any non-auth `401`, performs a **single-flight** `POST /api/auth/refresh` then retries the request. `AuthContext` does a silent refresh-on-mount (`GET /api/auth/me`) and clears state on session expiry.
- **Authorization**: an ASP.NET **fallback policy** requires authentication for every endpoint; only `/api/health` and the auth endpoints opt out. `MapInboundClaims = false` keeps the raw `sub`/`email` claim names.
- **Passwords**: PBKDF2 (`Rfc2898DeriveBytes`, SHA-256, 100k iterations, 16-byte salt), stored as `pbkdf2$sha256$iter$salt$hash`.
- **New user bootstrap**: registration seeds the 4 default categories + a `UserProfile` (currency `INR`, week starts Monday) for that user.

## API

Every response is wrapped in an envelope: `{ message, status, statusCode, data }` (`ApiResponse.Success` / `ApiResponse.Fail`). Model-validation failures answer `400` with field errors in `data`; exceptions are mapped by `ApiExceptionHandler` (401/404/409 from services, 500 otherwise). Swagger UI at `/swagger` (Development). **All endpoints require a bearer token** except `/api/health` and the `/api/auth/*` group.

| Endpoint | Methods | Notes |
|---|---|---|
| `/api/health` | GET | liveness (anonymous) |
| `/api/auth/register` | POST | `{ email, password, displayName }` → 201 + access token + refresh cookie; seeds defaults |
| `/api/auth/login` | POST | `{ email, password }` → access token + refresh cookie |
| `/api/auth/refresh` | POST | rotates refresh cookie → new access token (anonymous; cookie-driven) |
| `/api/auth/logout` | POST | revokes refresh token + clears cookie |
| `/api/auth/me` | GET | current user (requires bearer token) |
| `/api/auth/mcp-token` | POST | mints a long-lived (30-day) bearer token for the MCP server |
| `/api/categories` | GET `?includeArchived`, GET `/{id}`, POST, PUT `/{id}`, DELETE `/{id}` | DELETE → 409 if category has active items |
| `/api/profile` | GET, PUT | per-user settings row |
| `/api/todos` | GET `?categoryId&isDone&dueBefore&includeSubtasks`, GET `/{id}`, POST, PUT `/{id}`, DELETE `/{id}` | tags auto-upsert; `isDone` stamps `CompletedAt`; DELETE soft-cascades to subtasks; `GET /{id}` nests `subTasks` |
| `/api/daily-entries` | GET `?categoryId&from&to`, POST, PUT `/{id}`, DELETE `/{id}` | |
| `/api/journal` | GET `?categoryId&from&to&search`, GET `/{id}`, POST, PUT `/{id}`, DELETE `/{id}` | tags auto-upsert |
| `/api/schedule` | GET `?categoryId&from&to&status`, POST, PUT `/{id}`, DELETE `/{id}` | |
| `/api/finance` | GET `?categoryId&from&to&type`, POST, PUT `/{id}`, DELETE `/{id}` | |
| `/api/goals` | GET `?categoryId&status`, POST, PUT `/{id}`, DELETE `/{id}` | |
| `/api/habits` | GET `?categoryId&active`, GET `/{id}`, POST, PUT `/{id}`, DELETE `/{id}` | DELETE removes logs too |
| `/api/habits/{id}/logs` | GET `?from&to`, POST (upsert by date) | one log per (habit, date) |
| `/api/habits/logs/{logId}` | DELETE | |
| `/api/notes` | GET `?categoryId&search&tag&pinned`, GET `/{id}`, POST, PUT `/{id}`, DELETE `/{id}` | tags auto-upsert |
| `/api/tags` | GET, DELETE `/{id}` | |
| `/api/dashboard/today` | GET `?date=yyyy-MM-dd` | overdue + due-today todos, today's schedule/entries, habit completion flags |
| `/api/dashboard/summary` | GET `?year&month` | income/expense/balance, per-category finance, open todos, daily-habit streaks |
| `/api/dashboard/briefing` | GET `?weekStart=yyyy-MM-dd` | weekly stats + narrative; week runs Monday–Sunday (defaults to the current week) |

CORS: `Cors:AllowedOrigins` in appsettings (`http://localhost:5173` for Vite), with `AllowCredentials` so the refresh cookie flows.

## Weekly briefing

`GET /api/dashboard/briefing` aggregates the current (or requested) week into `stats` — todos completed/created/overdue, habit check-ins vs opportunities, best current streak, income/expense/balance + top expense category, journal/daily-entry counts, completed schedule events, average mood — and pairs them with a short prose `narrative`.

Two narrative implementations, chosen at startup:

- **Template (default, no API key)** — `TemplateNarrativeService` renders deterministic sentences from the real numbers. This is what runs out of the box, and it is also the automatic fallback whenever the AI call fails (bad key, wrong model name, network, misconfig), so the briefing never breaks.
- **AI (any provider)** — enabled the moment `Ai:ApiKey` is set. Claude/Anthropic names go to the native Messages API; everything else goes to an OpenAI-compatible `POST <BaseUrl>/chat/completions`, which covers ChatGPT/OpenAI, Gemini (via Google's official OpenAI-compat endpoint), OpenRouter, Groq, DeepSeek, Mistral, Ollama, LM Studio, and any custom gateway. The response reports what actually ran: `narrativeSource` (`"template"` | `"ai"`) and `narrativeProvider` (the configured `Ai:Provider`).

| `Ai:*` key | Required | Notes |
|---|---|---|
| `Ai:ApiKey` | yes | enables the AI narrative (any provider's key) |
| `Ai:Provider` | yes | `claude`/`anthropic`, `chatgpt`/`openai`, `gemini`, `openrouter`, `groq`, `deepseek`, `mistral`, `ollama`, `lmstudio`, or any custom name |
| `Ai:Model` | no* | defaults per provider (`claude-sonnet-4-5`, `gpt-4o-mini`, `gemini-3.8-flash`, `deepseek-chat`, ...); required for unknown providers |
| `Ai:BaseUrl` | no* | defaults per provider; required for unknown providers — point at any OpenAI-compatible base (no trailing `/chat/completions`) |
| `Ai:MaxTokens` | no | omitted by default on OpenAI-compatible calls (`0`/unset = none); Anthropic defaults to 1024 when unset |

```bash
# Claude
dotnet user-secrets set "Ai:Provider" "claude" --project src/TimeIsNow.API
dotnet user-secrets set "Ai:ApiKey" "<anthropic key>" --project src/TimeIsNow.API

# ChatGPT / OpenAI (defaults: api.openai.com/v1, gpt-4o-mini)
dotnet user-secrets set "Ai:Provider" "chatgpt" --project src/TimeIsNow.API
dotnet user-secrets set "Ai:ApiKey" "<openai key>" --project src/TimeIsNow.API
dotnet user-secrets set "Ai:Model" "gpt-5" --project src/TimeIsNow.API        # optional override

# Gemini (defaults to Google's official OpenAI-compatible endpoint)
dotnet user-secrets set "Ai:Provider" "gemini" --project src/TimeIsNow.API
dotnet user-secrets set "Ai:ApiKey" "<gemini key>" --project src/TimeIsNow.API
dotnet user-secrets set "Ai:Model" "gemini-3.8-flash" --project src/TimeIsNow.API  # optional override

# Any other OpenAI-compatible gateway
dotnet user-secrets set "Ai:Provider" "my-gateway" --project src/TimeIsNow.API
dotnet user-secrets set "Ai:BaseUrl" "https://llm.example.com/v1" --project src/TimeIsNow.API
dotnet user-secrets set "Ai:Model" "<model id>" --project src/TimeIsNow.API
dotnet user-secrets set "Ai:ApiKey" "<key>" --project src/TimeIsNow.API
```

The Dashboard renders the briefing as a "Weekly briefing" card (stat chips + narrative, `data-testid="weekly-briefing"`).

## MCP server

The API also hosts an MCP (Model Context Protocol) server over **Streamable HTTP** at `POST /mcp`, so MCP clients — Claude Code, Claude Desktop, and anything else that speaks the protocol — can read and write the signed-in user's data directly. It is stateless (one HTTP request per JSON-RPC message, no session handshake to keep), so no sticky sessions or `Mcp-Session-Id` bookkeeping are needed.

`/mcp` sits behind the same JWT bearer policy as the rest of the API (401 without a token), and every tool call runs as the token's user — the existing user-scoped `DbContext` filters mean an MCP client can only ever see its own rows. Because the normal access token expires in 15 minutes (useless for a long-running client), `POST /api/auth/mcp-token` mints a **30-day** token for this purpose. It takes no body; send a normal access token and copy `data.token`.

Tools (9, all `read`/`write` against the same services the REST API uses):

| Tool | Arguments (all optional unless noted) | Purpose |
| --- | --- | --- |
| `get_weekly_briefing` | `weekStart` (`yyyy-MM-dd`) | weekly stats + narrative (same payload as the REST endpoint) |
| `get_today` | `date` (`yyyy-MM-dd`) | overdue/today todos, today's schedule, habit flags |
| `list_categories` | — | id/name/color/slug for every non-archived category |
| `list_todos` | `completed`, `dueBefore` | todos with nested subtasks/tags; filter by done state or due date |
| `create_todo` | `title` (**required**), `categoryId`, `description`, `priority` (`low\|medium\|high`), `dueDate` | creates a todo |
| `list_habits` | `activeOnly` | habits with streak/check-in counts |
| `log_habit` | `habitId` (**required**), `date`, `completed`, `note` | upserts one habit log per (habit, date) |
| `list_journal_entries` | `from`, `to`, `search` | journal entries, newest first |
| `add_journal_entry` | `content` (**required**), `categoryId`, `date`, `title`, `mood` (1–5), `energy` (1–5) | creates a journal entry |

`categoryId` is optional on the write tools — omitted, the entry lands in the user's first non-archived category (registration seeds Personal, Study, Work, Leisure). Validation and service failures (empty title, unknown category, 1–5 range) come back as `isError: true` with the real message, so the model can correct itself.

### Connecting a client

The server needs a token first, then the URL + header:

```bash
# 1. register/login your user, then mint a long-lived token (30 days)
curl -s -X POST http://localhost:5099/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"you@example.com","password":"..."}'          # → data.accessToken
curl -s -X POST http://localhost:5099/api/auth/mcp-token \
  -H "Authorization: Bearer <accessToken>"                   # → data.token (copy this)

# 2. point Claude Code at it
claude mcp add --transport http timeisnow http://localhost:5099/mcp \
  --header "Authorization: Bearer <mcp-token>"

claude mcp list      # → timeisnow: connected
```

For Claude Desktop, add the same URL/header under `mcpServers` in `claude_desktop_config.json` (using an `mcp-remote` bridge, since Desktop speaks stdio). The token lasts 30 days; mint a new one the same way when it expires.

## Roadmap

1. ~~Database + schema + seed~~ ✅
2. ~~CRUD endpoints per entity + dashboard aggregation~~ ✅
3. ~~React client: layout, category sidebar, per-feature views (Ink dark theme)~~ ✅
4. ~~Authentication: JWT + rotating refresh cookie, multi-user scoping~~ ✅
5. ~~Weekly briefing: weekly stats + template narrative, any AI provider (Claude/ChatGPT/Gemini/OpenAI-compatible) when a key is set~~ ✅
6. ~~MCP server (`/mcp`, Streamable HTTP): read/write tools for Claude Code and other MCP clients, long-lived tokens~~ ✅
7. Insights (mood/energy trends, budget summaries)
8. Polish: reminders/recurrence expansion, export

## Commands

### Daily start (one command)

```powershell
.\dev.ps1            # start API + client (background), open http://localhost:5173
.\dev.ps1 -NoBrowser # start without opening a browser
.\dev.ps1 -Rebuild   # force a dotnet build first
.\stop.ps1           # stop both
```

`start.cmd` / `stop.cmd` are double-click wrappers (pin `start.cmd` to the taskbar for a one-click launch). The launcher reuses anything already listening on 5099/5173, runs the prebuilt API DLL (fast start), writes PIDs to `.run\` and logs to `logs\`. Requires MySQL to be running.

```bash
# run API (Development loads user secrets; listens on http://localhost:5099 via --urls)
cd src/TimeIsNow.API
dotnet run

# EF migrations (run from repo root; --project pins Infrastructure, --startup-project pins API)
dotnet ef migrations add <Name> --project src/TimeIsNow.Infrastructure --startup-project src/TimeIsNow.API
dotnet ef database update --project src/TimeIsNow.Infrastructure --startup-project src/TimeIsNow.API

# reset DB
dotnet ef database drop --force --project src/TimeIsNow.Infrastructure --startup-project src/TimeIsNow.API
dotnet ef migrations add InitialCreate --project src/TimeIsNow.Infrastructure --startup-project src/TimeIsNow.API
dotnet ef database update --project src/TimeIsNow.Infrastructure --startup-project src/TimeIsNow.API

# configure secrets (Development)
dotnet user-secrets set "ConnectionStrings:TimeIsNow" "server=localhost;port=3306;database=timeisnow;user=root;password=..."
dotnet user-secrets set "Jwt:SecretKey" "<random base64 ≥ 32 bytes>" --project src/TimeIsNow.API

# client (Vite dev server on http://localhost:5173, proxies /api → 5099)
cd client
npm install
npm run dev
```

### Tests (Playwright)

```bash
cd client
npm test           # run the E2E suite (starts API + client if not already running)
npm run test:headed
npx playwright show-trace test-results/<name>/trace.zip  # inspect a failed run
```

`client/playwright.config.ts` defines two `webServer` entries (API on 5099, Vite on 5173) with `reuseExistingServer: true`, so it reuses your running servers when present and starts them itself otherwise — never use `dev.ps1` for tests.

Specs live in `client/tests/`:

| File | Covers |
| --- | --- |
| `auth.spec.ts` | login guard, register, login, bad password, logout |
| `navigation.spec.ts` | All vs. per-category scope filtering of sidebar modules |
| `modules.spec.ts` | Settings toggles, persistence after reload, per-category isolation |
| `todos.spec.ts` | create / complete / delete, persistence after reload |
| `briefing.spec.ts` | weekly briefing endpoint contract + Dashboard briefing card |
| `mcp.spec.ts` | `/mcp` auth (401), tool listing, a live `create_todo` call, error surfacing |

`fixtures.ts` registers a fresh user against the API per test (email `pw-<ts>-<n>@test.local`, password `Test1234!`) and exposes an `app` fixture that logs in via the UI and waits for the sidebar. Tests reach the app through stable hooks — `data-testid` on `user-email`, `sidebar-nav`, `scope-chip`, `modules-all`, `modules-category`, `module-toggle`, `weekly-briefing`.
