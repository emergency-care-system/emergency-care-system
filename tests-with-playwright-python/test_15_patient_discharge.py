"""Playwright + pytest test for tests-with-given-when-then-features/15-patient-discharge.feature
(equivalent to tests-with-playwright-javascript/15-patient-discharge.test.js).

Assumes the app exposes data-testid attributes matching each Gherkin field
label (kebab-cased, see support/fields.py) and the shared data-testid
contract in support/login.py (login-identity, login-submit, app-root).
"""

from playwright.sync_api import sync_playwright

from support.fields import fill_fields, get_text, wait_for_test_id
from support.login import login, verify_system_is_operational


class TestPatientDischarge:
    @classmethod
    def setup_class(cls):
        cls.playwright = sync_playwright().start()
        cls.browser = cls.playwright.chromium.launch()
        cls.page = cls.browser.new_page()

    @classmethod
    def teardown_class(cls):
        cls.browser.close()
        cls.playwright.stop()

    def setup_method(self):
        # Background:
        #   Given the emergency care system is operational
        #   And I am logged in as "Dr. Johnson"
        #   And the discharge module is active
        #   And billing integration is enabled
        #   And bed management system is connected
        verify_system_is_operational(self.page)
        login(self.page, "Dr. Johnson")
        # The discharge module, billing integration, and the bed
        # management system connection are assumed to be active backend
        # configuration already in place for this environment.

        patient_discharge_nav_link = wait_for_test_id(self.page, "Nav Patient Discharge")
        patient_discharge_nav_link.click()
        wait_for_test_id(self.page, "Patient Discharge Panel")

    def test_complete_standard_patient_discharge_with_instructions(self):
        # Given a patient "Jennifer Martinez" is in bed "ED-8"
        # And the patient has completed treatment for "urinary tract infection"
        # And all diagnostic tests and treatments are finished
        # And the patient is medically stable for discharge
        # (assumed pre-seeded test data)
        # When I enter discharge orders and instructions:
        fill_fields(
            self.page,
            [
                {"Field": "Discharge Status", "Value": "Home with medications"},
                {"Field": "Primary Diagnosis", "Value": "Urinary tract infection (N39.0)"},
                {"Field": "Medications", "Value": "Trimethoprim-Sulfamethoxazole 800mg BID x7d"},
                {"Field": "Follow-up Care", "Value": "Primary care physician in 3-5 days"},
                {"Field": "Activity Level", "Value": "Regular activities as tolerated"},
                {"Field": "Diet", "Value": "Regular diet, increase fluid intake"},
            ],
        )
        # "Return Precautions" is, for this scenario, both a fillable order
        # field and (simultaneously, before submit) a generated document
        # label rendered with the same data-testid -- so target the form's
        # copy explicitly (the first match in document order) instead of
        # the ambiguous fill_field/get_by_test_id helper.
        self.page.get_by_test_id("return-precautions").first.fill(
            "Fever >101°F, worsening symptoms, blood in urine"
        )
        # And I submit the discharge orders
        self.page.get_by_test_id("submit-discharge-form").click()

        # Then the system generates comprehensive discharge paperwork:
        wait_for_test_id(self.page, "Discharge Summary")
        discharge_paperwork = [
            {"Document": "Discharge Summary", "Content": "Treatment summary, diagnosis, medications"},
            {"Document": "Medication List", "Content": "Prescriptions with dosing instructions"},
            {"Document": "Follow-up Instructions", "Content": "PCP appointment scheduling information"},
            {"Document": "Return Precautions", "Content": "When to seek emergency care"},
            {"Document": "Patient Education", "Content": "UTI prevention and care instructions"},
        ]
        for row in discharge_paperwork:
            assert get_text(self.page, row["Document"]) == row["Content"]

        # And the system updates bed availability:
        bed_availability_updates = [
            {"Change": "Previous Status", "Details": "Occupied by Jennifer Martinez"},
            {"Change": "New Status", "Details": "Needs cleaning"},
            {"Change": "Availability", "Details": "Removed from available bed count"},
            {"Change": "Housekeeping Alert", "Details": "Cleaning notification sent"},
        ]
        for row in bed_availability_updates:
            assert get_text(self.page, row["Change"]) == row["Details"]

        # And billing processes are automatically triggered:
        billing_actions = [
            {"Action": "Final Charges", "Details": "All services and procedures captured"},
            {"Action": "Insurance Billing", "Details": "Claims prepared for submission"},
            {"Action": "Patient Statement", "Details": "Financial responsibility calculated"},
            {"Action": "Coding Review", "Details": "ICD-10 and CPT codes validated"},
        ]
        for row in billing_actions:
            assert get_text(self.page, row["Action"]) == row["Details"]

    def test_discharge_patient_with_prescription_medications(self):
        # Given a patient "Robert Chen" is ready for discharge
        # And treatment required multiple medications
        # (assumed pre-seeded test data)
        # When I enter discharge orders including prescriptions:
        fill_fields(
            self.page,
            [
                {"Field": "Amoxicillin Dose", "Value": "500mg"},
                {"Field": "Amoxicillin Frequency", "Value": "TID"},
                {"Field": "Amoxicillin Duration", "Value": "10 days"},
                {"Field": "Amoxicillin Special Instructions", "Value": "Take with food"},
                {"Field": "Ibuprofen Dose", "Value": "600mg"},
                {"Field": "Ibuprofen Frequency", "Value": "Q6H PRN"},
                {"Field": "Ibuprofen Duration", "Value": "5 days"},
                {"Field": "Ibuprofen Special Instructions", "Value": "For pain only"},
                {"Field": "Omeprazole Dose", "Value": "20mg"},
                {"Field": "Omeprazole Frequency", "Value": "Daily"},
                {"Field": "Omeprazole Duration", "Value": "14 days"},
                {"Field": "Omeprazole Special Instructions", "Value": "Take before breakfast"},
            ],
        )
        # And I include medication education:
        fill_fields(
            self.page,
            [
                {"Field": "Drug Interactions", "Value": "Avoid alcohol with antibiotics"},
                {"Field": "Side Effects", "Value": "Watch for nausea, diarrhea, allergic reactions"},
                {"Field": "Compliance", "Value": "Complete full antibiotic course"},
            ],
        )
        # And I submit the discharge
        self.page.get_by_test_id("submit-discharge-form").click()

        # Then the system generates medication-specific documentation:
        wait_for_test_id(self.page, "Prescription List")
        medication_documentation = [
            {"Document": "Prescription List", "Content": "All medications with complete instructions"},
            {"Document": "Drug Information", "Content": "Side effects, interactions, precautions"},
            {"Document": "Pharmacy List", "Content": "Nearby pharmacies with hours"},
            {"Document": "Medication Calendar", "Content": "Dosing schedule for patient reference"},
        ]
        for row in medication_documentation:
            assert get_text(self.page, row["Document"]) == row["Content"]

        # And prescriptions are electronically transmitted to patient's preferred pharmacy
        assert "transmitted" in get_text(self.page, "Prescription Transmission Status").lower()

        # And medication allergy checking is performed one final time
        allergy_check_status = get_text(self.page, "Medication Allergy Check Status").lower()
        assert any(word in allergy_check_status for word in ("checked", "performed", "cleared"))

        # And patient receives medication counseling checklist
        counseling_checklist = wait_for_test_id(self.page, "Medication Counseling Checklist")
        assert counseling_checklist.is_visible()

    def test_discharge_patient_requiring_follow_up_appointments(self):
        # Given a patient "Maria Santos" needs specialized follow-up care
        # And the treatment was for "complex laceration repair"
        # (assumed pre-seeded test data)
        # When I enter discharge orders with follow-up requirements:
        fill_fields(
            self.page,
            [
                {"Field": "Wound Check Timeframe", "Value": "2-3 days"},
                {"Field": "Wound Check Specialist Required", "Value": "Primary care"},
                {"Field": "Wound Check Special Instructions", "Value": "Remove sutures"},
                {"Field": "Specialist Consult Timeframe", "Value": "1 week"},
                {"Field": "Specialist Consult Specialist Required", "Value": "Plastic surgeon"},
                {"Field": "Specialist Consult Special Instructions", "Value": "Scar management"},
                {"Field": "Lab Follow-up Timeframe", "Value": "5 days"},
                {"Field": "Lab Follow-up Specialist Required", "Value": "Primary care"},
                {"Field": "Lab Follow-up Special Instructions", "Value": "Check CBC"},
            ],
        )
        # And I specify wound care instructions:
        fill_fields(
            self.page,
            [
                {"Field": "Dressing Changes", "Value": "Change daily, keep dry for 48 hours"},
                {"Field": "Cleaning Protocol", "Value": "Gentle soap and water after 48 hours"},
                {"Field": "Activity Restrictions", "Value": "No heavy lifting >10 lbs for 2 weeks"},
                {"Field": "Signs of Infection", "Value": "Redness, swelling, pus, fever"},
            ],
        )

        # Then the system schedules and documents follow-up care:
        wait_for_test_id(self.page, "Appointment Booking")
        follow_up_scheduling = [
            {"Action": "Appointment Booking", "Details": "Attempts to schedule with preferred providers"},
            {"Action": "Referral Generation", "Details": "Electronic referrals to specialists"},
            {"Action": "Reminder Setup", "Details": "Patient reminders for appointments"},
        ]
        for row in follow_up_scheduling:
            assert get_text(self.page, row["Action"]) == row["Details"]

        # And comprehensive wound care instructions are provided
        wound_care_instructions = wait_for_test_id(self.page, "Wound Care Instructions")
        assert wound_care_instructions.is_visible()

        # And follow-up appointment confirmations are sent to patient
        appointment_confirmation_status = get_text(self.page, "Appointment Confirmation Status").lower()
        assert "sent" in appointment_confirmation_status or "confirmed" in appointment_confirmation_status

        # And referring physician receives notification of specialist referral
        specialist_referral_notification_status = get_text(
            self.page, "Specialist Referral Notification Status"
        ).lower()
        assert "sent" in specialist_referral_notification_status or "notified" in specialist_referral_notification_status

    def test_handle_discharge_with_insurance_authorization_requirements(self):
        # Given a patient "David Kim" requires expensive follow-up imaging
        # And the patient's insurance requires prior authorization
        # (assumed pre-seeded test data)
        # When I enter discharge orders including:
        fill_fields(
            self.page,
            [
                {"Field": "Imaging Study", "Value": "MRI lumbar spine within 2 weeks"},
                {"Field": "Estimated Cost", "Value": "$2,400"},
                {"Field": "Medical Necessity", "Value": "Rule out disc herniation"},
            ],
        )
        # And I submit the discharge orders
        self.page.get_by_test_id("submit-discharge-form").click()

        # Then the system handles insurance requirements:
        wait_for_test_id(self.page, "Authorization Check")
        insurance_requirements = [
            {"Process": "Authorization Check", "Action": "Prior auth required for MRI"},
            {"Process": "Documentation Prep", "Action": "Clinical justification prepared"},
            {"Process": "Patient Notification", "Action": "Informed of authorization process"},
            {"Process": "Alternative Options", "Action": "Suggest urgent care MRI if auth denied"},
        ]
        for row in insurance_requirements:
            assert get_text(self.page, row["Process"]) == row["Action"]

        # And the patient receives information about:
        patient_information = [
            {"Type": "Authorization Process", "Content": "Timeline and requirements explained"},
            {"Type": "Financial Options", "Content": "Self-pay rates and payment plans"},
            {"Type": "Alternative Providers", "Content": "Facilities that may not require pre-auth"},
        ]
        for row in patient_information:
            assert get_text(self.page, row["Type"]) == row["Content"]

        # And insurance pre-authorization request is automatically submitted
        assert "submitted" in get_text(self.page, "Insurance Pre-Authorization Status").lower()

    def test_discharge_pediatric_patient_with_parent_guardian_instructions(self):
        # Given a pediatric patient "Emma Foster" (age 6) is ready for discharge
        # And the parent "Sarah Foster" is present
        # And treatment was for "febrile seizure"
        # (assumed pre-seeded test data)
        # When I enter pediatric discharge orders:
        fill_fields(
            self.page,
            [
                {"Field": "Weight-based Medications", "Value": "Acetaminophen 10mg/kg Q6H PRN fever"},
                {"Field": "Parent Education", "Value": "Fever management, seizure precautions"},
                {"Field": "Activity Restrictions", "Value": "No swimming for 24 hours"},
                {"Field": "School Return", "Value": "May return tomorrow if fever-free"},
            ],
        )
        # And I provide seizure-specific education:
        fill_fields(
            self.page,
            [
                {"Field": "Seizure Precautions", "Value": "Keep child safe during future episodes"},
                {"Field": "When to Call 911", "Value": "Seizure >5 minutes, difficulty breathing"},
                {"Field": "Temperature Control", "Value": "Aggressive fever reduction strategies"},
            ],
        )

        # Then the system generates pediatric-specific discharge materials:
        wait_for_test_id(self.page, "Parent Instructions")
        pediatric_materials = [
            {"Document": "Parent Instructions", "Content": "Age-appropriate medication dosing"},
            {"Document": "Emergency Signs", "Content": "When to bring child back to ED"},
            {"Document": "School Note", "Content": "Medical excuse and return instructions"},
            {"Document": "Developmental Info", "Content": "Normal vs concerning behaviors post-seizure"},
        ]
        for row in pediatric_materials:
            assert get_text(self.page, row["Document"]) == row["Content"]

        # And parent acknowledgment is electronically captured
        parent_acknowledgment_status = get_text(self.page, "Parent Acknowledgment Status").lower()
        assert "captured" in parent_acknowledgment_status or "recorded" in parent_acknowledgment_status

        # And pediatric follow-up with primary care pediatrician is scheduled
        assert "scheduled" in get_text(self.page, "Pediatric Follow-up Status").lower()

        # And school nurse receives medical summary if parent consents
        school_nurse_notification_status = get_text(self.page, "School Nurse Notification Status").lower()
        assert "sent" in school_nurse_notification_status or "notified" in school_nurse_notification_status

    def test_handle_discharge_during_shift_change(self):
        # Given a patient "Lisa Brown" is ready for discharge at 18:45
        # And shift change occurs at 19:00
        # And "Dr. Day" (day shift) is discharging the patient
        # And "Dr. Night" (evening shift) is incoming
        # (assumed pre-seeded test data)
        # When "Dr. Day" enters the discharge orders
        # And the discharge process extends past shift change
        # (assumed to have already occurred / triggered by the system)

        # Then the system manages the transition seamlessly:
        wait_for_test_id(self.page, "Discharge Ownership")
        transition_management = [
            {"Item": "Discharge Ownership", "Action": "Dr. Day completes discharge process"},
            {"Item": "Documentation", "Action": "All discharge notes under Dr. Day's name"},
            {"Item": "Follow-up Responsibility", "Action": "Any issues route to Dr. Night"},
            {"Item": "Billing Attribution", "Action": "Dr. Day receives credit for discharge"},
        ]
        for row in transition_management:
            assert get_text(self.page, row["Item"]) == row["Action"]

        # And both physicians receive handoff notification:
        handoff_notifications = [
            {"Physician": "Dr. Day", "Content": "Discharge completed for Lisa Brown"},
            {"Physician": "Dr. Night", "Content": "Lisa Brown discharged - available for questions"},
        ]
        for row in handoff_notifications:
            assert get_text(self.page, f"{row['Physician']} Notification") == row["Content"]

        # And the bed becomes available for evening shift patient flow
        assert "available" in get_text(self.page, "Bed Availability Status").lower()

    def test_discharge_patient_against_medical_advice_ama(self):
        # Given a patient "Michael Davis" wants to leave against medical advice
        # And the patient has been informed of risks
        # And the patient has decision-making capacity
        # (assumed pre-seeded test data)
        # When I process an AMA discharge:
        fill_fields(
            self.page,
            [
                {"Field": "Risk Explanation", "Value": "Documented that risks were explained"},
                {"Field": "Patient Understanding", "Value": "Patient verbalized understanding of risks"},
                {"Field": "Capacity Assessment", "Value": "Patient has decision-making capacity"},
                {"Field": "Witness Required", "Value": "Nurse witness to AMA conversation"},
            ],
        )
        # And I enter minimal safe discharge instructions:
        fill_fields(
            self.page,
            [
                {"Field": "Return Immediately", "Value": "If symptoms worsen or new symptoms develop"},
                {"Field": "Follow-up Care", "Value": "Strong recommendation for PCP visit"},
                {"Field": "Medication Safety", "Value": "Critical medications must be continued"},
            ],
        )

        # Then the system generates AMA-specific documentation:
        wait_for_test_id(self.page, "AMA Form")
        ama_documentation = [
            {"Document": "AMA Form", "Content": "Legal documentation of patient choice"},
            {"Document": "Risk Documentation", "Content": "Medical risks of leaving explained"},
            {"Document": "Witness Signatures", "Content": "Patient, physician, and nurse signatures"},
            {"Document": "Limited Liability", "Content": "Hospital liability limitations documented"},
        ]
        for row in ama_documentation:
            assert get_text(self.page, row["Document"]) == row["Content"]

        # And billing processes reflect AMA status
        assert "ama" in get_text(self.page, "Billing AMA Status").lower()

        # And legal risk management is notified of AMA discharge
        assert "notified" in get_text(self.page, "Legal Risk Management Notification Status").lower()

        # And patient still receives basic safety instructions
        basic_safety_instructions = wait_for_test_id(self.page, "Basic Safety Instructions")
        assert basic_safety_instructions.is_visible()

    def test_batch_discharge_processing_during_high_volume(self):
        # Given multiple patients are ready for simultaneous discharge:
        #   | Patient Name    | Bed    | Diagnosis        | Discharge Type     |
        #   | Patient A       | ED-3   | Minor injury     | Home              |
        #   | Patient B       | ED-7   | Gastroenteritis  | Home with meds    |
        #   | Patient C       | ED-11  | Anxiety          | Home with referral |
        # (assumed pre-seeded test data)
        # When I process multiple discharges efficiently
        # (assumed to have already occurred / triggered by the system)

        # Then the system handles batch processing:
        wait_for_test_id(self.page, "Template Usage")
        batch_processing = [
            {"Feature": "Template Usage", "Functionality": "Common discharge templates applied"},
            {"Feature": "Automated Documentation", "Functionality": "Standard instructions auto-populated"},
            {"Feature": "Concurrent Processing", "Functionality": "Multiple discharges processed simultaneously"},
        ]
        for row in batch_processing:
            assert get_text(self.page, row["Feature"]) == row["Functionality"]

        # And all bed updates occur simultaneously:
        bed_updates = [
            {"Management": "Status Updates", "Action": 'All beds marked "needs cleaning"'},
            {"Management": "Housekeeping Batch", "Action": "Single notification for multiple rooms"},
            {"Management": "Availability Count", "Action": "Bed count updated after all discharges"},
        ]
        for row in bed_updates:
            assert get_text(self.page, row["Management"]) == row["Action"]

        # And billing processes are optimized for batch handling
        assert "optimized" in get_text(self.page, "Batch Billing Status").lower()

    def test_track_discharge_metrics_and_quality_indicators(self):
        # Given patient discharges are being processed
        # (assumed pre-seeded test data)
        # When discharge orders are completed
        # (assumed to have already occurred / triggered by the system)

        # Then the system tracks key performance indicators:
        wait_for_test_id(self.page, "Discharge Time")
        performance_indicators = [
            {"Metric": "Discharge Time", "Measurement": "Order entry to patient departure"},
            {"Metric": "Readmission Rate", "Measurement": "72-hour return rate tracking"},
            {"Metric": "Instruction Quality", "Measurement": "Patient understanding verification"},
            {"Metric": "Follow-up Compliance", "Measurement": "Scheduled appointment attendance"},
        ]
        for row in performance_indicators:
            assert get_text(self.page, row["Metric"]) == row["Measurement"]

        # And generates quality reports:
        quality_reports = [
            {"Report": "Provider Performance", "Content": "Discharge efficiency by physician"},
            {"Report": "Patient Satisfaction", "Content": "Discharge process satisfaction scores"},
            {"Report": "Readmission Analysis", "Content": "Patterns in early returns"},
        ]
        for row in quality_reports:
            assert get_text(self.page, row["Report"]) == row["Content"]

        # And identifies improvement opportunities:
        improvement_opportunities = [
            {"Area": "Process Efficiency", "Recommendation": "Streamline documentation workflows"},
            {"Area": "Patient Education", "Recommendation": "Enhance instruction clarity"},
            {"Area": "Follow-up Coordination", "Recommendation": "Improve appointment scheduling system"},
        ]
        for row in improvement_opportunities:
            assert get_text(self.page, row["Area"]) == row["Recommendation"]
