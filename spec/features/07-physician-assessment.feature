# # Physician Assessment
#
# **Key Components:**
#
# - Comprehensive patient data display with all critical clinical information
#
# - Real-time data synchronization ensuring current information availability
#
# - Mobile-optimized interface for bedside access and clinical decision-making
#
# - Timestamp tracking for all clinical data and assessments
#
# **Comprehensive Scenarios:**
#
# - Complete assessment access - Full patient chart with all nursing data available
#
# - Active treatment monitoring - Real-time updates during ongoing patient care
#
# - Allergy and medication management - Critical safety information with interaction checking
#
# - Pediatric patient handling - Age-appropriate data display and calculations
#
# - Incomplete assessment handling - Managing partial data with clear status indicators
#
# - Shift change integration - Handoff notes and continuity of care information
#
# - Network connectivity resilience - Offline capability and data caching
#
# - Time-sensitive alerts - Critical notifications and alert management
#
# **Healthcare-Specific Features:**
#
# - ESI triage level integration
#
# - Vital signs with trending and normal ranges
#
# - Drug allergy and interaction checking
#
# - Pediatric-specific calculations and normal ranges
#
# - Weight-based medication dosing
#
# - Critical lab value alerts
#
# - Pain assessment tools (including pediatric FACES scale)
#
# - Shift handoff documentation
#
# - Real-time clinical alerts and notifications
#
# - Mobile accessibility for point-of-care use

