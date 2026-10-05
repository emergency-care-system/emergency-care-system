// Playwright + NUnit test for
// tests-with-given-when-then-features/03-initial-triage-assessment.feature
// (equivalent to tests-with-playwright-javascript/03-initial-triage-assessment.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see Support/Fields.cs) and the shared
// data-testid contract in Support/Login.cs (login-identity, login-submit,
// app-root).

namespace EmergencyCareSystem.PlaywrightTests;

[TestFixture]
[NonParallelizable]
public class T03InitialTriageAssessmentTests
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
        //   And I am logged in as a triage nurse
        //   And the ESI (Emergency Severity Index) scoring module is active
        await VerifySystemIsOperational(page);
        await Login(page, "a triage nurse");
        // The ESI scoring module being active is assumed pre-seeded test data /
        // environment configuration.

        var featureNavLink = await WaitForTestId(page, "Nav Initial Triage Assessment");
        await featureNavLink.ClickAsync();
        await WaitForTestId(page, "Initial Triage Assessment Panel");
    }

    [Test, Order(1)]
    [Description("Assess patient with chest pain (ESI Level 2)")]
    public async Task AssessPatientWithChestPainESILevel2()
    {
        // Given a registered patient "John Doe" is waiting for triage
        // And the patient was registered 10 minutes ago
        // (assumed pre-seeded test data)
        // When I select the patient for triage assessment
        await page.GetByTestId("select-patient-for-triage-button").First.ClickAsync();

        // And I enter the vital signs:
        await FillFields(page, new List<Dictionary<string, string>>
        {
            Row(("Field", "Blood Pressure"), ("Value", "160/95")),
            Row(("Field", "Heart Rate"), ("Value", "110")),
            Row(("Field", "Respiratory Rate"), ("Value", "22")),
            Row(("Field", "Temperature"), ("Value", "98.6°F")),
            Row(("Field", "Oxygen Saturation"), ("Value", "94%")),
        });
        // And I enter the chief complaint as "Chest pain and shortness of breath"
        await FillField(page, "Chief Complaint", "Chest pain and shortness of breath");
        // And I enter the pain scale as "8/10"
        await FillField(page, "Pain Scale", "8/10");
        // And I document onset as "Started 2 hours ago"
        await FillField(page, "Onset", "Started 2 hours ago");
        // And I submit the triage assessment
        await page.GetByTestId("submit-triage-assessment-form").First.ClickAsync();

        // Then the system calculates an ESI score of "2"
        var esiScore = await GetText(page, "ESI Score");
        Assert.That(esiScore, Is.EqualTo("2"));
        // And the system assigns triage level "High Priority"
        var triageLevel = await GetText(page, "Triage Level");
        Assert.That(triageLevel, Is.EqualTo("High Priority"));
        // And the patient is positioned at the front of the high priority queue
        var queuePosition = await GetText(page, "Queue Position");
        Assert.That(queuePosition, Is.EqualTo("1"));
        // And an alert is sent to the attending physician
        var physicianAlert = await GetText(page, "Physician Alert");
        Assert.That(physicianAlert, Does.Match(@"attending physician").IgnoreCase);
        // And the estimated wait time is updated to "Immediate"
        var estimatedWaitTime = await GetText(page, "Estimated Wait Time");
        Assert.That(estimatedWaitTime, Is.EqualTo("Immediate"));
    }

    [Test, Order(2)]
    [Description("Assess patient with minor injury (ESI Level 4)")]
    public async Task AssessPatientWithMinorInjuryESILevel4()
    {
        // Given a registered patient "Jane Smith" is waiting for triage
        // When I select the patient for triage assessment
        await page.GetByTestId("select-patient-for-triage-button").First.ClickAsync();

        // And I enter the vital signs:
        await FillFields(page, new List<Dictionary<string, string>>
        {
            Row(("Field", "Blood Pressure"), ("Value", "120/80")),
            Row(("Field", "Heart Rate"), ("Value", "75")),
            Row(("Field", "Respiratory Rate"), ("Value", "16")),
            Row(("Field", "Temperature"), ("Value", "98.2°F")),
            Row(("Field", "Oxygen Saturation"), ("Value", "99%")),
        });
        // And I enter the chief complaint as "Sprained ankle from fall"
        await FillField(page, "Chief Complaint", "Sprained ankle from fall");
        // And I enter the pain scale as "4/10"
        await FillField(page, "Pain Scale", "4/10");
        // And I document onset as "This morning while jogging"
        await FillField(page, "Onset", "This morning while jogging");
        // And I submit the triage assessment
        await page.GetByTestId("submit-triage-assessment-form").First.ClickAsync();

        // Then the system calculates an ESI score of "4"
        var esiScore = await GetText(page, "ESI Score");
        Assert.That(esiScore, Is.EqualTo("4"));
        // And the system assigns triage level "Less Urgent"
        var triageLevel = await GetText(page, "Triage Level");
        Assert.That(triageLevel, Is.EqualTo("Less Urgent"));
        // And the patient is positioned in the less urgent queue
        var assignedQueue = await GetText(page, "Assigned Queue");
        Assert.That(assignedQueue, Does.Match(@"less urgent").IgnoreCase);
        // And the estimated wait time is updated to "60-90 minutes"
        var estimatedWaitTime = await GetText(page, "Estimated Wait Time");
        Assert.That(estimatedWaitTime, Is.EqualTo("60-90 minutes"));
        // And no immediate alerts are generated
        var physicianAlerts = Locator(page, "Physician Alert");
        Assert.That(await physicianAlerts.CountAsync(), Is.EqualTo(0));
    }

    [Test, Order(3)]
    [Description("Assess critical patient requiring immediate attention (ESI Level 1)")]
    public async Task AssessCriticalPatientRequiringImmediateAttentionESILevel1()
    {
        // Given a registered patient "Emergency Patient" is waiting for triage
        // When I select the patient for triage assessment
        await page.GetByTestId("select-patient-for-triage-button").First.ClickAsync();

        // And I enter the vital signs:
        await FillFields(page, new List<Dictionary<string, string>>
        {
            Row(("Field", "Blood Pressure"), ("Value", "70/40")),
            Row(("Field", "Heart Rate"), ("Value", "140")),
            Row(("Field", "Respiratory Rate"), ("Value", "8")),
            Row(("Field", "Temperature"), ("Value", "95.0°F")),
            Row(("Field", "Oxygen Saturation"), ("Value", "85%")),
        });
        // And I enter the chief complaint as "Unresponsive after motor vehicle accident"
        await FillField(page, "Chief Complaint", "Unresponsive after motor vehicle accident");
        // And I enter the pain scale as "Unable to assess"
        await FillField(page, "Pain Scale", "Unable to assess");
        // And I mark the patient as "Requires immediate life-saving intervention"
        await FillField(page, "Intervention Flag", "Requires immediate life-saving intervention");
        // And I submit the triage assessment
        await page.GetByTestId("submit-triage-assessment-form").First.ClickAsync();

        // Then the system calculates an ESI score of "1"
        var esiScore = await GetText(page, "ESI Score");
        Assert.That(esiScore, Is.EqualTo("1"));
        // And the system assigns triage level "Resuscitation"
        var triageLevel = await GetText(page, "Triage Level");
        Assert.That(triageLevel, Is.EqualTo("Resuscitation"));
        // And the patient is moved to the top of all queues
        var queuePosition = await GetText(page, "Queue Position");
        Assert.That(queuePosition, Is.EqualTo("1"));
        // And a code alert is automatically triggered
        var codeAlert = await GetText(page, "Code Alert");
        Assert.That(codeAlert, Does.Match(@"triggered").IgnoreCase);
        // And the trauma team is notified immediately
        var traumaTeamNotification = await GetText(page, "Trauma Team Notification");
        Assert.That(traumaTeamNotification, Does.Match(@"notified").IgnoreCase);
        // And the estimated wait time shows "Immediate - In Progress"
        var estimatedWaitTime = await GetText(page, "Estimated Wait Time");
        Assert.That(estimatedWaitTime, Is.EqualTo("Immediate - In Progress"));
    }

    [Test, Order(4)]
    [Description("Assess pediatric patient with fever (ESI Level 3)")]
    public async Task AssessPediatricPatientWithFeverESILevel3()
    {
        // Given a registered patient "Tommy Jones" (age 5) is waiting for triage
        // When I select the patient for triage assessment
        await page.GetByTestId("select-patient-for-triage-button").First.ClickAsync();

        // And I enter the vital signs using pediatric parameters:
        await FillFields(page, new List<Dictionary<string, string>>
        {
            Row(("Field", "Blood Pressure"), ("Value", "95/60")),
            Row(("Field", "Heart Rate"), ("Value", "120")),
            Row(("Field", "Respiratory Rate"), ("Value", "24")),
            Row(("Field", "Temperature"), ("Value", "103.2°F")),
            Row(("Field", "Oxygen Saturation"), ("Value", "97%")),
        });
        // And I enter the chief complaint as "High fever and irritability"
        await FillField(page, "Chief Complaint", "High fever and irritability");
        // And I enter the pain scale as "6/10 (using FACES scale)"
        await FillField(page, "Pain Scale", "6/10 (using FACES scale)");
        // And I document onset as "Fever started yesterday evening"
        await FillField(page, "Onset", "Fever started yesterday evening");
        // And I submit the triage assessment
        await page.GetByTestId("submit-triage-assessment-form").First.ClickAsync();

        // Then the system calculates an ESI score of "3" using pediatric criteria
        var esiScore = await GetText(page, "ESI Score");
        Assert.That(esiScore, Is.EqualTo("3"));
        var scoringCriteria = await GetText(page, "Scoring Criteria");
        Assert.That(scoringCriteria, Does.Match(@"pediatric").IgnoreCase);
        // And the system assigns triage level "Urgent"
        var triageLevel = await GetText(page, "Triage Level");
        Assert.That(triageLevel, Is.EqualTo("Urgent"));
        // And the patient is positioned in the urgent pediatric queue
        var assignedQueue = await GetText(page, "Assigned Queue");
        Assert.That(assignedQueue, Does.Match(@"urgent pediatric").IgnoreCase);
        // And the pediatric team is notified
        var pediatricTeamNotification = await GetText(page, "Pediatric Team Notification");
        Assert.That(pediatricTeamNotification, Does.Match(@"notified").IgnoreCase);
        // And the estimated wait time is updated to "30-45 minutes"
        var estimatedWaitTime = await GetText(page, "Estimated Wait Time");
        Assert.That(estimatedWaitTime, Is.EqualTo("30-45 minutes"));
    }

    [Test, Order(5)]
    [Description("Handle incomplete vital signs during triage")]
    public async Task HandleIncompleteVitalSignsDuringTriage()
    {
        // Given a registered patient "Mary Johnson" is waiting for triage
        // When I select the patient for triage assessment
        await page.GetByTestId("select-patient-for-triage-button").First.ClickAsync();

        // And I attempt to enter incomplete vital signs:
        //   | Vital Sign          | Value    |
        //   | Blood Pressure      | 130/85   |
        //   | Heart Rate          |          |
        //   | Respiratory Rate    | 18       |
        //   | Temperature         |          |
        //   | Oxygen Saturation   | 98%      |
        await FillFields(page, new List<Dictionary<string, string>>
        {
            Row(("Field", "Blood Pressure"), ("Value", "130/85")),
            Row(("Field", "Heart Rate"), ("Value", "")),
            Row(("Field", "Respiratory Rate"), ("Value", "18")),
            Row(("Field", "Temperature"), ("Value", "")),
            Row(("Field", "Oxygen Saturation"), ("Value", "98%")),
        });
        // And I enter the chief complaint as "Headache"
        await FillField(page, "Chief Complaint", "Headache");
        // And I submit the triage assessment
        await page.GetByTestId("submit-triage-assessment-form").First.ClickAsync();

        // Then the system displays validation errors:
        //   | Missing Field       | Error Message                |
        //   | Heart Rate          | Heart rate is required       |
        //   | Temperature         | Temperature is required      |
        Assert.That((await GetText(page, "Heart Rate Error")), Is.EqualTo("Heart rate is required"));
        Assert.That((await GetText(page, "Temperature Error")), Is.EqualTo("Temperature is required"));
        // And the ESI score cannot be calculated
        var esiScoreElements = Locator(page, "ESI Score");
        Assert.That(await esiScoreElements.CountAsync(), Is.EqualTo(0));
        // And the assessment remains incomplete
        var assessmentStatus = await GetText(page, "Assessment Status");
        Assert.That(assessmentStatus, Is.EqualTo("Incomplete"));
        // And I must complete all required fields before proceeding
        var triageForm = page.GetByTestId("triage-assessment-form").First;
        Assert.That((await triageForm.IsVisibleAsync()), Is.True);
    }

    [Test, Order(6)]
    [Description("Reassess patient with worsening condition")]
    public async Task ReassessPatientWithWorseningCondition()
    {
        // Given a patient "Robert Davis" has been triaged as ESI Level 4
        // And the patient has been waiting for 90 minutes
        // When I select the patient for reassessment
        await page.GetByTestId("select-patient-for-reassessment-button").First.ClickAsync();

        // And I enter updated vital signs:
        await FillFields(page, new List<Dictionary<string, string>>
        {
            Row(("Field", "Blood Pressure"), ("Value", "90/50")),
            Row(("Field", "Heart Rate"), ("Value", "120")),
            Row(("Field", "Respiratory Rate"), ("Value", "26")),
            Row(("Field", "Temperature"), ("Value", "101.5°F")),
            Row(("Field", "Oxygen Saturation"), ("Value", "92%")),
        });
        // And I update the chief complaint to "Worsening abdominal pain with nausea"
        await FillField(page, "Chief Complaint", "Worsening abdominal pain with nausea");
        // And I enter the updated pain scale as "9/10"
        await FillField(page, "Pain Scale", "9/10");
        // And I submit the reassessment
        await page.GetByTestId("submit-reassessment-form").First.ClickAsync();

        // Then the system recalculates the ESI score to "2"
        var esiScore = await GetText(page, "ESI Score");
        Assert.That(esiScore, Is.EqualTo("2"));
        // And the system updates triage level to "High Priority"
        var triageLevel = await GetText(page, "Triage Level");
        Assert.That(triageLevel, Is.EqualTo("High Priority"));
        // And the patient is moved to the front of the high priority queue
        var queuePosition = await GetText(page, "Queue Position");
        Assert.That(queuePosition, Is.EqualTo("1"));
        // And an escalation alert is sent to the charge nurse
        var escalationAlert = await GetText(page, "Escalation Alert");
        Assert.That(escalationAlert, Does.Match(@"charge nurse").IgnoreCase);
        // And a note is added documenting the condition change
        var conditionChangeNote = await GetText(page, "Condition Change Note");
        Assert.That(conditionChangeNote.Length > 0, Is.True);
    }

    [Test, Order(7)]
    [Description("Process multiple patients in triage queue")]
    public async Task ProcessMultiplePatientsInTriageQueue()
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
        await page.GetByTestId("complete-all-triage-assessments-button").First.ClickAsync();

        // Then the system positions patients in queue order:
        //   | Queue Position | Patient Name | Triage Level  |
        //   | 1              | Carol Davis  | High Priority |
        //   | 2              | Alice Brown  | Urgent        |
        //   | 3              | Bob Wilson   | Less Urgent   |
        var queueEntries = page.GetByTestId("triage-queue-entry");
        Assert.That(await queueEntries.CountAsync(), Is.EqualTo(3));
        var firstQueueEntryText = await TextOf(queueEntries.Nth(0));
        Assert.That(firstQueueEntryText, Does.Match(@"Carol Davis"));
        // And wait times are calculated based on queue position and available resources
        var waitTimeCalculationStatus = await GetText(page, "Wait Time Calculation Status");
        Assert.That(waitTimeCalculationStatus, Does.Match(@"calculated").IgnoreCase);
        // And the triage dashboard is updated with current queue status
        var triageDashboard = await WaitForTestId(page, "Triage Dashboard");
        Assert.That((await triageDashboard.IsVisibleAsync()), Is.True);
    }
}
