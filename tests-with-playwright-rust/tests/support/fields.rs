// Helpers for interacting with form fields identified by data-testid
// attributes, and for translating Gherkin data-table field labels (e.g.
// "Given Name", "Date of Birth") into the kebab-case testid the app is
// assumed to expose (e.g. "given-name", "date-of-birth").
//
// Playwright's `get_by_test_id` locates elements by the `data-testid`
// attribute by default, which is exactly the app's convention, so most of
// this is a thin, label-based wrapper around it.

use playwright_rs::protocol::WaitForOptions;
use playwright_rs::{Locator, Page, Result};
use regex::Regex;

use super::runner::Row;

pub fn kebab_case(label: &str) -> String {
    let re = Regex::new("[^a-z0-9]+").unwrap();
    re.replace_all(&label.trim().to_lowercase(), "-").trim_matches('-').to_string()
}

/// A Locator for the element matching a Gherkin label, e.g.
/// locator(page, "Medical Record Number") ->
/// page.get_by_test_id("medical-record-number")
pub fn locator(page: &Page, label: impl AsRef<str>) -> Locator {
    page.get_by_test_id(&kebab_case(label.as_ref()))
}

// Like Selenium's find_element, the helpers below act on the first match:
// Playwright locators are strict and error when a test id matches more than
// one element (e.g. one "placeholder-id" per queued patient).

/// Waits until the element exists in the DOM. Playwright's locator actions
/// (fill, click, ...) already auto-wait for the element to exist and be
/// actionable, so this is mainly for the cases the Selenium suite used an
/// explicit wait before reading text or asserting visibility on something
/// that renders after a click, submit, or navigation.
pub async fn wait_located(page: &Page, test_id: &str, timeout_ms: u64) -> Result<Locator> {
    let target = page.get_by_test_id(test_id).first();
    target.wait_for(WaitForOptions::builder().timeout(timeout_ms as f64).build()).await?;
    Ok(target)
}

pub async fn wait_for_test_id(page: &Page, label: impl AsRef<str>) -> Result<Locator> {
    wait_located(page, &kebab_case(label.as_ref()), 10000).await
}

/// Fills a text-like input identified by its Gherkin field label.
pub async fn fill_field(page: &Page, label: impl AsRef<str>, value: impl AsRef<str>) -> Result<()> {
    locator(page, label).first().fill(value.as_ref(), None).await
}

/// Fills every "Field" / "Value" row of a Gherkin data table, e.g. the
/// tables under "When I enter the patient's demographic information:".
pub async fn fill_fields(page: &Page, rows: &[Row]) -> Result<()> {
    for row in rows {
        fill_field(page, &row["Field"], &row["Value"]).await?;
    }
    Ok(())
}

/// The visible text of a single element, trimmed (like Selenium's getText).
pub async fn text_of(element: &Locator) -> Result<String> {
    Ok(element.inner_text().await?.trim().to_string())
}

pub async fn texts_of(elements: &Locator) -> Result<Vec<String>> {
    Ok(elements.all_inner_texts().await?.iter().map(|t| t.trim().to_string()).collect())
}

pub async fn get_text(page: &Page, label: impl AsRef<str>) -> Result<String> {
    text_of(&locator(page, label).first()).await
}
