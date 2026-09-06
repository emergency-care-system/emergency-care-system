// Playwright test for spec/features/04-triage-re-assessment.feature
// (equivalent to tests-with-selenium-javascript/04-triage-re-assessment.test.js).
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

test.describe('Feature: Triage Re-assessment', () => {
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
    //   And I am logged in as a triage nurse
    //   And the automatic reassessment alerts are enabled
    await verifySystemIsOperational(page);
    await login(page, 'a triage nurse');
    // The automatic reassessment alerts being enabled is assumed pre-seeded
    // test data / environment configuration.

    const featureNavLink = await waitForTestId(page, 'Nav Triage Re-assessment');
    await featureNavLink.click();
    await waitForTestId(page, 'Triage Re-assessment Panel');
  });

  test('Reassess patient with worsening condition after 2 hours', async () => {
    // Given a patient "Sarah Johnson" has been waiting in the queue for 2 hours
    // And the patient's initial triage was ESI Level 4 (Less Urgent)
    // And the patient's initial vital signs were:
    //   | Vital Sign          | Initial Value |
    //   | Blood Pressure      | 125/78        |
    //   | Heart Rate          | 82            |
    //   | Respiratory Rate    | 16            |
    //   | Temperature         | 99.1°F        |
    //   | Oxygen Saturation   | 98%           |
    //   | Pain Scale          | 3/10          |
    // (assumed pre-seeded test data)
    // When the system triggers a reassessment alert at the 2-hour mark
    await waitForTestId(page, 'Reassessment Alert');
    // And I select the patient for reassessment
    await page.getByTestId('select-patient-for-reassessment-button').click();

    // And I enter the updated vital signs:
    await fillFields(page, [
      { Field: 'Blood Pressure', Value: '95/55' },
      { Field: 'Heart Rate', Value: '115' },
      { Field: 'Respiratory Rate', Value: '24' },
      { Field: 'Temperature', Value: '101.8°F' },
      { Field: 'Oxygen Saturation', Value: '94%' },
      { Field: 'Pain Scale', Value: '8/10' },
    ]);
    // And I update the chief complaint to "Severe abdominal pain with dizziness"
    await fillField(page, 'Chief Complaint', 'Severe abdominal pain with dizziness');
    // And I submit the reassessment
    await page.getByTestId('submit-reassessment-form').click();

    // Then the system recalculates the ESI score from "4" to "2"
    const esiScore = await getText(page, 'ESI Score');
    assert.strictEqual(esiScore, '2');
    // And the system updates the triage level from "Less Urgent" to "High Priority"
    const triageLevel = await getText(page, 'Triage Level');
    assert.strictEqual(triageLevel, 'High Priority');
    // And the patient is moved from position 12 to position 2 in the queue
    const queuePosition = await getText(page, 'Queue Position');
    assert.strictEqual(queuePosition, '2');
    // And an escalation alert is sent to the charge nurse
    const escalationAlert = await getText(page, 'Escalation Alert');
    assert.match(escalationAlert, /charge nurse/i);
    // And the estimated wait time is updated from "90 minutes" to "15 minutes"
    const estimatedWaitTime = await getText(page, 'Estimated Wait Time');
    assert.strictEqual(estimatedWaitTime, '15 minutes');
    // And a reassessment note is automatically added to the patient record
    const reassessmentNote = await getText(page, 'Reassessment Note');
    assert.ok(reassessmentNote.length > 0);
  });

  test('Reassess patient with stable condition', async () => {
    // Given a patient "Michael Chen" has been waiting in the queue for 2 hours
    // And the patient's initial triage was ESI Level 3 (Urgent)
    // And the patient's initial vital signs were:
    //   | Vital Sign          | Initial Value |
    //   | Blood Pressure      | 140/90        |
    //   | Heart Rate          | 95            |
    //   | Respiratory Rate    | 20            |
    //   | Temperature         | 100.2°F       |
    //   | Oxygen Saturation   | 96%           |
    //   | Pain Scale          | 6/10          |
    // (assumed pre-seeded test data)
    // When I perform a scheduled reassessment
    await page.getByTestId('select-patient-for-reassessment-button').click();

    // And I enter the updated vital signs:
    await fillFields(page, [
      { Field: 'Blood Pressure', Value: '135/85' },
      { Field: 'Heart Rate', Value: '88' },
      { Field: 'Respiratory Rate', Value: '18' },
      { Field: 'Temperature', Value: '99.8°F' },
      { Field: 'Oxygen Saturation', Value: '97%' },
      { Field: 'Pain Scale', Value: '5/10' },
    ]);
    // And I note "Patient reports feeling slightly better"
    await fillField(page, 'Reassessment Note', 'Patient reports feeling slightly better');
    // And I submit the reassessment
    await page.getByTestId('submit-reassessment-form').click();

    // Then the system recalculates and maintains ESI score of "3"
    const esiScore = await getText(page, 'ESI Score');
    assert.strictEqual(esiScore, '3');
    // And the triage level remains "Urgent"
    const triageLevel = await getText(page, 'Triage Level');
    assert.strictEqual(triageLevel, 'Urgent');
    // And the patient's queue position remains unchanged
    const queuePosition = await getText(page, 'Queue Position');
    assert.ok(queuePosition.length > 0);
    // And no escalation alerts are generated
    const escalationAlerts = await locator(page, 'Escalation Alert').all();
    assert.strictEqual(escalationAlerts.length, 0);
    // And a reassessment note is added documenting stable condition
    const reassessmentNote = await getText(page, 'Reassessment Note');
    assert.strictEqual(reassessmentNote, 'Patient reports feeling slightly better');
    // And the next reassessment is scheduled for 1 hour
    const nextReassessmentSchedule = await getText(page, 'Next Reassessment Schedule');
    assert.strictEqual(nextReassessmentSchedule, '1 hour');
  });

  test('Reassess patient with improving condition', async () => {
    // Given a patient "Lisa Rodriguez" has been waiting in the queue for 2 hours
    // And the patient's initial triage was ESI Level 2 (High Priority)
    // And the patient's initial vital signs were:
    //   | Vital Sign          | Initial Value |
    //   | Blood Pressure      | 170/100       |
    //   | Heart Rate          | 120           |
    //   | Respiratory Rate    | 28            |
    //   | Temperature         | 98.9°F        |
    //   | Oxygen Saturation   | 92%           |
    //   | Pain Scale          | 9/10          |
    // (assumed pre-seeded test data)
    // When I perform a reassessment
    await page.getByTestId('select-patient-for-reassessment-button').click();

    // And I enter the updated vital signs:
    await fillFields(page, [
      { Field: 'Blood Pressure', Value: '145/85' },
      { Field: 'Heart Rate', Value: '95' },
      { Field: 'Respiratory Rate', Value: '20' },
      { Field: 'Temperature', Value: '98.6°F' },
      { Field: 'Oxygen Saturation', Value: '96%' },
      { Field: 'Pain Scale', Value: '4/10' },
    ]);
    // And I note "Patient reports significant improvement after medication"
    await fillField(page, 'Reassessment Note', 'Patient reports significant improvement after medication');
    // And I submit the reassessment
    await page.getByTestId('submit-reassessment-form').click();

    // Then the system recalculates the ESI score from "2" to "3"
    const esiScore = await getText(page, 'ESI Score');
    assert.strictEqual(esiScore, '3');
    // And the system updates the triage level from "High Priority" to "Urgent"
    const triageLevel = await getText(page, 'Triage Level');
    assert.strictEqual(triageLevel, 'Urgent');
    // And the patient is moved from position 1 to position 5 in the queue
    const queuePosition = await getText(page, 'Queue Position');
    assert.strictEqual(queuePosition, '5');
    // And the charge nurse is notified of the priority change
    const chargeNurseNotification = await getText(page, 'Charge Nurse Notification');
    assert.match(chargeNurseNotification, /priority change/i);
    // And the estimated wait time is updated from "Immediate" to "45 minutes"
    const estimatedWaitTime = await getText(page, 'Estimated Wait Time');
    assert.strictEqual(estimatedWaitTime, '45 minutes');
    // And higher priority patients are moved up in the queue
    const queueEntries = await page.getByTestId('triage-queue-entry').all();
    assert.ok(queueEntries.length > 0);
  });

  test('Automatic reassessment alert triggers', async () => {
    // Given multiple patients have been waiting for extended periods:
    //   | Patient Name     | Wait Time | Current ESI | Due for Reassessment |
    //   | John Williams    | 2 hours   | 4           | Yes                  |
    //   | Emma Thompson    | 1.5 hours | 3           | No                   |
    //   | David Kim        | 3 hours   | 3           | Yes                  |
    // (assumed pre-seeded test data)
    // When the system performs its hourly reassessment check
    await page.getByTestId('trigger-hourly-reassessment-check-button').click();

    // Then reassessment alerts are generated for:
    //   | Patient Name  | Alert Type           | Reason                    |
    //   | John Williams | Standard Reassess    | 2 hours ESI Level 4       |
    //   | David Kim     | Urgent Reassess      | 3 hours ESI Level 3       |
    const reassessmentAlerts = await page.getByTestId('reassessment-alert').all();
    assert.strictEqual(reassessmentAlerts.length, 2);
    // And the alerts appear on the triage nurse dashboard
    const triageNurseDashboard = await waitForTestId(page, 'Triage Nurse Dashboard');
    assert.ok(await triageNurseDashboard.isVisible());
    // And the patients are flagged with "Reassessment Due" status
    const reassessmentDueFlags = await page.getByTestId('reassessment-due-flag').all();
    assert.strictEqual(reassessmentDueFlags.length, 2);
    // And Emma Thompson does not receive an alert
    const alertTexts = await Promise.all(reassessmentAlerts.map((element) => element.innerText()));
    assert.ok(!alertTexts.some((text) => text.includes('Emma Thompson')));
  });

  test('Handle patient who becomes critical during reassessment', async () => {
    // Given a patient "Robert Martinez" has been waiting in the queue for 2 hours
    // And the patient's initial triage was ESI Level 3 (Urgent)
    // When I begin the reassessment process
    await page.getByTestId('select-patient-for-reassessment-button').click();
    // And I observe the patient is now unresponsive

    // And I enter critical vital signs:
    await fillFields(page, [
      { Field: 'Blood Pressure', Value: '60/30' },
      { Field: 'Heart Rate', Value: '150' },
      { Field: 'Respiratory Rate', Value: '6' },
      { Field: 'Temperature', Value: '96.2°F' },
      { Field: 'Oxygen Saturation', Value: '80%' },
      { Field: 'Consciousness', Value: 'Unresponsive' },
    ]);
    // And I submit the emergency reassessment
    await page.getByTestId('submit-reassessment-form').click();

    // Then the system immediately calculates ESI score as "1"
    const esiScore = await getText(page, 'ESI Score');
    assert.strictEqual(esiScore, '1');
    // And the system updates triage level to "Resuscitation"
    const triageLevel = await getText(page, 'Triage Level');
    assert.strictEqual(triageLevel, 'Resuscitation');
    // And the patient is moved to the top of all queues
    const queuePosition = await getText(page, 'Queue Position');
    assert.strictEqual(queuePosition, '1');
    // And a code blue alert is automatically triggered
    const codeBlueAlert = await getText(page, 'Code Blue Alert');
    assert.match(codeBlueAlert, /triggered/i);
    // And the rapid response team is notified immediately
    const rapidResponseNotification = await getText(page, 'Rapid Response Notification');
    assert.match(rapidResponseNotification, /notified/i);
    // And the patient is flagged for immediate intervention
    const interventionFlag = await getText(page, 'Intervention Flag');
    assert.match(interventionFlag, /immediate intervention/i);
    // And I am prompted to initiate emergency protocols
    await waitForTestId(page, 'Emergency Protocol Prompt');
  });

  test('Reassess pediatric patient with different parameters', async () => {
    // Given a pediatric patient "Amy Foster" (age 8) has been waiting for 2 hours
    // And the patient's initial triage was ESI Level 3 (Urgent)
    // When I perform a pediatric reassessment
    await page.getByTestId('select-patient-for-reassessment-button').click();

    // And I enter updated vital signs using age-appropriate parameters:
    await fillFields(page, [
      { Field: 'Blood Pressure', Value: '85/50' },
      { Field: 'Heart Rate', Value: '140' },
      { Field: 'Respiratory Rate', Value: '32' },
      { Field: 'Temperature', Value: '103.8°F' },
      { Field: 'Oxygen Saturation', Value: '93%' },
      { Field: 'Pain Scale (FACES)', Value: '8/10' },
    ]);
    // And I note "Child appears more lethargic than initial assessment"
    await fillField(page, 'Reassessment Note', 'Child appears more lethargic than initial assessment');
    // And I submit the pediatric reassessment
    await page.getByTestId('submit-reassessment-form').click();

    // Then the system recalculates using pediatric ESI criteria
    const scoringCriteria = await getText(page, 'Scoring Criteria');
    assert.match(scoringCriteria, /pediatric/i);
    // And the ESI score is updated from "3" to "2"
    const esiScore = await getText(page, 'ESI Score');
    assert.strictEqual(esiScore, '2');
    // And the triage level is updated to "High Priority"
    const triageLevel = await getText(page, 'Triage Level');
    assert.strictEqual(triageLevel, 'High Priority');
    // And the pediatric emergency team is notified
    const pediatricTeamNotification = await getText(page, 'Pediatric Team Notification');
    assert.match(pediatricTeamNotification, /notified/i);
    // And the patient is moved to the pediatric high priority queue
    const assignedQueue = await getText(page, 'Assigned Queue');
    assert.match(assignedQueue, /pediatric high priority/i);
    // And parent/guardian notification protocols are initiated
    const guardianNotification = await getText(page, 'Guardian Notification');
    assert.match(guardianNotification, /initiated/i);
  });

  test('Document reassessment with no vital sign changes', async () => {
    // Given a patient "Catherine Lee" has been waiting for 2 hours
    // And a reassessment is due
    // When I perform the reassessment
    await page.getByTestId('select-patient-for-reassessment-button').click();
    // And the vital signs remain identical to the initial assessment

    // But I note "Patient reports increased anxiety about wait time"
    await fillField(page, 'Reassessment Note', 'Patient reports increased anxiety about wait time');
    // And I provide reassurance and update on expected wait time
    // And I submit the reassessment
    await page.getByTestId('submit-reassessment-form').click();

    // Then the ESI score and triage level remain unchanged
    const esiScore = await getText(page, 'ESI Score');
    assert.ok(esiScore.length > 0);
    const triageLevel = await getText(page, 'Triage Level');
    assert.ok(triageLevel.length > 0);
    // And the queue position is maintained
    const queuePosition = await getText(page, 'Queue Position');
    assert.ok(queuePosition.length > 0);
    // And a documentation note is added about patient anxiety
    const reassessmentNote = await getText(page, 'Reassessment Note');
    assert.match(reassessmentNote, /anxiety/i);
    // And comfort measures are suggested in the patient instructions
    const patientInstructions = await getText(page, 'Patient Instructions');
    assert.match(patientInstructions, /comfort/i);
    // And the next reassessment interval is maintained
    const nextReassessmentSchedule = await getText(page, 'Next Reassessment Schedule');
    assert.ok(nextReassessmentSchedule.length > 0);
  });

  test('Handle reassessment during shift change', async () => {
    // Given a patient "Thomas Wilson" is due for reassessment
    // And the day shift triage nurse is preparing to leave
    // And the night shift triage nurse is arriving
    // When the day shift nurse initiates the reassessment handoff
    await page.getByTestId('initiate-reassessment-handoff-button').click();
    // And transfers the patient assessment to the night shift nurse
    await page.getByTestId('transfer-assessment-button').click();

    // Then the reassessment timing is preserved
    const reassessmentTiming = await getText(page, 'Reassessment Timing');
    assert.ok(reassessmentTiming.length > 0);
    // And all previous assessment data remains accessible
    const previousAssessmentData = await waitForTestId(page, 'Previous Assessment Data');
    assert.ok(await previousAssessmentData.isVisible());
    // And the night shift nurse can complete the reassessment
    const reassessmentForm = page.getByTestId('reassessment-form');
    assert.ok(await reassessmentForm.isVisible());
    // And continuity of care documentation is maintained
    const continuityDocumentation = await getText(page, 'Continuity Documentation');
    assert.ok(continuityDocumentation.length > 0);
    // And the handoff is logged in the system audit trail
    const auditTrailEntries = await page.getByTestId('audit-trail-entry').all();
    assert.ok(auditTrailEntries.length > 0);
  });
});
