// Playwright + JUnit 5 test for
// tests-with-given-when-then-features/16-discharge-follow-up.feature
// (equivalent to tests-with-playwright-javascript/16-discharge-follow-up.test.js).
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
class T16DischargeFollowUpTest {
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
        //   And the patient portal is connected and functional
        //   And the follow-up scheduling module is active
        //   And automated reminder systems are enabled
        //   And patient communication preferences are configured
        verifySystemIsOperational(page);
        login(page, "a discharge coordinator");
        // The patient portal connection, follow-up scheduling module,
        // automated reminder systems, and communication preference
        // configuration are assumed to be active backend configuration
        // already in place for this environment.

        var dischargeFollowUpNavLink = waitForTestId(page, "Nav Discharge Follow-up");
        dischargeFollowUpNavLink.click();
        waitForTestId(page, "Discharge Follow-up Panel");
    }

    @Test
    @Order(1)
    @DisplayName("Automatically schedule follow-up reminder for primary care")
    void automaticallyScheduleFollowUpReminderForPrimaryCare() {
        // Given a patient "Jennifer Martinez" has been discharged from bed "ED-8"
        // And the discharge orders include:
        //   | Follow-up Requirement | Details                                    |
        //   | Primary Care Visit    | Schedule within 3-5 days                  |
        //   | Reason for Follow-up  | UTI treatment response, medication review  |
        //   | Urgency Level         | Routine                                    |
        //   | Special Instructions  | Bring discharge paperwork and medication list |
        // And the patient has a registered primary care physician "Dr. Sarah Wilson"
        // (assumed pre-seeded test data)
        // When the discharge process is completed at 14:30
        // (assumed to have already occurred / triggered by the system)

        // Then the system automatically schedules a follow-up reminder:
        waitForTestId(page, "Initial Reminder");
        List<Map<String, String>> followUpReminders = List.of(
            Map.of("Type", "Initial Reminder", "Details", "Day 2 after discharge (in 48 hours)"),
            Map.of("Type", "Follow-up Reminder", "Details", "Day 4 after discharge if no appointment"),
            Map.of("Type", "Final Reminder", "Details", "Day 6 after discharge (urgent)"),
            Map.of("Type", "Reminder Methods", "Details", "Text, email, phone call")
        );
        for (var rowData : followUpReminders) {
            var type = rowData.get("Type");
            var details = rowData.get("Details");
            assertEquals(details, getText(page, type));
        }

        // And discharge instructions are automatically sent to the patient portal:
        List<Map<String, String>> portalContent = List.of(
            Map.of("Section", "Discharge Summary", "Content", "Complete treatment summary and diagnosis"),
            Map.of("Section", "Medication Instructions", "Content", "Prescription details and dosing schedule"),
            Map.of("Section", "Follow-up Requirements", "Content", "Primary care appointment needed in 3-5 days"),
            Map.of("Section", "Return Precautions", "Content", "When to seek emergency care"),
            Map.of("Section", "Care Instructions", "Content", "Home care guidelines and activity restrictions")
        );
        for (var rowData : portalContent) {
            var section = rowData.get("Section");
            var content = rowData.get("Content");
            assertEquals(content, getText(page, section));
        }

        // And the patient receives immediate portal notification:
        List<Map<String, String>> portalNotifications = List.of(
            Map.of("Type", "Portal Alert", "Content", "📋 New discharge instructions available"),
            Map.of("Type", "Text Message", "Content", "\"ED discharge complete. Check patient portal for instructions\""),
            Map.of("Type", "Email Notification", "Content", "Detailed discharge summary with portal link")
        );
        for (var rowData : portalNotifications) {
            var type = rowData.get("Type");
            var content = rowData.get("Content");
            assertEquals(content, getText(page, type));
        }
    }

    @Test
    @Order(2)
    @DisplayName("Handle follow-up scheduling with multiple appointment types")
    void handleFollowUpSchedulingWithMultipleAppointmentTypes() {
        // Given a patient "Robert Chen" is discharged with complex follow-up needs
        // And the discharge orders specify:
        //   | Follow-up Type        | Timeframe | Provider Type     | Priority  |
        //   | Primary Care         | 3 days    | Family Medicine   | High      |
        //   | Cardiology Consult   | 1 week    | Cardiologist      | Urgent    |
        //   | Lab Work Follow-up   | 5 days    | Lab/Primary Care  | Routine   |
        //   | Physical Therapy     | 2 weeks   | PT Specialist     | Routine   |
        // (assumed pre-seeded test data)
        // When the discharge process is completed
        // (assumed to have already occurred / triggered by the system)

        // Then the system creates multiple follow-up reminders:
        waitForTestId(page, "Primary Care Timing");
        List<Map<String, String>> reminderSchedule = List.of(
            Map.of("Type", "Primary Care", "Timing", "Schedule within 2 days"),
            Map.of("Type", "Cardiology", "Timing", "Schedule urgent consult"),
            Map.of("Type", "Lab Work", "Timing", "Schedule blood draw"),
            Map.of("Type", "Physical Therapy", "Timing", "Schedule PT evaluation")
        );
        for (var rowData : reminderSchedule) {
            var type = rowData.get("Type");
            var timing = rowData.get("Timing");
            assertEquals(timing, getText(page, type + " Timing"));
        }

        // And the patient portal receives comprehensive follow-up information:
        List<Map<String, String>> portalFollowUpInfo = List.of(
            Map.of("Section", "Appointment Dashboard", "Content", "All required follow-ups with deadlines"),
            Map.of("Section", "Provider Contacts", "Content", "Phone numbers and scheduling information"),
            Map.of("Section", "Priority Indicators", "Content", "Urgent vs routine appointment labeling"),
            Map.of("Section", "Preparation Instructions", "Content", "What to bring to each appointment")
        );
        for (var rowData : portalFollowUpInfo) {
            var section = rowData.get("Section");
            var content = rowData.get("Content");
            assertEquals(content, getText(page, section));
        }

        // And automated referrals are generated:
        List<Map<String, String>> automatedReferrals = List.of(
            Map.of("Type", "Electronic Referral", "Action", "Sent to cardiology for urgent consult"),
            Map.of("Type", "Lab Order", "Action", "Standing orders for follow-up labs"),
            Map.of("Type", "PT Referral", "Action", "Physical therapy evaluation requested")
        );
        for (var rowData : automatedReferrals) {
            var type = rowData.get("Type");
            var action = rowData.get("Action");
            assertEquals(action, getText(page, type));
        }
    }

    @Test
    @Order(3)
    @DisplayName("Send discharge instructions to patient portal with multimedia content")
    void sendDischargeInstructionsToPatientPortalWithMultimediaContent() {
        // Given a patient "Maria Santos" was treated for "wound care management"
        // And the patient requires detailed home care instructions
        // (assumed pre-seeded test data)
        // When the discharge process includes educational materials:
        //   | Education Type        | Content Provided                           |
        //   | Wound Care Video      | Step-by-step dressing change demonstration |
        //   | Medication Guide      | Interactive dosing calculator              |
        //   | Warning Signs Chart   | Visual guide for infection symptoms        |
        //   | Activity Guidelines   | Illustrated movement restrictions          |
        // And the discharge is completed
        // (assumed to have already occurred / triggered by the system)

        // Then the patient portal receives multimedia instructions:
        waitForTestId(page, "Video Instructions");
        List<Map<String, String>> multimediaInstructions = List.of(
            Map.of("Type", "Video Instructions", "Material", "Wound care demonstration (3 minutes)"),
            Map.of("Type", "Interactive Tools", "Material", "Medication reminder scheduler"),
            Map.of("Type", "Visual Guides", "Material", "Infection warning signs with photos"),
            Map.of("Type", "Progress Tracking", "Material", "Healing milestone checklist")
        );
        for (var rowData : multimediaInstructions) {
            var type = rowData.get("Type");
            var material = rowData.get("Material");
            assertEquals(material, getText(page, type));
        }

        // And the patient receives learning verification:
        List<Map<String, String>> learningVerification = List.of(
            Map.of("Method", "Video Completion", "Requirement", "Must watch wound care video fully"),
            Map.of("Method", "Knowledge Check", "Requirement", "Brief quiz on warning signs"),
            Map.of("Method", "Acknowledgment", "Requirement", "Confirm understanding of instructions")
        );
        for (var rowData : learningVerification) {
            var method = rowData.get("Method");
            var requirement = rowData.get("Requirement");
            assertEquals(requirement, getText(page, method));
        }

        // And completion tracking is recorded for quality assurance
        var completionTrackingStatus = getText(page, "Completion Tracking Status");
        assertMatches(completionTrackingStatus, "recorded", true);
    }

    @Test
    @Order(4)
    @DisplayName("Handle follow-up reminders for patients without primary care physicians")
    void handleFollowUpRemindersForPatientsWithoutPrimaryCarePhysicians() {
        // Given a patient "David Kim" is discharged
        // And the patient does not have an established primary care physician
        // And follow-up care is required within 5 days
        // (assumed pre-seeded test data)
        // When the discharge process is completed
        // (assumed to have already occurred / triggered by the system)

        // Then the system provides alternative follow-up options:
        waitForTestId(page, "Urgent Care Centers");
        List<Map<String, String>> followUpOptions = List.of(
            Map.of("Option", "Urgent Care Centers", "Details", "List of nearby facilities with hours"),
            Map.of("Option", "Hospital Clinic", "Details", "Available appointment slots"),
            Map.of("Option", "Telehealth Options", "Details", "Virtual visit scheduling information"),
            Map.of("Option", "Community Health Centers", "Details", "Low-cost provider options")
        );
        for (var rowData : followUpOptions) {
            var option = rowData.get("Option");
            var details = rowData.get("Details");
            assertEquals(details, getText(page, option));
        }

        // And enhanced reminder scheduling is activated:
        List<Map<String, String>> enhancedReminders = List.of(
            Map.of("Type", "Daily Reminders", "Frequency", "For first 3 days after discharge"),
            Map.of("Type", "Resource Assistance", "Frequency", "Links to find primary care providers"),
            Map.of("Type", "Financial Counseling", "Frequency", "Information about insurance and payment")
        );
        for (var rowData : enhancedReminders) {
            var type = rowData.get("Type");
            var frequency = rowData.get("Frequency");
            assertEquals(frequency, getText(page, type));
        }

        // And the patient portal includes provider finding tools:
        List<Map<String, String>> providerFindingTools = List.of(
            Map.of("Tool", "Provider Search", "Functionality", "Find doctors accepting new patients"),
            Map.of("Tool", "Insurance Verification", "Functionality", "Check coverage for potential providers"),
            Map.of("Tool", "Appointment Booking", "Functionality", "Direct scheduling with available providers")
        );
        for (var rowData : providerFindingTools) {
            var tool = rowData.get("Tool");
            var functionality = rowData.get("Functionality");
            assertEquals(functionality, getText(page, tool));
        }
    }

    @Test
    @Order(5)
    @DisplayName("Customize follow-up based on patient communication preferences")
    void customizeFollowUpBasedOnPatientCommunicationPreferences() {
        // Given a patient "Lisa Brown" has specified communication preferences:
        //   | Communication Method  | Preference    | Contact Information        |
        //   | Text Messages         | Preferred     | 555-123-4567              |
        //   | Email                 | Secondary     | lisa.brown@email.com      |
        //   | Phone Calls           | Emergency Only| 555-123-4567              |
        //   | Portal Notifications  | Enabled       | Username: lbrown123       |
        // And the patient is discharged with routine follow-up requirements
        // (assumed pre-seeded test data)
        // When the discharge process triggers follow-up communications
        // (assumed to have already occurred / triggered by the system)

        // Then the system respects patient communication preferences:
        waitForTestId(page, "Initial Instructions Method");
        List<Map<String, String>> communicationPreferences = List.of(
            Map.of("Type", "Initial Instructions", "Method", "Text + Portal", "Content", "Brief summary with portal link"),
            Map.of("Type", "Follow-up Reminders", "Method", "Text Message", "Content", "Appointment reminders"),
            Map.of("Type", "Urgent Notifications", "Method", "Phone Call", "Content", "Critical lab results only"),
            Map.of("Type", "Educational Content", "Method", "Portal Only", "Content", "Detailed instructions and videos")
        );
        for (var rowData : communicationPreferences) {
            var type = rowData.get("Type");
            var method = rowData.get("Method");
            var content = rowData.get("Content");
            assertEquals(method, getText(page, type + " Method"));
            assertEquals(content, getText(page, type + " Content"));
        }

        // And communication tracking records patient engagement:
        List<Map<String, String>> communicationTracking = List.of(
            Map.of("Metric", "Message Delivery", "Measurement", "Successful text delivery confirmed"),
            Map.of("Metric", "Portal Access", "Measurement", "Login timestamps and content viewed"),
            Map.of("Metric", "Engagement Level", "Measurement", "Time spent reviewing instructions")
        );
        for (var rowData : communicationTracking) {
            var metric = rowData.get("Metric");
            var measurement = rowData.get("Measurement");
            assertEquals(measurement, getText(page, metric));
        }
    }

    @Test
    @Order(6)
    @DisplayName("Handle follow-up for pediatric patients with parent/guardian coordination")
    void handleFollowUpForPediatricPatientsWithParentGuardianCoordination() {
        // Given a pediatric patient "Emma Foster" (age 6) is discharged
        // And the parent "Sarah Foster" is the primary contact
        // And follow-up includes pediatric-specific requirements:
        //   | Follow-up Type        | Pediatric Considerations                   |
        //   | Pediatrician Visit    | Growth and development check               |
        //   | Vaccination Updates   | Catch-up on missed immunizations          |
        //   | School Health Forms   | Medical clearance for return to school     |
        // (assumed pre-seeded test data)
        // When the discharge process is completed
        // (assumed to have already occurred / triggered by the system)

        // Then the system creates parent-focused follow-up communications:
        waitForTestId(page, "Parent Portal Account");
        List<Map<String, String>> parentCommunications = List.of(
            Map.of("Target", "Parent Portal Account", "Type", "Child's medical summary and instructions"),
            Map.of("Target", "School Notifications", "Type", "Medical excuse and return guidelines"),
            Map.of("Target", "Pediatrician Alert", "Type", "ED visit summary and follow-up needs")
        );
        for (var rowData : parentCommunications) {
            var target = rowData.get("Target");
            var type = rowData.get("Type");
            assertEquals(type, getText(page, target));
        }

        // And pediatric-specific reminders are scheduled:
        List<Map<String, String>> pediatricReminders = List.of(
            Map.of("Type", "Medication Reminders", "Instructions", "Weight-based dosing with schedule"),
            Map.of("Type", "Development Milestones", "Instructions", "Age-appropriate recovery expectations"),
            Map.of("Type", "School Return Criteria", "Instructions", "When child can safely return to activities")
        );
        for (var rowData : pediatricReminders) {
            var type = rowData.get("Type");
            var instructions = rowData.get("Instructions");
            assertEquals(instructions, getText(page, type));
        }

        // And child safety verification is included:
        List<Map<String, String>> childSafetyChecks = List.of(
            Map.of("Check", "Home Safety Assessment", "Requirement", "Childproofing for medication storage"),
            Map.of("Check", "Caregiver Instructions", "Requirement", "Multiple caregivers receive instructions"),
            Map.of("Check", "Emergency Contacts", "Requirement", "Updated emergency contact information")
        );
        for (var rowData : childSafetyChecks) {
            var check = rowData.get("Check");
            var requirement = rowData.get("Requirement");
            assertEquals(requirement, getText(page, check));
        }
    }

    @Test
    @Order(7)
    @DisplayName("Track follow-up compliance and patient outcomes")
    void trackFollowUpComplianceAndPatientOutcomes() {
        // Given multiple patients have been discharged with follow-up requirements
        // (assumed pre-seeded test data)
        // When follow-up reminders are sent and appointments are scheduled
        // (assumed to have already occurred / triggered by the system)

        // Then the system tracks compliance metrics:
        waitForTestId(page, "Appointment Scheduling");
        List<Map<String, String>> complianceMetrics = List.of(
            Map.of("Metric", "Appointment Scheduling", "Measurement", "% of patients who schedule within timeframe"),
            Map.of("Metric", "Appointment Attendance", "Measurement", "% of scheduled appointments kept"),
            Map.of("Metric", "Portal Engagement", "Measurement", "% of patients accessing discharge instructions"),
            Map.of("Metric", "Medication Compliance", "Measurement", "% following prescription instructions")
        );
        for (var rowData : complianceMetrics) {
            var metric = rowData.get("Metric");
            var measurement = rowData.get("Measurement");
            assertEquals(measurement, getText(page, metric));
        }

        // And outcome tracking is performed:
        List<Map<String, String>> outcomeTracking = List.of(
            Map.of("Metric", "ED Readmissions", "Method", "72-hour and 30-day return rates"),
            Map.of("Metric", "Complication Rates", "Method", "Follow-up visits for related issues"),
            Map.of("Metric", "Patient Satisfaction", "Method", "Follow-up surveys about discharge process")
        );
        for (var rowData : outcomeTracking) {
            var metric = rowData.get("Metric");
            var method = rowData.get("Method");
            assertEquals(method, getText(page, metric));
        }

        // And quality improvement reports are generated:
        List<Map<String, String>> qualityImprovementReports = List.of(
            Map.of("Report", "Follow-up Effectiveness", "Content", "Success rates by discharge diagnosis"),
            Map.of("Report", "Communication Analysis", "Content", "Best-performing reminder methods"),
            Map.of("Report", "Provider Performance", "Content", "Follow-up compliance by discharging physician")
        );
        for (var rowData : qualityImprovementReports) {
            var report = rowData.get("Report");
            var content = rowData.get("Content");
            assertEquals(content, getText(page, report));
        }
    }

    @Test
    @Order(8)
    @DisplayName("Handle follow-up complications and escalation procedures")
    void handleFollowUpComplicationsAndEscalationProcedures() {
        // Given a patient "Michael Davis" was discharged 2 days ago
        // And follow-up reminders have been sent
        // (assumed pre-seeded test data)
        // When the patient contacts the ED with worsening symptoms
        // And the patient has not yet scheduled the required follow-up appointment
        // (assumed to have already occurred / triggered by the system)

        // Then the system escalates the follow-up process:
        waitForTestId(page, "Urgent Scheduling");
        List<Map<String, String>> escalationActions = List.of(
            Map.of("Action", "Urgent Scheduling", "Details", "Same-day appointment coordination"),
            Map.of("Action", "Provider Notification", "Details", "Original discharging physician alerted"),
            Map.of("Action", "Symptom Assessment", "Details", "Nurse triage for immediate vs delayed care"),
            Map.of("Action", "Documentation Update", "Details", "Patient contact and status change recorded")
        );
        for (var rowData : escalationActions) {
            var action = rowData.get("Action");
            var details = rowData.get("Details");
            assertEquals(details, getText(page, action));
        }

        // And enhanced monitoring is activated:
        List<Map<String, String>> enhancedMonitoring = List.of(
            Map.of("Type", "Daily Check-ins", "Action", "Nurse calls patient for status updates"),
            Map.of("Type", "Expedited Referrals", "Action", "Fast-track specialist appointments"),
            Map.of("Type", "Safety Net Activation", "Action", "Ensure patient has immediate care access")
        );
        for (var rowData : enhancedMonitoring) {
            var type = rowData.get("Type");
            var action = rowData.get("Action");
            assertEquals(action, getText(page, type));
        }

        // And the care team receives comprehensive updates:
        List<Map<String, String>> careTeamUpdates = List.of(
            Map.of("Member", "Discharging Physician", "Information", "Patient contact and current status"),
            Map.of("Member", "Primary Care Provider", "Information", "Urgent need for appointment"),
            Map.of("Member", "Charge Nurse", "Information", "Potential readmission risk identified")
        );
        for (var rowData : careTeamUpdates) {
            var member = rowData.get("Member");
            var information = rowData.get("Information");
            assertEquals(information, getText(page, member));
        }
    }
}
