# Fenster — History

## Core Context

**Project:** Cookbook Web Application  
**User:** Rick Wilkerson  
**Stack:** ASP.NET Core Razor Pages, EF Core, ASP.NET Core Identity, HTMX, Bootstrap 5, PostgreSQL, .NET Aspire (orchestration)

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
- **Database:** PostgreSQL via `Npgsql.EntityFrameworkCore.PostgreSQL` provider
- **Orchestration:** .NET Aspire — web app and PostgreSQL registered in AppHost; use Aspire service defaults and connection string injection

**URL Routes:**
- Pages identified by publicId from route; slug mismatch can optionally redirect (future)

**Architecture: Vertical Slice + MediatR**
- My primary domain is `Features/` — all commands, queries, and handlers live here
- PageModels (.cshtml.cs) are thin shells: inject `IMediator`, call `Send()`, bind result to page properties. Zero business logic.
- Each slice is self-contained: `Features/{Domain}/{FeatureName}/` contains the Request, Handler, and Response
- MediatR package: `MediatR` + `MediatR.Extensions.Microsoft.DependencyInjection`
- Services (PublicIdService, SlugService, AuthorizationService) are injected into handlers, NOT into PageModels
- EF Core DbContext injected into handlers directly (or via repository if needed)
- Validation: use FluentValidation pipeline behaviors or inline handler validation

## Session: Identity UI Package + Login/Register/Logout Pages

**Date:** 2026-03-03

### What Was Done

1. **Added `Microsoft.AspNetCore.Identity.UI` package** (version `10.0.0-preview.3.25172.1`) to `Recipe.Web.csproj` — matches the preview version pattern of other Identity packages.

2. **Updated Program.cs** — changed from `AddIdentity<ApplicationUser, IdentityRole>()` to `AddDefaultIdentity<ApplicationUser>().AddRoles<IdentityRole>().AddDefaultUI()`. This enables the built-in Identity UI scaffold at `/Identity/Account/...`.

3. **Existing `.cshtml.cs` code-behind files confirmed** — `Login.cshtml.cs`, `Register.cshtml.cs`, and `Logout.cshtml.cs` already existed in `Pages/Account/` (previously created but unlinked). The build errors were from a stale cache after swapping the Identity setup; after restore + rebuild all compiled cleanly.

### What Worked
- `AddDefaultIdentity` + `AddRoles` + `AddDefaultUI()` chain compiles correctly and preserves role support.
- Custom pages at `/Account/Login`, `/Account/Register`, `/Account/Logout` use their own `PageModel` code-behind files; they coexist with `AddDefaultUI()` which serves Identity pages at the `/Identity/Account/...` area route.
- Build: **0 errors, 1 warning** (unrelated NuGet vulnerability in `Microsoft.Build.Tasks.Core`).

## Session: Initial Backend Implementation

**Date:** [Current Session]

### What I Implemented

1. **AppDbContext Configuration** - Added complete `OnModelCreating` with:
   - Cookbook entity configuration (indexes, constraints, relationships)
   - Recipe entity configuration (indexes, self-referential foreign key for cloning)
   - CookbookRecipe join table with composite key
   - Share entity with proper multi-reference relationships (Owner, TargetUser, Cookbook?, Recipe?)

2. **Services Implementation**:
   - **PublicIdService**: Base62 random string generator (10 chars) with DB uniqueness checks and retry logic
   - **SlugService**: URL slug generation from titles/names (lowercase, hyphenation, sanitization)

3. **Feature Handlers** - Implemented all MediatR handlers:
   - **Cookbooks**: CreateCookbook, GetCookbook, ListCookbooks, AddRecipeToCookbook, ShareCookbook
   - **Recipes**: CreateRecipe, GetRecipe, CloneRecipe, ShareRecipe
   - **Authorization**: GetCookbookAccess, GetRecipeAccess (checking ownership and Share permissions)

4. **Access Control Logic**:
   - Integrated authorization checks directly in Get handlers
   - Return `null` (404) for unauthorized access to avoid revealing existence
   - Recipe access includes transitive access through cookbooks

5. **PageModels** - Updated to:
   - Extract userId from ClaimsPrincipal
   - Pass userId to queries for authorization
   - Added `[Authorize]` to Cookbooks/Index

6. **Share Model Fix**:
   - Corrected scaffolded Share model to match charter spec (OwnerId, TargetUserId, CookbookId?, RecipeId?)
   - Previous scaffold used different property names (SharedWithUserId, SharedByUserId, ResourceId)

7. **EF Migration**:
   - Generated `InitialCreate` migration with all entities and relationships
   - Added Microsoft.EntityFrameworkCore.Design package reference

### Tricky Decisions

1. **Authorization Placement**: Charter specified separate GetCookbookAccessQuery/GetRecipeAccessQuery handlers returning access results, but the scaffolded queries returned `bool`. I implemented both:
   - Authorization handlers for programmatic access checks (return bool)
   - Get handlers with inline authorization (check userId and return null if unauthorized)

