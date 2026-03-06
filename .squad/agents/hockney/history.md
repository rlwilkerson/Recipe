# Hockney — History

## Core Context

**Project:** Cookbook Web Application  
**User:** Rick Wilkerson  
**Stack:** ASP.NET Core Razor Pages, EF Core, ASP.NET Core Identity, HTMX, Bootstrap 5, PostgreSQL, .NET Aspire

### Domain & Responsibilities
- Razor Page markup (.cshtml templates)
- HTMX interactions (hx-get, hx-post, hx-target, hx-swap, hx-boost, hx-on)
- Bootstrap 5 layout, responsive design, components
- Partial views for modals, lists, forms, cards
- Progressive enhancement — pages work without JavaScript

### Key Patterns & Conventions
- All cookbook URLs: `/cookbooks/{publicId}/{slug}` — publicId required, slug decorative
- All recipe URLs: `/recipes/{publicId}/{slug}` — same pattern
- HTMX handlers via query string: `?handler=RecipesPartial`, `?handler=AddRecipe`, etc.
- Razor Page route declarations: `@page "/cookbooks/{publicId}/{slug}"`
- `[FromRoute]` properties on PageModel for route parameters
- Partial views use query string handlers for HTMX GET/POST operations
- No `@page` directive in partials (not standalone pages)

### Page Structure
```
/Pages
  /Cookbooks
    Index.cshtml + .cs         → /cookbooks (list all)
    Details.cshtml + .cs       → /cookbooks/{publicId}/{slug} (detail view)
    _CookbookList.cshtml       → list of cookbooks (partial)
    _RecipeList.cshtml         → list of recipes (partial)
    _AddRecipeForm.cshtml      → add recipe inline form (partial)
    _RecipeViewContent.cshtml  → recipe display (partial)
    _RecipeEditForm.cshtml     → recipe edit inline form (partial)
    _CreateModal.cshtml        → cookbook create modal (legacy, being replaced)
    _ShareModal.cshtml         → share modal (partial)
  /Recipes
    Details.cshtml + .cs       → /recipes/{publicId}/{slug} (detail view)
    _RecipeViewContent.cshtml  → recipe display (partial, shared with Cookbooks)
    _RecipeEditForm.cshtml     → recipe edit form (partial, shared)
    _ShareModal.cshtml         → share modal (partial)
  Index.cshtml                 → / (home)
  _Layout.cshtml               → site layout
  _LoginPartial.cshtml         → login/user nav partial
```

### HTMX Patterns
- **Edit-in-place:** `<div id="section-id">` wrapper with `hx-swap="outerHTML"` to replace entire section
- **Form submission:** `hx-post="?handler=Name"` with `hx-include="closest form"` to include form data
- **Modal replacement:** Legacy pattern being phased out in favor of edit-in-place
- **Progressive enhancement:** All forms work without HTMX (standard POST to same handler)

### Bootstrap & Styling
- Bootstrap 5 via Bootswatch Materia theme (CDN)
- Flexbox layout patterns: `d-flex flex-column`, `mt-auto` for button positioning
- Card layouts with `h-100` for consistent heights
- Text truncation: `-webkit-line-clamp: 2` for multi-line ellipsis (better than `text-truncate`)
- Color utilities: `text-muted`, `btn-sm`, `btn-warning`, etc.
- Spacing: `gap-2`, `shadow-sm`, responsive padding

### Recent Work Summary (2026-03-03 to 2026-03-04)
- ✅ Custom login/register/logout pages at `/Account/` (no Identity area scaffolding)
- ✅ Bootswatch Materia theme applied (light navbar, clean styling)
- ✅ Edit-in-place pattern for recipes (replaces modal, uses HTMX outerHTML swapping)
- ✅ Add-in-place pattern for recipes in cookbooks (inline form, no modal)
- ✅ Recipe card layout enhancement (flexbox, description clamp, timing metadata with emoji)
- ✅ Extended recipe cards with Description, PrepTime, CookTime, Servings fields
- ✅ Cross-agent coordination with Fenster on data models and handlers
- ✅ All pages fully responsive, progressive enhancement maintained

## Session History (Detailed)

### 2026-03-03: Identity Pages (Login, Register, Logout)
- Manually scaffolded custom pages at `/Account/` (no Identity area)
- PageModels use SignInManager/UserManager directly
- Updated _LoginPartial and _Layout for auth-conditional navigation

### 2026-03-03: Edit Recipe UI (Modal Pattern)
- Created _EditRecipeModal.cshtml partial with pre-populated form
- Added "Edit Recipe" button (owner-only, btn-warning) to Details page
- HTMX POST to `?handler=Edit`, modal closes on success

### 2026-03-04: Switch to Edit-in-Place Pattern
- Replaced modal pattern with edit-in-place using HTMX outerHTML swapping
- Created _RecipeViewContent and _RecipeEditForm partials
- `#recipe-content` div swaps between view and edit modes
- Cleaner UX, less JavaScript, consistent with web app patterns

### 2026-03-04: Add Recipe In-Place Pattern
- Replaced modal add recipe flow with edit-in-place pattern
- Created _RecipeList and _AddRecipeForm partials
- Uses `#recipe-list-section` swap target for consistency
- Progressive enhancement maintained (works without HTMX)

### 2026-03-04: Recipe Card Layout Enhancement
- Extended recipe cards with Description, PrepTime, CookTime, Servings
- Used flexbox with `mt-auto` for button positioning
- Description uses `-webkit-line-clamp: 2` for multi-line truncation
- Timing metadata displayed with emoji icons (🕐, 🍳, 🍽️)

### 2026-03-05: Admin CLI Plan Review (UX Perspective)
- Reviewed admin CLI + admin API architecture
- Confirmed architecture sound: lean CLI, reusable API, vertical-slice + MediatR
- Identified 6 UX gaps: hybrid balance, session persistence, error handling, scope, confirmations, rollout
- Provided detailed recommendations in orchestration log
