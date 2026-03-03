# SKILL: HTMX Redirect After Slug-Changing POST

## Problem
When a POST mutates data that changes the page URL (e.g., editing a recipe title changes the slug), a standard redirect won't work with HTMX because HTMX handles responses in-place. The client needs to navigate to the new URL.

## Solution Pattern
In the Razor Page POST handler, set the `HX-Redirect` response header before returning `OkResult()`:

```csharp
public async Task<IActionResult> OnPostEditAsync()
{
    var result = await _mediator.Send(new EditRecipeCommand(...));
    Response.Headers["HX-Redirect"] = $"/recipes/{result.PublicId}/{result.NewSlug}";
    return new OkResult();
}
```

## How It Works
- HTMX sees the `HX-Redirect` header on the 200 response
- HTMX performs a full browser navigation to the new URL (not a swap)
- The page reloads at the new canonical URL with the updated slug

## When to Use
- Any HTMX POST that changes the canonical URL of the current page
- Edit operations where a title/name field is part of the URL

## Stack
- ASP.NET Core Razor Pages
- HTMX (any version supporting `HX-Redirect`)

## References
- HTMX docs: https://htmx.org/reference/#response_headers
- Used in: `Recipe.Web/Pages/Recipes/Details.cshtml.cs` — `OnPostEditAsync()`
