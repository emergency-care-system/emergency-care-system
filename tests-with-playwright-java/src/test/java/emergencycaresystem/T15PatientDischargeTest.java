// Playwright + JUnit 5 test for
// tests-with-given-when-then-features/15-patient-discharge.feature
// (equivalent to tests-with-playwright-javascript/15-patient-discharge.test.js).
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
class T15PatientDischargeTest {
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
        //   And I am logged in as "Dr. Johnson"
        //   And the discharge module is active
        //   And billing integration is enabled
        //   And bed management system is connected
        verifySystemIsOperational(page);
        login(page, "Dr. Johnson");
        // The discharge module, billing integration, and the bed management
        // system connection are assumed to be active backend configuration
        // already in place for this environment.

        var patientDischargeNavLink = waitForTestId(page, "Nav Patient Discharge");
        patientDischargeNavLink.click();
        waitForTestId(page, "Patient Discharge Panel");
    }

    @Test
    @Order(1)
    @DisplayName("Complete standard patient discharge with instructions")
    void completeStandardPatientDischargeWithInstructions() {
        // Given a patient "Jennifer Martinez" is in bed "ED-8"
        // And the patient has completed treatment for "urinary tract infection"
        // And all diagnostic tests and treatments are finished
        // And the patient is medically stable for discharge
        // (assumed pre-seeded test data)
        // When I enter discharge orders and instructions:
        fillFields(page, List.of(
            Map.of("Field", "Discharge Status", "Value", "Home with medications"),
            Map.of("Field", "Primary Diagnosis", "Value", "Urinary tract infection (N39.0)"),
            Map.of("Field", "Medications", "Value", "Trimethoprim-Sulfamethoxazole 800mg BID x7d"),
            Map.of("Field", "Follow-up Care", "Value", "Primary care physician in 3-5 days"),
            Map.of("Field", "Activity Level", "Value", "Regular activities as tolerated"),
            Map.of("Field", "Diet", "Value", "Regular diet, increase fluid intake"),
            Map.of("Field", "Return Precautions", "Value", "Fever >101°F, worsening symptoms, blood in urine")
        ));
        // And I submit the discharge orders
        page.getByTestId("submit-discharge-form").first().click();

        // Then the system generates comprehensive discharge paperwork:
        waitForTestId(page, "Discharge Summary");
        List<Map<String, String>> dischargePaperwork = List.of(
            Map.of("Document", "Discharge Summary", "Content", "Treatment summary, diagnosis, medications"),
            Map.of("Document", "Medication List", "Content", "Prescriptions with dosing instructions"),
            Map.of("Document", "Follow-up Instructions", "Content", "PCP appointment scheduling information"),
            Map.of("Document", "Return Precautions", "Content", "When to seek emergency care"),
            Map.of("Document", "Patient Education", "Content", "UTI prevention and care instructions")
        );
        for (var rowData : dischargePaperwork) {
            var document = rowData.get("Document");
            var content = rowData.get("Content");
            assertEquals(content, getText(page, document));
        }

        // And the system updates bed availability:
        List<Map<String, String>> bedAvailabilityUpdates = List.of(
            Map.of("Change", "Previous Status", "Details", "Occupied by Jennifer Martinez"),
            Map.of("Change", "New Status", "Details", "Needs cleaning"),
            Map.of("Change", "Availability", "Details", "Removed from available bed count"),
            Map.of("Change", "Housekeeping Alert", "Details", "Cleaning notification sent")
        );
        for (var rowData : bedAvailabilityUpdates) {
            var change = rowData.get("Change");
            var details = rowData.get("Details");
            assertEquals(details, getText(page, change));
        }

        // And billing processes are automatically triggered:
        List<Map<String, String>> billingActions = List.of(
            Map.of("Action", "Final Charges", "Details", "All services and procedures captured"),
            Map.of("Action", "Insurance Billing", "Details", "Claims prepared for submission"),
            Map.of("Action", "Patient Statement", "Details", "Financial responsibility calculated"),
            Map.of("Action", "Coding Review", "Details", "ICD-10 and CPT codes validated")
        );
        for (var rowData : billingActions) {
            var action = rowData.get("Action");
            var details = rowData.get("Details");
            assertEquals(details, getText(page, action));
        }
    }

    @Test
    @Order(2)
    @DisplayName("Discharge patient with prescription medications")
    void dischargePatientWithPrescriptionMedications() {
        // Given a patient "Robert Chen" is ready for discharge
        // And treatment required multiple medications
        // (assumed pre-seeded test data)
        // When I enter discharge orders including prescriptions:
        fillFields(page, List.of(
            Map.of("Field", "Amoxicillin Dose", "Value", "500mg"),
            Map.of("Field", "Amoxicillin Frequency", "Value", "TID"),
            Map.of("Field", "Amoxicillin Duration", "Value", "10 days"),
            Map.of("Field", "Amoxicillin Special Instructions", "Value", "Take with food"),
            Map.of("Field", "Ibuprofen Dose", "Value", "600mg"),
            Map.of("Field", "Ibuprofen Frequency", "Value", "Q6H PRN"),
            Map.of("Field", "Ibuprofen Duration", "Value", "5 days"),
            Map.of("Field", "Ibuprofen Special Instructions", "Value", "For pain only"),
            Map.of("Field", "Omeprazole Dose", "Value", "20mg"),
            Map.of("Field", "Omeprazole Frequency", "Value", "Daily"),
            Map.of("Field", "Omeprazole Duration", "Value", "14 days"),
            Map.of("Field", "Omeprazole Special Instructions", "Value", "Take before breakfast")
        ));
        // And I include medication education:
        fillFields(page, List.of(
            Map.of("Field", "Drug Interactions", "Value", "Avoid alcohol with antibiotics"),
            Map.of("Field", "Side Effects", "Value", "Watch for nausea, diarrhea, allergic reactions"),
            Map.of("Field", "Compliance", "Value", "Complete full antibiotic course")
        ));
        // And I submit the discharge
        page.getByTestId("submit-discharge-form").first().click();

        // Then the system generates medication-specific documentation:
        waitForTestId(page, "Prescription List");
        List<Map<String, String>> medicationDocumentation = List.of(
            Map.of("Document", "Prescription List", "Content", "All medications with complete instructions"),
            Map.of("Document", "Drug Information", "Content", "Side effects, interactions, precautions"),
            Map.of("Document", "Pharmacy List", "Content", "Nearby pharmacies with hours"),
            Map.of("Document", "Medication Calendar", "Content", "Dosing schedule for patient reference")
        );
        for (var rowData : medicationDocumentation) {
            var document = rowData.get("Document");
            var content = rowData.get("Content");
            assertEquals(content, getText(page, document));
        }

        // And prescriptions are electronically transmitted to patient's preferred pharmacy
        var prescriptionTransmissionStatus = getText(page, "Prescription Transmission Status");
        assertMatches(prescriptionTransmissionStatus, "transmitted", true);

        // And medication allergy checking is performed one final time
        var allergyCheckStatus = getText(page, "Medication Allergy Check Status");
        assertMatches(allergyCheckStatus, "checked|performed|cleared", true);

        // And patient receives medication counseling checklist
        var counselingChecklist = waitForTestId(page, "Medication Counseling Checklist");
        assertTrue(counselingChecklist.isVisible());
    }

    @Test
    @Order(3)
    @DisplayName("Discharge patient requiring follow-up appointments")
    void dischargePatientRequiringFollowUpAppointments() {
        // Given a patient "Maria Santos" needs specialized follow-up care
        // And the treatment was for "complex laceration repair"
        // (assumed pre-seeded test data)
        // When I enter discharge orders with follow-up requirements:
        fillFields(page, List.of(
            Map.of("Field", "Wound Check Timeframe", "Value", "2-3 days"),
            Map.of("Field", "Wound Check Specialist Required", "Value", "Primary care"),
            Map.of("Field", "Wound Check Special Instructions", "Value", "Remove sutures"),
            Map.of("Field", "Specialist Consult Timeframe", "Value", "1 week"),
            Map.of("Field", "Specialist Consult Specialist Required", "Value", "Plastic surgeon"),
            Map.of("Field", "Specialist Consult Special Instructions", "Value", "Scar management"),
            Map.of("Field", "Lab Follow-up Timeframe", "Value", "5 days"),
            Map.of("Field", "Lab Follow-up Specialist Required", "Value", "Primary care"),
            Map.of("Field", "Lab Follow-up Special Instructions", "Value", "Check CBC")
        ));
        // And I specify wound care instructions:
        fillFields(page, List.of(
            Map.of("Field", "Dressing Changes", "Value", "Change daily, keep dry for 48 hours"),
            Map.of("Field", "Cleaning Protocol", "Value", "Gentle soap and water after 48 hours"),
            Map.of("Field", "Activity Restrictions", "Value", "No heavy lifting >10 lbs for 2 weeks"),
            Map.of("Field", "Signs of Infection", "Value", "Redness, swelling, pus, fever")
        ));

        // Then the system schedules and documents follow-up care:
        waitForTestId(page, "Appointment Booking");
        List<Map<String, String>> followUpScheduling = List.of(
            Map.of("Action", "Appointment Booking", "Details", "Attempts to schedule with preferred providers"),
            Map.of("Action", "Referral Generation", "Details", "Electronic referrals to specialists"),
            Map.of("Action", "Reminder Setup", "Details", "Patient reminders for appointments")
        );
        for (var rowData : followUpScheduling) {
            var action = rowData.get("Action");
            var details = rowData.get("Details");
            assertEquals(details, getText(page, action));
        }

        // And comprehensive wound care instructions are provided
        var woundCareInstructions = waitForTestId(page, "Wound Care Instructions");
        assertTrue(woundCareInstructions.isVisible());

        // And follow-up appointment confirmations are sent to patient
        var appointmentConfirmationStatus = getText(page, "Appointment Confirmation Status");
        assertMatches(appointmentConfirmationStatus, "sent|confirmed", true);

        // And referring physician receives notification of specialist referral
        var specialistReferralNotificationStatus = getText(page, "Specialist Referral Notification Status");
        assertMatches(specialistReferralNotificationStatus, "sent|notified", true);
    }

    @Test
    @Order(4)
    @DisplayName("Handle discharge with insurance authorization requirements")
    void handleDischargeWithInsuranceAuthorizationRequirements() {
        // Given a patient "David Kim" requires expensive follow-up imaging
        // And the patient's insurance requires prior authorization
        // (assumed pre-seeded test data)
        // When I enter discharge orders including:
        fillFields(page, List.of(
            Map.of("Field", "Imaging Study", "Value", "MRI lumbar spine within 2 weeks"),
            Map.of("Field", "Estimated Cost", "Value", "$2,400"),
            Map.of("Field", "Medical Necessity", "Value", "Rule out disc herniation")
        ));
        // And I submit the discharge orders
        page.getByTestId("submit-discharge-form").first().click();

        // Then the system handles insurance requirements:
        waitForTestId(page, "Authorization Check");
        List<Map<String, String>> insuranceRequirements = List.of(
            Map.of("Process", "Authorization Check", "Action", "Prior auth required for MRI"),
            Map.of("Process", "Documentation Prep", "Action", "Clinical justification prepared"),
            Map.of("Process", "Patient Notification", "Action", "Informed of authorization process"),
            Map.of("Process", "Alternative Options", "Action", "Suggest urgent care MRI if auth denied")
        );
        for (var rowData : insuranceRequirements) {
            var process = rowData.get("Process");
            var action = rowData.get("Action");
            assertEquals(action, getText(page, process));
        }

        // And the patient receives information about:
        List<Map<String, String>> patientInformation = List.of(
            Map.of("Type", "Authorization Process", "Content", "Timeline and requirements explained"),
            Map.of("Type", "Financial Options", "Content", "Self-pay rates and payment plans"),
            Map.of("Type", "Alternative Providers", "Content", "Facilities that may not require pre-auth")
        );
        for (var rowData : patientInformation) {
            var type = rowData.get("Type");
            var content = rowData.get("Content");
            assertEquals(content, getText(page, type));
        }

        // And insurance pre-authorization request is automatically submitted
        var preAuthorizationStatus = getText(page, "Insurance Pre-Authorization Status");
        assertMatches(preAuthorizationStatus, "submitted", true);
    }

    @Test
    @Order(5)
    @DisplayName("Discharge pediatric patient with parent/guardian instructions")
    void dischargePediatricPatientWithParentGuardianInstructions() {
        // Given a pediatric patient "Emma Foster" (age 6) is ready for discharge
        // And the parent "Sarah Foster" is present
        // And treatment was for "febrile seizure"
        // (assumed pre-seeded test data)
        // When I enter pediatric discharge orders:
        fillFields(page, List.of(
            Map.of("Field", "Weight-based Medications", "Value", "Acetaminophen 10mg/kg Q6H PRN fever"),
            Map.of("Field", "Parent Education", "Value", "Fever management, seizure precautions"),
            Map.of("Field", "Activity Restrictions", "Value", "No swimming for 24 hours"),
            Map.of("Field", "School Return", "Value", "May return tomorrow if fever-free")
        ));
        // And I provide seizure-specific education:
        fillFields(page, List.of(
            Map.of("Field", "Seizure Precautions", "Value", "Keep child safe during future episodes"),
            Map.of("Field", "When to Call 911", "Value", "Seizure >5 minutes, difficulty breathing"),
            Map.of("Field", "Temperature Control", "Value", "Aggressive fever reduction strategies")
        ));

        // Then the system generates pediatric-specific discharge materials:
        waitForTestId(page, "Parent Instructions");
        List<Map<String, String>> pediatricMaterials = List.of(
            Map.of("Document", "Parent Instructions", "Content", "Age-appropriate medication dosing"),
            Map.of("Document", "Emergency Signs", "Content", "When to bring child back to ED"),
            Map.of("Document", "School Note", "Content", "Medical excuse and return instructions"),
            Map.of("Document", "Developmental Info", "Content", "Normal vs concerning behaviors post-seizure")
        );
        for (var rowData : pediatricMaterials) {
            var document = rowData.get("Document");
            var content = rowData.get("Content");
            assertEquals(content, getText(page, document));
        }

        // And parent acknowledgment is electronically captured
        var parentAcknowledgmentStatus = getText(page, "Parent Acknowledgment Status");
        assertMatches(parentAcknowledgmentStatus, "captured|recorded", true);

        // And pediatric follow-up with primary care pediatrician is scheduled
        var pediatricFollowUpStatus = getText(page, "Pediatric Follow-up Status");
        assertMatches(pediatricFollowUpStatus, "scheduled", true);

        // And school nurse receives medical summary if parent consents
        var schoolNurseNotificationStatus = getText(page, "School Nurse Notification Status");
        assertMatches(schoolNurseNotificationStatus, "sent|notified", true);
    }

    @Test
    @Order(6)
    @DisplayName("Handle discharge during shift change")
    void handleDischargeDuringShiftChange() {
        // Given a patient "Lisa Brown" is ready for discharge at 18:45
        // And shift change occurs at 19:00
        // And "Dr. Day" (day shift) is discharging the patient
        // And "Dr. Night" (evening shift) is incoming
        // (assumed pre-seeded test data)
        // When "Dr. Day" enters the discharge orders
        // And the discharge process extends past shift change
        // (assumed to have already occurred / triggered by the system)

        // Then the system manages the transition seamlessly:
        waitForTestId(page, "Discharge Ownership");
        List<Map<String, String>> transitionManagement = List.of(
            Map.of("Item", "Discharge Ownership", "Action", "Dr. Day completes discharge process"),
            Map.of("Item", "Documentation", "Action", "All discharge notes under Dr. Day's name"),
            Map.of("Item", "Follow-up Responsibility", "Action", "Any issues route to Dr. Night"),
            Map.of("Item", "Billing Attribution", "Action", "Dr. Day receives credit for discharge")
        );
        for (var rowData : transitionManagement) {
            var item = rowData.get("Item");
            var action = rowData.get("Action");
            assertEquals(action, getText(page, item));
        }

        // And both physicians receive handoff notification:
        List<Map<String, String>> handoffNotifications = List.of(
            Map.of("Physician", "Dr. Day", "Content", "Discharge completed for Lisa Brown"),
            Map.of("Physician", "Dr. Night", "Content", "Lisa Brown discharged - available for questions")
        );
        for (var rowData : handoffNotifications) {
            var physician = rowData.get("Physician");
            var content = rowData.get("Content");
            assertEquals(content, getText(page, physician + " Notification"));
        }

        // And the bed becomes available for evening shift patient flow
        var bedAvailabilityStatus = getText(page, "Bed Availability Status");
        assertMatches(bedAvailabilityStatus, "available", true);
    }

    @Test
    @Order(7)
    @DisplayName("Discharge patient against medical advice (AMA)")
    void dischargePatientAgainstMedicalAdviceAMA() {
        // Given a patient "Michael Davis" wants to leave against medical advice
        // And the patient has been informed of risks
        // And the patient has decision-making capacity
        // (assumed pre-seeded test data)
        // When I process an AMA discharge:
        fillFields(page, List.of(
            Map.of("Field", "Risk Explanation", "Value", "Documented that risks were explained"),
            Map.of("Field", "Patient Understanding", "Value", "Patient verbalized understanding of risks"),
            Map.of("Field", "Capacity Assessment", "Value", "Patient has decision-making capacity"),
            Map.of("Field", "Witness Required", "Value", "Nurse witness to AMA conversation")
        ));
        // And I enter minimal safe discharge instructions:
        fillFields(page, List.of(
            Map.of("Field", "Return Immediately", "Value", "If symptoms worsen or new symptoms develop"),
            Map.of("Field", "Follow-up Care", "Value", "Strong recommendation for PCP visit"),
            Map.of("Field", "Medication Safety", "Value", "Critical medications must be continued")
        ));

        // Then the system generates AMA-specific documentation:
        waitForTestId(page, "AMA Form");
        List<Map<String, String>> amaDocumentation = List.of(
            Map.of("Document", "AMA Form", "Content", "Legal documentation of patient choice"),
            Map.of("Document", "Risk Documentation", "Content", "Medical risks of leaving explained"),
            Map.of("Document", "Witness Signatures", "Content", "Patient, physician, and nurse signatures"),
            Map.of("Document", "Limited Liability", "Content", "Hospital liability limitations documented")
        );
        for (var rowData : amaDocumentation) {
            var document = rowData.get("Document");
            var content = rowData.get("Content");
            assertEquals(content, getText(page, document));
        }

        // And billing processes reflect AMA status
        var billingAmaStatus = getText(page, "Billing AMA Status");
        assertMatches(billingAmaStatus, "AMA", true);

        // And legal risk management is notified of AMA discharge
        var legalRiskNotificationStatus = getText(page, "Legal Risk Management Notification Status");
        assertMatches(legalRiskNotificationStatus, "notified", true);

        // And patient still receives basic safety instructions
        var basicSafetyInstructions = waitForTestId(page, "Basic Safety Instructions");
        assertTrue(basicSafetyInstructions.isVisible());
    }

    @Test
    @Order(8)
    @DisplayName("Batch discharge processing during high volume")
    void batchDischargeProcessingDuringHighVolume() {
        // Given multiple patients are ready for simultaneous discharge:
        //   | Patient Name    | Bed    | Diagnosis        | Discharge Type     |
        //   | Patient A       | ED-3   | Minor injury     | Home              |
        //   | Patient B       | ED-7   | Gastroenteritis  | Home with meds    |
        //   | Patient C       | ED-11  | Anxiety          | Home with referral |
        // (assumed pre-seeded test data)
        // When I process multiple discharges efficiently
        // (assumed to have already occurred / triggered by the system)

        // Then the system handles batch processing:
        waitForTestId(page, "Template Usage");
        List<Map<String, String>> batchProcessing = List.of(
            Map.of("Feature", "Template Usage", "Functionality", "Common discharge templates applied"),
            Map.of("Feature", "Automated Documentation", "Functionality", "Standard instructions auto-populated"),
            Map.of("Feature", "Concurrent Processing", "Functionality", "Multiple discharges processed simultaneously")
        );
        for (var rowData : batchProcessing) {
            var feature = rowData.get("Feature");
            var functionality = rowData.get("Functionality");
            assertEquals(functionality, getText(page, feature));
        }

        // And all bed updates occur simultaneously:
        List<Map<String, String>> bedUpdates = List.of(
            Map.of("Management", "Status Updates", "Action", "All beds marked \"needs cleaning\""),
            Map.of("Management", "Housekeeping Batch", "Action", "Single notification for multiple rooms"),
            Map.of("Management", "Availability Count", "Action", "Bed count updated after all discharges")
        );
        for (var rowData : bedUpdates) {
            var management = rowData.get("Management");
            var action = rowData.get("Action");
            assertEquals(action, getText(page, management));
        }

        // And billing processes are optimized for batch handling
        var batchBillingStatus = getText(page, "Batch Billing Status");
        assertMatches(batchBillingStatus, "optimized", true);
    }

    @Test
    @Order(9)
    @DisplayName("Track discharge metrics and quality indicators")
    void trackDischargeMetricsAndQualityIndicators() {
        // Given patient discharges are being processed
        // (assumed pre-seeded test data)
        // When discharge orders are completed
        // (assumed to have already occurred / triggered by the system)

        // Then the system tracks key performance indicators:
        waitForTestId(page, "Discharge Time");
        List<Map<String, String>> performanceIndicators = List.of(
            Map.of("Metric", "Discharge Time", "Measurement", "Order entry to patient departure"),
            Map.of("Metric", "Readmission Rate", "Measurement", "72-hour return rate tracking"),
            Map.of("Metric", "Instruction Quality", "Measurement", "Patient understanding verification"),
            Map.of("Metric", "Follow-up Compliance", "Measurement", "Scheduled appointment attendance")
        );
        for (var rowData : performanceIndicators) {
            var metric = rowData.get("Metric");
            var measurement = rowData.get("Measurement");
            assertEquals(measurement, getText(page, metric));
        }

        // And generates quality reports:
        List<Map<String, String>> qualityReports = List.of(
            Map.of("Report", "Provider Performance", "Content", "Discharge efficiency by physician"),
            Map.of("Report", "Patient Satisfaction", "Content", "Discharge process satisfaction scores"),
            Map.of("Report", "Readmission Analysis", "Content", "Patterns in early returns")
        );
        for (var rowData : qualityReports) {
            var report = rowData.get("Report");
            var content = rowData.get("Content");
            assertEquals(content, getText(page, report));
        }

        // And identifies improvement opportunities:
        List<Map<String, String>> improvementOpportunities = List.of(
            Map.of("Area", "Process Efficiency", "Recommendation", "Streamline documentation workflows"),
            Map.of("Area", "Patient Education", "Recommendation", "Enhance instruction clarity"),
            Map.of("Area", "Follow-up Coordination", "Recommendation", "Improve appointment scheduling system")
        );
        for (var rowData : improvementOpportunities) {
            var area = rowData.get("Area");
            var recommendation = rowData.get("Recommendation");
            assertEquals(recommendation, getText(page, area));
        }
    }
}
