// Selenium WebDriver + JUnit 5 test for
// tests-with-given-when-then-features/09-lab-result-processing.feature
// (equivalent to tests-with-selenium-javascript/09-lab-result-processing.test.js).
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
import emergencycaresystem.support.DriverFactory;
import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.WebElement;
import org.junit.jupiter.api.AfterAll;
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.MethodOrderer;
import org.junit.jupiter.api.Order;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.TestMethodOrder;

@TestMethodOrder(MethodOrderer.OrderAnnotation.class)
class T09LabResultProcessingTest {
    private static WebDriver driver;

    @BeforeAll
    static void setUpClass() {
        driver = DriverFactory.build();
    }

    @AfterAll
    static void tearDownClass() {
        driver.quit();
    }

    @BeforeEach
    void setUp() {
        // Background:
        //   Given the emergency care system is operational
        //   And the HL7 interface with the laboratory system is active
        //   And critical value alert system is enabled
        //   And physician notification system is functional
        verifySystemIsOperational(driver);
        // The HL7 interface, critical value alert system, and physician
        // notification system being active/enabled are assumed pre-seeded
        // test data / environment configuration. This feature has no
        // "logged in as" step in its Background.
        login(driver, "a lab technician");

        var labResultProcessingNavLink = waitForTestId(driver, "Nav Lab Result Processing");
        labResultProcessingNavLink.click();
        waitForTestId(driver, "Lab Result Processing Panel");
    }

    @Test
    @Order(1)
    @DisplayName("Process normal lab results via HL7 interface")
    void processNormalLabResultsViaHL7Interface() {
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
        driver.findElement(By.cssSelector("[data-testid=\"receive-lab-results-button\"]")).click();

        // Then the system updates the patient record with all results
        var patientRecordUpdateStatus = getText(driver, "Patient Record Update Status");
        assertMatches(patientRecordUpdateStatus, "updated", true);

        // And the results are marked as "Normal" in the patient chart
        var resultStatus = getText(driver, "Result Status");
        assertEquals("Normal", resultStatus);

        // And a standard notification is sent to "Dr. Smith":
        assertEquals("Normal CBC and BMP available for review", getText(driver, "Lab Results"));
        assertEquals("Jennifer Lopez, Bed ED-8", getText(driver, "Patient"));
        assertEquals("14:30", getText(driver, "Timestamp"));
        assertEquals("Standard", getText(driver, "Priority"));

        // And the results appear in the patient's timeline with normal value indicators
        var timelineEntries = driver.findElements(By.cssSelector("[data-testid=\"timeline-result-entry\"]"));
        assertTrue(timelineEntries.size() > 0);

        // And no critical value alerts are generated
        var criticalValueAlerts = driver.findElements(By.cssSelector("[data-testid=\"critical-value-alert\"]"));
        assertEquals(0, criticalValueAlerts.size());

        // And the nursing staff is notified that results are available for review
        var nursingResultsNotification = getText(driver, "Nursing Results Notification");
        assertTrue(nursingResultsNotification.length() > 0);
    }

