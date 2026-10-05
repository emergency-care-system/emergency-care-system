// Playwright + JUnit 5 test for
// tests-with-given-when-then-features/18-performance-metrics.feature
// (equivalent to tests-with-playwright-javascript/18-performance-metrics.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see support/Fields.java) and the shared
// data-testid contract in support/Login.java (login-identity, login-submit,
// app-root).

package emergencycaresystem;

import static emergencycaresystem.support.Config.BASE_URL;
import static emergencycaresystem.support.Fields.*;
import static emergencycaresystem.support.Login.*;
import static emergencycaresystem.support.Matchers.assertMatches;
import static org.junit.jupiter.api.Assertions.*;

import java.util.List;
import java.util.Map;
import com.microsoft.playwright.Page;
import emergencycaresystem.support.Session;
import org.junit.jupiter.api.AfterAll;
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.MethodOrderer;
import org.junit.jupiter.api.Order;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.TestMethodOrder;

@TestMethodOrder(MethodOrderer.OrderAnnotation.class)
class T18PerformanceMetricsTest {
    private static Session session;
    private static Page page;

    @BeforeAll
    static void setUpClass() {
        session = Session.start();
        page = session.page();
    }

    @AfterAll
    static void tearDownClass() {
        session.close();
    }

    @BeforeEach
    void setUp() {
        // Background:
        //   Given the emergency care system is operational
        //   And I am logged in as "ED Manager Thompson"
        //   And the reporting module has access to historical data (assumed pre-seeded test data)
        //   And the performance metrics calculation engine is enabled (assumed pre-seeded test data)
        //   And patient satisfaction data is integrated from survey systems (assumed pre-seeded test data)
        verifySystemIsOperational(page);
        login(page, "ED Manager Thompson");

        var featureNavLink = waitForTestId(page, "Nav Performance Metrics");
        featureNavLink.click();
        waitForTestId(page, "Performance Metrics Panel");
    }

    @Test
    @Order(1)
    @DisplayName("Generate comprehensive monthly performance report")
    void generateComprehensiveMonthlyPerformanceReport() {
        // Given it is the first day of the new month (assumed pre-seeded test data)
        // And I need to review performance data for the previous month (May 2025) (assumed pre-seeded test data)
        // And the system has collected data from May 1-31, 2025 (assumed pre-seeded test data)
        // When I run the monthly performance report for May 2025
        fillField(page, "Report Month", "May 2025");
        page.getByTestId("generate-monthly-report").first().click();

        // Then the system generates comprehensive metrics including:
        waitForTestId(page, "Core Metric Category Report");
        List<Map<String, String>> coreMetricCategoryRows = List.of(
            Map.of("label", "Length of Stay", "value", "Average, median, 95th percentile by ESI"),
            Map.of("label", "LWBS Rates", "value", "Left without being seen percentages"),
            Map.of("label", "Patient Satisfaction", "value", "Overall scores and domain-specific ratings"),
            Map.of("label", "Throughput Metrics", "value", "Door-to-provider, bed turnaround times"),
            Map.of("label", "Quality Indicators", "value", "Safety events, readmission rates")
        );
        for (var row : coreMetricCategoryRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }

        // And the average length of stay metrics show:
        List<Map<String, String>> averageLosRows = List.of(
            Map.of("label", "ESI 1 (Critical)", "value", "180 minutes"),
            Map.of("label", "ESI 2 (High Priority)", "value", "165 minutes"),
            Map.of("label", "ESI 3 (Urgent)", "value", "145 minutes"),
            Map.of("label", "ESI 4 (Less Urgent)", "value", "95 minutes"),
            Map.of("label", "ESI 5 (Non-urgent)", "value", "75 minutes"),
            Map.of("label", "Overall Average", "value", "132 minutes")
        );
        for (var row : averageLosRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }

        // And left without being seen (LWBS) rates are calculated:
        List<Map<String, String>> lwbsRateRows = List.of(
            Map.of("label", "Overall LWBS Rate", "value", "3.2%"),
            Map.of("label", "ESI 3 LWBS Rate", "value", "4.1%"),
            Map.of("label", "ESI 4 LWBS Rate", "value", "5.8%"),
            Map.of("label", "ESI 5 LWBS Rate", "value", "8.2%"),
            Map.of("label", "Peak Hours LWBS", "value", "6.1%")
        );
        for (var row : lwbsRateRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }

        // And patient satisfaction scores are displayed:
        List<Map<String, String>> satisfactionScoreRows = List.of(
            Map.of("label", "Overall Satisfaction", "value", "87.3%"),
            Map.of("label", "Communication", "value", "89.1%"),
            Map.of("label", "Pain Management", "value", "84.7%"),
            Map.of("label", "Staff Responsiveness", "value", "88.9%"),
            Map.of("label", "Cleanliness", "value", "92.4%"),
            Map.of("label", "Discharge Process", "value", "86.2%")
        );
        for (var row : satisfactionScoreRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }
    }

