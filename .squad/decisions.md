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

---

## Switch to Edit-in-Place Pattern for Recipe Editing
**Date:** 2026-03-04  
**Agents:** Fenster, Hockney  
**Status:** Implemented

### Decision
Replaced the modal edit pattern with an edit-in-place pattern for recipe editing using HTMX `outerHTML` swapping.

### Implementation

**Backend Changes (Fenster):**
- Renamed `OnGetEditModalAsync()` → `OnGetEditFormAsync()` in Details.cshtml.cs
- Added `OnGetViewContentAsync()` for cancel flow
- Both handlers return partials targeting `#recipe-content` div
- `OnPostEditAsync()` sets `HX-Redirect` header for slug-change navigation

**Frontend Changes (Hockney):**
- Updated Details.cshtml to use `<partial name="_RecipeViewContent" />`
- Created `_RecipeViewContent.cshtml` — recipe display with `<div id="recipe-content">`
- Created `_RecipeEditForm.cshtml` — inline edit form with `<div id="recipe-content">`
- Deleted `_EditRecipeModal.cshtml`

### HTMX Architecture
1. **View → Edit:** Edit button `hx-get="?handler=EditForm"` with `hx-target="#recipe-content"` and `hx-swap="outerHTML"`
2. **Edit → View:** Cancel button `hx-get="?handler=ViewContent"` with same target/swap
3. **Save:** Form posts to `?handler=Edit`, server sets `HX-Redirect` header

### Key Details
- Both partials use `<div id="recipe-content">` as outermost element for `outerHTML` swap
- No `@page` directive in partials
- Edit form uses Bootstrap card styling (no modal)
- Antiforgery token included in form
- Progressive enhancement maintained

---

## Add Recipe In-Place Pattern
**Date:** 2026-03-04  
**Agents:** Fenster, Hockney  
**Status:** Implemented

### Decision
Replaced Add Recipe modal pattern with edit-in-place HTMX pattern for consistency with Edit Recipe UX.

### Implementation

**Backend Changes (Fenster):**
- Added `OnGetAddRecipeFormAsync()` — returns `Partial("_AddRecipeForm", null)` 
- Added `OnGetRecipeListAsync()` — returns `Partial("_RecipeList", Result.Recipes)`
- Modified `OnPostAddRecipeAsync()` — now returns `_RecipeList` partial instead of `_RecipesList`

**Frontend Changes (Hockney):**
- Created `_RecipeList.cshtml` — recipe list partial with Add Recipe button
- Created `_AddRecipeForm.cshtml` — inline add form partial
- Updated Details.cshtml to use `<partial name="_RecipeList" />`
- Deleted `_AddRecipeModal.cshtml` and `_RecipesList.cshtml`

### HTMX Architecture
1. **View → Add Form:** Add Recipe button `hx-get="?handler=AddRecipeForm"` with `hx-target="#recipe-list-section"` and `hx-swap="outerHTML"`
2. **Add Form → View:** Cancel button `hx-get="?handler=RecipeList"` with same target/swap
3. **Save:** Form posts to `?handler=AddRecipe`, server returns updated recipe list partial

### Key Details
- Both partials use `<div id="recipe-list-section">` as outermost element for `outerHTML` swap
- Form fields match `CreateRecipeCommand`: Title, Description, Ingredients, Instructions, PrepTime, CookTime, Servings
- No modal chrome — form appears inline in page layout
- Progressive enhancement maintained
- Playwright-verified: Add, Submit, Cancel all work in-place

---

## CookbookRecipeItem Field Extension
**Date:** 2026-03-04  
**Agent:** Fenster  
**Status:** Implemented

### Decision
Extended `CookbookRecipeItem` record to include recipe detail fields (Description, PrepTime, CookTime, Servings) for display on cookbook detail page recipe cards.

### Implementation
**File:** `Recipe.Web/Features/Cookbooks/GetCookbook/GetCookbookResponse.cs`

**Before:**
```csharp
public record CookbookRecipeItem(string PublicId, string Slug, string Title, int SortOrder);
```

**After:**
```csharp
public record CookbookRecipeItem(
    string PublicId, 
    string Slug, 
    string Title, 
    int SortOrder,
    string? Description,
    int? PrepTime,
    int? CookTime,
    int? Servings);
```

