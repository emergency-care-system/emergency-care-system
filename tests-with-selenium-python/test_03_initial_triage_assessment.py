"""Selenium WebDriver + pytest test for
tests-with-given-when-then-features/03-initial-triage-assessment.feature
(equivalent to tests-with-selenium-javascript/03-initial-triage-assessment.test.js).

Assumes the app exposes data-testid attributes matching each Gherkin field
label (kebab-cased, see support/fields.py) and the shared data-testid
contract in support/login.py (login-identity, login-submit, app-root).
"""

from selenium.webdriver.common.by import By

from support.build_driver import build_driver
from support.fields import fill_field, fill_fields, get_text, locator, wait_for_test_id
from support.login import login, verify_system_is_operational


class TestInitialTriageAssessment:
    @classmethod
    def setup_class(cls):
        cls.driver = build_driver()

    @classmethod
    def teardown_class(cls):
        cls.driver.quit()

    def setup_method(self):
        # Background:
        #   Given the emergency care system is operational
        #   And I am logged in as a triage nurse
        #   And the ESI (Emergency Severity Index) scoring module is active
        verify_system_is_operational(self.driver)
        login(self.driver, "a triage nurse")
        # The ESI scoring module being active is assumed pre-seeded test data /
        # environment configuration.

        feature_nav_link = wait_for_test_id(self.driver, "Nav Initial Triage Assessment")
        feature_nav_link.click()
        wait_for_test_id(self.driver, "Initial Triage Assessment Panel")

    def test_assess_patient_with_chest_pain_esi_level_2(self):
        # Given a registered patient "John Doe" is waiting for triage
        # And the patient was registered 10 minutes ago
        # (assumed pre-seeded test data)
        # When I select the patient for triage assessment
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="select-patient-for-triage-button"]').click()

        # And I enter the vital signs:
        fill_fields(
            self.driver,
            [
                {"Field": "Blood Pressure", "Value": "160/95"},
                {"Field": "Heart Rate", "Value": "110"},
                {"Field": "Respiratory Rate", "Value": "22"},
                {"Field": "Temperature", "Value": "98.6°F"},
                {"Field": "Oxygen Saturation", "Value": "94%"},
            ],
        )
        # And I enter the chief complaint as "Chest pain and shortness of breath"
        fill_field(self.driver, "Chief Complaint", "Chest pain and shortness of breath")
        # And I enter the pain scale as "8/10"
        fill_field(self.driver, "Pain Scale", "8/10")
        # And I document onset as "Started 2 hours ago"
        fill_field(self.driver, "Onset", "Started 2 hours ago")
        # And I submit the triage assessment
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-triage-assessment-form"]').click()

        # Then the system calculates an ESI score of "2"
        esi_score = get_text(self.driver, "ESI Score")
        assert esi_score == "2"
        # And the system assigns triage level "High Priority"
        triage_level = get_text(self.driver, "Triage Level")
        assert triage_level == "High Priority"
        # And the patient is positioned at the front of the high priority queue
        queue_position = get_text(self.driver, "Queue Position")
        assert queue_position == "1"
        # And an alert is sent to the attending physician
        physician_alert = get_text(self.driver, "Physician Alert")
        assert "attending physician" in physician_alert.lower()
        # And the estimated wait time is updated to "Immediate"
        estimated_wait_time = get_text(self.driver, "Estimated Wait Time")
        assert estimated_wait_time == "Immediate"

    def test_assess_patient_with_minor_injury_esi_level_4(self):
        # Given a registered patient "Jane Smith" is waiting for triage
        # When I select the patient for triage assessment
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="select-patient-for-triage-button"]').click()

        # And I enter the vital signs:
        fill_fields(
            self.driver,
            [
                {"Field": "Blood Pressure", "Value": "120/80"},
                {"Field": "Heart Rate", "Value": "75"},
                {"Field": "Respiratory Rate", "Value": "16"},
                {"Field": "Temperature", "Value": "98.2°F"},
                {"Field": "Oxygen Saturation", "Value": "99%"},
            ],
        )
        # And I enter the chief complaint as "Sprained ankle from fall"
        fill_field(self.driver, "Chief Complaint", "Sprained ankle from fall")
        # And I enter the pain scale as "4/10"
        fill_field(self.driver, "Pain Scale", "4/10")
        # And I document onset as "This morning while jogging"
        fill_field(self.driver, "Onset", "This morning while jogging")
        # And I submit the triage assessment
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-triage-assessment-form"]').click()

        # Then the system calculates an ESI score of "4"
        esi_score = get_text(self.driver, "ESI Score")
        assert esi_score == "4"
        # And the system assigns triage level "Less Urgent"
        triage_level = get_text(self.driver, "Triage Level")
        assert triage_level == "Less Urgent"
        # And the patient is positioned in the less urgent queue
        assigned_queue = get_text(self.driver, "Assigned Queue")
        assert "less urgent" in assigned_queue.lower()
        # And the estimated wait time is updated to "60-90 minutes"
        estimated_wait_time = get_text(self.driver, "Estimated Wait Time")
        assert estimated_wait_time == "60-90 minutes"
        # And no immediate alerts are generated
        physician_alerts = self.driver.find_elements(*locator("Physician Alert"))
        assert len(physician_alerts) == 0

    def test_assess_critical_patient_requiring_immediate_attention_esi_level_1(self):
        # Given a registered patient "Emergency Patient" is waiting for triage
        # When I select the patient for triage assessment
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="select-patient-for-triage-button"]').click()

        # And I enter the vital signs:
        fill_fields(
            self.driver,
            [
                {"Field": "Blood Pressure", "Value": "70/40"},
                {"Field": "Heart Rate", "Value": "140"},
                {"Field": "Respiratory Rate", "Value": "8"},
                {"Field": "Temperature", "Value": "95.0°F"},
                {"Field": "Oxygen Saturation", "Value": "85%"},
            ],
        )
        # And I enter the chief complaint as "Unresponsive after motor vehicle accident"
        fill_field(self.driver, "Chief Complaint", "Unresponsive after motor vehicle accident")
        # And I enter the pain scale as "Unable to assess"
        fill_field(self.driver, "Pain Scale", "Unable to assess")
        # And I mark the patient as "Requires immediate life-saving intervention"
        fill_field(self.driver, "Intervention Flag", "Requires immediate life-saving intervention")
        # And I submit the triage assessment
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-triage-assessment-form"]').click()

        # Then the system calculates an ESI score of "1"
        esi_score = get_text(self.driver, "ESI Score")
        assert esi_score == "1"
        # And the system assigns triage level "Resuscitation"
        triage_level = get_text(self.driver, "Triage Level")
        assert triage_level == "Resuscitation"
        # And the patient is moved to the top of all queues
        queue_position = get_text(self.driver, "Queue Position")
        assert queue_position == "1"
        # And a code alert is automatically triggered
        code_alert = get_text(self.driver, "Code Alert")
        assert "triggered" in code_alert.lower()
        # And the trauma team is notified immediately
        trauma_team_notification = get_text(self.driver, "Trauma Team Notification")
        assert "notified" in trauma_team_notification.lower()
        # And the estimated wait time shows "Immediate - In Progress"
        estimated_wait_time = get_text(self.driver, "Estimated Wait Time")
        assert estimated_wait_time == "Immediate - In Progress"

    def test_assess_pediatric_patient_with_fever_esi_level_3(self):
        # Given a registered patient "Tommy Jones" (age 5) is waiting for triage
        # When I select the patient for triage assessment
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="select-patient-for-triage-button"]').click()

        # And I enter the vital signs using pediatric parameters:
        fill_fields(
            self.driver,
            [
                {"Field": "Blood Pressure", "Value": "95/60"},
                {"Field": "Heart Rate", "Value": "120"},
                {"Field": "Respiratory Rate", "Value": "24"},
                {"Field": "Temperature", "Value": "103.2°F"},
                {"Field": "Oxygen Saturation", "Value": "97%"},
            ],
        )
        # And I enter the chief complaint as "High fever and irritability"
        fill_field(self.driver, "Chief Complaint", "High fever and irritability")
        # And I enter the pain scale as "6/10 (using FACES scale)"
        fill_field(self.driver, "Pain Scale", "6/10 (using FACES scale)")
        # And I document onset as "Fever started yesterday evening"
        fill_field(self.driver, "Onset", "Fever started yesterday evening")
        # And I submit the triage assessment
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-triage-assessment-form"]').click()

        # Then the system calculates an ESI score of "3" using pediatric criteria
        esi_score = get_text(self.driver, "ESI Score")
        assert esi_score == "3"
        scoring_criteria = get_text(self.driver, "Scoring Criteria")
        assert "pediatric" in scoring_criteria.lower()
        # And the system assigns triage level "Urgent"
        triage_level = get_text(self.driver, "Triage Level")
        assert triage_level == "Urgent"
        # And the patient is positioned in the urgent pediatric queue
        assigned_queue = get_text(self.driver, "Assigned Queue")
        assert "urgent pediatric" in assigned_queue.lower()
        # And the pediatric team is notified
        pediatric_team_notification = get_text(self.driver, "Pediatric Team Notification")
        assert "notified" in pediatric_team_notification.lower()
        # And the estimated wait time is updated to "30-45 minutes"
        estimated_wait_time = get_text(self.driver, "Estimated Wait Time")
        assert estimated_wait_time == "30-45 minutes"

    def test_handle_incomplete_vital_signs_during_triage(self):
        # Given a registered patient "Mary Johnson" is waiting for triage
        # When I select the patient for triage assessment
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="select-patient-for-triage-button"]').click()

        # And I attempt to enter incomplete vital signs:
        #   | Vital Sign          | Value    |
        #   | Blood Pressure      | 130/85   |
        #   | Heart Rate          |          |
        #   | Respiratory Rate    | 18       |
        #   | Temperature         |          |
        #   | Oxygen Saturation   | 98%      |
        fill_fields(
            self.driver,
            [
                {"Field": "Blood Pressure", "Value": "130/85"},
                {"Field": "Heart Rate", "Value": ""},
                {"Field": "Respiratory Rate", "Value": "18"},
                {"Field": "Temperature", "Value": ""},
                {"Field": "Oxygen Saturation", "Value": "98%"},
            ],
        )
        # And I enter the chief complaint as "Headache"
        fill_field(self.driver, "Chief Complaint", "Headache")
        # And I submit the triage assessment
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-triage-assessment-form"]').click()

        # Then the system displays validation errors:
        #   | Missing Field       | Error Message                |
        #   | Heart Rate          | Heart rate is required       |
        #   | Temperature         | Temperature is required      |
        assert get_text(self.driver, "Heart Rate Error") == "Heart rate is required"
        assert get_text(self.driver, "Temperature Error") == "Temperature is required"
        # And the ESI score cannot be calculated
        esi_score_elements = self.driver.find_elements(*locator("ESI Score"))
        assert len(esi_score_elements) == 0
        # And the assessment remains incomplete
        assessment_status = get_text(self.driver, "Assessment Status")
        assert assessment_status == "Incomplete"
        # And I must complete all required fields before proceeding
        triage_form = self.driver.find_element(By.CSS_SELECTOR, '[data-testid="triage-assessment-form"]')
        assert triage_form.is_displayed()

    def test_reassess_patient_with_worsening_condition(self):
        # Given a patient "Robert Davis" has been triaged as ESI Level 4
        # And the patient has been waiting for 90 minutes
        # When I select the patient for reassessment
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="select-patient-for-reassessment-button"]').click()

        # And I enter updated vital signs:
        fill_fields(
            self.driver,
            [
                {"Field": "Blood Pressure", "Value": "90/50"},
                {"Field": "Heart Rate", "Value": "120"},
                {"Field": "Respiratory Rate", "Value": "26"},
                {"Field": "Temperature", "Value": "101.5°F"},
                {"Field": "Oxygen Saturation", "Value": "92%"},
            ],
        )
        # And I update the chief complaint to "Worsening abdominal pain with nausea"
        fill_field(self.driver, "Chief Complaint", "Worsening abdominal pain with nausea")
        # And I enter the updated pain scale as "9/10"
        fill_field(self.driver, "Pain Scale", "9/10")
        # And I submit the reassessment
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-reassessment-form"]').click()

        # Then the system recalculates the ESI score to "2"
        esi_score = get_text(self.driver, "ESI Score")
        assert esi_score == "2"
        # And the system updates triage level to "High Priority"
        triage_level = get_text(self.driver, "Triage Level")
        assert triage_level == "High Priority"
        # And the patient is moved to the front of the high priority queue
        queue_position = get_text(self.driver, "Queue Position")
        assert queue_position == "1"
        # And an escalation alert is sent to the charge nurse
        escalation_alert = get_text(self.driver, "Escalation Alert")
        assert "charge nurse" in escalation_alert.lower()
        # And a note is added documenting the condition change
        condition_change_note = get_text(self.driver, "Condition Change Note")
        assert len(condition_change_note) > 0

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
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="complete-all-triage-assessments-button"]').click()

        # Then the system positions patients in queue order:
        #   | Queue Position | Patient Name | Triage Level  |
        #   | 1              | Carol Davis  | High Priority |
        #   | 2              | Alice Brown  | Urgent        |
        #   | 3              | Bob Wilson   | Less Urgent   |
        queue_entries = self.driver.find_elements(By.CSS_SELECTOR, '[data-testid="triage-queue-entry"]')
        assert len(queue_entries) == 3
        first_queue_entry_text = queue_entries[0].text
        assert "Carol Davis" in first_queue_entry_text
        # And wait times are calculated based on queue position and available resources
        wait_time_calculation_status = get_text(self.driver, "Wait Time Calculation Status")
        assert "calculated" in wait_time_calculation_status.lower()
        # And the triage dashboard is updated with current queue status
        triage_dashboard = wait_for_test_id(self.driver, "Triage Dashboard")
        assert triage_dashboard.is_displayed()
