"""Playwright + pytest test for tests-with-given-when-then-features/09-lab-result-processing.feature
(equivalent to tests-with-playwright-javascript/09-lab-result-processing.test.js).

Assumes the app exposes data-testid attributes matching each Gherkin field
label (kebab-cased, see support/fields.py) and the shared data-testid
contract in support/login.py (login-identity, login-submit, app-root).
"""

from playwright.sync_api import sync_playwright

from support.fields import get_text, wait_for_test_id
from support.login import login, verify_system_is_operational


class TestLabResultProcessing:
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
        #   And the HL7 interface with the laboratory system is active
        #   And critical value alert system is enabled
        #   And physician notification system is functional
        verify_system_is_operational(self.page)
        # The HL7 interface, critical value alert system, and physician
        # notification system being active/enabled are assumed pre-seeded
        # test data / environment configuration. This feature has no
        # "logged in as" step in its Background.
        login(self.page, "a lab technician")

        lab_result_processing_nav_link = wait_for_test_id(self.page, "Nav Lab Result Processing")
        lab_result_processing_nav_link.click()
        wait_for_test_id(self.page, "Lab Result Processing Panel")

    def test_process_normal_lab_results_via_hl7_interface(self):
        # Given a patient "Jennifer Lopez" is in bed "ED-8"
        # And laboratory orders were placed for "CBC, Basic Metabolic Panel"
        # And the attending physician is "Dr. Smith"
        # (assumed pre-seeded test data)

        # When the lab system sends results via HL7 interface:
        #   | Test Name          | Result    | Reference Range | Units  | Status   | Timestamp |
        #   | White Blood Cells  | 7.2       | 4.0-10.0       | K/uL   | Final    | 14:30     |
        #   | Hemoglobin         | 13.5      | 12.0-16.0      | g/dL   | Final    | 14:30     |
        #   | Sodium             | 140       | 136-145        | mmol/L | Final    | 14:30     |
        #   | Potassium          | 4.1       | 3.5-5.0        | mmol/L | Final    | 14:30     |
        #   | Creatinine         | 1.0       | 0.6-1.2        | mg/dL  | Final    | 14:30     |
        self.page.get_by_test_id("receive-lab-results-button").click()

        # Then the system updates the patient record with all results
        assert "updated" in get_text(self.page, "Patient Record Update Status").lower()

        # And the results are marked as "Normal" in the patient chart
        assert get_text(self.page, "Result Status") == "Normal"

        # And a standard notification is sent to "Dr. Smith":
        assert get_text(self.page, "Lab Results") == "Normal CBC and BMP available for review"
        assert get_text(self.page, "Patient") == "Jennifer Lopez, Bed ED-8"
        assert get_text(self.page, "Timestamp") == "14:30"
        assert get_text(self.page, "Priority") == "Standard"

        # And the results appear in the patient's timeline with normal value indicators
        timeline_entries = self.page.get_by_test_id("timeline-result-entry").all()
        assert len(timeline_entries) > 0

        # And no critical value alerts are generated
        critical_value_alerts = self.page.get_by_test_id("critical-value-alert").all()
        assert len(critical_value_alerts) == 0

        # And the nursing staff is notified that results are available for review
        assert len(get_text(self.page, "Nursing Results Notification")) > 0

    def test_process_critical_lab_results_with_immediate_alerts(self):
        # Given a patient "Michael Davis" is in bed "ED-12"
        # And laboratory orders were placed for "Troponin, BNP, D-Dimer"
        # And the attending physician is "Dr. Johnson"
        # (assumed pre-seeded test data)

        # When the lab system sends critical results via HL7 interface:
        #   | Test Name    | Result | Reference Range | Units  | Status | Critical | Timestamp |
        #   | Troponin I   | 8.5    | 0.0-0.04       | ng/mL  | Final  | Yes      | 15:45     |
        #   | BNP          | 1200   | 0-100          | pg/mL  | Final  | Yes      | 15:45     |
        #   | D-Dimer      | 0.8    | 0.0-0.5        | mg/L   | Final  | No       | 15:45     |
        self.page.get_by_test_id("receive-lab-results-button").click()

        # Then the system immediately flags critical values:
        #   | Test Name    | Critical Flag | Severity Level |
        #   | Troponin I   | CRITICAL HIGH | Severe         |
        #   | BNP          | CRITICAL HIGH | High           |
        assert get_text(self.page, "Troponin I Critical Flag") == "CRITICAL HIGH"
        assert get_text(self.page, "Troponin I Severity Level") == "Severe"
        assert get_text(self.page, "BNP Critical Flag") == "CRITICAL HIGH"
        assert get_text(self.page, "BNP Severity Level") == "High"

        # And popup notifications are displayed for all logged-in providers:
        assert get_text(self.page, "Critical Alert") == "🔴 CRITICAL: Troponin I = 8.5 ng/mL"
        assert get_text(self.page, "High Alert") == "🟠 HIGH: BNP = 1200 pg/mL"
        assert get_text(self.page, "Patient Info") == "Michael Davis, Bed ED-12"

        # And an immediate notification is sent to "Dr. Johnson":
        assert get_text(self.page, "Mobile Push") == "CRITICAL LAB: Troponin 8.5 - Michael Davis"
        assert get_text(self.page, "SMS Alert") == "ED-12 CRITICAL Troponin I: 8.5 ng/mL"
        assert get_text(self.page, "In-App Alert") == "High priority popup requiring acknowledgment"

        # And the charge nurse receives a critical value notification
        assert len(get_text(self.page, "Charge Nurse Notification")) > 0

        # And the results are highlighted in red on all patient displays
        critical_result_highlights = self.page.get_by_test_id("critical-result-highlight").all()
        assert len(critical_result_highlights) > 0

        # And an audit trail is created for the critical value communication
        audit_trail = self.page.get_by_test_id("critical-value-audit-trail")
        assert audit_trail.is_visible()

    def test_handle_lab_results_with_different_statuses_and_corrections(self):
        # Given a patient "Sarah Wilson" is in bed "ED-6"
        # And previous lab results were reported
        # (assumed pre-seeded test data)

        # When the lab system sends updated results via HL7 interface:
        #   | Test Name     | Result | Status     | Previous Result | Correction Reason    | Timestamp |
        #   | Hemoglobin    | 9.2    | Corrected  | 11.2           | Sample hemolysis     | 16:15     |
        #   | Glucose       | 250    | Final      | -              | -                    | 16:15     |
        #   | Pending Test  | -      | Pending    | -              | Sample reprocessing  | 16:15     |
        self.page.get_by_test_id("receive-lab-results-button").click()

        # Then the system processes different result statuses:
        assert get_text(self.page, "Corrected") == "Replace previous value, maintain history"
        assert get_text(self.page, "Final") == "Add new result to patient record"
        assert get_text(self.page, "Pending") == "Update status, maintain order tracking"

        # And correction notifications are sent:
        assert get_text(self.page, "Correction Alert") == "Lab value corrected: Hgb 11.2 → 9.2 g/dL"
        assert get_text(self.page, "Reason") == "Sample hemolysis detected"
        assert get_text(self.page, "Clinical Impact") == "Anemia now more severe than initially reported"

        # And the attending physician "Dr. Martinez" is notified of the correction
        assert len(get_text(self.page, "Physician Correction Notification")) > 0

        # And the original result is preserved in the audit trail
        original_result_audit_entry = self.page.get_by_test_id("original-result-audit-entry")
        assert original_result_audit_entry.is_visible()

        # And the corrected value triggers anemia protocol alerts
        wait_for_test_id(self.page, "Anemia Protocol Alert")

    def test_process_pediatric_lab_results_with_age_specific_reference_ranges(self):
        # Given a pediatric patient "Emma Foster" (age 6) is in bed "ED-PEDS-2"
        # And laboratory orders were placed for "CBC, CMP"
        # (assumed pre-seeded test data)

        # When the lab system sends pediatric results via HL7 interface:
        #   | Test Name          | Result | Adult Range    | Pediatric Range (Age 6) | Units  | Status |
        #   | White Blood Cells  | 12.5   | 4.0-10.0      | 5.0-14.5               | K/uL   | Final  |
        #   | Hemoglobin         | 11.8   | 12.0-16.0     | 11.5-13.5              | g/dL   | Final  |
        #   | Alkaline Phosphatase| 250   | 44-147        | 156-369                | U/L    | Final  |
        self.page.get_by_test_id("receive-lab-results-button").click()

        # Then the system applies age-appropriate reference ranges:
        #   | Test Name          | Interpretation      | Flag        |
        #   | White Blood Cells  | Normal for age 6    | Normal      |
        #   | Hemoglobin         | Normal for age 6    | Normal      |
        #   | Alkaline Phosphatase| Normal for age 6   | Normal      |
        assert get_text(self.page, "White Blood Cells Interpretation") == "Normal for age 6"
        assert get_text(self.page, "White Blood Cells Flag") == "Normal"
        assert get_text(self.page, "Hemoglobin Interpretation") == "Normal for age 6"
        assert get_text(self.page, "Hemoglobin Flag") == "Normal"
        assert get_text(self.page, "Alkaline Phosphatase Interpretation") == "Normal for age 6"
        assert get_text(self.page, "Alkaline Phosphatase Flag") == "Normal"

        # And the pediatric attending "Dr. Chen" is notified with age-specific context
        assert len(get_text(self.page, "Pediatric Attending Notification")) > 0

        # And the results display shows both adult and pediatric reference ranges
        wait_for_test_id(self.page, "Adult Reference Range")
        wait_for_test_id(self.page, "Pediatric Reference Range")

        # And no inappropriate critical alerts are generated for age-normal values
        critical_value_alerts = self.page.get_by_test_id("critical-value-alert").all()
        assert len(critical_value_alerts) == 0

    def test_handle_lab_results_during_physician_handoff(self):
        # Given a patient "Robert Kim" is in bed "ED-15"
        # And the day shift physician "Dr. Adams" ordered labs at 18:00
        # And the evening shift physician "Dr. Brown" has taken over at 19:00
        # (assumed pre-seeded test data)

        # When the lab system sends results via HL7 interface at 19:30:
        #   | Test Name     | Result | Reference Range | Status | Critical |
        #   | Lipase        | 350    | 10-140         | Final  | Yes      |
        #   | Amylase       | 180    | 25-125         | Final  | No       |
        self.page.get_by_test_id("receive-lab-results-button").click()

        # Then the system determines the appropriate physician to notify:
        #   | Notification Target | Rationale                                  |
        #   | Primary: Dr. Brown  | Current attending physician                |
        #   | Secondary: Dr. Adams| Ordered the tests, may need notification   |
        assert get_text(self.page, "Primary Notification Target") == "Dr. Brown"
        assert get_text(self.page, "Primary Notification Rationale") == "Current attending physician"
        assert get_text(self.page, "Secondary Notification Target") == "Dr. Adams"
        assert get_text(self.page, "Secondary Notification Rationale") == "Ordered the tests, may need notification"

        # And both physicians receive notifications with handoff context:
        assert get_text(self.page, "Dr. Brown Notification") == "CRITICAL: Lipase 350 - Patient from Dr. Adams"
        assert get_text(self.page, "Dr. Adams Notification") == "FYI: Your lipase order critical - Now Dr. Brown"

        # And the handoff log is updated with the critical result information
        handoff_log = self.page.get_by_test_id("handoff-log")
        assert handoff_log.is_visible()

        # And the charge nurse is notified of the critical value during shift change
        assert len(get_text(self.page, "Charge Nurse Shift Change Notification")) > 0

    def test_process_lab_results_with_technical_failures_and_retries(self):
        # Given a patient "Lisa Garcia" is in bed "ED-3"
        # And laboratory results are ready for transmission
        # (assumed pre-seeded test data)

        # When the lab system attempts to send results via HL7 interface
        # And the initial transmission fails due to network connectivity
        # And the lab system retries transmission after 5 minutes
        # (no direct UI action for these narrative steps)

        # And the retry is successful with results:
        #   | Test Name   | Result | Reference Range | Status | Timestamp |
        #   | Troponin    | 0.02   | 0.0-0.04       | Final  | 20:15     |
        self.page.get_by_test_id("receive-lab-results-button").click()

        # Then the system processes the delayed results
        assert "processed" in get_text(self.page, "Delayed Result Processing Status").lower()

        # And a delay notification is included:
        assert get_text(self.page, "Delay Notice") == "Results delayed due to technical issues"
        assert get_text(self.page, "Original Time") == "Results ready at 20:10"
        assert get_text(self.page, "Received Time") == "Results received at 20:15"

        # And the attending physician is notified of both the results and the delay
        assert len(get_text(self.page, "Physician Delay Notification")) > 0

        # And system administrators are alerted to the interface failure
        assert len(get_text(self.page, "System Administrator Alert")) > 0

        # And the delay is documented in the interface audit log
        interface_audit_log = self.page.get_by_test_id("interface-audit-log")
        assert interface_audit_log.is_visible()

    def test_handle_batch_lab_results_processing(self):
        # Given multiple patients have pending lab results:
        #   | Patient Name    | Bed    | Attending     | Tests Ordered        |
        #   | Alice Johnson   | ED-4   | Dr. Smith     | CBC, BMP             |
        #   | Bob Thompson    | ED-7   | Dr. Smith     | Liver function tests |
        #   | Carol Martinez  | ED-11  | Dr. Brown     | Cardiac enzymes      |
        # (assumed pre-seeded test data)

        # When the lab system sends batch results via HL7 interface:
        #   | Patient       | Test Results                              | Critical Values |
        #   | Alice Johnson | All normal values                         | None           |
        #   | Bob Thompson  | ALT: 150 (High), AST: 120 (High)        | None           |
        #   | Carol Martinez| Troponin: 2.1 (Critical)                | Troponin       |
        self.page.get_by_test_id("receive-lab-results-button").click()

        # Then the system processes all results simultaneously
        assert "simultaneously" in get_text(self.page, "Batch Processing Status").lower()

        # And notifications are prioritized by criticality:
        #   | Priority | Patient        | Notification Type    |
        #   | 1        | Carol Martinez | Critical value alert |
        #   | 2        | Bob Thompson   | Abnormal value alert |
        #   | 3        | Alice Johnson  | Normal results       |
        assert get_text(self.page, "Priority 1 Patient") == "Carol Martinez"
        assert get_text(self.page, "Priority 1 Notification Type") == "Critical value alert"
        assert get_text(self.page, "Priority 2 Patient") == "Bob Thompson"
        assert get_text(self.page, "Priority 2 Notification Type") == "Abnormal value alert"
        assert get_text(self.page, "Priority 3 Patient") == "Alice Johnson"
        assert get_text(self.page, "Priority 3 Notification Type") == "Normal results"

        # And physicians receive consolidated notifications when appropriate
        assert len(get_text(self.page, "Consolidated Notification")) > 0

        # And system performance metrics are maintained during batch processing
        wait_for_test_id(self.page, "System Performance Metrics")

    def test_process_lab_results_with_interpretation_comments(self):
        # Given a patient "David Lee" is in bed "ED-9"
        # And complex laboratory tests were ordered
        # (assumed pre-seeded test data)

        # When the lab system sends results with pathologist interpretation:
        #   | Test Name        | Result | Reference | Interpretation                    | Timestamp |
        #   | Blood Smear      | -      | -         | Moderate anisocytosis noted      | 21:00     |
        #   | Hemoglobin A1C   | 9.2%   | <5.7%     | Consistent with poor DM control  | 21:00     |
        #   | Thyroid Function | -      | -         | Pattern suggests hyperthyroidism | 21:00     |
        self.page.get_by_test_id("receive-lab-results-button").click()

        # Then the system includes interpretation comments in the patient record
        interpretation_comments = self.page.get_by_test_id("interpretation-comment").all()
        assert len(interpretation_comments) == 3

        # And the attending physician receives enhanced notifications:
        assert get_text(self.page, "Raw Results") == "Numeric values and reference ranges"
        assert get_text(self.page, "Interpretation") == "Pathologist comments and clinical significance"
        assert get_text(self.page, "Recommendations") == "Suggested follow-up or additional testing"

        # And interpretation comments are highlighted in the patient chart
        highlighted_interpretation_comments = self.page.get_by_test_id("highlighted-interpretation-comment").all()
        assert len(highlighted_interpretation_comments) > 0

        # And complex results are flagged for physician review and acknowledgment
        wait_for_test_id(self.page, "Physician Review Flag")
