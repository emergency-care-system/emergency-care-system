// Playwright + NUnit test for
// tests-with-given-when-then-features/14-provider-assignment.feature
// (equivalent to tests-with-playwright-javascript/14-provider-assignment.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see Support/Fields.cs) and the shared
// data-testid contract in Support/Login.cs (login-identity, login-submit,
// app-root).

namespace EmergencyCareSystem.PlaywrightTests;

[TestFixture]
[NonParallelizable]
public class T14ProviderAssignmentTests
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
        //   And the provider assignment module is active
        //   And provider workload tracking is enabled
        //   And mobile notification system is functional
        await VerifySystemIsOperational(page);
        await Login(page, "a charge nurse");
        // The provider assignment module, provider workload tracking, and the
        // mobile notification system are assumed to be active/enabled backend
        // configuration already in place for this environment.

        var providerAssignmentNavLink = await WaitForTestId(page, "Nav Provider Assignment");
        await providerAssignmentNavLink.ClickAsync();
        await WaitForTestId(page, "Provider Assignment Panel");
    }

    [Test, Order(1)]
    [Description("Assign highest priority patient to newly available physician")]
    public async Task AssignHighestPriorityPatientToNewlyAvailablePhysician()
    {
        // Given "Dr. Johnson" was seeing a patient in bed "ED-8"
        // And the current patient queue contains:
        //   | Position | Patient Name    | ESI Level | Triage Level   | Wait Time | Bed Ready |
        //   | 1        | Maria Santos    | 2         | High Priority  | 45 min    | Yes       |
        //   | 2        | Robert Kim      | 2         | High Priority  | 60 min    | Yes       |
        //   | 3        | Lisa Chen       | 3         | Urgent         | 90 min    | Yes       |
        //   | 4        | David Brown     | 3         | Urgent         | 105 min   | No        |
        // (assumed pre-seeded test data)
        // When "Dr. Johnson" completes the discharge for the patient in bed "ED-8"
        // And the system detects "Dr. Johnson" is now available
        // (assumed to have already occurred / triggered by the system)

        // Then the system identifies the next patient assignment:
        await WaitForTestId(page, "Highest Priority");
        var nextAssignmentCriteria = new List<Dictionary<string, string>>
        {
            Row(("Criteria", "Highest Priority"), ("Value", "Maria Santos (ESI Level 2)")),
            Row(("Criteria", "Bed Availability"), ("Value", "Bed ready for immediate assignment")),
            Row(("Criteria", "Provider Match"), ("Value", "Dr. Johnson available and qualified")),
        };
        foreach (var rowData in nextAssignmentCriteria)
        {
            var criteria = rowData["Criteria"];
            var value = rowData["Value"];
            Assert.That((await GetText(page, criteria)), Is.EqualTo(value));
        }

        // And the system assigns "Maria Santos" to "Dr. Johnson"
        Assert.That((await GetText(page, "Assigned Patient")), Is.EqualTo("Maria Santos"));
        Assert.That((await GetText(page, "Assigned Provider")), Is.EqualTo("Dr. Johnson"));

        // And a notification is sent to Dr. Johnson's mobile device:
        var mobileNotification = new List<Dictionary<string, string>>
        {
            Row(("Type", "Patient Assignment"), ("Content", "📱 New Patient: Maria Santos, Bed ED-12")),
            Row(("Type", "Priority Level"), ("Content", "ESI Level 2 - High Priority")),
            Row(("Type", "Chief Complaint"), ("Content", "Severe chest pain")),
            Row(("Type", "Wait Time"), ("Content", "Patient waiting 45 minutes")),
            Row(("Type", "Action Required"), ("Content", "Please proceed to ED-12")),
        };
        foreach (var rowData in mobileNotification)
        {
            var type = rowData["Type"];
            var content = rowData["Content"];
            Assert.That((await GetText(page, type)), Is.EqualTo(content));
        }

        // And the patient status is updated to "Assigned to Dr. Johnson"
        Assert.That((await GetText(page, "Patient Status")), Is.EqualTo("Assigned to Dr. Johnson"));

        // And the queue position is updated for remaining patients
        var patientQueue = await WaitForTestId(page, "Patient Queue");
        Assert.That((await patientQueue.IsVisibleAsync()), Is.True);
    }

    [Test, Order(2)]
    [Description("Handle provider assignment with specialty requirements")]
    public async Task HandleProviderAssignmentWithSpecialtyRequirements()
    {
        // Given "Dr. Martinez" (Emergency Medicine) becomes available
        // And "Dr. Patel" (Pediatric Emergency) becomes available
        // And the current queue contains:
        //   | Patient Name     | Age | ESI Level | Specialty Required     | Wait Time |
        //   | Adult Patient    | 45  | 2         | Emergency Medicine     | 30 min    |
        //   | Child Patient    | 8   | 2         | Pediatric Emergency    | 35 min    |
        //   | General Patient  | 30  | 3         | Any                    | 60 min    |
        // (assumed pre-seeded test data)
        // When both providers request their next patient assignment
        // (assumed to have already occurred / triggered by the system)

        // Then the system matches providers to appropriate patients:
        await WaitForTestId(page, "Dr. Patel Assigned Patient");
        var providerMatches = new List<Dictionary<string, string>>
        {
            Row(("Provider", "Dr. Patel"), ("Assigned Patient", "Child Patient")),
            Row(("Provider", "Dr. Martinez"), ("Assigned Patient", "Adult Patient")),
        };
        foreach (var row in providerMatches)
        {
            Assert.That((await GetText(page, $"{row["Provider"]} Assigned Patient")), Is.EqualTo(row["Assigned Patient"]));
        }

        // And specialty-specific notifications are sent:
        var specialtyNotifications = new List<Dictionary<string, string>>
        {
            Row(("Provider", "Dr. Patel"), ("Content", "👶 Pediatric Patient: Age 8, ESI 2, Fever")),
            Row(("Provider", "Dr. Martinez"), ("Content", "🏥 Adult Patient: Age 45, ESI 2, Chest pain")),
        };
        foreach (var rowData in specialtyNotifications)
        {
            var provider = rowData["Provider"];
            var content = rowData["Content"];
            Assert.That((await GetText(page, $"{provider} Notification")), Is.EqualTo(content));
        }

        // And the general patient remains in queue for the next available provider
        var generalPatientStatus = await GetText(page, "General Patient Queue Status");
        Assert.That(generalPatientStatus, Does.Match(@"queue").IgnoreCase);
    }

    [Test, Order(3)]
    [Description("Prioritize critical patient over standard queue order")]
    public async Task PrioritizeCriticalPatientOverStandardQueueOrder()
    {
        // Given "Dr. Thompson" becomes available
        // And the queue contains patients in order:
        //   | Position | Patient Name    | ESI Level | Assigned Bed | Special Circumstances |
        //   | 1        | Standard Patient| 3         | ED-5         | None                  |
        //   | 2        | Urgent Patient  | 3         | ED-7         | None                  |
        //   | 3        | Critical Patient| 1         | ED-TRAUMA-1  | Just arrived          |
        // (assumed pre-seeded test data)
        // When the system identifies the next patient for "Dr. Thompson"
        // (assumed to have already occurred / triggered by the system)

        // Then the system prioritizes by acuity over queue position:
        await WaitForTestId(page, "Skip Queue Order");
        var prioritizationLogic = new List<Dictionary<string, string>>
        {
            Row(("Logic", "Skip Queue Order"), ("Reasoning", "ESI Level 1 takes priority over Level 3")),
            Row(("Logic", "Critical Priority"), ("Reasoning", "Life-threatening condition requires immediate")),
            Row(("Logic", "Provider Capability"), ("Reasoning", "Dr. Thompson qualified for trauma cases")),
        };
        foreach (var rowData in prioritizationLogic)
        {
            var logic = rowData["Logic"];
            var reasoning = rowData["Reasoning"];
            Assert.That((await GetText(page, logic)), Is.EqualTo(reasoning));
        }

        // And "Critical Patient" is assigned to "Dr. Thompson"
        Assert.That((await GetText(page, "Assigned Patient")), Is.EqualTo("Critical Patient"));
        Assert.That((await GetText(page, "Assigned Provider")), Is.EqualTo("Dr. Thompson"));

        // And the mobile notification includes urgency indicators:
        var urgencyIndicators = new List<Dictionary<string, string>>
        {
            Row(("Field", "Priority Alert"), ("Content", "🚨 CRITICAL: ESI Level 1 - Trauma")),
            Row(("Field", "Patient Location"), ("Content", "ED-TRAUMA-1")),
            Row(("Field", "Immediate Action"), ("Content", "Requires immediate assessment")),
            Row(("Field", "Support Teams"), ("Content", "Trauma team standing by")),
        };
        foreach (var rowData in urgencyIndicators)
        {
            var field = rowData["Field"];
            var content = rowData["Content"];
            Assert.That((await GetText(page, field)), Is.EqualTo(content));
        }
    }

    [Test, Order(4)]
    [Description("Handle provider assignment during high volume period")]
    public async Task HandleProviderAssignmentDuringHighVolumePeriod()
    {
        // Given the ED is operating at 95% capacity
        // And multiple providers become available simultaneously:
        //   | Provider Name | Specialty          | Last Patient Completed |
        //   | Dr. Adams     | Emergency Medicine | 14:30                 |
        //   | Dr. Brown     | Emergency Medicine | 14:32                 |
        //   | Dr. Wilson    | Emergency Medicine | 14:35                 |
        // And 12 patients are waiting to be seen
        // (assumed pre-seeded test data)
        // When the system processes multiple provider assignments
        // (assumed to have already occurred / triggered by the system)

        // Then the system optimizes assignments across all available providers:
        await WaitForTestId(page, "Dr. Adams Assigned Patient");
        var optimizedAssignments = new List<Dictionary<string, string>>
        {
            Row(("Provider", "Dr. Adams"), ("Assigned Patient", "Patient A")),
            Row(("Provider", "Dr. Brown"), ("Assigned Patient", "Patient B")),
            Row(("Provider", "Dr. Wilson"), ("Assigned Patient", "Patient C")),
        };
        foreach (var row in optimizedAssignments)
        {
            Assert.That((await GetText(page, $"{row["Provider"]} Assigned Patient")), Is.EqualTo(row["Assigned Patient"]));
        }

        // And coordinated notifications are sent to prevent conflicts
        var notificationCoordinationStatus = await GetText(page, "Notification Coordination Status");
        Assert.That(notificationCoordinationStatus, Does.Match(@"coordinated").IgnoreCase);

        // And remaining patients receive updated wait time estimates
        var waitTimeUpdateStatus = await GetText(page, "Wait Time Update Status");
        Assert.That(waitTimeUpdateStatus, Does.Match(@"updated").IgnoreCase);

        // And surge capacity protocols are activated if needed
        var surgeCapacityStatus = await GetText(page, "Surge Capacity Protocol Status");
        Assert.That(surgeCapacityStatus, Does.Match(@"activated|standby").IgnoreCase);
    }

    [Test, Order(5)]
    [Description("Provider assignment with workload balancing")]
    public async Task ProviderAssignmentWithWorkloadBalancing()
    {
        // Given provider workload tracking shows:
        //   | Provider Name | Patients Seen Today | Current Workload | Complexity Score |
        //   | Dr. Garcia    | 12                 | Light            | 85              |
        //   | Dr. Lee       | 18                 | Heavy            | 140             |
        //   | Dr. Foster    | 15                 | Moderate         | 110             |
        // And "Dr. Garcia" and "Dr. Foster" both become available
        // And the next patient is "Complex Patient" with multiple comorbidities
        // (assumed pre-seeded test data)
        // When the system determines provider assignment
        // (assumed to have already occurred / triggered by the system)

        // Then the system considers workload balancing:
        await WaitForTestId(page, "Current Workload Decision");
        var workloadBalancing = new List<Dictionary<string, string>>
        {
            Row(("Factor", "Current Workload"), ("Decision", "Favors Dr. Garcia")),
            Row(("Factor", "Complexity Fit"), ("Decision", "Both qualified")),
            Row(("Factor", "Fatigue Factor"), ("Decision", "Dr. Garcia preferred")),
        };
        foreach (var rowData in workloadBalancing)
        {
            var factor = rowData["Factor"];
            var decision = rowData["Decision"];
            Assert.That((await GetText(page, $"{factor} Decision")), Is.EqualTo(decision));
        }

        // And "Complex Patient" is assigned to "Dr. Garcia"
        Assert.That((await GetText(page, "Assigned Patient")), Is.EqualTo("Complex Patient"));
        Assert.That((await GetText(page, "Assigned Provider")), Is.EqualTo("Dr. Garcia"));

        // And workload metrics are updated for both providers
        var drGarciaWorkload = await WaitForTestId(page, "Dr. Garcia Workload");
        Assert.That((await drGarciaWorkload.IsVisibleAsync()), Is.True);
        var drFosterWorkload = await WaitForTestId(page, "Dr. Foster Workload");
        Assert.That((await drFosterWorkload.IsVisibleAsync()), Is.True);
    }

    [Test, Order(6)]
    [Description("Handle provider assignment with patient preferences")]
    public async Task HandleProviderAssignmentWithPatientPreferences()
    {
        // Given a patient "VIP Patient" has requested "Dr. Johnson" if available
        // And "Dr. Johnson" and "Dr. Smith" both become available
        // And "VIP Patient" is next in the queue with ESI Level 3
        // (assumed pre-seeded test data)
        // When the system processes provider assignment
        // (assumed to have already occurred / triggered by the system)

        // Then the system considers patient preferences:
        await WaitForTestId(page, "Patient Request");
        var preferenceFactors = new List<Dictionary<string, string>>
        {
            Row(("Factor", "Patient Request"), ("Details", "Specifically requested Dr. Johnson")),
            Row(("Factor", "Medical Appropriateness"), ("Details", "Both doctors qualified for ESI Level 3")),
            Row(("Factor", "Availability"), ("Details", "Dr. Johnson available and willing")),
        };
        foreach (var rowData in preferenceFactors)
        {
            var factor = rowData["Factor"];
            var details = rowData["Details"];
            Assert.That((await GetText(page, factor)), Is.EqualTo(details));
        }

        // And "VIP Patient" is assigned to "Dr. Johnson"
        Assert.That((await GetText(page, "Assigned Patient")), Is.EqualTo("VIP Patient"));
        Assert.That((await GetText(page, "Assigned Provider")), Is.EqualTo("Dr. Johnson"));

        // And "Dr. Smith" receives the next patient in queue
        var drSmithAssignedPatient = await GetText(page, "Dr. Smith Assigned Patient");
        Assert.That(drSmithAssignedPatient.Length > 0, Is.True);

        // And the assignment includes preference notation:
        var preferenceNotation = new List<Dictionary<string, string>>
        {
            Row(("Field", "Assignment Reason"), ("Content", "Patient preference request honored")),
            Row(("Field", "Special Notes"), ("Content", "VIP status - provide enhanced service")),
        };
        foreach (var rowData in preferenceNotation)
        {
            var field = rowData["Field"];
            var content = rowData["Content"];
            Assert.That((await GetText(page, field)), Is.EqualTo(content));
        }
    }

    [Test, Order(7)]
    [Description("Provider assignment failure and backup procedures")]
    public async Task ProviderAssignmentFailureAndBackupProcedures()
    {
        // Given "Dr. Williams" becomes available
        // And the highest priority patient is "Emergency Patient" (ESI Level 1)
        // (assumed pre-seeded test data)
        // When the system attempts to send assignment notification to Dr. Williams
        // And the mobile device notification fails to deliver
        // (assumed to have already occurred / triggered by the system)

        // Then the system activates backup notification procedures:
        await WaitForTestId(page, "Overhead Page");
        var backupProcedures = new List<Dictionary<string, string>>
        {
            Row(("Method", "Overhead Page"), ("Action", "\"Dr. Williams to ED-TRAUMA-1 immediately\"")),
            Row(("Method", "Desktop Alert"), ("Action", "Popup on all ED workstations")),
            Row(("Method", "Charge Nurse Alert"), ("Action", "Direct notification to charge nurse")),
            Row(("Method", "Secondary Provider"), ("Action", "Alert backup doctor if no response in 2 min")),
        };
        foreach (var rowData in backupProcedures)
        {
            var method = rowData["Method"];
            var action = rowData["Action"];
            Assert.That((await GetText(page, method)), Is.EqualTo(action));
        }

        // And the system logs the notification failure for IT review
        var notificationFailureLog = await GetText(page, "Notification Failure Log");
        Assert.That(notificationFailureLog, Does.Match(@"IT review").IgnoreCase);

        // And continues attempting mobile notification every 30 seconds
        var mobileRetryStatus = await GetText(page, "Mobile Notification Retry Status");
        Assert.That(mobileRetryStatus, Does.Match(@"30 seconds").IgnoreCase);

        // And tracks response time for quality metrics
        var responseTimeTrackingStatus = await GetText(page, "Response Time Tracking Status");
        Assert.That(responseTimeTrackingStatus, Does.Match(@"tracking|tracked").IgnoreCase);
    }

    [Test, Order(8)]
    [Description("Handle provider assignment during shift change")]
    public async Task HandleProviderAssignmentDuringShiftChange()
    {
        // Given it is 19:00 during evening shift change
        // And "Dr. Day" (day shift) is completing final patients
        // And "Dr. Night" (evening shift) is beginning shift
        // And a critical patient arrives requiring immediate attention
        // (assumed pre-seeded test data)
        // When the system determines provider assignment for the critical patient
        // (assumed to have already occurred / triggered by the system)

        // Then the system considers shift transition factors:
        await WaitForTestId(page, "Shift Status");
        var shiftTransitionFactors = new List<Dictionary<string, string>>
        {
            Row(("Factor", "Shift Status"), ("Consideration", "Dr. Day finishing, Dr. Night starting")),
            Row(("Factor", "Continuity"), ("Consideration", "Assign to Dr. Night for ongoing care")),
            Row(("Factor", "Availability"), ("Consideration", "Dr. Night has capacity for complex case")),
        };
        foreach (var rowData in shiftTransitionFactors)
        {
            var factor = rowData["Factor"];
            var consideration = rowData["Consideration"];
            Assert.That((await GetText(page, factor)), Is.EqualTo(consideration));
        }

        // And the critical patient is assigned to "Dr. Night"
        Assert.That((await GetText(page, "Assigned Provider")), Is.EqualTo("Dr. Night"));

        // And shift handoff information is included in the notification:
        var handoffInformation = new List<Dictionary<string, string>>
        {
            Row(("Component", "Shift Context"), ("Content", "New critical patient - evening shift start")),
            Row(("Component", "Day Shift Status"), ("Content", "Dr. Day finishing last 2 patients")),
            Row(("Component", "Support Available"), ("Content", "Day shift available for consultation")),
        };
        foreach (var rowData in handoffInformation)
        {
            var component = rowData["Component"];
            var content = rowData["Content"];
            Assert.That((await GetText(page, component)), Is.EqualTo(content));
        }
    }

    [Test, Order(9)]
    [Description("Track provider response times and assignment efficiency")]
    public async Task TrackProviderResponseTimesAndAssignmentEfficiency()
    {
        // Given provider assignment notifications are sent
        // (assumed pre-seeded test data)
        // When providers respond to patient assignments
        // (assumed to have already occurred / triggered by the system)

        // Then the system tracks performance metrics:
        await WaitForTestId(page, "Notification to Response");
        var performanceMetrics = new List<Dictionary<string, string>>
        {
            Row(("Metric", "Notification to Response"), ("Measurement", "Time from alert to bedside presence")),
            Row(("Metric", "Assignment Accuracy"), ("Measurement", "Correct provider-patient matching")),
            Row(("Metric", "Queue Optimization"), ("Measurement", "Wait time reduction effectiveness")),
        };
        foreach (var rowData in performanceMetrics)
        {
            var metric = rowData["Metric"];
            var measurement = rowData["Measurement"];
            Assert.That((await GetText(page, metric)), Is.EqualTo(measurement));
        }

        // And generates provider performance reports:
        var performanceReports = new List<Dictionary<string, string>>
        {
            Row(("Provider", "Dr. Johnson"), ("Avg Response Time", "3.2 minutes"), ("Assignment Accuracy", "98%"), ("Patient Satisfaction", "4.8/5")),
            Row(("Provider", "Dr. Smith"), ("Avg Response Time", "4.1 minutes"), ("Assignment Accuracy", "96%"), ("Patient Satisfaction", "4.6/5")),
        };
        foreach (var report in performanceReports)
        {
            Assert.That((await GetText(page, $"{report["Provider"]} Avg Response Time")), Is.EqualTo(report["Avg Response Time"]));
            Assert.That((await GetText(page, $"{report["Provider"]} Assignment Accuracy")), Is.EqualTo(report["Assignment Accuracy"]));
            Assert.That((await GetText(page, $"{report["Provider"]} Patient Satisfaction")), Is.EqualTo(report["Patient Satisfaction"]));
        }

        // And identifies optimization opportunities:
        var optimizationOpportunities = new List<Dictionary<string, string>>
        {
            Row(("Area", "Response Time"), ("Recommendation", "Target <3 minutes for critical patients")),
            Row(("Area", "Assignment Matching"), ("Recommendation", "Consider additional specialty training")),
            Row(("Area", "Communication"), ("Recommendation", "Implement two-way acknowledgment system")),
        };
        foreach (var rowData in optimizationOpportunities)
        {
            var area = rowData["Area"];
            var recommendation = rowData["Recommendation"];
            Assert.That((await GetText(page, area)), Is.EqualTo(recommendation));
        }
    }
}
