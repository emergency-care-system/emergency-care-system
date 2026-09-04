# # Discharge Follow-up
#
# **Key Components:**
#
# - Automated follow-up reminder scheduling with tiered reminder systems
#
# - Patient portal integration for comprehensive discharge instruction delivery
#
# - Multi-modal communication respecting patient preferences
#
# - Compliance tracking and outcome monitoring for quality improvement
#
# **Comprehensive Scenarios:**
#
# - Basic follow-up automation - Standard primary care follow-up with automated reminders
#
# - Complex multi-appointment coordination - Multiple specialist and service appointments
#
# - Multimedia patient education - Interactive content delivery through patient portal
#
# - Unestablished primary care handling - Alternative options for patients without PCPs
#
# - Communication preference customization - Personalized messaging based on patient choices
#
# - Pediatric follow-up coordination - Parent-focused communications and child-specific requirements
#
# - Compliance and outcome tracking - Quality metrics and performance monitoring
#
# - Escalation procedures - Handling complications and urgent follow-up needs
#
# **Healthcare-Specific Features:**
#
# - Tiered reminder systems (initial, follow-up, urgent reminders)
#
# - Multi-appointment coordination with priority levels
#
# - Electronic referral generation and specialist communication
#
# - Patient portal integration with multimedia educational content
#
# - Pediatric-specific considerations with parent/guardian coordination
#
# - Communication preference management and tracking
#
# - Compliance monitoring with appointment scheduling and attendance
#
# - Quality metrics tracking (readmission rates, satisfaction scores)
#
# - Escalation protocols for patients with complications
#
# - Provider finding tools for patients without established care
#
# - Interactive educational tools and knowledge verification
#
# - Safety net procedures for high-risk patients
#
# - Integration with school health requirements for pediatric patients
#
# - Financial counseling resources for care access

