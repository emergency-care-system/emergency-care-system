# # Provider Assignment
#
# **Key Components:**
#
# - Automated provider-patient matching based on priority, specialty, and availability
#
# - Mobile notification system with rich content and urgency indicators
#
# - Workload balancing algorithms considering provider fatigue and case complexity
#
# - Real-time queue optimization ensuring highest priority patients are seen first
#
# **Comprehensive Scenarios:**
#
# - Basic assignment workflow - Standard provider assignment when becoming available
#
# - Specialty matching - Assigning patients to providers with appropriate specializations
#
# - Critical patient prioritization - Override queue order for life-threatening cases
#
# - High volume coordination - Managing multiple simultaneous provider assignments
#
# - Workload balancing - Distributing patients based on provider capacity and fatigue
#
# - Patient preferences - Honoring specific provider requests when medically appropriate
#
# - Notification failure handling - Backup communication methods and escalation procedures
#
# - Shift change management - Provider assignments during transition periods
#
# - Performance tracking - Metrics and optimization for assignment efficiency
#
# **Healthcare-Specific Features:**
#
# - ESI-based priority algorithms for patient assignment
#
# - Specialty and competency matching (pediatrics, trauma, etc.)
#
# - Provider workload and fatigue tracking
#
# - Mobile notification system with clinical context
#
# - Backup communication methods (overhead page, desktop alerts)
#
# - Shift handoff integration and continuity planning
#
# - Patient preference accommodation when clinically appropriate
#
# - Real-time queue optimization and wait time updates
#
# - Performance metrics tracking (response times, accuracy)
#
# - VIP and special needs patient handling
#
# - Critical patient escalation protocols
#
# - Quality assurance and improvement analytics

