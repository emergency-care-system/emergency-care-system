// Selenium WebDriver + NUnit test for
// tests-with-given-when-then-features/04-triage-re-assessment.feature
// (equivalent to tests-with-selenium-javascript/04-triage-re-assessment.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see Support/Fields.cs) and the shared
// data-testid contract in Support/Login.cs (login-identity, login-submit,
// app-root).

namespace EmergencyCareSystem.SeleniumTests;

[TestFixture]
[NonParallelizable]
public class T04TriageReAssessmentTests
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
        //   And I am logged in as a triage nurse
        //   And the automatic reassessment alerts are enabled
        VerifySystemIsOperational(driver);
        Login(driver, "a triage nurse");
        // The automatic reassessment alerts being enabled is assumed pre-seeded
        // test data / environment configuration.

        var featureNavLink = WaitForTestId(driver, "Nav Triage Re-assessment");
        featureNavLink.Click();
        WaitForTestId(driver, "Triage Re-assessment Panel");
    }

    [Test, Order(1)]
    [Description("Reassess patient with worsening condition after 2 hours")]
    public void ReassessPatientWithWorseningConditionAfter2Hours()
    {
        // Given a patient "Sarah Johnson" has been waiting in the queue for 2 hours
        // And the patient's initial triage was ESI Level 4 (Less Urgent)
        // And the patient's initial vital signs were:
        //   | Vital Sign          | Initial Value |
        //   | Blood Pressure      | 125/78        |
        //   | Heart Rate          | 82            |
        //   | Respiratory Rate    | 16            |
        //   | Temperature         | 99.1°F        |
        //   | Oxygen Saturation   | 98%           |
        //   | Pain Scale          | 3/10          |
        // (assumed pre-seeded test data)
        // When the system triggers a reassessment alert at the 2-hour mark
        WaitForTestId(driver, "Reassessment Alert");
        // And I select the patient for reassessment
        driver.FindElement(By.CssSelector("[data-testid=\"select-patient-for-reassessment-button\"]")).Click();

        // And I enter the updated vital signs:
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Blood Pressure"), ("Value", "95/55")),
            Row(("Field", "Heart Rate"), ("Value", "115")),
            Row(("Field", "Respiratory Rate"), ("Value", "24")),
            Row(("Field", "Temperature"), ("Value", "101.8°F")),
            Row(("Field", "Oxygen Saturation"), ("Value", "94%")),
            Row(("Field", "Pain Scale"), ("Value", "8/10")),
        });
        // And I update the chief complaint to "Severe abdominal pain with dizziness"
        FillField(driver, "Chief Complaint", "Severe abdominal pain with dizziness");
        // And I submit the reassessment
        driver.FindElement(By.CssSelector("[data-testid=\"submit-reassessment-form\"]")).Click();

        // Then the system recalculates the ESI score from "4" to "2"
        var esiScore = GetText(driver, "ESI Score");
        Assert.That(esiScore, Is.EqualTo("2"));
        // And the system updates the triage level from "Less Urgent" to "High Priority"
        var triageLevel = GetText(driver, "Triage Level");
        Assert.That(triageLevel, Is.EqualTo("High Priority"));
        // And the patient is moved from position 12 to position 2 in the queue
        var queuePosition = GetText(driver, "Queue Position");
        Assert.That(queuePosition, Is.EqualTo("2"));
        // And an escalation alert is sent to the charge nurse
        var escalationAlert = GetText(driver, "Escalation Alert");
        Assert.That(escalationAlert, Does.Match(@"charge nurse").IgnoreCase);
        // And the estimated wait time is updated from "90 minutes" to "15 minutes"
        var estimatedWaitTime = GetText(driver, "Estimated Wait Time");
        Assert.That(estimatedWaitTime, Is.EqualTo("15 minutes"));
        // And a reassessment note is automatically added to the patient record
        var reassessmentNote = GetText(driver, "Reassessment Note");
        Assert.That(reassessmentNote.Length > 0, Is.True);
    }

    [Test, Order(2)]
    [Description("Reassess patient with stable condition")]
    public void ReassessPatientWithStableCondition()
    {
        // Given a patient "Michael Chen" has been waiting in the queue for 2 hours
        // And the patient's initial triage was ESI Level 3 (Urgent)
        // And the patient's initial vital signs were:
        //   | Vital Sign          | Initial Value |
        //   | Blood Pressure      | 140/90        |
        //   | Heart Rate          | 95            |
        //   | Respiratory Rate    | 20            |
        //   | Temperature         | 100.2°F       |
        //   | Oxygen Saturation   | 96%           |
        //   | Pain Scale          | 6/10          |
        // (assumed pre-seeded test data)
        // When I perform a scheduled reassessment
        driver.FindElement(By.CssSelector("[data-testid=\"select-patient-for-reassessment-button\"]")).Click();

        // And I enter the updated vital signs:
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Blood Pressure"), ("Value", "135/85")),
            Row(("Field", "Heart Rate"), ("Value", "88")),
            Row(("Field", "Respiratory Rate"), ("Value", "18")),
            Row(("Field", "Temperature"), ("Value", "99.8°F")),
            Row(("Field", "Oxygen Saturation"), ("Value", "97%")),
            Row(("Field", "Pain Scale"), ("Value", "5/10")),
        });
        // And I note "Patient reports feeling slightly better"
        FillField(driver, "Reassessment Note", "Patient reports feeling slightly better");
        // And I submit the reassessment
        driver.FindElement(By.CssSelector("[data-testid=\"submit-reassessment-form\"]")).Click();

        // Then the system recalculates and maintains ESI score of "3"
        var esiScore = GetText(driver, "ESI Score");
        Assert.That(esiScore, Is.EqualTo("3"));
        // And the triage level remains "Urgent"
        var triageLevel = GetText(driver, "Triage Level");
        Assert.That(triageLevel, Is.EqualTo("Urgent"));
        // And the patient's queue position remains unchanged
        var queuePosition = GetText(driver, "Queue Position");
        Assert.That(queuePosition.Length > 0, Is.True);
        // And no escalation alerts are generated
        var escalationAlerts = driver.FindElements(Locator("Escalation Alert"));
        Assert.That(escalationAlerts.Count, Is.EqualTo(0));
        // And a reassessment note is added documenting stable condition
        var reassessmentNote = GetText(driver, "Reassessment Note");
        Assert.That(reassessmentNote, Is.EqualTo("Patient reports feeling slightly better"));
        // And the next reassessment is scheduled for 1 hour
        var nextReassessmentSchedule = GetText(driver, "Next Reassessment Schedule");
        Assert.That(nextReassessmentSchedule, Is.EqualTo("1 hour"));
    }

    [Test, Order(3)]
    [Description("Reassess patient with improving condition")]
    public void ReassessPatientWithImprovingCondition()
    {
        // Given a patient "Lisa Rodriguez" has been waiting in the queue for 2 hours
        // And the patient's initial triage was ESI Level 2 (High Priority)
        // And the patient's initial vital signs were:
        //   | Vital Sign          | Initial Value |
        //   | Blood Pressure      | 170/100       |
        //   | Heart Rate          | 120           |
        //   | Respiratory Rate    | 28            |
        //   | Temperature         | 98.9°F        |
        //   | Oxygen Saturation   | 92%           |
        //   | Pain Scale          | 9/10          |
        // (assumed pre-seeded test data)
        // When I perform a reassessment
        driver.FindElement(By.CssSelector("[data-testid=\"select-patient-for-reassessment-button\"]")).Click();

        // And I enter the updated vital signs:
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Blood Pressure"), ("Value", "145/85")),
            Row(("Field", "Heart Rate"), ("Value", "95")),
            Row(("Field", "Respiratory Rate"), ("Value", "20")),
            Row(("Field", "Temperature"), ("Value", "98.6°F")),
            Row(("Field", "Oxygen Saturation"), ("Value", "96%")),
            Row(("Field", "Pain Scale"), ("Value", "4/10")),
        });
        // And I note "Patient reports significant improvement after medication"
        FillField(driver, "Reassessment Note", "Patient reports significant improvement after medication");
        // And I submit the reassessment
        driver.FindElement(By.CssSelector("[data-testid=\"submit-reassessment-form\"]")).Click();

        // Then the system recalculates the ESI score from "2" to "3"
        var esiScore = GetText(driver, "ESI Score");
        Assert.That(esiScore, Is.EqualTo("3"));
        // And the system updates the triage level from "High Priority" to "Urgent"
        var triageLevel = GetText(driver, "Triage Level");
        Assert.That(triageLevel, Is.EqualTo("Urgent"));
        // And the patient is moved from position 1 to position 5 in the queue
        var queuePosition = GetText(driver, "Queue Position");
        Assert.That(queuePosition, Is.EqualTo("5"));
        // And the charge nurse is notified of the priority change
        var chargeNurseNotification = GetText(driver, "Charge Nurse Notification");
        Assert.That(chargeNurseNotification, Does.Match(@"priority change").IgnoreCase);
        // And the estimated wait time is updated from "Immediate" to "45 minutes"
        var estimatedWaitTime = GetText(driver, "Estimated Wait Time");
        Assert.That(estimatedWaitTime, Is.EqualTo("45 minutes"));
        // And higher priority patients are moved up in the queue
        var queueEntries = driver.FindElements(By.CssSelector("[data-testid=\"triage-queue-entry\"]"));
        Assert.That(queueEntries.Count > 0, Is.True);
    }

    [Test, Order(4)]
    [Description("Automatic reassessment alert triggers")]
    public void AutomaticReassessmentAlertTriggers()
    {
        // Given multiple patients have been waiting for extended periods:
        //   | Patient Name     | Wait Time | Current ESI | Due for Reassessment |
        //   | John Williams    | 2 hours   | 4           | Yes                  |
        //   | Emma Thompson    | 1.5 hours | 3           | No                   |
        //   | David Kim        | 3 hours   | 3           | Yes                  |
        // (assumed pre-seeded test data)
        // When the system performs its hourly reassessment check
        driver.FindElement(By.CssSelector("[data-testid=\"trigger-hourly-reassessment-check-button\"]")).Click();

        // Then reassessment alerts are generated for:
        //   | Patient Name  | Alert Type           | Reason                    |
        //   | John Williams | Standard Reassess    | 2 hours ESI Level 4       |
        //   | David Kim     | Urgent Reassess      | 3 hours ESI Level 3       |
        var reassessmentAlerts = driver.FindElements(By.CssSelector("[data-testid=\"reassessment-alert\"]"));
        Assert.That(reassessmentAlerts.Count, Is.EqualTo(2));
        // And the alerts appear on the triage nurse dashboard
        var triageNurseDashboard = WaitForTestId(driver, "Triage Nurse Dashboard");
        Assert.That(triageNurseDashboard.Displayed, Is.True);
        // And the patients are flagged with "Reassessment Due" status
        var reassessmentDueFlags = driver.FindElements(By.CssSelector("[data-testid=\"reassessment-due-flag\"]"));
        Assert.That(reassessmentDueFlags.Count, Is.EqualTo(2));
        // And Emma Thompson does not receive an alert
        var alertTexts = reassessmentAlerts.Select(x => x.Text).ToList();
        Assert.That(alertTexts.Any(text => text.Contains("Emma Thompson")), Is.False);
    }

    [Test, Order(5)]
    [Description("Handle patient who becomes critical during reassessment")]
    public void HandlePatientWhoBecomesCriticalDuringReassessment()
    {
        // Given a patient "Robert Martinez" has been waiting in the queue for 2 hours
        // And the patient's initial triage was ESI Level 3 (Urgent)
        // When I begin the reassessment process
        driver.FindElement(By.CssSelector("[data-testid=\"select-patient-for-reassessment-button\"]")).Click();
        // And I observe the patient is now unresponsive

        // And I enter critical vital signs:
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Blood Pressure"), ("Value", "60/30")),
            Row(("Field", "Heart Rate"), ("Value", "150")),
            Row(("Field", "Respiratory Rate"), ("Value", "6")),
            Row(("Field", "Temperature"), ("Value", "96.2°F")),
            Row(("Field", "Oxygen Saturation"), ("Value", "80%")),
            Row(("Field", "Consciousness"), ("Value", "Unresponsive")),
        });
        // And I submit the emergency reassessment
        driver.FindElement(By.CssSelector("[data-testid=\"submit-reassessment-form\"]")).Click();

        // Then the system immediately calculates ESI score as "1"
        var esiScore = GetText(driver, "ESI Score");
        Assert.That(esiScore, Is.EqualTo("1"));
        // And the system updates triage level to "Resuscitation"
        var triageLevel = GetText(driver, "Triage Level");
        Assert.That(triageLevel, Is.EqualTo("Resuscitation"));
        // And the patient is moved to the top of all queues
        var queuePosition = GetText(driver, "Queue Position");
        Assert.That(queuePosition, Is.EqualTo("1"));
        // And a code blue alert is automatically triggered
        var codeBlueAlert = GetText(driver, "Code Blue Alert");
        Assert.That(codeBlueAlert, Does.Match(@"triggered").IgnoreCase);
        // And the rapid response team is notified immediately
        var rapidResponseNotification = GetText(driver, "Rapid Response Notification");
        Assert.That(rapidResponseNotification, Does.Match(@"notified").IgnoreCase);
        // And the patient is flagged for immediate intervention
        var interventionFlag = GetText(driver, "Intervention Flag");
        Assert.That(interventionFlag, Does.Match(@"immediate intervention").IgnoreCase);
        // And I am prompted to initiate emergency protocols
        WaitForTestId(driver, "Emergency Protocol Prompt");
    }

    [Test, Order(6)]
    [Description("Reassess pediatric patient with different parameters")]
    public void ReassessPediatricPatientWithDifferentParameters()
    {
        // Given a pediatric patient "Amy Foster" (age 8) has been waiting for 2 hours
        // And the patient's initial triage was ESI Level 3 (Urgent)
        // When I perform a pediatric reassessment
        driver.FindElement(By.CssSelector("[data-testid=\"select-patient-for-reassessment-button\"]")).Click();

        // And I enter updated vital signs using age-appropriate parameters:
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Blood Pressure"), ("Value", "85/50")),
            Row(("Field", "Heart Rate"), ("Value", "140")),
            Row(("Field", "Respiratory Rate"), ("Value", "32")),
            Row(("Field", "Temperature"), ("Value", "103.8°F")),
            Row(("Field", "Oxygen Saturation"), ("Value", "93%")),
            Row(("Field", "Pain Scale (FACES)"), ("Value", "8/10")),
        });
        // And I note "Child appears more lethargic than initial assessment"
        FillField(driver, "Reassessment Note", "Child appears more lethargic than initial assessment");
        // And I submit the pediatric reassessment
        driver.FindElement(By.CssSelector("[data-testid=\"submit-reassessment-form\"]")).Click();

        // Then the system recalculates using pediatric ESI criteria
        var scoringCriteria = GetText(driver, "Scoring Criteria");
        Assert.That(scoringCriteria, Does.Match(@"pediatric").IgnoreCase);
        // And the ESI score is updated from "3" to "2"
        var esiScore = GetText(driver, "ESI Score");
        Assert.That(esiScore, Is.EqualTo("2"));
        // And the triage level is updated to "High Priority"
        var triageLevel = GetText(driver, "Triage Level");
        Assert.That(triageLevel, Is.EqualTo("High Priority"));
        // And the pediatric emergency team is notified
        var pediatricTeamNotification = GetText(driver, "Pediatric Team Notification");
        Assert.That(pediatricTeamNotification, Does.Match(@"notified").IgnoreCase);
        // And the patient is moved to the pediatric high priority queue
        var assignedQueue = GetText(driver, "Assigned Queue");
        Assert.That(assignedQueue, Does.Match(@"pediatric high priority").IgnoreCase);
        // And parent/guardian notification protocols are initiated
        var guardianNotification = GetText(driver, "Guardian Notification");
        Assert.That(guardianNotification, Does.Match(@"initiated").IgnoreCase);
    }

    [Test, Order(7)]
    [Description("Document reassessment with no vital sign changes")]
    public void DocumentReassessmentWithNoVitalSignChanges()
    {
        // Given a patient "Catherine Lee" has been waiting for 2 hours
        // And a reassessment is due
        // When I perform the reassessment
        driver.FindElement(By.CssSelector("[data-testid=\"select-patient-for-reassessment-button\"]")).Click();
        // And the vital signs remain identical to the initial assessment

        // But I note "Patient reports increased anxiety about wait time"
        FillField(driver, "Reassessment Note", "Patient reports increased anxiety about wait time");
        // And I provide reassurance and update on expected wait time
        // And I submit the reassessment
        driver.FindElement(By.CssSelector("[data-testid=\"submit-reassessment-form\"]")).Click();

        // Then the ESI score and triage level remain unchanged
        var esiScore = GetText(driver, "ESI Score");
        Assert.That(esiScore.Length > 0, Is.True);
        var triageLevel = GetText(driver, "Triage Level");
        Assert.That(triageLevel.Length > 0, Is.True);
        // And the queue position is maintained
        var queuePosition = GetText(driver, "Queue Position");
        Assert.That(queuePosition.Length > 0, Is.True);
        // And a documentation note is added about patient anxiety
        var reassessmentNote = GetText(driver, "Reassessment Note");
        Assert.That(reassessmentNote, Does.Match(@"anxiety").IgnoreCase);
        // And comfort measures are suggested in the patient instructions
        var patientInstructions = GetText(driver, "Patient Instructions");
        Assert.That(patientInstructions, Does.Match(@"comfort").IgnoreCase);
        // And the next reassessment interval is maintained
        var nextReassessmentSchedule = GetText(driver, "Next Reassessment Schedule");
        Assert.That(nextReassessmentSchedule.Length > 0, Is.True);
    }

    [Test, Order(8)]
    [Description("Handle reassessment during shift change")]
    public void HandleReassessmentDuringShiftChange()
    {
        // Given a patient "Thomas Wilson" is due for reassessment
        // And the day shift triage nurse is preparing to leave
        // And the night shift triage nurse is arriving
        // When the day shift nurse initiates the reassessment handoff
        driver.FindElement(By.CssSelector("[data-testid=\"initiate-reassessment-handoff-button\"]")).Click();
        // And transfers the patient assessment to the night shift nurse
        driver.FindElement(By.CssSelector("[data-testid=\"transfer-assessment-button\"]")).Click();

        // Then the reassessment timing is preserved
        var reassessmentTiming = GetText(driver, "Reassessment Timing");
        Assert.That(reassessmentTiming.Length > 0, Is.True);
        // And all previous assessment data remains accessible
        var previousAssessmentData = WaitForTestId(driver, "Previous Assessment Data");
        Assert.That(previousAssessmentData.Displayed, Is.True);
        // And the night shift nurse can complete the reassessment
        var reassessmentForm = driver.FindElement(By.CssSelector("[data-testid=\"reassessment-form\"]"));
        Assert.That(reassessmentForm.Displayed, Is.True);
        // And continuity of care documentation is maintained
        var continuityDocumentation = GetText(driver, "Continuity Documentation");
        Assert.That(continuityDocumentation.Length > 0, Is.True);
        // And the handoff is logged in the system audit trail
        var auditTrailEntries = driver.FindElements(By.CssSelector("[data-testid=\"audit-trail-entry\"]"));
        Assert.That(auditTrailEntries.Count > 0, Is.True);
    }
}