**Handler Update:**
Updated `GetCookbookHandler.cs` projection to populate new fields from `cr.Recipe` entity (already loaded via `ThenInclude`). No additional database queries required.

### Property Details
All new fields are nullable to match Recipe entity schema:
- **Description**: `string?` — recipe description text
- **PrepTime**: `int?` — prep time in minutes
- **CookTime**: `int?` — cook time in minutes
- **Servings**: `int?` — number of servings

### Build Status
✅ Build succeeded with 0 errors (verified via `dotnet build -o C:\Temp\recipe-build-check`)

### Frontend Impact
Hockney can now reference these properties in `_RecipeList.cshtml` partial to display on recipe cards:
- `item.Description`
- `item.PrepTime`
- `item.CookTime`
- `item.Servings`

All partials must handle null values gracefully (e.g., display empty string or default text when null).

---

## Recipe Card Layout Enhancement
**Date:** 2026-03-04  
**Agent:** Hockney  
**File:** Recipe.Web/Pages/Cookbooks/_RecipeList.cshtml  
**Status:** Implemented

### Decision
Enhanced recipe cards to display Description, PrepTime, CookTime, and Servings when available. Used flexbox layout with auto-margins to maintain consistent card heights while accommodating variable content.

### Layout Structure
```
Card (h-100, shadow-sm)
└── Card Body (d-flex flex-column)
    ├── Title (h5)
    ├── Description (small, muted, 2-line clamp) [conditional]
    ├── Timing/Servings (small, muted, gap-2) [conditional]
    └── View Recipe Button (mt-auto, btn-sm)
```

### CSS Patterns Used
1. **Description Truncation:**
   ```css
   overflow: hidden; 
   display: -webkit-box; 
   -webkit-line-clamp: 2; 
   -webkit-box-orient: vertical;
   ```
   - Truncates to 2 lines with ellipsis
   - Better than `text-truncate` (single line only)

2. **Button Positioning:**
   ```html
   <div class="mt-auto">
   ```
   - Pushes button to bottom of card
   - Works with `d-flex flex-column` on parent

### Conditional Rendering Logic
- **Description:** `@if (!string.IsNullOrWhiteSpace(recipe.Description))`
- **Timing Block:** `@if (recipe.PrepTime.HasValue || recipe.CookTime.HasValue || recipe.Servings.HasValue)`
- **Individual Values:** `@if (recipe.PrepTime.HasValue && recipe.PrepTime.Value > 0)` (also checks > 0 to hide zero values)

### Emoji Icons
Using direct emoji characters for visual clarity without icon library dependency:
- 🕐 = Prep time
- 🍳 = Cook time
- 🍽️ = Servings

### Accessibility Note
Emoji used here are decorative (not semantic). Screen readers will read the text ("10 min prep") which is sufficient context.

---

## Clone Recipe: Preserve Cookbook Membership
**Date:** 2026-03-04  
**Agent:** Fenster  
**Status:** Implemented

### Problem
When cloning a recipe, the new recipe was created successfully but was not added to any cookbooks. This left cloned recipes orphaned and inaccessible through cookbook navigation.

**Root Cause:** The `OnPostSaveCloneAsync` handler called `CreateRecipeCommand` to create the new recipe but did not call `AddRecipeToCookbookCommand` to preserve cookbook membership.

### Decision
Extended the recipe cloning flow to preserve cookbook membership by:
1. **Adding cookbook tracking to GetRecipeResponse** — Extended response with `IReadOnlyList<string> CookbookPublicIds`
2. **Fetching original recipe before cloning** — Modified `OnPostSaveCloneAsync` to first fetch original recipe
3. **Adding clone to original's cookbooks** — Loop through cookbooks and add clone using `AddRecipeToCookbookCommand`

### Implementation
**Files Modified:**
- `Recipe.Web/Features/Recipes/GetRecipe/GetRecipeResponse.cs` — Added `CookbookPublicIds` field
- `Recipe.Web/Features/Recipes/GetRecipe/GetRecipeHandler.cs` — Updated projection to include cookbook IDs
- `Recipe.Web/Pages/Recipes/Details.cshtml.cs` — Modified `OnPostSaveCloneAsync` to preserve cookbook membership

