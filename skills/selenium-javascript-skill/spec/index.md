# Spec

## Summary

This repo publishes one Claude Code Skill, `SKILL.md`, that teaches how to
write Selenium WebDriver browser-automation code and tests in JavaScript.
This spec defines the contract that `SKILL.md` must satisfy.

## Scope

In scope:

- Accurately describing the `selenium-webdriver` npm package's real API for
  locating elements (`By`, `driver.findElement`, `driver.findElements`),
  performing actions (`sendKeys`, `click`, `Select`), waiting
  (`driver.wait`, `until`), and asserting (Mocha + Node's `assert`, or Chai).
- Explaining the lack of Selenium built-in auto-waiting, contrasted with
  Playwright, and the flakiness risk of not waiting explicitly.
- Explaining browser-driver setup (Selenium Manager auto-download, manual
  chromedriver, macOS Gatekeeper).
- Linking to this project's real sibling demo repos as further reading.

Out of scope:

- Selenium bindings in languages other than JavaScript.
- Grid/remote WebDriver setup, CI configuration, and other advanced topics
  not needed to write a first correct Selenium JavaScript test.
- Being a runnable demo or test suite itself.

## Principles and rules

- Every code sample in `SKILL.md` must be real, working `selenium-webdriver`
  syntax — no invented methods, options, or selectors. Where possible, base
  samples on the actual `src/demo.js` from
  https://github.com/joelparkerhenderson/demo-selenium-javascript.
- The skill must clearly distinguish a plain walkthrough script (logs what
  it finds) from a real test (Mocha `describe`/`it` plus assertions), since
  Selenium itself has no built-in test runner or assertion library.
- Any mention of Google Search or Google Maps demo repos must carry the
  caveat that Google's Terms of Service restrict automated querying of
  those sites, and that those repos are illustrative only.
- Sibling-repo links must point to real, existing repos in the
  `joelparkerhenderson` or `testingexamples` GitHub orgs.

## Acceptance criteria

- Every code sample in `SKILL.md` is valid JavaScript using
  `selenium-webdriver`'s real API.
- `SKILL.md` explicitly contrasts a walkthrough script with a real Mocha +
  assert test.
- `SKILL.md` explains explicit waits (`driver.wait(until....)`) and states
  plainly that Selenium does not auto-wait the way Playwright does.
- `SKILL.md` includes a "common pitfalls" list covering at minimum: missing
  `await`, `NoSuchElementError` from missing waits, not calling
  `driver.quit()` in a `finally`, the macOS Gatekeeper chromedriver warning,
  and `By.className` not accepting multiple space-separated classes.
- Every linked sibling repo URL resolves:
  - https://github.com/joelparkerhenderson/demo-selenium-javascript
  - https://github.com/testingexamples/demo-selenium-javascript-for-google-search
  - https://github.com/testingexamples/demo-selenium-javascript-for-google-maps
  - https://github.com/joelparkerhenderson/demo-selenium-javascript-for-nhs-wales
- `SKILL.md` frontmatter's `name` and `description` fields match this
  repo's published skill name and purpose.

## Related topics

- [README.md](../README.md)
- [AGENTS.md](../AGENTS.md)
- [SKILL.md](../SKILL.md)
