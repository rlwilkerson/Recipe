# Team Decisions

_Append-only. Managed by Scribe. Agents write to `.squad/decisions/inbox/` — Scribe merges here._

---

### 2026-03-03: Project initialized
**By:** Rick Wilkerson  
**What:** Cookbook Web Application spec approved. Team hired: Keaton (Lead), Fenster (Backend), Hockney (Frontend), McManus (Tester).  
**Stack:** ASP.NET Core Razor Pages, EF Core, ASP.NET Core Identity, HTMX, Bootstrap 5, SQL Server/SQLite.  
**Key patterns:** publicId + slug URLs, drop-box decisions pattern, HTMX progressive enhancement.

---

## Stack Decisions
**Date:** 2026-03-03  
**Source:** copilot-directive-20260303T041817Z.md  
**Content:**
- Use **.NET Aspire** for development orchestration (AppHost, service discovery, dashboard)
- Use **PostgreSQL** as database (explicit user directive)
- Use **Playwright** for UI integration testing
- Use **ASP.NET Core Identity** for authentication

---

## Vertical Slice Architecture with MediatR
**Date:** 2026-03-03  
**Source:** copilot-directive-20260303T042119Z.md  
**Content:**
- All application functionality (commands, queries, handlers, responses) in `Features/` folder
- Razor Pages are thin — inject `IMediator` and call `Send()` only
- No business logic in PageModels; enables unit testability and future client support (API, mobile, CLI)
- Clear separation: Features contain domain logic; Pages contain only presentation logic

---

## Solution Scaffold Structure
**Date:** 2026-03-03  
**Source:** keaton-solution-scaffold.md  
**Content:**
- Solution structure: Recipe.slnx (XML format), Recipe.Web, Recipe.AppHost, Recipe.ServiceDefaults, Recipe.Tests, Recipe.Tests.Playwright
- Target framework: net10.0 (SDK pinned in global.json)
- NuGet packages: MediatR 12.4.1, Identity.EntityFrameworkCore, Npgsql.EntityFrameworkCore.PostgreSQL, Aspire.Hosting.PostgreSQL, NSubstitute 5.3.0
- Identity strategy: `AddIdentity<ApplicationUser, IdentityRole>()` (not `AddDefaultIdentity`)
- Handlers throw `NotImplementedException` — ready for backend implementation

---

## Fenster Backend Implementation Decisions
**Date:** 2026-03-03  
**Source:** fenster-backend-impl.md  
**Content:**
- **PublicIdService:** Separate async methods for cookbooks and recipes (Base62, 10-char, collision retry)
- **SlugService:** Synchronous, pure string manipulation; no DB dependency
- **Authorization:** Inline checks in Get handlers return `null` for unauthorized (404 security pattern)
- **Recipe Access:** Transitive through cookbooks — users read recipes via cookbook shares
- **Share Entity:** Nullable foreign keys (exactly one: CookbookId OR RecipeId); uses OwnerId pattern

---

## Test Coverage Report — McManus
**Date:** 2026-03-03  
**Source:** mcmanus-test-coverage.md  
**Content:**
- **46 total tests:** 10 SlugService, 8 PublicIdService, 7 CreateCookbookHandler, 5 GetCookbookHandler, 8 CreateRecipeHandler, 8 Authorization placeholders
- **100% pass:** All service and handler tests compile and pass; Authorization tests are documented placeholders pending implementation
- **Playwright stubs:** 9 skipped tests with meaningful messages; ready for app runtime
- **Infrastructure:** In-memory EF Core, NSubstitute mocking; DbContextHelper utility created

---

## Fenster: Switch to AddDefaultIdentity + AddDefaultUI
**Date:** 2026-03-03  
**Source:** fenster-identity-ui.md  
**Content:**
- Switched from `AddIdentity<ApplicationUser, IdentityRole>()` to `AddDefaultIdentity<ApplicationUser>().AddRoles<IdentityRole>().AddDefaultUI()`
- Added `Microsoft.AspNetCore.Identity.UI` (v10.0.0-preview.3.25172.1)
- Enables built-in Identity scaffold while preserving role support
- Custom pages at `/Account/` coexist with Identity UI area pages; no conflicts
- Build clean: 0 errors

---

## Hockney: Custom Identity Pages (No Identity Area Scaffolding)
**Date:** 2026-03-03  
**Source:** hockney-identity-pages.md  
**Content:**
- Custom Login, Register, Logout Razor Pages created under `Pages/Account/` (not `Areas/Identity/`)
- Avoids area overhead; aligns with app structure
- PageModels for auth pages inject `SignInManager`/`UserManager` directly (Identity is infrastructure, not MediatR domain)
- Updated `_LoginPartial.cshtml` to reference custom pages
- Updated `_Layout.cshtml` with auth-conditional "My Cookbooks" link
- Build clean: 0 errors

---

## Bootswatch Materia Theme
**Date:** 2026-03-04  
**Source:** hockney-bootswatch-materia.md  
**Content:**
- Replaced Bootstrap 5 CDN with Bootswatch Materia (`bootswatch@5.3.3/dist/materia/bootstrap.min.css`)
- Navbar updated from `navbar-dark bg-dark` to `navbar-light bg-white shadow-sm`
- Bootstrap JS, HTMX CDN, and site.css unchanged
- To swap themes in future: change `/materia/` in CDN URL to desired theme name

---

## EditRecipe Feature + IsOwner on GetRecipeResponse
**Date:** 2026-03-03T151215Z  
**Source:** fenster-edit-recipe.md  
**Content:**
- `IsOwner` added to `GetRecipeResponse` — computed inline as `recipe.OwnerId == request.UserId` (no extra DB call)
- `UnauthorizedAccessException` thrown by mutating handlers (Edit, Delete) to distinguish "not found" from "not authorized"; Get handlers return `null` for both (404 security pattern)
- `[Authorize]` added to `Recipes/Details` PageModel — entire page requires authentication

---

## Edit Recipe UI (Modal + Details Page)
**Date:** 2026-03-03T151215Z  
**Source:** hockney-edit-recipe-ui.md  
**Content:**
- `_EditRecipeModal.cshtml` uses `@model GetRecipeResponse` (partial, no `@page` directive); served via HTMX GET handler
- HTMX `hx-on::after-request` closes modal on success; `HX-Redirect` response header handles navigation after slug-changing edits
- `closeEditModal()` sets container `innerHTML = ''` (consistent with `closeRecipeModal()` pattern)
- "Edit Recipe" button gated with `@if (Model.Result?.IsOwner == true)`, styled `btn-warning`
- Add Recipe modal (`_AddRecipeModal.cshtml`) extended with Ingredients, Instructions, PrepTime, CookTime, Servings fields
