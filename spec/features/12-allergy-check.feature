# # Allergy Check
#
# **Key Components:**
#
# - Automatic allergy verification against documented patient allergies
#
# - Cross-reactivity checking for related drug classes and compounds
#
# - Override capabilities with clinical justification requirements
#
# - Real-time alerts with severity levels and reaction details
#
# **Comprehensive Scenarios:**
#
# - Direct allergy match - Patient with documented penicillin allergy receiving penicillin prescription
#
# - No allergies documented - Normal processing when patient has no known allergies
#
# - Cross-reactive allergies - Detecting related drug allergies (penicillin/amoxicillin)
#
# - Clinical override - Physician override with documented justification for severe cases
#
# - Multiple medication checking - Simultaneous allergy verification for multiple orders
#
# - Unknown allergy status - Handling patients with undocumented allergy history
#
# - Drug class allergies - Checking against broader drug categories (sulfa drugs)
#
# - Multiple information sources - Prioritizing verified vs. unverified allergy information
#
# - Real-time predictive alerts - Proactive warnings during order entry
#
# **Healthcare-Specific Features:**
#
# - Severity-based allergy classification (mild, moderate, severe)
#
# - Cross-reactivity databases for related medications
#
# - Drug class allergy recognition (beta-lactams, sulfonamides, NSAIDs)
#
# - Clinical override documentation requirements
#
# - Alternative medication suggestions
#
# - Source credibility assessment (patient report vs. medical records)
#
# - Real-time predictive typing alerts
#
# - Enhanced monitoring flags for allergy overrides
#
# - Comprehensive audit trails for safety compliance
#
# - Integration with pharmacy and nursing notification systems
#
# - Emergency protocol recommendations for severe allergies
#
# - Educational information about drug classes and cross-reactivity

