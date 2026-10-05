// Playwright + JUnit 5 test for
// tests-with-given-when-then-features/17-real-time-dashboard.feature
// (equivalent to tests-with-playwright-javascript/17-real-time-dashboard.test.js).
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
class T17RealTimeDashboardTest {
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
        //   And I am logged in as "Charge Nurse Williams"
        //   And the real-time dashboard module is active (assumed pre-seeded test data)
        //   And automatic data refresh is enabled at 30-second intervals (assumed pre-seeded test data)
        //   And all data sources are connected and synchronized (assumed pre-seeded test data)
        verifySystemIsOperational(page);
        login(page, "Charge Nurse Williams");

        var featureNavLink = waitForTestId(page, "Nav Real-time Dashboard");
        featureNavLink.click();
        waitForTestId(page, "Real-time Dashboard Panel");
    }

    @Test
    @Order(1)
    @DisplayName("Display comprehensive department status on main dashboard")
    void displayComprehensiveDepartmentStatusOnMainDashboard() {
        // Given it is 14:30 on a busy Tuesday afternoon (assumed pre-seeded test data)
        // And the ED has the following current status: (assumed pre-seeded test data)
        // When I access the main dashboard
        page.getByTestId("main-dashboard-nav").first().click();

        // Then the system displays the current patient census:
        waitForTestId(page, "Patient Census Display");
        List<Map<String, String>> censusRows = List.of(
            Map.of("label", "Total Patients", "value", "18 (90% capacity) - Yellow indicator"),
            Map.of("label", "Admitted Patients", "value", "14 currently in beds"),
            Map.of("label", "Waiting Patients", "value", "12 in triage queue - Red alert"),
            Map.of("label", "Discharged Today", "value", "23 patients processed")
        );
        for (var row : censusRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }

        // And the system shows average wait times:
        List<Map<String, String>> waitTimeRows = List.of(
            Map.of("label", "Triage to Bed", "value", "45 minutes"),
            Map.of("label", "Bed to Provider", "value", "25 minutes"),
            Map.of("label", "Provider to Discharge", "value", "120 minutes"),
            Map.of("label", "Total ED Length of Stay", "value", "190 minutes")
        );
        for (var row : waitTimeRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }

        // And bed availability is displayed with visual indicators:
        List<Map<String, String>> bedStatusRows = List.of(
            Map.of("label", "Available Clean", "value", "2"),
            Map.of("label", "Needs Cleaning", "value", "2"),
            Map.of("label", "Occupied", "value", "16"),
            Map.of("label", "Out of Service", "value", "0")
        );
        for (var row : bedStatusRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }

        // And staff assignments are shown in real-time:
        List<Map<String, String>> staffAssignmentRows = List.of(
            Map.of("label", "Dr. Smith", "value", "3 patients"),
            Map.of("label", "Dr. Johnson", "value", "4 patients"),
            Map.of("label", "Nurse Martinez", "value", "5 patients"),
            Map.of("label", "Nurse Chen", "value", "4 patients")
        );
        for (var row : staffAssignmentRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }
    }

    @Test
    @Order(2)
    @DisplayName("Monitor real-time updates with 30-second refresh cycle")
    void monitorRealTimeUpdatesWith30SecondRefreshCycle() {
        // Given the dashboard is displaying current data at 15:00:00 (assumed pre-seeded test data)
        // And automatic refresh is set to 30-second intervals (assumed pre-seeded test data)
        // When 30 seconds elapse and new data becomes available: (assumed simulated by test fixture data)

        // Then the dashboard automatically updates at 15:00:30:
        List<Map<String, String>> updatedMetricRows = List.of(
            Map.of("label", "Patient Count", "value", "19 (95% capacity) - Red indicator"),
            Map.of("label", "Bed Availability", "value", "1 available - Critical level alert"),
            Map.of("label", "Wait Time Alert", "value", "Triage wait exceeds 45 min threshold"),
            Map.of("label", "Staff Status", "value", "Dr. Smith now unavailable")
        );
        for (var row : updatedMetricRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }

        // And visual indicators reflect the changes:
        List<Map<String, String>> visualUpdateRows = List.of(
            Map.of("label", "Capacity Indicator", "value", "Yellow → Red (approaching full capacity)"),
            Map.of("label", "Bed Status Alert", "value", "New warning for low bed availability"),
            Map.of("label", "Provider Icon", "value", "Dr. Smith icon changes to \"busy\" status")
        );
        for (var row : visualUpdateRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }

        // And the last update timestamp shows "Updated: 15:00:30"
        var lastUpdateTimestamp = getText(page, "Last Update Timestamp");
        assertEquals("Updated: 15:00:30", lastUpdateTimestamp);

        // And critical alerts are highlighted with flashing indicators
        var criticalAlertIndicator = waitForTestId(page, "Critical Alert Indicator");
        assertTrue(criticalAlertIndicator.isVisible());
    }

    @Test
    @Order(3)
    @DisplayName("Display priority alerts and notifications on dashboard")
    void displayPriorityAlertsAndNotificationsOnDashboard() {
        // Given the dashboard is actively monitoring department status
        // When critical conditions occur requiring immediate attention: (assumed simulated by test fixture data)

        // Then the dashboard displays prominent alert notifications:
        List<Map<String, String>> alertPriorityRows = List.of(
            Map.of("label", "CRITICAL", "value", "🚨 Red flashing banner at top of screen"),
            Map.of("label", "HIGH", "value", "🟠 Orange banner with urgent icon"),
            Map.of("label", "MEDIUM", "value", "🟡 Yellow notification strip"),
            Map.of("label", "INFO", "value", "🔵 Blue informational indicator")
        );
        for (var row : alertPriorityRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }

        // And specific alert details are shown:
        List<Map<String, String>> alertDetailRows = List.of(
            Map.of("label", "Capacity Critical", "value", "ED at 95% capacity - Consider diversion"),
            Map.of("label", "Wait Time Excessive", "value", "Triage wait: 65 min - Expedite process"),
            Map.of("label", "ESI 1 Patient", "value", "Critical patient waiting - Bed needed"),
            Map.of("label", "Staffing Concern", "value", "Provider ratio 1:5 - Additional coverage needed")
        );
        for (var row : alertDetailRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }

        // And alert acknowledgment is required for critical notifications
        var alertAcknowledgment = waitForTestId(page, "Alert Acknowledgment Required");
        assertTrue(alertAcknowledgment.isVisible());

        // And alert history is maintained for trend analysis
        var alertHistoryEntries = page.getByTestId("alert-history-entry");
        assertTrue(alertHistoryEntries.count() > 0);
    }

    @Test
    @Order(4)
    @DisplayName("Monitor specific patient flow metrics and trends")
    void monitorSpecificPatientFlowMetricsAndTrends() {
        // Given I need to track detailed patient flow performance
        // When I access the detailed metrics section of the dashboard
        page.getByTestId("detailed-metrics-nav").first().click();

        // Then the system displays comprehensive flow metrics:
        List<Map<String, String>> flowMetricRows = List.of(
            Map.of("label", "Patients per Hour", "value", "3.2 arrivals"),
            Map.of("label", "Discharge Rate", "value", "2.1 per hour"),
            Map.of("label", "Bed Turnover Time", "value", "35 minutes"),
            Map.of("label", "Left Without Being Seen", "value", "2 patients")
        );
        for (var row : flowMetricRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }

        // And trending graphs show patterns over time:
        List<Map<String, String>> trendGraphRows = List.of(
            Map.of("label", "Census Trend", "value", "24 hours"),
            Map.of("label", "Wait Time Trend", "value", "12 hours"),
            Map.of("label", "Capacity Utilization", "value", "7 days"),
            Map.of("label", "Provider Productivity", "value", "Shift")
        );
        for (var row : trendGraphRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }

        // And predictive indicators show projected status:
        List<Map<String, String>> predictiveIndicatorRows = List.of(
            Map.of("label", "Peak Time Prediction", "value", "Expected surge at 18:00-20:00"),
            Map.of("label", "Capacity Projection", "value", "Will reach 100% capacity in 90 minutes"),
            Map.of("label", "Staffing Needs", "value", "Additional provider needed by 16:00")
        );
        for (var row : predictiveIndicatorRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }
    }

    @Test
    @Order(5)
    @DisplayName("Customize dashboard view based on role and preferences")
    void customizeDashboardViewBasedOnRoleAndPreferences() {
        // Given I have charge nurse privileges and preferences set
        // When I access my personalized dashboard view
        page.getByTestId("personalized-dashboard-nav").first().click();

        // Then the system displays role-specific information:
        List<Map<String, String>> dashboardSectionRows = List.of(
            Map.of("label", "Resource Management", "value", "Bed status, staffing levels, equipment"),
            Map.of("label", "Quality Metrics", "value", "Wait times, satisfaction scores, safety"),
            Map.of("label", "Operational Alerts", "value", "Capacity issues, workflow bottlenecks"),
            Map.of("label", "Staff Coordination", "value", "Break schedules, assignments, coverage")
        );
        for (var row : dashboardSectionRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }

        // And customizable widgets show preferred metrics:
        List<Map<String, String>> widgetConfigurationRows = List.of(
            Map.of("label", "Bed Management Grid", "value", "Color-coded bed status with room numbers"),
            Map.of("label", "Provider Status Board", "value", "Real-time availability and patient load"),
            Map.of("label", "Queue Management", "value", "ESI-sorted patient list with wait times"),
            Map.of("label", "Performance KPIs", "value", "Key metrics with targets and trends")
        );
        for (var row : widgetConfigurationRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }

        // And I can modify the layout and priority of information displayed
        var layoutEditor = waitForTestId(page, "Layout Editor");
        assertTrue(layoutEditor.isVisible());

        // And the system saves my preferences for future sessions
        var preferencesSavedStatus = getText(page, "Preferences Saved Status");
        assertMatches(preferencesSavedStatus, "saved", true);
    }

    @Test
    @Order(6)
    @DisplayName("Display emergency and crisis mode indicators")
    void displayEmergencyAndCrisisModeIndicators() {
        // Given the ED is operating under normal conditions
        // When emergency conditions trigger crisis mode: (assumed simulated by test fixture data)

        // Then the dashboard displays crisis mode indicators:
        List<Map<String, String>> crisisDisplayRows = List.of(
            Map.of("label", "Mode Banner", "value", "Red \"CRISIS MODE ACTIVE\" across top"),
            Map.of("label", "Protocol Status", "value", "Active emergency protocols listed"),
            Map.of("label", "Resource Allocation", "value", "Special staffing and bed assignments"),
            Map.of("label", "Communication Center", "value", "Emergency contact information prominent")
        );
        for (var row : crisisDisplayRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }

        // And specialized crisis metrics are shown:
        List<Map<String, String>> crisisMetricRows = List.of(
            Map.of("label", "Response Teams", "value", "Available emergency response personnel"),
            Map.of("label", "Special Equipment", "value", "Crisis supplies and equipment status"),
            Map.of("label", "External Coordination", "value", "Communication with EMS, other hospitals"),
            Map.of("label", "Surge Capacity", "value", "Additional beds and overflow areas")
        );
        for (var row : crisisMetricRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }

        // And normal operations metrics are supplemented with crisis-specific data
        var crisisSpecificData = waitForTestId(page, "Crisis Specific Data");
        assertTrue(crisisSpecificData.isVisible());
    }

    @Test
    @Order(7)
    @DisplayName("Integrate with mobile devices for dashboard access")
    void integrateWithMobileDevicesForDashboardAccess() {
        // Given I need to monitor the dashboard while mobile in the department
        // When I access the dashboard on my mobile device
        login(page, "Charge Nurse Williams", true);

        // This is a second, mid-scenario login (the desktop login from
        // beforeEach already happened), so the dashboard reloads back to its
        // Overview screen -- reselect the panel before checking its content.
        var mobileNavLink = waitForTestId(page, "Nav Real-time Dashboard");
        mobileNavLink.click();
        waitForTestId(page, "Real-time Dashboard Panel");

        // Then the system provides a mobile-optimized view:
        List<Map<String, String>> mobileFeatureRows = List.of(
            Map.of("label", "Summary Cards", "value", "Key metrics in swipeable card format"),
            Map.of("label", "Alert Notifications", "value", "Push notifications for critical alerts"),
            Map.of("label", "Quick Actions", "value", "Rapid access to common charge nurse tasks"),
            Map.of("label", "Touch Interface", "value", "Optimized for touch navigation")
        );
        for (var row : mobileFeatureRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }

        // And essential information is prioritized for small screen:
        List<Map<String, String>> priorityDisplayRows = List.of(
            Map.of("label", "Critical Alerts", "value", "Top of screen with prominent notification"),
            Map.of("label", "Bed Status Summary", "value", "Visual grid with color-coded indicators"),
            Map.of("label", "Staff Availability", "value", "Provider status with quick contact options"),
            Map.of("label", "Key Metrics", "value", "Current census, wait times, alerts")
        );
        for (var row : priorityDisplayRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }

        // And synchronization occurs in real-time between desktop and mobile views
        var syncStatus = getText(page, "Sync Status");
        assertMatches(syncStatus, "real-time", true);

        // And offline capability maintains last-known status when connectivity is lost
        var offlineStatusIndicator = waitForTestId(page, "Offline Status Indicator");
        assertTrue(offlineStatusIndicator.isVisible());
    }

    @Test
    @Order(8)
    @DisplayName("Historical data comparison and trend analysis")
    void historicalDataComparisonAndTrendAnalysis() {
        // Given I want to compare current performance to historical patterns
        // When I access the trend analysis section of the dashboard
        page.getByTestId("trend-analysis-nav").first().click();

        // Then the system displays comparative data:
        List<Map<String, String>> comparisonTypeRows = List.of(
            Map.of("label", "Same Day Last Week", "value", "Tuesday to Tuesday comparison"),
            Map.of("label", "Monthly Average", "value", "Current day vs monthly average"),
            Map.of("label", "Seasonal Patterns", "value", "Year-over-year seasonal comparison"),
            Map.of("label", "Shift Comparisons", "value", "Day vs evening vs night shift metrics")
        );
        for (var row : comparisonTypeRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }

        // And performance benchmarks are shown:
        List<Map<String, String>> benchmarkRows = List.of(
            Map.of("label", "Internal Targets", "value", "Hospital-specific performance goals"),
            Map.of("label", "Industry Standards", "value", "National ED performance benchmarks"),
            Map.of("label", "Peer Comparison", "value", "Similar-sized ED performance data")
        );
        for (var row : benchmarkRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }

        // And variance analysis highlights significant deviations:
        List<Map<String, String>> varianceAlertRows = List.of(
            Map.of("label", "Significant Increase", "value", ">20% above normal pattern"),
            Map.of("label", "Significant Decrease", "value", ">15% below expected performance"),
            Map.of("label", "Unusual Pattern", "value", "Unexpected trends or anomalies")
        );
        for (var row : varianceAlertRows) {
            assertEquals(row.get("value"), getText(page, row.get("label")));
        }

        // And recommendations are provided for performance improvement
        var recommendations = page.getByTestId("performance-recommendation");
        assertTrue(recommendations.count() > 0);
    }
}