    @Test
    @Order(2)
    @DisplayName("Process critical lab results with immediate alerts")
    void processCriticalLabResultsWithImmediateAlerts() {
        // Given a patient "Michael Davis" is in bed "ED-12"
        // And laboratory orders were placed for "Troponin, BNP, D-Dimer"
        // And the attending physician is "Dr. Johnson"
        // (assumed pre-seeded test data)

        // When the lab system sends critical results via HL7 interface:
        //   | Test Name    | Result | Reference Range | Units  | Status | Critical | Timestamp |
        //   | Troponin I   | 8.5    | 0.0-0.04       | ng/mL  | Final  | Yes      | 15:45     |
        //   | BNP          | 1200   | 0-100          | pg/mL  | Final  | Yes      | 15:45     |
        //   | D-Dimer      | 0.8    | 0.0-0.5        | mg/L   | Final  | No       | 15:45     |
        driver.findElement(By.cssSelector("[data-testid=\"receive-lab-results-button\"]")).click();

        // Then the system immediately flags critical values:
        //   | Test Name    | Critical Flag | Severity Level |
        //   | Troponin I   | CRITICAL HIGH | Severe         |
        //   | BNP          | CRITICAL HIGH | High           |
        assertEquals("CRITICAL HIGH", getText(driver, "Troponin I Critical Flag"));
        assertEquals("Severe", getText(driver, "Troponin I Severity Level"));
        assertEquals("CRITICAL HIGH", getText(driver, "BNP Critical Flag"));
        assertEquals("High", getText(driver, "BNP Severity Level"));

        // And popup notifications are displayed for all logged-in providers:
        assertEquals("🔴 CRITICAL: Troponin I = 8.5 ng/mL", getText(driver, "Critical Alert"));
        assertEquals("🟠 HIGH: BNP = 1200 pg/mL", getText(driver, "High Alert"));
        assertEquals("Michael Davis, Bed ED-12", getText(driver, "Patient Info"));

        // And an immediate notification is sent to "Dr. Johnson":
        assertEquals("CRITICAL LAB: Troponin 8.5 - Michael Davis", getText(driver, "Mobile Push"));
        assertEquals("ED-12 CRITICAL Troponin I: 8.5 ng/mL", getText(driver, "SMS Alert"));
        assertEquals("High priority popup requiring acknowledgment", getText(driver, "In-App Alert"));

        // And the charge nurse receives a critical value notification
        var chargeNurseNotification = getText(driver, "Charge Nurse Notification");
        assertTrue(chargeNurseNotification.length() > 0);

        // And the results are highlighted in red on all patient displays
        var criticalResultHighlights = driver.findElements(By.cssSelector("[data-testid=\"critical-result-highlight\"]"));
        assertTrue(criticalResultHighlights.size() > 0);

        // And an audit trail is created for the critical value communication
        var auditTrail = driver.findElement(By.cssSelector("[data-testid=\"critical-value-audit-trail\"]"));
        assertTrue(auditTrail.isDisplayed());
    }

    @Test
    @Order(3)
    @DisplayName("Handle lab results with different statuses and corrections")
    void handleLabResultsWithDifferentStatusesAndCorrections() {
        // Given a patient "Sarah Wilson" is in bed "ED-6"
        // And previous lab results were reported
        // (assumed pre-seeded test data)

        // When the lab system sends updated results via HL7 interface:
        //   | Test Name     | Result | Status     | Previous Result | Correction Reason    | Timestamp |
        //   | Hemoglobin    | 9.2    | Corrected  | 11.2           | Sample hemolysis     | 16:15     |
        //   | Glucose       | 250    | Final      | -              | -                    | 16:15     |
        //   | Pending Test  | -      | Pending    | -              | Sample reprocessing  | 16:15     |
        driver.findElement(By.cssSelector("[data-testid=\"receive-lab-results-button\"]")).click();

        // Then the system processes different result statuses:
        assertEquals("Replace previous value, maintain history", getText(driver, "Corrected"));
        assertEquals("Add new result to patient record", getText(driver, "Final"));
        assertEquals("Update status, maintain order tracking", getText(driver, "Pending"));

        // And correction notifications are sent:
        assertEquals("Lab value corrected: Hgb 11.2 → 9.2 g/dL", getText(driver, "Correction Alert"));
        assertEquals("Sample hemolysis detected", getText(driver, "Reason"));
        assertEquals("Anemia now more severe than initially reported", getText(driver, "Clinical Impact"));

        // And the attending physician "Dr. Martinez" is notified of the correction
        var physicianCorrectionNotification = getText(driver, "Physician Correction Notification");
        assertTrue(physicianCorrectionNotification.length() > 0);

        // And the original result is preserved in the audit trail
        var originalResultAuditEntry = driver.findElement(By.cssSelector("[data-testid=\"original-result-audit-entry\"]"));
        assertTrue(originalResultAuditEntry.isDisplayed());

        // And the corrected value triggers anemia protocol alerts
        waitForTestId(driver, "Anemia Protocol Alert");
    }

