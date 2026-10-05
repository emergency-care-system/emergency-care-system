// Playwright + JUnit 5 test for
// tests-with-given-when-then-features/14-provider-assignment.feature
// (equivalent to tests-with-playwright-javascript/14-provider-assignment.test.js).
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
class T14ProviderAssignmentTest {
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
        //   And the provider assignment module is active
        //   And provider workload tracking is enabled
        //   And mobile notification system is functional
        verifySystemIsOperational(page);
        login(page, "a charge nurse");
        // The provider assignment module, provider workload tracking, and the
        // mobile notification system are assumed to be active/enabled backend
        // configuration already in place for this environment.

        var providerAssignmentNavLink = waitForTestId(page, "Nav Provider Assignment");
        providerAssignmentNavLink.click();
        waitForTestId(page, "Provider Assignment Panel");
    }

    @Test
    @Order(1)
    @DisplayName("Assign highest priority patient to newly available physician")
    void assignHighestPriorityPatientToNewlyAvailablePhysician() {
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
        waitForTestId(page, "Highest Priority");
        List<Map<String, String>> nextAssignmentCriteria = List.of(
            Map.of("Criteria", "Highest Priority", "Value", "Maria Santos (ESI Level 2)"),
            Map.of("Criteria", "Bed Availability", "Value", "Bed ready for immediate assignment"),
            Map.of("Criteria", "Provider Match", "Value", "Dr. Johnson available and qualified")
        );
        for (var rowData : nextAssignmentCriteria) {
            var criteria = rowData.get("Criteria");
            var value = rowData.get("Value");
            assertEquals(value, getText(page, criteria));
        }

        // And the system assigns "Maria Santos" to "Dr. Johnson"
        assertEquals("Maria Santos", getText(page, "Assigned Patient"));
        assertEquals("Dr. Johnson", getText(page, "Assigned Provider"));

        // And a notification is sent to Dr. Johnson's mobile device:
        List<Map<String, String>> mobileNotification = List.of(
            Map.of("Type", "Patient Assignment", "Content", "📱 New Patient: Maria Santos, Bed ED-12"),
            Map.of("Type", "Priority Level", "Content", "ESI Level 2 - High Priority"),
            Map.of("Type", "Chief Complaint", "Content", "Severe chest pain"),
            Map.of("Type", "Wait Time", "Content", "Patient waiting 45 minutes"),
            Map.of("Type", "Action Required", "Content", "Please proceed to ED-12")
        );
        for (var rowData : mobileNotification) {
            var type = rowData.get("Type");
            var content = rowData.get("Content");
            assertEquals(content, getText(page, type));
        }

        // And the patient status is updated to "Assigned to Dr. Johnson"
        assertEquals("Assigned to Dr. Johnson", getText(page, "Patient Status"));

        // And the queue position is updated for remaining patients
        var patientQueue = waitForTestId(page, "Patient Queue");
        assertTrue(patientQueue.isVisible());
    }

    @Test
    @Order(2)
    @DisplayName("Handle provider assignment with specialty requirements")
    void handleProviderAssignmentWithSpecialtyRequirements() {
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
        waitForTestId(page, "Dr. Patel Assigned Patient");
        List<Map<String, String>> providerMatches = List.of(
            Map.of("Provider", "Dr. Patel", "Assigned Patient", "Child Patient"),
            Map.of("Provider", "Dr. Martinez", "Assigned Patient", "Adult Patient")
        );
        for (var row : providerMatches) {
            assertEquals(row.get("Assigned Patient"), getText(page, row.get("Provider") + " Assigned Patient"));
        }

        // And specialty-specific notifications are sent:
        List<Map<String, String>> specialtyNotifications = List.of(
            Map.of("Provider", "Dr. Patel", "Content", "👶 Pediatric Patient: Age 8, ESI 2, Fever"),
            Map.of("Provider", "Dr. Martinez", "Content", "🏥 Adult Patient: Age 45, ESI 2, Chest pain")
        );
        for (var rowData : specialtyNotifications) {
            var provider = rowData.get("Provider");
            var content = rowData.get("Content");
            assertEquals(content, getText(page, provider + " Notification"));
        }

        // And the general patient remains in queue for the next available provider
        var generalPatientStatus = getText(page, "General Patient Queue Status");
        assertMatches(generalPatientStatus, "queue", true);
    }

    @Test
    @Order(3)
    @DisplayName("Prioritize critical patient over standard queue order")
    void prioritizeCriticalPatientOverStandardQueueOrder() {
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
        waitForTestId(page, "Skip Queue Order");
        List<Map<String, String>> prioritizationLogic = List.of(
            Map.of("Logic", "Skip Queue Order", "Reasoning", "ESI Level 1 takes priority over Level 3"),
            Map.of("Logic", "Critical Priority", "Reasoning", "Life-threatening condition requires immediate"),
            Map.of("Logic", "Provider Capability", "Reasoning", "Dr. Thompson qualified for trauma cases")
        );
        for (var rowData : prioritizationLogic) {
            var logic = rowData.get("Logic");
            var reasoning = rowData.get("Reasoning");
            assertEquals(reasoning, getText(page, logic));
        }

        // And "Critical Patient" is assigned to "Dr. Thompson"
        assertEquals("Critical Patient", getText(page, "Assigned Patient"));
        assertEquals("Dr. Thompson", getText(page, "Assigned Provider"));

        // And the mobile notification includes urgency indicators:
        List<Map<String, String>> urgencyIndicators = List.of(
            Map.of("Field", "Priority Alert", "Content", "🚨 CRITICAL: ESI Level 1 - Trauma"),
            Map.of("Field", "Patient Location", "Content", "ED-TRAUMA-1"),
            Map.of("Field", "Immediate Action", "Content", "Requires immediate assessment"),
            Map.of("Field", "Support Teams", "Content", "Trauma team standing by")
        );
        for (var rowData : urgencyIndicators) {
            var field = rowData.get("Field");
            var content = rowData.get("Content");
            assertEquals(content, getText(page, field));
        }
    }

    @Test
    @Order(4)
    @DisplayName("Handle provider assignment during high volume period")
    void handleProviderAssignmentDuringHighVolumePeriod() {
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
        waitForTestId(page, "Dr. Adams Assigned Patient");
        List<Map<String, String>> optimizedAssignments = List.of(
            Map.of("Provider", "Dr. Adams", "Assigned Patient", "Patient A"),
            Map.of("Provider", "Dr. Brown", "Assigned Patient", "Patient B"),
            Map.of("Provider", "Dr. Wilson", "Assigned Patient", "Patient C")
        );
        for (var row : optimizedAssignments) {
            assertEquals(row.get("Assigned Patient"), getText(page, row.get("Provider") + " Assigned Patient"));
        }

        // And coordinated notifications are sent to prevent conflicts
        var notificationCoordinationStatus = getText(page, "Notification Coordination Status");
        assertMatches(notificationCoordinationStatus, "coordinated", true);

        // And remaining patients receive updated wait time estimates
        var waitTimeUpdateStatus = getText(page, "Wait Time Update Status");
        assertMatches(waitTimeUpdateStatus, "updated", true);

        // And surge capacity protocols are activated if needed
        var surgeCapacityStatus = getText(page, "Surge Capacity Protocol Status");
        assertMatches(surgeCapacityStatus, "activated|standby", true);
    }

    @Test
    @Order(5)
    @DisplayName("Provider assignment with workload balancing")
    void providerAssignmentWithWorkloadBalancing() {
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
        waitForTestId(page, "Current Workload Decision");
        List<Map<String, String>> workloadBalancing = List.of(
            Map.of("Factor", "Current Workload", "Decision", "Favors Dr. Garcia"),
            Map.of("Factor", "Complexity Fit", "Decision", "Both qualified"),
            Map.of("Factor", "Fatigue Factor", "Decision", "Dr. Garcia preferred")
        );
        for (var rowData : workloadBalancing) {
            var factor = rowData.get("Factor");
            var decision = rowData.get("Decision");
            assertEquals(decision, getText(page, factor + " Decision"));
        }

        // And "Complex Patient" is assigned to "Dr. Garcia"
        assertEquals("Complex Patient", getText(page, "Assigned Patient"));
        assertEquals("Dr. Garcia", getText(page, "Assigned Provider"));

        // And workload metrics are updated for both providers
        var drGarciaWorkload = waitForTestId(page, "Dr. Garcia Workload");
        assertTrue(drGarciaWorkload.isVisible());
        var drFosterWorkload = waitForTestId(page, "Dr. Foster Workload");
        assertTrue(drFosterWorkload.isVisible());
    }

    @Test
    @Order(6)
    @DisplayName("Handle provider assignment with patient preferences")
    void handleProviderAssignmentWithPatientPreferences() {
        // Given a patient "VIP Patient" has requested "Dr. Johnson" if available
        // And "Dr. Johnson" and "Dr. Smith" both become available
        // And "VIP Patient" is next in the queue with ESI Level 3
        // (assumed pre-seeded test data)
        // When the system processes provider assignment
        // (assumed to have already occurred / triggered by the system)

        // Then the system considers patient preferences:
        waitForTestId(page, "Patient Request");
        List<Map<String, String>> preferenceFactors = List.of(
            Map.of("Factor", "Patient Request", "Details", "Specifically requested Dr. Johnson"),
            Map.of("Factor", "Medical Appropriateness", "Details", "Both doctors qualified for ESI Level 3"),
            Map.of("Factor", "Availability", "Details", "Dr. Johnson available and willing")
        );
        for (var rowData : preferenceFactors) {
            var factor = rowData.get("Factor");
            var details = rowData.get("Details");
            assertEquals(details, getText(page, factor));
        }

        // And "VIP Patient" is assigned to "Dr. Johnson"
        assertEquals("VIP Patient", getText(page, "Assigned Patient"));
        assertEquals("Dr. Johnson", getText(page, "Assigned Provider"));

        // And "Dr. Smith" receives the next patient in queue
        var drSmithAssignedPatient = getText(page, "Dr. Smith Assigned Patient");
        assertTrue(drSmithAssignedPatient.length() > 0);

        // And the assignment includes preference notation:
        List<Map<String, String>> preferenceNotation = List.of(
            Map.of("Field", "Assignment Reason", "Content", "Patient preference request honored"),
            Map.of("Field", "Special Notes", "Content", "VIP status - provide enhanced service")
        );
        for (var rowData : preferenceNotation) {
            var field = rowData.get("Field");
            var content = rowData.get("Content");
            assertEquals(content, getText(page, field));
        }
    }

    @Test
    @Order(7)
    @DisplayName("Provider assignment failure and backup procedures")
    void providerAssignmentFailureAndBackupProcedures() {
        // Given "Dr. Williams" becomes available
        // And the highest priority patient is "Emergency Patient" (ESI Level 1)
        // (assumed pre-seeded test data)
        // When the system attempts to send assignment notification to Dr. Williams
        // And the mobile device notification fails to deliver
        // (assumed to have already occurred / triggered by the system)

        // Then the system activates backup notification procedures:
        waitForTestId(page, "Overhead Page");
        List<Map<String, String>> backupProcedures = List.of(
            Map.of("Method", "Overhead Page", "Action", "\"Dr. Williams to ED-TRAUMA-1 immediately\""),
            Map.of("Method", "Desktop Alert", "Action", "Popup on all ED workstations"),
            Map.of("Method", "Charge Nurse Alert", "Action", "Direct notification to charge nurse"),
            Map.of("Method", "Secondary Provider", "Action", "Alert backup doctor if no response in 2 min")
        );
        for (var rowData : backupProcedures) {
            var method = rowData.get("Method");
            var action = rowData.get("Action");
            assertEquals(action, getText(page, method));
        }

        // And the system logs the notification failure for IT review
        var notificationFailureLog = getText(page, "Notification Failure Log");
        assertMatches(notificationFailureLog, "IT review", true);

        // And continues attempting mobile notification every 30 seconds
        var mobileRetryStatus = getText(page, "Mobile Notification Retry Status");
        assertMatches(mobileRetryStatus, "30 seconds", true);

        // And tracks response time for quality metrics
        var responseTimeTrackingStatus = getText(page, "Response Time Tracking Status");
        assertMatches(responseTimeTrackingStatus, "tracking|tracked", true);
    }

    @Test
    @Order(8)
    @DisplayName("Handle provider assignment during shift change")
    void handleProviderAssignmentDuringShiftChange() {
        // Given it is 19:00 during evening shift change
        // And "Dr. Day" (day shift) is completing final patients
        // And "Dr. Night" (evening shift) is beginning shift
        // And a critical patient arrives requiring immediate attention
        // (assumed pre-seeded test data)
        // When the system determines provider assignment for the critical patient
        // (assumed to have already occurred / triggered by the system)

        // Then the system considers shift transition factors:
        waitForTestId(page, "Shift Status");
        List<Map<String, String>> shiftTransitionFactors = List.of(
            Map.of("Factor", "Shift Status", "Consideration", "Dr. Day finishing, Dr. Night starting"),
            Map.of("Factor", "Continuity", "Consideration", "Assign to Dr. Night for ongoing care"),
            Map.of("Factor", "Availability", "Consideration", "Dr. Night has capacity for complex case")
        );
        for (var rowData : shiftTransitionFactors) {
            var factor = rowData.get("Factor");
            var consideration = rowData.get("Consideration");
            assertEquals(consideration, getText(page, factor));
        }

        // And the critical patient is assigned to "Dr. Night"
        assertEquals("Dr. Night", getText(page, "Assigned Provider"));

        // And shift handoff information is included in the notification:
        List<Map<String, String>> handoffInformation = List.of(
            Map.of("Component", "Shift Context", "Content", "New critical patient - evening shift start"),
            Map.of("Component", "Day Shift Status", "Content", "Dr. Day finishing last 2 patients"),
            Map.of("Component", "Support Available", "Content", "Day shift available for consultation")
        );
        for (var rowData : handoffInformation) {
            var component = rowData.get("Component");
            var content = rowData.get("Content");
            assertEquals(content, getText(page, component));
        }
    }

    @Test
    @Order(9)
    @DisplayName("Track provider response times and assignment efficiency")
    void trackProviderResponseTimesAndAssignmentEfficiency() {
        // Given provider assignment notifications are sent
        // (assumed pre-seeded test data)
        // When providers respond to patient assignments
        // (assumed to have already occurred / triggered by the system)

        // Then the system tracks performance metrics:
        waitForTestId(page, "Notification to Response");
        List<Map<String, String>> performanceMetrics = List.of(
            Map.of("Metric", "Notification to Response", "Measurement", "Time from alert to bedside presence"),
            Map.of("Metric", "Assignment Accuracy", "Measurement", "Correct provider-patient matching"),
            Map.of("Metric", "Queue Optimization", "Measurement", "Wait time reduction effectiveness")
        );
        for (var rowData : performanceMetrics) {
            var metric = rowData.get("Metric");
            var measurement = rowData.get("Measurement");
            assertEquals(measurement, getText(page, metric));
        }

        // And generates provider performance reports:
        List<Map<String, String>> performanceReports = List.of(
            Map.of("Provider", "Dr. Johnson", "Avg Response Time", "3.2 minutes", "Assignment Accuracy", "98%", "Patient Satisfaction", "4.8/5"),
            Map.of("Provider", "Dr. Smith", "Avg Response Time", "4.1 minutes", "Assignment Accuracy", "96%", "Patient Satisfaction", "4.6/5")
        );
        for (var report : performanceReports) {
            assertEquals(report.get("Avg Response Time"), getText(page, report.get("Provider") + " Avg Response Time"));
            assertEquals(report.get("Assignment Accuracy"), getText(page, report.get("Provider") + " Assignment Accuracy"));
            assertEquals(report.get("Patient Satisfaction"), getText(page, report.get("Provider") + " Patient Satisfaction"));
        }

        // And identifies optimization opportunities:
        List<Map<String, String>> optimizationOpportunities = List.of(
            Map.of("Area", "Response Time", "Recommendation", "Target <3 minutes for critical patients"),
            Map.of("Area", "Assignment Matching", "Recommendation", "Consider additional specialty training"),
            Map.of("Area", "Communication", "Recommendation", "Implement two-way acknowledgment system")
        );
        for (var rowData : optimizationOpportunities) {
            var area = rowData.get("Area");
            var recommendation = rowData.get("Recommendation");
            assertEquals(recommendation, getText(page, area));
        }
    }
}
