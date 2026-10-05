// Selenium WebDriver + NUnit test for
// tests-with-given-when-then-features/06-bed-status-updates.feature
// (equivalent to tests-with-selenium-javascript/06-bed-status-updates.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see Support/Fields.cs) and the shared
// data-testid contract in Support/Login.cs (login-identity, login-submit,
// app-root).

namespace EmergencyCareSystem.SeleniumTests;

[TestFixture]
[NonParallelizable]
public class T06BedStatusUpdatesTests
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
        //   And I am logged in as a nurse
        //   And the bed management module is active
        //   And housekeeping notification system is enabled
        VerifySystemIsOperational(driver);
        Login(driver, "a nurse");
        // The bed management module and housekeeping notification system are
        // assumed to be pre-seeded/enabled test data.

        var featureNavLink = WaitForTestId(driver, "Nav Bed Status Updates");
        featureNavLink.Click();
        WaitForTestId(driver, "Bed Status Updates Panel");
    }

    [Test, Order(1)]
    [Description("Mark bed as needs cleaning after patient discharge")]
    public void MarkBedAsNeedsCleaningAfterPatientDischarge()
    {
        // Given a patient "John Doe" is currently occupying bed "ED-12"
        // And the bed status is "Occupied"
        // And the available bed count shows 8 out of 20 beds available
        // (assumed pre-seeded test data)

        // When the patient is discharged from bed "ED-12"
        driver.FindElement(By.CssSelector("[data-testid=\"discharge-patient-button\"]")).Click();

        // And I mark the bed as "Needs Cleaning"
        FillField(driver, "New Bed Status", "Needs Cleaning");

        // And I submit the bed status update
        driver.FindElement(By.CssSelector("[data-testid=\"submit-bed-status-update\"]")).Click();

        // Then the system updates the bed status to "Dirty"
        WaitForTestId(driver, "Bed Status");
        var bedStatus = GetText(driver, "Bed Status");
        Assert.That(bedStatus, Is.EqualTo("Dirty"));

        // And a notification is sent to housekeeping with details:
        var notificationFields = new List<Dictionary<string, string>>
        {
            Row(("Field", "Room Number"), ("Value", "ED-12")),
            Row(("Field", "Status"), ("Value", "Needs Cleaning")),
            Row(("Field", "Priority"), ("Value", "Standard")),
            Row(("Field", "Patient Type"), ("Value", "Standard discharge")),
            Row(("Field", "Special Requirements"), ("Value", "Standard cleaning protocol")),
            Row(("Field", "Timestamp"), ("Value", "Current time")),
        };
        foreach (var rowData in notificationFields)
        {
            var field = rowData["Field"];
            var value = rowData["Value"];
            Assert.That(GetText(driver, field), Is.EqualTo(value));
        }

        // And the bed is removed from the available bed count
        // And the available bed count updates to 7 out of 20 beds available
        var availableBedCount = GetText(driver, "Available Bed Count");
        Assert.That(availableBedCount, Is.EqualTo("7 out of 20 beds available"));

        // And the bed appears as "Dirty" on the bed management dashboard
        var dashboardBedStatus = GetText(driver, "Dashboard Bed Status");
        Assert.That(dashboardBedStatus, Is.EqualTo("Dirty"));
    }

    [Test, Order(2)]
    [Description("Mark isolation bed for deep cleaning after infectious patient")]
    public void MarkIsolationBedForDeepCleaningAfterInfectiousPatient()
    {
        // Given a patient "Jane Smith" with isolation precautions is in bed "ED-ISO-2"
        // And the bed status is "Occupied - Isolation"
        // And the patient had confirmed MRSA infection
        // (assumed pre-seeded test data)

        // When the patient is discharged from bed "ED-ISO-2"
        driver.FindElement(By.CssSelector("[data-testid=\"discharge-patient-button\"]")).Click();

        // And I mark the bed as "Needs Deep Cleaning"
        FillField(driver, "New Bed Status", "Needs Deep Cleaning");

        // And I specify the isolation type as "Contact Precautions - MRSA"
        FillField(driver, "Isolation Type", "Contact Precautions - MRSA");

        // And I submit the bed status update
        driver.FindElement(By.CssSelector("[data-testid=\"submit-bed-status-update\"]")).Click();

        // Then the system updates the bed status to "Dirty - Isolation"
        WaitForTestId(driver, "Bed Status");
        var bedStatus = GetText(driver, "Bed Status");
        Assert.That(bedStatus, Is.EqualTo("Dirty - Isolation"));

        // And a high-priority notification is sent to housekeeping with details:
        var notificationFields = new List<Dictionary<string, string>>
        {
            Row(("Field", "Room Number"), ("Value", "ED-ISO-2")),
            Row(("Field", "Status"), ("Value", "Needs Deep Cleaning")),
            Row(("Field", "Priority"), ("Value", "High")),
            Row(("Field", "Infection Type"), ("Value", "MRSA - Contact Precautions")),
            Row(("Field", "Special Requirements"), ("Value", "Terminal cleaning required")),
            Row(("Field", "PPE Required"), ("Value", "Gowns, gloves, masks")),
        };
        foreach (var rowData in notificationFields)
        {
            var field = rowData["Field"];
            var value = rowData["Value"];
            Assert.That(GetText(driver, field), Is.EqualTo(value));
        }

        // And the bed is flagged as "Out of Service" until deep cleaning completion
        var bedServiceFlag = GetText(driver, "Bed Service Flag");
        Assert.That(bedServiceFlag, Is.EqualTo("Out of Service"));

        // And the isolation bed count is reduced by one
        WaitForTestId(driver, "Isolation Bed Count");

        // And an alert is sent to infection control team
        WaitForTestId(driver, "Infection Control Alert");
    }

    [Test, Order(3)]
    [Description("Housekeeping completes cleaning and marks bed ready")]
    public void HousekeepingCompletesCleaningAndMarksBedReady()
    {
        // Given bed "ED-8" has status "Dirty"
        // And housekeeping was notified 30 minutes ago
        // (assumed pre-seeded test data)

        // When the housekeeping staff completes cleaning of bed "ED-8"
        driver.FindElement(By.CssSelector("[data-testid=\"complete-cleaning-button\"]")).Click();

        // And the housekeeping supervisor marks the bed as "Clean and Ready"
        FillField(driver, "New Bed Status", "Clean and Ready");

        // And submits the cleaning completion with details:
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Cleaning Staff"), ("Value", "Maria Rodriguez")),
            Row(("Field", "Cleaning Start"), ("Value", "14:30")),
            Row(("Field", "Cleaning End"), ("Value", "15:00")),
            Row(("Field", "Cleaning Type"), ("Value", "Standard")),
            Row(("Field", "Supplies Used"), ("Value", "Standard disinfection")),
        });
        driver.FindElement(By.CssSelector("[data-testid=\"submit-cleaning-completion-form\"]")).Click();

        // Then the system updates the bed status to "Available"
        WaitForTestId(driver, "Bed Status");
        var bedStatus = GetText(driver, "Bed Status");
        Assert.That(bedStatus, Is.EqualTo("Available"));

        // And the bed is added back to the available bed count
        // And the available bed count increases by one
        WaitForTestId(driver, "Available Bed Count");

        // And a notification is sent to the charge nurse that bed "ED-8" is ready
        WaitForTestId(driver, "Charge Nurse Notification");

        // And the bed appears as "Available" on the bed management dashboard
        var dashboardBedStatus = GetText(driver, "Dashboard Bed Status");
        Assert.That(dashboardBedStatus, Is.EqualTo("Available"));
    }

    [Test, Order(4)]
    [Description("Handle bed maintenance request during status update")]
    public void HandleBedMaintenanceRequestDuringStatusUpdate()
    {
        // Given a patient is discharged from bed "ED-15"
        // (assumed pre-seeded test data)

        // When I attempt to mark the bed as "Needs Cleaning"
        FillField(driver, "New Bed Status", "Needs Cleaning");

        // And I notice equipment malfunction in the room
        // (observation, no direct UI action)

        // And I select "Maintenance Required" in addition to cleaning needs
        driver.FindElement(By.CssSelector("[data-testid=\"maintenance-required-checkbox\"]")).Click();

        // And I specify the issue as "IV pump not functioning, call light broken"
        FillField(driver, "Issue Description", "IV pump not functioning, call light broken");

        // And I submit the bed status update
        driver.FindElement(By.CssSelector("[data-testid=\"submit-bed-status-update\"]")).Click();

        // Then the system updates the bed status to "Out of Service - Maintenance"
        WaitForTestId(driver, "Bed Status");
        var bedStatus = GetText(driver, "Bed Status");
        Assert.That(bedStatus, Is.EqualTo("Out of Service - Maintenance"));

        // And notifications are sent to both:
        var notifications = new List<Dictionary<string, string>>
        {
            Row(("Department", "Housekeeping"), ("Notification Details", "Hold cleaning until maintenance")),
            Row(("Department", "Maintenance"), ("Notification Details", "IV pump and call light repair")),
        };
        foreach (var notification in notifications)
        {
            Assert.That(GetText(driver, notification["Department"]), Is.EqualTo(notification["Notification Details"]));
        }

        // And the bed is removed from available count until both issues are resolved
        WaitForTestId(driver, "Available Bed Count");

        // And a work order is automatically generated for maintenance
        WaitForTestId(driver, "Maintenance Work Order");

        // And the estimated downtime is calculated and displayed
        var estimatedDowntime = GetText(driver, "Estimated Downtime");
        Assert.That(estimatedDowntime.Length > 0, Is.True);
    }

    [Test, Order(5)]
    [Description("Update bed status during patient transfer")]
    public void UpdateBedStatusDuringPatientTransfer()
    {
        // Given a patient "Robert Wilson" is in bed "ED-6"
        // And the patient needs to be transferred to ICU
        // (assumed pre-seeded test data)

        // When the transport team arrives to transfer the patient
        driver.FindElement(By.CssSelector("[data-testid=\"transport-team-arrived-button\"]")).Click();

        // And I update the bed status to "Patient in Transit"
        FillField(driver, "New Bed Status", "Patient in Transit");

        // And I specify the destination as "ICU Room 302"
        FillField(driver, "Destination", "ICU Room 302");

        // And I submit the status update
        driver.FindElement(By.CssSelector("[data-testid=\"submit-bed-status-update\"]")).Click();

        // Then the bed status is temporarily set to "In Transit"
        WaitForTestId(driver, "Bed Status");
        var bedStatus = GetText(driver, "Bed Status");
        Assert.That(bedStatus, Is.EqualTo("In Transit"));

        // And the bed remains unavailable for new assignments
        var bedAvailability = GetText(driver, "Bed Availability");
        Assert.That(bedAvailability, Does.Match(@"unavailable").IgnoreCase);

        // And a notification is sent to the receiving unit
        WaitForTestId(driver, "Receiving Unit Notification");

        // And when the transfer is confirmed complete, I can mark the bed as "Needs Cleaning"
        driver.FindElement(By.CssSelector("[data-testid=\"confirm-transfer-complete-button\"]")).Click();
        FillField(driver, "New Bed Status", "Needs Cleaning");

        // And the normal cleaning workflow is initiated
        WaitForTestId(driver, "Cleaning Workflow Status");
    }

    [Test, Order(6)]
    [Description("Handle multiple bed status updates simultaneously")]
    public void HandleMultipleBedStatusUpdatesSimultaneously()
    {
        // Given multiple beds require status updates:
        // (assumed pre-seeded test data)

        // When I perform batch bed status updates:
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "ED-3 New Status"), ("Value", "Needs Cleaning")),
            Row(("Field", "ED-7 New Status"), ("Value", "Patient in Transit")),
            Row(("Field", "ED-11 New Status"), ("Value", "Available")),
            Row(("Field", "ED-14 New Status"), ("Value", "Needs Cleaning")),
        });
        driver.FindElement(By.CssSelector("[data-testid=\"submit-batch-bed-status-update\"]")).Click();

        // Then the system processes all updates simultaneously
        WaitForTestId(driver, "Batch Update Status");

        // And appropriate notifications are sent to all relevant departments
        WaitForTestId(driver, "Department Notifications Sent");

        // And the bed availability dashboard is updated in real-time
        WaitForTestId(driver, "Bed Availability Dashboard");

        // And the total available bed count reflects all changes
        WaitForTestId(driver, "Available Bed Count");
    }

    [Test, Order(7)]
    [Description("Handle urgent bed turnover request")]
    public void HandleUrgentBedTurnoverRequest()
    {
        // Given the ED is at 95% capacity
        // And there is a trauma patient incoming requiring immediate bed
        // And bed "ED-4" patient is ready for discharge
        // (assumed pre-seeded test data)

        // When I mark the discharge as "Urgent Turnover Required"
        FillField(driver, "New Bed Status", "Urgent Turnover Required");

        // And I request expedited cleaning for bed "ED-4"
        driver.FindElement(By.CssSelector("[data-testid=\"request-expedited-cleaning-button\"]")).Click();

        // And I submit the urgent status update
        driver.FindElement(By.CssSelector("[data-testid=\"submit-bed-status-update\"]")).Click();

        // Then the system updates bed status to "Dirty - Urgent"
        WaitForTestId(driver, "Bed Status");
        var bedStatus = GetText(driver, "Bed Status");
        Assert.That(bedStatus, Is.EqualTo("Dirty - Urgent"));

        // And a high-priority notification is sent to housekeeping:
        var notificationFields = new List<Dictionary<string, string>>
        {
            Row(("Field", "Priority Level"), ("Value", "URGENT")),
            Row(("Field", "Room Number"), ("Value", "ED-4")),
            Row(("Field", "Reason"), ("Value", "Incoming trauma patient")),
            Row(("Field", "Target Time"), ("Value", "15 minutes")),
            Row(("Field", "Special Instructions"), ("Value", "Expedited cleaning protocol")),
        };
        foreach (var rowData in notificationFields)
        {
            var field = rowData["Field"];
            var value = rowData["Value"];
            Assert.That(GetText(driver, field), Is.EqualTo(value));
        }

        // And the charge nurse is notified of the urgent turnover request
        WaitForTestId(driver, "Charge Nurse Notification");

        // And a timer is started to track cleaning completion time
        WaitForTestId(driver, "Cleaning Completion Timer");
    }

    [Test, Order(8)]
    [Description("Validate bed status change restrictions")]
    public void ValidateBedStatusChangeRestrictions()
    {
        // Given bed "ED-9" currently has status "Occupied"
        // And a patient "Susan Davis" is actively receiving treatment
        // (assumed pre-seeded test data)

        // When I attempt to mark the bed as "Available"
        FillField(driver, "New Bed Status", "Available");

        // And I submit the invalid status change
        driver.FindElement(By.CssSelector("[data-testid=\"submit-bed-status-update\"]")).Click();

        // Then the system displays a validation error:
        var validationErrors = new List<Dictionary<string, string>>
        {
            Row(("Error Type", "Invalid Transition"), ("Message", "Cannot mark occupied bed as available")),
            Row(("Error Type", "Required Action"), ("Message", "Discharge patient first")),
            Row(("Error Type", "Current Patient"), ("Message", "Susan Davis - Active treatment")),
        };
        foreach (var error in validationErrors)
        {
            Assert.That(GetText(driver, error["Error Type"]), Is.EqualTo(error["Message"]));
        }

        // And the bed status remains "Occupied"
        var bedStatus = GetText(driver, "Bed Status");
        Assert.That(bedStatus, Is.EqualTo("Occupied"));

        // And no notifications are sent
        var notifications = driver.FindElements(By.CssSelector("[data-testid=\"housekeeping-notification\"]"));
        Assert.That(notifications.Count, Is.EqualTo(0));

        // And I am prompted to follow proper discharge workflow
        WaitForTestId(driver, "Discharge Workflow Prompt");
    }

    [Test, Order(9)]
    [Description("Track bed status history and audit trail")]
    public void TrackBedStatusHistoryAndAuditTrail()
    {
        // Given bed "ED-5" has had multiple status changes today
        // (assumed pre-seeded test data)

        // When I access the bed status history
        driver.FindElement(By.CssSelector("[data-testid=\"view-bed-status-history-button\"]")).Click();

        // Then the system displays the complete audit trail:
        var auditTrailEntries = driver.FindElements(By.CssSelector("[data-testid=\"audit-trail-entry\"]"));
        Assert.That(auditTrailEntries.Count, Is.EqualTo(5));

        // And each status change includes timestamp and user identification
        WaitForTestId(driver, "Audit Trail");

        // And the audit trail is preserved for compliance reporting
        var complianceStatus = GetText(driver, "Compliance Reporting Status");
        Assert.That(complianceStatus, Does.Match(@"preserved").IgnoreCase);

        // And I can generate reports on bed utilization patterns
        WaitForTestId(driver, "Generate Utilization Report Button");
    }

    [Test, Order(10)]
    [Description("Handle bed status update during system maintenance")]
    public void HandleBedStatusUpdateDuringSystemMaintenance()
    {
        // Given the housekeeping notification system is temporarily offline
        // And a patient is discharged from bed "ED-16"
        // (assumed pre-seeded test data)

        // When I mark the bed as "Needs Cleaning"
        FillField(driver, "New Bed Status", "Needs Cleaning");

        // And I submit the status update
        driver.FindElement(By.CssSelector("[data-testid=\"submit-bed-status-update\"]")).Click();

        // Then the system updates the bed status to "Dirty"
        WaitForTestId(driver, "Bed Status");
        var bedStatus = GetText(driver, "Bed Status");
        Assert.That(bedStatus, Is.EqualTo("Dirty"));

        // And the system queues the housekeeping notification for later delivery
        WaitForTestId(driver, "Queued Notification Status");

        // And a warning message is displayed: "Housekeeping system offline - notification queued"
        var warningMessage = GetText(driver, "Warning Message");
        Assert.That(warningMessage, Is.EqualTo("Housekeeping system offline - notification queued"));

        // And the bed is still removed from available count
        WaitForTestId(driver, "Available Bed Count");

        // And when the housekeeping system comes back online, queued notifications are automatically sent
        // (system behavior outside the scope of this interaction)

        // And a log entry is created documenting the delayed notification
        WaitForTestId(driver, "Delayed Notification Log Entry");
    }
}
