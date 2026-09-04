# # Critical Lab Alert:
#
# **Key Components:**
#
# - Immediate multi-channel notifications to attending physicians and charge nurses
#
# - Visual alert systems with red flags on all patient displays
#
# - Acknowledgment tracking to ensure critical values are acted upon
#
# - Escalation protocols for unacknowledged alerts
#
# **Comprehensive Scenarios:**
#
# - Standard critical troponin - Basic alert workflow with immediate notifications
#
# - Physician acknowledgment - Required acknowledgment with escalation procedures
#
# - Multiple simultaneous alerts - Priority-based processing of multiple critical values
#
# - Shift change handling - Managing alerts during physician handoffs
#
# - Grouped cardiac markers - Related critical values with clinical context
#
# - False positive correction - Handling laboratory errors and alert cancellations
#
# - Transfer coordination - Critical alerts with patient movement requirements
#
# - System validation - Testing alert functionality and performance metrics
#
# **Healthcare-Specific Features:**
#
# - Critical value thresholds and clinical significance
#
# - Multi-channel notification systems (mobile, SMS, desktop, overhead page)
#
# - Acknowledgment requirements with time limits
#
# - Escalation protocols for unacknowledged alerts
#
# - Clinical context and treatment recommendations
#
# - STEMI protocol integration for cardiac emergencies
#
# - Shift handoff coordination during critical results
#
# - Laboratory error correction workflows
#
# - Patient transfer coordination with receiving units
#
# - Performance monitoring and quality assurance
#
# - Visual alert systems across all patient displays
#
# - Audit trails for compliance and safety monitoring

