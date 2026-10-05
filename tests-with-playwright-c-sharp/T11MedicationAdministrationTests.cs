// Playwright + NUnit test for
// tests-with-given-when-then-features/11-medication-administration.feature
// (equivalent to tests-with-playwright-javascript/11-medication-administration.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see Support/Fields.cs) and the shared
// data-testid contract in Support/Login.cs (login-identity, login-submit,
// app-root).

namespace EmergencyCareSystem.PlaywrightTests;

[TestFixture]
[NonParallelizable]
public class T11MedicationAdministrationTests
{
    private Session session = null!;
    private IPage page => session.Page;

    [OneTimeSetUp]
    public async Task SetUpClass()
    {
        session = await Session.StartAsync();
    }

    [OneTimeTearDown]
    public async Task TearDownClass()
    {
        await session.DisposeAsync();
    }

    [SetUp]
    public async Task SetUp()
    {
        // Background:
        //   Given the emergency care system is operational
        //   And I am logged in as "Nurse Johnson"
        //   And the medication administration module is active
        //   And the barcode scanning system is functional
        //   And the medication administration record (MAR) is accessible
        await VerifySystemIsOperational(page);
        await Login(page, "Nurse Johnson");
        // The medication administration module, barcode scanning system, and
        // MAR accessibility are assumed to be pre-seeded test environment state.

        var medicationAdministrationNavLink = await WaitForTestId(page, "Nav Medication Administration");
        await medicationAdministrationNavLink.ClickAsync();
        await WaitForTestId(page, "Medication Administration Panel");
    }

    [Test, Order(1)]
    [Description("Successfully administer scheduled medication with barcode verification")]
    public async Task SuccessfullyAdministerScheduledMedicationWithBarcodeVerification()
    {
        // Given a patient "Maria Gonzalez" is in bed "ED-8"
        // And the patient has a prescribed medication:
        //   | Medication    | Dose      | Route | Frequency | Scheduled Time | Status    |
        //   | Metoprolol    | 25mg      | PO    | BID       | 14:00         | Due       |
        // And the patient is wearing a wristband with barcode "PT-12345"
        // And I have the medication with barcode "MED-METOPROLOL-25MG"

        // When I scan the patient's wristband barcode "PT-12345"
        await page.GetByTestId("scan-wristband-barcode").First.ClickAsync();
        // And I scan the medication barcode "MED-METOPROLOL-25MG"
        await page.GetByTestId("scan-medication-barcode").First.ClickAsync();

        // Then the system verifies the five rights of medication administration:
        //   | Right          | Verification                                  | Status   |
        //   | Right Patient  | Maria Gonzalez confirmed via wristband       | ✓ Valid  |
        //   | Right Drug     | Metoprolol matches prescribed medication     | ✓ Valid  |
        //   | Right Dose     | 25mg matches prescribed dose                 | ✓ Valid  |
        //   | Right Route    | PO (oral) matches prescribed route           | ✓ Valid  |
        //   | Right Time     | Within acceptable window (13:30-14:30)      | ✓ Valid  |
        await WaitForTestId(page, "Right Patient Status");
        Assert.That((await GetText(page, "Right Patient Status")), Is.EqualTo("✓ Valid"));
        Assert.That((await GetText(page, "Right Drug Status")), Is.EqualTo("✓ Valid"));
        Assert.That((await GetText(page, "Right Dose Status")), Is.EqualTo("✓ Valid"));
        Assert.That((await GetText(page, "Right Route Status")), Is.EqualTo("✓ Valid"));
        Assert.That((await GetText(page, "Right Time Status")), Is.EqualTo("✓ Valid"));

        // And the system displays confirmation screen:
        //   | Field             | Value                                        |
        //   | Patient           | Maria Gonzalez, DOB: 1975-08-15            |
        //   | Medication        | Metoprolol 25mg                            |
        //   | Route             | PO (By mouth)                              |
        //   | Administration Time| 14:05                                      |
        Assert.That((await GetText(page, "Patient")), Is.EqualTo("Maria Gonzalez, DOB: 1975-08-15"));
        Assert.That((await GetText(page, "Medication")), Is.EqualTo("Metoprolol 25mg"));
        Assert.That((await GetText(page, "Route")), Is.EqualTo("PO (By mouth)"));
        Assert.That((await GetText(page, "Administration Time")), Is.EqualTo("14:05"));

        // And I confirm the administration
        await page.GetByTestId("confirm-administration").First.ClickAsync();

        // Then the system records the administration with timestamp:
        //   | Field             | Value                                        |
        //   | Patient ID        | PT-12345                                   |
        //   | Medication        | Metoprolol 25mg PO                         |
        //   | Administered By   | Nurse Johnson                              |
        //   | Administration Time| 2025-06-24 14:05:32                       |
        //   | Verification Method| Barcode scan                              |
        await WaitForTestId(page, "Patient ID");
        Assert.That((await GetText(page, "Patient ID")), Is.EqualTo("PT-12345"));
        Assert.That((await GetText(page, "Medication")), Is.EqualTo("Metoprolol 25mg PO"));
        Assert.That((await GetText(page, "Administered By")), Is.EqualTo("Nurse Johnson"));
        Assert.That((await GetText(page, "Administration Time")), Is.EqualTo("2025-06-24 14:05:32"));
        Assert.That((await GetText(page, "Verification Method")), Is.EqualTo("Barcode scan"));

        // And the medication status is updated to "Given"
        Assert.That((await GetText(page, "Medication Status")), Is.EqualTo("Given"));
        // And the next dose is automatically scheduled for "02:00 tomorrow"
        Assert.That((await GetText(page, "Next Dose Scheduled")), Is.EqualTo("02:00 tomorrow"));
    }

