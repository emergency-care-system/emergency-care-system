# # Mass Casualty Activation
#
# **Key Components:**
#
# - Rapid protocol activation switching the ED to surge capacity mode
#
# - Automated staff notification alerting additional personnel across all departments
#
# - Streamlined registration workflows for rapid patient processing during emergencies
#
# - Resource optimization expanding capacity and reallocating equipment
#
# **Comprehensive Scenarios:**
#
# - Basic MCI activation - Fundamental protocol activation for multi-vehicle accident
#
# - Surge capacity configuration - Bed expansion and resource reallocation
#
# - Rapid registration and triage - Streamlined patient processing with electronic tagging
#
# - External coordination - Inter-facility communication and regional emergency management
#
# - Family reunification - Information center and family communication systems
#
# - Transition to normal operations - Deactivation process and quality improvement
#
# - Shift change management - Handling MCI during staff transitions
#
# - Drill testing - System readiness verification through regular exercises
#
# **Healthcare-Specific Features:**
#
# - START triage protocol implementation with color-coded electronic tags
#
# - Surge capacity activation with hallway beds, converted spaces, and ambulatory areas
#
# - Mass transfusion protocol and blood bank coordination
#
# - Inter-facility patient distribution and transfer coordination
#
# - Family reunification center with HIPAA-compliant information sharing
#
# - Incident command system integration with hospital emergency operations
#
# - Resource sharing protocols with regional healthcare facilities
#
# - Electronic patient tracking with sequential MCI identification numbers
#
# - Emergency contact notification systems with automated family alerts
#
# - Quality improvement processes including after-action reviews
#
# - Drill and exercise capabilities for readiness testing
#
# - Staff role assignments and emergency coverage protocols
#
# - Regional trauma center coordination for specialty care transfers

