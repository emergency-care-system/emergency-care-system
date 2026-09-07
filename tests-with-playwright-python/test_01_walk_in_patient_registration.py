"""Playwright + pytest test for
tests-with-given-when-then-features/01-walk-in-patient-registration.feature
(equivalent to tests-with-playwright-javascript/01-walk-in-patient-registration.test.js).

Assumes the app exposes data-testid attributes matching each Gherkin field
label (kebab-cased, see support/fields.py) and the shared data-testid
contract in support/login.py (login-identity, login-submit, app-root).
"""

from playwright.sync_api import sync_playwright

from support.fields import fill_field, fill_fields, get_text, locator, wait_for_test_id
from support.login import login, verify_system_is_operational


class TestWalkInPatientRegistration:
    @classmethod
    def setup_class(cls):
        # All scenarios in this class share one page (like the Selenium
        # suite shares one WebDriver per file), since later scenarios rely
        # on data earlier scenarios registered persisting in the app's
        # localStorage -- so pytest must run them in file order, not in
        # parallel (pytest's default behavior; no extra config needed).
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
        #   And I am logged in as a registration clerk
        verify_system_is_operational(self.page)
        login(self.page, "a registration clerk")

        # The demo app is a single-page dashboard: after login, select this
        # feature's panel from the sidebar nav (data-testid="nav-<slug>").
        registration_nav_link = wait_for_test_id(self.page, "Nav Walk In Patient Registration")
        registration_nav_link.click()
        wait_for_test_id(self.page, "Patient Registration Form")

    def test_successfully_register_a_new_walk_in_patient(self):
        # Given a new patient arrives at the ED without prior registration
        # And the patient provides valid identification
        # When I enter the patient's demographic information:
        fill_fields(
            self.page,
            [
                {"Field": "Given Name", "Value": "John"},
                {"Field": "Family Name", "Value": "Doe"},
                {"Field": "Date of Birth", "Value": "1985-06-15"},
                {"Field": "Phone Number", "Value": "555-123-4567"},
                {"Field": "Address", "Value": "123 Main St"},
                {"Field": "City", "Value": "Springfield"},
                {"Field": "State", "Value": "IL"},
                {"Field": "Zip Code", "Value": "62701"},
            ],
        )
        # And I enter the patient's insurance details:
        fill_fields(
            self.page,
            [
                {"Field": "Insurance Type", "Value": "Blue Cross"},
                {"Field": "Policy Number", "Value": "BC123456789"},
                {"Field": "Group Number", "Value": "GRP001"},
            ],
        )
        # And I submit the registration form
        self.page.get_by_test_id("submit-registration-form").click()

        # Then the system creates a unique patient record
        wait_for_test_id(self.page, "Medical Record Number")
        # And the system assigns a medical record number
        medical_record_number = get_text(self.page, "Medical Record Number")
        assert len(medical_record_number) > 0
        # And the patient is queued for triage
        triage_queue_status = get_text(self.page, "Triage Queue Status")
        assert "queued for triage" in triage_queue_status.lower()
        # And I see a confirmation message "Patient successfully registered"
        confirmation_message = get_text(self.page, "Confirmation Message")
        assert confirmation_message == "Patient successfully registered"
        # And the medical record number is displayed
        assert locator(self.page, "Medical Record Number").is_visible()

    def test_register_patient_with_missing_insurance_information(self):
        # Given a new patient arrives at the ED without prior registration
        # And the patient does not have insurance information
        # When I enter the patient's demographic information:
        fill_fields(
            self.page,
            [
                {"Field": "Given Name", "Value": "Jane"},
                {"Field": "Family Name", "Value": "Smith"},
                {"Field": "Date of Birth", "Value": "1990-03-22"},
                {"Field": "Phone Number", "Value": "555-987-6543"},
                {"Field": "Address", "Value": "456 Oak Ave"},
            ],
        )
        # And I select "Self-Pay" as the insurance type
        fill_field(self.page, "Insurance Type", "Self-Pay")
        # And I submit the registration form
        self.page.get_by_test_id("submit-registration-form").click()

        # Then the system creates a unique patient record
        wait_for_test_id(self.page, "Medical Record Number")
        # And the system assigns a medical record number
        medical_record_number = get_text(self.page, "Medical Record Number")
        assert len(medical_record_number) > 0
        # And the patient is queued for triage
        triage_queue_status = get_text(self.page, "Triage Queue Status")
        assert "queued for triage" in triage_queue_status.lower()
        # And the insurance status is marked as "Self-Pay"
        insurance_status = get_text(self.page, "Insurance Status")
        assert insurance_status == "Self-Pay"

    def test_handle_duplicate_patient_registration_attempt(self):
        # Given a patient with the same name and date of birth already exists in the system
        # When I enter the patient's demographic information:
        fill_fields(
            self.page,
            [
                {"Field": "Given Name", "Value": "John"},
                {"Field": "Family Name", "Value": "Doe"},
                {"Field": "Date of Birth", "Value": "1985-06-15"},
            ],
        )
        # And I submit the registration form
        self.page.get_by_test_id("submit-registration-form").click()

        # Then the system displays a warning "Potential duplicate patient found"
        warning_message = get_text(self.page, "Duplicate Patient Warning")
        assert warning_message == "Potential duplicate patient found"
        # And the system shows existing patient records for verification
        existing_records = self.page.get_by_test_id("existing-patient-record").all()
        assert len(existing_records) > 0
        # And I can choose to link to existing record or create new record
        wait_for_test_id(self.page, "Link to Existing Record")
        wait_for_test_id(self.page, "Create New Record")

    def test_registration_with_invalid_demographic_data(self):
        # Given a new patient arrives at the ED without prior registration
        # When I enter incomplete demographic information:
        fill_fields(
            self.page,
            [
                {"Field": "Given Name", "Value": "John"},
                {"Field": "Family Name", "Value": ""},
                {"Field": "Date of Birth", "Value": "invalid-date"},
            ],
        )
        # And I submit the registration form
        self.page.get_by_test_id("submit-registration-form").click()

        # Then the system displays validation errors:
        family_name_error = get_text(self.page, "Family Name Error")
        assert family_name_error == "Family name is required"
        date_of_birth_error = get_text(self.page, "Date of Birth Error")
        assert date_of_birth_error == "Invalid date format"
        # And the patient record is not created
        medical_record_numbers = locator(self.page, "Medical Record Number").all()
        assert len(medical_record_numbers) == 0
        # And the form remains open for correction
        assert self.page.get_by_test_id("patient-registration-form").is_visible()
