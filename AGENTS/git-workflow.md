# Git workflow

**Automatic commit and push are enabled.** Claude (and other AI coding
agents) may commit and push routine work in this repository
autonomously, without asking first. The git-hygiene expectations in
[`spec/index.md`](../spec/index.md#git-workflow) still apply: branch off
`main`, write clear messages, fast-forward merge back. Force-pushes and
rewriting shared history still call for asking first.

## Practical rules

- **Commit only your own changes.** The working tree often holds the
  human's unrelated edits (dependency bumps, staged deletions). Stage
  explicit paths, and when a file mixes your hunk with theirs, stage just
  your hunk (`git apply --cached` with a one-hunk patch). Don't `git add
  -A`, and check `git diff --cached --stat` before committing.
- **Chain git steps with care.** If an earlier command in a `&&` chain
  fails, a later `;`-separated `git commit` still runs and can commit
  whatever was already staged. Verify the staged set first, in its own
  command.
- **A mistaken, unpushed commit is fixable** with `git reset --soft
  HEAD~1`; a pushed one is not — ask before rewriting.
- **Add the commit attribution** your harness specifies to commit messages.
- **The companion site is published by subtree.** After committing a
  change under `emergency-care-system.github.io/`, deploy it as described
  in [`companion-site.md`](companion-site.md).
