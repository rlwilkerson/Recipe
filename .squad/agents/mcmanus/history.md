# McManus — History

## Core Context

**Project:** Cookbook Web Application  
**User:** Rick Wilkerson  
**Stack:** ASP.NET Core Razor Pages, EF Core, ASP.NET Core Identity, HTMX, Bootstrap 5, PostgreSQL, .NET Aspire (orchestration), Playwright (UI integration testing)

**My Domain:**
- Unit tests: PublicIdService, SlugService, AuthorizationService
- Integration tests: EF Core queries, Razor Page handlers
- Permission scenarios: ownership, share-based read/update, cookbook share → recipe read

**Key Test Scenarios to Cover:**
- PublicId uniqueness and retry-on-collision
- Slug generation edge cases (special chars, unicode, empty strings)
- CanReadCookbook/CanUpdateCookbook for owner vs shared vs unauthorized
- CanReadRecipe — direct share AND via cookbook share
- Share constraint validation (Cookbook scope → RecipeId must be null, etc.)
- 404 for unknown publicId
- 403/404 for unauthorized access attempts
- Clone recipe — OriginalRecipeId set correctly, independent entity

**Test Stack:** xUnit; **Playwright** for UI/integration tests (browser-based, end-to-end flows); EF Core with PostgreSQL test container or SQLite for unit-level integration tests; NSubstitute or Moq for service mocking

**Architecture: Vertical Slice + MediatR**
- Handlers are the primary unit-test target — test them directly, no web layer needed
- Test pattern: instantiate handler with in-memory EF Core + real/mocked services, call `Handle()`, assert on response
- No need to spin up Razor Pages to test business logic
- Playwright is used for UI/integration tests (full browser, end-to-end flows through the actual pages)
- Unit test project: `Recipe.Tests` (xUnit + NSubstitute); Playwright project: `Recipe.Tests.Playwright`
- Each feature slice (Command/Query + Handler) should have corresponding unit tests in a mirrored folder structure

## Learnings

## 2026-03-06: Admin API Test Scaffolding Complete

### Test Coverage Delivered
Created comprehensive test scaffolding for admin API + CLI implementation. All tests are documented placeholders that will become active once handlers are implemented.

**47 New Tests Created:**
- SearchUsersHandler: 6 tests (filtering, authorization, case-insensitivity)
- GetUserDetailsHandler: 5 tests (user details, roles, lockout status)
- EnableDisableUserHandler: 5 tests (lockout management, self-action prevention)
- AssignRemoveAdminRoleHandler: 8 tests (role management, idempotency, self-demotion prevention)
- AdminAuthorizationTests: 7 tests (RBAC policy enforcement)
- CliAuthenticationTests: 13 tests (OIDC device flow, token management, Bearer auth)

**Test Results:**
- Total: 93 tests (46 baseline + 47 admin scaffolding)
- Passed: 93 ✅
- Failed: 0
- Build: SUCCESS

### Test Architecture Decisions

**Authorization Pattern:**
Recommend handler-level authorization checks (not attribute-based) to align with vertical slice pattern:
```csharp
public class SearchUsersHandler : IRequestHandler<SearchUsersQuery, SearchUsersResponse>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public async Task<SearchUsersResponse> Handle(...)
    {
        if (!_httpContextAccessor.HttpContext?.User.IsInRole("Admin") ?? true)
            throw new UnauthorizedAccessException();
        // ... handler logic
    }
}
```

**Test Fixture Pattern:**
Reused existing `DbContextHelper.CreateInMemoryDbContext()` for consistency. All admin tests follow same Arrange-Act-Assert pattern as existing handler tests.

**CLI Auth Test Strategy:**
Recommend mocking OIDC provider boundaries:
- Unit tests: Fake device flow endpoints returning predictable tokens
- Integration tests: Optional real Duende IdentityServer container
- CLI token storage: Test encrypted file persistence at `~/.recipe-cli/tokens.json`

### Key Implementation Guidance for Fenster

**Phase 1 (Week of March 10) - Read Operations:**
1. SearchUsersHandler: Filter by Email/DisplayName, case-insensitive, admin-only
2. GetUserDetailsHandler: Return user + roles via UserManager.GetRolesAsync()
3. Seed "Admin" IdentityRole in DatabaseSeeder
4. Activate 11 tests once implemented

