// Playwright test for spec/features/07-physician-assessment.feature
// (equivalent to tests-with-selenium-javascript/07-physician-assessment.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see support/fields.js) and the shared
// data-testid contract in support/login.js (login-identity, login-submit,
// app-root).

import { chromium, test } from '@playwright/test';
import { strict as assert } from 'assert';
import { login, verifySystemIsOperational } from './support/login.js';
import { getText, waitForTestId } from './support/fields.js';

test.describe.configure({ mode: 'serial' });

test.describe('Feature: Physician Assessment', () => {
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
    //   And I am logged in as "Dr. Smith" on the mobile app
    //   And the patient chart access module is enabled
    //   And real-time data synchronization is active
    await verifySystemIsOperational(page);
    await login(page, 'Dr. Smith', { mobile: true });
    // The patient chart access module and real-time data synchronization are
    // assumed to be pre-seeded/enabled test data.

    const featureNavLink = await waitForTestId(page, 'Nav Physician Assessment');
    await featureNavLink.click();
    await waitForTestId(page, 'Physician Assessment Panel');
  });

  test('Access patient chart with complete nursing assessment', async () => {
    // Given a patient "Jennifer Martinez" is assigned to bed "ED-8"
    // And the nursing assessment is completed with the following data:
    // (assumed pre-seeded test data)

    // When I open the patient's chart for "Jennifer Martinez" in bed "ED-8"
    await page.getByTestId('open-patient-chart-button').click();

    // Then the system displays the patient summary with:
    const patientSummaryFields = [
      { Section: 'Patient Identity', Content: 'Jennifer Martinez, DOB: 1975-03-15' },
      { Section: 'Bed Assignment', Content: 'ED-8' },
      { Section: 'Arrival Time', Content: '14:00' },
      { Section: 'Triage Notes', Content: 'ESI Level 2 - Severe chest pain, onset 2h ago' },
    ];
    for (const { Section, Content } of patientSummaryFields) {
      assert.strictEqual(await getText(page, Section), Content);
    }

    // And the vital signs section shows:
    const vitalSigns = [
      { 'Vital Sign': 'Blood Pressure', Value: '160/95', Trend: 'High' },
      { 'Vital Sign': 'Heart Rate', Value: '110', Trend: 'Elevated' },
      { 'Vital Sign': 'Respiratory Rate', Value: '22', Trend: 'Elevated' },
      { 'Vital Sign': 'Temperature', Value: '98.6°F', Trend: 'Normal' },
      { 'Vital Sign': 'Oxygen Saturation', Value: '94%', Trend: 'Low' },
      { 'Vital Sign': 'Pain Score', Value: '8/10', Trend: 'Severe' },
    ];
    for (const vital of vitalSigns) {
      assert.strictEqual(await getText(page, vital['Vital Sign']), vital.Value);
      assert.strictEqual(await getText(page, `${vital['Vital Sign']} Trend`), vital.Trend);
    }

    // And the allergies section displays:
    const allergies = [
      { Allergy: 'Penicillin', 'Reaction Type': 'Rash', Severity: 'Moderate' },
      { Allergy: 'Shellfish', 'Reaction Type': 'Unknown', Severity: 'Unknown' },
    ];
    for (const allergy of allergies) {
      assert.strictEqual(await getText(page, `${allergy.Allergy} Reaction`), allergy['Reaction Type']);
      assert.strictEqual(await getText(page, `${allergy.Allergy} Severity`), allergy.Severity);
    }

    // And the current medications section shows:
    const medications = [
      { Medication: 'Metoprolol', Dosage: '50mg', Status: 'Active' },
      { Medication: 'Aspirin', Dosage: '81mg', Status: 'Active' },
    ];
    for (const medication of medications) {
      assert.strictEqual(await getText(page, `${medication.Medication} Dosage`), medication.Dosage);
      assert.strictEqual(await getText(page, `${medication.Medication} Status`), medication.Status);
    }
  });

  test('Access patient chart during active treatment', async () => {
    // Given a patient "Michael Chen" is assigned to bed "ED-3"
    // And the patient is currently receiving active treatment
    // And recent assessments include:
    // (assumed pre-seeded test data)

    // When I open the patient's chart for "Michael Chen" in bed "ED-3"
    await page.getByTestId('open-patient-chart-button').click();

    // Then the system displays real-time information with:
    const summaryFields = [
      { Section: 'Current Status', Content: 'Active treatment in progress' },
      { Section: 'Most Recent Vitals', Content: 'BP: 130/80, HR: 88, T: 100.2°F (14:15)' },
      { Section: 'Active Orders', Content: 'Lab work in progress' },
      { Section: 'Triage Summary', Content: 'ESI 3 - Abd pain, onset 6h ago' },
    ];
    for (const { Section, Content } of summaryFields) {
      assert.strictEqual(await getText(page, Section), Content);
    }

    // And all data includes timestamps showing data freshness
    await waitForTestId(page, 'Data Freshness Timestamp');

    // And any alerts or critical values are highlighted in red
    const criticalValueElement = await waitForTestId(page, 'Critical Value Highlight');
    assert.ok(await criticalValueElement.isVisible());

    // And pending lab results show "In Progress" status with expected completion time
    const labResultStatus = await getText(page, 'Lab Result Status');
    assert.strictEqual(labResultStatus, 'In Progress');
  });

  test('View patient chart with medication allergies and interactions', async () => {
    // Given a patient "Robert Johnson" is assigned to bed "ED-12"
    // And the patient has multiple drug allergies:
    // And current medications include:
    // (assumed pre-seeded test data)

    // When I open the patient's chart for "Robert Johnson"
    await page.getByTestId('open-patient-chart-button').click();

    // Then the allergy section prominently displays:
    const allergyAlerts = [
      { 'Alert Type': 'Critical Alert', Message: 'SEVERE ALLERGIES: Morphine, NSAIDs' },
      { 'Alert Type': 'Warning', Message: 'Moderate allergy: Codeine' },
    ];
    for (const alert of allergyAlerts) {
      assert.strictEqual(await getText(page, alert['Alert Type']), alert.Message);
    }

    // And the medication section shows:
    const medications = [
      { Medication: 'Warfarin', Status: 'Active', 'Interaction Alerts': 'Monitor for bleeding risk' },
      { Medication: 'Metformin', Status: 'Active', 'Interaction Alerts': 'No interactions detected' },
    ];
    for (const medication of medications) {
      assert.strictEqual(await getText(page, `${medication.Medication} Status`), medication.Status);
      assert.strictEqual(await getText(page, `${medication.Medication} Interaction Alerts`), medication['Interaction Alerts']);
    }

    // And any new medication orders will trigger allergy checking
    await waitForTestId(page, 'Allergy Checking Notice');

    // And interaction warnings are displayed for contraindicated drugs
    await waitForTestId(page, 'Interaction Warning');
  });

  test('Access chart for pediatric patient with age-appropriate data', async () => {
    // Given a pediatric patient "Emma Foster" (age 7) is assigned to bed "ED-PEDS-2"
    // And the nursing assessment includes pediatric-specific data:
    // (assumed pre-seeded test data)

    // When I open the pediatric patient's chart for "Emma Foster"
    await page.getByTestId('open-patient-chart-button').click();

    // Then the system displays pediatric-specific information:
    const pediatricFields = [
      { Section: 'Patient Age/Weight', Content: '7 years old, 22 kg' },
      { Section: 'Pediatric Vital Ranges', Content: 'All vitals with age-appropriate norms' },
      { Section: 'Growth Percentiles', Content: 'Weight: 50th percentile' },
      { Section: 'Guardian Information', Content: 'Sarah Foster (mother) - present' },
    ];
    for (const { Section, Content } of pediatricFields) {
      assert.strictEqual(await getText(page, Section), Content);
    }

    // And vital signs are displayed with pediatric normal ranges:
    const vitalSigns = [
      { 'Vital Sign': 'Blood Pressure', Value: '95/60', Status: 'Normal' },
      { 'Vital Sign': 'Heart Rate', Value: '110', Status: 'Normal' },
      { 'Vital Sign': 'Respiratory', Value: '24', Status: 'Normal' },
      { 'Vital Sign': 'Temperature', Value: '102.8°F', Status: 'Elevated' },
    ];
    for (const vital of vitalSigns) {
      assert.strictEqual(await getText(page, vital['Vital Sign']), vital.Value);
      assert.strictEqual(await getText(page, `${vital['Vital Sign']} Status`), vital.Status);
    }

    // And medication dosing shows weight-based calculations
    await waitForTestId(page, 'Weight-Based Dosing');

    // And parental consent status is clearly indicated
    await waitForTestId(page, 'Parental Consent Status');
  });

  test('Handle incomplete nursing assessment', async () => {
    // Given a patient "David Wilson" is assigned to bed "ED-6"
    // And the nursing assessment is partially completed:
    // (assumed pre-seeded test data)

    // When I open the patient's chart for "David Wilson"
    await page.getByTestId('open-patient-chart-button').click();

    // Then the system displays available information clearly marked:
    const completedData = await getText(page, 'Completed Data');
    assert.strictEqual(completedData, 'Triage notes, initial vitals available');

    const missingDataItems = await page.getByTestId('missing-data-item').all();
    assert.strictEqual(missingDataItems.length, 3);
    const missingDataTexts = await Promise.all(missingDataItems.map(async (element) => (await element.innerText()).trim()));
    assert.deepStrictEqual(missingDataTexts, [
      'Allergies: Assessment in progress',
      'Medications: History pending',
      'Pain scale: Not yet assessed',
    ]);

    // And incomplete sections are highlighted with:
    const visualIndicators = [
      { 'Visual Indicator': 'Yellow Warning', Description: 'Assessment in progress' },
      { 'Visual Indicator': 'Refresh Timer', Description: 'Auto-refresh every 30 seconds' },
      { 'Visual Indicator': 'Notification', Description: '"Assessment updating - refresh for latest"' },
    ];
    for (const indicator of visualIndicators) {
      assert.strictEqual(await getText(page, indicator['Visual Indicator']), indicator.Description);
    }

    // And I can request priority completion of missing critical data
    await waitForTestId(page, 'Request Priority Completion Button');
  });

  test('Access chart during shift change with handoff notes', async () => {
    // Given a patient "Lisa Brown" is assigned to bed "ED-9"
    // And it is during the evening shift change (19:00)
    // And the day shift nurse added handoff notes:
    // (assumed pre-seeded test data)

    // When I open the patient's chart for "Lisa Brown"
    await page.getByTestId('open-patient-chart-button').click();

    // Then the system prominently displays shift handoff information:
    const handoffFields = [
      { 'Handoff Section': 'Clinical Summary', Content: 'Stable condition, pain controlled' },
      { 'Handoff Section': 'Pending Tasks', Content: 'Orthopedic consult ordered - pending' },
      { 'Handoff Section': 'Communication Log', Content: 'Family contact: Son updated 18:30' },
      { 'Handoff Section': 'Special Needs', Content: 'Patient preference: Female staff' },
    ];
    for (const field of handoffFields) {
      assert.strictEqual(await getText(page, field['Handoff Section']), field.Content);
    }

    // And the handoff notes are clearly timestamped
    await waitForTestId(page, 'Handoff Notes Timestamp');

    // And I can add my own physician handoff notes
    await waitForTestId(page, 'Add Physician Handoff Notes');

    // And the evening nurse can see both nursing and physician handoff information
    await waitForTestId(page, 'Combined Handoff Information');
  });

  test('Handle patient chart access during network connectivity issues', async () => {
    // Given a patient "Thomas Anderson" is assigned to bed "ED-4"
    // And the mobile app has intermittent network connectivity
    // (assumed pre-seeded test data)

    // When I attempt to open the patient's chart
    await page.getByTestId('open-patient-chart-button').click();

    // And the network connection is temporarily unavailable
    // (simulated network condition, no direct UI action)

    // Then the system displays cached patient data with:
    const cachedDataFields = [
      { 'Data Type': 'Basic Demographics', Availability: 'Available (cached)' },
      { 'Data Type': 'Last Known Vitals', Availability: 'Available - last sync 16:45' },
      { 'Data Type': 'Medication Data', Availability: 'Available (cached)' },
      { 'Data Type': 'Recent Lab Results', Availability: 'May not be current - sync pending' },
    ];
    for (const field of cachedDataFields) {
      assert.strictEqual(await getText(page, field['Data Type']), field.Availability);
    }

    // And a connectivity warning is displayed: "Limited connectivity - data may not be current"
    const connectivityWarning = await getText(page, 'Connectivity Warning');
    assert.strictEqual(connectivityWarning, 'Limited connectivity - data may not be current');

    // And the app attempts automatic sync when connection is restored
    await waitForTestId(page, 'Automatic Sync Status');

    // And critical data is prioritized for sync when connectivity returns
    await waitForTestId(page, 'Sync Priority Notice');

    // And I can manually trigger refresh when connection improves
    await waitForTestId(page, 'Manual Refresh Button');
  });

  test('Access chart with time-sensitive alerts and notifications', async () => {
    // Given a patient "Karen White" is assigned to bed "ED-7"
    // And the patient has time-sensitive clinical alerts:
    // (assumed pre-seeded test data)

    // When I open the patient's chart for "Karen White"
    await page.getByTestId('open-patient-chart-button').click();

    // Then the system prominently displays active alerts:
    const activeAlerts = [
      { 'Alert Priority': 'CRITICAL', 'Alert Details': '🔴 Troponin 0.8 - Possible MI (17:15)' },
      { 'Alert Priority': 'WARNING', 'Alert Details': '🟡 Medication due - Metoprolol (17:30)' },
      { 'Alert Priority': 'INFO', 'Alert Details': '🔵 Pain reassessment overdue (17:25)' },
    ];
    for (const alert of activeAlerts) {
      assert.strictEqual(await getText(page, alert['Alert Priority']), alert['Alert Details']);
    }

    // And critical alerts require acknowledgment before proceeding
    await waitForTestId(page, 'Alert Acknowledgment');

    // And the timestamp shows how long ago each alert was generated
    await waitForTestId(page, 'Alert Timestamp');

    // And I can take direct action on alerts (order meds, document assessment)
    await waitForTestId(page, 'Alert Action Button');

    // And alert resolution is tracked and timestamped
    await waitForTestId(page, 'Alert Resolution Tracking');
  });
});
