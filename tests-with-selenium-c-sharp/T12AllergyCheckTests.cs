// Selenium WebDriver + NUnit test for
// tests-with-given-when-then-features/12-allergy-check.feature
// (equivalent to tests-with-selenium-javascript/12-allergy-check.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see Support/Fields.cs) and the shared
// data-testid contract in Support/Login.cs (login-identity, login-submit,
// app-root).

namespace EmergencyCareSystem.SeleniumTests;

[TestFixture]
[NonParallelizable]
public class T12AllergyCheckTests
{
    private IWebDriver driver = null!;

    [OneTimeSetUp]
    public void SetUpClass()
    {
        driver = DriverFactory.Build();
    }

    [OneTimeTearDown]
    public void TearDownClass()
    {
        driver.Quit();
    }

    [SetUp]
    public void SetUp()
    {
        // Background:
        //   Given the emergency care system is operational
        //   And I am logged in as "Dr. Smith"
        //   And the allergy checking module is active
        //   And the drug interaction database is up-to-date
        VerifySystemIsOperational(driver);
        Login(driver, "Dr. Smith");
        // The allergy checking module and the drug interaction database being
        // up-to-date are assumed to be pre-seeded test environment state.

        var allergyCheckNavLink = WaitForTestId(driver, "Nav Allergy Check");
        allergyCheckNavLink.Click();
        WaitForTestId(driver, "Allergy Check Panel");
    }