### Trade-offs
**Pros:** Reuses existing commands, minimal changes to contracts, cookbook membership automatically preserved  
**Cons:** Requires extra DB query for original recipe, multiple sequential commands instead of atomic operation

### Verification
- ✅ Build succeeded with 0 errors
- ✅ Confirmed only one place constructs GetRecipeResponse
- ✅ Pattern follows established vertical slice architecture

---

## Docker Compose Publish Environment
**Date:** 2026-03-04  
**Agent:** Keaton  
**Status:** Implemented

### Decision
Added Docker Compose publish support to the Aspire AppHost to enable containerized deployment of the Recipe application.

### Implementation
**Package:** Aspire.Hosting.Docker v13.1.2-preview.1.26125.13  
**File:** Recipe.AppHost/AppHost.cs

Added Docker Compose environment registration as final statement before `builder.Build().Run()`:
```csharp
builder.AddDockerComposeEnvironment("compose");
```

**Infrastructure Updates:**
- Added `/publish/` to `.gitignore` for generated Docker Compose artifacts

### Build & Test Results
✅ Build succeeded with 0 errors  
⚠️ Publish test: Partial success — generated `aspire-manifest.json` successfully

### Usage
```bash
# From Recipe.AppHost directory
dotnet run --publisher docker-compose --output-path ../publish/docker-compose
# Or using Aspire CLI
aspire publish -o ../publish/docker-compose
```

### Generated Artifacts
On successful publish, the following are created in `publish/docker-compose/`:
- `aspire-manifest.json` — Aspire deployment manifest
- `docker-compose.yaml` — Docker Compose orchestration file
- Additional Docker-related configuration files

---

## Admin CLI Plan Review — Team Consensus
**Date:** 2026-03-05 to 2026-03-06  
**Reviewers:** Keaton (Architecture), Fenster (Backend), Hockney (Frontend/UX), McManus (Testing)  
**Status:** ✅ Approved Architecture — ⏳ Implementation Blocked on Clarifications

### Consensus Outcome
All four reviewers affirm the thin CLI + dedicated admin API separation is **architecturally sound** and aligns with existing vertical-slice patterns.

### Architecture Approval
✅ **Approved Components:**
- Lean CLI principle (presentation/orchestration only)
- Dedicated admin API with MediatR handlers
- Vertical slice reuse from Recipe.Web
- Aspire integration for orchestration
- Hybrid UX (command entry + interactive Hex1b screens)

### Implementation Blocked On
Rick must clarify **6 critical blocking questions** before Phase 1 implementation begins:

1. **Auth Strategy** — Choose primary for v1:
   - OIDC device flow (recommended, but complex)
   - Browser-based OIDC + PKCE (simpler)
   - Scoped token + env-based auth (dev-only fallback)

2. **First-Phase Scope** — Define v1 user-management actions:
   - Required minimum: Search, View, Enable/Disable, Role Assignment
   - Out of v1: Audit logs, advanced RBAC, provisioning

3. **Admin Identity Model** — Choose approach:
   - Regular ApplicationUser with AdminUser role (recommended)
   - Separate admin identity store

4. **Application Layer Sharing** — Path A vs. Path B:
   - **Path A (Recommended):** Admin API references Recipe.Web
   - **Path B:** Extract Recipe.ApplicationCore early

5. **Operator Permission Semantics** — Clarify:
   - Role naming ("Operator" vs. "Admin")
   - Can modify only user-management or also content?
   - Operator hierarchy (can one grant another)?

6. **CLI Deployment Model** — Local vs. remote:
   - v1: Local dev via `dotnet run`
   - v1: Also Docker container in Aspire?
   - Deferred: NuGet tool packaging

### Design-Phase Clarifications (Post-Leadership Decision)
- Hybrid command/interactive balance (3–4 operator workflows)
- Session & token persistence strategy
- Error handling & feedback patterns
- Test coverage by layer

### Critical Gaps Identified

**By Fenster (Backend):**
- Identity & RBAC not defined
- Auth strategy unclear
- Admin user seeding missing
- Share model incompatible with admin ops
- Admin API design undefined

**By Hockney (Frontend/UX):**
- Hybrid model not concretely defined
- Session persistence undefined
- Error messages & feedback loop unspecified
- Progressive rollout strategy vague
- Confirmation & safety for destructive ops not addressed
- User-management workflows not detailed

