# Decision: Bootswatch Materia Theme

**Date:** 2026-03-04  
**By:** Hockney (Frontend Dev)  
**Requested by:** Rick Wilkerson

## What
Applied the Bootswatch Materia theme to the Cookbook web application, replacing the standard Bootstrap 5 CSS with a Material Design-inspired theme.

## Why
Materia provides a modern, clean aesthetic with Material Design principles: bold primary colors, subtle shadows, and clean typography.

## Technical Details

### Bootstrap CSS CDN Replaced
- **OLD:** `https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css`
- **NEW:** `https://cdn.jsdelivr.net/npm/bootswatch@5.3.3/dist/materia/bootstrap.min.css`

### Navbar Updated
Changed from dark theme to light theme in `_Layout.cshtml`:
- **OLD:** `navbar-dark bg-dark border-bottom`
- **NEW:** `navbar-light bg-white shadow-sm`

### What Stayed the Same
- Bootstrap JS bundle CDN (unchanged — Bootswatch only replaces CSS, not JavaScript)
- HTMX CDN link (unchanged)
- site.css (no dark theme overrides found)
- All other .cshtml files (no hardcoded dark classes found)

## Impact
- All pages now use Materia's primary blue (#2196F3) for buttons, links, and accents
- Clean white backgrounds with subtle Material Design shadows
- Light navbar with shadow instead of dark navbar with border
- Improved visual hierarchy and modern aesthetic

## Files Modified
- `Recipe.Web/Pages/Shared/_Layout.cshtml`

## Team Notes
Bootswatch themes are Bootstrap-compatible drop-in replacements. If a different Bootswatch theme is desired in the future, simply swap the CDN URL (e.g., `/materia/` → `/flatly/` or `/lux/`).
