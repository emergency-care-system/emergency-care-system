// Shared helpers for the Playwright Rust test suite: one module per
// concern, mirroring the support/ directory of the other suites.

#![allow(dead_code, unused_imports)]

pub mod config;
pub mod fields;
pub mod login;
pub mod runner;

pub use config::base_url;
pub use fields::*;
pub use login::{login, verify_system_is_operational};
pub use playwright_rs::{Locator, Page, Result};
pub use runner::{assert_match, row, run_feature, Row, ScenarioFuture};
