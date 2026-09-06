// Helpers for interacting with form fields identified by data-testid
// attributes, and for translating Gherkin data-table field labels (e.g.
// "Given Name", "Date of Birth") into the kebab-case testid the app is
// assumed to expose (e.g. "given-name", "date-of-birth").

import { By, until, type WebDriver, type WebElement } from 'selenium-webdriver';

// A "Field" / "Value" row of a Gherkin data table, e.g. one row of the
// tables under "When I enter the patient's demographic information:".
export type FieldRow = {
  Field: string;
  Value: string;
};

export function kebabCase(label: string): string {
  return label
    .trim()
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/(^-|-$)/g, '');
}

export function testId(label: string): string {
  return `[data-testid="${kebabCase(label)}"]`;
}

// A By locator for the element matching a Gherkin label, e.g.
// locator('Medical Record Number') -> By.css('[data-testid="medical-record-number"]')
export function locator(label: string): By {
  return By.css(testId(label));
}

export async function waitForTestId(
  driver: WebDriver,
  label: string,
  timeout = 10000
): Promise<WebElement> {
  return driver.wait(until.elementLocated(locator(label)), timeout);
}

// Fills a text-like input identified by its Gherkin field label.
export async function fillField(driver: WebDriver, label: string, value: string): Promise<void> {
  const element = await waitForTestId(driver, label);
  await element.clear();
  await element.sendKeys(value);
}

// Fills every "Field" / "Value" row of a Gherkin data table, e.g. the
// tables under "When I enter the patient's demographic information:".
export async function fillFields(driver: WebDriver, rows: FieldRow[]): Promise<void> {
  for (const row of rows) {
    await fillField(driver, row.Field, row.Value);
  }
}

export async function getText(driver: WebDriver, label: string): Promise<string> {
  const element = await waitForTestId(driver, label);
  return element.getText();
}
