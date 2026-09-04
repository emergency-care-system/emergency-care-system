# # Triage Re-assessment
#
# **Key Components:**
#
# - Automatic reassessment scheduling based on wait times and acuity levels
#
# - Dynamic queue repositioning when patient conditions change
#
# - ESI score recalculation with updated clinical data
#
# - Alert generation for condition changes and escalations
#
# **Comprehensive Scenarios:**
#
# - Worsening condition - Patient deterioration requiring priority escalation
#
# - Stable condition - No change in status, maintaining current priority
#
# - Improving condition - Patient improvement allowing priority reduction
#
# - Automatic alerts - System-generated reassessment reminders
#
# - Critical deterioration - Emergency response for patients becoming unstable
#
# - Pediatric reassessment - Age-specific criteria and specialized protocols
#
# - Documentation-only reassessment - Non-clinical updates and patient communication
#
# - Shift change continuity - Handoff procedures and care continuity
#
# **Healthcare-Specific Features:**
#
# - Time-based reassessment triggers (2-hour standard)
#
# - ESI score recalculation algorithms
#
# - Queue management with dynamic repositioning
#
# - Escalation alerts for clinical staff
#
# - Pediatric-specific assessment criteria
#
# - Emergency protocol activation for critical patients
#
# - Shift handoff procedures
#
# - Comprehensive clinical documentation

