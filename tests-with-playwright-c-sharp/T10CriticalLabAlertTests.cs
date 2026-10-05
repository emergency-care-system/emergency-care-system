// Playwright + NUnit test for
// tests-with-given-when-then-features/10-critical-lab-alert.feature
// (equivalent to tests-with-playwright-javascript/10-critical-lab-alert.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see Support/Fields.cs) and the shared
// data-testid contract in Support/Login.cs (login-identity, login-submit,
// app-root).

namespace EmergencyCareSystem.PlaywrightTests;

[TestFixture]
[NonParallelizable]
public class T10CriticalLabAlertTests
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
        //   And the critical value alert system is enabled
        //   And laboratory interfaces are functioning
        //   And all patient displays are connected to the alert system
        await VerifySystemIsOperational(page);
        // The critical value alert system, laboratory interfaces, and patient
        // display connections are assumed pre-seeded test data / environment
        // configuration. This feature has no "logged in as" step in its
        // Background.
        await Login(page, "a lab technician");

        var criticalLabAlertNavLink = await WaitForTestId(page, "Nav Critical Lab Alert");
        await criticalLabAlertNavLink.ClickAsync();
        await WaitForTestId(page, "Critical Lab Alert Panel");
    }

    [Test, Order(1)]
    [Description("Process critically high troponin result with immediate alerts")]
    public async Task ProcessCriticallyHighTroponinResultWithImmediateAlerts()
    {
        // Given a patient "Robert Martinez" is in bed "ED-7"
        // And the attending physician is "Dr. Johnson"
        // And the charge nurse is "Nurse Williams"
        // And troponin was ordered for "chest pain evaluation"
        // (assumed pre-seeded test data)

        // When the laboratory result is received:
        //   | Test Name       | Result | Reference Range | Units  | Critical Threshold | Status |
        //   | Troponin I      | 5.8    | 0.0-0.04       | ng/mL  | >0.4              | Final  |
        await FillFields(page, new List<Dictionary<string, string>>
        {
            Row(("Field", "Test Name"), ("Value", "Troponin I")),
            Row(("Field", "Result"), ("Value", "5.8")),
            Row(("Field", "Reference Range"), ("Value", "0.0-0.04")),
            Row(("Field", "Units"), ("Value", "ng/mL")),
            Row(("Field", "Critical Threshold"), ("Value", ">0.4")),
            Row(("Field", "Status"), ("Value", "Final")),
        });
        await page.GetByTestId("receive-lab-result-button").First.ClickAsync();

        // Then the system immediately triggers critical alerts
        var criticalAlertStatus = await GetText(page, "Critical Alert Status");
        Assert.That(criticalAlertStatus, Does.Match(@"triggered").IgnoreCase);

        // And the attending physician "Dr. Johnson" receives immediate notifications:
        //   | Notification Type | Content                                        | Delivery Method |
        //   | Mobile Push Alert | 🔴 CRITICAL: Troponin I 5.8 ng/mL - ED-7     | Mobile App      |
        //   | SMS Alert        | CRITICAL LAB: R.Martinez ED-7 Troponin 5.8    | Text Message    |
        //   | Popup Alert      | CRITICAL VALUE - Requires Acknowledgment       | Workstation     |
        Assert.That((await GetText(page, "Mobile Push Alert")), Is.EqualTo("🔴 CRITICAL: Troponin I 5.8 ng/mL - ED-7"));
        Assert.That((await GetText(page, "SMS Alert")), Is.EqualTo("CRITICAL LAB: R.Martinez ED-7 Troponin 5.8"));
        Assert.That((await GetText(page, "Popup Alert")), Is.EqualTo("CRITICAL VALUE - Requires Acknowledgment"));

        // And the charge nurse "Nurse Williams" receives critical notifications:
        //   | Notification Type | Content                                        | Delivery Method |
        //   | Desktop Alert    | CRITICAL: Troponin 5.8 - Bed ED-7            | Workstation     |
        //   | Overhead Page    | Critical lab value bed ED-7                   | PA System       |
        //   | Mobile Alert     | Critical troponin result requires attention   | Mobile Device   |
        Assert.That((await GetText(page, "Desktop Alert")), Is.EqualTo("CRITICAL: Troponin 5.8 - Bed ED-7"));
        Assert.That((await GetText(page, "Overhead Page")), Is.EqualTo("Critical lab value bed ED-7"));
        Assert.That((await GetText(page, "Mobile Alert")), Is.EqualTo("Critical troponin result requires attention"));

        // And red flag indicators appear on all patient displays:
        //   | Display Location     | Alert Indicator                              |
        //   | Patient Monitor      | 🔴 CRITICAL LAB flashing red banner         |
        //   | Bedside Workstation  | Red alert icon next to patient name         |
        //   | Main ED Dashboard    | Red flag on bed ED-7 status                 |
        //   | Mobile Devices       | Red notification badge on patient chart     |
        //   | Nursing Station      | Critical value alert on patient board       |
        var redFlagIndicators = page.GetByTestId("red-flag-indicator");
        Assert.That(await redFlagIndicators.CountAsync(), Is.EqualTo(5));
    }

    [Test, Order(2)]
    [Description("Handle critical troponin with physician acknowledgment requirements")]
    public async Task HandleCriticalTroponinWithPhysicianAcknowledgmentRequirements()
    {
        // Given a patient "Maria Santos" is in bed "ED-12"
        // And the attending physician is "Dr. Lee"
        // And a critically high troponin result of "7.2 ng/mL" is received
        // (assumed pre-seeded test data)

        // When the critical alert is triggered
        await page.GetByTestId("trigger-critical-alert-button").First.ClickAsync();

        // Then the system requires physician acknowledgment:
        Assert.That((await GetText(page, "Initial Alert")), Is.EqualTo("Must acknowledge receipt within 15 minutes"));
        Assert.That((await GetText(page, "Clinical Review")), Is.EqualTo("Must document result review"));
        Assert.That((await GetText(page, "Action Plan")), Is.EqualTo("Must indicate next steps taken"));

        // And if "Dr. Lee" does not acknowledge within 15 minutes:
        Assert.That((await GetText(page, "Secondary Alert")), Is.EqualTo("Alert sent to backup physician"));
        Assert.That((await GetText(page, "Charge Nurse Alert")), Is.EqualTo("Escalation notice to charge nurse"));
        Assert.That((await GetText(page, "Supervisor Alert")), Is.EqualTo("Department supervisor notified"));

        // And the acknowledgment status is tracked:
        //   | Status              | Timestamp | Provider    | Action              |
        //   | Alert Sent          | 14:30:15  | System      | Initial notification|
        //   | Acknowledged        | 14:32:45  | Dr. Lee     | Acknowledged receipt|
        //   | Reviewed            | 14:35:20  | Dr. Lee     | Documented review   |
        //   | Action Taken        | 14:40:10  | Dr. Lee     | Treatment initiated |
        var acknowledgmentStatusEntries = page.GetByTestId("acknowledgment-status-entry");
        Assert.That(await acknowledgmentStatusEntries.CountAsync(), Is.EqualTo(4));
    }

    [Test, Order(3)]
    [Description("Process multiple critical values simultaneously")]
    public async Task ProcessMultipleCriticalValuesSimultaneously()
    {
        // Given multiple patients have critical troponin results:
        //   | Patient Name    | Bed   | Troponin Result | Attending     | Severity  |
        //   | John Williams   | ED-3  | 3.2 ng/mL      | Dr. Adams     | High      |
        //   | Lisa Johnson    | ED-8  | 8.9 ng/mL      | Dr. Brown     | Critical  |
        //   | Mike Davis      | ED-15 | 4.1 ng/mL      | Dr. Adams     | High      |
        // (assumed pre-seeded test data)

        // When all critical results are received simultaneously
        await page.GetByTestId("receive-critical-results-button").First.ClickAsync();

        // Then the system prioritizes alerts by severity:
        //   | Priority | Patient      | Alert Level | Notification Urgency    |
        //   | 1        | Lisa Johnson | Critical    | Immediate - All channels|
        //   | 2        | Mike Davis   | High        | Urgent - Standard alerts|
        //   | 3        | John Williams| High        | Urgent - Standard alerts|
        Assert.That((await GetText(page, "Priority 1 Patient")), Is.EqualTo("Lisa Johnson"));
        Assert.That((await GetText(page, "Priority 1 Alert Level")), Is.EqualTo("Critical"));
        Assert.That((await GetText(page, "Priority 2 Patient")), Is.EqualTo("Mike Davis"));
        Assert.That((await GetText(page, "Priority 2 Alert Level")), Is.EqualTo("High"));
        Assert.That((await GetText(page, "Priority 3 Patient")), Is.EqualTo("John Williams"));
        Assert.That((await GetText(page, "Priority 3 Alert Level")), Is.EqualTo("High"));

        // And physicians receive prioritized notifications:
        Assert.That((await GetText(page, "Dr. Brown Alert Summary")), Is.EqualTo("CRITICAL: Lisa Johnson Trop 8.9 - IMMEDIATE"));
        Assert.That((await GetText(page, "Dr. Adams Alert Summary")), Is.EqualTo("HIGH: 2 patients with elevated troponin"));

        // And the charge nurse receives a summary alert:
        Assert.That((await GetText(page, "Mass Alert")), Is.EqualTo("3 critical troponin results requiring attention"));
        Assert.That((await GetText(page, "Priority List")), Is.EqualTo("Lisa Johnson (Critical), others (High)"));

        // And all patient displays show appropriately color-coded flags
        var colorCodedFlags = page.GetByTestId("color-coded-flag");
        Assert.That(await colorCodedFlags.CountAsync(), Is.EqualTo(3));
    }

    [Test, Order(4)]
    [Description("Handle critical troponin during shift change")]
    public async Task HandleCriticalTroponinDuringShiftChange()
    {
        // Given a patient "Catherine Brown" is in bed "ED-6"
        // And it is 19:00 during evening shift change
        // And the day shift physician "Dr. Wilson" ordered the troponin
        // And the evening shift physician "Dr. Taylor" has assumed care
        // (assumed pre-seeded test data)

        // When a critically high troponin result of "6.1 ng/mL" is received
        await FillField(page, "Troponin Result", "6.1 ng/mL");
        await page.GetByTestId("receive-lab-result-button").First.ClickAsync();

        // Then both physicians receive critical alerts:
        //   | Physician  | Alert Type    | Content                                |
        //   | Dr. Taylor | Primary Alert | CRITICAL Troponin 6.1 - Your patient  |
        //   | Dr. Wilson | Handoff Alert | FYI: Critical result on your order     |
        Assert.That((await GetText(page, "Dr. Taylor Alert")), Is.EqualTo("CRITICAL Troponin 6.1 - Your patient"));
        Assert.That((await GetText(page, "Dr. Wilson Alert")), Is.EqualTo("FYI: Critical result on your order"));

        // And the charge nurse receives handoff-specific notification:
        Assert.That((await GetText(page, "Shift Context")), Is.EqualTo("Critical result during physician handoff"));
        Assert.That((await GetText(page, "Current MD")), Is.EqualTo("Dr. Taylor (assuming care)"));
        Assert.That((await GetText(page, "Ordering MD")), Is.EqualTo("Dr. Wilson (ordered test)"));

        // And the handoff documentation is automatically updated
        var handoffDocumentation = page.GetByTestId("handoff-documentation").First;
        Assert.That((await handoffDocumentation.IsVisibleAsync()), Is.True);

        // And red flags appear with shift change context indicators
        var redFlagIndicators = page.GetByTestId("red-flag-indicator");
        Assert.That(await redFlagIndicators.CountAsync() > 0, Is.True);
    }

    [Test, Order(5)]
    [Description("Process critical troponin with additional cardiac markers")]
    public async Task ProcessCriticalTroponinWithAdditionalCardiacMarkers()
    {
        // Given a patient "Steven Kim" is in bed "ED-11"
        // And multiple cardiac markers were ordered
        // (assumed pre-seeded test data)

        // When critical and related results are received:
        //   | Test Name    | Result | Reference Range | Critical | Clinical Significance |
        //   | Troponin I   | 4.7    | 0.0-0.04       | Yes      | Acute MI indicated    |
        //   | CK-MB        | 45     | 0-6.3          | Yes      | Myocardial damage     |
        //   | Myoglobin    | 280    | 25-72          | No       | Elevated but not critical|
        await page.GetByTestId("receive-lab-results-button").First.ClickAsync();

        // Then the system groups related critical values:
        //   | Alert Category  | Content                                      |
        //   | Cardiac Panel   | Multiple critical cardiac markers            |
        //   | Primary Alert   | Troponin I: 4.7 ng/mL (CRITICAL)           |
        //   | Secondary Alert | CK-MB: 45 ng/mL (CRITICAL)                 |
        //   | Supporting Data | Myoglobin: 280 ng/mL (Elevated)            |
        Assert.That((await GetText(page, "Cardiac Panel")), Is.EqualTo("Multiple critical cardiac markers"));
        Assert.That((await GetText(page, "Primary Alert")), Is.EqualTo("Troponin I: 4.7 ng/mL (CRITICAL)"));
        Assert.That((await GetText(page, "Secondary Alert")), Is.EqualTo("CK-MB: 45 ng/mL (CRITICAL)"));
        Assert.That((await GetText(page, "Supporting Data")), Is.EqualTo("Myoglobin: 280 ng/mL (Elevated)"));

        // And enhanced clinical context is provided:
        Assert.That((await GetText(page, "Clinical Indication")), Is.EqualTo("Acute myocardial infarction likely"));
        Assert.That((await GetText(page, "Recommended Actions")), Is.EqualTo("Cardiology consult, STEMI protocol"));
        Assert.That((await GetText(page, "Time Sensitivity")), Is.EqualTo("Treatment within 90 minutes critical"));

        // And STEMI protocol alerts are automatically triggered
        await WaitForTestId(page, "STEMI Protocol Alert");
    }

    [Test, Order(6)]
    [Description("Handle false positive critical troponin alerts")]
    public async Task HandleFalsePositiveCriticalTroponinAlerts()
    {
        // Given a patient "Nancy Rodriguez" is in bed "ED-4"
        // And a troponin result of "5.1 ng/mL" triggers a critical alert
        // (assumed pre-seeded test data)

        // When the laboratory calls to report a sample error
        await page.GetByTestId("report-sample-error-button").First.ClickAsync();

        // And a corrected result shows "0.03 ng/mL" (normal)
        await FillField(page, "Corrected Troponin Result", "0.03 ng/mL");
        await page.GetByTestId("submit-correction-button").First.ClickAsync();

        // Then the system processes the correction:
        Assert.That((await GetText(page, "Cancel Alert")), Is.EqualTo("Original critical alert is cancelled"));
        Assert.That((await GetText(page, "Send Correction")), Is.EqualTo("Corrected value sent to all recipients"));
        Assert.That((await GetText(page, "Document Error")), Is.EqualTo("Lab error documented in audit trail"));

        // And correction notifications are sent:
        Assert.That((await GetText(page, "Alert Cancellation")), Is.EqualTo("CANCELLED: Previous critical troponin alert"));
        Assert.That((await GetText(page, "Corrected Value")), Is.EqualTo("Troponin corrected to 0.03 ng/mL (Normal)"));
        Assert.That((await GetText(page, "Error Explanation")), Is.EqualTo("Laboratory sample contamination identified"));

        // And red flags are removed from all patient displays
        var redFlagIndicators = page.GetByTestId("red-flag-indicator");
        Assert.That(await redFlagIndicators.CountAsync(), Is.EqualTo(0));

        // And the correction is logged for quality assurance review
        var qualityAssuranceLog = page.GetByTestId("quality-assurance-log").First;
        Assert.That((await qualityAssuranceLog.IsVisibleAsync()), Is.True);
    }

    [Test, Order(7)]
    [Description("Critical troponin with patient transfer requirements")]
    public async Task CriticalTroponinWithPatientTransferRequirements()
    {
        // Given a patient "Timothy Chang" is in bed "ED-9"
        // And a critically high troponin of "9.3 ng/mL" is received
        // And the patient requires immediate transfer to cardiac unit
        // (assumed pre-seeded test data)

        // When the critical alert is processed
        await page.GetByTestId("process-critical-alert-button").First.ClickAsync();

        // Then transfer coordination alerts are included:
        Assert.That((await GetText(page, "Transfer Required")), Is.EqualTo("Patient needs immediate cardiac unit transfer"));
        Assert.That((await GetText(page, "Bed Availability")), Is.EqualTo("CCU bed 302 available"));
        Assert.That((await GetText(page, "Transport Time")), Is.EqualTo("Transport team ETA 10 minutes"));

        // And receiving unit notifications are sent:
        Assert.That((await GetText(page, "CCU Alert")), Is.EqualTo("Incoming transfer - Critical troponin 9.3"));
        Assert.That((await GetText(page, "Cardiology Alert")), Is.EqualTo("Urgent consult needed - STEMI protocol"));

        // And transfer documentation is automatically initiated
        var transferDocumentation = page.GetByTestId("transfer-documentation").First;
        Assert.That((await transferDocumentation.IsVisibleAsync()), Is.True);

        // And critical alerts follow the patient to the receiving unit
        await WaitForTestId(page, "Patient Alert Handoff");
    }

    [Test, Order(8)]
    [Description("Validate critical troponin alert system functionality")]
    public async Task ValidateCriticalTroponinAlertSystemFunctionality()
    {
        // Given the critical alert system is being tested
        // (assumed pre-seeded test data)

        // When a test troponin result of "TEST-5.0 ng/mL" is processed
        await FillField(page, "Test Troponin Result", "TEST-5.0 ng/mL");
        await page.GetByTestId("process-test-result-button").First.ClickAsync();

        // Then the system validates all alert pathways:
        Assert.That((await GetText(page, "Physician Mobile")), Is.EqualTo("Test alert delivered successfully"));
        Assert.That((await GetText(page, "Charge Nurse")), Is.EqualTo("Test alert delivered successfully"));
        Assert.That((await GetText(page, "Patient Displays")), Is.EqualTo("Red flags displayed correctly"));
        Assert.That((await GetText(page, "Audit Trail")), Is.EqualTo("Test alert logged with timestamp"));

        // And test alerts are clearly marked as "SYSTEM TEST"
        var testAlertMarking = await GetText(page, "Test Alert Marking");
        Assert.That(testAlertMarking, Is.EqualTo("SYSTEM TEST"));

        // And all test alerts are automatically cleared after validation
        var testAlerts = page.GetByTestId("test-alert");
        Assert.That(await testAlerts.CountAsync(), Is.EqualTo(0));

        // And system performance metrics are recorded:
        //   | Metric           | Measurement                                  |
        //   | Alert Latency    | <30 seconds from result to notification     |
        //   | Delivery Success | 100% successful delivery to all recipients  |
        //   | Display Update   | <5 seconds to update all patient displays   |
        Assert.That((await GetText(page, "Alert Latency")), Is.EqualTo("<30 seconds from result to notification"));
        Assert.That((await GetText(page, "Delivery Success")), Is.EqualTo("100% successful delivery to all recipients"));
        Assert.That((await GetText(page, "Display Update")), Is.EqualTo("<5 seconds to update all patient displays"));
    }
}
