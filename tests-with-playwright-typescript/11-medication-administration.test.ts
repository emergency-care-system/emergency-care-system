// Playwright test for spec/features/11-medication-administration.feature
// (equivalent to tests-with-selenium-javascript/11-medication-administration.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see support/fields.js) and the shared
// data-testid contract in support/login.js (login-identity, login-submit,
// app-root).

import { chromium, test, type Browser, type Page } from '@playwright/test';
import { strict as assert } from 'assert';
import { login, verifySystemIsOperational } from './support/login.js';
import { fillFields, fillField, getText, waitForTestId } from './support/fields.js';

test.describe.configure({ mode: 'serial' });

test.describe('Feature: Medication Administration', () => {
  let browser: Browser;
  let page: Page;

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
    //   And I am logged in as "Nurse Johnson"
    //   And the medication administration module is active
    //   And the barcode scanning system is functional
    //   And the medication administration record (MAR) is accessible
    await verifySystemIsOperational(page);
    await login(page, 'Nurse Johnson');
    // The medication administration module, barcode scanning system, and
    // MAR accessibility are assumed to be pre-seeded test environment state.

    const medicationAdministrationNavLink = await waitForTestId(page, 'Nav Medication Administration');
    await medicationAdministrationNavLink.click();
    await waitForTestId(page, 'Medication Administration Panel');
  });

  test('Successfully administer scheduled medication with barcode verification', async () => {
    // Given a patient "Maria Gonzalez" is in bed "ED-8"
    // And the patient has a prescribed medication:
    //   | Medication    | Dose      | Route | Frequency | Scheduled Time | Status    |
    //   | Metoprolol    | 25mg      | PO    | BID       | 14:00         | Due       |
    // And the patient is wearing a wristband with barcode "PT-12345"
    // And I have the medication with barcode "MED-METOPROLOL-25MG"

    // When I scan the patient's wristband barcode "PT-12345"
    await page.getByTestId('scan-wristband-barcode').click();
    // And I scan the medication barcode "MED-METOPROLOL-25MG"
    await page.getByTestId('scan-medication-barcode').click();

    // Then the system verifies the five rights of medication administration:
    //   | Right          | Verification                                  | Status   |
    //   | Right Patient  | Maria Gonzalez confirmed via wristband       | ✓ Valid  |
    //   | Right Drug     | Metoprolol matches prescribed medication     | ✓ Valid  |
    //   | Right Dose     | 25mg matches prescribed dose                 | ✓ Valid  |
    //   | Right Route    | PO (oral) matches prescribed route           | ✓ Valid  |
    //   | Right Time     | Within acceptable window (13:30-14:30)      | ✓ Valid  |
    await waitForTestId(page, 'Right Patient Status');
    assert.strictEqual(await getText(page, 'Right Patient Status'), '✓ Valid');
    assert.strictEqual(await getText(page, 'Right Drug Status'), '✓ Valid');
    assert.strictEqual(await getText(page, 'Right Dose Status'), '✓ Valid');
    assert.strictEqual(await getText(page, 'Right Route Status'), '✓ Valid');
    assert.strictEqual(await getText(page, 'Right Time Status'), '✓ Valid');

    // And the system displays confirmation screen:
    //   | Field             | Value                                        |
    //   | Patient           | Maria Gonzalez, DOB: 1975-08-15            |
    //   | Medication        | Metoprolol 25mg                            |
    //   | Route             | PO (By mouth)                              |
    //   | Administration Time| 14:05                                      |
    assert.strictEqual(await getText(page, 'Patient'), 'Maria Gonzalez, DOB: 1975-08-15');
    assert.strictEqual(await getText(page, 'Medication'), 'Metoprolol 25mg');
    assert.strictEqual(await getText(page, 'Route'), 'PO (By mouth)');
    assert.strictEqual(await getText(page, 'Administration Time'), '14:05');

    // And I confirm the administration
    await page.getByTestId('confirm-administration').click();

    // Then the system records the administration with timestamp:
    //   | Field             | Value                                        |
    //   | Patient ID        | PT-12345                                   |
    //   | Medication        | Metoprolol 25mg PO                         |
    //   | Administered By   | Nurse Johnson                              |
    //   | Administration Time| 2025-06-24 14:05:32                       |
    //   | Verification Method| Barcode scan                              |
    await waitForTestId(page, 'Patient ID');
    assert.strictEqual(await getText(page, 'Patient ID'), 'PT-12345');
    assert.strictEqual(await getText(page, 'Medication'), 'Metoprolol 25mg PO');
    assert.strictEqual(await getText(page, 'Administered By'), 'Nurse Johnson');
    assert.strictEqual(await getText(page, 'Administration Time'), '2025-06-24 14:05:32');
    assert.strictEqual(await getText(page, 'Verification Method'), 'Barcode scan');

    // And the medication status is updated to "Given"
    assert.strictEqual(await getText(page, 'Medication Status'), 'Given');
    // And the next dose is automatically scheduled for "02:00 tomorrow"
    assert.strictEqual(await getText(page, 'Next Dose Scheduled'), '02:00 tomorrow');
  });

  test('Handle medication administration with allergy alert', async () => {
    // Given a patient "Robert Chen" is in bed "ED-12"
    // And the patient has documented allergies:
    //   | Allergy       | Reaction Type      | Severity  |
    //   | Penicillin    | Rash, hives       | Moderate  |
    //   | Morphine      | Respiratory depression | Severe |
    // And there is a prescribed medication:
    //   | Medication    | Dose      | Route | Prescriber |
    //   | Amoxicillin   | 500mg     | PO    | Dr. Smith  |

    // When I scan the patient's wristband barcode
    await page.getByTestId('scan-wristband-barcode').click();
    // And I scan the amoxicillin medication barcode
    await page.getByTestId('scan-medication-barcode').click();

    // Then the system displays an allergy alert:
    //   | Alert Type        | Message                                      |
    //   | DRUG ALLERGY      | ⚠️ WARNING: Patient allergic to Penicillin |
    //   | Cross-Reaction    | Amoxicillin contains penicillin             |
    //   | Severity          | Moderate - Rash, hives                      |
    //   | Recommendation    | Contact physician before administration      |
    await waitForTestId(page, 'DRUG ALLERGY');
    assert.strictEqual(await getText(page, 'DRUG ALLERGY'), '⚠️ WARNING: Patient allergic to Penicillin');
    assert.strictEqual(await getText(page, 'Cross-Reaction'), 'Amoxicillin contains penicillin');
    assert.strictEqual(await getText(page, 'Severity'), 'Moderate - Rash, hives');
    assert.strictEqual(await getText(page, 'Recommendation'), 'Contact physician before administration');

    // And the system requires additional verification:
    //   | Verification Step | Requirement                                  |
    //   | Physician Approval| Must have override from prescribing MD      |
    //   | Documentation     | Must document reason for override           |
    //   | Monitoring Plan   | Must specify allergy monitoring protocol    |
    assert.strictEqual(await getText(page, 'Physician Approval'), 'Must have override from prescribing MD');
    assert.strictEqual(await getText(page, 'Documentation'), 'Must document reason for override');
    assert.strictEqual(await getText(page, 'Monitoring Plan'), 'Must specify allergy monitoring protocol');

    // And the medication administration is held pending physician confirmation
    const administrationStatus = await getText(page, 'Administration Status');
    assert.match(administrationStatus, /pending physician confirmation/i);
    // And an alert is sent to the prescribing physician "Dr. Smith"
    const physicianAlert = await getText(page, 'Physician Alert');
    assert.match(physicianAlert, /Dr\. Smith/);
  });

  test('Administer PRN medication with clinical assessment', async () => {
    // Given a patient "Jennifer Lopez" is in bed "ED-6"
    // And there is a PRN medication order:
    //   | Medication     | Dose    | Route | Indication           | Max Frequency |
    //   | Morphine       | 2mg     | IV    | Pain score ≥7/10     | Q4H PRN       |
    // And the patient's current pain score is "8/10"
    // And the last morphine dose was given 5 hours ago

    // When I scan the patient's wristband barcode
    await page.getByTestId('scan-wristband-barcode').click();
    // And I scan the morphine medication barcode
    await page.getByTestId('scan-medication-barcode').click();

    // Then the system verifies PRN medication criteria:
    //   | Criteria          | Assessment                                   | Status   |
    //   | Clinical Indication| Pain score 8/10 meets threshold ≥7/10      | ✓ Met    |
    //   | Frequency Limit   | Last dose 5h ago, within Q4H limit         | ✓ Valid  |
    //   | Patient Safety    | No respiratory contraindications            | ✓ Safe   |
    await waitForTestId(page, 'Clinical Indication Status');
    assert.strictEqual(await getText(page, 'Clinical Indication Status'), '✓ Met');
    assert.strictEqual(await getText(page, 'Frequency Limit Status'), '✓ Valid');
    assert.strictEqual(await getText(page, 'Patient Safety Status'), '✓ Safe');

    // And I document the clinical assessment:
    //   | Assessment Field  | Value                                        |
    //   | Pain Score        | 8/10                                        |
    //   | Pain Location     | Chest                                       |
    //   | Pain Quality      | Sharp, stabbing                             |
    //   | Vital Signs       | BP: 140/85, HR: 95, RR: 18, O2: 96%       |
    await fillFields(page, [
      { Field: 'Pain Score', Value: '8/10' },
      { Field: 'Pain Location', Value: 'Chest' },
      { Field: 'Pain Quality', Value: 'Sharp, stabbing' },
      { Field: 'Vital Signs', Value: 'BP: 140/85, HR: 95, RR: 18, O2: 96%' },
    ]);

    // And I confirm the PRN administration
    await page.getByTestId('confirm-administration').click();

    // Then the system records the administration with clinical justification:
    //   | Documentation     | Details                                      |
    //   | PRN Indication    | Pain score 8/10, patient requested relief   |
    //   | Clinical Assessment| Stable vitals, no respiratory distress     |
    //   | Next Available    | Not before 18:15 (Q4H)                     |
    await waitForTestId(page, 'PRN Indication');
    assert.strictEqual(await getText(page, 'PRN Indication'), 'Pain score 8/10, patient requested relief');
    assert.strictEqual(await getText(page, 'Clinical Assessment'), 'Stable vitals, no respiratory distress');
    assert.strictEqual(await getText(page, 'Next Available'), 'Not before 18:15 (Q4H)');
  });

  test('Handle medication administration timing variances', async () => {
    // Given a patient "David Kim" is in bed "ED-3"
    // And there is a scheduled medication:
    //   | Medication    | Scheduled Time | Acceptable Window | Current Time |
    //   | Insulin       | 12:00         | ±30 minutes       | 13:15        |

    // When I scan the patient's wristband barcode
    await page.getByTestId('scan-wristband-barcode').click();
    // And I scan the insulin medication barcode
    await page.getByTestId('scan-medication-barcode').click();

    // Then the system detects a timing variance:
    //   | Variance Type     | Details                                      |
    //   | Late Administration| 75 minutes past scheduled time             |
    //   | Outside Window    | Beyond acceptable ±30 minute window         |
    //   | Clinical Risk     | Delayed insulin may affect glucose control  |
    await waitForTestId(page, 'Late Administration');
    assert.strictEqual(await getText(page, 'Late Administration'), '75 minutes past scheduled time');
    assert.strictEqual(await getText(page, 'Outside Window'), 'Beyond acceptable ±30 minute window');
    assert.strictEqual(await getText(page, 'Clinical Risk'), 'Delayed insulin may affect glucose control');

    // And the system requires variance documentation:
    //   | Required Field    | Purpose                                      |
    //   | Delay Reason      | Why medication was not given on time        |
    //   | Patient Status    | Current clinical condition assessment       |
    //   | Physician Notification| Whether MD was contacted about delay    |
    assert.strictEqual(await getText(page, 'Delay Reason Purpose'), 'Why medication was not given on time');
    assert.strictEqual(await getText(page, 'Patient Status Purpose'), 'Current clinical condition assessment');
    assert.strictEqual(await getText(page, 'Physician Notification Purpose'), 'Whether MD was contacted about delay');

    // And I document the variance:
    //   | Field             | Value                                        |
    //   | Delay Reason      | Patient NPO for procedure until 13:00      |
    //   | Patient Assessment| Blood glucose 140 mg/dL, stable            |
    //   | MD Notified       | Dr. Patel aware of delay                    |
    await fillFields(page, [
      { Field: 'Delay Reason', Value: 'Patient NPO for procedure until 13:00' },
      { Field: 'Patient Assessment', Value: 'Blood glucose 140 mg/dL, stable' },
      { Field: 'MD Notified', Value: 'Dr. Patel aware of delay' },
    ]);

    // Then the administration is recorded with variance documentation
    await waitForTestId(page, 'Variance Documentation Status');
    const varianceDocumentationStatus = await getText(page, 'Variance Documentation Status');
    assert.ok(varianceDocumentationStatus.length > 0);
    // And the next scheduled dose timing is adjusted accordingly
    const nextScheduledDose = await getText(page, 'Next Scheduled Dose');
    assert.ok(nextScheduledDose.length > 0);
  });

  test('Administer high-risk medication with double verification', async () => {
    // Given a patient "Susan Williams" is in bed "ED-15"
    // And there is a high-risk medication order:
    //   | Medication    | Dose      | Route | Risk Category        | Special Requirements |
    //   | Heparin       | 5000 units| IV    | High-alert drug      | Double verification  |

    // When I scan the patient's wristband barcode
    await page.getByTestId('scan-wristband-barcode').click();
    // And I scan the heparin medication barcode
    await page.getByTestId('scan-medication-barcode').click();

    // Then the system flags the high-risk medication:
    //   | Alert Type        | Message                                      |
    //   | HIGH-ALERT DRUG   | 🔴 Heparin requires double verification     |
    //   | Risk Factors      | Bleeding risk, dosing errors common         |
    //   | Requirements      | Second nurse must verify before administration|
    await waitForTestId(page, 'HIGH-ALERT DRUG');
    assert.strictEqual(await getText(page, 'HIGH-ALERT DRUG'), '🔴 Heparin requires double verification');
    assert.strictEqual(await getText(page, 'Risk Factors'), 'Bleeding risk, dosing errors common');
    assert.strictEqual(await getText(page, 'Requirements'), 'Second nurse must verify before administration');

    // And the system requires independent double verification:
    //   | Verification Step | Nurse 1 (Primary) | Nurse 2 (Verifying) |
    //   | Patient Identity  | Nurse Johnson      | Pending             |
    //   | Medication        | Verified           | Pending             |
    //   | Dose Calculation  | 5000 units         | Pending             |
    //   | Route/Rate        | IV bolus           | Pending             |
    assert.strictEqual(await getText(page, 'Patient Identity Primary Nurse'), 'Nurse Johnson');
    assert.strictEqual(await getText(page, 'Patient Identity Verifying Nurse'), 'Pending');
    assert.strictEqual(await getText(page, 'Medication Primary Nurse'), 'Verified');
    assert.strictEqual(await getText(page, 'Medication Verifying Nurse'), 'Pending');
    assert.strictEqual(await getText(page, 'Dose Calculation Primary Nurse'), '5000 units');
    assert.strictEqual(await getText(page, 'Dose Calculation Verifying Nurse'), 'Pending');
    assert.strictEqual(await getText(page, 'Route/Rate Primary Nurse'), 'IV bolus');
    assert.strictEqual(await getText(page, 'Route/Rate Verifying Nurse'), 'Pending');

    // When "Nurse Martinez" performs the second verification
    await fillField(page, 'Verifying Nurse', 'Nurse Martinez');
    await page.getByTestId('submit-second-verification').click();
    // And both nurses confirm the administration
    await page.getByTestId('confirm-administration').click();

    // Then the system records dual verification:
    //   | Field             | Value                                        |
    //   | Primary Nurse     | Nurse Johnson                               |
    //   | Verifying Nurse   | Nurse Martinez                              |
    //   | Verification Time | 2025-06-24 15:30:15                        |
    //   | Both Signatures   | Electronic signatures captured              |
    await waitForTestId(page, 'Primary Nurse');
    assert.strictEqual(await getText(page, 'Primary Nurse'), 'Nurse Johnson');
    assert.strictEqual(await getText(page, 'Verifying Nurse'), 'Nurse Martinez');
    assert.strictEqual(await getText(page, 'Verification Time'), '2025-06-24 15:30:15');
    assert.strictEqual(await getText(page, 'Both Signatures'), 'Electronic signatures captured');
  });

  test('Handle medication barcode scanning errors', async () => {
    // Given a patient "Michael Davis" is in bed "ED-11"
    // And I need to administer prescribed medication

    // When I scan the patient's wristband barcode successfully
    await page.getByTestId('scan-wristband-barcode').click();
    // And I attempt to scan a medication barcode that is damaged/unreadable
    await page.getByTestId('scan-medication-barcode').click();

    // Then the system displays a barcode error:
    //   | Error Type        | Message                                      |
    //   | Barcode Unreadable| Unable to scan medication barcode           |
    //   | Manual Options    | Enter medication manually or get replacement|
    //   | Safety Warning    | Manual entry bypasses barcode verification  |
    await waitForTestId(page, 'Barcode Unreadable');
    assert.strictEqual(await getText(page, 'Barcode Unreadable'), 'Unable to scan medication barcode');
    assert.strictEqual(await getText(page, 'Manual Options'), 'Enter medication manually or get replacement');
    assert.strictEqual(await getText(page, 'Safety Warning'), 'Manual entry bypasses barcode verification');

    // And I choose to manually enter the medication information:
    //   | Manual Entry Field| Value                                        |
    //   | Medication Name   | Tylenol                                     |
    //   | Strength          | 650mg                                       |
    //   | NDC Number        | 50580-506-02                               |
    //   | Lot Number        | ABC123                                      |
    //   | Expiration Date   | 2026-12-31                                 |
    await fillFields(page, [
      { Field: 'Medication Name', Value: 'Tylenol' },
      { Field: 'Strength', Value: '650mg' },
      { Field: 'NDC Number', Value: '50580-506-02' },
      { Field: 'Lot Number', Value: 'ABC123' },
      { Field: 'Expiration Date', Value: '2026-12-31' },
    ]);

    // Then the system validates the manual entry against the order
    const manualEntryValidation = await waitForTestId(page, 'Manual Entry Validation');
    assert.ok(await manualEntryValidation.isVisible());
    // And requires supervisor override for manual medication entry
    await waitForTestId(page, 'Supervisor Override');
    // And documents the barcode scanning issue for pharmacy review
    const pharmacyReviewFlag = await waitForTestId(page, 'Pharmacy Review Flag');
    assert.ok(await pharmacyReviewFlag.isVisible());
  });

  test('Administer pediatric medication with weight-based verification', async () => {
    // Given a pediatric patient "Emma Foster" (age 5, weight 18kg) is in bed "ED-PEDS-1"
    // And there is a weight-based medication order:
    //   | Medication     | Dose Calculation    | Prescribed Dose | Route |
    //   | Acetaminophen  | 15mg/kg            | 270mg          | PO    |

    // When I scan the patient's wristband barcode
    await page.getByTestId('scan-wristband-barcode').click();
    // And I scan the acetaminophen medication barcode
    await page.getByTestId('scan-medication-barcode').click();

    // Then the system verifies pediatric dosing:
    //   | Verification       | Calculation                                  | Status   |
    //   | Weight Confirmation| Patient weight: 18kg                        | ✓ Valid  |
    //   | Dose Calculation   | 15mg/kg × 18kg = 270mg                     | ✓ Correct|
    //   | Maximum Safe Dose  | 270mg < 400mg max (safe)                    | ✓ Safe   |
    //   | Age Appropriateness| Acetaminophen approved for age 5            | ✓ Valid  |
    await waitForTestId(page, 'Weight Confirmation Status');
    assert.strictEqual(await getText(page, 'Weight Confirmation Status'), '✓ Valid');
    assert.strictEqual(await getText(page, 'Dose Calculation Status'), '✓ Correct');
    assert.strictEqual(await getText(page, 'Maximum Safe Dose Status'), '✓ Safe');
    assert.strictEqual(await getText(page, 'Age Appropriateness Status'), '✓ Valid');

    // And the system displays pediatric-specific information:
    //   | Field              | Value                                        |
    //   | Patient Age/Weight | 5 years old, 18kg                          |
    //   | Calculation Shown  | 15mg/kg × 18kg = 270mg                     |
    //   | Liquid Formulation | 160mg/5mL suspension                       |
    //   | Volume to Give     | 8.4mL                                       |
    assert.strictEqual(await getText(page, 'Patient Age/Weight'), '5 years old, 18kg');
    assert.strictEqual(await getText(page, 'Calculation Shown'), '15mg/kg × 18kg = 270mg');
    assert.strictEqual(await getText(page, 'Liquid Formulation'), '160mg/5mL suspension');
    assert.strictEqual(await getText(page, 'Volume to Give'), '8.4mL');

    // And I confirm the pediatric administration
    await page.getByTestId('confirm-administration').click();

    // Then the system records with pediatric-specific documentation
    const pediatricDocumentation = await waitForTestId(page, 'Pediatric Documentation');
    assert.ok(await pediatricDocumentation.isVisible());
  });

  test('Handle medication administration during code blue emergency', async () => {
    // Given a patient "Crisis Patient" is in bed "ED-TRAUMA-1"
    // And a code blue emergency is in progress
    // And emergency medications are ordered:
    //   | Medication    | Dose      | Route | Urgency    |
    //   | Epinephrine   | 1mg       | IV    | STAT       |
    //   | Atropine      | 0.5mg     | IV    | STAT       |

    // When I scan the patient's wristband during the emergency
    await page.getByTestId('scan-wristband-barcode').click();
    // And I scan the epinephrine medication barcode
    await page.getByTestId('scan-medication-barcode').click();

    // Then the system activates emergency administration mode:
    //   | Emergency Feature | Behavior                                     |
    //   | Rapid Verification| Abbreviated safety checks for life-saving   |
    //   | Time Documentation| Precise timestamp for code blue timeline    |
    //   | Team Notification | Alert code team of medication administration |
    await waitForTestId(page, 'Rapid Verification');
    assert.strictEqual(await getText(page, 'Rapid Verification'), 'Abbreviated safety checks for life-saving');
    assert.strictEqual(await getText(page, 'Time Documentation'), 'Precise timestamp for code blue timeline');
    assert.strictEqual(await getText(page, 'Team Notification'), 'Alert code team of medication administration');

    // And the system allows emergency override of timing restrictions
    await waitForTestId(page, 'Emergency Override');

    // And records the administration with code blue context:
    //   | Field             | Value                                        |
    //   | Emergency Context | Code Blue - Cardiac arrest                  |
    //   | Rapid Administration| Life-saving intervention                   |
    //   | Code Blue Timeline| 15:45:32 - Epinephrine given               |
    assert.strictEqual(await getText(page, 'Emergency Context'), 'Code Blue - Cardiac arrest');
    assert.strictEqual(await getText(page, 'Rapid Administration'), 'Life-saving intervention');
    assert.strictEqual(await getText(page, 'Code Blue Timeline'), '15:45:32 - Epinephrine given');

    // And the code blue medication log is automatically updated
    const codeBlueMedicationLog = await waitForTestId(page, 'Code Blue Medication Log');
    assert.ok(await codeBlueMedicationLog.isVisible());
  });
});
