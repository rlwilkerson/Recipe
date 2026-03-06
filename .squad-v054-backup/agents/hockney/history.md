# Hockney — History

## Core Context

**Project:** Cookbook Web Application  
**User:** Rick Wilkerson  
**Stack:** ASP.NET Core Razor Pages, EF Core, ASP.NET Core Identity, HTMX, Bootstrap 5, PostgreSQL, .NET Aspire (orchestration)

**My Domain:**
- Razor Page markup (.cshtml templates)
- HTMX interactions (hx-get, hx-post, hx-target, hx-swap, hx-boost)
- Bootstrap 5 layout, responsive design, components
- Partial views: `_RecipesList`, `_CookbookList`, `_CreateModal`, `_ShareModal`
- Progressive enhancement — pages work without JS/HTMX

**Key Patterns:**
- All cookbook URLs: `/cookbooks/{publicId}/{slug}`
- All recipe URLs: `/recipes/{publicId}/{slug}`
- HTMX handlers via query string: `?handler=RecipesPartial`, `?handler=AddRecipe`
- Razor Page route declarations: `@page "/cookbooks/{publicId}/{slug}"`
- `[FromRoute]` properties on PageModel for publicId and Slug

**Page Structure:**
```
/Pages
  /Cookbooks
    Index.cshtml + .cs         → /cookbooks
    Details.cshtml + .cs       → /cookbooks/{publicId}/{slug}
    _CookbookList.cshtml
    _RecipesList.cshtml
    _CreateModal.cshtml
    _ShareModal.cshtml
  /Recipes
    Details.cshtml + .cs       → /recipes/{publicId}/{slug}
    _ShareModal.cshtml
  Index.cshtml                 → home
```

**Architecture: Vertical Slice + MediatR**
- Razor Pages are thin: inject `IMediator`, call `Send()`, bind the result to page properties
- No service calls, no DbContext, no business logic in .cshtml.cs
- Example PageModel pattern:
  ```csharp
  public async Task<IActionResult> OnGetAsync()
  {
      Result = await _mediator.Send(new GetCookbookQuery(PublicId));
      if (Result is null) return NotFound();
      return Page();
  }
  ```
- HTMX partial handlers also use IMediator: `OnGetRecipesPartialAsync()` → `Send(new ListCookbookRecipesQuery(...))`
- Page properties are bound from handler Response objects, not from raw EF entities

## Learnings

### 2026-03-03 — Edit Recipe UI (Modal + Details Page)

**What I Built:**
- Added Ingredients, Instructions, PrepTime, CookTime, Servings fields to `Cookbooks/_AddRecipeModal.cshtml` (Create Recipe flow)
- Added conditional "Edit Recipe" button (btn-warning, owner-only) to Actions card in `Recipes/Details.cshtml`, before Clone button
- Added `<div id="recipe-edit-modal-container"></div>` at bottom of Details.cshtml (before `@section Scripts`) as HTMX swap target
- Created `Recipes/_EditRecipeModal.cshtml` partial — Bootstrap modal (show d-block, backdrop overlay) with pre-populated form fields bound to `GetRecipeResponse`

