// Playwright + JUnit 5 test for
// tests-with-given-when-then-features/11-medication-administration.feature
// (equivalent to tests-with-playwright-javascript/11-medication-administration.test.js).
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
class T11MedicationAdministrationTest {
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
        //   And I am logged in as "Nurse Johnson"
        //   And the medication administration module is active
        //   And the barcode scanning system is functional
        //   And the medication administration record (MAR) is accessible
        verifySystemIsOperational(page);
        login(page, "Nurse Johnson");
        // The medication administration module, barcode scanning system, and
        // MAR accessibility are assumed to be pre-seeded test environment state.

        var medicationAdministrationNavLink = waitForTestId(page, "Nav Medication Administration");
        medicationAdministrationNavLink.click();
        waitForTestId(page, "Medication Administration Panel");
    }

    @Test
    @Order(1)
    @DisplayName("Successfully administer scheduled medication with barcode verification")
    void successfullyAdministerScheduledMedicationWithBarcodeVerification() {
        // Given a patient "Maria Gonzalez" is in bed "ED-8"
        // And the patient has a prescribed medication:
        //   | Medication    | Dose      | Route | Frequency | Scheduled Time | Status    |
        //   | Metoprolol    | 25mg      | PO    | BID       | 14:00         | Due       |
        // And the patient is wearing a wristband with barcode "PT-12345"
        // And I have the medication with barcode "MED-METOPROLOL-25MG"

        // When I scan the patient's wristband barcode "PT-12345"
        page.getByTestId("scan-wristband-barcode").first().click();
        // And I scan the medication barcode "MED-METOPROLOL-25MG"
        page.getByTestId("scan-medication-barcode").first().click();

        // Then the system verifies the five rights of medication administration:
        //   | Right          | Verification                                  | Status   |
        //   | Right Patient  | Maria Gonzalez confirmed via wristband       | ✓ Valid  |
        //   | Right Drug     | Metoprolol matches prescribed medication     | ✓ Valid  |
        //   | Right Dose     | 25mg matches prescribed dose                 | ✓ Valid  |
        //   | Right Route    | PO (oral) matches prescribed route           | ✓ Valid  |
        //   | Right Time     | Within acceptable window (13:30-14:30)      | ✓ Valid  |
        waitForTestId(page, "Right Patient Status");
        assertEquals("✓ Valid", getText(page, "Right Patient Status"));
        assertEquals("✓ Valid", getText(page, "Right Drug Status"));
        assertEquals("✓ Valid", getText(page, "Right Dose Status"));
        assertEquals("✓ Valid", getText(page, "Right Route Status"));
        assertEquals("✓ Valid", getText(page, "Right Time Status"));

        // And the system displays confirmation screen:
        //   | Field             | Value                                        |
        //   | Patient           | Maria Gonzalez, DOB: 1975-08-15            |
        //   | Medication        | Metoprolol 25mg                            |
        //   | Route             | PO (By mouth)                              |
        //   | Administration Time| 14:05                                      |
        assertEquals("Maria Gonzalez, DOB: 1975-08-15", getText(page, "Patient"));
        assertEquals("Metoprolol 25mg", getText(page, "Medication"));
        assertEquals("PO (By mouth)", getText(page, "Route"));
        assertEquals("14:05", getText(page, "Administration Time"));

        // And I confirm the administration
        page.getByTestId("confirm-administration").first().click();

        // Then the system records the administration with timestamp:
        //   | Field             | Value                                        |
        //   | Patient ID        | PT-12345                                   |
        //   | Medication        | Metoprolol 25mg PO                         |
        //   | Administered By   | Nurse Johnson                              |
        //   | Administration Time| 2025-06-24 14:05:32                       |
        //   | Verification Method| Barcode scan                              |
        waitForTestId(page, "Patient ID");
        assertEquals("PT-12345", getText(page, "Patient ID"));
        assertEquals("Metoprolol 25mg PO", getText(page, "Medication"));
        assertEquals("Nurse Johnson", getText(page, "Administered By"));
        assertEquals("2025-06-24 14:05:32", getText(page, "Administration Time"));
        assertEquals("Barcode scan", getText(page, "Verification Method"));

        // And the medication status is updated to "Given"
        assertEquals("Given", getText(page, "Medication Status"));
        // And the next dose is automatically scheduled for "02:00 tomorrow"
        assertEquals("02:00 tomorrow", getText(page, "Next Dose Scheduled"));
    }

    @Test
    @Order(2)
    @DisplayName("Handle medication administration with allergy alert")
    void handleMedicationAdministrationWithAllergyAlert() {
        // Given a patient "Robert Chen" is in bed "ED-12"
        // And the patient has documented allergies:
        //   | Allergy       | Reaction Type      | Severity  |
        //   | Penicillin    | Rash, hives       | Moderate  |
        //   | Morphine      | Respiratory depression | Severe |
        // And there is a prescribed medication:
        //   | Medication    | Dose      | Route | Prescriber |
        //   | Amoxicillin   | 500mg     | PO    | Dr. Smith  |

        // When I scan the patient's wristband barcode
        page.getByTestId("scan-wristband-barcode").first().click();
        // And I scan the amoxicillin medication barcode
        page.getByTestId("scan-medication-barcode").first().click();

        // Then the system displays an allergy alert:
        //   | Alert Type        | Message                                      |
        //   | DRUG ALLERGY      | ⚠️ WARNING: Patient allergic to Penicillin |
        //   | Cross-Reaction    | Amoxicillin contains penicillin             |
        //   | Severity          | Moderate - Rash, hives                      |
        //   | Recommendation    | Contact physician before administration      |
        waitForTestId(page, "DRUG ALLERGY");
        assertEquals("⚠️ WARNING: Patient allergic to Penicillin", getText(page, "DRUG ALLERGY"));
        assertEquals("Amoxicillin contains penicillin", getText(page, "Cross-Reaction"));
        assertEquals("Moderate - Rash, hives", getText(page, "Severity"));
        assertEquals("Contact physician before administration", getText(page, "Recommendation"));

        // And the system requires additional verification:
        //   | Verification Step | Requirement                                  |
        //   | Physician Approval| Must have override from prescribing MD      |
        //   | Documentation     | Must document reason for override           |
        //   | Monitoring Plan   | Must specify allergy monitoring protocol    |
        assertEquals("Must have override from prescribing MD", getText(page, "Physician Approval"));
        assertEquals("Must document reason for override", getText(page, "Documentation"));
        assertEquals("Must specify allergy monitoring protocol", getText(page, "Monitoring Plan"));

        // And the medication administration is held pending physician confirmation
        var administrationStatus = getText(page, "Administration Status");
        assertMatches(administrationStatus, "pending physician confirmation", true);
        // And an alert is sent to the prescribing physician "Dr. Smith"
        var physicianAlert = getText(page, "Physician Alert");
        assertMatches(physicianAlert, "Dr\\. Smith", false);
    }

    @Test
    @Order(3)
    @DisplayName("Administer PRN medication with clinical assessment")
    void administerPRNMedicationWithClinicalAssessment() {
        // Given a patient "Jennifer Lopez" is in bed "ED-6"
        // And there is a PRN medication order:
        //   | Medication     | Dose    | Route | Indication           | Max Frequency |
        //   | Morphine       | 2mg     | IV    | Pain score ≥7/10     | Q4H PRN       |
        // And the patient's current pain score is "8/10"
        // And the last morphine dose was given 5 hours ago

        // When I scan the patient's wristband barcode
        page.getByTestId("scan-wristband-barcode").first().click();
        // And I scan the morphine medication barcode
        page.getByTestId("scan-medication-barcode").first().click();

        // Then the system verifies PRN medication criteria:
        //   | Criteria          | Assessment                                   | Status   |
        //   | Clinical Indication| Pain score 8/10 meets threshold ≥7/10      | ✓ Met    |
        //   | Frequency Limit   | Last dose 5h ago, within Q4H limit         | ✓ Valid  |
        //   | Patient Safety    | No respiratory contraindications            | ✓ Safe   |
        waitForTestId(page, "Clinical Indication Status");
        assertEquals("✓ Met", getText(page, "Clinical Indication Status"));
        assertEquals("✓ Valid", getText(page, "Frequency Limit Status"));
        assertEquals("✓ Safe", getText(page, "Patient Safety Status"));

        // And I document the clinical assessment:
        //   | Assessment Field  | Value                                        |
        //   | Pain Score        | 8/10                                        |
        //   | Pain Location     | Chest                                       |
        //   | Pain Quality      | Sharp, stabbing                             |
        //   | Vital Signs       | BP: 140/85, HR: 95, RR: 18, O2: 96%       |
        fillFields(page, List.of(
            Map.of("Field", "Pain Score", "Value", "8/10"),
            Map.of("Field", "Pain Location", "Value", "Chest"),
            Map.of("Field", "Pain Quality", "Value", "Sharp, stabbing"),
            Map.of("Field", "Vital Signs", "Value", "BP: 140/85, HR: 95, RR: 18, O2: 96%")
        ));

        // And I confirm the PRN administration
        page.getByTestId("confirm-administration").first().click();

        // Then the system records the administration with clinical justification:
        //   | Documentation     | Details                                      |
        //   | PRN Indication    | Pain score 8/10, patient requested relief   |
        //   | Clinical Assessment| Stable vitals, no respiratory distress     |
        //   | Next Available    | Not before 18:15 (Q4H)                     |
        waitForTestId(page, "PRN Indication");
        assertEquals("Pain score 8/10, patient requested relief", getText(page, "PRN Indication"));
        assertEquals("Stable vitals, no respiratory distress", getText(page, "Clinical Assessment"));
        assertEquals("Not before 18:15 (Q4H)", getText(page, "Next Available"));
    }

    @Test
    @Order(4)
    @DisplayName("Handle medication administration timing variances")
    void handleMedicationAdministrationTimingVariances() {
        // Given a patient "David Kim" is in bed "ED-3"
        // And there is a scheduled medication:
        //   | Medication    | Scheduled Time | Acceptable Window | Current Time |
        //   | Insulin       | 12:00         | ±30 minutes       | 13:15        |

        // When I scan the patient's wristband barcode
        page.getByTestId("scan-wristband-barcode").first().click();
        // And I scan the insulin medication barcode
        page.getByTestId("scan-medication-barcode").first().click();

        // Then the system detects a timing variance:
        //   | Variance Type     | Details                                      |
        //   | Late Administration| 75 minutes past scheduled time             |
        //   | Outside Window    | Beyond acceptable ±30 minute window         |
        //   | Clinical Risk     | Delayed insulin may affect glucose control  |
        waitForTestId(page, "Late Administration");
        assertEquals("75 minutes past scheduled time", getText(page, "Late Administration"));
        assertEquals("Beyond acceptable ±30 minute window", getText(page, "Outside Window"));
        assertEquals("Delayed insulin may affect glucose control", getText(page, "Clinical Risk"));

        // And the system requires variance documentation:
        //   | Required Field    | Purpose                                      |
        //   | Delay Reason      | Why medication was not given on time        |
        //   | Patient Status    | Current clinical condition assessment       |
        //   | Physician Notification| Whether MD was contacted about delay    |
        assertEquals("Why medication was not given on time", getText(page, "Delay Reason Purpose"));
        assertEquals("Current clinical condition assessment", getText(page, "Patient Status Purpose"));
        assertEquals("Whether MD was contacted about delay", getText(page, "Physician Notification Purpose"));

        // And I document the variance:
        //   | Field             | Value                                        |
        //   | Delay Reason      | Patient NPO for procedure until 13:00      |
        //   | Patient Assessment| Blood glucose 140 mg/dL, stable            |
        //   | MD Notified       | Dr. Patel aware of delay                    |
        fillFields(page, List.of(
            Map.of("Field", "Delay Reason", "Value", "Patient NPO for procedure until 13:00"),
            Map.of("Field", "Patient Assessment", "Value", "Blood glucose 140 mg/dL, stable"),
            Map.of("Field", "MD Notified", "Value", "Dr. Patel aware of delay")
        ));

        // Then the administration is recorded with variance documentation
        waitForTestId(page, "Variance Documentation Status");
        var varianceDocumentationStatus = getText(page, "Variance Documentation Status");
        assertTrue(varianceDocumentationStatus.length() > 0);
        // And the next scheduled dose timing is adjusted accordingly
        var nextScheduledDose = getText(page, "Next Scheduled Dose");
        assertTrue(nextScheduledDose.length() > 0);
    }

    @Test
    @Order(5)
    @DisplayName("Administer high-risk medication with double verification")
    void administerHighRiskMedicationWithDoubleVerification() {
        // Given a patient "Susan Williams" is in bed "ED-15"
        // And there is a high-risk medication order:
        //   | Medication    | Dose      | Route | Risk Category        | Special Requirements |
        //   | Heparin       | 5000 units| IV    | High-alert drug      | Double verification  |

        // When I scan the patient's wristband barcode
        page.getByTestId("scan-wristband-barcode").first().click();
        // And I scan the heparin medication barcode
        page.getByTestId("scan-medication-barcode").first().click();

        // Then the system flags the high-risk medication:
        //   | Alert Type        | Message                                      |
        //   | HIGH-ALERT DRUG   | 🔴 Heparin requires double verification     |
        //   | Risk Factors      | Bleeding risk, dosing errors common         |
        //   | Requirements      | Second nurse must verify before administration|
        waitForTestId(page, "HIGH-ALERT DRUG");
        assertEquals("🔴 Heparin requires double verification", getText(page, "HIGH-ALERT DRUG"));
        assertEquals("Bleeding risk, dosing errors common", getText(page, "Risk Factors"));
        assertEquals("Second nurse must verify before administration", getText(page, "Requirements"));

        // And the system requires independent double verification:
        //   | Verification Step | Nurse 1 (Primary) | Nurse 2 (Verifying) |
        //   | Patient Identity  | Nurse Johnson      | Pending             |
        //   | Medication        | Verified           | Pending             |
        //   | Dose Calculation  | 5000 units         | Pending             |
        //   | Route/Rate        | IV bolus           | Pending             |
        assertEquals("Nurse Johnson", getText(page, "Patient Identity Primary Nurse"));
        assertEquals("Pending", getText(page, "Patient Identity Verifying Nurse"));
        assertEquals("Verified", getText(page, "Medication Primary Nurse"));
        assertEquals("Pending", getText(page, "Medication Verifying Nurse"));
        assertEquals("5000 units", getText(page, "Dose Calculation Primary Nurse"));
        assertEquals("Pending", getText(page, "Dose Calculation Verifying Nurse"));
        assertEquals("IV bolus", getText(page, "Route/Rate Primary Nurse"));
        assertEquals("Pending", getText(page, "Route/Rate Verifying Nurse"));

        // When "Nurse Martinez" performs the second verification
        fillField(page, "Verifying Nurse", "Nurse Martinez");
        page.getByTestId("submit-second-verification").first().click();
        // And both nurses confirm the administration
        page.getByTestId("confirm-administration").first().click();

        // Then the system records dual verification:
        //   | Field             | Value                                        |
        //   | Primary Nurse     | Nurse Johnson                               |
        //   | Verifying Nurse   | Nurse Martinez                              |
        //   | Verification Time | 2025-06-24 15:30:15                        |
        //   | Both Signatures   | Electronic signatures captured              |
        waitForTestId(page, "Primary Nurse");
        assertEquals("Nurse Johnson", getText(page, "Primary Nurse"));
        assertEquals("Nurse Martinez", getText(page, "Verifying Nurse"));
        assertEquals("2025-06-24 15:30:15", getText(page, "Verification Time"));
        assertEquals("Electronic signatures captured", getText(page, "Both Signatures"));
    }

    @Test
    @Order(6)
    @DisplayName("Handle medication barcode scanning errors")
    void handleMedicationBarcodeScanningErrors() {
        // Given a patient "Michael Davis" is in bed "ED-11"
        // And I need to administer prescribed medication

        // When I scan the patient's wristband barcode successfully
        page.getByTestId("scan-wristband-barcode").first().click();
        // And I attempt to scan a medication barcode that is damaged/unreadable
        page.getByTestId("scan-medication-barcode").first().click();

        // Then the system displays a barcode error:
        //   | Error Type        | Message                                      |
        //   | Barcode Unreadable| Unable to scan medication barcode           |
        //   | Manual Options    | Enter medication manually or get replacement|
        //   | Safety Warning    | Manual entry bypasses barcode verification  |
        waitForTestId(page, "Barcode Unreadable");
        assertEquals("Unable to scan medication barcode", getText(page, "Barcode Unreadable"));
        assertEquals("Enter medication manually or get replacement", getText(page, "Manual Options"));
        assertEquals("Manual entry bypasses barcode verification", getText(page, "Safety Warning"));

        // And I choose to manually enter the medication information:
        //   | Manual Entry Field| Value                                        |
        //   | Medication Name   | Tylenol                                     |
        //   | Strength          | 650mg                                       |
        //   | NDC Number        | 50580-506-02                               |
        //   | Lot Number        | ABC123                                      |
        //   | Expiration Date   | 2026-12-31                                 |
        fillFields(page, List.of(
            Map.of("Field", "Medication Name", "Value", "Tylenol"),
            Map.of("Field", "Strength", "Value", "650mg"),
            Map.of("Field", "NDC Number", "Value", "50580-506-02"),
            Map.of("Field", "Lot Number", "Value", "ABC123"),
            Map.of("Field", "Expiration Date", "Value", "2026-12-31")
        ));

        // Then the system validates the manual entry against the order
        var manualEntryValidation = waitForTestId(page, "Manual Entry Validation");
        assertTrue(manualEntryValidation.isVisible());
        // And requires supervisor override for manual medication entry
        waitForTestId(page, "Supervisor Override");
        // And documents the barcode scanning issue for pharmacy review
        var pharmacyReviewFlag = waitForTestId(page, "Pharmacy Review Flag");
        assertTrue(pharmacyReviewFlag.isVisible());
    }

    @Test
    @Order(7)
    @DisplayName("Administer pediatric medication with weight-based verification")
    void administerPediatricMedicationWithWeightBasedVerification() {
        // Given a pediatric patient "Emma Foster" (age 5, weight 18kg) is in bed "ED-PEDS-1"
        // And there is a weight-based medication order:
        //   | Medication     | Dose Calculation    | Prescribed Dose | Route |
        //   | Acetaminophen  | 15mg/kg            | 270mg          | PO    |

        // When I scan the patient's wristband barcode
        page.getByTestId("scan-wristband-barcode").first().click();
        // And I scan the acetaminophen medication barcode
        page.getByTestId("scan-medication-barcode").first().click();

        // Then the system verifies pediatric dosing:
        //   | Verification       | Calculation                                  | Status   |
        //   | Weight Confirmation| Patient weight: 18kg                        | ✓ Valid  |
        //   | Dose Calculation   | 15mg/kg × 18kg = 270mg                     | ✓ Correct|
        //   | Maximum Safe Dose  | 270mg < 400mg max (safe)                    | ✓ Safe   |
        //   | Age Appropriateness| Acetaminophen approved for age 5            | ✓ Valid  |
        waitForTestId(page, "Weight Confirmation Status");
        assertEquals("✓ Valid", getText(page, "Weight Confirmation Status"));
        assertEquals("✓ Correct", getText(page, "Dose Calculation Status"));
        assertEquals("✓ Safe", getText(page, "Maximum Safe Dose Status"));
        assertEquals("✓ Valid", getText(page, "Age Appropriateness Status"));

        // And the system displays pediatric-specific information:
        //   | Field              | Value                                        |
        //   | Patient Age/Weight | 5 years old, 18kg                          |
        //   | Calculation Shown  | 15mg/kg × 18kg = 270mg                     |
        //   | Liquid Formulation | 160mg/5mL suspension                       |
        //   | Volume to Give     | 8.4mL                                       |
        assertEquals("5 years old, 18kg", getText(page, "Patient Age/Weight"));
        assertEquals("15mg/kg × 18kg = 270mg", getText(page, "Calculation Shown"));
        assertEquals("160mg/5mL suspension", getText(page, "Liquid Formulation"));
        assertEquals("8.4mL", getText(page, "Volume to Give"));

        // And I confirm the pediatric administration
        page.getByTestId("confirm-administration").first().click();

        // Then the system records with pediatric-specific documentation
        var pediatricDocumentation = waitForTestId(page, "Pediatric Documentation");
        assertTrue(pediatricDocumentation.isVisible());
    }

    @Test
    @Order(8)
    @DisplayName("Handle medication administration during code blue emergency")
    void handleMedicationAdministrationDuringCodeBlueEmergency() {
        // Given a patient "Crisis Patient" is in bed "ED-TRAUMA-1"
        // And a code blue emergency is in progress
        // And emergency medications are ordered:
        //   | Medication    | Dose      | Route | Urgency    |
        //   | Epinephrine   | 1mg       | IV    | STAT       |
        //   | Atropine      | 0.5mg     | IV    | STAT       |

        // When I scan the patient's wristband during the emergency
        page.getByTestId("scan-wristband-barcode").first().click();
        // And I scan the epinephrine medication barcode
        page.getByTestId("scan-medication-barcode").first().click();

        // Then the system activates emergency administration mode:
        //   | Emergency Feature | Behavior                                     |
        //   | Rapid Verification| Abbreviated safety checks for life-saving   |
        //   | Time Documentation| Precise timestamp for code blue timeline    |
        //   | Team Notification | Alert code team of medication administration |
        waitForTestId(page, "Rapid Verification");
        assertEquals("Abbreviated safety checks for life-saving", getText(page, "Rapid Verification"));
        assertEquals("Precise timestamp for code blue timeline", getText(page, "Time Documentation"));
        assertEquals("Alert code team of medication administration", getText(page, "Team Notification"));

        // And the system allows emergency override of timing restrictions
        waitForTestId(page, "Emergency Override");

        // And records the administration with code blue context:
        //   | Field             | Value                                        |
        //   | Emergency Context | Code Blue - Cardiac arrest                  |
        //   | Rapid Administration| Life-saving intervention                   |
        //   | Code Blue Timeline| 15:45:32 - Epinephrine given               |
        assertEquals("Code Blue - Cardiac arrest", getText(page, "Emergency Context"));
        assertEquals("Life-saving intervention", getText(page, "Rapid Administration"));
        assertEquals("15:45:32 - Epinephrine given", getText(page, "Code Blue Timeline"));

        // And the code blue medication log is automatically updated
        var codeBlueMedicationLog = waitForTestId(page, "Code Blue Medication Log");
        assertTrue(codeBlueMedicationLog.isVisible());
    }
}
