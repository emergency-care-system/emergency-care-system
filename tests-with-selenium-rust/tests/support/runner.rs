// Runs one feature file's scenarios.
//
// The tests/ files use `harness = false` (see Cargo.toml) with libtest-mimic
// instead of the default libtest harness, because the default one runs a
// file's tests in parallel threads and in alphabetical order. Scenarios in a
// file depend on earlier scenarios' side effects (localStorage persisting in
// the same browser), so this runner:
//   * starts ONE browser for the file and shares it across its scenarios,
//   * runs the scenarios one at a time, in source order,
//   * still reports each scenario as its own pass/fail test.

use std::collections::HashMap;
use std::future::Future;
use std::pin::Pin;
use std::sync::Arc;

use libtest_mimic::{Arguments, Failed, Trial};
use thirtyfour::prelude::*;

use super::driver::{build_driver, start_server};

/// One row of a Gherkin data table: column name -> cell value.
pub type Row = HashMap<String, String>;

pub fn row<const N: usize>(pairs: [(&str, &str); N]) -> Row {
    pairs.into_iter().map(|(k, v)| (k.to_string(), v.to_string())).collect()
}

pub type ScenarioFuture<'a> = Pin<Box<dyn Future<Output = WebDriverResult<()>> + 'a>>;
pub type Scenario = for<'a> fn(&'a WebDriver) -> ScenarioFuture<'a>;

/// Passes when `pattern` is found anywhere in `actual` (like JavaScript's
/// `assert.match(actual, /pattern/i)`).
pub fn assert_match(actual: impl AsRef<str>, pattern: &str, ignore_case: bool) {
    let source = if ignore_case { format!("(?i){pattern}") } else { pattern.to_string() };
    let re = regex::Regex::new(&source).expect("invalid regex");
    assert!(re.is_match(actual.as_ref()), "expected {:?} to match /{pattern}/", actual.as_ref());
}

pub fn run_feature(scenarios: &[(&'static str, Scenario)]) -> ! {
    let runtime = Arc::new(tokio::runtime::Builder::new_multi_thread().enable_all().build().unwrap());
    let server = start_server();
    let driver = runtime.block_on(build_driver(&server)).expect("could not start the browser");

    let mut args = Arguments::from_args();
    args.test_threads = Some(1);

    let trials: Vec<Trial> = scenarios
        .iter()
        .map(|&(name, scenario)| {
            let runtime = runtime.clone();
            let driver = driver.clone();
            Trial::test(name, move || {
                runtime.block_on(scenario(&driver)).map_err(|err| Failed::from(err.to_string()))
            })
        })
        .collect();

    let conclusion = libtest_mimic::run(&args, trials);
    let _ = runtime.block_on(driver.quit());
    drop(server);
    conclusion.exit()
}
