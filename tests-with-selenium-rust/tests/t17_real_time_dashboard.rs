// Selenium WebDriver + libtest-mimic test for
// tests-with-given-when-then-features/17-real-time-dashboard.feature
// (equivalent to tests-with-selenium-javascript/17-real-time-dashboard.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see tests/support/fields.rs) and the shared
// data-testid contract in tests/support/login.rs (login-identity, login-submit,
// app-root).

#![allow(unused_variables)]

mod support;
use support::*;

async fn background(driver: &WebDriver) -> WebDriverResult<()> {
    // Background:
    //   Given the emergency care system is operational
    //   And I am logged in as "Charge Nurse Williams"
    //   And the real-time dashboard module is active (assumed pre-seeded test data)
    //   And automatic data refresh is enabled at 30-second intervals (assumed pre-seeded test data)
    //   And all data sources are connected and synchronized (assumed pre-seeded test data)
    verify_system_is_operational(driver).await?;
    login(driver, "Charge Nurse Williams", false).await?;

    let feature_nav_link = wait_for_test_id(driver, "Nav Real-time Dashboard").await?;
    feature_nav_link.click().await?;
    wait_for_test_id(driver, "Real-time Dashboard Panel").await?;
    Ok(())
}

fn scenario_01_display_comprehensive_department_status_on_main_dashboard(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given it is 14:30 on a busy Tuesday afternoon (assumed pre-seeded test data)
        // And the ED has the following current status: (assumed pre-seeded test data)
        // When I access the main dashboard
        driver.find(By::Css("[data-testid=\"main-dashboard-nav\"]")).await?.click().await?;

        // Then the system displays the current patient census:
        wait_for_test_id(driver, "Patient Census Display").await?;
        let census_rows = vec![
            row([("label", "Total Patients"), ("value", "18 (90% capacity) - Yellow indicator")]),
            row([("label", "Admitted Patients"), ("value", "14 currently in beds")]),
            row([("label", "Waiting Patients"), ("value", "12 in triage queue - Red alert")]),
            row([("label", "Discharged Today"), ("value", "23 patients processed")]),
        ];
        for row in &census_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And the system shows average wait times:
        let wait_time_rows = vec![
            row([("label", "Triage to Bed"), ("value", "45 minutes")]),
            row([("label", "Bed to Provider"), ("value", "25 minutes")]),
            row([("label", "Provider to Discharge"), ("value", "120 minutes")]),
            row([("label", "Total ED Length of Stay"), ("value", "190 minutes")]),
        ];
        for row in &wait_time_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And bed availability is displayed with visual indicators:
        let bed_status_rows = vec![
            row([("label", "Available Clean"), ("value", "2")]),
            row([("label", "Needs Cleaning"), ("value", "2")]),
            row([("label", "Occupied"), ("value", "16")]),
            row([("label", "Out of Service"), ("value", "0")]),
        ];
        for row in &bed_status_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And staff assignments are shown in real-time:
        let staff_assignment_rows = vec![
            row([("label", "Dr. Smith"), ("value", "3 patients")]),
            row([("label", "Dr. Johnson"), ("value", "4 patients")]),
            row([("label", "Nurse Martinez"), ("value", "5 patients")]),
            row([("label", "Nurse Chen"), ("value", "4 patients")]),
        ];
        for row in &staff_assignment_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }
        Ok(())
    })
}

