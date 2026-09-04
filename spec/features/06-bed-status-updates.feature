# # Bed Status Updates
#
# **Key Components:**
#
# - Automated notification system for housekeeping and maintenance teams
#
# - Real-time bed availability tracking with accurate counts
#
# - Status workflow management ensuring proper transitions between states
#
# - Integration with multiple departments (housekeeping, maintenance, infection control)
#
# **Comprehensive Scenarios:**
#
# - Standard discharge cleaning - Basic workflow for marking beds dirty after patient discharge
#
# - Isolation deep cleaning - Special protocols for infectious patients requiring enhanced cleaning
#
# - Cleaning completion - Housekeeping workflow to mark beds ready for use
#
# - Maintenance requests - Handling equipment issues during bed transitions
#
# - Patient transfer status - Managing beds during patient movement between units
#
# - Batch updates - Simultaneous status changes for multiple beds
#
# - Urgent turnover - Expedited cleaning protocols during high-capacity periods
#
# - Status validation - Preventing invalid status transitions and ensuring data integrity
#
# - Audit trail tracking - Complete history and compliance documentation
#
# - System resilience - Handling notification failures and system maintenance
#
# **Healthcare-Specific Features:**
#
# - Infection control protocols for isolation rooms
#
# - Priority-based cleaning notifications
#
# - Equipment maintenance integration
#
# - Capacity management during high-volume periods
#
# - Compliance and audit trail requirements
#
# - Multi-department coordination
#
# - Real-time dashboard updates
#
# - Emergency protocols for urgent bed turnover

