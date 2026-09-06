// Selenium WebDriver + Mocha test for
// spec/features/14-provider-assignment.feature
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see support/fields.js) and the shared
// data-testid contract in support/login.js (login-identity, login-submit,
// app-root).

import { strict as assert } from 'assert';
import { type WebDriver } from 'selenium-webdriver';
import { buildDriver } from './support/build-driver.js';
import { login, verifySystemIsOperational } from './support/login.js';
import { getText, waitForTestId } from './support/fields.js';

describe('Feature: Provider Assignment', function () {
  this.timeout(20000);
  let driver: WebDriver;

  before(async () => {
    driver = await buildDriver();
  });

  after(async () => {
    await driver.quit();
  });

  beforeEach(async () => {
    // Background:
    //   Given the emergency care system is operational
    //   And the provider assignment module is active
    //   And provider workload tracking is enabled
    //   And mobile notification system is functional
    await verifySystemIsOperational(driver);
    await login(driver, 'a charge nurse');
    // The provider assignment module, provider workload tracking, and the
    // mobile notification system are assumed to be active/enabled backend
    // configuration already in place for this environment.

    const providerAssignmentNavLink = await waitForTestId(driver, 'Nav Provider Assignment');
    await providerAssignmentNavLink.click();
    await waitForTestId(driver, 'Provider Assignment Panel');
  });

  it('Assign highest priority patient to newly available physician', async () => {
    // Given "Dr. Johnson" was seeing a patient in bed "ED-8"
    // And the current patient queue contains:
    //   | Position | Patient Name    | ESI Level | Triage Level   | Wait Time | Bed Ready |
    //   | 1        | Maria Santos    | 2         | High Priority  | 45 min    | Yes       |
    //   | 2        | Robert Kim      | 2         | High Priority  | 60 min    | Yes       |
    //   | 3        | Lisa Chen       | 3         | Urgent         | 90 min    | Yes       |
    //   | 4        | David Brown     | 3         | Urgent         | 105 min   | No        |
    // (assumed pre-seeded test data)
    // When "Dr. Johnson" completes the discharge for the patient in bed "ED-8"
    // And the system detects "Dr. Johnson" is now available
    // (assumed to have already occurred / triggered by the system)

    // Then the system identifies the next patient assignment:
    await waitForTestId(driver, 'Highest Priority');
    const nextAssignmentCriteria = [
      { Criteria: 'Highest Priority', Value: 'Maria Santos (ESI Level 2)' },
      { Criteria: 'Bed Availability', Value: 'Bed ready for immediate assignment' },
      { Criteria: 'Provider Match', Value: 'Dr. Johnson available and qualified' },
    ];
    for (const { Criteria, Value } of nextAssignmentCriteria) {
      assert.strictEqual(await getText(driver, Criteria), Value);
    }

    // And the system assigns "Maria Santos" to "Dr. Johnson"
    assert.strictEqual(await getText(driver, 'Assigned Patient'), 'Maria Santos');
    assert.strictEqual(await getText(driver, 'Assigned Provider'), 'Dr. Johnson');

    // And a notification is sent to Dr. Johnson's mobile device:
    const mobileNotification = [
      { Type: 'Patient Assignment', Content: '📱 New Patient: Maria Santos, Bed ED-12' },
      { Type: 'Priority Level', Content: 'ESI Level 2 - High Priority' },
      { Type: 'Chief Complaint', Content: 'Severe chest pain' },
      { Type: 'Wait Time', Content: 'Patient waiting 45 minutes' },
      { Type: 'Action Required', Content: 'Please proceed to ED-12' },
    ];
    for (const { Type, Content } of mobileNotification) {
      assert.strictEqual(await getText(driver, Type), Content);
    }

    // And the patient status is updated to "Assigned to Dr. Johnson"
    assert.strictEqual(await getText(driver, 'Patient Status'), 'Assigned to Dr. Johnson');

    // And the queue position is updated for remaining patients
    const patientQueue = await waitForTestId(driver, 'Patient Queue');
    assert.ok(await patientQueue.isDisplayed());
  });

  it('Handle provider assignment with specialty requirements', async () => {
    // Given "Dr. Martinez" (Emergency Medicine) becomes available
    // And "Dr. Patel" (Pediatric Emergency) becomes available
    // And the current queue contains:
    //   | Patient Name     | Age | ESI Level | Specialty Required     | Wait Time |
    //   | Adult Patient    | 45  | 2         | Emergency Medicine     | 30 min    |
    //   | Child Patient    | 8   | 2         | Pediatric Emergency    | 35 min    |
    //   | General Patient  | 30  | 3         | Any                    | 60 min    |
    // (assumed pre-seeded test data)
    // When both providers request their next patient assignment
    // (assumed to have already occurred / triggered by the system)

    // Then the system matches providers to appropriate patients:
    await waitForTestId(driver, 'Dr. Patel Assigned Patient');
    const providerMatches = [
      { Provider: 'Dr. Patel', 'Assigned Patient': 'Child Patient' },
      { Provider: 'Dr. Martinez', 'Assigned Patient': 'Adult Patient' },
    ];
    for (const row of providerMatches) {
      assert.strictEqual(await getText(driver, `${row.Provider} Assigned Patient`), row['Assigned Patient']);
    }

    // And specialty-specific notifications are sent:
    const specialtyNotifications = [
      { Provider: 'Dr. Patel', Content: '👶 Pediatric Patient: Age 8, ESI 2, Fever' },
      { Provider: 'Dr. Martinez', Content: '🏥 Adult Patient: Age 45, ESI 2, Chest pain' },
    ];
    for (const { Provider, Content } of specialtyNotifications) {
      assert.strictEqual(await getText(driver, `${Provider} Notification`), Content);
    }

    // And the general patient remains in queue for the next available provider
    const generalPatientStatus = await getText(driver, 'General Patient Queue Status');
    assert.match(generalPatientStatus, /queue/i);
  });

  it('Prioritize critical patient over standard queue order', async () => {
    // Given "Dr. Thompson" becomes available
    // And the queue contains patients in order:
    //   | Position | Patient Name    | ESI Level | Assigned Bed | Special Circumstances |
    //   | 1        | Standard Patient| 3         | ED-5         | None                  |
    //   | 2        | Urgent Patient  | 3         | ED-7         | None                  |
    //   | 3        | Critical Patient| 1         | ED-TRAUMA-1  | Just arrived          |
    // (assumed pre-seeded test data)
    // When the system identifies the next patient for "Dr. Thompson"
    // (assumed to have already occurred / triggered by the system)

    // Then the system prioritizes by acuity over queue position:
    await waitForTestId(driver, 'Skip Queue Order');
    const prioritizationLogic = [
      { Logic: 'Skip Queue Order', Reasoning: 'ESI Level 1 takes priority over Level 3' },
      { Logic: 'Critical Priority', Reasoning: 'Life-threatening condition requires immediate' },
      { Logic: 'Provider Capability', Reasoning: 'Dr. Thompson qualified for trauma cases' },
    ];
    for (const { Logic, Reasoning } of prioritizationLogic) {
      assert.strictEqual(await getText(driver, Logic), Reasoning);
    }

    // And "Critical Patient" is assigned to "Dr. Thompson"
    assert.strictEqual(await getText(driver, 'Assigned Patient'), 'Critical Patient');
    assert.strictEqual(await getText(driver, 'Assigned Provider'), 'Dr. Thompson');

    // And the mobile notification includes urgency indicators:
    const urgencyIndicators = [
      { Field: 'Priority Alert', Content: '🚨 CRITICAL: ESI Level 1 - Trauma' },
      { Field: 'Patient Location', Content: 'ED-TRAUMA-1' },
      { Field: 'Immediate Action', Content: 'Requires immediate assessment' },
      { Field: 'Support Teams', Content: 'Trauma team standing by' },
    ];
    for (const { Field, Content } of urgencyIndicators) {
      assert.strictEqual(await getText(driver, Field), Content);
    }
  });

  it('Handle provider assignment during high volume period', async () => {
    // Given the ED is operating at 95% capacity
    // And multiple providers become available simultaneously:
    //   | Provider Name | Specialty          | Last Patient Completed |
    //   | Dr. Adams     | Emergency Medicine | 14:30                 |
    //   | Dr. Brown     | Emergency Medicine | 14:32                 |
    //   | Dr. Wilson    | Emergency Medicine | 14:35                 |
    // And 12 patients are waiting to be seen
    // (assumed pre-seeded test data)
    // When the system processes multiple provider assignments
    // (assumed to have already occurred / triggered by the system)

    // Then the system optimizes assignments across all available providers:
    await waitForTestId(driver, 'Dr. Adams Assigned Patient');
    const optimizedAssignments = [
      { Provider: 'Dr. Adams', 'Assigned Patient': 'Patient A' },
      { Provider: 'Dr. Brown', 'Assigned Patient': 'Patient B' },
      { Provider: 'Dr. Wilson', 'Assigned Patient': 'Patient C' },
    ];
    for (const row of optimizedAssignments) {
      assert.strictEqual(await getText(driver, `${row.Provider} Assigned Patient`), row['Assigned Patient']);
    }

    // And coordinated notifications are sent to prevent conflicts
    const notificationCoordinationStatus = await getText(driver, 'Notification Coordination Status');
    assert.match(notificationCoordinationStatus, /coordinated/i);

    // And remaining patients receive updated wait time estimates
    const waitTimeUpdateStatus = await getText(driver, 'Wait Time Update Status');
    assert.match(waitTimeUpdateStatus, /updated/i);

    // And surge capacity protocols are activated if needed
    const surgeCapacityStatus = await getText(driver, 'Surge Capacity Protocol Status');
    assert.match(surgeCapacityStatus, /activated|standby/i);
  });

  it('Provider assignment with workload balancing', async () => {
    // Given provider workload tracking shows:
    //   | Provider Name | Patients Seen Today | Current Workload | Complexity Score |
    //   | Dr. Garcia    | 12                 | Light            | 85              |
    //   | Dr. Lee       | 18                 | Heavy            | 140             |
    //   | Dr. Foster    | 15                 | Moderate         | 110             |
    // And "Dr. Garcia" and "Dr. Foster" both become available
    // And the next patient is "Complex Patient" with multiple comorbidities
    // (assumed pre-seeded test data)
    // When the system determines provider assignment
    // (assumed to have already occurred / triggered by the system)

    // Then the system considers workload balancing:
    await waitForTestId(driver, 'Current Workload Decision');
    const workloadBalancing = [
      { Factor: 'Current Workload', Decision: 'Favors Dr. Garcia' },
      { Factor: 'Complexity Fit', Decision: 'Both qualified' },
      { Factor: 'Fatigue Factor', Decision: 'Dr. Garcia preferred' },
    ];
    for (const { Factor, Decision } of workloadBalancing) {
      assert.strictEqual(await getText(driver, `${Factor} Decision`), Decision);
    }

    // And "Complex Patient" is assigned to "Dr. Garcia"
    assert.strictEqual(await getText(driver, 'Assigned Patient'), 'Complex Patient');
    assert.strictEqual(await getText(driver, 'Assigned Provider'), 'Dr. Garcia');

    // And workload metrics are updated for both providers
    const drGarciaWorkload = await waitForTestId(driver, 'Dr. Garcia Workload');
    assert.ok(await drGarciaWorkload.isDisplayed());
    const drFosterWorkload = await waitForTestId(driver, 'Dr. Foster Workload');
    assert.ok(await drFosterWorkload.isDisplayed());
  });

  it('Handle provider assignment with patient preferences', async () => {
    // Given a patient "VIP Patient" has requested "Dr. Johnson" if available
    // And "Dr. Johnson" and "Dr. Smith" both become available
    // And "VIP Patient" is next in the queue with ESI Level 3
    // (assumed pre-seeded test data)
    // When the system processes provider assignment
    // (assumed to have already occurred / triggered by the system)

    // Then the system considers patient preferences:
    await waitForTestId(driver, 'Patient Request');
    const preferenceFactors = [
      { Factor: 'Patient Request', Details: 'Specifically requested Dr. Johnson' },
      { Factor: 'Medical Appropriateness', Details: 'Both doctors qualified for ESI Level 3' },
      { Factor: 'Availability', Details: 'Dr. Johnson available and willing' },
    ];
    for (const { Factor, Details } of preferenceFactors) {
      assert.strictEqual(await getText(driver, Factor), Details);
    }

    // And "VIP Patient" is assigned to "Dr. Johnson"
    assert.strictEqual(await getText(driver, 'Assigned Patient'), 'VIP Patient');
    assert.strictEqual(await getText(driver, 'Assigned Provider'), 'Dr. Johnson');

    // And "Dr. Smith" receives the next patient in queue
    const drSmithAssignedPatient = await getText(driver, 'Dr. Smith Assigned Patient');
    assert.ok(drSmithAssignedPatient.length > 0);

    // And the assignment includes preference notation:
    const preferenceNotation = [
      { Field: 'Assignment Reason', Content: 'Patient preference request honored' },
      { Field: 'Special Notes', Content: 'VIP status - provide enhanced service' },
    ];
    for (const { Field, Content } of preferenceNotation) {
      assert.strictEqual(await getText(driver, Field), Content);
    }
  });

  it('Provider assignment failure and backup procedures', async () => {
    // Given "Dr. Williams" becomes available
    // And the highest priority patient is "Emergency Patient" (ESI Level 1)
    // (assumed pre-seeded test data)
    // When the system attempts to send assignment notification to Dr. Williams
    // And the mobile device notification fails to deliver
    // (assumed to have already occurred / triggered by the system)

    // Then the system activates backup notification procedures:
    await waitForTestId(driver, 'Overhead Page');
    const backupProcedures = [
      { Method: 'Overhead Page', Action: '"Dr. Williams to ED-TRAUMA-1 immediately"' },
      { Method: 'Desktop Alert', Action: 'Popup on all ED workstations' },
      { Method: 'Charge Nurse Alert', Action: 'Direct notification to charge nurse' },
      { Method: 'Secondary Provider', Action: 'Alert backup doctor if no response in 2 min' },
    ];
    for (const { Method, Action } of backupProcedures) {
      assert.strictEqual(await getText(driver, Method), Action);
    }

    // And the system logs the notification failure for IT review
    const notificationFailureLog = await getText(driver, 'Notification Failure Log');
    assert.match(notificationFailureLog, /IT review/i);

    // And continues attempting mobile notification every 30 seconds
    const mobileRetryStatus = await getText(driver, 'Mobile Notification Retry Status');
    assert.match(mobileRetryStatus, /30 seconds/i);

    // And tracks response time for quality metrics
    const responseTimeTrackingStatus = await getText(driver, 'Response Time Tracking Status');
    assert.match(responseTimeTrackingStatus, /tracking|tracked/i);
  });

  it('Handle provider assignment during shift change', async () => {
    // Given it is 19:00 during evening shift change
    // And "Dr. Day" (day shift) is completing final patients
    // And "Dr. Night" (evening shift) is beginning shift
    // And a critical patient arrives requiring immediate attention
    // (assumed pre-seeded test data)
    // When the system determines provider assignment for the critical patient
    // (assumed to have already occurred / triggered by the system)

    // Then the system considers shift transition factors:
    await waitForTestId(driver, 'Shift Status');
    const shiftTransitionFactors = [
      { Factor: 'Shift Status', Consideration: 'Dr. Day finishing, Dr. Night starting' },
      { Factor: 'Continuity', Consideration: 'Assign to Dr. Night for ongoing care' },
      { Factor: 'Availability', Consideration: 'Dr. Night has capacity for complex case' },
    ];
    for (const { Factor, Consideration } of shiftTransitionFactors) {
      assert.strictEqual(await getText(driver, Factor), Consideration);
    }

    // And the critical patient is assigned to "Dr. Night"
    assert.strictEqual(await getText(driver, 'Assigned Provider'), 'Dr. Night');

    // And shift handoff information is included in the notification:
    const handoffInformation = [
      { Component: 'Shift Context', Content: 'New critical patient - evening shift start' },
      { Component: 'Day Shift Status', Content: 'Dr. Day finishing last 2 patients' },
      { Component: 'Support Available', Content: 'Day shift available for consultation' },
    ];
    for (const { Component, Content } of handoffInformation) {
      assert.strictEqual(await getText(driver, Component), Content);
    }
  });

  it('Track provider response times and assignment efficiency', async () => {
    // Given provider assignment notifications are sent
    // (assumed pre-seeded test data)
    // When providers respond to patient assignments
    // (assumed to have already occurred / triggered by the system)

    // Then the system tracks performance metrics:
    await waitForTestId(driver, 'Notification to Response');
    const performanceMetrics = [
      { Metric: 'Notification to Response', Measurement: 'Time from alert to bedside presence' },
      { Metric: 'Assignment Accuracy', Measurement: 'Correct provider-patient matching' },
      { Metric: 'Queue Optimization', Measurement: 'Wait time reduction effectiveness' },
    ];
    for (const { Metric, Measurement } of performanceMetrics) {
      assert.strictEqual(await getText(driver, Metric), Measurement);
    }

    // And generates provider performance reports:
    const performanceReports = [
      { Provider: 'Dr. Johnson', 'Avg Response Time': '3.2 minutes', 'Assignment Accuracy': '98%', 'Patient Satisfaction': '4.8/5' },
      { Provider: 'Dr. Smith', 'Avg Response Time': '4.1 minutes', 'Assignment Accuracy': '96%', 'Patient Satisfaction': '4.6/5' },
    ];
    for (const report of performanceReports) {
      assert.strictEqual(await getText(driver, `${report.Provider} Avg Response Time`), report['Avg Response Time']);
      assert.strictEqual(await getText(driver, `${report.Provider} Assignment Accuracy`), report['Assignment Accuracy']);
      assert.strictEqual(await getText(driver, `${report.Provider} Patient Satisfaction`), report['Patient Satisfaction']);
    }

    // And identifies optimization opportunities:
    const optimizationOpportunities = [
      { Area: 'Response Time', Recommendation: 'Target <3 minutes for critical patients' },
      { Area: 'Assignment Matching', Recommendation: 'Consider additional specialty training' },
      { Area: 'Communication', Recommendation: 'Implement two-way acknowledgment system' },
    ];
    for (const { Area, Recommendation } of optimizationOpportunities) {
      assert.strictEqual(await getText(driver, Area), Recommendation);
    }
  });
});
