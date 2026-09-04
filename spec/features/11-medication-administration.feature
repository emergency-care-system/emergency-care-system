# # Medication Administration
#
# **Key Components:**
#
# - Barcode verification system ensuring the five rights of medication administration
#
# - Real-time safety checks including allergy alerts and drug interactions
#
# - Comprehensive documentation with timestamps and verification methods
#
# - Clinical decision support for PRN medications and high-risk drugs
#
# **Comprehensive Scenarios:**
#
# - Standard medication administration - Basic workflow with barcode verification and five rights checking
#
# - Allergy alert handling - Safety alerts with physician override requirements
#
# - PRN medication administration - Clinical assessment and indication documentation
#
# - Timing variance management - Handling late or early medication administration
#
# - High-risk medication protocols - Double verification requirements for dangerous drugs
#
# - Barcode scanning errors - Manual entry procedures with supervisor oversight
#
# - Pediatric medication safety - Weight-based dosing verification and calculations
#
# - Emergency administration - Code blue procedures with rapid verification modes
#
# **Healthcare-Specific Features:**
#
# - Five rights of medication administration (patient, drug, dose, route, time)
#
# - Drug allergy and interaction checking with cross-reactivity alerts
#
# - PRN medication clinical assessment requirements
#
# - High-alert drug double verification protocols
#
# - Weight-based pediatric dosing calculations
#
# - Medication timing windows and variance documentation
#
# - Emergency override capabilities for life-saving situations
#
# - Comprehensive audit trails with electronic signatures
#
# - Integration with medication administration records (MAR)
#
# - Real-time safety alerts and clinical decision support
#
# - Supervisor override requirements for safety bypasses
#
# - Code blue emergency medication tracking

