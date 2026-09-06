# Skills

Claude Code skills used while building this repo. Each is a standalone,
independently maintained package vendored into its own `*-skill/`
subdirectory — see each one's own `SKILL.md` for what it teaches and when
to use it. Don't edit their content directly; this repo's
[`AGENTS.md`](../AGENTS.md) and [`spec/index.md`](../spec/index.md) are
the source of truth when they disagree with a vendored skill's own docs
(each skill's `SKILL.md` says so).

- [`selenium-javascript-skill/`](selenium-javascript-skill/SKILL.md) —
  writing, explaining, debugging, and extending Selenium WebDriver
  browser automation in JavaScript (locators, explicit waits, Mocha
  assertions). Used for `tests-with-selenium-javascript/`, the reference
  suite every other test suite in this repo is a mechanical port of.
- [`lily-design-system-svelte-headless-skill/`](lily-design-system-svelte-headless-skill/SKILL.md) —
  using Lily Design System's headless Svelte components. Used throughout
  `src/lib/features/*/Panel.svelte`.
- [`lily-design-system-svelte-helpers-skill/`](lily-design-system-svelte-helpers-skill/SKILL.md) —
  Lily Design System's Svelte helper utilities that complement the
  headless components above.

This repo doesn't currently have skills for Playwright, TypeScript, or
Python test writing specifically — those five suites were built by
porting the Selenium JavaScript suite scenario-for-scenario (see
[`spec/testing.md`](../spec/testing.md)) rather than from a dedicated
skill for each combination.
