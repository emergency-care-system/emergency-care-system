// Playwright test for spec/features/18-performance-metrics.feature
// (equivalent to tests-with-selenium-javascript/18-performance-metrics.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see support/fields.js) and the shared
// data-testid contract in support/login.js (login-identity, login-submit,
// app-root).

import { chromium, test } from '@playwright/test';
import { strict as assert } from 'assert';
import { login, verifySystemIsOperational } from './support/login.js';
import { fillField, getText, waitForTestId } from './support/fields.js';

test.describe.configure({ mode: 'serial' });

test.describe('Feature: Performance Metrics', () => {
  let browser;
  let page;

  test.beforeAll(async () => {
    browser = await chromium.launch();
    page = await browser.newPage();
  });

  test.afterAll(async () => {
    await browser.close();
  });

  test.beforeEach(async () => {
    // Background:
    //   Given the emergency care system is operational
    //   And I am logged in as "ED Manager Thompson"
    //   And the reporting module has access to historical data (assumed pre-seeded test data)
    //   And the performance metrics calculation engine is enabled (assumed pre-seeded test data)
    //   And patient satisfaction data is integrated from survey systems (assumed pre-seeded test data)
    await verifySystemIsOperational(page);
    await login(page, 'ED Manager Thompson');

    const featureNavLink = await waitForTestId(page, 'Nav Performance Metrics');
    await featureNavLink.click();
    await waitForTestId(page, 'Performance Metrics Panel');
  });

  test('Generate comprehensive monthly performance report', async () => {
    // Given it is the first day of the new month (assumed pre-seeded test data)
    // And I need to review performance data for the previous month (May 2025) (assumed pre-seeded test data)
    // And the system has collected data from May 1-31, 2025 (assumed pre-seeded test data)
    // When I run the monthly performance report for May 2025
    await fillField(page, 'Report Month', 'May 2025');
    await page.getByTestId('generate-monthly-report').click();

    // Then the system generates comprehensive metrics including:
    await waitForTestId(page, 'Core Metric Category Report');
    const coreMetricCategoryRows = [
      { label: 'Length of Stay', value: 'Average, median, 95th percentile by ESI' },
      { label: 'LWBS Rates', value: 'Left without being seen percentages' },
      { label: 'Patient Satisfaction', value: 'Overall scores and domain-specific ratings' },
      { label: 'Throughput Metrics', value: 'Door-to-provider, bed turnaround times' },
      { label: 'Quality Indicators', value: 'Safety events, readmission rates' },
    ];
    for (const row of coreMetricCategoryRows) {
      assert.strictEqual(await getText(page, row.label), row.value);
    }

    // And the average length of stay metrics show:
    const averageLosRows = [
      { label: 'ESI 1 (Critical)', value: '180 minutes' },
      { label: 'ESI 2 (High Priority)', value: '165 minutes' },
      { label: 'ESI 3 (Urgent)', value: '145 minutes' },
      { label: 'ESI 4 (Less Urgent)', value: '95 minutes' },
      { label: 'ESI 5 (Non-urgent)', value: '75 minutes' },
      { label: 'Overall Average', value: '132 minutes' },
    ];
    for (const row of averageLosRows) {
      assert.strictEqual(await getText(page, row.label), row.value);
    }

    // And left without being seen (LWBS) rates are calculated:
    const lwbsRateRows = [
      { label: 'Overall LWBS Rate', value: '3.2%' },
      { label: 'ESI 3 LWBS Rate', value: '4.1%' },
      { label: 'ESI 4 LWBS Rate', value: '5.8%' },
      { label: 'ESI 5 LWBS Rate', value: '8.2%' },
      { label: 'Peak Hours LWBS', value: '6.1%' },
    ];
    for (const row of lwbsRateRows) {
      assert.strictEqual(await getText(page, row.label), row.value);
    }

    // And patient satisfaction scores are displayed:
    const satisfactionScoreRows = [
      { label: 'Overall Satisfaction', value: '87.3%' },
      { label: 'Communication', value: '89.1%' },
      { label: 'Pain Management', value: '84.7%' },
      { label: 'Staff Responsiveness', value: '88.9%' },
      { label: 'Cleanliness', value: '92.4%' },
      { label: 'Discharge Process', value: '86.2%' },
    ];
    for (const row of satisfactionScoreRows) {
      assert.strictEqual(await getText(page, row.label), row.value);
    }
  });

  test('Compare current month performance to historical benchmarks', async () => {
    // Given historical performance data is available for trending analysis
    // When I generate the monthly report with comparative analysis
    await page.getByTestId('generate-comparative-report').click();

    // Then the system displays month-over-month comparisons:
    await waitForTestId(page, 'Average Length of Stay Comparison');
    const monthOverMonthRows = [
      { label: 'Average Length of Stay Comparison', value: '132 min' },
      { label: 'LWBS Rate Comparison', value: '3.2%' },
      { label: 'Patient Satisfaction Comparison', value: '87.3%' },
      { label: 'Door-to-Provider Comparison', value: '35 min' },
      { label: 'Bed Turnaround Comparison', value: '28 min' },
    ];
    for (const row of monthOverMonthRows) {
      assert.strictEqual(await getText(page, row.label), row.value);
    }

    // And year-over-year comparisons are shown:
    const yearOverYearRows = [
      { label: 'Average LOS YoY', value: '132 min' },
      { label: 'LWBS Rate YoY', value: '3.2%' },
      { label: 'Patient Volume YoY', value: '2,847 pts' },
      { label: 'Satisfaction YoY', value: '87.3%' },
    ];
    for (const row of yearOverYearRows) {
      assert.strictEqual(await getText(page, row.label), row.value);
    }

    // And statistical significance testing results are included:
    const statisticalSignificanceRows = [
      { label: 'Length of Stay Significance', value: '0.003' },
      { label: 'LWBS Rate Significance', value: '0.012' },
      { label: 'Satisfaction Significance', value: '0.001' },
    ];
    for (const row of statisticalSignificanceRows) {
      assert.strictEqual(await getText(page, row.label), row.value);
    }
  });

  test('Generate detailed LWBS analysis with root cause identification', async () => {
    // Given LWBS events occurred throughout May 2025
    // When I request detailed LWBS analysis in the monthly report
    await page.getByTestId('request-lwbs-analysis').click();

    // Then the system provides comprehensive LWBS breakdown:
    await waitForTestId(page, 'Time-based Patterns');
    const lwbsBreakdownRows = [
      { label: 'Time-based Patterns', value: 'LWBS rates by hour, day of week, shift' },
      { label: 'Acuity Distribution', value: 'LWBS percentage by ESI level' },
      { label: 'Wait Time Correlation', value: 'LWBS rates vs wait time thresholds' },
      { label: 'Seasonal Factors', value: 'Weather, holidays, local events impact' },
    ];
    for (const row of lwbsBreakdownRows) {
      assert.strictEqual(await getText(page, row.label), row.value);
    }

    // And root cause analysis is provided:
    const rootCauseRows = [
      { label: 'Extended Wait Times', value: 'High' },
      { label: 'Staffing Shortages', value: 'High' },
      { label: 'Bed Availability', value: 'Medium' },
      { label: 'Triage Delays', value: 'Medium' },
    ];
    for (const row of rootCauseRows) {
      assert.strictEqual(await getText(page, row.label), row.value);
    }

    // And specific LWBS prevention recommendations are included:
    const preventionRecommendationRows = [
      { label: 'Fast Track Protocol', value: 'High' },
      { label: 'Provider Scheduling', value: 'High' },
      { label: 'Patient Communication', value: 'Medium' },
      { label: 'Comfort Amenities', value: 'Low' },
    ];
    for (const row of preventionRecommendationRows) {
      assert.strictEqual(await getText(page, row.label), row.value);
    }
  });

  test('Analyze patient satisfaction trends with demographic breakdown', async () => {
    // Given patient satisfaction surveys were collected throughout May 2025
    // When I generate detailed satisfaction analysis
    await page.getByTestId('generate-satisfaction-analysis').click();

    // Then the system provides demographic-based satisfaction breakdown:
    await waitForTestId(page, 'Age 18-35');
    const demographicSatisfactionRows = [
      { label: 'Age 18-35', value: '85.1%' },
      { label: 'Age 36-55', value: '88.7%' },
      { label: 'Age 56-75', value: '89.2%' },
      { label: 'Age 75+', value: '86.8%' },
      { label: 'Male Patients', value: '86.9%' },
      { label: 'Female Patients', value: '87.7%' },
    ];
    for (const row of demographicSatisfactionRows) {
      assert.strictEqual(await getText(page, row.label), row.value);
    }

    // And satisfaction domain analysis by patient type:
    const patientTypeOverallRows = [
      { label: 'Trauma Patients Overall', value: '82.0%' },
      { label: 'Chest Pain Overall', value: '90.1%' },
      { label: 'Abdominal Pain Overall', value: '84.4%' },
      { label: 'Minor Injuries Overall', value: '89.6%' },
    ];
    for (const row of patientTypeOverallRows) {
      assert.strictEqual(await getText(page, row.label), row.value);
    }

    // And improvement opportunities are identified:
    const improvementOpportunityRows = [
      { label: 'Trauma Communication', value: '82.3%' },
      { label: 'Pain Management Improvement Opportunity', value: '79.1%' },
      { label: 'Young Adult Experience', value: '85.1%' },
    ];
    for (const row of improvementOpportunityRows) {
      assert.strictEqual(await getText(page, row.label), row.value);
    }
  });

  test('Generate quality and safety metrics with benchmarking', async () => {
    // Given quality and safety data is tracked throughout May 2025
    // When I include quality metrics in the monthly report
    await page.getByTestId('include-quality-metrics').click();

    // Then the system displays comprehensive quality indicators:
    await waitForTestId(page, 'Medication Errors');
    const qualityIndicatorRows = [
      { label: 'Medication Errors', value: '0.12%' },
      { label: 'Patient Falls', value: '0 events' },
      { label: 'Hospital Readmissions', value: '2.1%' },
      { label: 'Infection Control', value: '99.8%' },
      { label: 'Adverse Events', value: '3 events' },
    ];
    for (const row of qualityIndicatorRows) {
      assert.strictEqual(await getText(page, row.label), row.value);
    }

    // And safety event analysis is provided:
    const safetyEventRows = [
      { label: 'Medication Error Count', value: '2 events' },
      { label: 'Diagnostic Delay Count', value: '1 event' },
      { label: 'Equipment Failure Count', value: '0 events' },
    ];
    for (const row of safetyEventRows) {
      assert.strictEqual(await getText(page, row.label), row.value);
    }

    // And regulatory compliance status is shown:
    const complianceStatusRows = [
      { label: 'Joint Commission', value: 'Compliant' },
      { label: 'CMS Core Measures', value: 'Compliant' },
      { label: 'State Regulations', value: 'Compliant' },
    ];
    for (const row of complianceStatusRows) {
      assert.strictEqual(await getText(page, row.label), row.value);
    }
  });

  test('Generate financial and operational efficiency metrics', async () => {
    // Given financial and operational data is available for May 2025
    // When I request comprehensive operational metrics
    await page.getByTestId('request-operational-metrics').click();

    // Then the system displays efficiency indicators:
    await waitForTestId(page, 'Cost per Patient');
    const efficiencyIndicatorRows = [
      { label: 'Cost per Patient', value: '$847' },
      { label: 'Revenue per Patient', value: '$1,245' },
      { label: 'Staff Productivity', value: '2.8 pts/hr' },
      { label: 'Bed Utilization', value: '87.3%' },
      { label: 'Equipment Uptime', value: '98.7%' },
    ];
    for (const row of efficiencyIndicatorRows) {
      assert.strictEqual(await getText(page, row.label), row.value);
    }

    // And staffing metrics are included:
    const staffingMetricRows = [
      { label: 'RN Hours per Patient', value: '4.2 hours' },
      { label: 'Physician Coverage', value: '1:12 ratio' },
      { label: 'Overtime Hours', value: '3.2%' },
      { label: 'Agency Staff Usage', value: '1.8%' },
    ];
    for (const row of staffingMetricRows) {
      assert.strictEqual(await getText(page, row.label), row.value);
    }

    // And productivity analysis by shift is provided:
    const shiftProductivityRows = [
      { label: 'Day (7a-7p)', value: '3.2' },
      { label: 'Evening (7p-11p)', value: '2.8' },
      { label: 'Night (11p-7a)', value: '1.9' },
    ];
    for (const row of shiftProductivityRows) {
      assert.strictEqual(await getText(page, row.label), row.value);
    }
  });

  test('Generate executive summary with actionable recommendations', async () => {
    // Given all performance data has been analyzed for May 2025
    // When I request the executive summary section of the report
    await page.getByTestId('request-executive-summary').click();

    // Then the system provides a concise executive overview:
    await waitForTestId(page, 'Overall Performance');
    const executiveSummaryRows = [
      { label: 'Overall Performance', value: '87% of targets met, significant improvements' },
      { label: 'Major Achievements', value: 'LOS reduction, LWBS improvement, satisfaction up' },
      { label: 'Areas of Concern', value: 'Night shift efficiency, young adult satisfaction' },
      { label: 'Financial Impact', value: '$2.1M revenue, $53 cost reduction per patient' },
    ];
    for (const row of executiveSummaryRows) {
      assert.strictEqual(await getText(page, row.label), row.value);
    }

    // And specific actionable recommendations are provided:
    const actionableRecommendationRows = [
      { label: 'Implement fast-track for ESI 4-5', value: '15% LOS reduction' },
      { label: 'Night shift staffing optimization', value: '10% efficiency ↗' },
      { label: 'Young adult communication program', value: '3% satisfaction ↗' },
      { label: 'Comfort amenity upgrades', value: '2% satisfaction ↗' },
    ];
    for (const row of actionableRecommendationRows) {
      assert.strictEqual(await getText(page, row.label), row.value);
    }

    // And ROI analysis for recommendations is included:
    const roiAnalysisRows = [
      { label: '$125K (Fast Track)', value: '$340K' },
      { label: '$85K (Staffing)', value: '$220K' },
      { label: '$45K (Communication)', value: '$95K' },
    ];
    for (const row of roiAnalysisRows) {
      assert.strictEqual(await getText(page, row.label), row.value);
    }

    // And next month's focus areas are identified for tracking progress
    const focusAreas = await page.getByTestId('next-month-focus-area').all();
    assert.ok(focusAreas.length > 0);
  });
});
