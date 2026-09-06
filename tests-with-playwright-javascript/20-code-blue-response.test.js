// Playwright test for spec/features/20-code-blue-response.feature
// (equivalent to tests-with-selenium-javascript/20-code-blue-response.test.js).
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

test.describe('Feature: Code Blue Response', () => {
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
    //   And the code blue alert system is active
    //   And the resuscitation documentation module is enabled
    //   And all display devices are connected to the alert network
    //   And the code team roster is current and available
    await verifySystemIsOperational(page);
    await login(page, 'a code blue team leader');
    // The remaining Background steps describe pre-seeded system state
    // (alert system, documentation module, display devices, and code team
    // roster readiness) assumed to already be configured in the test
    // environment.
    const featureNavLink = await waitForTestId(page, 'Nav Code Blue Response');
    await featureNavLink.click();
    await waitForTestId(page, 'Code Blue Response Panel');
  });

  test('Activate code blue for cardiac arrest in bed 5', async () => {
    // Given a patient "Robert Martinez" is in bed "ED-5"
    // And the patient is being monitored for chest pain
    // And I am "Nurse Johnson" providing direct patient care
    // When the patient suddenly becomes unresponsive and pulseless
    // And I immediately press the code blue button at bedside
    await page.getByTestId('code-blue-button').click();

    // Then the system instantly activates the code blue alert:
    //   | Alert Component       | Activation Details                         |
    //   | Alert Timestamp       | 14:35:22 - Precise time recorded          |
    //   | Location              | ED-5 clearly identified                   |
    //   | Initiating Staff      | Nurse Johnson                             |
    //   | Patient Identity      | Robert Martinez (if available)            |
    //   | Alert Type            | Code Blue - Cardiac Arrest                |
    const alertActivationDetails = [
      { label: 'Alert Timestamp', value: '14:35:22 - Precise time recorded' },
      { label: 'Location', value: 'ED-5 clearly identified' },
      { label: 'Initiating Staff', value: 'Nurse Johnson' },
      { label: 'Patient Identity', value: 'Robert Martinez (if available)' },
      { label: 'Alert Type', value: 'Code Blue - Cardiac Arrest' },
    ];
    for (const { label, value } of alertActivationDetails) {
      assert.strictEqual(await getText(page, label), value);
    }

    // And the code team is immediately alerted through multiple channels:
    //   | Team Member           | Alert Method          | Expected Response Time |
    //   | Emergency Physician   | Overhead page, mobile | Immediate             |
    //   | Cardiologist         | Mobile alert, pager   | Within 2 minutes      |
    //   | Anesthesiologist     | Overhead page, mobile | Within 3 minutes      |
    //   | ICU Nurse            | Mobile alert, pager   | Within 2 minutes      |
    //   | Respiratory Therapist | Overhead page, mobile | Within 2 minutes      |
    //   | Pharmacist           | Mobile alert          | Within 3 minutes      |
    //   | Chaplain             | Silent alert          | Within 5 minutes      |
    const codeTeamAlerts = await page.getByTestId('code-team-alert-entry').all();
    assert.strictEqual(codeTeamAlerts.length, 7);

    // And patient location is displayed on all devices:
    //   | Display Location      | Information Shown                          |
    //   | ED Dashboard          | 🚨 CODE BLUE - BED ED-5 flashing red     |
    //   | Mobile Devices        | Push notification with location           |
    //   | Overhead Displays     | "CODE BLUE BED ED-5" prominently shown   |
    //   | Pager System          | "CODE BLUE ED-5" message                  |
    //   | Hospital Information  | Alert on all connected terminals          |
    const patientLocationDisplays = [
      { label: 'ED Dashboard', value: '🚨 CODE BLUE - BED ED-5 flashing red' },
      { label: 'Mobile Devices', value: 'Push notification with location' },
      { label: 'Overhead Displays', value: '"CODE BLUE BED ED-5" prominently shown' },
      { label: 'Pager System', value: '"CODE BLUE ED-5" message' },
      { label: 'Hospital Information', value: 'Alert on all connected terminals' },
    ];
    for (const { label, value } of patientLocationDisplays) {
      assert.strictEqual(await getText(page, label), value);
    }

    // And the resuscitation documentation template opens automatically:
    //   | Documentation Section | Template Fields                            |
    //   | Event Details         | Time, location, discoverer, initial rhythm|
    //   | Timeline Tracker      | Medication times, defibrillation, procedures|
    //   | Team Members          | Roles and arrival times                    |
    //   | Vital Signs           | Real-time monitoring integration           |
    //   | Interventions         | CPR quality, airway management, IV access |
    const documentationTemplateSections = [
      { label: 'Event Details', value: 'Time, location, discoverer, initial rhythm' },
      { label: 'Timeline Tracker', value: 'Medication times, defibrillation, procedures' },
      { label: 'Team Members', value: 'Roles and arrival times' },
      { label: 'Vital Signs', value: 'Real-time monitoring integration' },
      { label: 'Interventions', value: 'CPR quality, airway management, IV access' },
    ];
    for (const { label, value } of documentationTemplateSections) {
      assert.strictEqual(await getText(page, label), value);
    }
  });

  test('Code blue response with automatic equipment alerts', async () => {
    // Given a code blue has been activated in bed "ED-5"
    // When the code blue alert is triggered

    // Then emergency equipment alerts are automatically generated:
    //   | Equipment Type        | Alert Message                              |
    //   | Crash Cart            | Crash cart dispatch to ED-5               |
    //   | Defibrillator        | AED/Manual defibrillator to ED-5          |
    //   | Airway Equipment     | Intubation kit and ventilator to ED-5     |
    //   | Emergency Medications | Code blue medication box to ED-5          |
    //   | IV Access Supplies   | Central line kit and fluids to ED-5       |
    const equipmentAlerts = [
      { label: 'Crash Cart', value: 'Crash cart dispatch to ED-5' },
      { label: 'Defibrillator', value: 'AED/Manual defibrillator to ED-5' },
      { label: 'Airway Equipment', value: 'Intubation kit and ventilator to ED-5' },
      { label: 'Emergency Medications', value: 'Code blue medication box to ED-5' },
      { label: 'IV Access Supplies', value: 'Central line kit and fluids to ED-5' },
    ];
    for (const { label, value } of equipmentAlerts) {
      assert.strictEqual(await getText(page, label), value);
    }

    // And equipment tracking is initiated:
    //   | Equipment Item        | Status Tracking                            |
    //   | Crash Cart Location   | GPS tracking to bed ED-5                  |
    //   | Defibrillator Readiness| Battery level and functionality check    |
    //   | Medication Expiration | Code blue drugs expiration verification   |
    //   | Equipment Arrival     | Timestamp when equipment reaches bedside  |
    const equipmentTracking = [
      { label: 'Crash Cart Location', value: 'GPS tracking to bed ED-5' },
      { label: 'Defibrillator Readiness', value: 'Battery level and functionality check' },
      { label: 'Medication Expiration', value: 'Code blue drugs expiration verification' },
      { label: 'Equipment Arrival', value: 'Timestamp when equipment reaches bedside' },
    ];
    for (const { label, value } of equipmentTracking) {
      assert.strictEqual(await getText(page, label), value);
    }

    // And backup equipment is automatically prepared:
    //   | Backup Equipment      | Preparation Action                         |
    //   | Secondary Crash Cart  | Made ready for potential second code       |
    //   | Additional Ventilator | Checked and moved closer to ED            |
    //   | Blood Bank Alert      | Emergency blood products prepared          |
    //   | OR Notification       | Operating room placed on standby          |
    const backupEquipment = [
      { label: 'Secondary Crash Cart', value: 'Made ready for potential second code' },
      { label: 'Additional Ventilator', value: 'Checked and moved closer to ED' },
      { label: 'Blood Bank Alert', value: 'Emergency blood products prepared' },
      { label: 'OR Notification', value: 'Operating room placed on standby' },
    ];
    for (const { label, value } of backupEquipment) {
      assert.strictEqual(await getText(page, label), value);
    }
  });

  test('Real-time code blue documentation during resuscitation', async () => {
    // Given a code blue is in progress in bed "ED-5"
    // And the resuscitation documentation template is open
    // And "Dr. Smith" is the code team leader
    // When resuscitation interventions are performed

    // Then real-time documentation captures all activities:
    //   | Intervention Type     | Documentation Fields                       |
    //   | CPR Administration    | Start time, compression quality, provider  |
    //   | Medication Given      | Drug name, dose, route, time, provider     |
    //   | Defibrillation       | Joules delivered, rhythm before/after     |
    //   | Airway Management    | Type of airway, success, provider         |
    //   | IV Access            | Location, size, number of attempts        |
    const documentedActivities = [
      { label: 'CPR Administration', value: 'Start time, compression quality, provider' },
      { label: 'Medication Given', value: 'Drug name, dose, route, time, provider' },
      { label: 'Defibrillation', value: 'Joules delivered, rhythm before/after' },
      { label: 'Airway Management', value: 'Type of airway, success, provider' },
      { label: 'IV Access', value: 'Location, size, number of attempts' },
    ];
    for (const { label, value } of documentedActivities) {
      assert.strictEqual(await getText(page, label), value);
    }

    // And timeline tracking maintains precise chronology:
    //   | Timeline Entry        | Automatic Capture                          |
    //   | Event Start           | 14:35:22 - Code blue activated            |
    //   | CPR Initiated         | 14:35:45 - CPR started by Nurse Johnson   |
    //   | Team Leader Arrival   | 14:36:15 - Dr. Smith assumes leadership   |
    //   | First Medication      | 14:37:30 - Epinephrine 1mg IV push       |
    //   | Defibrillation       | 14:38:45 - 200J biphasic shock delivered  |
    const timelineEntries = [
      { label: 'Event Start', value: '14:35:22 - Code blue activated' },
      { label: 'CPR Initiated', value: '14:35:45 - CPR started by Nurse Johnson' },
      { label: 'Team Leader Arrival', value: '14:36:15 - Dr. Smith assumes leadership' },
      { label: 'First Medication', value: '14:37:30 - Epinephrine 1mg IV push' },
      { label: 'Defibrillation Timeline', value: '14:38:45 - 200J biphasic shock delivered' },
    ];
    for (const { label, value } of timelineEntries) {
      assert.strictEqual(await getText(page, label), value);
    }

    // And quality metrics are tracked in real-time:
    //   | Quality Metric        | Real-time Monitoring                       |
    //   | Compression Depth     | CPR feedback device integration           |
    //   | Compression Rate      | Metronome guidance and measurement        |
    //   | No-flow Time         | Automatic calculation of interruptions    |
    //   | Medication Timing     | Alert for time-critical drug intervals   |
    const qualityMetrics = [
      { label: 'Compression Depth', value: 'CPR feedback device integration' },
      { label: 'Compression Rate', value: 'Metronome guidance and measurement' },
      { label: 'No-flow Time', value: 'Automatic calculation of interruptions' },
      { label: 'Medication Timing', value: 'Alert for time-critical drug intervals' },
    ];
    for (const { label, value } of qualityMetrics) {
      assert.strictEqual(await getText(page, label), value);
    }
  });

  test('Code blue with return of spontaneous circulation (ROSC)', async () => {
    // Given a code blue has been in progress for 8 minutes
    // And resuscitation efforts are ongoing with documentation active
    // When the patient achieves return of spontaneous circulation (ROSC)
    // And "Dr. Smith" confirms pulse and blood pressure of 110/70

    // Then the system updates the code status:
    //   | Status Update         | Documentation Changes                      |
    //   | ROSC Achievement      | Time: 14:43:15 - ROSC achieved            |
    //   | Vital Signs          | BP: 110/70, HR: 85, documented           |
    //   | Rhythm Change        | Normal sinus rhythm confirmed             |
    //   | Intervention Pause   | CPR discontinued, monitoring intensified  |
    const codeStatusUpdates = [
      { label: 'ROSC Achievement', value: 'Time: 14:43:15 - ROSC achieved' },
      { label: 'Post-ROSC Vital Signs', value: 'BP: 110/70, HR: 85, documented' },
      { label: 'Rhythm Change', value: 'Normal sinus rhythm confirmed' },
      { label: 'Intervention Pause', value: 'CPR discontinued, monitoring intensified' },
    ];
    for (const { label, value } of codeStatusUpdates) {
      assert.strictEqual(await getText(page, label), value);
    }

    // And post-ROSC care protocols are activated:
    //   | Post-ROSC Protocol    | Automated Alerts                           |
    //   | ICU Transfer          | ICU bed request and transport coordination |
    //   | Cardiology Consult    | Urgent cardiology evaluation requested     |
    //   | Temperature Management| Therapeutic hypothermia consideration      |
    //   | Neurological Assessment| Baseline neuro checks ordered             |
    const postRoscProtocols = [
      { label: 'ICU Transfer', value: 'ICU bed request and transport coordination' },
      { label: 'Cardiology Consult', value: 'Urgent cardiology evaluation requested' },
      { label: 'Temperature Management', value: 'Therapeutic hypothermia consideration' },
      { label: 'Neurological Assessment', value: 'Baseline neuro checks ordered' },
    ];
    for (const { label, value } of postRoscProtocols) {
      assert.strictEqual(await getText(page, label), value);
    }

    // And family notification procedures are initiated:
    //   | Family Communication  | Process                                    |
    //   | Contact Attempt       | Emergency contact called by social worker  |
    //   | Status Update         | "Patient being treated, stable condition" |
    //   | Visitation Arrangement| Family arrival and bedside visit coordination|
    //   | Chaplain Services     | Spiritual care offered to family          |
    const familyNotificationProcedures = [
      { label: 'Contact Attempt', value: 'Emergency contact called by social worker' },
      { label: 'Status Update', value: '"Patient being treated, stable condition"' },
      { label: 'Visitation Arrangement', value: 'Family arrival and bedside visit coordination' },
      { label: 'Chaplain Services', value: 'Spiritual care offered to family' },
    ];
    for (const { label, value } of familyNotificationProcedures) {
      assert.strictEqual(await getText(page, label), value);
    }
  });

  test('Unsuccessful code blue with transition to end-of-life care', async () => {
    // Given a code blue has been in progress for 25 minutes
    // And multiple rounds of medications and defibrillation have been attempted
    // And no return of spontaneous circulation has been achieved
    // When "Dr. Smith" as code team leader determines resuscitation efforts should cease
    // And the time of death is called at 15:00:15

    // Then the system handles end-of-life documentation:
    //   | End-of-Life Process   | Documentation Requirements                 |
    //   | Time of Death         | 15:00:15 - Officially recorded           |
    //   | Resuscitation Duration| 24 minutes 53 seconds total time         |
    //   | Interventions Summary | Complete list of all attempted treatments |
    //   | Team Members Present  | All providers involved in resuscitation   |
    const endOfLifeDocumentation = [
      { label: 'Time of Death', value: '15:00:15 - Officially recorded' },
      { label: 'Resuscitation Duration', value: '24 minutes 53 seconds total time' },
      { label: 'Interventions Summary', value: 'Complete list of all attempted treatments' },
      { label: 'Team Members Present', value: 'All providers involved in resuscitation' },
    ];
    for (const { label, value } of endOfLifeDocumentation) {
      assert.strictEqual(await getText(page, label), value);
    }

    // And family notification and support procedures are activated:
    //   | Family Support        | Coordinated Response                       |
    //   | Immediate Contact     | Personal notification by physician        |
    //   | Bereavement Support   | Chaplain and social worker assigned       |
    //   | Viewing Arrangement   | Private room prepared for family viewing   |
    //   | Organ Donation        | Coordinator contacted per protocol        |
    const familySupportProcedures = [
      { label: 'Immediate Contact', value: 'Personal notification by physician' },
      { label: 'Bereavement Support', value: 'Chaplain and social worker assigned' },
      { label: 'Viewing Arrangement', value: 'Private room prepared for family viewing' },
      { label: 'Organ Donation', value: 'Coordinator contacted per protocol' },
    ];
    for (const { label, value } of familySupportProcedures) {
      assert.strictEqual(await getText(page, label), value);
    }

    // And administrative processes are initiated:
    //   | Administrative Task   | Required Actions                           |
    //   | Medical Examiner      | Contact if death meets criteria           |
    //   | Autopsy Consent       | Family discussion and documentation       |
    //   | Death Certificate     | Physician completion requirements         |
    //   | Quality Review        | Case review scheduled within 24 hours     |
    const administrativeProcesses = [
      { label: 'Medical Examiner', value: 'Contact if death meets criteria' },
      { label: 'Autopsy Consent', value: 'Family discussion and documentation' },
      { label: 'Death Certificate', value: 'Physician completion requirements' },
      { label: 'Quality Review', value: 'Case review scheduled within 24 hours' },
    ];
    for (const { label, value } of administrativeProcesses) {
      assert.strictEqual(await getText(page, label), value);
    }
  });

  test('Code blue during visitor hours with family present', async () => {
    // Given it is 19:30 during evening visitor hours
    // And the patient's family members are at bedside when cardiac arrest occurs
    // When the code blue is activated

    // Then family management protocols are immediately implemented:
    //   | Family Management     | Immediate Actions                          |
    //   | Family Escort         | Security escorts family to private area    |
    //   | Communication         | Social worker provides immediate support   |
    //   | Information Updates   | Regular updates provided during resuscitation|
    //   | Chaplain Services     | Spiritual care offered immediately         |
    const familyManagementProtocols = [
      { label: 'Family Escort', value: 'Security escorts family to private area' },
      { label: 'Communication', value: 'Social worker provides immediate support' },
      { label: 'Information Updates', value: 'Regular updates provided during resuscitation' },
      { label: 'Chaplain Services Immediate Support', value: 'Spiritual care offered immediately' },
    ];
    for (const { label, value } of familyManagementProtocols) {
      assert.strictEqual(await getText(page, label), value);
    }

    // And visitor area management is coordinated:
    //   | Visitor Control       | Safety Measures                            |
    //   | Area Clearance        | Non-family visitors moved from immediate area|
    //   | Privacy Protection    | Screens and barriers deployed             |
    //   | Crowd Control         | Security manages visitor flow             |
    //   | Other Patient Care    | Continued care for nearby patients        |
    const visitorAreaManagement = [
      { label: 'Area Clearance', value: 'Non-family visitors moved from immediate area' },
      { label: 'Privacy Protection', value: 'Screens and barriers deployed' },
      { label: 'Crowd Control', value: 'Security manages visitor flow' },
      { label: 'Other Patient Care', value: 'Continued care for nearby patients' },
    ];
    for (const { label, value } of visitorAreaManagement) {
      assert.strictEqual(await getText(page, label), value);
    }

    // And family preference accommodation occurs:
    //   | Family Preference     | Options Provided                           |
    //   | Bedside Presence      | Option to remain during resuscitation     |
    //   | Waiting Area          | Comfortable private space with updates    |
    //   | Family Spokesperson   | Designated family member for communication |
    //   | Support Person        | Additional family/friend notification     |
    const familyPreferenceAccommodation = [
      { label: 'Bedside Presence', value: 'Option to remain during resuscitation' },
      { label: 'Waiting Area', value: 'Comfortable private space with updates' },
      { label: 'Family Spokesperson', value: 'Designated family member for communication' },
      { label: 'Support Person', value: 'Additional family/friend notification' },
    ];
    for (const { label, value } of familyPreferenceAccommodation) {
      assert.strictEqual(await getText(page, label), value);
    }
  });

  test('Code blue team performance metrics and quality improvement', async () => {
    // Given a code blue event has been completed
    // And all documentation has been finalized
    // When the quality review process is initiated

    // Then performance metrics are automatically calculated:
    //   | Performance Metric    | Measurement                                |
    //   | Response Time         | 1 minute 23 seconds from alert to arrival |
    //   | No-flow Time          | 15 seconds total interruption time        |
    //   | First Shock Time      | 3 minutes 45 seconds from arrest          |
    //   | Medication Timing     | All drugs given within target windows     |
    //   | Team Coordination     | Communication effectiveness score          |
    const performanceMetrics = [
      { label: 'Response Time', value: '1 minute 23 seconds from alert to arrival' },
      { label: 'No-flow Time Performance', value: '15 seconds total interruption time' },
      { label: 'First Shock Time', value: '3 minutes 45 seconds from arrest' },
      { label: 'Medication Timing Compliance', value: 'All drugs given within target windows' },
      { label: 'Team Coordination', value: 'Communication effectiveness score' },
    ];
    for (const { label, value } of performanceMetrics) {
      assert.strictEqual(await getText(page, label), value);
    }

    // And quality improvement data is captured:
    //   | QI Data Element       | Assessment                                 |
    //   | Protocol Adherence    | 95% compliance with ACLS guidelines       |
    //   | Equipment Function    | All equipment functioned properly         |
    //   | Team Performance      | Effective leadership and role clarity     |
    //   | Communication Quality | Clear, concise, and timely communication  |
    const qiDataElements = [
      { label: 'Protocol Adherence', value: '95% compliance with ACLS guidelines' },
      { label: 'Equipment Function', value: 'All equipment functioned properly' },
      { label: 'Team Performance', value: 'Effective leadership and role clarity' },
      { label: 'Communication Quality', value: 'Clear, concise, and timely communication' },
    ];
    for (const { label, value } of qiDataElements) {
      assert.strictEqual(await getText(page, label), value);
    }

    // And improvement opportunities are identified:
    //   | Improvement Area      | Recommendation                             |
    //   | Response Time         | Consider additional code cart placement    |
    //   | Team Training         | Schedule quarterly simulation training     |
    //   | Equipment Maintenance | Review defibrillator calibration schedule |
    //   | Documentation         | Streamline real-time entry process        |
    const improvementOpportunities = [
      { label: 'Response Time Recommendation', value: 'Consider additional code cart placement' },
      { label: 'Team Training', value: 'Schedule quarterly simulation training' },
      { label: 'Equipment Maintenance', value: 'Review defibrillator calibration schedule' },
      { label: 'Documentation', value: 'Streamline real-time entry process' },
    ];
    for (const { label, value } of improvementOpportunities) {
      assert.strictEqual(await getText(page, label), value);
    }
  });

  test('Code blue false alarm with appropriate system response', async () => {
    // Given a code blue alert has been activated in bed "ED-5"
    // And the code team is responding
    // When it is determined that the patient is conscious and stable
    // And the alert was triggered accidentally by equipment malfunction

    // Then the false alarm protocol is activated:
    //   | False Alarm Response  | Actions Taken                              |
    //   | Alert Cancellation    | "Code blue canceled - false alarm" announcement|
    //   | Team Stand-down       | Code team notified to return to normal duties|
    //   | Equipment Check       | Investigate and repair malfunctioning equipment|
    //   | Documentation         | Document false alarm and cause            |
    const falseAlarmProtocol = [
      { label: 'Alert Cancellation', value: '"Code blue canceled - false alarm" announcement' },
      { label: 'Team Stand-down', value: 'Code team notified to return to normal duties' },
      { label: 'Equipment Check', value: 'Investigate and repair malfunctioning equipment' },
      { label: 'False Alarm Documentation', value: 'Document false alarm and cause' },
    ];
    for (const { label, value } of falseAlarmProtocol) {
      assert.strictEqual(await getText(page, label), value);
    }

    // And system improvements are implemented:
    //   | Improvement Action    | Preventive Measures                        |
    //   | Equipment Maintenance | Immediate repair of faulty equipment       |
    //   | Staff Education       | Review proper code blue activation         |
    //   | System Calibration    | Adjust sensitivity to prevent false alarms |
    //   | Audit Trail          | Record incident for system improvement     |
    const systemImprovements = [
      { label: 'Equipment Maintenance Repair', value: 'Immediate repair of faulty equipment' },
      { label: 'Staff Education', value: 'Review proper code blue activation' },
      { label: 'System Calibration', value: 'Adjust sensitivity to prevent false alarms' },
      { label: 'Audit Trail', value: 'Record incident for system improvement' },
    ];
    for (const { label, value } of systemImprovements) {
      assert.strictEqual(await getText(page, label), value);
    }

    // And normal operations resume with lessons learned integrated into protocols
    assert.match(await getText(page, 'System Operations Status'), /normal/i);
  });
});
