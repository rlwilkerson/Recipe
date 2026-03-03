# Fenster — History

## Core Context

**Project:** Cookbook Web Application  
**User:** Rick Wilkerson  
**Stack:** ASP.NET Core Razor Pages, EF Core, ASP.NET Core Identity, HTMX, Bootstrap 5, SQL Server/SQLite (dev)

**My Domain:**
- EF Core entities and DbContext
- Services: PublicIdService (Base62 publicId gen), SlugService (name → slug), AuthorizationService (ownership + Share checks)
- ASP.NET Core Identity setup
- Razor Page PageModel handler methods (.cshtml.cs)
- Share permission enforcement

**Key Rules:**
- `PublicId` is unique per entity type — unique DB index required
- `Slug` is NOT unique — publicId is the real lookup key
- `OriginalRecipeId` FK uses `DeleteBehavior.Restrict`
- Share scope rules: Cookbook scope → CookbookId non-null, RecipeId null; Recipe scope → opposite
- Cookbook share implies recipe read access (business rule in AuthorizationService)
- All actions require authenticated user; 404 for unknown/unauthorized publicId

**URL Routes:**
- Pages identified by publicId from route; slug mismatch can optionally redirect (future)

## Learnings
