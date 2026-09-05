// Selenium WebDriver + Mocha test for
// spec/features/12-allergy-check.feature
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see support/fields.js) and the shared
// data-testid contract in support/login.js (login-identity, login-submit,
// app-root).

import { strict as assert } from 'assert';
import { buildDriver } from './support/build-driver.js';
import { login, verifySystemIsOperational } from './support/login.js';
import { fillFields, fillField, getText, waitForTestId } from './support/fields.js';
import { By } from 'selenium-webdriver';

describe('Feature: Allergy Check', function () {
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
    //   And I am logged in as "Dr. Smith"
    //   And the allergy checking module is active
    //   And the drug interaction database is up-to-date
    await verifySystemIsOperational(driver);
    await login(driver, 'Dr. Smith');
    // The allergy checking module and the drug interaction database being
    // up-to-date are assumed to be pre-seeded test environment state.

    const allergyCheckNavLink = await waitForTestId(driver, 'Nav Allergy Check');
    await allergyCheckNavLink.click();
    await waitForTestId(driver, 'Allergy Check Panel');
  });

  it('Prescribe penicillin to patient with documented penicillin allergy', async () => {
    // Given a patient "Maria Rodriguez" is in bed "ED-8"
    // And the patient has documented allergies:
    //   | Allergy        | Reaction Type       | Severity  | Date Documented | Source     |
    //   | Penicillin     | Rash, hives        | Moderate  | 2023-05-15     | Patient    |
    //   | Shellfish      | Anaphylaxis        | Severe    | 2022-08-10     | Patient    |

    // When I enter a medication order for:
    //   | Medication     | Dose      | Route | Frequency | Duration |
    //   | Penicillin VK  | 500mg     | PO    | QID       | 10 days  |
    await fillFields(driver, [
      { Field: 'Medication', Value: 'Penicillin VK' },
      { Field: 'Dose', Value: '500mg' },
      { Field: 'Route', Value: 'PO' },
      { Field: 'Frequency', Value: 'QID' },
      { Field: 'Duration', Value: '10 days' },
    ]);
    // And I submit the order
    await driver.findElement(By.css('[data-testid="submit-medication-order"]')).click();

    // Then the system displays an allergy warning:
    //   | Alert Type         | Details                                      |
    //   | DRUG ALLERGY       | ⚠️ ALLERGY ALERT: Patient allergic to Penicillin |
    //   | Severity Level     | Moderate                                     |
    //   | Reaction Type      | Rash, hives                                  |
    //   | Date Documented    | May 15, 2023                                |
    //   | Source             | Patient reported                             |
    await waitForTestId(driver, 'DRUG ALLERGY');
    assert.strictEqual(await getText(driver, 'DRUG ALLERGY'), '⚠️ ALLERGY ALERT: Patient allergic to Penicillin');
    assert.strictEqual(await getText(driver, 'Severity Level'), 'Moderate');
    assert.strictEqual(await getText(driver, 'Reaction Type'), 'Rash, hives');
    assert.strictEqual(await getText(driver, 'Date Documented'), 'May 15, 2023');
    assert.strictEqual(await getText(driver, 'Source'), 'Patient reported');

    // And the system blocks the order submission
    const orderStatus = await getText(driver, 'Order Status');
    assert.match(orderStatus, /blocked/i);

    // And I am presented with options:
    //   | Option             | Description                                  |
    //   | Cancel Order       | Remove penicillin order                      |
    //   | Override with Reason| Document clinical justification            |
    //   | Alternative Drugs  | View suggested alternative antibiotics       |
    await waitForTestId(driver, 'Cancel Order');
    await waitForTestId(driver, 'Override with Reason');
    await waitForTestId(driver, 'Alternative Drugs');

    // And the allergy alert is logged in the audit trail
    const auditTrailEntry = await waitForTestId(driver, 'Audit Trail Entry');
    assert.ok(await auditTrailEntry.isDisplayed());
  });

  it('Prescribe medication with no documented allergies', async () => {
    // Given a patient "John Taylor" is in bed "ED-12"
    // And the patient has no documented allergies

    // When I enter a medication order for:
    //   | Medication     | Dose      | Route | Frequency |
    //   | Amoxicillin    | 875mg     | PO    | BID       |
    await fillFields(driver, [
      { Field: 'Medication', Value: 'Amoxicillin' },
      { Field: 'Dose', Value: '875mg' },
      { Field: 'Route', Value: 'PO' },
      { Field: 'Frequency', Value: 'BID' },
    ]);
    // And I submit the order
    await driver.findElement(By.css('[data-testid="submit-medication-order"]')).click();

    // Then the system performs allergy checking
    // And no allergy alerts are triggered
    const allergyAlerts = await driver.findElements(By.css('[data-testid="allergy-alert"]'));
    assert.strictEqual(allergyAlerts.length, 0);

    // And the order is processed normally
    // And the system displays confirmation:
    //   | Confirmation Type  | Message                                      |
    //   | No Allergies Found | No known allergies to Amoxicillin          |
    //   | Order Status       | Order submitted successfully                 |
    await waitForTestId(driver, 'No Allergies Found');
    assert.strictEqual(await getText(driver, 'No Allergies Found'), 'No known allergies to Amoxicillin');
    assert.strictEqual(await getText(driver, 'Order Status'), 'Order submitted successfully');

    // And the medication order is routed to pharmacy
    const pharmacyRoutingStatus = await getText(driver, 'Pharmacy Routing Status');
    assert.match(pharmacyRoutingStatus, /pharmacy/i);
  });

  it('Prescribe medication with cross-reactive allergy', async () => {
    // Given a patient "Sarah Johnson" is in bed "ED-5"
    // And the patient has documented allergies:
    //   | Allergy        | Reaction Type       | Severity  |
    //   | Penicillin     | Respiratory distress| Severe    |

    // When I enter a medication order for:
    //   | Medication     | Dose      | Route | Frequency |
    //   | Amoxicillin    | 500mg     | PO    | TID       |
    await fillFields(driver, [
      { Field: 'Medication', Value: 'Amoxicillin' },
      { Field: 'Dose', Value: '500mg' },
      { Field: 'Route', Value: 'PO' },
      { Field: 'Frequency', Value: 'TID' },
    ]);
    // And I submit the order
    await driver.findElement(By.css('[data-testid="submit-medication-order"]')).click();

    // Then the system displays a cross-reactivity warning:
    //   | Alert Type         | Details                                      |
    //   | CROSS-REACTIVITY   | ⚠️ WARNING: Cross-reactivity with Penicillin|
    //   | Known Allergy      | Patient allergic to Penicillin (Severe)     |
    //   | Cross-Reaction Risk| Amoxicillin is a penicillin derivative     |
    //   | Reaction Type      | Respiratory distress                         |
    //   | Risk Level         | High - Severe reaction possible              |
    await waitForTestId(driver, 'CROSS-REACTIVITY');
    assert.strictEqual(await getText(driver, 'CROSS-REACTIVITY'), '⚠️ WARNING: Cross-reactivity with Penicillin');
    assert.strictEqual(await getText(driver, 'Known Allergy'), 'Patient allergic to Penicillin (Severe)');
    assert.strictEqual(await getText(driver, 'Cross-Reaction Risk'), 'Amoxicillin is a penicillin derivative');
    assert.strictEqual(await getText(driver, 'Reaction Type'), 'Respiratory distress');
    assert.strictEqual(await getText(driver, 'Risk Level'), 'High - Severe reaction possible');

    // And the system provides additional information:
    //   | Information Type   | Content                                      |
    //   | Cross-Reaction Rate| 8-10% cross-reactivity with penicillin     |
    //   | Clinical Guidance  | Consider non-beta-lactam alternatives       |
    //   | Emergency Prep     | Have epinephrine available if administered   |
    assert.strictEqual(await getText(driver, 'Cross-Reaction Rate'), '8-10% cross-reactivity with penicillin');
    assert.strictEqual(await getText(driver, 'Clinical Guidance'), 'Consider non-beta-lactam alternatives');
    assert.strictEqual(await getText(driver, 'Emergency Prep'), 'Have epinephrine available if administered');

    // And I must acknowledge the cross-reactivity risk before proceeding
    await waitForTestId(driver, 'Acknowledge Cross-Reactivity Risk');
  });

  it('Override allergy alert with clinical justification', async () => {
    // Given a patient "Michael Chen" is in bed "ED-15"
    // And the patient has a documented penicillin allergy with "mild rash"
    // And the patient has severe sepsis requiring immediate antibiotic treatment

    // When I enter a penicillin order and receive an allergy alert
    await fillField(driver, 'Medication', 'Penicillin');
    await driver.findElement(By.css('[data-testid="submit-medication-order"]')).click();
    await waitForTestId(driver, 'DRUG ALLERGY');
    // And I choose to override the allergy warning
    await driver.findElement(By.css('[data-testid="override-allergy-warning"]')).click();

    // Then the system requires detailed justification:
    //   | Required Field     | Description                                  |
    //   | Clinical Rationale | Why this medication is medically necessary   |
    //   | Risk Assessment    | Evaluation of allergy risk vs benefit       |
    //   | Monitoring Plan    | How allergic reactions will be monitored    |
    //   | Alternative Review | Why alternatives are not suitable            |
    await waitForTestId(driver, 'Clinical Rationale Description');
    assert.strictEqual(await getText(driver, 'Clinical Rationale Description'), 'Why this medication is medically necessary');
    assert.strictEqual(await getText(driver, 'Risk Assessment Description'), 'Evaluation of allergy risk vs benefit');
    assert.strictEqual(await getText(driver, 'Monitoring Plan Description'), 'How allergic reactions will be monitored');
    assert.strictEqual(await getText(driver, 'Alternative Review Description'), 'Why alternatives are not suitable');

    // And I document the override:
    //   | Field              | Value                                        |
    //   | Clinical Rationale | Life-threatening sepsis, first-line antibiotic needed |
    //   | Risk Assessment    | Mild rash risk acceptable vs sepsis mortality |
    //   | Monitoring Plan    | Continuous monitoring, diphenhydramine available |
    //   | Alternative Review | Other antibiotics inadequate for organism     |
    await fillFields(driver, [
      { Field: 'Clinical Rationale', Value: 'Life-threatening sepsis, first-line antibiotic needed' },
      { Field: 'Risk Assessment', Value: 'Mild rash risk acceptable vs sepsis mortality' },
      { Field: 'Monitoring Plan', Value: 'Continuous monitoring, diphenhydramine available' },
      { Field: 'Alternative Review', Value: 'Other antibiotics inadequate for organism' },
    ]);
    await driver.findElement(By.css('[data-testid="submit-override-documentation"]')).click();

    // Then the system accepts the override
    await waitForTestId(driver, 'Override Status');
    const overrideStatus = await getText(driver, 'Override Status');
    assert.match(overrideStatus, /accepted/i);

    // And logs the override decision with full documentation
    const overrideAuditLog = await waitForTestId(driver, 'Override Audit Log');
    assert.ok(await overrideAuditLog.isDisplayed());

    // And notifies nursing staff of the allergy override for enhanced monitoring
    const nursingNotification = await getText(driver, 'Nursing Notification');
    assert.match(nursingNotification, /enhanced monitoring/i);
  });

  it('Check allergies for multiple medications simultaneously', async () => {
    // Given a patient "Lisa Brown" is in bed "ED-7"
    // And the patient has documented allergies:
    //   | Allergy        | Reaction Type       | Severity  |
    //   | Morphine       | Respiratory depression | Severe |
    //   | NSAIDs         | GI bleeding        | Moderate  |

    // When I enter multiple medication orders:
    //   | Medication     | Dose      | Route | Purpose           |
    //   | Fentanyl       | 50mcg     | IV    | Pain control      |
    //   | Ibuprofen      | 600mg     | PO    | Anti-inflammatory |
    //   | Acetaminophen  | 650mg     | PO    | Pain/fever        |
    await fillFields(driver, [
      { Field: 'Medication 1', Value: 'Fentanyl' },
      { Field: 'Dose 1', Value: '50mcg' },
      { Field: 'Route 1', Value: 'IV' },
      { Field: 'Purpose 1', Value: 'Pain control' },
      { Field: 'Medication 2', Value: 'Ibuprofen' },
      { Field: 'Dose 2', Value: '600mg' },
      { Field: 'Route 2', Value: 'PO' },
      { Field: 'Purpose 2', Value: 'Anti-inflammatory' },
      { Field: 'Medication 3', Value: 'Acetaminophen' },
      { Field: 'Dose 3', Value: '650mg' },
      { Field: 'Route 3', Value: 'PO' },
      { Field: 'Purpose 3', Value: 'Pain/fever' },
    ]);
    // And I submit all orders simultaneously
    await driver.findElement(By.css('[data-testid="submit-medication-orders"]')).click();

    // Then the system checks each medication against documented allergies:
    //   | Medication     | Allergy Status | Alert Level |
    //   | Fentanyl       | No direct allergy | Safe      |
    //   | Ibuprofen      | NSAID allergy  | WARNING   |
    //   | Acetaminophen  | No allergy     | Safe      |
    await waitForTestId(driver, 'Fentanyl Alert Level');
    assert.strictEqual(await getText(driver, 'Fentanyl Allergy Status'), 'No direct allergy');
    assert.strictEqual(await getText(driver, 'Fentanyl Alert Level'), 'Safe');
    assert.strictEqual(await getText(driver, 'Ibuprofen Allergy Status'), 'NSAID allergy');
    assert.strictEqual(await getText(driver, 'Ibuprofen Alert Level'), 'WARNING');
    assert.strictEqual(await getText(driver, 'Acetaminophen Allergy Status'), 'No allergy');
    assert.strictEqual(await getText(driver, 'Acetaminophen Alert Level'), 'Safe');

    // And I receive specific alerts for problematic medications:
    //   | Alert Medication | Warning Message                              |
    //   | Ibuprofen        | Patient allergic to NSAIDs - GI bleeding risk |
    assert.strictEqual(await getText(driver, 'Ibuprofen Warning Message'), 'Patient allergic to NSAIDs - GI bleeding risk');

    // And safe medications are processed without alerts
    const fentanylWarnings = await driver.findElements(By.css('[data-testid="fentanyl-warning-message"]'));
    assert.strictEqual(fentanylWarnings.length, 0);

    // And I can review and modify orders before final submission
    await waitForTestId(driver, 'Review and Modify Orders');
  });

  it('Handle unknown or "No Known Allergies" status', async () => {
    // Given a patient "Robert Davis" is in bed "ED-3"
    // And the patient's allergy status is "Unknown - Unable to assess"

    // When I enter a medication order for:
    //   | Medication     | Dose      | Route |
    //   | Cephalexin     | 500mg     | PO    |
    await fillFields(driver, [
      { Field: 'Medication', Value: 'Cephalexin' },
      { Field: 'Dose', Value: '500mg' },
      { Field: 'Route', Value: 'PO' },
    ]);
    // And I submit the order
    await driver.findElement(By.css('[data-testid="submit-medication-order"]')).click();

    // Then the system displays an information alert:
    //   | Alert Type         | Message                                      |
    //   | ALLERGY UNKNOWN    | ⚠️ INFO: Patient allergy status unknown     |
    //   | Risk Consideration | Cannot verify medication allergies          |
    //   | Recommendation     | Consider allergy assessment before administration |
    await waitForTestId(driver, 'ALLERGY UNKNOWN');
    assert.strictEqual(await getText(driver, 'ALLERGY UNKNOWN'), '⚠️ INFO: Patient allergy status unknown');
    assert.strictEqual(await getText(driver, 'Risk Consideration'), 'Cannot verify medication allergies');
    assert.strictEqual(await getText(driver, 'Recommendation'), 'Consider allergy assessment before administration');

    // And the system provides safety recommendations:
    //   | Recommendation     | Details                                      |
    //   | Allergy Assessment | Attempt to obtain allergy history           |
    //   | Start Monitoring   | Monitor for allergic reactions closely      |
    //   | Have Antidotes Ready| Ensure emergency medications available      |
    assert.strictEqual(await getText(driver, 'Allergy Assessment'), 'Attempt to obtain allergy history');
    assert.strictEqual(await getText(driver, 'Start Monitoring'), 'Monitor for allergic reactions closely');
    assert.strictEqual(await getText(driver, 'Have Antidotes Ready'), 'Ensure emergency medications available');

    // And the order is flagged for enhanced allergy monitoring
    const enhancedMonitoringFlag = await getText(driver, 'Enhanced Monitoring Flag');
    assert.match(enhancedMonitoringFlag, /enhanced allergy monitoring/i);
  });

  it('Check for drug class allergies', async () => {
    // Given a patient "Jennifer Wilson" is in bed "ED-11"
    // And the patient has documented allergies:
    //   | Allergy        | Reaction Type       | Severity  | Drug Class |
    //   | Sulfa drugs    | Stevens-Johnson syndrome | Severe | Sulfonamides |

    // When I enter a medication order for:
    //   | Medication           | Dose      | Route | Drug Class    |
    //   | Trimethoprim-Sulfamethoxazole | 800mg | PO | Sulfonamide |
    await fillFields(driver, [
      { Field: 'Medication', Value: 'Trimethoprim-Sulfamethoxazole' },
      { Field: 'Dose', Value: '800mg' },
      { Field: 'Route', Value: 'PO' },
      { Field: 'Drug Class', Value: 'Sulfonamide' },
    ]);
    // And I submit the order
    await driver.findElement(By.css('[data-testid="submit-medication-order"]')).click();

    // Then the system identifies the drug class allergy:
    //   | Alert Type         | Details                                      |
    //   | DRUG CLASS ALLERGY | ⚠️ SEVERE: Patient allergic to Sulfa drugs |
    //   | Specific Drug      | TMP-SMX contains sulfamethoxazole           |
    //   | Reaction History   | Stevens-Johnson syndrome                     |
    //   | Severity           | Severe - Life-threatening reaction possible  |
    await waitForTestId(driver, 'DRUG CLASS ALLERGY');
    assert.strictEqual(await getText(driver, 'DRUG CLASS ALLERGY'), '⚠️ SEVERE: Patient allergic to Sulfa drugs');
    assert.strictEqual(await getText(driver, 'Specific Drug'), 'TMP-SMX contains sulfamethoxazole');
    assert.strictEqual(await getText(driver, 'Reaction History'), 'Stevens-Johnson syndrome');
    assert.strictEqual(await getText(driver, 'Severity'), 'Severe - Life-threatening reaction possible');

    // And the system provides drug class education:
    //   | Information        | Content                                      |
    //   | Drug Class         | Sulfonamide antibiotics                     |
    //   | Cross-Reactivity   | All sulfa-containing medications at risk    |
    //   | Alternative Classes| Beta-lactams, fluoroquinolones available   |
    assert.strictEqual(await getText(driver, 'Drug Class'), 'Sulfonamide antibiotics');
    assert.strictEqual(await getText(driver, 'Cross-Reactivity'), 'All sulfa-containing medications at risk');
    assert.strictEqual(await getText(driver, 'Alternative Classes'), 'Beta-lactams, fluoroquinolones available');
  });

  it('Handle allergy information from multiple sources', async () => {
    // Given a patient "David Kim" is in bed "ED-9"
    // And the patient has allergy information from multiple sources:
    //   | Source             | Allergy    | Reaction        | Reliability |
    //   | Patient Report     | Penicillin | "Bad reaction"  | Unverified  |
    //   | Medical Records    | Penicillin | Urticaria, rash | Verified    |
    //   | Family Member      | Codeine    | Nausea         | Unverified  |

    // When I enter a penicillin order
    await fillField(driver, 'Medication', 'Penicillin');
    await driver.findElement(By.css('[data-testid="submit-medication-order"]')).click();

    // Then the system displays comprehensive allergy information:
    //   | Source Type        | Allergy Details                              |
    //   | Verified Record    | Penicillin - Urticaria, rash (Medical Records) |
    //   | Patient Report     | Penicillin - "Bad reaction" (Unverified)    |
    await waitForTestId(driver, 'Verified Record');
    assert.strictEqual(await getText(driver, 'Verified Record'), 'Penicillin - Urticaria, rash (Medical Records)');
    assert.strictEqual(await getText(driver, 'Patient Report'), 'Penicillin - "Bad reaction" (Unverified)');

    // And the system prioritizes verified information in the alert
    // And provides source credibility indicators:
    //   | Source             | Credibility Level | Clinical Weight      |
    //   | Medical Records    | High reliability  | Primary consideration |
    //   | Patient Report     | Moderate reliability | Secondary consideration |
    assert.strictEqual(await getText(driver, 'Medical Records Credibility Level'), 'High reliability');
    assert.strictEqual(await getText(driver, 'Medical Records Clinical Weight'), 'Primary consideration');
    assert.strictEqual(await getText(driver, 'Patient Report Credibility Level'), 'Moderate reliability');
    assert.strictEqual(await getText(driver, 'Patient Report Clinical Weight'), 'Secondary consideration');

    // And I can review detailed allergy history before making decisions
    await waitForTestId(driver, 'Detailed Allergy History');
  });

  it('Real-time allergy checking during order modification', async () => {
    // Given a patient "Susan Martinez" is in bed "ED-4"
    // And the patient has a penicillin allergy
    // And I have started entering a medication order

    // When I begin typing "Pen" in the medication field
    await fillField(driver, 'Medication', 'Pen');

    // Then the system provides real-time allergy warnings:
    //   | Alert Type         | Message                                      |
    //   | PREDICTIVE ALERT   | ⚠️ Patient allergic to Penicillin          |
    //   | Medication Match   | "Pen" may be penicillin-related drug       |
    //   | Suggestion         | Consider alternative antibiotics            |
    await waitForTestId(driver, 'PREDICTIVE ALERT');
    assert.strictEqual(await getText(driver, 'PREDICTIVE ALERT'), '⚠️ Patient allergic to Penicillin');
    assert.strictEqual(await getText(driver, 'Medication Match'), '"Pen" may be penicillin-related drug');
    assert.strictEqual(await getText(driver, 'Suggestion'), 'Consider alternative antibiotics');

    // And the system highlights potential allergy matches as I type
    await waitForTestId(driver, 'Allergy Match Highlight');

    // And provides alternative medication suggestions:
    //   | Alternative        | Drug Class        | Reason               |
    //   | Cephalexin         | Cephalosporin     | Lower cross-reactivity |
    //   | Azithromycin       | Macrolide         | No cross-reactivity   |
    //   | Ciprofloxacin      | Fluoroquinolone   | Different mechanism   |
    assert.strictEqual(await getText(driver, 'Cephalexin Drug Class'), 'Cephalosporin');
    assert.strictEqual(await getText(driver, 'Cephalexin Reason'), 'Lower cross-reactivity');
    assert.strictEqual(await getText(driver, 'Azithromycin Drug Class'), 'Macrolide');
    assert.strictEqual(await getText(driver, 'Azithromycin Reason'), 'No cross-reactivity');
    assert.strictEqual(await getText(driver, 'Ciprofloxacin Drug Class'), 'Fluoroquinolone');
    assert.strictEqual(await getText(driver, 'Ciprofloxacin Reason'), 'Different mechanism');

    // And I can select alternatives directly from the suggestion list
    await waitForTestId(driver, 'Alternative Suggestion List');
  });
});
