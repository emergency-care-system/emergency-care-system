# Testing

The single source of truth for how the six test suites in this repo are
built and how to extend them consistently. `AGENTS.md`, `README.md`, and
any suite's own file-header comments should point here rather than
restate these facts — update this file, not copies of it.

## The six suites

Every one of the 171 scenarios across all 22 `tests-with-given-when-then-features/*.feature`
files is implemented once per suite, pairing two automation libraries
with three languages:

| Directory | Library | Language | Runner |
|---|---|---|---|
| `tests-with-selenium-javascript/` | Selenium WebDriver | JavaScript | Mocha |
| `tests-with-playwright-javascript/` | Playwright | JavaScript | Playwright Test |
| `tests-with-selenium-typescript/` | Selenium WebDriver | TypeScript | Mocha (via `tsx`) |
| `tests-with-playwright-typescript/` | Playwright | TypeScript | Playwright Test |
| `tests-with-selenium-python/` | Selenium WebDriver | Python | pytest |
| `tests-with-playwright-python/` | Playwright | Python | pytest |

`tests-with-selenium-javascript/` is the original suite — every other
suite is a mechanical port of it (same scenarios, same assertions, same
`describe`/`it` or `test`/`test.beforeEach` structure translated to the
target language's idiom). When adding or fixing a scenario, change the
JavaScript suite first, then port the same change to the other five.

## The shared contract

All six suites drive the same running SvelteKit app (`src/`) through one
contract, so none of them need to know anything about the app's internal
implementation:

- **`data-testid` attributes.** Every interactive element and every piece
  of text a scenario asserts on carries a `data-testid` matching its
  Gherkin field label, kebab-cased (e.g. "Given Name" →
  `data-testid="given-name"`, "Submit Registration" →
  `data-testid="submit-registration"`). Each suite's `support/fields.*`
  helper does the kebab-casing; Playwright's `page.getByTestId(...)` /
  `page.get_by_test_id(...)` uses this natively since it's also
  Playwright's default locator strategy, and Selenium's
  `By.css('[data-testid="..."]')` matches the same attribute.
- **Login contract.** `data-testid="login-identity"`,
  `"login-submit"`, and `"app-root"` (present once authenticated) are
  the fixed contract every suite's `support/login.*` helper depends on.
  Free-text identities are accepted unconditionally (see
  `src/lib/data/directory.ts`); four narrower accounts (`sjohnson`,
  `mchen`, `awilson`, `pmartinez`) replay the fully scripted scenarios in
  `21-user-authentication.feature`.
- **One feature panel mounted at a time.** Every scenario's setup clicks
  `data-testid="nav-<slug>"` for its feature, then waits for
  `data-testid="<Feature Title> Panel"` before interacting.

## Architecture rules that apply to every suite

- **One dedicated browser per test file**, not a shared fixture across
  files. A shared browser/worker handling many files' logins back-to-back
  proved unreliable in practice (a fast, automated login could race
  SvelteKit's hydration and land a stray native form submission before
  the click handler attached). Each file launches its own browser in
  `before`/`beforeAll`/`setup_class` and closes it in
  `after`/`afterAll`/`teardown_class`.
- **Serial execution within a file, one worker across files.** Later
  scenarios in many features depend on earlier scenarios' side effects
  (`localStorage`/`sessionStorage` — patient duplicate detection,
  scenario-cycling panel counters) persisting within the same browser, so
  scenario order within a file matters. Mocha and pytest already run a
  file's tests top-to-bottom by default; Playwright needs
  `test.describe.configure({ mode: 'serial' })` plus `workers: 1` in its
  config to get the same guarantee.
- **`login()` waits for `feature-nav`, not `app-root`, with a bounded
  retry.** `app-root` is present on `/login` itself before hydration
  finishes, so waiting on it doesn't actually confirm a successful login
  — a real hydration-race bug this exact mistake caused (see git history
  on `support/login.*` in the JavaScript suites for the fix). Wait for
  `feature-nav`, which only renders once authenticated, and retry the
  login attempt once (bounded, not infinite) if it doesn't appear in time.
- Each suite's `support/` module exposes the same small set of helpers,
  named idiomatically for its language:
  `login` / `verify_system_is_operational` (or the camelCase
  equivalent), `fill_field(s)` / `fillField(s)`, `get_text` / `getText`,
  `locator`, `wait_for_test_id` / `waitForTestId`.

## Running a suite

```sh
pnpm run test:selenium              # Mocha (JS)
pnpm run test:playwright            # Playwright (JS)
pnpm run test:selenium-typescript   # Mocha (TS, via tsx)
pnpm run test:playwright-typescript # Playwright (TS)

pip install -r requirements.txt     # once, for the two Python suites
playwright install chromium
pnpm run test:selenium-python       # pytest + Selenium
pnpm run test:playwright-python     # pytest + Playwright
```

The JS/TS Selenium suites and both Python suites need a dev server
already running (`pnpm run dev`, or `BASE_URL` pointed at one); the JS/TS
Playwright suites start their own dev server automatically if `BASE_URL`
isn't set (see `playwright.config.js` / `playwright.typescript.config.ts`).

**Python suites run independently of each other and of themselves.**
`tests-with-selenium-python` and `tests-with-playwright-python` both ship
a `support` package and same-named `test_NN_*.py` files; running both in
one `pytest` invocation hits a module-cache collision (neither directory
can have `__init__.py`, since hyphens in the directory name make it an
invalid Python package name). Each `conftest.py` adds a `sys.path` entry
as a safety net for `--import-mode=importlib`, but the supported usage is
one suite per `pytest` invocation, matching the one-script-per-suite
pattern the four JS/TS npm scripts already use.

## Adding a new feature

1. Write `tests-with-given-when-then-features/NN-slug.feature` first (Background + scenarios,
   `As a / I want / So that`).
2. Build the app panel (`src/lib/features/NN-slug/Panel.svelte`,
   registered in `src/lib/features/registry.ts`) with `data-testid`
   attributes matching every field label and every assertable text
   value in the feature file.
3. Write `tests-with-selenium-javascript/NN-slug.test.js` against it —
   this is the reference implementation every other suite ports.
4. Port the same file, scenario-for-scenario, to the other five suites,
   translating helper calls and assertions to each language's idiom
   (see the shared contract and architecture rules above) but changing
   nothing about which scenarios exist or what they assert.
5. Run all six suites and confirm the new scenarios pass in every one
   before considering the feature done.

## Known limitations

The 22 test files were themselves generated independently (one file per
feature, in parallel) before being ported to the other five languages, so
a handful of scenarios assert exact wording that was invented by that
generation process rather than drawn from the `.feature` file's own
quoted text. Occasional single-scenario flakiness has been observed when
running a full 171-scenario suite twice back-to-back in rapid succession
(browser/dev-server resource contention, not a code defect) — a failing
run is worth retrying in isolation before treating it as a regression.
