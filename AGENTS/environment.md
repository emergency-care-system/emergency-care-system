# Local environment gotchas

Problems that look like test failures but aren't. Check these before
debugging a suite.

- **`session not created: This version of ChromeDriver only supports Chrome
  version N`** (Selenium suites). A `chromedriver` on `PATH` that doesn't
  match the installed Chrome shadows the one Selenium Manager would
  download. Remove or upgrade it. The Rust Selenium suite has no Selenium
  Manager: it starts chromedriver itself, found via `CHROMEDRIVER`, then
  `SELENIUM_MANAGER`, then `PATH` (see `tests-with-selenium-rust/README.md`).
- **Cold dev server.** The first scenarios after `pnpm run dev` starts can
  fail (stale element, a login that races hydration) while Vite compiles,
  and later scenarios that depend on earlier ones' `localStorage` then
  cascade. Warm the server (load `/login` once) and re-run the file before
  treating it as a regression. `spec/testing.md` lists this under known
  limitations.
- **Maven: `JAVA_HOME is not defined correctly`.** Point it at a JDK, e.g.
  `export JAVA_HOME=$(/usr/libexec/java_home)` on macOS or the sdkman
  `current` directory. JDK 21+ works.
- **`.NET` / `cargo` first build is slow** (package restore; the Rust
  Playwright suite also downloads the ~130 MB Playwright driver and
  compiles 22 test binaries). Don't treat a long first build as a hang.
- **Playwright browsers.** The JS/TS, C# and Java Playwright suites share
  the Playwright 1.63 browser cache (`pnpm exec playwright install
  chromium`). The Rust one installs through
  `cargo run --example install-browsers` in its directory.
- **Shell state doesn't persist between commands** in some agent
  harnesses: re-export `JAVA_HOME`, `CHROMEDRIVER`, etc. in each command.
