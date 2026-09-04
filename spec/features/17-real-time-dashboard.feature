# # Real-time Dashboard
#
# **Key Components:**
#
# - Real-time data display with 30-second automatic refresh cycles
#
# - Comprehensive departmental metrics including census, wait times, bed availability, and staff assignments
#
# - Visual indicators and alerts for critical conditions and capacity management
#
# - Customizable views based on user roles and preferences
#
# **Comprehensive Scenarios:**
#
# - Main dashboard overview - Complete departmental status display with all key metrics
#
# - Real-time updates - Automatic 30-second refresh with visual change indicators
#
# - Priority alerts and notifications - Critical condition monitoring with escalation levels
#
# - Patient flow metrics - Detailed performance tracking with trends and predictions
#
# - Role-based customization - Personalized dashboard views for charge nurse functions
#
# - Crisis mode indicators - Emergency protocol activation and specialized displays
#
# - Mobile device integration - Optimized mobile access with touch interface
#
# - Historical comparison - Trend analysis and performance benchmarking
#
# **Healthcare-Specific Features:**
#
# - Patient census tracking with capacity utilization percentages
#
# - ESI-based patient prioritization and queue management
#
# - Bed status monitoring with housekeeping coordination
#
# - Provider workload balancing and availability tracking
#
# - Wait time monitoring by care phase (triage, provider, discharge)
#
# - Length of stay tracking against target benchmarks
#
# - Crisis mode protocols for mass casualty and emergency events
#
# - Quality metrics integration (patient satisfaction, safety indicators)
#
# - Predictive analytics for capacity planning and surge preparation
#
# - Industry benchmark comparisons and performance standards
#
# - Multi-level alert systems with acknowledgment requirements
#
# - Real-time synchronization across multiple viewing platforms

