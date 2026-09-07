// Playwright + TypeScript test for
// tests-with-given-when-then-features/01-walk-in-patient-registration.feature
// (equivalent to tests-with-playwright-javascript/01-walk-in-patient-registration.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see support/fields.ts) and the shared
// data-testid contract in support/login.ts (login-identity, login-submit,
// app-root).

import { chromium, test, type Browser, type Page } from '@playwright/test';
import { strict as assert } from 'assert';
import { login, verifySystemIsOperational } from './support/login.js';
import { fillFields, fillField, getText, locator, waitForTestId } from './support/fields.js';

// All scenarios in this file share one page (like the Selenium suite shares
// one WebDriver per file), since later scenarios rely on data earlier
// scenarios registered persisting in the app's localStorage -- so they must
// run in file order, not in parallel.
test.describe.configure({ mode: 'serial' });

test.describe('Feature: Walk-in Patient Registration', () => {
  let browser: Browser;
  let page: Page;

  test.beforeAll(async () => {
    browser = await chromium.launch();
    page = await browser.newPage();
  });

  test.afterAll(async () => {
    await browser.close();
  });

  test.beforeEach(async () => {
    // Background:
    //   Given the emergency care system is operational
    //   And I am logged in as a registration clerk
    await verifySystemIsOperational(page);
    await login(page, 'a registration clerk');

    // The demo app is a single-page dashboard: after login, select this
    // feature's panel from the sidebar nav (data-testid="nav-<slug>").
    const registrationNavLink = await waitForTestId(page, 'Nav Walk In Patient Registration');
    await registrationNavLink.click();
    await waitForTestId(page, 'Patient Registration Form');
  });

  test('Successfully register a new walk-in patient', async () => {
    // Given a new patient arrives at the ED without prior registration
    // And the patient provides valid identification
    // When I enter the patient's demographic information:
    await fillFields(page, [
      { Field: 'Given Name', Value: 'John' },
      { Field: 'Family Name', Value: 'Doe' },
      { Field: 'Date of Birth', Value: '1985-06-15' },
      { Field: 'Phone Number', Value: '555-123-4567' },
      { Field: 'Address', Value: '123 Main St' },
      { Field: 'City', Value: 'Springfield' },
      { Field: 'State', Value: 'IL' },
      { Field: 'Zip Code', Value: '62701' }
    ]);
    // And I enter the patient's insurance details:
    await fillFields(page, [
      { Field: 'Insurance Type', Value: 'Blue Cross' },
      { Field: 'Policy Number', Value: 'BC123456789' },
      { Field: 'Group Number', Value: 'GRP001' }
    ]);
    // And I submit the registration form
    await page.getByTestId('submit-registration-form').click();

    // Then the system creates a unique patient record
    await waitForTestId(page, 'Medical Record Number');
    // And the system assigns a medical record number
    const medicalRecordNumber = await getText(page, 'Medical Record Number');
    assert.ok(medicalRecordNumber.length > 0);
    // And the patient is queued for triage
    const triageQueueStatus = await getText(page, 'Triage Queue Status');
    assert.match(triageQueueStatus, /queued for triage/i);
    // And I see a confirmation message "Patient successfully registered"
    const confirmationMessage = await getText(page, 'Confirmation Message');
    assert.strictEqual(confirmationMessage, 'Patient successfully registered');
    // And the medical record number is displayed
    assert.ok(await locator(page, 'Medical Record Number').isVisible());
  });

  test('Register patient with missing insurance information', async () => {
    // Given a new patient arrives at the ED without prior registration
    // And the patient does not have insurance information
    // When I enter the patient's demographic information:
    await fillFields(page, [
      { Field: 'Given Name', Value: 'Jane' },
      { Field: 'Family Name', Value: 'Smith' },
      { Field: 'Date of Birth', Value: '1990-03-22' },
      { Field: 'Phone Number', Value: '555-987-6543' },
      { Field: 'Address', Value: '456 Oak Ave' }
    ]);
    // And I select "Self-Pay" as the insurance type
    await fillField(page, 'Insurance Type', 'Self-Pay');
    // And I submit the registration form
    await page.getByTestId('submit-registration-form').click();

    // Then the system creates a unique patient record
    await waitForTestId(page, 'Medical Record Number');
    // And the system assigns a medical record number
    const medicalRecordNumber = await getText(page, 'Medical Record Number');
    assert.ok(medicalRecordNumber.length > 0);
    // And the patient is queued for triage
    const triageQueueStatus = await getText(page, 'Triage Queue Status');
    assert.match(triageQueueStatus, /queued for triage/i);
    // And the insurance status is marked as "Self-Pay"
    const insuranceStatus = await getText(page, 'Insurance Status');
    assert.strictEqual(insuranceStatus, 'Self-Pay');
  });

  test('Handle duplicate patient registration attempt', async () => {
    // Given a patient with the same name and date of birth already exists in the system
    // When I enter the patient's demographic information:
    await fillFields(page, [
      { Field: 'Given Name', Value: 'John' },
      { Field: 'Family Name', Value: 'Doe' },
      { Field: 'Date of Birth', Value: '1985-06-15' }
    ]);
    // And I submit the registration form
    await page.getByTestId('submit-registration-form').click();

    // Then the system displays a warning "Potential duplicate patient found"
    const warningMessage = await getText(page, 'Duplicate Patient Warning');
    assert.strictEqual(warningMessage, 'Potential duplicate patient found');
    // And the system shows existing patient records for verification
    const existingRecords = await page.getByTestId('existing-patient-record').all();
    assert.ok(existingRecords.length > 0);
    // And I can choose to link to existing record or create new record
    await waitForTestId(page, 'Link to Existing Record');
    await waitForTestId(page, 'Create New Record');
  });

  test('Registration with invalid demographic data', async () => {
    // Given a new patient arrives at the ED without prior registration
    // When I enter incomplete demographic information:
    await fillFields(page, [
      { Field: 'Given Name', Value: 'John' },
      { Field: 'Family Name', Value: '' },
      { Field: 'Date of Birth', Value: 'invalid-date' }
    ]);
    // And I submit the registration form
    await page.getByTestId('submit-registration-form').click();

    // Then the system displays validation errors:
    const familyNameError = await getText(page, 'Family Name Error');
    assert.strictEqual(familyNameError, 'Family name is required');
    const dateOfBirthError = await getText(page, 'Date of Birth Error');
    assert.strictEqual(dateOfBirthError, 'Invalid date format');
    // And the patient record is not created
    const medicalRecordNumbers = await locator(page, 'Medical Record Number').all();
    assert.strictEqual(medicalRecordNumbers.length, 0);
    // And the form remains open for correction
    assert.ok(await page.getByTestId('patient-registration-form').isVisible());
  });
});
