# Cookbook Web Application

A modern web application for managing recipes and cookbooks. Built with ASP.NET Core Razor Pages, HTMX, and PostgreSQL, orchestrated with .NET Aspire.

## Stack

- **Framework:** ASP.NET Core Razor Pages with Vertical Slice Architecture + MediatR
- **Styling:** Bootstrap 5 (Bootswatch Materia theme)
- **Interactivity:** HTMX for progressive page updates (edit-in-place patterns)
- **Data:** Entity Framework Core with PostgreSQL
- **Auth:** ASP.NET Core Identity (custom pages)
- **Orchestration:** .NET Aspire (Docker Compose ready)
- **Testing:** xUnit + Playwright for UI E2E tests
- **Target Framework:** .NET 10

## Solution Structure

| Project | Purpose |
|---------|---------|
| **Recipe.AppHost** | .NET Aspire orchestration host; registers resources (PostgreSQL, Recipe.Web) |
| **Recipe.Web** | ASP.NET Core Razor Pages web app; main application code |
| **Recipe.MigrationService** | EF Core migrations + seed data worker; runs on app startup |
| **Recipe.ServiceDefaults** | Shared Aspire service defaults (resilience, health checks, tracing) |
| **Recipe.Tests** | xUnit unit tests (handlers, services) with in-memory EF Core |
| **Recipe.Tests.Playwright** | Playwright end-to-end browser tests |

## Prerequisites

- **.NET 10 SDK** — pinned in `global.json`; matches `net10.0` target framework
- **Docker Desktop** — required for .NET Aspire PostgreSQL container
- **Git** — for cloning the repo

> ℹ️ The Aspire workload is **not** required. Aspire is integrated via SDK reference in Recipe.AppHost.csproj.

## Getting Started (Developer)

1. **Clone the repository:**
   ```bash
   git clone <repo-url>
   cd Recipe
   ```

2. **Run the application:**
   ```bash
   cd Recipe.AppHost
   dotnet run
   ```
   Or with Aspire CLI (if installed):
   ```bash
   aspire run
   ```

3. **Open the Aspire Dashboard:**
   - URL: typically `http://localhost:18888`
   - View live resource status, logs, traces, and access the web app

4. **Access the web application:**
   - URL shown in dashboard under `recipe-web`
   - Default: `http://localhost:5000`

### First Run (Automatic Setup)

- The `Recipe.MigrationService` worker runs migrations and seeds the database
- The `recipe-web` service waits for migrations to complete
- Database is ephemeral (reset on each fresh Aspire run)

## Seed Users

| Email | Password | Notes |
|-------|----------|-------|
| `testuser@cookbook.test` | `Test@12345!` | Playwright test user |
| `alice@recipe.test` | `Seed@12345!` | Seed user with sample cookbooks |
| `bob@recipe.test` | `Seed@12345!` | Seed user |

All seed data uses `Bogus` with fixed seed (42) for reproducible test scenarios.

## Running Tests

### Unit Tests
```bash
dotnet test Recipe.Tests
```
Tests: SlugService, PublicIdService, CreateCookbookHandler, GetCookbookHandler, CreateRecipeHandler, and authorization checks.

### Playwright E2E Tests
```bash
dotnet test Recipe.Tests.Playwright
```
Requires the app running (either via `aspire run` or separately).  
Tests cover auth flows, cookbook CRUD, recipe CRUD, sharing, and edit-in-place workflows.

## Domain Model Overview

### Core Entities

- **ApplicationUser** — extends IdentityUser; owns cookbooks and recipes
- **Cookbook** — Id, OwnerId, Name, Description, PublicId (unique), Slug, CreatedAt, UpdatedAt
- **Recipe** — Id, OwnerId, Title, Description, Ingredients, Instructions, PrepTime, CookTime, Servings, PublicId (unique), Slug, OriginalRecipeId (for clones), CreatedAt, UpdatedAt
- **CookbookRecipe** — join table (CookbookId, RecipeId, SortOrder)
- **Share** — grants Read or Update permission on a Cookbook or Recipe to another user

### Key Patterns

- **URLs:** Use `{publicId}/{slug}` — never internal int IDs
  - Example: `/cookbooks/a1b2c3d4/my-cookbook`
- **PublicId:** Random Base62, 10 chars, unique per entity, stable
- **Slug:** Derived from name/title, lowercase with hyphens, not required to be unique
- **Authorization:** Ownership OR Share record grants access

