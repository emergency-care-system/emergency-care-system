// Playwright + NUnit test for
// tests-with-given-when-then-features/09-lab-result-processing.feature
// (equivalent to tests-with-playwright-javascript/09-lab-result-processing.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see Support/Fields.cs) and the shared
// data-testid contract in Support/Login.cs (login-identity, login-submit,
// app-root).

namespace EmergencyCareSystem.PlaywrightTests;

[TestFixture]
[NonParallelizable]
public class T09LabResultProcessingTests
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
        //   And the HL7 interface with the laboratory system is active
        //   And critical value alert system is enabled
        //   And physician notification system is functional
        await VerifySystemIsOperational(page);
        // The HL7 interface, critical value alert system, and physician
        // notification system being active/enabled are assumed pre-seeded
        // test data / environment configuration. This feature has no
        // "logged in as" step in its Background.
        await Login(page, "a lab technician");

        var labResultProcessingNavLink = await WaitForTestId(page, "Nav Lab Result Processing");
        await labResultProcessingNavLink.ClickAsync();
        await WaitForTestId(page, "Lab Result Processing Panel");
    }

    [Test, Order(1)]
    [Description("Process normal lab results via HL7 interface")]
    public async Task ProcessNormalLabResultsViaHL7Interface()
    {
        // Given a patient "Jennifer Lopez" is in bed "ED-8"
        // And laboratory orders were placed for "CBC, Basic Metabolic Panel"
        // And the attending physician is "Dr. Smith"
        // (assumed pre-seeded test data)

        // When the lab system sends results via HL7 interface:
        //   | Test Name          | Result    | Reference Range | Units  | Status   | Timestamp |
        //   | White Blood Cells  | 7.2       | 4.0-10.0       | K/uL   | Final    | 14:30     |
        //   | Hemoglobin         | 13.5      | 12.0-16.0      | g/dL   | Final    | 14:30     |
        //   | Sodium             | 140       | 136-145        | mmol/L | Final    | 14:30     |
        //   | Potassium          | 4.1       | 3.5-5.0        | mmol/L | Final    | 14:30     |
        //   | Creatinine         | 1.0       | 0.6-1.2        | mg/dL  | Final    | 14:30     |
        await page.GetByTestId("receive-lab-results-button").First.ClickAsync();

        // Then the system updates the patient record with all results
        var patientRecordUpdateStatus = await GetText(page, "Patient Record Update Status");
        Assert.That(patientRecordUpdateStatus, Does.Match(@"updated").IgnoreCase);

        // And the results are marked as "Normal" in the patient chart
        var resultStatus = await GetText(page, "Result Status");
        Assert.That(resultStatus, Is.EqualTo("Normal"));

        // And a standard notification is sent to "Dr. Smith":
        Assert.That((await GetText(page, "Lab Results")), Is.EqualTo("Normal CBC and BMP available for review"));
        Assert.That((await GetText(page, "Patient")), Is.EqualTo("Jennifer Lopez, Bed ED-8"));
        Assert.That((await GetText(page, "Timestamp")), Is.EqualTo("14:30"));
        Assert.That((await GetText(page, "Priority")), Is.EqualTo("Standard"));

        // And the results appear in the patient's timeline with normal value indicators
        var timelineEntries = page.GetByTestId("timeline-result-entry");
        Assert.That(await timelineEntries.CountAsync() > 0, Is.True);

        // And no critical value alerts are generated
        var criticalValueAlerts = page.GetByTestId("critical-value-alert");
        Assert.That(await criticalValueAlerts.CountAsync(), Is.EqualTo(0));

        // And the nursing staff is notified that results are available for review
        var nursingResultsNotification = await GetText(page, "Nursing Results Notification");
        Assert.That(nursingResultsNotification.Length > 0, Is.True);
    }

    [Test, Order(2)]
    [Description("Process critical lab results with immediate alerts")]
    public async Task ProcessCriticalLabResultsWithImmediateAlerts()
    {
        // Given a patient "Michael Davis" is in bed "ED-12"
        // And laboratory orders were placed for "Troponin, BNP, D-Dimer"
        // And the attending physician is "Dr. Johnson"
        // (assumed pre-seeded test data)

        // When the lab system sends critical results via HL7 interface:
        //   | Test Name    | Result | Reference Range | Units  | Status | Critical | Timestamp |
        //   | Troponin I   | 8.5    | 0.0-0.04       | ng/mL  | Final  | Yes      | 15:45     |
        //   | BNP          | 1200   | 0-100          | pg/mL  | Final  | Yes      | 15:45     |
        //   | D-Dimer      | 0.8    | 0.0-0.5        | mg/L   | Final  | No       | 15:45     |
        await page.GetByTestId("receive-lab-results-button").First.ClickAsync();

        // Then the system immediately flags critical values:
        //   | Test Name    | Critical Flag | Severity Level |
        //   | Troponin I   | CRITICAL HIGH | Severe         |
        //   | BNP          | CRITICAL HIGH | High           |
        Assert.That((await GetText(page, "Troponin I Critical Flag")), Is.EqualTo("CRITICAL HIGH"));
        Assert.That((await GetText(page, "Troponin I Severity Level")), Is.EqualTo("Severe"));
        Assert.That((await GetText(page, "BNP Critical Flag")), Is.EqualTo("CRITICAL HIGH"));
        Assert.That((await GetText(page, "BNP Severity Level")), Is.EqualTo("High"));

        // And popup notifications are displayed for all logged-in providers:
        Assert.That((await GetText(page, "Critical Alert")), Is.EqualTo("🔴 CRITICAL: Troponin I = 8.5 ng/mL"));
        Assert.That((await GetText(page, "High Alert")), Is.EqualTo("🟠 HIGH: BNP = 1200 pg/mL"));
        Assert.That((await GetText(page, "Patient Info")), Is.EqualTo("Michael Davis, Bed ED-12"));

        // And an immediate notification is sent to "Dr. Johnson":
        Assert.That((await GetText(page, "Mobile Push")), Is.EqualTo("CRITICAL LAB: Troponin 8.5 - Michael Davis"));
        Assert.That((await GetText(page, "SMS Alert")), Is.EqualTo("ED-12 CRITICAL Troponin I: 8.5 ng/mL"));
        Assert.That((await GetText(page, "In-App Alert")), Is.EqualTo("High priority popup requiring acknowledgment"));

        // And the charge nurse receives a critical value notification
        var chargeNurseNotification = await GetText(page, "Charge Nurse Notification");
        Assert.That(chargeNurseNotification.Length > 0, Is.True);

        // And the results are highlighted in red on all patient displays
        var criticalResultHighlights = page.GetByTestId("critical-result-highlight");
        Assert.That(await criticalResultHighlights.CountAsync() > 0, Is.True);

        // And an audit trail is created for the critical value communication
        var auditTrail = page.GetByTestId("critical-value-audit-trail").First;
        Assert.That((await auditTrail.IsVisibleAsync()), Is.True);
    }

    [Test, Order(3)]
    [Description("Handle lab results with different statuses and corrections")]
    public async Task HandleLabResultsWithDifferentStatusesAndCorrections()
    {
        // Given a patient "Sarah Wilson" is in bed "ED-6"
        // And previous lab results were reported
        // (assumed pre-seeded test data)

        // When the lab system sends updated results via HL7 interface:
        //   | Test Name     | Result | Status     | Previous Result | Correction Reason    | Timestamp |
        //   | Hemoglobin    | 9.2    | Corrected  | 11.2           | Sample hemolysis     | 16:15     |
        //   | Glucose       | 250    | Final      | -              | -                    | 16:15     |
        //   | Pending Test  | -      | Pending    | -              | Sample reprocessing  | 16:15     |
        await page.GetByTestId("receive-lab-results-button").First.ClickAsync();

        // Then the system processes different result statuses:
        Assert.That((await GetText(page, "Corrected")), Is.EqualTo("Replace previous value, maintain history"));
        Assert.That((await GetText(page, "Final")), Is.EqualTo("Add new result to patient record"));
        Assert.That((await GetText(page, "Pending")), Is.EqualTo("Update status, maintain order tracking"));

        // And correction notifications are sent:
        Assert.That((await GetText(page, "Correction Alert")), Is.EqualTo("Lab value corrected: Hgb 11.2 → 9.2 g/dL"));
        Assert.That((await GetText(page, "Reason")), Is.EqualTo("Sample hemolysis detected"));
        Assert.That((await GetText(page, "Clinical Impact")), Is.EqualTo("Anemia now more severe than initially reported"));

        // And the attending physician "Dr. Martinez" is notified of the correction
        var physicianCorrectionNotification = await GetText(page, "Physician Correction Notification");
        Assert.That(physicianCorrectionNotification.Length > 0, Is.True);

        // And the original result is preserved in the audit trail
        var originalResultAuditEntry = page.GetByTestId("original-result-audit-entry").First;
        Assert.That((await originalResultAuditEntry.IsVisibleAsync()), Is.True);

        // And the corrected value triggers anemia protocol alerts
        await WaitForTestId(page, "Anemia Protocol Alert");
    }

    [Test, Order(4)]
    [Description("Process pediatric lab results with age-specific reference ranges")]
    public async Task ProcessPediatricLabResultsWithAgeSpecificReferenceRanges()
    {
        // Given a pediatric patient "Emma Foster" (age 6) is in bed "ED-PEDS-2"
        // And laboratory orders were placed for "CBC, CMP"
        // (assumed pre-seeded test data)

        // When the lab system sends pediatric results via HL7 interface:
        //   | Test Name          | Result | Adult Range    | Pediatric Range (Age 6) | Units  | Status |
        //   | White Blood Cells  | 12.5   | 4.0-10.0      | 5.0-14.5               | K/uL   | Final  |
        //   | Hemoglobin         | 11.8   | 12.0-16.0     | 11.5-13.5              | g/dL   | Final  |
        //   | Alkaline Phosphatase| 250   | 44-147        | 156-369                | U/L    | Final  |
        await page.GetByTestId("receive-lab-results-button").First.ClickAsync();

        // Then the system applies age-appropriate reference ranges:
        //   | Test Name          | Interpretation      | Flag        |
        //   | White Blood Cells  | Normal for age 6    | Normal      |
        //   | Hemoglobin         | Normal for age 6    | Normal      |
        //   | Alkaline Phosphatase| Normal for age 6   | Normal      |
        Assert.That((await GetText(page, "White Blood Cells Interpretation")), Is.EqualTo("Normal for age 6"));
        Assert.That((await GetText(page, "White Blood Cells Flag")), Is.EqualTo("Normal"));
        Assert.That((await GetText(page, "Hemoglobin Interpretation")), Is.EqualTo("Normal for age 6"));
        Assert.That((await GetText(page, "Hemoglobin Flag")), Is.EqualTo("Normal"));
        Assert.That((await GetText(page, "Alkaline Phosphatase Interpretation")), Is.EqualTo("Normal for age 6"));
        Assert.That((await GetText(page, "Alkaline Phosphatase Flag")), Is.EqualTo("Normal"));

        // And the pediatric attending "Dr. Chen" is notified with age-specific context
        var pediatricAttendingNotification = await GetText(page, "Pediatric Attending Notification");
        Assert.That(pediatricAttendingNotification.Length > 0, Is.True);

        // And the results display shows both adult and pediatric reference ranges
        await WaitForTestId(page, "Adult Reference Range");
        await WaitForTestId(page, "Pediatric Reference Range");

        // And no inappropriate critical alerts are generated for age-normal values
        var criticalValueAlerts = page.GetByTestId("critical-value-alert");
        Assert.That(await criticalValueAlerts.CountAsync(), Is.EqualTo(0));
    }

    [Test, Order(5)]
    [Description("Handle lab results during physician handoff")]
    public async Task HandleLabResultsDuringPhysicianHandoff()
    {
        // Given a patient "Robert Kim" is in bed "ED-15"
        // And the day shift physician "Dr. Adams" ordered labs at 18:00
        // And the evening shift physician "Dr. Brown" has taken over at 19:00
        // (assumed pre-seeded test data)

        // When the lab system sends results via HL7 interface at 19:30:
        //   | Test Name     | Result | Reference Range | Status | Critical |
        //   | Lipase        | 350    | 10-140         | Final  | Yes      |
        //   | Amylase       | 180    | 25-125         | Final  | No       |
        await page.GetByTestId("receive-lab-results-button").First.ClickAsync();

        // Then the system determines the appropriate physician to notify:
        //   | Notification Target | Rationale                                  |
        //   | Primary: Dr. Brown  | Current attending physician                |
        //   | Secondary: Dr. Adams| Ordered the tests, may need notification   |
        Assert.That((await GetText(page, "Primary Notification Target")), Is.EqualTo("Dr. Brown"));
        Assert.That((await GetText(page, "Primary Notification Rationale")), Is.EqualTo("Current attending physician"));
        Assert.That((await GetText(page, "Secondary Notification Target")), Is.EqualTo("Dr. Adams"));
        Assert.That((await GetText(page, "Secondary Notification Rationale")), Is.EqualTo("Ordered the tests, may need notification"));

        // And both physicians receive notifications with handoff context:
        Assert.That((await GetText(page, "Dr. Brown Notification")), Is.EqualTo("CRITICAL: Lipase 350 - Patient from Dr. Adams"));
        Assert.That((await GetText(page, "Dr. Adams Notification")), Is.EqualTo("FYI: Your lipase order critical - Now Dr. Brown"));

        // And the handoff log is updated with the critical result information
        var handoffLog = page.GetByTestId("handoff-log").First;
        Assert.That((await handoffLog.IsVisibleAsync()), Is.True);

        // And the charge nurse is notified of the critical value during shift change
        var chargeNurseShiftChangeNotification = await GetText(page, "Charge Nurse Shift Change Notification");
        Assert.That(chargeNurseShiftChangeNotification.Length > 0, Is.True);
    }

    [Test, Order(6)]
    [Description("Process lab results with technical failures and retries")]
    public async Task ProcessLabResultsWithTechnicalFailuresAndRetries()
    {
        // Given a patient "Lisa Garcia" is in bed "ED-3"
        // And laboratory results are ready for transmission
        // (assumed pre-seeded test data)

        // When the lab system attempts to send results via HL7 interface
        // And the initial transmission fails due to network connectivity
        // And the lab system retries transmission after 5 minutes
        // (no direct UI action for these narrative steps)

        // And the retry is successful with results:
        //   | Test Name   | Result | Reference Range | Status | Timestamp |
        //   | Troponin    | 0.02   | 0.0-0.04       | Final  | 20:15     |
        await page.GetByTestId("receive-lab-results-button").First.ClickAsync();

        // Then the system processes the delayed results
        var delayedResultProcessingStatus = await GetText(page, "Delayed Result Processing Status");
        Assert.That(delayedResultProcessingStatus, Does.Match(@"processed").IgnoreCase);

        // And a delay notification is included:
        Assert.That((await GetText(page, "Delay Notice")), Is.EqualTo("Results delayed due to technical issues"));
        Assert.That((await GetText(page, "Original Time")), Is.EqualTo("Results ready at 20:10"));
        Assert.That((await GetText(page, "Received Time")), Is.EqualTo("Results received at 20:15"));

        // And the attending physician is notified of both the results and the delay
        var physicianDelayNotification = await GetText(page, "Physician Delay Notification");
        Assert.That(physicianDelayNotification.Length > 0, Is.True);

        // And system administrators are alerted to the interface failure
        var systemAdministratorAlert = await GetText(page, "System Administrator Alert");
        Assert.That(systemAdministratorAlert.Length > 0, Is.True);

        // And the delay is documented in the interface audit log
        var interfaceAuditLog = page.GetByTestId("interface-audit-log").First;
        Assert.That((await interfaceAuditLog.IsVisibleAsync()), Is.True);
    }

    [Test, Order(7)]
    [Description("Handle batch lab results processing")]
    public async Task HandleBatchLabResultsProcessing()
    {
        // Given multiple patients have pending lab results:
        //   | Patient Name    | Bed    | Attending     | Tests Ordered        |
        //   | Alice Johnson   | ED-4   | Dr. Smith     | CBC, BMP             |
        //   | Bob Thompson    | ED-7   | Dr. Smith     | Liver function tests |
        //   | Carol Martinez  | ED-11  | Dr. Brown     | Cardiac enzymes      |
        // (assumed pre-seeded test data)

        // When the lab system sends batch results via HL7 interface:
        //   | Patient       | Test Results                              | Critical Values |
        //   | Alice Johnson | All normal values                         | None           |
        //   | Bob Thompson  | ALT: 150 (High), AST: 120 (High)        | None           |
        //   | Carol Martinez| Troponin: 2.1 (Critical)                | Troponin       |
        await page.GetByTestId("receive-lab-results-button").First.ClickAsync();

        // Then the system processes all results simultaneously
        var batchProcessingStatus = await GetText(page, "Batch Processing Status");
        Assert.That(batchProcessingStatus, Does.Match(@"simultaneously").IgnoreCase);

        // And notifications are prioritized by criticality:
        //   | Priority | Patient        | Notification Type    |
        //   | 1        | Carol Martinez | Critical value alert |
        //   | 2        | Bob Thompson   | Abnormal value alert |
        //   | 3        | Alice Johnson  | Normal results       |
        Assert.That((await GetText(page, "Priority 1 Patient")), Is.EqualTo("Carol Martinez"));
        Assert.That((await GetText(page, "Priority 1 Notification Type")), Is.EqualTo("Critical value alert"));
        Assert.That((await GetText(page, "Priority 2 Patient")), Is.EqualTo("Bob Thompson"));
        Assert.That((await GetText(page, "Priority 2 Notification Type")), Is.EqualTo("Abnormal value alert"));
        Assert.That((await GetText(page, "Priority 3 Patient")), Is.EqualTo("Alice Johnson"));
        Assert.That((await GetText(page, "Priority 3 Notification Type")), Is.EqualTo("Normal results"));

        // And physicians receive consolidated notifications when appropriate
        var consolidatedNotification = await GetText(page, "Consolidated Notification");
        Assert.That(consolidatedNotification.Length > 0, Is.True);

        // And system performance metrics are maintained during batch processing
        await WaitForTestId(page, "System Performance Metrics");
    }

    [Test, Order(8)]
    [Description("Process lab results with interpretation comments")]
    public async Task ProcessLabResultsWithInterpretationComments()
    {
        // Given a patient "David Lee" is in bed "ED-9"
        // And complex laboratory tests were ordered
        // (assumed pre-seeded test data)

        // When the lab system sends results with pathologist interpretation:
        //   | Test Name        | Result | Reference | Interpretation                    | Timestamp |
        //   | Blood Smear      | -      | -         | Moderate anisocytosis noted      | 21:00     |
        //   | Hemoglobin A1C   | 9.2%   | <5.7%     | Consistent with poor DM control  | 21:00     |
        //   | Thyroid Function | -      | -         | Pattern suggests hyperthyroidism | 21:00     |
        await page.GetByTestId("receive-lab-results-button").First.ClickAsync();

        // Then the system includes interpretation comments in the patient record
        var interpretationComments = page.GetByTestId("interpretation-comment");
        Assert.That(await interpretationComments.CountAsync(), Is.EqualTo(3));

        // And the attending physician receives enhanced notifications:
        Assert.That((await GetText(page, "Raw Results")), Is.EqualTo("Numeric values and reference ranges"));
        Assert.That((await GetText(page, "Interpretation")), Is.EqualTo("Pathologist comments and clinical significance"));
        Assert.That((await GetText(page, "Recommendations")), Is.EqualTo("Suggested follow-up or additional testing"));

        // And interpretation comments are highlighted in the patient chart
        var highlightedInterpretationComments = page.GetByTestId("highlighted-interpretation-comment");
        Assert.That(await highlightedInterpretationComments.CountAsync() > 0, Is.True);

        // And complex results are flagged for physician review and acknowledgment
        await WaitForTestId(page, "Physician Review Flag");
    }
}
