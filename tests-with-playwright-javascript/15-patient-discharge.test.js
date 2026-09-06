// Playwright test for spec/features/15-patient-discharge.feature
// (equivalent to tests-with-selenium-javascript/15-patient-discharge.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see support/fields.js) and the shared
// data-testid contract in support/login.js (login-identity, login-submit,
// app-root).

import { chromium, test } from '@playwright/test';
import { strict as assert } from 'assert';
import { login, verifySystemIsOperational } from './support/login.js';
import { fillFields, getText, waitForTestId } from './support/fields.js';

// All scenarios in this file share one page (like the Selenium suite shares
// one WebDriver per file), since later scenarios rely on data earlier
// scenarios registered persisting in the app's localStorage -- so they must
// run in file order, not in parallel.
test.describe.configure({ mode: 'serial' });

test.describe('Feature: Patient Discharge', () => {
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
    //   And I am logged in as "Dr. Johnson"
    //   And the discharge module is active
    //   And billing integration is enabled
    //   And bed management system is connected
    await verifySystemIsOperational(page);
    await login(page, 'Dr. Johnson');
    // The discharge module, billing integration, and the bed management
    // system connection are assumed to be active backend configuration
    // already in place for this environment.

    const patientDischargeNavLink = await waitForTestId(page, 'Nav Patient Discharge');
    await patientDischargeNavLink.click();
    await waitForTestId(page, 'Patient Discharge Panel');
  });

  test('Complete standard patient discharge with instructions', async () => {
    // Given a patient "Jennifer Martinez" is in bed "ED-8"
    // And the patient has completed treatment for "urinary tract infection"
    // And all diagnostic tests and treatments are finished
    // And the patient is medically stable for discharge
    // (assumed pre-seeded test data)
    // When I enter discharge orders and instructions:
    await fillFields(page, [
      { Field: 'Discharge Status', Value: 'Home with medications' },
      { Field: 'Primary Diagnosis', Value: 'Urinary tract infection (N39.0)' },
      { Field: 'Medications', Value: 'Trimethoprim-Sulfamethoxazole 800mg BID x7d' },
      { Field: 'Follow-up Care', Value: 'Primary care physician in 3-5 days' },
      { Field: 'Activity Level', Value: 'Regular activities as tolerated' },
      { Field: 'Diet', Value: 'Regular diet, increase fluid intake' },
    ]);
    // "Return Precautions" is, for this scenario, both a fillable order
    // field and (simultaneously, before submit) a generated document label
    // rendered with the same data-testid -- so target the form's copy
    // explicitly (the first match in document order) instead of the
    // ambiguous fillField/getByTestId helper.
    await page.getByTestId('return-precautions').first().fill('Fever >101°F, worsening symptoms, blood in urine');
    // And I submit the discharge orders
    await page.getByTestId('submit-discharge-form').click();

    // Then the system generates comprehensive discharge paperwork:
    await waitForTestId(page, 'Discharge Summary');
    const dischargePaperwork = [
      { Document: 'Discharge Summary', Content: 'Treatment summary, diagnosis, medications' },
      { Document: 'Medication List', Content: 'Prescriptions with dosing instructions' },
      { Document: 'Follow-up Instructions', Content: 'PCP appointment scheduling information' },
      { Document: 'Return Precautions', Content: 'When to seek emergency care' },
      { Document: 'Patient Education', Content: 'UTI prevention and care instructions' },
    ];
    for (const { Document, Content } of dischargePaperwork) {
      assert.strictEqual(await getText(page, Document), Content);
    }

    // And the system updates bed availability:
    const bedAvailabilityUpdates = [
      { Change: 'Previous Status', Details: 'Occupied by Jennifer Martinez' },
      { Change: 'New Status', Details: 'Needs cleaning' },
      { Change: 'Availability', Details: 'Removed from available bed count' },
      { Change: 'Housekeeping Alert', Details: 'Cleaning notification sent' },
    ];
    for (const { Change, Details } of bedAvailabilityUpdates) {
      assert.strictEqual(await getText(page, Change), Details);
    }

    // And billing processes are automatically triggered:
    const billingActions = [
      { Action: 'Final Charges', Details: 'All services and procedures captured' },
      { Action: 'Insurance Billing', Details: 'Claims prepared for submission' },
      { Action: 'Patient Statement', Details: 'Financial responsibility calculated' },
      { Action: 'Coding Review', Details: 'ICD-10 and CPT codes validated' },
    ];
    for (const { Action, Details } of billingActions) {
      assert.strictEqual(await getText(page, Action), Details);
    }
  });

  test('Discharge patient with prescription medications', async () => {
    // Given a patient "Robert Chen" is ready for discharge
    // And treatment required multiple medications
    // (assumed pre-seeded test data)
    // When I enter discharge orders including prescriptions:
    await fillFields(page, [
      { Field: 'Amoxicillin Dose', Value: '500mg' },
      { Field: 'Amoxicillin Frequency', Value: 'TID' },
      { Field: 'Amoxicillin Duration', Value: '10 days' },
      { Field: 'Amoxicillin Special Instructions', Value: 'Take with food' },
      { Field: 'Ibuprofen Dose', Value: '600mg' },
      { Field: 'Ibuprofen Frequency', Value: 'Q6H PRN' },
      { Field: 'Ibuprofen Duration', Value: '5 days' },
      { Field: 'Ibuprofen Special Instructions', Value: 'For pain only' },
      { Field: 'Omeprazole Dose', Value: '20mg' },
      { Field: 'Omeprazole Frequency', Value: 'Daily' },
      { Field: 'Omeprazole Duration', Value: '14 days' },
      { Field: 'Omeprazole Special Instructions', Value: 'Take before breakfast' },
    ]);
    // And I include medication education:
    await fillFields(page, [
      { Field: 'Drug Interactions', Value: 'Avoid alcohol with antibiotics' },
      { Field: 'Side Effects', Value: 'Watch for nausea, diarrhea, allergic reactions' },
      { Field: 'Compliance', Value: 'Complete full antibiotic course' },
    ]);
    // And I submit the discharge
    await page.getByTestId('submit-discharge-form').click();

    // Then the system generates medication-specific documentation:
    await waitForTestId(page, 'Prescription List');
    const medicationDocumentation = [
      { Document: 'Prescription List', Content: 'All medications with complete instructions' },
      { Document: 'Drug Information', Content: 'Side effects, interactions, precautions' },
      { Document: 'Pharmacy List', Content: 'Nearby pharmacies with hours' },
      { Document: 'Medication Calendar', Content: 'Dosing schedule for patient reference' },
    ];
    for (const { Document, Content } of medicationDocumentation) {
      assert.strictEqual(await getText(page, Document), Content);
    }

    // And prescriptions are electronically transmitted to patient's preferred pharmacy
    const prescriptionTransmissionStatus = await getText(page, 'Prescription Transmission Status');
    assert.match(prescriptionTransmissionStatus, /transmitted/i);

    // And medication allergy checking is performed one final time
    const allergyCheckStatus = await getText(page, 'Medication Allergy Check Status');
    assert.match(allergyCheckStatus, /checked|performed|cleared/i);

    // And patient receives medication counseling checklist
    const counselingChecklist = await waitForTestId(page, 'Medication Counseling Checklist');
    assert.ok(await counselingChecklist.isVisible());
  });

  test('Discharge patient requiring follow-up appointments', async () => {
    // Given a patient "Maria Santos" needs specialized follow-up care
    // And the treatment was for "complex laceration repair"
    // (assumed pre-seeded test data)
    // When I enter discharge orders with follow-up requirements:
    await fillFields(page, [
      { Field: 'Wound Check Timeframe', Value: '2-3 days' },
      { Field: 'Wound Check Specialist Required', Value: 'Primary care' },
      { Field: 'Wound Check Special Instructions', Value: 'Remove sutures' },
      { Field: 'Specialist Consult Timeframe', Value: '1 week' },
      { Field: 'Specialist Consult Specialist Required', Value: 'Plastic surgeon' },
      { Field: 'Specialist Consult Special Instructions', Value: 'Scar management' },
      { Field: 'Lab Follow-up Timeframe', Value: '5 days' },
      { Field: 'Lab Follow-up Specialist Required', Value: 'Primary care' },
      { Field: 'Lab Follow-up Special Instructions', Value: 'Check CBC' },
    ]);
    // And I specify wound care instructions:
    await fillFields(page, [
      { Field: 'Dressing Changes', Value: 'Change daily, keep dry for 48 hours' },
      { Field: 'Cleaning Protocol', Value: 'Gentle soap and water after 48 hours' },
      { Field: 'Activity Restrictions', Value: 'No heavy lifting >10 lbs for 2 weeks' },
      { Field: 'Signs of Infection', Value: 'Redness, swelling, pus, fever' },
    ]);

    // Then the system schedules and documents follow-up care:
    await waitForTestId(page, 'Appointment Booking');
    const followUpScheduling = [
      { Action: 'Appointment Booking', Details: 'Attempts to schedule with preferred providers' },
      { Action: 'Referral Generation', Details: 'Electronic referrals to specialists' },
      { Action: 'Reminder Setup', Details: 'Patient reminders for appointments' },
    ];
    for (const { Action, Details } of followUpScheduling) {
      assert.strictEqual(await getText(page, Action), Details);
    }

    // And comprehensive wound care instructions are provided
    const woundCareInstructions = await waitForTestId(page, 'Wound Care Instructions');
    assert.ok(await woundCareInstructions.isVisible());

    // And follow-up appointment confirmations are sent to patient
    const appointmentConfirmationStatus = await getText(page, 'Appointment Confirmation Status');
    assert.match(appointmentConfirmationStatus, /sent|confirmed/i);

    // And referring physician receives notification of specialist referral
    const specialistReferralNotificationStatus = await getText(page, 'Specialist Referral Notification Status');
    assert.match(specialistReferralNotificationStatus, /sent|notified/i);
  });

  test('Handle discharge with insurance authorization requirements', async () => {
    // Given a patient "David Kim" requires expensive follow-up imaging
    // And the patient's insurance requires prior authorization
    // (assumed pre-seeded test data)
    // When I enter discharge orders including:
    await fillFields(page, [
      { Field: 'Imaging Study', Value: 'MRI lumbar spine within 2 weeks' },
      { Field: 'Estimated Cost', Value: '$2,400' },
      { Field: 'Medical Necessity', Value: 'Rule out disc herniation' },
    ]);
    // And I submit the discharge orders
    await page.getByTestId('submit-discharge-form').click();

    // Then the system handles insurance requirements:
    await waitForTestId(page, 'Authorization Check');
    const insuranceRequirements = [
      { Process: 'Authorization Check', Action: 'Prior auth required for MRI' },
      { Process: 'Documentation Prep', Action: 'Clinical justification prepared' },
      { Process: 'Patient Notification', Action: 'Informed of authorization process' },
      { Process: 'Alternative Options', Action: 'Suggest urgent care MRI if auth denied' },
    ];
    for (const { Process, Action } of insuranceRequirements) {
      assert.strictEqual(await getText(page, Process), Action);
    }

    // And the patient receives information about:
    const patientInformation = [
      { Type: 'Authorization Process', Content: 'Timeline and requirements explained' },
      { Type: 'Financial Options', Content: 'Self-pay rates and payment plans' },
      { Type: 'Alternative Providers', Content: 'Facilities that may not require pre-auth' },
    ];
    for (const { Type, Content } of patientInformation) {
      assert.strictEqual(await getText(page, Type), Content);
    }

    // And insurance pre-authorization request is automatically submitted
    const preAuthorizationStatus = await getText(page, 'Insurance Pre-Authorization Status');
    assert.match(preAuthorizationStatus, /submitted/i);
  });

  test('Discharge pediatric patient with parent/guardian instructions', async () => {
    // Given a pediatric patient "Emma Foster" (age 6) is ready for discharge
    // And the parent "Sarah Foster" is present
    // And treatment was for "febrile seizure"
    // (assumed pre-seeded test data)
    // When I enter pediatric discharge orders:
    await fillFields(page, [
      { Field: 'Weight-based Medications', Value: 'Acetaminophen 10mg/kg Q6H PRN fever' },
      { Field: 'Parent Education', Value: 'Fever management, seizure precautions' },
      { Field: 'Activity Restrictions', Value: 'No swimming for 24 hours' },
      { Field: 'School Return', Value: 'May return tomorrow if fever-free' },
    ]);
    // And I provide seizure-specific education:
    await fillFields(page, [
      { Field: 'Seizure Precautions', Value: 'Keep child safe during future episodes' },
      { Field: 'When to Call 911', Value: 'Seizure >5 minutes, difficulty breathing' },
      { Field: 'Temperature Control', Value: 'Aggressive fever reduction strategies' },
    ]);

    // Then the system generates pediatric-specific discharge materials:
    await waitForTestId(page, 'Parent Instructions');
    const pediatricMaterials = [
      { Document: 'Parent Instructions', Content: 'Age-appropriate medication dosing' },
      { Document: 'Emergency Signs', Content: 'When to bring child back to ED' },
      { Document: 'School Note', Content: 'Medical excuse and return instructions' },
      { Document: 'Developmental Info', Content: 'Normal vs concerning behaviors post-seizure' },
    ];
    for (const { Document, Content } of pediatricMaterials) {
      assert.strictEqual(await getText(page, Document), Content);
    }

    // And parent acknowledgment is electronically captured
    const parentAcknowledgmentStatus = await getText(page, 'Parent Acknowledgment Status');
    assert.match(parentAcknowledgmentStatus, /captured|recorded/i);

    // And pediatric follow-up with primary care pediatrician is scheduled
    const pediatricFollowUpStatus = await getText(page, 'Pediatric Follow-up Status');
    assert.match(pediatricFollowUpStatus, /scheduled/i);

    // And school nurse receives medical summary if parent consents
    const schoolNurseNotificationStatus = await getText(page, 'School Nurse Notification Status');
    assert.match(schoolNurseNotificationStatus, /sent|notified/i);
  });

  test('Handle discharge during shift change', async () => {
    // Given a patient "Lisa Brown" is ready for discharge at 18:45
    // And shift change occurs at 19:00
    // And "Dr. Day" (day shift) is discharging the patient
    // And "Dr. Night" (evening shift) is incoming
    // (assumed pre-seeded test data)
    // When "Dr. Day" enters the discharge orders
    // And the discharge process extends past shift change
    // (assumed to have already occurred / triggered by the system)

    // Then the system manages the transition seamlessly:
    await waitForTestId(page, 'Discharge Ownership');
    const transitionManagement = [
      { Item: 'Discharge Ownership', Action: 'Dr. Day completes discharge process' },
      { Item: 'Documentation', Action: "All discharge notes under Dr. Day's name" },
      { Item: 'Follow-up Responsibility', Action: 'Any issues route to Dr. Night' },
      { Item: 'Billing Attribution', Action: 'Dr. Day receives credit for discharge' },
    ];
    for (const { Item, Action } of transitionManagement) {
      assert.strictEqual(await getText(page, Item), Action);
    }

    // And both physicians receive handoff notification:
    const handoffNotifications = [
      { Physician: 'Dr. Day', Content: 'Discharge completed for Lisa Brown' },
      { Physician: 'Dr. Night', Content: 'Lisa Brown discharged - available for questions' },
    ];
    for (const { Physician, Content } of handoffNotifications) {
      assert.strictEqual(await getText(page, `${Physician} Notification`), Content);
    }

    // And the bed becomes available for evening shift patient flow
    const bedAvailabilityStatus = await getText(page, 'Bed Availability Status');
    assert.match(bedAvailabilityStatus, /available/i);
  });

  test('Discharge patient against medical advice (AMA)', async () => {
    // Given a patient "Michael Davis" wants to leave against medical advice
    // And the patient has been informed of risks
    // And the patient has decision-making capacity
    // (assumed pre-seeded test data)
    // When I process an AMA discharge:
    await fillFields(page, [
      { Field: 'Risk Explanation', Value: 'Documented that risks were explained' },
      { Field: 'Patient Understanding', Value: 'Patient verbalized understanding of risks' },
      { Field: 'Capacity Assessment', Value: 'Patient has decision-making capacity' },
      { Field: 'Witness Required', Value: 'Nurse witness to AMA conversation' },
    ]);
    // And I enter minimal safe discharge instructions:
    await fillFields(page, [
      { Field: 'Return Immediately', Value: 'If symptoms worsen or new symptoms develop' },
      { Field: 'Follow-up Care', Value: 'Strong recommendation for PCP visit' },
      { Field: 'Medication Safety', Value: 'Critical medications must be continued' },
    ]);

    // Then the system generates AMA-specific documentation:
    await waitForTestId(page, 'AMA Form');
    const amaDocumentation = [
      { Document: 'AMA Form', Content: 'Legal documentation of patient choice' },
      { Document: 'Risk Documentation', Content: 'Medical risks of leaving explained' },
      { Document: 'Witness Signatures', Content: 'Patient, physician, and nurse signatures' },
      { Document: 'Limited Liability', Content: 'Hospital liability limitations documented' },
    ];
    for (const { Document, Content } of amaDocumentation) {
      assert.strictEqual(await getText(page, Document), Content);
    }

    // And billing processes reflect AMA status
    const billingAmaStatus = await getText(page, 'Billing AMA Status');
    assert.match(billingAmaStatus, /AMA/i);

    // And legal risk management is notified of AMA discharge
    const legalRiskNotificationStatus = await getText(page, 'Legal Risk Management Notification Status');
    assert.match(legalRiskNotificationStatus, /notified/i);

    // And patient still receives basic safety instructions
    const basicSafetyInstructions = await waitForTestId(page, 'Basic Safety Instructions');
    assert.ok(await basicSafetyInstructions.isVisible());
  });

  test('Batch discharge processing during high volume', async () => {
    // Given multiple patients are ready for simultaneous discharge:
    //   | Patient Name    | Bed    | Diagnosis        | Discharge Type     |
    //   | Patient A       | ED-3   | Minor injury     | Home              |
    //   | Patient B       | ED-7   | Gastroenteritis  | Home with meds    |
    //   | Patient C       | ED-11  | Anxiety          | Home with referral |
    // (assumed pre-seeded test data)
    // When I process multiple discharges efficiently
    // (assumed to have already occurred / triggered by the system)

    // Then the system handles batch processing:
    await waitForTestId(page, 'Template Usage');
    const batchProcessing = [
      { Feature: 'Template Usage', Functionality: 'Common discharge templates applied' },
      { Feature: 'Automated Documentation', Functionality: 'Standard instructions auto-populated' },
      { Feature: 'Concurrent Processing', Functionality: 'Multiple discharges processed simultaneously' },
    ];
    for (const { Feature, Functionality } of batchProcessing) {
      assert.strictEqual(await getText(page, Feature), Functionality);
    }

    // And all bed updates occur simultaneously:
    const bedUpdates = [
      { Management: 'Status Updates', Action: 'All beds marked "needs cleaning"' },
      { Management: 'Housekeeping Batch', Action: 'Single notification for multiple rooms' },
      { Management: 'Availability Count', Action: 'Bed count updated after all discharges' },
    ];
    for (const { Management, Action } of bedUpdates) {
      assert.strictEqual(await getText(page, Management), Action);
    }

    // And billing processes are optimized for batch handling
    const batchBillingStatus = await getText(page, 'Batch Billing Status');
    assert.match(batchBillingStatus, /optimized/i);
  });

  test('Track discharge metrics and quality indicators', async () => {
    // Given patient discharges are being processed
    // (assumed pre-seeded test data)
    // When discharge orders are completed
    // (assumed to have already occurred / triggered by the system)

    // Then the system tracks key performance indicators:
    await waitForTestId(page, 'Discharge Time');
    const performanceIndicators = [
      { Metric: 'Discharge Time', Measurement: 'Order entry to patient departure' },
      { Metric: 'Readmission Rate', Measurement: '72-hour return rate tracking' },
      { Metric: 'Instruction Quality', Measurement: 'Patient understanding verification' },
      { Metric: 'Follow-up Compliance', Measurement: 'Scheduled appointment attendance' },
    ];
    for (const { Metric, Measurement } of performanceIndicators) {
      assert.strictEqual(await getText(page, Metric), Measurement);
    }

    // And generates quality reports:
    const qualityReports = [
      { Report: 'Provider Performance', Content: 'Discharge efficiency by physician' },
      { Report: 'Patient Satisfaction', Content: 'Discharge process satisfaction scores' },
      { Report: 'Readmission Analysis', Content: 'Patterns in early returns' },
    ];
    for (const { Report, Content } of qualityReports) {
      assert.strictEqual(await getText(page, Report), Content);
    }

    // And identifies improvement opportunities:
    const improvementOpportunities = [
      { Area: 'Process Efficiency', Recommendation: 'Streamline documentation workflows' },
      { Area: 'Patient Education', Recommendation: 'Enhance instruction clarity' },
      { Area: 'Follow-up Coordination', Recommendation: 'Improve appointment scheduling system' },
    ];
    for (const { Area, Recommendation } of improvementOpportunities) {
      assert.strictEqual(await getText(page, Area), Recommendation);
    }
  });
});
