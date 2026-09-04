# # Lab Result Processing
#
# **Key Components:**
#
# - HL7 interface integration for seamless lab system communication
#
# - Critical value alerting with immediate notifications and popup alerts
#
# - Real-time patient record updates with timestamp tracking
#
# - Multi-channel physician notifications (mobile, SMS, in-app alerts)
#
# **Comprehensive Scenarios:**
#
# - Normal lab results - Standard processing and notification workflow
#
# - Critical values - Immediate alerts with popup notifications and multi-channel communication
#
# - Result corrections - Handling amended results with audit trails
#
# - Pediatric results - Age-specific reference ranges and appropriate interpretations
#
# - Physician handoff - Managing notifications during shift changes
#
# - Technical failures - Retry mechanisms and delay notifications
#
# - Batch processing - Handling multiple simultaneous results with prioritization
#
# - Interpreted results - Complex results with pathologist comments and clinical significance
#
# **Healthcare-Specific Features:**
#
# - Critical value management with severity levels
#
# - Age-appropriate reference ranges for pediatric patients
#
# - Audit trails for result corrections and critical value communications
#
# - Physician handoff protocols during shift changes
#
# - Multi-priority notification systems (critical, high, standard)
#
# - HL7 standard compliance for healthcare data exchange
#
# - Interface failure handling and recovery procedures
#
# - Pathologist interpretation integration
#
# - Real-time patient chart updates with visual indicators
#
# - Compliance tracking for critical value communication requirements

