# Decision: Solution Scaffold Structure

**Date:** 2026-03-03  
**By:** Keaton (Lead & Architect)  
**Status:** Accepted

## Context
Scaffolded the complete .NET solution skeleton for the Cookbook Web Application. This establishes the project structure, NuGet dependencies, and folder conventions that all subsequent feature work builds on.

## Decision

### Solution Structure
```
Recipe.slnx                          (solution file — .slnx XML format from .NET 10)
├── Recipe.Web/                      (ASP.NET Core Razor Pages — main app)
├── Recipe.AppHost/                  (Aspire orchestrator — PostgreSQL + web)
├── Recipe.ServiceDefaults/          (Aspire shared service config)
├── Recipe.Tests/                    (xUnit unit tests + NSubstitute)
└── Recipe.Tests.Playwright/         (Playwright UI integration tests)
```

### Target Framework
- **net10.0** — matches the SDK pinned in global.json (10.0.200-preview.0)

### NuGet Packages
- **MediatR 12.4.1** — vertical slice command/query dispatch
- **Microsoft.AspNetCore.Identity.EntityFrameworkCore** — Identity + EF integration
- **Npgsql.EntityFrameworkCore.PostgreSQL** — PostgreSQL provider
- **Aspire.Hosting.PostgreSQL 13.0.0** — Aspire PostgreSQL container orchestration
- **NSubstitute 5.3.0** — test mocking
- **Microsoft.Playwright** — browser-based UI testing

### Features/ Folder Layout (Vertical Slices)
```
Features/
├── Cookbooks/
│   ├── CreateCookbook/    (Command + Handler + Response)
│   ├── GetCookbook/       (Query + Handler + Response)
│   ├── ListCookbooks/     (Query + Handler + Response)
│   ├── AddRecipeToCookbook/ (Command + Handler)
│   └── ShareCookbook/     (Command + Handler)
├── Recipes/
│   ├── CreateRecipe/      (Command + Handler + Response)
│   ├── GetRecipe/         (Query + Handler + Response)
│   ├── CloneRecipe/       (Command + Handler)
│   └── ShareRecipe/       (Command + Handler)
└── Authorization/
    ├── GetCookbookAccessQuery.cs
    └── GetRecipeAccessQuery.cs
```

### Identity Strategy
- Using `AddIdentity<ApplicationUser, IdentityRole>()` (not `AddDefaultIdentity`) to avoid Identity.UI dependency for now.
- Identity UI scaffolding can be added when login/register pages are needed.

### AppHost Wiring
- PostgreSQL resource named `"postgres"` with database `"RecipeDb"`
- Recipe.Web references the database via connection string `"RecipeDb"`

## Consequences
- All agents can now reference this structure when implementing features.
- Handlers throw `NotImplementedException` — Fenster will implement.
- Pages are stub shells — Hockney will implement UI.
- No EF configuration (Fluent API) yet — that comes with the first migration.
