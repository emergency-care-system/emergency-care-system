# # Patient Discharge
#
# **Key Components:**
#
# - Comprehensive discharge documentation with automated paperwork generation
#
# - Integrated bed management updating availability and triggering housekeeping
#
# - Automated billing processes with charge capture and insurance handling
#
# - Follow-up care coordination with appointment scheduling and referrals
#
# **Comprehensive Scenarios:**
#
# - Standard discharge workflow - Complete discharge process with instructions and documentation
#
# - Prescription medication handling - Electronic prescribing with patient education
#
# - Specialized follow-up requirements - Multiple appointment types and specialist referrals
#
# - Insurance authorization management - Prior authorization and financial counseling
#
# - Pediatric discharge considerations - Age-specific instructions and parent education
#
# - Shift change coordination - Seamless discharge processing during staff transitions
#
# - Against Medical Advice (AMA) - Legal documentation and risk management
#
# - Batch processing capabilities - Efficient handling of multiple simultaneous discharges
#
# - Quality metrics tracking - Performance monitoring and improvement analytics
#
# **Healthcare-Specific Features:**
#
# - ICD-10 diagnosis coding and CPT procedure coding
#
# - Electronic prescription transmission to pharmacies
#
# - Medication allergy final checking and drug interaction screening
#
# - Weight-based pediatric dosing calculations
#
# - Insurance pre-authorization workflow integration
#
# - Legal AMA documentation with witness requirements
#
# - Specialist referral and appointment coordination
#
# - Patient education materials with condition-specific instructions
#
# - Return precaution guidelines and emergency contact information
#
# - Housekeeping coordination for bed turnover
#
# - Billing charge capture and claims preparation
#
# - Quality metrics tracking (readmission rates, patient satisfaction)
#
# - Follow-up compliance monitoring and reminder systems

