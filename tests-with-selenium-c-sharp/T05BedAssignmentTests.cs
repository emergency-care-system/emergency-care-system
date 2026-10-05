// Selenium WebDriver + NUnit test for
// tests-with-given-when-then-features/05-bed-assignment.feature
// (equivalent to tests-with-selenium-javascript/05-bed-assignment.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see Support/Fields.cs) and the shared
// data-testid contract in Support/Login.cs (login-identity, login-submit,
// app-root).

namespace EmergencyCareSystem.SeleniumTests;

[TestFixture]
[NonParallelizable]
public class T05BedAssignmentTests
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
        //   And I am logged in as a charge nurse
        //   And the bed management module is active
        //   And the patient prioritization algorithm is enabled
        VerifySystemIsOperational(driver);
        Login(driver, "a charge nurse");
        // The bed management module and patient prioritization algorithm are
        // assumed to be pre-seeded/enabled test data.

        var featureNavLink = WaitForTestId(driver, "Nav Bed Assignment");
        featureNavLink.Click();
        WaitForTestId(driver, "Bed Assignment Panel");
    }

    [Test, Order(1)]
    [Description("Assign standard room to highest acuity patient")]
    public void AssignStandardRoomToHighestAcuityPatient()
    {
        // Given multiple patients are waiting for beds:
        // And a standard room "ED-12" becomes available
        // (assumed pre-seeded test data)

        // When I request bed assignment recommendations
        driver.FindElement(By.CssSelector("[data-testid=\"request-recommendations-button\"]")).Click();

        // Then the system suggests "Maria Gonzalez" as the top recommendation
        WaitForTestId(driver, "Top Recommendation");
        var topRecommendation = GetText(driver, "Top Recommendation");
        Assert.That(topRecommendation, Is.EqualTo("Maria Gonzalez"));

        // And the recommendation shows:
        var recommendationFields = new List<Dictionary<string, string>>
        {
            Row(("Field", "Recommended Patient"), ("Value", "Maria Gonzalez")),
            Row(("Field", "ESI Level"), ("Value", "2")),
            Row(("Field", "Wait Time"), ("Value", "45 minutes")),
            Row(("Field", "Room Match"), ("Value", "Standard room suitable")),
            Row(("Field", "Rationale"), ("Value", "Highest acuity patient requiring standard bed")),
        };
        foreach (var rowData in recommendationFields)
        {
            var field = rowData["Field"];
            var value = rowData["Value"];
            Assert.That(GetText(driver, field), Is.EqualTo(value));
        }

        // And the system displays updated wait times for remaining patients:
        var waitTimeUpdates = driver.FindElements(By.CssSelector("[data-testid=\"wait-time-update-row\"]"));
        Assert.That(waitTimeUpdates.Count, Is.EqualTo(3));
    }

    [Test, Order(2)]
    [Description("Assign isolation room based on patient requirements")]
    public void AssignIsolationRoomBasedOnPatientRequirements()
    {
        // Given multiple patients are waiting for beds:
        // And an isolation room "ED-ISO-2" becomes available
        // (assumed pre-seeded test data)

        // When I request bed assignment recommendations
        driver.FindElement(By.CssSelector("[data-testid=\"request-recommendations-button\"]")).Click();

        // Then the system suggests "Alex Johnson" as the top recommendation
        WaitForTestId(driver, "Top Recommendation");
        var topRecommendation = GetText(driver, "Top Recommendation");
        Assert.That(topRecommendation, Is.EqualTo("Alex Johnson"));

        // And the recommendation shows:
        var recommendationFields = new List<Dictionary<string, string>>
        {
            Row(("Field", "Recommended Patient"), ("Value", "Alex Johnson")),
            Row(("Field", "Room Type"), ("Value", "Isolation room")),
            Row(("Field", "Rationale"), ("Value", "Patient requires isolation precautions")),
            Row(("Field", "Infection Control"), ("Value", "Airborne precautions needed")),
        };
        foreach (var rowData in recommendationFields)
        {
            var field = rowData["Field"];
            var value = rowData["Value"];
            Assert.That(GetText(driver, field), Is.EqualTo(value));
        }

        // And the system displays that other patients cannot use this room type
        var roomTypeRestriction = GetText(driver, "Room Type Restriction Notice");
        Assert.That(roomTypeRestriction.Length > 0, Is.True);

        // And the estimated wait times for standard rooms remain unchanged for other patients
        var standardRoomWaitTimeChange = GetText(driver, "Standard Room Wait Time Change");
        Assert.That(standardRoomWaitTimeChange, Is.EqualTo("Unchanged"));
    }

    [Test, Order(3)]
    [Description("Handle pediatric room assignment")]
    public void HandlePediatricRoomAssignment()
    {
        // Given multiple patients are waiting for beds:
        // And a pediatric room "ED-PEDS-3" becomes available
        // (assumed pre-seeded test data)

        // When I request bed assignment recommendations
        driver.FindElement(By.CssSelector("[data-testid=\"request-recommendations-button\"]")).Click();

        // Then the system suggests "Sarah Mitchell" as the top recommendation
        WaitForTestId(driver, "Top Recommendation");
        var topRecommendation = GetText(driver, "Top Recommendation");
        Assert.That(topRecommendation, Is.EqualTo("Sarah Mitchell"));

        // And the recommendation shows:
        var recommendationFields = new List<Dictionary<string, string>>
        {
            Row(("Field", "Recommended Patient"), ("Value", "Sarah Mitchell")),
            Row(("Field", "Age"), ("Value", "4 years old")),
            Row(("Field", "ESI Level"), ("Value", "2")),
            Row(("Field", "Room Type"), ("Value", "Pediatric room")),
            Row(("Field", "Rationale"), ("Value", "Highest acuity pediatric patient")),
        };
        foreach (var rowData in recommendationFields)
        {
            var field = rowData["Field"];
            var value = rowData["Value"];
            Assert.That(GetText(driver, field), Is.EqualTo(value));
        }

        // And the system displays updated wait times for remaining pediatric patients:
        var pediatricWaitTimeUpdates = driver.FindElements(By.CssSelector("[data-testid=\"wait-time-update-row\"]"));
        Assert.That(pediatricWaitTimeUpdates.Count, Is.EqualTo(2));

        // And David Chen remains in the adult standard room queue
        var davidChenQueueStatus = GetText(driver, "David Chen Queue Status");
        Assert.That(davidChenQueueStatus, Does.Match(@"adult standard room queue").IgnoreCase);
    }

    [Test, Order(4)]
    [Description("No suitable patients for available room type")]
    public void NoSuitablePatientsForAvailableRoomType()
    {
        // Given multiple patients are waiting for beds:
        // And a trauma room "ED-TRAUMA-1" becomes available
        // (assumed pre-seeded test data)

        // When I request bed assignment recommendations
        driver.FindElement(By.CssSelector("[data-testid=\"request-recommendations-button\"]")).Click();

        // Then the system displays "No suitable patients for trauma room"
        var noSuitablePatientsMessage = GetText(driver, "No Suitable Patients Message");
        Assert.That(noSuitablePatientsMessage, Is.EqualTo("No suitable patients for trauma room"));

        // And the system suggests:
        var suggestions = new List<Dictionary<string, string>>
        {
            Row(("Recommendation Type", "Alternative Use"), ("Details", "Consider using for high acuity standard patients")),
            Row(("Recommendation Type", "Room Conversion"), ("Details", "Can be downgraded to standard room if needed")),
            Row(("Recommendation Type", "Hold for Emergency"), ("Details", "Keep available for incoming trauma cases")),
        };
        foreach (var suggestion in suggestions)
        {
            Assert.That(GetText(driver, suggestion["Recommendation Type"]), Is.EqualTo(suggestion["Details"]));
        }

        // And the system maintains the trauma room as available
        var traumaRoomStatus = GetText(driver, "Trauma Room Status");
        Assert.That(traumaRoomStatus, Is.EqualTo("Available"));

        // And no patient assignments are automatically made
        var autoAssignments = driver.FindElements(By.CssSelector("[data-testid=\"patient-assignment\"]"));
        Assert.That(autoAssignments.Count, Is.EqualTo(0));
    }

    [Test, Order(5)]
    [Description("Handle multiple rooms becoming available simultaneously")]
    public void HandleMultipleRoomsBecomingAvailableSimultaneously()
    {
        // Given multiple patients are waiting for beds:
        // And multiple rooms become available:
        // (assumed pre-seeded test data)

        // When I request bed assignment recommendations
        driver.FindElement(By.CssSelector("[data-testid=\"request-recommendations-button\"]")).Click();

        // Then the system provides multiple recommendations:
        var recommendations = driver.FindElements(By.CssSelector("[data-testid=\"bed-recommendation-row\"]"));
        Assert.That(recommendations.Count, Is.EqualTo(3));

        // And the system updates wait times for all remaining patients
        WaitForTestId(driver, "Wait Times Updated Notice");

        // And the recommendations are ranked by patient acuity priority
        var rankingOrder = GetText(driver, "Recommendation Ranking Order");
        Assert.That(rankingOrder, Is.EqualTo("Ranked by patient acuity priority"));
    }

    [Test, Order(6)]
    [Description("Consider patient gender for room assignment")]
    public void ConsiderPatientGenderForRoomAssignment()
    {
        // Given multiple patients are waiting for beds:
        // And a standard room "ED-8" becomes available
        // And the room currently has a male patient in the adjacent bed
        // (assumed pre-seeded test data)

        // When I request bed assignment recommendations considering privacy preferences
        driver.FindElement(By.CssSelector("[data-testid=\"request-recommendations-button\"]")).Click();

        // Then the system suggests "Anthony Clark" as the top recommendation
        WaitForTestId(driver, "Top Recommendation");
        var topRecommendation = GetText(driver, "Top Recommendation");
        Assert.That(topRecommendation, Is.EqualTo("Anthony Clark"));

        // And the recommendation includes:
        var recommendationFields = new List<Dictionary<string, string>>
        {
            Row(("Field", "Privacy Consideration"), ("Value", "Same gender as adjacent patient")),
            Row(("Field", "Alternative Option"), ("Value", "Michelle Lee (if privacy not a concern)")),
        };
        foreach (var rowData in recommendationFields)
        {
            var field = rowData["Field"];
            var value = rowData["Value"];
            Assert.That(GetText(driver, field), Is.EqualTo(value));
        }

        // And I can override the gender consideration if clinically necessary
        WaitForTestId(driver, "Override Gender Consideration");
    }

    [Test, Order(7)]
    [Description("Handle bed assignment during high volume period")]
    public void HandleBedAssignmentDuringHighVolumePeriod()
    {
        // Given the ED is operating at 95% capacity
        // And multiple high-acuity patients are waiting:
        // And a standard room "ED-6" becomes available
        // (assumed pre-seeded test data)

        // When I request bed assignment recommendations during crisis mode
        driver.FindElement(By.CssSelector("[data-testid=\"request-recommendations-button\"]")).Click();

        // Then the system prioritizes "Crisis Patient A" despite room type mismatch
        WaitForTestId(driver, "Top Recommendation");
        var topRecommendation = GetText(driver, "Top Recommendation");
        Assert.That(topRecommendation, Is.EqualTo("Crisis Patient A"));

        // And the system displays:
        var alerts = new List<Dictionary<string, string>>
        {
            Row(("Alert Type", "High Volume Alert"), ("Message", "ED at capacity - emergency protocols active")),
            Row(("Alert Type", "Room Flex Option"), ("Message", "Standard room can accommodate ESI Level 1")),
            Row(("Alert Type", "Resource Alert"), ("Message", "Additional equipment may be needed")),
        };
        foreach (var alert in alerts)
        {
            Assert.That(GetText(driver, alert["Alert Type"]), Is.EqualTo(alert["Message"]));
        }

        // And the system suggests moving lower acuity patients to make trauma rooms available
        var traumaRoomSuggestion = GetText(driver, "Trauma Room Availability Suggestion");
        Assert.That(traumaRoomSuggestion, Does.Match(@"moving lower acuity patients").IgnoreCase);
    }

    [Test, Order(8)]
    [Description("Update wait times after bed assignment")]
    public void UpdateWaitTimesAfterBedAssignment()
    {
        // Given the following patients are waiting:
        // And I assign Patient A to the available room
        // (assumed pre-seeded test data)

        // When the bed assignment is confirmed
        driver.FindElement(By.CssSelector("[data-testid=\"confirm-bed-assignment-button\"]")).Click();

        // Then the system recalculates wait times for remaining patients:
        var waitTimeUpdates = driver.FindElements(By.CssSelector("[data-testid=\"wait-time-update-row\"]"));
        Assert.That(waitTimeUpdates.Count, Is.EqualTo(3));

        // And the updated wait times are displayed on the patient tracking board
        WaitForTestId(driver, "Patient Tracking Board");

        // And family members are notified of updated estimates via the patient portal
        var familyNotificationStatus = GetText(driver, "Family Notification Status");
        Assert.That(familyNotificationStatus, Does.Match(@"notified").IgnoreCase);
    }

    [Test, Order(9)]
    [Description("Handle bed assignment rejection and alternative selection")]
    public void HandleBedAssignmentRejectionAndAlternativeSelection()
    {
        // Given the system recommends "John Smith" for room "ED-10"
        // And John Smith has ESI Level 2 with chest pain
        // (assumed pre-seeded test data)

        // When I review the recommendation
        WaitForTestId(driver, "Top Recommendation");

        // And I determine that John Smith needs cardiac monitoring not available in ED-10
        // (clinical judgement, no direct UI action)

        // And I reject the system recommendation
        driver.FindElement(By.CssSelector("[data-testid=\"reject-recommendation-button\"]")).Click();

        // Then the system provides alternative recommendations:
        var alternativeRecommendations = driver.FindElements(By.CssSelector("[data-testid=\"alternative-recommendation-row\"]"));
        Assert.That(alternativeRecommendations.Count, Is.EqualTo(2));

        // And the system suggests alternative rooms for John Smith:
        var alternativeRooms = driver.FindElements(By.CssSelector("[data-testid=\"alternative-room-row\"]"));
        Assert.That(alternativeRooms.Count, Is.EqualTo(2));

        // And I can select an alternative patient or wait for appropriate room for John Smith
        WaitForTestId(driver, "Select Alternative Patient");
    }
}
