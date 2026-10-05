# tests-with-playwright-rust

The 171 scenarios from `tests-with-given-when-then-features/*.feature`, in
Playwright + Rust. A port of `tests-with-selenium-javascript/` (the
Playwright-specific idioms follow `tests-with-playwright-python/`); see
[`spec/testing.md`](../spec/testing.md) for the shared contract. All data in
the app is synthetic.

Playwright has no official Rust binding. This suite uses the community
[`playwright-rs`](https://crates.io/crates/playwright-rs) crate (pre-1.0; it
bundles the Playwright 1.63 driver and has the full locator API), so expect
its API to change between releases.

```sh
pnpm run dev                                                         # in the repo root: app at http://localhost:5173
cargo run --manifest-path tests-with-playwright-rust/Cargo.toml --example install-browsers   # once
cargo test --manifest-path tests-with-playwright-rust/Cargo.toml     # or: pnpm run test:playwright-rust
```

- The first build downloads the ~130 MB Playwright driver and compiles all 22
  test binaries, so it takes a while.
- `BASE_URL` points at another instance of the app; there is no automatic
  dev-server startup.
- Chromium is headless unless `HEADLESS=0`.
- Each `tests/tNN_*.rs` file is its own test binary (`harness = false`, run by
  `libtest-mimic`), so it gets its own browser; its scenarios run in source
  order on one thread and are reported individually.
- Playwright locators are strict, so the helpers act on `.first()` of a
  `data-testid` match (like Selenium's `findElement`); see
  `spec/testing.md`.
