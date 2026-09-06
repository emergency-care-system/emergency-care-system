// Playwright test for spec/features/02-ambulance-arrival-registration.feature
// (equivalent to tests-with-selenium-javascript/02-ambulance-arrival-registration.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see support/fields.js) and the shared
// data-testid contract in support/login.js (login-identity, login-submit,
// app-root).

import { chromium, test } from '@playwright/test';
import { strict as assert } from 'assert';
import { login, verifySystemIsOperational } from './support/login.js';
import { fillFields, fillField, getText, locator, waitForTestId } from './support/fields.js';

test.describe.configure({ mode: 'serial' });

test.describe('Feature: Ambulance Arrival Registration', () => {
  let browser;
  let page;

  test.beforeAll(async () => {
    browser = await chromium.launch();
    page = await browser.newPage();
  });

  test.afterAll(async () => {
    await browser.close();
  });

  test.beforeEach(async () => {
    // Background:
    //   Given the ED management system is operational
    //   And I am logged in as registration staff
    //   And the "unknown patient" registration module is available
    await verifySystemIsOperational(page);
    await login(page, 'registration staff');
    // The "unknown patient" registration module availability is assumed
    // pre-seeded test data / environment configuration.

    const featureNavLink = await waitForTestId(page, 'Nav Ambulance Arrival Registration');
    await featureNavLink.click();
    await waitForTestId(page, 'Ambulance Arrival Registration Panel');
  });

  test('Register unconscious patient brought by ambulance', async () => {
    // Given an ambulance arrives with a patient who cannot provide identification
    // And the patient is unconscious and has no identification documents
    // And EMS provides the following information:
    //   | Field                 | Value                    |
    //   | Estimated Age         | 45-50 years              |
    //   | Gender                | Male                     |
    //   | Chief Complaint       | Motor vehicle accident   |
    //   | Vital Signs           | BP: 90/60, HR: 120       |
    //   | Incident Location     | Highway 55 Mile Marker 12|
    //   | EMS Unit              | Ambulance 205            |
    //   | Arrival Time          | 14:30                    |
    // When I select "Unknown Patient" registration type
    await fillField(page, 'Registration Type', 'Unknown Patient');
    // And I enter the available information from EMS
    await fillFields(page, [
      { Field: 'Estimated Age', Value: '45-50 years' },
      { Field: 'Gender', Value: 'Male' },
      { Field: 'Chief Complaint', Value: 'Motor vehicle accident' },
      { Field: 'Vital Signs', Value: 'BP: 90/60, HR: 120' },
      { Field: 'Incident Location', Value: 'Highway 55 Mile Marker 12' },
      { Field: 'EMS Unit', Value: 'Ambulance 205' },
      { Field: 'Arrival Time', Value: '14:30' },
    ]);
    // And I submit the registration
    await page.getByTestId('submit-registration-form').click();

    // Then the system creates a temporary patient record
    await waitForTestId(page, 'Temporary Patient Record');
    // And the system assigns a placeholder ID starting with "UNK"
    const placeholderId = await getText(page, 'Placeholder ID');
    assert.match(placeholderId, /^UNK/);
    // And the patient record is flagged for "Identity Verification Required"
    const identityFlag = await getText(page, 'Identity Verification Flag');
    assert.strictEqual(identityFlag, 'Identity Verification Required');
    // And the patient is immediately queued for triage
    const triageQueueStatus = await getText(page, 'Triage Queue Status');
    assert.match(triageQueueStatus, /queued for triage/i);
    // And a notification is sent to the charge nurse about the unknown patient
    const chargeNurseNotification = await getText(page, 'Charge Nurse Notification');
    assert.match(chargeNurseNotification, /unknown patient/i);
    // And the record shows status as "Temporary - Pending Identification"
    const recordStatus = await getText(page, 'Record Status');
    assert.strictEqual(recordStatus, 'Temporary - Pending Identification');
  });

  test('Register patient with partial identification from personal effects', async () => {
    // Given an ambulance arrives with a patient who cannot provide identification
    // And the patient has a wallet with partial information
    // And EMS provides the following information:
    //   | Field                 | Value                    |
    //   | Estimated Age         | 30-35 years              |
    //   | Gender                | Female                   |
    //   | Chief Complaint       | Drug overdose            |
    //   | Found Name            | Sarah (from credit card) |
    //   | Partial Phone         | 555-1234 (last 4 digits) |
    // When I select "Unknown Patient" registration type
    await fillField(page, 'Registration Type', 'Unknown Patient');
    // And I enter the EMS information including partial identity details
    await fillFields(page, [
      { Field: 'Estimated Age', Value: '30-35 years' },
      { Field: 'Gender', Value: 'Female' },
      { Field: 'Chief Complaint', Value: 'Drug overdose' },
      { Field: 'Found Name', Value: 'Sarah (from credit card)' },
      { Field: 'Partial Phone', Value: '555-1234 (last 4 digits)' },
    ]);
    // And I mark the identity fields as "Unverified"
    await fillField(page, 'Identity Status', 'Unverified');
    // And I submit the registration
    await page.getByTestId('submit-registration-form').click();

    // Then the system creates a temporary patient record
    await waitForTestId(page, 'Temporary Patient Record');
    // And the system assigns a placeholder ID starting with "UNK"
    const placeholderId = await getText(page, 'Placeholder ID');
    assert.match(placeholderId, /^UNK/);
    // And the partial identity information is stored with "Unverified" status
    const identityStatus = await getText(page, 'Identity Status');
    assert.strictEqual(identityStatus, 'Unverified');
    // And the patient record is flagged for "Identity Verification Required"
    const identityFlag = await getText(page, 'Identity Verification Flag');
    assert.strictEqual(identityFlag, 'Identity Verification Required');
    // And a task is created for social services to assist with identification
    const socialServicesTask = await getText(page, 'Social Services Task');
    assert.match(socialServicesTask, /identification/i);
  });

  test('Register patient who becomes conscious during registration', async () => {
    // Given an ambulance arrives with a patient who initially cannot provide identification
    // And I have started the "Unknown Patient" registration process

    // When the patient becomes conscious and provides identification:
    await fillFields(page, [
      { Field: 'Full Name', Value: 'Michael Johnson' },
      { Field: 'Date of Birth', Value: '1980-12-15' },
      { Field: 'Phone Number', Value: '555-876-5432' },
    ]);
    // And I verify the provided identification
    await page.getByTestId('verify-identification-button').click();

    // Then the system converts the temporary record to a verified patient record
    await waitForTestId(page, 'Medical Record Number');
    // And the placeholder ID is replaced with a permanent medical record number
    const medicalRecordNumber = await getText(page, 'Medical Record Number');
    assert.ok(!medicalRecordNumber.startsWith('UNK'));
    // And the "Identity Verification Required" flag is removed
    const identityFlagElements = await locator(page, 'Identity Verification Flag').all();
    assert.strictEqual(identityFlagElements.length, 0);
    // And the patient demographic information is updated
    const patientName = await getText(page, 'Patient Name');
    assert.strictEqual(patientName, 'Michael Johnson');
    // And a note is added documenting the identification process
    const identificationNote = await getText(page, 'Identification Note');
    assert.ok(identificationNote.length > 0);
  });

  test('Handle multiple unknown patients from mass casualty incident', async () => {
    // Given multiple ambulances arrive from a mass casualty incident
    // And none of the patients can provide identification

    // When I select "Unknown Patient - Mass Casualty" registration type
    await fillField(page, 'Registration Type', 'Unknown Patient - Mass Casualty');
    // And I enter the incident information:
    await fillFields(page, [
      { Field: 'Incident Type', Value: 'Multi-vehicle accident' },
      { Field: 'Incident Location', Value: 'Interstate 70 Exit 45' },
      { Field: 'Total Patients', Value: '4' },
    ]);
    // And I register each patient with EMS-provided information
    await page.getByTestId('submit-registration-form').click();

    // Then the system creates temporary records for all patients
    const temporaryRecords = await page.getByTestId('temporary-patient-record').all();
    assert.ok(temporaryRecords.length > 0);
    // And each patient gets a sequential placeholder ID (UNK-001, UNK-002, etc.)
    const placeholderId = (await locator(page, 'Placeholder ID').first().innerText()).trim();
    assert.match(placeholderId, /^UNK-\d{3}$/);
    // And all records are linked to the same incident number
    const incidentNumber = await getText(page, 'Incident Number');
    assert.ok(incidentNumber.length > 0);
    // And the mass casualty protocol is activated
    const massCasualtyProtocolStatus = await getText(page, 'Mass Casualty Protocol Status');
    assert.match(massCasualtyProtocolStatus, /activated/i);
    // And notifications are sent to administration and social services
    const notificationRecipients = await getText(page, 'Notification Recipients');
    assert.match(notificationRecipients, /administration/i);
  });

  test('Attempt to register unknown patient without EMS information', async () => {
    // Given an ambulance arrives with a patient who cannot provide identification
    // And EMS has minimal information available

    // When I select "Unknown Patient" registration type
    await fillField(page, 'Registration Type', 'Unknown Patient');
    // And I attempt to submit with only basic information:
    //   | Field                 | Value                    |
    //   | Gender                | Unknown                  |
    //   | Estimated Age         | Unknown                  |
    //   | Chief Complaint       |                          |
    await fillFields(page, [
      { Field: 'Gender', Value: 'Unknown' },
      { Field: 'Estimated Age', Value: 'Unknown' },
      { Field: 'Chief Complaint', Value: '' },
    ]);
    await page.getByTestId('submit-registration-form').click();

    // Then the system displays a warning "Insufficient information for registration"
    const warningMessage = await getText(page, 'Warning Message');
    assert.strictEqual(warningMessage, 'Insufficient information for registration');
    // And the system requires minimum data fields:
    //   | Required Field        | Requirement                       |
    //   | Estimated Age Range   | Must be provided                  |
    //   | Gender                | Must be Male, Female, or Unknown  |
    //   | Chief Complaint       | Must be provided                  |
    assert.strictEqual(await getText(page, 'Estimated Age Range Error'), 'Must be provided');
    assert.strictEqual(await getText(page, 'Gender Error'), 'Must be Male, Female, or Unknown');
    assert.strictEqual(await getText(page, 'Chief Complaint Error'), 'Must be provided');
    // And the registration cannot be completed until minimum requirements are met
    const placeholderIds = await locator(page, 'Placeholder ID').all();
    assert.strictEqual(placeholderIds.length, 0);
  });

  test('Identity verification process after patient stabilization', async () => {
    // Given a patient was registered as "Unknown Patient"
    // And the patient has now stabilized
    // And the patient can provide identification

    // When the nurse initiates the identity verification process
    await page.getByTestId('initiate-identity-verification-button').click();
    // And the patient provides valid identification:
    await fillFields(page, [
      { Field: 'Full Name', Value: 'Robert Davis' },
      { Field: 'Date of Birth', Value: '1975-08-20' },
      { Field: 'Social Security', Value: 'XXX-XX-1234 (last 4)' },
    ]);
    // And the identification is verified
    await page.getByTestId('verify-identification-button').click();

    // Then the system merges the temporary record with verified information
    await waitForTestId(page, 'Medical Record Number');
    const mergeStatus = await getText(page, 'Record Merge Status');
    assert.match(mergeStatus, /merged/i);
    // And the "Identity Verification Required" flag is cleared
    const identityFlagElements = await locator(page, 'Identity Verification Flag').all();
    assert.strictEqual(identityFlagElements.length, 0);
    // And a permanent medical record number is assigned
    const medicalRecordNumber = await getText(page, 'Medical Record Number');
    assert.ok(medicalRecordNumber.length > 0);
    // And all clinical documentation is preserved under the new verified record
    const clinicalDocumentationStatus = await getText(page, 'Clinical Documentation Status');
    assert.match(clinicalDocumentationStatus, /preserved/i);
    // And billing information is updated with verified patient details
    const billingStatus = await getText(page, 'Billing Status');
    assert.match(billingStatus, /updated/i);
  });
});