Feature: Discharge Follow-up
  As a patient care coordinator
  I want automated follow-up scheduling and patient communication after discharge
  So that patients receive proper continuity of care and adhere to treatment plans

  Background:
    Given the ED management system is operational
    And the patient portal is connected and functional
    And the follow-up scheduling module is active
    And automated reminder systems are enabled
    And patient communication preferences are configured

  Scenario: Automatically schedule follow-up reminder for primary care
    Given a patient "Jennifer Martinez" has been discharged from bed "ED-8"
    And the discharge orders include:
      | Follow-up Requirement | Details                                    |
      | Primary Care Visit    | Schedule within 3-5 days                  |
      | Reason for Follow-up  | UTI treatment response, medication review  |
      | Urgency Level         | Routine                                    |
      | Special Instructions  | Bring discharge paperwork and medication list |
    And the patient has a registered primary care physician "Dr. Sarah Wilson"
    When the discharge process is completed at 14:30
    Then the system automatically schedules a follow-up reminder:
      | Reminder Type         | Details                                    |
      | Initial Reminder      | Day 2 after discharge (in 48 hours)      |
      | Follow-up Reminder    | Day 4 after discharge if no appointment   |
      | Final Reminder        | Day 6 after discharge (urgent)           |
      | Reminder Methods      | Text, email, phone call                   |
    And discharge instructions are automatically sent to the patient portal:
      | Portal Section        | Content Delivered                          |
      | Discharge Summary     | Complete treatment summary and diagnosis   |
      | Medication Instructions| Prescription details and dosing schedule |
      | Follow-up Requirements| Primary care appointment needed in 3-5 days|
      | Return Precautions    | When to seek emergency care               |
      | Care Instructions     | Home care guidelines and activity restrictions|
    And the patient receives immediate portal notification:
      | Notification Type     | Content                                    |
      | Portal Alert          | 📋 New discharge instructions available   |
      | Text Message          | "ED discharge complete. Check patient portal for instructions" |
      | Email Notification    | Detailed discharge summary with portal link|

  Scenario: Handle follow-up scheduling with multiple appointment types
    Given a patient "Robert Chen" is discharged with complex follow-up needs
    And the discharge orders specify:
      | Follow-up Type        | Timeframe | Provider Type     | Priority  |
      | Primary Care         | 3 days    | Family Medicine   | High      |
      | Cardiology Consult   | 1 week    | Cardiologist      | Urgent    |
      | Lab Work Follow-up   | 5 days    | Lab/Primary Care  | Routine   |
      | Physical Therapy     | 2 weeks   | PT Specialist     | Routine   |
    When the discharge process is completed
    Then the system creates multiple follow-up reminders:
      | Reminder Schedule     | Appointment Type | Timing             |
      | Day 1 (Tomorrow)     | Primary Care     | Schedule within 2 days |
      | Day 3                | Cardiology       | Schedule urgent consult |
      | Day 4                | Lab Work         | Schedule blood draw    |
      | Day 10               | Physical Therapy | Schedule PT evaluation |
    And the patient portal receives comprehensive follow-up information:
      | Portal Section        | Content                                    |
      | Appointment Dashboard | All required follow-ups with deadlines    |
      | Provider Contacts     | Phone numbers and scheduling information   |
      | Priority Indicators   | Urgent vs routine appointment labeling     |
      | Preparation Instructions| What to bring to each appointment        |
    And automated referrals are generated:
      | Referral Type         | Action Taken                               |
      | Electronic Referral   | Sent to cardiology for urgent consult     |
      | Lab Order            | Standing orders for follow-up labs         |
      | PT Referral          | Physical therapy evaluation requested      |

  Scenario: Send discharge instructions to patient portal with multimedia content
    Given a patient "Maria Santos" was treated for "wound care management"
    And the patient requires detailed home care instructions
    When the discharge process includes educational materials:
      | Education Type        | Content Provided                           |
      | Wound Care Video      | Step-by-step dressing change demonstration |
      | Medication Guide      | Interactive dosing calculator              |
      | Warning Signs Chart   | Visual guide for infection symptoms        |
      | Activity Guidelines   | Illustrated movement restrictions          |
    And the discharge is completed
    Then the patient portal receives multimedia instructions:
      | Portal Content Type   | Educational Material                       |
      | Video Instructions    | Wound care demonstration (3 minutes)      |
      | Interactive Tools     | Medication reminder scheduler              |
      | Visual Guides         | Infection warning signs with photos        |
      | Progress Tracking     | Healing milestone checklist                |
    And the patient receives learning verification:
      | Verification Method   | Requirement                                |
      | Video Completion     | Must watch wound care video fully         |
      | Knowledge Check      | Brief quiz on warning signs               |
      | Acknowledgment       | Confirm understanding of instructions      |
    And completion tracking is recorded for quality assurance

  Scenario: Handle follow-up reminders for patients without primary care physicians
    Given a patient "David Kim" is discharged
    And the patient does not have an established primary care physician
    And follow-up care is required within 5 days
    When the discharge process is completed
    Then the system provides alternative follow-up options:
      | Follow-up Option      | Details Provided                           |
      | Urgent Care Centers   | List of nearby facilities with hours      |
      | Hospital Clinic       | Available appointment slots                |
      | Telehealth Options    | Virtual visit scheduling information       |
      | Community Health Centers| Low-cost provider options               |
    And enhanced reminder scheduling is activated:
      | Reminder Type         | Frequency                                  |
      | Daily Reminders       | For first 3 days after discharge         |
      | Resource Assistance   | Links to find primary care providers      |
      | Financial Counseling  | Information about insurance and payment    |
    And the patient portal includes provider finding tools:
      | Portal Tool           | Functionality                              |
      | Provider Search       | Find doctors accepting new patients        |
      | Insurance Verification| Check coverage for potential providers     |
      | Appointment Booking   | Direct scheduling with available providers |

  Scenario: Customize follow-up based on patient communication preferences
    Given a patient "Lisa Brown" has specified communication preferences:
      | Communication Method  | Preference    | Contact Information        |
      | Text Messages         | Preferred     | 555-123-4567              |
      | Email                 | Secondary     | lisa.brown@email.com      |
      | Phone Calls           | Emergency Only| 555-123-4567              |
      | Portal Notifications  | Enabled       | Username: lbrown123       |
    And the patient is discharged with routine follow-up requirements
    When the discharge process triggers follow-up communications
    Then the system respects patient communication preferences:
      | Communication Type    | Method Used   | Content                    |
      | Initial Instructions  | Text + Portal | Brief summary with portal link |
      | Follow-up Reminders   | Text Message  | Appointment reminders          |
      | Urgent Notifications  | Phone Call    | Critical lab results only      |
      | Educational Content   | Portal Only   | Detailed instructions and videos|
    And communication tracking records patient engagement:
      | Tracking Metric       | Measurement                                |
      | Message Delivery      | Successful text delivery confirmed        |
      | Portal Access         | Login timestamps and content viewed       |
      | Engagement Level      | Time spent reviewing instructions          |

  Scenario: Handle follow-up for pediatric patients with parent/guardian coordination
    Given a pediatric patient "Emma Foster" (age 6) is discharged
    And the parent "Sarah Foster" is the primary contact
    And follow-up includes pediatric-specific requirements:
      | Follow-up Type        | Pediatric Considerations                   |
      | Pediatrician Visit    | Growth and development check               |
      | Vaccination Updates   | Catch-up on missed immunizations          |
      | School Health Forms   | Medical clearance for return to school     |
    When the discharge process is completed
    Then the system creates parent-focused follow-up communications:
      | Communication Target  | Content Type                               |
      | Parent Portal Account | Child's medical summary and instructions   |
      | School Notifications  | Medical excuse and return guidelines       |
      | Pediatrician Alert    | ED visit summary and follow-up needs      |
    And pediatric-specific reminders are scheduled:
      | Reminder Type         | Parent Instructions                        |
      | Medication Reminders  | Weight-based dosing with schedule          |
      | Development Milestones| Age-appropriate recovery expectations      |
      | School Return Criteria| When child can safely return to activities |
    And child safety verification is included:
      | Safety Check          | Parent Confirmation Required               |
      | Home Safety Assessment| Childproofing for medication storage       |
      | Caregiver Instructions| Multiple caregivers receive instructions   |
      | Emergency Contacts    | Updated emergency contact information       |

  Scenario: Track follow-up compliance and patient outcomes
    Given multiple patients have been discharged with follow-up requirements
    When follow-up reminders are sent and appointments are scheduled
    Then the system tracks compliance metrics:
      | Compliance Metric     | Measurement                                |
      | Appointment Scheduling| % of patients who schedule within timeframe|
      | Appointment Attendance| % of scheduled appointments kept           |
      | Portal Engagement     | % of patients accessing discharge instructions|
      | Medication Compliance | % following prescription instructions       |
    And outcome tracking is performed:
      | Outcome Metric        | Tracking Method                            |
      | ED Readmissions       | 72-hour and 30-day return rates          |
      | Complication Rates    | Follow-up visits for related issues       |
      | Patient Satisfaction  | Follow-up surveys about discharge process  |
    And quality improvement reports are generated:
      | Report Type           | Content                                    |
      | Follow-up Effectiveness| Success rates by discharge diagnosis      |
      | Communication Analysis | Best-performing reminder methods           |
      | Provider Performance  | Follow-up compliance by discharging physician|

  Scenario: Handle follow-up complications and escalation procedures
    Given a patient "Michael Davis" was discharged 2 days ago
    And follow-up reminders have been sent
    When the patient contacts the ED with worsening symptoms
    And the patient has not yet scheduled the required follow-up appointment
    Then the system escalates the follow-up process:
      | Escalation Action     | Details                                    |
      | Urgent Scheduling     | Same-day appointment coordination          |
      | Provider Notification | Original discharging physician alerted    |
      | Symptom Assessment    | Nurse triage for immediate vs delayed care |
      | Documentation Update  | Patient contact and status change recorded |
    And enhanced monitoring is activated:
      | Monitoring Type       | Action                                     |
      | Daily Check-ins       | Nurse calls patient for status updates    |
      | Expedited Referrals   | Fast-track specialist appointments        |
      | Safety Net Activation | Ensure patient has immediate care access   |
    And the care team receives comprehensive updates:
      | Team Member           | Information Provided                       |
      | Discharging Physician | Patient contact and current status        |
      | Primary Care Provider | Urgent need for appointment                |
      | Charge Nurse          | Potential readmission risk identified     |
