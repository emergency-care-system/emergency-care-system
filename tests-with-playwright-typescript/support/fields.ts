// Helpers for interacting with form fields identified by data-testid
// attributes, and for translating Gherkin data-table field labels (e.g.
// "Given Name", "Date of Birth") into the kebab-case testid the app is
// assumed to expose (e.g. "given-name", "date-of-birth").
//
// Playwright's `getByTestId` locates elements by the `data-testid`
// attribute by default, which is exactly the app's convention, so most of
// this is a thin, label-based wrapper around it.

import type { Locator, Page } from '@playwright/test';

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

// A Locator for the element matching a Gherkin label, e.g.
// locator(page, 'Medical Record Number') -> page.getByTestId('medical-record-number')
export function locator(page: Page, label: string): Locator {
  return page.getByTestId(kebabCase(label));
}

// Playwright's locator actions (fill, click, ...) already auto-wait for the
// element to exist and be actionable, so this is mainly for the cases the
// Selenium suite used an explicit wait before reading text or asserting
// visibility on something that renders after a click, submit, or navigation.
export async function waitForTestId(page: Page, label: string, timeout = 10000): Promise<Locator> {
  const target = locator(page, label);
  await target.waitFor({ timeout });
  return target;
}

// Fills a text-like input identified by its Gherkin field label.
export async function fillField(page: Page, label: string, value: string): Promise<void> {
  await locator(page, label).fill(value);
}

// Fills every "Field" / "Value" row of a Gherkin data table, e.g. the
// tables under "When I enter the patient's demographic information:".
export async function fillFields(page: Page, rows: FieldRow[]): Promise<void> {
  for (const row of rows) {
    await fillField(page, row.Field, row.Value);
  }
}

export async function getText(page: Page, label: string): Promise<string> {
  const text = await locator(page, label).innerText();
  return text.trim();
}