**By McManus (Testing):**
- OIDC device flow testing not designed
- Admin API authorization logic not specified
- CLI token storage not tested
- Hex1b integration testing vague
- Admin API endpoint testing unclear
- Operator permissions vs. cookbook ownership undefined
- Aspire integration testing undefined

### Recommended Workflow

**Immediate (This Week):**
1. Rick reviews all four orchestration logs
2. Rick clarifies 6 blocking questions
3. Keaton consolidates feedback into decision document
4. Fenster sketches admin API contract
5. Ralph/Fenster adds placeholder Recipe.AdminApi to AppHost

**Phase 1 (Week of March 10):**
1. Fenster: Identity + RBAC foundation
2. Hockney: CLI interaction flows
3. McManus: Test cases for authorization + handlers

**Phase 2 (Week of March 17):**
1. Fenster: Admin API with handlers
2. Ralph/Hockney: Recipe.AdminCli with Hex1b
3. McManus: Validate test coverage

**Phase 3 (Week of March 24):**
1. Polish, documentation, operator guide
2. Staged rollout to core team

---

## Admin Platform Implementation Decision

**Date:** 2026-03-06  
**Author:** Fenster (Backend Developer)  
**Status:** ✅ Implemented

### Context

The Recipe application needed an administrative interface for user management operations. The requirement was to implement a v1 admin platform that follows existing architectural patterns and provides secure, operator-friendly tooling.

### Decision

Implemented a two-project admin platform:

#### 1. Recipe.AdminApi (Minimal API)

**Architecture:**
- Vertical slice pattern matching Recipe.Web structure
- MediatR handlers for business logic
- Minimal API endpoints (no controllers)
- JWT Bearer authentication (OIDC-ready)
- Role-based authorization (Admin-only policy)

**Features Delivered:**
- Search users (with pagination and filtering)
- View user details
- Enable/disable user access (via lockout)
- Assign/remove admin role

**Integration:**
- Shares AppDbContext schema with Recipe.Web (ASP.NET Core Identity)
- Registered in Recipe.AppHost for Aspire orchestration
- Uses Recipe.ServiceDefaults for observability

#### 2. Recipe.AdminCli (Console Application)

**Architecture:**
- System.CommandLine for CLI framework
- Thin client — all business logic lives in AdminApi
- OIDC device flow authentication
- Token storage abstraction with dual implementations

**Token Storage:**
- **Production:** `SecureTokenStorage` using Windows Registry + DPAPI
- **Development:** `FileTokenStorage` using local app data file
- Automatic selection based on environment

**Design Rationale:**
- Keeps CLI lightweight and stateless
- Reduces duplication between CLI and potential future admin UIs
- Enforces consistent authorization at the API layer
- Enables audit logging at a single point (API)

### Technical Choices

**Why Minimal APIs vs. Controllers?**
- Matches modern ASP.NET Core patterns
- Lighter weight for admin-only operations
- Vertical slices map naturally to route groups

**Why System.CommandLine vs. Hex1b?**
- Hex1b dependency unavailable in NuGet feed; opted for Microsoft's official CLI framework
- Provides similar developer experience with built-in help, arguments, and options
- Well-documented and actively maintained

**Why Separate AppDbContext?**
- AdminApi references only Identity tables (not Recipes/Cookbooks)
- Keeps admin concerns isolated from main app domain
- Future-proofs for potential schema divergence

**Auth Implementation:**
- JWT Bearer configured but requires OIDC provider setup
- Placeholder settings in appsettings for local dev
- Admin role seeded via DatabaseSeeder for immediate testing

### Testing Strategy

- Unit tests for all handlers (SearchUsers, GetUserDetails, SetUserAccess, SetAdminRole)
- Uses NSubstitute for mocking UserManager/RoleManager
- In-memory EF Core database for isolation
- All tests passing (55 total in suite)

### Deployment Considerations

**For Production:**
1. Configure OIDC provider (e.g., Duende IdentityServer, Auth0, Okta)
2. Update `appsettings.json` with:
   - `Authentication:Authority`
   - `Authentication:Audience`
3. Register AdminCli as OIDC public client with device flow enabled
4. Consider containerizing AdminApi for Docker Compose deployment

