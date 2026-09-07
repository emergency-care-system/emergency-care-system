"""Playwright + pytest test for tests-with-given-when-then-features/18-performance-metrics.feature
(equivalent to tests-with-playwright-javascript/18-performance-metrics.test.js).

Assumes the app exposes data-testid attributes matching each Gherkin field
label (kebab-cased, see support/fields.py) and the shared data-testid
contract in support/login.py (login-identity, login-submit, app-root).
"""

from playwright.sync_api import sync_playwright

from support.fields import fill_field, get_text, wait_for_test_id
from support.login import login, verify_system_is_operational


class TestPerformanceMetrics:
    @classmethod
    def setup_class(cls):
        cls.playwright = sync_playwright().start()
        cls.browser = cls.playwright.chromium.launch()
        cls.page = cls.browser.new_page()

    @classmethod
    def teardown_class(cls):
        cls.browser.close()
        cls.playwright.stop()

    def setup_method(self):
        # Background:
        #   Given the emergency care system is operational
        #   And I am logged in as "ED Manager Thompson"
        #   And the reporting module has access to historical data (assumed pre-seeded test data)
        #   And the performance metrics calculation engine is enabled (assumed pre-seeded test data)
        #   And patient satisfaction data is integrated from survey systems (assumed pre-seeded test data)
        verify_system_is_operational(self.page)
        login(self.page, "ED Manager Thompson")

        feature_nav_link = wait_for_test_id(self.page, "Nav Performance Metrics")
        feature_nav_link.click()
        wait_for_test_id(self.page, "Performance Metrics Panel")

    def test_generate_comprehensive_monthly_performance_report(self):
        # Given it is the first day of the new month (assumed pre-seeded test data)
        # And I need to review performance data for the previous month (May 2025) (assumed pre-seeded test data)
        # And the system has collected data from May 1-31, 2025 (assumed pre-seeded test data)
        # When I run the monthly performance report for May 2025
        fill_field(self.page, "Report Month", "May 2025")
        self.page.get_by_test_id("generate-monthly-report").click()

        # Then the system generates comprehensive metrics including:
        wait_for_test_id(self.page, "Core Metric Category Report")
        core_metric_category_rows = [
            {"label": "Length of Stay", "value": "Average, median, 95th percentile by ESI"},
            {"label": "LWBS Rates", "value": "Left without being seen percentages"},
            {"label": "Patient Satisfaction", "value": "Overall scores and domain-specific ratings"},
            {"label": "Throughput Metrics", "value": "Door-to-provider, bed turnaround times"},
            {"label": "Quality Indicators", "value": "Safety events, readmission rates"},
        ]
        for row in core_metric_category_rows:
            assert get_text(self.page, row["label"]) == row["value"]

        # And the average length of stay metrics show:
        average_los_rows = [
            {"label": "ESI 1 (Critical)", "value": "180 minutes"},
            {"label": "ESI 2 (High Priority)", "value": "165 minutes"},
            {"label": "ESI 3 (Urgent)", "value": "145 minutes"},
            {"label": "ESI 4 (Less Urgent)", "value": "95 minutes"},
            {"label": "ESI 5 (Non-urgent)", "value": "75 minutes"},
            {"label": "Overall Average", "value": "132 minutes"},
        ]
        for row in average_los_rows:
            assert get_text(self.page, row["label"]) == row["value"]

        # And left without being seen (LWBS) rates are calculated:
        lwbs_rate_rows = [
            {"label": "Overall LWBS Rate", "value": "3.2%"},
            {"label": "ESI 3 LWBS Rate", "value": "4.1%"},
            {"label": "ESI 4 LWBS Rate", "value": "5.8%"},
            {"label": "ESI 5 LWBS Rate", "value": "8.2%"},
            {"label": "Peak Hours LWBS", "value": "6.1%"},
        ]
        for row in lwbs_rate_rows:
            assert get_text(self.page, row["label"]) == row["value"]

        # And patient satisfaction scores are displayed:
        satisfaction_score_rows = [
            {"label": "Overall Satisfaction", "value": "87.3%"},
            {"label": "Communication", "value": "89.1%"},
            {"label": "Pain Management", "value": "84.7%"},
            {"label": "Staff Responsiveness", "value": "88.9%"},
            {"label": "Cleanliness", "value": "92.4%"},
            {"label": "Discharge Process", "value": "86.2%"},
        ]
        for row in satisfaction_score_rows:
            assert get_text(self.page, row["label"]) == row["value"]

    def test_compare_current_month_performance_to_historical_benchmarks(self):
        # Given historical performance data is available for trending analysis
        # When I generate the monthly report with comparative analysis
        self.page.get_by_test_id("generate-comparative-report").click()

        # Then the system displays month-over-month comparisons:
        wait_for_test_id(self.page, "Average Length of Stay Comparison")
        month_over_month_rows = [
            {"label": "Average Length of Stay Comparison", "value": "132 min"},
            {"label": "LWBS Rate Comparison", "value": "3.2%"},
            {"label": "Patient Satisfaction Comparison", "value": "87.3%"},
            {"label": "Door-to-Provider Comparison", "value": "35 min"},
            {"label": "Bed Turnaround Comparison", "value": "28 min"},
        ]
        for row in month_over_month_rows:
            assert get_text(self.page, row["label"]) == row["value"]

        # And year-over-year comparisons are shown:
        year_over_year_rows = [
            {"label": "Average LOS YoY", "value": "132 min"},
            {"label": "LWBS Rate YoY", "value": "3.2%"},
            {"label": "Patient Volume YoY", "value": "2,847 pts"},
            {"label": "Satisfaction YoY", "value": "87.3%"},
        ]
        for row in year_over_year_rows:
            assert get_text(self.page, row["label"]) == row["value"]

        # And statistical significance testing results are included:
        statistical_significance_rows = [
            {"label": "Length of Stay Significance", "value": "0.003"},
            {"label": "LWBS Rate Significance", "value": "0.012"},
            {"label": "Satisfaction Significance", "value": "0.001"},
        ]
        for row in statistical_significance_rows:
            assert get_text(self.page, row["label"]) == row["value"]

    def test_generate_detailed_lwbs_analysis_with_root_cause_identification(self):
        # Given LWBS events occurred throughout May 2025
        # When I request detailed LWBS analysis in the monthly report
        self.page.get_by_test_id("request-lwbs-analysis").click()

        # Then the system provides comprehensive LWBS breakdown:
        wait_for_test_id(self.page, "Time-based Patterns")
        lwbs_breakdown_rows = [
            {"label": "Time-based Patterns", "value": "LWBS rates by hour, day of week, shift"},
            {"label": "Acuity Distribution", "value": "LWBS percentage by ESI level"},
            {"label": "Wait Time Correlation", "value": "LWBS rates vs wait time thresholds"},
            {"label": "Seasonal Factors", "value": "Weather, holidays, local events impact"},
        ]
        for row in lwbs_breakdown_rows:
            assert get_text(self.page, row["label"]) == row["value"]

        # And root cause analysis is provided:
        root_cause_rows = [
            {"label": "Extended Wait Times", "value": "High"},
            {"label": "Staffing Shortages", "value": "High"},
            {"label": "Bed Availability", "value": "Medium"},
            {"label": "Triage Delays", "value": "Medium"},
        ]
        for row in root_cause_rows:
            assert get_text(self.page, row["label"]) == row["value"]

        # And specific LWBS prevention recommendations are included:
        prevention_recommendation_rows = [
            {"label": "Fast Track Protocol", "value": "High"},
            {"label": "Provider Scheduling", "value": "High"},
            {"label": "Patient Communication", "value": "Medium"},
            {"label": "Comfort Amenities", "value": "Low"},
        ]
        for row in prevention_recommendation_rows:
            assert get_text(self.page, row["label"]) == row["value"]

    def test_analyze_patient_satisfaction_trends_with_demographic_breakdown(self):
        # Given patient satisfaction surveys were collected throughout May 2025
        # When I generate detailed satisfaction analysis
        self.page.get_by_test_id("generate-satisfaction-analysis").click()

        # Then the system provides demographic-based satisfaction breakdown:
        wait_for_test_id(self.page, "Age 18-35")
        demographic_satisfaction_rows = [
            {"label": "Age 18-35", "value": "85.1%"},
            {"label": "Age 36-55", "value": "88.7%"},
            {"label": "Age 56-75", "value": "89.2%"},
            {"label": "Age 75+", "value": "86.8%"},
            {"label": "Male Patients", "value": "86.9%"},
            {"label": "Female Patients", "value": "87.7%"},
        ]
        for row in demographic_satisfaction_rows:
            assert get_text(self.page, row["label"]) == row["value"]

        # And satisfaction domain analysis by patient type:
        patient_type_overall_rows = [
            {"label": "Trauma Patients Overall", "value": "82.0%"},
            {"label": "Chest Pain Overall", "value": "90.1%"},
            {"label": "Abdominal Pain Overall", "value": "84.4%"},
            {"label": "Minor Injuries Overall", "value": "89.6%"},
        ]
        for row in patient_type_overall_rows:
            assert get_text(self.page, row["label"]) == row["value"]

        # And improvement opportunities are identified:
        improvement_opportunity_rows = [
            {"label": "Trauma Communication", "value": "82.3%"},
            {"label": "Pain Management Improvement Opportunity", "value": "79.1%"},
            {"label": "Young Adult Experience", "value": "85.1%"},
        ]
        for row in improvement_opportunity_rows:
            assert get_text(self.page, row["label"]) == row["value"]

    def test_generate_quality_and_safety_metrics_with_benchmarking(self):
        # Given quality and safety data is tracked throughout May 2025
        # When I include quality metrics in the monthly report
        self.page.get_by_test_id("include-quality-metrics").click()

        # Then the system displays comprehensive quality indicators:
        wait_for_test_id(self.page, "Medication Errors")
        quality_indicator_rows = [
            {"label": "Medication Errors", "value": "0.12%"},
            {"label": "Patient Falls", "value": "0 events"},
            {"label": "Hospital Readmissions", "value": "2.1%"},
            {"label": "Infection Control", "value": "99.8%"},
            {"label": "Adverse Events", "value": "3 events"},
        ]
        for row in quality_indicator_rows:
            assert get_text(self.page, row["label"]) == row["value"]

        # And safety event analysis is provided:
        safety_event_rows = [
            {"label": "Medication Error Count", "value": "2 events"},
            {"label": "Diagnostic Delay Count", "value": "1 event"},
            {"label": "Equipment Failure Count", "value": "0 events"},
        ]
        for row in safety_event_rows:
            assert get_text(self.page, row["label"]) == row["value"]

        # And regulatory compliance status is shown:
        compliance_status_rows = [
            {"label": "Joint Commission", "value": "Compliant"},
            {"label": "CMS Core Measures", "value": "Compliant"},
            {"label": "State Regulations", "value": "Compliant"},
        ]
        for row in compliance_status_rows:
            assert get_text(self.page, row["label"]) == row["value"]

    def test_generate_financial_and_operational_efficiency_metrics(self):
        # Given financial and operational data is available for May 2025
        # When I request comprehensive operational metrics
        self.page.get_by_test_id("request-operational-metrics").click()

        # Then the system displays efficiency indicators:
        wait_for_test_id(self.page, "Cost per Patient")
        efficiency_indicator_rows = [
            {"label": "Cost per Patient", "value": "$847"},
            {"label": "Revenue per Patient", "value": "$1,245"},
            {"label": "Staff Productivity", "value": "2.8 pts/hr"},
            {"label": "Bed Utilization", "value": "87.3%"},
            {"label": "Equipment Uptime", "value": "98.7%"},
        ]
        for row in efficiency_indicator_rows:
            assert get_text(self.page, row["label"]) == row["value"]

        # And staffing metrics are included:
        staffing_metric_rows = [
            {"label": "RN Hours per Patient", "value": "4.2 hours"},
            {"label": "Physician Coverage", "value": "1:12 ratio"},
            {"label": "Overtime Hours", "value": "3.2%"},
            {"label": "Agency Staff Usage", "value": "1.8%"},
        ]
        for row in staffing_metric_rows:
            assert get_text(self.page, row["label"]) == row["value"]

        # And productivity analysis by shift is provided:
        shift_productivity_rows = [
            {"label": "Day (7a-7p)", "value": "3.2"},
            {"label": "Evening (7p-11p)", "value": "2.8"},
            {"label": "Night (11p-7a)", "value": "1.9"},
        ]
        for row in shift_productivity_rows:
            assert get_text(self.page, row["label"]) == row["value"]

    def test_generate_executive_summary_with_actionable_recommendations(self):
        # Given all performance data has been analyzed for May 2025
        # When I request the executive summary section of the report
        self.page.get_by_test_id("request-executive-summary").click()

        # Then the system provides a concise executive overview:
        wait_for_test_id(self.page, "Overall Performance")
        executive_summary_rows = [
            {"label": "Overall Performance", "value": "87% of targets met, significant improvements"},
            {"label": "Major Achievements", "value": "LOS reduction, LWBS improvement, satisfaction up"},
            {"label": "Areas of Concern", "value": "Night shift efficiency, young adult satisfaction"},
            {"label": "Financial Impact", "value": "$2.1M revenue, $53 cost reduction per patient"},
        ]
        for row in executive_summary_rows:
            assert get_text(self.page, row["label"]) == row["value"]

        # And specific actionable recommendations are provided:
        actionable_recommendation_rows = [
            {"label": "Implement fast-track for ESI 4-5", "value": "15% LOS reduction"},
            {"label": "Night shift staffing optimization", "value": "10% efficiency ↗"},
            {"label": "Young adult communication program", "value": "3% satisfaction ↗"},
            {"label": "Comfort amenity upgrades", "value": "2% satisfaction ↗"},
        ]
        for row in actionable_recommendation_rows:
            assert get_text(self.page, row["label"]) == row["value"]

        # And ROI analysis for recommendations is included:
        roi_analysis_rows = [
            {"label": "$125K (Fast Track)", "value": "$340K"},
            {"label": "$85K (Staffing)", "value": "$220K"},
            {"label": "$45K (Communication)", "value": "$95K"},
        ]
        for row in roi_analysis_rows:
            assert get_text(self.page, row["label"]) == row["value"]

        # And next month's focus areas are identified for tracking progress
        focus_areas = self.page.get_by_test_id("next-month-focus-area").all()
        assert len(focus_areas) > 0
