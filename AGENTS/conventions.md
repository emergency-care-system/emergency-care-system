# Working conventions

These apply to every change in the repo. The one-line versions are also in
the root [`AGENTS.md`](../AGENTS.md).

- **Specification-driven.** `tests-with-given-when-then-features/*.feature`
  is the single source of truth for what a feature does. The app panel and
  all twelve test suites are built to match it, not the other way around —
  if a test and a `.feature` file disagree, the `.feature` file wins and
  the test is wrong.
- **Never fabricate** a hosted-demo URL, a metric, or a claim about a real
  hospital, product, or dataset. Every patient name, staff name, vital
  sign, timestamp, and statistic anywhere in this repo (app data, Gherkin
  tables, test assertions, docs) is invented for this demo — say so
  plainly wherever a reader might otherwise assume it's real.
- **No SSO, by design.** Every login path — the app's own `/login` screen
  and every test suite's precondition check — goes through the same
  explicit sign-in. Don't add a bypass, even for test convenience; see
  `spec/index.md`'s Authentication section for the accepted demo
  identities.
- **The `data-testid` contract is load-bearing.** Renaming, removing, or
  changing the semantics of a `data-testid` in `src/lib/features/**`
  breaks all twelve test suites at once. If a feature's UI changes,
  update the test suites in the same change — see `spec/testing.md`.
- **Twelve suites, one change.** A behavior change to a feature (in the
  `.feature` file, the app panel, or a scenario's expected text) needs the
  corresponding scenario updated in all twelve `tests-with-*/`
  directories, not just one. `tests-with-selenium-javascript/` is the
  reference; port from there.
- **Keep the docs current.** If you change how the repo works in a way
  future agents need to know, update the relevant `spec/*.md` file (or
  the matching `AGENTS/*.md` file for agent workflow) in the same change —
  don't leave it only in a commit message.
- **Vendored skills are read-only.** Don't edit
  `skills/lily-design-system-*-skill/` or
  `skills/selenium-javascript-skill/` directly — they're each
  independently maintained packages/skills. This repo's `AGENTS.md`,
  `AGENTS/*.md` and `spec/*.md` are the source of truth when they
  disagree with a vendored skill's own docs (each skill's `SKILL.md` says
  so).