    [Test, Order(1)]
    [Description("Prescribe penicillin to patient with documented penicillin allergy")]
    public void PrescribePenicillinToPatientWithDocumentedPenicillinAllergy()
    {
        // Given a patient "Maria Rodriguez" is in bed "ED-8"
        // And the patient has documented allergies:
        //   | Allergy        | Reaction Type       | Severity  | Date Documented | Source     |
        //   | Penicillin     | Rash, hives        | Moderate  | 2023-05-15     | Patient    |
        //   | Shellfish      | Anaphylaxis        | Severe    | 2022-08-10     | Patient    |

        // When I enter a medication order for:
        //   | Medication     | Dose      | Route | Frequency | Duration |
        //   | Penicillin VK  | 500mg     | PO    | QID       | 10 days  |
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Medication"), ("Value", "Penicillin VK")),
            Row(("Field", "Dose"), ("Value", "500mg")),
            Row(("Field", "Route"), ("Value", "PO")),
            Row(("Field", "Frequency"), ("Value", "QID")),
            Row(("Field", "Duration"), ("Value", "10 days")),
        });
        // And I submit the order
        driver.FindElement(By.CssSelector("[data-testid=\"submit-medication-order\"]")).Click();

        // Then the system displays an allergy warning:
        //   | Alert Type         | Details                                      |
        //   | DRUG ALLERGY       | ⚠️ ALLERGY ALERT: Patient allergic to Penicillin |
        //   | Severity Level     | Moderate                                     |
        //   | Reaction Type      | Rash, hives                                  |
        //   | Date Documented    | May 15, 2023                                |
        //   | Source             | Patient reported                             |
        WaitForTestId(driver, "DRUG ALLERGY");
        Assert.That(GetText(driver, "DRUG ALLERGY"), Is.EqualTo("⚠️ ALLERGY ALERT: Patient allergic to Penicillin"));
        Assert.That(GetText(driver, "Severity Level"), Is.EqualTo("Moderate"));
        Assert.That(GetText(driver, "Reaction Type"), Is.EqualTo("Rash, hives"));
        Assert.That(GetText(driver, "Date Documented"), Is.EqualTo("May 15, 2023"));
        Assert.That(GetText(driver, "Source"), Is.EqualTo("Patient reported"));

        // And the system blocks the order submission
        var orderStatus = GetText(driver, "Order Status");
        Assert.That(orderStatus, Does.Match(@"blocked").IgnoreCase);

        // And I am presented with options:
        //   | Option             | Description                                  |
        //   | Cancel Order       | Remove penicillin order                      |
        //   | Override with Reason| Document clinical justification            |
        //   | Alternative Drugs  | View suggested alternative antibiotics       |
        WaitForTestId(driver, "Cancel Order");
        WaitForTestId(driver, "Override with Reason");
        WaitForTestId(driver, "Alternative Drugs");

        // And the allergy alert is logged in the audit trail
        var auditTrailEntry = WaitForTestId(driver, "Audit Trail Entry");
        Assert.That(auditTrailEntry.Displayed, Is.True);
    }

    [Test, Order(2)]
    [Description("Prescribe medication with no documented allergies")]
    public void PrescribeMedicationWithNoDocumentedAllergies()
    {
        // Given a patient "John Taylor" is in bed "ED-12"
        // And the patient has no documented allergies

        // When I enter a medication order for:
        //   | Medication     | Dose      | Route | Frequency |
        //   | Amoxicillin    | 875mg     | PO    | BID       |
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Medication"), ("Value", "Amoxicillin")),
            Row(("Field", "Dose"), ("Value", "875mg")),
            Row(("Field", "Route"), ("Value", "PO")),
            Row(("Field", "Frequency"), ("Value", "BID")),
        });
        // And I submit the order
        driver.FindElement(By.CssSelector("[data-testid=\"submit-medication-order\"]")).Click();

        // Then the system performs allergy checking
        // And no allergy alerts are triggered
        var allergyAlerts = driver.FindElements(By.CssSelector("[data-testid=\"allergy-alert\"]"));
        Assert.That(allergyAlerts.Count, Is.EqualTo(0));

        // And the order is processed normally
        // And the system displays confirmation:
        //   | Confirmation Type  | Message                                      |
        //   | No Allergies Found | No known allergies to Amoxicillin          |
        //   | Order Status       | Order submitted successfully                 |
        WaitForTestId(driver, "No Allergies Found");
        Assert.That(GetText(driver, "No Allergies Found"), Is.EqualTo("No known allergies to Amoxicillin"));
        Assert.That(GetText(driver, "Order Status"), Is.EqualTo("Order submitted successfully"));

        // And the medication order is routed to pharmacy
        var pharmacyRoutingStatus = GetText(driver, "Pharmacy Routing Status");
        Assert.That(pharmacyRoutingStatus, Does.Match(@"pharmacy").IgnoreCase);
    }

    [Test, Order(3)]
    [Description("Prescribe medication with cross-reactive allergy")]
    public void PrescribeMedicationWithCrossReactiveAllergy()
    {
        // Given a patient "Sarah Johnson" is in bed "ED-5"
        // And the patient has documented allergies:
        //   | Allergy        | Reaction Type       | Severity  |
        //   | Penicillin     | Respiratory distress| Severe    |

        // When I enter a medication order for:
        //   | Medication     | Dose      | Route | Frequency |
        //   | Amoxicillin    | 500mg     | PO    | TID       |
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Medication"), ("Value", "Amoxicillin")),
            Row(("Field", "Dose"), ("Value", "500mg")),
            Row(("Field", "Route"), ("Value", "PO")),
            Row(("Field", "Frequency"), ("Value", "TID")),
        });
        // And I submit the order
        driver.FindElement(By.CssSelector("[data-testid=\"submit-medication-order\"]")).Click();

        // Then the system displays a cross-reactivity warning:
        //   | Alert Type         | Details                                      |
        //   | CROSS-REACTIVITY   | ⚠️ WARNING: Cross-reactivity with Penicillin|
        //   | Known Allergy      | Patient allergic to Penicillin (Severe)     |
        //   | Cross-Reaction Risk| Amoxicillin is a penicillin derivative     |
        //   | Reaction Type      | Respiratory distress                         |
        //   | Risk Level         | High - Severe reaction possible              |
        WaitForTestId(driver, "CROSS-REACTIVITY");
        Assert.That(GetText(driver, "CROSS-REACTIVITY"), Is.EqualTo("⚠️ WARNING: Cross-reactivity with Penicillin"));
        Assert.That(GetText(driver, "Known Allergy"), Is.EqualTo("Patient allergic to Penicillin (Severe)"));
        Assert.That(GetText(driver, "Cross-Reaction Risk"), Is.EqualTo("Amoxicillin is a penicillin derivative"));
        Assert.That(GetText(driver, "Reaction Type"), Is.EqualTo("Respiratory distress"));
        Assert.That(GetText(driver, "Risk Level"), Is.EqualTo("High - Severe reaction possible"));

        // And the system provides additional information:
        //   | Information Type   | Content                                      |
        //   | Cross-Reaction Rate| 8-10% cross-reactivity with penicillin     |
        //   | Clinical Guidance  | Consider non-beta-lactam alternatives       |
        //   | Emergency Prep     | Have epinephrine available if administered   |
        Assert.That(GetText(driver, "Cross-Reaction Rate"), Is.EqualTo("8-10% cross-reactivity with penicillin"));
        Assert.That(GetText(driver, "Clinical Guidance"), Is.EqualTo("Consider non-beta-lactam alternatives"));
        Assert.That(GetText(driver, "Emergency Prep"), Is.EqualTo("Have epinephrine available if administered"));

        // And I must acknowledge the cross-reactivity risk before proceeding
        WaitForTestId(driver, "Acknowledge Cross-Reactivity Risk");
    }

    [Test, Order(4)]
    [Description("Override allergy alert with clinical justification")]
    public void OverrideAllergyAlertWithClinicalJustification()
    {
        // Given a patient "Michael Chen" is in bed "ED-15"
        // And the patient has a documented penicillin allergy with "mild rash"
        // And the patient has severe sepsis requiring immediate antibiotic treatment

        // When I enter a penicillin order and receive an allergy alert
        FillField(driver, "Medication", "Penicillin");
        driver.FindElement(By.CssSelector("[data-testid=\"submit-medication-order\"]")).Click();
        WaitForTestId(driver, "DRUG ALLERGY");
        // And I choose to override the allergy warning
        driver.FindElement(By.CssSelector("[data-testid=\"override-allergy-warning\"]")).Click();

        // Then the system requires detailed justification:
        //   | Required Field     | Description                                  |
        //   | Clinical Rationale | Why this medication is medically necessary   |
        //   | Risk Assessment    | Evaluation of allergy risk vs benefit       |
        //   | Monitoring Plan    | How allergic reactions will be monitored    |
        //   | Alternative Review | Why alternatives are not suitable            |
        WaitForTestId(driver, "Clinical Rationale Description");
        Assert.That(GetText(driver, "Clinical Rationale Description"), Is.EqualTo("Why this medication is medically necessary"));
        Assert.That(GetText(driver, "Risk Assessment Description"), Is.EqualTo("Evaluation of allergy risk vs benefit"));
        Assert.That(GetText(driver, "Monitoring Plan Description"), Is.EqualTo("How allergic reactions will be monitored"));
        Assert.That(GetText(driver, "Alternative Review Description"), Is.EqualTo("Why alternatives are not suitable"));

        // And I document the override:
        //   | Field              | Value                                        |
        //   | Clinical Rationale | Life-threatening sepsis, first-line antibiotic needed |
        //   | Risk Assessment    | Mild rash risk acceptable vs sepsis mortality |
        //   | Monitoring Plan    | Continuous monitoring, diphenhydramine available |
        //   | Alternative Review | Other antibiotics inadequate for organism     |
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Clinical Rationale"), ("Value", "Life-threatening sepsis, first-line antibiotic needed")),
            Row(("Field", "Risk Assessment"), ("Value", "Mild rash risk acceptable vs sepsis mortality")),
            Row(("Field", "Monitoring Plan"), ("Value", "Continuous monitoring, diphenhydramine available")),
            Row(("Field", "Alternative Review"), ("Value", "Other antibiotics inadequate for organism")),
        });
        driver.FindElement(By.CssSelector("[data-testid=\"submit-override-documentation\"]")).Click();

        // Then the system accepts the override
        WaitForTestId(driver, "Override Status");
        var overrideStatus = GetText(driver, "Override Status");
        Assert.That(overrideStatus, Does.Match(@"accepted").IgnoreCase);

        // And logs the override decision with full documentation
        var overrideAuditLog = WaitForTestId(driver, "Override Audit Log");
        Assert.That(overrideAuditLog.Displayed, Is.True);

        // And notifies nursing staff of the allergy override for enhanced monitoring
        var nursingNotification = GetText(driver, "Nursing Notification");
        Assert.That(nursingNotification, Does.Match(@"enhanced monitoring").IgnoreCase);
    }

    [Test, Order(5)]
    [Description("Check allergies for multiple medications simultaneously")]
    public void CheckAllergiesForMultipleMedicationsSimultaneously()
    {
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
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Medication 1"), ("Value", "Fentanyl")),
            Row(("Field", "Dose 1"), ("Value", "50mcg")),
            Row(("Field", "Route 1"), ("Value", "IV")),
            Row(("Field", "Purpose 1"), ("Value", "Pain control")),
            Row(("Field", "Medication 2"), ("Value", "Ibuprofen")),
            Row(("Field", "Dose 2"), ("Value", "600mg")),
            Row(("Field", "Route 2"), ("Value", "PO")),
            Row(("Field", "Purpose 2"), ("Value", "Anti-inflammatory")),
            Row(("Field", "Medication 3"), ("Value", "Acetaminophen")),
            Row(("Field", "Dose 3"), ("Value", "650mg")),
            Row(("Field", "Route 3"), ("Value", "PO")),
            Row(("Field", "Purpose 3"), ("Value", "Pain/fever")),
        });
        // And I submit all orders simultaneously
        driver.FindElement(By.CssSelector("[data-testid=\"submit-medication-orders\"]")).Click();

        // Then the system checks each medication against documented allergies:
        //   | Medication     | Allergy Status | Alert Level |
        //   | Fentanyl       | No direct allergy | Safe      |
        //   | Ibuprofen      | NSAID allergy  | WARNING   |
        //   | Acetaminophen  | No allergy     | Safe      |
        WaitForTestId(driver, "Fentanyl Alert Level");
        Assert.That(GetText(driver, "Fentanyl Allergy Status"), Is.EqualTo("No direct allergy"));
        Assert.That(GetText(driver, "Fentanyl Alert Level"), Is.EqualTo("Safe"));
        Assert.That(GetText(driver, "Ibuprofen Allergy Status"), Is.EqualTo("NSAID allergy"));
        Assert.That(GetText(driver, "Ibuprofen Alert Level"), Is.EqualTo("WARNING"));
        Assert.That(GetText(driver, "Acetaminophen Allergy Status"), Is.EqualTo("No allergy"));
        Assert.That(GetText(driver, "Acetaminophen Alert Level"), Is.EqualTo("Safe"));

        // And I receive specific alerts for problematic medications:
        //   | Alert Medication | Warning Message                              |
        //   | Ibuprofen        | Patient allergic to NSAIDs - GI bleeding risk |
        Assert.That(GetText(driver, "Ibuprofen Warning Message"), Is.EqualTo("Patient allergic to NSAIDs - GI bleeding risk"));

        // And safe medications are processed without alerts
        var fentanylWarnings = driver.FindElements(By.CssSelector("[data-testid=\"fentanyl-warning-message\"]"));
        Assert.That(fentanylWarnings.Count, Is.EqualTo(0));

        // And I can review and modify orders before final submission
        WaitForTestId(driver, "Review and Modify Orders");
    }

    [Test, Order(6)]
    [Description("Handle unknown or \"No Known Allergies\" status")]
    public void HandleUnknownOrNoKnownAllergiesStatus()
    {
        // Given a patient "Robert Davis" is in bed "ED-3"
        // And the patient's allergy status is "Unknown - Unable to assess"

        // When I enter a medication order for:
        //   | Medication     | Dose      | Route |
        //   | Cephalexin     | 500mg     | PO    |
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Medication"), ("Value", "Cephalexin")),
            Row(("Field", "Dose"), ("Value", "500mg")),
            Row(("Field", "Route"), ("Value", "PO")),
        });
        // And I submit the order
        driver.FindElement(By.CssSelector("[data-testid=\"submit-medication-order\"]")).Click();

        // Then the system displays an information alert:
        //   | Alert Type         | Message                                      |
        //   | ALLERGY UNKNOWN    | ⚠️ INFO: Patient allergy status unknown     |
        //   | Risk Consideration | Cannot verify medication allergies          |
        //   | Recommendation     | Consider allergy assessment before administration |
        WaitForTestId(driver, "ALLERGY UNKNOWN");
        Assert.That(GetText(driver, "ALLERGY UNKNOWN"), Is.EqualTo("⚠️ INFO: Patient allergy status unknown"));
        Assert.That(GetText(driver, "Risk Consideration"), Is.EqualTo("Cannot verify medication allergies"));
        Assert.That(GetText(driver, "Recommendation"), Is.EqualTo("Consider allergy assessment before administration"));

        // And the system provides safety recommendations:
        //   | Recommendation     | Details                                      |
        //   | Allergy Assessment | Attempt to obtain allergy history           |
        //   | Start Monitoring   | Monitor for allergic reactions closely      |
        //   | Have Antidotes Ready| Ensure emergency medications available      |
        Assert.That(GetText(driver, "Allergy Assessment"), Is.EqualTo("Attempt to obtain allergy history"));
        Assert.That(GetText(driver, "Start Monitoring"), Is.EqualTo("Monitor for allergic reactions closely"));
        Assert.That(GetText(driver, "Have Antidotes Ready"), Is.EqualTo("Ensure emergency medications available"));

        // And the order is flagged for enhanced allergy monitoring
        var enhancedMonitoringFlag = GetText(driver, "Enhanced Monitoring Flag");
        Assert.That(enhancedMonitoringFlag, Does.Match(@"enhanced allergy monitoring").IgnoreCase);
    }

    [Test, Order(7)]
    [Description("Check for drug class allergies")]
    public void CheckForDrugClassAllergies()
    {
        // Given a patient "Jennifer Wilson" is in bed "ED-11"
        // And the patient has documented allergies:
        //   | Allergy        | Reaction Type       | Severity  | Drug Class |
        //   | Sulfa drugs    | Stevens-Johnson syndrome | Severe | Sulfonamides |

        // When I enter a medication order for:
        //   | Medication           | Dose      | Route | Drug Class    |
        //   | Trimethoprim-Sulfamethoxazole | 800mg | PO | Sulfonamide |
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Medication"), ("Value", "Trimethoprim-Sulfamethoxazole")),
            Row(("Field", "Dose"), ("Value", "800mg")),
            Row(("Field", "Route"), ("Value", "PO")),
            Row(("Field", "Drug Class"), ("Value", "Sulfonamide")),
        });
        // And I submit the order
        driver.FindElement(By.CssSelector("[data-testid=\"submit-medication-order\"]")).Click();

        // Then the system identifies the drug class allergy:
        //   | Alert Type         | Details                                      |
        //   | DRUG CLASS ALLERGY | ⚠️ SEVERE: Patient allergic to Sulfa drugs |
        //   | Specific Drug      | TMP-SMX contains sulfamethoxazole           |
        //   | Reaction History   | Stevens-Johnson syndrome                     |
        //   | Severity           | Severe - Life-threatening reaction possible  |
        WaitForTestId(driver, "DRUG CLASS ALLERGY");
        Assert.That(GetText(driver, "DRUG CLASS ALLERGY"), Is.EqualTo("⚠️ SEVERE: Patient allergic to Sulfa drugs"));
        Assert.That(GetText(driver, "Specific Drug"), Is.EqualTo("TMP-SMX contains sulfamethoxazole"));
        Assert.That(GetText(driver, "Reaction History"), Is.EqualTo("Stevens-Johnson syndrome"));
        Assert.That(GetText(driver, "Severity"), Is.EqualTo("Severe - Life-threatening reaction possible"));

        // And the system provides drug class education:
        //   | Information        | Content                                      |
        //   | Drug Class         | Sulfonamide antibiotics                     |
        //   | Cross-Reactivity   | All sulfa-containing medications at risk    |
        //   | Alternative Classes| Beta-lactams, fluoroquinolones available   |
        Assert.That(GetText(driver, "Drug Class"), Is.EqualTo("Sulfonamide antibiotics"));
        Assert.That(GetText(driver, "Cross-Reactivity"), Is.EqualTo("All sulfa-containing medications at risk"));
        Assert.That(GetText(driver, "Alternative Classes"), Is.EqualTo("Beta-lactams, fluoroquinolones available"));
    }

    [Test, Order(8)]
    [Description("Handle allergy information from multiple sources")]
    public void HandleAllergyInformationFromMultipleSources()
    {
        // Given a patient "David Kim" is in bed "ED-9"
        // And the patient has allergy information from multiple sources:
        //   | Source             | Allergy    | Reaction        | Reliability |
        //   | Patient Report     | Penicillin | "Bad reaction"  | Unverified  |
        //   | Medical Records    | Penicillin | Urticaria, rash | Verified    |
        //   | Family Member      | Codeine    | Nausea         | Unverified  |

        // When I enter a penicillin order
        FillField(driver, "Medication", "Penicillin");
        driver.FindElement(By.CssSelector("[data-testid=\"submit-medication-order\"]")).Click();

        // Then the system displays comprehensive allergy information:
        //   | Source Type        | Allergy Details                              |
        //   | Verified Record    | Penicillin - Urticaria, rash (Medical Records) |
        //   | Patient Report     | Penicillin - "Bad reaction" (Unverified)    |
        WaitForTestId(driver, "Verified Record");
        Assert.That(GetText(driver, "Verified Record"), Is.EqualTo("Penicillin - Urticaria, rash (Medical Records)"));
        Assert.That(GetText(driver, "Patient Report"), Is.EqualTo("Penicillin - \"Bad reaction\" (Unverified)"));

        // And the system prioritizes verified information in the alert
        // And provides source credibility indicators:
        //   | Source             | Credibility Level | Clinical Weight      |
        //   | Medical Records    | High reliability  | Primary consideration |
        //   | Patient Report     | Moderate reliability | Secondary consideration |
        Assert.That(GetText(driver, "Medical Records Credibility Level"), Is.EqualTo("High reliability"));
        Assert.That(GetText(driver, "Medical Records Clinical Weight"), Is.EqualTo("Primary consideration"));
        Assert.That(GetText(driver, "Patient Report Credibility Level"), Is.EqualTo("Moderate reliability"));
        Assert.That(GetText(driver, "Patient Report Clinical Weight"), Is.EqualTo("Secondary consideration"));

        // And I can review detailed allergy history before making decisions
        WaitForTestId(driver, "Detailed Allergy History");
    }

    [Test, Order(9)]
    [Description("Real-time allergy checking during order modification")]
    public void RealTimeAllergyCheckingDuringOrderModification()
    {
        // Given a patient "Susan Martinez" is in bed "ED-4"
        // And the patient has a penicillin allergy
        // And I have started entering a medication order

        // When I begin typing "Pen" in the medication field
        FillField(driver, "Medication", "Pen");

        // Then the system provides real-time allergy warnings:
        //   | Alert Type         | Message                                      |
        //   | PREDICTIVE ALERT   | ⚠️ Patient allergic to Penicillin          |
        //   | Medication Match   | "Pen" may be penicillin-related drug       |
        //   | Suggestion         | Consider alternative antibiotics            |
        WaitForTestId(driver, "PREDICTIVE ALERT");
        Assert.That(GetText(driver, "PREDICTIVE ALERT"), Is.EqualTo("⚠️ Patient allergic to Penicillin"));
        Assert.That(GetText(driver, "Medication Match"), Is.EqualTo("\"Pen\" may be penicillin-related drug"));
        Assert.That(GetText(driver, "Suggestion"), Is.EqualTo("Consider alternative antibiotics"));

        // And the system highlights potential allergy matches as I type
        WaitForTestId(driver, "Allergy Match Highlight");

        // And provides alternative medication suggestions:
        //   | Alternative        | Drug Class        | Reason               |
        //   | Cephalexin         | Cephalosporin     | Lower cross-reactivity |
        //   | Azithromycin       | Macrolide         | No cross-reactivity   |
        //   | Ciprofloxacin      | Fluoroquinolone   | Different mechanism   |
        Assert.That(GetText(driver, "Cephalexin Drug Class"), Is.EqualTo("Cephalosporin"));
        Assert.That(GetText(driver, "Cephalexin Reason"), Is.EqualTo("Lower cross-reactivity"));
        Assert.That(GetText(driver, "Azithromycin Drug Class"), Is.EqualTo("Macrolide"));
        Assert.That(GetText(driver, "Azithromycin Reason"), Is.EqualTo("No cross-reactivity"));
        Assert.That(GetText(driver, "Ciprofloxacin Drug Class"), Is.EqualTo("Fluoroquinolone"));
        Assert.That(GetText(driver, "Ciprofloxacin Reason"), Is.EqualTo("Different mechanism"));

        // And I can select alternatives directly from the suggestion list
        WaitForTestId(driver, "Alternative Suggestion List");
    }
}
