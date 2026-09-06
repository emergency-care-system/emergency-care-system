"""Playwright + pytest test for spec/features/03-initial-triage-assessment.feature
(equivalent to tests-with-playwright-javascript/03-initial-triage-assessment.test.js).

Assumes the app exposes data-testid attributes matching each Gherkin field
label (kebab-cased, see support/fields.py) and the shared data-testid
contract in support/login.py (login-identity, login-submit, app-root).
"""

from playwright.sync_api import sync_playwright

from support.fields import fill_field, fill_fields, get_text, locator, wait_for_test_id
from support.login import login, verify_system_is_operational


class TestInitialTriageAssessment:
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
        #   And the ESI (Emergency Severity Index) scoring module is active
        verify_system_is_operational(self.page)
        login(self.page, "a triage nurse")
        # The ESI scoring module being active is assumed pre-seeded test
        # data / environment configuration.

        feature_nav_link = wait_for_test_id(self.page, "Nav Initial Triage Assessment")
        feature_nav_link.click()
        wait_for_test_id(self.page, "Initial Triage Assessment Panel")

    def test_assess_patient_with_chest_pain_esi_level_2(self):
        # Given a registered patient "John Doe" is waiting for triage
        # And the patient was registered 10 minutes ago
        # (assumed pre-seeded test data)
        # When I select the patient for triage assessment
        self.page.get_by_test_id("select-patient-for-triage-button").click()

        # And I enter the vital signs:
        fill_fields(
            self.page,
            [
                {"Field": "Blood Pressure", "Value": "160/95"},
                {"Field": "Heart Rate", "Value": "110"},
                {"Field": "Respiratory Rate", "Value": "22"},
                {"Field": "Temperature", "Value": "98.6°F"},
                {"Field": "Oxygen Saturation", "Value": "94%"},
            ],
        )
        # And I enter the chief complaint as "Chest pain and shortness of breath"
        fill_field(self.page, "Chief Complaint", "Chest pain and shortness of breath")
        # And I enter the pain scale as "8/10"
        fill_field(self.page, "Pain Scale", "8/10")
        # And I document onset as "Started 2 hours ago"
        fill_field(self.page, "Onset", "Started 2 hours ago")
        # And I submit the triage assessment
        self.page.get_by_test_id("submit-triage-assessment-form").click()

        # Then the system calculates an ESI score of "2"
        assert get_text(self.page, "ESI Score") == "2"
        # And the system assigns triage level "High Priority"
        assert get_text(self.page, "Triage Level") == "High Priority"
        # And the patient is positioned at the front of the high priority queue
        assert get_text(self.page, "Queue Position") == "1"
        # And an alert is sent to the attending physician
        assert "attending physician" in get_text(self.page, "Physician Alert").lower()
        # And the estimated wait time is updated to "Immediate"
        assert get_text(self.page, "Estimated Wait Time") == "Immediate"

    def test_assess_patient_with_minor_injury_esi_level_4(self):
        # Given a registered patient "Jane Smith" is waiting for triage
        # When I select the patient for triage assessment
        self.page.get_by_test_id("select-patient-for-triage-button").click()

        # And I enter the vital signs:
        fill_fields(
            self.page,
            [
                {"Field": "Blood Pressure", "Value": "120/80"},
                {"Field": "Heart Rate", "Value": "75"},
                {"Field": "Respiratory Rate", "Value": "16"},
                {"Field": "Temperature", "Value": "98.2°F"},
                {"Field": "Oxygen Saturation", "Value": "99%"},
            ],
        )
        # And I enter the chief complaint as "Sprained ankle from fall"
        fill_field(self.page, "Chief Complaint", "Sprained ankle from fall")
        # And I enter the pain scale as "4/10"
        fill_field(self.page, "Pain Scale", "4/10")
        # And I document onset as "This morning while jogging"
        fill_field(self.page, "Onset", "This morning while jogging")
        # And I submit the triage assessment
        self.page.get_by_test_id("submit-triage-assessment-form").click()

        # Then the system calculates an ESI score of "4"
        assert get_text(self.page, "ESI Score") == "4"
        # And the system assigns triage level "Less Urgent"
        assert get_text(self.page, "Triage Level") == "Less Urgent"
        # And the patient is positioned in the less urgent queue
        assert "less urgent" in get_text(self.page, "Assigned Queue").lower()
        # And the estimated wait time is updated to "60-90 minutes"
        assert get_text(self.page, "Estimated Wait Time") == "60-90 minutes"
        # And no immediate alerts are generated
        physician_alerts = locator(self.page, "Physician Alert").all()
        assert len(physician_alerts) == 0

    def test_assess_critical_patient_requiring_immediate_attention_esi_level_1(self):
        # Given a registered patient "Emergency Patient" is waiting for triage
        # When I select the patient for triage assessment
        self.page.get_by_test_id("select-patient-for-triage-button").click()

        # And I enter the vital signs:
        fill_fields(
            self.page,
            [
                {"Field": "Blood Pressure", "Value": "70/40"},
                {"Field": "Heart Rate", "Value": "140"},
                {"Field": "Respiratory Rate", "Value": "8"},
                {"Field": "Temperature", "Value": "95.0°F"},
                {"Field": "Oxygen Saturation", "Value": "85%"},
            ],
        )
        # And I enter the chief complaint as "Unresponsive after motor vehicle accident"
        fill_field(self.page, "Chief Complaint", "Unresponsive after motor vehicle accident")
        # And I enter the pain scale as "Unable to assess"
        fill_field(self.page, "Pain Scale", "Unable to assess")
        # And I mark the patient as "Requires immediate life-saving intervention"
        fill_field(self.page, "Intervention Flag", "Requires immediate life-saving intervention")
        # And I submit the triage assessment
        self.page.get_by_test_id("submit-triage-assessment-form").click()

        # Then the system calculates an ESI score of "1"
        assert get_text(self.page, "ESI Score") == "1"
        # And the system assigns triage level "Resuscitation"
        assert get_text(self.page, "Triage Level") == "Resuscitation"
        # And the patient is moved to the top of all queues
        assert get_text(self.page, "Queue Position") == "1"
        # And a code alert is automatically triggered
        assert "triggered" in get_text(self.page, "Code Alert").lower()
        # And the trauma team is notified immediately
        assert "notified" in get_text(self.page, "Trauma Team Notification").lower()
        # And the estimated wait time shows "Immediate - In Progress"
        assert get_text(self.page, "Estimated Wait Time") == "Immediate - In Progress"

    def test_assess_pediatric_patient_with_fever_esi_level_3(self):
        # Given a registered patient "Tommy Jones" (age 5) is waiting for triage
        # When I select the patient for triage assessment
        self.page.get_by_test_id("select-patient-for-triage-button").click()

        # And I enter the vital signs using pediatric parameters:
        fill_fields(
            self.page,
            [
                {"Field": "Blood Pressure", "Value": "95/60"},
                {"Field": "Heart Rate", "Value": "120"},
                {"Field": "Respiratory Rate", "Value": "24"},
                {"Field": "Temperature", "Value": "103.2°F"},
                {"Field": "Oxygen Saturation", "Value": "97%"},
            ],
        )
        # And I enter the chief complaint as "High fever and irritability"
        fill_field(self.page, "Chief Complaint", "High fever and irritability")
        # And I enter the pain scale as "6/10 (using FACES scale)"
        fill_field(self.page, "Pain Scale", "6/10 (using FACES scale)")
        # And I document onset as "Fever started yesterday evening"
        fill_field(self.page, "Onset", "Fever started yesterday evening")
        # And I submit the triage assessment
        self.page.get_by_test_id("submit-triage-assessment-form").click()

        # Then the system calculates an ESI score of "3" using pediatric criteria
        assert get_text(self.page, "ESI Score") == "3"
        assert "pediatric" in get_text(self.page, "Scoring Criteria").lower()
        # And the system assigns triage level "Urgent"
        assert get_text(self.page, "Triage Level") == "Urgent"
        # And the patient is positioned in the urgent pediatric queue
        assert "urgent pediatric" in get_text(self.page, "Assigned Queue").lower()
        # And the pediatric team is notified
        assert "notified" in get_text(self.page, "Pediatric Team Notification").lower()
        # And the estimated wait time is updated to "30-45 minutes"
        assert get_text(self.page, "Estimated Wait Time") == "30-45 minutes"

    def test_handle_incomplete_vital_signs_during_triage(self):
        # Given a registered patient "Mary Johnson" is waiting for triage
        # When I select the patient for triage assessment
        self.page.get_by_test_id("select-patient-for-triage-button").click()

        # And I attempt to enter incomplete vital signs:
        #   | Vital Sign          | Value    |
        #   | Blood Pressure      | 130/85   |
        #   | Heart Rate          |          |
        #   | Respiratory Rate    | 18       |
        #   | Temperature         |          |
        #   | Oxygen Saturation   | 98%      |
        fill_fields(
            self.page,
            [
                {"Field": "Blood Pressure", "Value": "130/85"},
                {"Field": "Heart Rate", "Value": ""},
                {"Field": "Respiratory Rate", "Value": "18"},
                {"Field": "Temperature", "Value": ""},
                {"Field": "Oxygen Saturation", "Value": "98%"},
            ],
        )
        # And I enter the chief complaint as "Headache"
        fill_field(self.page, "Chief Complaint", "Headache")
        # And I submit the triage assessment
        self.page.get_by_test_id("submit-triage-assessment-form").click()

        # Then the system displays validation errors:
        #   | Missing Field       | Error Message                |
        #   | Heart Rate          | Heart rate is required       |
        #   | Temperature         | Temperature is required      |
        assert get_text(self.page, "Heart Rate Error") == "Heart rate is required"
        assert get_text(self.page, "Temperature Error") == "Temperature is required"
        # And the ESI score cannot be calculated
        esi_score_elements = locator(self.page, "ESI Score").all()
        assert len(esi_score_elements) == 0
        # And the assessment remains incomplete
        assert get_text(self.page, "Assessment Status") == "Incomplete"
        # And I must complete all required fields before proceeding
        triage_form = self.page.get_by_test_id("triage-assessment-form")
        assert triage_form.is_visible()

    def test_reassess_patient_with_worsening_condition(self):
        # Given a patient "Robert Davis" has been triaged as ESI Level 4
        # And the patient has been waiting for 90 minutes
        # When I select the patient for reassessment
        self.page.get_by_test_id("select-patient-for-reassessment-button").click()

        # And I enter updated vital signs:
        fill_fields(
            self.page,
            [
                {"Field": "Blood Pressure", "Value": "90/50"},
                {"Field": "Heart Rate", "Value": "120"},
                {"Field": "Respiratory Rate", "Value": "26"},
                {"Field": "Temperature", "Value": "101.5°F"},
                {"Field": "Oxygen Saturation", "Value": "92%"},
            ],
        )
        # And I update the chief complaint to "Worsening abdominal pain with nausea"
        fill_field(self.page, "Chief Complaint", "Worsening abdominal pain with nausea")
        # And I enter the updated pain scale as "9/10"
        fill_field(self.page, "Pain Scale", "9/10")
        # And I submit the reassessment
        self.page.get_by_test_id("submit-reassessment-form").click()

        # Then the system recalculates the ESI score to "2"
        assert get_text(self.page, "ESI Score") == "2"
        # And the system updates triage level to "High Priority"
        assert get_text(self.page, "Triage Level") == "High Priority"
        # And the patient is moved to the front of the high priority queue
        assert get_text(self.page, "Queue Position") == "1"
        # And an escalation alert is sent to the charge nurse
        assert "charge nurse" in get_text(self.page, "Escalation Alert").lower()
        # And a note is added documenting the condition change
        assert len(get_text(self.page, "Condition Change Note")) > 0

    def test_process_multiple_patients_in_triage_queue(self):
        # Given multiple patients are waiting for triage:
        #   | Patient Name    | Registration Time | Status        |
        #   | Alice Brown     | 10:00 AM           | Waiting       |
        #   | Bob Wilson      | 10:15 AM           | Waiting       |
        #   | Carol Davis     | 10:30 AM           | Waiting       |
        # (assumed pre-seeded test data)
        # When I complete triage assessments for all patients:
        #   | Patient Name | ESI Score | Triage Level  |
        #   | Alice Brown  | 3         | Urgent        |
        #   | Bob Wilson   | 4         | Less Urgent   |
        #   | Carol Davis  | 2         | High Priority |
        self.page.get_by_test_id("complete-all-triage-assessments-button").click()

        # Then the system positions patients in queue order:
        #   | Queue Position | Patient Name | Triage Level  |
        #   | 1              | Carol Davis  | High Priority |
        #   | 2              | Alice Brown  | Urgent        |
        #   | 3              | Bob Wilson   | Less Urgent   |
        queue_entries = self.page.get_by_test_id("triage-queue-entry").all()
        assert len(queue_entries) == 3
        first_queue_entry_text = queue_entries[0].inner_text()
        assert "Carol Davis" in first_queue_entry_text
        # And wait times are calculated based on queue position and available resources
        assert "calculated" in get_text(self.page, "Wait Time Calculation Status").lower()
        # And the triage dashboard is updated with current queue status
        triage_dashboard = wait_for_test_id(self.page, "Triage Dashboard")
        assert triage_dashboard.is_visible()