2. **GetRecipeResponse.OriginalRecipePublicId**: The cshtml file expected OriginalRecipePublicId (string) but model had OriginalRecipeId (int). Changed response to include the PublicId of the original recipe for routing.

3. **Share Model Updates**: Had to correct the Share entity to match charter spec - previous scaffolding used incompatible property names.

4. **Test Updates**: Updated existing GetCookbookHandlerTests to pass userId parameter after changing query signature.

5. **Partial View Fixes**: Fixed cshtml partial namespaces (_CookbookList, _RecipesList) to reference correct Feature types instead of non-existent Recipe.Application namespace.

## Learnings

### Session: EditRecipe Feature + BindProperty Expansion

**Date:** [Current Session]

#### What Was Done

1. **Expanded Add Recipe binding** in `Cookbooks/Details.cshtml.cs`:
   - Added `RecipeIngredients`, `RecipeInstructions`, `RecipePrepTime`, `RecipeCookTime`, `RecipeServings` BindProperty fields
   - Updated `OnPostAddRecipeAsync()` to pass all fields to `CreateRecipeCommand` (replaced `null, null, null, null, null` placeholders)

2. **Created EditRecipe vertical slice** (`Features/Recipes/EditRecipe/`):
   - `EditRecipeCommand.cs` — record with all recipe fields + RequestingUserId
   - `EditRecipeResponse.cs` — returns PublicId, NewSlug, Title
   - `EditRecipeHandler.cs` — ownership check (throws `UnauthorizedAccessException` if not owner), updates all fields, regenerates slug, saves

3. **Extended `GetRecipeResponse`** — added `bool IsOwner` as the last field

4. **Extended `GetRecipeHandler`** — added `recipe.OwnerId == request.UserId` for the new IsOwner field

5. **Updated `Recipes/Details.cshtml.cs`**:
   - Added `[Authorize]` attribute
   - Added 7 BindProperty fields for edit (EditTitle, EditDescription, EditIngredients, EditInstructions, EditPrepTime, EditCookTime, EditServings)
   - Added `OnGetEditModalAsync()` — loads recipe, checks IsOwner, returns `_EditRecipeModal` partial
   - Added `OnPostEditAsync()` — dispatches EditRecipeCommand, sets `HX-Redirect` header, returns OkResult

#### Key Pattern: HTMX Redirect After POST
Set `Response.Headers["HX-Redirect"]` before returning `OkResult()` to let HTMX navigate to the new URL after a successful mutation. This is the correct pattern for slug-changing edits where the URL changes.

#### Key Pattern: IsOwner on Query Response
Adding `IsOwner` directly to the response record (rather than a separate query) avoids an extra round-trip. The handler already loads the entity and has the OwnerId; `recipe.OwnerId == request.UserId` is free.

#### Build Note
When Recipe.Web is running (via Aspire), `dotnet build` fails with MSB3027 file-lock errors on ServiceDefaults.dll. Build to a temp output dir (`-o C:\Temp\...`) avoids the lock and confirms 0 compiler errors.

## Cross-Agent Update — 2026-03-03T151215Z

**From Hockney:**
- `_EditRecipeModal.cshtml` form field names: `EditTitle`, `EditDescription`, `EditIngredients`, `EditInstructions`, `EditPrepTime`, `EditCookTime`, `EditServings` — must match PageModel BindProperty names in `Recipes/Details.cshtml.cs`
- `_AddRecipeModal.cshtml` field names: `RecipeIngredients`, `RecipeInstructions`, `RecipePrepTime`, `RecipeCookTime`, `RecipeServings` — must match BindProperty names in `Cookbooks/Details.cshtml.cs`
- Edit modal HTMX target: `#recipe-edit-modal-container` (div added to Details.cshtml)
- Edit modal close: `closeEditModal()` clears container innerHTML; no Bootstrap JS `.modal('hide')` call needed

### Session: Edit-in-Place Handler Refactor

**Date:** 2026-03-03

#### What Was Changed
Refactored `Recipes/Details.cshtml.cs` to support edit-in-place pattern instead of modal-based editing:
- **Renamed `OnGetEditModalAsync` → `OnGetEditFormAsync`**: Changed return to `Partial("_RecipeEditForm", Result)` — loads recipe, checks IsOwner, returns edit form partial
- **Added `OnGetViewContentAsync`**: New handler that loads recipe and returns `Partial("_RecipeViewContent", Result)` — used to restore view mode after cancel; no IsOwner check since view content is visible to all who can see the page
- **Preserved all BindProperty fields**: EditTitle, EditDescription, EditIngredients, EditInstructions, EditPrepTime, EditCookTime, EditServings still needed for `OnPostEditAsync`
- **No changes to `OnPostEditAsync`**: Still uses HX-Redirect pattern for slug-changing edits

#### Why
Switching from Bootstrap modal UI to edit-in-place HTMX pattern where the `#recipe-content` div swaps between view and edit partials. Cleaner UX, less JavaScript, simpler DOM structure.