Feature: Patient Discharge
  As a physician
  I want to efficiently process patient discharges with automated documentation and workflow
  So that patients receive proper instructions and departmental processes are streamlined

  Background:
    Given the ED management system is operational
    And I am logged in as "Dr. Johnson"
    And the discharge module is active
    And billing integration is enabled
    And bed management system is connected

  Scenario: Complete standard patient discharge with instructions
    Given a patient "Jennifer Martinez" is in bed "ED-8"
    And the patient has completed treatment for "urinary tract infection"
    And all diagnostic tests and treatments are finished
    And the patient is medically stable for discharge
    When I enter discharge orders and instructions:
      | Order Type         | Details                                      |
      | Discharge Status   | Home with medications                        |
      | Primary Diagnosis  | Urinary tract infection (N39.0)             |
      | Medications        | Trimethoprim-Sulfamethoxazole 800mg BID x7d |
      | Follow-up Care     | Primary care physician in 3-5 days          |
      | Activity Level     | Regular activities as tolerated              |
      | Diet              | Regular diet, increase fluid intake          |
      | Return Precautions | Fever >101°F, worsening symptoms, blood in urine |
    And I submit the discharge orders
    Then the system generates comprehensive discharge paperwork:
      | Document Type      | Content Generated                            |
      | Discharge Summary  | Treatment summary, diagnosis, medications    |
      | Medication List    | Prescriptions with dosing instructions       |
      | Follow-up Instructions| PCP appointment scheduling information   |
      | Return Precautions | When to seek emergency care                  |
      | Patient Education  | UTI prevention and care instructions         |
    And the system updates bed availability:
      | Bed Status Change  | Details                                      |
      | Previous Status    | Occupied by Jennifer Martinez                |
      | New Status         | Needs cleaning                               |
      | Availability       | Removed from available bed count             |
      | Housekeeping Alert | Cleaning notification sent                   |
    And billing processes are automatically triggered:
      | Billing Action     | Details                                      |
      | Final Charges      | All services and procedures captured        |
      | Insurance Billing  | Claims prepared for submission               |
      | Patient Statement  | Financial responsibility calculated          |
      | Coding Review      | ICD-10 and CPT codes validated              |

  Scenario: Discharge patient with prescription medications
    Given a patient "Robert Chen" is ready for discharge
    And treatment required multiple medications
    When I enter discharge orders including prescriptions:
      | Medication         | Dose       | Frequency | Duration | Special Instructions |
      | Amoxicillin        | 500mg      | TID       | 10 days  | Take with food       |
      | Ibuprofen          | 600mg      | Q6H PRN   | 5 days   | For pain only        |
      | Omeprazole         | 20mg       | Daily     | 14 days  | Take before breakfast|
    And I include medication education:
      | Education Topic    | Instructions                                 |
      | Drug Interactions  | Avoid alcohol with antibiotics              |
      | Side Effects       | Watch for nausea, diarrhea, allergic reactions |
      | Compliance         | Complete full antibiotic course             |
    And I submit the discharge
    Then the system generates medication-specific documentation:
      | Document           | Content                                      |
      | Prescription List  | All medications with complete instructions   |
      | Drug Information   | Side effects, interactions, precautions     |
      | Pharmacy List      | Nearby pharmacies with hours                 |
      | Medication Calendar| Dosing schedule for patient reference       |
    And prescriptions are electronically transmitted to patient's preferred pharmacy
    And medication allergy checking is performed one final time
    And patient receives medication counseling checklist

  Scenario: Discharge patient requiring follow-up appointments
    Given a patient "Maria Santos" needs specialized follow-up care
    And the treatment was for "complex laceration repair"
    When I enter discharge orders with follow-up requirements:
      | Follow-up Type     | Timeframe | Specialist Required | Special Instructions |
      | Wound Check        | 2-3 days  | Primary care       | Remove sutures      |
      | Specialist Consult | 1 week    | Plastic surgeon    | Scar management     |
      | Lab Follow-up      | 5 days    | Primary care       | Check CBC           |
    And I specify wound care instructions:
      | Care Instructions  | Details                                      |
      | Dressing Changes   | Change daily, keep dry for 48 hours         |
      | Cleaning Protocol  | Gentle soap and water after 48 hours        |
      | Activity Restrictions| No heavy lifting >10 lbs for 2 weeks      |
      | Signs of Infection | Redness, swelling, pus, fever               |
    Then the system schedules and documents follow-up care:
      | Scheduling Action  | Details                                      |
      | Appointment Booking| Attempts to schedule with preferred providers|
      | Referral Generation| Electronic referrals to specialists         |
      | Reminder Setup     | Patient reminders for appointments          |
    And comprehensive wound care instructions are provided
    And follow-up appointment confirmations are sent to patient
    And referring physician receives notification of specialist referral

  Scenario: Handle discharge with insurance authorization requirements
    Given a patient "David Kim" requires expensive follow-up imaging
    And the patient's insurance requires prior authorization
    When I enter discharge orders including:
      | Order Type         | Details                                      |
      | Imaging Study      | MRI lumbar spine within 2 weeks             |
      | Estimated Cost     | $2,400                                       |
      | Medical Necessity  | Rule out disc herniation                     |
    And I submit the discharge orders
    Then the system handles insurance requirements:
      | Insurance Process  | Action Taken                                 |
      | Authorization Check| Prior auth required for MRI                 |
      | Documentation Prep | Clinical justification prepared              |
      | Patient Notification| Informed of authorization process           |
      | Alternative Options| Suggest urgent care MRI if auth denied      |
    And the patient receives information about:
      | Information Type   | Content                                      |
      | Authorization Process| Timeline and requirements explained        |
      | Financial Options  | Self-pay rates and payment plans            |
      | Alternative Providers| Facilities that may not require pre-auth   |
    And insurance pre-authorization request is automatically submitted

  Scenario: Discharge pediatric patient with parent/guardian instructions
    Given a pediatric patient "Emma Foster" (age 6) is ready for discharge
    And the parent "Sarah Foster" is present
    And treatment was for "febrile seizure"
    When I enter pediatric discharge orders:
      | Pediatric Considerations| Details                                |
      | Weight-based Medications| Acetaminophen 10mg/kg Q6H PRN fever   |
      | Parent Education        | Fever management, seizure precautions  |
      | Activity Restrictions   | No swimming for 24 hours              |
      | School Return           | May return tomorrow if fever-free      |
    And I provide seizure-specific education:
      | Education Topic    | Parent Instructions                          |
      | Seizure Precautions| Keep child safe during future episodes      |
      | When to Call 911   | Seizure >5 minutes, difficulty breathing    |
      | Temperature Control| Aggressive fever reduction strategies        |
    Then the system generates pediatric-specific discharge materials:
      | Document Type      | Pediatric Content                            |
      | Parent Instructions| Age-appropriate medication dosing            |
      | Emergency Signs    | When to bring child back to ED              |
      | School Note        | Medical excuse and return instructions       |
      | Developmental Info | Normal vs concerning behaviors post-seizure  |
    And parent acknowledgment is electronically captured
    And pediatric follow-up with primary care pediatrician is scheduled
    And school nurse receives medical summary if parent consents

  Scenario: Handle discharge during shift change
    Given a patient "Lisa Brown" is ready for discharge at 18:45
    And shift change occurs at 19:00
    And "Dr. Day" (day shift) is discharging the patient
    And "Dr. Night" (evening shift) is incoming
    When "Dr. Day" enters the discharge orders
    And the discharge process extends past shift change
    Then the system manages the transition seamlessly:
      | Transition Management| Action                                     |
      | Discharge Ownership  | Dr. Day completes discharge process        |
      | Documentation        | All discharge notes under Dr. Day's name  |
      | Follow-up Responsibility| Any issues route to Dr. Night           |
      | Billing Attribution | Dr. Day receives credit for discharge     |
    And both physicians receive handoff notification:
      | Physician | Notification Content                          |
      | Dr. Day   | Discharge completed for Lisa Brown            |
      | Dr. Night | Lisa Brown discharged - available for questions|
    And the bed becomes available for evening shift patient flow

  Scenario: Discharge patient against medical advice (AMA)
    Given a patient "Michael Davis" wants to leave against medical advice
    And the patient has been informed of risks
    And the patient has decision-making capacity
    When I process an AMA discharge:
      | AMA Documentation  | Requirements                                 |
      | Risk Explanation   | Documented that risks were explained        |
      | Patient Understanding| Patient verbalized understanding of risks |
      | Capacity Assessment| Patient has decision-making capacity       |
      | Witness Required   | Nurse witness to AMA conversation          |
    And I enter minimal safe discharge instructions:
      | Safety Instructions| Details                                      |
      | Return Immediately | If symptoms worsen or new symptoms develop   |
      | Follow-up Care     | Strong recommendation for PCP visit         |
      | Medication Safety  | Critical medications must be continued       |
    Then the system generates AMA-specific documentation:
      | AMA Document       | Content                                      |
      | AMA Form          | Legal documentation of patient choice        |
      | Risk Documentation| Medical risks of leaving explained          |
      | Witness Signatures| Patient, physician, and nurse signatures    |
      | Limited Liability  | Hospital liability limitations documented    |
    And billing processes reflect AMA status
    And legal risk management is notified of AMA discharge
    And patient still receives basic safety instructions

  Scenario: Batch discharge processing during high volume
    Given multiple patients are ready for simultaneous discharge:
      | Patient Name    | Bed    | Diagnosis        | Discharge Type     |
      | Patient A       | ED-3   | Minor injury     | Home              |
      | Patient B       | ED-7   | Gastroenteritis  | Home with meds    |
      | Patient C       | ED-11  | Anxiety          | Home with referral |
    When I process multiple discharges efficiently
    Then the system handles batch processing:
      | Batch Feature      | Functionality                                |
      | Template Usage     | Common discharge templates applied           |
      | Automated Documentation| Standard instructions auto-populated    |
      | Concurrent Processing| Multiple discharges processed simultaneously|
    And all bed updates occur simultaneously:
      | Bed Management     | Action                                       |
      | Status Updates     | All beds marked "needs cleaning"            |
      | Housekeeping Batch | Single notification for multiple rooms      |
      | Availability Count | Bed count updated after all discharges     |
    And billing processes are optimized for batch handling

  Scenario: Track discharge metrics and quality indicators
    Given patient discharges are being processed
    When discharge orders are completed
    Then the system tracks key performance indicators:
      | Metric Type        | Measurement                                  |
      | Discharge Time     | Order entry to patient departure            |
      | Readmission Rate   | 72-hour return rate tracking               |
      | Instruction Quality| Patient understanding verification          |
      | Follow-up Compliance| Scheduled appointment attendance           |
    And generates quality reports:
      | Report Type        | Content                                      |
      | Provider Performance| Discharge efficiency by physician          |
      | Patient Satisfaction| Discharge process satisfaction scores      |
      | Readmission Analysis| Patterns in early returns                  |
    And identifies improvement opportunities:
      | Improvement Area   | Recommendation                               |
      | Process Efficiency | Streamline documentation workflows          |
      | Patient Education  | Enhance instruction clarity                  |
      | Follow-up Coordination| Improve appointment scheduling system     |