Feature: Mass Casualty Activation
  As a charge nurse
  I want to rapidly activate emergency protocols during mass casualty incidents
  So that the ED can efficiently manage multiple critically injured patients and maximize survival outcomes

  Background:
    Given the ED management system is operational
    And I am logged in as "Charge Nurse Williams"
    And the mass casualty incident (MCI) module is available
    And emergency contact systems are enabled
    And surge capacity protocols are configured

  Scenario: Activate mass casualty protocol for multi-vehicle accident
    Given it is 16:30 on a Friday afternoon
    And normal ED operations are in progress with 12 patients currently in the department
    And EMS reports a multi-vehicle accident with 8+ casualties en route
    When I receive notification of the mass casualty incident:
      | Incident Details      | Information Provided                       |
      | Incident Type         | Multi-vehicle collision on Interstate 70  |
      | Estimated Casualties  | 8-12 patients                             |
      | Severity Assessment   | Multiple critical, several urgent          |
      | ETA                   | First ambulance in 10 minutes             |
      | Resources Needed      | Trauma bays, surgery, blood bank          |
    And I activate the disaster protocol in the system
    Then the system immediately switches to surge capacity mode:
      | System Change         | Activation Details                         |
      | Mode Indicator        | "MASS CASUALTY ACTIVE" banner displayed   |
      | Interface Switch      | MCI-specific workflows activated           |
      | Normal Operations     | Routine tasks suspended/deprioritized      |
      | Resource Allocation   | Emergency resource management enabled      |
      | Communication Mode    | Critical alerts and notifications active   |
    And additional staff are automatically alerted:
      | Staff Category        | Alert Method          | Response Expected      |
      | Off-duty Physicians   | SMS, Phone call       | Report within 30 min   |
      | Off-duty Nurses       | SMS, Phone call       | Report within 45 min   |
      | Surgical Team         | Overhead page, SMS    | Report immediately     |
      | Lab/Radiology         | System alert, Phone   | Prepare for surge      |
      | Administration        | Phone call, SMS       | Incident command setup |
      | Security              | Radio, Overhead page  | Report immediately     |
    And rapid registration workflows are created:
      | Registration Feature  | MCI Configuration                          |
      | Patient Identification| Sequential numbering: MCI-001, MCI-002   |
      | Triage Tags           | Color-coded electronic tags               |
      | Minimal Data Entry    | Name, age, chief complaint only           |
      | Family Notification   | Automated family alert system            |
      | Tracking Board        | Real-time patient status dashboard        |

  Scenario: Configure surge capacity with bed and resource expansion
    Given the mass casualty protocol has been activated
    And normal bed capacity is 20 beds
    When the system enters surge capacity mode
    Then additional treatment areas are activated:
      | Surge Area            | Capacity Addition     | Equipment Status       |
      | Hallway Beds          | +6 treatment spaces   | Mobile equipment deployed|
      | Procedure Rooms       | +3 converted spaces   | Emergency supplies added|
      | Observation Area      | +8 holding spaces     | Monitoring equipment   |
      | Waiting Room Triage   | +4 assessment areas   | Portable triage stations|
      | Ambulatory Care       | +10 walking wounded   | Basic treatment supplies|
    And resource allocation is optimized for mass casualty:
      | Resource Type         | Normal Allocation     | Surge Allocation       |
      | Trauma Bays           | 2 available          | All 4 activated        |
      | Operating Rooms       | 1 ED-accessible      | 3 rooms on standby     |
      | Ventilators           | 3 available          | 8 total (5 from ICU)   |
      | Blood Products        | Standard inventory    | Massive transfusion protocol|
      | Medication Carts      | 2 active             | 5 carts deployed       |
    And staffing ratios are adjusted for emergency operations:
      | Staff Type            | Normal Ratio          | Surge Ratio            |
      | Physicians            | 1:8 patients         | 1:12 patients          |
      | Nurses                | 1:4 patients         | 1:6 patients           |
      | Support Staff         | Standard coverage     | Double coverage        |

  Scenario: Implement rapid patient registration and triage workflow
    Given mass casualty mode is active
    And the first ambulance arrives with 3 critical patients
    When EMS brings patients to the ED
    Then the rapid registration workflow is initiated:
      | Registration Step     | MCI Process                                |
      | Patient Arrival       | Immediate tag assignment: MCI-001, 002, 003|
      | Triage Assessment     | START triage protocol applied              |
      | Electronic Tagging    | Color-coded digital tags assigned          |
      | Minimal Documentation | Name, estimated age, mechanism of injury   |
      | Bed Assignment        | Automatic assignment by acuity             |
    And electronic triage tags are applied with color coding:
      | Triage Color          | Priority Level        | System Assignment      |
      | Red (Immediate)       | Life-threatening      | MCI-001 → Trauma Bay 1 |
      | Yellow (Delayed)      | Urgent but stable     | MCI-002 → Surge Bed 3  |
      | Green (Minor)         | Walking wounded       | MCI-003 → Ambulatory Area|
      | Black (Deceased)      | No vital signs        | Morgue coordination    |
    And family notification systems are activated:
      | Notification Process  | Automated Actions                          |
      | Emergency Contacts    | Auto-dial from patient personal effects    |
      | Public Information    | Hospital hotline number broadcasted       |
      | Media Coordination    | Incident command liaison activated        |
      | Social Services       | Family support team mobilized             |

  Scenario: Coordinate with external emergency services and hospitals
    Given a major mass casualty incident is in progress
    And local EMS is overwhelmed with the response
    When the system activates external coordination protocols
    Then inter-facility communication is established:
      | Communication Channel | Purpose                                    |
      | EMS Command Center    | Patient distribution and transport updates |
      | Other Area Hospitals  | Bed availability and transfer coordination |
      | Air Medical Services  | Helicopter transport for critical patients |
      | Regional Trauma Centers| Specialty care transfer arrangements     |
    And patient distribution management is activated:
      | Distribution Strategy | Implementation                             |
      | Load Balancing        | Distribute patients across regional facilities|
      | Specialty Matching    | Route patients to appropriate specialty care|
      | Capacity Monitoring   | Real-time bed availability tracking        |
      | Transport Coordination| Ambulance and helicopter scheduling       |
    And regional emergency management integration occurs:
      | Integration Element   | Coordination Details                       |
      | Incident Command      | Hospital EOC links with regional ICS      |
      | Resource Sharing      | Equipment and staff sharing protocols     |
      | Information Sharing   | Patient status updates to command center  |
      | Media Management      | Coordinated public information releases   |

  Scenario: Manage family reunification and information center
    Given multiple patients from the mass casualty incident are being treated
    And families are arriving seeking information about their loved ones
    When the family information center is activated
    Then patient tracking and family communication systems are deployed:
      | Family Support System | Functionality                              |
      | Information Hotline   | Dedicated phone line with trained staff   |
      | Family Reunification  | Secure area for family waiting and updates|
      | Patient Tracking      | Real-time status board for authorized viewers|
      | Privacy Protection    | HIPAA-compliant information sharing       |
    And automated family notification processes are initiated:
      | Notification Method   | Information Provided                       |
      | SMS Updates          | "Your family member is being treated safely"|
      | Phone Calls          | Personal calls for critical status changes |
      | Information Boards   | General incident updates (no patient names)|
      | Social Workers       | One-on-one family support and counseling  |
    And patient status tracking is maintained:
      | Tracking Element      | Information Available                      |
      | Current Location      | Treatment area, OR, transferred, etc.     |
      | Medical Status        | Stable, critical, treated and released    |
      | Next of Kin Contact   | Verification and notification status       |
      | Discharge Planning    | Expected timeline and care needs          |

  Scenario: Transition from mass casualty mode back to normal operations
    Given the mass casualty incident has been resolved
    And all patients are stabilized or transferred
    And no additional casualties are expected
    When I initiate the transition back to normal operations
    Then the system manages the deactivation process:
      | Deactivation Step     | Process Details                            |
      | Incident Assessment   | Review of patient outcomes and resources used|
      | Staff Debriefing      | Immediate hot wash and formal debriefing  |
      | Resource Restoration  | Return equipment and supplies to normal areas|
      | Documentation         | Complete incident documentation and reports|
    And normal ED operations are gradually restored:
      | Restoration Phase     | Timeline              | Actions Required       |
      | Immediate (0-30 min)  | Secure scene         | Ensure all patients stable|
      | Short-term (30-60 min)| Resource cleanup     | Return surge equipment |
      | Medium-term (1-4 hrs) | Staff rotation       | Relieve emergency staff|
      | Long-term (4-24 hrs)  | Full restoration     | Return to normal staffing|
    And quality improvement activities are initiated:
      | QI Activity           | Purpose                                    |
      | After Action Review   | Identify strengths and improvement areas   |
      | Performance Metrics   | Analyze response times and patient outcomes|
      | Protocol Updates      | Revise procedures based on lessons learned |
      | Training Needs        | Identify staff training and education needs|

  Scenario: Handle mass casualty incident during shift change
    Given it is 19:00 during evening shift change
    And day shift staff are preparing to leave
    And evening shift staff are assuming duties
    When a mass casualty incident is declared
    Then the system manages staffing during the transition:
      | Staffing Strategy     | Implementation                             |
      | Shift Hold           | Day shift staff remain for incident response|
      | Double Coverage      | Both shifts work together during surge     |
      | Incident Command     | Clear leadership chain established         |
      | Communication        | All staff briefed on roles and responsibilities|
    And transition protocols are modified for the emergency:
      | Modified Protocol     | Emergency Adjustment                       |
      | Handoff Procedures    | Suspended until incident resolution        |
      | Staffing Ratios       | Enhanced coverage with both shifts        |
      | Leadership Structure  | Incident commander takes operational control|
      | Documentation        | Emergency documentation procedures active   |

  Scenario: Test mass casualty system readiness through drill
    Given it is a scheduled quarterly mass casualty drill
    And the drill scenario involves a simulated building collapse with 15 casualties
    When the drill coordinator activates the test mass casualty protocol
    Then the system activates in drill mode:
      | Drill Feature         | Test Implementation                        |
      | Test Mode Indicator   | "DRILL - NOT REAL EMERGENCY" displayed   |
      | Simulated Patients    | Test patient records created              |
      | Staff Participation   | All roles and responsibilities tested     |
      | Resource Tracking     | Equipment and supplies tracked but not used|
    And drill performance metrics are captured:
      | Performance Metric    | Measurement                                |
      | Activation Time       | Time from alert to full surge capacity    |
      | Staff Response Time   | Time for staff to report and assume roles |
      | Communication Speed   | Time for all notifications to be completed|
      | Resource Deployment   | Time to set up surge areas and equipment  |
    And drill evaluation and improvement planning occurs:
      | Evaluation Component  | Assessment Focus                           |
      | Protocol Effectiveness| How well procedures worked in practice     |
      | Staff Preparedness    | Knowledge and skill gaps identified       |
      | System Performance    | Technology and workflow efficiency        |
      | Improvement Plans     | Action items for enhancing response capabilities|
