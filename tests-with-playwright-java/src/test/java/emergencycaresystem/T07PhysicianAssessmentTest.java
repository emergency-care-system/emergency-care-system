// Playwright + JUnit 5 test for
// tests-with-given-when-then-features/07-physician-assessment.feature
// (equivalent to tests-with-playwright-javascript/07-physician-assessment.test.js).
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
class T07PhysicianAssessmentTest {
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
        //   And I am logged in as "Dr. Smith" on the mobile app
        //   And the patient chart access module is enabled
        //   And real-time data synchronization is active
        verifySystemIsOperational(page);
        login(page, "Dr. Smith", true);
        // The patient chart access module and real-time data synchronization are
        // assumed to be pre-seeded/enabled test data.

        var featureNavLink = waitForTestId(page, "Nav Physician Assessment");
        featureNavLink.click();
        waitForTestId(page, "Physician Assessment Panel");
    }

    @Test
    @Order(1)
    @DisplayName("Access patient chart with complete nursing assessment")
    void accessPatientChartWithCompleteNursingAssessment() {
        // Given a patient "Jennifer Martinez" is assigned to bed "ED-8"
        // And the nursing assessment is completed with the following data:
        // (assumed pre-seeded test data)

        // When I open the patient's chart for "Jennifer Martinez" in bed "ED-8"
        page.getByTestId("open-patient-chart-button").first().click();

        // Then the system displays the patient summary with:
        List<Map<String, String>> patientSummaryFields = List.of(
            Map.of("Section", "Patient Identity", "Content", "Jennifer Martinez, DOB: 1975-03-15"),
            Map.of("Section", "Bed Assignment", "Content", "ED-8"),
            Map.of("Section", "Arrival Time", "Content", "14:00"),
            Map.of("Section", "Triage Notes", "Content", "ESI Level 2 - Severe chest pain, onset 2h ago")
        );
        for (var rowData : patientSummaryFields) {
            var section = rowData.get("Section");
            var content = rowData.get("Content");
            assertEquals(content, getText(page, section));
        }

        // And the vital signs section shows:
        List<Map<String, String>> vitalSigns = List.of(
            Map.of("Vital Sign", "Blood Pressure", "Value", "160/95", "Trend", "High"),
            Map.of("Vital Sign", "Heart Rate", "Value", "110", "Trend", "Elevated"),
            Map.of("Vital Sign", "Respiratory Rate", "Value", "22", "Trend", "Elevated"),
            Map.of("Vital Sign", "Temperature", "Value", "98.6°F", "Trend", "Normal"),
            Map.of("Vital Sign", "Oxygen Saturation", "Value", "94%", "Trend", "Low"),
            Map.of("Vital Sign", "Pain Score", "Value", "8/10", "Trend", "Severe")
        );
        for (var vital : vitalSigns) {
            assertEquals(vital.get("Value"), getText(page, vital.get("Vital Sign")));
            assertEquals(vital.get("Trend"), getText(page, vital.get("Vital Sign") + " Trend"));
        }

        // And the allergies section displays:
        List<Map<String, String>> allergies = List.of(
            Map.of("Allergy", "Penicillin", "Reaction Type", "Rash", "Severity", "Moderate"),
            Map.of("Allergy", "Shellfish", "Reaction Type", "Unknown", "Severity", "Unknown")
        );
        for (var allergy : allergies) {
            assertEquals(allergy.get("Reaction Type"), getText(page, allergy.get("Allergy") + " Reaction"));
            assertEquals(allergy.get("Severity"), getText(page, allergy.get("Allergy") + " Severity"));
        }

        // And the current medications section shows:
        List<Map<String, String>> medications = List.of(
            Map.of("Medication", "Metoprolol", "Dosage", "50mg", "Status", "Active"),
            Map.of("Medication", "Aspirin", "Dosage", "81mg", "Status", "Active")
        );
        for (var medication : medications) {
            assertEquals(medication.get("Dosage"), getText(page, medication.get("Medication") + " Dosage"));
            assertEquals(medication.get("Status"), getText(page, medication.get("Medication") + " Status"));
        }
    }

    @Test
    @Order(2)
    @DisplayName("Access patient chart during active treatment")
    void accessPatientChartDuringActiveTreatment() {
        // Given a patient "Michael Chen" is assigned to bed "ED-3"
        // And the patient is currently receiving active treatment
        // And recent assessments include:
        // (assumed pre-seeded test data)

        // When I open the patient's chart for "Michael Chen" in bed "ED-3"
        page.getByTestId("open-patient-chart-button").first().click();

        // Then the system displays real-time information with:
        List<Map<String, String>> summaryFields = List.of(
            Map.of("Section", "Current Status", "Content", "Active treatment in progress"),
            Map.of("Section", "Most Recent Vitals", "Content", "BP: 130/80, HR: 88, T: 100.2°F (14:15)"),
            Map.of("Section", "Active Orders", "Content", "Lab work in progress"),
            Map.of("Section", "Triage Summary", "Content", "ESI 3 - Abd pain, onset 6h ago")
        );
        for (var rowData : summaryFields) {
            var section = rowData.get("Section");
            var content = rowData.get("Content");
            assertEquals(content, getText(page, section));
        }

        // And all data includes timestamps showing data freshness
        waitForTestId(page, "Data Freshness Timestamp");

        // And any alerts or critical values are highlighted in red
        var criticalValueElement = waitForTestId(page, "Critical Value Highlight");
        assertTrue(criticalValueElement.isVisible());

        // And pending lab results show "In Progress" status with expected completion time
        var labResultStatus = getText(page, "Lab Result Status");
        assertEquals("In Progress", labResultStatus);
    }

    @Test
    @Order(3)
    @DisplayName("View patient chart with medication allergies and interactions")
    void viewPatientChartWithMedicationAllergiesAndInteractions() {
        // Given a patient "Robert Johnson" is assigned to bed "ED-12"
        // And the patient has multiple drug allergies:
        // And current medications include:
        // (assumed pre-seeded test data)

        // When I open the patient's chart for "Robert Johnson"
        page.getByTestId("open-patient-chart-button").first().click();

        // Then the allergy section prominently displays:
        List<Map<String, String>> allergyAlerts = List.of(
            Map.of("Alert Type", "Critical Alert", "Message", "SEVERE ALLERGIES: Morphine, NSAIDs"),
            Map.of("Alert Type", "Warning", "Message", "Moderate allergy: Codeine")
        );
        for (var alert : allergyAlerts) {
            assertEquals(alert.get("Message"), getText(page, alert.get("Alert Type")));
        }

        // And the medication section shows:
        List<Map<String, String>> medications = List.of(
            Map.of("Medication", "Warfarin", "Status", "Active", "Interaction Alerts", "Monitor for bleeding risk"),
            Map.of("Medication", "Metformin", "Status", "Active", "Interaction Alerts", "No interactions detected")
        );
        for (var medication : medications) {
            assertEquals(medication.get("Status"), getText(page, medication.get("Medication") + " Status"));
            assertEquals(medication.get("Interaction Alerts"), getText(page, medication.get("Medication") + " Interaction Alerts"));
        }

        // And any new medication orders will trigger allergy checking
        waitForTestId(page, "Allergy Checking Notice");

        // And interaction warnings are displayed for contraindicated drugs
        waitForTestId(page, "Interaction Warning");
    }

    @Test
    @Order(4)
    @DisplayName("Access chart for pediatric patient with age-appropriate data")
    void accessChartForPediatricPatientWithAgeAppropriateData() {
        // Given a pediatric patient "Emma Foster" (age 7) is assigned to bed "ED-PEDS-2"
        // And the nursing assessment includes pediatric-specific data:
        // (assumed pre-seeded test data)

        // When I open the pediatric patient's chart for "Emma Foster"
        page.getByTestId("open-patient-chart-button").first().click();

        // Then the system displays pediatric-specific information:
        List<Map<String, String>> pediatricFields = List.of(
            Map.of("Section", "Patient Age/Weight", "Content", "7 years old, 22 kg"),
            Map.of("Section", "Pediatric Vital Ranges", "Content", "All vitals with age-appropriate norms"),
            Map.of("Section", "Growth Percentiles", "Content", "Weight: 50th percentile"),
            Map.of("Section", "Guardian Information", "Content", "Sarah Foster (mother) - present")
        );
        for (var rowData : pediatricFields) {
            var section = rowData.get("Section");
            var content = rowData.get("Content");
            assertEquals(content, getText(page, section));
        }

        // And vital signs are displayed with pediatric normal ranges:
        List<Map<String, String>> vitalSigns = List.of(
            Map.of("Vital Sign", "Blood Pressure", "Value", "95/60", "Status", "Normal"),
            Map.of("Vital Sign", "Heart Rate", "Value", "110", "Status", "Normal"),
            Map.of("Vital Sign", "Respiratory", "Value", "24", "Status", "Normal"),
            Map.of("Vital Sign", "Temperature", "Value", "102.8°F", "Status", "Elevated")
        );
        for (var vital : vitalSigns) {
            assertEquals(vital.get("Value"), getText(page, vital.get("Vital Sign")));
            assertEquals(vital.get("Status"), getText(page, vital.get("Vital Sign") + " Status"));
        }

        // And medication dosing shows weight-based calculations
        waitForTestId(page, "Weight-Based Dosing");

        // And parental consent status is clearly indicated
        waitForTestId(page, "Parental Consent Status");
    }

    @Test
    @Order(5)
    @DisplayName("Handle incomplete nursing assessment")
    void handleIncompleteNursingAssessment() {
        // Given a patient "David Wilson" is assigned to bed "ED-6"
        // And the nursing assessment is partially completed:
        // (assumed pre-seeded test data)

        // When I open the patient's chart for "David Wilson"
        page.getByTestId("open-patient-chart-button").first().click();

        // Then the system displays available information clearly marked:
        var completedData = getText(page, "Completed Data");
        assertEquals("Triage notes, initial vitals available", completedData);

        var missingDataItems = page.getByTestId("missing-data-item");
        assertEquals(3, missingDataItems.count());
        var missingDataTexts = textsOf(missingDataItems);
        assertEquals(List.of("Allergies: Assessment in progress", "Medications: History pending", "Pain scale: Not yet assessed"), missingDataTexts);

        // And incomplete sections are highlighted with:
        List<Map<String, String>> visualIndicators = List.of(
            Map.of("Visual Indicator", "Yellow Warning", "Description", "Assessment in progress"),
            Map.of("Visual Indicator", "Refresh Timer", "Description", "Auto-refresh every 30 seconds"),
            Map.of("Visual Indicator", "Notification", "Description", "\"Assessment updating - refresh for latest\"")
        );
        for (var indicator : visualIndicators) {
            assertEquals(indicator.get("Description"), getText(page, indicator.get("Visual Indicator")));
        }

        // And I can request priority completion of missing critical data
        waitForTestId(page, "Request Priority Completion Button");
    }

    @Test
    @Order(6)
    @DisplayName("Access chart during shift change with handoff notes")
    void accessChartDuringShiftChangeWithHandoffNotes() {
        // Given a patient "Lisa Brown" is assigned to bed "ED-9"
        // And it is during the evening shift change (19:00)
        // And the day shift nurse added handoff notes:
        // (assumed pre-seeded test data)

        // When I open the patient's chart for "Lisa Brown"
        page.getByTestId("open-patient-chart-button").first().click();

        // Then the system prominently displays shift handoff information:
        List<Map<String, String>> handoffFields = List.of(
            Map.of("Handoff Section", "Clinical Summary", "Content", "Stable condition, pain controlled"),
            Map.of("Handoff Section", "Pending Tasks", "Content", "Orthopedic consult ordered - pending"),
            Map.of("Handoff Section", "Communication Log", "Content", "Family contact: Son updated 18:30"),
            Map.of("Handoff Section", "Special Needs", "Content", "Patient preference: Female staff")
        );
        for (var field : handoffFields) {
            assertEquals(field.get("Content"), getText(page, field.get("Handoff Section")));
        }

        // And the handoff notes are clearly timestamped
        waitForTestId(page, "Handoff Notes Timestamp");

        // And I can add my own physician handoff notes
        waitForTestId(page, "Add Physician Handoff Notes");

        // And the evening nurse can see both nursing and physician handoff information
        waitForTestId(page, "Combined Handoff Information");
    }

    @Test
    @Order(7)
    @DisplayName("Handle patient chart access during network connectivity issues")
    void handlePatientChartAccessDuringNetworkConnectivityIssues() {
        // Given a patient "Thomas Anderson" is assigned to bed "ED-4"
        // And the mobile app has intermittent network connectivity
        // (assumed pre-seeded test data)

        // When I attempt to open the patient's chart
        page.getByTestId("open-patient-chart-button").first().click();

        // And the network connection is temporarily unavailable
        // (simulated network condition, no direct UI action)

        // Then the system displays cached patient data with:
        List<Map<String, String>> cachedDataFields = List.of(
            Map.of("Data Type", "Basic Demographics", "Availability", "Available (cached)"),
            Map.of("Data Type", "Last Known Vitals", "Availability", "Available - last sync 16:45"),
            Map.of("Data Type", "Medication Data", "Availability", "Available (cached)"),
            Map.of("Data Type", "Recent Lab Results", "Availability", "May not be current - sync pending")
        );
        for (var field : cachedDataFields) {
            assertEquals(field.get("Availability"), getText(page, field.get("Data Type")));
        }

        // And a connectivity warning is displayed: "Limited connectivity - data may not be current"
        var connectivityWarning = getText(page, "Connectivity Warning");
        assertEquals("Limited connectivity - data may not be current", connectivityWarning);

        // And the app attempts automatic sync when connection is restored
        waitForTestId(page, "Automatic Sync Status");

        // And critical data is prioritized for sync when connectivity returns
        waitForTestId(page, "Sync Priority Notice");

        // And I can manually trigger refresh when connection improves
        waitForTestId(page, "Manual Refresh Button");
    }

    @Test
    @Order(8)
    @DisplayName("Access chart with time-sensitive alerts and notifications")
    void accessChartWithTimeSensitiveAlertsAndNotifications() {
        // Given a patient "Karen White" is assigned to bed "ED-7"
        // And the patient has time-sensitive clinical alerts:
        // (assumed pre-seeded test data)

        // When I open the patient's chart for "Karen White"
        page.getByTestId("open-patient-chart-button").first().click();

        // Then the system prominently displays active alerts:
        List<Map<String, String>> activeAlerts = List.of(
            Map.of("Alert Priority", "CRITICAL", "Alert Details", "🔴 Troponin 0.8 - Possible MI (17:15)"),
            Map.of("Alert Priority", "WARNING", "Alert Details", "🟡 Medication due - Metoprolol (17:30)"),
            Map.of("Alert Priority", "INFO", "Alert Details", "🔵 Pain reassessment overdue (17:25)")
        );
        for (var alert : activeAlerts) {
            assertEquals(alert.get("Alert Details"), getText(page, alert.get("Alert Priority")));
        }

        // And critical alerts require acknowledgment before proceeding
        waitForTestId(page, "Alert Acknowledgment");

        // And the timestamp shows how long ago each alert was generated
        waitForTestId(page, "Alert Timestamp");

        // And I can take direct action on alerts (order meds, document assessment)
        waitForTestId(page, "Alert Action Button");

        // And alert resolution is tracked and timestamped
        waitForTestId(page, "Alert Resolution Tracking");
    }
}