Feature: Lab Result Processing
  As a healthcare provider
  I want laboratory results to be automatically processed and distributed
  So that critical values are immediately communicated and patient records are updated in real-time

  Background:
    Given the ED management system is operational
    And the HL7 interface with the laboratory system is active
    And critical value alert system is enabled
    And physician notification system is functional

  Scenario: Process normal lab results via HL7 interface
    Given a patient "Jennifer Lopez" is in bed "ED-8"
    And laboratory orders were placed for "CBC, Basic Metabolic Panel"
    And the attending physician is "Dr. Smith"
    When the lab system sends results via HL7 interface:
      | Test Name          | Result    | Reference Range | Units  | Status   | Timestamp |
      | White Blood Cells  | 7.2       | 4.0-10.0       | K/uL   | Final    | 14:30     |
      | Hemoglobin         | 13.5      | 12.0-16.0      | g/dL   | Final    | 14:30     |
      | Sodium             | 140       | 136-145        | mmol/L | Final    | 14:30     |
      | Potassium          | 4.1       | 3.5-5.0        | mmol/L | Final    | 14:30     |
      | Creatinine         | 1.0       | 0.6-1.2        | mg/dL  | Final    | 14:30     |
    Then the system updates the patient record with all results
    And the results are marked as "Normal" in the patient chart
    And a standard notification is sent to "Dr. Smith":
      | Notification Type  | Details                                    |
      | Lab Results        | Normal CBC and BMP available for review   |
      | Patient           | Jennifer Lopez, Bed ED-8                  |
      | Timestamp         | 14:30                                      |
      | Priority          | Standard                                   |
    And the results appear in the patient's timeline with normal value indicators
    And no critical value alerts are generated
    And the nursing staff is notified that results are available for review

  Scenario: Process critical lab results with immediate alerts
    Given a patient "Michael Davis" is in bed "ED-12"
    And laboratory orders were placed for "Troponin, BNP, D-Dimer"
    And the attending physician is "Dr. Johnson"
    When the lab system sends critical results via HL7 interface:
      | Test Name    | Result | Reference Range | Units  | Status | Critical | Timestamp |
      | Troponin I   | 8.5    | 0.0-0.04       | ng/mL  | Final  | Yes      | 15:45     |
      | BNP          | 1200   | 0-100          | pg/mL  | Final  | Yes      | 15:45     |
      | D-Dimer      | 0.8    | 0.0-0.5        | mg/L   | Final  | No       | 15:45     |
    Then the system immediately flags critical values:
      | Test Name    | Critical Flag | Severity Level |
      | Troponin I   | CRITICAL HIGH | Severe         |
      | BNP          | CRITICAL HIGH | High           |
    And popup notifications are displayed for all logged-in providers:
      | Alert Type     | Message                                      |
      | CRITICAL ALERT | 🔴 CRITICAL: Troponin I = 8.5 ng/mL        |
      | HIGH ALERT     | 🟠 HIGH: BNP = 1200 pg/mL                   |
      | Patient Info   | Michael Davis, Bed ED-12                    |
    And an immediate notification is sent to "Dr. Johnson":
      | Notification Method | Content                                    |
      | Mobile Push        | CRITICAL LAB: Troponin 8.5 - Michael Davis |
      | SMS Alert          | ED-12 CRITICAL Troponin I: 8.5 ng/mL      |
      | In-App Alert       | High priority popup requiring acknowledgment|
    And the charge nurse receives a critical value notification
    And the results are highlighted in red on all patient displays
    And an audit trail is created for the critical value communication

  Scenario: Handle lab results with different statuses and corrections
    Given a patient "Sarah Wilson" is in bed "ED-6"
    And previous lab results were reported
    When the lab system sends updated results via HL7 interface:
      | Test Name     | Result | Status     | Previous Result | Correction Reason    | Timestamp |
      | Hemoglobin    | 9.2    | Corrected  | 11.2           | Sample hemolysis     | 16:15     |
      | Glucose       | 250    | Final      | -              | -                    | 16:15     |
      | Pending Test  | -      | Pending    | -              | Sample reprocessing  | 16:15     |
    Then the system processes different result statuses:
      | Status Type | Processing Action                              |
      | Corrected   | Replace previous value, maintain history       |
      | Final       | Add new result to patient record              |
      | Pending     | Update status, maintain order tracking        |
    And correction notifications are sent:
      | Notification Type | Content                                    |
      | Correction Alert  | Lab value corrected: Hgb 11.2 → 9.2 g/dL  |
      | Reason           | Sample hemolysis detected                   |
      | Clinical Impact  | Anemia now more severe than initially reported|
    And the attending physician "Dr. Martinez" is notified of the correction
    And the original result is preserved in the audit trail
    And the corrected value triggers anemia protocol alerts

  Scenario: Process pediatric lab results with age-specific reference ranges
    Given a pediatric patient "Emma Foster" (age 6) is in bed "ED-PEDS-2"
    And laboratory orders were placed for "CBC, CMP"
    When the lab system sends pediatric results via HL7 interface:
      | Test Name          | Result | Adult Range    | Pediatric Range (Age 6) | Units  | Status |
      | White Blood Cells  | 12.5   | 4.0-10.0      | 5.0-14.5               | K/uL   | Final  |
      | Hemoglobin         | 11.8   | 12.0-16.0     | 11.5-13.5              | g/dL   | Final  |
      | Alkaline Phosphatase| 250   | 44-147        | 156-369                | U/L    | Final  |
    Then the system applies age-appropriate reference ranges:
      | Test Name          | Interpretation      | Flag        |
      | White Blood Cells  | Normal for age 6    | Normal      |
      | Hemoglobin         | Normal for age 6    | Normal      |
      | Alkaline Phosphatase| Normal for age 6   | Normal      |
    And the pediatric attending "Dr. Chen" is notified with age-specific context
    And the results display shows both adult and pediatric reference ranges
    And no inappropriate critical alerts are generated for age-normal values

  Scenario: Handle lab results during physician handoff
    Given a patient "Robert Kim" is in bed "ED-15"
    And the day shift physician "Dr. Adams" ordered labs at 18:00
    And the evening shift physician "Dr. Brown" has taken over at 19:00
    When the lab system sends results via HL7 interface at 19:30:
      | Test Name     | Result | Reference Range | Status | Critical |
      | Lipase        | 350    | 10-140         | Final  | Yes      |
      | Amylase       | 180    | 25-125         | Final  | No       |
    Then the system determines the appropriate physician to notify:
      | Notification Target | Rationale                                  |
      | Primary: Dr. Brown  | Current attending physician                |
      | Secondary: Dr. Adams| Ordered the tests, may need notification   |
    And both physicians receive notifications with handoff context:
      | Physician | Notification Content                              |
      | Dr. Brown | CRITICAL: Lipase 350 - Patient from Dr. Adams   |
      | Dr. Adams | FYI: Your lipase order critical - Now Dr. Brown |
    And the handoff log is updated with the critical result information
    And the charge nurse is notified of the critical value during shift change

  Scenario: Process lab results with technical failures and retries
    Given a patient "Lisa Garcia" is in bed "ED-3"
    And laboratory results are ready for transmission
    When the lab system attempts to send results via HL7 interface
    And the initial transmission fails due to network connectivity
    And the lab system retries transmission after 5 minutes
    And the retry is successful with results:
      | Test Name   | Result | Reference Range | Status | Timestamp |
      | Troponin    | 0.02   | 0.0-0.04       | Final  | 20:15     |
    Then the system processes the delayed results
    And a delay notification is included:
      | Alert Component | Content                                    |
      | Delay Notice    | Results delayed due to technical issues    |
      | Original Time   | Results ready at 20:10                    |
      | Received Time   | Results received at 20:15                 |
    And the attending physician is notified of both the results and the delay
    And system administrators are alerted to the interface failure
    And the delay is documented in the interface audit log

  Scenario: Handle batch lab results processing
    Given multiple patients have pending lab results:
      | Patient Name    | Bed    | Attending     | Tests Ordered        |
      | Alice Johnson   | ED-4   | Dr. Smith     | CBC, BMP             |
      | Bob Thompson    | ED-7   | Dr. Smith     | Liver function tests |
      | Carol Martinez  | ED-11  | Dr. Brown     | Cardiac enzymes      |
    When the lab system sends batch results via HL7 interface:
      | Patient       | Test Results                              | Critical Values |
      | Alice Johnson | All normal values                         | None           |
      | Bob Thompson  | ALT: 150 (High), AST: 120 (High)        | None           |
      | Carol Martinez| Troponin: 2.1 (Critical)                | Troponin       |
    Then the system processes all results simultaneously
    And notifications are prioritized by criticality:
      | Priority | Patient        | Notification Type    |
      | 1        | Carol Martinez | Critical value alert |
      | 2        | Bob Thompson   | Abnormal value alert |
      | 3        | Alice Johnson  | Normal results       |
    And physicians receive consolidated notifications when appropriate
    And system performance metrics are maintained during batch processing

  Scenario: Process lab results with interpretation comments
    Given a patient "David Lee" is in bed "ED-9"
    And complex laboratory tests were ordered
    When the lab system sends results with pathologist interpretation:
      | Test Name        | Result | Reference | Interpretation                    | Timestamp |
      | Blood Smear      | -      | -         | Moderate anisocytosis noted      | 21:00     |
      | Hemoglobin A1C   | 9.2%   | <5.7%     | Consistent with poor DM control  | 21:00     |
      | Thyroid Function | -      | -         | Pattern suggests hyperthyroidism | 21:00     |
    Then the system includes interpretation comments in the patient record
    And the attending physician receives enhanced notifications:
      | Component    | Content                                       |
      | Raw Results  | Numeric values and reference ranges          |
      | Interpretation| Pathologist comments and clinical significance|
      | Recommendations| Suggested follow-up or additional testing   |
    And interpretation comments are highlighted in the patient chart
    And complex results are flagged for physician review and acknowledgment
