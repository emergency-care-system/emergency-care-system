// Playwright + JUnit 5 test for
// tests-with-given-when-then-features/22-audit-trail.feature
// (equivalent to tests-with-playwright-javascript/22-audit-trail.test.js).
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
class T22AuditTrailTest {
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
        //   And I am logged in as "Compliance Officer Martinez"
        //   And the audit logging system is active and capturing all access events
        //   And patient record access is being monitored in real-time
        //   And audit reports are available for compliance review
        verifySystemIsOperational(page);
        login(page, "Compliance Officer Martinez");
        // The remaining Background steps describe pre-seeded system state
        // (audit logging capture, real-time access monitoring, and available
        // audit reports) assumed to already be configured in the test
        // environment.
        var featureNavLink = waitForTestId(page, "Nav Audit Trail");
        featureNavLink.click();
        waitForTestId(page, "Audit Trail Panel");
    }

    @Test
    @Order(1)
    @DisplayName("Review comprehensive access log for frequently accessed patient record")
    void reviewComprehensiveAccessLogForFrequentlyAccessedPatientRecord() {
        // Given a patient "Jennifer Rodriguez" was treated in the ED on June 20, 2025
        // And her medical record number is "MRN-789456"
        // And multiple healthcare providers accessed her record during and after her visit
        // When I search for access logs for patient "Jennifer Rodriguez" (MRN-789456)
        fillField(page, "Patient Search", "Jennifer Rodriguez (MRN-789456)");
        // And I set the date range from June 20-24, 2025
        fillField(page, "Start Date", "2025-06-20");
        fillField(page, "End Date", "2025-06-24");
        // And I generate the comprehensive audit report
        page.getByTestId("generate-audit-report-button").first().click();

        // Then the system displays all users who accessed the record:
        //   | Access # | User Name          | User Role           | Department       | Access Purpose    |
        //   | 1        | Dr. Sarah Kim      | Emergency Physician | Emergency Dept   | Direct patient care|
        //   | 2        | Nurse Johnson      | Registered Nurse    | Emergency Dept   | Direct patient care|
        //   | 3        | Tech Martinez      | Lab Technician      | Laboratory       | Lab result entry  |
        //   | 4        | Dr. Chen           | Radiologist         | Radiology        | Image interpretation|
        //   | 5        | Billing Clerk Adams| Billing Specialist  | Patient Financial| Billing/coding    |
        //   | 6        | Case Mgr Wilson    | Case Manager        | Social Services  | Discharge planning|
        //   | 7        | Dr. Patel          | Cardiologist        | Cardiology       | Consultation      |
        var accessLogEntries = page.getByTestId("audit-log-entry");
        assertEquals(7, accessLogEntries.count());

        // And detailed timestamps are shown for each access:
        //   | User Name          | Login Time           | Logout Time          | Session Duration |
        //   | Dr. Sarah Kim      | 2025-06-20 14:15:22 | 2025-06-20 14:45:10 | 29 min 48 sec   |
        //   | Nurse Johnson      | 2025-06-20 14:20:15 | 2025-06-20 16:30:22 | 2 hr 10 min 7 sec|
        //   | Tech Martinez      | 2025-06-20 15:22:45 | 2025-06-20 15:25:12 | 2 min 27 sec    |
        //   | Dr. Chen           | 2025-06-20 16:10:33 | 2025-06-20 16:18:45 | 8 min 12 sec    |
        //   | Billing Clerk Adams| 2025-06-21 09:15:20 | 2025-06-21 09:22:15 | 6 min 55 sec    |
        //   | Case Mgr Wilson    | 2025-06-21 11:30:10 | 2025-06-21 11:45:33 | 15 min 23 sec   |
        //   | Dr. Patel          | 2025-06-22 10:22:18 | 2025-06-22 10:35:45 | 13 min 27 sec   |
        var accessLogTimestamps = page.getByTestId("audit-log-timestamp-entry");
        assertEquals(7, accessLogTimestamps.count());

        // And specific data elements accessed are documented:
        //   | User Name          | Data Elements Accessed                           | Actions Performed        |
        //   | Dr. Sarah Kim      | Demographics, Chief complaint, Vital signs, Assessment, Orders | View, Edit, Create    |
        //   | Nurse Johnson      | Vital signs, Medications, Allergies, Care plans | View, Edit, Document   |
        //   | Tech Martinez      | Lab orders, Lab results                         | View, Enter results    |
        //   | Dr. Chen           | Imaging orders, Radiology reports              | View, Create report    |
        //   | Billing Clerk Adams| Diagnosis codes, Procedures, Insurance info     | View only             |
        //   | Case Mgr Wilson    | Discharge plans, Insurance, Social history      | View, Edit            |
        //   | Dr. Patel          | Cardiac tests, Consultation notes              | View, Create notes     |
        var accessLogDataElements = page.getByTestId("audit-log-data-element-entry");
        assertEquals(7, accessLogDataElements.count());
    }

    @Test
    @Order(2)
    @DisplayName("Investigate suspicious access pattern for patient record")
    void investigateSuspiciousAccessPatternForPatientRecord() {
        // Given a patient "Robert Thompson" has a high-profile status
        // And there have been unusual access patterns to his record
        // And the patient was not treated in the hospital during the access period
        // When I generate a detailed audit report for "Robert Thompson" (MRN-456789)
        fillField(page, "Patient Search", "Robert Thompson (MRN-456789)");
        page.getByTestId("generate-audit-report-button").first().click();
        // And I focus on the suspicious access period from June 15-18, 2025
        fillField(page, "Start Date", "2025-06-15");
        fillField(page, "End Date", "2025-06-18");

        // Then the system identifies potentially inappropriate access:
        //   | Suspicious Activity | Details                                          |
        //   | Unauthorized User   | Dr. Williams (Orthopedics) - No treatment relationship|
        //   | Unusual Timing      | Access at 11:45 PM on June 16 (outside normal hours)|
        //   | Excessive Duration  | 45-minute session for patient not under care    |
        //   | Data Mining Pattern | Accessed multiple unrelated patient records same night|
        List<Map<String, String>> suspiciousActivity = List.of(
            Map.of("label", "Unauthorized User", "value", "Dr. Williams (Orthopedics) - No treatment relationship"),
            Map.of("label", "Unusual Timing", "value", "Access at 11:45 PM on June 16 (outside normal hours)"),
            Map.of("label", "Excessive Duration", "value", "45-minute session for patient not under care"),
            Map.of("label", "Data Mining Pattern", "value", "Accessed multiple unrelated patient records same night")
        );
        for (var rowData : suspiciousActivity) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(page, label));
        }

        // And detailed forensic information is provided:
        //   | Forensic Data       | Investigation Details                            |
        //   | IP Address          | 192.168.1.205 (Dr. Williams' office computer)  |
        //   | Workstation ID      | WS-ORTHO-03                                     |
        //   | Access Method       | Valid credentials, no badge scan               |
        //   | Previous Pattern    | First time accessing this patient               |
        //   | Concurrent Activity | Accessed 8 other unrelated patients same session|
        List<Map<String, String>> forensicData = List.of(
            Map.of("label", "IP Address", "value", "192.168.1.205 (Dr. Williams' office computer)"),
            Map.of("label", "Workstation ID", "value", "WS-ORTHO-03"),
            Map.of("label", "Access Method", "value", "Valid credentials, no badge scan"),
            Map.of("label", "Previous Pattern", "value", "First time accessing this patient"),
            Map.of("label", "Concurrent Activity", "value", "Accessed 8 other unrelated patients same session")
        );
        for (var rowData : forensicData) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(page, label));
        }

        // And compliance violation indicators are flagged:
        //   | Violation Type      | HIPAA Concern                                   |
        //   | No Treatment Relationship| No medical necessity for access              |
        //   | Excessive Access    | Viewed entire medical history unnecessarily    |
        //   | Pattern of Behavior | Multiple inappropriate accesses detected       |
        //   | Time-based Concern  | Access outside normal work hours              |
        List<Map<String, String>> violationIndicators = List.of(
            Map.of("label", "No Treatment Relationship", "value", "No medical necessity for access"),
            Map.of("label", "Excessive Access", "value", "Viewed entire medical history unnecessarily"),
            Map.of("label", "Pattern of Behavior", "value", "Multiple inappropriate accesses detected"),
            Map.of("label", "Time-based Concern", "value", "Access outside normal work hours")
        );
        for (var rowData : violationIndicators) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(page, label));
        }

        // And automatic security alerts are generated:
        //   | Alert Type          | Notification Details                            |
        //   | Privacy Officer     | Immediate alert sent for investigation          |
        //   | Department Head     | Orthopedics supervisor notified                |
        //   | IT Security         | Account flagged for enhanced monitoring         |
        //   | Risk Management     | Potential HIPAA violation logged              |
        List<Map<String, String>> securityAlerts = List.of(
            Map.of("label", "Privacy Officer", "value", "Immediate alert sent for investigation"),
            Map.of("label", "Department Head", "value", "Orthopedics supervisor notified"),
            Map.of("label", "IT Security", "value", "Account flagged for enhanced monitoring"),
            Map.of("label", "Risk Management", "value", "Potential HIPAA violation logged")
        );
        for (var rowData : securityAlerts) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(page, label));
        }
    }

    @Test
    @Order(3)
    @DisplayName("Generate audit report for break-the-glass emergency access")
    void generateAuditReportForBreakTheGlassEmergencyAccess() {
        // Given a patient "Emergency John Doe" was brought unconscious to the ED
        // And normal consent procedures could not be followed due to patient condition
        // And emergency "break-the-glass" access was used to view records
        // When I review the emergency access audit trail
        page.getByTestId("review-emergency-access-button").first().click();

        // Then the system documents the break-the-glass access:
        //   | Emergency Access    | Documentation                                   |
        //   | Access Type         | Break-the-glass emergency override             |
        //   | Medical Justification| Patient unconscious, life-threatening condition|
        //   | Authorizing Physician| Dr. Emergency Chief (Emergency Department Head) |
        //   | Access Duration     | 2 hours during critical care period           |
        //   | Override Reason     | Unable to obtain consent, medical emergency    |
        List<Map<String, String>> breakTheGlassDocumentation = List.of(
            Map.of("label", "Access Type", "value", "Break-the-glass emergency override"),
            Map.of("label", "Medical Justification", "value", "Patient unconscious, life-threatening condition"),
            Map.of("label", "Authorizing Physician", "value", "Dr. Emergency Chief (Emergency Department Head)"),
            Map.of("label", "Access Duration", "value", "2 hours during critical care period"),
            Map.of("label", "Override Reason", "value", "Unable to obtain consent, medical emergency")
        );
        for (var rowData : breakTheGlassDocumentation) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(page, label));
        }

        // And all emergency access activities are logged:
        //   | Activity            | Details                                         |
        //   | Records Accessed    | Previous ED visits, medication allergies, medical history|
        //   | Users Involved      | Dr. Sarah Kim, Nurse Johnson, Pharmacist Lee   |
        //   | Data Viewed         | Allergies, medications, past procedures         |
        //   | Clinical Decisions  | Medication choices based on allergy history    |
        //   | Patient Outcome     | Successful treatment, patient stabilized       |
        List<Map<String, String>> emergencyAccessActivities = List.of(
            Map.of("label", "Records Accessed", "value", "Previous ED visits, medication allergies, medical history"),
            Map.of("label", "Users Involved", "value", "Dr. Sarah Kim, Nurse Johnson, Pharmacist Lee"),
            Map.of("label", "Data Viewed", "value", "Allergies, medications, past procedures"),
            Map.of("label", "Clinical Decisions", "value", "Medication choices based on allergy history"),
            Map.of("label", "Patient Outcome", "value", "Successful treatment, patient stabilized")
        );
        for (var rowData : emergencyAccessActivities) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(page, label));
        }

        // And post-emergency review requirements are documented:
        //   | Review Requirement  | Compliance Action                               |
        //   | Medical Necessity   | Clinical justification documented              |
        //   | Minimum Necessary   | Only essential health information accessed     |
        //   | Patient Notification| Patient to be informed of emergency access when able|
        //   | Quality Review      | Emergency access appropriateness reviewed      |
        List<Map<String, String>> reviewRequirements = List.of(
            Map.of("label", "Medical Necessity", "value", "Clinical justification documented"),
            Map.of("label", "Minimum Necessary", "value", "Only essential health information accessed"),
            Map.of("label", "Patient Notification", "value", "Patient to be informed of emergency access when able"),
            Map.of("label", "Quality Review", "value", "Emergency access appropriateness reviewed")
        );
        for (var rowData : reviewRequirements) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(page, label));
        }
    }

    @Test
    @Order(4)
    @DisplayName("Audit trail for patient who requested access log of their own record")
    void auditTrailForPatientWhoRequestedAccessLogOfTheirOwnRecord() {
        // Given a patient "Maria Santos" has requested a copy of her access log
        // And this is her legal right under HIPAA
        // And she was treated on multiple occasions in the past year
        // When I generate a patient-facing access report for "Maria Santos" (MRN-321654)
        fillField(page, "Patient Search", "Maria Santos (MRN-321654)");
        page.getByTestId("generate-patient-access-report-button").first().click();
        // And I include the past 12 months of access activity
        fillField(page, "Reporting Period", "12 months");

        // Then the system creates a patient-appropriate access summary:
        //   | Access Summary      | Patient-Friendly Information                    |
        //   | Healthcare Providers| Names and roles of providers who accessed record|
        //   | Treatment Dates     | Dates when records were accessed for care      |
        //   | Purpose Categories  | Treatment, payment, healthcare operations       |
        //   | Administrative Access| Billing, quality assurance, regulatory compliance|
        List<Map<String, String>> accessSummary = List.of(
            Map.of("label", "Healthcare Providers", "value", "Names and roles of providers who accessed record"),
            Map.of("label", "Treatment Dates", "value", "Dates when records were accessed for care"),
            Map.of("label", "Purpose Categories", "value", "Treatment, payment, healthcare operations"),
            Map.of("label", "Administrative Access", "value", "Billing, quality assurance, regulatory compliance")
        );
        for (var rowData : accessSummary) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(page, label));
        }

        // And sensitive details are appropriately filtered:
        //   | Information Included| Patient Access Report Content                  |
        //   | Provider Names      | Dr. Sarah Kim (Emergency Medicine)             |
        //   | Access Dates        | June 20, 2025 for emergency treatment         |
        //   | General Purpose     | Direct patient care and treatment             |
        //   | Department          | Emergency Department                           |
        List<Map<String, String>> informationIncluded = List.of(
            Map.of("label", "Provider Names", "value", "Dr. Sarah Kim (Emergency Medicine)"),
            Map.of("label", "Access Dates", "value", "June 20, 2025 for emergency treatment"),
            Map.of("label", "General Purpose", "value", "Direct patient care and treatment"),
            Map.of("label", "Department", "value", "Emergency Department")
        );
        for (var rowData : informationIncluded) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(page, label));
        }

        // And technical details are excluded:
        //   | Information Excluded| Reason for Exclusion                          |
        //   | IP Addresses        | Technical data not relevant to patient        |
        //   | Workstation IDs     | Internal system identifiers                   |
        //   | Session Details     | Technical access information                  |
        //   | Investigation Data  | Law enforcement sensitive information          |
        List<Map<String, String>> informationExcluded = List.of(
            Map.of("label", "IP Addresses", "value", "Technical data not relevant to patient"),
            Map.of("label", "Workstation IDs", "value", "Internal system identifiers"),
            Map.of("label", "Session Details", "value", "Technical access information"),
            Map.of("label", "Investigation Data", "value", "Law enforcement sensitive information")
        );
        for (var rowData : informationExcluded) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(page, label));
        }

        // And the report includes patient rights information:
        //   | Patient Rights      | Information Provided                           |
        //   | Right to Restrict   | How to request access restrictions            |
        //   | Right to Complain   | How to file privacy complaints                |
        //   | Contact Information | Privacy officer contact details               |
        List<Map<String, String>> patientRights = List.of(
            Map.of("label", "Right to Restrict", "value", "How to request access restrictions"),
            Map.of("label", "Right to Complain", "value", "How to file privacy complaints"),
            Map.of("label", "Contact Information", "value", "Privacy officer contact details")
        );
        for (var rowData : patientRights) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(page, label));
        }
    }

    @Test
    @Order(5)
    @DisplayName("Monthly compliance audit report for department oversight")
    void monthlyComplianceAuditReportForDepartmentOversight() {
        // Given it is the first week of July 2025
        // And I need to generate the monthly compliance report for June
        // When I run the comprehensive monthly audit analysis
        page.getByTestId("run-monthly-audit-analysis-button").first().click();

        // Then the system provides departmental access statistics:
        //   | Access Metric       | June 2025 Statistics                           |
        //   | Total Record Access | 15,847 patient record accesses                |
        //   | Unique Users        | 156 healthcare providers                       |
        //   | Average Session     | 12 minutes 34 seconds                         |
        //   | After-hours Access  | 892 accesses (5.6% of total)                 |
        //   | Emergency Override  | 12 break-the-glass accesses                  |
        List<Map<String, String>> accessStatistics = List.of(
            Map.of("label", "Total Record Access", "value", "15,847 patient record accesses"),
            Map.of("label", "Unique Users", "value", "156 healthcare providers"),
            Map.of("label", "Average Session", "value", "12 minutes 34 seconds"),
            Map.of("label", "After-hours Access", "value", "892 accesses (5.6% of total)"),
            Map.of("label", "Emergency Override", "value", "12 break-the-glass accesses")
        );
        for (var rowData : accessStatistics) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(page, label));
        }

        // And compliance indicators are summarized:
        //   | Compliance Area     | Status                                         |
        //   | Appropriate Access  | 99.2% of accesses had documented treatment relationship|
        //   | Minimum Necessary   | 98.7% accessed only required data elements    |
        //   | Timely Documentation| 99.8% of access properly documented within 24 hours|
        //   | Unauthorized Access | 0.3% flagged for investigation (47 instances) |
        List<Map<String, String>> complianceIndicators = List.of(
            Map.of("label", "Appropriate Access", "value", "99.2% of accesses had documented treatment relationship"),
            Map.of("label", "Minimum Necessary Compliance", "value", "98.7% accessed only required data elements"),
            Map.of("label", "Timely Documentation", "value", "99.8% of access properly documented within 24 hours"),
            Map.of("label", "Unauthorized Access", "value", "0.3% flagged for investigation (47 instances)")
        );
        for (var rowData : complianceIndicators) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(page, label));
        }

        // And trends and patterns are identified:
        //   | Trend Analysis      | Findings                                       |
        //   | Access Volume       | 15% increase from May (normal seasonal pattern)|
        //   | User Compliance     | 2 users require additional HIPAA training     |
        //   | System Performance  | No audit logging failures detected            |
        //   | Policy Adherence    | 99.1% compliance with access policies         |
        List<Map<String, String>> trendsAndPatterns = List.of(
            Map.of("label", "Access Volume", "value", "15% increase from May (normal seasonal pattern)"),
            Map.of("label", "User Compliance", "value", "2 users require additional HIPAA training"),
            Map.of("label", "System Performance", "value", "No audit logging failures detected"),
            Map.of("label", "Policy Adherence", "value", "99.1% compliance with access policies")
        );
        for (var rowData : trendsAndPatterns) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(page, label));
        }

        // And recommendations for improvement are provided:
        //   | Improvement Area    | Recommendation                                 |
        //   | User Training       | Schedule refresher training for 2 staff members|
        //   | Policy Updates      | Review after-hours access procedures          |
        //   | System Enhancement  | Consider additional automated monitoring       |
        //   | Process Improvement | Streamline emergency access documentation     |
        List<Map<String, String>> improvementRecommendations = List.of(
            Map.of("label", "User Training", "value", "Schedule refresher training for 2 staff members"),
            Map.of("label", "Policy Updates", "value", "Review after-hours access procedures"),
            Map.of("label", "System Enhancement", "value", "Consider additional automated monitoring"),
            Map.of("label", "Process Improvement", "value", "Streamline emergency access documentation")
        );
        for (var rowData : improvementRecommendations) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(page, label));
        }
    }

    @Test
    @Order(6)
    @DisplayName("Investigate potential data breach with forensic audit trail")
    void investigatePotentialDataBreachWithForensicAuditTrail() {
        // Given there are concerns about a potential data security incident
        // And multiple patient records may have been inappropriately accessed
        // And law enforcement has requested detailed audit information
        // When I conduct a forensic audit investigation
        page.getByTestId("conduct-forensic-audit-button").first().click();
        // And I analyze access patterns from June 1-30, 2025
        fillField(page, "Start Date", "2025-06-01");
        fillField(page, "End Date", "2025-06-30");

        // Then the system provides comprehensive forensic data:
        //   | Forensic Element    | Investigation Data                             |
        //   | User Activity       | Detailed timeline of all user actions         |
        //   | Data Accessed       | Specific patient information viewed/modified   |
        //   | System Interactions | Every click, search, and data retrieval       |
        //   | Network Activity    | IP addresses, network connections, file transfers|
        //   | Concurrent Sessions | Multiple simultaneous access attempts         |
        List<Map<String, String>> forensicElements = List.of(
            Map.of("label", "User Activity", "value", "Detailed timeline of all user actions"),
            Map.of("label", "Data Accessed", "value", "Specific patient information viewed/modified"),
            Map.of("label", "System Interactions", "value", "Every click, search, and data retrieval"),
            Map.of("label", "Network Activity", "value", "IP addresses, network connections, file transfers"),
            Map.of("label", "Concurrent Sessions", "value", "Multiple simultaneous access attempts")
        );
        for (var rowData : forensicElements) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(page, label));
        }

        // And security indicators are analyzed:
        //   | Security Indicator  | Analysis Results                               |
        //   | Unusual Patterns    | 15 users accessed >100 records in one day    |
        //   | Off-site Access     | 23 connections from non-hospital IP addresses |
        //   | Data Export Activity| 5 instances of bulk data downloads            |
        //   | Failed Login Attempts| 247 failed logins from external IPs          |
        List<Map<String, String>> securityIndicators = List.of(
            Map.of("label", "Unusual Patterns", "value", "15 users accessed >100 records in one day"),
            Map.of("label", "Off-site Access", "value", "23 connections from non-hospital IP addresses"),
            Map.of("label", "Data Export Activity", "value", "5 instances of bulk data downloads"),
            Map.of("label", "Failed Login Attempts", "value", "247 failed logins from external IPs")
        );
        for (var rowData : securityIndicators) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(page, label));
        }

        // And evidence preservation procedures are documented:
        //   | Evidence Type       | Preservation Method                            |
        //   | Audit Logs          | Tamper-proof digital signature applied        |
        //   | System Snapshots    | Full system state captured and archived       |
        //   | User Account Data   | Complete account history preserved            |
        //   | Network Logs        | Network traffic logs secured for analysis     |
        List<Map<String, String>> evidencePreservation = List.of(
            Map.of("label", "Audit Logs", "value", "Tamper-proof digital signature applied"),
            Map.of("label", "System Snapshots", "value", "Full system state captured and archived"),
            Map.of("label", "User Account Data", "value", "Complete account history preserved"),
            Map.of("label", "Network Logs", "value", "Network traffic logs secured for analysis")
        );
        for (var rowData : evidencePreservation) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(page, label));
        }

        // And legal compliance requirements are met:
        //   | Legal Requirement   | Compliance Action                              |
        //   | Chain of Custody    | Documented evidence handling procedures        |
        //   | Data Integrity      | Cryptographic verification of audit data      |
        //   | Discovery Response  | Legal hold procedures activated               |
        //   | Regulatory Reporting| Breach notification procedures initiated      |
        List<Map<String, String>> legalCompliance = List.of(
            Map.of("label", "Chain of Custody", "value", "Documented evidence handling procedures"),
            Map.of("label", "Data Integrity", "value", "Cryptographic verification of audit data"),
            Map.of("label", "Discovery Response", "value", "Legal hold procedures activated"),
            Map.of("label", "Regulatory Reporting", "value", "Breach notification procedures initiated")
        );
        for (var rowData : legalCompliance) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(page, label));
        }
    }
}