    @Test
    @Order(4)
    @DisplayName("Process pediatric lab results with age-specific reference ranges")
    void processPediatricLabResultsWithAgeSpecificReferenceRanges() {
        // Given a pediatric patient "Emma Foster" (age 6) is in bed "ED-PEDS-2"
        // And laboratory orders were placed for "CBC, CMP"
        // (assumed pre-seeded test data)

        // When the lab system sends pediatric results via HL7 interface:
        //   | Test Name          | Result | Adult Range    | Pediatric Range (Age 6) | Units  | Status |
        //   | White Blood Cells  | 12.5   | 4.0-10.0      | 5.0-14.5               | K/uL   | Final  |
        //   | Hemoglobin         | 11.8   | 12.0-16.0     | 11.5-13.5              | g/dL   | Final  |
        //   | Alkaline Phosphatase| 250   | 44-147        | 156-369                | U/L    | Final  |
        driver.findElement(By.cssSelector("[data-testid=\"receive-lab-results-button\"]")).click();

        // Then the system applies age-appropriate reference ranges:
        //   | Test Name          | Interpretation      | Flag        |
        //   | White Blood Cells  | Normal for age 6    | Normal      |
        //   | Hemoglobin         | Normal for age 6    | Normal      |
        //   | Alkaline Phosphatase| Normal for age 6   | Normal      |
        assertEquals("Normal for age 6", getText(driver, "White Blood Cells Interpretation"));
        assertEquals("Normal", getText(driver, "White Blood Cells Flag"));
        assertEquals("Normal for age 6", getText(driver, "Hemoglobin Interpretation"));
        assertEquals("Normal", getText(driver, "Hemoglobin Flag"));
        assertEquals("Normal for age 6", getText(driver, "Alkaline Phosphatase Interpretation"));
        assertEquals("Normal", getText(driver, "Alkaline Phosphatase Flag"));

        // And the pediatric attending "Dr. Chen" is notified with age-specific context
        var pediatricAttendingNotification = getText(driver, "Pediatric Attending Notification");
        assertTrue(pediatricAttendingNotification.length() > 0);

        // And the results display shows both adult and pediatric reference ranges
        waitForTestId(driver, "Adult Reference Range");
        waitForTestId(driver, "Pediatric Reference Range");

        // And no inappropriate critical alerts are generated for age-normal values
        var criticalValueAlerts = driver.findElements(By.cssSelector("[data-testid=\"critical-value-alert\"]"));
        assertEquals(0, criticalValueAlerts.size());
    }

    @Test
    @Order(5)
    @DisplayName("Handle lab results during physician handoff")
    void handleLabResultsDuringPhysicianHandoff() {
        // Given a patient "Robert Kim" is in bed "ED-15"
        // And the day shift physician "Dr. Adams" ordered labs at 18:00
        // And the evening shift physician "Dr. Brown" has taken over at 19:00
        // (assumed pre-seeded test data)

        // When the lab system sends results via HL7 interface at 19:30:
        //   | Test Name     | Result | Reference Range | Status | Critical |
        //   | Lipase        | 350    | 10-140         | Final  | Yes      |
        //   | Amylase       | 180    | 25-125         | Final  | No       |
        driver.findElement(By.cssSelector("[data-testid=\"receive-lab-results-button\"]")).click();

        // Then the system determines the appropriate physician to notify:
        //   | Notification Target | Rationale                                  |
        //   | Primary: Dr. Brown  | Current attending physician                |
        //   | Secondary: Dr. Adams| Ordered the tests, may need notification   |
        assertEquals("Dr. Brown", getText(driver, "Primary Notification Target"));
        assertEquals("Current attending physician", getText(driver, "Primary Notification Rationale"));
        assertEquals("Dr. Adams", getText(driver, "Secondary Notification Target"));
        assertEquals("Ordered the tests, may need notification", getText(driver, "Secondary Notification Rationale"));

        // And both physicians receive notifications with handoff context:
        assertEquals("CRITICAL: Lipase 350 - Patient from Dr. Adams", getText(driver, "Dr. Brown Notification"));
        assertEquals("FYI: Your lipase order critical - Now Dr. Brown", getText(driver, "Dr. Adams Notification"));

        // And the handoff log is updated with the critical result information
        var handoffLog = driver.findElement(By.cssSelector("[data-testid=\"handoff-log\"]"));
        assertTrue(handoffLog.isDisplayed());

        // And the charge nurse is notified of the critical value during shift change
        var chargeNurseShiftChangeNotification = getText(driver, "Charge Nurse Shift Change Notification");
        assertTrue(chargeNurseShiftChangeNotification.length() > 0);
    }

