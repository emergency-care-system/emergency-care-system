# # Dynamic Queue Updates
#
# **Key Components:**
#
# - Real-time queue reprioritization based on ESI levels and patient acuity
#
# - Cascading wait time calculations that account for new high-priority arrivals
#
# - Multi-factor prioritization considering bed availability, provider capacity, and specialty needs
#
# - Comprehensive notification system for staff, patients, and families
#
# **Comprehensive Scenarios:**
#
# - Single trauma arrival - Basic queue reprioritization when ESI Level 1 patient arrives
#
# - Multiple simultaneous arrivals - Handling several high-acuity patients arriving together
#
# - Bed availability constraints - Queue management considering physical bed limitations
#
# - Provider capacity changes - Adjusting queue times when staffing levels change
#
# - Patient deterioration - Real-time escalation when existing patients worsen
#
# - Specialty service coordination - Balancing acuity with specialist availability
#
# - Time-sensitive conditions - Managing stroke, STEMI, and other time-critical cases
#
# - Real-time visualization - Dashboard and family portal updates
#
# **Healthcare-Specific Features:**
#
# - ESI-based priority algorithms with automatic queue reordering
#
# - Cascading wait time impact calculations
#
# - Bed type and availability integration
#
# - Provider capacity and throughput modeling
#
# - Patient deterioration detection and escalation
#
# - Specialty service coordination and consultation alerts
#
# - Time-sensitive treatment window tracking
#
# - Multi-stakeholder notification systems (staff, patients, families)
#
# - Real-time dashboard and portal updates
#
# - Resource constraint awareness (beds, staff, equipment)
#
# - Emergency protocol activation for critical patients
#
# - Historical wait time analysis and prediction modeling

