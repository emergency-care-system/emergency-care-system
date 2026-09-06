// Selenium WebDriver + Mocha test for
// spec/features/09-lab-result-processing.feature
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

describe('Feature: Lab Result Processing', function () {
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
    //   And the HL7 interface with the laboratory system is active
    //   And critical value alert system is enabled
    //   And physician notification system is functional
    await verifySystemIsOperational(driver);
    // The HL7 interface, critical value alert system, and physician
    // notification system being active/enabled are assumed pre-seeded
    // test data / environment configuration. This feature has no
    // "logged in as" step in its Background.
    await login(driver, 'a lab technician');

    const labResultProcessingNavLink = await waitForTestId(driver, 'Nav Lab Result Processing');
    await labResultProcessingNavLink.click();
    await waitForTestId(driver, 'Lab Result Processing Panel');
  });

  it('Process normal lab results via HL7 interface', async () => {
    // Given a patient "Jennifer Lopez" is in bed "ED-8"
    // And laboratory orders were placed for "CBC, Basic Metabolic Panel"
    // And the attending physician is "Dr. Smith"
    // (assumed pre-seeded test data)

    // When the lab system sends results via HL7 interface:
    //   | Test Name          | Result    | Reference Range | Units  | Status   | Timestamp |
    //   | White Blood Cells  | 7.2       | 4.0-10.0       | K/uL   | Final    | 14:30     |
    //   | Hemoglobin         | 13.5      | 12.0-16.0      | g/dL   | Final    | 14:30     |
    //   | Sodium             | 140       | 136-145        | mmol/L | Final    | 14:30     |
    //   | Potassium          | 4.1       | 3.5-5.0        | mmol/L | Final    | 14:30     |
    //   | Creatinine         | 1.0       | 0.6-1.2        | mg/dL  | Final    | 14:30     |
    await driver.findElement(By.css('[data-testid="receive-lab-results-button"]')).click();

    // Then the system updates the patient record with all results
    const patientRecordUpdateStatus = await getText(driver, 'Patient Record Update Status');
    assert.match(patientRecordUpdateStatus, /updated/i);

    // And the results are marked as "Normal" in the patient chart
    const resultStatus = await getText(driver, 'Result Status');
    assert.strictEqual(resultStatus, 'Normal');

    // And a standard notification is sent to "Dr. Smith":
    assert.strictEqual(await getText(driver, 'Lab Results'), 'Normal CBC and BMP available for review');
    assert.strictEqual(await getText(driver, 'Patient'), 'Jennifer Lopez, Bed ED-8');
    assert.strictEqual(await getText(driver, 'Timestamp'), '14:30');
    assert.strictEqual(await getText(driver, 'Priority'), 'Standard');

    // And the results appear in the patient's timeline with normal value indicators
    const timelineEntries = await driver.findElements(By.css('[data-testid="timeline-result-entry"]'));
    assert.ok(timelineEntries.length > 0);

    // And no critical value alerts are generated
    const criticalValueAlerts = await driver.findElements(By.css('[data-testid="critical-value-alert"]'));
    assert.strictEqual(criticalValueAlerts.length, 0);

    // And the nursing staff is notified that results are available for review
    const nursingResultsNotification = await getText(driver, 'Nursing Results Notification');
    assert.ok(nursingResultsNotification.length > 0);
  });

  it('Process critical lab results with immediate alerts', async () => {
    // Given a patient "Michael Davis" is in bed "ED-12"
    // And laboratory orders were placed for "Troponin, BNP, D-Dimer"
    // And the attending physician is "Dr. Johnson"
    // (assumed pre-seeded test data)

    // When the lab system sends critical results via HL7 interface:
    //   | Test Name    | Result | Reference Range | Units  | Status | Critical | Timestamp |
    //   | Troponin I   | 8.5    | 0.0-0.04       | ng/mL  | Final  | Yes      | 15:45     |
    //   | BNP          | 1200   | 0-100          | pg/mL  | Final  | Yes      | 15:45     |
    //   | D-Dimer      | 0.8    | 0.0-0.5        | mg/L   | Final  | No       | 15:45     |
    await driver.findElement(By.css('[data-testid="receive-lab-results-button"]')).click();

    // Then the system immediately flags critical values:
    //   | Test Name    | Critical Flag | Severity Level |
    //   | Troponin I   | CRITICAL HIGH | Severe         |
    //   | BNP          | CRITICAL HIGH | High           |
    assert.strictEqual(await getText(driver, 'Troponin I Critical Flag'), 'CRITICAL HIGH');
    assert.strictEqual(await getText(driver, 'Troponin I Severity Level'), 'Severe');
    assert.strictEqual(await getText(driver, 'BNP Critical Flag'), 'CRITICAL HIGH');
    assert.strictEqual(await getText(driver, 'BNP Severity Level'), 'High');

    // And popup notifications are displayed for all logged-in providers:
    assert.strictEqual(await getText(driver, 'Critical Alert'), '🔴 CRITICAL: Troponin I = 8.5 ng/mL');
    assert.strictEqual(await getText(driver, 'High Alert'), '🟠 HIGH: BNP = 1200 pg/mL');
    assert.strictEqual(await getText(driver, 'Patient Info'), 'Michael Davis, Bed ED-12');

    // And an immediate notification is sent to "Dr. Johnson":
    assert.strictEqual(await getText(driver, 'Mobile Push'), 'CRITICAL LAB: Troponin 8.5 - Michael Davis');
    assert.strictEqual(await getText(driver, 'SMS Alert'), 'ED-12 CRITICAL Troponin I: 8.5 ng/mL');
    assert.strictEqual(await getText(driver, 'In-App Alert'), 'High priority popup requiring acknowledgment');

    // And the charge nurse receives a critical value notification
    const chargeNurseNotification = await getText(driver, 'Charge Nurse Notification');
    assert.ok(chargeNurseNotification.length > 0);

    // And the results are highlighted in red on all patient displays
    const criticalResultHighlights = await driver.findElements(By.css('[data-testid="critical-result-highlight"]'));
    assert.ok(criticalResultHighlights.length > 0);

    // And an audit trail is created for the critical value communication
    const auditTrail = await driver.findElement(By.css('[data-testid="critical-value-audit-trail"]'));
    assert.ok(await auditTrail.isDisplayed());
  });

  it('Handle lab results with different statuses and corrections', async () => {
    // Given a patient "Sarah Wilson" is in bed "ED-6"
    // And previous lab results were reported
    // (assumed pre-seeded test data)

    // When the lab system sends updated results via HL7 interface:
    //   | Test Name     | Result | Status     | Previous Result | Correction Reason    | Timestamp |
    //   | Hemoglobin    | 9.2    | Corrected  | 11.2           | Sample hemolysis     | 16:15     |
    //   | Glucose       | 250    | Final      | -              | -                    | 16:15     |
    //   | Pending Test  | -      | Pending    | -              | Sample reprocessing  | 16:15     |
    await driver.findElement(By.css('[data-testid="receive-lab-results-button"]')).click();

    // Then the system processes different result statuses:
    assert.strictEqual(await getText(driver, 'Corrected'), 'Replace previous value, maintain history');
    assert.strictEqual(await getText(driver, 'Final'), 'Add new result to patient record');
    assert.strictEqual(await getText(driver, 'Pending'), 'Update status, maintain order tracking');

    // And correction notifications are sent:
    assert.strictEqual(await getText(driver, 'Correction Alert'), 'Lab value corrected: Hgb 11.2 → 9.2 g/dL');
    assert.strictEqual(await getText(driver, 'Reason'), 'Sample hemolysis detected');
    assert.strictEqual(await getText(driver, 'Clinical Impact'), 'Anemia now more severe than initially reported');

    // And the attending physician "Dr. Martinez" is notified of the correction
    const physicianCorrectionNotification = await getText(driver, 'Physician Correction Notification');
    assert.ok(physicianCorrectionNotification.length > 0);

    // And the original result is preserved in the audit trail
    const originalResultAuditEntry = await driver.findElement(By.css('[data-testid="original-result-audit-entry"]'));
    assert.ok(await originalResultAuditEntry.isDisplayed());

    // And the corrected value triggers anemia protocol alerts
    await waitForTestId(driver, 'Anemia Protocol Alert');
  });

  it('Process pediatric lab results with age-specific reference ranges', async () => {
    // Given a pediatric patient "Emma Foster" (age 6) is in bed "ED-PEDS-2"
    // And laboratory orders were placed for "CBC, CMP"
    // (assumed pre-seeded test data)

    // When the lab system sends pediatric results via HL7 interface:
    //   | Test Name          | Result | Adult Range    | Pediatric Range (Age 6) | Units  | Status |
    //   | White Blood Cells  | 12.5   | 4.0-10.0      | 5.0-14.5               | K/uL   | Final  |
    //   | Hemoglobin         | 11.8   | 12.0-16.0     | 11.5-13.5              | g/dL   | Final  |
    //   | Alkaline Phosphatase| 250   | 44-147        | 156-369                | U/L    | Final  |
    await driver.findElement(By.css('[data-testid="receive-lab-results-button"]')).click();

    // Then the system applies age-appropriate reference ranges:
    //   | Test Name          | Interpretation      | Flag        |
    //   | White Blood Cells  | Normal for age 6    | Normal      |
    //   | Hemoglobin         | Normal for age 6    | Normal      |
    //   | Alkaline Phosphatase| Normal for age 6   | Normal      |
    assert.strictEqual(await getText(driver, 'White Blood Cells Interpretation'), 'Normal for age 6');
    assert.strictEqual(await getText(driver, 'White Blood Cells Flag'), 'Normal');
    assert.strictEqual(await getText(driver, 'Hemoglobin Interpretation'), 'Normal for age 6');
    assert.strictEqual(await getText(driver, 'Hemoglobin Flag'), 'Normal');
    assert.strictEqual(await getText(driver, 'Alkaline Phosphatase Interpretation'), 'Normal for age 6');
    assert.strictEqual(await getText(driver, 'Alkaline Phosphatase Flag'), 'Normal');

    // And the pediatric attending "Dr. Chen" is notified with age-specific context
    const pediatricAttendingNotification = await getText(driver, 'Pediatric Attending Notification');
    assert.ok(pediatricAttendingNotification.length > 0);

    // And the results display shows both adult and pediatric reference ranges
    await waitForTestId(driver, 'Adult Reference Range');
    await waitForTestId(driver, 'Pediatric Reference Range');

    // And no inappropriate critical alerts are generated for age-normal values
    const criticalValueAlerts = await driver.findElements(By.css('[data-testid="critical-value-alert"]'));
    assert.strictEqual(criticalValueAlerts.length, 0);
  });

  it('Handle lab results during physician handoff', async () => {
    // Given a patient "Robert Kim" is in bed "ED-15"
    // And the day shift physician "Dr. Adams" ordered labs at 18:00
    // And the evening shift physician "Dr. Brown" has taken over at 19:00
    // (assumed pre-seeded test data)

    // When the lab system sends results via HL7 interface at 19:30:
    //   | Test Name     | Result | Reference Range | Status | Critical |
    //   | Lipase        | 350    | 10-140         | Final  | Yes      |
    //   | Amylase       | 180    | 25-125         | Final  | No       |
    await driver.findElement(By.css('[data-testid="receive-lab-results-button"]')).click();

    // Then the system determines the appropriate physician to notify:
    //   | Notification Target | Rationale                                  |
    //   | Primary: Dr. Brown  | Current attending physician                |
    //   | Secondary: Dr. Adams| Ordered the tests, may need notification   |
    assert.strictEqual(await getText(driver, 'Primary Notification Target'), 'Dr. Brown');
    assert.strictEqual(await getText(driver, 'Primary Notification Rationale'), 'Current attending physician');
    assert.strictEqual(await getText(driver, 'Secondary Notification Target'), 'Dr. Adams');
    assert.strictEqual(
      await getText(driver, 'Secondary Notification Rationale'),
      'Ordered the tests, may need notification'
    );

    // And both physicians receive notifications with handoff context:
    assert.strictEqual(await getText(driver, 'Dr. Brown Notification'), 'CRITICAL: Lipase 350 - Patient from Dr. Adams');
    assert.strictEqual(await getText(driver, 'Dr. Adams Notification'), 'FYI: Your lipase order critical - Now Dr. Brown');

    // And the handoff log is updated with the critical result information
    const handoffLog = await driver.findElement(By.css('[data-testid="handoff-log"]'));
    assert.ok(await handoffLog.isDisplayed());

    // And the charge nurse is notified of the critical value during shift change
    const chargeNurseShiftChangeNotification = await getText(driver, 'Charge Nurse Shift Change Notification');
    assert.ok(chargeNurseShiftChangeNotification.length > 0);
  });

  it('Process lab results with technical failures and retries', async () => {
    // Given a patient "Lisa Garcia" is in bed "ED-3"
    // And laboratory results are ready for transmission
    // (assumed pre-seeded test data)

    // When the lab system attempts to send results via HL7 interface
    // And the initial transmission fails due to network connectivity
    // And the lab system retries transmission after 5 minutes
    // (no direct UI action for these narrative steps)

    // And the retry is successful with results:
    //   | Test Name   | Result | Reference Range | Status | Timestamp |
    //   | Troponin    | 0.02   | 0.0-0.04       | Final  | 20:15     |
    await driver.findElement(By.css('[data-testid="receive-lab-results-button"]')).click();

    // Then the system processes the delayed results
    const delayedResultProcessingStatus = await getText(driver, 'Delayed Result Processing Status');
    assert.match(delayedResultProcessingStatus, /processed/i);

    // And a delay notification is included:
    assert.strictEqual(await getText(driver, 'Delay Notice'), 'Results delayed due to technical issues');
    assert.strictEqual(await getText(driver, 'Original Time'), 'Results ready at 20:10');
    assert.strictEqual(await getText(driver, 'Received Time'), 'Results received at 20:15');

    // And the attending physician is notified of both the results and the delay
    const physicianDelayNotification = await getText(driver, 'Physician Delay Notification');
    assert.ok(physicianDelayNotification.length > 0);

    // And system administrators are alerted to the interface failure
    const systemAdministratorAlert = await getText(driver, 'System Administrator Alert');
    assert.ok(systemAdministratorAlert.length > 0);

    // And the delay is documented in the interface audit log
    const interfaceAuditLog = await driver.findElement(By.css('[data-testid="interface-audit-log"]'));
    assert.ok(await interfaceAuditLog.isDisplayed());
  });

  it('Handle batch lab results processing', async () => {
    // Given multiple patients have pending lab results:
    //   | Patient Name    | Bed    | Attending     | Tests Ordered        |
    //   | Alice Johnson   | ED-4   | Dr. Smith     | CBC, BMP             |
    //   | Bob Thompson    | ED-7   | Dr. Smith     | Liver function tests |
    //   | Carol Martinez  | ED-11  | Dr. Brown     | Cardiac enzymes      |
    // (assumed pre-seeded test data)

    // When the lab system sends batch results via HL7 interface:
    //   | Patient       | Test Results                              | Critical Values |
    //   | Alice Johnson | All normal values                         | None           |
    //   | Bob Thompson  | ALT: 150 (High), AST: 120 (High)        | None           |
    //   | Carol Martinez| Troponin: 2.1 (Critical)                | Troponin       |
    await driver.findElement(By.css('[data-testid="receive-lab-results-button"]')).click();

    // Then the system processes all results simultaneously
    const batchProcessingStatus = await getText(driver, 'Batch Processing Status');
    assert.match(batchProcessingStatus, /simultaneously/i);

    // And notifications are prioritized by criticality:
    //   | Priority | Patient        | Notification Type    |
    //   | 1        | Carol Martinez | Critical value alert |
    //   | 2        | Bob Thompson   | Abnormal value alert |
    //   | 3        | Alice Johnson  | Normal results       |
    assert.strictEqual(await getText(driver, 'Priority 1 Patient'), 'Carol Martinez');
    assert.strictEqual(await getText(driver, 'Priority 1 Notification Type'), 'Critical value alert');
    assert.strictEqual(await getText(driver, 'Priority 2 Patient'), 'Bob Thompson');
    assert.strictEqual(await getText(driver, 'Priority 2 Notification Type'), 'Abnormal value alert');
    assert.strictEqual(await getText(driver, 'Priority 3 Patient'), 'Alice Johnson');
    assert.strictEqual(await getText(driver, 'Priority 3 Notification Type'), 'Normal results');

    // And physicians receive consolidated notifications when appropriate
    const consolidatedNotification = await getText(driver, 'Consolidated Notification');
    assert.ok(consolidatedNotification.length > 0);

    // And system performance metrics are maintained during batch processing
    await waitForTestId(driver, 'System Performance Metrics');
  });

  it('Process lab results with interpretation comments', async () => {
    // Given a patient "David Lee" is in bed "ED-9"
    // And complex laboratory tests were ordered
    // (assumed pre-seeded test data)

    // When the lab system sends results with pathologist interpretation:
    //   | Test Name        | Result | Reference | Interpretation                    | Timestamp |
    //   | Blood Smear      | -      | -         | Moderate anisocytosis noted      | 21:00     |
    //   | Hemoglobin A1C   | 9.2%   | <5.7%     | Consistent with poor DM control  | 21:00     |
    //   | Thyroid Function | -      | -         | Pattern suggests hyperthyroidism | 21:00     |
    await driver.findElement(By.css('[data-testid="receive-lab-results-button"]')).click();

    // Then the system includes interpretation comments in the patient record
    const interpretationComments = await driver.findElements(By.css('[data-testid="interpretation-comment"]'));
    assert.strictEqual(interpretationComments.length, 3);

    // And the attending physician receives enhanced notifications:
    assert.strictEqual(await getText(driver, 'Raw Results'), 'Numeric values and reference ranges');
    assert.strictEqual(await getText(driver, 'Interpretation'), 'Pathologist comments and clinical significance');
    assert.strictEqual(await getText(driver, 'Recommendations'), 'Suggested follow-up or additional testing');

    // And interpretation comments are highlighted in the patient chart
    const highlightedInterpretationComments = await driver.findElements(
      By.css('[data-testid="highlighted-interpretation-comment"]')
    );
    assert.ok(highlightedInterpretationComments.length > 0);

    // And complex results are flagged for physician review and acknowledgment
    await waitForTestId(driver, 'Physician Review Flag');
  });
});
