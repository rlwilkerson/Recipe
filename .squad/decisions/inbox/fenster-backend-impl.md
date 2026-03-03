# Fenster Backend Implementation Decisions

**Date:** [Current Session]  
**Author:** Fenster (Backend Dev)  
**Status:** Implemented

## Key Architectural Decisions

### 1. Service Signatures

**PublicIdService:**
- Changed from single `GeneratePublicId()` to separate `GenerateForCookbookAsync()` and `GenerateForRecipeAsync()` 
- **Rationale**: Each entity type has its own table for uniqueness checks. Separate methods make the DB context dependency explicit and allow proper async/await for DB queries.
- **Implementation**: Base62 encoding (0-9a-zA-Z), 10-character random strings, retry loop on collision (max 10 attempts).

**SlugService:**
- Kept synchronous `GenerateSlug(string input)` - no DB dependency
- **Rationale**: Slug generation is pure string manipulation. Collisions handled at application layer (slugs aren't unique DB constraints).

### 2. Access Control Pattern

**Inline Authorization in Get Handlers:**
- Get handlers check userId inline and return `null` for unauthorized access
- **Rationale**: 
  - Simpler PageModel code
  - Returns 404 for unauthorized access (security best practice - don't reveal existence)
  - Reduces round-trips (single query instead of authorization query + get query)

**Separate Authorization Handlers:**
- Created GetCookbookAccessHandler and GetRecipeAccessHandler returning `bool`
- **Rationale**: Reusable for programmatic access checks in other handlers (e.g., ShareCookbook verifies ownership before creating share)

### 3. Recipe Access Rules

**Transitive Access Through Cookbooks:**
- Users can read a recipe if they can read ANY cookbook containing that recipe
- **Implementation**: GetRecipeHandler includes cookbooks in EF query, checks each cookbook's ownership and shares
- **Rationale**: Matches charter requirement - adding recipe to shared cookbook grants read access

### 4. Share Entity Design

**Nullable Foreign Keys:**
- `CookbookId?` and `RecipeId?` are nullable (exactly one is set based on Scope)
- **Rationale**: Single Share table for both scopes. Alternative would be separate CookbookShare/RecipeShare tables (more normalized but more complex).

**OwnerId vs SharedByUserId:**
- Used `OwnerId` (denormalized from resource owner) instead of `SharedByUserId`
- **Rationale**: Owner of the resource creates shares. Could be different from logged-in user if delegation exists. For now, same concept.

### 5. Migration Naming

**Migration Name: `InitialCreate`**
- **Rationale**: Standard naming for first migration. Clear intent. Includes all entities (Identity tables already exist from template, these are our app tables).

## Open Questions / Future Considerations

1. **PublicId Collision Handling**: Currently throws after 10 retries. Should log/alert on collisions (shouldn't happen with 62^10 keyspace).

2. **Slug Uniqueness**: Slugs aren't enforced unique. If two cookbooks/recipes have same slug, routing handles via PublicId (primary route param). Consider uniqueness constraint?

3. **Share Upsert Logic**: ShareCookbook/ShareRecipe update permission if share exists. Should we track who updated it and when?

4. **Soft Deletes**: No soft delete logic. Cascade deletes will remove shares when cookbook/recipe deleted. Is this desired?

5. **Authorization Caching**: Every Get query re-checks authorization. Consider caching access decisions?