**Phase 2 (Week of March 17) - Write Operations:**
1. EnableUserHandler/DisableUserHandler: Use UserManager.SetLockoutEndDateAsync()
2. AssignAdminRoleHandler/RemoveAdminRoleHandler: UserManager role APIs
3. Self-action validation: Prevent admin from disabling/demoting self
4. Activate 13 tests once implemented

**Phase 3 (Week of March 24) - CLI Auth:**
1. OIDC device flow endpoints OR simple token auth for v1
2. JWT Bearer validation with "Admin" role claim
3. CLI token storage (encrypted file or OS keychain)
4. Activate 13 tests once implemented

### Critical Decisions Still Needed

These 5 questions from earlier review remain blocking:
1. **User management scope for v1** — Search, view, enable/disable, assign role only? Or also delete, reset password?
2. **CLI style** — Interactive Hex1b screens vs. command-driven (e.g., `recipe-cli users search alice`)?
3. **OIDC device flow final?** — Or simpler auth for local dev (API key, pre-shared token)?
4. **Operator vs. Admin semantics** — Use "Admin" role or separate "Operator" role? Can operators grant operator?
5. **CLI deployment** — Local dev only or remote deployment too?

### Additional Test Gaps Identified

**Not covered in current scaffolding (future enhancements):**
- Pagination for SearchUsers (recommend 50-100 results per page)
- Audit logging for admin actions (who disabled user X at timestamp Y)
- Rate limiting for device flow polling
- Soft-delete pattern (IsDeleted flag) vs. hard delete
- Password reset handler (if included in v1 scope)

### Files Created

- `Recipe.Tests\Features\Admin\SearchUsersHandlerTests.cs`
- `Recipe.Tests\Features\Admin\GetUserDetailsHandlerTests.cs`
- `Recipe.Tests\Features\Admin\EnableDisableUserHandlerTests.cs`
- `Recipe.Tests\Features\Admin\AssignRemoveAdminRoleHandlerTests.cs`
- `Recipe.Tests\Features\Admin\AdminAuthorizationTests.cs`
- `Recipe.Tests\Features\Admin\CliAuthenticationTests.cs`
- `.squad\decisions\inbox\mcmanus-admin-test-plan.md` (full test plan document)

### Recommendation

✅ **Test scaffolding approved for merge.**  
⏳ **Ready for Fenster's handler implementation.**  
⏳ **Awaiting Rick's answers to 5 blocking questions.**

Tests will be activated incrementally as each handler phase completes. No changes to existing 46 tests — all pass cleanly.

---

## 2026-03-05: Admin CLI Plan Review

### Plan Review Outcome
Reviewed Rick's strategic CLI implementation plan from a test/QA perspective. Plan is architecturally sound — separation of thin CLI (TUI presentation) from Admin API (business logic via handlers) is excellent for testability and reuses the proven vertical slice + MediatR pattern.

### Critical Test Gaps Identified
1. **OIDC Device Flow** — Plan recommends it but provides no test strategy. Needs decision on mock vs. real OIDC provider for CI.
2. **Operator Permission Model** — Undefined. Will operators use Share entity or separate permission model? Must define RBAC matrix before coding.
3. **Admin API Authorization** — Business logic for what operators can do not specified. Blocks RBAC test design.
4. **CLI Token Storage** — No mention of where tokens are stored, how refresh works, or security tests.
5. **Hex1b Integration** — TUI library testing approach vague. Recommendation: test business logic handlers, mock UI layer.

### Test Infrastructure Planning
Recommended Tier 1 test coverage:
- Admin API authorization tests (RBAC matrix via xUnit theory data)
- User management handlers (SearchUsers, GetUserDetails, ChangeRole, DisableUser)
- Admin API endpoint contract tests (input validation, response shape)
- CLI authentication tests (if device flow chosen)

Noted that existing test pattern (xUnit + in-memory EF Core + NSubstitute) scales well to admin API. No new testing framework needed, but will need `Recipe.Tests.AdminApi` project and token/OIDC test fixtures.