fn scenario_02_monitor_real_time_updates_with_30_second_refresh_cycle(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given the dashboard is displaying current data at 15:00:00 (assumed pre-seeded test data)
        // And automatic refresh is set to 30-second intervals (assumed pre-seeded test data)
        // When 30 seconds elapse and new data becomes available: (assumed simulated by test fixture data)

        // Then the dashboard automatically updates at 15:00:30:
        let updated_metric_rows = vec![
            row([("label", "Patient Count"), ("value", "19 (95% capacity) - Red indicator")]),
            row([("label", "Bed Availability"), ("value", "1 available - Critical level alert")]),
            row([("label", "Wait Time Alert"), ("value", "Triage wait exceeds 45 min threshold")]),
            row([("label", "Staff Status"), ("value", "Dr. Smith now unavailable")]),
        ];
        for row in &updated_metric_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And visual indicators reflect the changes:
        let visual_update_rows = vec![
            row([("label", "Capacity Indicator"), ("value", "Yellow → Red (approaching full capacity)")]),
            row([("label", "Bed Status Alert"), ("value", "New warning for low bed availability")]),
            row([("label", "Provider Icon"), ("value", "Dr. Smith icon changes to \"busy\" status")]),
        ];
        for row in &visual_update_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And the last update timestamp shows "Updated: 15:00:30"
        let last_update_timestamp = get_text(driver, "Last Update Timestamp").await?;
        assert_eq!(last_update_timestamp, "Updated: 15:00:30");

        // And critical alerts are highlighted with flashing indicators
        let critical_alert_indicator = wait_for_test_id(driver, "Critical Alert Indicator").await?;
        assert!(critical_alert_indicator.is_displayed().await?);
        Ok(())
    })
}

fn scenario_03_display_priority_alerts_and_notifications_on_dashboard(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given the dashboard is actively monitoring department status
        // When critical conditions occur requiring immediate attention: (assumed simulated by test fixture data)

        // Then the dashboard displays prominent alert notifications:
        let alert_priority_rows = vec![
            row([("label", "CRITICAL"), ("value", "🚨 Red flashing banner at top of screen")]),
            row([("label", "HIGH"), ("value", "🟠 Orange banner with urgent icon")]),
            row([("label", "MEDIUM"), ("value", "🟡 Yellow notification strip")]),
            row([("label", "INFO"), ("value", "🔵 Blue informational indicator")]),
        ];
        for row in &alert_priority_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And specific alert details are shown:
        let alert_detail_rows = vec![
            row([("label", "Capacity Critical"), ("value", "ED at 95% capacity - Consider diversion")]),
            row([("label", "Wait Time Excessive"), ("value", "Triage wait: 65 min - Expedite process")]),
            row([("label", "ESI 1 Patient"), ("value", "Critical patient waiting - Bed needed")]),
            row([("label", "Staffing Concern"), ("value", "Provider ratio 1:5 - Additional coverage needed")]),
        ];
        for row in &alert_detail_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And alert acknowledgment is required for critical notifications
        let alert_acknowledgment = wait_for_test_id(driver, "Alert Acknowledgment Required").await?;
        assert!(alert_acknowledgment.is_displayed().await?);

        // And alert history is maintained for trend analysis
        let alert_history_entries = driver.find_all(By::Css("[data-testid=\"alert-history-entry\"]")).await?;
        assert!(alert_history_entries.len() > 0);
        Ok(())
    })
}

fn scenario_04_monitor_specific_patient_flow_metrics_and_trends(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given I need to track detailed patient flow performance
        // When I access the detailed metrics section of the dashboard
        driver.find(By::Css("[data-testid=\"detailed-metrics-nav\"]")).await?.click().await?;

        // Then the system displays comprehensive flow metrics:
        let flow_metric_rows = vec![
            row([("label", "Patients per Hour"), ("value", "3.2 arrivals")]),
            row([("label", "Discharge Rate"), ("value", "2.1 per hour")]),
            row([("label", "Bed Turnover Time"), ("value", "35 minutes")]),
            row([("label", "Left Without Being Seen"), ("value", "2 patients")]),
        ];
        for row in &flow_metric_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And trending graphs show patterns over time:
        let trend_graph_rows = vec![
            row([("label", "Census Trend"), ("value", "24 hours")]),
            row([("label", "Wait Time Trend"), ("value", "12 hours")]),
            row([("label", "Capacity Utilization"), ("value", "7 days")]),
            row([("label", "Provider Productivity"), ("value", "Shift")]),
        ];
        for row in &trend_graph_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And predictive indicators show projected status:
        let predictive_indicator_rows = vec![
            row([("label", "Peak Time Prediction"), ("value", "Expected surge at 18:00-20:00")]),
            row([("label", "Capacity Projection"), ("value", "Will reach 100% capacity in 90 minutes")]),
            row([("label", "Staffing Needs"), ("value", "Additional provider needed by 16:00")]),
        ];
        for row in &predictive_indicator_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }
        Ok(())
    })
}

