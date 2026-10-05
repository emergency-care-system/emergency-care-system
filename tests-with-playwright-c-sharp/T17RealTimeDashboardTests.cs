// Playwright + NUnit test for
// tests-with-given-when-then-features/17-real-time-dashboard.feature
// (equivalent to tests-with-playwright-javascript/17-real-time-dashboard.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see Support/Fields.cs) and the shared
// data-testid contract in Support/Login.cs (login-identity, login-submit,
// app-root).

namespace EmergencyCareSystem.PlaywrightTests;

[TestFixture]
[NonParallelizable]
public class T17RealTimeDashboardTests
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
        //   And I am logged in as "Charge Nurse Williams"
        //   And the real-time dashboard module is active (assumed pre-seeded test data)
        //   And automatic data refresh is enabled at 30-second intervals (assumed pre-seeded test data)
        //   And all data sources are connected and synchronized (assumed pre-seeded test data)
        await VerifySystemIsOperational(page);
        await Login(page, "Charge Nurse Williams");

        var featureNavLink = await WaitForTestId(page, "Nav Real-time Dashboard");
        await featureNavLink.ClickAsync();
        await WaitForTestId(page, "Real-time Dashboard Panel");
    }

    [Test, Order(1)]
    [Description("Display comprehensive department status on main dashboard")]
    public async Task DisplayComprehensiveDepartmentStatusOnMainDashboard()
    {
        // Given it is 14:30 on a busy Tuesday afternoon (assumed pre-seeded test data)
        // And the ED has the following current status: (assumed pre-seeded test data)
        // When I access the main dashboard
        await page.GetByTestId("main-dashboard-nav").First.ClickAsync();

        // Then the system displays the current patient census:
        await WaitForTestId(page, "Patient Census Display");
        var censusRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Total Patients"), ("value", "18 (90% capacity) - Yellow indicator")),
            Row(("label", "Admitted Patients"), ("value", "14 currently in beds")),
            Row(("label", "Waiting Patients"), ("value", "12 in triage queue - Red alert")),
            Row(("label", "Discharged Today"), ("value", "23 patients processed")),
        };
        foreach (var row in censusRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And the system shows average wait times:
        var waitTimeRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Triage to Bed"), ("value", "45 minutes")),
            Row(("label", "Bed to Provider"), ("value", "25 minutes")),
            Row(("label", "Provider to Discharge"), ("value", "120 minutes")),
            Row(("label", "Total ED Length of Stay"), ("value", "190 minutes")),
        };
        foreach (var row in waitTimeRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And bed availability is displayed with visual indicators:
        var bedStatusRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Available Clean"), ("value", "2")),
            Row(("label", "Needs Cleaning"), ("value", "2")),
            Row(("label", "Occupied"), ("value", "16")),
            Row(("label", "Out of Service"), ("value", "0")),
        };
        foreach (var row in bedStatusRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And staff assignments are shown in real-time:
        var staffAssignmentRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Dr. Smith"), ("value", "3 patients")),
            Row(("label", "Dr. Johnson"), ("value", "4 patients")),
            Row(("label", "Nurse Martinez"), ("value", "5 patients")),
            Row(("label", "Nurse Chen"), ("value", "4 patients")),
        };
        foreach (var row in staffAssignmentRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }
    }

    [Test, Order(2)]
    [Description("Monitor real-time updates with 30-second refresh cycle")]
    public async Task MonitorRealTimeUpdatesWith30SecondRefreshCycle()
    {
        // Given the dashboard is displaying current data at 15:00:00 (assumed pre-seeded test data)
        // And automatic refresh is set to 30-second intervals (assumed pre-seeded test data)
        // When 30 seconds elapse and new data becomes available: (assumed simulated by test fixture data)

        // Then the dashboard automatically updates at 15:00:30:
        var updatedMetricRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Patient Count"), ("value", "19 (95% capacity) - Red indicator")),
            Row(("label", "Bed Availability"), ("value", "1 available - Critical level alert")),
            Row(("label", "Wait Time Alert"), ("value", "Triage wait exceeds 45 min threshold")),
            Row(("label", "Staff Status"), ("value", "Dr. Smith now unavailable")),
        };
        foreach (var row in updatedMetricRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And visual indicators reflect the changes:
        var visualUpdateRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Capacity Indicator"), ("value", "Yellow → Red (approaching full capacity)")),
            Row(("label", "Bed Status Alert"), ("value", "New warning for low bed availability")),
            Row(("label", "Provider Icon"), ("value", "Dr. Smith icon changes to \"busy\" status")),
        };
        foreach (var row in visualUpdateRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And the last update timestamp shows "Updated: 15:00:30"
        var lastUpdateTimestamp = await GetText(page, "Last Update Timestamp");
        Assert.That(lastUpdateTimestamp, Is.EqualTo("Updated: 15:00:30"));

        // And critical alerts are highlighted with flashing indicators
        var criticalAlertIndicator = await WaitForTestId(page, "Critical Alert Indicator");
        Assert.That((await criticalAlertIndicator.IsVisibleAsync()), Is.True);
    }

    [Test, Order(3)]
    [Description("Display priority alerts and notifications on dashboard")]
    public async Task DisplayPriorityAlertsAndNotificationsOnDashboard()
    {
        // Given the dashboard is actively monitoring department status
        // When critical conditions occur requiring immediate attention: (assumed simulated by test fixture data)

        // Then the dashboard displays prominent alert notifications:
        var alertPriorityRows = new List<Dictionary<string, string>>
        {
            Row(("label", "CRITICAL"), ("value", "🚨 Red flashing banner at top of screen")),
            Row(("label", "HIGH"), ("value", "🟠 Orange banner with urgent icon")),
            Row(("label", "MEDIUM"), ("value", "🟡 Yellow notification strip")),
            Row(("label", "INFO"), ("value", "🔵 Blue informational indicator")),
        };
        foreach (var row in alertPriorityRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And specific alert details are shown:
        var alertDetailRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Capacity Critical"), ("value", "ED at 95% capacity - Consider diversion")),
            Row(("label", "Wait Time Excessive"), ("value", "Triage wait: 65 min - Expedite process")),
            Row(("label", "ESI 1 Patient"), ("value", "Critical patient waiting - Bed needed")),
            Row(("label", "Staffing Concern"), ("value", "Provider ratio 1:5 - Additional coverage needed")),
        };
        foreach (var row in alertDetailRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And alert acknowledgment is required for critical notifications
        var alertAcknowledgment = await WaitForTestId(page, "Alert Acknowledgment Required");
        Assert.That((await alertAcknowledgment.IsVisibleAsync()), Is.True);

        // And alert history is maintained for trend analysis
        var alertHistoryEntries = page.GetByTestId("alert-history-entry");
        Assert.That(await alertHistoryEntries.CountAsync() > 0, Is.True);
    }

    [Test, Order(4)]
    [Description("Monitor specific patient flow metrics and trends")]
    public async Task MonitorSpecificPatientFlowMetricsAndTrends()
    {
        // Given I need to track detailed patient flow performance
        // When I access the detailed metrics section of the dashboard
        await page.GetByTestId("detailed-metrics-nav").First.ClickAsync();

        // Then the system displays comprehensive flow metrics:
        var flowMetricRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Patients per Hour"), ("value", "3.2 arrivals")),
            Row(("label", "Discharge Rate"), ("value", "2.1 per hour")),
            Row(("label", "Bed Turnover Time"), ("value", "35 minutes")),
            Row(("label", "Left Without Being Seen"), ("value", "2 patients")),
        };
        foreach (var row in flowMetricRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And trending graphs show patterns over time:
        var trendGraphRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Census Trend"), ("value", "24 hours")),
            Row(("label", "Wait Time Trend"), ("value", "12 hours")),
            Row(("label", "Capacity Utilization"), ("value", "7 days")),
            Row(("label", "Provider Productivity"), ("value", "Shift")),
        };
        foreach (var row in trendGraphRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And predictive indicators show projected status:
        var predictiveIndicatorRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Peak Time Prediction"), ("value", "Expected surge at 18:00-20:00")),
            Row(("label", "Capacity Projection"), ("value", "Will reach 100% capacity in 90 minutes")),
            Row(("label", "Staffing Needs"), ("value", "Additional provider needed by 16:00")),
        };
        foreach (var row in predictiveIndicatorRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }
    }

    [Test, Order(5)]
    [Description("Customize dashboard view based on role and preferences")]
    public async Task CustomizeDashboardViewBasedOnRoleAndPreferences()
    {
        // Given I have charge nurse privileges and preferences set
        // When I access my personalized dashboard view
        await page.GetByTestId("personalized-dashboard-nav").First.ClickAsync();

        // Then the system displays role-specific information:
        var dashboardSectionRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Resource Management"), ("value", "Bed status, staffing levels, equipment")),
            Row(("label", "Quality Metrics"), ("value", "Wait times, satisfaction scores, safety")),
            Row(("label", "Operational Alerts"), ("value", "Capacity issues, workflow bottlenecks")),
            Row(("label", "Staff Coordination"), ("value", "Break schedules, assignments, coverage")),
        };
        foreach (var row in dashboardSectionRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And customizable widgets show preferred metrics:
        var widgetConfigurationRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Bed Management Grid"), ("value", "Color-coded bed status with room numbers")),
            Row(("label", "Provider Status Board"), ("value", "Real-time availability and patient load")),
            Row(("label", "Queue Management"), ("value", "ESI-sorted patient list with wait times")),
            Row(("label", "Performance KPIs"), ("value", "Key metrics with targets and trends")),
        };
        foreach (var row in widgetConfigurationRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And I can modify the layout and priority of information displayed
        var layoutEditor = await WaitForTestId(page, "Layout Editor");
        Assert.That((await layoutEditor.IsVisibleAsync()), Is.True);

        // And the system saves my preferences for future sessions
        var preferencesSavedStatus = await GetText(page, "Preferences Saved Status");
        Assert.That(preferencesSavedStatus, Does.Match(@"saved").IgnoreCase);
    }

    [Test, Order(6)]
    [Description("Display emergency and crisis mode indicators")]
    public async Task DisplayEmergencyAndCrisisModeIndicators()
    {
        // Given the ED is operating under normal conditions
        // When emergency conditions trigger crisis mode: (assumed simulated by test fixture data)

        // Then the dashboard displays crisis mode indicators:
        var crisisDisplayRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Mode Banner"), ("value", "Red \"CRISIS MODE ACTIVE\" across top")),
            Row(("label", "Protocol Status"), ("value", "Active emergency protocols listed")),
            Row(("label", "Resource Allocation"), ("value", "Special staffing and bed assignments")),
            Row(("label", "Communication Center"), ("value", "Emergency contact information prominent")),
        };
        foreach (var row in crisisDisplayRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And specialized crisis metrics are shown:
        var crisisMetricRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Response Teams"), ("value", "Available emergency response personnel")),
            Row(("label", "Special Equipment"), ("value", "Crisis supplies and equipment status")),
            Row(("label", "External Coordination"), ("value", "Communication with EMS, other hospitals")),
            Row(("label", "Surge Capacity"), ("value", "Additional beds and overflow areas")),
        };
        foreach (var row in crisisMetricRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And normal operations metrics are supplemented with crisis-specific data
        var crisisSpecificData = await WaitForTestId(page, "Crisis Specific Data");
        Assert.That((await crisisSpecificData.IsVisibleAsync()), Is.True);
    }

    [Test, Order(7)]
    [Description("Integrate with mobile devices for dashboard access")]
    public async Task IntegrateWithMobileDevicesForDashboardAccess()
    {
        // Given I need to monitor the dashboard while mobile in the department
        // When I access the dashboard on my mobile device
        await Login(page, "Charge Nurse Williams", mobile: true);

        // This is a second, mid-scenario login (the desktop login from
        // beforeEach already happened), so the dashboard reloads back to its
        // Overview screen -- reselect the panel before checking its content.
        var mobileNavLink = await WaitForTestId(page, "Nav Real-time Dashboard");
        await mobileNavLink.ClickAsync();
        await WaitForTestId(page, "Real-time Dashboard Panel");

        // Then the system provides a mobile-optimized view:
        var mobileFeatureRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Summary Cards"), ("value", "Key metrics in swipeable card format")),
            Row(("label", "Alert Notifications"), ("value", "Push notifications for critical alerts")),
            Row(("label", "Quick Actions"), ("value", "Rapid access to common charge nurse tasks")),
            Row(("label", "Touch Interface"), ("value", "Optimized for touch navigation")),
        };
        foreach (var row in mobileFeatureRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And essential information is prioritized for small screen:
        var priorityDisplayRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Critical Alerts"), ("value", "Top of screen with prominent notification")),
            Row(("label", "Bed Status Summary"), ("value", "Visual grid with color-coded indicators")),
            Row(("label", "Staff Availability"), ("value", "Provider status with quick contact options")),
            Row(("label", "Key Metrics"), ("value", "Current census, wait times, alerts")),
        };
        foreach (var row in priorityDisplayRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And synchronization occurs in real-time between desktop and mobile views
        var syncStatus = await GetText(page, "Sync Status");
        Assert.That(syncStatus, Does.Match(@"real-time").IgnoreCase);

        // And offline capability maintains last-known status when connectivity is lost
        var offlineStatusIndicator = await WaitForTestId(page, "Offline Status Indicator");
        Assert.That((await offlineStatusIndicator.IsVisibleAsync()), Is.True);
    }

    [Test, Order(8)]
    [Description("Historical data comparison and trend analysis")]
    public async Task HistoricalDataComparisonAndTrendAnalysis()
    {
        // Given I want to compare current performance to historical patterns
        // When I access the trend analysis section of the dashboard
        await page.GetByTestId("trend-analysis-nav").First.ClickAsync();

        // Then the system displays comparative data:
        var comparisonTypeRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Same Day Last Week"), ("value", "Tuesday to Tuesday comparison")),
            Row(("label", "Monthly Average"), ("value", "Current day vs monthly average")),
            Row(("label", "Seasonal Patterns"), ("value", "Year-over-year seasonal comparison")),
            Row(("label", "Shift Comparisons"), ("value", "Day vs evening vs night shift metrics")),
        };
        foreach (var row in comparisonTypeRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And performance benchmarks are shown:
        var benchmarkRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Internal Targets"), ("value", "Hospital-specific performance goals")),
            Row(("label", "Industry Standards"), ("value", "National ED performance benchmarks")),
            Row(("label", "Peer Comparison"), ("value", "Similar-sized ED performance data")),
        };
        foreach (var row in benchmarkRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And variance analysis highlights significant deviations:
        var varianceAlertRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Significant Increase"), ("value", ">20% above normal pattern")),
            Row(("label", "Significant Decrease"), ("value", ">15% below expected performance")),
            Row(("label", "Unusual Pattern"), ("value", "Unexpected trends or anomalies")),
        };
        foreach (var row in varianceAlertRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And recommendations are provided for performance improvement
        var recommendations = page.GetByTestId("performance-recommendation");
        Assert.That(await recommendations.CountAsync() > 0, Is.True);
    }
}