Feature: Bed Status Updates
  As a nurse
  I want to update bed statuses during patient flow transitions
  So that housekeeping is notified and bed availability is accurately tracked

  Background:
    Given the ED management system is operational
    And I am logged in as a nurse
    And the bed management module is active
    And housekeeping notification system is enabled

  Scenario: Mark bed as needs cleaning after patient discharge
    Given a patient "John Doe" is currently occupying bed "ED-12"
    And the bed status is "Occupied"
    And the available bed count shows 8 out of 20 beds available
    When the patient is discharged from bed "ED-12"
    And I mark the bed as "Needs Cleaning"
    And I submit the bed status update
    Then the system updates the bed status to "Dirty"
    And a notification is sent to housekeeping with details:
      | Field              | Value                           |
      | Room Number        | ED-12                          |
      | Status            | Needs Cleaning                  |
      | Priority          | Standard                       |
      | Patient Type      | Standard discharge             |
      | Special Requirements| Standard cleaning protocol     |
      | Timestamp         | Current time                   |
    And the bed is removed from the available bed count
    And the available bed count updates to 7 out of 20 beds available
    And the bed appears as "Dirty" on the bed management dashboard

  Scenario: Mark isolation bed for deep cleaning after infectious patient
    Given a patient "Jane Smith" with isolation precautions is in bed "ED-ISO-2"
    And the bed status is "Occupied - Isolation"
    And the patient had confirmed MRSA infection
    When the patient is discharged from bed "ED-ISO-2"
    And I mark the bed as "Needs Deep Cleaning"
    And I specify the isolation type as "Contact Precautions - MRSA"
    And I submit the bed status update
    Then the system updates the bed status to "Dirty - Isolation"
    And a high-priority notification is sent to housekeeping with details:
      | Field              | Value                           |
      | Room Number        | ED-ISO-2                       |
      | Status            | Needs Deep Cleaning             |
      | Priority          | High                           |
      | Infection Type    | MRSA - Contact Precautions     |
      | Special Requirements| Terminal cleaning required      |
      | PPE Required      | Gowns, gloves, masks           |
    And the bed is flagged as "Out of Service" until deep cleaning completion
    And the isolation bed count is reduced by one
    And an alert is sent to infection control team

  Scenario: Housekeeping completes cleaning and marks bed ready
    Given bed "ED-8" has status "Dirty"
    And housekeeping was notified 30 minutes ago
    When the housekeeping staff completes cleaning of bed "ED-8"
    And the housekeeping supervisor marks the bed as "Clean and Ready"
    And submits the cleaning completion with details:
      | Field              | Value                           |
      | Cleaning Staff     | Maria Rodriguez                 |
      | Cleaning Start     | 14:30                          |
      | Cleaning End       | 15:00                          |
      | Cleaning Type      | Standard                       |
      | Supplies Used      | Standard disinfection          |
    Then the system updates the bed status to "Available"
    And the bed is added back to the available bed count
    And the available bed count increases by one
    And a notification is sent to the charge nurse that bed "ED-8" is ready
    And the bed appears as "Available" on the bed management dashboard

  Scenario: Handle bed maintenance request during status update
    Given a patient is discharged from bed "ED-15"
    When I attempt to mark the bed as "Needs Cleaning"
    And I notice equipment malfunction in the room
    And I select "Maintenance Required" in addition to cleaning needs
    And I specify the issue as "IV pump not functioning, call light broken"
    And I submit the bed status update
    Then the system updates the bed status to "Out of Service - Maintenance"
    And notifications are sent to both:
      | Department        | Notification Details            |
      | Housekeeping      | Hold cleaning until maintenance |
      | Maintenance       | IV pump and call light repair   |
    And the bed is removed from available count until both issues are resolved
    And a work order is automatically generated for maintenance
    And the estimated downtime is calculated and displayed

  Scenario: Update bed status during patient transfer
    Given a patient "Robert Wilson" is in bed "ED-6"
    And the patient needs to be transferred to ICU
    When the transport team arrives to transfer the patient
    And I update the bed status to "Patient in Transit"
    And I specify the destination as "ICU Room 302"
    And I submit the status update
    Then the bed status is temporarily set to "In Transit"
    And the bed remains unavailable for new assignments
    And a notification is sent to the receiving unit
    And when the transfer is confirmed complete, I can mark the bed as "Needs Cleaning"
    And the normal cleaning workflow is initiated

  Scenario: Handle multiple bed status updates simultaneously
    Given multiple beds require status updates:
      | Bed Number | Current Status | Required Action        |
      | ED-3       | Occupied      | Patient discharged     |
      | ED-7       | Occupied      | Transfer to surgery    |
      | ED-11      | Dirty         | Cleaning completed     |
      | ED-14      | Occupied      | Patient discharged     |
    When I perform batch bed status updates:
      | Bed Number | New Status           | Action Required        |
      | ED-3       | Needs Cleaning       | Notify housekeeping    |
      | ED-7       | Patient in Transit   | Hold for confirmation  |
      | ED-11      | Available           | Add to available count |
      | ED-14      | Needs Cleaning       | Notify housekeeping    |
    Then the system processes all updates simultaneously
    And appropriate notifications are sent to all relevant departments
    And the bed availability dashboard is updated in real-time
    And the total available bed count reflects all changes

  Scenario: Handle urgent bed turnover request
    Given the ED is at 95% capacity
    And there is a trauma patient incoming requiring immediate bed
    And bed "ED-4" patient is ready for discharge
    When I mark the discharge as "Urgent Turnover Required"
    And I request expedited cleaning for bed "ED-4"
    And I submit the urgent status update
    Then the system updates bed status to "Dirty - Urgent"
    And a high-priority notification is sent to housekeeping:
      | Field              | Value                           |
      | Priority Level     | URGENT                         |
      | Room Number        | ED-4                           |
      | Reason            | Incoming trauma patient         |
      | Target Time       | 15 minutes                     |
      | Special Instructions| Expedited cleaning protocol    |
    And the charge nurse is notified of the urgent turnover request
    And a timer is started to track cleaning completion time

  Scenario: Validate bed status change restrictions
    Given bed "ED-9" currently has status "Occupied"
    And a patient "Susan Davis" is actively receiving treatment
    When I attempt to mark the bed as "Available"
    And I submit the invalid status change
    Then the system displays a validation error:
      | Error Type         | Message                                |
      | Invalid Transition | Cannot mark occupied bed as available  |
      | Required Action    | Discharge patient first               |
      | Current Patient    | Susan Davis - Active treatment        |
    And the bed status remains "Occupied"
    And no notifications are sent
    And I am prompted to follow proper discharge workflow

  Scenario: Track bed status history and audit trail
    Given bed "ED-5" has had multiple status changes today
    When I access the bed status history
    Then the system displays the complete audit trail:
      | Time     | Previous Status | New Status      | Changed By      | Reason           |
      | 08:00    | Available      | Occupied        | Nurse Johnson   | Patient admitted |
      | 12:30    | Occupied       | Needs Cleaning  | Nurse Smith     | Patient discharge|
      | 13:00    | Needs Cleaning | Dirty          | System Auto     | Status update    |
      | 13:45    | Dirty          | Available      | Housekeeping    | Cleaning complete|
      | 14:15    | Available      | Occupied       | Nurse Williams  | New patient      |
    And each status change includes timestamp and user identification
    And the audit trail is preserved for compliance reporting
    And I can generate reports on bed utilization patterns

  Scenario: Handle bed status update during system maintenance
    Given the housekeeping notification system is temporarily offline
    And a patient is discharged from bed "ED-16"
    When I mark the bed as "Needs Cleaning"
    And I submit the status update
    Then the system updates the bed status to "Dirty"
    And the system queues the housekeeping notification for later delivery
    And a warning message is displayed: "Housekeeping system offline - notification queued"
    And the bed is still removed from available count
    And when the housekeeping system comes back online, queued notifications are automatically sent
    And a log entry is created documenting the delayed notification
