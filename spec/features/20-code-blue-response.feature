# # Code Blue Response
#
# **Key Components:**
#
# - Instant multi-channel alerting of code team members with precise timing
#
# - Real-time location display across all hospital devices and systems
#
# - Automated resuscitation documentation with timeline tracking and quality metrics
#
# - Equipment coordination with crash cart dispatch and backup preparation
#
# **Comprehensive Scenarios:**
#
# - Basic code blue activation - Standard cardiac arrest response with team notification
#
# - Equipment coordination - Automatic dispatch and tracking of emergency equipment
#
# - Real-time documentation - Live documentation during resuscitation with quality metrics
#
# - Successful resuscitation (ROSC) - Post-resuscitation care coordination and family notification
#
# - Unsuccessful resuscitation - End-of-life care transition and administrative processes
#
# - Family presence management - Visitor coordination and family support during codes
#
# - Quality improvement tracking - Performance metrics and continuous improvement
#
# - False alarm handling - Appropriate response to accidental activations
#
# **Healthcare-Specific Features:**
#
# - ACLS (Advanced Cardiac Life Support) protocol integration
#
# - Real-time CPR quality monitoring with compression depth and rate tracking
#
# - Precise timeline documentation for quality assurance and legal requirements
#
# - Multi-disciplinary team coordination (physicians, nurses, respiratory therapy, pharmacy)
#
# - Equipment tracking and maintenance integration
#
# - Family support services including chaplain and social work
#
# - Post-resuscitation care protocols including ICU transfer coordination
#
# - End-of-life care procedures including organ donation protocols
#
# - Quality improvement metrics tracking response times and protocol adherence
#
# - Integration with hospital communication systems (overhead paging, mobile alerts)
#
# - ROSC (Return of Spontaneous Circulation) management protocols
#
# - Bereavement support and administrative death procedures
#
# - Training and simulation integration for team performance improvement

