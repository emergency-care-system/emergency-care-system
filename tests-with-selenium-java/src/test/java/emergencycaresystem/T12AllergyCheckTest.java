// Selenium WebDriver + JUnit 5 test for
// tests-with-given-when-then-features/12-allergy-check.feature
// (equivalent to tests-with-selenium-javascript/12-allergy-check.test.js).
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
class T12AllergyCheckTest {
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
        //   And I am logged in as "Dr. Smith"
        //   And the allergy checking module is active
        //   And the drug interaction database is up-to-date
        verifySystemIsOperational(driver);
        login(driver, "Dr. Smith");
        // The allergy checking module and the drug interaction database being
        // up-to-date are assumed to be pre-seeded test environment state.

        var allergyCheckNavLink = waitForTestId(driver, "Nav Allergy Check");
        allergyCheckNavLink.click();
        waitForTestId(driver, "Allergy Check Panel");
    }

    @Test
    @Order(1)
    @DisplayName("Prescribe penicillin to patient with documented penicillin allergy")
    void prescribePenicillinToPatientWithDocumentedPenicillinAllergy() {
        // Given a patient "Maria Rodriguez" is in bed "ED-8"
        // And the patient has documented allergies:
        //   | Allergy        | Reaction Type       | Severity  | Date Documented | Source     |
        //   | Penicillin     | Rash, hives        | Moderate  | 2023-05-15     | Patient    |
        //   | Shellfish      | Anaphylaxis        | Severe    | 2022-08-10     | Patient    |

        // When I enter a medication order for:
        //   | Medication     | Dose      | Route | Frequency | Duration |
        //   | Penicillin VK  | 500mg     | PO    | QID       | 10 days  |
        fillFields(driver, List.of(
            Map.of("Field", "Medication", "Value", "Penicillin VK"),
            Map.of("Field", "Dose", "Value", "500mg"),
            Map.of("Field", "Route", "Value", "PO"),
            Map.of("Field", "Frequency", "Value", "QID"),
            Map.of("Field", "Duration", "Value", "10 days")
        ));
        // And I submit the order
        driver.findElement(By.cssSelector("[data-testid=\"submit-medication-order\"]")).click();

        // Then the system displays an allergy warning:
        //   | Alert Type         | Details                                      |
        //   | DRUG ALLERGY       | ⚠️ ALLERGY ALERT: Patient allergic to Penicillin |
        //   | Severity Level     | Moderate                                     |
        //   | Reaction Type      | Rash, hives                                  |
        //   | Date Documented    | May 15, 2023                                |
        //   | Source             | Patient reported                             |
        waitForTestId(driver, "DRUG ALLERGY");
        assertEquals("⚠️ ALLERGY ALERT: Patient allergic to Penicillin", getText(driver, "DRUG ALLERGY"));
        assertEquals("Moderate", getText(driver, "Severity Level"));
        assertEquals("Rash, hives", getText(driver, "Reaction Type"));
        assertEquals("May 15, 2023", getText(driver, "Date Documented"));
        assertEquals("Patient reported", getText(driver, "Source"));

        // And the system blocks the order submission
        var orderStatus = getText(driver, "Order Status");
        assertMatches(orderStatus, "blocked", true);

        // And I am presented with options:
        //   | Option             | Description                                  |
        //   | Cancel Order       | Remove penicillin order                      |
        //   | Override with Reason| Document clinical justification            |
        //   | Alternative Drugs  | View suggested alternative antibiotics       |
        waitForTestId(driver, "Cancel Order");
        waitForTestId(driver, "Override with Reason");
        waitForTestId(driver, "Alternative Drugs");

        // And the allergy alert is logged in the audit trail
        var auditTrailEntry = waitForTestId(driver, "Audit Trail Entry");
        assertTrue(auditTrailEntry.isDisplayed());
    }

    @Test
    @Order(2)
    @DisplayName("Prescribe medication with no documented allergies")
    void prescribeMedicationWithNoDocumentedAllergies() {
        // Given a patient "John Taylor" is in bed "ED-12"
        // And the patient has no documented allergies

        // When I enter a medication order for:
        //   | Medication     | Dose      | Route | Frequency |
        //   | Amoxicillin    | 875mg     | PO    | BID       |
        fillFields(driver, List.of(
            Map.of("Field", "Medication", "Value", "Amoxicillin"),
            Map.of("Field", "Dose", "Value", "875mg"),
            Map.of("Field", "Route", "Value", "PO"),
            Map.of("Field", "Frequency", "Value", "BID")
        ));
        // And I submit the order
        driver.findElement(By.cssSelector("[data-testid=\"submit-medication-order\"]")).click();

        // Then the system performs allergy checking
        // And no allergy alerts are triggered
        var allergyAlerts = driver.findElements(By.cssSelector("[data-testid=\"allergy-alert\"]"));
        assertEquals(0, allergyAlerts.size());

        // And the order is processed normally
        // And the system displays confirmation:
        //   | Confirmation Type  | Message                                      |
        //   | No Allergies Found | No known allergies to Amoxicillin          |
        //   | Order Status       | Order submitted successfully                 |
        waitForTestId(driver, "No Allergies Found");
        assertEquals("No known allergies to Amoxicillin", getText(driver, "No Allergies Found"));
        assertEquals("Order submitted successfully", getText(driver, "Order Status"));

        // And the medication order is routed to pharmacy
        var pharmacyRoutingStatus = getText(driver, "Pharmacy Routing Status");
        assertMatches(pharmacyRoutingStatus, "pharmacy", true);
    }

    @Test
    @Order(3)
    @DisplayName("Prescribe medication with cross-reactive allergy")
    void prescribeMedicationWithCrossReactiveAllergy() {
        // Given a patient "Sarah Johnson" is in bed "ED-5"
        // And the patient has documented allergies:
        //   | Allergy        | Reaction Type       | Severity  |
        //   | Penicillin     | Respiratory distress| Severe    |

        // When I enter a medication order for:
        //   | Medication     | Dose      | Route | Frequency |
        //   | Amoxicillin    | 500mg     | PO    | TID       |
        fillFields(driver, List.of(
            Map.of("Field", "Medication", "Value", "Amoxicillin"),
            Map.of("Field", "Dose", "Value", "500mg"),
            Map.of("Field", "Route", "Value", "PO"),
            Map.of("Field", "Frequency", "Value", "TID")
        ));
        // And I submit the order
        driver.findElement(By.cssSelector("[data-testid=\"submit-medication-order\"]")).click();

        // Then the system displays a cross-reactivity warning:
        //   | Alert Type         | Details                                      |
        //   | CROSS-REACTIVITY   | ⚠️ WARNING: Cross-reactivity with Penicillin|
        //   | Known Allergy      | Patient allergic to Penicillin (Severe)     |
        //   | Cross-Reaction Risk| Amoxicillin is a penicillin derivative     |
        //   | Reaction Type      | Respiratory distress                         |
        //   | Risk Level         | High - Severe reaction possible              |
        waitForTestId(driver, "CROSS-REACTIVITY");
        assertEquals("⚠️ WARNING: Cross-reactivity with Penicillin", getText(driver, "CROSS-REACTIVITY"));
        assertEquals("Patient allergic to Penicillin (Severe)", getText(driver, "Known Allergy"));
        assertEquals("Amoxicillin is a penicillin derivative", getText(driver, "Cross-Reaction Risk"));
        assertEquals("Respiratory distress", getText(driver, "Reaction Type"));
        assertEquals("High - Severe reaction possible", getText(driver, "Risk Level"));

        // And the system provides additional information:
        //   | Information Type   | Content                                      |
        //   | Cross-Reaction Rate| 8-10% cross-reactivity with penicillin     |
        //   | Clinical Guidance  | Consider non-beta-lactam alternatives       |
        //   | Emergency Prep     | Have epinephrine available if administered   |
        assertEquals("8-10% cross-reactivity with penicillin", getText(driver, "Cross-Reaction Rate"));
        assertEquals("Consider non-beta-lactam alternatives", getText(driver, "Clinical Guidance"));
        assertEquals("Have epinephrine available if administered", getText(driver, "Emergency Prep"));

        // And I must acknowledge the cross-reactivity risk before proceeding
        waitForTestId(driver, "Acknowledge Cross-Reactivity Risk");
    }

    @Test
    @Order(4)
    @DisplayName("Override allergy alert with clinical justification")
    void overrideAllergyAlertWithClinicalJustification() {
        // Given a patient "Michael Chen" is in bed "ED-15"
        // And the patient has a documented penicillin allergy with "mild rash"
        // And the patient has severe sepsis requiring immediate antibiotic treatment

        // When I enter a penicillin order and receive an allergy alert
        fillField(driver, "Medication", "Penicillin");
        driver.findElement(By.cssSelector("[data-testid=\"submit-medication-order\"]")).click();
        waitForTestId(driver, "DRUG ALLERGY");
        // And I choose to override the allergy warning
        driver.findElement(By.cssSelector("[data-testid=\"override-allergy-warning\"]")).click();

        // Then the system requires detailed justification:
        //   | Required Field     | Description                                  |
        //   | Clinical Rationale | Why this medication is medically necessary   |
        //   | Risk Assessment    | Evaluation of allergy risk vs benefit       |
        //   | Monitoring Plan    | How allergic reactions will be monitored    |
        //   | Alternative Review | Why alternatives are not suitable            |
        waitForTestId(driver, "Clinical Rationale Description");
        assertEquals("Why this medication is medically necessary", getText(driver, "Clinical Rationale Description"));
        assertEquals("Evaluation of allergy risk vs benefit", getText(driver, "Risk Assessment Description"));
        assertEquals("How allergic reactions will be monitored", getText(driver, "Monitoring Plan Description"));
        assertEquals("Why alternatives are not suitable", getText(driver, "Alternative Review Description"));

        // And I document the override:
        //   | Field              | Value                                        |
        //   | Clinical Rationale | Life-threatening sepsis, first-line antibiotic needed |
        //   | Risk Assessment    | Mild rash risk acceptable vs sepsis mortality |
        //   | Monitoring Plan    | Continuous monitoring, diphenhydramine available |
        //   | Alternative Review | Other antibiotics inadequate for organism     |
        fillFields(driver, List.of(
            Map.of("Field", "Clinical Rationale", "Value", "Life-threatening sepsis, first-line antibiotic needed"),
            Map.of("Field", "Risk Assessment", "Value", "Mild rash risk acceptable vs sepsis mortality"),
            Map.of("Field", "Monitoring Plan", "Value", "Continuous monitoring, diphenhydramine available"),
            Map.of("Field", "Alternative Review", "Value", "Other antibiotics inadequate for organism")
        ));
        driver.findElement(By.cssSelector("[data-testid=\"submit-override-documentation\"]")).click();

        // Then the system accepts the override
        waitForTestId(driver, "Override Status");
        var overrideStatus = getText(driver, "Override Status");
        assertMatches(overrideStatus, "accepted", true);

        // And logs the override decision with full documentation
        var overrideAuditLog = waitForTestId(driver, "Override Audit Log");
        assertTrue(overrideAuditLog.isDisplayed());

        // And notifies nursing staff of the allergy override for enhanced monitoring
        var nursingNotification = getText(driver, "Nursing Notification");
        assertMatches(nursingNotification, "enhanced monitoring", true);
    }

    @Test
    @Order(5)
    @DisplayName("Check allergies for multiple medications simultaneously")
    void checkAllergiesForMultipleMedicationsSimultaneously() {
        // Given a patient "Lisa Brown" is in bed "ED-7"
        // And the patient has documented allergies:
        //   | Allergy        | Reaction Type       | Severity  |
        //   | Morphine       | Respiratory depression | Severe |
        //   | NSAIDs         | GI bleeding        | Moderate  |

        // When I enter multiple medication orders:
        //   | Medication     | Dose      | Route | Purpose           |
        //   | Fentanyl       | 50mcg     | IV    | Pain control      |
        //   | Ibuprofen      | 600mg     | PO    | Anti-inflammatory |
        //   | Acetaminophen  | 650mg     | PO    | Pain/fever        |
        fillFields(driver, List.of(
            Map.of("Field", "Medication 1", "Value", "Fentanyl"),
            Map.of("Field", "Dose 1", "Value", "50mcg"),
            Map.of("Field", "Route 1", "Value", "IV"),
            Map.of("Field", "Purpose 1", "Value", "Pain control"),
            Map.of("Field", "Medication 2", "Value", "Ibuprofen"),
            Map.of("Field", "Dose 2", "Value", "600mg"),
            Map.of("Field", "Route 2", "Value", "PO"),
            Map.of("Field", "Purpose 2", "Value", "Anti-inflammatory"),
            Map.of("Field", "Medication 3", "Value", "Acetaminophen"),
            Map.of("Field", "Dose 3", "Value", "650mg"),
            Map.of("Field", "Route 3", "Value", "PO"),
            Map.of("Field", "Purpose 3", "Value", "Pain/fever")
        ));
        // And I submit all orders simultaneously
        driver.findElement(By.cssSelector("[data-testid=\"submit-medication-orders\"]")).click();

        // Then the system checks each medication against documented allergies:
        //   | Medication     | Allergy Status | Alert Level |
        //   | Fentanyl       | No direct allergy | Safe      |
        //   | Ibuprofen      | NSAID allergy  | WARNING   |
        //   | Acetaminophen  | No allergy     | Safe      |
        waitForTestId(driver, "Fentanyl Alert Level");
        assertEquals("No direct allergy", getText(driver, "Fentanyl Allergy Status"));
        assertEquals("Safe", getText(driver, "Fentanyl Alert Level"));
        assertEquals("NSAID allergy", getText(driver, "Ibuprofen Allergy Status"));
        assertEquals("WARNING", getText(driver, "Ibuprofen Alert Level"));
        assertEquals("No allergy", getText(driver, "Acetaminophen Allergy Status"));
        assertEquals("Safe", getText(driver, "Acetaminophen Alert Level"));

        // And I receive specific alerts for problematic medications:
        //   | Alert Medication | Warning Message                              |
        //   | Ibuprofen        | Patient allergic to NSAIDs - GI bleeding risk |
        assertEquals("Patient allergic to NSAIDs - GI bleeding risk", getText(driver, "Ibuprofen Warning Message"));

        // And safe medications are processed without alerts
        var fentanylWarnings = driver.findElements(By.cssSelector("[data-testid=\"fentanyl-warning-message\"]"));
        assertEquals(0, fentanylWarnings.size());

        // And I can review and modify orders before final submission
        waitForTestId(driver, "Review and Modify Orders");
    }

    @Test
    @Order(6)
    @DisplayName("Handle unknown or \"No Known Allergies\" status")
    void handleUnknownOrNoKnownAllergiesStatus() {
        // Given a patient "Robert Davis" is in bed "ED-3"
        // And the patient's allergy status is "Unknown - Unable to assess"

        // When I enter a medication order for:
        //   | Medication     | Dose      | Route |
        //   | Cephalexin     | 500mg     | PO    |
        fillFields(driver, List.of(
            Map.of("Field", "Medication", "Value", "Cephalexin"),
            Map.of("Field", "Dose", "Value", "500mg"),
            Map.of("Field", "Route", "Value", "PO")
        ));
        // And I submit the order
        driver.findElement(By.cssSelector("[data-testid=\"submit-medication-order\"]")).click();

        // Then the system displays an information alert:
        //   | Alert Type         | Message                                      |
        //   | ALLERGY UNKNOWN    | ⚠️ INFO: Patient allergy status unknown     |
        //   | Risk Consideration | Cannot verify medication allergies          |
        //   | Recommendation     | Consider allergy assessment before administration |
        waitForTestId(driver, "ALLERGY UNKNOWN");
        assertEquals("⚠️ INFO: Patient allergy status unknown", getText(driver, "ALLERGY UNKNOWN"));
        assertEquals("Cannot verify medication allergies", getText(driver, "Risk Consideration"));
        assertEquals("Consider allergy assessment before administration", getText(driver, "Recommendation"));

        // And the system provides safety recommendations:
        //   | Recommendation     | Details                                      |
        //   | Allergy Assessment | Attempt to obtain allergy history           |
        //   | Start Monitoring   | Monitor for allergic reactions closely      |
        //   | Have Antidotes Ready| Ensure emergency medications available      |
        assertEquals("Attempt to obtain allergy history", getText(driver, "Allergy Assessment"));
        assertEquals("Monitor for allergic reactions closely", getText(driver, "Start Monitoring"));
        assertEquals("Ensure emergency medications available", getText(driver, "Have Antidotes Ready"));

        // And the order is flagged for enhanced allergy monitoring
        var enhancedMonitoringFlag = getText(driver, "Enhanced Monitoring Flag");
        assertMatches(enhancedMonitoringFlag, "enhanced allergy monitoring", true);
    }

    @Test
    @Order(7)
    @DisplayName("Check for drug class allergies")
    void checkForDrugClassAllergies() {
        // Given a patient "Jennifer Wilson" is in bed "ED-11"
        // And the patient has documented allergies:
        //   | Allergy        | Reaction Type       | Severity  | Drug Class |
        //   | Sulfa drugs    | Stevens-Johnson syndrome | Severe | Sulfonamides |

        // When I enter a medication order for:
        //   | Medication           | Dose      | Route | Drug Class    |
        //   | Trimethoprim-Sulfamethoxazole | 800mg | PO | Sulfonamide |
        fillFields(driver, List.of(
            Map.of("Field", "Medication", "Value", "Trimethoprim-Sulfamethoxazole"),
            Map.of("Field", "Dose", "Value", "800mg"),
            Map.of("Field", "Route", "Value", "PO"),
            Map.of("Field", "Drug Class", "Value", "Sulfonamide")
        ));
        // And I submit the order
        driver.findElement(By.cssSelector("[data-testid=\"submit-medication-order\"]")).click();

        // Then the system identifies the drug class allergy:
        //   | Alert Type         | Details                                      |
        //   | DRUG CLASS ALLERGY | ⚠️ SEVERE: Patient allergic to Sulfa drugs |
        //   | Specific Drug      | TMP-SMX contains sulfamethoxazole           |
        //   | Reaction History   | Stevens-Johnson syndrome                     |
        //   | Severity           | Severe - Life-threatening reaction possible  |
        waitForTestId(driver, "DRUG CLASS ALLERGY");
        assertEquals("⚠️ SEVERE: Patient allergic to Sulfa drugs", getText(driver, "DRUG CLASS ALLERGY"));
        assertEquals("TMP-SMX contains sulfamethoxazole", getText(driver, "Specific Drug"));
        assertEquals("Stevens-Johnson syndrome", getText(driver, "Reaction History"));
        assertEquals("Severe - Life-threatening reaction possible", getText(driver, "Severity"));

        // And the system provides drug class education:
        //   | Information        | Content                                      |
        //   | Drug Class         | Sulfonamide antibiotics                     |
        //   | Cross-Reactivity   | All sulfa-containing medications at risk    |
        //   | Alternative Classes| Beta-lactams, fluoroquinolones available   |
        assertEquals("Sulfonamide antibiotics", getText(driver, "Drug Class"));
        assertEquals("All sulfa-containing medications at risk", getText(driver, "Cross-Reactivity"));
        assertEquals("Beta-lactams, fluoroquinolones available", getText(driver, "Alternative Classes"));
    }

    @Test
    @Order(8)
    @DisplayName("Handle allergy information from multiple sources")
    void handleAllergyInformationFromMultipleSources() {
        // Given a patient "David Kim" is in bed "ED-9"
        // And the patient has allergy information from multiple sources:
        //   | Source             | Allergy    | Reaction        | Reliability |
        //   | Patient Report     | Penicillin | "Bad reaction"  | Unverified  |
        //   | Medical Records    | Penicillin | Urticaria, rash | Verified    |
        //   | Family Member      | Codeine    | Nausea         | Unverified  |

        // When I enter a penicillin order
        fillField(driver, "Medication", "Penicillin");
        driver.findElement(By.cssSelector("[data-testid=\"submit-medication-order\"]")).click();

        // Then the system displays comprehensive allergy information:
        //   | Source Type        | Allergy Details                              |
        //   | Verified Record    | Penicillin - Urticaria, rash (Medical Records) |
        //   | Patient Report     | Penicillin - "Bad reaction" (Unverified)    |
        waitForTestId(driver, "Verified Record");
        assertEquals("Penicillin - Urticaria, rash (Medical Records)", getText(driver, "Verified Record"));
        assertEquals("Penicillin - \"Bad reaction\" (Unverified)", getText(driver, "Patient Report"));

        // And the system prioritizes verified information in the alert
        // And provides source credibility indicators:
        //   | Source             | Credibility Level | Clinical Weight      |
        //   | Medical Records    | High reliability  | Primary consideration |
        //   | Patient Report     | Moderate reliability | Secondary consideration |
        assertEquals("High reliability", getText(driver, "Medical Records Credibility Level"));
        assertEquals("Primary consideration", getText(driver, "Medical Records Clinical Weight"));
        assertEquals("Moderate reliability", getText(driver, "Patient Report Credibility Level"));
        assertEquals("Secondary consideration", getText(driver, "Patient Report Clinical Weight"));

        // And I can review detailed allergy history before making decisions
        waitForTestId(driver, "Detailed Allergy History");
    }

    @Test
    @Order(9)
    @DisplayName("Real-time allergy checking during order modification")
    void realTimeAllergyCheckingDuringOrderModification() {
        // Given a patient "Susan Martinez" is in bed "ED-4"
        // And the patient has a penicillin allergy
        // And I have started entering a medication order

        // When I begin typing "Pen" in the medication field
        fillField(driver, "Medication", "Pen");

        // Then the system provides real-time allergy warnings:
        //   | Alert Type         | Message                                      |
        //   | PREDICTIVE ALERT   | ⚠️ Patient allergic to Penicillin          |
        //   | Medication Match   | "Pen" may be penicillin-related drug       |
        //   | Suggestion         | Consider alternative antibiotics            |
        waitForTestId(driver, "PREDICTIVE ALERT");
        assertEquals("⚠️ Patient allergic to Penicillin", getText(driver, "PREDICTIVE ALERT"));
        assertEquals("\"Pen\" may be penicillin-related drug", getText(driver, "Medication Match"));
        assertEquals("Consider alternative antibiotics", getText(driver, "Suggestion"));

        // And the system highlights potential allergy matches as I type
        waitForTestId(driver, "Allergy Match Highlight");

        // And provides alternative medication suggestions:
        //   | Alternative        | Drug Class        | Reason               |
        //   | Cephalexin         | Cephalosporin     | Lower cross-reactivity |
        //   | Azithromycin       | Macrolide         | No cross-reactivity   |
        //   | Ciprofloxacin      | Fluoroquinolone   | Different mechanism   |
        assertEquals("Cephalosporin", getText(driver, "Cephalexin Drug Class"));
        assertEquals("Lower cross-reactivity", getText(driver, "Cephalexin Reason"));
        assertEquals("Macrolide", getText(driver, "Azithromycin Drug Class"));
        assertEquals("No cross-reactivity", getText(driver, "Azithromycin Reason"));
        assertEquals("Fluoroquinolone", getText(driver, "Ciprofloxacin Drug Class"));
        assertEquals("Different mechanism", getText(driver, "Ciprofloxacin Reason"));

        // And I can select alternatives directly from the suggestion list
        waitForTestId(driver, "Alternative Suggestion List");
    }
}
