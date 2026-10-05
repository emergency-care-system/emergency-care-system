// Helpers for interacting with form fields identified by data-testid
// attributes, and for translating Gherkin data-table field labels (e.g.
// "Given Name", "Date of Birth") into the kebab-case testid the app is
// assumed to expose (e.g. "given-name", "date-of-birth").

use std::time::Duration;

use regex::Regex;
use thirtyfour::prelude::*;

use super::runner::Row;

pub fn kebab_case(label: &str) -> String {
    let re = Regex::new("[^a-z0-9]+").unwrap();
    re.replace_all(&label.trim().to_lowercase(), "-").trim_matches('-').to_string()
}

pub fn test_id(label: &str) -> String {
    format!("[data-testid=\"{}\"]", kebab_case(label))
}

/// A By locator for the element matching a Gherkin label, e.g.
/// locator("Medical Record Number") ->
/// By::Css("[data-testid=\"medical-record-number\"]")
pub fn locator(label: impl AsRef<str>) -> By {
    By::Css(test_id(label.as_ref()))
}

/// Waits until an element matching the locator is present in the DOM.
pub async fn wait_located(driver: &WebDriver, by: By, timeout_ms: u64) -> WebDriverResult<WebElement> {
    driver
        .query(by)
        .wait(Duration::from_millis(timeout_ms), Duration::from_millis(100))
        .first()
        .await
}

pub async fn wait_for_test_id(driver: &WebDriver, label: impl AsRef<str>) -> WebDriverResult<WebElement> {
    wait_located(driver, locator(label), 10000).await
}

/// Fills a text-like input identified by its Gherkin field label.
pub async fn fill_field(driver: &WebDriver, label: impl AsRef<str>, value: impl AsRef<str>) -> WebDriverResult<()> {
    let element = wait_for_test_id(driver, label).await?;
    element.clear().await?;
    element.send_keys(value.as_ref()).await
}

/// Fills every "Field" / "Value" row of a Gherkin data table, e.g. the
/// tables under "When I enter the patient's demographic information:".
pub async fn fill_fields(driver: &WebDriver, rows: &[Row]) -> WebDriverResult<()> {
    for row in rows {
        fill_field(driver, &row["Field"], &row["Value"]).await?;
    }
    Ok(())
}

pub async fn get_text(driver: &WebDriver, label: impl AsRef<str>) -> WebDriverResult<String> {
    wait_for_test_id(driver, label).await?.text().await
}

pub async fn texts_of(elements: &[WebElement]) -> WebDriverResult<Vec<String>> {
    let mut texts = Vec::new();
    for element in elements {
        texts.push(element.text().await?);
    }
    Ok(texts)
}