Feature: Medication Administration
  As a nurse
  I want to safely administer medications using barcode verification
  So that I can ensure the five rights of medication administration and maintain accurate documentation

  Background:
    Given the ED management system is operational
    And I am logged in as "Nurse Johnson"
    And the medication administration module is active
    And the barcode scanning system is functional
    And the medication administration record (MAR) is accessible

  Scenario: Successfully administer scheduled medication with barcode verification
    Given a patient "Maria Gonzalez" is in bed "ED-8"
    And the patient has a prescribed medication:
      | Medication    | Dose      | Route | Frequency | Scheduled Time | Status    |
      | Metoprolol    | 25mg      | PO    | BID       | 14:00         | Due       |
    And the patient is wearing a wristband with barcode "PT-12345"
    And I have the medication with barcode "MED-METOPROLOL-25MG"
    When I scan the patient's wristband barcode "PT-12345"
    And I scan the medication barcode "MED-METOPROLOL-25MG"
    Then the system verifies the five rights of medication administration:
      | Right          | Verification                                  | Status   |
      | Right Patient  | Maria Gonzalez confirmed via wristband       | ✓ Valid  |
      | Right Drug     | Metoprolol matches prescribed medication     | ✓ Valid  |
      | Right Dose     | 25mg matches prescribed dose                 | ✓ Valid  |
      | Right Route    | PO (oral) matches prescribed route           | ✓ Valid  |
      | Right Time     | Within acceptable window (13:30-14:30)      | ✓ Valid  |
    And the system displays confirmation screen:
      | Field             | Value                                        |
      | Patient           | Maria Gonzalez, DOB: 1975-08-15            |
      | Medication        | Metoprolol 25mg                            |
      | Route             | PO (By mouth)                              |
      | Administration Time| 14:05                                      |
    And I confirm the administration
    Then the system records the administration with timestamp:
      | Field             | Value                                        |
      | Patient ID        | PT-12345                                   |
      | Medication        | Metoprolol 25mg PO                         |
      | Administered By   | Nurse Johnson                              |
      | Administration Time| 2025-06-24 14:05:32                       |
      | Verification Method| Barcode scan                              |
    And the medication status is updated to "Given"
    And the next dose is automatically scheduled for "02:00 tomorrow"

  Scenario: Handle medication administration with allergy alert
    Given a patient "Robert Chen" is in bed "ED-12"
    And the patient has documented allergies:
      | Allergy       | Reaction Type      | Severity  |
      | Penicillin    | Rash, hives       | Moderate  |
      | Morphine      | Respiratory depression | Severe |
    And there is a prescribed medication:
      | Medication    | Dose      | Route | Prescriber |
      | Amoxicillin   | 500mg     | PO    | Dr. Smith  |
    When I scan the patient's wristband barcode
    And I scan the amoxicillin medication barcode
    Then the system displays an allergy alert:
      | Alert Type        | Message                                      |
      | DRUG ALLERGY      | ⚠️ WARNING: Patient allergic to Penicillin |
      | Cross-Reaction    | Amoxicillin contains penicillin             |
      | Severity          | Moderate - Rash, hives                      |
      | Recommendation    | Contact physician before administration      |
    And the system requires additional verification:
      | Verification Step | Requirement                                  |
      | Physician Approval| Must have override from prescribing MD      |
      | Documentation     | Must document reason for override           |
      | Monitoring Plan   | Must specify allergy monitoring protocol    |
    And the medication administration is held pending physician confirmation
    And an alert is sent to the prescribing physician "Dr. Smith"

  Scenario: Administer PRN medication with clinical assessment
    Given a patient "Jennifer Lopez" is in bed "ED-6"
    And there is a PRN medication order:
      | Medication     | Dose    | Route | Indication           | Max Frequency |
      | Morphine       | 2mg     | IV    | Pain score ≥7/10     | Q4H PRN       |
    And the patient's current pain score is "8/10"
    And the last morphine dose was given 5 hours ago
    When I scan the patient's wristband barcode
    And I scan the morphine medication barcode
    Then the system verifies PRN medication criteria:
      | Criteria          | Assessment                                   | Status   |
      | Clinical Indication| Pain score 8/10 meets threshold ≥7/10      | ✓ Met    |
      | Frequency Limit   | Last dose 5h ago, within Q4H limit         | ✓ Valid  |
      | Patient Safety    | No respiratory contraindications            | ✓ Safe   |
    And I document the clinical assessment:
      | Assessment Field  | Value                                        |
      | Pain Score        | 8/10                                        |
      | Pain Location     | Chest                                       |
      | Pain Quality      | Sharp, stabbing                             |
      | Vital Signs       | BP: 140/85, HR: 95, RR: 18, O2: 96%       |
    And I confirm the PRN administration
    Then the system records the administration with clinical justification:
      | Documentation     | Details                                      |
      | PRN Indication    | Pain score 8/10, patient requested relief   |
      | Clinical Assessment| Stable vitals, no respiratory distress     |
      | Next Available    | Not before 18:15 (Q4H)                     |

  Scenario: Handle medication administration timing variances
    Given a patient "David Kim" is in bed "ED-3"
    And there is a scheduled medication:
      | Medication    | Scheduled Time | Acceptable Window | Current Time |
      | Insulin       | 12:00         | ±30 minutes       | 13:15        |
    When I scan the patient's wristband barcode
    And I scan the insulin medication barcode
    Then the system detects a timing variance:
      | Variance Type     | Details                                      |
      | Late Administration| 75 minutes past scheduled time             |
      | Outside Window    | Beyond acceptable ±30 minute window         |
      | Clinical Risk     | Delayed insulin may affect glucose control  |
    And the system requires variance documentation:
      | Required Field    | Purpose                                      |
      | Delay Reason      | Why medication was not given on time        |
      | Patient Status    | Current clinical condition assessment       |
      | Physician Notification| Whether MD was contacted about delay    |
    And I document the variance:
      | Field             | Value                                        |
      | Delay Reason      | Patient NPO for procedure until 13:00      |
      | Patient Assessment| Blood glucose 140 mg/dL, stable            |
      | MD Notified       | Dr. Patel aware of delay                    |
    Then the administration is recorded with variance documentation
    And the next scheduled dose timing is adjusted accordingly

  Scenario: Administer high-risk medication with double verification
    Given a patient "Susan Williams" is in bed "ED-15"
    And there is a high-risk medication order:
      | Medication    | Dose      | Route | Risk Category        | Special Requirements |
      | Heparin       | 5000 units| IV    | High-alert drug      | Double verification  |
    When I scan the patient's wristband barcode
    And I scan the heparin medication barcode
    Then the system flags the high-risk medication:
      | Alert Type        | Message                                      |
      | HIGH-ALERT DRUG   | 🔴 Heparin requires double verification     |
      | Risk Factors      | Bleeding risk, dosing errors common         |
      | Requirements      | Second nurse must verify before administration|
    And the system requires independent double verification:
      | Verification Step | Nurse 1 (Primary) | Nurse 2 (Verifying) |
      | Patient Identity  | Nurse Johnson      | Pending             |
      | Medication        | Verified           | Pending             |
      | Dose Calculation  | 5000 units         | Pending             |
      | Route/Rate        | IV bolus           | Pending             |
    When "Nurse Martinez" performs the second verification
    And both nurses confirm the administration
    Then the system records dual verification:
      | Field             | Value                                        |
      | Primary Nurse     | Nurse Johnson                               |
      | Verifying Nurse   | Nurse Martinez                              |
      | Verification Time | 2025-06-24 15:30:15                        |
      | Both Signatures   | Electronic signatures captured              |

  Scenario: Handle medication barcode scanning errors
    Given a patient "Michael Davis" is in bed "ED-11"
    And I need to administer prescribed medication
    When I scan the patient's wristband barcode successfully
    And I attempt to scan a medication barcode that is damaged/unreadable
    Then the system displays a barcode error:
      | Error Type        | Message                                      |
      | Barcode Unreadable| Unable to scan medication barcode           |
      | Manual Options    | Enter medication manually or get replacement|
      | Safety Warning    | Manual entry bypasses barcode verification  |
    And I choose to manually enter the medication information:
      | Manual Entry Field| Value                                        |
      | Medication Name   | Tylenol                                     |
      | Strength          | 650mg                                       |
      | NDC Number        | 50580-506-02                               |
      | Lot Number        | ABC123                                      |
      | Expiration Date   | 2026-12-31                                 |
    Then the system validates the manual entry against the order
    And requires supervisor override for manual medication entry
    And documents the barcode scanning issue for pharmacy review

  Scenario: Administer pediatric medication with weight-based verification
    Given a pediatric patient "Emma Foster" (age 5, weight 18kg) is in bed "ED-PEDS-1"
    And there is a weight-based medication order:
      | Medication     | Dose Calculation    | Prescribed Dose | Route |
      | Acetaminophen  | 15mg/kg            | 270mg          | PO    |
    When I scan the patient's wristband barcode
    And I scan the acetaminophen medication barcode
    Then the system verifies pediatric dosing:
      | Verification       | Calculation                                  | Status   |
      | Weight Confirmation| Patient weight: 18kg                        | ✓ Valid  |
      | Dose Calculation   | 15mg/kg × 18kg = 270mg                     | ✓ Correct|
      | Maximum Safe Dose  | 270mg < 400mg max (safe)                    | ✓ Safe   |
      | Age Appropriateness| Acetaminophen approved for age 5            | ✓ Valid  |
    And the system displays pediatric-specific information:
      | Field              | Value                                        |
      | Patient Age/Weight | 5 years old, 18kg                          |
      | Calculation Shown  | 15mg/kg × 18kg = 270mg                     |
      | Liquid Formulation | 160mg/5mL suspension                       |
      | Volume to Give     | 8.4mL                                       |
    And I confirm the pediatric administration
    Then the system records with pediatric-specific documentation

  Scenario: Handle medication administration during code blue emergency
    Given a patient "Crisis Patient" is in bed "ED-TRAUMA-1"
    And a code blue emergency is in progress
    And emergency medications are ordered:
      | Medication    | Dose      | Route | Urgency    |
      | Epinephrine   | 1mg       | IV    | STAT       |
      | Atropine      | 0.5mg     | IV    | STAT       |
    When I scan the patient's wristband during the emergency
    And I scan the epinephrine medication barcode
    Then the system activates emergency administration mode:
      | Emergency Feature | Behavior                                     |
      | Rapid Verification| Abbreviated safety checks for life-saving   |
      | Time Documentation| Precise timestamp for code blue timeline    |
      | Team Notification | Alert code team of medication administration |
    And the system allows emergency override of timing restrictions
    And records the administration with code blue context:
      | Field             | Value                                        |
      | Emergency Context | Code Blue - Cardiac arrest                  |
      | Rapid Administration| Life-saving intervention                   |
      | Code Blue Timeline| 15:45:32 - Epinephrine given               |
    And the code blue medication log is automatically updated
