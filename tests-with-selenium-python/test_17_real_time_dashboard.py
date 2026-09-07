"""Selenium WebDriver + pytest test for
tests-with-given-when-then-features/17-real-time-dashboard.feature
(equivalent to tests-with-selenium-javascript/17-real-time-dashboard.test.js).

Assumes the app exposes data-testid attributes matching each Gherkin field
label (kebab-cased, see support/fields.py) and the shared data-testid
contract in support/login.py (login-identity, login-submit, app-root).
"""

from selenium.webdriver.common.by import By

from support.build_driver import build_driver
from support.fields import get_text, wait_for_test_id
from support.login import login, verify_system_is_operational


class TestRealTimeDashboard:
    @classmethod
    def setup_class(cls):
        cls.driver = build_driver()

    @classmethod
    def teardown_class(cls):
        cls.driver.quit()

    def setup_method(self):
        # Background:
        #   Given the emergency care system is operational
        #   And I am logged in as "Charge Nurse Williams"
        #   And the real-time dashboard module is active (assumed pre-seeded test data)
        #   And automatic data refresh is enabled at 30-second intervals (assumed pre-seeded test data)
        #   And all data sources are connected and synchronized (assumed pre-seeded test data)
        verify_system_is_operational(self.driver)
        login(self.driver, "Charge Nurse Williams")

        feature_nav_link = wait_for_test_id(self.driver, "Nav Real-time Dashboard")
        feature_nav_link.click()
        wait_for_test_id(self.driver, "Real-time Dashboard Panel")

    def test_display_comprehensive_department_status_on_main_dashboard(self):
        # Given it is 14:30 on a busy Tuesday afternoon (assumed pre-seeded test data)
        # And the ED has the following current status: (assumed pre-seeded test data)
        # When I access the main dashboard
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="main-dashboard-nav"]').click()

        # Then the system displays the current patient census:
        wait_for_test_id(self.driver, "Patient Census Display")
        census_rows = [
            {"label": "Total Patients", "value": "18 (90% capacity) - Yellow indicator"},
            {"label": "Admitted Patients", "value": "14 currently in beds"},
            {"label": "Waiting Patients", "value": "12 in triage queue - Red alert"},
            {"label": "Discharged Today", "value": "23 patients processed"},
        ]
        for row in census_rows:
            assert get_text(self.driver, row["label"]) == row["value"]

        # And the system shows average wait times:
        wait_time_rows = [
            {"label": "Triage to Bed", "value": "45 minutes"},
            {"label": "Bed to Provider", "value": "25 minutes"},
            {"label": "Provider to Discharge", "value": "120 minutes"},
            {"label": "Total ED Length of Stay", "value": "190 minutes"},
        ]
        for row in wait_time_rows:
            assert get_text(self.driver, row["label"]) == row["value"]

        # And bed availability is displayed with visual indicators:
        bed_status_rows = [
            {"label": "Available Clean", "value": "2"},
            {"label": "Needs Cleaning", "value": "2"},
            {"label": "Occupied", "value": "16"},
            {"label": "Out of Service", "value": "0"},
        ]
        for row in bed_status_rows:
            assert get_text(self.driver, row["label"]) == row["value"]

        # And staff assignments are shown in real-time:
        staff_assignment_rows = [
            {"label": "Dr. Smith", "value": "3 patients"},
            {"label": "Dr. Johnson", "value": "4 patients"},
            {"label": "Nurse Martinez", "value": "5 patients"},
            {"label": "Nurse Chen", "value": "4 patients"},
        ]
        for row in staff_assignment_rows:
            assert get_text(self.driver, row["label"]) == row["value"]

    def test_monitor_real_time_updates_with_30_second_refresh_cycle(self):
        # Given the dashboard is displaying current data at 15:00:00 (assumed pre-seeded test data)
        # And automatic refresh is set to 30-second intervals (assumed pre-seeded test data)
        # When 30 seconds elapse and new data becomes available: (assumed simulated by test fixture data)

        # Then the dashboard automatically updates at 15:00:30:
        updated_metric_rows = [
            {"label": "Patient Count", "value": "19 (95% capacity) - Red indicator"},
            {"label": "Bed Availability", "value": "1 available - Critical level alert"},
            {"label": "Wait Time Alert", "value": "Triage wait exceeds 45 min threshold"},
            {"label": "Staff Status", "value": "Dr. Smith now unavailable"},
        ]
        for row in updated_metric_rows:
            assert get_text(self.driver, row["label"]) == row["value"]

        # And visual indicators reflect the changes:
        visual_update_rows = [
            {"label": "Capacity Indicator", "value": "Yellow → Red (approaching full capacity)"},
            {"label": "Bed Status Alert", "value": "New warning for low bed availability"},
            {"label": "Provider Icon", "value": 'Dr. Smith icon changes to "busy" status'},
        ]
        for row in visual_update_rows:
            assert get_text(self.driver, row["label"]) == row["value"]

        # And the last update timestamp shows "Updated: 15:00:30"
        last_update_timestamp = get_text(self.driver, "Last Update Timestamp")
        assert last_update_timestamp == "Updated: 15:00:30"

        # And critical alerts are highlighted with flashing indicators
        critical_alert_indicator = wait_for_test_id(self.driver, "Critical Alert Indicator")
        assert critical_alert_indicator.is_displayed()

    def test_display_priority_alerts_and_notifications_on_dashboard(self):
        # Given the dashboard is actively monitoring department status
        # When critical conditions occur requiring immediate attention: (assumed simulated by test fixture data)

        # Then the dashboard displays prominent alert notifications:
        alert_priority_rows = [
            {"label": "CRITICAL", "value": "🚨 Red flashing banner at top of screen"},
            {"label": "HIGH", "value": "🟠 Orange banner with urgent icon"},
            {"label": "MEDIUM", "value": "🟡 Yellow notification strip"},
            {"label": "INFO", "value": "🔵 Blue informational indicator"},
        ]
        for row in alert_priority_rows:
            assert get_text(self.driver, row["label"]) == row["value"]

        # And specific alert details are shown:
        alert_detail_rows = [
            {"label": "Capacity Critical", "value": "ED at 95% capacity - Consider diversion"},
            {"label": "Wait Time Excessive", "value": "Triage wait: 65 min - Expedite process"},
            {"label": "ESI 1 Patient", "value": "Critical patient waiting - Bed needed"},
            {"label": "Staffing Concern", "value": "Provider ratio 1:5 - Additional coverage needed"},
        ]
        for row in alert_detail_rows:
            assert get_text(self.driver, row["label"]) == row["value"]

        # And alert acknowledgment is required for critical notifications
        alert_acknowledgment = wait_for_test_id(self.driver, "Alert Acknowledgment Required")
        assert alert_acknowledgment.is_displayed()

        # And alert history is maintained for trend analysis
        alert_history_entries = self.driver.find_elements(By.CSS_SELECTOR, '[data-testid="alert-history-entry"]')
        assert len(alert_history_entries) > 0

    def test_monitor_specific_patient_flow_metrics_and_trends(self):
        # Given I need to track detailed patient flow performance
        # When I access the detailed metrics section of the dashboard
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="detailed-metrics-nav"]').click()

        # Then the system displays comprehensive flow metrics:
        flow_metric_rows = [
            {"label": "Patients per Hour", "value": "3.2 arrivals"},
            {"label": "Discharge Rate", "value": "2.1 per hour"},
            {"label": "Bed Turnover Time", "value": "35 minutes"},
            {"label": "Left Without Being Seen", "value": "2 patients"},
        ]
        for row in flow_metric_rows:
            assert get_text(self.driver, row["label"]) == row["value"]

        # And trending graphs show patterns over time:
        trend_graph_rows = [
            {"label": "Census Trend", "value": "24 hours"},
            {"label": "Wait Time Trend", "value": "12 hours"},
            {"label": "Capacity Utilization", "value": "7 days"},
            {"label": "Provider Productivity", "value": "Shift"},
        ]
        for row in trend_graph_rows:
            assert get_text(self.driver, row["label"]) == row["value"]

        # And predictive indicators show projected status:
        predictive_indicator_rows = [
            {"label": "Peak Time Prediction", "value": "Expected surge at 18:00-20:00"},
            {"label": "Capacity Projection", "value": "Will reach 100% capacity in 90 minutes"},
            {"label": "Staffing Needs", "value": "Additional provider needed by 16:00"},
        ]
        for row in predictive_indicator_rows:
            assert get_text(self.driver, row["label"]) == row["value"]

    def test_customize_dashboard_view_based_on_role_and_preferences(self):
        # Given I have charge nurse privileges and preferences set
        # When I access my personalized dashboard view
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="personalized-dashboard-nav"]').click()

        # Then the system displays role-specific information:
        dashboard_section_rows = [
            {"label": "Resource Management", "value": "Bed status, staffing levels, equipment"},
            {"label": "Quality Metrics", "value": "Wait times, satisfaction scores, safety"},
            {"label": "Operational Alerts", "value": "Capacity issues, workflow bottlenecks"},
            {"label": "Staff Coordination", "value": "Break schedules, assignments, coverage"},
        ]
        for row in dashboard_section_rows:
            assert get_text(self.driver, row["label"]) == row["value"]

        # And customizable widgets show preferred metrics:
        widget_configuration_rows = [
            {"label": "Bed Management Grid", "value": "Color-coded bed status with room numbers"},
            {"label": "Provider Status Board", "value": "Real-time availability and patient load"},
            {"label": "Queue Management", "value": "ESI-sorted patient list with wait times"},
            {"label": "Performance KPIs", "value": "Key metrics with targets and trends"},
        ]
        for row in widget_configuration_rows:
            assert get_text(self.driver, row["label"]) == row["value"]

        # And I can modify the layout and priority of information displayed
        layout_editor = wait_for_test_id(self.driver, "Layout Editor")
        assert layout_editor.is_displayed()

        # And the system saves my preferences for future sessions
        preferences_saved_status = get_text(self.driver, "Preferences Saved Status")
        assert "saved" in preferences_saved_status.lower()

    def test_display_emergency_and_crisis_mode_indicators(self):
        # Given the ED is operating under normal conditions
        # When emergency conditions trigger crisis mode: (assumed simulated by test fixture data)

        # Then the dashboard displays crisis mode indicators:
        crisis_display_rows = [
            {"label": "Mode Banner", "value": 'Red "CRISIS MODE ACTIVE" across top'},
            {"label": "Protocol Status", "value": "Active emergency protocols listed"},
            {"label": "Resource Allocation", "value": "Special staffing and bed assignments"},
            {"label": "Communication Center", "value": "Emergency contact information prominent"},
        ]
        for row in crisis_display_rows:
            assert get_text(self.driver, row["label"]) == row["value"]

        # And specialized crisis metrics are shown:
        crisis_metric_rows = [
            {"label": "Response Teams", "value": "Available emergency response personnel"},
            {"label": "Special Equipment", "value": "Crisis supplies and equipment status"},
            {"label": "External Coordination", "value": "Communication with EMS, other hospitals"},
            {"label": "Surge Capacity", "value": "Additional beds and overflow areas"},
        ]
        for row in crisis_metric_rows:
            assert get_text(self.driver, row["label"]) == row["value"]

        # And normal operations metrics are supplemented with crisis-specific data
        crisis_specific_data = wait_for_test_id(self.driver, "Crisis Specific Data")
        assert crisis_specific_data.is_displayed()

    def test_integrate_with_mobile_devices_for_dashboard_access(self):
        # Given I need to monitor the dashboard while mobile in the department
        # When I access the dashboard on my mobile device
        login(self.driver, "Charge Nurse Williams", mobile=True)

        # This is a second, mid-scenario login (the desktop login from
        # setup_method already happened), so the dashboard reloads back to its
        # Overview screen -- reselect the panel before checking its content.
        mobile_nav_link = wait_for_test_id(self.driver, "Nav Real-time Dashboard")
        mobile_nav_link.click()
        wait_for_test_id(self.driver, "Real-time Dashboard Panel")

        # Then the system provides a mobile-optimized view:
        mobile_feature_rows = [
            {"label": "Summary Cards", "value": "Key metrics in swipeable card format"},
            {"label": "Alert Notifications", "value": "Push notifications for critical alerts"},
            {"label": "Quick Actions", "value": "Rapid access to common charge nurse tasks"},
            {"label": "Touch Interface", "value": "Optimized for touch navigation"},
        ]
        for row in mobile_feature_rows:
            assert get_text(self.driver, row["label"]) == row["value"]

        # And essential information is prioritized for small screen:
        priority_display_rows = [
            {"label": "Critical Alerts", "value": "Top of screen with prominent notification"},
            {"label": "Bed Status Summary", "value": "Visual grid with color-coded indicators"},
            {"label": "Staff Availability", "value": "Provider status with quick contact options"},
            {"label": "Key Metrics", "value": "Current census, wait times, alerts"},
        ]
        for row in priority_display_rows:
            assert get_text(self.driver, row["label"]) == row["value"]

        # And synchronization occurs in real-time between desktop and mobile views
        sync_status = get_text(self.driver, "Sync Status")
        assert "real-time" in sync_status.lower()

        # And offline capability maintains last-known status when connectivity is lost
        offline_status_indicator = wait_for_test_id(self.driver, "Offline Status Indicator")
        assert offline_status_indicator.is_displayed()

    def test_historical_data_comparison_and_trend_analysis(self):
        # Given I want to compare current performance to historical patterns
        # When I access the trend analysis section of the dashboard
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="trend-analysis-nav"]').click()

        # Then the system displays comparative data:
        comparison_type_rows = [
            {"label": "Same Day Last Week", "value": "Tuesday to Tuesday comparison"},
            {"label": "Monthly Average", "value": "Current day vs monthly average"},
            {"label": "Seasonal Patterns", "value": "Year-over-year seasonal comparison"},
            {"label": "Shift Comparisons", "value": "Day vs evening vs night shift metrics"},
        ]
        for row in comparison_type_rows:
            assert get_text(self.driver, row["label"]) == row["value"]

        # And performance benchmarks are shown:
        benchmark_rows = [
            {"label": "Internal Targets", "value": "Hospital-specific performance goals"},
            {"label": "Industry Standards", "value": "National ED performance benchmarks"},
            {"label": "Peer Comparison", "value": "Similar-sized ED performance data"},
        ]
        for row in benchmark_rows:
            assert get_text(self.driver, row["label"]) == row["value"]

        # And variance analysis highlights significant deviations:
        variance_alert_rows = [
            {"label": "Significant Increase", "value": ">20% above normal pattern"},
            {"label": "Significant Decrease", "value": ">15% below expected performance"},
            {"label": "Unusual Pattern", "value": "Unexpected trends or anomalies"},
        ]
        for row in variance_alert_rows:
            assert get_text(self.driver, row["label"]) == row["value"]

        # And recommendations are provided for performance improvement
        recommendations = self.driver.find_elements(By.CSS_SELECTOR, '[data-testid="performance-recommendation"]')
        assert len(recommendations) > 0
