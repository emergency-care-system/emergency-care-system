// Playwright + libtest-mimic test for
// tests-with-given-when-then-features/18-performance-metrics.feature
// (equivalent to tests-with-playwright-javascript/18-performance-metrics.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see tests/support/fields.rs) and the shared
// data-testid contract in tests/support/login.rs (login-identity, login-submit,
// app-root).

#![allow(unused_variables)]

mod support;
use support::*;

async fn background(page: &Page) -> Result<()> {
    // Background:
    //   Given the emergency care system is operational
    //   And I am logged in as "ED Manager Thompson"
    //   And the reporting module has access to historical data (assumed pre-seeded test data)
    //   And the performance metrics calculation engine is enabled (assumed pre-seeded test data)
    //   And patient satisfaction data is integrated from survey systems (assumed pre-seeded test data)
    verify_system_is_operational(page).await?;
    login(page, "ED Manager Thompson", false).await?;

    let feature_nav_link = wait_for_test_id(page, "Nav Performance Metrics").await?;
    feature_nav_link.click(None).await?;
    wait_for_test_id(page, "Performance Metrics Panel").await?;
    Ok(())
}

fn scenario_01_generate_comprehensive_monthly_performance_report(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given it is the first day of the new month (assumed pre-seeded test data)
        // And I need to review performance data for the previous month (May 2025) (assumed pre-seeded test data)
        // And the system has collected data from May 1-31, 2025 (assumed pre-seeded test data)
        // When I run the monthly performance report for May 2025
        fill_field(page, "Report Month", "May 2025").await?;
        page.get_by_test_id("generate-monthly-report").first().click(None).await?;

        // Then the system generates comprehensive metrics including:
        wait_for_test_id(page, "Core Metric Category Report").await?;
        let core_metric_category_rows = vec![
            row([("label", "Length of Stay"), ("value", "Average, median, 95th percentile by ESI")]),
            row([("label", "LWBS Rates"), ("value", "Left without being seen percentages")]),
            row([("label", "Patient Satisfaction"), ("value", "Overall scores and domain-specific ratings")]),
            row([("label", "Throughput Metrics"), ("value", "Door-to-provider, bed turnaround times")]),
            row([("label", "Quality Indicators"), ("value", "Safety events, readmission rates")]),
        ];
        for row in &core_metric_category_rows {
            assert_eq!(get_text(page, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And the average length of stay metrics show:
        let average_los_rows = vec![
            row([("label", "ESI 1 (Critical)"), ("value", "180 minutes")]),
            row([("label", "ESI 2 (High Priority)"), ("value", "165 minutes")]),
            row([("label", "ESI 3 (Urgent)"), ("value", "145 minutes")]),
            row([("label", "ESI 4 (Less Urgent)"), ("value", "95 minutes")]),
            row([("label", "ESI 5 (Non-urgent)"), ("value", "75 minutes")]),
            row([("label", "Overall Average"), ("value", "132 minutes")]),
        ];
        for row in &average_los_rows {
            assert_eq!(get_text(page, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And left without being seen (LWBS) rates are calculated:
        let lwbs_rate_rows = vec![
            row([("label", "Overall LWBS Rate"), ("value", "3.2%")]),
            row([("label", "ESI 3 LWBS Rate"), ("value", "4.1%")]),
            row([("label", "ESI 4 LWBS Rate"), ("value", "5.8%")]),
            row([("label", "ESI 5 LWBS Rate"), ("value", "8.2%")]),
            row([("label", "Peak Hours LWBS"), ("value", "6.1%")]),
        ];
        for row in &lwbs_rate_rows {
            assert_eq!(get_text(page, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And patient satisfaction scores are displayed:
        let satisfaction_score_rows = vec![
            row([("label", "Overall Satisfaction"), ("value", "87.3%")]),
            row([("label", "Communication"), ("value", "89.1%")]),
            row([("label", "Pain Management"), ("value", "84.7%")]),
            row([("label", "Staff Responsiveness"), ("value", "88.9%")]),
            row([("label", "Cleanliness"), ("value", "92.4%")]),
            row([("label", "Discharge Process"), ("value", "86.2%")]),
        ];
        for row in &satisfaction_score_rows {
            assert_eq!(get_text(page, row["label"].as_str()).await?, row["value"].as_str());
        }
        Ok(())
    })
}

fn scenario_02_compare_current_month_performance_to_historical_benchmarks(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given historical performance data is available for trending analysis
        // When I generate the monthly report with comparative analysis
        page.get_by_test_id("generate-comparative-report").first().click(None).await?;

        // Then the system displays month-over-month comparisons:
        wait_for_test_id(page, "Average Length of Stay Comparison").await?;
        let month_over_month_rows = vec![
            row([("label", "Average Length of Stay Comparison"), ("value", "132 min")]),
            row([("label", "LWBS Rate Comparison"), ("value", "3.2%")]),
            row([("label", "Patient Satisfaction Comparison"), ("value", "87.3%")]),
            row([("label", "Door-to-Provider Comparison"), ("value", "35 min")]),
            row([("label", "Bed Turnaround Comparison"), ("value", "28 min")]),
        ];
        for row in &month_over_month_rows {
            assert_eq!(get_text(page, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And year-over-year comparisons are shown:
        let year_over_year_rows = vec![
            row([("label", "Average LOS YoY"), ("value", "132 min")]),
            row([("label", "LWBS Rate YoY"), ("value", "3.2%")]),
            row([("label", "Patient Volume YoY"), ("value", "2,847 pts")]),
            row([("label", "Satisfaction YoY"), ("value", "87.3%")]),
        ];
        for row in &year_over_year_rows {
            assert_eq!(get_text(page, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And statistical significance testing results are included:
        let statistical_significance_rows = vec![
            row([("label", "Length of Stay Significance"), ("value", "0.003")]),
            row([("label", "LWBS Rate Significance"), ("value", "0.012")]),
            row([("label", "Satisfaction Significance"), ("value", "0.001")]),
        ];
        for row in &statistical_significance_rows {
            assert_eq!(get_text(page, row["label"].as_str()).await?, row["value"].as_str());
        }
        Ok(())
    })
}

fn scenario_03_generate_detailed_lwbs_analysis_with_root_cause_identificati(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given LWBS events occurred throughout May 2025
        // When I request detailed LWBS analysis in the monthly report
        page.get_by_test_id("request-lwbs-analysis").first().click(None).await?;

        // Then the system provides comprehensive LWBS breakdown:
        wait_for_test_id(page, "Time-based Patterns").await?;
        let lwbs_breakdown_rows = vec![
            row([("label", "Time-based Patterns"), ("value", "LWBS rates by hour, day of week, shift")]),
            row([("label", "Acuity Distribution"), ("value", "LWBS percentage by ESI level")]),
            row([("label", "Wait Time Correlation"), ("value", "LWBS rates vs wait time thresholds")]),
            row([("label", "Seasonal Factors"), ("value", "Weather, holidays, local events impact")]),
        ];
        for row in &lwbs_breakdown_rows {
            assert_eq!(get_text(page, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And root cause analysis is provided:
        let root_cause_rows = vec![
            row([("label", "Extended Wait Times"), ("value", "High")]),
            row([("label", "Staffing Shortages"), ("value", "High")]),
            row([("label", "Bed Availability"), ("value", "Medium")]),
            row([("label", "Triage Delays"), ("value", "Medium")]),
        ];
        for row in &root_cause_rows {
            assert_eq!(get_text(page, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And specific LWBS prevention recommendations are included:
        let prevention_recommendation_rows = vec![
            row([("label", "Fast Track Protocol"), ("value", "High")]),
            row([("label", "Provider Scheduling"), ("value", "High")]),
            row([("label", "Patient Communication"), ("value", "Medium")]),
            row([("label", "Comfort Amenities"), ("value", "Low")]),
        ];
        for row in &prevention_recommendation_rows {
            assert_eq!(get_text(page, row["label"].as_str()).await?, row["value"].as_str());
        }
        Ok(())
    })
}

fn scenario_04_analyze_patient_satisfaction_trends_with_demographic_breakdo(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given patient satisfaction surveys were collected throughout May 2025
        // When I generate detailed satisfaction analysis
        page.get_by_test_id("generate-satisfaction-analysis").first().click(None).await?;

        // Then the system provides demographic-based satisfaction breakdown:
        wait_for_test_id(page, "Age 18-35").await?;
        let demographic_satisfaction_rows = vec![
            row([("label", "Age 18-35"), ("value", "85.1%")]),
            row([("label", "Age 36-55"), ("value", "88.7%")]),
            row([("label", "Age 56-75"), ("value", "89.2%")]),
            row([("label", "Age 75+"), ("value", "86.8%")]),
            row([("label", "Male Patients"), ("value", "86.9%")]),
            row([("label", "Female Patients"), ("value", "87.7%")]),
        ];
        for row in &demographic_satisfaction_rows {
            assert_eq!(get_text(page, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And satisfaction domain analysis by patient type:
        let patient_type_overall_rows = vec![
            row([("label", "Trauma Patients Overall"), ("value", "82.0%")]),
            row([("label", "Chest Pain Overall"), ("value", "90.1%")]),
            row([("label", "Abdominal Pain Overall"), ("value", "84.4%")]),
            row([("label", "Minor Injuries Overall"), ("value", "89.6%")]),
        ];
        for row in &patient_type_overall_rows {
            assert_eq!(get_text(page, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And improvement opportunities are identified:
        let improvement_opportunity_rows = vec![
            row([("label", "Trauma Communication"), ("value", "82.3%")]),
            row([("label", "Pain Management Improvement Opportunity"), ("value", "79.1%")]),
            row([("label", "Young Adult Experience"), ("value", "85.1%")]),
        ];
        for row in &improvement_opportunity_rows {
            assert_eq!(get_text(page, row["label"].as_str()).await?, row["value"].as_str());
        }
        Ok(())
    })
}

fn scenario_05_generate_quality_and_safety_metrics_with_benchmarking(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given quality and safety data is tracked throughout May 2025
        // When I include quality metrics in the monthly report
        page.get_by_test_id("include-quality-metrics").first().click(None).await?;

        // Then the system displays comprehensive quality indicators:
        wait_for_test_id(page, "Medication Errors").await?;
        let quality_indicator_rows = vec![
            row([("label", "Medication Errors"), ("value", "0.12%")]),
            row([("label", "Patient Falls"), ("value", "0 events")]),
            row([("label", "Hospital Readmissions"), ("value", "2.1%")]),
            row([("label", "Infection Control"), ("value", "99.8%")]),
            row([("label", "Adverse Events"), ("value", "3 events")]),
        ];
        for row in &quality_indicator_rows {
            assert_eq!(get_text(page, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And safety event analysis is provided:
        let safety_event_rows = vec![
            row([("label", "Medication Error Count"), ("value", "2 events")]),
            row([("label", "Diagnostic Delay Count"), ("value", "1 event")]),
            row([("label", "Equipment Failure Count"), ("value", "0 events")]),
        ];
        for row in &safety_event_rows {
            assert_eq!(get_text(page, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And regulatory compliance status is shown:
        let compliance_status_rows = vec![
            row([("label", "Joint Commission"), ("value", "Compliant")]),
            row([("label", "CMS Core Measures"), ("value", "Compliant")]),
            row([("label", "State Regulations"), ("value", "Compliant")]),
        ];
        for row in &compliance_status_rows {
            assert_eq!(get_text(page, row["label"].as_str()).await?, row["value"].as_str());
        }
        Ok(())
    })
}

fn scenario_06_generate_financial_and_operational_efficiency_metrics(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given financial and operational data is available for May 2025
        // When I request comprehensive operational metrics
        page.get_by_test_id("request-operational-metrics").first().click(None).await?;

        // Then the system displays efficiency indicators:
        wait_for_test_id(page, "Cost per Patient").await?;
        let efficiency_indicator_rows = vec![
            row([("label", "Cost per Patient"), ("value", "$847")]),
            row([("label", "Revenue per Patient"), ("value", "$1,245")]),
            row([("label", "Staff Productivity"), ("value", "2.8 pts/hr")]),
            row([("label", "Bed Utilization"), ("value", "87.3%")]),
            row([("label", "Equipment Uptime"), ("value", "98.7%")]),
        ];
        for row in &efficiency_indicator_rows {
            assert_eq!(get_text(page, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And staffing metrics are included:
        let staffing_metric_rows = vec![
            row([("label", "RN Hours per Patient"), ("value", "4.2 hours")]),
            row([("label", "Physician Coverage"), ("value", "1:12 ratio")]),
            row([("label", "Overtime Hours"), ("value", "3.2%")]),
            row([("label", "Agency Staff Usage"), ("value", "1.8%")]),
        ];
        for row in &staffing_metric_rows {
            assert_eq!(get_text(page, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And productivity analysis by shift is provided:
        let shift_productivity_rows = vec![
            row([("label", "Day (7a-7p)"), ("value", "3.2")]),
            row([("label", "Evening (7p-11p)"), ("value", "2.8")]),
            row([("label", "Night (11p-7a)"), ("value", "1.9")]),
        ];
        for row in &shift_productivity_rows {
            assert_eq!(get_text(page, row["label"].as_str()).await?, row["value"].as_str());
        }
        Ok(())
    })
}

fn scenario_07_generate_executive_summary_with_actionable_recommendations(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given all performance data has been analyzed for May 2025
        // When I request the executive summary section of the report
        page.get_by_test_id("request-executive-summary").first().click(None).await?;

        // Then the system provides a concise executive overview:
        wait_for_test_id(page, "Overall Performance").await?;
        let executive_summary_rows = vec![
            row([("label", "Overall Performance"), ("value", "87% of targets met, significant improvements")]),
            row([("label", "Major Achievements"), ("value", "LOS reduction, LWBS improvement, satisfaction up")]),
            row([("label", "Areas of Concern"), ("value", "Night shift efficiency, young adult satisfaction")]),
            row([("label", "Financial Impact"), ("value", "$2.1M revenue, $53 cost reduction per patient")]),
        ];
        for row in &executive_summary_rows {
            assert_eq!(get_text(page, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And specific actionable recommendations are provided:
        let actionable_recommendation_rows = vec![
            row([("label", "Implement fast-track for ESI 4-5"), ("value", "15% LOS reduction")]),
            row([("label", "Night shift staffing optimization"), ("value", "10% efficiency ↗")]),
            row([("label", "Young adult communication program"), ("value", "3% satisfaction ↗")]),
            row([("label", "Comfort amenity upgrades"), ("value", "2% satisfaction ↗")]),
        ];
        for row in &actionable_recommendation_rows {
            assert_eq!(get_text(page, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And ROI analysis for recommendations is included:
        let roi_analysis_rows = vec![
            row([("label", "$125K (Fast Track)"), ("value", "$340K")]),
            row([("label", "$85K (Staffing)"), ("value", "$220K")]),
            row([("label", "$45K (Communication)"), ("value", "$95K")]),
        ];
        for row in &roi_analysis_rows {
            assert_eq!(get_text(page, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And next month's focus areas are identified for tracking progress
        let focus_areas = page.get_by_test_id("next-month-focus-area");
        assert!(focus_areas.count().await? > 0);
        Ok(())
    })
}

fn main() {
    run_feature(&[
        ("Generate comprehensive monthly performance report", scenario_01_generate_comprehensive_monthly_performance_report),
        ("Compare current month performance to historical benchmarks", scenario_02_compare_current_month_performance_to_historical_benchmarks),
        ("Generate detailed LWBS analysis with root cause identification", scenario_03_generate_detailed_lwbs_analysis_with_root_cause_identificati),
        ("Analyze patient satisfaction trends with demographic breakdown", scenario_04_analyze_patient_satisfaction_trends_with_demographic_breakdo),
        ("Generate quality and safety metrics with benchmarking", scenario_05_generate_quality_and_safety_metrics_with_benchmarking),
        ("Generate financial and operational efficiency metrics", scenario_06_generate_financial_and_operational_efficiency_metrics),
        ("Generate executive summary with actionable recommendations", scenario_07_generate_executive_summary_with_actionable_recommendations),
    ]);
}