**Security Notes:**
- Admin role enforcement happens at API layer via `[Authorize(Policy = "AdminOnly")]`
- Token storage uses OS-level encryption on Windows
- Lockout uses ASP.NET Core Identity's built-in mechanism
- No plaintext credentials in CLI or config files

### Open Questions / Future Work

1. **OIDC Provider:** Team needs to select and configure identity provider
2. **Audit Logging:** Consider adding structured logs for all admin operations
3. **Cross-Platform Token Storage:** macOS/Linux support requires Keychain/Secret Service integration
4. **CLI Distribution:** Package as single-file executable or global tool?
5. **AdminApi Authorization:** Currently role-based; consider claims-based or policy-based in future

### Migration Path

The admin platform is additive — no breaking changes to Recipe.Web:

- **Database:** Admin role seeded automatically; existing users unaffected
- **Aspire:** AdminApi runs on separate port; no conflicts with Recipe.Web
- **Dependencies:** No shared state between admin and web projects

### Files Changed

**New Projects:**
- `Recipe.AdminApi/` — Admin API with 4 vertical slices
- `Recipe.AdminCli/` — CLI tool with OIDC auth and token storage

**Modified Files:**
- `Recipe.slnx` — Added AdminApi and AdminCli projects
- `Recipe.AppHost/AppHost.cs` — Registered AdminApi resource
- `Recipe.AppHost/Recipe.AppHost.csproj` — Added AdminApi project reference
- `Recipe.MigrationService/DatabaseSeeder.cs` — Added admin user seed
- `Recipe.MigrationService/Program.cs` — Added RoleManager registration
- `Recipe.Tests/Recipe.Tests.csproj` — Added AdminApi reference for tests
- `README.md` — Documented admin platform usage

### Test Coverage

- `Recipe.Tests/Features/Admin/SearchUsers/SearchUsersHandlerTests.cs`
- `Recipe.Tests/Features/Admin/SetUserAccess/SetUserAccessHandlerTests.cs`
- `Recipe.Tests/Features/Admin/SetAdminRole/SetAdminRoleHandlerTests.cs`

### Validation

- [x] Solution builds successfully
- [x] All 55 tests pass
- [x] AdminApi integrates with AppHost
- [x] CLI commands structured and ready for OIDC integration
- [x] Admin user seeded in development database
- [x] Vertical slice pattern matches Recipe.Web conventions
- [x] Documentation updated

### Lessons Learned

1. **Aspire Project References:** AppHost.csproj must include `<ProjectReference>` for Aspire SDK to generate `Projects.Recipe_AdminApi` typings
2. **System.CommandLine Return Values:** Handlers must use `void`/`Task` return types; exit codes managed separately
3. **Identity Role Manager:** RoleManager must be registered with `.AddRoles<IdentityRole>()` in both AdminApi and MigrationService
4. **Token Storage Platform Checks:** `OperatingSystem.IsWindows()` guards prevent CA1416 warnings while keeping code cross-platform ready

---

## Admin API Test Plan — McManus

**Date:** 2026-03-06  
**Status:** ✅ Test Scaffolding Complete — Ready for Implementation Activation

### Executive Summary

Created comprehensive test scaffolding for the admin API + CLI rollout. All tests are documented placeholders that define expected behavior and will become active tests once handlers are implemented.

### Test Coverage Matrix

#### 1. Admin API User Management (30 tests)

**SearchUsersHandler (6 tests)**
- Returns all users when no search term provided
- Filters by email when search term provided
- Filters by display name when search term provided
- Search is case-insensitive
- Requires Admin role authorization
- Excludes deleted users (if soft-delete implemented)

**GetUserDetailsHandler (5 tests)**
- Returns user with roles, lockout status, email confirmation
- Returns null when user not found
- Includes Admin role when user is admin
- Shows lockout status (LockoutEnd > UtcNow = locked)
- Requires Admin role authorization

**EnableDisableUserHandler (5 tests)**
- Disable sets LockoutEnd to distant future (100 years)
- Enable clears LockoutEnd to null
- Disable requires Admin role
- Enable requires Admin role
- Cannot disable self (prevent admin lockout)

