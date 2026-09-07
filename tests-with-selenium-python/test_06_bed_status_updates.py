"""Selenium WebDriver + pytest test for
tests-with-given-when-then-features/06-bed-status-updates.feature
(equivalent to tests-with-selenium-javascript/06-bed-status-updates.test.js).

Assumes the app exposes data-testid attributes matching each Gherkin field
label (kebab-cased, see support/fields.py) and the shared data-testid
contract in support/login.py (login-identity, login-submit, app-root).
"""

from selenium.webdriver.common.by import By

from support.build_driver import build_driver
from support.fields import fill_field, fill_fields, get_text, wait_for_test_id
from support.login import login, verify_system_is_operational


class TestBedStatusUpdates:
    @classmethod
    def setup_class(cls):
        cls.driver = build_driver()

    @classmethod
    def teardown_class(cls):
        cls.driver.quit()

    def setup_method(self):
        # Background:
        #   Given the emergency care system is operational
        #   And I am logged in as a nurse
        #   And the bed management module is active
        #   And housekeeping notification system is enabled
        verify_system_is_operational(self.driver)
        login(self.driver, "a nurse")
        # The bed management module and housekeeping notification system are
        # assumed to be pre-seeded/enabled test data.

        feature_nav_link = wait_for_test_id(self.driver, "Nav Bed Status Updates")
        feature_nav_link.click()
        wait_for_test_id(self.driver, "Bed Status Updates Panel")

    def test_mark_bed_as_needs_cleaning_after_patient_discharge(self):
        # Given a patient "John Doe" is currently occupying bed "ED-12"
        # And the bed status is "Occupied"
        # And the available bed count shows 8 out of 20 beds available
        # (assumed pre-seeded test data)

        # When the patient is discharged from bed "ED-12"
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="discharge-patient-button"]').click()

        # And I mark the bed as "Needs Cleaning"
        fill_field(self.driver, "New Bed Status", "Needs Cleaning")

        # And I submit the bed status update
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-bed-status-update"]').click()

        # Then the system updates the bed status to "Dirty"
        wait_for_test_id(self.driver, "Bed Status")
        bed_status = get_text(self.driver, "Bed Status")
        assert bed_status == "Dirty"

        # And a notification is sent to housekeeping with details:
        notification_fields = [
            {"Field": "Room Number", "Value": "ED-12"},
            {"Field": "Status", "Value": "Needs Cleaning"},
            {"Field": "Priority", "Value": "Standard"},
            {"Field": "Patient Type", "Value": "Standard discharge"},
            {"Field": "Special Requirements", "Value": "Standard cleaning protocol"},
            {"Field": "Timestamp", "Value": "Current time"},
        ]
        for row in notification_fields:
            assert get_text(self.driver, row["Field"]) == row["Value"]

        # And the bed is removed from the available bed count
        # And the available bed count updates to 7 out of 20 beds available
        available_bed_count = get_text(self.driver, "Available Bed Count")
        assert available_bed_count == "7 out of 20 beds available"

        # And the bed appears as "Dirty" on the bed management dashboard
        dashboard_bed_status = get_text(self.driver, "Dashboard Bed Status")
        assert dashboard_bed_status == "Dirty"

    def test_mark_isolation_bed_for_deep_cleaning_after_infectious_patient(self):
        # Given a patient "Jane Smith" with isolation precautions is in bed "ED-ISO-2"
        # And the bed status is "Occupied - Isolation"
        # And the patient had confirmed MRSA infection
        # (assumed pre-seeded test data)

        # When the patient is discharged from bed "ED-ISO-2"
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="discharge-patient-button"]').click()

        # And I mark the bed as "Needs Deep Cleaning"
        fill_field(self.driver, "New Bed Status", "Needs Deep Cleaning")

        # And I specify the isolation type as "Contact Precautions - MRSA"
        fill_field(self.driver, "Isolation Type", "Contact Precautions - MRSA")

        # And I submit the bed status update
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-bed-status-update"]').click()

        # Then the system updates the bed status to "Dirty - Isolation"
        wait_for_test_id(self.driver, "Bed Status")
        bed_status = get_text(self.driver, "Bed Status")
        assert bed_status == "Dirty - Isolation"

        # And a high-priority notification is sent to housekeeping with details:
        notification_fields = [
            {"Field": "Room Number", "Value": "ED-ISO-2"},
            {"Field": "Status", "Value": "Needs Deep Cleaning"},
            {"Field": "Priority", "Value": "High"},
            {"Field": "Infection Type", "Value": "MRSA - Contact Precautions"},
            {"Field": "Special Requirements", "Value": "Terminal cleaning required"},
            {"Field": "PPE Required", "Value": "Gowns, gloves, masks"},
        ]
        for row in notification_fields:
            assert get_text(self.driver, row["Field"]) == row["Value"]

        # And the bed is flagged as "Out of Service" until deep cleaning completion
        bed_service_flag = get_text(self.driver, "Bed Service Flag")
        assert bed_service_flag == "Out of Service"

        # And the isolation bed count is reduced by one
        wait_for_test_id(self.driver, "Isolation Bed Count")

        # And an alert is sent to infection control team
        wait_for_test_id(self.driver, "Infection Control Alert")

    def test_housekeeping_completes_cleaning_and_marks_bed_ready(self):
        # Given bed "ED-8" has status "Dirty"
        # And housekeeping was notified 30 minutes ago
        # (assumed pre-seeded test data)

        # When the housekeeping staff completes cleaning of bed "ED-8"
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="complete-cleaning-button"]').click()

        # And the housekeeping supervisor marks the bed as "Clean and Ready"
        fill_field(self.driver, "New Bed Status", "Clean and Ready")

        # And submits the cleaning completion with details:
        fill_fields(
            self.driver,
            [
                {"Field": "Cleaning Staff", "Value": "Maria Rodriguez"},
                {"Field": "Cleaning Start", "Value": "14:30"},
                {"Field": "Cleaning End", "Value": "15:00"},
                {"Field": "Cleaning Type", "Value": "Standard"},
                {"Field": "Supplies Used", "Value": "Standard disinfection"},
            ],
        )
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-cleaning-completion-form"]').click()

        # Then the system updates the bed status to "Available"
        wait_for_test_id(self.driver, "Bed Status")
        bed_status = get_text(self.driver, "Bed Status")
        assert bed_status == "Available"

        # And the bed is added back to the available bed count
        # And the available bed count increases by one
        wait_for_test_id(self.driver, "Available Bed Count")

        # And a notification is sent to the charge nurse that bed "ED-8" is ready
        wait_for_test_id(self.driver, "Charge Nurse Notification")

        # And the bed appears as "Available" on the bed management dashboard
        dashboard_bed_status = get_text(self.driver, "Dashboard Bed Status")
        assert dashboard_bed_status == "Available"

    def test_handle_bed_maintenance_request_during_status_update(self):
        # Given a patient is discharged from bed "ED-15"
        # (assumed pre-seeded test data)

        # When I attempt to mark the bed as "Needs Cleaning"
        fill_field(self.driver, "New Bed Status", "Needs Cleaning")

        # And I notice equipment malfunction in the room
        # (observation, no direct UI action)

        # And I select "Maintenance Required" in addition to cleaning needs
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="maintenance-required-checkbox"]').click()

        # And I specify the issue as "IV pump not functioning, call light broken"
        fill_field(self.driver, "Issue Description", "IV pump not functioning, call light broken")

        # And I submit the bed status update
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-bed-status-update"]').click()

        # Then the system updates the bed status to "Out of Service - Maintenance"
        wait_for_test_id(self.driver, "Bed Status")
        bed_status = get_text(self.driver, "Bed Status")
        assert bed_status == "Out of Service - Maintenance"

        # And notifications are sent to both:
        notifications = [
            {"Department": "Housekeeping", "Notification Details": "Hold cleaning until maintenance"},
            {"Department": "Maintenance", "Notification Details": "IV pump and call light repair"},
        ]
        for notification in notifications:
            assert get_text(self.driver, notification["Department"]) == notification["Notification Details"]

        # And the bed is removed from available count until both issues are resolved
        wait_for_test_id(self.driver, "Available Bed Count")

        # And a work order is automatically generated for maintenance
        wait_for_test_id(self.driver, "Maintenance Work Order")

        # And the estimated downtime is calculated and displayed
        estimated_downtime = get_text(self.driver, "Estimated Downtime")
        assert len(estimated_downtime) > 0

    def test_update_bed_status_during_patient_transfer(self):
        # Given a patient "Robert Wilson" is in bed "ED-6"
        # And the patient needs to be transferred to ICU
        # (assumed pre-seeded test data)

        # When the transport team arrives to transfer the patient
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="transport-team-arrived-button"]').click()

        # And I update the bed status to "Patient in Transit"
        fill_field(self.driver, "New Bed Status", "Patient in Transit")

        # And I specify the destination as "ICU Room 302"
        fill_field(self.driver, "Destination", "ICU Room 302")

        # And I submit the status update
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-bed-status-update"]').click()

        # Then the bed status is temporarily set to "In Transit"
        wait_for_test_id(self.driver, "Bed Status")
        bed_status = get_text(self.driver, "Bed Status")
        assert bed_status == "In Transit"

        # And the bed remains unavailable for new assignments
        bed_availability = get_text(self.driver, "Bed Availability")
        assert "unavailable" in bed_availability.lower()

        # And a notification is sent to the receiving unit
        wait_for_test_id(self.driver, "Receiving Unit Notification")

        # And when the transfer is confirmed complete, I can mark the bed as "Needs Cleaning"
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="confirm-transfer-complete-button"]').click()
        fill_field(self.driver, "New Bed Status", "Needs Cleaning")

        # And the normal cleaning workflow is initiated
        wait_for_test_id(self.driver, "Cleaning Workflow Status")

    def test_handle_multiple_bed_status_updates_simultaneously(self):
        # Given multiple beds require status updates:
        # (assumed pre-seeded test data)

        # When I perform batch bed status updates:
        fill_fields(
            self.driver,
            [
                {"Field": "ED-3 New Status", "Value": "Needs Cleaning"},
                {"Field": "ED-7 New Status", "Value": "Patient in Transit"},
                {"Field": "ED-11 New Status", "Value": "Available"},
                {"Field": "ED-14 New Status", "Value": "Needs Cleaning"},
            ],
        )
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-batch-bed-status-update"]').click()

        # Then the system processes all updates simultaneously
        wait_for_test_id(self.driver, "Batch Update Status")

        # And appropriate notifications are sent to all relevant departments
        wait_for_test_id(self.driver, "Department Notifications Sent")

        # And the bed availability dashboard is updated in real-time
        wait_for_test_id(self.driver, "Bed Availability Dashboard")

        # And the total available bed count reflects all changes
        wait_for_test_id(self.driver, "Available Bed Count")

    def test_handle_urgent_bed_turnover_request(self):
        # Given the ED is at 95% capacity
        # And there is a trauma patient incoming requiring immediate bed
        # And bed "ED-4" patient is ready for discharge
        # (assumed pre-seeded test data)

        # When I mark the discharge as "Urgent Turnover Required"
        fill_field(self.driver, "New Bed Status", "Urgent Turnover Required")

        # And I request expedited cleaning for bed "ED-4"
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="request-expedited-cleaning-button"]').click()

        # And I submit the urgent status update
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-bed-status-update"]').click()

        # Then the system updates bed status to "Dirty - Urgent"
        wait_for_test_id(self.driver, "Bed Status")
        bed_status = get_text(self.driver, "Bed Status")
        assert bed_status == "Dirty - Urgent"

        # And a high-priority notification is sent to housekeeping:
        notification_fields = [
            {"Field": "Priority Level", "Value": "URGENT"},
            {"Field": "Room Number", "Value": "ED-4"},
            {"Field": "Reason", "Value": "Incoming trauma patient"},
            {"Field": "Target Time", "Value": "15 minutes"},
            {"Field": "Special Instructions", "Value": "Expedited cleaning protocol"},
        ]
        for row in notification_fields:
            assert get_text(self.driver, row["Field"]) == row["Value"]

        # And the charge nurse is notified of the urgent turnover request
        wait_for_test_id(self.driver, "Charge Nurse Notification")

        # And a timer is started to track cleaning completion time
        wait_for_test_id(self.driver, "Cleaning Completion Timer")

    def test_validate_bed_status_change_restrictions(self):
        # Given bed "ED-9" currently has status "Occupied"
        # And a patient "Susan Davis" is actively receiving treatment
        # (assumed pre-seeded test data)

        # When I attempt to mark the bed as "Available"
        fill_field(self.driver, "New Bed Status", "Available")

        # And I submit the invalid status change
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-bed-status-update"]').click()

        # Then the system displays a validation error:
        validation_errors = [
            {"Error Type": "Invalid Transition", "Message": "Cannot mark occupied bed as available"},
            {"Error Type": "Required Action", "Message": "Discharge patient first"},
            {"Error Type": "Current Patient", "Message": "Susan Davis - Active treatment"},
        ]
        for error in validation_errors:
            assert get_text(self.driver, error["Error Type"]) == error["Message"]

        # And the bed status remains "Occupied"
        bed_status = get_text(self.driver, "Bed Status")
        assert bed_status == "Occupied"

        # And no notifications are sent
        notifications = self.driver.find_elements(By.CSS_SELECTOR, '[data-testid="housekeeping-notification"]')
        assert len(notifications) == 0

        # And I am prompted to follow proper discharge workflow
        wait_for_test_id(self.driver, "Discharge Workflow Prompt")

    def test_track_bed_status_history_and_audit_trail(self):
        # Given bed "ED-5" has had multiple status changes today
        # (assumed pre-seeded test data)

        # When I access the bed status history
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="view-bed-status-history-button"]').click()

        # Then the system displays the complete audit trail:
        audit_trail_entries = self.driver.find_elements(By.CSS_SELECTOR, '[data-testid="audit-trail-entry"]')
        assert len(audit_trail_entries) == 5

        # And each status change includes timestamp and user identification
        wait_for_test_id(self.driver, "Audit Trail")

        # And the audit trail is preserved for compliance reporting
        compliance_status = get_text(self.driver, "Compliance Reporting Status")
        assert "preserved" in compliance_status.lower()

        # And I can generate reports on bed utilization patterns
        wait_for_test_id(self.driver, "Generate Utilization Report Button")

    def test_handle_bed_status_update_during_system_maintenance(self):
        # Given the housekeeping notification system is temporarily offline
        # And a patient is discharged from bed "ED-16"
        # (assumed pre-seeded test data)

        # When I mark the bed as "Needs Cleaning"
        fill_field(self.driver, "New Bed Status", "Needs Cleaning")

        # And I submit the status update
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-bed-status-update"]').click()

        # Then the system updates the bed status to "Dirty"
        wait_for_test_id(self.driver, "Bed Status")
        bed_status = get_text(self.driver, "Bed Status")
        assert bed_status == "Dirty"

        # And the system queues the housekeeping notification for later delivery
        wait_for_test_id(self.driver, "Queued Notification Status")

        # And a warning message is displayed: "Housekeeping system offline - notification queued"
        warning_message = get_text(self.driver, "Warning Message")
        assert warning_message == "Housekeeping system offline - notification queued"

        # And the bed is still removed from available count
        wait_for_test_id(self.driver, "Available Bed Count")

        # And when the housekeeping system comes back online, queued notifications are automatically sent
        # (system behavior outside the scope of this interaction)

        # And a log entry is created documenting the delayed notification
        wait_for_test_id(self.driver, "Delayed Notification Log Entry")
