"""Playwright + pytest test for tests-with-given-when-then-features/19-mass-casualty-activation.feature
(equivalent to tests-with-playwright-javascript/19-mass-casualty-activation.test.js).

Assumes the app exposes data-testid attributes matching each Gherkin field
label (kebab-cased, see support/fields.py) and the shared data-testid
contract in support/login.py (login-identity, login-submit, app-root).
"""

from playwright.sync_api import sync_playwright

from support.fields import get_text, wait_for_test_id
from support.login import login, verify_system_is_operational


class TestMassCasualtyActivation:
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
        #   And I am logged in as "Charge Nurse Williams"
        #   And the mass casualty incident (MCI) module is available (assumed pre-seeded test data)
        #   And emergency contact systems are enabled (assumed pre-seeded test data)
        #   And surge capacity protocols are configured (assumed pre-seeded test data)
        verify_system_is_operational(self.page)
        login(self.page, "Charge Nurse Williams")

        feature_nav_link = wait_for_test_id(self.page, "Nav Mass Casualty Activation")
        feature_nav_link.click()
        wait_for_test_id(self.page, "Mass Casualty Activation Panel")

    def test_activate_mass_casualty_protocol_for_multi_vehicle_accident(self):
        # Given it is 16:30 on a Friday afternoon (assumed pre-seeded test data)
        # And normal ED operations are in progress with 12 patients currently in the department (assumed pre-seeded test data)
        # And EMS reports a multi-vehicle accident with 8+ casualties en route (assumed pre-seeded test data)
        # When I receive notification of the mass casualty incident: (assumed simulated by test fixture data)
        # And I activate the disaster protocol in the system
        self.page.get_by_test_id("activate-disaster-protocol").click()

        # Then the system immediately switches to surge capacity mode:
        wait_for_test_id(self.page, "Mode Indicator")
        system_change_rows = [
            {"label": "Mode Indicator", "value": '"MASS CASUALTY ACTIVE" banner displayed'},
            {"label": "Interface Switch", "value": "MCI-specific workflows activated"},
            {"label": "Normal Operations", "value": "Routine tasks suspended/deprioritized"},
            {"label": "Resource Allocation", "value": "Emergency resource management enabled"},
            {"label": "Communication Mode", "value": "Critical alerts and notifications active"},
        ]
        for row in system_change_rows:
            assert get_text(self.page, row["label"]) == row["value"]

        # And additional staff are automatically alerted:
        staff_alert_rows = [
            {"label": "Off-duty Physicians", "value": "SMS, Phone call"},
            {"label": "Off-duty Nurses", "value": "SMS, Phone call"},
            {"label": "Surgical Team", "value": "Overhead page, SMS"},
            {"label": "Lab/Radiology", "value": "System alert, Phone"},
            {"label": "Administration", "value": "Phone call, SMS"},
            {"label": "Security", "value": "Radio, Overhead page"},
        ]
        for row in staff_alert_rows:
            assert get_text(self.page, row["label"]) == row["value"]

        # And rapid registration workflows are created:
        registration_feature_rows = [
            {"label": "Patient Identification", "value": "Sequential numbering: MCI-001, MCI-002"},
            {"label": "Triage Tags", "value": "Color-coded electronic tags"},
            {"label": "Minimal Data Entry", "value": "Name, age, chief complaint only"},
            {"label": "Family Notification", "value": "Automated family alert system"},
            {"label": "Tracking Board", "value": "Real-time patient status dashboard"},
        ]
        for row in registration_feature_rows:
            assert get_text(self.page, row["label"]) == row["value"]

    def test_configure_surge_capacity_with_bed_and_resource_expansion(self):
        # Given the mass casualty protocol has been activated
        # And normal bed capacity is 20 beds
        # When the system enters surge capacity mode

        # Then additional treatment areas are activated:
        wait_for_test_id(self.page, "Hallway Beds")
        surge_area_rows = [
            {"label": "Hallway Beds", "value": "+6 treatment spaces"},
            {"label": "Procedure Rooms", "value": "+3 converted spaces"},
            {"label": "Observation Area", "value": "+8 holding spaces"},
            {"label": "Waiting Room Triage", "value": "+4 assessment areas"},
            {"label": "Ambulatory Care", "value": "+10 walking wounded"},
        ]
        for row in surge_area_rows:
            assert get_text(self.page, row["label"]) == row["value"]

        # And resource allocation is optimized for mass casualty:
        resource_allocation_rows = [
            {"label": "Trauma Bays", "value": "All 4 activated"},
            {"label": "Operating Rooms", "value": "3 rooms on standby"},
            {"label": "Ventilators", "value": "8 total (5 from ICU)"},
            {"label": "Blood Products", "value": "Massive transfusion protocol"},
            {"label": "Medication Carts", "value": "5 carts deployed"},
        ]
        for row in resource_allocation_rows:
            assert get_text(self.page, row["label"]) == row["value"]

        # And staffing ratios are adjusted for emergency operations:
        staffing_ratio_rows = [
            {"label": "Physicians", "value": "1:12 patients"},
            {"label": "Nurses", "value": "1:6 patients"},
            {"label": "Support Staff", "value": "Double coverage"},
        ]
        for row in staffing_ratio_rows:
            assert get_text(self.page, row["label"]) == row["value"]

    def test_implement_rapid_patient_registration_and_triage_workflow(self):
        # Given mass casualty mode is active
        # And the first ambulance arrives with 3 critical patients
        # When EMS brings patients to the ED

        # Then the rapid registration workflow is initiated:
        wait_for_test_id(self.page, "Patient Arrival")
        registration_step_rows = [
            {"label": "Patient Arrival", "value": "Immediate tag assignment: MCI-001, 002, 003"},
            {"label": "Triage Assessment", "value": "START triage protocol applied"},
            {"label": "Electronic Tagging", "value": "Color-coded digital tags assigned"},
            {"label": "Minimal Documentation", "value": "Name, estimated age, mechanism of injury"},
            {"label": "Bed Assignment", "value": "Automatic assignment by acuity"},
        ]
        for row in registration_step_rows:
            assert get_text(self.page, row["label"]) == row["value"]

        # And electronic triage tags are applied with color coding:
        triage_color_rows = [
            {"label": "Red (Immediate)", "value": "MCI-001 → Trauma Bay 1"},
            {"label": "Yellow (Delayed)", "value": "MCI-002 → Surge Bed 3"},
            {"label": "Green (Minor)", "value": "MCI-003 → Ambulatory Area"},
            {"label": "Black (Deceased)", "value": "Morgue coordination"},
        ]
        for row in triage_color_rows:
            assert get_text(self.page, row["label"]) == row["value"]

        # And family notification systems are activated:
        notification_process_rows = [
            {"label": "Emergency Contacts", "value": "Auto-dial from patient personal effects"},
            {"label": "Public Information", "value": "Hospital hotline number broadcasted"},
            {"label": "Media Coordination", "value": "Incident command liaison activated"},
            {"label": "Social Services", "value": "Family support team mobilized"},
        ]
        for row in notification_process_rows:
            assert get_text(self.page, row["label"]) == row["value"]

    def test_coordinate_with_external_emergency_services_and_hospitals(self):
        # Given a major mass casualty incident is in progress
        # And local EMS is overwhelmed with the response
        # When the system activates external coordination protocols

        # Then inter-facility communication is established:
        wait_for_test_id(self.page, "EMS Command Center")
        communication_channel_rows = [
            {"label": "EMS Command Center", "value": "Patient distribution and transport updates"},
            {"label": "Other Area Hospitals", "value": "Bed availability and transfer coordination"},
            {"label": "Air Medical Services", "value": "Helicopter transport for critical patients"},
            {"label": "Regional Trauma Centers", "value": "Specialty care transfer arrangements"},
        ]
        for row in communication_channel_rows:
            assert get_text(self.page, row["label"]) == row["value"]

        # And patient distribution management is activated:
        distribution_strategy_rows = [
            {"label": "Load Balancing", "value": "Distribute patients across regional facilities"},
            {"label": "Specialty Matching", "value": "Route patients to appropriate specialty care"},
            {"label": "Capacity Monitoring", "value": "Real-time bed availability tracking"},
            {"label": "Transport Coordination", "value": "Ambulance and helicopter scheduling"},
        ]
        for row in distribution_strategy_rows:
            assert get_text(self.page, row["label"]) == row["value"]

        # And regional emergency management integration occurs:
        integration_element_rows = [
            {"label": "Incident Command", "value": "Hospital EOC links with regional ICS"},
            {"label": "Resource Sharing", "value": "Equipment and staff sharing protocols"},
            {"label": "Information Sharing", "value": "Patient status updates to command center"},
            {"label": "Media Management", "value": "Coordinated public information releases"},
        ]
        for row in integration_element_rows:
            assert get_text(self.page, row["label"]) == row["value"]

    def test_manage_family_reunification_and_information_center(self):
        # Given multiple patients from the mass casualty incident are being treated
        # And families are arriving seeking information about their loved ones
        # When the family information center is activated

        # Then patient tracking and family communication systems are deployed:
        wait_for_test_id(self.page, "Information Hotline")
        family_support_system_rows = [
            {"label": "Information Hotline", "value": "Dedicated phone line with trained staff"},
            {"label": "Family Reunification", "value": "Secure area for family waiting and updates"},
            {"label": "Patient Tracking", "value": "Real-time status board for authorized viewers"},
            {"label": "Privacy Protection", "value": "HIPAA-compliant information sharing"},
        ]
        for row in family_support_system_rows:
            assert get_text(self.page, row["label"]) == row["value"]

        # And automated family notification processes are initiated:
        notification_method_rows = [
            {"label": "SMS Updates", "value": "Your family member is being treated safely"},
            {"label": "Phone Calls", "value": "Personal calls for critical status changes"},
            {"label": "Information Boards", "value": "General incident updates (no patient names)"},
            {"label": "Social Workers", "value": "One-on-one family support and counseling"},
        ]
        for row in notification_method_rows:
            assert get_text(self.page, row["label"]) == row["value"]

        # And patient status tracking is maintained:
        tracking_element_rows = [
            {"label": "Current Location", "value": "Treatment area, OR, transferred, etc."},
            {"label": "Medical Status", "value": "Stable, critical, treated and released"},
            {"label": "Next of Kin Contact", "value": "Verification and notification status"},
            {"label": "Discharge Planning", "value": "Expected timeline and care needs"},
        ]
        for row in tracking_element_rows:
            assert get_text(self.page, row["label"]) == row["value"]

    def test_transition_from_mass_casualty_mode_back_to_normal_operations(self):
        # Given the mass casualty incident has been resolved
        # And all patients are stabilized or transferred
        # And no additional casualties are expected
        # When I initiate the transition back to normal operations
        self.page.get_by_test_id("initiate-transition-to-normal").click()

        # Then the system manages the deactivation process:
        wait_for_test_id(self.page, "Incident Assessment")
        deactivation_step_rows = [
            {"label": "Incident Assessment", "value": "Review of patient outcomes and resources used"},
            {"label": "Staff Debriefing", "value": "Immediate hot wash and formal debriefing"},
            {"label": "Resource Restoration", "value": "Return equipment and supplies to normal areas"},
            {"label": "Documentation", "value": "Complete incident documentation and reports"},
        ]
        for row in deactivation_step_rows:
            assert get_text(self.page, row["label"]) == row["value"]

        # And normal ED operations are gradually restored:
        restoration_phase_rows = [
            {"label": "Immediate (0-30 min)", "value": "Secure scene"},
            {"label": "Short-term (30-60 min)", "value": "Resource cleanup"},
            {"label": "Medium-term (1-4 hrs)", "value": "Staff rotation"},
            {"label": "Long-term (4-24 hrs)", "value": "Full restoration"},
        ]
        for row in restoration_phase_rows:
            assert get_text(self.page, row["label"]) == row["value"]

        # And quality improvement activities are initiated:
        qi_activity_rows = [
            {"label": "After Action Review", "value": "Identify strengths and improvement areas"},
            {"label": "Performance Metrics", "value": "Analyze response times and patient outcomes"},
            {"label": "Protocol Updates", "value": "Revise procedures based on lessons learned"},
            {"label": "Training Needs", "value": "Identify staff training and education needs"},
        ]
        for row in qi_activity_rows:
            assert get_text(self.page, row["label"]) == row["value"]

    def test_handle_mass_casualty_incident_during_shift_change(self):
        # Given it is 19:00 during evening shift change
        # And day shift staff are preparing to leave
        # And evening shift staff are assuming duties
        # When a mass casualty incident is declared

        # Then the system manages staffing during the transition:
        wait_for_test_id(self.page, "Shift Hold")
        staffing_strategy_rows = [
            {"label": "Shift Hold", "value": "Day shift staff remain for incident response"},
            {"label": "Double Coverage", "value": "Both shifts work together during surge"},
            {"label": "Incident Command Continuity", "value": "Clear leadership chain established"},
            {"label": "Communication", "value": "All staff briefed on roles and responsibilities"},
        ]
        for row in staffing_strategy_rows:
            assert get_text(self.page, row["label"]) == row["value"]

        # And transition protocols are modified for the emergency:
        modified_protocol_rows = [
            {"label": "Handoff Procedures", "value": "Suspended until incident resolution"},
            {"label": "Staffing Ratios", "value": "Enhanced coverage with both shifts"},
            {"label": "Leadership Structure", "value": "Incident commander takes operational control"},
            {"label": "Modified Documentation", "value": "Emergency documentation procedures active"},
        ]
        for row in modified_protocol_rows:
            assert get_text(self.page, row["label"]) == row["value"]

    def test_test_mass_casualty_system_readiness_through_drill(self):
        # Given it is a scheduled quarterly mass casualty drill
        # And the drill scenario involves a simulated building collapse with 15 casualties
        # When the drill coordinator activates the test mass casualty protocol
        self.page.get_by_test_id("activate-test-mci-protocol").click()

        # Then the system activates in drill mode:
        wait_for_test_id(self.page, "Test Mode Indicator")
        drill_feature_rows = [
            {"label": "Test Mode Indicator", "value": '"DRILL - NOT REAL EMERGENCY" displayed'},
            {"label": "Simulated Patients", "value": "Test patient records created"},
            {"label": "Staff Participation", "value": "All roles and responsibilities tested"},
            {"label": "Resource Tracking", "value": "Equipment and supplies tracked but not used"},
        ]
        for row in drill_feature_rows:
            assert get_text(self.page, row["label"]) == row["value"]

        # And drill performance metrics are captured:
        drill_performance_metric_rows = [
            {"label": "Activation Time", "value": "Time from alert to full surge capacity"},
            {"label": "Staff Response Time", "value": "Time for staff to report and assume roles"},
            {"label": "Communication Speed", "value": "Time for all notifications to be completed"},
            {"label": "Resource Deployment", "value": "Time to set up surge areas and equipment"},
        ]
        for row in drill_performance_metric_rows:
            assert get_text(self.page, row["label"]) == row["value"]

        # And drill evaluation and improvement planning occurs:
        evaluation_component_rows = [
            {"label": "Protocol Effectiveness", "value": "How well procedures worked in practice"},
            {"label": "Staff Preparedness", "value": "Knowledge and skill gaps identified"},
            {"label": "System Performance", "value": "Technology and workflow efficiency"},
            {"label": "Improvement Plans", "value": "Action items for enhancing response capabilities"},
        ]
        for row in evaluation_component_rows:
            assert get_text(self.page, row["label"]) == row["value"]
