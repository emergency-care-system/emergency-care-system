# # Initial Triage Assessment
#
# **Key Components:**
#
# - ESI scoring algorithm with automatic calculation
#
# - Triage level assignment based on acuity
#
# - Queue positioning based on priority
#
# - Vital signs validation and error handling
#
# **Comprehensive Scenarios:**
#
# - High acuity chest pain (ESI Level 2) - Critical patient pathway
#
# - Minor injury (ESI Level 4) - Standard low-priority assessment
#
# - Critical resuscitation (ESI Level 1) - Emergency response activation
#
# - Pediatric assessment - Age-specific criteria and specialized handling
#
# - Incomplete data validation - Error handling and required field enforcement
#
# - Patient reassessment - Condition deterioration and priority escalation
#
# - Multiple patient processing - Queue management and prioritization
#
# **Healthcare-Specific Features:**
#
# - ESI (Emergency Severity Index) scoring system
#
# - Age-appropriate vital sign parameters
#
# - Pain scale documentation (including pediatric FACES scale)
#
# - Automatic alert generation for critical patients
#
# - Reassessment workflows for condition changes
#
# - Queue management with dynamic prioritization

Feature: Initial Triage Assessment
  As a triage nurse
  I want to perform initial assessments on registered patients
  So that they are properly prioritized based on their medical acuity

  Background:
    Given the ED management system is operational
    And I am logged in as a triage nurse
    And the ESI (Emergency Severity Index) scoring module is active

  Scenario: Assess patient with chest pain (ESI Level 2)
    Given a registered patient "John Doe" is waiting for triage
    And the patient was registered 10 minutes ago
    When I select the patient for triage assessment
    And I enter the vital signs:
      | Vital Sign          | Value    |
      | Blood Pressure      | 160/95   |
      | Heart Rate          | 110      |
      | Respiratory Rate    | 22       |
      | Temperature         | 98.6°F   |
      | Oxygen Saturation   | 94%      |
    And I enter the chief complaint as "Chest pain and shortness of breath"
    And I enter the pain scale as "8/10"
    And I document onset as "Started 2 hours ago"
    And I submit the triage assessment
    Then the system calculates an ESI score of "2"
    And the system assigns triage level "High Priority"
    And the patient is positioned at the front of the high priority queue
    And an alert is sent to the attending physician
    And the estimated wait time is updated to "Immediate"

  Scenario: Assess patient with minor injury (ESI Level 4)
    Given a registered patient "Jane Smith" is waiting for triage
    When I select the patient for triage assessment
    And I enter the vital signs:
      | Vital Sign          | Value    |
      | Blood Pressure      | 120/80   |
      | Heart Rate          | 75       |
      | Respiratory Rate    | 16       |
      | Temperature         | 98.2°F   |
      | Oxygen Saturation   | 99%      |
    And I enter the chief complaint as "Sprained ankle from fall"
    And I enter the pain scale as "4/10"
    And I document onset as "This morning while jogging"
    And I submit the triage assessment
    Then the system calculates an ESI score of "4"
    And the system assigns triage level "Less Urgent"
    And the patient is positioned in the less urgent queue
    And the estimated wait time is updated to "60-90 minutes"
    And no immediate alerts are generated

  Scenario: Assess critical patient requiring immediate attention (ESI Level 1)
    Given a registered patient "Emergency Patient" is waiting for triage
    When I select the patient for triage assessment
    And I enter the vital signs:
      | Vital Sign          | Value    |
      | Blood Pressure      | 70/40    |
      | Heart Rate          | 140      |
      | Respiratory Rate    | 8        |
      | Temperature         | 95.0°F   |
      | Oxygen Saturation   | 85%      |
    And I enter the chief complaint as "Unresponsive after motor vehicle accident"
    And I enter the pain scale as "Unable to assess"
    And I mark the patient as "Requires immediate life-saving intervention"
    And I submit the triage assessment
    Then the system calculates an ESI score of "1"
    And the system assigns triage level "Resuscitation"
    And the patient is moved to the top of all queues
    And a code alert is automatically triggered
    And the trauma team is notified immediately
    And the estimated wait time shows "Immediate - In Progress"

  Scenario: Assess pediatric patient with fever (ESI Level 3)
    Given a registered patient "Tommy Jones" (age 5) is waiting for triage
    When I select the patient for triage assessment
    And I enter the vital signs using pediatric parameters:
      | Vital Sign          | Value    |
      | Blood Pressure      | 95/60    |
      | Heart Rate          | 120      |
      | Respiratory Rate    | 24       |
      | Temperature         | 103.2°F  |
      | Oxygen Saturation   | 97%      |
    And I enter the chief complaint as "High fever and irritability"
    And I enter the pain scale as "6/10 (using FACES scale)"
    And I document onset as "Fever started yesterday evening"
    And I submit the triage assessment
    Then the system calculates an ESI score of "3" using pediatric criteria
    And the system assigns triage level "Urgent"
    And the patient is positioned in the urgent pediatric queue
    And the pediatric team is notified
    And the estimated wait time is updated to "30-45 minutes"

  Scenario: Handle incomplete vital signs during triage
    Given a registered patient "Mary Johnson" is waiting for triage
    When I select the patient for triage assessment
    And I attempt to enter incomplete vital signs:
      | Vital Sign          | Value    |
      | Blood Pressure      | 130/85   |
      | Heart Rate          |          |
      | Respiratory Rate    | 18       |
      | Temperature         |          |
      | Oxygen Saturation   | 98%      |
    And I enter the chief complaint as "Headache"
    And I submit the triage assessment
    Then the system displays validation errors:
      | Missing Field       | Error Message                |
      | Heart Rate          | Heart rate is required       |
      | Temperature         | Temperature is required      |
    And the ESI score cannot be calculated
    And the assessment remains incomplete
    And I must complete all required fields before proceeding

  Scenario: Reassess patient with worsening condition
    Given a patient "Robert Davis" has been triaged as ESI Level 4
    And the patient has been waiting for 90 minutes
    When I select the patient for reassessment
    And I enter updated vital signs:
      | Vital Sign          | Value    |
      | Blood Pressure      | 90/50    |
      | Heart Rate          | 120      |
      | Respiratory Rate    | 26       |
      | Temperature         | 101.5°F  |
      | Oxygen Saturation   | 92%      |
    And I update the chief complaint to "Worsening abdominal pain with nausea"
    And I enter the updated pain scale as "9/10"
    And I submit the reassessment
    Then the system recalculates the ESI score to "2"
    And the system updates triage level to "High Priority"
    And the patient is moved to the front of the high priority queue
    And an escalation alert is sent to the charge nurse
    And a note is added documenting the condition change

  Scenario: Process multiple patients in triage queue
    Given multiple patients are waiting for triage:
      | Patient Name    | Registration Time | Status        |
      | Alice Brown     | 10:00 AM         | Waiting       |
      | Bob Wilson      | 10:15 AM         | Waiting       |
      | Carol Davis     | 10:30 AM         | Waiting       |
    When I complete triage assessments for all patients:
      | Patient Name | ESI Score | Triage Level  |
      | Alice Brown  | 3         | Urgent        |
      | Bob Wilson   | 4         | Less Urgent   |
      | Carol Davis  | 2         | High Priority |
    Then the system positions patients in queue order:
      | Queue Position | Patient Name | Triage Level  |
      | 1              | Carol Davis  | High Priority |
      | 2              | Alice Brown  | Urgent        |
      | 3              | Bob Wilson   | Less Urgent   |
    And wait times are calculated based on queue position and available resources
    And the triage dashboard is updated with current queue status
