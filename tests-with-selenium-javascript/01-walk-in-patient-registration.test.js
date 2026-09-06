// Selenium WebDriver + Mocha test for
// spec/features/01-walk-in-patient-registration.feature
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see support/fields.js) and the shared
// data-testid contract in support/login.js (login-identity, login-submit,
// app-root).

import { strict as assert } from 'assert';
import { buildDriver } from './support/build-driver.js';
import { login, verifySystemIsOperational } from './support/login.js';
import { fillFields, fillField, getText, locator, waitForTestId } from './support/fields.js';
import { By } from 'selenium-webdriver';

describe('Feature: Walk-in Patient Registration', function () {
  this.timeout(20000);
  let driver;

  before(async () => {
    driver = await buildDriver();
  });

  after(async () => {
    await driver.quit();
  });

  beforeEach(async () => {
    // Background:
    //   Given the emergency care system is operational
    //   And I am logged in as a registration clerk
    await verifySystemIsOperational(driver);
    await login(driver, 'a registration clerk');

    // The demo app is a single-page dashboard: after login, select this
    // feature's panel from the sidebar nav (data-testid="nav-<slug>").
    const registrationNavLink = await waitForTestId(driver, 'Nav Walk In Patient Registration');
    await registrationNavLink.click();
    await waitForTestId(driver, 'Patient Registration Form');
  });

  it('Successfully register a new walk-in patient', async () => {
    // Given a new patient arrives at the ED without prior registration
    // And the patient provides valid identification
    // When I enter the patient's demographic information:
    await fillFields(driver, [
      { Field: 'Given Name', Value: 'John' },
      { Field: 'Family Name', Value: 'Doe' },
      { Field: 'Date of Birth', Value: '1985-06-15' },
      { Field: 'Phone Number', Value: '555-123-4567' },
      { Field: 'Address', Value: '123 Main St' },
      { Field: 'City', Value: 'Springfield' },
      { Field: 'State', Value: 'IL' },
      { Field: 'Zip Code', Value: '62701' },
    ]);
    // And I enter the patient's insurance details:
    await fillFields(driver, [
      { Field: 'Insurance Type', Value: 'Blue Cross' },
      { Field: 'Policy Number', Value: 'BC123456789' },
      { Field: 'Group Number', Value: 'GRP001' },
    ]);
    // And I submit the registration form
    await driver.findElement(By.css('[data-testid="submit-registration-form"]')).click();

    // Then the system creates a unique patient record
    await waitForTestId(driver, 'Medical Record Number');
    // And the system assigns a medical record number
    const medicalRecordNumber = await getText(driver, 'Medical Record Number');
    assert.ok(medicalRecordNumber.length > 0);
    // And the patient is queued for triage
    const triageQueueStatus = await getText(driver, 'Triage Queue Status');
    assert.match(triageQueueStatus, /queued for triage/i);
    // And I see a confirmation message "Patient successfully registered"
    const confirmationMessage = await getText(driver, 'Confirmation Message');
    assert.strictEqual(confirmationMessage, 'Patient successfully registered');
    // And the medical record number is displayed
    const medicalRecordNumberElement = await driver.findElement(locator('Medical Record Number'));
    assert.ok(await medicalRecordNumberElement.isDisplayed());
  });

  it('Register patient with missing insurance information', async () => {
    // Given a new patient arrives at the ED without prior registration
    // And the patient does not have insurance information
    // When I enter the patient's demographic information:
    await fillFields(driver, [
      { Field: 'Given Name', Value: 'Jane' },
      { Field: 'Family Name', Value: 'Smith' },
      { Field: 'Date of Birth', Value: '1990-03-22' },
      { Field: 'Phone Number', Value: '555-987-6543' },
      { Field: 'Address', Value: '456 Oak Ave' },
    ]);
    // And I select "Self-Pay" as the insurance type
    await fillField(driver, 'Insurance Type', 'Self-Pay');
    // And I submit the registration form
    await driver.findElement(By.css('[data-testid="submit-registration-form"]')).click();

    // Then the system creates a unique patient record
    await waitForTestId(driver, 'Medical Record Number');
    // And the system assigns a medical record number
    const medicalRecordNumber = await getText(driver, 'Medical Record Number');
    assert.ok(medicalRecordNumber.length > 0);
    // And the patient is queued for triage
    const triageQueueStatus = await getText(driver, 'Triage Queue Status');
    assert.match(triageQueueStatus, /queued for triage/i);
    // And the insurance status is marked as "Self-Pay"
    const insuranceStatus = await getText(driver, 'Insurance Status');
    assert.strictEqual(insuranceStatus, 'Self-Pay');
  });

  it('Handle duplicate patient registration attempt', async () => {
    // Given a patient with the same name and date of birth already exists in the system
    // When I enter the patient's demographic information:
    await fillFields(driver, [
      { Field: 'Given Name', Value: 'John' },
      { Field: 'Family Name', Value: 'Doe' },
      { Field: 'Date of Birth', Value: '1985-06-15' },
    ]);
    // And I submit the registration form
    await driver.findElement(By.css('[data-testid="submit-registration-form"]')).click();

    // Then the system displays a warning "Potential duplicate patient found"
    const warningMessage = await getText(driver, 'Duplicate Patient Warning');
    assert.strictEqual(warningMessage, 'Potential duplicate patient found');
    // And the system shows existing patient records for verification
    const existingRecords = await driver.findElements(By.css('[data-testid="existing-patient-record"]'));
    assert.ok(existingRecords.length > 0);
    // And I can choose to link to existing record or create new record
    await waitForTestId(driver, 'Link to Existing Record');
    await waitForTestId(driver, 'Create New Record');
  });

  it('Registration with invalid demographic data', async () => {
    // Given a new patient arrives at the ED without prior registration
    // When I enter incomplete demographic information:
    await fillFields(driver, [
      { Field: 'Given Name', Value: 'John' },
      { Field: 'Family Name', Value: '' },
      { Field: 'Date of Birth', Value: 'invalid-date' },
    ]);
    // And I submit the registration form
    await driver.findElement(By.css('[data-testid="submit-registration-form"]')).click();

    // Then the system displays validation errors:
    const familyNameError = await getText(driver, 'Family Name Error');
    assert.strictEqual(familyNameError, 'Family name is required');
    const dateOfBirthError = await getText(driver, 'Date of Birth Error');
    assert.strictEqual(dateOfBirthError, 'Invalid date format');
    // And the patient record is not created
    const medicalRecordNumbers = await driver.findElements(locator('Medical Record Number'));
    assert.strictEqual(medicalRecordNumbers.length, 0);
    // And the form remains open for correction
    const registrationForm = await driver.findElement(By.css('[data-testid="patient-registration-form"]'));
    assert.ok(await registrationForm.isDisplayed());
  });
});
