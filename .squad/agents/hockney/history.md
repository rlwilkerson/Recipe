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
