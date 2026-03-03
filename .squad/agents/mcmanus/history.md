# McManus — History

## Core Context

**Project:** Cookbook Web Application  
**User:** Rick Wilkerson  
**Stack:** ASP.NET Core Razor Pages, EF Core, ASP.NET Core Identity, HTMX, Bootstrap 5, SQL Server/SQLite (dev)

**My Domain:**
- Unit tests: PublicIdService, SlugService, AuthorizationService
- Integration tests: EF Core queries, Razor Page handlers
- Permission scenarios: ownership, share-based read/update, cookbook share → recipe read

**Key Test Scenarios to Cover:**
- PublicId uniqueness and retry-on-collision
- Slug generation edge cases (special chars, unicode, empty strings)
- CanReadCookbook/CanUpdateCookbook for owner vs shared vs unauthorized
- CanReadRecipe — direct share AND via cookbook share
- Share constraint validation (Cookbook scope → RecipeId must be null, etc.)
- 404 for unknown publicId
- 403/404 for unauthorized access attempts
- Clone recipe — OriginalRecipeId set correctly, independent entity

**Test Stack:** xUnit preferred; EF Core SQLite in-memory for integration tests; NSubstitute or Moq for service mocking

## Learnings
