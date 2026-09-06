// Playwright test for spec/features/08-order-entry.feature
// (equivalent to tests-with-selenium-javascript/08-order-entry.test.js).
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

test.describe('Feature: Order Entry', () => {
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
    //   And I am logged in as "Dr. Smith"
    //   And the electronic order entry module is active
    //   And departmental interfaces (lab, radiology, pharmacy) are connected
    await verifySystemIsOperational(page);
    await login(page, 'Dr. Smith');
    // The electronic order entry module being active and the departmental
    // interfaces being connected are assumed pre-seeded test data /
    // environment configuration.

    const orderEntryNavLink = await waitForTestId(page, 'Nav Order Entry');
    await orderEntryNavLink.click();
    await waitForTestId(page, 'Order Entry Panel');
  });

  test('Enter standard orders for chest pain workup', async () => {
    // Given I have examined a patient "John Martinez" in bed "ED-5"
    // And the patient presents with "acute chest pain"
    // And the patient's allergies and contraindications have been reviewed
    // (assumed pre-seeded test data)

    // When I open the order entry module for the patient
    await page.getByTestId('open-order-entry-module-button').click();

    // And I enter the following laboratory orders:
    //   | Order Type     | Test Name        | Priority | Special Instructions |
    //   | Laboratory     | CBC with diff    | Routine  | None                |
    //   | Laboratory     | Troponin I       | STAT     | Serial in 6 hours   |
    //   | Laboratory     | Basic Metabolic  | Routine  | None                |
    await page.getByTestId('enter-order-button').click();

    // And I enter the following radiology order:
    //   | Order Type     | Study Name       | Priority | Special Instructions |
    //   | Radiology      | Chest X-ray PA/LAT| STAT    | R/O pneumonia       |
    await page.getByTestId('enter-order-button').click();

    // And I submit all orders with my electronic signature
    await page.getByTestId('submit-orders-button').click();

    // Then the system sends electronic orders to the laboratory with details:
    //   | Order ID | Test Name     | Patient Info        | Priority | Timestamp |
    //   | LAB-001  | CBC with diff | John Martinez ED-5  | Routine  | Current   |
    //   | LAB-002  | Troponin I    | John Martinez ED-5  | STAT     | Current   |
    //   | LAB-003  | Basic Metabolic| John Martinez ED-5 | Routine  | Current   |
    const labOrdersSent = await page.getByTestId('laboratory-order-sent').all();
    assert.strictEqual(labOrdersSent.length, 3);
    const firstLabOrderText = await labOrdersSent[0].innerText();
    assert.match(firstLabOrderText, /LAB-001/);

    // And the system sends electronic orders to radiology with details:
    //   | Order ID | Study Name    | Patient Info        | Priority | Timestamp |
    //   | RAD-001  | CXR PA/LAT    | John Martinez ED-5  | STAT     | Current   |
    const radiologyOrdersSent = await page.getByTestId('radiology-order-sent').all();
    assert.strictEqual(radiologyOrdersSent.length, 1);

    // And specimen labels are automatically generated:
    //   | Label Type     | Content                                    |
    //   | Blood Draw     | John Martinez, DOB: 1975-08-15, ED-5     |
    //   | Test Codes     | CBC, Troponin, BMP                        |
    //   | Collection Time| STAT - Collect immediately                |
    //   | Barcode        | Patient and order identifiers             |
    const specimenLabels = await page.getByTestId('specimen-label').all();
    assert.strictEqual(specimenLabels.length, 4);

    // And nursing tasks are added to the workflow:
    //   | Task Type           | Description                    | Priority | Due Time    |
    //   | Blood Collection    | Draw CBC, Troponin, BMP       | STAT     | Immediate   |
    //   | Patient Transport   | Transport to X-ray            | STAT     | After labs  |
    //   | Monitor Results     | Watch for critical values     | High     | Ongoing     |
    const nursingTasks = await page.getByTestId('nursing-task').all();
    assert.strictEqual(nursingTasks.length, 3);
  });

  test('Enter orders with drug allergy checking', async () => {
    // Given I have examined a patient "Sarah Johnson" in bed "ED-8"
    // And the patient has documented allergies:
    //   | Allergy    | Reaction Type    | Severity |
    //   | Penicillin | Rash, hives      | Moderate |
    //   | Morphine   | Respiratory depression | Severe |
    // (assumed pre-seeded test data)

    // When I attempt to enter a medication order:
    //   | Order Type | Medication  | Dose     | Route | Frequency |
    //   | Medication | Amoxicillin | 500mg    | PO    | TID       |
    await page.getByTestId('enter-order-button').click();

    // And I submit the order
    await page.getByTestId('submit-orders-button').click();

    // Then the system displays an allergy alert:
    await waitForTestId(page, 'Drug Allergy');
    assert.strictEqual(await getText(page, 'Drug Allergy'), 'WARNING: Patient allergic to Penicillin');
    assert.strictEqual(await getText(page, 'Severity'), 'Moderate - Rash, hives');
    assert.strictEqual(await getText(page, 'Cross-reaction'), 'Amoxicillin contains penicillin');
    assert.strictEqual(await getText(page, 'Recommendation'), 'Consider alternative antibiotic');

    // And the order is held pending confirmation
    const orderStatus = await getText(page, 'Order Status');
    assert.match(orderStatus, /pending confirmation/i);

    // And I must either:
    //   | Action Option      | Description                                |
    //   | Override with reason| Document clinical justification          |
    //   | Cancel order       | Remove the problematic medication         |
    //   | Select alternative | Choose non-penicillin antibiotic          |
    const allergyActionOptions = await page.getByTestId('allergy-action-option').all();
    assert.strictEqual(allergyActionOptions.length, 3);

    // And the allergy alert is logged in the patient record
    const allergyAlertLogEntry = await getText(page, 'Allergy Alert Log Entry');
    assert.ok(allergyAlertLogEntry.length > 0);
  });

  test('Enter STAT orders during emergency situation', async () => {
    // Given I have examined a patient "Emergency Patient" in bed "ED-TRAUMA-1"
    // And the patient is in critical condition with "severe trauma"
    // (assumed pre-seeded test data)

    // When I enter emergency orders:
    //   | Order Type     | Description           | Priority | Special Instructions    |
    //   | Laboratory     | Type and Crossmatch   | STAT     | 6 units PRBC on hold   |
    //   | Laboratory     | PT/INR, PTT          | STAT     | Pre-surgery labs       |
    //   | Radiology      | CT Head without contrast| STAT   | Rule out intracranial bleeding |
    //   | Radiology      | CT Chest/Abd/Pelvis  | STAT     | Trauma protocol        |
    //   | Medication     | Normal Saline        | STAT     | 1L wide open IV        |
    await page.getByTestId('enter-order-button').click();

    // And I mark all orders as "Emergency - Life threatening"
    await fillField(page, 'Order Marking', 'Emergency - Life threatening');

    // And I submit the orders
    await page.getByTestId('submit-orders-button').click();

    // Then all orders are immediately transmitted with highest priority
    const orderTransmissionStatus = await getText(page, 'Order Transmission Status');
    assert.match(orderTransmissionStatus, /highest priority/i);

    // And the laboratory receives orders marked "CRITICAL - TRAUMA"
    const laboratoryOrderMarking = await getText(page, 'Laboratory Order Marking');
    assert.strictEqual(laboratoryOrderMarking, 'CRITICAL - TRAUMA');

    // And blood bank is notified to prepare emergency release protocol
    const bloodBankNotification = await getText(page, 'Blood Bank Notification');
    assert.match(bloodBankNotification, /emergency release protocol/i);

    // And radiology is alerted for trauma CT protocol
    const radiologyAlert = await getText(page, 'Radiology Alert');
    assert.match(radiologyAlert, /trauma CT protocol/i);

    // And nursing receives immediate action items:
    //   | Task               | Action Required           | Time Limit |
    //   | Blood Draw         | Collect trauma labs       | 5 minutes  |
    //   | IV Access          | Large bore IV x2          | Immediate  |
    //   | Patient Prep       | Prepare for CT transport  | 10 minutes |
    const nursingActionItems = await page.getByTestId('nursing-action-item').all();
    assert.strictEqual(nursingActionItems.length, 3);

    // And all departments receive automatic status updates
    const departmentStatusUpdate = await getText(page, 'Department Status Update');
    assert.ok(departmentStatusUpdate.length > 0);
  });

  test('Enter pediatric orders with weight-based dosing', async () => {
    // Given I have examined a pediatric patient "Tommy Chen" (age 5, weight 18kg) in bed "ED-PEDS-1"
    // And the patient presents with "febrile seizure"
    // (assumed pre-seeded test data)

    // When I enter pediatric medication orders:
    //   | Order Type | Medication | Dose Calculation        | Route | Frequency |
    //   | Medication | Acetaminophen| 15mg/kg (270mg)      | PO    | Q6H PRN   |
    //   | Medication | Lorazepam  | 0.1mg/kg (1.8mg)      | IV    | Once      |
    await page.getByTestId('enter-order-button').click();

    // And I enter diagnostic orders:
    //   | Order Type | Test/Study    | Pediatric Protocol    | Priority |
    //   | Laboratory | CBC with diff | Pediatric collection  | STAT     |
    //   | Laboratory | Blood glucose | Fingerstick acceptable| STAT     |
    await page.getByTestId('enter-order-button').click();

    // And I submit the pediatric orders
    await page.getByTestId('submit-orders-button').click();

    // Then the system validates weight-based dosing calculations
    const dosingValidationStatus = await getText(page, 'Dosing Validation Status');
    assert.match(dosingValidationStatus, /validated/i);

    // And pediatric-specific protocols are applied:
    //   | Protocol Type      | Details                                |
    //   | Collection Volume  | Minimum blood volume for pediatric labs|
    //   | Dosing Alerts      | Maximum safe dose verified             |
    //   | Administration     | Child-friendly instructions           |
    const pediatricProtocols = await page.getByTestId('pediatric-protocol').all();
    assert.strictEqual(pediatricProtocols.length, 3);

    // And nursing receives pediatric-specific tasks:
    //   | Task Type          | Pediatric Instructions                 |
    //   | Medication Admin   | Use pediatric dosing chart            |
    //   | Blood Collection   | Minimize collection volume            |
    //   | Comfort Measures   | Parent/caregiver involvement          |
    const pediatricNursingTasks = await page.getByTestId('pediatric-nursing-task').all();
    assert.strictEqual(pediatricNursingTasks.length, 3);

    // And pharmacy receives weight-verified dosing information
    const pharmacyDosingNotification = await getText(page, 'Pharmacy Dosing Notification');
    assert.ok(pharmacyDosingNotification.length > 0);
  });

  test('Handle order modifications and cancellations', async () => {
    // Given I previously entered orders for patient "Maria Rodriguez" in bed "ED-12"
    // And the existing orders include:
    //   | Order ID | Order Type | Description    | Status      | Entered Time |
    //   | ORD-101  | Laboratory | CBC           | In Progress | 10:30        |
    //   | ORD-102  | Radiology  | Chest X-ray   | Pending     | 10:30        |
    //   | ORD-103  | Medication | Morphine 2mg  | Pending     | 10:30        |
    // (assumed pre-seeded test data)

    // When I need to modify the orders based on new clinical information
    // (no direct UI action for this narrative step)

    // And I cancel order "ORD-103" with reason "Patient reports morphine allergy"
    await fillFields(page, [
      { Field: 'Order ID', Value: 'ORD-103' },
      { Field: 'Cancellation Reason', Value: 'Patient reports morphine allergy' },
    ]);
    await page.getByTestId('cancel-order-button').click();

    // And I modify order "ORD-102" to add "portable" due to patient instability
    await fillFields(page, [
      { Field: 'Order ID', Value: 'ORD-102' },
      { Field: 'Modification', Value: 'Add portable' },
    ]);
    await page.getByTestId('modify-order-button').click();

    // And I add a new order for "Fentanyl 50mcg IV push"
    await fillField(page, 'New Order Description', 'Fentanyl 50mcg IV push');
    await page.getByTestId('add-order-button').click();

    // Then the system processes the order changes:
    //   | Action Type | Order ID | New Status    | Reason/Details              |
    //   | Cancelled   | ORD-103  | Cancelled     | Morphine allergy discovered |
    //   | Modified    | ORD-102  | Updated       | Changed to portable CXR     |
    //   | New Order   | ORD-104  | Pending       | Fentanyl 50mcg IV push     |
    const orderChanges = await page.getByTestId('order-change-entry').all();
    assert.strictEqual(orderChanges.length, 3);
    const firstOrderChangeText = await orderChanges[0].innerText();
    assert.match(firstOrderChangeText, /ORD-103/);

    // And notifications are sent to affected departments:
    //   | Department | Notification                               |
    //   | Pharmacy   | Morphine order cancelled - allergy        |
    //   | Radiology  | CXR modified to portable study            |
    //   | Nursing    | New pain medication order available       |
    const departmentNotifications = await page.getByTestId('department-notification').all();
    assert.strictEqual(departmentNotifications.length, 3);

    // And an audit trail is maintained for all order changes
    const auditTrail = page.getByTestId('order-change-audit-trail');
    assert.ok(await auditTrail.isVisible());
  });

  test('Enter orders with insurance authorization requirements', async () => {
    // Given I have examined a patient "Robert Davis" in bed "ED-6"
    // And the patient has insurance requiring prior authorization for certain studies
    // (assumed pre-seeded test data)

    // When I enter an order for:
    //   | Order Type | Study Name | Estimated Cost | Insurance Notes        |
    //   | Radiology  | CT Abdomen | $1,200        | Requires pre-auth      |
    await page.getByTestId('enter-order-button').click();

    // And I submit the order
    await page.getByTestId('submit-orders-button').click();

    // Then the system checks insurance requirements:
    assert.strictEqual(await getText(page, 'Coverage Verification'), 'CT covered with prior authorization');
    assert.strictEqual(await getText(page, 'Authorization Status'), 'Prior auth required');
    assert.strictEqual(await getText(page, 'Alternative Options'), 'Ultrasound covered without pre-auth');

    // And I am presented with options:
    //   | Option             | Description                            |
    //   | Submit for auth    | Send for insurance approval (delay)    |
    //   | Order alternative  | Consider ultrasound instead           |
    //   | Emergency override | Document medical necessity            |
    const insuranceOptions = await page.getByTestId('insurance-option').all();
    assert.strictEqual(insuranceOptions.length, 3);

    // And the order status is marked "Pending Authorization"
    const orderStatus = await getText(page, 'Order Status');
    assert.strictEqual(orderStatus, 'Pending Authorization');

    // And the patient financial counselor is notified
    const financialCounselorNotification = await getText(page, 'Financial Counselor Notification');
    assert.ok(financialCounselorNotification.length > 0);
  });

  test('Handle order entry during system integration failures', async () => {
    // Given I am entering orders for patient "Lisa Wong" in bed "ED-14"
    // And the laboratory information system is temporarily offline
    // (assumed pre-seeded test data)

    // When I enter laboratory orders:
    //   | Order Type | Test Name     | Priority |
    //   | Laboratory | Troponin      | STAT     |
    //   | Laboratory | CBC          | Routine  |
    await page.getByTestId('enter-order-button').click();

    // And I submit the orders
    await page.getByTestId('submit-orders-button').click();

    // Then the system displays a warning: "Lab system offline - orders will be queued"
    const warningMessage = await getText(page, 'Warning Message');
    assert.strictEqual(warningMessage, 'Lab system offline - orders will be queued');

    // And the orders are stored locally with status "Queued for transmission"
    const orderStatus = await getText(page, 'Order Status');
    assert.strictEqual(orderStatus, 'Queued for transmission');

    // And nursing is notified to manually coordinate with lab
    const nursingCoordinationNotification = await getText(page, 'Nursing Coordination Notification');
    assert.ok(nursingCoordinationNotification.length > 0);

    // And I receive a notification when lab system connectivity is restored
    const connectivityRestoredNotification = await getText(page, 'Connectivity Restored Notification');
    assert.ok(connectivityRestoredNotification.length > 0);

    // And queued orders are automatically transmitted when system is available
    const queuedOrderTransmissionStatus = await getText(page, 'Queued Order Transmission Status');
    assert.match(queuedOrderTransmissionStatus, /transmitted/i);

    // And manual backup procedures are documented for critical orders
    const manualBackupProcedureDocumentation = await getText(page, 'Manual Backup Procedure Documentation');
    assert.ok(manualBackupProcedureDocumentation.length > 0);
  });

  test('Enter complex order sets for specific protocols', async () => {
    // Given I have examined a patient "James Thompson" in bed "ED-11"
    // And the patient presents with "suspected stroke"
    // (assumed pre-seeded test data)

    // When I select the "Acute Stroke Protocol" order set
    await fillField(page, 'Order Set', 'Acute Stroke Protocol');

    // Then the system presents the standardized stroke workup orders:
    //   | Category   | Order Description              | Priority | Default |
    //   | Laboratory | CBC, BMP, PT/INR, PTT         | STAT     | Selected|
    //   | Laboratory | Troponin, Lipid panel         | STAT     | Selected|
    //   | Radiology  | CT Head without contrast      | STAT     | Selected|
    //   | Radiology  | CT Angiogram head/neck        | STAT     | Optional|
    //   | Medication | Aspirin 325mg                 | STAT     | Selected|
    //   | Consults   | Neurology consult             | STAT     | Selected|
    const strokeProtocolOrders = await page.getByTestId('stroke-protocol-order').all();
    assert.strictEqual(strokeProtocolOrders.length, 6);

    // And I can modify or remove individual orders from the set
    await page.getByTestId('modify-order').first().waitFor();
    await page.getByTestId('remove-order').first().waitFor();

    // And I add stroke-specific timing requirements:
    //   | Order          | Time Requirement                      |
    //   | CT Head        | Within 25 minutes of arrival         |
    //   | Lab results    | Within 45 minutes of arrival         |
    //   | Neurology      | Consult within 15 minutes           |
    await page.getByTestId('add-timing-requirement-button').click();

    // And the system tracks compliance with stroke protocol timing
    const protocolComplianceTrackingStatus = await getText(page, 'Protocol Compliance Tracking Status');
    assert.match(protocolComplianceTrackingStatus, /tracks compliance/i);

    // And automatic reminders are set for time-sensitive elements
    const automaticReminders = await page.getByTestId('automatic-reminder').all();
    assert.strictEqual(automaticReminders.length, 3);
  });
});
