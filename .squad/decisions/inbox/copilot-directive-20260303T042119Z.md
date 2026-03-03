### 2026-03-03T04:21:19Z: User directive — Vertical Slice Architecture with MediatR

**By:** Rick Wilkerson (via Copilot)

**What:** Use Vertical Slice Architecture with MediatR. All application functionality (commands, queries, handlers, responses) must live in a `Features/` folder, completely separate from Razor Pages. Pages are thin — they inject `IMediator` and call `Send()` only. No business logic in PageModels.

**Why:** Unit testability (handlers testable without web layer) and future client support (API, mobile, CLI can all dispatch the same MediatR requests).

**Structural implication:**
```
/Features
  /Cookbooks
    /CreateCookbook/   → Command + Handler + Response
    /GetCookbook/      → Query + Handler + Response
    /ListCookbooks/
    /AddRecipeToCookbook/
    /ShareCookbook/
  /Recipes
    /CreateRecipe/
    /GetRecipe/
    /CloneRecipe/
    /ShareRecipe/
  /Shares/
/Pages
  /Cookbooks
    Index.cshtml.cs    → IMediator.Send(new ListCookbooksQuery(...))
    Details.cshtml.cs  → IMediator.Send(new GetCookbookQuery(publicId))
```

**Why:** User request — captured for team memory
