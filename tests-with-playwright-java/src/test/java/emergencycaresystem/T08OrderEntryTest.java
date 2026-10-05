// Playwright + JUnit 5 test for
// tests-with-given-when-then-features/08-order-entry.feature
// (equivalent to tests-with-playwright-javascript/08-order-entry.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see support/Fields.java) and the shared
// data-testid contract in support/Login.java (login-identity, login-submit,
// app-root).

package emergencycaresystem;

import static emergencycaresystem.support.Config.BASE_URL;
import static emergencycaresystem.support.Fields.*;
import static emergencycaresystem.support.Login.*;
import static emergencycaresystem.support.Matchers.assertMatches;
import static org.junit.jupiter.api.Assertions.*;

import java.util.List;
import java.util.Map;
import com.microsoft.playwright.Page;
import emergencycaresystem.support.Session;
import org.junit.jupiter.api.AfterAll;
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.MethodOrderer;
import org.junit.jupiter.api.Order;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.TestMethodOrder;

@TestMethodOrder(MethodOrderer.OrderAnnotation.class)
class T08OrderEntryTest {
    private static Session session;
    private static Page page;

    @BeforeAll
    static void setUpClass() {
        session = Session.start();
        page = session.page();
    }

    @AfterAll
    static void tearDownClass() {
        session.close();
    }

    @BeforeEach
    void setUp() {
        // Background:
        //   Given the emergency care system is operational
        //   And I am logged in as "Dr. Smith"
        //   And the electronic order entry module is active
        //   And departmental interfaces (lab, radiology, pharmacy) are connected
        verifySystemIsOperational(page);
        login(page, "Dr. Smith");
        // The electronic order entry module being active and the departmental
        // interfaces being connected are assumed pre-seeded test data /
        // environment configuration.

        var orderEntryNavLink = waitForTestId(page, "Nav Order Entry");
        orderEntryNavLink.click();
        waitForTestId(page, "Order Entry Panel");
    }

    @Test
    @Order(1)
    @DisplayName("Enter standard orders for chest pain workup")
    void enterStandardOrdersForChestPainWorkup() {
        // Given I have examined a patient "John Martinez" in bed "ED-5"
        // And the patient presents with "acute chest pain"
        // And the patient's allergies and contraindications have been reviewed
        // (assumed pre-seeded test data)

        // When I open the order entry module for the patient
        page.getByTestId("open-order-entry-module-button").first().click();

        // And I enter the following laboratory orders:
        //   | Order Type     | Test Name        | Priority | Special Instructions |
        //   | Laboratory     | CBC with diff    | Routine  | None                |
        //   | Laboratory     | Troponin I       | STAT     | Serial in 6 hours   |
        //   | Laboratory     | Basic Metabolic  | Routine  | None                |
        page.getByTestId("enter-order-button").first().click();

        // And I enter the following radiology order:
        //   | Order Type     | Study Name       | Priority | Special Instructions |
        //   | Radiology      | Chest X-ray PA/LAT| STAT    | R/O pneumonia       |
        page.getByTestId("enter-order-button").first().click();

        // And I submit all orders with my electronic signature
        page.getByTestId("submit-orders-button").first().click();

        // Then the system sends electronic orders to the laboratory with details:
        //   | Order ID | Test Name     | Patient Info        | Priority | Timestamp |
        //   | LAB-001  | CBC with diff | John Martinez ED-5  | Routine  | Current   |
        //   | LAB-002  | Troponin I    | John Martinez ED-5  | STAT     | Current   |
        //   | LAB-003  | Basic Metabolic| John Martinez ED-5 | Routine  | Current   |
        var labOrdersSent = page.getByTestId("laboratory-order-sent");
        assertEquals(3, labOrdersSent.count());
        var firstLabOrderText = textOf(labOrdersSent.nth(0));
        assertMatches(firstLabOrderText, "LAB-001", false);

        // And the system sends electronic orders to radiology with details:
        //   | Order ID | Study Name    | Patient Info        | Priority | Timestamp |
        //   | RAD-001  | CXR PA/LAT    | John Martinez ED-5  | STAT     | Current   |
        var radiologyOrdersSent = page.getByTestId("radiology-order-sent");
        assertEquals(1, radiologyOrdersSent.count());

        // And specimen labels are automatically generated:
        //   | Label Type     | Content                                    |
        //   | Blood Draw     | John Martinez, DOB: 1975-08-15, ED-5     |
        //   | Test Codes     | CBC, Troponin, BMP                        |
        //   | Collection Time| STAT - Collect immediately                |
        //   | Barcode        | Patient and order identifiers             |
        var specimenLabels = page.getByTestId("specimen-label");
        assertEquals(4, specimenLabels.count());

        // And nursing tasks are added to the workflow:
        //   | Task Type           | Description                    | Priority | Due Time    |
        //   | Blood Collection    | Draw CBC, Troponin, BMP       | STAT     | Immediate   |
        //   | Patient Transport   | Transport to X-ray            | STAT     | After labs  |
        //   | Monitor Results     | Watch for critical values     | High     | Ongoing     |
        var nursingTasks = page.getByTestId("nursing-task");
        assertEquals(3, nursingTasks.count());
    }

