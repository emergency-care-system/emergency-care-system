"""Selenium WebDriver + pytest test for spec/features/16-discharge-follow-up.feature
(equivalent to tests-with-selenium-javascript/16-discharge-follow-up.test.js).

Assumes the app exposes data-testid attributes matching each Gherkin field
label (kebab-cased, see support/fields.py) and the shared data-testid
contract in support/login.py (login-identity, login-submit, app-root).
"""

from support.build_driver import build_driver
from support.fields import get_text, wait_for_test_id
from support.login import login, verify_system_is_operational


class TestDischargeFollowUp:
    @classmethod
    def setup_class(cls):
        cls.driver = build_driver()

    @classmethod
    def teardown_class(cls):
        cls.driver.quit()

    def setup_method(self):
        # Background:
        #   Given the emergency care system is operational
        #   And the patient portal is connected and functional
        #   And the follow-up scheduling module is active
        #   And automated reminder systems are enabled
        #   And patient communication preferences are configured
        verify_system_is_operational(self.driver)
        login(self.driver, "a discharge coordinator")
        # The patient portal connection, follow-up scheduling module,
        # automated reminder systems, and communication preference
        # configuration are assumed to be active backend configuration
        # already in place for this environment.

        discharge_follow_up_nav_link = wait_for_test_id(self.driver, "Nav Discharge Follow-up")
        discharge_follow_up_nav_link.click()
        wait_for_test_id(self.driver, "Discharge Follow-up Panel")

    def test_automatically_schedule_follow_up_reminder_for_primary_care(self):
        # Given a patient "Jennifer Martinez" has been discharged from bed "ED-8"
        # And the discharge orders include:
        #   | Follow-up Requirement | Details                                    |
        #   | Primary Care Visit    | Schedule within 3-5 days                  |
        #   | Reason for Follow-up  | UTI treatment response, medication review  |
        #   | Urgency Level         | Routine                                    |
        #   | Special Instructions  | Bring discharge paperwork and medication list |
        # And the patient has a registered primary care physician "Dr. Sarah Wilson"
        # (assumed pre-seeded test data)
        # When the discharge process is completed at 14:30
        # (assumed to have already occurred / triggered by the system)

        # Then the system automatically schedules a follow-up reminder:
        wait_for_test_id(self.driver, "Initial Reminder")
        follow_up_reminders = [
            {"Type": "Initial Reminder", "Details": "Day 2 after discharge (in 48 hours)"},
            {"Type": "Follow-up Reminder", "Details": "Day 4 after discharge if no appointment"},
            {"Type": "Final Reminder", "Details": "Day 6 after discharge (urgent)"},
            {"Type": "Reminder Methods", "Details": "Text, email, phone call"},
        ]
        for row in follow_up_reminders:
            assert get_text(self.driver, row["Type"]) == row["Details"]

        # And discharge instructions are automatically sent to the patient portal:
        portal_content = [
            {"Section": "Discharge Summary", "Content": "Complete treatment summary and diagnosis"},
            {"Section": "Medication Instructions", "Content": "Prescription details and dosing schedule"},
            {"Section": "Follow-up Requirements", "Content": "Primary care appointment needed in 3-5 days"},
            {"Section": "Return Precautions", "Content": "When to seek emergency care"},
            {"Section": "Care Instructions", "Content": "Home care guidelines and activity restrictions"},
        ]
        for row in portal_content:
            assert get_text(self.driver, row["Section"]) == row["Content"]

        # And the patient receives immediate portal notification:
        portal_notifications = [
            {"Type": "Portal Alert", "Content": "📋 New discharge instructions available"},
            {"Type": "Text Message", "Content": '"ED discharge complete. Check patient portal for instructions"'},
            {"Type": "Email Notification", "Content": "Detailed discharge summary with portal link"},
        ]
        for row in portal_notifications:
            assert get_text(self.driver, row["Type"]) == row["Content"]

    def test_handle_follow_up_scheduling_with_multiple_appointment_types(self):
        # Given a patient "Robert Chen" is discharged with complex follow-up needs
        # And the discharge orders specify:
        #   | Follow-up Type        | Timeframe | Provider Type     | Priority  |
        #   | Primary Care         | 3 days    | Family Medicine   | High      |
        #   | Cardiology Consult   | 1 week    | Cardiologist      | Urgent    |
        #   | Lab Work Follow-up   | 5 days    | Lab/Primary Care  | Routine   |
        #   | Physical Therapy     | 2 weeks   | PT Specialist     | Routine   |
        # (assumed pre-seeded test data)
        # When the discharge process is completed
        # (assumed to have already occurred / triggered by the system)

        # Then the system creates multiple follow-up reminders:
        wait_for_test_id(self.driver, "Primary Care Timing")
        reminder_schedule = [
            {"Type": "Primary Care", "Timing": "Schedule within 2 days"},
            {"Type": "Cardiology", "Timing": "Schedule urgent consult"},
            {"Type": "Lab Work", "Timing": "Schedule blood draw"},
            {"Type": "Physical Therapy", "Timing": "Schedule PT evaluation"},
        ]
        for row in reminder_schedule:
            assert get_text(self.driver, f"{row['Type']} Timing") == row["Timing"]

        # And the patient portal receives comprehensive follow-up information:
        portal_follow_up_info = [
            {"Section": "Appointment Dashboard", "Content": "All required follow-ups with deadlines"},
            {"Section": "Provider Contacts", "Content": "Phone numbers and scheduling information"},
            {"Section": "Priority Indicators", "Content": "Urgent vs routine appointment labeling"},
            {"Section": "Preparation Instructions", "Content": "What to bring to each appointment"},
        ]
        for row in portal_follow_up_info:
            assert get_text(self.driver, row["Section"]) == row["Content"]

        # And automated referrals are generated:
        automated_referrals = [
            {"Type": "Electronic Referral", "Action": "Sent to cardiology for urgent consult"},
            {"Type": "Lab Order", "Action": "Standing orders for follow-up labs"},
            {"Type": "PT Referral", "Action": "Physical therapy evaluation requested"},
        ]
        for row in automated_referrals:
            assert get_text(self.driver, row["Type"]) == row["Action"]

    def test_send_discharge_instructions_to_patient_portal_with_multimedia_content(self):
        # Given a patient "Maria Santos" was treated for "wound care management"
        # And the patient requires detailed home care instructions
        # (assumed pre-seeded test data)
        # When the discharge process includes educational materials:
        #   | Education Type        | Content Provided                           |
        #   | Wound Care Video      | Step-by-step dressing change demonstration |
        #   | Medication Guide      | Interactive dosing calculator              |
        #   | Warning Signs Chart   | Visual guide for infection symptoms        |
        #   | Activity Guidelines   | Illustrated movement restrictions          |
        # And the discharge is completed
        # (assumed to have already occurred / triggered by the system)

        # Then the patient portal receives multimedia instructions:
        wait_for_test_id(self.driver, "Video Instructions")
        multimedia_instructions = [
            {"Type": "Video Instructions", "Material": "Wound care demonstration (3 minutes)"},
            {"Type": "Interactive Tools", "Material": "Medication reminder scheduler"},
            {"Type": "Visual Guides", "Material": "Infection warning signs with photos"},
            {"Type": "Progress Tracking", "Material": "Healing milestone checklist"},
        ]
        for row in multimedia_instructions:
            assert get_text(self.driver, row["Type"]) == row["Material"]

        # And the patient receives learning verification:
        learning_verification = [
            {"Method": "Video Completion", "Requirement": "Must watch wound care video fully"},
            {"Method": "Knowledge Check", "Requirement": "Brief quiz on warning signs"},
            {"Method": "Acknowledgment", "Requirement": "Confirm understanding of instructions"},
        ]
        for row in learning_verification:
            assert get_text(self.driver, row["Method"]) == row["Requirement"]

        # And completion tracking is recorded for quality assurance
        completion_tracking_status = get_text(self.driver, "Completion Tracking Status")
        assert "recorded" in completion_tracking_status.lower()

    def test_handle_follow_up_reminders_for_patients_without_primary_care_physicians(self):
        # Given a patient "David Kim" is discharged
        # And the patient does not have an established primary care physician
        # And follow-up care is required within 5 days
        # (assumed pre-seeded test data)
        # When the discharge process is completed
        # (assumed to have already occurred / triggered by the system)

        # Then the system provides alternative follow-up options:
        wait_for_test_id(self.driver, "Urgent Care Centers")
        follow_up_options = [
            {"Option": "Urgent Care Centers", "Details": "List of nearby facilities with hours"},
            {"Option": "Hospital Clinic", "Details": "Available appointment slots"},
            {"Option": "Telehealth Options", "Details": "Virtual visit scheduling information"},
            {"Option": "Community Health Centers", "Details": "Low-cost provider options"},
        ]
        for row in follow_up_options:
            assert get_text(self.driver, row["Option"]) == row["Details"]

        # And enhanced reminder scheduling is activated:
        enhanced_reminders = [
            {"Type": "Daily Reminders", "Frequency": "For first 3 days after discharge"},
            {"Type": "Resource Assistance", "Frequency": "Links to find primary care providers"},
            {"Type": "Financial Counseling", "Frequency": "Information about insurance and payment"},
        ]
        for row in enhanced_reminders:
            assert get_text(self.driver, row["Type"]) == row["Frequency"]

        # And the patient portal includes provider finding tools:
        provider_finding_tools = [
            {"Tool": "Provider Search", "Functionality": "Find doctors accepting new patients"},
            {"Tool": "Insurance Verification", "Functionality": "Check coverage for potential providers"},
            {"Tool": "Appointment Booking", "Functionality": "Direct scheduling with available providers"},
        ]
        for row in provider_finding_tools:
            assert get_text(self.driver, row["Tool"]) == row["Functionality"]

    def test_customize_follow_up_based_on_patient_communication_preferences(self):
        # Given a patient "Lisa Brown" has specified communication preferences:
        #   | Communication Method  | Preference    | Contact Information        |
        #   | Text Messages         | Preferred     | 555-123-4567              |
        #   | Email                 | Secondary     | lisa.brown@email.com      |
        #   | Phone Calls           | Emergency Only| 555-123-4567              |
        #   | Portal Notifications  | Enabled       | Username: lbrown123       |
        # And the patient is discharged with routine follow-up requirements
        # (assumed pre-seeded test data)
        # When the discharge process triggers follow-up communications
        # (assumed to have already occurred / triggered by the system)

        # Then the system respects patient communication preferences:
        wait_for_test_id(self.driver, "Initial Instructions Method")
        communication_preferences = [
            {"Type": "Initial Instructions", "Method": "Text + Portal", "Content": "Brief summary with portal link"},
            {"Type": "Follow-up Reminders", "Method": "Text Message", "Content": "Appointment reminders"},
            {"Type": "Urgent Notifications", "Method": "Phone Call", "Content": "Critical lab results only"},
            {"Type": "Educational Content", "Method": "Portal Only", "Content": "Detailed instructions and videos"},
        ]
        for row in communication_preferences:
            assert get_text(self.driver, f"{row['Type']} Method") == row["Method"]
            assert get_text(self.driver, f"{row['Type']} Content") == row["Content"]

        # And communication tracking records patient engagement:
        communication_tracking = [
            {"Metric": "Message Delivery", "Measurement": "Successful text delivery confirmed"},
            {"Metric": "Portal Access", "Measurement": "Login timestamps and content viewed"},
            {"Metric": "Engagement Level", "Measurement": "Time spent reviewing instructions"},
        ]
        for row in communication_tracking:
            assert get_text(self.driver, row["Metric"]) == row["Measurement"]

    def test_handle_follow_up_for_pediatric_patients_with_parent_guardian_coordination(self):
        # Given a pediatric patient "Emma Foster" (age 6) is discharged
        # And the parent "Sarah Foster" is the primary contact
        # And follow-up includes pediatric-specific requirements:
        #   | Follow-up Type        | Pediatric Considerations                   |
        #   | Pediatrician Visit    | Growth and development check               |
        #   | Vaccination Updates   | Catch-up on missed immunizations          |
        #   | School Health Forms   | Medical clearance for return to school     |
        # (assumed pre-seeded test data)
        # When the discharge process is completed
        # (assumed to have already occurred / triggered by the system)

        # Then the system creates parent-focused follow-up communications:
        wait_for_test_id(self.driver, "Parent Portal Account")
        parent_communications = [
            {"Target": "Parent Portal Account", "Type": "Child's medical summary and instructions"},
            {"Target": "School Notifications", "Type": "Medical excuse and return guidelines"},
            {"Target": "Pediatrician Alert", "Type": "ED visit summary and follow-up needs"},
        ]
        for row in parent_communications:
            assert get_text(self.driver, row["Target"]) == row["Type"]

        # And pediatric-specific reminders are scheduled:
        pediatric_reminders = [
            {"Type": "Medication Reminders", "Instructions": "Weight-based dosing with schedule"},
            {"Type": "Development Milestones", "Instructions": "Age-appropriate recovery expectations"},
            {"Type": "School Return Criteria", "Instructions": "When child can safely return to activities"},
        ]
        for row in pediatric_reminders:
            assert get_text(self.driver, row["Type"]) == row["Instructions"]

        # And child safety verification is included:
        child_safety_checks = [
            {"Check": "Home Safety Assessment", "Requirement": "Childproofing for medication storage"},
            {"Check": "Caregiver Instructions", "Requirement": "Multiple caregivers receive instructions"},
            {"Check": "Emergency Contacts", "Requirement": "Updated emergency contact information"},
        ]
        for row in child_safety_checks:
            assert get_text(self.driver, row["Check"]) == row["Requirement"]

    def test_track_follow_up_compliance_and_patient_outcomes(self):
        # Given multiple patients have been discharged with follow-up requirements
        # (assumed pre-seeded test data)
        # When follow-up reminders are sent and appointments are scheduled
        # (assumed to have already occurred / triggered by the system)

        # Then the system tracks compliance metrics:
        wait_for_test_id(self.driver, "Appointment Scheduling")
        compliance_metrics = [
            {"Metric": "Appointment Scheduling", "Measurement": "% of patients who schedule within timeframe"},
            {"Metric": "Appointment Attendance", "Measurement": "% of scheduled appointments kept"},
            {"Metric": "Portal Engagement", "Measurement": "% of patients accessing discharge instructions"},
            {"Metric": "Medication Compliance", "Measurement": "% following prescription instructions"},
        ]
        for row in compliance_metrics:
            assert get_text(self.driver, row["Metric"]) == row["Measurement"]

        # And outcome tracking is performed:
        outcome_tracking = [
            {"Metric": "ED Readmissions", "Method": "72-hour and 30-day return rates"},
            {"Metric": "Complication Rates", "Method": "Follow-up visits for related issues"},
            {"Metric": "Patient Satisfaction", "Method": "Follow-up surveys about discharge process"},
        ]
        for row in outcome_tracking:
            assert get_text(self.driver, row["Metric"]) == row["Method"]

        # And quality improvement reports are generated:
        quality_improvement_reports = [
            {"Report": "Follow-up Effectiveness", "Content": "Success rates by discharge diagnosis"},
            {"Report": "Communication Analysis", "Content": "Best-performing reminder methods"},
            {"Report": "Provider Performance", "Content": "Follow-up compliance by discharging physician"},
        ]
        for row in quality_improvement_reports:
            assert get_text(self.driver, row["Report"]) == row["Content"]

    def test_handle_follow_up_complications_and_escalation_procedures(self):
        # Given a patient "Michael Davis" was discharged 2 days ago
        # And follow-up reminders have been sent
        # (assumed pre-seeded test data)
        # When the patient contacts the ED with worsening symptoms
        # And the patient has not yet scheduled the required follow-up appointment
        # (assumed to have already occurred / triggered by the system)

        # Then the system escalates the follow-up process:
        wait_for_test_id(self.driver, "Urgent Scheduling")
        escalation_actions = [
            {"Action": "Urgent Scheduling", "Details": "Same-day appointment coordination"},
            {"Action": "Provider Notification", "Details": "Original discharging physician alerted"},
            {"Action": "Symptom Assessment", "Details": "Nurse triage for immediate vs delayed care"},
            {"Action": "Documentation Update", "Details": "Patient contact and status change recorded"},
        ]
        for row in escalation_actions:
            assert get_text(self.driver, row["Action"]) == row["Details"]

        # And enhanced monitoring is activated:
        enhanced_monitoring = [
            {"Type": "Daily Check-ins", "Action": "Nurse calls patient for status updates"},
            {"Type": "Expedited Referrals", "Action": "Fast-track specialist appointments"},
            {"Type": "Safety Net Activation", "Action": "Ensure patient has immediate care access"},
        ]
        for row in enhanced_monitoring:
            assert get_text(self.driver, row["Type"]) == row["Action"]

        # And the care team receives comprehensive updates:
        care_team_updates = [
            {"Member": "Discharging Physician", "Information": "Patient contact and current status"},
            {"Member": "Primary Care Provider", "Information": "Urgent need for appointment"},
            {"Member": "Charge Nurse", "Information": "Potential readmission risk identified"},
        ]
        for row in care_team_updates:
            assert get_text(self.driver, row["Member"]) == row["Information"]
