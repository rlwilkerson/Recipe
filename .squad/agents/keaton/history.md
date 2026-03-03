# Keaton — History

## Core Context

**Project:** Cookbook Web Application  
**User:** Rick Wilkerson  
**Stack:** ASP.NET Core Razor Pages, EF Core, ASP.NET Core Identity, HTMX, Bootstrap 5, SQL Server/SQLite (dev)

**Domain Model Summary:**
- `ApplicationUser` (extends IdentityUser) — owns cookbooks and recipes
- `Cookbook` — Id (int), OwnerId, Name, Description, PublicId (unique), Slug, CreatedAt, UpdatedAt
- `Recipe` — Id (int), OwnerId, Title, Description, Ingredients, Instructions, PrepTime, CookTime, Servings, PublicId (unique), Slug, OriginalRecipeId (for clones), CreatedAt, UpdatedAt
- `CookbookRecipe` — join table (CookbookId, RecipeId, SortOrder), composite PK
- `Share` — grants Read or Update permission on a Cookbook or Recipe to another user

**Key Patterns:**
- URLs use `{publicId}/{slug}` — never internal int IDs
- publicId: random Base62, 8–12 chars, stable, unique per entity
- Slug: derived from name/title, lowercase with hyphens, NOT required to be unique
- Authorization: ownership OR Share record grants access
- Cookbook share implies read access to contained recipes

**URL Routes:**
- `/cookbooks` — list my cookbooks
- `/cookbooks/{publicId}/{slug}` — cookbook details
- `/recipes/{publicId}/{slug}` — recipe details

## Learnings