    @Test
    @Order(2)
    @DisplayName("Enter orders with drug allergy checking")
    void enterOrdersWithDrugAllergyChecking() {
        // Given I have examined a patient "Sarah Johnson" in bed "ED-8"
        // And the patient has documented allergies:
        //   | Allergy    | Reaction Type    | Severity |
        //   | Penicillin | Rash, hives      | Moderate |
        //   | Morphine   | Respiratory depression | Severe |
        // (assumed pre-seeded test data)

        // When I attempt to enter a medication order:
        //   | Order Type | Medication  | Dose     | Route | Frequency |
        //   | Medication | Amoxicillin | 500mg    | PO    | TID       |
        page.getByTestId("enter-order-button").first().click();

        // And I submit the order
        page.getByTestId("submit-orders-button").first().click();

        // Then the system displays an allergy alert:
        waitForTestId(page, "Drug Allergy");
        assertEquals("WARNING: Patient allergic to Penicillin", getText(page, "Drug Allergy"));
        assertEquals("Moderate - Rash, hives", getText(page, "Severity"));
        assertEquals("Amoxicillin contains penicillin", getText(page, "Cross-reaction"));
        assertEquals("Consider alternative antibiotic", getText(page, "Recommendation"));

        // And the order is held pending confirmation
        var orderStatus = getText(page, "Order Status");
        assertMatches(orderStatus, "pending confirmation", true);

        // And I must either:
        //   | Action Option      | Description                                |
        //   | Override with reason| Document clinical justification          |
        //   | Cancel order       | Remove the problematic medication         |
        //   | Select alternative | Choose non-penicillin antibiotic          |
        var allergyActionOptions = page.getByTestId("allergy-action-option");
        assertEquals(3, allergyActionOptions.count());

        // And the allergy alert is logged in the patient record
        var allergyAlertLogEntry = getText(page, "Allergy Alert Log Entry");
        assertTrue(allergyAlertLogEntry.length() > 0);
    }

    @Test
    @Order(3)
    @DisplayName("Enter STAT orders during emergency situation")
    void enterSTATOrdersDuringEmergencySituation() {
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
        page.getByTestId("enter-order-button").first().click();

        // And I mark all orders as "Emergency - Life threatening"
        fillField(page, "Order Marking", "Emergency - Life threatening");

        // And I submit the orders
        page.getByTestId("submit-orders-button").first().click();

        // Then all orders are immediately transmitted with highest priority
        var orderTransmissionStatus = getText(page, "Order Transmission Status");
        assertMatches(orderTransmissionStatus, "highest priority", true);

        // And the laboratory receives orders marked "CRITICAL - TRAUMA"
        var laboratoryOrderMarking = getText(page, "Laboratory Order Marking");
        assertEquals("CRITICAL - TRAUMA", laboratoryOrderMarking);

        // And blood bank is notified to prepare emergency release protocol
        var bloodBankNotification = getText(page, "Blood Bank Notification");
        assertMatches(bloodBankNotification, "emergency release protocol", true);

        // And radiology is alerted for trauma CT protocol
        var radiologyAlert = getText(page, "Radiology Alert");
        assertMatches(radiologyAlert, "trauma CT protocol", true);

        // And nursing receives immediate action items:
        //   | Task               | Action Required           | Time Limit |
        //   | Blood Draw         | Collect trauma labs       | 5 minutes  |
        //   | IV Access          | Large bore IV x2          | Immediate  |
        //   | Patient Prep       | Prepare for CT transport  | 10 minutes |
        var nursingActionItems = page.getByTestId("nursing-action-item");
        assertEquals(3, nursingActionItems.count());

        // And all departments receive automatic status updates
        var departmentStatusUpdate = getText(page, "Department Status Update");
        assertTrue(departmentStatusUpdate.length() > 0);
    }

