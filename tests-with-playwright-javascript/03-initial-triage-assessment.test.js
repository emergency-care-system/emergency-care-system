// Playwright test for spec/features/03-initial-triage-assessment.feature
// (equivalent to tests-with-selenium-javascript/03-initial-triage-assessment.test.js).
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

test.describe('Feature: Initial Triage Assessment', () => {
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
    //   And I am logged in as a triage nurse
    //   And the ESI (Emergency Severity Index) scoring module is active
    await verifySystemIsOperational(page);
    await login(page, 'a triage nurse');
    // The ESI scoring module being active is assumed pre-seeded test data /
    // environment configuration.

    const featureNavLink = await waitForTestId(page, 'Nav Initial Triage Assessment');
    await featureNavLink.click();
    await waitForTestId(page, 'Initial Triage Assessment Panel');
  });

  test('Assess patient with chest pain (ESI Level 2)', async () => {
    // Given a registered patient "John Doe" is waiting for triage
    // And the patient was registered 10 minutes ago
    // (assumed pre-seeded test data)
    // When I select the patient for triage assessment
    await page.getByTestId('select-patient-for-triage-button').click();

    // And I enter the vital signs:
    await fillFields(page, [
      { Field: 'Blood Pressure', Value: '160/95' },
      { Field: 'Heart Rate', Value: '110' },
      { Field: 'Respiratory Rate', Value: '22' },
      { Field: 'Temperature', Value: '98.6°F' },
      { Field: 'Oxygen Saturation', Value: '94%' },
    ]);
    // And I enter the chief complaint as "Chest pain and shortness of breath"
    await fillField(page, 'Chief Complaint', 'Chest pain and shortness of breath');
    // And I enter the pain scale as "8/10"
    await fillField(page, 'Pain Scale', '8/10');
    // And I document onset as "Started 2 hours ago"
    await fillField(page, 'Onset', 'Started 2 hours ago');
    // And I submit the triage assessment
    await page.getByTestId('submit-triage-assessment-form').click();

    // Then the system calculates an ESI score of "2"
    const esiScore = await getText(page, 'ESI Score');
    assert.strictEqual(esiScore, '2');
    // And the system assigns triage level "High Priority"
    const triageLevel = await getText(page, 'Triage Level');
    assert.strictEqual(triageLevel, 'High Priority');
    // And the patient is positioned at the front of the high priority queue
    const queuePosition = await getText(page, 'Queue Position');
    assert.strictEqual(queuePosition, '1');
    // And an alert is sent to the attending physician
    const physicianAlert = await getText(page, 'Physician Alert');
    assert.match(physicianAlert, /attending physician/i);
    // And the estimated wait time is updated to "Immediate"
    const estimatedWaitTime = await getText(page, 'Estimated Wait Time');
    assert.strictEqual(estimatedWaitTime, 'Immediate');
  });

  test('Assess patient with minor injury (ESI Level 4)', async () => {
    // Given a registered patient "Jane Smith" is waiting for triage
    // When I select the patient for triage assessment
    await page.getByTestId('select-patient-for-triage-button').click();

    // And I enter the vital signs:
    await fillFields(page, [
      { Field: 'Blood Pressure', Value: '120/80' },
      { Field: 'Heart Rate', Value: '75' },
      { Field: 'Respiratory Rate', Value: '16' },
      { Field: 'Temperature', Value: '98.2°F' },
      { Field: 'Oxygen Saturation', Value: '99%' },
    ]);
    // And I enter the chief complaint as "Sprained ankle from fall"
    await fillField(page, 'Chief Complaint', 'Sprained ankle from fall');
    // And I enter the pain scale as "4/10"
    await fillField(page, 'Pain Scale', '4/10');
    // And I document onset as "This morning while jogging"
    await fillField(page, 'Onset', 'This morning while jogging');
    // And I submit the triage assessment
    await page.getByTestId('submit-triage-assessment-form').click();

    // Then the system calculates an ESI score of "4"
    const esiScore = await getText(page, 'ESI Score');
    assert.strictEqual(esiScore, '4');
    // And the system assigns triage level "Less Urgent"
    const triageLevel = await getText(page, 'Triage Level');
    assert.strictEqual(triageLevel, 'Less Urgent');
    // And the patient is positioned in the less urgent queue
    const assignedQueue = await getText(page, 'Assigned Queue');
    assert.match(assignedQueue, /less urgent/i);
    // And the estimated wait time is updated to "60-90 minutes"
    const estimatedWaitTime = await getText(page, 'Estimated Wait Time');
    assert.strictEqual(estimatedWaitTime, '60-90 minutes');
    // And no immediate alerts are generated
    const physicianAlerts = await locator(page, 'Physician Alert').all();
    assert.strictEqual(physicianAlerts.length, 0);
  });

  test('Assess critical patient requiring immediate attention (ESI Level 1)', async () => {
    // Given a registered patient "Emergency Patient" is waiting for triage
    // When I select the patient for triage assessment
    await page.getByTestId('select-patient-for-triage-button').click();

    // And I enter the vital signs:
    await fillFields(page, [
      { Field: 'Blood Pressure', Value: '70/40' },
      { Field: 'Heart Rate', Value: '140' },
      { Field: 'Respiratory Rate', Value: '8' },
      { Field: 'Temperature', Value: '95.0°F' },
      { Field: 'Oxygen Saturation', Value: '85%' },
    ]);
    // And I enter the chief complaint as "Unresponsive after motor vehicle accident"
    await fillField(page, 'Chief Complaint', 'Unresponsive after motor vehicle accident');
    // And I enter the pain scale as "Unable to assess"
    await fillField(page, 'Pain Scale', 'Unable to assess');
    // And I mark the patient as "Requires immediate life-saving intervention"
    await fillField(page, 'Intervention Flag', 'Requires immediate life-saving intervention');
    // And I submit the triage assessment
    await page.getByTestId('submit-triage-assessment-form').click();

    // Then the system calculates an ESI score of "1"
    const esiScore = await getText(page, 'ESI Score');
    assert.strictEqual(esiScore, '1');
    // And the system assigns triage level "Resuscitation"
    const triageLevel = await getText(page, 'Triage Level');
    assert.strictEqual(triageLevel, 'Resuscitation');
    // And the patient is moved to the top of all queues
    const queuePosition = await getText(page, 'Queue Position');
    assert.strictEqual(queuePosition, '1');
    // And a code alert is automatically triggered
    const codeAlert = await getText(page, 'Code Alert');
    assert.match(codeAlert, /triggered/i);
    // And the trauma team is notified immediately
    const traumaTeamNotification = await getText(page, 'Trauma Team Notification');
    assert.match(traumaTeamNotification, /notified/i);
    // And the estimated wait time shows "Immediate - In Progress"
    const estimatedWaitTime = await getText(page, 'Estimated Wait Time');
    assert.strictEqual(estimatedWaitTime, 'Immediate - In Progress');
  });

  test('Assess pediatric patient with fever (ESI Level 3)', async () => {
    // Given a registered patient "Tommy Jones" (age 5) is waiting for triage
    // When I select the patient for triage assessment
    await page.getByTestId('select-patient-for-triage-button').click();

    // And I enter the vital signs using pediatric parameters:
    await fillFields(page, [
      { Field: 'Blood Pressure', Value: '95/60' },
      { Field: 'Heart Rate', Value: '120' },
      { Field: 'Respiratory Rate', Value: '24' },
      { Field: 'Temperature', Value: '103.2°F' },
      { Field: 'Oxygen Saturation', Value: '97%' },
    ]);
    // And I enter the chief complaint as "High fever and irritability"
    await fillField(page, 'Chief Complaint', 'High fever and irritability');
    // And I enter the pain scale as "6/10 (using FACES scale)"
    await fillField(page, 'Pain Scale', '6/10 (using FACES scale)');
    // And I document onset as "Fever started yesterday evening"
    await fillField(page, 'Onset', 'Fever started yesterday evening');
    // And I submit the triage assessment
    await page.getByTestId('submit-triage-assessment-form').click();

    // Then the system calculates an ESI score of "3" using pediatric criteria
    const esiScore = await getText(page, 'ESI Score');
    assert.strictEqual(esiScore, '3');
    const scoringCriteria = await getText(page, 'Scoring Criteria');
    assert.match(scoringCriteria, /pediatric/i);
    // And the system assigns triage level "Urgent"
    const triageLevel = await getText(page, 'Triage Level');
    assert.strictEqual(triageLevel, 'Urgent');
    // And the patient is positioned in the urgent pediatric queue
    const assignedQueue = await getText(page, 'Assigned Queue');
    assert.match(assignedQueue, /urgent pediatric/i);
    // And the pediatric team is notified
    const pediatricTeamNotification = await getText(page, 'Pediatric Team Notification');
    assert.match(pediatricTeamNotification, /notified/i);
    // And the estimated wait time is updated to "30-45 minutes"
    const estimatedWaitTime = await getText(page, 'Estimated Wait Time');
    assert.strictEqual(estimatedWaitTime, '30-45 minutes');
  });

  test('Handle incomplete vital signs during triage', async () => {
    // Given a registered patient "Mary Johnson" is waiting for triage
    // When I select the patient for triage assessment
    await page.getByTestId('select-patient-for-triage-button').click();

    // And I attempt to enter incomplete vital signs:
    //   | Vital Sign          | Value    |
    //   | Blood Pressure      | 130/85   |
    //   | Heart Rate          |          |
    //   | Respiratory Rate    | 18       |
    //   | Temperature         |          |
    //   | Oxygen Saturation   | 98%      |
    await fillFields(page, [
      { Field: 'Blood Pressure', Value: '130/85' },
      { Field: 'Heart Rate', Value: '' },
      { Field: 'Respiratory Rate', Value: '18' },
      { Field: 'Temperature', Value: '' },
      { Field: 'Oxygen Saturation', Value: '98%' },
    ]);
    // And I enter the chief complaint as "Headache"
    await fillField(page, 'Chief Complaint', 'Headache');
    // And I submit the triage assessment
    await page.getByTestId('submit-triage-assessment-form').click();

    // Then the system displays validation errors:
    //   | Missing Field       | Error Message                |
    //   | Heart Rate          | Heart rate is required       |
    //   | Temperature         | Temperature is required      |
    assert.strictEqual(await getText(page, 'Heart Rate Error'), 'Heart rate is required');
    assert.strictEqual(await getText(page, 'Temperature Error'), 'Temperature is required');
    // And the ESI score cannot be calculated
    const esiScoreElements = await locator(page, 'ESI Score').all();
    assert.strictEqual(esiScoreElements.length, 0);
    // And the assessment remains incomplete
    const assessmentStatus = await getText(page, 'Assessment Status');
    assert.strictEqual(assessmentStatus, 'Incomplete');
    // And I must complete all required fields before proceeding
    const triageForm = page.getByTestId('triage-assessment-form');
    assert.ok(await triageForm.isVisible());
  });

  test('Reassess patient with worsening condition', async () => {
    // Given a patient "Robert Davis" has been triaged as ESI Level 4
    // And the patient has been waiting for 90 minutes
    // When I select the patient for reassessment
    await page.getByTestId('select-patient-for-reassessment-button').click();

    // And I enter updated vital signs:
    await fillFields(page, [
      { Field: 'Blood Pressure', Value: '90/50' },
      { Field: 'Heart Rate', Value: '120' },
      { Field: 'Respiratory Rate', Value: '26' },
      { Field: 'Temperature', Value: '101.5°F' },
      { Field: 'Oxygen Saturation', Value: '92%' },
    ]);
    // And I update the chief complaint to "Worsening abdominal pain with nausea"
    await fillField(page, 'Chief Complaint', 'Worsening abdominal pain with nausea');
    // And I enter the updated pain scale as "9/10"
    await fillField(page, 'Pain Scale', '9/10');
    // And I submit the reassessment
    await page.getByTestId('submit-reassessment-form').click();

    // Then the system recalculates the ESI score to "2"
    const esiScore = await getText(page, 'ESI Score');
    assert.strictEqual(esiScore, '2');
    // And the system updates triage level to "High Priority"
    const triageLevel = await getText(page, 'Triage Level');
    assert.strictEqual(triageLevel, 'High Priority');
    // And the patient is moved to the front of the high priority queue
    const queuePosition = await getText(page, 'Queue Position');
    assert.strictEqual(queuePosition, '1');
    // And an escalation alert is sent to the charge nurse
    const escalationAlert = await getText(page, 'Escalation Alert');
    assert.match(escalationAlert, /charge nurse/i);
    // And a note is added documenting the condition change
    const conditionChangeNote = await getText(page, 'Condition Change Note');
    assert.ok(conditionChangeNote.length > 0);
  });

  test('Process multiple patients in triage queue', async () => {
    // Given multiple patients are waiting for triage:
    //   | Patient Name    | Registration Time | Status        |
    //   | Alice Brown     | 10:00 AM           | Waiting       |
    //   | Bob Wilson      | 10:15 AM           | Waiting       |
    //   | Carol Davis     | 10:30 AM           | Waiting       |
    // (assumed pre-seeded test data)
    // When I complete triage assessments for all patients:
    //   | Patient Name | ESI Score | Triage Level  |
    //   | Alice Brown  | 3         | Urgent        |
    //   | Bob Wilson   | 4         | Less Urgent   |
    //   | Carol Davis  | 2         | High Priority |
    await page.getByTestId('complete-all-triage-assessments-button').click();

    // Then the system positions patients in queue order:
    //   | Queue Position | Patient Name | Triage Level  |
    //   | 1              | Carol Davis  | High Priority |
    //   | 2              | Alice Brown  | Urgent        |
    //   | 3              | Bob Wilson   | Less Urgent   |
    const queueEntries = await page.getByTestId('triage-queue-entry').all();
    assert.strictEqual(queueEntries.length, 3);
    const firstQueueEntryText = await queueEntries[0].innerText();
    assert.match(firstQueueEntryText, /Carol Davis/);
    // And wait times are calculated based on queue position and available resources
    const waitTimeCalculationStatus = await getText(page, 'Wait Time Calculation Status');
    assert.match(waitTimeCalculationStatus, /calculated/i);
    // And the triage dashboard is updated with current queue status
    const triageDashboard = await waitForTestId(page, 'Triage Dashboard');
    assert.ok(await triageDashboard.isVisible());
  });
});
