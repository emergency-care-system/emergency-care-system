"""Playwright + pytest test for tests-with-given-when-then-features/10-critical-lab-alert.feature
(equivalent to tests-with-playwright-javascript/10-critical-lab-alert.test.js).

Assumes the app exposes data-testid attributes matching each Gherkin field
label (kebab-cased, see support/fields.py) and the shared data-testid
contract in support/login.py (login-identity, login-submit, app-root).
"""

from playwright.sync_api import sync_playwright

from support.fields import fill_field, fill_fields, get_text, wait_for_test_id
from support.login import login, verify_system_is_operational


class TestCriticalLabAlert:
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
        #   And the critical value alert system is enabled
        #   And laboratory interfaces are functioning
        #   And all patient displays are connected to the alert system
        verify_system_is_operational(self.page)
        # The critical value alert system, laboratory interfaces, and
        # patient display connections are assumed pre-seeded test data /
        # environment configuration. This feature has no "logged in as"
        # step in its Background.
        login(self.page, "a lab technician")

        critical_lab_alert_nav_link = wait_for_test_id(self.page, "Nav Critical Lab Alert")
        critical_lab_alert_nav_link.click()
        wait_for_test_id(self.page, "Critical Lab Alert Panel")

    def test_process_critically_high_troponin_result_with_immediate_alerts(self):
        # Given a patient "Robert Martinez" is in bed "ED-7"
        # And the attending physician is "Dr. Johnson"
        # And the charge nurse is "Nurse Williams"
        # And troponin was ordered for "chest pain evaluation"
        # (assumed pre-seeded test data)

        # When the laboratory result is received:
        #   | Test Name       | Result | Reference Range | Units  | Critical Threshold | Status |
        #   | Troponin I      | 5.8    | 0.0-0.04       | ng/mL  | >0.4              | Final  |
        fill_fields(
            self.page,
            [
                {"Field": "Test Name", "Value": "Troponin I"},
                {"Field": "Result", "Value": "5.8"},
                {"Field": "Reference Range", "Value": "0.0-0.04"},
                {"Field": "Units", "Value": "ng/mL"},
                {"Field": "Critical Threshold", "Value": ">0.4"},
                {"Field": "Status", "Value": "Final"},
            ],
        )
        self.page.get_by_test_id("receive-lab-result-button").click()

        # Then the system immediately triggers critical alerts
        assert "triggered" in get_text(self.page, "Critical Alert Status").lower()

        # And the attending physician "Dr. Johnson" receives immediate notifications:
        #   | Notification Type | Content                                        | Delivery Method |
        #   | Mobile Push Alert | 🔴 CRITICAL: Troponin I 5.8 ng/mL - ED-7     | Mobile App      |
        #   | SMS Alert        | CRITICAL LAB: R.Martinez ED-7 Troponin 5.8    | Text Message    |
        #   | Popup Alert      | CRITICAL VALUE - Requires Acknowledgment       | Workstation     |
        assert get_text(self.page, "Mobile Push Alert") == "🔴 CRITICAL: Troponin I 5.8 ng/mL - ED-7"
        assert get_text(self.page, "SMS Alert") == "CRITICAL LAB: R.Martinez ED-7 Troponin 5.8"
        assert get_text(self.page, "Popup Alert") == "CRITICAL VALUE - Requires Acknowledgment"

        # And the charge nurse "Nurse Williams" receives critical notifications:
        #   | Notification Type | Content                                        | Delivery Method |
        #   | Desktop Alert    | CRITICAL: Troponin 5.8 - Bed ED-7            | Workstation     |
        #   | Overhead Page    | Critical lab value bed ED-7                   | PA System       |
        #   | Mobile Alert     | Critical troponin result requires attention   | Mobile Device   |
        assert get_text(self.page, "Desktop Alert") == "CRITICAL: Troponin 5.8 - Bed ED-7"
        assert get_text(self.page, "Overhead Page") == "Critical lab value bed ED-7"
        assert get_text(self.page, "Mobile Alert") == "Critical troponin result requires attention"

        # And red flag indicators appear on all patient displays:
        #   | Display Location     | Alert Indicator                              |
        #   | Patient Monitor      | 🔴 CRITICAL LAB flashing red banner         |
        #   | Bedside Workstation  | Red alert icon next to patient name         |
        #   | Main ED Dashboard    | Red flag on bed ED-7 status                 |
        #   | Mobile Devices       | Red notification badge on patient chart     |
        #   | Nursing Station      | Critical value alert on patient board       |
        red_flag_indicators = self.page.get_by_test_id("red-flag-indicator").all()
        assert len(red_flag_indicators) == 5

    def test_handle_critical_troponin_with_physician_acknowledgment_requirements(self):
        # Given a patient "Maria Santos" is in bed "ED-12"
        # And the attending physician is "Dr. Lee"
        # And a critically high troponin result of "7.2 ng/mL" is received
        # (assumed pre-seeded test data)

        # When the critical alert is triggered
        self.page.get_by_test_id("trigger-critical-alert-button").click()

        # Then the system requires physician acknowledgment:
        assert get_text(self.page, "Initial Alert") == "Must acknowledge receipt within 15 minutes"
        assert get_text(self.page, "Clinical Review") == "Must document result review"
        assert get_text(self.page, "Action Plan") == "Must indicate next steps taken"

        # And if "Dr. Lee" does not acknowledge within 15 minutes:
        assert get_text(self.page, "Secondary Alert") == "Alert sent to backup physician"
        assert get_text(self.page, "Charge Nurse Alert") == "Escalation notice to charge nurse"
        assert get_text(self.page, "Supervisor Alert") == "Department supervisor notified"

        # And the acknowledgment status is tracked:
        #   | Status              | Timestamp | Provider    | Action              |
        #   | Alert Sent          | 14:30:15  | System      | Initial notification|
        #   | Acknowledged        | 14:32:45  | Dr. Lee     | Acknowledged receipt|
        #   | Reviewed            | 14:35:20  | Dr. Lee     | Documented review   |
        #   | Action Taken        | 14:40:10  | Dr. Lee     | Treatment initiated |
        acknowledgment_status_entries = self.page.get_by_test_id("acknowledgment-status-entry").all()
        assert len(acknowledgment_status_entries) == 4

    def test_process_multiple_critical_values_simultaneously(self):
        # Given multiple patients have critical troponin results:
        #   | Patient Name    | Bed   | Troponin Result | Attending     | Severity  |
        #   | John Williams   | ED-3  | 3.2 ng/mL      | Dr. Adams     | High      |
        #   | Lisa Johnson    | ED-8  | 8.9 ng/mL      | Dr. Brown     | Critical  |
        #   | Mike Davis      | ED-15 | 4.1 ng/mL      | Dr. Adams     | High      |
        # (assumed pre-seeded test data)

        # When all critical results are received simultaneously
        self.page.get_by_test_id("receive-critical-results-button").click()

        # Then the system prioritizes alerts by severity:
        #   | Priority | Patient      | Alert Level | Notification Urgency    |
        #   | 1        | Lisa Johnson | Critical    | Immediate - All channels|
        #   | 2        | Mike Davis   | High        | Urgent - Standard alerts|
        #   | 3        | John Williams| High        | Urgent - Standard alerts|
        assert get_text(self.page, "Priority 1 Patient") == "Lisa Johnson"
        assert get_text(self.page, "Priority 1 Alert Level") == "Critical"
        assert get_text(self.page, "Priority 2 Patient") == "Mike Davis"
        assert get_text(self.page, "Priority 2 Alert Level") == "High"
        assert get_text(self.page, "Priority 3 Patient") == "John Williams"
        assert get_text(self.page, "Priority 3 Alert Level") == "High"

        # And physicians receive prioritized notifications:
        assert get_text(self.page, "Dr. Brown Alert Summary") == "CRITICAL: Lisa Johnson Trop 8.9 - IMMEDIATE"
        assert get_text(self.page, "Dr. Adams Alert Summary") == "HIGH: 2 patients with elevated troponin"

        # And the charge nurse receives a summary alert:
        assert get_text(self.page, "Mass Alert") == "3 critical troponin results requiring attention"
        assert get_text(self.page, "Priority List") == "Lisa Johnson (Critical), others (High)"

        # And all patient displays show appropriately color-coded flags
        color_coded_flags = self.page.get_by_test_id("color-coded-flag").all()
        assert len(color_coded_flags) == 3

    def test_handle_critical_troponin_during_shift_change(self):
        # Given a patient "Catherine Brown" is in bed "ED-6"
        # And it is 19:00 during evening shift change
        # And the day shift physician "Dr. Wilson" ordered the troponin
        # And the evening shift physician "Dr. Taylor" has assumed care
        # (assumed pre-seeded test data)

        # When a critically high troponin result of "6.1 ng/mL" is received
        fill_field(self.page, "Troponin Result", "6.1 ng/mL")
        self.page.get_by_test_id("receive-lab-result-button").click()

        # Then both physicians receive critical alerts:
        #   | Physician  | Alert Type    | Content                                |
        #   | Dr. Taylor | Primary Alert | CRITICAL Troponin 6.1 - Your patient  |
        #   | Dr. Wilson | Handoff Alert | FYI: Critical result on your order     |
        assert get_text(self.page, "Dr. Taylor Alert") == "CRITICAL Troponin 6.1 - Your patient"
        assert get_text(self.page, "Dr. Wilson Alert") == "FYI: Critical result on your order"

        # And the charge nurse receives handoff-specific notification:
        assert get_text(self.page, "Shift Context") == "Critical result during physician handoff"
        assert get_text(self.page, "Current MD") == "Dr. Taylor (assuming care)"
        assert get_text(self.page, "Ordering MD") == "Dr. Wilson (ordered test)"

        # And the handoff documentation is automatically updated
        handoff_documentation = self.page.get_by_test_id("handoff-documentation")
        assert handoff_documentation.is_visible()

        # And red flags appear with shift change context indicators
        red_flag_indicators = self.page.get_by_test_id("red-flag-indicator").all()
        assert len(red_flag_indicators) > 0

    def test_process_critical_troponin_with_additional_cardiac_markers(self):
        # Given a patient "Steven Kim" is in bed "ED-11"
        # And multiple cardiac markers were ordered
        # (assumed pre-seeded test data)

        # When critical and related results are received:
        #   | Test Name    | Result | Reference Range | Critical | Clinical Significance |
        #   | Troponin I   | 4.7    | 0.0-0.04       | Yes      | Acute MI indicated    |
        #   | CK-MB        | 45     | 0-6.3          | Yes      | Myocardial damage     |
        #   | Myoglobin    | 280    | 25-72          | No       | Elevated but not critical|
        self.page.get_by_test_id("receive-lab-results-button").click()

        # Then the system groups related critical values:
        #   | Alert Category  | Content                                      |
        #   | Cardiac Panel   | Multiple critical cardiac markers            |
        #   | Primary Alert   | Troponin I: 4.7 ng/mL (CRITICAL)           |
        #   | Secondary Alert | CK-MB: 45 ng/mL (CRITICAL)                 |
        #   | Supporting Data | Myoglobin: 280 ng/mL (Elevated)            |
        assert get_text(self.page, "Cardiac Panel") == "Multiple critical cardiac markers"
        assert get_text(self.page, "Primary Alert") == "Troponin I: 4.7 ng/mL (CRITICAL)"
        assert get_text(self.page, "Secondary Alert") == "CK-MB: 45 ng/mL (CRITICAL)"
        assert get_text(self.page, "Supporting Data") == "Myoglobin: 280 ng/mL (Elevated)"

        # And enhanced clinical context is provided:
        assert get_text(self.page, "Clinical Indication") == "Acute myocardial infarction likely"
        assert get_text(self.page, "Recommended Actions") == "Cardiology consult, STEMI protocol"
        assert get_text(self.page, "Time Sensitivity") == "Treatment within 90 minutes critical"

        # And STEMI protocol alerts are automatically triggered
        wait_for_test_id(self.page, "STEMI Protocol Alert")

    def test_handle_false_positive_critical_troponin_alerts(self):
        # Given a patient "Nancy Rodriguez" is in bed "ED-4"
        # And a troponin result of "5.1 ng/mL" triggers a critical alert
        # (assumed pre-seeded test data)

        # When the laboratory calls to report a sample error
        self.page.get_by_test_id("report-sample-error-button").click()

        # And a corrected result shows "0.03 ng/mL" (normal)
        fill_field(self.page, "Corrected Troponin Result", "0.03 ng/mL")
        self.page.get_by_test_id("submit-correction-button").click()

        # Then the system processes the correction:
        assert get_text(self.page, "Cancel Alert") == "Original critical alert is cancelled"
        assert get_text(self.page, "Send Correction") == "Corrected value sent to all recipients"
        assert get_text(self.page, "Document Error") == "Lab error documented in audit trail"

        # And correction notifications are sent:
        assert get_text(self.page, "Alert Cancellation") == "CANCELLED: Previous critical troponin alert"
        assert get_text(self.page, "Corrected Value") == "Troponin corrected to 0.03 ng/mL (Normal)"
        assert get_text(self.page, "Error Explanation") == "Laboratory sample contamination identified"

        # And red flags are removed from all patient displays
        red_flag_indicators = self.page.get_by_test_id("red-flag-indicator").all()
        assert len(red_flag_indicators) == 0

        # And the correction is logged for quality assurance review
        quality_assurance_log = self.page.get_by_test_id("quality-assurance-log")
        assert quality_assurance_log.is_visible()

    def test_critical_troponin_with_patient_transfer_requirements(self):
        # Given a patient "Timothy Chang" is in bed "ED-9"
        # And a critically high troponin of "9.3 ng/mL" is received
        # And the patient requires immediate transfer to cardiac unit
        # (assumed pre-seeded test data)

        # When the critical alert is processed
        self.page.get_by_test_id("process-critical-alert-button").click()

        # Then transfer coordination alerts are included:
        assert get_text(self.page, "Transfer Required") == "Patient needs immediate cardiac unit transfer"
        assert get_text(self.page, "Bed Availability") == "CCU bed 302 available"
        assert get_text(self.page, "Transport Time") == "Transport team ETA 10 minutes"

        # And receiving unit notifications are sent:
        assert get_text(self.page, "CCU Alert") == "Incoming transfer - Critical troponin 9.3"
        assert get_text(self.page, "Cardiology Alert") == "Urgent consult needed - STEMI protocol"

        # And transfer documentation is automatically initiated
        transfer_documentation = self.page.get_by_test_id("transfer-documentation")
        assert transfer_documentation.is_visible()

        # And critical alerts follow the patient to the receiving unit
        wait_for_test_id(self.page, "Patient Alert Handoff")

    def test_validate_critical_troponin_alert_system_functionality(self):
        # Given the critical alert system is being tested
        # (assumed pre-seeded test data)

        # When a test troponin result of "TEST-5.0 ng/mL" is processed
        fill_field(self.page, "Test Troponin Result", "TEST-5.0 ng/mL")
        self.page.get_by_test_id("process-test-result-button").click()

        # Then the system validates all alert pathways:
        assert get_text(self.page, "Physician Mobile") == "Test alert delivered successfully"
        assert get_text(self.page, "Charge Nurse") == "Test alert delivered successfully"
        assert get_text(self.page, "Patient Displays") == "Red flags displayed correctly"
        assert get_text(self.page, "Audit Trail") == "Test alert logged with timestamp"

        # And test alerts are clearly marked as "SYSTEM TEST"
        assert get_text(self.page, "Test Alert Marking") == "SYSTEM TEST"

        # And all test alerts are automatically cleared after validation
        test_alerts = self.page.get_by_test_id("test-alert").all()
        assert len(test_alerts) == 0

        # And system performance metrics are recorded:
        #   | Metric           | Measurement                                  |
        #   | Alert Latency    | <30 seconds from result to notification     |
        #   | Delivery Success | 100% successful delivery to all recipients  |
        #   | Display Update   | <5 seconds to update all patient displays   |
        assert get_text(self.page, "Alert Latency") == "<30 seconds from result to notification"
        assert get_text(self.page, "Delivery Success") == "100% successful delivery to all recipients"
        assert get_text(self.page, "Display Update") == "<5 seconds to update all patient displays"
