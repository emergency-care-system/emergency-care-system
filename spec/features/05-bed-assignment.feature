# # Bed Assignment
#
# **Key Components:**
#
# - Acuity-based prioritization using ESI levels for bed assignments
#
# - Room type matching ensuring patients get appropriate care environments
#
# - Dynamic wait time calculations updated after each assignment
#
# - Multi-criteria decision making considering medical needs, privacy, and capacity
#
# **Comprehensive Scenarios:**
#
# - Standard room assignment - Basic acuity-based patient selection
#
# - Isolation room requirements - Infection control and specialized room needs
#
# - Pediatric room assignment - Age-specific room allocation with separate queues
#
# - No suitable patients - Handling room availability when no patients match criteria
#
# - Multiple simultaneous rooms - Optimizing assignments across multiple available beds
#
# - Gender considerations - Privacy preferences in room assignments
#
# - High volume crisis mode - Emergency protocols during capacity constraints
#
# - Wait time updates - Real-time recalculation after assignments
#
# - Recommendation rejection - Alternative options when initial suggestions aren't suitable
#
# **Healthcare-Specific Features:**
#
# - ESI-based priority algorithms
#
# - Room type categorization (standard, isolation, trauma, pediatric, cardiac)
#
# - Privacy and gender considerations
#
# - Infection control requirements
#
# - Crisis mode protocols for high-volume periods
#
# - Family notification systems
#
# - Equipment and monitoring requirements
#
# - Alternative recommendation engines

