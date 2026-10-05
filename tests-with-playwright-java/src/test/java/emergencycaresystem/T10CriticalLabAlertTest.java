// Playwright + JUnit 5 test for
// tests-with-given-when-then-features/10-critical-lab-alert.feature
// (equivalent to tests-with-playwright-javascript/10-critical-lab-alert.test.js).
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
class T10CriticalLabAlertTest {
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
        //   And the critical value alert system is enabled
        //   And laboratory interfaces are functioning
        //   And all patient displays are connected to the alert system
        verifySystemIsOperational(page);
        // The critical value alert system, laboratory interfaces, and patient
        // display connections are assumed pre-seeded test data / environment
        // configuration. This feature has no "logged in as" step in its
        // Background.
        login(page, "a lab technician");

        var criticalLabAlertNavLink = waitForTestId(page, "Nav Critical Lab Alert");
        criticalLabAlertNavLink.click();
        waitForTestId(page, "Critical Lab Alert Panel");
    }

    @Test
    @Order(1)
    @DisplayName("Process critically high troponin result with immediate alerts")
    void processCriticallyHighTroponinResultWithImmediateAlerts() {
        // Given a patient "Robert Martinez" is in bed "ED-7"
        // And the attending physician is "Dr. Johnson"
        // And the charge nurse is "Nurse Williams"
        // And troponin was ordered for "chest pain evaluation"
        // (assumed pre-seeded test data)

        // When the laboratory result is received:
        //   | Test Name       | Result | Reference Range | Units  | Critical Threshold | Status |
        //   | Troponin I      | 5.8    | 0.0-0.04       | ng/mL  | >0.4              | Final  |
        fillFields(page, List.of(
            Map.of("Field", "Test Name", "Value", "Troponin I"),
            Map.of("Field", "Result", "Value", "5.8"),
            Map.of("Field", "Reference Range", "Value", "0.0-0.04"),
            Map.of("Field", "Units", "Value", "ng/mL"),
            Map.of("Field", "Critical Threshold", "Value", ">0.4"),
            Map.of("Field", "Status", "Value", "Final")
        ));
        page.getByTestId("receive-lab-result-button").first().click();

        // Then the system immediately triggers critical alerts
        var criticalAlertStatus = getText(page, "Critical Alert Status");
        assertMatches(criticalAlertStatus, "triggered", true);

        // And the attending physician "Dr. Johnson" receives immediate notifications:
        //   | Notification Type | Content                                        | Delivery Method |
        //   | Mobile Push Alert | 🔴 CRITICAL: Troponin I 5.8 ng/mL - ED-7     | Mobile App      |
        //   | SMS Alert        | CRITICAL LAB: R.Martinez ED-7 Troponin 5.8    | Text Message    |
        //   | Popup Alert      | CRITICAL VALUE - Requires Acknowledgment       | Workstation     |
        assertEquals("🔴 CRITICAL: Troponin I 5.8 ng/mL - ED-7", getText(page, "Mobile Push Alert"));
        assertEquals("CRITICAL LAB: R.Martinez ED-7 Troponin 5.8", getText(page, "SMS Alert"));
        assertEquals("CRITICAL VALUE - Requires Acknowledgment", getText(page, "Popup Alert"));

        // And the charge nurse "Nurse Williams" receives critical notifications:
        //   | Notification Type | Content                                        | Delivery Method |
        //   | Desktop Alert    | CRITICAL: Troponin 5.8 - Bed ED-7            | Workstation     |
        //   | Overhead Page    | Critical lab value bed ED-7                   | PA System       |
        //   | Mobile Alert     | Critical troponin result requires attention   | Mobile Device   |
        assertEquals("CRITICAL: Troponin 5.8 - Bed ED-7", getText(page, "Desktop Alert"));
        assertEquals("Critical lab value bed ED-7", getText(page, "Overhead Page"));
        assertEquals("Critical troponin result requires attention", getText(page, "Mobile Alert"));

        // And red flag indicators appear on all patient displays:
        //   | Display Location     | Alert Indicator                              |
        //   | Patient Monitor      | 🔴 CRITICAL LAB flashing red banner         |
        //   | Bedside Workstation  | Red alert icon next to patient name         |
        //   | Main ED Dashboard    | Red flag on bed ED-7 status                 |
        //   | Mobile Devices       | Red notification badge on patient chart     |
        //   | Nursing Station      | Critical value alert on patient board       |
        var redFlagIndicators = page.getByTestId("red-flag-indicator");
        assertEquals(5, redFlagIndicators.count());
    }

    @Test
    @Order(2)
    @DisplayName("Handle critical troponin with physician acknowledgment requirements")
    void handleCriticalTroponinWithPhysicianAcknowledgmentRequirements() {
        // Given a patient "Maria Santos" is in bed "ED-12"
        // And the attending physician is "Dr. Lee"
        // And a critically high troponin result of "7.2 ng/mL" is received
        // (assumed pre-seeded test data)

        // When the critical alert is triggered
        page.getByTestId("trigger-critical-alert-button").first().click();

        // Then the system requires physician acknowledgment:
        assertEquals("Must acknowledge receipt within 15 minutes", getText(page, "Initial Alert"));
        assertEquals("Must document result review", getText(page, "Clinical Review"));
        assertEquals("Must indicate next steps taken", getText(page, "Action Plan"));

        // And if "Dr. Lee" does not acknowledge within 15 minutes:
        assertEquals("Alert sent to backup physician", getText(page, "Secondary Alert"));
        assertEquals("Escalation notice to charge nurse", getText(page, "Charge Nurse Alert"));
        assertEquals("Department supervisor notified", getText(page, "Supervisor Alert"));

        // And the acknowledgment status is tracked:
        //   | Status              | Timestamp | Provider    | Action              |
        //   | Alert Sent          | 14:30:15  | System      | Initial notification|
        //   | Acknowledged        | 14:32:45  | Dr. Lee     | Acknowledged receipt|
        //   | Reviewed            | 14:35:20  | Dr. Lee     | Documented review   |
        //   | Action Taken        | 14:40:10  | Dr. Lee     | Treatment initiated |
        var acknowledgmentStatusEntries = page.getByTestId("acknowledgment-status-entry");
        assertEquals(4, acknowledgmentStatusEntries.count());
    }

    @Test
    @Order(3)
    @DisplayName("Process multiple critical values simultaneously")
    void processMultipleCriticalValuesSimultaneously() {
        // Given multiple patients have critical troponin results:
        //   | Patient Name    | Bed   | Troponin Result | Attending     | Severity  |
        //   | John Williams   | ED-3  | 3.2 ng/mL      | Dr. Adams     | High      |
        //   | Lisa Johnson    | ED-8  | 8.9 ng/mL      | Dr. Brown     | Critical  |
        //   | Mike Davis      | ED-15 | 4.1 ng/mL      | Dr. Adams     | High      |
        // (assumed pre-seeded test data)

        // When all critical results are received simultaneously
        page.getByTestId("receive-critical-results-button").first().click();

        // Then the system prioritizes alerts by severity:
        //   | Priority | Patient      | Alert Level | Notification Urgency    |
        //   | 1        | Lisa Johnson | Critical    | Immediate - All channels|
        //   | 2        | Mike Davis   | High        | Urgent - Standard alerts|
        //   | 3        | John Williams| High        | Urgent - Standard alerts|
        assertEquals("Lisa Johnson", getText(page, "Priority 1 Patient"));
        assertEquals("Critical", getText(page, "Priority 1 Alert Level"));
        assertEquals("Mike Davis", getText(page, "Priority 2 Patient"));
        assertEquals("High", getText(page, "Priority 2 Alert Level"));
        assertEquals("John Williams", getText(page, "Priority 3 Patient"));
        assertEquals("High", getText(page, "Priority 3 Alert Level"));

        // And physicians receive prioritized notifications:
        assertEquals("CRITICAL: Lisa Johnson Trop 8.9 - IMMEDIATE", getText(page, "Dr. Brown Alert Summary"));
        assertEquals("HIGH: 2 patients with elevated troponin", getText(page, "Dr. Adams Alert Summary"));

        // And the charge nurse receives a summary alert:
        assertEquals("3 critical troponin results requiring attention", getText(page, "Mass Alert"));
        assertEquals("Lisa Johnson (Critical), others (High)", getText(page, "Priority List"));

        // And all patient displays show appropriately color-coded flags
        var colorCodedFlags = page.getByTestId("color-coded-flag");
        assertEquals(3, colorCodedFlags.count());
    }

    @Test
    @Order(4)
    @DisplayName("Handle critical troponin during shift change")
    void handleCriticalTroponinDuringShiftChange() {
        // Given a patient "Catherine Brown" is in bed "ED-6"
        // And it is 19:00 during evening shift change
        // And the day shift physician "Dr. Wilson" ordered the troponin
        // And the evening shift physician "Dr. Taylor" has assumed care
        // (assumed pre-seeded test data)

        // When a critically high troponin result of "6.1 ng/mL" is received
        fillField(page, "Troponin Result", "6.1 ng/mL");
        page.getByTestId("receive-lab-result-button").first().click();

        // Then both physicians receive critical alerts:
        //   | Physician  | Alert Type    | Content                                |
        //   | Dr. Taylor | Primary Alert | CRITICAL Troponin 6.1 - Your patient  |
        //   | Dr. Wilson | Handoff Alert | FYI: Critical result on your order     |
        assertEquals("CRITICAL Troponin 6.1 - Your patient", getText(page, "Dr. Taylor Alert"));
        assertEquals("FYI: Critical result on your order", getText(page, "Dr. Wilson Alert"));

        // And the charge nurse receives handoff-specific notification:
        assertEquals("Critical result during physician handoff", getText(page, "Shift Context"));
        assertEquals("Dr. Taylor (assuming care)", getText(page, "Current MD"));
        assertEquals("Dr. Wilson (ordered test)", getText(page, "Ordering MD"));

        // And the handoff documentation is automatically updated
        var handoffDocumentation = page.getByTestId("handoff-documentation").first();
        assertTrue(handoffDocumentation.isVisible());

        // And red flags appear with shift change context indicators
        var redFlagIndicators = page.getByTestId("red-flag-indicator");
        assertTrue(redFlagIndicators.count() > 0);
    }

    @Test
    @Order(5)
    @DisplayName("Process critical troponin with additional cardiac markers")
    void processCriticalTroponinWithAdditionalCardiacMarkers() {
        // Given a patient "Steven Kim" is in bed "ED-11"
        // And multiple cardiac markers were ordered
        // (assumed pre-seeded test data)

        // When critical and related results are received:
        //   | Test Name    | Result | Reference Range | Critical | Clinical Significance |
        //   | Troponin I   | 4.7    | 0.0-0.04       | Yes      | Acute MI indicated    |
        //   | CK-MB        | 45     | 0-6.3          | Yes      | Myocardial damage     |
        //   | Myoglobin    | 280    | 25-72          | No       | Elevated but not critical|
        page.getByTestId("receive-lab-results-button").first().click();

        // Then the system groups related critical values:
        //   | Alert Category  | Content                                      |
        //   | Cardiac Panel   | Multiple critical cardiac markers            |
        //   | Primary Alert   | Troponin I: 4.7 ng/mL (CRITICAL)           |
        //   | Secondary Alert | CK-MB: 45 ng/mL (CRITICAL)                 |
        //   | Supporting Data | Myoglobin: 280 ng/mL (Elevated)            |
        assertEquals("Multiple critical cardiac markers", getText(page, "Cardiac Panel"));
        assertEquals("Troponin I: 4.7 ng/mL (CRITICAL)", getText(page, "Primary Alert"));
        assertEquals("CK-MB: 45 ng/mL (CRITICAL)", getText(page, "Secondary Alert"));
        assertEquals("Myoglobin: 280 ng/mL (Elevated)", getText(page, "Supporting Data"));

        // And enhanced clinical context is provided:
        assertEquals("Acute myocardial infarction likely", getText(page, "Clinical Indication"));
        assertEquals("Cardiology consult, STEMI protocol", getText(page, "Recommended Actions"));
        assertEquals("Treatment within 90 minutes critical", getText(page, "Time Sensitivity"));

        // And STEMI protocol alerts are automatically triggered
        waitForTestId(page, "STEMI Protocol Alert");
    }

    @Test
    @Order(6)
    @DisplayName("Handle false positive critical troponin alerts")
    void handleFalsePositiveCriticalTroponinAlerts() {
        // Given a patient "Nancy Rodriguez" is in bed "ED-4"
        // And a troponin result of "5.1 ng/mL" triggers a critical alert
        // (assumed pre-seeded test data)

        // When the laboratory calls to report a sample error
        page.getByTestId("report-sample-error-button").first().click();

        // And a corrected result shows "0.03 ng/mL" (normal)
        fillField(page, "Corrected Troponin Result", "0.03 ng/mL");
        page.getByTestId("submit-correction-button").first().click();

        // Then the system processes the correction:
        assertEquals("Original critical alert is cancelled", getText(page, "Cancel Alert"));
        assertEquals("Corrected value sent to all recipients", getText(page, "Send Correction"));
        assertEquals("Lab error documented in audit trail", getText(page, "Document Error"));

        // And correction notifications are sent:
        assertEquals("CANCELLED: Previous critical troponin alert", getText(page, "Alert Cancellation"));
        assertEquals("Troponin corrected to 0.03 ng/mL (Normal)", getText(page, "Corrected Value"));
        assertEquals("Laboratory sample contamination identified", getText(page, "Error Explanation"));

        // And red flags are removed from all patient displays
        var redFlagIndicators = page.getByTestId("red-flag-indicator");
        assertEquals(0, redFlagIndicators.count());

        // And the correction is logged for quality assurance review
        var qualityAssuranceLog = page.getByTestId("quality-assurance-log").first();
        assertTrue(qualityAssuranceLog.isVisible());
    }

    @Test
    @Order(7)
    @DisplayName("Critical troponin with patient transfer requirements")
    void criticalTroponinWithPatientTransferRequirements() {
        // Given a patient "Timothy Chang" is in bed "ED-9"
        // And a critically high troponin of "9.3 ng/mL" is received
        // And the patient requires immediate transfer to cardiac unit
        // (assumed pre-seeded test data)

        // When the critical alert is processed
        page.getByTestId("process-critical-alert-button").first().click();

        // Then transfer coordination alerts are included:
        assertEquals("Patient needs immediate cardiac unit transfer", getText(page, "Transfer Required"));
        assertEquals("CCU bed 302 available", getText(page, "Bed Availability"));
        assertEquals("Transport team ETA 10 minutes", getText(page, "Transport Time"));

        // And receiving unit notifications are sent:
        assertEquals("Incoming transfer - Critical troponin 9.3", getText(page, "CCU Alert"));
        assertEquals("Urgent consult needed - STEMI protocol", getText(page, "Cardiology Alert"));

        // And transfer documentation is automatically initiated
        var transferDocumentation = page.getByTestId("transfer-documentation").first();
        assertTrue(transferDocumentation.isVisible());

        // And critical alerts follow the patient to the receiving unit
        waitForTestId(page, "Patient Alert Handoff");
    }

    @Test
    @Order(8)
    @DisplayName("Validate critical troponin alert system functionality")
    void validateCriticalTroponinAlertSystemFunctionality() {
        // Given the critical alert system is being tested
        // (assumed pre-seeded test data)

        // When a test troponin result of "TEST-5.0 ng/mL" is processed
        fillField(page, "Test Troponin Result", "TEST-5.0 ng/mL");
        page.getByTestId("process-test-result-button").first().click();

        // Then the system validates all alert pathways:
        assertEquals("Test alert delivered successfully", getText(page, "Physician Mobile"));
        assertEquals("Test alert delivered successfully", getText(page, "Charge Nurse"));
        assertEquals("Red flags displayed correctly", getText(page, "Patient Displays"));
        assertEquals("Test alert logged with timestamp", getText(page, "Audit Trail"));

        // And test alerts are clearly marked as "SYSTEM TEST"
        var testAlertMarking = getText(page, "Test Alert Marking");
        assertEquals("SYSTEM TEST", testAlertMarking);

        // And all test alerts are automatically cleared after validation
        var testAlerts = page.getByTestId("test-alert");
        assertEquals(0, testAlerts.count());

        // And system performance metrics are recorded:
        //   | Metric           | Measurement                                  |
        //   | Alert Latency    | <30 seconds from result to notification     |
        //   | Delivery Success | 100% successful delivery to all recipients  |
        //   | Display Update   | <5 seconds to update all patient displays   |
        assertEquals("<30 seconds from result to notification", getText(page, "Alert Latency"));
        assertEquals("100% successful delivery to all recipients", getText(page, "Delivery Success"));
        assertEquals("<5 seconds to update all patient displays", getText(page, "Display Update"));
    }
}