    @Test
    @Order(4)
    @DisplayName("Enter pediatric orders with weight-based dosing")
    void enterPediatricOrdersWithWeightBasedDosing() {
        // Given I have examined a pediatric patient "Tommy Chen" (age 5, weight 18kg) in bed "ED-PEDS-1"
        // And the patient presents with "febrile seizure"
        // (assumed pre-seeded test data)

        // When I enter pediatric medication orders:
        //   | Order Type | Medication | Dose Calculation        | Route | Frequency |
        //   | Medication | Acetaminophen| 15mg/kg (270mg)      | PO    | Q6H PRN   |
        //   | Medication | Lorazepam  | 0.1mg/kg (1.8mg)      | IV    | Once      |
        page.getByTestId("enter-order-button").first().click();

        // And I enter diagnostic orders:
        //   | Order Type | Test/Study    | Pediatric Protocol    | Priority |
        //   | Laboratory | CBC with diff | Pediatric collection  | STAT     |
        //   | Laboratory | Blood glucose | Fingerstick acceptable| STAT     |
        page.getByTestId("enter-order-button").first().click();

        // And I submit the pediatric orders
        page.getByTestId("submit-orders-button").first().click();

        // Then the system validates weight-based dosing calculations
        var dosingValidationStatus = getText(page, "Dosing Validation Status");
        assertMatches(dosingValidationStatus, "validated", true);

        // And pediatric-specific protocols are applied:
        //   | Protocol Type      | Details                                |
        //   | Collection Volume  | Minimum blood volume for pediatric labs|
        //   | Dosing Alerts      | Maximum safe dose verified             |
        //   | Administration     | Child-friendly instructions           |
        var pediatricProtocols = page.getByTestId("pediatric-protocol");
        assertEquals(3, pediatricProtocols.count());

        // And nursing receives pediatric-specific tasks:
        //   | Task Type          | Pediatric Instructions                 |
        //   | Medication Admin   | Use pediatric dosing chart            |
        //   | Blood Collection   | Minimize collection volume            |
        //   | Comfort Measures   | Parent/caregiver involvement          |
        var pediatricNursingTasks = page.getByTestId("pediatric-nursing-task");
        assertEquals(3, pediatricNursingTasks.count());

        // And pharmacy receives weight-verified dosing information
        var pharmacyDosingNotification = getText(page, "Pharmacy Dosing Notification");
        assertTrue(pharmacyDosingNotification.length() > 0);
    }

    @Test
    @Order(5)
    @DisplayName("Handle order modifications and cancellations")
    void handleOrderModificationsAndCancellations() {
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
        fillFields(page, List.of(
            Map.of("Field", "Order ID", "Value", "ORD-103"),
            Map.of("Field", "Cancellation Reason", "Value", "Patient reports morphine allergy")
        ));
        page.getByTestId("cancel-order-button").first().click();

        // And I modify order "ORD-102" to add "portable" due to patient instability
        fillFields(page, List.of(
            Map.of("Field", "Order ID", "Value", "ORD-102"),
            Map.of("Field", "Modification", "Value", "Add portable")
        ));
        page.getByTestId("modify-order-button").first().click();

        // And I add a new order for "Fentanyl 50mcg IV push"
        fillField(page, "New Order Description", "Fentanyl 50mcg IV push");
        page.getByTestId("add-order-button").first().click();

        // Then the system processes the order changes:
        //   | Action Type | Order ID | New Status    | Reason/Details              |
        //   | Cancelled   | ORD-103  | Cancelled     | Morphine allergy discovered |
        //   | Modified    | ORD-102  | Updated       | Changed to portable CXR     |
        //   | New Order   | ORD-104  | Pending       | Fentanyl 50mcg IV push     |
        var orderChanges = page.getByTestId("order-change-entry");
        assertEquals(3, orderChanges.count());
        var firstOrderChangeText = textOf(orderChanges.nth(0));
        assertMatches(firstOrderChangeText, "ORD-103", false);

        // And notifications are sent to affected departments:
        //   | Department | Notification                               |
        //   | Pharmacy   | Morphine order cancelled - allergy        |
        //   | Radiology  | CXR modified to portable study            |
        //   | Nursing    | New pain medication order available       |
        var departmentNotifications = page.getByTestId("department-notification");
        assertEquals(3, departmentNotifications.count());

        // And an audit trail is maintained for all order changes
        var auditTrail = page.getByTestId("order-change-audit-trail").first();
        assertTrue(auditTrail.isVisible());
    }

