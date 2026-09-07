"""Playwright + pytest test for tests-with-given-when-then-features/04-triage-re-assessment.feature
(equivalent to tests-with-playwright-javascript/04-triage-re-assessment.test.js).

Assumes the app exposes data-testid attributes matching each Gherkin field
label (kebab-cased, see support/fields.py) and the shared data-testid
contract in support/login.py (login-identity, login-submit, app-root).
"""

from playwright.sync_api import sync_playwright

from support.fields import fill_field, fill_fields, get_text, locator, wait_for_test_id
from support.login import login, verify_system_is_operational


class TestTriageReAssessment:
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
        #   And I am logged in as a triage nurse
        #   And the automatic reassessment alerts are enabled
        verify_system_is_operational(self.page)
        login(self.page, "a triage nurse")
        # The automatic reassessment alerts being enabled is assumed
        # pre-seeded test data / environment configuration.

        feature_nav_link = wait_for_test_id(self.page, "Nav Triage Re-assessment")
        feature_nav_link.click()
        wait_for_test_id(self.page, "Triage Re-assessment Panel")

    def test_reassess_patient_with_worsening_condition_after_2_hours(self):
        # Given a patient "Sarah Johnson" has been waiting in the queue for 2 hours
        # And the patient's initial triage was ESI Level 4 (Less Urgent)
        # And the patient's initial vital signs were:
        #   | Vital Sign          | Initial Value |
        #   | Blood Pressure      | 125/78        |
        #   | Heart Rate          | 82            |
        #   | Respiratory Rate    | 16            |
        #   | Temperature         | 99.1°F        |
        #   | Oxygen Saturation   | 98%           |
        #   | Pain Scale          | 3/10          |
        # (assumed pre-seeded test data)
        # When the system triggers a reassessment alert at the 2-hour mark
        wait_for_test_id(self.page, "Reassessment Alert")
        # And I select the patient for reassessment
        self.page.get_by_test_id("select-patient-for-reassessment-button").click()

        # And I enter the updated vital signs:
        fill_fields(
            self.page,
            [
                {"Field": "Blood Pressure", "Value": "95/55"},
                {"Field": "Heart Rate", "Value": "115"},
                {"Field": "Respiratory Rate", "Value": "24"},
                {"Field": "Temperature", "Value": "101.8°F"},
                {"Field": "Oxygen Saturation", "Value": "94%"},
                {"Field": "Pain Scale", "Value": "8/10"},
            ],
        )
        # And I update the chief complaint to "Severe abdominal pain with dizziness"
        fill_field(self.page, "Chief Complaint", "Severe abdominal pain with dizziness")
        # And I submit the reassessment
        self.page.get_by_test_id("submit-reassessment-form").click()

        # Then the system recalculates the ESI score from "4" to "2"
        assert get_text(self.page, "ESI Score") == "2"
        # And the system updates the triage level from "Less Urgent" to "High Priority"
        assert get_text(self.page, "Triage Level") == "High Priority"
        # And the patient is moved from position 12 to position 2 in the queue
        assert get_text(self.page, "Queue Position") == "2"
        # And an escalation alert is sent to the charge nurse
        assert "charge nurse" in get_text(self.page, "Escalation Alert").lower()
        # And the estimated wait time is updated from "90 minutes" to "15 minutes"
        assert get_text(self.page, "Estimated Wait Time") == "15 minutes"
        # And a reassessment note is automatically added to the patient record
        assert len(get_text(self.page, "Reassessment Note")) > 0

    def test_reassess_patient_with_stable_condition(self):
        # Given a patient "Michael Chen" has been waiting in the queue for 2 hours
        # And the patient's initial triage was ESI Level 3 (Urgent)
        # And the patient's initial vital signs were:
        #   | Vital Sign          | Initial Value |
        #   | Blood Pressure      | 140/90        |
        #   | Heart Rate          | 95            |
        #   | Respiratory Rate    | 20            |
        #   | Temperature         | 100.2°F       |
        #   | Oxygen Saturation   | 96%           |
        #   | Pain Scale          | 6/10          |
        # (assumed pre-seeded test data)
        # When I perform a scheduled reassessment
        self.page.get_by_test_id("select-patient-for-reassessment-button").click()

        # And I enter the updated vital signs:
        fill_fields(
            self.page,
            [
                {"Field": "Blood Pressure", "Value": "135/85"},
                {"Field": "Heart Rate", "Value": "88"},
                {"Field": "Respiratory Rate", "Value": "18"},
                {"Field": "Temperature", "Value": "99.8°F"},
                {"Field": "Oxygen Saturation", "Value": "97%"},
                {"Field": "Pain Scale", "Value": "5/10"},
            ],
        )
        # And I note "Patient reports feeling slightly better"
        fill_field(self.page, "Reassessment Note", "Patient reports feeling slightly better")
        # And I submit the reassessment
        self.page.get_by_test_id("submit-reassessment-form").click()

        # Then the system recalculates and maintains ESI score of "3"
        assert get_text(self.page, "ESI Score") == "3"
        # And the triage level remains "Urgent"
        assert get_text(self.page, "Triage Level") == "Urgent"
        # And the patient's queue position remains unchanged
        assert len(get_text(self.page, "Queue Position")) > 0
        # And no escalation alerts are generated
        escalation_alerts = locator(self.page, "Escalation Alert").all()
        assert len(escalation_alerts) == 0
        # And a reassessment note is added documenting stable condition
        assert get_text(self.page, "Reassessment Note") == "Patient reports feeling slightly better"
        # And the next reassessment is scheduled for 1 hour
        assert get_text(self.page, "Next Reassessment Schedule") == "1 hour"

    def test_reassess_patient_with_improving_condition(self):
        # Given a patient "Lisa Rodriguez" has been waiting in the queue for 2 hours
        # And the patient's initial triage was ESI Level 2 (High Priority)
        # And the patient's initial vital signs were:
        #   | Vital Sign          | Initial Value |
        #   | Blood Pressure      | 170/100       |
        #   | Heart Rate          | 120           |
        #   | Respiratory Rate    | 28            |
        #   | Temperature         | 98.9°F        |
        #   | Oxygen Saturation   | 92%           |
        #   | Pain Scale          | 9/10          |
        # (assumed pre-seeded test data)
        # When I perform a reassessment
        self.page.get_by_test_id("select-patient-for-reassessment-button").click()

        # And I enter the updated vital signs:
        fill_fields(
            self.page,
            [
                {"Field": "Blood Pressure", "Value": "145/85"},
                {"Field": "Heart Rate", "Value": "95"},
                {"Field": "Respiratory Rate", "Value": "20"},
                {"Field": "Temperature", "Value": "98.6°F"},
                {"Field": "Oxygen Saturation", "Value": "96%"},
                {"Field": "Pain Scale", "Value": "4/10"},
            ],
        )
        # And I note "Patient reports significant improvement after medication"
        fill_field(self.page, "Reassessment Note", "Patient reports significant improvement after medication")
        # And I submit the reassessment
        self.page.get_by_test_id("submit-reassessment-form").click()

        # Then the system recalculates the ESI score from "2" to "3"
        assert get_text(self.page, "ESI Score") == "3"
        # And the system updates the triage level from "High Priority" to "Urgent"
        assert get_text(self.page, "Triage Level") == "Urgent"
        # And the patient is moved from position 1 to position 5 in the queue
        assert get_text(self.page, "Queue Position") == "5"
        # And the charge nurse is notified of the priority change
        assert "priority change" in get_text(self.page, "Charge Nurse Notification").lower()
        # And the estimated wait time is updated from "Immediate" to "45 minutes"
        assert get_text(self.page, "Estimated Wait Time") == "45 minutes"
        # And higher priority patients are moved up in the queue
        queue_entries = self.page.get_by_test_id("triage-queue-entry").all()
        assert len(queue_entries) > 0

    def test_automatic_reassessment_alert_triggers(self):
        # Given multiple patients have been waiting for extended periods:
        #   | Patient Name     | Wait Time | Current ESI | Due for Reassessment |
        #   | John Williams    | 2 hours   | 4           | Yes                  |
        #   | Emma Thompson    | 1.5 hours | 3           | No                   |
        #   | David Kim        | 3 hours   | 3           | Yes                  |
        # (assumed pre-seeded test data)
        # When the system performs its hourly reassessment check
        self.page.get_by_test_id("trigger-hourly-reassessment-check-button").click()

        # Then reassessment alerts are generated for:
        #   | Patient Name  | Alert Type           | Reason                    |
        #   | John Williams | Standard Reassess    | 2 hours ESI Level 4       |
        #   | David Kim     | Urgent Reassess      | 3 hours ESI Level 3       |
        reassessment_alerts = self.page.get_by_test_id("reassessment-alert").all()
        assert len(reassessment_alerts) == 2
        # And the alerts appear on the triage nurse dashboard
        triage_nurse_dashboard = wait_for_test_id(self.page, "Triage Nurse Dashboard")
        assert triage_nurse_dashboard.is_visible()
        # And the patients are flagged with "Reassessment Due" status
        reassessment_due_flags = self.page.get_by_test_id("reassessment-due-flag").all()
        assert len(reassessment_due_flags) == 2
        # And Emma Thompson does not receive an alert
        alert_texts = [element.inner_text() for element in reassessment_alerts]
        assert not any("Emma Thompson" in text for text in alert_texts)

    def test_handle_patient_who_becomes_critical_during_reassessment(self):
        # Given a patient "Robert Martinez" has been waiting in the queue for 2 hours
        # And the patient's initial triage was ESI Level 3 (Urgent)
        # When I begin the reassessment process
        self.page.get_by_test_id("select-patient-for-reassessment-button").click()
        # And I observe the patient is now unresponsive

        # And I enter critical vital signs:
        fill_fields(
            self.page,
            [
                {"Field": "Blood Pressure", "Value": "60/30"},
                {"Field": "Heart Rate", "Value": "150"},
                {"Field": "Respiratory Rate", "Value": "6"},
                {"Field": "Temperature", "Value": "96.2°F"},
                {"Field": "Oxygen Saturation", "Value": "80%"},
                {"Field": "Consciousness", "Value": "Unresponsive"},
            ],
        )
        # And I submit the emergency reassessment
        self.page.get_by_test_id("submit-reassessment-form").click()

        # Then the system immediately calculates ESI score as "1"
        assert get_text(self.page, "ESI Score") == "1"
        # And the system updates triage level to "Resuscitation"
        assert get_text(self.page, "Triage Level") == "Resuscitation"
        # And the patient is moved to the top of all queues
        assert get_text(self.page, "Queue Position") == "1"
        # And a code blue alert is automatically triggered
        assert "triggered" in get_text(self.page, "Code Blue Alert").lower()
        # And the rapid response team is notified immediately
        assert "notified" in get_text(self.page, "Rapid Response Notification").lower()
        # And the patient is flagged for immediate intervention
        assert "immediate intervention" in get_text(self.page, "Intervention Flag").lower()
        # And I am prompted to initiate emergency protocols
        wait_for_test_id(self.page, "Emergency Protocol Prompt")

    def test_reassess_pediatric_patient_with_different_parameters(self):
        # Given a pediatric patient "Amy Foster" (age 8) has been waiting for 2 hours
        # And the patient's initial triage was ESI Level 3 (Urgent)
        # When I perform a pediatric reassessment
        self.page.get_by_test_id("select-patient-for-reassessment-button").click()

        # And I enter updated vital signs using age-appropriate parameters:
        fill_fields(
            self.page,
            [
                {"Field": "Blood Pressure", "Value": "85/50"},
                {"Field": "Heart Rate", "Value": "140"},
                {"Field": "Respiratory Rate", "Value": "32"},
                {"Field": "Temperature", "Value": "103.8°F"},
                {"Field": "Oxygen Saturation", "Value": "93%"},
                {"Field": "Pain Scale (FACES)", "Value": "8/10"},
            ],
        )
        # And I note "Child appears more lethargic than initial assessment"
        fill_field(self.page, "Reassessment Note", "Child appears more lethargic than initial assessment")
        # And I submit the pediatric reassessment
        self.page.get_by_test_id("submit-reassessment-form").click()

        # Then the system recalculates using pediatric ESI criteria
        assert "pediatric" in get_text(self.page, "Scoring Criteria").lower()
        # And the ESI score is updated from "3" to "2"
        assert get_text(self.page, "ESI Score") == "2"
        # And the triage level is updated to "High Priority"
        assert get_text(self.page, "Triage Level") == "High Priority"
        # And the pediatric emergency team is notified
        assert "notified" in get_text(self.page, "Pediatric Team Notification").lower()
        # And the patient is moved to the pediatric high priority queue
        assert "pediatric high priority" in get_text(self.page, "Assigned Queue").lower()
        # And parent/guardian notification protocols are initiated
        assert "initiated" in get_text(self.page, "Guardian Notification").lower()

    def test_document_reassessment_with_no_vital_sign_changes(self):
        # Given a patient "Catherine Lee" has been waiting for 2 hours
        # And a reassessment is due
        # When I perform the reassessment
        self.page.get_by_test_id("select-patient-for-reassessment-button").click()
        # And the vital signs remain identical to the initial assessment

        # But I note "Patient reports increased anxiety about wait time"
        fill_field(self.page, "Reassessment Note", "Patient reports increased anxiety about wait time")
        # And I provide reassurance and update on expected wait time
        # And I submit the reassessment
        self.page.get_by_test_id("submit-reassessment-form").click()

        # Then the ESI score and triage level remain unchanged
        assert len(get_text(self.page, "ESI Score")) > 0
        assert len(get_text(self.page, "Triage Level")) > 0
        # And the queue position is maintained
        assert len(get_text(self.page, "Queue Position")) > 0
        # And a documentation note is added about patient anxiety
        assert "anxiety" in get_text(self.page, "Reassessment Note").lower()
        # And comfort measures are suggested in the patient instructions
        assert "comfort" in get_text(self.page, "Patient Instructions").lower()
        # And the next reassessment interval is maintained
        assert len(get_text(self.page, "Next Reassessment Schedule")) > 0

    def test_handle_reassessment_during_shift_change(self):
        # Given a patient "Thomas Wilson" is due for reassessment
        # And the day shift triage nurse is preparing to leave
        # And the night shift triage nurse is arriving
        # When the day shift nurse initiates the reassessment handoff
        self.page.get_by_test_id("initiate-reassessment-handoff-button").click()
        # And transfers the patient assessment to the night shift nurse
        self.page.get_by_test_id("transfer-assessment-button").click()

        # Then the reassessment timing is preserved
        assert len(get_text(self.page, "Reassessment Timing")) > 0
        # And all previous assessment data remains accessible
        previous_assessment_data = wait_for_test_id(self.page, "Previous Assessment Data")
        assert previous_assessment_data.is_visible()
        # And the night shift nurse can complete the reassessment
        reassessment_form = self.page.get_by_test_id("reassessment-form")
        assert reassessment_form.is_visible()
        # And continuity of care documentation is maintained
        assert len(get_text(self.page, "Continuity Documentation")) > 0
        # And the handoff is logged in the system audit trail
        audit_trail_entries = self.page.get_by_test_id("audit-trail-entry").all()
        assert len(audit_trail_entries) > 0
