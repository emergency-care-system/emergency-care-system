# AGENTS.md

Instructions for AI coding agents (Claude Code, Codex, Cursor, etc.)
working in this repository — a public, free, open-source demonstration
of how to plan an emergency care system, from higher-level use cases to
lower-level test automation. All data anywhere in the repo is synthetic;
this is not a real emergency care system.

**This file is the canonical agent-facing doc**, kept short: it holds the
rules that must never be missed and an index of the detail in
[`AGENTS/`](AGENTS/). Tools that look for a different filename (such as
`CLAUDE.md`) should import this file; don't fork guidance between them —
edit here and in `AGENTS/`.

## Source of truth

For anything about how the repo is *built* — stack, directory layout,
package manager, authentication, the pattern for adding a new feature —
read [`spec/index.md`](spec/index.md) first. It's kept current; this
file won't repeat it.

For the twelve test suites specifically — their shared `data-testid`
contract, the one-browser-per-file architecture, and how to port a
change across all twelve — read [`spec/testing.md`](spec/testing.md).

If you change how the repo works in a way future agents need to know,
update the relevant `spec/*.md` or `AGENTS/*.md` file in the same change
— don't leave it only in a commit message.

## Agent guides (`AGENTS/`)

- [`AGENTS/commands.md`](AGENTS/commands.md) — install, dev, check,
  build, and all twelve test-suite commands (pnpm, plus the pip, dotnet,
  mvn, cargo and npm exceptions).
- [`AGENTS/conventions.md`](AGENTS/conventions.md) — the working
  conventions in full.
- [`AGENTS/git-workflow.md`](AGENTS/git-workflow.md) — automatic commit
  and push, and how to commit only your own changes.
- [`AGENTS/companion-site.md`](AGENTS/companion-site.md) — the GitHub
  Pages site in `emergency-care-system.github.io/` and its `git subtree`
  deploy.
- [`AGENTS/environment.md`](AGENTS/environment.md) — local-environment
  problems that look like test failures.

## Rules that always apply

- Use **pnpm**, not npm or yarn, in this repo (exceptions in
  [`AGENTS/commands.md`](AGENTS/commands.md)). Run `pnpm run check` after
  any `.svelte`/`.ts` change.
- **Automatic commit and push are enabled** for routine work; force-pushes
  and rewriting shared history still need asking first. Commit only your
  own changes ([`AGENTS/git-workflow.md`](AGENTS/git-workflow.md)).
- **Specification-driven:** `tests-with-given-when-then-features/*.feature`
  wins over any test or app code that disagrees.
- **Never fabricate** a hosted-demo URL, a metric, or a claim about a real
  hospital, product, or dataset; all data is synthetic and should say so.
- **No SSO, by design** — no login bypass, even for tests.
- **The `data-testid` contract is load-bearing** — changing one in
  `src/lib/features/**` breaks all twelve suites at once.
- **Twelve suites, one change** — a behavior change needs the scenario
  updated in all twelve `tests-with-*/` directories
  (`tests-with-selenium-javascript/` is the reference), and the companion
  site updated too.
- Don't edit vendored content under `skills/lily-design-system-*-skill/`
  or `skills/selenium-javascript-skill/`.
