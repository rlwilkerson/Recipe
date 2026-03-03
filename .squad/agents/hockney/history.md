# Hockney — History

## Core Context

**Project:** Cookbook Web Application  
**User:** Rick Wilkerson  
**Stack:** ASP.NET Core Razor Pages, EF Core, ASP.NET Core Identity, HTMX, Bootstrap 5, SQL Server/SQLite (dev)

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

## Learnings