    @Test
    @Order(2)
    @DisplayName("Compare current month performance to historical benchmarks")
    void compareCurrentMonthPerformanceToHistoricalBenchmarks() {
        // Given historical performance data is available for trending analysis
        // When I generate the monthly report with comparative analysis
        page.getByTestId("generate-comparative-report").first().click();

        // Then the system displays month-over-month comparisons:
        waitForTestId(page, "Average Length of Stay Comparison");
        List<Map<String, String>> monthOverMonthRows = List.of(
            Map.of("label", "Average Length of Stay Comparison", "value", "132 min"),
            Map.of("label", "LWBS Rate Comparison", "value", "3.2%"),
            Map.of("label", "Patient Satisfaction Comparison", "value", "87.3%"),
            Map.of("label", "Door-to-Provider Comparison", "value", "35 min"),
            Map.of("label", "Bed Turnaround Comparison", "value", "28 min")
        );
        for (var row : monthOverMonthRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }

        // And year-over-year comparisons are shown:
        List<Map<String, String>> yearOverYearRows = List.of(
            Map.of("label", "Average LOS YoY", "value", "132 min"),
            Map.of("label", "LWBS Rate YoY", "value", "3.2%"),
            Map.of("label", "Patient Volume YoY", "value", "2,847 pts"),
            Map.of("label", "Satisfaction YoY", "value", "87.3%")
        );
        for (var row : yearOverYearRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }

        // And statistical significance testing results are included:
        List<Map<String, String>> statisticalSignificanceRows = List.of(
            Map.of("label", "Length of Stay Significance", "value", "0.003"),
            Map.of("label", "LWBS Rate Significance", "value", "0.012"),
            Map.of("label", "Satisfaction Significance", "value", "0.001")
        );
        for (var row : statisticalSignificanceRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }
    }

    @Test
    @Order(3)
    @DisplayName("Generate detailed LWBS analysis with root cause identification")
    void generateDetailedLWBSAnalysisWithRootCauseIdentification() {
        // Given LWBS events occurred throughout May 2025
        // When I request detailed LWBS analysis in the monthly report
        page.getByTestId("request-lwbs-analysis").first().click();

        // Then the system provides comprehensive LWBS breakdown:
        waitForTestId(page, "Time-based Patterns");
        List<Map<String, String>> lwbsBreakdownRows = List.of(
            Map.of("label", "Time-based Patterns", "value", "LWBS rates by hour, day of week, shift"),
            Map.of("label", "Acuity Distribution", "value", "LWBS percentage by ESI level"),
            Map.of("label", "Wait Time Correlation", "value", "LWBS rates vs wait time thresholds"),
            Map.of("label", "Seasonal Factors", "value", "Weather, holidays, local events impact")
        );
        for (var row : lwbsBreakdownRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }

        // And root cause analysis is provided:
        List<Map<String, String>> rootCauseRows = List.of(
            Map.of("label", "Extended Wait Times", "value", "High"),
            Map.of("label", "Staffing Shortages", "value", "High"),
            Map.of("label", "Bed Availability", "value", "Medium"),
            Map.of("label", "Triage Delays", "value", "Medium")
        );
        for (var row : rootCauseRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }

        // And specific LWBS prevention recommendations are included:
        List<Map<String, String>> preventionRecommendationRows = List.of(
            Map.of("label", "Fast Track Protocol", "value", "High"),
            Map.of("label", "Provider Scheduling", "value", "High"),
            Map.of("label", "Patient Communication", "value", "Medium"),
            Map.of("label", "Comfort Amenities", "value", "Low")
        );
        for (var row : preventionRecommendationRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }
    }

