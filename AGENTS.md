# AGENTS.md

Instructions for AI coding agents (Claude Code, Codex, Cursor, etc.)
working in this repository — a public, free, open-source demonstration
of how to plan an emergency care system, from higher-level use cases to
lower-level test automation. All data anywhere in the repo is synthetic;
this is not a real emergency care system.

**This file is the canonical agent-facing doc.** `CLAUDE.md` just
imports it. Don't fork guidance between the two — edit here.

## Source of truth

For anything about how the repo is *built* — stack, directory layout,
package manager, authentication, the pattern for adding a new feature —
read [`spec/index.md`](spec/index.md) first. It's kept current; this
file won't repeat it.

For the twelve test suites specifically — their shared `data-testid`
contract, the one-browser-per-file architecture, and how to port a
change across all twelve — read [`spec/testing.md`](spec/testing.md).

If you change how the repo works in a way future agents need to know,
update the relevant `spec/*.md` file in the same change — don't leave it
only in a commit message or in this file.

## Commands

```sh
pnpm install
pnpm run dev                        # app at http://localhost:5173
pnpm run check                      # svelte-kit sync + svelte-check -- run after any .svelte/.ts change
pnpm run build

pnpm run test:selenium              # Mocha (JS)
pnpm run test:playwright            # Playwright (JS)
pnpm run test:selenium-typescript   # Mocha (TS, via tsx)
pnpm run test:playwright-typescript # Playwright (TS)

pip install -r requirements.txt     # once, for the two Python suites
playwright install chromium
pnpm run test:selenium-python       # pytest + Selenium
pnpm run test:playwright-python     # pytest + Playwright

# C#, Java and Rust suites need dotnet, a JDK + Maven, and cargo respectively
# (and a dev server already running); see spec/testing.md.
pnpm run test:selenium-c-sharp      # NUnit + Selenium
pnpm run test:playwright-c-sharp    # NUnit + Playwright
pnpm run test:selenium-java         # JUnit 5 + Selenium (Maven)
pnpm run test:playwright-java       # JUnit 5 + Playwright (Maven)
pnpm run test:selenium-rust         # thirtyfour (cargo test)
pnpm run test:playwright-rust       # playwright-rs (cargo test)
```

Use **pnpm**, not npm or yarn, for every install/run/build command —
see `spec/index.md`. The exceptions are the two Python suites (`pip`) and the
C#/Java/Rust suites (`dotnet`, `mvn`, `cargo`).

## Working conventions

- **Automatic commit and push are enabled.** Claude (and other AI coding
  agents) may commit and push routine work in this repository
  autonomously, without asking first — see
  [`spec/index.md`](spec/index.md#git-workflow) for the git-hygiene
  expectations that still apply (branch off `main`, clear messages,
  fast-forward merge back). Force-pushes and rewriting shared history
  still call for asking first.
- **Specification-driven.** `tests-with-given-when-then-features/*.feature` is the single
  source of truth for what a feature does. The app panel and all six
  test suites are built to match it, not the other way around — if a
  test and a `.feature` file disagree, the `.feature` file wins and the
  test is wrong.
- **Never fabricate** a hosted-demo URL, a metric, or a claim about a
  real hospital, product, or dataset. Every patient name, staff name,
  vital sign, timestamp, and statistic anywhere in this repo (app data,
  Gherkin tables, test assertions, docs) is invented for this demo — say
  so plainly wherever a reader might otherwise assume it's real.
- **No SSO, by design.** Every login path — the app's own `/login`
  screen and every test suite's precondition check — goes through the
  same explicit sign-in. Don't add a bypass, even for test convenience;
  see `spec/index.md`'s Authentication section for the accepted demo
  identities.
- **The `data-testid` contract is load-bearing.** Renaming, removing, or
  changing the semantics of a `data-testid` in `src/lib/features/**`
  breaks all twelve test suites at once. If a feature's UI changes, update
  the test suites in the same change — see `spec/testing.md`.
- **Six suites, one change.** A behavior change to a feature (in the
  `.feature` file, the app panel, or a scenario's expected text) needs
  the corresponding scenario updated in all twelve `tests-with-*/`
  directories, not just one. `tests-with-selenium-javascript/` is the
  reference; port from there.
- Don't edit vendored content under `skills/lily-design-system-*-skill/`
  or `skills/selenium-javascript-skill/` directly — they're each
  independently maintained packages/skills. This repo's `AGENTS.md` and
  `spec/*.md` are the source of truth when they disagree with a vendored
  skill's own docs (each skill's `SKILL.md` says so).
- There's a companion GitHub Pages site,
  [`emergency-care-system.github.io`](https://github.com/emergency-care-system/emergency-care-system.github.io)
  (separate repository, not yet deployed), that documents this repo's
  features and test suites for a general audience. It has its own
  `AGENTS.md` and `spec/site-architecture.md` — don't duplicate its
  content here or vice versa; link instead.