### Main Routes

- `GET /cookbooks` — list my cookbooks
- `GET /cookbooks/{publicId}/{slug}` — cookbook details with recipes
- `GET /recipes/{publicId}/{slug}` — recipe details
- `POST /account/login`, `/account/register`, `/account/logout` — custom identity pages

## Architecture Highlights

### Vertical Slice + MediatR

Application logic lives in `Features/` folder, not PageModels:

```
Features/
├── Cookbooks/
│   ├── CreateCookbook/
│   │   ├── Command.cs
│   │   ├── Handler.cs
│   │   └── Response.cs
│   └── GetCookbook/
│       ├── Query.cs
│       ├── Handler.cs
│       └── Response.cs
└── Recipes/
    └── CreateRecipe/
        ├── Command.cs
        ├── Handler.cs
        └── Response.cs
```

- **Razor Pages** inject `IMediator` and call `Send()` only — no business logic
- **Handlers** are the unit-testable seam; future clients (API, CLI) dispatch the same requests
- **Services** (PublicIdService, SlugService) injected by DI into handlers

### HTMX Progressive Enhancement

- Edit-in-place recipe details and cookbook management
- Partial page swaps (`outerHTML`) for seamless UX
- `hx-get`, `hx-post` targets and swaps configured per feature
- Example: Edit Recipe button triggers inline form, form posts, server returns updated content

### EF Core with PostgreSQL

- Identity tables managed by `AddDefaultIdentity`
- Recipes/Cookbooks use `Npgsql.EntityFrameworkCore.PostgreSQL`
- Migrations bundled in `Recipe.MigrationService`

## Publishing with Docker Compose

Generate and deploy containerized environment:

1. **From the repo root:**
   ```bash
   cd Recipe.AppHost
   ```

2. **Generate Docker Compose artifacts:**
   ```bash
   dotnet run --publisher docker-compose --output-path ../publish/docker-compose
   ```
   Or with Aspire CLI:
   ```bash
   aspire publish -o ../publish/docker-compose
   ```

3. **Output location:** `publish/docker-compose/` (gitignored)
   - `docker-compose.yml` — wires PostgreSQL, migrations service, and web app
   - `aspire-manifest.json` — Aspire configuration manifest

4. **Deploy:**
   ```bash
   cd ../publish/docker-compose
   docker compose up
   ```

The Docker Compose environment is registered in `AppHost.cs` via `builder.AddDockerComposeEnvironment("compose")` and uses `Aspire.Hosting.Docker` v13.1.2-preview.1.26125.13.

## Common Development Tasks

### Add a New Feature (Handler + Tests)

1. Create a slice under `Features/{Domain}/{FeatureName}/`
2. Define `Command.cs` or `Query.cs`
3. Implement `Handler.cs` with business logic
4. Define `Response.cs` for result data
5. Wire in `Program.cs` if needed (MediatR auto-registers handlers)
6. Add tests in `Recipe.Tests` using in-memory EF Core

### Update the Database Schema

1. Make model changes in `Recipe.Web/Models/`
2. Run: `dotnet ef migrations add MigrationName` (from Recipe.Web directory)
3. Migration automatically runs on app startup via Recipe.MigrationService

### Troubleshooting

**App won't start:**
- Check Docker is running (required for Aspire PostgreSQL)
- View logs in Aspire Dashboard → Logs tab
- Restart: `aspire run` (re-prompts to stop existing instance)

**Migrations fail:**
- Check PostgreSQL container is healthy in Aspire Dashboard
- Manual check: `docker ps | grep postgres`

**Tests fail:**
- Unit tests: run `dotnet test Recipe.Tests`
- E2E tests: ensure app is running (`aspire run` in separate terminal)

## Project Context

**Lead Architect:** Keaton  
**Project:** Cookbook Web Application  
**Stack Decision:** ASP.NET Core Razor Pages, EF Core, Identity, HTMX, Bootstrap 5, PostgreSQL, .NET Aspire  
**Vertical Slice Architecture:** All domain logic in Features/; Razor Pages are thin presentation layers  

See `.squad/decisions.md` for team decisions and `.squad/agents/keaton/history.md` for project history.

---

**Ready to code.** Clone, run, test. Questions? Check the Aspire Dashboard logs.
