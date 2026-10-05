// Selenium WebDriver + NUnit test for
// tests-with-given-when-then-features/03-initial-triage-assessment.feature
// (equivalent to tests-with-selenium-javascript/03-initial-triage-assessment.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see Support/Fields.cs) and the shared
// data-testid contract in Support/Login.cs (login-identity, login-submit,
// app-root).

namespace EmergencyCareSystem.SeleniumTests;

[TestFixture]
[NonParallelizable]
public class T03InitialTriageAssessmentTests
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
        //   And the ESI (Emergency Severity Index) scoring module is active
        VerifySystemIsOperational(driver);
        Login(driver, "a triage nurse");
        // The ESI scoring module being active is assumed pre-seeded test data /
        // environment configuration.

        var featureNavLink = WaitForTestId(driver, "Nav Initial Triage Assessment");
        featureNavLink.Click();
        WaitForTestId(driver, "Initial Triage Assessment Panel");
    }

    [Test, Order(1)]
    [Description("Assess patient with chest pain (ESI Level 2)")]
    public void AssessPatientWithChestPainESILevel2()
    {
        // Given a registered patient "John Doe" is waiting for triage
        // And the patient was registered 10 minutes ago
        // (assumed pre-seeded test data)
        // When I select the patient for triage assessment
        driver.FindElement(By.CssSelector("[data-testid=\"select-patient-for-triage-button\"]")).Click();

        // And I enter the vital signs:
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Blood Pressure"), ("Value", "160/95")),
            Row(("Field", "Heart Rate"), ("Value", "110")),
            Row(("Field", "Respiratory Rate"), ("Value", "22")),
            Row(("Field", "Temperature"), ("Value", "98.6°F")),
            Row(("Field", "Oxygen Saturation"), ("Value", "94%")),
        });
        // And I enter the chief complaint as "Chest pain and shortness of breath"
        FillField(driver, "Chief Complaint", "Chest pain and shortness of breath");
        // And I enter the pain scale as "8/10"
        FillField(driver, "Pain Scale", "8/10");
        // And I document onset as "Started 2 hours ago"
        FillField(driver, "Onset", "Started 2 hours ago");
        // And I submit the triage assessment
        driver.FindElement(By.CssSelector("[data-testid=\"submit-triage-assessment-form\"]")).Click();

        // Then the system calculates an ESI score of "2"
        var esiScore = GetText(driver, "ESI Score");
        Assert.That(esiScore, Is.EqualTo("2"));
        // And the system assigns triage level "High Priority"
        var triageLevel = GetText(driver, "Triage Level");
        Assert.That(triageLevel, Is.EqualTo("High Priority"));
        // And the patient is positioned at the front of the high priority queue
        var queuePosition = GetText(driver, "Queue Position");
        Assert.That(queuePosition, Is.EqualTo("1"));
        // And an alert is sent to the attending physician
        var physicianAlert = GetText(driver, "Physician Alert");
        Assert.That(physicianAlert, Does.Match(@"attending physician").IgnoreCase);
        // And the estimated wait time is updated to "Immediate"
        var estimatedWaitTime = GetText(driver, "Estimated Wait Time");
        Assert.That(estimatedWaitTime, Is.EqualTo("Immediate"));
    }

    [Test, Order(2)]
    [Description("Assess patient with minor injury (ESI Level 4)")]
    public void AssessPatientWithMinorInjuryESILevel4()
    {
        // Given a registered patient "Jane Smith" is waiting for triage
        // When I select the patient for triage assessment
        driver.FindElement(By.CssSelector("[data-testid=\"select-patient-for-triage-button\"]")).Click();

        // And I enter the vital signs:
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Blood Pressure"), ("Value", "120/80")),
            Row(("Field", "Heart Rate"), ("Value", "75")),
            Row(("Field", "Respiratory Rate"), ("Value", "16")),
            Row(("Field", "Temperature"), ("Value", "98.2°F")),
            Row(("Field", "Oxygen Saturation"), ("Value", "99%")),
        });
        // And I enter the chief complaint as "Sprained ankle from fall"
        FillField(driver, "Chief Complaint", "Sprained ankle from fall");
        // And I enter the pain scale as "4/10"
        FillField(driver, "Pain Scale", "4/10");
        // And I document onset as "This morning while jogging"
        FillField(driver, "Onset", "This morning while jogging");
        // And I submit the triage assessment
        driver.FindElement(By.CssSelector("[data-testid=\"submit-triage-assessment-form\"]")).Click();

        // Then the system calculates an ESI score of "4"
        var esiScore = GetText(driver, "ESI Score");
        Assert.That(esiScore, Is.EqualTo("4"));
        // And the system assigns triage level "Less Urgent"
        var triageLevel = GetText(driver, "Triage Level");
        Assert.That(triageLevel, Is.EqualTo("Less Urgent"));
        // And the patient is positioned in the less urgent queue
        var assignedQueue = GetText(driver, "Assigned Queue");
        Assert.That(assignedQueue, Does.Match(@"less urgent").IgnoreCase);
        // And the estimated wait time is updated to "60-90 minutes"
        var estimatedWaitTime = GetText(driver, "Estimated Wait Time");
        Assert.That(estimatedWaitTime, Is.EqualTo("60-90 minutes"));
        // And no immediate alerts are generated
        var physicianAlerts = driver.FindElements(Locator("Physician Alert"));
        Assert.That(physicianAlerts.Count, Is.EqualTo(0));
    }

    [Test, Order(3)]
    [Description("Assess critical patient requiring immediate attention (ESI Level 1)")]
    public void AssessCriticalPatientRequiringImmediateAttentionESILevel1()
    {
        // Given a registered patient "Emergency Patient" is waiting for triage
        // When I select the patient for triage assessment
        driver.FindElement(By.CssSelector("[data-testid=\"select-patient-for-triage-button\"]")).Click();

        // And I enter the vital signs:
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Blood Pressure"), ("Value", "70/40")),
            Row(("Field", "Heart Rate"), ("Value", "140")),
            Row(("Field", "Respiratory Rate"), ("Value", "8")),
            Row(("Field", "Temperature"), ("Value", "95.0°F")),
            Row(("Field", "Oxygen Saturation"), ("Value", "85%")),
        });
        // And I enter the chief complaint as "Unresponsive after motor vehicle accident"
        FillField(driver, "Chief Complaint", "Unresponsive after motor vehicle accident");
        // And I enter the pain scale as "Unable to assess"
        FillField(driver, "Pain Scale", "Unable to assess");
        // And I mark the patient as "Requires immediate life-saving intervention"
        FillField(driver, "Intervention Flag", "Requires immediate life-saving intervention");
        // And I submit the triage assessment
        driver.FindElement(By.CssSelector("[data-testid=\"submit-triage-assessment-form\"]")).Click();

        // Then the system calculates an ESI score of "1"
        var esiScore = GetText(driver, "ESI Score");
        Assert.That(esiScore, Is.EqualTo("1"));
        // And the system assigns triage level "Resuscitation"
        var triageLevel = GetText(driver, "Triage Level");
        Assert.That(triageLevel, Is.EqualTo("Resuscitation"));
        // And the patient is moved to the top of all queues
        var queuePosition = GetText(driver, "Queue Position");
        Assert.That(queuePosition, Is.EqualTo("1"));
        // And a code alert is automatically triggered
        var codeAlert = GetText(driver, "Code Alert");
        Assert.That(codeAlert, Does.Match(@"triggered").IgnoreCase);
        // And the trauma team is notified immediately
        var traumaTeamNotification = GetText(driver, "Trauma Team Notification");
        Assert.That(traumaTeamNotification, Does.Match(@"notified").IgnoreCase);
        // And the estimated wait time shows "Immediate - In Progress"
        var estimatedWaitTime = GetText(driver, "Estimated Wait Time");
        Assert.That(estimatedWaitTime, Is.EqualTo("Immediate - In Progress"));
    }

    [Test, Order(4)]
    [Description("Assess pediatric patient with fever (ESI Level 3)")]
    public void AssessPediatricPatientWithFeverESILevel3()
    {
        // Given a registered patient "Tommy Jones" (age 5) is waiting for triage
        // When I select the patient for triage assessment
        driver.FindElement(By.CssSelector("[data-testid=\"select-patient-for-triage-button\"]")).Click();

        // And I enter the vital signs using pediatric parameters:
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Blood Pressure"), ("Value", "95/60")),
            Row(("Field", "Heart Rate"), ("Value", "120")),
            Row(("Field", "Respiratory Rate"), ("Value", "24")),
            Row(("Field", "Temperature"), ("Value", "103.2°F")),
            Row(("Field", "Oxygen Saturation"), ("Value", "97%")),
        });
        // And I enter the chief complaint as "High fever and irritability"
        FillField(driver, "Chief Complaint", "High fever and irritability");
        // And I enter the pain scale as "6/10 (using FACES scale)"
        FillField(driver, "Pain Scale", "6/10 (using FACES scale)");
        // And I document onset as "Fever started yesterday evening"
        FillField(driver, "Onset", "Fever started yesterday evening");
        // And I submit the triage assessment
        driver.FindElement(By.CssSelector("[data-testid=\"submit-triage-assessment-form\"]")).Click();

        // Then the system calculates an ESI score of "3" using pediatric criteria
        var esiScore = GetText(driver, "ESI Score");
        Assert.That(esiScore, Is.EqualTo("3"));
        var scoringCriteria = GetText(driver, "Scoring Criteria");
        Assert.That(scoringCriteria, Does.Match(@"pediatric").IgnoreCase);
        // And the system assigns triage level "Urgent"
        var triageLevel = GetText(driver, "Triage Level");
        Assert.That(triageLevel, Is.EqualTo("Urgent"));
        // And the patient is positioned in the urgent pediatric queue
        var assignedQueue = GetText(driver, "Assigned Queue");
        Assert.That(assignedQueue, Does.Match(@"urgent pediatric").IgnoreCase);
        // And the pediatric team is notified
        var pediatricTeamNotification = GetText(driver, "Pediatric Team Notification");
        Assert.That(pediatricTeamNotification, Does.Match(@"notified").IgnoreCase);
        // And the estimated wait time is updated to "30-45 minutes"
        var estimatedWaitTime = GetText(driver, "Estimated Wait Time");
        Assert.That(estimatedWaitTime, Is.EqualTo("30-45 minutes"));
    }

    [Test, Order(5)]
    [Description("Handle incomplete vital signs during triage")]
    public void HandleIncompleteVitalSignsDuringTriage()
    {
        // Given a registered patient "Mary Johnson" is waiting for triage
        // When I select the patient for triage assessment
        driver.FindElement(By.CssSelector("[data-testid=\"select-patient-for-triage-button\"]")).Click();

        // And I attempt to enter incomplete vital signs:
        //   | Vital Sign          | Value    |
        //   | Blood Pressure      | 130/85   |
        //   | Heart Rate          |          |
        //   | Respiratory Rate    | 18       |
        //   | Temperature         |          |
        //   | Oxygen Saturation   | 98%      |
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Blood Pressure"), ("Value", "130/85")),
            Row(("Field", "Heart Rate"), ("Value", "")),
            Row(("Field", "Respiratory Rate"), ("Value", "18")),
            Row(("Field", "Temperature"), ("Value", "")),
            Row(("Field", "Oxygen Saturation"), ("Value", "98%")),
        });
        // And I enter the chief complaint as "Headache"
        FillField(driver, "Chief Complaint", "Headache");
        // And I submit the triage assessment
        driver.FindElement(By.CssSelector("[data-testid=\"submit-triage-assessment-form\"]")).Click();

        // Then the system displays validation errors:
        //   | Missing Field       | Error Message                |
        //   | Heart Rate          | Heart rate is required       |
        //   | Temperature         | Temperature is required      |
        Assert.That(GetText(driver, "Heart Rate Error"), Is.EqualTo("Heart rate is required"));
        Assert.That(GetText(driver, "Temperature Error"), Is.EqualTo("Temperature is required"));
        // And the ESI score cannot be calculated
        var esiScoreElements = driver.FindElements(Locator("ESI Score"));
        Assert.That(esiScoreElements.Count, Is.EqualTo(0));
        // And the assessment remains incomplete
        var assessmentStatus = GetText(driver, "Assessment Status");
        Assert.That(assessmentStatus, Is.EqualTo("Incomplete"));
        // And I must complete all required fields before proceeding
        var triageForm = driver.FindElement(By.CssSelector("[data-testid=\"triage-assessment-form\"]"));
        Assert.That(triageForm.Displayed, Is.True);
    }

    [Test, Order(6)]
    [Description("Reassess patient with worsening condition")]
    public void ReassessPatientWithWorseningCondition()
    {
        // Given a patient "Robert Davis" has been triaged as ESI Level 4
        // And the patient has been waiting for 90 minutes
        // When I select the patient for reassessment
        driver.FindElement(By.CssSelector("[data-testid=\"select-patient-for-reassessment-button\"]")).Click();

        // And I enter updated vital signs:
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Blood Pressure"), ("Value", "90/50")),
            Row(("Field", "Heart Rate"), ("Value", "120")),
            Row(("Field", "Respiratory Rate"), ("Value", "26")),
            Row(("Field", "Temperature"), ("Value", "101.5°F")),
            Row(("Field", "Oxygen Saturation"), ("Value", "92%")),
        });
        // And I update the chief complaint to "Worsening abdominal pain with nausea"
        FillField(driver, "Chief Complaint", "Worsening abdominal pain with nausea");
        // And I enter the updated pain scale as "9/10"
        FillField(driver, "Pain Scale", "9/10");
        // And I submit the reassessment
        driver.FindElement(By.CssSelector("[data-testid=\"submit-reassessment-form\"]")).Click();

        // Then the system recalculates the ESI score to "2"
        var esiScore = GetText(driver, "ESI Score");
        Assert.That(esiScore, Is.EqualTo("2"));
        // And the system updates triage level to "High Priority"
        var triageLevel = GetText(driver, "Triage Level");
        Assert.That(triageLevel, Is.EqualTo("High Priority"));
        // And the patient is moved to the front of the high priority queue
        var queuePosition = GetText(driver, "Queue Position");
        Assert.That(queuePosition, Is.EqualTo("1"));
        // And an escalation alert is sent to the charge nurse
        var escalationAlert = GetText(driver, "Escalation Alert");
        Assert.That(escalationAlert, Does.Match(@"charge nurse").IgnoreCase);
        // And a note is added documenting the condition change
        var conditionChangeNote = GetText(driver, "Condition Change Note");
        Assert.That(conditionChangeNote.Length > 0, Is.True);
    }

    [Test, Order(7)]
    [Description("Process multiple patients in triage queue")]
    public void ProcessMultiplePatientsInTriageQueue()
    {
        // Given multiple patients are waiting for triage:
        //   | Patient Name    | Registration Time | Status        |
        //   | Alice Brown     | 10:00 AM           | Waiting       |
        //   | Bob Wilson      | 10:15 AM           | Waiting       |
        //   | Carol Davis     | 10:30 AM           | Waiting       |
        // (assumed pre-seeded test data)
        // When I complete triage assessments for all patients:
        //   | Patient Name | ESI Score | Triage Level  |
        //   | Alice Brown  | 3         | Urgent        |
        //   | Bob Wilson   | 4         | Less Urgent   |
        //   | Carol Davis  | 2         | High Priority |
        driver.FindElement(By.CssSelector("[data-testid=\"complete-all-triage-assessments-button\"]")).Click();

        // Then the system positions patients in queue order:
        //   | Queue Position | Patient Name | Triage Level  |
        //   | 1              | Carol Davis  | High Priority |
        //   | 2              | Alice Brown  | Urgent        |
        //   | 3              | Bob Wilson   | Less Urgent   |
        var queueEntries = driver.FindElements(By.CssSelector("[data-testid=\"triage-queue-entry\"]"));
        Assert.That(queueEntries.Count, Is.EqualTo(3));
        var firstQueueEntryText = queueEntries[0].Text;
        Assert.That(firstQueueEntryText, Does.Match(@"Carol Davis"));
        // And wait times are calculated based on queue position and available resources
        var waitTimeCalculationStatus = GetText(driver, "Wait Time Calculation Status");
        Assert.That(waitTimeCalculationStatus, Does.Match(@"calculated").IgnoreCase);
        // And the triage dashboard is updated with current queue status
        var triageDashboard = WaitForTestId(driver, "Triage Dashboard");
        Assert.That(triageDashboard.Displayed, Is.True);
    }
}
