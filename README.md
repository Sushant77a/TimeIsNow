# Time is Now

Time is Now is a personal life dashboard and assistant. It organizes todos, journal entries, schedules, finances, goals, habits, practices, daily entries, and notes by life category such as Personal, Study, Work, or Leisure.

The repository contains a .NET Web API and a React client. It is currently designed for local development.

## Features

- Multi-user authentication with JWT access tokens and rotating httpOnly refresh cookies
- Category-scoped life management across todos, journal, schedule, finances, goals, habits, practices, daily entries, and notes
- Multi-day practice plans: type a goal (e.g. "chakra balancing"), let AI draft a day/session plan, tick sessions off in a daily log, and track streaks on a progress graph
- MySQL persistence with Entity Framework Core migrations
- Weekly dashboard briefing with a deterministic template or an optional AI provider
- Streamable HTTP MCP server at `/mcp`
- Swagger UI in the Development environment

## Tech stack

- API: .NET 10, ASP.NET Core, clean architecture
- Database: MySQL 8
- ORM: Entity Framework Core 10 with the Oracle MySQL provider
- Client: React 19, TypeScript, Vite, Tailwind CSS
- Tests: Playwright

## Prerequisites

Install the following before starting:

- .NET 10 SDK
- Node.js 20 or newer and npm
- MySQL 8.x
- Git

The application expects MySQL to be available on `localhost:3306`.

## Setup

### 1. Clone the repository

```bash
git clone <repository-url>
cd project_timeisnow
```

### 2. Create the database

Create an empty MySQL database. For example:

```bash
mysql -u root -p -e "CREATE DATABASE IF NOT EXISTS timeisnow CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
```

### 3. Configure local secrets

The API uses .NET User Secrets for values that must not be committed. Set the connection string and a local JWT signing key:

```bash
dotnet user-secrets set --project src/TimeIsNow.API "ConnectionStrings:TimeIsNow" "server=localhost;port=3306;database=timeisnow;user=root;password=<your-password>"

dotnet user-secrets set --project src/TimeIsNow.API "Jwt:SecretKey" "<use-a-long-random-local-secret>"
```

Use a separate MySQL user instead of `root` when possible. Never commit connection strings, JWT keys, API keys, refresh tokens, or `.env` files.

### 4. Restore dependencies and apply migrations

```bash
dotnet restore TimeIsNow.slnx
dotnet tool install --global dotnet-ef  # only if dotnet ef is not installed
dotnet ef database update --project src/TimeIsNow.Infrastructure --startup-project src/TimeIsNow.API

npm --prefix client ci
```

The migration command creates the schema in the `timeisnow` database.

## Run locally

On Windows, the included launcher starts both services:

```powershell
.\dev.ps1
```

Useful variants:

```powershell
.\dev.ps1 -NoBrowser  # start without opening a browser
.\dev.ps1 -Rebuild    # rebuild the API before starting
.\stop.ps1            # stop the API and client
```

The application is then available at:

- Client: <http://localhost:5173>
- API health check: <http://localhost:5099/api/health>
- Swagger UI: <http://localhost:5099/swagger>
- MCP endpoint: <http://localhost:5099/mcp>

The launcher writes local runtime output to `logs/` and process IDs to `.run/`; both are ignored by Git.

To run the services manually:

```bash
dotnet run --project src/TimeIsNow.API --urls http://localhost:5099
npm --prefix client run dev
```

Register a user from the client, or use the API endpoints under `/api/auth`.

## Optional AI briefing

Without an AI key, weekly briefings use the built-in template narrative. To enable an AI provider, configure these local secrets:

```bash
dotnet user-secrets set --project src/TimeIsNow.API "Ai:Provider" "openai"
dotnet user-secrets set --project src/TimeIsNow.API "Ai:ApiKey" "<your-api-key>"
dotnet user-secrets set --project src/TimeIsNow.API "Ai:Model" "<model-name>"  # optional
```

The application supports Anthropic and OpenAI-compatible providers. For a custom OpenAI-compatible gateway, also set `Ai:BaseUrl`. The same key also powers AI-drafted practice plans; without it, plan generation falls back to a deterministic template.

## MCP integration

The API exposes MCP tools for the authenticated user's data (11 tools covering briefings, todos, habits, journal, and practices). First sign in, call `POST /api/auth/mcp-token`, and use the returned 30-day token as a bearer token for `/mcp`.

Example for Claude Code:

```bash
claude mcp add --transport http timeisnow http://localhost:5099/mcp --header "Authorization: Bearer <mcp-token>"
```

Treat MCP tokens like passwords and revoke or replace them if exposed.

## Tests and validation

Build the API and client:

```bash
dotnet build TimeIsNow.slnx
npm --prefix client run build
```

Run the Playwright suite. The first run may require browser installation:

```bash
npm --prefix client exec -- playwright install
npm --prefix client test
```

The end-to-end tests expect the API, client, MySQL database, and local user secrets to be configured as described above.

## Project layout

```text
src/TimeIsNow.Domain          Entities and enums
src/TimeIsNow.Application     DTOs, interfaces, and business services
src/TimeIsNow.Infrastructure  EF Core data access, migrations, security, and AI clients
src/TimeIsNow.API              HTTP controllers, authentication, Swagger, and MCP
client/                        React/Vite application and Playwright tests
PROJECT.md                     Detailed domain, schema, API, and architecture notes
```

## Public repository safety

Before publishing or deploying, verify that no credentials or personal data are present in Git history. This project is configured for local development; production deployment should additionally use managed secrets, HTTPS, a non-root database account, restrictive CORS origins, and a production database backup strategy.

If generated files were tracked by an earlier checkout, remove them from Git's index before the first public push while keeping local copies:

```bash
git rm -r --cached logs .run
git rm -r --cached src/*/bin src/*/obj client/node_modules client/dist
git add .gitignore
```

## License

No license has been selected yet. Add a `LICENSE` file before accepting reuse or outside contributions.