**Key Patterns:**
- Edit modal partial uses `@model Recipe.Web.Features.Recipes.GetRecipe.GetRecipeResponse` (no `@page` directive — it's a partial)
- HTMX POST to `?handler=Edit`, target `#recipe-edit-modal-container`, `hx-swap="innerHTML"`
- `hx-on::after-request="if(event.detail.successful) closeEditModal()"` closes modal on success; `HX-Redirect` header from handler handles navigation
- `closeEditModal()` JS function: sets `innerHTML = ''` on the container div
- Owner check: `@if (Model.Result?.IsOwner == true)` gates visibility of Edit button

**File Locations:**
```
Recipe.Web/Pages/Cookbooks/_AddRecipeModal.cshtml  (updated — added Ingredients, Instructions, PrepTime, CookTime, Servings)
Recipe.Web/Pages/Recipes/Details.cshtml            (updated — Edit button + modal container)
Recipe.Web/Pages/Recipes/_EditRecipeModal.cshtml   (created — new partial)
```

### 2026-03-03 — Custom Identity Pages (Login, Register, Logout)

**What I Built:**
- Manually scaffolded ASP.NET Core Identity Razor Pages under `Pages/Account/`:
  - `Login.cshtml` + `Login.cshtml.cs` — Email/password sign-in form with RememberMe
  - `Register.cshtml` + `Register.cshtml.cs` — Registration form with confirm password
  - `Logout.cshtml` + `Logout.cshtml.cs` — POST-only sign out, GET redirects home

**Key Decisions:**
- PageModels are **not** thin (no MediatR) — Identity auth is infrastructure, not app domain logic; `SignInManager`/`UserManager` injected directly
- `_LoginPartial.cshtml` updated to point to custom `/Account/Login`, `/Account/Register`, `/Account/Logout` (removed `asp-area="Identity"`)
- `_Layout.cshtml` updated: "My Cookbooks" nav link is now auth-conditional (only visible when signed in)
- `_ValidationScriptsPartial.cshtml` already existed — no duplicate created
- Removed `text-light` classes from nav links (Materia theme uses light navbar)

**File Locations:**
```
Pages/Account/Login.cshtml + .cs
Pages/Account/Register.cshtml + .cs
Pages/Account/Logout.cshtml + .cs
Pages/Shared/_LoginPartial.cshtml  (updated)
Pages/Shared/_Layout.cshtml        (updated — conditional My Cookbooks)
```

### 2026-01-XX — Full Razor Markup Implementation

**What I Built:**
- Implemented complete Razor Page markup for all core pages:
  - `Index.cshtml` — Hero section with conditional CTA (authenticated → /cookbooks, else → /register)
  - `Cookbooks/Index.cshtml` — Cookbook list with HTMX create button
  - `Cookbooks/Details.cshtml` — Cookbook detail with recipes list, breadcrumbs, add recipe button
  - `Recipes/Details.cshtml` — Recipe detail with ingredients/instructions rendering, clone/share/copy link actions
  - `Error.cshtml` — Clean error page with Bootstrap styling
  - `_Layout.cshtml` — Minor update (nav link text "My Cookbooks")

**Partials Created for HTMX:**
- `Cookbooks/_CookbookList.cshtml` — Renders cookbook cards, used as HTMX swap target after create
- `Cookbooks/_CreateCookbookModal.cshtml` — Bootstrap modal with form (Name, Description) for creating cookbooks
- `Cookbooks/_RecipesList.cshtml` — Renders recipe cards, used as HTMX swap target after adding recipe
- `Cookbooks/_AddRecipeModal.cshtml` — Bootstrap modal for adding recipe to cookbook (by PublicId)

**HTMX Patterns Used:**
1. **Modal Loading:**
   ```html
   <button hx-get="/cookbooks?handler=CreateCookbookModal"
           hx-target="#modal-container"
           hx-swap="innerHTML">
   ```
   
2. **Form Submission with Swap:**
   ```html
   <form hx-post="/cookbooks?handler=CreateCookbook"
         hx-target="#cookbook-list"
         hx-swap="outerHTML"
         hx-on::after-request="if(event.detail.successful) closeModal()">
   ```

3. **Progressive Enhancement:** All forms work without HTMX (standard POST), HTMX enhances with inline updates

**PageModel Properties Assumed (Fenster will need to implement):**
- `Cookbooks/Index`: `Result` with `Cookbooks` (IReadOnlyList<CookbookSummary>)
- `Cookbooks/Details`: `Result` with `Name`, `Description`, `PublicId`, `Slug`, `Recipes` (IReadOnlyList<RecipeSummary>)
- `Recipes/Details`: `Result` with `Title`, `Description`, `Ingredients`, `Instructions`, `PrepTime`, `CookTime`, `Servings`, `PublicId`, `Slug`, `OriginalRecipePublicId`
- All partials accept model types matching Application layer DTOs

**Bootstrap 5 Components:**
- Cards for cookbook/recipe lists
- Modals for create/add actions
- Breadcrumbs for navigation hierarchy
- Alerts for info messages (e.g., cloned recipe)
- Forms with validation styling

**Key UI Features:**
- Empty states ("No cookbooks yet", "No recipes in this cookbook")
- Metadata display (prep/cook time, servings) with conditional rendering
- Ingredients/Instructions parsing from newline-delimited strings
- Copy link to clipboard (JS-only, progressive enhancement)
- Share modals (stubbed for future implementation)

### 2026-03-04 — Bootswatch Materia Theme Applied
**What I Changed:**
- Applied Bootswatch Materia theme (Material Design-inspired Bootstrap 5 theme)
- **CDN URL:** `https://cdn.jsdelivr.net/npm/bootswatch@5.3.3/dist/materia/bootstrap.min.css`
- Updated navbar from `navbar-dark bg-dark border-bottom` to `navbar-light bg-white shadow-sm` to complement the Materia theme's light, clean aesthetic
- Kept Bootstrap JS bundle and HTMX CDN links unchanged (Bootswatch only replaces CSS)
- Verified site.css has no dark theme overrides that clash with Materia's light palette
- Scanned all .cshtml files — only navbar had dark classes, now updated

**Theme Colors:** Primary blue (#2196F3), clean white backgrounds, subtle shadows, Material Design typography

## Cross-Agent Update — 2026-03-03T151215Z

**From Fenster:**
- `GetRecipeResponse` now has `bool IsOwner` — use `Model.Result?.IsOwner == true` to gate Edit button (already implemented)
- `Recipes/Details.cshtml.cs` is now `[Authorize]` — unauthenticated users are redirected to login before reaching the page; HTMX calls to `?handler=EditModal` from non-authenticated contexts will redirect
- `OnPostEditAsync()` sets `Response.Headers["HX-Redirect"]` to new URL on success — HTMX will navigate; modal close via `closeEditModal()` may not fire if redirect happens first (this is expected/correct behavior)
- BindProperty names in `Recipes/Details.cshtml.cs`: EditTitle, EditDescription, EditIngredients, EditInstructions, EditPrepTime, EditCookTime, EditServings
- BindProperty names in `Cookbooks/Details.cshtml.cs`: RecipeIngredients, RecipeInstructions, RecipePrepTime, RecipeCookTime, RecipeServings

## Learnings

### 2026-03-04 — Recipe Card Layout Enhancement (_RecipeList.cshtml)

**What I Changed:**
- Enhanced recipe cards in `_RecipeList.cshtml` to support Description, PrepTime, CookTime, and Servings fields
- Cards now use `d-flex flex-column` layout with `mt-auto` on button container to keep "View Recipe" button at bottom
- Description shows as muted small text with 2-line clamp using `-webkit-line-clamp` CSS
- Timing/servings display as small text with emoji icons (🕐 prep, 🍳 cook, 🍽️ servings) in a gap-2 flex row
- All new fields are conditionally rendered with null checks and zero-value checks
- Removed `card-footer` in favor of inline button in `card-body` for more flexible spacing

**Key Pattern:**
- Layout uses flexbox with `mt-auto` to push button to bottom of card while maintaining consistent card heights (h-100)
- `-webkit-line-clamp: 2` truncates description to 2 lines with ellipsis
- Emoji + text pattern for metadata: "🕐 10 min prep  🍳 30 min cook  🍽️ 4 servings"
- Conditional rendering: `@if (!string.IsNullOrWhiteSpace(recipe.Description))` for description, `HasValue` + value > 0 checks for numbers

**Current State:**
- `CookbookRecipeItem` record currently only has: `PublicId`, `Slug`, `Title`, `SortOrder`
- Markup is ready for `Description`, `PrepTime`, `CookTime`, `Servings` properties when Fenster adds them
- Conditional checks ensure no build errors while fields are missing

**File Locations:**
```
Recipe.Web/Pages/Cookbooks/_RecipeList.cshtml  (updated)
```

### 2026-03-04 — Add Recipe In-Place Pattern (Cookbook Details)

**What I Built:**
- Replaced modal-based Add Recipe flow with edit-in-place pattern matching the Edit Recipe pattern
- Created `_RecipeList.cshtml` partial — wraps recipe list + Add Recipe button in `<div id="recipe-list-section">`
- Created `_AddRecipeForm.cshtml` partial — inline add recipe form in Bootstrap card, same `<div id="recipe-list-section">` wrapper
- Updated `Details.cshtml` — now renders `<partial name="_RecipeList" model="Model.Result?.Recipes" />` instead of separate button + partial
- Deleted `_AddRecipeModal.cshtml` and `_RecipesList.cshtml` (old modal pattern)

**HTMX Flow:**
1. Add Recipe button in `_RecipeList`: `hx-get="?handler=AddRecipeForm" hx-target="#recipe-list-section" hx-swap="outerHTML"` → loads add form
2. Cancel button in `_AddRecipeForm`: `hx-get="?handler=RecipeList" hx-target="#recipe-list-section" hx-swap="outerHTML"` → restores list
3. Save button: `hx-post="?handler=AddRecipe" hx-target="#recipe-list-section" hx-swap="outerHTML" hx-include="closest form"` → adds recipe and refreshes list

**Key Pattern:**
- Both partials use `<div id="recipe-list-section">` as outer wrapper for `outerHTML` swap
- Form fields match `CreateRecipeCommand`: Title, Description, Ingredients, Instructions, PrepTime, CookTime, Servings
- Ingredients/instructions use textarea with "one per line" pattern matching edit form
- No modal backdrop/chrome — form appears inline in page layout
- Progressive enhancement maintained — form works without HTMX (standard POST)

**File Locations:**
```
Recipe.Web/Pages/Cookbooks/_RecipeList.cshtml     (created)
Recipe.Web/Pages/Cookbooks/_AddRecipeForm.cshtml  (created)
Recipe.Web/Pages/Cookbooks/Details.cshtml         (updated — uses _RecipeList partial)
Recipe.Web/Pages/Cookbooks/_AddRecipeModal.cshtml (deleted)
Recipe.Web/Pages/Cookbooks/_RecipesList.cshtml    (deleted)
```

### 2026-03-04 — Edit-in-Place Pattern (Replaced Modal)

**What I Changed:**
- Replaced modal edit pattern with edit-in-place pattern using HTMX outerHTML swapping
- Created `_RecipeViewContent.cshtml` partial — wraps recipe display in `<div id="recipe-content">` for HTMX targeting
- Created `_RecipeEditForm.cshtml` partial — inline edit form in Bootstrap card, same `<div id="recipe-content">` wrapper
- Updated `Details.cshtml` — now renders `<partial name="_RecipeViewContent" model="Model.Result" />` instead of inline markup
- Deleted `_EditRecipeModal.cshtml` (old modal pattern)

**HTMX Flow:**
1. Edit button in `_RecipeViewContent`: `hx-get="?handler=EditForm" hx-target="#recipe-content" hx-swap="outerHTML"` → loads edit form
2. Cancel button in `_RecipeEditForm`: `hx-get="?handler=ViewContent" hx-target="#recipe-content" hx-swap="outerHTML"` → restores view
3. Save button: `hx-post="?handler=Edit"` → server sets `HX-Redirect` header, HTMX navigates to updated recipe URL

**Key Pattern:**
- Both partials use `<div id="recipe-content">` as outer wrapper — this is required for `outerHTML` swap to work correctly
- No modal backdrop/chrome — form appears inline in page layout
- Progressive enhancement maintained — form works without HTMX (standard POST)
- Matches Fenster's PageModel handler names: `EditForm`, `ViewContent`, `Edit`

## Cross-Agent Update — 2026-03-04T16:24:55Z

**From Fenster + Coordinator:**
- Add Recipe now uses edit-in-place pattern on cookbook details page (matching Edit Recipe pattern)
- `OnGetAddRecipeFormAsync()` returns `_AddRecipeForm` partial (form appears inline)
- `OnGetRecipeListAsync()` returns `_RecipeList` partial (cancel restores list)
- `OnPostAddRecipeAsync()` now returns `_RecipeList` partial instead of `_RecipesList`
- Swap target is `#recipe-list-section` (outerHTML) — both partials wrap content in outer div for full replacement
- Build succeeded with 0 errors; Playwright-verified: Add, Submit, Cancel all work in-place

## Cross-Agent Update — 2026-03-03T10:32:11Z

**Session: Recipe Cards Enriched**

Fenster successfully extended `CookbookRecipeItem` with Description, PrepTime, CookTime, Servings fields. EF query projection updated to populate these fields from Recipe entity — no additional database queries required. Build verified with 0 errors.

Updated `_RecipeList.cshtml` recipe cards to display:
- Description: 2-line clamp with muted text styling
- Timing/servings metadata row: emoji-prefixed (🕐 prep, 🍳 cook, 🍽️ servings) with full null guards
- Layout: flexbox with `mt-auto` to anchor "View Recipe" button to bottom of card

Both backend and frontend changes integrated and validated.



### 2026-03-04 — Clone Recipe Edit-in-Place Pattern

**What I Built:**
- Created `_RecipeCloneForm.cshtml` partial — inline clone form using edit-in-place pattern (matching Edit Recipe UX)
- Updated `_RecipeViewContent.cshtml` — replaced form POST Clone button with HTMX GET button

**Clone Form Specifics:**
- Model: `@model GetRecipeResponse` (same as edit form)
- Outer wrapper: `<div id="recipe-content">` (required for outerHTML swap)
- Card header: **"Clone Recipe"** (not "Edit Recipe")
- Title field pre-filled with `@(Model?.Title + " [Copy]")` to append " [Copy]" suffix
- All other fields (Description, Ingredients, Instructions, PrepTime, CookTime, Servings) pre-filled from Model
- Form action: `hx-post="?handler=SaveClone"` (not `?handler=Edit`)
- Submit button: **"Create Copy"** with `bi-files` icon (not "Save Changes")
- Cancel button: `hx-get="?handler=ViewContent"` targeting `#recipe-content` with `outerHTML` swap (restores view)

**Field Names:**
All field names identical to `_RecipeEditForm.cshtml` (EditTitle, EditDescription, EditIngredients, EditInstructions, EditPrepTime, EditCookTime, EditServings) so existing BindProperty bindings work without backend changes.

**HTMX Flow:**
1. Clone button in `_RecipeViewContent`: `hx-get="?handler=CloneForm"` → loads clone form inline
2. Cancel button in `_RecipeCloneForm`: `hx-get="?handler=ViewContent"` → restores original view
3. Save button: `hx-post="?handler=SaveClone"` → Fenster will handle creation and redirect

**Key Pattern:**
Consistent with Edit Recipe and Add Recipe edit-in-place patterns — no modals, inline form replacement using HTMX outerHTML swapping on `#recipe-content` div.

**File Locations:**
```
Recipe.Web/Pages/Recipes/_RecipeCloneForm.cshtml    (created)
Recipe.Web/Pages/Recipes/_RecipeViewContent.cshtml  (updated — Clone button now HTMX)
```
