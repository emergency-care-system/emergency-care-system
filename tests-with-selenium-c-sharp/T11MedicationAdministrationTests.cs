// Selenium WebDriver + NUnit test for
// tests-with-given-when-then-features/11-medication-administration.feature
// (equivalent to tests-with-selenium-javascript/11-medication-administration.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see Support/Fields.cs) and the shared
// data-testid contract in Support/Login.cs (login-identity, login-submit,
// app-root).

namespace EmergencyCareSystem.SeleniumTests;

[TestFixture]
[NonParallelizable]
public class T11MedicationAdministrationTests
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
        //   And I am logged in as "Nurse Johnson"
        //   And the medication administration module is active
        //   And the barcode scanning system is functional
        //   And the medication administration record (MAR) is accessible
        VerifySystemIsOperational(driver);
        Login(driver, "Nurse Johnson");
        // The medication administration module, barcode scanning system, and
        // MAR accessibility are assumed to be pre-seeded test environment state.

        var medicationAdministrationNavLink = WaitForTestId(driver, "Nav Medication Administration");
        medicationAdministrationNavLink.Click();
        WaitForTestId(driver, "Medication Administration Panel");
    }

    [Test, Order(1)]
    [Description("Successfully administer scheduled medication with barcode verification")]
    public void SuccessfullyAdministerScheduledMedicationWithBarcodeVerification()
    {
        // Given a patient "Maria Gonzalez" is in bed "ED-8"
        // And the patient has a prescribed medication:
        //   | Medication    | Dose      | Route | Frequency | Scheduled Time | Status    |
        //   | Metoprolol    | 25mg      | PO    | BID       | 14:00         | Due       |
        // And the patient is wearing a wristband with barcode "PT-12345"
        // And I have the medication with barcode "MED-METOPROLOL-25MG"

        // When I scan the patient's wristband barcode "PT-12345"
        driver.FindElement(By.CssSelector("[data-testid=\"scan-wristband-barcode\"]")).Click();
        // And I scan the medication barcode "MED-METOPROLOL-25MG"
        driver.FindElement(By.CssSelector("[data-testid=\"scan-medication-barcode\"]")).Click();

        // Then the system verifies the five rights of medication administration:
        //   | Right          | Verification                                  | Status   |
        //   | Right Patient  | Maria Gonzalez confirmed via wristband       | ✓ Valid  |
        //   | Right Drug     | Metoprolol matches prescribed medication     | ✓ Valid  |
        //   | Right Dose     | 25mg matches prescribed dose                 | ✓ Valid  |
        //   | Right Route    | PO (oral) matches prescribed route           | ✓ Valid  |
        //   | Right Time     | Within acceptable window (13:30-14:30)      | ✓ Valid  |
        WaitForTestId(driver, "Right Patient Status");
        Assert.That(GetText(driver, "Right Patient Status"), Is.EqualTo("✓ Valid"));
        Assert.That(GetText(driver, "Right Drug Status"), Is.EqualTo("✓ Valid"));
        Assert.That(GetText(driver, "Right Dose Status"), Is.EqualTo("✓ Valid"));
        Assert.That(GetText(driver, "Right Route Status"), Is.EqualTo("✓ Valid"));
        Assert.That(GetText(driver, "Right Time Status"), Is.EqualTo("✓ Valid"));

        // And the system displays confirmation screen:
        //   | Field             | Value                                        |
        //   | Patient           | Maria Gonzalez, DOB: 1975-08-15            |
        //   | Medication        | Metoprolol 25mg                            |
        //   | Route             | PO (By mouth)                              |
        //   | Administration Time| 14:05                                      |
        Assert.That(GetText(driver, "Patient"), Is.EqualTo("Maria Gonzalez, DOB: 1975-08-15"));
        Assert.That(GetText(driver, "Medication"), Is.EqualTo("Metoprolol 25mg"));
        Assert.That(GetText(driver, "Route"), Is.EqualTo("PO (By mouth)"));
        Assert.That(GetText(driver, "Administration Time"), Is.EqualTo("14:05"));

        // And I confirm the administration
        driver.FindElement(By.CssSelector("[data-testid=\"confirm-administration\"]")).Click();

        // Then the system records the administration with timestamp:
        //   | Field             | Value                                        |
        //   | Patient ID        | PT-12345                                   |
        //   | Medication        | Metoprolol 25mg PO                         |
        //   | Administered By   | Nurse Johnson                              |
        //   | Administration Time| 2025-06-24 14:05:32                       |
        //   | Verification Method| Barcode scan                              |
        WaitForTestId(driver, "Patient ID");
        Assert.That(GetText(driver, "Patient ID"), Is.EqualTo("PT-12345"));
        Assert.That(GetText(driver, "Medication"), Is.EqualTo("Metoprolol 25mg PO"));
        Assert.That(GetText(driver, "Administered By"), Is.EqualTo("Nurse Johnson"));
        Assert.That(GetText(driver, "Administration Time"), Is.EqualTo("2025-06-24 14:05:32"));
        Assert.That(GetText(driver, "Verification Method"), Is.EqualTo("Barcode scan"));

        // And the medication status is updated to "Given"
        Assert.That(GetText(driver, "Medication Status"), Is.EqualTo("Given"));
        // And the next dose is automatically scheduled for "02:00 tomorrow"
        Assert.That(GetText(driver, "Next Dose Scheduled"), Is.EqualTo("02:00 tomorrow"));
    }

    [Test, Order(2)]
    [Description("Handle medication administration with allergy alert")]
    public void HandleMedicationAdministrationWithAllergyAlert()
    {
        // Given a patient "Robert Chen" is in bed "ED-12"
        // And the patient has documented allergies:
        //   | Allergy       | Reaction Type      | Severity  |
        //   | Penicillin    | Rash, hives       | Moderate  |
        //   | Morphine      | Respiratory depression | Severe |
        // And there is a prescribed medication:
        //   | Medication    | Dose      | Route | Prescriber |
        //   | Amoxicillin   | 500mg     | PO    | Dr. Smith  |

        // When I scan the patient's wristband barcode
        driver.FindElement(By.CssSelector("[data-testid=\"scan-wristband-barcode\"]")).Click();
        // And I scan the amoxicillin medication barcode
        driver.FindElement(By.CssSelector("[data-testid=\"scan-medication-barcode\"]")).Click();

        // Then the system displays an allergy alert:
        //   | Alert Type        | Message                                      |
        //   | DRUG ALLERGY      | ⚠️ WARNING: Patient allergic to Penicillin |
        //   | Cross-Reaction    | Amoxicillin contains penicillin             |
        //   | Severity          | Moderate - Rash, hives                      |
        //   | Recommendation    | Contact physician before administration      |
        WaitForTestId(driver, "DRUG ALLERGY");
        Assert.That(GetText(driver, "DRUG ALLERGY"), Is.EqualTo("⚠️ WARNING: Patient allergic to Penicillin"));
        Assert.That(GetText(driver, "Cross-Reaction"), Is.EqualTo("Amoxicillin contains penicillin"));
        Assert.That(GetText(driver, "Severity"), Is.EqualTo("Moderate - Rash, hives"));
        Assert.That(GetText(driver, "Recommendation"), Is.EqualTo("Contact physician before administration"));

        // And the system requires additional verification:
        //   | Verification Step | Requirement                                  |
        //   | Physician Approval| Must have override from prescribing MD      |
        //   | Documentation     | Must document reason for override           |
        //   | Monitoring Plan   | Must specify allergy monitoring protocol    |
        Assert.That(GetText(driver, "Physician Approval"), Is.EqualTo("Must have override from prescribing MD"));
        Assert.That(GetText(driver, "Documentation"), Is.EqualTo("Must document reason for override"));
        Assert.That(GetText(driver, "Monitoring Plan"), Is.EqualTo("Must specify allergy monitoring protocol"));

        // And the medication administration is held pending physician confirmation
        var administrationStatus = GetText(driver, "Administration Status");
        Assert.That(administrationStatus, Does.Match(@"pending physician confirmation").IgnoreCase);
        // And an alert is sent to the prescribing physician "Dr. Smith"
        var physicianAlert = GetText(driver, "Physician Alert");
        Assert.That(physicianAlert, Does.Match(@"Dr\. Smith"));
    }

    [Test, Order(3)]
    [Description("Administer PRN medication with clinical assessment")]
    public void AdministerPRNMedicationWithClinicalAssessment()
    {
        // Given a patient "Jennifer Lopez" is in bed "ED-6"
        // And there is a PRN medication order:
        //   | Medication     | Dose    | Route | Indication           | Max Frequency |
        //   | Morphine       | 2mg     | IV    | Pain score ≥7/10     | Q4H PRN       |
        // And the patient's current pain score is "8/10"
        // And the last morphine dose was given 5 hours ago

        // When I scan the patient's wristband barcode
        driver.FindElement(By.CssSelector("[data-testid=\"scan-wristband-barcode\"]")).Click();
        // And I scan the morphine medication barcode
        driver.FindElement(By.CssSelector("[data-testid=\"scan-medication-barcode\"]")).Click();

        // Then the system verifies PRN medication criteria:
        //   | Criteria          | Assessment                                   | Status   |
        //   | Clinical Indication| Pain score 8/10 meets threshold ≥7/10      | ✓ Met    |
        //   | Frequency Limit   | Last dose 5h ago, within Q4H limit         | ✓ Valid  |
        //   | Patient Safety    | No respiratory contraindications            | ✓ Safe   |
        WaitForTestId(driver, "Clinical Indication Status");
        Assert.That(GetText(driver, "Clinical Indication Status"), Is.EqualTo("✓ Met"));
        Assert.That(GetText(driver, "Frequency Limit Status"), Is.EqualTo("✓ Valid"));
        Assert.That(GetText(driver, "Patient Safety Status"), Is.EqualTo("✓ Safe"));

        // And I document the clinical assessment:
        //   | Assessment Field  | Value                                        |
        //   | Pain Score        | 8/10                                        |
        //   | Pain Location     | Chest                                       |
        //   | Pain Quality      | Sharp, stabbing                             |
        //   | Vital Signs       | BP: 140/85, HR: 95, RR: 18, O2: 96%       |
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Pain Score"), ("Value", "8/10")),
            Row(("Field", "Pain Location"), ("Value", "Chest")),
            Row(("Field", "Pain Quality"), ("Value", "Sharp, stabbing")),
            Row(("Field", "Vital Signs"), ("Value", "BP: 140/85, HR: 95, RR: 18, O2: 96%")),
        });

        // And I confirm the PRN administration
        driver.FindElement(By.CssSelector("[data-testid=\"confirm-administration\"]")).Click();

        // Then the system records the administration with clinical justification:
        //   | Documentation     | Details                                      |
        //   | PRN Indication    | Pain score 8/10, patient requested relief   |
        //   | Clinical Assessment| Stable vitals, no respiratory distress     |
        //   | Next Available    | Not before 18:15 (Q4H)                     |
        WaitForTestId(driver, "PRN Indication");
        Assert.That(GetText(driver, "PRN Indication"), Is.EqualTo("Pain score 8/10, patient requested relief"));
        Assert.That(GetText(driver, "Clinical Assessment"), Is.EqualTo("Stable vitals, no respiratory distress"));
        Assert.That(GetText(driver, "Next Available"), Is.EqualTo("Not before 18:15 (Q4H)"));
    }

    [Test, Order(4)]
    [Description("Handle medication administration timing variances")]
    public void HandleMedicationAdministrationTimingVariances()
    {
        // Given a patient "David Kim" is in bed "ED-3"
        // And there is a scheduled medication:
        //   | Medication    | Scheduled Time | Acceptable Window | Current Time |
        //   | Insulin       | 12:00         | ±30 minutes       | 13:15        |

        // When I scan the patient's wristband barcode
        driver.FindElement(By.CssSelector("[data-testid=\"scan-wristband-barcode\"]")).Click();
        // And I scan the insulin medication barcode
        driver.FindElement(By.CssSelector("[data-testid=\"scan-medication-barcode\"]")).Click();

        // Then the system detects a timing variance:
        //   | Variance Type     | Details                                      |
        //   | Late Administration| 75 minutes past scheduled time             |
        //   | Outside Window    | Beyond acceptable ±30 minute window         |
        //   | Clinical Risk     | Delayed insulin may affect glucose control  |
        WaitForTestId(driver, "Late Administration");
        Assert.That(GetText(driver, "Late Administration"), Is.EqualTo("75 minutes past scheduled time"));
        Assert.That(GetText(driver, "Outside Window"), Is.EqualTo("Beyond acceptable ±30 minute window"));
        Assert.That(GetText(driver, "Clinical Risk"), Is.EqualTo("Delayed insulin may affect glucose control"));

        // And the system requires variance documentation:
        //   | Required Field    | Purpose                                      |
        //   | Delay Reason      | Why medication was not given on time        |
        //   | Patient Status    | Current clinical condition assessment       |
        //   | Physician Notification| Whether MD was contacted about delay    |
        Assert.That(GetText(driver, "Delay Reason Purpose"), Is.EqualTo("Why medication was not given on time"));
        Assert.That(GetText(driver, "Patient Status Purpose"), Is.EqualTo("Current clinical condition assessment"));
        Assert.That(GetText(driver, "Physician Notification Purpose"), Is.EqualTo("Whether MD was contacted about delay"));

        // And I document the variance:
        //   | Field             | Value                                        |
        //   | Delay Reason      | Patient NPO for procedure until 13:00      |
        //   | Patient Assessment| Blood glucose 140 mg/dL, stable            |
        //   | MD Notified       | Dr. Patel aware of delay                    |
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Delay Reason"), ("Value", "Patient NPO for procedure until 13:00")),
            Row(("Field", "Patient Assessment"), ("Value", "Blood glucose 140 mg/dL, stable")),
            Row(("Field", "MD Notified"), ("Value", "Dr. Patel aware of delay")),
        });

        // Then the administration is recorded with variance documentation
        WaitForTestId(driver, "Variance Documentation Status");
        var varianceDocumentationStatus = GetText(driver, "Variance Documentation Status");
        Assert.That(varianceDocumentationStatus.Length > 0, Is.True);
        // And the next scheduled dose timing is adjusted accordingly
        var nextScheduledDose = GetText(driver, "Next Scheduled Dose");
        Assert.That(nextScheduledDose.Length > 0, Is.True);
    }

    [Test, Order(5)]
    [Description("Administer high-risk medication with double verification")]
    public void AdministerHighRiskMedicationWithDoubleVerification()
    {
        // Given a patient "Susan Williams" is in bed "ED-15"
        // And there is a high-risk medication order:
        //   | Medication    | Dose      | Route | Risk Category        | Special Requirements |
        //   | Heparin       | 5000 units| IV    | High-alert drug      | Double verification  |

        // When I scan the patient's wristband barcode
        driver.FindElement(By.CssSelector("[data-testid=\"scan-wristband-barcode\"]")).Click();
        // And I scan the heparin medication barcode
        driver.FindElement(By.CssSelector("[data-testid=\"scan-medication-barcode\"]")).Click();

        // Then the system flags the high-risk medication:
        //   | Alert Type        | Message                                      |
        //   | HIGH-ALERT DRUG   | 🔴 Heparin requires double verification     |
        //   | Risk Factors      | Bleeding risk, dosing errors common         |
        //   | Requirements      | Second nurse must verify before administration|
        WaitForTestId(driver, "HIGH-ALERT DRUG");
        Assert.That(GetText(driver, "HIGH-ALERT DRUG"), Is.EqualTo("🔴 Heparin requires double verification"));
        Assert.That(GetText(driver, "Risk Factors"), Is.EqualTo("Bleeding risk, dosing errors common"));
        Assert.That(GetText(driver, "Requirements"), Is.EqualTo("Second nurse must verify before administration"));

        // And the system requires independent double verification:
        //   | Verification Step | Nurse 1 (Primary) | Nurse 2 (Verifying) |
        //   | Patient Identity  | Nurse Johnson      | Pending             |
        //   | Medication        | Verified           | Pending             |
        //   | Dose Calculation  | 5000 units         | Pending             |
        //   | Route/Rate        | IV bolus           | Pending             |
        Assert.That(GetText(driver, "Patient Identity Primary Nurse"), Is.EqualTo("Nurse Johnson"));
        Assert.That(GetText(driver, "Patient Identity Verifying Nurse"), Is.EqualTo("Pending"));
        Assert.That(GetText(driver, "Medication Primary Nurse"), Is.EqualTo("Verified"));
        Assert.That(GetText(driver, "Medication Verifying Nurse"), Is.EqualTo("Pending"));
        Assert.That(GetText(driver, "Dose Calculation Primary Nurse"), Is.EqualTo("5000 units"));
        Assert.That(GetText(driver, "Dose Calculation Verifying Nurse"), Is.EqualTo("Pending"));
        Assert.That(GetText(driver, "Route/Rate Primary Nurse"), Is.EqualTo("IV bolus"));
        Assert.That(GetText(driver, "Route/Rate Verifying Nurse"), Is.EqualTo("Pending"));

        // When "Nurse Martinez" performs the second verification
        FillField(driver, "Verifying Nurse", "Nurse Martinez");
        driver.FindElement(By.CssSelector("[data-testid=\"submit-second-verification\"]")).Click();
        // And both nurses confirm the administration
        driver.FindElement(By.CssSelector("[data-testid=\"confirm-administration\"]")).Click();

        // Then the system records dual verification:
        //   | Field             | Value                                        |
        //   | Primary Nurse     | Nurse Johnson                               |
        //   | Verifying Nurse   | Nurse Martinez                              |
        //   | Verification Time | 2025-06-24 15:30:15                        |
        //   | Both Signatures   | Electronic signatures captured              |
        WaitForTestId(driver, "Primary Nurse");
        Assert.That(GetText(driver, "Primary Nurse"), Is.EqualTo("Nurse Johnson"));
        Assert.That(GetText(driver, "Verifying Nurse"), Is.EqualTo("Nurse Martinez"));
        Assert.That(GetText(driver, "Verification Time"), Is.EqualTo("2025-06-24 15:30:15"));
        Assert.That(GetText(driver, "Both Signatures"), Is.EqualTo("Electronic signatures captured"));
    }

    [Test, Order(6)]
    [Description("Handle medication barcode scanning errors")]
    public void HandleMedicationBarcodeScanningErrors()
    {
        // Given a patient "Michael Davis" is in bed "ED-11"
        // And I need to administer prescribed medication

        // When I scan the patient's wristband barcode successfully
        driver.FindElement(By.CssSelector("[data-testid=\"scan-wristband-barcode\"]")).Click();
        // And I attempt to scan a medication barcode that is damaged/unreadable
        driver.FindElement(By.CssSelector("[data-testid=\"scan-medication-barcode\"]")).Click();

        // Then the system displays a barcode error:
        //   | Error Type        | Message                                      |
        //   | Barcode Unreadable| Unable to scan medication barcode           |
        //   | Manual Options    | Enter medication manually or get replacement|
        //   | Safety Warning    | Manual entry bypasses barcode verification  |
        WaitForTestId(driver, "Barcode Unreadable");
        Assert.That(GetText(driver, "Barcode Unreadable"), Is.EqualTo("Unable to scan medication barcode"));
        Assert.That(GetText(driver, "Manual Options"), Is.EqualTo("Enter medication manually or get replacement"));
        Assert.That(GetText(driver, "Safety Warning"), Is.EqualTo("Manual entry bypasses barcode verification"));

        // And I choose to manually enter the medication information:
        //   | Manual Entry Field| Value                                        |
        //   | Medication Name   | Tylenol                                     |
        //   | Strength          | 650mg                                       |
        //   | NDC Number        | 50580-506-02                               |
        //   | Lot Number        | ABC123                                      |
        //   | Expiration Date   | 2026-12-31                                 |
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Medication Name"), ("Value", "Tylenol")),
            Row(("Field", "Strength"), ("Value", "650mg")),
            Row(("Field", "NDC Number"), ("Value", "50580-506-02")),
            Row(("Field", "Lot Number"), ("Value", "ABC123")),
            Row(("Field", "Expiration Date"), ("Value", "2026-12-31")),
        });

        // Then the system validates the manual entry against the order
        var manualEntryValidation = WaitForTestId(driver, "Manual Entry Validation");
        Assert.That(manualEntryValidation.Displayed, Is.True);
        // And requires supervisor override for manual medication entry
        WaitForTestId(driver, "Supervisor Override");
        // And documents the barcode scanning issue for pharmacy review
        var pharmacyReviewFlag = WaitForTestId(driver, "Pharmacy Review Flag");
        Assert.That(pharmacyReviewFlag.Displayed, Is.True);
    }

    [Test, Order(7)]
    [Description("Administer pediatric medication with weight-based verification")]
    public void AdministerPediatricMedicationWithWeightBasedVerification()
    {
        // Given a pediatric patient "Emma Foster" (age 5, weight 18kg) is in bed "ED-PEDS-1"
        // And there is a weight-based medication order:
        //   | Medication     | Dose Calculation    | Prescribed Dose | Route |
        //   | Acetaminophen  | 15mg/kg            | 270mg          | PO    |

        // When I scan the patient's wristband barcode
        driver.FindElement(By.CssSelector("[data-testid=\"scan-wristband-barcode\"]")).Click();
        // And I scan the acetaminophen medication barcode
        driver.FindElement(By.CssSelector("[data-testid=\"scan-medication-barcode\"]")).Click();

        // Then the system verifies pediatric dosing:
        //   | Verification       | Calculation                                  | Status   |
        //   | Weight Confirmation| Patient weight: 18kg                        | ✓ Valid  |
        //   | Dose Calculation   | 15mg/kg × 18kg = 270mg                     | ✓ Correct|
        //   | Maximum Safe Dose  | 270mg < 400mg max (safe)                    | ✓ Safe   |
        //   | Age Appropriateness| Acetaminophen approved for age 5            | ✓ Valid  |
        WaitForTestId(driver, "Weight Confirmation Status");
        Assert.That(GetText(driver, "Weight Confirmation Status"), Is.EqualTo("✓ Valid"));
        Assert.That(GetText(driver, "Dose Calculation Status"), Is.EqualTo("✓ Correct"));
        Assert.That(GetText(driver, "Maximum Safe Dose Status"), Is.EqualTo("✓ Safe"));
        Assert.That(GetText(driver, "Age Appropriateness Status"), Is.EqualTo("✓ Valid"));

        // And the system displays pediatric-specific information:
        //   | Field              | Value                                        |
        //   | Patient Age/Weight | 5 years old, 18kg                          |
        //   | Calculation Shown  | 15mg/kg × 18kg = 270mg                     |
        //   | Liquid Formulation | 160mg/5mL suspension                       |
        //   | Volume to Give     | 8.4mL                                       |
        Assert.That(GetText(driver, "Patient Age/Weight"), Is.EqualTo("5 years old, 18kg"));
        Assert.That(GetText(driver, "Calculation Shown"), Is.EqualTo("15mg/kg × 18kg = 270mg"));
        Assert.That(GetText(driver, "Liquid Formulation"), Is.EqualTo("160mg/5mL suspension"));
        Assert.That(GetText(driver, "Volume to Give"), Is.EqualTo("8.4mL"));

        // And I confirm the pediatric administration
        driver.FindElement(By.CssSelector("[data-testid=\"confirm-administration\"]")).Click();

        // Then the system records with pediatric-specific documentation
        var pediatricDocumentation = WaitForTestId(driver, "Pediatric Documentation");
        Assert.That(pediatricDocumentation.Displayed, Is.True);
    }

    [Test, Order(8)]
    [Description("Handle medication administration during code blue emergency")]
    public void HandleMedicationAdministrationDuringCodeBlueEmergency()
    {
        // Given a patient "Crisis Patient" is in bed "ED-TRAUMA-1"
        // And a code blue emergency is in progress
        // And emergency medications are ordered:
        //   | Medication    | Dose      | Route | Urgency    |
        //   | Epinephrine   | 1mg       | IV    | STAT       |
        //   | Atropine      | 0.5mg     | IV    | STAT       |

        // When I scan the patient's wristband during the emergency
        driver.FindElement(By.CssSelector("[data-testid=\"scan-wristband-barcode\"]")).Click();
        // And I scan the epinephrine medication barcode
        driver.FindElement(By.CssSelector("[data-testid=\"scan-medication-barcode\"]")).Click();

        // Then the system activates emergency administration mode:
        //   | Emergency Feature | Behavior                                     |
        //   | Rapid Verification| Abbreviated safety checks for life-saving   |
        //   | Time Documentation| Precise timestamp for code blue timeline    |
        //   | Team Notification | Alert code team of medication administration |
        WaitForTestId(driver, "Rapid Verification");
        Assert.That(GetText(driver, "Rapid Verification"), Is.EqualTo("Abbreviated safety checks for life-saving"));
        Assert.That(GetText(driver, "Time Documentation"), Is.EqualTo("Precise timestamp for code blue timeline"));
        Assert.That(GetText(driver, "Team Notification"), Is.EqualTo("Alert code team of medication administration"));

        // And the system allows emergency override of timing restrictions
        WaitForTestId(driver, "Emergency Override");

        // And records the administration with code blue context:
        //   | Field             | Value                                        |
        //   | Emergency Context | Code Blue - Cardiac arrest                  |
        //   | Rapid Administration| Life-saving intervention                   |
        //   | Code Blue Timeline| 15:45:32 - Epinephrine given               |
        Assert.That(GetText(driver, "Emergency Context"), Is.EqualTo("Code Blue - Cardiac arrest"));
        Assert.That(GetText(driver, "Rapid Administration"), Is.EqualTo("Life-saving intervention"));
        Assert.That(GetText(driver, "Code Blue Timeline"), Is.EqualTo("15:45:32 - Epinephrine given"));

        // And the code blue medication log is automatically updated
        var codeBlueMedicationLog = WaitForTestId(driver, "Code Blue Medication Log");
        Assert.That(codeBlueMedicationLog.Displayed, Is.True);
    }
}
