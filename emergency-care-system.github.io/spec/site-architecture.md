# Site architecture

The single source of truth for how this site is built and how to extend
it consistently. `AGENTS.md`, `README.md`, and any other onboarding doc
should point here rather than restate these facts — update this file,
not copies of it.

## Stack

- **SvelteKit 3** (Svelte 5), built with **npm**, fully static via `@sveltejs/adapter-static`.
  Every route sets `export const prerender = true` (from the root
  `src/routes/+layout.ts`) and `trailingSlash = 'always'` — routes build
  to `directory/index.html`, not `directory.html`.
- **Lily Design System** (the `@lilydesignsystem/*` npm scope).
  `@lilydesignsystem/svelte-headless` is a dependency for future
  component needs; the current four pages are plain HTML/CSS (see
  `src/lib/styles/theme.css`) since they don't yet need more than
  headings, cards, and link lists. The header uses
  `@lilydesignsystem/svelte-picker-bar` — search, theme, locale,
  text-size, and share pickers in one row — configured by
  `SitePickers.svelte` in `src/lib/components/site/`. It is headless (no
  shipped CSS): it renders only fixed class names (`.picker-bar`,
  `.search-picker*`, `.theme-picker*`, `.text-size-picker*`,
  `.share-picker*`), which `theme.css` styles directly, rather than
  pulling in Lily's full 45-theme, 492-class-hook catalog aimed at much
  larger sites. Two bar defaults are overridden on purpose: the theme
  list is this site's five themes, and search goes to GitHub code
  search on the source repository (the site has no search index of its
  own). PickerBar always renders a locale picker; the site is English
  only, so `theme.css` hides it.
- **Theming.** Five themes (`light`, `dark`, `high-contrast`,
  `ambulance`, `midnight` — see `src/lib/data/themes.ts`), each a
  standalone stylesheet in `static/themes/<id>.css` that redefines this
  site's own small set of CSS custom properties
  (`--color-bg`, `--color-text`, ...; see `theme.css`'s header
  comment). `app.html` hardcodes an initial
  `<link id="lily-theme" href="/themes/light.css">` and applies a
  visitor's stored choice (`localStorage["ecs-theme"]`) before first
  paint, so `light` is also correct with JavaScript disabled. Text size
  works the same way via `data-text-size` (Lily's seven-step scale,
  `smallest` … `largest`, `normal` being the default) /
  `--user-font-scale` and `localStorage["ecs-text-size"]`.
- **GitHub Pages**, deployed by `.github/workflows/deploy.yml` on every
  push to `main` of the standalone `emergency-care-system.github.io`
  repository: `npm ci && npm run build`, then
  `actions/upload-pages-artifact` + `actions/deploy-pages`. The Pages
  source is "GitHub Actions". That workflow only runs in the standalone
  repository: GitHub ignores `.github/` in a monorepo subdirectory.
- **Monorepo + git subtree.** This directory is
  `emergency-care-system.github.io/` in the
  `emergency-care-system` monorepo. It is published with
  `git subtree push --prefix=emergency-care-system.github.io pages main`
  (remote `pages` = the standalone repository), so edit it here, never
  in the standalone repository, or the next subtree push is rejected as
  non-fast-forward.
- No custom domain, no `static/CNAME` — this is `emergency-care-system.github.io`.

## Directory map

- `src/routes/` — one folder per route (`+page.svelte`, plus
  `sitemap.xml/+server.ts`, which generates the XML sitemap from the route
  tree at build time).
- `src/lib/components/site/` — `Seo.svelte`, `SiteNav.svelte`,
  `FooterNav.svelte`, and `SitePickers.svelte` (the configured
  PickerBar) rendered in `SiteNav.svelte`.
- `src/lib/data/themes.ts` — the five themes offered by the theme picker,
  each backed by a `static/themes/<id>.css` file.
- `src/lib/data/features.ts` — the 22 features (title, user story,
  slug), kept in sync by hand with `tests-with-given-when-then-features/*.feature` in the
  [source repository](https://github.com/emergency-care-system/emergency-care-system).
  If a feature file is added, renamed, or removed there, update this
  list in the same change.
- `src/lib/styles/theme.css` — theme tokens and global class hooks
  (`.page-hero`, `.card`, `.card-grid`, `.link-list`, `.badge`,
  `.notice`) shared by every route — routes don't define their own
  `<style>` blocks for these.
- `static/` — `robots.txt`, `llms.txt` / `llms.json`, `themes/*.css`.
- `spec/` — this directory.

## Adding a new page

1. `<Seo {title} {description} path="/route/" />` at the top of the
   new `+page.svelte` — `title` ends in `— Emergency Care System`,
   `description` is one sentence.
2. A `<section class="page-hero">` with an `<h1>` and one-to-two intro
   `<p>` tags.
3. Content as `<div class="card">` / `<div class="card-grid">` blocks —
   no bespoke CSS needed, it's all in `theme.css`.
4. **Sitemap**: nothing to do — `sitemap.xml/+server.ts` lists every
   `src/routes/**/+page.svelte` automatically. To keep a page out of it,
   add its path to `UNLISTED` there; dynamic `[param]` routes are
   skipped, so list their concrete paths in `EXTRA_ROUTES`.
5. **Decide navigation visibility** — add it to `SiteNav.svelte` and/or
   `FooterNav.svelte` if it should be discoverable from every page.
6. Update `static/llms.txt` and `static/llms.json` together — they're
   two representations of the same page list, not two independent
   documents — unless the page is deliberately unlisted.

## Adding a new theme

1. Add a `{ id, label }` entry to `src/lib/data/themes.ts`.
2. Add `static/themes/<id>.css`, redefining every custom property the
   other theme files define (`color-scheme`, `--color-bg`,
   `--color-bg-subtle`, `--color-text`, `--color-text-muted`,
   `--color-border`, `--color-accent`, `--color-link`) under
   `:root[data-theme='<id>']` — copy an existing file (not `light.css`,
   whose selector also matches bare `:root` for the pre-JS default) as
   the template.
3. No other change needed — the theme picker builds the stylesheet URL
   from the slug, and `theme.css`'s structural rules already reference
   these custom properties rather than hardcoded colors.

## Content rules

- No fabricated stats, hosted-demo URLs, testimonials, or claims — this
  site links to the source repository and its own `.feature` files
  rather than restating numbers that could drift from them.
- All patient/staff data referenced anywhere is synthetic; say so
  plainly wherever the site describes the demo app.
- Keep `src/lib/data/features.ts`, `tests-with-given-when-then-features/*.feature` in the
  source repo, and the `/features/` page's rendering in sync — the
  feature list here is a hand-maintained mirror, not a build-time
  fetch, so an edit to one side needs the matching edit on this side.
