// Playwright test for spec/features/22-audit-trail.feature
// (equivalent to tests-with-selenium-javascript/22-audit-trail.test.js).
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

test.describe('Feature: Audit Trail', () => {
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
    //   And I am logged in as "Compliance Officer Martinez"
    //   And the audit logging system is active and capturing all access events
    //   And patient record access is being monitored in real-time
    //   And audit reports are available for compliance review
    await verifySystemIsOperational(page);
    await login(page, 'Compliance Officer Martinez');
    // The remaining Background steps describe pre-seeded system state
    // (audit logging capture, real-time access monitoring, and available
    // audit reports) assumed to already be configured in the test
    // environment.
    const featureNavLink = await waitForTestId(page, 'Nav Audit Trail');
    await featureNavLink.click();
    await waitForTestId(page, 'Audit Trail Panel');
  });

  test('Review comprehensive access log for frequently accessed patient record', async () => {
    // Given a patient "Jennifer Rodriguez" was treated in the ED on June 20, 2025
    // And her medical record number is "MRN-789456"
    // And multiple healthcare providers accessed her record during and after her visit
    // When I search for access logs for patient "Jennifer Rodriguez" (MRN-789456)
    await fillField(page, 'Patient Search', 'Jennifer Rodriguez (MRN-789456)');
    // And I set the date range from June 20-24, 2025
    await fillField(page, 'Start Date', '2025-06-20');
    await fillField(page, 'End Date', '2025-06-24');
    // And I generate the comprehensive audit report
    await page.getByTestId('generate-audit-report-button').click();

    // Then the system displays all users who accessed the record:
    //   | Access # | User Name          | User Role           | Department       | Access Purpose    |
    //   | 1        | Dr. Sarah Kim      | Emergency Physician | Emergency Dept   | Direct patient care|
    //   | 2        | Nurse Johnson      | Registered Nurse    | Emergency Dept   | Direct patient care|
    //   | 3        | Tech Martinez      | Lab Technician      | Laboratory       | Lab result entry  |
    //   | 4        | Dr. Chen           | Radiologist         | Radiology        | Image interpretation|
    //   | 5        | Billing Clerk Adams| Billing Specialist  | Patient Financial| Billing/coding    |
    //   | 6        | Case Mgr Wilson    | Case Manager        | Social Services  | Discharge planning|
    //   | 7        | Dr. Patel          | Cardiologist        | Cardiology       | Consultation      |
    const accessLogEntries = await page.getByTestId('audit-log-entry').all();
    assert.strictEqual(accessLogEntries.length, 7);

    // And detailed timestamps are shown for each access:
    //   | User Name          | Login Time           | Logout Time          | Session Duration |
    //   | Dr. Sarah Kim      | 2025-06-20 14:15:22 | 2025-06-20 14:45:10 | 29 min 48 sec   |
    //   | Nurse Johnson      | 2025-06-20 14:20:15 | 2025-06-20 16:30:22 | 2 hr 10 min 7 sec|
    //   | Tech Martinez      | 2025-06-20 15:22:45 | 2025-06-20 15:25:12 | 2 min 27 sec    |
    //   | Dr. Chen           | 2025-06-20 16:10:33 | 2025-06-20 16:18:45 | 8 min 12 sec    |
    //   | Billing Clerk Adams| 2025-06-21 09:15:20 | 2025-06-21 09:22:15 | 6 min 55 sec    |
    //   | Case Mgr Wilson    | 2025-06-21 11:30:10 | 2025-06-21 11:45:33 | 15 min 23 sec   |
    //   | Dr. Patel          | 2025-06-22 10:22:18 | 2025-06-22 10:35:45 | 13 min 27 sec   |
    const accessLogTimestamps = await page.getByTestId('audit-log-timestamp-entry').all();
    assert.strictEqual(accessLogTimestamps.length, 7);

    // And specific data elements accessed are documented:
    //   | User Name          | Data Elements Accessed                           | Actions Performed        |
    //   | Dr. Sarah Kim      | Demographics, Chief complaint, Vital signs, Assessment, Orders | View, Edit, Create    |
    //   | Nurse Johnson      | Vital signs, Medications, Allergies, Care plans | View, Edit, Document   |
    //   | Tech Martinez      | Lab orders, Lab results                         | View, Enter results    |
    //   | Dr. Chen           | Imaging orders, Radiology reports              | View, Create report    |
    //   | Billing Clerk Adams| Diagnosis codes, Procedures, Insurance info     | View only             |
    //   | Case Mgr Wilson    | Discharge plans, Insurance, Social history      | View, Edit            |
    //   | Dr. Patel          | Cardiac tests, Consultation notes              | View, Create notes     |
    const accessLogDataElements = await page.getByTestId('audit-log-data-element-entry').all();
    assert.strictEqual(accessLogDataElements.length, 7);
  });

  test('Investigate suspicious access pattern for patient record', async () => {
    // Given a patient "Robert Thompson" has a high-profile status
    // And there have been unusual access patterns to his record
    // And the patient was not treated in the hospital during the access period
    // When I generate a detailed audit report for "Robert Thompson" (MRN-456789)
    await fillField(page, 'Patient Search', 'Robert Thompson (MRN-456789)');
    await page.getByTestId('generate-audit-report-button').click();
    // And I focus on the suspicious access period from June 15-18, 2025
    await fillField(page, 'Start Date', '2025-06-15');
    await fillField(page, 'End Date', '2025-06-18');

    // Then the system identifies potentially inappropriate access:
    //   | Suspicious Activity | Details                                          |
    //   | Unauthorized User   | Dr. Williams (Orthopedics) - No treatment relationship|
    //   | Unusual Timing      | Access at 11:45 PM on June 16 (outside normal hours)|
    //   | Excessive Duration  | 45-minute session for patient not under care    |
    //   | Data Mining Pattern | Accessed multiple unrelated patient records same night|
    const suspiciousActivity = [
      { label: 'Unauthorized User', value: 'Dr. Williams (Orthopedics) - No treatment relationship' },
      { label: 'Unusual Timing', value: 'Access at 11:45 PM on June 16 (outside normal hours)' },
      { label: 'Excessive Duration', value: '45-minute session for patient not under care' },
      { label: 'Data Mining Pattern', value: 'Accessed multiple unrelated patient records same night' },
    ];
    for (const { label, value } of suspiciousActivity) {
      assert.strictEqual(await getText(page, label), value);
    }

    // And detailed forensic information is provided:
    //   | Forensic Data       | Investigation Details                            |
    //   | IP Address          | 192.168.1.205 (Dr. Williams' office computer)  |
    //   | Workstation ID      | WS-ORTHO-03                                     |
    //   | Access Method       | Valid credentials, no badge scan               |
    //   | Previous Pattern    | First time accessing this patient               |
    //   | Concurrent Activity | Accessed 8 other unrelated patients same session|
    const forensicData = [
      { label: 'IP Address', value: "192.168.1.205 (Dr. Williams' office computer)" },
      { label: 'Workstation ID', value: 'WS-ORTHO-03' },
      { label: 'Access Method', value: 'Valid credentials, no badge scan' },
      { label: 'Previous Pattern', value: 'First time accessing this patient' },
      { label: 'Concurrent Activity', value: 'Accessed 8 other unrelated patients same session' },
    ];
    for (const { label, value } of forensicData) {
      assert.strictEqual(await getText(page, label), value);
    }

    // And compliance violation indicators are flagged:
    //   | Violation Type      | HIPAA Concern                                   |
    //   | No Treatment Relationship| No medical necessity for access              |
    //   | Excessive Access    | Viewed entire medical history unnecessarily    |
    //   | Pattern of Behavior | Multiple inappropriate accesses detected       |
    //   | Time-based Concern  | Access outside normal work hours              |
    const violationIndicators = [
      { label: 'No Treatment Relationship', value: 'No medical necessity for access' },
      { label: 'Excessive Access', value: 'Viewed entire medical history unnecessarily' },
      { label: 'Pattern of Behavior', value: 'Multiple inappropriate accesses detected' },
      { label: 'Time-based Concern', value: 'Access outside normal work hours' },
    ];
    for (const { label, value } of violationIndicators) {
      assert.strictEqual(await getText(page, label), value);
    }

    // And automatic security alerts are generated:
    //   | Alert Type          | Notification Details                            |
    //   | Privacy Officer     | Immediate alert sent for investigation          |
    //   | Department Head     | Orthopedics supervisor notified                |
    //   | IT Security         | Account flagged for enhanced monitoring         |
    //   | Risk Management     | Potential HIPAA violation logged              |
    const securityAlerts = [
      { label: 'Privacy Officer', value: 'Immediate alert sent for investigation' },
      { label: 'Department Head', value: 'Orthopedics supervisor notified' },
      { label: 'IT Security', value: 'Account flagged for enhanced monitoring' },
      { label: 'Risk Management', value: 'Potential HIPAA violation logged' },
    ];
    for (const { label, value } of securityAlerts) {
      assert.strictEqual(await getText(page, label), value);
    }
  });

  test('Generate audit report for break-the-glass emergency access', async () => {
    // Given a patient "Emergency John Doe" was brought unconscious to the ED
    // And normal consent procedures could not be followed due to patient condition
    // And emergency "break-the-glass" access was used to view records
    // When I review the emergency access audit trail
    await page.getByTestId('review-emergency-access-button').click();

    // Then the system documents the break-the-glass access:
    //   | Emergency Access    | Documentation                                   |
    //   | Access Type         | Break-the-glass emergency override             |
    //   | Medical Justification| Patient unconscious, life-threatening condition|
    //   | Authorizing Physician| Dr. Emergency Chief (Emergency Department Head) |
    //   | Access Duration     | 2 hours during critical care period           |
    //   | Override Reason     | Unable to obtain consent, medical emergency    |
    const breakTheGlassDocumentation = [
      { label: 'Access Type', value: 'Break-the-glass emergency override' },
      { label: 'Medical Justification', value: 'Patient unconscious, life-threatening condition' },
      { label: 'Authorizing Physician', value: 'Dr. Emergency Chief (Emergency Department Head)' },
      { label: 'Access Duration', value: '2 hours during critical care period' },
      { label: 'Override Reason', value: 'Unable to obtain consent, medical emergency' },
    ];
    for (const { label, value } of breakTheGlassDocumentation) {
      assert.strictEqual(await getText(page, label), value);
    }

    // And all emergency access activities are logged:
    //   | Activity            | Details                                         |
    //   | Records Accessed    | Previous ED visits, medication allergies, medical history|
    //   | Users Involved      | Dr. Sarah Kim, Nurse Johnson, Pharmacist Lee   |
    //   | Data Viewed         | Allergies, medications, past procedures         |
    //   | Clinical Decisions  | Medication choices based on allergy history    |
    //   | Patient Outcome     | Successful treatment, patient stabilized       |
    const emergencyAccessActivities = [
      { label: 'Records Accessed', value: 'Previous ED visits, medication allergies, medical history' },
      { label: 'Users Involved', value: 'Dr. Sarah Kim, Nurse Johnson, Pharmacist Lee' },
      { label: 'Data Viewed', value: 'Allergies, medications, past procedures' },
      { label: 'Clinical Decisions', value: 'Medication choices based on allergy history' },
      { label: 'Patient Outcome', value: 'Successful treatment, patient stabilized' },
    ];
    for (const { label, value } of emergencyAccessActivities) {
      assert.strictEqual(await getText(page, label), value);
    }

    // And post-emergency review requirements are documented:
    //   | Review Requirement  | Compliance Action                               |
    //   | Medical Necessity   | Clinical justification documented              |
    //   | Minimum Necessary   | Only essential health information accessed     |
    //   | Patient Notification| Patient to be informed of emergency access when able|
    //   | Quality Review      | Emergency access appropriateness reviewed      |
    const reviewRequirements = [
      { label: 'Medical Necessity', value: 'Clinical justification documented' },
      { label: 'Minimum Necessary', value: 'Only essential health information accessed' },
      { label: 'Patient Notification', value: 'Patient to be informed of emergency access when able' },
      { label: 'Quality Review', value: 'Emergency access appropriateness reviewed' },
    ];
    for (const { label, value } of reviewRequirements) {
      assert.strictEqual(await getText(page, label), value);
    }
  });

  test('Audit trail for patient who requested access log of their own record', async () => {
    // Given a patient "Maria Santos" has requested a copy of her access log
    // And this is her legal right under HIPAA
    // And she was treated on multiple occasions in the past year
    // When I generate a patient-facing access report for "Maria Santos" (MRN-321654)
    await fillField(page, 'Patient Search', 'Maria Santos (MRN-321654)');
    await page.getByTestId('generate-patient-access-report-button').click();
    // And I include the past 12 months of access activity
    await fillField(page, 'Reporting Period', '12 months');

    // Then the system creates a patient-appropriate access summary:
    //   | Access Summary      | Patient-Friendly Information                    |
    //   | Healthcare Providers| Names and roles of providers who accessed record|
    //   | Treatment Dates     | Dates when records were accessed for care      |
    //   | Purpose Categories  | Treatment, payment, healthcare operations       |
    //   | Administrative Access| Billing, quality assurance, regulatory compliance|
    const accessSummary = [
      { label: 'Healthcare Providers', value: 'Names and roles of providers who accessed record' },
      { label: 'Treatment Dates', value: 'Dates when records were accessed for care' },
      { label: 'Purpose Categories', value: 'Treatment, payment, healthcare operations' },
      { label: 'Administrative Access', value: 'Billing, quality assurance, regulatory compliance' },
    ];
    for (const { label, value } of accessSummary) {
      assert.strictEqual(await getText(page, label), value);
    }

    // And sensitive details are appropriately filtered:
    //   | Information Included| Patient Access Report Content                  |
    //   | Provider Names      | Dr. Sarah Kim (Emergency Medicine)             |
    //   | Access Dates        | June 20, 2025 for emergency treatment         |
    //   | General Purpose     | Direct patient care and treatment             |
    //   | Department          | Emergency Department                           |
    const informationIncluded = [
      { label: 'Provider Names', value: 'Dr. Sarah Kim (Emergency Medicine)' },
      { label: 'Access Dates', value: 'June 20, 2025 for emergency treatment' },
      { label: 'General Purpose', value: 'Direct patient care and treatment' },
      { label: 'Department', value: 'Emergency Department' },
    ];
    for (const { label, value } of informationIncluded) {
      assert.strictEqual(await getText(page, label), value);
    }

    // And technical details are excluded:
    //   | Information Excluded| Reason for Exclusion                          |
    //   | IP Addresses        | Technical data not relevant to patient        |
    //   | Workstation IDs     | Internal system identifiers                   |
    //   | Session Details     | Technical access information                  |
    //   | Investigation Data  | Law enforcement sensitive information          |
    const informationExcluded = [
      { label: 'IP Addresses', value: 'Technical data not relevant to patient' },
      { label: 'Workstation IDs', value: 'Internal system identifiers' },
      { label: 'Session Details', value: 'Technical access information' },
      { label: 'Investigation Data', value: 'Law enforcement sensitive information' },
    ];
    for (const { label, value } of informationExcluded) {
      assert.strictEqual(await getText(page, label), value);
    }

    // And the report includes patient rights information:
    //   | Patient Rights      | Information Provided                           |
    //   | Right to Restrict   | How to request access restrictions            |
    //   | Right to Complain   | How to file privacy complaints                |
    //   | Contact Information | Privacy officer contact details               |
    const patientRights = [
      { label: 'Right to Restrict', value: 'How to request access restrictions' },
      { label: 'Right to Complain', value: 'How to file privacy complaints' },
      { label: 'Contact Information', value: 'Privacy officer contact details' },
    ];
    for (const { label, value } of patientRights) {
      assert.strictEqual(await getText(page, label), value);
    }
  });

  test('Monthly compliance audit report for department oversight', async () => {
    // Given it is the first week of July 2025
    // And I need to generate the monthly compliance report for June
    // When I run the comprehensive monthly audit analysis
    await page.getByTestId('run-monthly-audit-analysis-button').click();

    // Then the system provides departmental access statistics:
    //   | Access Metric       | June 2025 Statistics                           |
    //   | Total Record Access | 15,847 patient record accesses                |
    //   | Unique Users        | 156 healthcare providers                       |
    //   | Average Session     | 12 minutes 34 seconds                         |
    //   | After-hours Access  | 892 accesses (5.6% of total)                 |
    //   | Emergency Override  | 12 break-the-glass accesses                  |
    const accessStatistics = [
      { label: 'Total Record Access', value: '15,847 patient record accesses' },
      { label: 'Unique Users', value: '156 healthcare providers' },
      { label: 'Average Session', value: '12 minutes 34 seconds' },
      { label: 'After-hours Access', value: '892 accesses (5.6% of total)' },
      { label: 'Emergency Override', value: '12 break-the-glass accesses' },
    ];
    for (const { label, value } of accessStatistics) {
      assert.strictEqual(await getText(page, label), value);
    }

    // And compliance indicators are summarized:
    //   | Compliance Area     | Status                                         |
    //   | Appropriate Access  | 99.2% of accesses had documented treatment relationship|
    //   | Minimum Necessary   | 98.7% accessed only required data elements    |
    //   | Timely Documentation| 99.8% of access properly documented within 24 hours|
    //   | Unauthorized Access | 0.3% flagged for investigation (47 instances) |
    const complianceIndicators = [
      { label: 'Appropriate Access', value: '99.2% of accesses had documented treatment relationship' },
      { label: 'Minimum Necessary Compliance', value: '98.7% accessed only required data elements' },
      { label: 'Timely Documentation', value: '99.8% of access properly documented within 24 hours' },
      { label: 'Unauthorized Access', value: '0.3% flagged for investigation (47 instances)' },
    ];
    for (const { label, value } of complianceIndicators) {
      assert.strictEqual(await getText(page, label), value);
    }

    // And trends and patterns are identified:
    //   | Trend Analysis      | Findings                                       |
    //   | Access Volume       | 15% increase from May (normal seasonal pattern)|
    //   | User Compliance     | 2 users require additional HIPAA training     |
    //   | System Performance  | No audit logging failures detected            |
    //   | Policy Adherence    | 99.1% compliance with access policies         |
    const trendsAndPatterns = [
      { label: 'Access Volume', value: '15% increase from May (normal seasonal pattern)' },
      { label: 'User Compliance', value: '2 users require additional HIPAA training' },
      { label: 'System Performance', value: 'No audit logging failures detected' },
      { label: 'Policy Adherence', value: '99.1% compliance with access policies' },
    ];
    for (const { label, value } of trendsAndPatterns) {
      assert.strictEqual(await getText(page, label), value);
    }

    // And recommendations for improvement are provided:
    //   | Improvement Area    | Recommendation                                 |
    //   | User Training       | Schedule refresher training for 2 staff members|
    //   | Policy Updates      | Review after-hours access procedures          |
    //   | System Enhancement  | Consider additional automated monitoring       |
    //   | Process Improvement | Streamline emergency access documentation     |
    const improvementRecommendations = [
      { label: 'User Training', value: 'Schedule refresher training for 2 staff members' },
      { label: 'Policy Updates', value: 'Review after-hours access procedures' },
      { label: 'System Enhancement', value: 'Consider additional automated monitoring' },
      { label: 'Process Improvement', value: 'Streamline emergency access documentation' },
    ];
    for (const { label, value } of improvementRecommendations) {
      assert.strictEqual(await getText(page, label), value);
    }
  });

  test('Investigate potential data breach with forensic audit trail', async () => {
    // Given there are concerns about a potential data security incident
    // And multiple patient records may have been inappropriately accessed
    // And law enforcement has requested detailed audit information
    // When I conduct a forensic audit investigation
    await page.getByTestId('conduct-forensic-audit-button').click();
    // And I analyze access patterns from June 1-30, 2025
    await fillField(page, 'Start Date', '2025-06-01');
    await fillField(page, 'End Date', '2025-06-30');

    // Then the system provides comprehensive forensic data:
    //   | Forensic Element    | Investigation Data                             |
    //   | User Activity       | Detailed timeline of all user actions         |
    //   | Data Accessed       | Specific patient information viewed/modified   |
    //   | System Interactions | Every click, search, and data retrieval       |
    //   | Network Activity    | IP addresses, network connections, file transfers|
    //   | Concurrent Sessions | Multiple simultaneous access attempts         |
    const forensicElements = [
      { label: 'User Activity', value: 'Detailed timeline of all user actions' },
      { label: 'Data Accessed', value: 'Specific patient information viewed/modified' },
      { label: 'System Interactions', value: 'Every click, search, and data retrieval' },
      { label: 'Network Activity', value: 'IP addresses, network connections, file transfers' },
      { label: 'Concurrent Sessions', value: 'Multiple simultaneous access attempts' },
    ];
    for (const { label, value } of forensicElements) {
      assert.strictEqual(await getText(page, label), value);
    }

    // And security indicators are analyzed:
    //   | Security Indicator  | Analysis Results                               |
    //   | Unusual Patterns    | 15 users accessed >100 records in one day    |
    //   | Off-site Access     | 23 connections from non-hospital IP addresses |
    //   | Data Export Activity| 5 instances of bulk data downloads            |
    //   | Failed Login Attempts| 247 failed logins from external IPs          |
    const securityIndicators = [
      { label: 'Unusual Patterns', value: '15 users accessed >100 records in one day' },
      { label: 'Off-site Access', value: '23 connections from non-hospital IP addresses' },
      { label: 'Data Export Activity', value: '5 instances of bulk data downloads' },
      { label: 'Failed Login Attempts', value: '247 failed logins from external IPs' },
    ];
    for (const { label, value } of securityIndicators) {
      assert.strictEqual(await getText(page, label), value);
    }

    // And evidence preservation procedures are documented:
    //   | Evidence Type       | Preservation Method                            |
    //   | Audit Logs          | Tamper-proof digital signature applied        |
    //   | System Snapshots    | Full system state captured and archived       |
    //   | User Account Data   | Complete account history preserved            |
    //   | Network Logs        | Network traffic logs secured for analysis     |
    const evidencePreservation = [
      { label: 'Audit Logs', value: 'Tamper-proof digital signature applied' },
      { label: 'System Snapshots', value: 'Full system state captured and archived' },
      { label: 'User Account Data', value: 'Complete account history preserved' },
      { label: 'Network Logs', value: 'Network traffic logs secured for analysis' },
    ];
    for (const { label, value } of evidencePreservation) {
      assert.strictEqual(await getText(page, label), value);
    }

    // And legal compliance requirements are met:
    //   | Legal Requirement   | Compliance Action                              |
    //   | Chain of Custody    | Documented evidence handling procedures        |
    //   | Data Integrity      | Cryptographic verification of audit data      |
    //   | Discovery Response  | Legal hold procedures activated               |
    //   | Regulatory Reporting| Breach notification procedures initiated      |
    const legalCompliance = [
      { label: 'Chain of Custody', value: 'Documented evidence handling procedures' },
      { label: 'Data Integrity', value: 'Cryptographic verification of audit data' },
      { label: 'Discovery Response', value: 'Legal hold procedures activated' },
      { label: 'Regulatory Reporting', value: 'Breach notification procedures initiated' },
    ];
    for (const { label, value } of legalCompliance) {
      assert.strictEqual(await getText(page, label), value);
    }
  });
});
