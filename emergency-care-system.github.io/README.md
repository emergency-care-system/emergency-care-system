# emergency-care-system.github.io

The source for [emergency-care-system.github.io](https://emergency-care-system.github.io/),
a small static SvelteKit site that documents the
[Emergency Care System](https://github.com/emergency-care-system/emergency-care-system)
demo: a public, free, open-source demonstration of how to plan an
emergency care system, from higher-level use cases to lower-level test
automation. All data referenced anywhere is synthetic; this is not a
real emergency care system.

## Pages

- `/` — overview of the demo, its 22 features, and its twelve test suites.
- `/features/` — all 22 Gherkin features, each with its user story and
  a link to its `.feature` file in the source repository.
- `/testing/` — how the same 171 scenarios are ported across
  Selenium/Playwright × JavaScript/TypeScript/Python/C#/Java/Rust.
- `/about/` — what the demo is, what it isn't, and how the source
  repository is organized.

## Develop

```sh
npm install
npm run dev -- --open
npm run check   # svelte-kit sync + svelte-check
npm run build   # static output to build/
```

## Deploy

This directory lives in the
[emergency-care-system](https://github.com/emergency-care-system/emergency-care-system)
monorepo and is published to the standalone
[emergency-care-system.github.io](https://github.com/emergency-care-system/emergency-care-system.github.io)
repository with `git subtree`. From the monorepo root, after committing:

```sh
git remote add pages git@github.com:emergency-care-system/emergency-care-system.github.io.git   # once
git subtree push --prefix=emergency-care-system.github.io pages main
```

Every push to that repository's `main` runs
[`.github/workflows/deploy.yml`](.github/workflows/deploy.yml), which builds
with npm and publishes to GitHub Pages (repository Settings → Pages →
Source: GitHub Actions). Make changes in the monorepo, not in the
standalone repository, or the next subtree push is rejected as
non-fast-forward.

See [`AGENTS.md`](AGENTS.md) and [`spec/site-architecture.md`](spec/site-architecture.md)
for the stack, directory layout, and conventions for adding a page.