Feature: Provider Assignment
  As a charge nurse
  I want the system to automatically assign the next appropriate patient when providers become available
  So that patient flow is optimized and wait times are minimized

  Background:
    Given the ED management system is operational
    And the provider assignment module is active
    And provider workload tracking is enabled
    And mobile notification system is functional

  Scenario: Assign highest priority patient to newly available physician
    Given "Dr. Johnson" was seeing a patient in bed "ED-8"
    And the current patient queue contains:
      | Position | Patient Name    | ESI Level | Triage Level   | Wait Time | Bed Ready |
      | 1        | Maria Santos    | 2         | High Priority  | 45 min    | Yes       |
      | 2        | Robert Kim      | 2         | High Priority  | 60 min    | Yes       |
      | 3        | Lisa Chen       | 3         | Urgent         | 90 min    | Yes       |
      | 4        | David Brown     | 3         | Urgent         | 105 min   | No        |
    When "Dr. Johnson" completes the discharge for the patient in bed "ED-8"
    And the system detects "Dr. Johnson" is now available
    Then the system identifies the next patient assignment:
      | Selection Criteria | Value                                         |
      | Highest Priority   | Maria Santos (ESI Level 2)                   |
      | Bed Availability   | Bed ready for immediate assignment            |
      | Provider Match     | Dr. Johnson available and qualified          |
    And the system assigns "Maria Santos" to "Dr. Johnson"
    And a notification is sent to Dr. Johnson's mobile device:
      | Notification Type  | Content                                       |
      | Patient Assignment | 📱 New Patient: Maria Santos, Bed ED-12     |
      | Priority Level     | ESI Level 2 - High Priority                  |
      | Chief Complaint    | Severe chest pain                            |
      | Wait Time          | Patient waiting 45 minutes                   |
      | Action Required    | Please proceed to ED-12                      |
    And the patient status is updated to "Assigned to Dr. Johnson"
    And the queue position is updated for remaining patients

  Scenario: Handle provider assignment with specialty requirements
    Given "Dr. Martinez" (Emergency Medicine) becomes available
    And "Dr. Patel" (Pediatric Emergency) becomes available
    And the current queue contains:
      | Patient Name     | Age | ESI Level | Specialty Required     | Wait Time |
      | Adult Patient    | 45  | 2         | Emergency Medicine     | 30 min    |
      | Child Patient    | 8   | 2         | Pediatric Emergency    | 35 min    |
      | General Patient  | 30  | 3         | Any                    | 60 min    |
    When both providers request their next patient assignment
    Then the system matches providers to appropriate patients:
      | Provider      | Assigned Patient | Rationale                              |
      | Dr. Patel     | Child Patient    | Pediatric specialty match              |
      | Dr. Martinez  | Adult Patient    | Emergency medicine, same priority      |
    And specialty-specific notifications are sent:
      | Provider      | Notification Content                              |
      | Dr. Patel     | 👶 Pediatric Patient: Age 8, ESI 2, Fever       |
      | Dr. Martinez  | 🏥 Adult Patient: Age 45, ESI 2, Chest pain     |
    And the general patient remains in queue for the next available provider

  Scenario: Prioritize critical patient over standard queue order
    Given "Dr. Thompson" becomes available
    And the queue contains patients in order:
      | Position | Patient Name    | ESI Level | Assigned Bed | Special Circumstances |
      | 1        | Standard Patient| 3         | ED-5         | None                  |
      | 2        | Urgent Patient  | 3         | ED-7         | None                  |
      | 3        | Critical Patient| 1         | ED-TRAUMA-1  | Just arrived          |
    When the system identifies the next patient for "Dr. Thompson"
    Then the system prioritizes by acuity over queue position:
      | Assignment Logic   | Reasoning                                     |
      | Skip Queue Order   | ESI Level 1 takes priority over Level 3      |
      | Critical Priority  | Life-threatening condition requires immediate |
      | Provider Capability| Dr. Thompson qualified for trauma cases      |
    And "Critical Patient" is assigned to "Dr. Thompson"
    And the mobile notification includes urgency indicators:
      | Notification Field | Content                                       |
      | Priority Alert     | 🚨 CRITICAL: ESI Level 1 - Trauma           |
      | Patient Location   | ED-TRAUMA-1                                   |
      | Immediate Action   | Requires immediate assessment                 |
      | Support Teams      | Trauma team standing by                       |

  Scenario: Handle provider assignment during high volume period
    Given the ED is operating at 95% capacity
    And multiple providers become available simultaneously:
      | Provider Name | Specialty          | Last Patient Completed |
      | Dr. Adams     | Emergency Medicine | 14:30                 |
      | Dr. Brown     | Emergency Medicine | 14:32                 |
      | Dr. Wilson    | Emergency Medicine | 14:35                 |
    And 12 patients are waiting to be seen
    When the system processes multiple provider assignments
    Then the system optimizes assignments across all available providers:
      | Provider    | Assigned Patient | ESI Level | Assignment Rationale        |
      | Dr. Adams   | Patient A        | 1         | Highest acuity available    |
      | Dr. Brown   | Patient B        | 2         | Second highest priority     |
      | Dr. Wilson  | Patient C        | 2         | Next in high priority queue |
    And coordinated notifications are sent to prevent conflicts
    And remaining patients receive updated wait time estimates
    And surge capacity protocols are activated if needed

  Scenario: Provider assignment with workload balancing
    Given provider workload tracking shows:
      | Provider Name | Patients Seen Today | Current Workload | Complexity Score |
      | Dr. Garcia    | 12                 | Light            | 85              |
      | Dr. Lee       | 18                 | Heavy            | 140             |
      | Dr. Foster    | 15                 | Moderate         | 110             |
    And "Dr. Garcia" and "Dr. Foster" both become available
    And the next patient is "Complex Patient" with multiple comorbidities
    When the system determines provider assignment
    Then the system considers workload balancing:
      | Assignment Factor | Dr. Garcia | Dr. Foster | Decision            |
      | Current Workload  | Light      | Moderate   | Favors Dr. Garcia   |
      | Complexity Fit    | Good       | Good       | Both qualified      |
      | Fatigue Factor    | Low        | Moderate   | Dr. Garcia preferred|
    And "Complex Patient" is assigned to "Dr. Garcia"
    And workload metrics are updated for both providers

  Scenario: Handle provider assignment with patient preferences
    Given a patient "VIP Patient" has requested "Dr. Johnson" if available
    And "Dr. Johnson" and "Dr. Smith" both become available
    And "VIP Patient" is next in the queue with ESI Level 3
    When the system processes provider assignment
    Then the system considers patient preferences:
      | Preference Factor | Details                                       |
      | Patient Request   | Specifically requested Dr. Johnson           |
      | Medical Appropriateness| Both doctors qualified for ESI Level 3   |
      | Availability      | Dr. Johnson available and willing            |
    And "VIP Patient" is assigned to "Dr. Johnson"
    And "Dr. Smith" receives the next patient in queue
    And the assignment includes preference notation:
      | Notification Field | Content                                       |
      | Assignment Reason  | Patient preference request honored            |
      | Special Notes      | VIP status - provide enhanced service         |

  Scenario: Provider assignment failure and backup procedures
    Given "Dr. Williams" becomes available
    And the highest priority patient is "Emergency Patient" (ESI Level 1)
    When the system attempts to send assignment notification to Dr. Williams
    And the mobile device notification fails to deliver
    Then the system activates backup notification procedures:
      | Backup Method     | Action Taken                                  |
      | Overhead Page     | "Dr. Williams to ED-TRAUMA-1 immediately"    |
      | Desktop Alert     | Popup on all ED workstations                 |
      | Charge Nurse Alert| Direct notification to charge nurse          |
      | Secondary Provider| Alert backup doctor if no response in 2 min  |
    And the system logs the notification failure for IT review
    And continues attempting mobile notification every 30 seconds
    And tracks response time for quality metrics

  Scenario: Handle provider assignment during shift change
    Given it is 19:00 during evening shift change
    And "Dr. Day" (day shift) is completing final patients
    And "Dr. Night" (evening shift) is beginning shift
    And a critical patient arrives requiring immediate attention
    When the system determines provider assignment for the critical patient
    Then the system considers shift transition factors:
      | Assignment Factor | Consideration                                 |
      | Shift Status      | Dr. Day finishing, Dr. Night starting        |
      | Continuity        | Assign to Dr. Night for ongoing care         |
      | Availability      | Dr. Night has capacity for complex case      |
    And the critical patient is assigned to "Dr. Night"
    And shift handoff information is included in the notification:
      | Handoff Component | Content                                       |
      | Shift Context     | New critical patient - evening shift start   |
      | Day Shift Status  | Dr. Day finishing last 2 patients           |
      | Support Available | Day shift available for consultation          |

  Scenario: Track provider response times and assignment efficiency
    Given provider assignment notifications are sent
    When providers respond to patient assignments
    Then the system tracks performance metrics:
      | Metric Type           | Measurement                                |
      | Notification to Response| Time from alert to bedside presence     |
      | Assignment Accuracy   | Correct provider-patient matching         |
      | Queue Optimization    | Wait time reduction effectiveness         |
    And generates provider performance reports:
      | Provider Name | Avg Response Time | Assignment Accuracy | Patient Satisfaction |
      | Dr. Johnson   | 3.2 minutes      | 98%                | 4.8/5               |
      | Dr. Smith     | 4.1 minutes      | 96%                | 4.6/5               |
    And identifies optimization opportunities:
      | Improvement Area      | Recommendation                             |
      | Response Time         | Target <3 minutes for critical patients   |
      | Assignment Matching   | Consider additional specialty training     |
      | Communication         | Implement two-way acknowledgment system    |