Feature: Code Blue Response
  As a nurse
  I want to rapidly activate emergency response for cardiac arrest patients
  So that the code team can respond immediately with proper coordination and documentation

  Background:
    Given the ED management system is operational
    And the code blue alert system is active
    And the resuscitation documentation module is enabled
    And all display devices are connected to the alert network
    And the code team roster is current and available

  Scenario: Activate code blue for cardiac arrest in bed 5
    Given a patient "Robert Martinez" is in bed "ED-5"
    And the patient is being monitored for chest pain
    And I am "Nurse Johnson" providing direct patient care
    When the patient suddenly becomes unresponsive and pulseless
    And I immediately press the code blue button at bedside
    Then the system instantly activates the code blue alert:
      | Alert Component       | Activation Details                         |
      | Alert Timestamp       | 14:35:22 - Precise time recorded          |
      | Location              | ED-5 clearly identified                   |
      | Initiating Staff      | Nurse Johnson                             |
      | Patient Identity      | Robert Martinez (if available)            |
      | Alert Type            | Code Blue - Cardiac Arrest                |
    And the code team is immediately alerted through multiple channels:
      | Team Member           | Alert Method          | Expected Response Time |
      | Emergency Physician   | Overhead page, mobile | Immediate             |
      | Cardiologist         | Mobile alert, pager   | Within 2 minutes      |
      | Anesthesiologist     | Overhead page, mobile | Within 3 minutes      |
      | ICU Nurse            | Mobile alert, pager   | Within 2 minutes      |
      | Respiratory Therapist | Overhead page, mobile | Within 2 minutes      |
      | Pharmacist           | Mobile alert          | Within 3 minutes      |
      | Chaplain             | Silent alert          | Within 5 minutes      |
    And patient location is displayed on all devices:
      | Display Location      | Information Shown                          |
      | ED Dashboard          | 🚨 CODE BLUE - BED ED-5 flashing red     |
      | Mobile Devices        | Push notification with location           |
      | Overhead Displays     | "CODE BLUE BED ED-5" prominently shown   |
      | Pager System          | "CODE BLUE ED-5" message                  |
      | Hospital Information  | Alert on all connected terminals          |
    And the resuscitation documentation template opens automatically:
      | Documentation Section | Template Fields                            |
      | Event Details         | Time, location, discoverer, initial rhythm|
      | Timeline Tracker      | Medication times, defibrillation, procedures|
      | Team Members          | Roles and arrival times                    |
      | Vital Signs           | Real-time monitoring integration           |
      | Interventions         | CPR quality, airway management, IV access |

  Scenario: Code blue response with automatic equipment alerts
    Given a code blue has been activated in bed "ED-5"
    When the code blue alert is triggered
    Then emergency equipment alerts are automatically generated:
      | Equipment Type        | Alert Message                              |
      | Crash Cart            | Crash cart dispatch to ED-5               |
      | Defibrillator        | AED/Manual defibrillator to ED-5          |
      | Airway Equipment     | Intubation kit and ventilator to ED-5     |
      | Emergency Medications | Code blue medication box to ED-5          |
      | IV Access Supplies   | Central line kit and fluids to ED-5       |
    And equipment tracking is initiated:
      | Equipment Item        | Status Tracking                            |
      | Crash Cart Location   | GPS tracking to bed ED-5                  |
      | Defibrillator Readiness| Battery level and functionality check    |
      | Medication Expiration | Code blue drugs expiration verification   |
      | Equipment Arrival     | Timestamp when equipment reaches bedside  |
    And backup equipment is automatically prepared:
      | Backup Equipment      | Preparation Action                         |
      | Secondary Crash Cart  | Made ready for potential second code       |
      | Additional Ventilator | Checked and moved closer to ED            |
      | Blood Bank Alert      | Emergency blood products prepared          |
      | OR Notification       | Operating room placed on standby          |

  Scenario: Real-time code blue documentation during resuscitation
    Given a code blue is in progress in bed "ED-5"
    And the resuscitation documentation template is open
    And "Dr. Smith" is the code team leader
    When resuscitation interventions are performed
    Then real-time documentation captures all activities:
      | Intervention Type     | Documentation Fields                       |
      | CPR Administration    | Start time, compression quality, provider  |
      | Medication Given      | Drug name, dose, route, time, provider     |
      | Defibrillation       | Joules delivered, rhythm before/after     |
      | Airway Management    | Type of airway, success, provider         |
      | IV Access            | Location, size, number of attempts        |
    And timeline tracking maintains precise chronology:
      | Timeline Entry        | Automatic Capture                          |
      | Event Start           | 14:35:22 - Code blue activated            |
      | CPR Initiated         | 14:35:45 - CPR started by Nurse Johnson   |
      | Team Leader Arrival   | 14:36:15 - Dr. Smith assumes leadership   |
      | First Medication      | 14:37:30 - Epinephrine 1mg IV push       |
      | Defibrillation       | 14:38:45 - 200J biphasic shock delivered  |
    And quality metrics are tracked in real-time:
      | Quality Metric        | Real-time Monitoring                       |
      | Compression Depth     | CPR feedback device integration           |
      | Compression Rate      | Metronome guidance and measurement        |
      | No-flow Time         | Automatic calculation of interruptions    |
      | Medication Timing     | Alert for time-critical drug intervals   |

  Scenario: Code blue with return of spontaneous circulation (ROSC)
    Given a code blue has been in progress for 8 minutes
    And resuscitation efforts are ongoing with documentation active
    When the patient achieves return of spontaneous circulation (ROSC)
    And "Dr. Smith" confirms pulse and blood pressure of 110/70
    Then the system updates the code status:
      | Status Update         | Documentation Changes                      |
      | ROSC Achievement      | Time: 14:43:15 - ROSC achieved            |
      | Vital Signs          | BP: 110/70, HR: 85, documented           |
      | Rhythm Change        | Normal sinus rhythm confirmed             |
      | Intervention Pause   | CPR discontinued, monitoring intensified  |
    And post-ROSC care protocols are activated:
      | Post-ROSC Protocol    | Automated Alerts                           |
      | ICU Transfer          | ICU bed request and transport coordination |
      | Cardiology Consult    | Urgent cardiology evaluation requested     |
      | Temperature Management| Therapeutic hypothermia consideration      |
      | Neurological Assessment| Baseline neuro checks ordered             |
    And family notification procedures are initiated:
      | Family Communication  | Process                                    |
      | Contact Attempt       | Emergency contact called by social worker  |
      | Status Update         | "Patient being treated, stable condition" |
      | Visitation Arrangement| Family arrival and bedside visit coordination|
      | Chaplain Services     | Spiritual care offered to family          |

  Scenario: Unsuccessful code blue with transition to end-of-life care
    Given a code blue has been in progress for 25 minutes
    And multiple rounds of medications and defibrillation have been attempted
    And no return of spontaneous circulation has been achieved
    When "Dr. Smith" as code team leader determines resuscitation efforts should cease
    And the time of death is called at 15:00:15
    Then the system handles end-of-life documentation:
      | End-of-Life Process   | Documentation Requirements                 |
      | Time of Death         | 15:00:15 - Officially recorded           |
      | Resuscitation Duration| 24 minutes 53 seconds total time         |
      | Interventions Summary | Complete list of all attempted treatments |
      | Team Members Present  | All providers involved in resuscitation   |
    And family notification and support procedures are activated:
      | Family Support        | Coordinated Response                       |
      | Immediate Contact     | Personal notification by physician        |
      | Bereavement Support   | Chaplain and social worker assigned       |
      | Viewing Arrangement   | Private room prepared for family viewing   |
      | Organ Donation        | Coordinator contacted per protocol        |
    And administrative processes are initiated:
      | Administrative Task   | Required Actions                           |
      | Medical Examiner      | Contact if death meets criteria           |
      | Autopsy Consent       | Family discussion and documentation       |
      | Death Certificate     | Physician completion requirements         |
      | Quality Review        | Case review scheduled within 24 hours     |

  Scenario: Code blue during visitor hours with family present
    Given it is 19:30 during evening visitor hours
    And the patient's family members are at bedside when cardiac arrest occurs
    When the code blue is activated
    Then family management protocols are immediately implemented:
      | Family Management     | Immediate Actions                          |
      | Family Escort         | Security escorts family to private area    |
      | Communication         | Social worker provides immediate support   |
      | Information Updates   | Regular updates provided during resuscitation|
      | Chaplain Services     | Spiritual care offered immediately         |
    And visitor area management is coordinated:
      | Visitor Control       | Safety Measures                            |
      | Area Clearance        | Non-family visitors moved from immediate area|
      | Privacy Protection    | Screens and barriers deployed             |
      | Crowd Control         | Security manages visitor flow             |
      | Other Patient Care    | Continued care for nearby patients        |
    And family preference accommodation occurs:
      | Family Preference     | Options Provided                           |
      | Bedside Presence      | Option to remain during resuscitation     |
      | Waiting Area          | Comfortable private space with updates    |
      | Family Spokesperson   | Designated family member for communication |
      | Support Person        | Additional family/friend notification     |

  Scenario: Code blue team performance metrics and quality improvement
    Given a code blue event has been completed
    And all documentation has been finalized
    When the quality review process is initiated
    Then performance metrics are automatically calculated:
      | Performance Metric    | Measurement                                |
      | Response Time         | 1 minute 23 seconds from alert to arrival |
      | No-flow Time          | 15 seconds total interruption time        |
      | First Shock Time      | 3 minutes 45 seconds from arrest          |
      | Medication Timing     | All drugs given within target windows     |
      | Team Coordination     | Communication effectiveness score          |
    And quality improvement data is captured:
      | QI Data Element       | Assessment                                 |
      | Protocol Adherence    | 95% compliance with ACLS guidelines       |
      | Equipment Function    | All equipment functioned properly         |
      | Team Performance      | Effective leadership and role clarity     |
      | Communication Quality | Clear, concise, and timely communication  |
    And improvement opportunities are identified:
      | Improvement Area      | Recommendation                             |
      | Response Time         | Consider additional code cart placement    |
      | Team Training         | Schedule quarterly simulation training     |
      | Equipment Maintenance | Review defibrillator calibration schedule |
      | Documentation         | Streamline real-time entry process        |

  Scenario: Code blue false alarm with appropriate system response
    Given a code blue alert has been activated in bed "ED-5"
    And the code team is responding
    When it is determined that the patient is conscious and stable
    And the alert was triggered accidentally by equipment malfunction
    Then the false alarm protocol is activated:
      | False Alarm Response  | Actions Taken                              |
      | Alert Cancellation    | "Code blue canceled - false alarm" announcement|
      | Team Stand-down       | Code team notified to return to normal duties|
      | Equipment Check       | Investigate and repair malfunctioning equipment|
      | Documentation         | Document false alarm and cause            |
    And system improvements are implemented:
      | Improvement Action    | Preventive Measures                        |
      | Equipment Maintenance | Immediate repair of faulty equipment       |
      | Staff Education       | Review proper code blue activation         |
      | System Calibration    | Adjust sensitivity to prevent false alarms |
      | Audit Trail          | Record incident for system improvement     |
    And normal operations resume with lessons learned integrated into protocols
