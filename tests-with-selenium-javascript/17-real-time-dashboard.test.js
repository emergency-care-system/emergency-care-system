// Selenium WebDriver + Mocha test for
// spec/features/17-real-time-dashboard.feature
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see support/fields.js) and the shared
// data-testid contract in support/login.js (login-identity, login-submit,
// app-root).

import { strict as assert } from 'assert';
import { buildDriver } from './support/build-driver.js';
import { login, verifySystemIsOperational } from './support/login.js';
import { getText, waitForTestId } from './support/fields.js';
import { By } from 'selenium-webdriver';

describe('Feature: Real-time Dashboard', function () {
  this.timeout(20000);
  let driver;

  before(async () => {
    driver = await buildDriver();
  });

  after(async () => {
    await driver.quit();
  });

  beforeEach(async () => {
    // Background:
    //   Given the ED management system is operational
    //   And I am logged in as "Charge Nurse Williams"
    //   And the real-time dashboard module is active (assumed pre-seeded test data)
    //   And automatic data refresh is enabled at 30-second intervals (assumed pre-seeded test data)
    //   And all data sources are connected and synchronized (assumed pre-seeded test data)
    await verifySystemIsOperational(driver);
    await login(driver, 'Charge Nurse Williams');

    const featureNavLink = await waitForTestId(driver, 'Nav Real-time Dashboard');
    await featureNavLink.click();
    await waitForTestId(driver, 'Real-time Dashboard Panel');
  });

  it('Display comprehensive department status on main dashboard', async () => {
    // Given it is 14:30 on a busy Tuesday afternoon (assumed pre-seeded test data)
    // And the ED has the following current status: (assumed pre-seeded test data)
    // When I access the main dashboard
    await driver.findElement(By.css('[data-testid="main-dashboard-nav"]')).click();

    // Then the system displays the current patient census:
    await waitForTestId(driver, 'Patient Census Display');
    const censusRows = [
      { label: 'Total Patients', value: '18 (90% capacity) - Yellow indicator' },
      { label: 'Admitted Patients', value: '14 currently in beds' },
      { label: 'Waiting Patients', value: '12 in triage queue - Red alert' },
      { label: 'Discharged Today', value: '23 patients processed' },
    ];
    for (const row of censusRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }

    // And the system shows average wait times:
    const waitTimeRows = [
      { label: 'Triage to Bed', value: '45 minutes' },
      { label: 'Bed to Provider', value: '25 minutes' },
      { label: 'Provider to Discharge', value: '120 minutes' },
      { label: 'Total ED Length of Stay', value: '190 minutes' },
    ];
    for (const row of waitTimeRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }

    // And bed availability is displayed with visual indicators:
    const bedStatusRows = [
      { label: 'Available Clean', value: '2' },
      { label: 'Needs Cleaning', value: '2' },
      { label: 'Occupied', value: '16' },
      { label: 'Out of Service', value: '0' },
    ];
    for (const row of bedStatusRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }

    // And staff assignments are shown in real-time:
    const staffAssignmentRows = [
      { label: 'Dr. Smith', value: '3 patients' },
      { label: 'Dr. Johnson', value: '4 patients' },
      { label: 'Nurse Martinez', value: '5 patients' },
      { label: 'Nurse Chen', value: '4 patients' },
    ];
    for (const row of staffAssignmentRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }
  });

  it('Monitor real-time updates with 30-second refresh cycle', async () => {
    // Given the dashboard is displaying current data at 15:00:00 (assumed pre-seeded test data)
    // And automatic refresh is set to 30-second intervals (assumed pre-seeded test data)
    // When 30 seconds elapse and new data becomes available: (assumed simulated by test fixture data)

    // Then the dashboard automatically updates at 15:00:30:
    const updatedMetricRows = [
      { label: 'Patient Count', value: '19 (95% capacity) - Red indicator' },
      { label: 'Bed Availability', value: '1 available - Critical level alert' },
      { label: 'Wait Time Alert', value: 'Triage wait exceeds 45 min threshold' },
      { label: 'Staff Status', value: 'Dr. Smith now unavailable' },
    ];
    for (const row of updatedMetricRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }

    // And visual indicators reflect the changes:
    const visualUpdateRows = [
      { label: 'Capacity Indicator', value: 'Yellow → Red (approaching full capacity)' },
      { label: 'Bed Status Alert', value: 'New warning for low bed availability' },
      { label: 'Provider Icon', value: 'Dr. Smith icon changes to "busy" status' },
    ];
    for (const row of visualUpdateRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }

    // And the last update timestamp shows "Updated: 15:00:30"
    const lastUpdateTimestamp = await getText(driver, 'Last Update Timestamp');
    assert.strictEqual(lastUpdateTimestamp, 'Updated: 15:00:30');

    // And critical alerts are highlighted with flashing indicators
    const criticalAlertIndicator = await waitForTestId(driver, 'Critical Alert Indicator');
    assert.ok(await criticalAlertIndicator.isDisplayed());
  });

  it('Display priority alerts and notifications on dashboard', async () => {
    // Given the dashboard is actively monitoring department status
    // When critical conditions occur requiring immediate attention: (assumed simulated by test fixture data)

    // Then the dashboard displays prominent alert notifications:
    const alertPriorityRows = [
      { label: 'CRITICAL', value: '🚨 Red flashing banner at top of screen' },
      { label: 'HIGH', value: '🟠 Orange banner with urgent icon' },
      { label: 'MEDIUM', value: '🟡 Yellow notification strip' },
      { label: 'INFO', value: '🔵 Blue informational indicator' },
    ];
    for (const row of alertPriorityRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }

    // And specific alert details are shown:
    const alertDetailRows = [
      { label: 'Capacity Critical', value: 'ED at 95% capacity - Consider diversion' },
      { label: 'Wait Time Excessive', value: 'Triage wait: 65 min - Expedite process' },
      { label: 'ESI 1 Patient', value: 'Critical patient waiting - Bed needed' },
      { label: 'Staffing Concern', value: 'Provider ratio 1:5 - Additional coverage needed' },
    ];
    for (const row of alertDetailRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }

    // And alert acknowledgment is required for critical notifications
    const alertAcknowledgment = await waitForTestId(driver, 'Alert Acknowledgment Required');
    assert.ok(await alertAcknowledgment.isDisplayed());

    // And alert history is maintained for trend analysis
    const alertHistoryEntries = await driver.findElements(By.css('[data-testid="alert-history-entry"]'));
    assert.ok(alertHistoryEntries.length > 0);
  });

  it('Monitor specific patient flow metrics and trends', async () => {
    // Given I need to track detailed patient flow performance
    // When I access the detailed metrics section of the dashboard
    await driver.findElement(By.css('[data-testid="detailed-metrics-nav"]')).click();

    // Then the system displays comprehensive flow metrics:
    const flowMetricRows = [
      { label: 'Patients per Hour', value: '3.2 arrivals' },
      { label: 'Discharge Rate', value: '2.1 per hour' },
      { label: 'Bed Turnover Time', value: '35 minutes' },
      { label: 'Left Without Being Seen', value: '2 patients' },
    ];
    for (const row of flowMetricRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }

    // And trending graphs show patterns over time:
    const trendGraphRows = [
      { label: 'Census Trend', value: '24 hours' },
      { label: 'Wait Time Trend', value: '12 hours' },
      { label: 'Capacity Utilization', value: '7 days' },
      { label: 'Provider Productivity', value: 'Shift' },
    ];
    for (const row of trendGraphRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }

    // And predictive indicators show projected status:
    const predictiveIndicatorRows = [
      { label: 'Peak Time Prediction', value: 'Expected surge at 18:00-20:00' },
      { label: 'Capacity Projection', value: 'Will reach 100% capacity in 90 minutes' },
      { label: 'Staffing Needs', value: 'Additional provider needed by 16:00' },
    ];
    for (const row of predictiveIndicatorRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }
  });

  it('Customize dashboard view based on role and preferences', async () => {
    // Given I have charge nurse privileges and preferences set
    // When I access my personalized dashboard view
    await driver.findElement(By.css('[data-testid="personalized-dashboard-nav"]')).click();

    // Then the system displays role-specific information:
    const dashboardSectionRows = [
      { label: 'Resource Management', value: 'Bed status, staffing levels, equipment' },
      { label: 'Quality Metrics', value: 'Wait times, satisfaction scores, safety' },
      { label: 'Operational Alerts', value: 'Capacity issues, workflow bottlenecks' },
      { label: 'Staff Coordination', value: 'Break schedules, assignments, coverage' },
    ];
    for (const row of dashboardSectionRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }

    // And customizable widgets show preferred metrics:
    const widgetConfigurationRows = [
      { label: 'Bed Management Grid', value: 'Color-coded bed status with room numbers' },
      { label: 'Provider Status Board', value: 'Real-time availability and patient load' },
      { label: 'Queue Management', value: 'ESI-sorted patient list with wait times' },
      { label: 'Performance KPIs', value: 'Key metrics with targets and trends' },
    ];
    for (const row of widgetConfigurationRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }

    // And I can modify the layout and priority of information displayed
    const layoutEditor = await waitForTestId(driver, 'Layout Editor');
    assert.ok(await layoutEditor.isDisplayed());

    // And the system saves my preferences for future sessions
    const preferencesSavedStatus = await getText(driver, 'Preferences Saved Status');
    assert.match(preferencesSavedStatus, /saved/i);
  });

  it('Display emergency and crisis mode indicators', async () => {
    // Given the ED is operating under normal conditions
    // When emergency conditions trigger crisis mode: (assumed simulated by test fixture data)

    // Then the dashboard displays crisis mode indicators:
    const crisisDisplayRows = [
      { label: 'Mode Banner', value: 'Red "CRISIS MODE ACTIVE" across top' },
      { label: 'Protocol Status', value: 'Active emergency protocols listed' },
      { label: 'Resource Allocation', value: 'Special staffing and bed assignments' },
      { label: 'Communication Center', value: 'Emergency contact information prominent' },
    ];
    for (const row of crisisDisplayRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }

    // And specialized crisis metrics are shown:
    const crisisMetricRows = [
      { label: 'Response Teams', value: 'Available emergency response personnel' },
      { label: 'Special Equipment', value: 'Crisis supplies and equipment status' },
      { label: 'External Coordination', value: 'Communication with EMS, other hospitals' },
      { label: 'Surge Capacity', value: 'Additional beds and overflow areas' },
    ];
    for (const row of crisisMetricRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }

    // And normal operations metrics are supplemented with crisis-specific data
    const crisisSpecificData = await waitForTestId(driver, 'Crisis Specific Data');
    assert.ok(await crisisSpecificData.isDisplayed());
  });

  it('Integrate with mobile devices for dashboard access', async () => {
    // Given I need to monitor the dashboard while mobile in the department
    // When I access the dashboard on my mobile device
    await login(driver, 'Charge Nurse Williams', { mobile: true });

    // Then the system provides a mobile-optimized view:
    const mobileFeatureRows = [
      { label: 'Summary Cards', value: 'Key metrics in swipeable card format' },
      { label: 'Alert Notifications', value: 'Push notifications for critical alerts' },
      { label: 'Quick Actions', value: 'Rapid access to common charge nurse tasks' },
      { label: 'Touch Interface', value: 'Optimized for touch navigation' },
    ];
    for (const row of mobileFeatureRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }

    // And essential information is prioritized for small screen:
    const priorityDisplayRows = [
      { label: 'Critical Alerts', value: 'Top of screen with prominent notification' },
      { label: 'Bed Status Summary', value: 'Visual grid with color-coded indicators' },
      { label: 'Staff Availability', value: 'Provider status with quick contact options' },
      { label: 'Key Metrics', value: 'Current census, wait times, alerts' },
    ];
    for (const row of priorityDisplayRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }

    // And synchronization occurs in real-time between desktop and mobile views
    const syncStatus = await getText(driver, 'Sync Status');
    assert.match(syncStatus, /real-time/i);

    // And offline capability maintains last-known status when connectivity is lost
    const offlineStatusIndicator = await waitForTestId(driver, 'Offline Status Indicator');
    assert.ok(await offlineStatusIndicator.isDisplayed());
  });

  it('Historical data comparison and trend analysis', async () => {
    // Given I want to compare current performance to historical patterns
    // When I access the trend analysis section of the dashboard
    await driver.findElement(By.css('[data-testid="trend-analysis-nav"]')).click();

    // Then the system displays comparative data:
    const comparisonTypeRows = [
      { label: 'Same Day Last Week', value: 'Tuesday to Tuesday comparison' },
      { label: 'Monthly Average', value: 'Current day vs monthly average' },
      { label: 'Seasonal Patterns', value: 'Year-over-year seasonal comparison' },
      { label: 'Shift Comparisons', value: 'Day vs evening vs night shift metrics' },
    ];
    for (const row of comparisonTypeRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }

    // And performance benchmarks are shown:
    const benchmarkRows = [
      { label: 'Internal Targets', value: 'Hospital-specific performance goals' },
      { label: 'Industry Standards', value: 'National ED performance benchmarks' },
      { label: 'Peer Comparison', value: 'Similar-sized ED performance data' },
    ];
    for (const row of benchmarkRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }

    // And variance analysis highlights significant deviations:
    const varianceAlertRows = [
      { label: 'Significant Increase', value: '>20% above normal pattern' },
      { label: 'Significant Decrease', value: '>15% below expected performance' },
      { label: 'Unusual Pattern', value: 'Unexpected trends or anomalies' },
    ];
    for (const row of varianceAlertRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }

    // And recommendations are provided for performance improvement
    const recommendations = await driver.findElements(By.css('[data-testid="performance-recommendation"]'));
    assert.ok(recommendations.length > 0);
  });
});
