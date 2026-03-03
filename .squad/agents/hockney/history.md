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
