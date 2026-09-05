// Helpers for interacting with form fields identified by data-testid
// attributes, and for translating Gherkin data-table field labels (e.g.
// "Given Name", "Date of Birth") into the kebab-case testid the app is
// assumed to expose (e.g. "given-name", "date-of-birth").

import { By, until } from 'selenium-webdriver';

export function kebabCase(label) {
  return label
    .trim()
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/(^-|-$)/g, '');
}

export function testId(label) {
  return `[data-testid="${kebabCase(label)}"]`;
}

// A By locator for the element matching a Gherkin label, e.g.
// locator('Medical Record Number') -> By.css('[data-testid="medical-record-number"]')
export function locator(label) {
  return By.css(testId(label));
}

export async function waitForTestId(driver, label, timeout = 10000) {
  return driver.wait(until.elementLocated(locator(label)), timeout);
}

// Fills a text-like input identified by its Gherkin field label.
export async function fillField(driver, label, value) {
  const element = await waitForTestId(driver, label);
  await element.clear();
  await element.sendKeys(value);
}

// Fills every "Field" / "Value" row of a Gherkin data table, e.g. the
// tables under "When I enter the patient's demographic information:".
export async function fillFields(driver, rows) {
  for (const row of rows) {
    await fillField(driver, row.Field, row.Value);
  }
}

export async function getText(driver, label) {
  const element = await waitForTestId(driver, label);
  return element.getText();
}
