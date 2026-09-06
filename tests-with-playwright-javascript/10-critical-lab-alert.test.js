// Playwright test for spec/features/10-critical-lab-alert.feature
// (equivalent to tests-with-selenium-javascript/10-critical-lab-alert.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see support/fields.js) and the shared
// data-testid contract in support/login.js (login-identity, login-submit,
// app-root).

import { chromium, test } from '@playwright/test';
import { strict as assert } from 'assert';
import { login, verifySystemIsOperational } from './support/login.js';
import { fillFields, fillField, getText, waitForTestId } from './support/fields.js';

// All scenarios in this file share one page (like the Selenium suite shares
// one WebDriver per file), since later scenarios rely on data earlier
// scenarios registered persisting in the app's localStorage -- so they must
// run in file order, not in parallel.
test.describe.configure({ mode: 'serial' });

test.describe('Feature: Critical Lab Alert', () => {
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
    //   Given the emergency care system is operational
    //   And the critical value alert system is enabled
    //   And laboratory interfaces are functioning
    //   And all patient displays are connected to the alert system
    await verifySystemIsOperational(page);
    // The critical value alert system, laboratory interfaces, and patient
    // display connections are assumed pre-seeded test data / environment
    // configuration. This feature has no "logged in as" step in its
    // Background.
    await login(page, 'a lab technician');

    const criticalLabAlertNavLink = await waitForTestId(page, 'Nav Critical Lab Alert');
    await criticalLabAlertNavLink.click();
    await waitForTestId(page, 'Critical Lab Alert Panel');
  });

  test('Process critically high troponin result with immediate alerts', async () => {
    // Given a patient "Robert Martinez" is in bed "ED-7"
    // And the attending physician is "Dr. Johnson"
    // And the charge nurse is "Nurse Williams"
    // And troponin was ordered for "chest pain evaluation"
    // (assumed pre-seeded test data)

    // When the laboratory result is received:
    //   | Test Name       | Result | Reference Range | Units  | Critical Threshold | Status |
    //   | Troponin I      | 5.8    | 0.0-0.04       | ng/mL  | >0.4              | Final  |
    await fillFields(page, [
      { Field: 'Test Name', Value: 'Troponin I' },
      { Field: 'Result', Value: '5.8' },
      { Field: 'Reference Range', Value: '0.0-0.04' },
      { Field: 'Units', Value: 'ng/mL' },
      { Field: 'Critical Threshold', Value: '>0.4' },
      { Field: 'Status', Value: 'Final' },
    ]);
    await page.getByTestId('receive-lab-result-button').click();

    // Then the system immediately triggers critical alerts
    const criticalAlertStatus = await getText(page, 'Critical Alert Status');
    assert.match(criticalAlertStatus, /triggered/i);

    // And the attending physician "Dr. Johnson" receives immediate notifications:
    //   | Notification Type | Content                                        | Delivery Method |
    //   | Mobile Push Alert | 🔴 CRITICAL: Troponin I 5.8 ng/mL - ED-7     | Mobile App      |
    //   | SMS Alert        | CRITICAL LAB: R.Martinez ED-7 Troponin 5.8    | Text Message    |
    //   | Popup Alert      | CRITICAL VALUE - Requires Acknowledgment       | Workstation     |
    assert.strictEqual(await getText(page, 'Mobile Push Alert'), '🔴 CRITICAL: Troponin I 5.8 ng/mL - ED-7');
    assert.strictEqual(await getText(page, 'SMS Alert'), 'CRITICAL LAB: R.Martinez ED-7 Troponin 5.8');
    assert.strictEqual(await getText(page, 'Popup Alert'), 'CRITICAL VALUE - Requires Acknowledgment');

    // And the charge nurse "Nurse Williams" receives critical notifications:
    //   | Notification Type | Content                                        | Delivery Method |
    //   | Desktop Alert    | CRITICAL: Troponin 5.8 - Bed ED-7            | Workstation     |
    //   | Overhead Page    | Critical lab value bed ED-7                   | PA System       |
    //   | Mobile Alert     | Critical troponin result requires attention   | Mobile Device   |
    assert.strictEqual(await getText(page, 'Desktop Alert'), 'CRITICAL: Troponin 5.8 - Bed ED-7');
    assert.strictEqual(await getText(page, 'Overhead Page'), 'Critical lab value bed ED-7');
    assert.strictEqual(await getText(page, 'Mobile Alert'), 'Critical troponin result requires attention');

    // And red flag indicators appear on all patient displays:
    //   | Display Location     | Alert Indicator                              |
    //   | Patient Monitor      | 🔴 CRITICAL LAB flashing red banner         |
    //   | Bedside Workstation  | Red alert icon next to patient name         |
    //   | Main ED Dashboard    | Red flag on bed ED-7 status                 |
    //   | Mobile Devices       | Red notification badge on patient chart     |
    //   | Nursing Station      | Critical value alert on patient board       |
    const redFlagIndicators = await page.getByTestId('red-flag-indicator').all();
    assert.strictEqual(redFlagIndicators.length, 5);
  });

  test('Handle critical troponin with physician acknowledgment requirements', async () => {
    // Given a patient "Maria Santos" is in bed "ED-12"
    // And the attending physician is "Dr. Lee"
    // And a critically high troponin result of "7.2 ng/mL" is received
    // (assumed pre-seeded test data)

    // When the critical alert is triggered
    await page.getByTestId('trigger-critical-alert-button').click();

    // Then the system requires physician acknowledgment:
    assert.strictEqual(await getText(page, 'Initial Alert'), 'Must acknowledge receipt within 15 minutes');
    assert.strictEqual(await getText(page, 'Clinical Review'), 'Must document result review');
    assert.strictEqual(await getText(page, 'Action Plan'), 'Must indicate next steps taken');

    // And if "Dr. Lee" does not acknowledge within 15 minutes:
    assert.strictEqual(await getText(page, 'Secondary Alert'), 'Alert sent to backup physician');
    assert.strictEqual(await getText(page, 'Charge Nurse Alert'), 'Escalation notice to charge nurse');
    assert.strictEqual(await getText(page, 'Supervisor Alert'), 'Department supervisor notified');

    // And the acknowledgment status is tracked:
    //   | Status              | Timestamp | Provider    | Action              |
    //   | Alert Sent          | 14:30:15  | System      | Initial notification|
    //   | Acknowledged        | 14:32:45  | Dr. Lee     | Acknowledged receipt|
    //   | Reviewed            | 14:35:20  | Dr. Lee     | Documented review   |
    //   | Action Taken        | 14:40:10  | Dr. Lee     | Treatment initiated |
    const acknowledgmentStatusEntries = await page.getByTestId('acknowledgment-status-entry').all();
    assert.strictEqual(acknowledgmentStatusEntries.length, 4);
  });

  test('Process multiple critical values simultaneously', async () => {
    // Given multiple patients have critical troponin results:
    //   | Patient Name    | Bed   | Troponin Result | Attending     | Severity  |
    //   | John Williams   | ED-3  | 3.2 ng/mL      | Dr. Adams     | High      |
    //   | Lisa Johnson    | ED-8  | 8.9 ng/mL      | Dr. Brown     | Critical  |
    //   | Mike Davis      | ED-15 | 4.1 ng/mL      | Dr. Adams     | High      |
    // (assumed pre-seeded test data)

    // When all critical results are received simultaneously
    await page.getByTestId('receive-critical-results-button').click();

    // Then the system prioritizes alerts by severity:
    //   | Priority | Patient      | Alert Level | Notification Urgency    |
    //   | 1        | Lisa Johnson | Critical    | Immediate - All channels|
    //   | 2        | Mike Davis   | High        | Urgent - Standard alerts|
    //   | 3        | John Williams| High        | Urgent - Standard alerts|
    assert.strictEqual(await getText(page, 'Priority 1 Patient'), 'Lisa Johnson');
    assert.strictEqual(await getText(page, 'Priority 1 Alert Level'), 'Critical');
    assert.strictEqual(await getText(page, 'Priority 2 Patient'), 'Mike Davis');
    assert.strictEqual(await getText(page, 'Priority 2 Alert Level'), 'High');
    assert.strictEqual(await getText(page, 'Priority 3 Patient'), 'John Williams');
    assert.strictEqual(await getText(page, 'Priority 3 Alert Level'), 'High');

    // And physicians receive prioritized notifications:
    assert.strictEqual(await getText(page, 'Dr. Brown Alert Summary'), 'CRITICAL: Lisa Johnson Trop 8.9 - IMMEDIATE');
    assert.strictEqual(await getText(page, 'Dr. Adams Alert Summary'), 'HIGH: 2 patients with elevated troponin');

    // And the charge nurse receives a summary alert:
    assert.strictEqual(await getText(page, 'Mass Alert'), '3 critical troponin results requiring attention');
    assert.strictEqual(await getText(page, 'Priority List'), 'Lisa Johnson (Critical), others (High)');

    // And all patient displays show appropriately color-coded flags
    const colorCodedFlags = await page.getByTestId('color-coded-flag').all();
    assert.strictEqual(colorCodedFlags.length, 3);
  });

  test('Handle critical troponin during shift change', async () => {
    // Given a patient "Catherine Brown" is in bed "ED-6"
    // And it is 19:00 during evening shift change
    // And the day shift physician "Dr. Wilson" ordered the troponin
    // And the evening shift physician "Dr. Taylor" has assumed care
    // (assumed pre-seeded test data)

    // When a critically high troponin result of "6.1 ng/mL" is received
    await fillField(page, 'Troponin Result', '6.1 ng/mL');
    await page.getByTestId('receive-lab-result-button').click();

    // Then both physicians receive critical alerts:
    //   | Physician  | Alert Type    | Content                                |
    //   | Dr. Taylor | Primary Alert | CRITICAL Troponin 6.1 - Your patient  |
    //   | Dr. Wilson | Handoff Alert | FYI: Critical result on your order     |
    assert.strictEqual(await getText(page, 'Dr. Taylor Alert'), 'CRITICAL Troponin 6.1 - Your patient');
    assert.strictEqual(await getText(page, 'Dr. Wilson Alert'), 'FYI: Critical result on your order');

    // And the charge nurse receives handoff-specific notification:
    assert.strictEqual(await getText(page, 'Shift Context'), 'Critical result during physician handoff');
    assert.strictEqual(await getText(page, 'Current MD'), 'Dr. Taylor (assuming care)');
    assert.strictEqual(await getText(page, 'Ordering MD'), 'Dr. Wilson (ordered test)');

    // And the handoff documentation is automatically updated
    const handoffDocumentation = page.getByTestId('handoff-documentation');
    assert.ok(await handoffDocumentation.isVisible());

    // And red flags appear with shift change context indicators
    const redFlagIndicators = await page.getByTestId('red-flag-indicator').all();
    assert.ok(redFlagIndicators.length > 0);
  });

  test('Process critical troponin with additional cardiac markers', async () => {
    // Given a patient "Steven Kim" is in bed "ED-11"
    // And multiple cardiac markers were ordered
    // (assumed pre-seeded test data)

    // When critical and related results are received:
    //   | Test Name    | Result | Reference Range | Critical | Clinical Significance |
    //   | Troponin I   | 4.7    | 0.0-0.04       | Yes      | Acute MI indicated    |
    //   | CK-MB        | 45     | 0-6.3          | Yes      | Myocardial damage     |
    //   | Myoglobin    | 280    | 25-72          | No       | Elevated but not critical|
    await page.getByTestId('receive-lab-results-button').click();

    // Then the system groups related critical values:
    //   | Alert Category  | Content                                      |
    //   | Cardiac Panel   | Multiple critical cardiac markers            |
    //   | Primary Alert   | Troponin I: 4.7 ng/mL (CRITICAL)           |
    //   | Secondary Alert | CK-MB: 45 ng/mL (CRITICAL)                 |
    //   | Supporting Data | Myoglobin: 280 ng/mL (Elevated)            |
    assert.strictEqual(await getText(page, 'Cardiac Panel'), 'Multiple critical cardiac markers');
    assert.strictEqual(await getText(page, 'Primary Alert'), 'Troponin I: 4.7 ng/mL (CRITICAL)');
    assert.strictEqual(await getText(page, 'Secondary Alert'), 'CK-MB: 45 ng/mL (CRITICAL)');
    assert.strictEqual(await getText(page, 'Supporting Data'), 'Myoglobin: 280 ng/mL (Elevated)');

    // And enhanced clinical context is provided:
    assert.strictEqual(await getText(page, 'Clinical Indication'), 'Acute myocardial infarction likely');
    assert.strictEqual(await getText(page, 'Recommended Actions'), 'Cardiology consult, STEMI protocol');
    assert.strictEqual(await getText(page, 'Time Sensitivity'), 'Treatment within 90 minutes critical');

    // And STEMI protocol alerts are automatically triggered
    await waitForTestId(page, 'STEMI Protocol Alert');
  });

  test('Handle false positive critical troponin alerts', async () => {
    // Given a patient "Nancy Rodriguez" is in bed "ED-4"
    // And a troponin result of "5.1 ng/mL" triggers a critical alert
    // (assumed pre-seeded test data)

    // When the laboratory calls to report a sample error
    await page.getByTestId('report-sample-error-button').click();

    // And a corrected result shows "0.03 ng/mL" (normal)
    await fillField(page, 'Corrected Troponin Result', '0.03 ng/mL');
    await page.getByTestId('submit-correction-button').click();

    // Then the system processes the correction:
    assert.strictEqual(await getText(page, 'Cancel Alert'), 'Original critical alert is cancelled');
    assert.strictEqual(await getText(page, 'Send Correction'), 'Corrected value sent to all recipients');
    assert.strictEqual(await getText(page, 'Document Error'), 'Lab error documented in audit trail');

    // And correction notifications are sent:
    assert.strictEqual(await getText(page, 'Alert Cancellation'), 'CANCELLED: Previous critical troponin alert');
    assert.strictEqual(await getText(page, 'Corrected Value'), 'Troponin corrected to 0.03 ng/mL (Normal)');
    assert.strictEqual(await getText(page, 'Error Explanation'), 'Laboratory sample contamination identified');

    // And red flags are removed from all patient displays
    const redFlagIndicators = await page.getByTestId('red-flag-indicator').all();
    assert.strictEqual(redFlagIndicators.length, 0);

    // And the correction is logged for quality assurance review
    const qualityAssuranceLog = page.getByTestId('quality-assurance-log');
    assert.ok(await qualityAssuranceLog.isVisible());
  });

  test('Critical troponin with patient transfer requirements', async () => {
    // Given a patient "Timothy Chang" is in bed "ED-9"
    // And a critically high troponin of "9.3 ng/mL" is received
    // And the patient requires immediate transfer to cardiac unit
    // (assumed pre-seeded test data)

    // When the critical alert is processed
    await page.getByTestId('process-critical-alert-button').click();

    // Then transfer coordination alerts are included:
    assert.strictEqual(await getText(page, 'Transfer Required'), 'Patient needs immediate cardiac unit transfer');
    assert.strictEqual(await getText(page, 'Bed Availability'), 'CCU bed 302 available');
    assert.strictEqual(await getText(page, 'Transport Time'), 'Transport team ETA 10 minutes');

    // And receiving unit notifications are sent:
    assert.strictEqual(await getText(page, 'CCU Alert'), 'Incoming transfer - Critical troponin 9.3');
    assert.strictEqual(await getText(page, 'Cardiology Alert'), 'Urgent consult needed - STEMI protocol');

    // And transfer documentation is automatically initiated
    const transferDocumentation = page.getByTestId('transfer-documentation');
    assert.ok(await transferDocumentation.isVisible());

    // And critical alerts follow the patient to the receiving unit
    await waitForTestId(page, 'Patient Alert Handoff');
  });

  test('Validate critical troponin alert system functionality', async () => {
    // Given the critical alert system is being tested
    // (assumed pre-seeded test data)

    // When a test troponin result of "TEST-5.0 ng/mL" is processed
    await fillField(page, 'Test Troponin Result', 'TEST-5.0 ng/mL');
    await page.getByTestId('process-test-result-button').click();

    // Then the system validates all alert pathways:
    assert.strictEqual(await getText(page, 'Physician Mobile'), 'Test alert delivered successfully');
    assert.strictEqual(await getText(page, 'Charge Nurse'), 'Test alert delivered successfully');
    assert.strictEqual(await getText(page, 'Patient Displays'), 'Red flags displayed correctly');
    assert.strictEqual(await getText(page, 'Audit Trail'), 'Test alert logged with timestamp');

    // And test alerts are clearly marked as "SYSTEM TEST"
    const testAlertMarking = await getText(page, 'Test Alert Marking');
    assert.strictEqual(testAlertMarking, 'SYSTEM TEST');

    // And all test alerts are automatically cleared after validation
    const testAlerts = await page.getByTestId('test-alert').all();
    assert.strictEqual(testAlerts.length, 0);

    // And system performance metrics are recorded:
    //   | Metric           | Measurement                                  |
    //   | Alert Latency    | <30 seconds from result to notification     |
    //   | Delivery Success | 100% successful delivery to all recipients  |
    //   | Display Update   | <5 seconds to update all patient displays   |
    assert.strictEqual(await getText(page, 'Alert Latency'), '<30 seconds from result to notification');
    assert.strictEqual(await getText(page, 'Delivery Success'), '100% successful delivery to all recipients');
    assert.strictEqual(await getText(page, 'Display Update'), '<5 seconds to update all patient displays');
  });
});
