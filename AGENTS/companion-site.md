# Companion site

`emergency-care-system.github.io/` is a static SvelteKit 3 site for GitHub
Pages that documents this repo's 22 features and twelve test suites for a
general audience. It lives in this monorepo and is published to the
standalone repository
[emergency-care-system.github.io](https://github.com/emergency-care-system/emergency-care-system.github.io)
(served at <https://emergency-care-system.github.io/>).

It has its own [`AGENTS.md`](../emergency-care-system.github.io/AGENTS.md)
and [`spec/site-architecture.md`](../emergency-care-system.github.io/spec/site-architecture.md),
which are the source of truth for how it is built. Don't duplicate their
content here; link instead.

## Differences from the rest of the repo

- **npm, not pnpm.** Run `npm install`, `npm run check`, `npm run build`
  inside the directory.
- **Lily packages from the `@lilydesignsystem/*` scope**
  (`svelte-headless`, `svelte-picker-bar`, `svelte-share-picker`).
- **Edit it here, never in the standalone repository**, or the next
  subtree push is rejected as non-fast-forward.

## Keeping it in sync

When a feature or test suite is added, renamed, or removed in this repo,
update the site in the same change: `src/lib/data/features.ts`, the
`/testing/` page's suite list, the counts in the page copy, and
`static/llms.txt` / `static/llms.json` together.

## Deploying

Commit first, then from the repository root:

```sh
git remote add pages git@github.com:emergency-care-system/emergency-care-system.github.io.git   # once
git subtree push --prefix=emergency-care-system.github.io pages main
```

Every push to the standalone repository's `main` runs its
`.github/workflows/deploy.yml` (npm build → GitHub Pages; the Pages source
is "GitHub Actions"). GitHub ignores `.github/` in a monorepo
subdirectory, so the workflow only runs there. Check with
`gh run list -R emergency-care-system/emergency-care-system.github.io`.
