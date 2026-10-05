// Selenium WebDriver + NUnit test for
// tests-with-given-when-then-features/10-critical-lab-alert.feature
// (equivalent to tests-with-selenium-javascript/10-critical-lab-alert.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see Support/Fields.cs) and the shared
// data-testid contract in Support/Login.cs (login-identity, login-submit,
// app-root).

namespace EmergencyCareSystem.SeleniumTests;

[TestFixture]
[NonParallelizable]
public class T10CriticalLabAlertTests
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
        //   And the critical value alert system is enabled
        //   And laboratory interfaces are functioning
        //   And all patient displays are connected to the alert system
        VerifySystemIsOperational(driver);
        // The critical value alert system, laboratory interfaces, and patient
        // display connections are assumed pre-seeded test data / environment
        // configuration. This feature has no "logged in as" step in its
        // Background.
        Login(driver, "a lab technician");

        var criticalLabAlertNavLink = WaitForTestId(driver, "Nav Critical Lab Alert");
        criticalLabAlertNavLink.Click();
        WaitForTestId(driver, "Critical Lab Alert Panel");
    }

    [Test, Order(1)]
    [Description("Process critically high troponin result with immediate alerts")]
    public void ProcessCriticallyHighTroponinResultWithImmediateAlerts()
    {
        // Given a patient "Robert Martinez" is in bed "ED-7"
        // And the attending physician is "Dr. Johnson"
        // And the charge nurse is "Nurse Williams"
        // And troponin was ordered for "chest pain evaluation"
        // (assumed pre-seeded test data)

        // When the laboratory result is received:
        //   | Test Name       | Result | Reference Range | Units  | Critical Threshold | Status |
        //   | Troponin I      | 5.8    | 0.0-0.04       | ng/mL  | >0.4              | Final  |
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Test Name"), ("Value", "Troponin I")),
            Row(("Field", "Result"), ("Value", "5.8")),
            Row(("Field", "Reference Range"), ("Value", "0.0-0.04")),
            Row(("Field", "Units"), ("Value", "ng/mL")),
            Row(("Field", "Critical Threshold"), ("Value", ">0.4")),
            Row(("Field", "Status"), ("Value", "Final")),
        });
        driver.FindElement(By.CssSelector("[data-testid=\"receive-lab-result-button\"]")).Click();

        // Then the system immediately triggers critical alerts
        var criticalAlertStatus = GetText(driver, "Critical Alert Status");
        Assert.That(criticalAlertStatus, Does.Match(@"triggered").IgnoreCase);

        // And the attending physician "Dr. Johnson" receives immediate notifications:
        //   | Notification Type | Content                                        | Delivery Method |
        //   | Mobile Push Alert | 🔴 CRITICAL: Troponin I 5.8 ng/mL - ED-7     | Mobile App      |
        //   | SMS Alert        | CRITICAL LAB: R.Martinez ED-7 Troponin 5.8    | Text Message    |
        //   | Popup Alert      | CRITICAL VALUE - Requires Acknowledgment       | Workstation     |
        Assert.That(GetText(driver, "Mobile Push Alert"), Is.EqualTo("🔴 CRITICAL: Troponin I 5.8 ng/mL - ED-7"));
        Assert.That(GetText(driver, "SMS Alert"), Is.EqualTo("CRITICAL LAB: R.Martinez ED-7 Troponin 5.8"));
        Assert.That(GetText(driver, "Popup Alert"), Is.EqualTo("CRITICAL VALUE - Requires Acknowledgment"));

        // And the charge nurse "Nurse Williams" receives critical notifications:
        //   | Notification Type | Content                                        | Delivery Method |
        //   | Desktop Alert    | CRITICAL: Troponin 5.8 - Bed ED-7            | Workstation     |
        //   | Overhead Page    | Critical lab value bed ED-7                   | PA System       |
        //   | Mobile Alert     | Critical troponin result requires attention   | Mobile Device   |
        Assert.That(GetText(driver, "Desktop Alert"), Is.EqualTo("CRITICAL: Troponin 5.8 - Bed ED-7"));
        Assert.That(GetText(driver, "Overhead Page"), Is.EqualTo("Critical lab value bed ED-7"));
        Assert.That(GetText(driver, "Mobile Alert"), Is.EqualTo("Critical troponin result requires attention"));

        // And red flag indicators appear on all patient displays:
        //   | Display Location     | Alert Indicator                              |
        //   | Patient Monitor      | 🔴 CRITICAL LAB flashing red banner         |
        //   | Bedside Workstation  | Red alert icon next to patient name         |
        //   | Main ED Dashboard    | Red flag on bed ED-7 status                 |
        //   | Mobile Devices       | Red notification badge on patient chart     |
        //   | Nursing Station      | Critical value alert on patient board       |
        var redFlagIndicators = driver.FindElements(By.CssSelector("[data-testid=\"red-flag-indicator\"]"));
        Assert.That(redFlagIndicators.Count, Is.EqualTo(5));
    }

    [Test, Order(2)]
    [Description("Handle critical troponin with physician acknowledgment requirements")]
    public void HandleCriticalTroponinWithPhysicianAcknowledgmentRequirements()
    {
        // Given a patient "Maria Santos" is in bed "ED-12"
        // And the attending physician is "Dr. Lee"
        // And a critically high troponin result of "7.2 ng/mL" is received
        // (assumed pre-seeded test data)

        // When the critical alert is triggered
        driver.FindElement(By.CssSelector("[data-testid=\"trigger-critical-alert-button\"]")).Click();

        // Then the system requires physician acknowledgment:
        Assert.That(GetText(driver, "Initial Alert"), Is.EqualTo("Must acknowledge receipt within 15 minutes"));
        Assert.That(GetText(driver, "Clinical Review"), Is.EqualTo("Must document result review"));
        Assert.That(GetText(driver, "Action Plan"), Is.EqualTo("Must indicate next steps taken"));

        // And if "Dr. Lee" does not acknowledge within 15 minutes:
        Assert.That(GetText(driver, "Secondary Alert"), Is.EqualTo("Alert sent to backup physician"));
        Assert.That(GetText(driver, "Charge Nurse Alert"), Is.EqualTo("Escalation notice to charge nurse"));
        Assert.That(GetText(driver, "Supervisor Alert"), Is.EqualTo("Department supervisor notified"));

        // And the acknowledgment status is tracked:
        //   | Status              | Timestamp | Provider    | Action              |
        //   | Alert Sent          | 14:30:15  | System      | Initial notification|
        //   | Acknowledged        | 14:32:45  | Dr. Lee     | Acknowledged receipt|
        //   | Reviewed            | 14:35:20  | Dr. Lee     | Documented review   |
        //   | Action Taken        | 14:40:10  | Dr. Lee     | Treatment initiated |
        var acknowledgmentStatusEntries = driver.FindElements(By.CssSelector("[data-testid=\"acknowledgment-status-entry\"]"));
        Assert.That(acknowledgmentStatusEntries.Count, Is.EqualTo(4));
    }

    [Test, Order(3)]
    [Description("Process multiple critical values simultaneously")]
    public void ProcessMultipleCriticalValuesSimultaneously()
    {
        // Given multiple patients have critical troponin results:
        //   | Patient Name    | Bed   | Troponin Result | Attending     | Severity  |
        //   | John Williams   | ED-3  | 3.2 ng/mL      | Dr. Adams     | High      |
        //   | Lisa Johnson    | ED-8  | 8.9 ng/mL      | Dr. Brown     | Critical  |
        //   | Mike Davis      | ED-15 | 4.1 ng/mL      | Dr. Adams     | High      |
        // (assumed pre-seeded test data)

        // When all critical results are received simultaneously
        driver.FindElement(By.CssSelector("[data-testid=\"receive-critical-results-button\"]")).Click();

        // Then the system prioritizes alerts by severity:
        //   | Priority | Patient      | Alert Level | Notification Urgency    |
        //   | 1        | Lisa Johnson | Critical    | Immediate - All channels|
        //   | 2        | Mike Davis   | High        | Urgent - Standard alerts|
        //   | 3        | John Williams| High        | Urgent - Standard alerts|
        Assert.That(GetText(driver, "Priority 1 Patient"), Is.EqualTo("Lisa Johnson"));
        Assert.That(GetText(driver, "Priority 1 Alert Level"), Is.EqualTo("Critical"));
        Assert.That(GetText(driver, "Priority 2 Patient"), Is.EqualTo("Mike Davis"));
        Assert.That(GetText(driver, "Priority 2 Alert Level"), Is.EqualTo("High"));
        Assert.That(GetText(driver, "Priority 3 Patient"), Is.EqualTo("John Williams"));
        Assert.That(GetText(driver, "Priority 3 Alert Level"), Is.EqualTo("High"));

        // And physicians receive prioritized notifications:
        Assert.That(GetText(driver, "Dr. Brown Alert Summary"), Is.EqualTo("CRITICAL: Lisa Johnson Trop 8.9 - IMMEDIATE"));
        Assert.That(GetText(driver, "Dr. Adams Alert Summary"), Is.EqualTo("HIGH: 2 patients with elevated troponin"));

        // And the charge nurse receives a summary alert:
        Assert.That(GetText(driver, "Mass Alert"), Is.EqualTo("3 critical troponin results requiring attention"));
        Assert.That(GetText(driver, "Priority List"), Is.EqualTo("Lisa Johnson (Critical), others (High)"));

        // And all patient displays show appropriately color-coded flags
        var colorCodedFlags = driver.FindElements(By.CssSelector("[data-testid=\"color-coded-flag\"]"));
        Assert.That(colorCodedFlags.Count, Is.EqualTo(3));
    }

    [Test, Order(4)]
    [Description("Handle critical troponin during shift change")]
    public void HandleCriticalTroponinDuringShiftChange()
    {
        // Given a patient "Catherine Brown" is in bed "ED-6"
        // And it is 19:00 during evening shift change
        // And the day shift physician "Dr. Wilson" ordered the troponin
        // And the evening shift physician "Dr. Taylor" has assumed care
        // (assumed pre-seeded test data)

        // When a critically high troponin result of "6.1 ng/mL" is received
        FillField(driver, "Troponin Result", "6.1 ng/mL");
        driver.FindElement(By.CssSelector("[data-testid=\"receive-lab-result-button\"]")).Click();

        // Then both physicians receive critical alerts:
        //   | Physician  | Alert Type    | Content                                |
        //   | Dr. Taylor | Primary Alert | CRITICAL Troponin 6.1 - Your patient  |
        //   | Dr. Wilson | Handoff Alert | FYI: Critical result on your order     |
        Assert.That(GetText(driver, "Dr. Taylor Alert"), Is.EqualTo("CRITICAL Troponin 6.1 - Your patient"));
        Assert.That(GetText(driver, "Dr. Wilson Alert"), Is.EqualTo("FYI: Critical result on your order"));

        // And the charge nurse receives handoff-specific notification:
        Assert.That(GetText(driver, "Shift Context"), Is.EqualTo("Critical result during physician handoff"));
        Assert.That(GetText(driver, "Current MD"), Is.EqualTo("Dr. Taylor (assuming care)"));
        Assert.That(GetText(driver, "Ordering MD"), Is.EqualTo("Dr. Wilson (ordered test)"));

        // And the handoff documentation is automatically updated
        var handoffDocumentation = driver.FindElement(By.CssSelector("[data-testid=\"handoff-documentation\"]"));
        Assert.That(handoffDocumentation.Displayed, Is.True);

        // And red flags appear with shift change context indicators
        var redFlagIndicators = driver.FindElements(By.CssSelector("[data-testid=\"red-flag-indicator\"]"));
        Assert.That(redFlagIndicators.Count > 0, Is.True);
    }

    [Test, Order(5)]
    [Description("Process critical troponin with additional cardiac markers")]
    public void ProcessCriticalTroponinWithAdditionalCardiacMarkers()
    {
        // Given a patient "Steven Kim" is in bed "ED-11"
        // And multiple cardiac markers were ordered
        // (assumed pre-seeded test data)

        // When critical and related results are received:
        //   | Test Name    | Result | Reference Range | Critical | Clinical Significance |
        //   | Troponin I   | 4.7    | 0.0-0.04       | Yes      | Acute MI indicated    |
        //   | CK-MB        | 45     | 0-6.3          | Yes      | Myocardial damage     |
        //   | Myoglobin    | 280    | 25-72          | No       | Elevated but not critical|
        driver.FindElement(By.CssSelector("[data-testid=\"receive-lab-results-button\"]")).Click();

        // Then the system groups related critical values:
        //   | Alert Category  | Content                                      |
        //   | Cardiac Panel   | Multiple critical cardiac markers            |
        //   | Primary Alert   | Troponin I: 4.7 ng/mL (CRITICAL)           |
        //   | Secondary Alert | CK-MB: 45 ng/mL (CRITICAL)                 |
        //   | Supporting Data | Myoglobin: 280 ng/mL (Elevated)            |
        Assert.That(GetText(driver, "Cardiac Panel"), Is.EqualTo("Multiple critical cardiac markers"));
        Assert.That(GetText(driver, "Primary Alert"), Is.EqualTo("Troponin I: 4.7 ng/mL (CRITICAL)"));
        Assert.That(GetText(driver, "Secondary Alert"), Is.EqualTo("CK-MB: 45 ng/mL (CRITICAL)"));
        Assert.That(GetText(driver, "Supporting Data"), Is.EqualTo("Myoglobin: 280 ng/mL (Elevated)"));

        // And enhanced clinical context is provided:
        Assert.That(GetText(driver, "Clinical Indication"), Is.EqualTo("Acute myocardial infarction likely"));
        Assert.That(GetText(driver, "Recommended Actions"), Is.EqualTo("Cardiology consult, STEMI protocol"));
        Assert.That(GetText(driver, "Time Sensitivity"), Is.EqualTo("Treatment within 90 minutes critical"));

        // And STEMI protocol alerts are automatically triggered
        WaitForTestId(driver, "STEMI Protocol Alert");
    }

    [Test, Order(6)]
    [Description("Handle false positive critical troponin alerts")]
    public void HandleFalsePositiveCriticalTroponinAlerts()
    {
        // Given a patient "Nancy Rodriguez" is in bed "ED-4"
        // And a troponin result of "5.1 ng/mL" triggers a critical alert
        // (assumed pre-seeded test data)

        // When the laboratory calls to report a sample error
        driver.FindElement(By.CssSelector("[data-testid=\"report-sample-error-button\"]")).Click();

        // And a corrected result shows "0.03 ng/mL" (normal)
        FillField(driver, "Corrected Troponin Result", "0.03 ng/mL");
        driver.FindElement(By.CssSelector("[data-testid=\"submit-correction-button\"]")).Click();

        // Then the system processes the correction:
        Assert.That(GetText(driver, "Cancel Alert"), Is.EqualTo("Original critical alert is cancelled"));
        Assert.That(GetText(driver, "Send Correction"), Is.EqualTo("Corrected value sent to all recipients"));
        Assert.That(GetText(driver, "Document Error"), Is.EqualTo("Lab error documented in audit trail"));

        // And correction notifications are sent:
        Assert.That(GetText(driver, "Alert Cancellation"), Is.EqualTo("CANCELLED: Previous critical troponin alert"));
        Assert.That(GetText(driver, "Corrected Value"), Is.EqualTo("Troponin corrected to 0.03 ng/mL (Normal)"));
        Assert.That(GetText(driver, "Error Explanation"), Is.EqualTo("Laboratory sample contamination identified"));

        // And red flags are removed from all patient displays
        var redFlagIndicators = driver.FindElements(By.CssSelector("[data-testid=\"red-flag-indicator\"]"));
        Assert.That(redFlagIndicators.Count, Is.EqualTo(0));

        // And the correction is logged for quality assurance review
        var qualityAssuranceLog = driver.FindElement(By.CssSelector("[data-testid=\"quality-assurance-log\"]"));
        Assert.That(qualityAssuranceLog.Displayed, Is.True);
    }

    [Test, Order(7)]
    [Description("Critical troponin with patient transfer requirements")]
    public void CriticalTroponinWithPatientTransferRequirements()
    {
        // Given a patient "Timothy Chang" is in bed "ED-9"
        // And a critically high troponin of "9.3 ng/mL" is received
        // And the patient requires immediate transfer to cardiac unit
        // (assumed pre-seeded test data)

        // When the critical alert is processed
        driver.FindElement(By.CssSelector("[data-testid=\"process-critical-alert-button\"]")).Click();

        // Then transfer coordination alerts are included:
        Assert.That(GetText(driver, "Transfer Required"), Is.EqualTo("Patient needs immediate cardiac unit transfer"));
        Assert.That(GetText(driver, "Bed Availability"), Is.EqualTo("CCU bed 302 available"));
        Assert.That(GetText(driver, "Transport Time"), Is.EqualTo("Transport team ETA 10 minutes"));

        // And receiving unit notifications are sent:
        Assert.That(GetText(driver, "CCU Alert"), Is.EqualTo("Incoming transfer - Critical troponin 9.3"));
        Assert.That(GetText(driver, "Cardiology Alert"), Is.EqualTo("Urgent consult needed - STEMI protocol"));

        // And transfer documentation is automatically initiated
        var transferDocumentation = driver.FindElement(By.CssSelector("[data-testid=\"transfer-documentation\"]"));
        Assert.That(transferDocumentation.Displayed, Is.True);

        // And critical alerts follow the patient to the receiving unit
        WaitForTestId(driver, "Patient Alert Handoff");
    }

    [Test, Order(8)]
    [Description("Validate critical troponin alert system functionality")]
    public void ValidateCriticalTroponinAlertSystemFunctionality()
    {
        // Given the critical alert system is being tested
        // (assumed pre-seeded test data)

        // When a test troponin result of "TEST-5.0 ng/mL" is processed
        FillField(driver, "Test Troponin Result", "TEST-5.0 ng/mL");
        driver.FindElement(By.CssSelector("[data-testid=\"process-test-result-button\"]")).Click();

        // Then the system validates all alert pathways:
        Assert.That(GetText(driver, "Physician Mobile"), Is.EqualTo("Test alert delivered successfully"));
        Assert.That(GetText(driver, "Charge Nurse"), Is.EqualTo("Test alert delivered successfully"));
        Assert.That(GetText(driver, "Patient Displays"), Is.EqualTo("Red flags displayed correctly"));
        Assert.That(GetText(driver, "Audit Trail"), Is.EqualTo("Test alert logged with timestamp"));

        // And test alerts are clearly marked as "SYSTEM TEST"
        var testAlertMarking = GetText(driver, "Test Alert Marking");
        Assert.That(testAlertMarking, Is.EqualTo("SYSTEM TEST"));

        // And all test alerts are automatically cleared after validation
        var testAlerts = driver.FindElements(By.CssSelector("[data-testid=\"test-alert\"]"));
        Assert.That(testAlerts.Count, Is.EqualTo(0));

        // And system performance metrics are recorded:
        //   | Metric           | Measurement                                  |
        //   | Alert Latency    | <30 seconds from result to notification     |
        //   | Delivery Success | 100% successful delivery to all recipients  |
        //   | Display Update   | <5 seconds to update all patient displays   |
        Assert.That(GetText(driver, "Alert Latency"), Is.EqualTo("<30 seconds from result to notification"));
        Assert.That(GetText(driver, "Delivery Success"), Is.EqualTo("100% successful delivery to all recipients"));
        Assert.That(GetText(driver, "Display Update"), Is.EqualTo("<5 seconds to update all patient displays"));
    }
}
