# Gemboard API — ASP.NET Core backend

REST + real-time backend for **Gemboard**, a real-time collaborative Kanban board.

**👉 Full project overview, screenshots and live demo:** https://github.com/cyriltheeten-sudo/kanban-front
**🔗 Live demo:** https://kanban-cyril14.vercel.app

## Overview

ASP.NET Core (C#) API providing:

- **REST endpoints** for boards, columns and cards (CRUD) — Entity Framework Core over PostgreSQL.
- **Authentication** — JWT, password hashing, protected routes.
- **Real-time** — a SignalR hub broadcasts changes to all clients connected to a board.

## Tech stack

ASP.NET Core · C# · Entity Framework Core · SignalR · JWT · PostgreSQL · Docker

## Running locally

```bash
# 1. restore dependencies
dotnet restore

# 2. provide secrets (never committed) via user-secrets
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=...;Database=...;Username=...;Password=...;SSL Mode=Require"
dotnet user-secrets set "Jwt:Key" "<a-random-secret-key>"

# 3. run — migrations are applied automatically on startup
dotnet run
```

The API exposes Swagger at `/swagger` in development.

## Deployment

Containerized with the included `Dockerfile` and deployed on Render; database on Neon (managed PostgreSQL). Configuration (connection string, JWT key, allowed CORS origins) is provided through environment variables.
