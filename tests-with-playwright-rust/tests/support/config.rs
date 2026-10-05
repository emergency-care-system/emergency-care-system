// Shared configuration for the Playwright Rust test suite.
//
// Override BASE_URL to point at a running instance of the app, e.g.:
//   BASE_URL=http://localhost:5173 cargo test
//
// Unlike the JavaScript Playwright suite, this one drives the Playwright
// library directly rather than the `playwright test` CLI runner, so there
// is no automatic dev-server startup -- start `pnpm run dev` yourself first.

pub fn base_url() -> String {
    std::env::var("BASE_URL").unwrap_or_else(|_| "http://localhost:5173".to_string())
}
