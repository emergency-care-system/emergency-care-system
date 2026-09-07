"""Selenium WebDriver + pytest test for
tests-with-given-when-then-features/02-ambulance-arrival-registration.feature
(equivalent to tests-with-selenium-javascript/02-ambulance-arrival-registration.test.js).

Assumes the app exposes data-testid attributes matching each Gherkin field
label (kebab-cased, see support/fields.py) and the shared data-testid
contract in support/login.py (login-identity, login-submit, app-root).
"""

import re

from selenium.webdriver.common.by import By

from support.build_driver import build_driver
from support.fields import fill_field, fill_fields, get_text, locator, wait_for_test_id
from support.login import login, verify_system_is_operational


class TestAmbulanceArrivalRegistration:
    @classmethod
    def setup_class(cls):
        cls.driver = build_driver()

    @classmethod
    def teardown_class(cls):
        cls.driver.quit()

    def setup_method(self):
        # Background:
        #   Given the emergency care system is operational
        #   And I am logged in as registration staff
        #   And the "unknown patient" registration module is available
        verify_system_is_operational(self.driver)
        login(self.driver, "registration staff")
        # The "unknown patient" registration module availability is assumed
        # pre-seeded test data / environment configuration.

        feature_nav_link = wait_for_test_id(self.driver, "Nav Ambulance Arrival Registration")
        feature_nav_link.click()
        wait_for_test_id(self.driver, "Ambulance Arrival Registration Panel")

    def test_register_unconscious_patient_brought_by_ambulance(self):
        # Given an ambulance arrives with a patient who cannot provide identification
        # And the patient is unconscious and has no identification documents
        # And EMS provides the following information:
        #   | Field                 | Value                    |
        #   | Estimated Age         | 45-50 years              |
        #   | Gender                | Male                     |
        #   | Chief Complaint       | Motor vehicle accident   |
        #   | Vital Signs           | BP: 90/60, HR: 120       |
        #   | Incident Location     | Highway 55 Mile Marker 12|
        #   | EMS Unit              | Ambulance 205            |
        #   | Arrival Time          | 14:30                    |
        # When I select "Unknown Patient" registration type
        fill_field(self.driver, "Registration Type", "Unknown Patient")
        # And I enter the available information from EMS
        fill_fields(
            self.driver,
            [
                {"Field": "Estimated Age", "Value": "45-50 years"},
                {"Field": "Gender", "Value": "Male"},
                {"Field": "Chief Complaint", "Value": "Motor vehicle accident"},
                {"Field": "Vital Signs", "Value": "BP: 90/60, HR: 120"},
                {"Field": "Incident Location", "Value": "Highway 55 Mile Marker 12"},
                {"Field": "EMS Unit", "Value": "Ambulance 205"},
                {"Field": "Arrival Time", "Value": "14:30"},
            ],
        )
        # And I submit the registration
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-registration-form"]').click()

        # Then the system creates a temporary patient record
        wait_for_test_id(self.driver, "Temporary Patient Record")
        # And the system assigns a placeholder ID starting with "UNK"
        placeholder_id = get_text(self.driver, "Placeholder ID")
        assert placeholder_id.startswith("UNK")
        # And the patient record is flagged for "Identity Verification Required"
        identity_flag = get_text(self.driver, "Identity Verification Flag")
        assert identity_flag == "Identity Verification Required"
        # And the patient is immediately queued for triage
        triage_queue_status = get_text(self.driver, "Triage Queue Status")
        assert "queued for triage" in triage_queue_status.lower()
        # And a notification is sent to the charge nurse about the unknown patient
        charge_nurse_notification = get_text(self.driver, "Charge Nurse Notification")
        assert "unknown patient" in charge_nurse_notification.lower()
        # And the record shows status as "Temporary - Pending Identification"
        record_status = get_text(self.driver, "Record Status")
        assert record_status == "Temporary - Pending Identification"

    def test_register_patient_with_partial_identification_from_personal_effects(self):
        # Given an ambulance arrives with a patient who cannot provide identification
        # And the patient has a wallet with partial information
        # And EMS provides the following information:
        #   | Field                 | Value                    |
        #   | Estimated Age         | 30-35 years              |
        #   | Gender                | Female                   |
        #   | Chief Complaint       | Drug overdose            |
        #   | Found Name            | Sarah (from credit card) |
        #   | Partial Phone         | 555-1234 (last 4 digits) |
        # When I select "Unknown Patient" registration type
        fill_field(self.driver, "Registration Type", "Unknown Patient")
        # And I enter the EMS information including partial identity details
        fill_fields(
            self.driver,
            [
                {"Field": "Estimated Age", "Value": "30-35 years"},
                {"Field": "Gender", "Value": "Female"},
                {"Field": "Chief Complaint", "Value": "Drug overdose"},
                {"Field": "Found Name", "Value": "Sarah (from credit card)"},
                {"Field": "Partial Phone", "Value": "555-1234 (last 4 digits)"},
            ],
        )
        # And I mark the identity fields as "Unverified"
        fill_field(self.driver, "Identity Status", "Unverified")
        # And I submit the registration
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-registration-form"]').click()

        # Then the system creates a temporary patient record
        wait_for_test_id(self.driver, "Temporary Patient Record")
        # And the system assigns a placeholder ID starting with "UNK"
        placeholder_id = get_text(self.driver, "Placeholder ID")
        assert placeholder_id.startswith("UNK")
        # And the partial identity information is stored with "Unverified" status
        identity_status = get_text(self.driver, "Identity Status")
        assert identity_status == "Unverified"
        # And the patient record is flagged for "Identity Verification Required"
        identity_flag = get_text(self.driver, "Identity Verification Flag")
        assert identity_flag == "Identity Verification Required"
        # And a task is created for social services to assist with identification
        social_services_task = get_text(self.driver, "Social Services Task")
        assert "identification" in social_services_task.lower()

    def test_register_patient_who_becomes_conscious_during_registration(self):
        # Given an ambulance arrives with a patient who initially cannot provide identification
        # And I have started the "Unknown Patient" registration process

        # When the patient becomes conscious and provides identification:
        fill_fields(
            self.driver,
            [
                {"Field": "Full Name", "Value": "Michael Johnson"},
                {"Field": "Date of Birth", "Value": "1980-12-15"},
                {"Field": "Phone Number", "Value": "555-876-5432"},
            ],
        )
        # And I verify the provided identification
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="verify-identification-button"]').click()

        # Then the system converts the temporary record to a verified patient record
        wait_for_test_id(self.driver, "Medical Record Number")
        # And the placeholder ID is replaced with a permanent medical record number
        medical_record_number = get_text(self.driver, "Medical Record Number")
        assert not medical_record_number.startswith("UNK")
        # And the "Identity Verification Required" flag is removed
        identity_flag_elements = self.driver.find_elements(*locator("Identity Verification Flag"))
        assert len(identity_flag_elements) == 0
        # And the patient demographic information is updated
        patient_name = get_text(self.driver, "Patient Name")
        assert patient_name == "Michael Johnson"
        # And a note is added documenting the identification process
        identification_note = get_text(self.driver, "Identification Note")
        assert len(identification_note) > 0

    def test_handle_multiple_unknown_patients_from_mass_casualty_incident(self):
        # Given multiple ambulances arrive from a mass casualty incident
        # And none of the patients can provide identification

        # When I select "Unknown Patient - Mass Casualty" registration type
        fill_field(self.driver, "Registration Type", "Unknown Patient - Mass Casualty")
        # And I enter the incident information:
        fill_fields(
            self.driver,
            [
                {"Field": "Incident Type", "Value": "Multi-vehicle accident"},
                {"Field": "Incident Location", "Value": "Interstate 70 Exit 45"},
                {"Field": "Total Patients", "Value": "4"},
            ],
        )
        # And I register each patient with EMS-provided information
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-registration-form"]').click()

        # Then the system creates temporary records for all patients
        temporary_records = self.driver.find_elements(By.CSS_SELECTOR, '[data-testid="temporary-patient-record"]')
        assert len(temporary_records) > 0
        # And each patient gets a sequential placeholder ID (UNK-001, UNK-002, etc.)
        placeholder_id = get_text(self.driver, "Placeholder ID")
        assert re.match(r"^UNK-\d{3}$", placeholder_id)
        # And all records are linked to the same incident number
        incident_number = get_text(self.driver, "Incident Number")
        assert len(incident_number) > 0
        # And the mass casualty protocol is activated
        mass_casualty_protocol_status = get_text(self.driver, "Mass Casualty Protocol Status")
        assert "activated" in mass_casualty_protocol_status.lower()
        # And notifications are sent to administration and social services
        notification_recipients = get_text(self.driver, "Notification Recipients")
        assert "administration" in notification_recipients.lower()

    def test_attempt_to_register_unknown_patient_without_ems_information(self):
        # Given an ambulance arrives with a patient who cannot provide identification
        # And EMS has minimal information available

        # When I select "Unknown Patient" registration type
        fill_field(self.driver, "Registration Type", "Unknown Patient")
        # And I attempt to submit with only basic information:
        #   | Field                 | Value                    |
        #   | Gender                | Unknown                  |
        #   | Estimated Age         | Unknown                  |
        #   | Chief Complaint       |                          |
        fill_fields(
            self.driver,
            [
                {"Field": "Gender", "Value": "Unknown"},
                {"Field": "Estimated Age", "Value": "Unknown"},
                {"Field": "Chief Complaint", "Value": ""},
            ],
        )
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-registration-form"]').click()

        # Then the system displays a warning "Insufficient information for registration"
        warning_message = get_text(self.driver, "Warning Message")
        assert warning_message == "Insufficient information for registration"
        # And the system requires minimum data fields:
        #   | Required Field        | Requirement                       |
        #   | Estimated Age Range   | Must be provided                  |
        #   | Gender                | Must be Male, Female, or Unknown  |
        #   | Chief Complaint       | Must be provided                  |
        assert get_text(self.driver, "Estimated Age Range Error") == "Must be provided"
        assert get_text(self.driver, "Gender Error") == "Must be Male, Female, or Unknown"
        assert get_text(self.driver, "Chief Complaint Error") == "Must be provided"
        # And the registration cannot be completed until minimum requirements are met
        placeholder_ids = self.driver.find_elements(*locator("Placeholder ID"))
        assert len(placeholder_ids) == 0

    def test_identity_verification_process_after_patient_stabilization(self):
        # Given a patient was registered as "Unknown Patient"
        # And the patient has now stabilized
        # And the patient can provide identification

        # When the nurse initiates the identity verification process
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="initiate-identity-verification-button"]').click()
        # And the patient provides valid identification:
        fill_fields(
            self.driver,
            [
                {"Field": "Full Name", "Value": "Robert Davis"},
                {"Field": "Date of Birth", "Value": "1975-08-20"},
                {"Field": "Social Security", "Value": "XXX-XX-1234 (last 4)"},
            ],
        )
        # And the identification is verified
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="verify-identification-button"]').click()

        # Then the system merges the temporary record with verified information
        wait_for_test_id(self.driver, "Medical Record Number")
        merge_status = get_text(self.driver, "Record Merge Status")
        assert "merged" in merge_status.lower()
        # And the "Identity Verification Required" flag is cleared
        identity_flag_elements = self.driver.find_elements(*locator("Identity Verification Flag"))
        assert len(identity_flag_elements) == 0
        # And a permanent medical record number is assigned
        medical_record_number = get_text(self.driver, "Medical Record Number")
        assert len(medical_record_number) > 0
        # And all clinical documentation is preserved under the new verified record
        clinical_documentation_status = get_text(self.driver, "Clinical Documentation Status")
        assert "preserved" in clinical_documentation_status.lower()
        # And billing information is updated with verified patient details
        billing_status = get_text(self.driver, "Billing Status")
        assert "updated" in billing_status.lower()