Feature: Real-time Dashboard
  As a charge nurse
  I want to monitor real-time department status through a comprehensive dashboard
  So that I can make informed decisions about patient flow, staffing, and resource allocation

  Background:
    Given the ED management system is operational
    And I am logged in as "Charge Nurse Williams"
    And the real-time dashboard module is active
    And automatic data refresh is enabled at 30-second intervals
    And all data sources are connected and synchronized

  Scenario: Display comprehensive department status on main dashboard
    Given it is 14:30 on a busy Tuesday afternoon
    And the ED has the following current status:
      | Metric Type           | Current Value  | Capacity/Target |
      | Total Patient Census  | 18 patients    | 20 bed capacity |
      | Patients Waiting      | 12 patients    | Target <8       |
      | Occupied Beds         | 16 beds        | 20 total beds   |
      | Available Beds        | 2 beds         | 4 beds clean    |
      | Beds Needing Cleaning | 2 beds         | Standard process|
    When I access the main dashboard
    Then the system displays the current patient census:
      | Census Information    | Display Details                            |
      | Total Patients        | 18 (90% capacity) - Yellow indicator      |
      | Admitted Patients     | 14 currently in beds                       |
      | Waiting Patients      | 12 in triage queue - Red alert            |
      | Discharged Today      | 23 patients processed                      |
    And the system shows average wait times:
      | Wait Time Category    | Current Average | Target Time    | Status    |
      | Triage to Bed         | 45 minutes     | <30 minutes    | Over target|
      | Bed to Provider       | 25 minutes     | <15 minutes    | Over target|
      | Provider to Discharge | 120 minutes    | <90 minutes    | Over target|
      | Total ED Length of Stay| 190 minutes   | <240 minutes   | On target |
    And bed availability is displayed with visual indicators:
      | Bed Status            | Count | Visual Indicator                           |
      | Available Clean       | 2     | 🟢 Green beds ready for patients          |
      | Needs Cleaning        | 2     | 🟡 Yellow beds awaiting housekeeping      |
      | Occupied              | 16    | 🔴 Red beds with current patients          |
      | Out of Service        | 0     | ⚫ Black beds unavailable                 |
    And staff assignments are shown in real-time:
      | Staff Role            | Current Assignment | Status/Location           |
      | Dr. Smith             | 3 patients         | Available for new patient |
      | Dr. Johnson           | 4 patients         | At capacity               |
      | Nurse Martinez        | 5 patients         | High workload             |
      | Nurse Chen            | 4 patients         | Normal workload           |

  Scenario: Monitor real-time updates with 30-second refresh cycle
    Given the dashboard is displaying current data at 15:00:00
    And automatic refresh is set to 30-second intervals
    When 30 seconds elapse and new data becomes available:
      | Data Change           | Previous Value | New Value      | Change Type |
      | Patient Census        | 18 patients    | 19 patients    | Increase    |
      | Available Beds        | 2 beds         | 1 bed          | Decrease    |
      | Average Triage Wait   | 45 minutes     | 50 minutes     | Increase    |
      | Dr. Smith Availability| Available      | Busy           | Status change|
    Then the dashboard automatically updates at 15:00:30:
      | Updated Metric        | New Display                                |
      | Patient Count         | 19 (95% capacity) - Red indicator         |
      | Bed Availability      | 1 available - Critical level alert        |
      | Wait Time Alert       | Triage wait exceeds 45 min threshold      |
      | Staff Status          | Dr. Smith now unavailable                 |
    And visual indicators reflect the changes:
      | Visual Update         | Change Description                         |
      | Capacity Indicator    | Yellow → Red (approaching full capacity)   |
      | Bed Status Alert      | New warning for low bed availability       |
      | Provider Icon         | Dr. Smith icon changes to "busy" status   |
    And the last update timestamp shows "Updated: 15:00:30"
    And critical alerts are highlighted with flashing indicators

  Scenario: Display priority alerts and notifications on dashboard
    Given the dashboard is actively monitoring department status
    When critical conditions occur requiring immediate attention:
      | Alert Type            | Trigger Condition                          |
      | Capacity Alert        | >95% bed occupancy reached                 |
      | Wait Time Alert       | Triage wait time >60 minutes              |
      | Critical Patient      | ESI Level 1 patient in queue              |
      | Staffing Alert        | Provider-to-patient ratio exceeded        |
    Then the dashboard displays prominent alert notifications:
      | Alert Priority        | Visual Display                             |
      | CRITICAL              | 🚨 Red flashing banner at top of screen   |
      | HIGH                  | 🟠 Orange banner with urgent icon         |
      | MEDIUM                | 🟡 Yellow notification strip              |
      | INFO                  | 🔵 Blue informational indicator           |
    And specific alert details are shown:
      | Alert Type            | Alert Message                              |
      | Capacity Critical     | "ED at 95% capacity - Consider diversion" |
      | Wait Time Excessive   | "Triage wait: 65 min - Expedite process"  |
      | ESI 1 Patient         | "Critical patient waiting - Bed needed"   |
      | Staffing Concern      | "Provider ratio 1:5 - Additional coverage needed"|
    And alert acknowledgment is required for critical notifications
    And alert history is maintained for trend analysis

  Scenario: Monitor specific patient flow metrics and trends
    Given I need to track detailed patient flow performance
    When I access the detailed metrics section of the dashboard
    Then the system displays comprehensive flow metrics:
      | Flow Metric           | Current Value | 4-Hour Trend | Daily Target  |
      | Patients per Hour     | 3.2 arrivals  | ↗ Increasing | 2.8 target   |
      | Discharge Rate        | 2.1 per hour  | ↘ Decreasing | 2.5 target   |
      | Bed Turnover Time     | 35 minutes    | ↗ Increasing | 25 min target|
      | Left Without Being Seen| 2 patients   | → Stable     | <1 target    |
    And trending graphs show patterns over time:
      | Graph Type            | Time Period | Display Information                |
      | Census Trend          | 24 hours    | Patient volume by hour             |
      | Wait Time Trend       | 12 hours    | Average wait times by ESI level    |
      | Capacity Utilization  | 7 days      | Bed occupancy percentage over week |
      | Provider Productivity | Shift       | Patients seen per provider         |
    And predictive indicators show projected status:
      | Prediction Type       | Forecast                                   |
      | Peak Time Prediction  | Expected surge at 18:00-20:00            |
      | Capacity Projection   | Will reach 100% capacity in 90 minutes   |
      | Staffing Needs        | Additional provider needed by 16:00       |

  Scenario: Customize dashboard view based on role and preferences
    Given I have charge nurse privileges and preferences set
    When I access my personalized dashboard view
    Then the system displays role-specific information:
      | Dashboard Section     | Charge Nurse Focus                         |
      | Resource Management   | Bed status, staffing levels, equipment    |
      | Quality Metrics       | Wait times, satisfaction scores, safety   |
      | Operational Alerts    | Capacity issues, workflow bottlenecks     |
      | Staff Coordination    | Break schedules, assignments, coverage    |
    And customizable widgets show preferred metrics:
      | Widget Type           | Configuration                              |
      | Bed Management Grid   | Color-coded bed status with room numbers  |
      | Provider Status Board | Real-time availability and patient load   |
      | Queue Management      | ESI-sorted patient list with wait times   |
      | Performance KPIs      | Key metrics with targets and trends       |
    And I can modify the layout and priority of information displayed
    And the system saves my preferences for future sessions

  Scenario: Display emergency and crisis mode indicators
    Given the ED is operating under normal conditions
    When emergency conditions trigger crisis mode:
      | Crisis Trigger        | Condition                                  |
      | Mass Casualty Event   | 5+ trauma patients arriving               |
      | Infectious Disease    | Isolation protocols activated             |
      | System Failure        | Critical IT systems offline               |
      | Severe Weather        | Emergency preparedness activated          |
    Then the dashboard displays crisis mode indicators:
      | Crisis Display        | Visual Appearance                          |
      | Mode Banner           | Red "CRISIS MODE ACTIVE" across top       |
      | Protocol Status       | Active emergency protocols listed          |
      | Resource Allocation   | Special staffing and bed assignments      |
      | Communication Center  | Emergency contact information prominent    |
    And specialized crisis metrics are shown:
      | Crisis Metric         | Information Displayed                      |
      | Response Teams        | Available emergency response personnel     |
      | Special Equipment     | Crisis supplies and equipment status       |
      | External Coordination | Communication with EMS, other hospitals   |
      | Surge Capacity        | Additional beds and overflow areas         |
    And normal operations metrics are supplemented with crisis-specific data

  Scenario: Integrate with mobile devices for dashboard access
    Given I need to monitor the dashboard while mobile in the department
    When I access the dashboard on my mobile device
    Then the system provides a mobile-optimized view:
      | Mobile Feature        | Functionality                              |
      | Summary Cards         | Key metrics in swipeable card format      |
      | Alert Notifications   | Push notifications for critical alerts    |
      | Quick Actions         | Rapid access to common charge nurse tasks |
      | Touch Interface       | Optimized for touch navigation             |
    And essential information is prioritized for small screen:
      | Priority Display      | Mobile Layout                              |
      | Critical Alerts       | Top of screen with prominent notification  |
      | Bed Status Summary    | Visual grid with color-coded indicators   |
      | Staff Availability    | Provider status with quick contact options|
      | Key Metrics           | Current census, wait times, alerts        |
    And synchronization occurs in real-time between desktop and mobile views
    And offline capability maintains last-known status when connectivity is lost

  Scenario: Historical data comparison and trend analysis
    Given I want to compare current performance to historical patterns
    When I access the trend analysis section of the dashboard
    Then the system displays comparative data:
      | Comparison Type       | Time Periods                               |
      | Same Day Last Week    | Tuesday to Tuesday comparison              |
      | Monthly Average       | Current day vs monthly average             |
      | Seasonal Patterns     | Year-over-year seasonal comparison         |
      | Shift Comparisons     | Day vs evening vs night shift metrics     |
    And performance benchmarks are shown:
      | Benchmark Type        | Reference Standard                         |
      | Internal Targets      | Hospital-specific performance goals        |
      | Industry Standards    | National ED performance benchmarks        |
      | Peer Comparison       | Similar-sized ED performance data          |
    And variance analysis highlights significant deviations:
      | Variance Alert        | Threshold                                  |
      | Significant Increase  | >20% above normal pattern                  |
      | Significant Decrease  | >15% below expected performance            |
      | Unusual Pattern       | Unexpected trends or anomalies             |
    And recommendations are provided for performance improvement
