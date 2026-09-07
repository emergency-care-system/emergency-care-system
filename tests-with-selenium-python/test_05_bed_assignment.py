"""Selenium WebDriver + pytest test for
tests-with-given-when-then-features/05-bed-assignment.feature
(equivalent to tests-with-selenium-javascript/05-bed-assignment.test.js).

Assumes the app exposes data-testid attributes matching each Gherkin field
label (kebab-cased, see support/fields.py) and the shared data-testid
contract in support/login.py (login-identity, login-submit, app-root).
"""

from selenium.webdriver.common.by import By

from support.build_driver import build_driver
from support.fields import get_text, wait_for_test_id
from support.login import login, verify_system_is_operational


class TestBedAssignment:
    @classmethod
    def setup_class(cls):
        cls.driver = build_driver()

    @classmethod
    def teardown_class(cls):
        cls.driver.quit()

    def setup_method(self):
        # Background:
        #   Given the emergency care system is operational
        #   And I am logged in as a charge nurse
        #   And the bed management module is active
        #   And the patient prioritization algorithm is enabled
        verify_system_is_operational(self.driver)
        login(self.driver, "a charge nurse")
        # The bed management module and patient prioritization algorithm are
        # assumed to be pre-seeded/enabled test data.

        feature_nav_link = wait_for_test_id(self.driver, "Nav Bed Assignment")
        feature_nav_link.click()
        wait_for_test_id(self.driver, "Bed Assignment Panel")

    def test_assign_standard_room_to_highest_acuity_patient(self):
        # Given multiple patients are waiting for beds:
        # And a standard room "ED-12" becomes available
        # (assumed pre-seeded test data)

        # When I request bed assignment recommendations
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="request-recommendations-button"]').click()

        # Then the system suggests "Maria Gonzalez" as the top recommendation
        wait_for_test_id(self.driver, "Top Recommendation")
        top_recommendation = get_text(self.driver, "Top Recommendation")
        assert top_recommendation == "Maria Gonzalez"

        # And the recommendation shows:
        recommendation_fields = [
            {"Field": "Recommended Patient", "Value": "Maria Gonzalez"},
            {"Field": "ESI Level", "Value": "2"},
            {"Field": "Wait Time", "Value": "45 minutes"},
            {"Field": "Room Match", "Value": "Standard room suitable"},
            {"Field": "Rationale", "Value": "Highest acuity patient requiring standard bed"},
        ]
        for row in recommendation_fields:
            assert get_text(self.driver, row["Field"]) == row["Value"]

        # And the system displays updated wait times for remaining patients:
        wait_time_updates = self.driver.find_elements(By.CSS_SELECTOR, '[data-testid="wait-time-update-row"]')
        assert len(wait_time_updates) == 3

    def test_assign_isolation_room_based_on_patient_requirements(self):
        # Given multiple patients are waiting for beds:
        # And an isolation room "ED-ISO-2" becomes available
        # (assumed pre-seeded test data)

        # When I request bed assignment recommendations
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="request-recommendations-button"]').click()

        # Then the system suggests "Alex Johnson" as the top recommendation
        wait_for_test_id(self.driver, "Top Recommendation")
        top_recommendation = get_text(self.driver, "Top Recommendation")
        assert top_recommendation == "Alex Johnson"

        # And the recommendation shows:
        recommendation_fields = [
            {"Field": "Recommended Patient", "Value": "Alex Johnson"},
            {"Field": "Room Type", "Value": "Isolation room"},
            {"Field": "Rationale", "Value": "Patient requires isolation precautions"},
            {"Field": "Infection Control", "Value": "Airborne precautions needed"},
        ]
        for row in recommendation_fields:
            assert get_text(self.driver, row["Field"]) == row["Value"]

        # And the system displays that other patients cannot use this room type
        room_type_restriction = get_text(self.driver, "Room Type Restriction Notice")
        assert len(room_type_restriction) > 0

        # And the estimated wait times for standard rooms remain unchanged for other patients
        standard_room_wait_time_change = get_text(self.driver, "Standard Room Wait Time Change")
        assert standard_room_wait_time_change == "Unchanged"

    def test_handle_pediatric_room_assignment(self):
        # Given multiple patients are waiting for beds:
        # And a pediatric room "ED-PEDS-3" becomes available
        # (assumed pre-seeded test data)

        # When I request bed assignment recommendations
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="request-recommendations-button"]').click()

        # Then the system suggests "Sarah Mitchell" as the top recommendation
        wait_for_test_id(self.driver, "Top Recommendation")
        top_recommendation = get_text(self.driver, "Top Recommendation")
        assert top_recommendation == "Sarah Mitchell"

        # And the recommendation shows:
        recommendation_fields = [
            {"Field": "Recommended Patient", "Value": "Sarah Mitchell"},
            {"Field": "Age", "Value": "4 years old"},
            {"Field": "ESI Level", "Value": "2"},
            {"Field": "Room Type", "Value": "Pediatric room"},
            {"Field": "Rationale", "Value": "Highest acuity pediatric patient"},
        ]
        for row in recommendation_fields:
            assert get_text(self.driver, row["Field"]) == row["Value"]

        # And the system displays updated wait times for remaining pediatric patients:
        pediatric_wait_time_updates = self.driver.find_elements(By.CSS_SELECTOR, '[data-testid="wait-time-update-row"]')
        assert len(pediatric_wait_time_updates) == 2

        # And David Chen remains in the adult standard room queue
        david_chen_queue_status = get_text(self.driver, "David Chen Queue Status")
        assert "adult standard room queue" in david_chen_queue_status.lower()

    def test_no_suitable_patients_for_available_room_type(self):
        # Given multiple patients are waiting for beds:
        # And a trauma room "ED-TRAUMA-1" becomes available
        # (assumed pre-seeded test data)

        # When I request bed assignment recommendations
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="request-recommendations-button"]').click()

        # Then the system displays "No suitable patients for trauma room"
        no_suitable_patients_message = get_text(self.driver, "No Suitable Patients Message")
        assert no_suitable_patients_message == "No suitable patients for trauma room"

        # And the system suggests:
        suggestions = [
            {"Recommendation Type": "Alternative Use", "Details": "Consider using for high acuity standard patients"},
            {"Recommendation Type": "Room Conversion", "Details": "Can be downgraded to standard room if needed"},
            {"Recommendation Type": "Hold for Emergency", "Details": "Keep available for incoming trauma cases"},
        ]
        for suggestion in suggestions:
            assert get_text(self.driver, suggestion["Recommendation Type"]) == suggestion["Details"]

        # And the system maintains the trauma room as available
        trauma_room_status = get_text(self.driver, "Trauma Room Status")
        assert trauma_room_status == "Available"

        # And no patient assignments are automatically made
        auto_assignments = self.driver.find_elements(By.CSS_SELECTOR, '[data-testid="patient-assignment"]')
        assert len(auto_assignments) == 0

    def test_handle_multiple_rooms_becoming_available_simultaneously(self):
        # Given multiple patients are waiting for beds:
        # And multiple rooms become available:
        # (assumed pre-seeded test data)

        # When I request bed assignment recommendations
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="request-recommendations-button"]').click()

        # Then the system provides multiple recommendations:
        recommendations = self.driver.find_elements(By.CSS_SELECTOR, '[data-testid="bed-recommendation-row"]')
        assert len(recommendations) == 3

        # And the system updates wait times for all remaining patients
        wait_for_test_id(self.driver, "Wait Times Updated Notice")

        # And the recommendations are ranked by patient acuity priority
        ranking_order = get_text(self.driver, "Recommendation Ranking Order")
        assert ranking_order == "Ranked by patient acuity priority"

    def test_consider_patient_gender_for_room_assignment(self):
        # Given multiple patients are waiting for beds:
        # And a standard room "ED-8" becomes available
        # And the room currently has a male patient in the adjacent bed
        # (assumed pre-seeded test data)

        # When I request bed assignment recommendations considering privacy preferences
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="request-recommendations-button"]').click()

        # Then the system suggests "Anthony Clark" as the top recommendation
        wait_for_test_id(self.driver, "Top Recommendation")
        top_recommendation = get_text(self.driver, "Top Recommendation")
        assert top_recommendation == "Anthony Clark"

        # And the recommendation includes:
        recommendation_fields = [
            {"Field": "Privacy Consideration", "Value": "Same gender as adjacent patient"},
            {"Field": "Alternative Option", "Value": "Michelle Lee (if privacy not a concern)"},
        ]
        for row in recommendation_fields:
            assert get_text(self.driver, row["Field"]) == row["Value"]

        # And I can override the gender consideration if clinically necessary
        wait_for_test_id(self.driver, "Override Gender Consideration")

    def test_handle_bed_assignment_during_high_volume_period(self):
        # Given the ED is operating at 95% capacity
        # And multiple high-acuity patients are waiting:
        # And a standard room "ED-6" becomes available
        # (assumed pre-seeded test data)

        # When I request bed assignment recommendations during crisis mode
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="request-recommendations-button"]').click()

        # Then the system prioritizes "Crisis Patient A" despite room type mismatch
        wait_for_test_id(self.driver, "Top Recommendation")
        top_recommendation = get_text(self.driver, "Top Recommendation")
        assert top_recommendation == "Crisis Patient A"

        # And the system displays:
        alerts = [
            {"Alert Type": "High Volume Alert", "Message": "ED at capacity - emergency protocols active"},
            {"Alert Type": "Room Flex Option", "Message": "Standard room can accommodate ESI Level 1"},
            {"Alert Type": "Resource Alert", "Message": "Additional equipment may be needed"},
        ]
        for alert in alerts:
            assert get_text(self.driver, alert["Alert Type"]) == alert["Message"]

        # And the system suggests moving lower acuity patients to make trauma rooms available
        trauma_room_suggestion = get_text(self.driver, "Trauma Room Availability Suggestion")
        assert "moving lower acuity patients" in trauma_room_suggestion.lower()

    def test_update_wait_times_after_bed_assignment(self):
        # Given the following patients are waiting:
        # And I assign Patient A to the available room
        # (assumed pre-seeded test data)

        # When the bed assignment is confirmed
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="confirm-bed-assignment-button"]').click()

        # Then the system recalculates wait times for remaining patients:
        wait_time_updates = self.driver.find_elements(By.CSS_SELECTOR, '[data-testid="wait-time-update-row"]')
        assert len(wait_time_updates) == 3

        # And the updated wait times are displayed on the patient tracking board
        wait_for_test_id(self.driver, "Patient Tracking Board")

        # And family members are notified of updated estimates via the patient portal
        family_notification_status = get_text(self.driver, "Family Notification Status")
        assert "notified" in family_notification_status.lower()

    def test_handle_bed_assignment_rejection_and_alternative_selection(self):
        # Given the system recommends "John Smith" for room "ED-10"
        # And John Smith has ESI Level 2 with chest pain
        # (assumed pre-seeded test data)

        # When I review the recommendation
        wait_for_test_id(self.driver, "Top Recommendation")

        # And I determine that John Smith needs cardiac monitoring not available in ED-10
        # (clinical judgement, no direct UI action)

        # And I reject the system recommendation
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="reject-recommendation-button"]').click()

        # Then the system provides alternative recommendations:
        alternative_recommendations = self.driver.find_elements(By.CSS_SELECTOR, '[data-testid="alternative-recommendation-row"]')
        assert len(alternative_recommendations) == 2

        # And the system suggests alternative rooms for John Smith:
        alternative_rooms = self.driver.find_elements(By.CSS_SELECTOR, '[data-testid="alternative-room-row"]')
        assert len(alternative_rooms) == 2

        # And I can select an alternative patient or wait for appropriate room for John Smith
        wait_for_test_id(self.driver, "Select Alternative Patient")
