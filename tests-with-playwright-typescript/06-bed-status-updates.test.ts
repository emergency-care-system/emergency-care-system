// Playwright test for tests-with-given-when-then-features/06-bed-status-updates.feature
// (equivalent to tests-with-selenium-javascript/06-bed-status-updates.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see support/fields.js) and the shared
// data-testid contract in support/login.js (login-identity, login-submit,
// app-root).

import { chromium, test, type Browser, type Page } from '@playwright/test';
import { strict as assert } from 'assert';
import { login, verifySystemIsOperational } from './support/login.js';
import { fillField, fillFields, getText, waitForTestId } from './support/fields.js';

test.describe.configure({ mode: 'serial' });

test.describe('Feature: Bed Status Updates', () => {
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
    //   And I am logged in as a nurse
    //   And the bed management module is active
    //   And housekeeping notification system is enabled
    await verifySystemIsOperational(page);
    await login(page, 'a nurse');
    // The bed management module and housekeeping notification system are
    // assumed to be pre-seeded/enabled test data.

    const featureNavLink = await waitForTestId(page, 'Nav Bed Status Updates');
    await featureNavLink.click();
    await waitForTestId(page, 'Bed Status Updates Panel');
  });

  test('Mark bed as needs cleaning after patient discharge', async () => {
    // Given a patient "John Doe" is currently occupying bed "ED-12"
    // And the bed status is "Occupied"
    // And the available bed count shows 8 out of 20 beds available
    // (assumed pre-seeded test data)

    // When the patient is discharged from bed "ED-12"
    await page.getByTestId('discharge-patient-button').click();

    // And I mark the bed as "Needs Cleaning"
    await fillField(page, 'New Bed Status', 'Needs Cleaning');

    // And I submit the bed status update
    await page.getByTestId('submit-bed-status-update').click();

    // Then the system updates the bed status to "Dirty"
    await waitForTestId(page, 'Bed Status');
    const bedStatus = await getText(page, 'Bed Status');
    assert.strictEqual(bedStatus, 'Dirty');

    // And a notification is sent to housekeeping with details:
    const notificationFields = [
      { Field: 'Room Number', Value: 'ED-12' },
      { Field: 'Status', Value: 'Needs Cleaning' },
      { Field: 'Priority', Value: 'Standard' },
      { Field: 'Patient Type', Value: 'Standard discharge' },
      { Field: 'Special Requirements', Value: 'Standard cleaning protocol' },
      { Field: 'Timestamp', Value: 'Current time' },
    ];
    for (const { Field, Value } of notificationFields) {
      assert.strictEqual(await getText(page, Field), Value);
    }

    // And the bed is removed from the available bed count
    // And the available bed count updates to 7 out of 20 beds available
    const availableBedCount = await getText(page, 'Available Bed Count');
    assert.strictEqual(availableBedCount, '7 out of 20 beds available');

    // And the bed appears as "Dirty" on the bed management dashboard
    const dashboardBedStatus = await getText(page, 'Dashboard Bed Status');
    assert.strictEqual(dashboardBedStatus, 'Dirty');
  });

  test('Mark isolation bed for deep cleaning after infectious patient', async () => {
    // Given a patient "Jane Smith" with isolation precautions is in bed "ED-ISO-2"
    // And the bed status is "Occupied - Isolation"
    // And the patient had confirmed MRSA infection
    // (assumed pre-seeded test data)

    // When the patient is discharged from bed "ED-ISO-2"
    await page.getByTestId('discharge-patient-button').click();

    // And I mark the bed as "Needs Deep Cleaning"
    await fillField(page, 'New Bed Status', 'Needs Deep Cleaning');

    // And I specify the isolation type as "Contact Precautions - MRSA"
    await fillField(page, 'Isolation Type', 'Contact Precautions - MRSA');

    // And I submit the bed status update
    await page.getByTestId('submit-bed-status-update').click();

    // Then the system updates the bed status to "Dirty - Isolation"
    await waitForTestId(page, 'Bed Status');
    const bedStatus = await getText(page, 'Bed Status');
    assert.strictEqual(bedStatus, 'Dirty - Isolation');

    // And a high-priority notification is sent to housekeeping with details:
    const notificationFields = [
      { Field: 'Room Number', Value: 'ED-ISO-2' },
      { Field: 'Status', Value: 'Needs Deep Cleaning' },
      { Field: 'Priority', Value: 'High' },
      { Field: 'Infection Type', Value: 'MRSA - Contact Precautions' },
      { Field: 'Special Requirements', Value: 'Terminal cleaning required' },
      { Field: 'PPE Required', Value: 'Gowns, gloves, masks' },
    ];
    for (const { Field, Value } of notificationFields) {
      assert.strictEqual(await getText(page, Field), Value);
    }

    // And the bed is flagged as "Out of Service" until deep cleaning completion
    const bedServiceFlag = await getText(page, 'Bed Service Flag');
    assert.strictEqual(bedServiceFlag, 'Out of Service');

    // And the isolation bed count is reduced by one
    await waitForTestId(page, 'Isolation Bed Count');

    // And an alert is sent to infection control team
    await waitForTestId(page, 'Infection Control Alert');
  });

  test('Housekeeping completes cleaning and marks bed ready', async () => {
    // Given bed "ED-8" has status "Dirty"
    // And housekeeping was notified 30 minutes ago
    // (assumed pre-seeded test data)

    // When the housekeeping staff completes cleaning of bed "ED-8"
    await page.getByTestId('complete-cleaning-button').click();

    // And the housekeeping supervisor marks the bed as "Clean and Ready"
    await fillField(page, 'New Bed Status', 'Clean and Ready');

    // And submits the cleaning completion with details:
    await fillFields(page, [
      { Field: 'Cleaning Staff', Value: 'Maria Rodriguez' },
      { Field: 'Cleaning Start', Value: '14:30' },
      { Field: 'Cleaning End', Value: '15:00' },
      { Field: 'Cleaning Type', Value: 'Standard' },
      { Field: 'Supplies Used', Value: 'Standard disinfection' },
    ]);
    await page.getByTestId('submit-cleaning-completion-form').click();

    // Then the system updates the bed status to "Available"
    await waitForTestId(page, 'Bed Status');
    const bedStatus = await getText(page, 'Bed Status');
    assert.strictEqual(bedStatus, 'Available');

    // And the bed is added back to the available bed count
    // And the available bed count increases by one
    await waitForTestId(page, 'Available Bed Count');

    // And a notification is sent to the charge nurse that bed "ED-8" is ready
    await waitForTestId(page, 'Charge Nurse Notification');

    // And the bed appears as "Available" on the bed management dashboard
    const dashboardBedStatus = await getText(page, 'Dashboard Bed Status');
    assert.strictEqual(dashboardBedStatus, 'Available');
  });

  test('Handle bed maintenance request during status update', async () => {
    // Given a patient is discharged from bed "ED-15"
    // (assumed pre-seeded test data)

    // When I attempt to mark the bed as "Needs Cleaning"
    await fillField(page, 'New Bed Status', 'Needs Cleaning');

    // And I notice equipment malfunction in the room
    // (observation, no direct UI action)

    // And I select "Maintenance Required" in addition to cleaning needs
    await page.getByTestId('maintenance-required-checkbox').click();

    // And I specify the issue as "IV pump not functioning, call light broken"
    await fillField(page, 'Issue Description', 'IV pump not functioning, call light broken');

    // And I submit the bed status update
    await page.getByTestId('submit-bed-status-update').click();

    // Then the system updates the bed status to "Out of Service - Maintenance"
    await waitForTestId(page, 'Bed Status');
    const bedStatus = await getText(page, 'Bed Status');
    assert.strictEqual(bedStatus, 'Out of Service - Maintenance');

    // And notifications are sent to both:
    const notifications = [
      { Department: 'Housekeeping', 'Notification Details': 'Hold cleaning until maintenance' },
      { Department: 'Maintenance', 'Notification Details': 'IV pump and call light repair' },
    ];
    for (const notification of notifications) {
      assert.strictEqual(await getText(page, notification.Department), notification['Notification Details']);
    }

    // And the bed is removed from available count until both issues are resolved
    await waitForTestId(page, 'Available Bed Count');

    // And a work order is automatically generated for maintenance
    await waitForTestId(page, 'Maintenance Work Order');

    // And the estimated downtime is calculated and displayed
    const estimatedDowntime = await getText(page, 'Estimated Downtime');
    assert.ok(estimatedDowntime.length > 0);
  });

  test('Update bed status during patient transfer', async () => {
    // Given a patient "Robert Wilson" is in bed "ED-6"
    // And the patient needs to be transferred to ICU
    // (assumed pre-seeded test data)

    // When the transport team arrives to transfer the patient
    await page.getByTestId('transport-team-arrived-button').click();

    // And I update the bed status to "Patient in Transit"
    await fillField(page, 'New Bed Status', 'Patient in Transit');

    // And I specify the destination as "ICU Room 302"
    await fillField(page, 'Destination', 'ICU Room 302');

    // And I submit the status update
    await page.getByTestId('submit-bed-status-update').click();

    // Then the bed status is temporarily set to "In Transit"
    await waitForTestId(page, 'Bed Status');
    const bedStatus = await getText(page, 'Bed Status');
    assert.strictEqual(bedStatus, 'In Transit');

    // And the bed remains unavailable for new assignments
    const bedAvailability = await getText(page, 'Bed Availability');
    assert.match(bedAvailability, /unavailable/i);

    // And a notification is sent to the receiving unit
    await waitForTestId(page, 'Receiving Unit Notification');

    // And when the transfer is confirmed complete, I can mark the bed as "Needs Cleaning"
    await page.getByTestId('confirm-transfer-complete-button').click();
    await fillField(page, 'New Bed Status', 'Needs Cleaning');

    // And the normal cleaning workflow is initiated
    await waitForTestId(page, 'Cleaning Workflow Status');
  });

  test('Handle multiple bed status updates simultaneously', async () => {
    // Given multiple beds require status updates:
    // (assumed pre-seeded test data)

    // When I perform batch bed status updates:
    await fillFields(page, [
      { Field: 'ED-3 New Status', Value: 'Needs Cleaning' },
      { Field: 'ED-7 New Status', Value: 'Patient in Transit' },
      { Field: 'ED-11 New Status', Value: 'Available' },
      { Field: 'ED-14 New Status', Value: 'Needs Cleaning' },
    ]);
    await page.getByTestId('submit-batch-bed-status-update').click();

    // Then the system processes all updates simultaneously
    await waitForTestId(page, 'Batch Update Status');

    // And appropriate notifications are sent to all relevant departments
    await waitForTestId(page, 'Department Notifications Sent');

    // And the bed availability dashboard is updated in real-time
    await waitForTestId(page, 'Bed Availability Dashboard');

    // And the total available bed count reflects all changes
    await waitForTestId(page, 'Available Bed Count');
  });

  test('Handle urgent bed turnover request', async () => {
    // Given the ED is at 95% capacity
    // And there is a trauma patient incoming requiring immediate bed
    // And bed "ED-4" patient is ready for discharge
    // (assumed pre-seeded test data)

    // When I mark the discharge as "Urgent Turnover Required"
    await fillField(page, 'New Bed Status', 'Urgent Turnover Required');

    // And I request expedited cleaning for bed "ED-4"
    await page.getByTestId('request-expedited-cleaning-button').click();

    // And I submit the urgent status update
    await page.getByTestId('submit-bed-status-update').click();

    // Then the system updates bed status to "Dirty - Urgent"
    await waitForTestId(page, 'Bed Status');
    const bedStatus = await getText(page, 'Bed Status');
    assert.strictEqual(bedStatus, 'Dirty - Urgent');

    // And a high-priority notification is sent to housekeeping:
    const notificationFields = [
      { Field: 'Priority Level', Value: 'URGENT' },
      { Field: 'Room Number', Value: 'ED-4' },
      { Field: 'Reason', Value: 'Incoming trauma patient' },
      { Field: 'Target Time', Value: '15 minutes' },
      { Field: 'Special Instructions', Value: 'Expedited cleaning protocol' },
    ];
    for (const { Field, Value } of notificationFields) {
      assert.strictEqual(await getText(page, Field), Value);
    }

    // And the charge nurse is notified of the urgent turnover request
    await waitForTestId(page, 'Charge Nurse Notification');

    // And a timer is started to track cleaning completion time
    await waitForTestId(page, 'Cleaning Completion Timer');
  });

  test('Validate bed status change restrictions', async () => {
    // Given bed "ED-9" currently has status "Occupied"
    // And a patient "Susan Davis" is actively receiving treatment
    // (assumed pre-seeded test data)

    // When I attempt to mark the bed as "Available"
    await fillField(page, 'New Bed Status', 'Available');

    // And I submit the invalid status change
    await page.getByTestId('submit-bed-status-update').click();

    // Then the system displays a validation error:
    const validationErrors = [
      { 'Error Type': 'Invalid Transition', Message: 'Cannot mark occupied bed as available' },
      { 'Error Type': 'Required Action', Message: 'Discharge patient first' },
      { 'Error Type': 'Current Patient', Message: 'Susan Davis - Active treatment' },
    ];
    for (const error of validationErrors) {
      assert.strictEqual(await getText(page, error['Error Type']), error.Message);
    }

    // And the bed status remains "Occupied"
    const bedStatus = await getText(page, 'Bed Status');
    assert.strictEqual(bedStatus, 'Occupied');

    // And no notifications are sent
    const notifications = await page.getByTestId('housekeeping-notification').all();
    assert.strictEqual(notifications.length, 0);

    // And I am prompted to follow proper discharge workflow
    await waitForTestId(page, 'Discharge Workflow Prompt');
  });

  test('Track bed status history and audit trail', async () => {
    // Given bed "ED-5" has had multiple status changes today
    // (assumed pre-seeded test data)

    // When I access the bed status history
    await page.getByTestId('view-bed-status-history-button').click();

    // Then the system displays the complete audit trail:
    const auditTrailEntries = await page.getByTestId('audit-trail-entry').all();
    assert.strictEqual(auditTrailEntries.length, 5);

    // And each status change includes timestamp and user identification
    await waitForTestId(page, 'Audit Trail');

    // And the audit trail is preserved for compliance reporting
    const complianceStatus = await getText(page, 'Compliance Reporting Status');
    assert.match(complianceStatus, /preserved/i);

    // And I can generate reports on bed utilization patterns
    await waitForTestId(page, 'Generate Utilization Report Button');
  });

  test('Handle bed status update during system maintenance', async () => {
    // Given the housekeeping notification system is temporarily offline
    // And a patient is discharged from bed "ED-16"
    // (assumed pre-seeded test data)

    // When I mark the bed as "Needs Cleaning"
    await fillField(page, 'New Bed Status', 'Needs Cleaning');

    // And I submit the status update
    await page.getByTestId('submit-bed-status-update').click();

    // Then the system updates the bed status to "Dirty"
    await waitForTestId(page, 'Bed Status');
    const bedStatus = await getText(page, 'Bed Status');
    assert.strictEqual(bedStatus, 'Dirty');

    // And the system queues the housekeeping notification for later delivery
    await waitForTestId(page, 'Queued Notification Status');

    // And a warning message is displayed: "Housekeeping system offline - notification queued"
    const warningMessage = await getText(page, 'Warning Message');
    assert.strictEqual(warningMessage, 'Housekeeping system offline - notification queued');

    // And the bed is still removed from available count
    await waitForTestId(page, 'Available Bed Count');

    // And when the housekeeping system comes back online, queued notifications are automatically sent
    // (system behavior outside the scope of this interaction)

    // And a log entry is created documenting the delayed notification
    await waitForTestId(page, 'Delayed Notification Log Entry');
  });
});
