# Routing Rules

## Signal → Agent Mapping

| Signal / Domain | Agent |
|-----------------|-------|
| Schema design, EF Core entities, domain model | Keaton |
| Architecture decisions, tech tradeoffs | Keaton |
| Code review, PR review | Keaton |
| Auth strategy, Identity configuration | Keaton or Fenster |
| Services (PublicIdService, SlugService, AuthorizationService) | Fenster |
| EF Core DbContext, migrations, queries | Fenster |
| ASP.NET Core Identity, user management | Fenster |
| Share/permission logic, data access | Fenster |
| Razor Pages (.cshtml, .cshtml.cs) | Hockney |
| HTMX interactions, partial views | Hockney |
| Bootstrap 5 layout, components, styling | Hockney |
| Forms, modals, UI partials | Hockney |
| Unit tests, integration tests | McManus |
| Edge cases, permission scenarios, test coverage | McManus |
| Memory, decisions, session logs | Scribe |
| Work queue, backlog, GitHub issues | Ralph |

## Multi-Domain Requests

- "Build the cookbook details page" → Fenster (data/handlers) + Hockney (UI) in parallel
- "Set up authentication" → Keaton (strategy) then Fenster (implementation)
- "Add sharing feature" → Keaton (design) + Fenster (backend) + Hockney (UI) + McManus (tests)