    [Test, Order(2)]
    [Description("Handle medication administration with allergy alert")]
    public async Task HandleMedicationAdministrationWithAllergyAlert()
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
        await page.GetByTestId("scan-wristband-barcode").First.ClickAsync();
        // And I scan the amoxicillin medication barcode
        await page.GetByTestId("scan-medication-barcode").First.ClickAsync();

        // Then the system displays an allergy alert:
        //   | Alert Type        | Message                                      |
        //   | DRUG ALLERGY      | ⚠️ WARNING: Patient allergic to Penicillin |
        //   | Cross-Reaction    | Amoxicillin contains penicillin             |
        //   | Severity          | Moderate - Rash, hives                      |
        //   | Recommendation    | Contact physician before administration      |
        await WaitForTestId(page, "DRUG ALLERGY");
        Assert.That((await GetText(page, "DRUG ALLERGY")), Is.EqualTo("⚠️ WARNING: Patient allergic to Penicillin"));
        Assert.That((await GetText(page, "Cross-Reaction")), Is.EqualTo("Amoxicillin contains penicillin"));
        Assert.That((await GetText(page, "Severity")), Is.EqualTo("Moderate - Rash, hives"));
        Assert.That((await GetText(page, "Recommendation")), Is.EqualTo("Contact physician before administration"));

        // And the system requires additional verification:
        //   | Verification Step | Requirement                                  |
        //   | Physician Approval| Must have override from prescribing MD      |
        //   | Documentation     | Must document reason for override           |
        //   | Monitoring Plan   | Must specify allergy monitoring protocol    |
        Assert.That((await GetText(page, "Physician Approval")), Is.EqualTo("Must have override from prescribing MD"));
        Assert.That((await GetText(page, "Documentation")), Is.EqualTo("Must document reason for override"));
        Assert.That((await GetText(page, "Monitoring Plan")), Is.EqualTo("Must specify allergy monitoring protocol"));