Feature: Dynamic Queue Updates
  As a charge nurse
  I want the patient queue to automatically adjust when new high-acuity patients arrive
  So that critical patients receive immediate priority and wait times remain accurate

  Background:
    Given the ED management system is operational
    And the dynamic queue management module is active
    And the ESI (Emergency Severity Index) prioritization system is enabled
    And 15 patients are currently waiting to be seen

  Scenario: High-priority trauma patient bumps existing queue
    Given the current patient queue contains:
      | Position | Patient Name    | ESI Level | Triage Level   | Current Wait Time |
      | 1        | Alice Johnson   | 2         | High Priority  | 20 minutes       |
      | 2        | Bob Williams    | 2         | High Priority  | 35 minutes       |
      | 3        | Carol Davis     | 3         | Urgent         | 45 minutes       |
      | 4        | David Brown     | 3         | Urgent         | 60 minutes       |
      | 5        | Emma Wilson     | 3         | Urgent         | 75 minutes       |
      | 6-15     | Other patients  | 3-5       | Various        | 90-180 minutes   |
    And available providers can see 1 patient every 30 minutes
    When a new trauma patient "Emergency Trauma" arrives with ESI level 1
    And the patient is triaged as "Resuscitation - Life threatening"
    Then the system immediately reprioritizes the queue:
      | New Position | Patient Name      | ESI Level | Wait Time Impact    |
      | 1            | Emergency Trauma  | 1         | Immediate           |
      | 2            | Alice Johnson     | 2         | +30 min (50 min)    |
      | 3            | Bob Williams      | 2         | +30 min (65 min)    |
      | 4            | Carol Davis       | 3         | +30 min (75 min)    |
      | 5            | David Brown       | 3         | +30 min (90 min)    |
    And the system updates wait time estimates for all affected patients:
      | Patient Name    | Previous Estimate | New Estimate | Change      |
      | Alice Johnson   | 20 minutes       | 50 minutes   | +30 minutes |
      | Bob Williams    | 35 minutes       | 65 minutes   | +30 minutes |
      | Carol Davis     | 45 minutes       | 75 minutes   | +30 minutes |
      | All others      | Various          | +30 minutes  | Increased   |
    And notifications are sent to affected patients and families
    And the trauma team is immediately alerted for the ESI Level 1 patient

  Scenario: Multiple high-acuity patients arrive simultaneously
    Given the current queue has patients with ESI levels 3-5
    And the next available appointment slot is in 60 minutes
    When multiple high-acuity patients arrive within 10 minutes:
      | Arrival Time | Patient Name     | ESI Level | Chief Complaint          |
      | 14:00        | Critical Patient | 1         | Cardiac arrest           |
      | 14:05        | Urgent Patient A | 2         | Severe chest pain        |
      | 14:08        | Urgent Patient B | 2         | Difficulty breathing     |
    Then the system prioritizes patients by ESI level and arrival time:
      | New Position | Patient Name     | ESI Level | Priority Rationale        |
      | 1            | Critical Patient | 1         | Highest acuity - immediate |
      | 2            | Urgent Patient A | 2         | ESI 2 - arrived first     |
      | 3            | Urgent Patient B | 2         | ESI 2 - arrived second    |
      | 4-18         | Existing patients| 3-5       | Lower priority            |
    And the system calculates cascading wait time impacts:
      | Patient Category | Wait Time Impact                              |
      | ESI Level 3      | +90 minutes (3 new higher priority patients) |
      | ESI Level 4      | +90 minutes                                   |
      | ESI Level 5      | +90 minutes                                   |
    And multiple department alerts are triggered:
      | Department    | Alert Type                                    |
      | Trauma Team   | ESI 1 - Immediate response required          |
      | Cardiology    | Multiple cardiac-related ESI 2 patients     |
      | Administration| Surge capacity - consider additional staff   |

  Scenario: Queue updates with bed availability constraints
    Given 15 patients are waiting and only 2 beds are currently available
    And the bed types are:
      | Bed Number | Bed Type     | Status    |
      | ED-5       | Standard     | Available |
      | ED-TRAUMA-1| Trauma       | Available |
      | ED-8       | Standard     | Occupied  |
      | ED-12      | Isolation    | Occupied  |
    When a trauma patient with ESI level 1 arrives requiring trauma bay
    Then the system updates the queue considering bed constraints:
      | Queue Position | Patient Name    | ESI Level | Bed Assignment Strategy     |
      | 1              | Trauma Patient  | 1         | ED-TRAUMA-1 (immediate)     |
      | 2              | Alice Johnson   | 2         | ED-5 when available         |
      | 3              | Bob Williams    | 2         | Wait for next bed           |
    And wait times reflect both queue position and bed availability:
      | Patient Name    | Queue Wait | Bed Wait  | Total Estimate |
      | Trauma Patient  | 0 minutes  | 0 minutes | Immediate      |
      | Alice Johnson   | 0 minutes  | 0 minutes | Immediate      |
      | Bob Williams    | 0 minutes  | 45 minutes| 45 minutes     |
    And the system provides realistic expectations based on resource constraints

  Scenario: Handle queue updates during provider capacity changes
    Given the current provider capacity is 3 physicians seeing patients
    And average patient encounter time is 30 minutes
    And 15 patients are in queue with estimated wait times
    When one physician becomes unavailable due to emergency procedure
    And a new ESI level 1 patient arrives
    Then the system recalculates queue times with reduced capacity:
      | Capacity Change | Impact                                        |
      | 3 → 2 providers | 50% increase in wait times for existing patients |
      | ESI 1 arrival  | All patients bumped down one position        |
    And updated wait time calculations reflect both changes:
      | Patient Category | Original Wait | Capacity Impact | ESI 1 Impact | New Wait   |
      | ESI Level 2      | 30 minutes   | +15 minutes     | +30 minutes  | 75 minutes |
      | ESI Level 3      | 60 minutes   | +30 minutes     | +30 minutes  | 120 minutes|
      | ESI Level 4      | 90 minutes   | +45 minutes     | +30 minutes  | 165 minutes|
    And the system sends capacity alerts to administration
    And patients/families are notified of updated wait times

  Scenario: Prioritize patient with rapidly deteriorating condition
    Given a patient "Sarah Mitchell" is currently position 8 in queue with ESI level 3
    And her initial complaint was "mild abdominal pain"
    When the patient's condition deteriorates and reassessment shows:
      | Assessment Change | New Value                                     |
      | Pain Level        | 3/10 → 9/10                                  |
      | Vital Signs       | BP: 120/80 → 85/50, HR: 80 → 120            |
      | Mental Status     | Alert → Confused                             |
      | ESI Level         | 3 → 2                                        |
    Then the system immediately updates her queue position:
      | Action Type       | Details                                       |
      | Priority Escalation| ESI 3 → ESI 2 due to deterioration          |
      | Queue Repositioning| Position 8 → Position 2                     |
      | Wait Time Update  | 120 minutes → 15 minutes                     |
    And escalation notifications are sent:
      | Recipient         | Notification Content                          |
      | Attending Physician| Patient deterioration - Priority escalated   |
      | Charge Nurse      | Sarah Mitchell moved to position 2           |
      | Triage Nurse      | Reassessment resulted in ESI upgrade         |
    And all subsequent patients are shifted down in the queue
    And family members are notified of the priority change

  Scenario: Handle specialty service requirements in queue management
    Given 15 patients are waiting with various specialty needs:
      | Patient Name    | ESI Level | Specialty Required  | Current Position |
      | General Patient | 3         | None               | 3                |
      | Cardiac Patient | 2         | Cardiology         | 5                |
      | Peds Patient    | 3         | Pediatrics         | 8                |
    When a new trauma patient arrives requiring neurosurgery consultation
    And the patient has ESI level 1
    Then the system considers both acuity and specialty availability:
      | Priority Factor   | Consideration                                 |
      | ESI Level 1       | Highest medical priority                     |
      | Neurosurgery Need | Specialty consultant availability            |
      | Resource Planning | OR availability for potential surgery        |
    And the queue is updated with specialty considerations:
      | Position | Patient Name    | Priority Reason                          |
      | 1        | Trauma/Neuro    | ESI 1 + Specialty coordination needed   |
      | 2        | Cardiac Patient | ESI 2 + Cardiology available           |
      | 3        | General Patient | ESI 3 but no specialty delay           |
    And specialty teams are notified with urgency levels

  Scenario: Queue updates with time-sensitive treatment windows
    Given several patients with time-sensitive conditions are in queue:
      | Patient Name    | Condition           | Treatment Window | Queue Position |
      | Stroke Patient  | Suspected stroke    | 4.5 hours       | 4              |
      | STEMI Patient   | Heart attack        | 90 minutes      | 6              |
    When a new ESI level 1 trauma patient arrives
    Then the system balances acuity with time sensitivity:
      | Prioritization Logic | Decision Rationale                        |
      | ESI 1 Trauma        | Immediate life threat - top priority      |
      | STEMI Patient       | Time-critical (90 min) - position 2      |
      | Stroke Patient      | Time-critical (4.5 hr) - position 3      |
    And time-sensitive alerts are maintained:
      | Patient Type    | Alert Status                                  |
      | Stroke Patient  | 45 minutes remaining in optimal window       |
      | STEMI Patient   | 25 minutes remaining for door-to-balloon     |
    And the system tracks treatment deadlines for all time-sensitive cases

  Scenario: Real-time queue visualization updates
    Given the ED dashboard displays the current patient queue
    And family members can view estimated wait times on patient portal
    When queue positions change due to new arrivals
    Then all displays update in real-time:
      | Display Location    | Update Type                                 |
      | Main ED Dashboard   | Queue positions and wait times refreshed   |
      | Patient Portal      | Family notifications of wait time changes  |
      | Mobile Apps         | Provider apps show updated patient lists   |
      | Waiting Room Display| General wait time estimates updated        |
    And update timestamps are shown on all displays:
      | Display Element     | Information Provided                        |
      | Last Updated        | "Queue updated at 14:35:22"                |
      | Next Update         | "Automatic refresh in 30 seconds"          |
      | Manual Refresh      | Button available for immediate update       |
    And change notifications highlight significant updates for staff attention