Feature: Allergy Check
  As a physician
  I want the system to automatically check for drug allergies when prescribing medications
  So that I can prevent allergic reactions and ensure patient safety

  Background:
    Given the ED management system is operational
    And I am logged in as "Dr. Smith"
    And the allergy checking module is active
    And the drug interaction database is up-to-date

  Scenario: Prescribe penicillin to patient with documented penicillin allergy
    Given a patient "Maria Rodriguez" is in bed "ED-8"
    And the patient has documented allergies:
      | Allergy        | Reaction Type       | Severity  | Date Documented | Source     |
      | Penicillin     | Rash, hives        | Moderate  | 2023-05-15     | Patient    |
      | Shellfish      | Anaphylaxis        | Severe    | 2022-08-10     | Patient    |
    When I enter a medication order for:
      | Medication     | Dose      | Route | Frequency | Duration |
      | Penicillin VK  | 500mg     | PO    | QID       | 10 days  |
    And I submit the order
    Then the system displays an allergy warning:
      | Alert Type         | Details                                      |
      | DRUG ALLERGY       | ⚠️ ALLERGY ALERT: Patient allergic to Penicillin |
      | Severity Level     | Moderate                                     |
      | Reaction Type      | Rash, hives                                  |
      | Date Documented    | May 15, 2023                                |
      | Source             | Patient reported                             |
    And the system blocks the order submission
    And I am presented with options:
      | Option             | Description                                  |
      | Cancel Order       | Remove penicillin order                      |
      | Override with Reason| Document clinical justification            |
      | Alternative Drugs  | View suggested alternative antibiotics       |
    And the allergy alert is logged in the audit trail

  Scenario: Prescribe medication with no documented allergies
    Given a patient "John Taylor" is in bed "ED-12"
    And the patient has no documented allergies
    When I enter a medication order for:
      | Medication     | Dose      | Route | Frequency |
      | Amoxicillin    | 875mg     | PO    | BID       |
    And I submit the order
    Then the system performs allergy checking
    And no allergy alerts are triggered
    And the order is processed normally
    And the system displays confirmation:
      | Confirmation Type  | Message                                      |
      | No Allergies Found | No known allergies to Amoxicillin          |
      | Order Status       | Order submitted successfully                 |
    And the medication order is routed to pharmacy

  Scenario: Prescribe medication with cross-reactive allergy
    Given a patient "Sarah Johnson" is in bed "ED-5"
    And the patient has documented allergies:
      | Allergy        | Reaction Type       | Severity  |
      | Penicillin     | Respiratory distress| Severe    |
    When I enter a medication order for:
      | Medication     | Dose      | Route | Frequency |
      | Amoxicillin    | 500mg     | PO    | TID       |
    And I submit the order
    Then the system displays a cross-reactivity warning:
      | Alert Type         | Details                                      |
      | CROSS-REACTIVITY   | ⚠️ WARNING: Cross-reactivity with Penicillin|
      | Known Allergy      | Patient allergic to Penicillin (Severe)     |
      | Cross-Reaction Risk| Amoxicillin is a penicillin derivative     |
      | Reaction Type      | Respiratory distress                         |
      | Risk Level         | High - Severe reaction possible              |
    And the system provides additional information:
      | Information Type   | Content                                      |
      | Cross-Reaction Rate| 8-10% cross-reactivity with penicillin     |
      | Clinical Guidance  | Consider non-beta-lactam alternatives       |
      | Emergency Prep     | Have epinephrine available if administered   |
    And I must acknowledge the cross-reactivity risk before proceeding

  Scenario: Override allergy alert with clinical justification
    Given a patient "Michael Chen" is in bed "ED-15"
    And the patient has a documented penicillin allergy with "mild rash"
    And the patient has severe sepsis requiring immediate antibiotic treatment
    When I enter a penicillin order and receive an allergy alert
    And I choose to override the allergy warning
    Then the system requires detailed justification:
      | Required Field     | Description                                  |
      | Clinical Rationale | Why this medication is medically necessary   |
      | Risk Assessment    | Evaluation of allergy risk vs benefit       |
      | Monitoring Plan    | How allergic reactions will be monitored    |
      | Alternative Review | Why alternatives are not suitable            |
    And I document the override:
      | Field              | Value                                        |
      | Clinical Rationale | Life-threatening sepsis, first-line antibiotic needed |
      | Risk Assessment    | Mild rash risk acceptable vs sepsis mortality |
      | Monitoring Plan    | Continuous monitoring, diphenhydramine available |
      | Alternative Review | Other antibiotics inadequate for organism     |
    Then the system accepts the override
    And logs the override decision with full documentation
    And notifies nursing staff of the allergy override for enhanced monitoring

  Scenario: Check allergies for multiple medications simultaneously
    Given a patient "Lisa Brown" is in bed "ED-7"
    And the patient has documented allergies:
      | Allergy        | Reaction Type       | Severity  |
      | Morphine       | Respiratory depression | Severe |
      | NSAIDs         | GI bleeding        | Moderate  |
    When I enter multiple medication orders:
      | Medication     | Dose      | Route | Purpose           |
      | Fentanyl       | 50mcg     | IV    | Pain control      |
      | Ibuprofen      | 600mg     | PO    | Anti-inflammatory |
      | Acetaminophen  | 650mg     | PO    | Pain/fever        |
    And I submit all orders simultaneously
    Then the system checks each medication against documented allergies:
      | Medication     | Allergy Status | Alert Level |
      | Fentanyl       | No direct allergy | Safe      |
      | Ibuprofen      | NSAID allergy  | WARNING   |
      | Acetaminophen  | No allergy     | Safe      |
    And I receive specific alerts for problematic medications:
      | Alert Medication | Warning Message                              |
      | Ibuprofen        | Patient allergic to NSAIDs - GI bleeding risk |
    And safe medications are processed without alerts
    And I can review and modify orders before final submission

  Scenario: Handle unknown or "No Known Allergies" status
    Given a patient "Robert Davis" is in bed "ED-3"
    And the patient's allergy status is "Unknown - Unable to assess"
    When I enter a medication order for:
      | Medication     | Dose      | Route |
      | Cephalexin     | 500mg     | PO    |
    And I submit the order
    Then the system displays an information alert:
      | Alert Type         | Message                                      |
      | ALLERGY UNKNOWN    | ⚠️ INFO: Patient allergy status unknown     |
      | Risk Consideration | Cannot verify medication allergies          |
      | Recommendation     | Consider allergy assessment before administration |
    And the system provides safety recommendations:
      | Recommendation     | Details                                      |
      | Allergy Assessment | Attempt to obtain allergy history           |
      | Start Monitoring   | Monitor for allergic reactions closely      |
      | Have Antidotes Ready| Ensure emergency medications available      |
    And the order is flagged for enhanced allergy monitoring

  Scenario: Check for drug class allergies
    Given a patient "Jennifer Wilson" is in bed "ED-11"
    And the patient has documented allergies:
      | Allergy        | Reaction Type       | Severity  | Drug Class |
      | Sulfa drugs    | Stevens-Johnson syndrome | Severe | Sulfonamides |
    When I enter a medication order for:
      | Medication           | Dose      | Route | Drug Class    |
      | Trimethoprim-Sulfamethoxazole | 800mg | PO | Sulfonamide |
    And I submit the order
    Then the system identifies the drug class allergy:
      | Alert Type         | Details                                      |
      | DRUG CLASS ALLERGY | ⚠️ SEVERE: Patient allergic to Sulfa drugs |
      | Specific Drug      | TMP-SMX contains sulfamethoxazole           |
      | Reaction History   | Stevens-Johnson syndrome                     |
      | Severity           | Severe - Life-threatening reaction possible  |
    And the system provides drug class education:
      | Information        | Content                                      |
      | Drug Class         | Sulfonamide antibiotics                     |
      | Cross-Reactivity   | All sulfa-containing medications at risk    |
      | Alternative Classes| Beta-lactams, fluoroquinolones available   |

  Scenario: Handle allergy information from multiple sources
    Given a patient "David Kim" is in bed "ED-9"
    And the patient has allergy information from multiple sources:
      | Source             | Allergy    | Reaction        | Reliability |
      | Patient Report     | Penicillin | "Bad reaction"  | Unverified  |
      | Medical Records    | Penicillin | Urticaria, rash | Verified    |
      | Family Member      | Codeine    | Nausea         | Unverified  |
    When I enter a penicillin order
    Then the system displays comprehensive allergy information:
      | Source Type        | Allergy Details                              |
      | Verified Record    | Penicillin - Urticaria, rash (Medical Records) |
      | Patient Report     | Penicillin - "Bad reaction" (Unverified)    |
    And the system prioritizes verified information in the alert
    And provides source credibility indicators:
      | Source             | Credibility Level | Clinical Weight      |
      | Medical Records    | High reliability  | Primary consideration |
      | Patient Report     | Moderate reliability | Secondary consideration |
    And I can review detailed allergy history before making decisions

  Scenario: Real-time allergy checking during order modification
    Given a patient "Susan Martinez" is in bed "ED-4"
    And the patient has a penicillin allergy
    And I have started entering a medication order
    When I begin typing "Pen" in the medication field
    Then the system provides real-time allergy warnings:
      | Alert Type         | Message                                      |
      | PREDICTIVE ALERT   | ⚠️ Patient allergic to Penicillin          |
      | Medication Match   | "Pen" may be penicillin-related drug       |
      | Suggestion         | Consider alternative antibiotics            |
    And the system highlights potential allergy matches as I type
    And provides alternative medication suggestions:
      | Alternative        | Drug Class        | Reason               |
      | Cephalexin         | Cephalosporin     | Lower cross-reactivity |
      | Azithromycin       | Macrolide         | No cross-reactivity   |
      | Ciprofloxacin      | Fluoroquinolone   | Different mechanism   |
    And I can select alternatives directly from the suggestion list