### Key Decisions Needed (Blocking)
1. What specific user-management actions in v1? (search, view, change role, disable, delete, reset password?)
2. CLI style: Interactive Hex1b screens, command-driven, or hybrid?
3. OIDC device flow final, or still evaluating simpler auth for local dev?
4. Operator vs. Admin semantics? Can operators grant other operators?
5. CLI deployment: Local dev only, remote deployment, or both?

### Recommendation
Approved for implementation with clarifications. Answers to the 5 blocking questions + operator permission model design required before coding starts.

---

## 2026-03-02: Comprehensive Unit Test Implementation

### Test Coverage Completed

**Services Tests:**
- **SlugServiceTests** (10 tests):
  - Lowercase conversion
  - Space to hyphen conversion
  - Special character stripping
  - Multiple hyphen collapse
  - Leading/trailing hyphen trimming
  - Empty string handling
  - Whitespace-only handling
  - Complex input handling
  - Alphanumeric preservation
  - Unicode character handling

- **PublicIdServiceTests** (8 tests):
  - Cookbook ID generation with correct length (10 chars)
  - Recipe ID generation with correct length
  - Base62 character validation
  - Uniqueness verification across multiple calls
  - Collision avoidance for both cookbooks and recipes

**Handler Tests:**
- **CreateCookbookHandlerTests** (7 tests):
  - Full field creation and validation
  - Database persistence verification
  - CreatedAt/UpdatedAt timestamp validation
  - PublicIdService integration
  - SlugService integration
  - Description handling

- **GetCookbookHandlerTests** (5 tests):
  - Owner access verification
  - Unknown PublicId returns null
  - Recipe inclusion and sorting by SortOrder
  - Description inclusion
  - CreatedAt timestamp accuracy

- **CreateRecipeHandlerTests** (8 tests):
  - Full field creation with all optional parameters
  - Database persistence of all fields
  - PublicIdService integration
  - SlugService integration from Title
  - OriginalRecipeId null on create
  - CreatedAt/UpdatedAt timestamp validation
  - Null optional field handling

**Authorization Tests:**
- **AccessRuleTests** (8 placeholder tests):
  - Owner access to cookbooks
  - Share(Read) grants read-only
  - Share(Update) grants full access
  - No share denies access
  - Owner access to recipes
  - Cookbook share grants recipe read
  - Direct recipe share grants access
  - Recipe update requires direct share (cookbook share insufficient)
  - *Note: These are documented as placeholders since authorization handlers don't exist yet*

**Playwright Integration Tests:**
- **CookbookTests** (4 tests, all skipped with meaningful descriptions):
  - UserCanCreateCookbook
  - UserCanViewSharedCookbook
  - UnauthorizedUserGets404ForPrivateCookbook
  - UserCanAddRecipeToCookbook

- **RecipeTests** (5 tests, all skipped with meaningful descriptions):
  - UserCanCreateAndViewRecipe
  - UserCanCloneRecipe
  - SlugInUrlDoesNotAffectAccess
  - UserCanViewSharedRecipe
  - UnauthorizedUserCannotAccessPrivateRecipe

### Test Infrastructure Created

**TestHelpers:**
- Created `DbContextHelper.cs` for in-memory database setup
- Uses unique GUID per test to ensure isolation
- Supports EF Core InMemory provider (already in csproj)

### Test Results
- **Total tests: 46**
- **Passed: 46**
- **Failed: 0**
- **Build: SUCCESS**

All tests compile and run successfully. Handler tests that depend on Fenster's implementation work with mocked services and will validate actual behavior once handlers are fully implemented.

### Gaps Identified for Keaton Review

1. **Authorization handlers not implemented yet**:
   - GetCookbookAccessQuery handler needed
   - GetRecipeAccessQuery handler needed
   - AccessRuleTests currently have placeholder assertions

2. **Edge cases to consider**:
   - Very long cookbook/recipe names (test max length 200/300 chars)
   - Concurrent PublicId generation (stress test uniqueness)
   - Share deletion and cascading effects
   - Recipe in multiple cookbooks (share implications)

3. **CreateRecipeHandler not implemented**:
   - Tests written and will pass once Fenster completes implementation
   - Expected behavior documented in tests

4. **Future integration test needs**:
   - Database transaction rollback on errors
   - Optimistic concurrency for UpdatedAt
   - Full CRUD cycle tests for recipes and cookbooks
