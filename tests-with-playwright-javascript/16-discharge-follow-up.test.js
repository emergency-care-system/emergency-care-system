// Playwright test for spec/features/16-discharge-follow-up.feature
// (equivalent to tests-with-selenium-javascript/16-discharge-follow-up.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see support/fields.js) and the shared
// data-testid contract in support/login.js (login-identity, login-submit,
// app-root).

import { chromium, test } from '@playwright/test';
import { strict as assert } from 'assert';
import { login, verifySystemIsOperational } from './support/login.js';
import { getText, waitForTestId } from './support/fields.js';

// All scenarios in this file share one page (like the Selenium suite shares
// one WebDriver per file), since later scenarios rely on data earlier
// scenarios registered persisting in the app's localStorage -- so they must
// run in file order, not in parallel.
test.describe.configure({ mode: 'serial' });

test.describe('Feature: Discharge Follow-up', () => {
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
    //   Given the ED management system is operational
    //   And the patient portal is connected and functional
    //   And the follow-up scheduling module is active
    //   And automated reminder systems are enabled
    //   And patient communication preferences are configured
    await verifySystemIsOperational(page);
    await login(page, 'a discharge coordinator');
    // The patient portal connection, follow-up scheduling module,
    // automated reminder systems, and communication preference
    // configuration are assumed to be active backend configuration
    // already in place for this environment.

    const dischargeFollowUpNavLink = await waitForTestId(page, 'Nav Discharge Follow-up');
    await dischargeFollowUpNavLink.click();
    await waitForTestId(page, 'Discharge Follow-up Panel');
  });

  test('Automatically schedule follow-up reminder for primary care', async () => {
    // Given a patient "Jennifer Martinez" has been discharged from bed "ED-8"
    // And the discharge orders include:
    //   | Follow-up Requirement | Details                                    |
    //   | Primary Care Visit    | Schedule within 3-5 days                  |
    //   | Reason for Follow-up  | UTI treatment response, medication review  |
    //   | Urgency Level         | Routine                                    |
    //   | Special Instructions  | Bring discharge paperwork and medication list |
    // And the patient has a registered primary care physician "Dr. Sarah Wilson"
    // (assumed pre-seeded test data)
    // When the discharge process is completed at 14:30
    // (assumed to have already occurred / triggered by the system)

    // Then the system automatically schedules a follow-up reminder:
    await waitForTestId(page, 'Initial Reminder');
    const followUpReminders = [
      { Type: 'Initial Reminder', Details: 'Day 2 after discharge (in 48 hours)' },
      { Type: 'Follow-up Reminder', Details: 'Day 4 after discharge if no appointment' },
      { Type: 'Final Reminder', Details: 'Day 6 after discharge (urgent)' },
      { Type: 'Reminder Methods', Details: 'Text, email, phone call' },
    ];
    for (const { Type, Details } of followUpReminders) {
      assert.strictEqual(await getText(page, Type), Details);
    }

    // And discharge instructions are automatically sent to the patient portal:
    const portalContent = [
      { Section: 'Discharge Summary', Content: 'Complete treatment summary and diagnosis' },
      { Section: 'Medication Instructions', Content: 'Prescription details and dosing schedule' },
      { Section: 'Follow-up Requirements', Content: 'Primary care appointment needed in 3-5 days' },
      { Section: 'Return Precautions', Content: 'When to seek emergency care' },
      { Section: 'Care Instructions', Content: 'Home care guidelines and activity restrictions' },
    ];
    for (const { Section, Content } of portalContent) {
      assert.strictEqual(await getText(page, Section), Content);
    }

    // And the patient receives immediate portal notification:
    const portalNotifications = [
      { Type: 'Portal Alert', Content: '📋 New discharge instructions available' },
      { Type: 'Text Message', Content: '"ED discharge complete. Check patient portal for instructions"' },
      { Type: 'Email Notification', Content: 'Detailed discharge summary with portal link' },
    ];
    for (const { Type, Content } of portalNotifications) {
      assert.strictEqual(await getText(page, Type), Content);
    }
  });

  test('Handle follow-up scheduling with multiple appointment types', async () => {
    // Given a patient "Robert Chen" is discharged with complex follow-up needs
    // And the discharge orders specify:
    //   | Follow-up Type        | Timeframe | Provider Type     | Priority  |
    //   | Primary Care         | 3 days    | Family Medicine   | High      |
    //   | Cardiology Consult   | 1 week    | Cardiologist      | Urgent    |
    //   | Lab Work Follow-up   | 5 days    | Lab/Primary Care  | Routine   |
    //   | Physical Therapy     | 2 weeks   | PT Specialist     | Routine   |
    // (assumed pre-seeded test data)
    // When the discharge process is completed
    // (assumed to have already occurred / triggered by the system)

    // Then the system creates multiple follow-up reminders:
    await waitForTestId(page, 'Primary Care Timing');
    const reminderSchedule = [
      { Type: 'Primary Care', Timing: 'Schedule within 2 days' },
      { Type: 'Cardiology', Timing: 'Schedule urgent consult' },
      { Type: 'Lab Work', Timing: 'Schedule blood draw' },
      { Type: 'Physical Therapy', Timing: 'Schedule PT evaluation' },
    ];
    for (const { Type, Timing } of reminderSchedule) {
      assert.strictEqual(await getText(page, `${Type} Timing`), Timing);
    }

    // And the patient portal receives comprehensive follow-up information:
    const portalFollowUpInfo = [
      { Section: 'Appointment Dashboard', Content: 'All required follow-ups with deadlines' },
      { Section: 'Provider Contacts', Content: 'Phone numbers and scheduling information' },
      { Section: 'Priority Indicators', Content: 'Urgent vs routine appointment labeling' },
      { Section: 'Preparation Instructions', Content: 'What to bring to each appointment' },
    ];
    for (const { Section, Content } of portalFollowUpInfo) {
      assert.strictEqual(await getText(page, Section), Content);
    }

    // And automated referrals are generated:
    const automatedReferrals = [
      { Type: 'Electronic Referral', Action: 'Sent to cardiology for urgent consult' },
      { Type: 'Lab Order', Action: 'Standing orders for follow-up labs' },
      { Type: 'PT Referral', Action: 'Physical therapy evaluation requested' },
    ];
    for (const { Type, Action } of automatedReferrals) {
      assert.strictEqual(await getText(page, Type), Action);
    }
  });

  test('Send discharge instructions to patient portal with multimedia content', async () => {
    // Given a patient "Maria Santos" was treated for "wound care management"
    // And the patient requires detailed home care instructions
    // (assumed pre-seeded test data)
    // When the discharge process includes educational materials:
    //   | Education Type        | Content Provided                           |
    //   | Wound Care Video      | Step-by-step dressing change demonstration |
    //   | Medication Guide      | Interactive dosing calculator              |
    //   | Warning Signs Chart   | Visual guide for infection symptoms        |
    //   | Activity Guidelines   | Illustrated movement restrictions          |
    // And the discharge is completed
    // (assumed to have already occurred / triggered by the system)

    // Then the patient portal receives multimedia instructions:
    await waitForTestId(page, 'Video Instructions');
    const multimediaInstructions = [
      { Type: 'Video Instructions', Material: 'Wound care demonstration (3 minutes)' },
      { Type: 'Interactive Tools', Material: 'Medication reminder scheduler' },
      { Type: 'Visual Guides', Material: 'Infection warning signs with photos' },
      { Type: 'Progress Tracking', Material: 'Healing milestone checklist' },
    ];
    for (const { Type, Material } of multimediaInstructions) {
      assert.strictEqual(await getText(page, Type), Material);
    }

    // And the patient receives learning verification:
    const learningVerification = [
      { Method: 'Video Completion', Requirement: 'Must watch wound care video fully' },
      { Method: 'Knowledge Check', Requirement: 'Brief quiz on warning signs' },
      { Method: 'Acknowledgment', Requirement: 'Confirm understanding of instructions' },
    ];
    for (const { Method, Requirement } of learningVerification) {
      assert.strictEqual(await getText(page, Method), Requirement);
    }

    // And completion tracking is recorded for quality assurance
    const completionTrackingStatus = await getText(page, 'Completion Tracking Status');
    assert.match(completionTrackingStatus, /recorded/i);
  });

  test('Handle follow-up reminders for patients without primary care physicians', async () => {
    // Given a patient "David Kim" is discharged
    // And the patient does not have an established primary care physician
    // And follow-up care is required within 5 days
    // (assumed pre-seeded test data)
    // When the discharge process is completed
    // (assumed to have already occurred / triggered by the system)

    // Then the system provides alternative follow-up options:
    await waitForTestId(page, 'Urgent Care Centers');
    const followUpOptions = [
      { Option: 'Urgent Care Centers', Details: 'List of nearby facilities with hours' },
      { Option: 'Hospital Clinic', Details: 'Available appointment slots' },
      { Option: 'Telehealth Options', Details: 'Virtual visit scheduling information' },
      { Option: 'Community Health Centers', Details: 'Low-cost provider options' },
    ];
    for (const { Option, Details } of followUpOptions) {
      assert.strictEqual(await getText(page, Option), Details);
    }

    // And enhanced reminder scheduling is activated:
    const enhancedReminders = [
      { Type: 'Daily Reminders', Frequency: 'For first 3 days after discharge' },
      { Type: 'Resource Assistance', Frequency: 'Links to find primary care providers' },
      { Type: 'Financial Counseling', Frequency: 'Information about insurance and payment' },
    ];
    for (const { Type, Frequency } of enhancedReminders) {
      assert.strictEqual(await getText(page, Type), Frequency);
    }

    // And the patient portal includes provider finding tools:
    const providerFindingTools = [
      { Tool: 'Provider Search', Functionality: 'Find doctors accepting new patients' },
      { Tool: 'Insurance Verification', Functionality: 'Check coverage for potential providers' },
      { Tool: 'Appointment Booking', Functionality: 'Direct scheduling with available providers' },
    ];
    for (const { Tool, Functionality } of providerFindingTools) {
      assert.strictEqual(await getText(page, Tool), Functionality);
    }
  });

  test('Customize follow-up based on patient communication preferences', async () => {
    // Given a patient "Lisa Brown" has specified communication preferences:
    //   | Communication Method  | Preference    | Contact Information        |
    //   | Text Messages         | Preferred     | 555-123-4567              |
    //   | Email                 | Secondary     | lisa.brown@email.com      |
    //   | Phone Calls           | Emergency Only| 555-123-4567              |
    //   | Portal Notifications  | Enabled       | Username: lbrown123       |
    // And the patient is discharged with routine follow-up requirements
    // (assumed pre-seeded test data)
    // When the discharge process triggers follow-up communications
    // (assumed to have already occurred / triggered by the system)

    // Then the system respects patient communication preferences:
    await waitForTestId(page, 'Initial Instructions Method');
    const communicationPreferences = [
      { Type: 'Initial Instructions', Method: 'Text + Portal', Content: 'Brief summary with portal link' },
      { Type: 'Follow-up Reminders', Method: 'Text Message', Content: 'Appointment reminders' },
      { Type: 'Urgent Notifications', Method: 'Phone Call', Content: 'Critical lab results only' },
      { Type: 'Educational Content', Method: 'Portal Only', Content: 'Detailed instructions and videos' },
    ];
    for (const { Type, Method, Content } of communicationPreferences) {
      assert.strictEqual(await getText(page, `${Type} Method`), Method);
      assert.strictEqual(await getText(page, `${Type} Content`), Content);
    }

    // And communication tracking records patient engagement:
    const communicationTracking = [
      { Metric: 'Message Delivery', Measurement: 'Successful text delivery confirmed' },
      { Metric: 'Portal Access', Measurement: 'Login timestamps and content viewed' },
      { Metric: 'Engagement Level', Measurement: 'Time spent reviewing instructions' },
    ];
    for (const { Metric, Measurement } of communicationTracking) {
      assert.strictEqual(await getText(page, Metric), Measurement);
    }
  });

  test('Handle follow-up for pediatric patients with parent/guardian coordination', async () => {
    // Given a pediatric patient "Emma Foster" (age 6) is discharged
    // And the parent "Sarah Foster" is the primary contact
    // And follow-up includes pediatric-specific requirements:
    //   | Follow-up Type        | Pediatric Considerations                   |
    //   | Pediatrician Visit    | Growth and development check               |
    //   | Vaccination Updates   | Catch-up on missed immunizations          |
    //   | School Health Forms   | Medical clearance for return to school     |
    // (assumed pre-seeded test data)
    // When the discharge process is completed
    // (assumed to have already occurred / triggered by the system)

    // Then the system creates parent-focused follow-up communications:
    await waitForTestId(page, 'Parent Portal Account');
    const parentCommunications = [
      { Target: 'Parent Portal Account', Type: "Child's medical summary and instructions" },
      { Target: 'School Notifications', Type: 'Medical excuse and return guidelines' },
      { Target: 'Pediatrician Alert', Type: 'ED visit summary and follow-up needs' },
    ];
    for (const { Target, Type } of parentCommunications) {
      assert.strictEqual(await getText(page, Target), Type);
    }

    // And pediatric-specific reminders are scheduled:
    const pediatricReminders = [
      { Type: 'Medication Reminders', Instructions: 'Weight-based dosing with schedule' },
      { Type: 'Development Milestones', Instructions: 'Age-appropriate recovery expectations' },
      { Type: 'School Return Criteria', Instructions: 'When child can safely return to activities' },
    ];
    for (const { Type, Instructions } of pediatricReminders) {
      assert.strictEqual(await getText(page, Type), Instructions);
    }

    // And child safety verification is included:
    const childSafetyChecks = [
      { Check: 'Home Safety Assessment', Requirement: 'Childproofing for medication storage' },
      { Check: 'Caregiver Instructions', Requirement: 'Multiple caregivers receive instructions' },
      { Check: 'Emergency Contacts', Requirement: 'Updated emergency contact information' },
    ];
    for (const { Check, Requirement } of childSafetyChecks) {
      assert.strictEqual(await getText(page, Check), Requirement);
    }
  });

  test('Track follow-up compliance and patient outcomes', async () => {
    // Given multiple patients have been discharged with follow-up requirements
    // (assumed pre-seeded test data)
    // When follow-up reminders are sent and appointments are scheduled
    // (assumed to have already occurred / triggered by the system)

    // Then the system tracks compliance metrics:
    await waitForTestId(page, 'Appointment Scheduling');
    const complianceMetrics = [
      { Metric: 'Appointment Scheduling', Measurement: '% of patients who schedule within timeframe' },
      { Metric: 'Appointment Attendance', Measurement: '% of scheduled appointments kept' },
      { Metric: 'Portal Engagement', Measurement: '% of patients accessing discharge instructions' },
      { Metric: 'Medication Compliance', Measurement: '% following prescription instructions' },
    ];
    for (const { Metric, Measurement } of complianceMetrics) {
      assert.strictEqual(await getText(page, Metric), Measurement);
    }

    // And outcome tracking is performed:
    const outcomeTracking = [
      { Metric: 'ED Readmissions', Method: '72-hour and 30-day return rates' },
      { Metric: 'Complication Rates', Method: 'Follow-up visits for related issues' },
      { Metric: 'Patient Satisfaction', Method: 'Follow-up surveys about discharge process' },
    ];
    for (const { Metric, Method } of outcomeTracking) {
      assert.strictEqual(await getText(page, Metric), Method);
    }

    // And quality improvement reports are generated:
    const qualityImprovementReports = [
      { Report: 'Follow-up Effectiveness', Content: 'Success rates by discharge diagnosis' },
      { Report: 'Communication Analysis', Content: 'Best-performing reminder methods' },
      { Report: 'Provider Performance', Content: 'Follow-up compliance by discharging physician' },
    ];
    for (const { Report, Content } of qualityImprovementReports) {
      assert.strictEqual(await getText(page, Report), Content);
    }
  });

  test('Handle follow-up complications and escalation procedures', async () => {
    // Given a patient "Michael Davis" was discharged 2 days ago
    // And follow-up reminders have been sent
    // (assumed pre-seeded test data)
    // When the patient contacts the ED with worsening symptoms
    // And the patient has not yet scheduled the required follow-up appointment
    // (assumed to have already occurred / triggered by the system)

    // Then the system escalates the follow-up process:
    await waitForTestId(page, 'Urgent Scheduling');
    const escalationActions = [
      { Action: 'Urgent Scheduling', Details: 'Same-day appointment coordination' },
      { Action: 'Provider Notification', Details: 'Original discharging physician alerted' },
      { Action: 'Symptom Assessment', Details: 'Nurse triage for immediate vs delayed care' },
      { Action: 'Documentation Update', Details: 'Patient contact and status change recorded' },
    ];
    for (const { Action, Details } of escalationActions) {
      assert.strictEqual(await getText(page, Action), Details);
    }

    // And enhanced monitoring is activated:
    const enhancedMonitoring = [
      { Type: 'Daily Check-ins', Action: 'Nurse calls patient for status updates' },
      { Type: 'Expedited Referrals', Action: 'Fast-track specialist appointments' },
      { Type: 'Safety Net Activation', Action: 'Ensure patient has immediate care access' },
    ];
    for (const { Type, Action } of enhancedMonitoring) {
      assert.strictEqual(await getText(page, Type), Action);
    }

    // And the care team receives comprehensive updates:
    const careTeamUpdates = [
      { Member: 'Discharging Physician', Information: 'Patient contact and current status' },
      { Member: 'Primary Care Provider', Information: 'Urgent need for appointment' },
      { Member: 'Charge Nurse', Information: 'Potential readmission risk identified' },
    ];
    for (const { Member, Information } of careTeamUpdates) {
      assert.strictEqual(await getText(page, Member), Information);
    }
  });
});