    @Test
    @Order(4)
    @DisplayName("Analyze patient satisfaction trends with demographic breakdown")
    void analyzePatientSatisfactionTrendsWithDemographicBreakdown() {
        // Given patient satisfaction surveys were collected throughout May 2025
        // When I generate detailed satisfaction analysis
        page.getByTestId("generate-satisfaction-analysis").first().click();

        // Then the system provides demographic-based satisfaction breakdown:
        waitForTestId(page, "Age 18-35");
        List<Map<String, String>> demographicSatisfactionRows = List.of(
            Map.of("label", "Age 18-35", "value", "85.1%"),
            Map.of("label", "Age 36-55", "value", "88.7%"),
            Map.of("label", "Age 56-75", "value", "89.2%"),
            Map.of("label", "Age 75+", "value", "86.8%"),
            Map.of("label", "Male Patients", "value", "86.9%"),
            Map.of("label", "Female Patients", "value", "87.7%")
        );
        for (var row : demographicSatisfactionRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }

        // And satisfaction domain analysis by patient type:
        List<Map<String, String>> patientTypeOverallRows = List.of(
            Map.of("label", "Trauma Patients Overall", "value", "82.0%"),
            Map.of("label", "Chest Pain Overall", "value", "90.1%"),
            Map.of("label", "Abdominal Pain Overall", "value", "84.4%"),
            Map.of("label", "Minor Injuries Overall", "value", "89.6%")
        );
        for (var row : patientTypeOverallRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }

        // And improvement opportunities are identified:
        List<Map<String, String>> improvementOpportunityRows = List.of(
            Map.of("label", "Trauma Communication", "value", "82.3%"),
            Map.of("label", "Pain Management Improvement Opportunity", "value", "79.1%"),
            Map.of("label", "Young Adult Experience", "value", "85.1%")
        );
        for (var row : improvementOpportunityRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }
    }

    @Test
    @Order(5)
    @DisplayName("Generate quality and safety metrics with benchmarking")
    void generateQualityAndSafetyMetricsWithBenchmarking() {
        // Given quality and safety data is tracked throughout May 2025
        // When I include quality metrics in the monthly report
        page.getByTestId("include-quality-metrics").first().click();

        // Then the system displays comprehensive quality indicators:
        waitForTestId(page, "Medication Errors");
        List<Map<String, String>> qualityIndicatorRows = List.of(
            Map.of("label", "Medication Errors", "value", "0.12%"),
            Map.of("label", "Patient Falls", "value", "0 events"),
            Map.of("label", "Hospital Readmissions", "value", "2.1%"),
            Map.of("label", "Infection Control", "value", "99.8%"),
            Map.of("label", "Adverse Events", "value", "3 events")
        );
        for (var row : qualityIndicatorRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }

        // And safety event analysis is provided:
        List<Map<String, String>> safetyEventRows = List.of(
            Map.of("label", "Medication Error Count", "value", "2 events"),
            Map.of("label", "Diagnostic Delay Count", "value", "1 event"),
            Map.of("label", "Equipment Failure Count", "value", "0 events")
        );
        for (var row : safetyEventRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }

        // And regulatory compliance status is shown:
        List<Map<String, String>> complianceStatusRows = List.of(
            Map.of("label", "Joint Commission", "value", "Compliant"),
            Map.of("label", "CMS Core Measures", "value", "Compliant"),
            Map.of("label", "State Regulations", "value", "Compliant")
        );
        for (var row : complianceStatusRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }
    }