Feature: Physician Assessment
  As a physician
  I want to access comprehensive patient information on my mobile device
  So that I can make informed clinical decisions based on current patient data

  Background:
    Given the ED management system is operational
    And I am logged in as "Dr. Smith" on the mobile app
    And the patient chart access module is enabled
    And real-time data synchronization is active

  Scenario: Access patient chart with complete nursing assessment
    Given a patient "Jennifer Martinez" is assigned to bed "ED-8"
    And the nursing assessment is completed with the following data:
      | Assessment Component | Value                           | Timestamp  |
      | Triage Level        | ESI Level 2 - High Priority    | 14:15      |
      | Chief Complaint     | Severe chest pain               | 14:15      |
      | Vital Signs         | BP: 160/95, HR: 110, RR: 22    | 14:20      |
      | Temperature         | 98.6°F                          | 14:20      |
      | Oxygen Saturation   | 94%                             | 14:20      |
      | Pain Scale          | 8/10                            | 14:20      |
      | Allergies           | Penicillin (rash), Shellfish   | 14:16      |
      | Current Medications | Metoprolol 50mg BID, Aspirin   | 14:17      |
    When I open the patient's chart for "Jennifer Martinez" in bed "ED-8"
    Then the system displays the patient summary with:
      | Section             | Content                                    |
      | Patient Identity    | Jennifer Martinez, DOB: 1975-03-15       |
      | Bed Assignment      | ED-8                                      |
      | Arrival Time        | 14:00                                     |
      | Triage Notes        | ESI Level 2 - Severe chest pain, onset 2h ago |
    And the vital signs section shows:
      | Vital Sign          | Value     | Timestamp | Trend   |
      | Blood Pressure      | 160/95    | 14:20     | High    |
      | Heart Rate          | 110       | 14:20     | Elevated|
      | Respiratory Rate    | 22        | 14:20     | Elevated|
      | Temperature         | 98.6°F    | 14:20     | Normal  |
      | Oxygen Saturation   | 94%       | 14:20     | Low     |
      | Pain Score          | 8/10      | 14:20     | Severe  |
    And the allergies section displays:
      | Allergy    | Reaction Type | Severity | Date Entered |
      | Penicillin | Rash         | Moderate | 14:16        |
      | Shellfish  | Unknown      | Unknown  | 14:16        |
    And the current medications section shows:
      | Medication     | Dosage    | Frequency | Last Given | Status  |
      | Metoprolol     | 50mg      | BID       | 08:00      | Active  |
      | Aspirin        | 81mg      | Daily     | 08:00      | Active  |

  Scenario: Access patient chart during active treatment
    Given a patient "Michael Chen" is assigned to bed "ED-3"
    And the patient is currently receiving active treatment
    And recent assessments include:
      | Assessment Type     | Data                            | Timestamp  |
      | Initial Triage      | ESI Level 3 - Abdominal pain   | 13:30      |
      | Nursing Assessment  | Vitals stable, pain 6/10        | 13:45      |
      | Lab Orders         | CBC, CMP, Lipase ordered        | 14:00      |
      | Recent Vitals      | BP: 130/80, HR: 88, T: 100.2°F | 14:15      |
    When I open the patient's chart for "Michael Chen" in bed "ED-3"
    Then the system displays real-time information with:
      | Section             | Content                                    |
      | Current Status      | Active treatment in progress              |
      | Most Recent Vitals  | BP: 130/80, HR: 88, T: 100.2°F (14:15)  |
      | Active Orders       | Lab work in progress                      |
      | Triage Summary      | ESI 3 - Abd pain, onset 6h ago          |
    And all data includes timestamps showing data freshness
    And any alerts or critical values are highlighted in red
    And pending lab results show "In Progress" status with expected completion time

  Scenario: View patient chart with medication allergies and interactions
    Given a patient "Robert Johnson" is assigned to bed "ED-12"
    And the patient has multiple drug allergies:
      | Allergy        | Reaction           | Severity | Source    |
      | Morphine       | Respiratory depression | Severe   | Patient   |
      | Codeine        | Nausea, vomiting   | Moderate | Patient   |
      | NSAIDs         | GI bleeding        | Severe   | History   |
    And current medications include:
      | Medication     | Dosage    | Prescriber | Timestamp |
      | Warfarin       | 5mg daily | Dr. Jones  | 10:00     |
      | Metformin      | 1000mg BID| Dr. Brown  | 10:00     |
    When I open the patient's chart for "Robert Johnson"
    Then the allergy section prominently displays:
      | Alert Type     | Message                                    |
      | Critical Alert | SEVERE ALLERGIES: Morphine, NSAIDs        |
      | Warning        | Moderate allergy: Codeine                 |
    And the medication section shows:
      | Medication | Status | Interaction Alerts           |
      | Warfarin   | Active | Monitor for bleeding risk    |
      | Metformin  | Active | No interactions detected     |
    And any new medication orders will trigger allergy checking
    And interaction warnings are displayed for contraindicated drugs

  Scenario: Access chart for pediatric patient with age-appropriate data
    Given a pediatric patient "Emma Foster" (age 7) is assigned to bed "ED-PEDS-2"
    And the nursing assessment includes pediatric-specific data:
      | Assessment Component | Value                           | Timestamp  |
      | Weight              | 22 kg (48 lbs)                  | 15:00      |
      | Height              | 120 cm                          | 15:00      |
      | Vital Signs         | BP: 95/60, HR: 110, RR: 24     | 15:05      |
      | Temperature         | 102.8°F                         | 15:05      |
      | Pain Scale          | 7/10 (FACES scale)              | 15:05      |
      | Parent/Guardian     | Sarah Foster (mother)           | 15:00      |
    When I open the pediatric patient's chart for "Emma Foster"
    Then the system displays pediatric-specific information:
      | Section                | Content                              |
      | Patient Age/Weight     | 7 years old, 22 kg                  |
      | Pediatric Vital Ranges | All vitals with age-appropriate norms|
      | Growth Percentiles     | Weight: 50th percentile              |
      | Guardian Information   | Sarah Foster (mother) - present      |
    And vital signs are displayed with pediatric normal ranges:
      | Vital Sign    | Value  | Normal Range (Age 7) | Status    |
      | Blood Pressure| 95/60  | 90-110/55-70        | Normal    |
      | Heart Rate    | 110    | 80-120              | Normal    |
      | Respiratory   | 24     | 18-25               | Normal    |
      | Temperature   | 102.8°F| 98.6°F ±1°F         | Elevated  |
    And medication dosing shows weight-based calculations
    And parental consent status is clearly indicated

  Scenario: Handle incomplete nursing assessment
    Given a patient "David Wilson" is assigned to bed "ED-6"
    And the nursing assessment is partially completed:
      | Completed Sections     | Status      | Timestamp |
      | Basic Demographics     | Complete    | 16:00     |
      | Initial Vital Signs    | Complete    | 16:05     |
      | Chief Complaint        | Complete    | 16:05     |
      | Allergy Assessment     | Incomplete  | -         |
      | Medication History     | Incomplete  | -         |
      | Pain Assessment        | Incomplete  | -         |
    When I open the patient's chart for "David Wilson"
    Then the system displays available information clearly marked:
      | Section           | Status                                    |
      | Completed Data    | Triage notes, initial vitals available   |
      | Missing Data      | Allergies: Assessment in progress         |
      | Missing Data      | Medications: History pending              |
      | Missing Data      | Pain scale: Not yet assessed             |
    And incomplete sections are highlighted with:
      | Visual Indicator  | Description                               |
      | Yellow Warning    | Assessment in progress                    |
      | Refresh Timer     | Auto-refresh every 30 seconds            |
      | Notification      | "Assessment updating - refresh for latest"|
    And I can request priority completion of missing critical data

  Scenario: Access chart during shift change with handoff notes
    Given a patient "Lisa Brown" is assigned to bed "ED-9"
    And it is during the evening shift change (19:00)
    And the day shift nurse added handoff notes:
      | Handoff Component  | Note                                      | Timestamp |
      | Clinical Status    | Patient stable, pain controlled           | 18:45     |
      | Pending Actions    | Awaiting orthopedic consult               | 18:45     |
      | Family Updates     | Son called, updated on condition          | 18:30     |
      | Special Instructions| Patient anxious, prefers female staff    | 18:45     |
    When I open the patient's chart for "Lisa Brown"
    Then the system prominently displays shift handoff information:
      | Handoff Section    | Content                                   |
      | Clinical Summary   | Stable condition, pain controlled         |
      | Pending Tasks      | Orthopedic consult ordered - pending      |
      | Communication Log  | Family contact: Son updated 18:30        |
      | Special Needs      | Patient preference: Female staff          |
    And the handoff notes are clearly timestamped
    And I can add my own physician handoff notes
    And the evening nurse can see both nursing and physician handoff information

  Scenario: Handle patient chart access during network connectivity issues
    Given a patient "Thomas Anderson" is assigned to bed "ED-4"
    And the mobile app has intermittent network connectivity
    When I attempt to open the patient's chart
    And the network connection is temporarily unavailable
    Then the system displays cached patient data with:
      | Data Type          | Availability                              |
      | Basic Demographics | Available (cached)                        |
      | Last Known Vitals  | Available - last sync 16:45              |
      | Medication Data    | Available (cached)                        |
      | Recent Lab Results | May not be current - sync pending        |
    And a connectivity warning is displayed: "Limited connectivity - data may not be current"
    And the app attempts automatic sync when connection is restored
    And critical data is prioritized for sync when connectivity returns
    And I can manually trigger refresh when connection improves

  Scenario: Access chart with time-sensitive alerts and notifications
    Given a patient "Karen White" is assigned to bed "ED-7"
    And the patient has time-sensitive clinical alerts:
      | Alert Type         | Message                           | Timestamp | Severity |
      | Critical Lab       | Troponin elevated 0.8 ng/mL      | 17:15     | High     |
      | Medication Due     | Scheduled medication due now      | 17:30     | Medium   |
      | Reassessment Due   | Pain reassessment overdue        | 17:25     | Medium   |
    When I open the patient's chart for "Karen White"
    Then the system prominently displays active alerts:
      | Alert Priority | Alert Details                             |
      | CRITICAL       | 🔴 Troponin 0.8 - Possible MI (17:15)   |
      | WARNING        | 🟡 Medication due - Metoprolol (17:30)   |
      | INFO           | 🔵 Pain reassessment overdue (17:25)     |
    And critical alerts require acknowledgment before proceeding
    And the timestamp shows how long ago each alert was generated
    And I can take direct action on alerts (order meds, document assessment)
    And alert resolution is tracked and timestamped