Feature: Triage Re-assessment
  As a triage nurse
  I want to reassess patients who have been waiting in the queue
  So that their priority can be adjusted if their condition has changed

  Background:
    Given the ED management system is operational
    And I am logged in as a triage nurse
    And the automatic reassessment alerts are enabled

  Scenario: Reassess patient with worsening condition after 2 hours
    Given a patient "Sarah Johnson" has been waiting in the queue for 2 hours
    And the patient's initial triage was ESI Level 4 (Less Urgent)
    And the patient's initial vital signs were:
      | Vital Sign          | Initial Value |
      | Blood Pressure      | 125/78        |
      | Heart Rate          | 82            |
      | Respiratory Rate    | 16            |
      | Temperature         | 99.1°F        |
      | Oxygen Saturation   | 98%           |
      | Pain Scale          | 3/10          |
    When the system triggers a reassessment alert at the 2-hour mark
    And I select the patient for reassessment
    And I enter the updated vital signs:
      | Vital Sign          | Updated Value |
      | Blood Pressure      | 95/55         |
      | Heart Rate          | 115           |
      | Respiratory Rate    | 24            |
      | Temperature         | 101.8°F       |
      | Oxygen Saturation   | 94%           |
      | Pain Scale          | 8/10          |
    And I update the chief complaint to "Severe abdominal pain with dizziness"
    And I submit the reassessment
    Then the system recalculates the ESI score from "4" to "2"
    And the system updates the triage level from "Less Urgent" to "High Priority"
    And the patient is moved from position 12 to position 2 in the queue
    And an escalation alert is sent to the charge nurse
    And the estimated wait time is updated from "90 minutes" to "15 minutes"
    And a reassessment note is automatically added to the patient record

  Scenario: Reassess patient with stable condition
    Given a patient "Michael Chen" has been waiting in the queue for 2 hours
    And the patient's initial triage was ESI Level 3 (Urgent)
    And the patient's initial vital signs were:
      | Vital Sign          | Initial Value |
      | Blood Pressure      | 140/90        |
      | Heart Rate          | 95            |
      | Respiratory Rate    | 20            |
      | Temperature         | 100.2°F       |
      | Oxygen Saturation   | 96%           |
      | Pain Scale          | 6/10          |
    When I perform a scheduled reassessment
    And I enter the updated vital signs:
      | Vital Sign          | Updated Value |
      | Blood Pressure      | 135/85        |
      | Heart Rate          | 88            |
      | Respiratory Rate    | 18            |
      | Temperature         | 99.8°F        |
      | Oxygen Saturation   | 97%           |
      | Pain Scale          | 5/10          |
    And I note "Patient reports feeling slightly better"
    And I submit the reassessment
    Then the system recalculates and maintains ESI score of "3"
    And the triage level remains "Urgent"
    And the patient's queue position remains unchanged
    And no escalation alerts are generated
    And a reassessment note is added documenting stable condition
    And the next reassessment is scheduled for 1 hour

  Scenario: Reassess patient with improving condition
    Given a patient "Lisa Rodriguez" has been waiting in the queue for 2 hours
    And the patient's initial triage was ESI Level 2 (High Priority)
    And the patient's initial vital signs were:
      | Vital Sign          | Initial Value |
      | Blood Pressure      | 170/100       |
      | Heart Rate          | 120           |
      | Respiratory Rate    | 28            |
      | Temperature         | 98.9°F        |
      | Oxygen Saturation   | 92%           |
      | Pain Scale          | 9/10          |
    When I perform a reassessment
    And I enter the updated vital signs:
      | Vital Sign          | Updated Value |
      | Blood Pressure      | 145/85        |
      | Heart Rate          | 95            |
      | Respiratory Rate    | 20            |
      | Temperature         | 98.6°F        |
      | Oxygen Saturation   | 96%           |
      | Pain Scale          | 4/10          |
    And I note "Patient reports significant improvement after medication"
    And I submit the reassessment
    Then the system recalculates the ESI score from "2" to "3"
    And the system updates the triage level from "High Priority" to "Urgent"
    And the patient is moved from position 1 to position 5 in the queue
    And the charge nurse is notified of the priority change
    And the estimated wait time is updated from "Immediate" to "45 minutes"
    And higher priority patients are moved up in the queue

  Scenario: Automatic reassessment alert triggers
    Given multiple patients have been waiting for extended periods:
      | Patient Name     | Wait Time | Current ESI | Due for Reassessment |
      | John Williams    | 2 hours   | 4           | Yes                  |
      | Emma Thompson    | 1.5 hours | 3           | No                   |
      | David Kim        | 3 hours   | 3           | Yes                  |
    When the system performs its hourly reassessment check
    Then reassessment alerts are generated for:
      | Patient Name  | Alert Type           | Reason                    |
      | John Williams | Standard Reassess    | 2 hours ESI Level 4       |
      | David Kim     | Urgent Reassess      | 3 hours ESI Level 3       |
    And the alerts appear on the triage nurse dashboard
    And the patients are flagged with "Reassessment Due" status
    And Emma Thompson does not receive an alert

  Scenario: Handle patient who becomes critical during reassessment
    Given a patient "Robert Martinez" has been waiting in the queue for 2 hours
    And the patient's initial triage was ESI Level 3 (Urgent)
    When I begin the reassessment process
    And I observe the patient is now unresponsive
    And I enter critical vital signs:
      | Vital Sign          | Critical Value |
      | Blood Pressure      | 60/30          |
      | Heart Rate          | 150            |
      | Respiratory Rate    | 6              |
      | Temperature         | 96.2°F         |
      | Oxygen Saturation   | 80%            |
      | Consciousness       | Unresponsive   |
    And I submit the emergency reassessment
    Then the system immediately calculates ESI score as "1"
    And the system updates triage level to "Resuscitation"
    And the patient is moved to the top of all queues
    And a code blue alert is automatically triggered
    And the rapid response team is notified immediately
    And the patient is flagged for immediate intervention
    And I am prompted to initiate emergency protocols

  Scenario: Reassess pediatric patient with different parameters
    Given a pediatric patient "Amy Foster" (age 8) has been waiting for 2 hours
    And the patient's initial triage was ESI Level 3 (Urgent)
    When I perform a pediatric reassessment
    And I enter updated vital signs using age-appropriate parameters:
      | Vital Sign          | Updated Value |
      | Blood Pressure      | 85/50         |
      | Heart Rate          | 140           |
      | Respiratory Rate    | 32            |
      | Temperature         | 103.8°F       |
      | Oxygen Saturation   | 93%           |
      | Pain Scale (FACES)  | 8/10          |
    And I note "Child appears more lethargic than initial assessment"
    And I submit the pediatric reassessment
    Then the system recalculates using pediatric ESI criteria
    And the ESI score is updated from "3" to "2"
    And the triage level is updated to "High Priority"
    And the pediatric emergency team is notified
    And the patient is moved to the pediatric high priority queue
    And parent/guardian notification protocols are initiated

  Scenario: Document reassessment with no vital sign changes
    Given a patient "Catherine Lee" has been waiting for 2 hours
    And a reassessment is due
    When I perform the reassessment
    And the vital signs remain identical to the initial assessment
    But I note "Patient reports increased anxiety about wait time"
    And I provide reassurance and update on expected wait time
    And I submit the reassessment
    Then the ESI score and triage level remain unchanged
    And the queue position is maintained
    And a documentation note is added about patient anxiety
    And comfort measures are suggested in the patient instructions
    And the next reassessment interval is maintained

  Scenario: Handle reassessment during shift change
    Given a patient "Thomas Wilson" is due for reassessment
    And the day shift triage nurse is preparing to leave
    And the night shift triage nurse is arriving
    When the day shift nurse initiates the reassessment handoff
    And transfers the patient assessment to the night shift nurse
    Then the reassessment timing is preserved
    And all previous assessment data remains accessible
    And the night shift nurse can complete the reassessment
    And continuity of care documentation is maintained
    And the handoff is logged in the system audit trail