fn scenario_05_customize_dashboard_view_based_on_role_and_preferences(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given I have charge nurse privileges and preferences set
        // When I access my personalized dashboard view
        driver.find(By::Css("[data-testid=\"personalized-dashboard-nav\"]")).await?.click().await?;

        // Then the system displays role-specific information:
        let dashboard_section_rows = vec![
            row([("label", "Resource Management"), ("value", "Bed status, staffing levels, equipment")]),
            row([("label", "Quality Metrics"), ("value", "Wait times, satisfaction scores, safety")]),
            row([("label", "Operational Alerts"), ("value", "Capacity issues, workflow bottlenecks")]),
            row([("label", "Staff Coordination"), ("value", "Break schedules, assignments, coverage")]),
        ];
        for row in &dashboard_section_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And customizable widgets show preferred metrics:
        let widget_configuration_rows = vec![
            row([("label", "Bed Management Grid"), ("value", "Color-coded bed status with room numbers")]),
            row([("label", "Provider Status Board"), ("value", "Real-time availability and patient load")]),
            row([("label", "Queue Management"), ("value", "ESI-sorted patient list with wait times")]),
            row([("label", "Performance KPIs"), ("value", "Key metrics with targets and trends")]),
        ];
        for row in &widget_configuration_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And I can modify the layout and priority of information displayed
        let layout_editor = wait_for_test_id(driver, "Layout Editor").await?;
        assert!(layout_editor.is_displayed().await?);

        // And the system saves my preferences for future sessions
        let preferences_saved_status = get_text(driver, "Preferences Saved Status").await?;
        assert_match(&preferences_saved_status, r"saved", true);
        Ok(())
    })
}

fn scenario_06_display_emergency_and_crisis_mode_indicators(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given the ED is operating under normal conditions
        // When emergency conditions trigger crisis mode: (assumed simulated by test fixture data)

        // Then the dashboard displays crisis mode indicators:
        let crisis_display_rows = vec![
            row([("label", "Mode Banner"), ("value", "Red \"CRISIS MODE ACTIVE\" across top")]),
            row([("label", "Protocol Status"), ("value", "Active emergency protocols listed")]),
            row([("label", "Resource Allocation"), ("value", "Special staffing and bed assignments")]),
            row([("label", "Communication Center"), ("value", "Emergency contact information prominent")]),
        ];
        for row in &crisis_display_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And specialized crisis metrics are shown:
        let crisis_metric_rows = vec![
            row([("label", "Response Teams"), ("value", "Available emergency response personnel")]),
            row([("label", "Special Equipment"), ("value", "Crisis supplies and equipment status")]),
            row([("label", "External Coordination"), ("value", "Communication with EMS, other hospitals")]),
            row([("label", "Surge Capacity"), ("value", "Additional beds and overflow areas")]),
        ];
        for row in &crisis_metric_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And normal operations metrics are supplemented with crisis-specific data
        let crisis_specific_data = wait_for_test_id(driver, "Crisis Specific Data").await?;
        assert!(crisis_specific_data.is_displayed().await?);
        Ok(())
    })
}