Feature: Bed Assignment
  As a charge nurse
  I want to receive bed assignment recommendations based on patient acuity and room type
  So that I can optimize patient flow and ensure appropriate care placement

  Background:
    Given the ED management system is operational
    And I am logged in as a charge nurse
    And the bed management module is active
    And the patient prioritization algorithm is enabled

  Scenario: Assign standard room to highest acuity patient
    Given multiple patients are waiting for beds:
      | Patient Name     | ESI Level | Triage Level   | Wait Time | Room Requirements |
      | Maria Gonzalez   | 2         | High Priority  | 45 min    | Standard          |
      | James Brown      | 3         | Urgent         | 90 min    | Standard          |
      | Susan Davis      | 4         | Less Urgent    | 120 min   | Standard          |
      | Peter Kim        | 3         | Urgent         | 60 min    | Standard          |
    And a standard room "ED-12" becomes available
    When I request bed assignment recommendations
    Then the system suggests "Maria Gonzalez" as the top recommendation
    And the recommendation shows:
      | Field               | Value                                    |
      | Recommended Patient | Maria Gonzalez                          |
      | ESI Level          | 2                                       |
      | Wait Time          | 45 minutes                              |
      | Room Match         | Standard room suitable                   |
      | Rationale          | Highest acuity patient requiring standard bed |
    And the system displays updated wait times for remaining patients:
      | Patient Name     | Updated Wait Time | Position in Queue |
      | Peter Kim        | 30-45 minutes     | Next for standard |
      | James Brown      | 45-60 minutes     | 2nd in queue      |
      | Susan Davis      | 90-120 minutes    | 3rd in queue      |

  Scenario: Assign isolation room based on patient requirements
    Given multiple patients are waiting for beds:
      | Patient Name     | ESI Level | Triage Level   | Room Requirements | Chief Complaint        |
      | Alex Johnson     | 3         | Urgent         | Isolation         | Suspected TB           |
      | Linda Wilson     | 2         | High Priority  | Standard          | Chest pain             |
      | Mark Rodriguez   | 3         | Urgent         | Standard          | Abdominal pain         |
    And an isolation room "ED-ISO-2" becomes available
    When I request bed assignment recommendations
    Then the system suggests "Alex Johnson" as the top recommendation
    And the recommendation shows:
      | Field               | Value                                    |
      | Recommended Patient | Alex Johnson                            |
      | Room Type          | Isolation room                          |
      | Rationale          | Patient requires isolation precautions   |
      | Infection Control  | Airborne precautions needed             |
    And the system displays that other patients cannot use this room type
    And the estimated wait times for standard rooms remain unchanged for other patients

  Scenario: Handle pediatric room assignment
    Given multiple patients are waiting for beds:
      | Patient Name     | Age | ESI Level | Triage Level   | Room Requirements |
      | Tommy Anderson   | 6   | 3         | Urgent         | Pediatric         |
      | Sarah Mitchell   | 4   | 2         | High Priority  | Pediatric         |
      | David Chen       | 35  | 2         | High Priority  | Standard          |
      | Emily Foster     | 8   | 4         | Less Urgent    | Pediatric         |
    And a pediatric room "ED-PEDS-3" becomes available
    When I request bed assignment recommendations
    Then the system suggests "Sarah Mitchell" as the top recommendation
    And the recommendation shows:
      | Field               | Value                                    |
      | Recommended Patient | Sarah Mitchell                          |
      | Age                | 4 years old                             |
      | ESI Level          | 2                                       |
      | Room Type          | Pediatric room                          |
      | Rationale          | Highest acuity pediatric patient         |
    And the system displays updated wait times for remaining pediatric patients:
      | Patient Name     | Updated Wait Time | Position in Pediatric Queue |
      | Tommy Anderson   | 20-30 minutes     | Next for pediatric room     |
      | Emily Foster     | 60-90 minutes     | 2nd in pediatric queue      |
    And David Chen remains in the adult standard room queue

  Scenario: No suitable patients for available room type
    Given multiple patients are waiting for beds:
      | Patient Name     | ESI Level | Room Requirements |
      | Robert Taylor    | 3         | Standard          |
      | Nancy White     | 4         | Standard          |
      | Kevin Brown     | 3         | Standard          |
    And a trauma room "ED-TRAUMA-1" becomes available
    When I request bed assignment recommendations
    Then the system displays "No suitable patients for trauma room"
    And the system suggests:
      | Recommendation Type | Details                                  |
      | Alternative Use     | Consider using for high acuity standard patients |
      | Room Conversion     | Can be downgraded to standard room if needed     |
      | Hold for Emergency  | Keep available for incoming trauma cases         |
    And the system maintains the trauma room as available
    And no patient assignments are automatically made

  Scenario: Handle multiple rooms becoming available simultaneously
    Given multiple patients are waiting for beds:
      | Patient Name     | ESI Level | Room Requirements | Wait Time |
      | Carol Davis      | 1         | Trauma            | 10 min    |
      | Frank Miller     | 2         | Standard          | 30 min    |
      | Helen Garcia     | 2         | Isolation         | 45 min    |
      | Joe Wilson       | 3         | Standard          | 60 min    |
      | Amy Johnson      | 3         | Standard          | 75 min    |
    And multiple rooms become available:
      | Room Number      | Room Type  |
      | ED-TRAUMA-2      | Trauma     |
      | ED-15            | Standard   |
      | ED-ISO-1         | Isolation  |
    When I request bed assignment recommendations
    Then the system provides multiple recommendations:
      | Room Number | Recommended Patient | ESI Level | Rationale                    |
      | ED-TRAUMA-2 | Carol Davis         | 1         | Critical patient, trauma room |
      | ED-ISO-1    | Helen Garcia        | 2         | Isolation requirements        |
      | ED-15       | Frank Miller        | 2         | Highest acuity for standard   |
    And the system updates wait times for all remaining patients
    And the recommendations are ranked by patient acuity priority

  Scenario: Consider patient gender for room assignment
    Given multiple patients are waiting for beds:
      | Patient Name     | Gender | ESI Level | Room Requirements |
      | Michelle Lee     | Female | 3         | Standard          |
      | Anthony Clark    | Male   | 3         | Standard          |
      | Patricia Moore   | Female | 4         | Standard          |
    And a standard room "ED-8" becomes available
    And the room currently has a male patient in the adjacent bed
    When I request bed assignment recommendations considering privacy preferences
    Then the system suggests "Anthony Clark" as the top recommendation
    And the recommendation includes:
      | Field               | Value                                    |
      | Privacy Consideration| Same gender as adjacent patient         |
      | Alternative Option  | Michelle Lee (if privacy not a concern)  |
    And I can override the gender consideration if clinically necessary

  Scenario: Handle bed assignment during high volume period
    Given the ED is operating at 95% capacity
    And multiple high-acuity patients are waiting:
      | Patient Name     | ESI Level | Wait Time | Special Requirements |
      | Crisis Patient A | 1         | 5 min     | Immediate care       |
      | Crisis Patient B | 2         | 15 min    | Cardiac monitoring   |
      | Crisis Patient C | 2         | 20 min    | None                |
    And a standard room "ED-6" becomes available
    When I request bed assignment recommendations during crisis mode
    Then the system prioritizes "Crisis Patient A" despite room type mismatch
    And the system displays:
      | Alert Type          | Message                                  |
      | High Volume Alert   | ED at capacity - emergency protocols active |
      | Room Flex Option    | Standard room can accommodate ESI Level 1    |
      | Resource Alert      | Additional equipment may be needed           |
    And the system suggests moving lower acuity patients to make trauma rooms available

  Scenario: Update wait times after bed assignment
    Given the following patients are waiting:
      | Patient Name     | ESI Level | Current Wait Time Estimate |
      | Patient A        | 2         | 30 minutes                |
      | Patient B        | 3         | 60 minutes                |
      | Patient C        | 3         | 75 minutes                |
      | Patient D        | 4         | 120 minutes               |
    And I assign Patient A to the available room
    When the bed assignment is confirmed
    Then the system recalculates wait times for remaining patients:
      | Patient Name     | Updated Wait Time | Change        |
      | Patient B        | 45 minutes       | Reduced 15 min |
      | Patient C        | 60 minutes       | Reduced 15 min |
      | Patient D        | 105 minutes      | Reduced 15 min |
    And the updated wait times are displayed on the patient tracking board
    And family members are notified of updated estimates via the patient portal

  Scenario: Handle bed assignment rejection and alternative selection
    Given the system recommends "John Smith" for room "ED-10"
    And John Smith has ESI Level 2 with chest pain
    When I review the recommendation
    And I determine that John Smith needs cardiac monitoring not available in ED-10
    And I reject the system recommendation
    Then the system provides alternative recommendations:
      | Alternative Patient | ESI Level | Rationale                           |
      | Lisa Brown         | 2         | Next highest acuity, standard care   |
      | Mike Johnson       | 3         | Suitable for standard room          |
    And the system suggests alternative rooms for John Smith:
      | Room Number | Room Type        | Availability    |
      | ED-CCU-1   | Cardiac Monitor  | Available now   |
      | ED-CCU-2   | Cardiac Monitor  | Available 15min |
    And I can select an alternative patient or wait for appropriate room for John Smith