        // And the medication administration is held pending physician confirmation
        var administrationStatus = await GetText(page, "Administration Status");
        Assert.That(administrationStatus, Does.Match(@"pending physician confirmation").IgnoreCase);
        // And an alert is sent to the prescribing physician "Dr. Smith"
        var physicianAlert = await GetText(page, "Physician Alert");
        Assert.That(physicianAlert, Does.Match(@"Dr\. Smith"));
    }

    [Test, Order(3)]
    [Description("Administer PRN medication with clinical assessment")]
    public async Task AdministerPRNMedicationWithClinicalAssessment()
    {
        // Given a patient "Jennifer Lopez" is in bed "ED-6"
        // And there is a PRN medication order:
        //   | Medication     | Dose    | Route | Indication           | Max Frequency |
        //   | Morphine       | 2mg     | IV    | Pain score ≥7/10     | Q4H PRN       |
        // And the patient's current pain score is "8/10"
        // And the last morphine dose was given 5 hours ago

        // When I scan the patient's wristband barcode
        await page.GetByTestId("scan-wristband-barcode").First.ClickAsync();
        // And I scan the morphine medication barcode
        await page.GetByTestId("scan-medication-barcode").First.ClickAsync();

        // Then the system verifies PRN medication criteria:
        //   | Criteria          | Assessment                                   | Status   |
        //   | Clinical Indication| Pain score 8/10 meets threshold ≥7/10      | ✓ Met    |
        //   | Frequency Limit   | Last dose 5h ago, within Q4H limit         | ✓ Valid  |
        //   | Patient Safety    | No respiratory contraindications            | ✓ Safe   |
        await WaitForTestId(page, "Clinical Indication Status");
        Assert.That((await GetText(page, "Clinical Indication Status")), Is.EqualTo("✓ Met"));
        Assert.That((await GetText(page, "Frequency Limit Status")), Is.EqualTo("✓ Valid"));
        Assert.That((await GetText(page, "Patient Safety Status")), Is.EqualTo("✓ Safe"));

        // And I document the clinical assessment:
        //   | Assessment Field  | Value                                        |
        //   | Pain Score        | 8/10                                        |
        //   | Pain Location     | Chest                                       |
        //   | Pain Quality      | Sharp, stabbing                             |
        //   | Vital Signs       | BP: 140/85, HR: 95, RR: 18, O2: 96%       |
        await FillFields(page, new List<Dictionary<string, string>>
        {
            Row(("Field", "Pain Score"), ("Value", "8/10")),
            Row(("Field", "Pain Location"), ("Value", "Chest")),
            Row(("Field", "Pain Quality"), ("Value", "Sharp, stabbing")),
            Row(("Field", "Vital Signs"), ("Value", "BP: 140/85, HR: 95, RR: 18, O2: 96%")),
        });

        // And I confirm the PRN administration
        await page.GetByTestId("confirm-administration").First.ClickAsync();

        // Then the system records the administration with clinical justification:
        //   | Documentation     | Details                                      |
        //   | PRN Indication    | Pain score 8/10, patient requested relief   |
        //   | Clinical Assessment| Stable vitals, no respiratory distress     |
        //   | Next Available    | Not before 18:15 (Q4H)                     |
        await WaitForTestId(page, "PRN Indication");
        Assert.That((await GetText(page, "PRN Indication")), Is.EqualTo("Pain score 8/10, patient requested relief"));
        Assert.That((await GetText(page, "Clinical Assessment")), Is.EqualTo("Stable vitals, no respiratory distress"));
        Assert.That((await GetText(page, "Next Available")), Is.EqualTo("Not before 18:15 (Q4H)"));
    }

    [Test, Order(4)]
    [Description("Handle medication administration timing variances")]
    public async Task HandleMedicationAdministrationTimingVariances()
    {
        // Given a patient "David Kim" is in bed "ED-3"
        // And there is a scheduled medication:
        //   | Medication    | Scheduled Time | Acceptable Window | Current Time |
        //   | Insulin       | 12:00         | ±30 minutes       | 13:15        |

        // When I scan the patient's wristband barcode
        await page.GetByTestId("scan-wristband-barcode").First.ClickAsync();
        // And I scan the insulin medication barcode
        await page.GetByTestId("scan-medication-barcode").First.ClickAsync();

        // Then the system detects a timing variance:
        //   | Variance Type     | Details                                      |
        //   | Late Administration| 75 minutes past scheduled time             |
        //   | Outside Window    | Beyond acceptable ±30 minute window         |
        //   | Clinical Risk     | Delayed insulin may affect glucose control  |
        await WaitForTestId(page, "Late Administration");
        Assert.That((await GetText(page, "Late Administration")), Is.EqualTo("75 minutes past scheduled time"));
        Assert.That((await GetText(page, "Outside Window")), Is.EqualTo("Beyond acceptable ±30 minute window"));
        Assert.That((await GetText(page, "Clinical Risk")), Is.EqualTo("Delayed insulin may affect glucose control"));

        // And the system requires variance documentation:
        //   | Required Field    | Purpose                                      |
        //   | Delay Reason      | Why medication was not given on time        |
        //   | Patient Status    | Current clinical condition assessment       |
        //   | Physician Notification| Whether MD was contacted about delay    |
        Assert.That((await GetText(page, "Delay Reason Purpose")), Is.EqualTo("Why medication was not given on time"));
        Assert.That((await GetText(page, "Patient Status Purpose")), Is.EqualTo("Current clinical condition assessment"));
        Assert.That((await GetText(page, "Physician Notification Purpose")), Is.EqualTo("Whether MD was contacted about delay"));

        // And I document the variance:
        //   | Field             | Value                                        |
        //   | Delay Reason      | Patient NPO for procedure until 13:00      |
        //   | Patient Assessment| Blood glucose 140 mg/dL, stable            |
        //   | MD Notified       | Dr. Patel aware of delay                    |
        await FillFields(page, new List<Dictionary<string, string>>
        {
            Row(("Field", "Delay Reason"), ("Value", "Patient NPO for procedure until 13:00")),
            Row(("Field", "Patient Assessment"), ("Value", "Blood glucose 140 mg/dL, stable")),
            Row(("Field", "MD Notified"), ("Value", "Dr. Patel aware of delay")),
        });

        // Then the administration is recorded with variance documentation
        await WaitForTestId(page, "Variance Documentation Status");
        var varianceDocumentationStatus = await GetText(page, "Variance Documentation Status");
        Assert.That(varianceDocumentationStatus.Length > 0, Is.True);
        // And the next scheduled dose timing is adjusted accordingly
        var nextScheduledDose = await GetText(page, "Next Scheduled Dose");
        Assert.That(nextScheduledDose.Length > 0, Is.True);
    }

    [Test, Order(5)]
    [Description("Administer high-risk medication with double verification")]
    public async Task AdministerHighRiskMedicationWithDoubleVerification()
    {
        // Given a patient "Susan Williams" is in bed "ED-15"
        // And there is a high-risk medication order:
        //   | Medication    | Dose      | Route | Risk Category        | Special Requirements |
        //   | Heparin       | 5000 units| IV    | High-alert drug      | Double verification  |

        // When I scan the patient's wristband barcode
        await page.GetByTestId("scan-wristband-barcode").First.ClickAsync();
        // And I scan the heparin medication barcode
        await page.GetByTestId("scan-medication-barcode").First.ClickAsync();

        // Then the system flags the high-risk medication:
        //   | Alert Type        | Message                                      |
        //   | HIGH-ALERT DRUG   | 🔴 Heparin requires double verification     |
        //   | Risk Factors      | Bleeding risk, dosing errors common         |
        //   | Requirements      | Second nurse must verify before administration|
        await WaitForTestId(page, "HIGH-ALERT DRUG");
        Assert.That((await GetText(page, "HIGH-ALERT DRUG")), Is.EqualTo("🔴 Heparin requires double verification"));
        Assert.That((await GetText(page, "Risk Factors")), Is.EqualTo("Bleeding risk, dosing errors common"));
        Assert.That((await GetText(page, "Requirements")), Is.EqualTo("Second nurse must verify before administration"));

        // And the system requires independent double verification:
        //   | Verification Step | Nurse 1 (Primary) | Nurse 2 (Verifying) |
        //   | Patient Identity  | Nurse Johnson      | Pending             |
        //   | Medication        | Verified           | Pending             |
        //   | Dose Calculation  | 5000 units         | Pending             |
        //   | Route/Rate        | IV bolus           | Pending             |
        Assert.That((await GetText(page, "Patient Identity Primary Nurse")), Is.EqualTo("Nurse Johnson"));
        Assert.That((await GetText(page, "Patient Identity Verifying Nurse")), Is.EqualTo("Pending"));
        Assert.That((await GetText(page, "Medication Primary Nurse")), Is.EqualTo("Verified"));
        Assert.That((await GetText(page, "Medication Verifying Nurse")), Is.EqualTo("Pending"));
        Assert.That((await GetText(page, "Dose Calculation Primary Nurse")), Is.EqualTo("5000 units"));
        Assert.That((await GetText(page, "Dose Calculation Verifying Nurse")), Is.EqualTo("Pending"));
        Assert.That((await GetText(page, "Route/Rate Primary Nurse")), Is.EqualTo("IV bolus"));
        Assert.That((await GetText(page, "Route/Rate Verifying Nurse")), Is.EqualTo("Pending"));

        // When "Nurse Martinez" performs the second verification
        await FillField(page, "Verifying Nurse", "Nurse Martinez");
        await page.GetByTestId("submit-second-verification").First.ClickAsync();
        // And both nurses confirm the administration
        await page.GetByTestId("confirm-administration").First.ClickAsync();

        // Then the system records dual verification:
        //   | Field             | Value                                        |
        //   | Primary Nurse     | Nurse Johnson                               |
        //   | Verifying Nurse   | Nurse Martinez                              |
        //   | Verification Time | 2025-06-24 15:30:15                        |
        //   | Both Signatures   | Electronic signatures captured              |
        await WaitForTestId(page, "Primary Nurse");
        Assert.That((await GetText(page, "Primary Nurse")), Is.EqualTo("Nurse Johnson"));
        Assert.That((await GetText(page, "Verifying Nurse")), Is.EqualTo("Nurse Martinez"));
        Assert.That((await GetText(page, "Verification Time")), Is.EqualTo("2025-06-24 15:30:15"));
        Assert.That((await GetText(page, "Both Signatures")), Is.EqualTo("Electronic signatures captured"));
    }

    [Test, Order(6)]
    [Description("Handle medication barcode scanning errors")]
    public async Task HandleMedicationBarcodeScanningErrors()
    {
        // Given a patient "Michael Davis" is in bed "ED-11"
        // And I need to administer prescribed medication

        // When I scan the patient's wristband barcode successfully
        await page.GetByTestId("scan-wristband-barcode").First.ClickAsync();
        // And I attempt to scan a medication barcode that is damaged/unreadable
        await page.GetByTestId("scan-medication-barcode").First.ClickAsync();

        // Then the system displays a barcode error:
        //   | Error Type        | Message                                      |
        //   | Barcode Unreadable| Unable to scan medication barcode           |
        //   | Manual Options    | Enter medication manually or get replacement|
        //   | Safety Warning    | Manual entry bypasses barcode verification  |
        await WaitForTestId(page, "Barcode Unreadable");
        Assert.That((await GetText(page, "Barcode Unreadable")), Is.EqualTo("Unable to scan medication barcode"));
        Assert.That((await GetText(page, "Manual Options")), Is.EqualTo("Enter medication manually or get replacement"));
        Assert.That((await GetText(page, "Safety Warning")), Is.EqualTo("Manual entry bypasses barcode verification"));

        // And I choose to manually enter the medication information:
        //   | Manual Entry Field| Value                                        |
        //   | Medication Name   | Tylenol                                     |
        //   | Strength          | 650mg                                       |
        //   | NDC Number        | 50580-506-02                               |
        //   | Lot Number        | ABC123                                      |
        //   | Expiration Date   | 2026-12-31                                 |
        await FillFields(page, new List<Dictionary<string, string>>
        {
            Row(("Field", "Medication Name"), ("Value", "Tylenol")),
            Row(("Field", "Strength"), ("Value", "650mg")),
            Row(("Field", "NDC Number"), ("Value", "50580-506-02")),
            Row(("Field", "Lot Number"), ("Value", "ABC123")),
            Row(("Field", "Expiration Date"), ("Value", "2026-12-31")),
        });

        // Then the system validates the manual entry against the order
        var manualEntryValidation = await WaitForTestId(page, "Manual Entry Validation");
        Assert.That((await manualEntryValidation.IsVisibleAsync()), Is.True);
        // And requires supervisor override for manual medication entry
        await WaitForTestId(page, "Supervisor Override");
        // And documents the barcode scanning issue for pharmacy review
        var pharmacyReviewFlag = await WaitForTestId(page, "Pharmacy Review Flag");
        Assert.That((await pharmacyReviewFlag.IsVisibleAsync()), Is.True);
    }

    [Test, Order(7)]
    [Description("Administer pediatric medication with weight-based verification")]
    public async Task AdministerPediatricMedicationWithWeightBasedVerification()
    {
        // Given a pediatric patient "Emma Foster" (age 5, weight 18kg) is in bed "ED-PEDS-1"
        // And there is a weight-based medication order:
        //   | Medication     | Dose Calculation    | Prescribed Dose | Route |
        //   | Acetaminophen  | 15mg/kg            | 270mg          | PO    |

        // When I scan the patient's wristband barcode
        await page.GetByTestId("scan-wristband-barcode").First.ClickAsync();
        // And I scan the acetaminophen medication barcode
        await page.GetByTestId("scan-medication-barcode").First.ClickAsync();

        // Then the system verifies pediatric dosing:
        //   | Verification       | Calculation                                  | Status   |
        //   | Weight Confirmation| Patient weight: 18kg                        | ✓ Valid  |
        //   | Dose Calculation   | 15mg/kg × 18kg = 270mg                     | ✓ Correct|
        //   | Maximum Safe Dose  | 270mg < 400mg max (safe)                    | ✓ Safe   |
        //   | Age Appropriateness| Acetaminophen approved for age 5            | ✓ Valid  |
        await WaitForTestId(page, "Weight Confirmation Status");
        Assert.That((await GetText(page, "Weight Confirmation Status")), Is.EqualTo("✓ Valid"));
        Assert.That((await GetText(page, "Dose Calculation Status")), Is.EqualTo("✓ Correct"));
        Assert.That((await GetText(page, "Maximum Safe Dose Status")), Is.EqualTo("✓ Safe"));
        Assert.That((await GetText(page, "Age Appropriateness Status")), Is.EqualTo("✓ Valid"));

        // And the system displays pediatric-specific information:
        //   | Field              | Value                                        |
        //   | Patient Age/Weight | 5 years old, 18kg                          |
        //   | Calculation Shown  | 15mg/kg × 18kg = 270mg                     |
        //   | Liquid Formulation | 160mg/5mL suspension                       |
        //   | Volume to Give     | 8.4mL                                       |
        Assert.That((await GetText(page, "Patient Age/Weight")), Is.EqualTo("5 years old, 18kg"));
        Assert.That((await GetText(page, "Calculation Shown")), Is.EqualTo("15mg/kg × 18kg = 270mg"));
        Assert.That((await GetText(page, "Liquid Formulation")), Is.EqualTo("160mg/5mL suspension"));
        Assert.That((await GetText(page, "Volume to Give")), Is.EqualTo("8.4mL"));

        // And I confirm the pediatric administration
        await page.GetByTestId("confirm-administration").First.ClickAsync();

        // Then the system records with pediatric-specific documentation
        var pediatricDocumentation = await WaitForTestId(page, "Pediatric Documentation");
        Assert.That((await pediatricDocumentation.IsVisibleAsync()), Is.True);
    }

    [Test, Order(8)]
    [Description("Handle medication administration during code blue emergency")]
    public async Task HandleMedicationAdministrationDuringCodeBlueEmergency()
    {
        // Given a patient "Crisis Patient" is in bed "ED-TRAUMA-1"
        // And a code blue emergency is in progress
        // And emergency medications are ordered:
        //   | Medication    | Dose      | Route | Urgency    |
        //   | Epinephrine   | 1mg       | IV    | STAT       |
        //   | Atropine      | 0.5mg     | IV    | STAT       |

        // When I scan the patient's wristband during the emergency
        await page.GetByTestId("scan-wristband-barcode").First.ClickAsync();
        // And I scan the epinephrine medication barcode
        await page.GetByTestId("scan-medication-barcode").First.ClickAsync();

        // Then the system activates emergency administration mode:
        //   | Emergency Feature | Behavior                                     |
        //   | Rapid Verification| Abbreviated safety checks for life-saving   |
        //   | Time Documentation| Precise timestamp for code blue timeline    |
        //   | Team Notification | Alert code team of medication administration |
        await WaitForTestId(page, "Rapid Verification");
        Assert.That((await GetText(page, "Rapid Verification")), Is.EqualTo("Abbreviated safety checks for life-saving"));
        Assert.That((await GetText(page, "Time Documentation")), Is.EqualTo("Precise timestamp for code blue timeline"));
        Assert.That((await GetText(page, "Team Notification")), Is.EqualTo("Alert code team of medication administration"));

        // And the system allows emergency override of timing restrictions
        await WaitForTestId(page, "Emergency Override");

        // And records the administration with code blue context:
        //   | Field             | Value                                        |
        //   | Emergency Context | Code Blue - Cardiac arrest                  |
        //   | Rapid Administration| Life-saving intervention                   |
        //   | Code Blue Timeline| 15:45:32 - Epinephrine given               |
        Assert.That((await GetText(page, "Emergency Context")), Is.EqualTo("Code Blue - Cardiac arrest"));
        Assert.That((await GetText(page, "Rapid Administration")), Is.EqualTo("Life-saving intervention"));
        Assert.That((await GetText(page, "Code Blue Timeline")), Is.EqualTo("15:45:32 - Epinephrine given"));

        // And the code blue medication log is automatically updated
        var codeBlueMedicationLog = await WaitForTestId(page, "Code Blue Medication Log");
        Assert.That((await codeBlueMedicationLog.IsVisibleAsync()), Is.True);
    }
}