fn scenario_07_integrate_with_mobile_devices_for_dashboard_access(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given I need to monitor the dashboard while mobile in the department
        // When I access the dashboard on my mobile device
        login(driver, "Charge Nurse Williams", true).await?;

        // This is a second, mid-scenario login (the desktop login from
        // beforeEach already happened), so the dashboard reloads back to its
        // Overview screen -- reselect the panel before checking its content.
        let mobile_nav_link = wait_for_test_id(driver, "Nav Real-time Dashboard").await?;
        mobile_nav_link.click().await?;
        wait_for_test_id(driver, "Real-time Dashboard Panel").await?;

        // Then the system provides a mobile-optimized view:
        let mobile_feature_rows = vec![
            row([("label", "Summary Cards"), ("value", "Key metrics in swipeable card format")]),
            row([("label", "Alert Notifications"), ("value", "Push notifications for critical alerts")]),
            row([("label", "Quick Actions"), ("value", "Rapid access to common charge nurse tasks")]),
            row([("label", "Touch Interface"), ("value", "Optimized for touch navigation")]),
        ];
        for row in &mobile_feature_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And essential information is prioritized for small screen:
        let priority_display_rows = vec![
            row([("label", "Critical Alerts"), ("value", "Top of screen with prominent notification")]),
            row([("label", "Bed Status Summary"), ("value", "Visual grid with color-coded indicators")]),
            row([("label", "Staff Availability"), ("value", "Provider status with quick contact options")]),
            row([("label", "Key Metrics"), ("value", "Current census, wait times, alerts")]),
        ];
        for row in &priority_display_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And synchronization occurs in real-time between desktop and mobile views
        let sync_status = get_text(driver, "Sync Status").await?;
        assert_match(&sync_status, r"real-time", true);

        // And offline capability maintains last-known status when connectivity is lost
        let offline_status_indicator = wait_for_test_id(driver, "Offline Status Indicator").await?;
        assert!(offline_status_indicator.is_displayed().await?);
        Ok(())
    })
}

fn scenario_08_historical_data_comparison_and_trend_analysis(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given I want to compare current performance to historical patterns
        // When I access the trend analysis section of the dashboard
        driver.find(By::Css("[data-testid=\"trend-analysis-nav\"]")).await?.click().await?;

        // Then the system displays comparative data:
        let comparison_type_rows = vec![
            row([("label", "Same Day Last Week"), ("value", "Tuesday to Tuesday comparison")]),
            row([("label", "Monthly Average"), ("value", "Current day vs monthly average")]),
            row([("label", "Seasonal Patterns"), ("value", "Year-over-year seasonal comparison")]),
            row([("label", "Shift Comparisons"), ("value", "Day vs evening vs night shift metrics")]),
        ];
        for row in &comparison_type_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And performance benchmarks are shown:
        let benchmark_rows = vec![
            row([("label", "Internal Targets"), ("value", "Hospital-specific performance goals")]),
            row([("label", "Industry Standards"), ("value", "National ED performance benchmarks")]),
            row([("label", "Peer Comparison"), ("value", "Similar-sized ED performance data")]),
        ];
        for row in &benchmark_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And variance analysis highlights significant deviations:
        let variance_alert_rows = vec![
            row([("label", "Significant Increase"), ("value", ">20% above normal pattern")]),
            row([("label", "Significant Decrease"), ("value", ">15% below expected performance")]),
            row([("label", "Unusual Pattern"), ("value", "Unexpected trends or anomalies")]),
        ];
        for row in &variance_alert_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And recommendations are provided for performance improvement
        let recommendations = driver.find_all(By::Css("[data-testid=\"performance-recommendation\"]")).await?;
        assert!(recommendations.len() > 0);
        Ok(())
    })
}

fn main() {
    run_feature(&[
        ("Display comprehensive department status on main dashboard", scenario_01_display_comprehensive_department_status_on_main_dashboard),
        ("Monitor real-time updates with 30-second refresh cycle", scenario_02_monitor_real_time_updates_with_30_second_refresh_cycle),
        ("Display priority alerts and notifications on dashboard", scenario_03_display_priority_alerts_and_notifications_on_dashboard),
        ("Monitor specific patient flow metrics and trends", scenario_04_monitor_specific_patient_flow_metrics_and_trends),
        ("Customize dashboard view based on role and preferences", scenario_05_customize_dashboard_view_based_on_role_and_preferences),
        ("Display emergency and crisis mode indicators", scenario_06_display_emergency_and_crisis_mode_indicators),
        ("Integrate with mobile devices for dashboard access", scenario_07_integrate_with_mobile_devices_for_dashboard_access),
        ("Historical data comparison and trend analysis", scenario_08_historical_data_comparison_and_trend_analysis),
    ]);
}