    @Test
    @Order(6)
    @DisplayName("Generate financial and operational efficiency metrics")
    void generateFinancialAndOperationalEfficiencyMetrics() {
        // Given financial and operational data is available for May 2025
        // When I request comprehensive operational metrics
        page.getByTestId("request-operational-metrics").first().click();

        // Then the system displays efficiency indicators:
        waitForTestId(page, "Cost per Patient");
        List<Map<String, String>> efficiencyIndicatorRows = List.of(
            Map.of("label", "Cost per Patient", "value", "$847"),
            Map.of("label", "Revenue per Patient", "value", "$1,245"),
            Map.of("label", "Staff Productivity", "value", "2.8 pts/hr"),
            Map.of("label", "Bed Utilization", "value", "87.3%"),
            Map.of("label", "Equipment Uptime", "value", "98.7%")
        );
        for (var row : efficiencyIndicatorRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }

        // And staffing metrics are included:
        List<Map<String, String>> staffingMetricRows = List.of(
            Map.of("label", "RN Hours per Patient", "value", "4.2 hours"),
            Map.of("label", "Physician Coverage", "value", "1:12 ratio"),
            Map.of("label", "Overtime Hours", "value", "3.2%"),
            Map.of("label", "Agency Staff Usage", "value", "1.8%")
        );
        for (var row : staffingMetricRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }

        // And productivity analysis by shift is provided:
        List<Map<String, String>> shiftProductivityRows = List.of(
            Map.of("label", "Day (7a-7p)", "value", "3.2"),
            Map.of("label", "Evening (7p-11p)", "value", "2.8"),
            Map.of("label", "Night (11p-7a)", "value", "1.9")
        );
        for (var row : shiftProductivityRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }
    }

    @Test
    @Order(7)
    @DisplayName("Generate executive summary with actionable recommendations")
    void generateExecutiveSummaryWithActionableRecommendations() {
        // Given all performance data has been analyzed for May 2025
        // When I request the executive summary section of the report
        page.getByTestId("request-executive-summary").first().click();

        // Then the system provides a concise executive overview:
        waitForTestId(page, "Overall Performance");
        List<Map<String, String>> executiveSummaryRows = List.of(
            Map.of("label", "Overall Performance", "value", "87% of targets met, significant improvements"),
            Map.of("label", "Major Achievements", "value", "LOS reduction, LWBS improvement, satisfaction up"),
            Map.of("label", "Areas of Concern", "value", "Night shift efficiency, young adult satisfaction"),
            Map.of("label", "Financial Impact", "value", "$2.1M revenue, $53 cost reduction per patient")
        );
        for (var row : executiveSummaryRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }

        // And specific actionable recommendations are provided:
        List<Map<String, String>> actionableRecommendationRows = List.of(
            Map.of("label", "Implement fast-track for ESI 4-5", "value", "15% LOS reduction"),
            Map.of("label", "Night shift staffing optimization", "value", "10% efficiency ↗"),
            Map.of("label", "Young adult communication program", "value", "3% satisfaction ↗"),
            Map.of("label", "Comfort amenity upgrades", "value", "2% satisfaction ↗")
        );
        for (var row : actionableRecommendationRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }

        // And ROI analysis for recommendations is included:
        List<Map<String, String>> roiAnalysisRows = List.of(
            Map.of("label", "$125K (Fast Track)", "value", "$340K"),
            Map.of("label", "$85K (Staffing)", "value", "$220K"),
            Map.of("label", "$45K (Communication)", "value", "$95K")
        );
        for (var row : roiAnalysisRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }

        // And next month's focus areas are identified for tracking progress
        var focusAreas = page.getByTestId("next-month-focus-area");
        assertTrue(focusAreas.count() > 0);
    }
}