    @Test
    @Order(6)
    @DisplayName("Process lab results with technical failures and retries")
    void processLabResultsWithTechnicalFailuresAndRetries() {
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
        driver.findElement(By.cssSelector("[data-testid=\"receive-lab-results-button\"]")).click();

        // Then the system processes the delayed results
        var delayedResultProcessingStatus = getText(driver, "Delayed Result Processing Status");
        assertMatches(delayedResultProcessingStatus, "processed", true);

        // And a delay notification is included:
        assertEquals("Results delayed due to technical issues", getText(driver, "Delay Notice"));
        assertEquals("Results ready at 20:10", getText(driver, "Original Time"));
        assertEquals("Results received at 20:15", getText(driver, "Received Time"));

        // And the attending physician is notified of both the results and the delay
        var physicianDelayNotification = getText(driver, "Physician Delay Notification");
        assertTrue(physicianDelayNotification.length() > 0);

        // And system administrators are alerted to the interface failure
        var systemAdministratorAlert = getText(driver, "System Administrator Alert");
        assertTrue(systemAdministratorAlert.length() > 0);

        // And the delay is documented in the interface audit log
        var interfaceAuditLog = driver.findElement(By.cssSelector("[data-testid=\"interface-audit-log\"]"));
        assertTrue(interfaceAuditLog.isDisplayed());
    }

    @Test
    @Order(7)
    @DisplayName("Handle batch lab results processing")
    void handleBatchLabResultsProcessing() {
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
        driver.findElement(By.cssSelector("[data-testid=\"receive-lab-results-button\"]")).click();

        // Then the system processes all results simultaneously
        var batchProcessingStatus = getText(driver, "Batch Processing Status");
        assertMatches(batchProcessingStatus, "simultaneously", true);

        // And notifications are prioritized by criticality:
        //   | Priority | Patient        | Notification Type    |
        //   | 1        | Carol Martinez | Critical value alert |
        //   | 2        | Bob Thompson   | Abnormal value alert |
        //   | 3        | Alice Johnson  | Normal results       |
        assertEquals("Carol Martinez", getText(driver, "Priority 1 Patient"));
        assertEquals("Critical value alert", getText(driver, "Priority 1 Notification Type"));
        assertEquals("Bob Thompson", getText(driver, "Priority 2 Patient"));
        assertEquals("Abnormal value alert", getText(driver, "Priority 2 Notification Type"));
        assertEquals("Alice Johnson", getText(driver, "Priority 3 Patient"));
        assertEquals("Normal results", getText(driver, "Priority 3 Notification Type"));

        // And physicians receive consolidated notifications when appropriate
        var consolidatedNotification = getText(driver, "Consolidated Notification");
        assertTrue(consolidatedNotification.length() > 0);

        // And system performance metrics are maintained during batch processing
        waitForTestId(driver, "System Performance Metrics");
    }

    @Test
    @Order(8)
    @DisplayName("Process lab results with interpretation comments")
    void processLabResultsWithInterpretationComments() {
        // Given a patient "David Lee" is in bed "ED-9"
        // And complex laboratory tests were ordered
        // (assumed pre-seeded test data)

        // When the lab system sends results with pathologist interpretation:
        //   | Test Name        | Result | Reference | Interpretation                    | Timestamp |
        //   | Blood Smear      | -      | -         | Moderate anisocytosis noted      | 21:00     |
        //   | Hemoglobin A1C   | 9.2%   | <5.7%     | Consistent with poor DM control  | 21:00     |
        //   | Thyroid Function | -      | -         | Pattern suggests hyperthyroidism | 21:00     |
        driver.findElement(By.cssSelector("[data-testid=\"receive-lab-results-button\"]")).click();

        // Then the system includes interpretation comments in the patient record
        var interpretationComments = driver.findElements(By.cssSelector("[data-testid=\"interpretation-comment\"]"));
        assertEquals(3, interpretationComments.size());

        // And the attending physician receives enhanced notifications:
        assertEquals("Numeric values and reference ranges", getText(driver, "Raw Results"));
        assertEquals("Pathologist comments and clinical significance", getText(driver, "Interpretation"));
        assertEquals("Suggested follow-up or additional testing", getText(driver, "Recommendations"));

        // And interpretation comments are highlighted in the patient chart
        var highlightedInterpretationComments = driver.findElements(By.cssSelector("[data-testid=\"highlighted-interpretation-comment\"]"));
        assertTrue(highlightedInterpretationComments.size() > 0);

        // And complex results are flagged for physician review and acknowledgment
        waitForTestId(driver, "Physician Review Flag");
    }
}
