# GemBoard — API

> A minimalist tool to frame your steps and track your progress. Each column is a **guided step**, and a card **accumulates its content** as it moves through the workflow — it becomes the readable history of its own progress.

Back-end API for GemBoard, built with **ASP.NET Core / C#**. It handles authentication, boards, columns, cards, per-step progress entries, project templates, and real-time synchronisation.

🔗 **Live demo:** https://kanban-cyril14.vercel.app
🔗 **Portfolio:** https://portfolio-cyril14.vercel.app
🔗 **Frontend repository (React):** https://github.com/cyriltheeten-sudo/Kanban

---

## Features

- **Authentication** via JWT (hashed passwords). Login is protected by **rate limiting** (5 attempts/min per IP) against brute-force.
- **Boards / columns / cards:** full CRUD, with drag-and-drop reordering persisted server-side.
- **Per-step card entries:** each card holds a distinct entry per column (objective, resources, journal, outcome…), created on demand and updated via an *upsert* — the core of the "guided progress" experience.
- **Project templates:** create a board from a predefined set of step-columns, each carrying its own guidance description (shared system templates + a base ready for per-user personal templates).
- **Real-time:** synchronisation across clients via SignalR (WebSockets) — one user's changes appear for the others without a reload.

## Tech stack

| Layer | Technologies |
|---|---|
| Back-end | C#, ASP.NET Core, REST API |
| Data access | Entity Framework Core |
| Database | PostgreSQL (hosted on Neon) |
| Real-time | SignalR |
| Authentication | JWT |
| Containerisation | Docker |
| Deployment | Render |
| Testing | xUnit (in-memory database) |

## Architecture

The API follows a **layered separation**:

- **Controllers** — HTTP entry point: validate the request, check authorisation, delegate to the service, return the right status code.
- **Services** (`CardService`, `ColumnService`, `BoardService`, `TemplateService`) — business logic, isolated and testable (single-responsibility principle).
- **DTOs** — dedicated read models: the API never serialises raw entities, it shapes exactly what the client receives (and avoids reference cycles).
- **Models** — entities and request contracts.
- **Data** — the Entity Framework `DbContext` and the seeding of reference data.

A few things I paid particular attention to:

- **Object-level authorisation:** every mutating action verifies ownership **before** acting (through a centralised `IsBoardOwnedBy` check, walking card → column → board → owner). Unauthorised access returns `404` without revealing whether the resource exists.
- **Stateless identity:** the user is always resolved from the JWT server-side, never from client-supplied data.
- **Secrets out of the repo:** connection string and JWT key via User Secrets locally, environment variables in production.

## Testing

The project has **35 xUnit tests**: 30 unit tests covering the service layer's business logic (create / update / delete / move cards, upsert of progress entries, board creation from a template, system/personal template filtering, per-user data isolation), plus 5 **integration tests** that run the real HTTP pipeline (`WebApplicationFactory`, real JWTs) to verify cross-user authorization at the controller level — including regression tests for two IDOR fixes (moving a card into another user's column, writing an entry onto another user's card).

See **[TESTING.md](./TESTING.md)** for the detailed testing strategy.

```bash
dotnet test Kanban.Tests/Kanban.Tests.csproj
```

## CI/CD

A GitHub Actions workflow (`.github/workflows/ci-cd.yml`) runs on every push and pull request targeting `main`:

1. **Build & test** — restores, builds, and runs the full xUnit suite.
2. **Docker image builds** — validates that the production `Dockerfile` still builds.
3. **Deploy** — on a push to `main`, once the previous jobs pass, triggers a Render [Deploy Hook](https://render.com/docs/deploy-hooks) to roll out the new version. This step is skipped on pull requests.

The deploy step requires a repository secret `RENDER_DEPLOY_HOOK_URL` (Render service → Settings → Deploy Hook). Render's own auto-deploy-on-push should be disabled for this service so deploys only happen after CI passes.

## Running locally

Prerequisites: the .NET 8 SDK and a PostgreSQL database (or a Neon account).

1. Configure the connection string and JWT key via **User Secrets** (never in plain text in the code):
   ```bash
   cd Kanban.Api
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=...;Database=...;Username=...;Password=...;SSL Mode=Require"
   dotnet user-secrets set "Jwt:Key" "your-secret-key"
   dotnet user-secrets set "Jwt:Issuer" "Kanban.Api"
   ```
2. Apply the migrations and run:
   ```bash
   dotnet ef database update
   dotnet run
   ```
3. The API starts and Swagger is available to explore the endpoints.

## Repository structure

```
.
├── Kanban.Api/        # the API project (controllers, services, DTOs, models, data)
├── Kanban.Tests/      # the unit tests (xUnit)
├── Kanban.Api.sln     # the solution
└── TESTING.md         # testing strategy
```

---

*Personal project built as part of a full-stack .NET / React upskilling effort.*