    @Test
    @Order(6)
    @DisplayName("Enter orders with insurance authorization requirements")
    void enterOrdersWithInsuranceAuthorizationRequirements() {
        // Given I have examined a patient "Robert Davis" in bed "ED-6"
        // And the patient has insurance requiring prior authorization for certain studies
        // (assumed pre-seeded test data)

        // When I enter an order for:
        //   | Order Type | Study Name | Estimated Cost | Insurance Notes        |
        //   | Radiology  | CT Abdomen | $1,200        | Requires pre-auth      |
        page.getByTestId("enter-order-button").first().click();

        // And I submit the order
        page.getByTestId("submit-orders-button").first().click();

        // Then the system checks insurance requirements:
        assertEquals("CT covered with prior authorization", getText(page, "Coverage Verification"));
        assertEquals("Prior auth required", getText(page, "Authorization Status"));
        assertEquals("Ultrasound covered without pre-auth", getText(page, "Alternative Options"));

        // And I am presented with options:
        //   | Option             | Description                            |
        //   | Submit for auth    | Send for insurance approval (delay)    |
        //   | Order alternative  | Consider ultrasound instead           |
        //   | Emergency override | Document medical necessity            |
        var insuranceOptions = page.getByTestId("insurance-option");
        assertEquals(3, insuranceOptions.count());

        // And the order status is marked "Pending Authorization"
        var orderStatus = getText(page, "Order Status");
        assertEquals("Pending Authorization", orderStatus);

        // And the patient financial counselor is notified
        var financialCounselorNotification = getText(page, "Financial Counselor Notification");
        assertTrue(financialCounselorNotification.length() > 0);
    }

    @Test
    @Order(7)
    @DisplayName("Handle order entry during system integration failures")
    void handleOrderEntryDuringSystemIntegrationFailures() {
        // Given I am entering orders for patient "Lisa Wong" in bed "ED-14"
        // And the laboratory information system is temporarily offline
        // (assumed pre-seeded test data)

        // When I enter laboratory orders:
        //   | Order Type | Test Name     | Priority |
        //   | Laboratory | Troponin      | STAT     |
        //   | Laboratory | CBC          | Routine  |
        page.getByTestId("enter-order-button").first().click();

        // And I submit the orders
        page.getByTestId("submit-orders-button").first().click();

        // Then the system displays a warning: "Lab system offline - orders will be queued"
        var warningMessage = getText(page, "Warning Message");
        assertEquals("Lab system offline - orders will be queued", warningMessage);

        // And the orders are stored locally with status "Queued for transmission"
        var orderStatus = getText(page, "Order Status");
        assertEquals("Queued for transmission", orderStatus);

        // And nursing is notified to manually coordinate with lab
        var nursingCoordinationNotification = getText(page, "Nursing Coordination Notification");
        assertTrue(nursingCoordinationNotification.length() > 0);

        // And I receive a notification when lab system connectivity is restored
        var connectivityRestoredNotification = getText(page, "Connectivity Restored Notification");
        assertTrue(connectivityRestoredNotification.length() > 0);

        // And queued orders are automatically transmitted when system is available
        var queuedOrderTransmissionStatus = getText(page, "Queued Order Transmission Status");
        assertMatches(queuedOrderTransmissionStatus, "transmitted", true);

        // And manual backup procedures are documented for critical orders
        var manualBackupProcedureDocumentation = getText(page, "Manual Backup Procedure Documentation");
        assertTrue(manualBackupProcedureDocumentation.length() > 0);
    }

    @Test
    @Order(8)
    @DisplayName("Enter complex order sets for specific protocols")
    void enterComplexOrderSetsForSpecificProtocols() {
        // Given I have examined a patient "James Thompson" in bed "ED-11"
        // And the patient presents with "suspected stroke"
        // (assumed pre-seeded test data)

        // When I select the "Acute Stroke Protocol" order set
        fillField(page, "Order Set", "Acute Stroke Protocol");

        // Then the system presents the standardized stroke workup orders:
        //   | Category   | Order Description              | Priority | Default |
        //   | Laboratory | CBC, BMP, PT/INR, PTT         | STAT     | Selected|
        //   | Laboratory | Troponin, Lipid panel         | STAT     | Selected|
        //   | Radiology  | CT Head without contrast      | STAT     | Selected|
        //   | Radiology  | CT Angiogram head/neck        | STAT     | Optional|
        //   | Medication | Aspirin 325mg                 | STAT     | Selected|
        //   | Consults   | Neurology consult             | STAT     | Selected|
        var strokeProtocolOrders = page.getByTestId("stroke-protocol-order");
        assertEquals(6, strokeProtocolOrders.count());

        // And I can modify or remove individual orders from the set
        waitForTestId(page, "Modify Order");
        waitForTestId(page, "Remove Order");

        // And I add stroke-specific timing requirements:
        //   | Order          | Time Requirement                      |
        //   | CT Head        | Within 25 minutes of arrival         |
        //   | Lab results    | Within 45 minutes of arrival         |
        //   | Neurology      | Consult within 15 minutes           |
        page.getByTestId("add-timing-requirement-button").first().click();

        // And the system tracks compliance with stroke protocol timing
        var protocolComplianceTrackingStatus = getText(page, "Protocol Compliance Tracking Status");
        assertMatches(protocolComplianceTrackingStatus, "tracks compliance", true);

        // And automatic reminders are set for time-sensitive elements
        var automaticReminders = page.getByTestId("automatic-reminder");
        assertEquals(3, automaticReminders.count());
    }
}
