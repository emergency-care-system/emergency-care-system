// Selenium WebDriver + Mocha test for
// spec/features/19-mass-casualty-activation.feature
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

describe('Feature: Mass Casualty Activation', function () {
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
    //   And the mass casualty incident (MCI) module is available (assumed pre-seeded test data)
    //   And emergency contact systems are enabled (assumed pre-seeded test data)
    //   And surge capacity protocols are configured (assumed pre-seeded test data)
    await verifySystemIsOperational(driver);
    await login(driver, 'Charge Nurse Williams');

    const featureNavLink = await waitForTestId(driver, 'Nav Mass Casualty Activation');
    await featureNavLink.click();
    await waitForTestId(driver, 'Mass Casualty Activation Panel');
  });

  it('Activate mass casualty protocol for multi-vehicle accident', async () => {
    // Given it is 16:30 on a Friday afternoon (assumed pre-seeded test data)
    // And normal ED operations are in progress with 12 patients currently in the department (assumed pre-seeded test data)
    // And EMS reports a multi-vehicle accident with 8+ casualties en route (assumed pre-seeded test data)
    // When I receive notification of the mass casualty incident: (assumed simulated by test fixture data)
    // And I activate the disaster protocol in the system
    await driver.findElement(By.css('[data-testid="activate-disaster-protocol"]')).click();

    // Then the system immediately switches to surge capacity mode:
    await waitForTestId(driver, 'Mode Indicator');
    const systemChangeRows = [
      { label: 'Mode Indicator', value: '"MASS CASUALTY ACTIVE" banner displayed' },
      { label: 'Interface Switch', value: 'MCI-specific workflows activated' },
      { label: 'Normal Operations', value: 'Routine tasks suspended/deprioritized' },
      { label: 'Resource Allocation', value: 'Emergency resource management enabled' },
      { label: 'Communication Mode', value: 'Critical alerts and notifications active' },
    ];
    for (const row of systemChangeRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }

    // And additional staff are automatically alerted:
    const staffAlertRows = [
      { label: 'Off-duty Physicians', value: 'SMS, Phone call' },
      { label: 'Off-duty Nurses', value: 'SMS, Phone call' },
      { label: 'Surgical Team', value: 'Overhead page, SMS' },
      { label: 'Lab/Radiology', value: 'System alert, Phone' },
      { label: 'Administration', value: 'Phone call, SMS' },
      { label: 'Security', value: 'Radio, Overhead page' },
    ];
    for (const row of staffAlertRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }

    // And rapid registration workflows are created:
    const registrationFeatureRows = [
      { label: 'Patient Identification', value: 'Sequential numbering: MCI-001, MCI-002' },
      { label: 'Triage Tags', value: 'Color-coded electronic tags' },
      { label: 'Minimal Data Entry', value: 'Name, age, chief complaint only' },
      { label: 'Family Notification', value: 'Automated family alert system' },
      { label: 'Tracking Board', value: 'Real-time patient status dashboard' },
    ];
    for (const row of registrationFeatureRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }
  });

  it('Configure surge capacity with bed and resource expansion', async () => {
    // Given the mass casualty protocol has been activated
    // And normal bed capacity is 20 beds
    // When the system enters surge capacity mode

    // Then additional treatment areas are activated:
    await waitForTestId(driver, 'Hallway Beds');
    const surgeAreaRows = [
      { label: 'Hallway Beds', value: '+6 treatment spaces' },
      { label: 'Procedure Rooms', value: '+3 converted spaces' },
      { label: 'Observation Area', value: '+8 holding spaces' },
      { label: 'Waiting Room Triage', value: '+4 assessment areas' },
      { label: 'Ambulatory Care', value: '+10 walking wounded' },
    ];
    for (const row of surgeAreaRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }

    // And resource allocation is optimized for mass casualty:
    const resourceAllocationRows = [
      { label: 'Trauma Bays', value: 'All 4 activated' },
      { label: 'Operating Rooms', value: '3 rooms on standby' },
      { label: 'Ventilators', value: '8 total (5 from ICU)' },
      { label: 'Blood Products', value: 'Massive transfusion protocol' },
      { label: 'Medication Carts', value: '5 carts deployed' },
    ];
    for (const row of resourceAllocationRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }

    // And staffing ratios are adjusted for emergency operations:
    const staffingRatioRows = [
      { label: 'Physicians', value: '1:12 patients' },
      { label: 'Nurses', value: '1:6 patients' },
      { label: 'Support Staff', value: 'Double coverage' },
    ];
    for (const row of staffingRatioRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }
  });

  it('Implement rapid patient registration and triage workflow', async () => {
    // Given mass casualty mode is active
    // And the first ambulance arrives with 3 critical patients
    // When EMS brings patients to the ED

    // Then the rapid registration workflow is initiated:
    await waitForTestId(driver, 'Patient Arrival');
    const registrationStepRows = [
      { label: 'Patient Arrival', value: 'Immediate tag assignment: MCI-001, 002, 003' },
      { label: 'Triage Assessment', value: 'START triage protocol applied' },
      { label: 'Electronic Tagging', value: 'Color-coded digital tags assigned' },
      { label: 'Minimal Documentation', value: 'Name, estimated age, mechanism of injury' },
      { label: 'Bed Assignment', value: 'Automatic assignment by acuity' },
    ];
    for (const row of registrationStepRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }

    // And electronic triage tags are applied with color coding:
    const triageColorRows = [
      { label: 'Red (Immediate)', value: 'MCI-001 → Trauma Bay 1' },
      { label: 'Yellow (Delayed)', value: 'MCI-002 → Surge Bed 3' },
      { label: 'Green (Minor)', value: 'MCI-003 → Ambulatory Area' },
      { label: 'Black (Deceased)', value: 'Morgue coordination' },
    ];
    for (const row of triageColorRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }

    // And family notification systems are activated:
    const notificationProcessRows = [
      { label: 'Emergency Contacts', value: 'Auto-dial from patient personal effects' },
      { label: 'Public Information', value: 'Hospital hotline number broadcasted' },
      { label: 'Media Coordination', value: 'Incident command liaison activated' },
      { label: 'Social Services', value: 'Family support team mobilized' },
    ];
    for (const row of notificationProcessRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }
  });

  it('Coordinate with external emergency services and hospitals', async () => {
    // Given a major mass casualty incident is in progress
    // And local EMS is overwhelmed with the response
    // When the system activates external coordination protocols

    // Then inter-facility communication is established:
    await waitForTestId(driver, 'EMS Command Center');
    const communicationChannelRows = [
      { label: 'EMS Command Center', value: 'Patient distribution and transport updates' },
      { label: 'Other Area Hospitals', value: 'Bed availability and transfer coordination' },
      { label: 'Air Medical Services', value: 'Helicopter transport for critical patients' },
      { label: 'Regional Trauma Centers', value: 'Specialty care transfer arrangements' },
    ];
    for (const row of communicationChannelRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }

    // And patient distribution management is activated:
    const distributionStrategyRows = [
      { label: 'Load Balancing', value: 'Distribute patients across regional facilities' },
      { label: 'Specialty Matching', value: 'Route patients to appropriate specialty care' },
      { label: 'Capacity Monitoring', value: 'Real-time bed availability tracking' },
      { label: 'Transport Coordination', value: 'Ambulance and helicopter scheduling' },
    ];
    for (const row of distributionStrategyRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }

    // And regional emergency management integration occurs:
    const integrationElementRows = [
      { label: 'Incident Command', value: 'Hospital EOC links with regional ICS' },
      { label: 'Resource Sharing', value: 'Equipment and staff sharing protocols' },
      { label: 'Information Sharing', value: 'Patient status updates to command center' },
      { label: 'Media Management', value: 'Coordinated public information releases' },
    ];
    for (const row of integrationElementRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }
  });

  it('Manage family reunification and information center', async () => {
    // Given multiple patients from the mass casualty incident are being treated
    // And families are arriving seeking information about their loved ones
    // When the family information center is activated

    // Then patient tracking and family communication systems are deployed:
    await waitForTestId(driver, 'Information Hotline');
    const familySupportSystemRows = [
      { label: 'Information Hotline', value: 'Dedicated phone line with trained staff' },
      { label: 'Family Reunification', value: 'Secure area for family waiting and updates' },
      { label: 'Patient Tracking', value: 'Real-time status board for authorized viewers' },
      { label: 'Privacy Protection', value: 'HIPAA-compliant information sharing' },
    ];
    for (const row of familySupportSystemRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }

    // And automated family notification processes are initiated:
    const notificationMethodRows = [
      { label: 'SMS Updates', value: 'Your family member is being treated safely' },
      { label: 'Phone Calls', value: 'Personal calls for critical status changes' },
      { label: 'Information Boards', value: 'General incident updates (no patient names)' },
      { label: 'Social Workers', value: 'One-on-one family support and counseling' },
    ];
    for (const row of notificationMethodRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }

    // And patient status tracking is maintained:
    const trackingElementRows = [
      { label: 'Current Location', value: 'Treatment area, OR, transferred, etc.' },
      { label: 'Medical Status', value: 'Stable, critical, treated and released' },
      { label: 'Next of Kin Contact', value: 'Verification and notification status' },
      { label: 'Discharge Planning', value: 'Expected timeline and care needs' },
    ];
    for (const row of trackingElementRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }
  });

  it('Transition from mass casualty mode back to normal operations', async () => {
    // Given the mass casualty incident has been resolved
    // And all patients are stabilized or transferred
    // And no additional casualties are expected
    // When I initiate the transition back to normal operations
    await driver.findElement(By.css('[data-testid="initiate-transition-to-normal"]')).click();

    // Then the system manages the deactivation process:
    await waitForTestId(driver, 'Incident Assessment');
    const deactivationStepRows = [
      { label: 'Incident Assessment', value: 'Review of patient outcomes and resources used' },
      { label: 'Staff Debriefing', value: 'Immediate hot wash and formal debriefing' },
      { label: 'Resource Restoration', value: 'Return equipment and supplies to normal areas' },
      { label: 'Documentation', value: 'Complete incident documentation and reports' },
    ];
    for (const row of deactivationStepRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }

    // And normal ED operations are gradually restored:
    const restorationPhaseRows = [
      { label: 'Immediate (0-30 min)', value: 'Secure scene' },
      { label: 'Short-term (30-60 min)', value: 'Resource cleanup' },
      { label: 'Medium-term (1-4 hrs)', value: 'Staff rotation' },
      { label: 'Long-term (4-24 hrs)', value: 'Full restoration' },
    ];
    for (const row of restorationPhaseRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }

    // And quality improvement activities are initiated:
    const qiActivityRows = [
      { label: 'After Action Review', value: 'Identify strengths and improvement areas' },
      { label: 'Performance Metrics', value: 'Analyze response times and patient outcomes' },
      { label: 'Protocol Updates', value: 'Revise procedures based on lessons learned' },
      { label: 'Training Needs', value: 'Identify staff training and education needs' },
    ];
    for (const row of qiActivityRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }
  });

  it('Handle mass casualty incident during shift change', async () => {
    // Given it is 19:00 during evening shift change
    // And day shift staff are preparing to leave
    // And evening shift staff are assuming duties
    // When a mass casualty incident is declared

    // Then the system manages staffing during the transition:
    await waitForTestId(driver, 'Shift Hold');
    const staffingStrategyRows = [
      { label: 'Shift Hold', value: 'Day shift staff remain for incident response' },
      { label: 'Double Coverage', value: 'Both shifts work together during surge' },
      { label: 'Incident Command', value: 'Clear leadership chain established' },
      { label: 'Communication', value: 'All staff briefed on roles and responsibilities' },
    ];
    for (const row of staffingStrategyRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }

    // And transition protocols are modified for the emergency:
    const modifiedProtocolRows = [
      { label: 'Handoff Procedures', value: 'Suspended until incident resolution' },
      { label: 'Staffing Ratios', value: 'Enhanced coverage with both shifts' },
      { label: 'Leadership Structure', value: 'Incident commander takes operational control' },
      { label: 'Documentation', value: 'Emergency documentation procedures active' },
    ];
    for (const row of modifiedProtocolRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }
  });

  it('Test mass casualty system readiness through drill', async () => {
    // Given it is a scheduled quarterly mass casualty drill
    // And the drill scenario involves a simulated building collapse with 15 casualties
    // When the drill coordinator activates the test mass casualty protocol
    await driver.findElement(By.css('[data-testid="activate-test-mci-protocol"]')).click();

    // Then the system activates in drill mode:
    await waitForTestId(driver, 'Test Mode Indicator');
    const drillFeatureRows = [
      { label: 'Test Mode Indicator', value: '"DRILL - NOT REAL EMERGENCY" displayed' },
      { label: 'Simulated Patients', value: 'Test patient records created' },
      { label: 'Staff Participation', value: 'All roles and responsibilities tested' },
      { label: 'Resource Tracking', value: 'Equipment and supplies tracked but not used' },
    ];
    for (const row of drillFeatureRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }

    // And drill performance metrics are captured:
    const drillPerformanceMetricRows = [
      { label: 'Activation Time', value: 'Time from alert to full surge capacity' },
      { label: 'Staff Response Time', value: 'Time for staff to report and assume roles' },
      { label: 'Communication Speed', value: 'Time for all notifications to be completed' },
      { label: 'Resource Deployment', value: 'Time to set up surge areas and equipment' },
    ];
    for (const row of drillPerformanceMetricRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }

    // And drill evaluation and improvement planning occurs:
    const evaluationComponentRows = [
      { label: 'Protocol Effectiveness', value: 'How well procedures worked in practice' },
      { label: 'Staff Preparedness', value: 'Knowledge and skill gaps identified' },
      { label: 'System Performance', value: 'Technology and workflow efficiency' },
      { label: 'Improvement Plans', value: 'Action items for enhancing response capabilities' },
    ];
    for (const row of evaluationComponentRows) {
      assert.strictEqual(await getText(driver, row.label), row.value);
    }
  });
});
