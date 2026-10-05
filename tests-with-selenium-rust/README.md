# tests-with-selenium-rust

The 171 scenarios from `tests-with-given-when-then-features/*.feature`, in
Selenium WebDriver + Rust, using the [`thirtyfour`](https://crates.io/crates/thirtyfour)
crate. A port of `tests-with-selenium-javascript/`; see
[`spec/testing.md`](../spec/testing.md) for the shared `data-testid` contract
and architecture rules. All data in the app is synthetic.

```sh
pnpm run dev                                                      # in the repo root: app at http://localhost:5173
cargo test --manifest-path tests-with-selenium-rust/Cargo.toml    # or: pnpm run test:selenium-rust
```

- **chromedriver.** `thirtyfour` does not bundle Selenium Manager, so
  `tests/support/driver.rs` starts chromedriver itself, looking for it in
  `CHROMEDRIVER` (path to the binary), then `SELENIUM_MANAGER` (path to a
  Selenium Manager binary, e.g.
  `node_modules/selenium-webdriver/bin/macos/selenium-manager`), then `PATH`.
  It must match your Chrome version.
- `BASE_URL` points at another instance of the app.
- Chrome opens a visible window unless `HEADLESS=1`.
- Each `tests/tNN_*.rs` file is its own test binary (`harness = false`, run by
  `libtest-mimic`), so it gets its own browser; its scenarios run in source
  order on one thread and are reported individually.
