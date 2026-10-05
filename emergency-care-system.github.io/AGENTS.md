# AGENTS.md

Instructions for AI coding agents (Claude Code, Codex, Cursor, etc.)
working in this repository — the source for
[emergency-care-system.github.io](https://emergency-care-system.github.io/),
a small static SvelteKit site deployed to GitHub Pages that documents the
[emergency-care-system/emergency-care-system](https://github.com/emergency-care-system/emergency-care-system)
demo repository.

**This file is the canonical agent-facing doc.** `CLAUDE.md` just
imports it. Don't fork guidance between the two — edit here.

## Source of truth

For anything about how the site is *built* — stack, directory layout,
the pattern for adding a new page — read
[`spec/site-architecture.md`](spec/site-architecture.md) first. It's
kept current; this file won't repeat it.

If you change how the site works in a way future agents need to know,
update `spec/site-architecture.md` in the same change — don't leave it
only in a commit message or in this file.

## Commands

```sh
npm install        # install dependencies
npm run dev -- --open  # local dev server
npm run build       # static output to build/
npm run preview     # serve the production build locally
npm run check       # svelte-kit sync + svelte-check -- run after any .svelte/.ts change
```

This directory is part of the
[emergency-care-system](https://github.com/emergency-care-system/emergency-care-system)
monorepo (at `emergency-care-system.github.io/`) and is published to
the standalone `emergency-care-system.github.io` repository with `git
subtree push` — see `README.md`. Use **npm**, not pnpm, here.

There is no separate test suite — `npm run check` and a local
`npm run build` are the correctness bar for a change.

## Working conventions

- This site only exists to point at the source repository — don't
  duplicate content that lives there (the feature scenarios, the test
  suite internals, the app's code) beyond the summaries already on
  these four pages. Link out instead.
- **Never fabricate** a hosted-demo URL, a metric, or a claim not
  already true of the source repository. The source repo's app has no
  hosted deployment — link to the repo and its `pnpm run dev`
  instructions, not to a URL that doesn't exist.
- All patient/staff data anywhere in the linked demo is synthetic —
  say so plainly if a page describes it.
- Keep `src/lib/data/features.ts` in sync with
  `tests-with-given-when-then-features/*.feature` in the source repository by hand (see
  `spec/site-architecture.md`'s directory map entry for it).
- Keep `static/llms.txt` and `static/llms.json` in sync with each other
  and with the live route list.
- Lily's `@lilydesignsystem/svelte-picker-bar` (search, theme, locale,
  text-size, and share pickers) is headless — style new states via the
  fixed class names it renders (`.search-picker*`, `.theme-picker*`,
  etc. in `theme.css`), don't expect any CSS from the packages
  themselves. Tooltips must stay out of flow (`position: absolute`):
  in flow they shift the button from under the pointer on hover.
- This directory doesn't currently have any Claude Code skills (`*-skill/`
  folders) or nested per-directory `AGENTS.md` files — it's four pages,
  small enough that this root file plus `spec/` covers it. Don't add
  either without a concrete, repeated task that justifies the overhead.