Feature: Critical Lab Alert
  As a healthcare provider
  I want immediate alerts when critical lab values are received
  So that life-threatening conditions can be identified and treated without delay

  Background:
    Given the ED management system is operational
    And the critical value alert system is enabled
    And laboratory interfaces are functioning
    And all patient displays are connected to the alert system

  Scenario: Process critically high troponin result with immediate alerts
    Given a patient "Robert Martinez" is in bed "ED-7"
    And the attending physician is "Dr. Johnson"
    And the charge nurse is "Nurse Williams"
    And troponin was ordered for "chest pain evaluation"
    When the laboratory result is received:
      | Test Name       | Result | Reference Range | Units  | Critical Threshold | Status |
      | Troponin I      | 5.8    | 0.0-0.04       | ng/mL  | >0.4              | Final  |
    Then the system immediately triggers critical alerts
    And the attending physician "Dr. Johnson" receives immediate notifications:
      | Notification Type | Content                                        | Delivery Method |
      | Mobile Push Alert | 🔴 CRITICAL: Troponin I 5.8 ng/mL - ED-7     | Mobile App      |
      | SMS Alert        | CRITICAL LAB: R.Martinez ED-7 Troponin 5.8    | Text Message    |
      | Popup Alert      | CRITICAL VALUE - Requires Acknowledgment       | Workstation     |
    And the charge nurse "Nurse Williams" receives critical notifications:
      | Notification Type | Content                                        | Delivery Method |
      | Desktop Alert    | CRITICAL: Troponin 5.8 - Bed ED-7            | Workstation     |
      | Overhead Page    | Critical lab value bed ED-7                   | PA System       |
      | Mobile Alert     | Critical troponin result requires attention   | Mobile Device   |
    And red flag indicators appear on all patient displays:
      | Display Location     | Alert Indicator                              |
      | Patient Monitor      | 🔴 CRITICAL LAB flashing red banner         |
      | Bedside Workstation  | Red alert icon next to patient name         |
      | Main ED Dashboard    | Red flag on bed ED-7 status                 |
      | Mobile Devices       | Red notification badge on patient chart     |
      | Nursing Station      | Critical value alert on patient board       |

  Scenario: Handle critical troponin with physician acknowledgment requirements
    Given a patient "Maria Santos" is in bed "ED-12"
    And the attending physician is "Dr. Lee"
    And a critically high troponin result of "7.2 ng/mL" is received
    When the critical alert is triggered
    Then the system requires physician acknowledgment:
      | Acknowledgment Type | Requirement                                   |
      | Initial Alert       | Must acknowledge receipt within 15 minutes   |
      | Clinical Review     | Must document result review                   |
      | Action Plan         | Must indicate next steps taken                |
    And if "Dr. Lee" does not acknowledge within 15 minutes:
      | Escalation Action   | Details                                       |
      | Secondary Alert     | Alert sent to backup physician                |
      | Charge Nurse Alert  | Escalation notice to charge nurse             |
      | Supervisor Alert    | Department supervisor notified                |
    And the acknowledgment status is tracked:
      | Status              | Timestamp | Provider    | Action              |
      | Alert Sent          | 14:30:15  | System      | Initial notification|
      | Acknowledged        | 14:32:45  | Dr. Lee     | Acknowledged receipt|
      | Reviewed            | 14:35:20  | Dr. Lee     | Documented review   |
      | Action Taken        | 14:40:10  | Dr. Lee     | Treatment initiated |

  Scenario: Process multiple critical values simultaneously
    Given multiple patients have critical troponin results:
      | Patient Name    | Bed   | Troponin Result | Attending     | Severity  |
      | John Williams   | ED-3  | 3.2 ng/mL      | Dr. Adams     | High      |
      | Lisa Johnson    | ED-8  | 8.9 ng/mL      | Dr. Brown     | Critical  |
      | Mike Davis      | ED-15 | 4.1 ng/mL      | Dr. Adams     | High      |
    When all critical results are received simultaneously
    Then the system prioritizes alerts by severity:
      | Priority | Patient      | Alert Level | Notification Urgency    |
      | 1        | Lisa Johnson | Critical    | Immediate - All channels|
      | 2        | Mike Davis   | High        | Urgent - Standard alerts|
      | 3        | John Williams| High        | Urgent - Standard alerts|
    And physicians receive prioritized notifications:
      | Physician | Alert Summary                                   |
      | Dr. Brown | CRITICAL: Lisa Johnson Trop 8.9 - IMMEDIATE   |
      | Dr. Adams | HIGH: 2 patients with elevated troponin       |
    And the charge nurse receives a summary alert:
      | Alert Type    | Content                                         |
      | Mass Alert    | 3 critical troponin results requiring attention|
      | Priority List | Lisa Johnson (Critical), others (High)         |
    And all patient displays show appropriately color-coded flags

  Scenario: Handle critical troponin during shift change
    Given a patient "Catherine Brown" is in bed "ED-6"
    And it is 19:00 during evening shift change
    And the day shift physician "Dr. Wilson" ordered the troponin
    And the evening shift physician "Dr. Taylor" has assumed care
    When a critically high troponin result of "6.1 ng/mL" is received
    Then both physicians receive critical alerts:
      | Physician  | Alert Type    | Content                                |
      | Dr. Taylor | Primary Alert | CRITICAL Troponin 6.1 - Your patient  |
      | Dr. Wilson | Handoff Alert | FYI: Critical result on your order     |
    And the charge nurse receives handoff-specific notification:
      | Alert Component | Details                                     |
      | Shift Context   | Critical result during physician handoff   |
      | Current MD      | Dr. Taylor (assuming care)                  |
      | Ordering MD     | Dr. Wilson (ordered test)                   |
    And the handoff documentation is automatically updated
    And red flags appear with shift change context indicators

  Scenario: Process critical troponin with additional cardiac markers
    Given a patient "Steven Kim" is in bed "ED-11"
    And multiple cardiac markers were ordered
    When critical and related results are received:
      | Test Name    | Result | Reference Range | Critical | Clinical Significance |
      | Troponin I   | 4.7    | 0.0-0.04       | Yes      | Acute MI indicated    |
      | CK-MB        | 45     | 0-6.3          | Yes      | Myocardial damage     |
      | Myoglobin    | 280    | 25-72          | No       | Elevated but not critical|
    Then the system groups related critical values:
      | Alert Category  | Content                                      |
      | Cardiac Panel   | Multiple critical cardiac markers            |
      | Primary Alert   | Troponin I: 4.7 ng/mL (CRITICAL)           |
      | Secondary Alert | CK-MB: 45 ng/mL (CRITICAL)                 |
      | Supporting Data | Myoglobin: 280 ng/mL (Elevated)            |
    And enhanced clinical context is provided:
      | Clinical Indication | Acute myocardial infarction likely         |
      | Recommended Actions | Cardiology consult, STEMI protocol         |
      | Time Sensitivity    | Treatment within 90 minutes critical       |
    And STEMI protocol alerts are automatically triggered

  Scenario: Handle false positive critical troponin alerts
    Given a patient "Nancy Rodriguez" is in bed "ED-4"
    And a troponin result of "5.1 ng/mL" triggers a critical alert
    When the laboratory calls to report a sample error
    And a corrected result shows "0.03 ng/mL" (normal)
    Then the system processes the correction:
      | Correction Action | Details                                      |
      | Cancel Alert      | Original critical alert is cancelled        |
      | Send Correction   | Corrected value sent to all recipients      |
      | Document Error    | Lab error documented in audit trail         |
    And correction notifications are sent:
      | Notification Type | Content                                      |
      | Alert Cancellation| CANCELLED: Previous critical troponin alert |
      | Corrected Value   | Troponin corrected to 0.03 ng/mL (Normal)  |
      | Error Explanation | Laboratory sample contamination identified  |
    And red flags are removed from all patient displays
    And the correction is logged for quality assurance review

  Scenario: Critical troponin with patient transfer requirements
    Given a patient "Timothy Chang" is in bed "ED-9"
    And a critically high troponin of "9.3 ng/mL" is received
    And the patient requires immediate transfer to cardiac unit
    When the critical alert is processed
    Then transfer coordination alerts are included:
      | Alert Component   | Details                                      |
      | Transfer Required | Patient needs immediate cardiac unit transfer|
      | Bed Availability  | CCU bed 302 available                       |
      | Transport Time    | Transport team ETA 10 minutes               |
    And receiving unit notifications are sent:
      | Receiving Unit | Alert Content                                |
      | CCU            | Incoming transfer - Critical troponin 9.3   |
      | Cardiology     | Urgent consult needed - STEMI protocol      |
    And transfer documentation is automatically initiated
    And critical alerts follow the patient to the receiving unit

  Scenario: Validate critical troponin alert system functionality
    Given the critical alert system is being tested
    When a test troponin result of "TEST-5.0 ng/mL" is processed
    Then the system validates all alert pathways:
      | Alert Pathway     | Status                                       |
      | Physician Mobile  | Test alert delivered successfully           |
      | Charge Nurse      | Test alert delivered successfully           |
      | Patient Displays  | Red flags displayed correctly               |
      | Audit Trail       | Test alert logged with timestamp            |
    And test alerts are clearly marked as "SYSTEM TEST"
    And all test alerts are automatically cleared after validation
    And system performance metrics are recorded:
      | Metric           | Measurement                                  |
      | Alert Latency    | <30 seconds from result to notification     |
      | Delivery Success | 100% successful delivery to all recipients  |
      | Display Update   | <5 seconds to update all patient displays   |