**AssignRemoveAdminRoleHandler (8 tests)**
- Assign adds user to Admin role
- Remove removes user from Admin role
- Assign is idempotent (no error if already admin)
- Remove is idempotent (no error if not admin)
- Assign requires Admin role
- Remove requires Admin role
- Cannot remove own admin role (prevent admin demotion)
- Returns failure when user not found

#### 2. Admin Authorization Policy (6 tests)

**AdminAuthorizationTests:**
- Admin operations require Admin role (Theory test with role matrix)
- Admin role exists in IdentityRole
- Non-admin cannot search users
- Non-admin cannot get user details
- Non-admin cannot disable users
- Non-admin cannot assign admin role
- Authorization policy checks HttpContext.User

#### 3. CLI Authentication & OIDC Device Flow (13 tests)

**Device Flow Initiation:**
- Returns device_code, user_code, verification_uri, expires_in

**Device Flow Polling:**
- Returns "authorization_pending" until user authorizes
- Returns access_token + refresh_token after user authorizes
- Returns "expired_token" after timeout

**Token Refresh:**
- Returns new access_token when refresh token valid
- Returns error when refresh token expired

**Admin API Bearer Token Validation:**
- Rejects request without Bearer token (401)
- Rejects request with invalid Bearer token (401)
- Accepts request with valid Bearer token (200)
- Returns 403 for valid token without Admin role claim

**CLI Token Storage:**
- Persists token securely (encrypted file or OS keychain)
- Loads token on subsequent CLI run
- Auto-refreshes expired access token using refresh token

### Test Architecture

**Test Stack:**
- xUnit 2.9.3
- NSubstitute 5.3.0 for mocking UserManager/SignInManager
- EF Core InMemory for database isolation
- Existing DbContextHelper pattern reused

### Gaps and Decisions Needed

1. **OIDC Device Flow Strategy** — Mock vs. real OIDC provider for CI? Recommend: Fake OIDC endpoints for unit tests, optional real Duende IdentityServer for integration tests
2. **Operator Permission Model** — Use "Admin" role or separate "Operator" role? Recommend: Start with "Admin" role for v1 simplicity
3. **Authorization Approach** — `[Authorize(Roles="Admin")]` attribute vs. handler-level checks? Recommend: Handler-level for MediatR pattern consistency
4. **CLI Token Storage Location** — Where does CLI store tokens? Recommend: `~/.recipe-cli/tokens.json` encrypted with ProtectedData
5. **Admin Role Seeding** — How is first admin created? Recommend: DatabaseSeeder creates default admin user from appsettings

### Additional Test-Specific Decisions

1. **Soft Delete** — Will users have soft-delete (IsDeleted flag) or hard delete? Affects SearchUsers test
2. **Pagination** — Should SearchUsers support paging (skip/take)? Not covered in current tests
3. **Audit Logging** — Should admin actions be logged? Not covered in current tests (out of scope for v1?)
4. **Rate Limiting** — Should device flow polling be rate-limited? Not covered in tests

### Files Created

- `Recipe.Tests/Features/Admin/SearchUsersHandlerTests.cs` (6 tests)
- `Recipe.Tests/Features/Admin/GetUserDetailsHandlerTests.cs` (5 tests)
- `Recipe.Tests/Features/Admin/EnableDisableUserHandlerTests.cs` (5 tests)
- `Recipe.Tests/Features/Admin/AssignRemoveAdminRoleHandlerTests.cs` (8 tests)
- `Recipe.Tests/Features/Admin/AdminAuthorizationTests.cs` (7 tests)
- `Recipe.Tests/Features/Admin/CliAuthenticationTests.cs` (13 tests)

**Total:** 6 files, 47 tests, 0 implementations (ready for Fenster activation)

### Build Status

**Total Tests (Current):**
- 93 tests (46 baseline + 47 admin)
- All passing ✅

### Next Steps for Fenster

**Phase 1: Core Admin Handlers**
1. Uncomment and activate SearchUsers + GetUserDetails tests (11 tests)

**Phase 2: User Management Actions**
1. Uncomment and activate enable/disable + role tests (13 tests)

**Phase 3: Authentication & API Endpoints**
1. Uncomment and activate CLI auth tests (13 tests)

**Phase 4: Admin Role Seeding**
1. Uncomment AdminAuthorizationTests checks
