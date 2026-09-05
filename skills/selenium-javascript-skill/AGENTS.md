# AGENTS.md

This repo has one job: maintain `SKILL.md`, a Claude Code Skill that teaches
how to write Selenium WebDriver browser-automation code in JavaScript.

- `spec/index.md` is the source of truth for what `SKILL.md` must accurately
  cover. If you change `SKILL.md`, check it against the spec first.
- `SKILL.md` is the actual skill content that Claude Code loads. Keep its
  frontmatter (`name`, `description`) intact, and keep its code examples
  real, working `selenium-webdriver` syntax — do not invent APIs or
  selectors that do not exist in the sibling demo repos this skill cites.
- `README.md` is for humans browsing the repo; it is not the skill itself.
- `CLAUDE.md` points here.

CLAUDE.md is a pointer to this file — it is the single source of truth for
agent instructions.
