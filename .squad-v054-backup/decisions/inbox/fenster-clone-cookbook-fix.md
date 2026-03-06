# Decision: Fix Clone Recipe Cookbook Saving

**Date:** 2026-03-04  
**Agent:** Fenster (Backend Dev)  
**Status:** Implemented

## Problem

When cloning a recipe, the new recipe was created successfully but was not added to any cookbooks. This left cloned recipes orphaned and inaccessible through cookbook navigation.

**Root Cause:** The `OnPostSaveCloneAsync` handler in `Recipe.Web/Pages/Recipes/Details.cshtml.cs` called `CreateRecipeCommand` to create the new recipe, but did not call `AddRecipeToCookbookCommand` to preserve the cookbook membership of the original recipe.

## Decision

Extended the recipe cloning flow to preserve cookbook membership by:

1. **Adding cookbook tracking to GetRecipeResponse** — Extended the response record with `IReadOnlyList<string> CookbookPublicIds` to track which cookbooks contain a recipe
2. **Fetching original recipe before cloning** — Modified `OnPostSaveCloneAsync` to first fetch the original recipe to get its cookbook memberships
3. **Adding clone to original's cookbooks** — After creating the cloned recipe, loop through the original's cookbooks and add the clone to each using `AddRecipeToCookbookCommand`

## Implementation

### Files Modified

**1. Recipe.Web/Features/Recipes/GetRecipe/GetRecipeResponse.cs**
- Added `IReadOnlyList<string> CookbookPublicIds` as the last parameter in the record

**2. Recipe.Web/Features/Recipes/GetRecipe/GetRecipeHandler.cs**
- Updated the response projection to include `recipe.CookbookRecipes.Select(cr => cr.Cookbook.PublicId).ToList()`
- Used existing `.Include(r => r.CookbookRecipes).ThenInclude(cr => cr.Cookbook)` — no additional queries needed

**3. Recipe.Web/Pages/Recipes/Details.cshtml.cs**
- Added using: `using Recipe.Web.Features.Cookbooks.AddRecipeToCookbook;`
- Modified `OnPostSaveCloneAsync`:
  ```csharp
  // Get original recipe to know which cookbooks it belongs to
  var original = await _mediator.Send(new GetRecipeQuery(PublicId, userId));
  
  // Create the cloned recipe
  var result = await _mediator.Send(new CreateRecipeCommand(...));
  
  // Add to same cookbook(s) as original
  foreach (var cookbookPublicId in original?.CookbookPublicIds ?? [])
  {
      await _mediator.Send(new AddRecipeToCookbookCommand(cookbookPublicId, result.PublicId, null));
  }
  ```

## Alternatives Considered

### 1. Pass cookbook IDs through form submission
**Rejected** — Would require changing the clone form to include hidden cookbook fields, increasing complexity and exposing cookbook IDs in client-side HTML

### 2. Modify CreateRecipeCommand to accept cookbook IDs
**Rejected** — CreateRecipeCommand is used in multiple contexts (add recipe to cookbook, standalone recipe creation). Adding cookbook IDs would couple recipe creation to cookbook membership inappropriately

### 3. Create separate CloneRecipeCommand
**Rejected** — Over-engineering. The clone operation is essentially "create + add to cookbooks", which can be composed from existing commands without introducing a new handler

## Trade-offs

**Pros:**
- Reuses existing commands (`CreateRecipeCommand`, `AddRecipeToCookbookCommand`) — no new handlers needed
- Minimal changes to existing contracts (only added one field to GetRecipeResponse)
- Cookbook membership is automatically preserved when cloning

**Cons:**
- Requires an extra database query to fetch the original recipe's cookbooks before cloning (but this data is needed anyway for the clone form)
- Multiple sequential commands instead of a single atomic operation (though this is acceptable given the low risk of partial failure)

## Verification

- **Build:** Succeeded with 0 errors
- **Grep check:** Confirmed only one place constructs `GetRecipeResponse` (no breaking changes to other code)
- **Pattern alignment:** Follows established vertical slice architecture — handlers remain focused, PageModel orchestrates multiple commands

## Future Considerations

If cookbook cloning becomes more complex (e.g., copying tags, preserving sort order, handling errors), consider creating a dedicated `CloneRecipeCommand` handler that encapsulates the full clone operation as a single transaction.
