// Playwright + NUnit test for
// tests-with-given-when-then-features/18-performance-metrics.feature
// (equivalent to tests-with-playwright-javascript/18-performance-metrics.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see Support/Fields.cs) and the shared
// data-testid contract in Support/Login.cs (login-identity, login-submit,
// app-root).

namespace EmergencyCareSystem.PlaywrightTests;

[TestFixture]
[NonParallelizable]
public class T18PerformanceMetricsTests
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
        //   And I am logged in as "ED Manager Thompson"
        //   And the reporting module has access to historical data (assumed pre-seeded test data)
        //   And the performance metrics calculation engine is enabled (assumed pre-seeded test data)
        //   And patient satisfaction data is integrated from survey systems (assumed pre-seeded test data)
        await VerifySystemIsOperational(page);
        await Login(page, "ED Manager Thompson");

        var featureNavLink = await WaitForTestId(page, "Nav Performance Metrics");
        await featureNavLink.ClickAsync();
        await WaitForTestId(page, "Performance Metrics Panel");
    }

    [Test, Order(1)]
    [Description("Generate comprehensive monthly performance report")]
    public async Task GenerateComprehensiveMonthlyPerformanceReport()
    {
        // Given it is the first day of the new month (assumed pre-seeded test data)
        // And I need to review performance data for the previous month (May 2025) (assumed pre-seeded test data)
        // And the system has collected data from May 1-31, 2025 (assumed pre-seeded test data)
        // When I run the monthly performance report for May 2025
        await FillField(page, "Report Month", "May 2025");
        await page.GetByTestId("generate-monthly-report").First.ClickAsync();

        // Then the system generates comprehensive metrics including:
        await WaitForTestId(page, "Core Metric Category Report");
        var coreMetricCategoryRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Length of Stay"), ("value", "Average, median, 95th percentile by ESI")),
            Row(("label", "LWBS Rates"), ("value", "Left without being seen percentages")),
            Row(("label", "Patient Satisfaction"), ("value", "Overall scores and domain-specific ratings")),
            Row(("label", "Throughput Metrics"), ("value", "Door-to-provider, bed turnaround times")),
            Row(("label", "Quality Indicators"), ("value", "Safety events, readmission rates")),
        };
        foreach (var row in coreMetricCategoryRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And the average length of stay metrics show:
        var averageLosRows = new List<Dictionary<string, string>>
        {
            Row(("label", "ESI 1 (Critical)"), ("value", "180 minutes")),
            Row(("label", "ESI 2 (High Priority)"), ("value", "165 minutes")),
            Row(("label", "ESI 3 (Urgent)"), ("value", "145 minutes")),
            Row(("label", "ESI 4 (Less Urgent)"), ("value", "95 minutes")),
            Row(("label", "ESI 5 (Non-urgent)"), ("value", "75 minutes")),
            Row(("label", "Overall Average"), ("value", "132 minutes")),
        };
        foreach (var row in averageLosRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And left without being seen (LWBS) rates are calculated:
        var lwbsRateRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Overall LWBS Rate"), ("value", "3.2%")),
            Row(("label", "ESI 3 LWBS Rate"), ("value", "4.1%")),
            Row(("label", "ESI 4 LWBS Rate"), ("value", "5.8%")),
            Row(("label", "ESI 5 LWBS Rate"), ("value", "8.2%")),
            Row(("label", "Peak Hours LWBS"), ("value", "6.1%")),
        };
        foreach (var row in lwbsRateRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And patient satisfaction scores are displayed:
        var satisfactionScoreRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Overall Satisfaction"), ("value", "87.3%")),
            Row(("label", "Communication"), ("value", "89.1%")),
            Row(("label", "Pain Management"), ("value", "84.7%")),
            Row(("label", "Staff Responsiveness"), ("value", "88.9%")),
            Row(("label", "Cleanliness"), ("value", "92.4%")),
            Row(("label", "Discharge Process"), ("value", "86.2%")),
        };
        foreach (var row in satisfactionScoreRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }
    }

    [Test, Order(2)]
    [Description("Compare current month performance to historical benchmarks")]
    public async Task CompareCurrentMonthPerformanceToHistoricalBenchmarks()
    {
        // Given historical performance data is available for trending analysis
        // When I generate the monthly report with comparative analysis
        await page.GetByTestId("generate-comparative-report").First.ClickAsync();

        // Then the system displays month-over-month comparisons:
        await WaitForTestId(page, "Average Length of Stay Comparison");
        var monthOverMonthRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Average Length of Stay Comparison"), ("value", "132 min")),
            Row(("label", "LWBS Rate Comparison"), ("value", "3.2%")),
            Row(("label", "Patient Satisfaction Comparison"), ("value", "87.3%")),
            Row(("label", "Door-to-Provider Comparison"), ("value", "35 min")),
            Row(("label", "Bed Turnaround Comparison"), ("value", "28 min")),
        };
        foreach (var row in monthOverMonthRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And year-over-year comparisons are shown:
        var yearOverYearRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Average LOS YoY"), ("value", "132 min")),
            Row(("label", "LWBS Rate YoY"), ("value", "3.2%")),
            Row(("label", "Patient Volume YoY"), ("value", "2,847 pts")),
            Row(("label", "Satisfaction YoY"), ("value", "87.3%")),
        };
        foreach (var row in yearOverYearRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And statistical significance testing results are included:
        var statisticalSignificanceRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Length of Stay Significance"), ("value", "0.003")),
            Row(("label", "LWBS Rate Significance"), ("value", "0.012")),
            Row(("label", "Satisfaction Significance"), ("value", "0.001")),
        };
        foreach (var row in statisticalSignificanceRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }
    }

    [Test, Order(3)]
    [Description("Generate detailed LWBS analysis with root cause identification")]
    public async Task GenerateDetailedLWBSAnalysisWithRootCauseIdentification()
    {
        // Given LWBS events occurred throughout May 2025
        // When I request detailed LWBS analysis in the monthly report
        await page.GetByTestId("request-lwbs-analysis").First.ClickAsync();

        // Then the system provides comprehensive LWBS breakdown:
        await WaitForTestId(page, "Time-based Patterns");
        var lwbsBreakdownRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Time-based Patterns"), ("value", "LWBS rates by hour, day of week, shift")),
            Row(("label", "Acuity Distribution"), ("value", "LWBS percentage by ESI level")),
            Row(("label", "Wait Time Correlation"), ("value", "LWBS rates vs wait time thresholds")),
            Row(("label", "Seasonal Factors"), ("value", "Weather, holidays, local events impact")),
        };
        foreach (var row in lwbsBreakdownRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And root cause analysis is provided:
        var rootCauseRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Extended Wait Times"), ("value", "High")),
            Row(("label", "Staffing Shortages"), ("value", "High")),
            Row(("label", "Bed Availability"), ("value", "Medium")),
            Row(("label", "Triage Delays"), ("value", "Medium")),
        };
        foreach (var row in rootCauseRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And specific LWBS prevention recommendations are included:
        var preventionRecommendationRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Fast Track Protocol"), ("value", "High")),
            Row(("label", "Provider Scheduling"), ("value", "High")),
            Row(("label", "Patient Communication"), ("value", "Medium")),
            Row(("label", "Comfort Amenities"), ("value", "Low")),
        };
        foreach (var row in preventionRecommendationRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }
    }

    [Test, Order(4)]
    [Description("Analyze patient satisfaction trends with demographic breakdown")]
    public async Task AnalyzePatientSatisfactionTrendsWithDemographicBreakdown()
    {
        // Given patient satisfaction surveys were collected throughout May 2025
        // When I generate detailed satisfaction analysis
        await page.GetByTestId("generate-satisfaction-analysis").First.ClickAsync();

        // Then the system provides demographic-based satisfaction breakdown:
        await WaitForTestId(page, "Age 18-35");
        var demographicSatisfactionRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Age 18-35"), ("value", "85.1%")),
            Row(("label", "Age 36-55"), ("value", "88.7%")),
            Row(("label", "Age 56-75"), ("value", "89.2%")),
            Row(("label", "Age 75+"), ("value", "86.8%")),
            Row(("label", "Male Patients"), ("value", "86.9%")),
            Row(("label", "Female Patients"), ("value", "87.7%")),
        };
        foreach (var row in demographicSatisfactionRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And satisfaction domain analysis by patient type:
        var patientTypeOverallRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Trauma Patients Overall"), ("value", "82.0%")),
            Row(("label", "Chest Pain Overall"), ("value", "90.1%")),
            Row(("label", "Abdominal Pain Overall"), ("value", "84.4%")),
            Row(("label", "Minor Injuries Overall"), ("value", "89.6%")),
        };
        foreach (var row in patientTypeOverallRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And improvement opportunities are identified:
        var improvementOpportunityRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Trauma Communication"), ("value", "82.3%")),
            Row(("label", "Pain Management Improvement Opportunity"), ("value", "79.1%")),
            Row(("label", "Young Adult Experience"), ("value", "85.1%")),
        };
        foreach (var row in improvementOpportunityRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }
    }

    [Test, Order(5)]
    [Description("Generate quality and safety metrics with benchmarking")]
    public async Task GenerateQualityAndSafetyMetricsWithBenchmarking()
    {
        // Given quality and safety data is tracked throughout May 2025
        // When I include quality metrics in the monthly report
        await page.GetByTestId("include-quality-metrics").First.ClickAsync();

        // Then the system displays comprehensive quality indicators:
        await WaitForTestId(page, "Medication Errors");
        var qualityIndicatorRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Medication Errors"), ("value", "0.12%")),
            Row(("label", "Patient Falls"), ("value", "0 events")),
            Row(("label", "Hospital Readmissions"), ("value", "2.1%")),
            Row(("label", "Infection Control"), ("value", "99.8%")),
            Row(("label", "Adverse Events"), ("value", "3 events")),
        };
        foreach (var row in qualityIndicatorRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And safety event analysis is provided:
        var safetyEventRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Medication Error Count"), ("value", "2 events")),
            Row(("label", "Diagnostic Delay Count"), ("value", "1 event")),
            Row(("label", "Equipment Failure Count"), ("value", "0 events")),
        };
        foreach (var row in safetyEventRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And regulatory compliance status is shown:
        var complianceStatusRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Joint Commission"), ("value", "Compliant")),
            Row(("label", "CMS Core Measures"), ("value", "Compliant")),
            Row(("label", "State Regulations"), ("value", "Compliant")),
        };
        foreach (var row in complianceStatusRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }
    }

    [Test, Order(6)]
    [Description("Generate financial and operational efficiency metrics")]
    public async Task GenerateFinancialAndOperationalEfficiencyMetrics()
    {
        // Given financial and operational data is available for May 2025
        // When I request comprehensive operational metrics
        await page.GetByTestId("request-operational-metrics").First.ClickAsync();

        // Then the system displays efficiency indicators:
        await WaitForTestId(page, "Cost per Patient");
        var efficiencyIndicatorRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Cost per Patient"), ("value", "$847")),
            Row(("label", "Revenue per Patient"), ("value", "$1,245")),
            Row(("label", "Staff Productivity"), ("value", "2.8 pts/hr")),
            Row(("label", "Bed Utilization"), ("value", "87.3%")),
            Row(("label", "Equipment Uptime"), ("value", "98.7%")),
        };
        foreach (var row in efficiencyIndicatorRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And staffing metrics are included:
        var staffingMetricRows = new List<Dictionary<string, string>>
        {
            Row(("label", "RN Hours per Patient"), ("value", "4.2 hours")),
            Row(("label", "Physician Coverage"), ("value", "1:12 ratio")),
            Row(("label", "Overtime Hours"), ("value", "3.2%")),
            Row(("label", "Agency Staff Usage"), ("value", "1.8%")),
        };
        foreach (var row in staffingMetricRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And productivity analysis by shift is provided:
        var shiftProductivityRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Day (7a-7p)"), ("value", "3.2")),
            Row(("label", "Evening (7p-11p)"), ("value", "2.8")),
            Row(("label", "Night (11p-7a)"), ("value", "1.9")),
        };
        foreach (var row in shiftProductivityRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }
    }

    [Test, Order(7)]
    [Description("Generate executive summary with actionable recommendations")]
    public async Task GenerateExecutiveSummaryWithActionableRecommendations()
    {
        // Given all performance data has been analyzed for May 2025
        // When I request the executive summary section of the report
        await page.GetByTestId("request-executive-summary").First.ClickAsync();

        // Then the system provides a concise executive overview:
        await WaitForTestId(page, "Overall Performance");
        var executiveSummaryRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Overall Performance"), ("value", "87% of targets met, significant improvements")),
            Row(("label", "Major Achievements"), ("value", "LOS reduction, LWBS improvement, satisfaction up")),
            Row(("label", "Areas of Concern"), ("value", "Night shift efficiency, young adult satisfaction")),
            Row(("label", "Financial Impact"), ("value", "$2.1M revenue, $53 cost reduction per patient")),
        };
        foreach (var row in executiveSummaryRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And specific actionable recommendations are provided:
        var actionableRecommendationRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Implement fast-track for ESI 4-5"), ("value", "15% LOS reduction")),
            Row(("label", "Night shift staffing optimization"), ("value", "10% efficiency ↗")),
            Row(("label", "Young adult communication program"), ("value", "3% satisfaction ↗")),
            Row(("label", "Comfort amenity upgrades"), ("value", "2% satisfaction ↗")),
        };
        foreach (var row in actionableRecommendationRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And ROI analysis for recommendations is included:
        var roiAnalysisRows = new List<Dictionary<string, string>>
        {
            Row(("label", "$125K (Fast Track)"), ("value", "$340K")),
            Row(("label", "$85K (Staffing)"), ("value", "$220K")),
            Row(("label", "$45K (Communication)"), ("value", "$95K")),
        };
        foreach (var row in roiAnalysisRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And next month's focus areas are identified for tracking progress
        var focusAreas = page.GetByTestId("next-month-focus-area");
        Assert.That(await focusAreas.CountAsync() > 0, Is.True);
    }
}
