"""Selenium WebDriver + pytest test for
spec/features/13-dynamic-queue-updates.feature
(equivalent to tests-with-selenium-javascript/13-dynamic-queue-updates.test.js).

Assumes the app exposes data-testid attributes matching each Gherkin field
label (kebab-cased, see support/fields.py) and the shared data-testid
contract in support/login.py (login-identity, login-submit, app-root).
"""

from selenium.webdriver.common.by import By

from support.build_driver import build_driver
from support.fields import fill_field, fill_fields, get_text, wait_for_test_id
from support.login import login, verify_system_is_operational


class TestDynamicQueueUpdates:
    @classmethod
    def setup_class(cls):
        cls.driver = build_driver()

    @classmethod
    def teardown_class(cls):
        cls.driver.quit()

    def setup_method(self):
        # Background:
        #   Given the emergency care system is operational
        #   And the dynamic queue management module is active
        #   And the ESI (Emergency Severity Index) prioritization system is enabled
        #   And 15 patients are currently waiting to be seen
        verify_system_is_operational(self.driver)
        login(self.driver, "a charge nurse")
        # The dynamic queue management module, the ESI prioritization system,
        # and the 15 already-waiting patients are assumed to be pre-seeded
        # test environment state.

        dynamic_queue_updates_nav_link = wait_for_test_id(self.driver, "Nav Dynamic Queue Updates")
        dynamic_queue_updates_nav_link.click()
        wait_for_test_id(self.driver, "Dynamic Queue Updates Panel")

    def test_high_priority_trauma_patient_bumps_existing_queue(self):
        # Given the current patient queue contains:
        #   | Position | Patient Name    | ESI Level | Triage Level   | Current Wait Time |
        #   | 1        | Alice Johnson   | 2         | High Priority  | 20 minutes       |
        #   | 2        | Bob Williams    | 2         | High Priority  | 35 minutes       |
        #   | 3        | Carol Davis     | 3         | Urgent         | 45 minutes       |
        #   | 4        | David Brown     | 3         | Urgent         | 60 minutes       |
        #   | 5        | Emma Wilson     | 3         | Urgent         | 75 minutes       |
        #   | 6-15     | Other patients  | 3-5       | Various        | 90-180 minutes   |
        # And available providers can see 1 patient every 30 minutes

        # When a new trauma patient "Emergency Trauma" arrives with ESI level 1
        fill_fields(
            self.driver,
            [
                {"Field": "Patient Name", "Value": "Emergency Trauma"},
                {"Field": "ESI Level", "Value": "1"},
            ],
        )
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-patient-arrival"]').click()
        # And the patient is triaged as "Resuscitation - Life threatening"
        fill_field(self.driver, "Triage Level", "Resuscitation - Life threatening")
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-triage"]').click()

        # Then the system immediately reprioritizes the queue:
        #   | New Position | Patient Name      | ESI Level | Wait Time Impact    |
        #   | 1            | Emergency Trauma  | 1         | Immediate           |
        #   | 2            | Alice Johnson     | 2         | +30 min (50 min)    |
        #   | 3            | Bob Williams      | 2         | +30 min (65 min)    |
        #   | 4            | Carol Davis       | 3         | +30 min (75 min)    |
        #   | 5            | David Brown       | 3         | +30 min (90 min)    |
        wait_for_test_id(self.driver, "Emergency Trauma Queue Position")
        assert get_text(self.driver, "Emergency Trauma Queue Position") == "1"
        assert get_text(self.driver, "Emergency Trauma Wait Time Impact") == "Immediate"
        assert get_text(self.driver, "Alice Johnson Queue Position") == "2"
        assert get_text(self.driver, "Alice Johnson Wait Time Impact") == "+30 min (50 min)"
        assert get_text(self.driver, "Bob Williams Queue Position") == "3"
        assert get_text(self.driver, "Bob Williams Wait Time Impact") == "+30 min (65 min)"
        assert get_text(self.driver, "Carol Davis Queue Position") == "4"
        assert get_text(self.driver, "Carol Davis Wait Time Impact") == "+30 min (75 min)"
        assert get_text(self.driver, "David Brown Queue Position") == "5"
        assert get_text(self.driver, "David Brown Wait Time Impact") == "+30 min (90 min)"

        # And the system updates wait time estimates for all affected patients:
        #   | Patient Name    | Previous Estimate | New Estimate | Change      |
        #   | Alice Johnson   | 20 minutes       | 50 minutes   | +30 minutes |
        #   | Bob Williams    | 35 minutes       | 65 minutes   | +30 minutes |
        #   | Carol Davis     | 45 minutes       | 75 minutes   | +30 minutes |
        #   | All others      | Various          | +30 minutes  | Increased   |
        assert get_text(self.driver, "Alice Johnson Wait Time Estimate") == "50 minutes"
        assert get_text(self.driver, "Bob Williams Wait Time Estimate") == "65 minutes"
        assert get_text(self.driver, "Carol Davis Wait Time Estimate") == "75 minutes"
        all_others_change = get_text(self.driver, "All Others Change")
        assert "increased" in all_others_change.lower()

        # And notifications are sent to affected patients and families
        wait_for_test_id(self.driver, "Patient Family Notifications Sent")

        # And the trauma team is immediately alerted for the ESI Level 1 patient
        trauma_team_alert = get_text(self.driver, "Trauma Team Alert")
        assert "ESI Level 1" in trauma_team_alert

    def test_multiple_high_acuity_patients_arrive_simultaneously(self):
        # Given the current queue has patients with ESI levels 3-5
        # And the next available appointment slot is in 60 minutes

        # When multiple high-acuity patients arrive within 10 minutes:
        #   | Arrival Time | Patient Name     | ESI Level | Chief Complaint          |
        #   | 14:00        | Critical Patient | 1         | Cardiac arrest           |
        #   | 14:05        | Urgent Patient A | 2         | Severe chest pain        |
        #   | 14:08        | Urgent Patient B | 2         | Difficulty breathing     |
        fill_fields(
            self.driver,
            [
                {"Field": "Patient Name 1", "Value": "Critical Patient"},
                {"Field": "ESI Level 1", "Value": "1"},
                {"Field": "Arrival Time 1", "Value": "14:00"},
                {"Field": "Patient Name 2", "Value": "Urgent Patient A"},
                {"Field": "ESI Level 2", "Value": "2"},
                {"Field": "Arrival Time 2", "Value": "14:05"},
                {"Field": "Patient Name 3", "Value": "Urgent Patient B"},
                {"Field": "ESI Level 3", "Value": "2"},
                {"Field": "Arrival Time 3", "Value": "14:08"},
            ],
        )
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-patient-arrivals"]').click()

        # Then the system prioritizes patients by ESI level and arrival time:
        #   | New Position | Patient Name     | ESI Level | Priority Rationale        |
        #   | 1            | Critical Patient | 1         | Highest acuity - immediate |
        #   | 2            | Urgent Patient A | 2         | ESI 2 - arrived first     |
        #   | 3            | Urgent Patient B | 2         | ESI 2 - arrived second    |
        #   | 4-18         | Existing patients| 3-5       | Lower priority            |
        wait_for_test_id(self.driver, "Critical Patient Queue Position")
        assert get_text(self.driver, "Critical Patient Queue Position") == "1"
        assert get_text(self.driver, "Urgent Patient A Queue Position") == "2"
        assert get_text(self.driver, "Urgent Patient B Queue Position") == "3"

        # And the system calculates cascading wait time impacts:
        #   | Patient Category | Wait Time Impact                              |
        #   | ESI Level 3      | +90 minutes (3 new higher priority patients) |
        #   | ESI Level 4      | +90 minutes                                   |
        #   | ESI Level 5      | +90 minutes                                   |
        assert get_text(self.driver, "ESI Level 3 Wait Time Impact") == "+90 minutes (3 new higher priority patients)"
        assert get_text(self.driver, "ESI Level 4 Wait Time Impact") == "+90 minutes"
        assert get_text(self.driver, "ESI Level 5 Wait Time Impact") == "+90 minutes"

        # And multiple department alerts are triggered:
        #   | Department    | Alert Type                                    |
        #   | Trauma Team   | ESI 1 - Immediate response required          |
        #   | Cardiology    | Multiple cardiac-related ESI 2 patients     |
        #   | Administration| Surge capacity - consider additional staff   |
        assert get_text(self.driver, "Trauma Team Alert Type") == "ESI 1 - Immediate response required"
        assert get_text(self.driver, "Cardiology Alert Type") == "Multiple cardiac-related ESI 2 patients"
        assert get_text(self.driver, "Administration Alert Type") == "Surge capacity - consider additional staff"

    def test_queue_updates_with_bed_availability_constraints(self):
        # Given 15 patients are waiting and only 2 beds are currently available
        # And the bed types are:
        #   | Bed Number | Bed Type     | Status    |
        #   | ED-5       | Standard     | Available |
        #   | ED-TRAUMA-1| Trauma       | Available |
        #   | ED-8       | Standard     | Occupied  |
        #   | ED-12      | Isolation    | Occupied  |

        # When a trauma patient with ESI level 1 arrives requiring trauma bay
        fill_fields(
            self.driver,
            [
                {"Field": "Patient Name", "Value": "Trauma Patient"},
                {"Field": "ESI Level", "Value": "1"},
                {"Field": "Bed Type Required", "Value": "Trauma"},
            ],
        )
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-patient-arrival"]').click()

        # Then the system updates the queue considering bed constraints:
        #   | Queue Position | Patient Name    | ESI Level | Bed Assignment Strategy     |
        #   | 1              | Trauma Patient  | 1         | ED-TRAUMA-1 (immediate)     |
        #   | 2              | Alice Johnson   | 2         | ED-5 when available         |
        #   | 3              | Bob Williams    | 2         | Wait for next bed           |
        wait_for_test_id(self.driver, "Trauma Patient Queue Position")
        assert get_text(self.driver, "Trauma Patient Queue Position") == "1"
        assert get_text(self.driver, "Trauma Patient Bed Assignment Strategy") == "ED-TRAUMA-1 (immediate)"
        assert get_text(self.driver, "Alice Johnson Queue Position") == "2"
        assert get_text(self.driver, "Alice Johnson Bed Assignment Strategy") == "ED-5 when available"
        assert get_text(self.driver, "Bob Williams Queue Position") == "3"
        assert get_text(self.driver, "Bob Williams Bed Assignment Strategy") == "Wait for next bed"

        # And wait times reflect both queue position and bed availability:
        #   | Patient Name    | Queue Wait | Bed Wait  | Total Estimate |
        #   | Trauma Patient  | 0 minutes  | 0 minutes | Immediate      |
        #   | Alice Johnson   | 0 minutes  | 0 minutes | Immediate      |
        #   | Bob Williams    | 0 minutes  | 45 minutes| 45 minutes     |
        assert get_text(self.driver, "Trauma Patient Total Estimate") == "Immediate"
        assert get_text(self.driver, "Alice Johnson Total Estimate") == "Immediate"
        assert get_text(self.driver, "Bob Williams Total Estimate") == "45 minutes"

        # And the system provides realistic expectations based on resource constraints
        wait_for_test_id(self.driver, "Resource Constraint Notice")

    def test_handle_queue_updates_during_provider_capacity_changes(self):
        # Given the current provider capacity is 3 physicians seeing patients
        # And average patient encounter time is 30 minutes
        # And 15 patients are in queue with estimated wait times

        # When one physician becomes unavailable due to emergency procedure
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="mark-physician-unavailable"]').click()
        # And a new ESI level 1 patient arrives
        fill_field(self.driver, "ESI Level", "1")
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-patient-arrival"]').click()

        # Then the system recalculates queue times with reduced capacity:
        #   | Capacity Change | Impact                                        |
        #   | 3 → 2 providers | 50% increase in wait times for existing patients |
        #   | ESI 1 arrival  | All patients bumped down one position        |
        wait_for_test_id(self.driver, "3 → 2 providers")
        assert get_text(self.driver, "3 → 2 providers") == "50% increase in wait times for existing patients"
        assert get_text(self.driver, "ESI 1 arrival") == "All patients bumped down one position"

        # And updated wait time calculations reflect both changes:
        #   | Patient Category | Original Wait | Capacity Impact | ESI 1 Impact | New Wait   |
        #   | ESI Level 2      | 30 minutes   | +15 minutes     | +30 minutes  | 75 minutes |
        #   | ESI Level 3      | 60 minutes   | +30 minutes     | +30 minutes  | 120 minutes|
        #   | ESI Level 4      | 90 minutes   | +45 minutes     | +30 minutes  | 165 minutes|
        assert get_text(self.driver, "ESI Level 2 New Wait") == "75 minutes"
        assert get_text(self.driver, "ESI Level 3 New Wait") == "120 minutes"
        assert get_text(self.driver, "ESI Level 4 New Wait") == "165 minutes"

        # And the system sends capacity alerts to administration
        wait_for_test_id(self.driver, "Capacity Alert")

        # And patients/families are notified of updated wait times
        wait_for_test_id(self.driver, "Patient Family Notifications Sent")

    def test_prioritize_patient_with_rapidly_deteriorating_condition(self):
        # Given a patient "Sarah Mitchell" is currently position 8 in queue with ESI level 3
        # And her initial complaint was "mild abdominal pain"

        # When the patient's condition deteriorates and reassessment shows:
        #   | Assessment Change | New Value                                     |
        #   | Pain Level        | 3/10 → 9/10                                  |
        #   | Vital Signs       | BP: 120/80 → 85/50, HR: 80 → 120            |
        #   | Mental Status     | Alert → Confused                             |
        #   | ESI Level         | 3 → 2                                        |
        fill_fields(
            self.driver,
            [
                {"Field": "Pain Level", "Value": "9/10"},
                {"Field": "Vital Signs", "Value": "BP: 85/50, HR: 120"},
                {"Field": "Mental Status", "Value": "Confused"},
                {"Field": "ESI Level", "Value": "2"},
            ],
        )
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-reassessment"]').click()

        # Then the system immediately updates her queue position:
        #   | Action Type       | Details                                       |
        #   | Priority Escalation| ESI 3 → ESI 2 due to deterioration          |
        #   | Queue Repositioning| Position 8 → Position 2                     |
        #   | Wait Time Update  | 120 minutes → 15 minutes                     |
        wait_for_test_id(self.driver, "Priority Escalation")
        assert get_text(self.driver, "Priority Escalation") == "ESI 3 → ESI 2 due to deterioration"
        assert get_text(self.driver, "Queue Repositioning") == "Position 8 → Position 2"
        assert get_text(self.driver, "Wait Time Update") == "120 minutes → 15 minutes"

        # And escalation notifications are sent:
        #   | Recipient         | Notification Content                          |
        #   | Attending Physician| Patient deterioration - Priority escalated   |
        #   | Charge Nurse      | Sarah Mitchell moved to position 2           |
        #   | Triage Nurse      | Reassessment resulted in ESI upgrade         |
        assert get_text(self.driver, "Attending Physician Notification") == "Patient deterioration - Priority escalated"
        assert get_text(self.driver, "Charge Nurse Notification") == "Sarah Mitchell moved to position 2"
        assert get_text(self.driver, "Triage Nurse Notification") == "Reassessment resulted in ESI upgrade"

        # And all subsequent patients are shifted down in the queue
        wait_for_test_id(self.driver, "Queue Shift Notice")

        # And family members are notified of the priority change
        wait_for_test_id(self.driver, "Family Priority Change Notification")

    def test_handle_specialty_service_requirements_in_queue_management(self):
        # Given 15 patients are waiting with various specialty needs:
        #   | Patient Name    | ESI Level | Specialty Required  | Current Position |
        #   | General Patient | 3         | None               | 3                |
        #   | Cardiac Patient | 2         | Cardiology         | 5                |
        #   | Peds Patient    | 3         | Pediatrics         | 8                |

        # When a new trauma patient arrives requiring neurosurgery consultation
        # And the patient has ESI level 1
        fill_fields(
            self.driver,
            [
                {"Field": "Patient Name", "Value": "Trauma/Neuro"},
                {"Field": "ESI Level", "Value": "1"},
                {"Field": "Specialty Required", "Value": "Neurosurgery"},
            ],
        )
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-patient-arrival"]').click()

        # Then the system considers both acuity and specialty availability:
        #   | Priority Factor   | Consideration                                 |
        #   | ESI Level 1       | Highest medical priority                     |
        #   | Neurosurgery Need | Specialty consultant availability            |
        #   | Resource Planning | OR availability for potential surgery        |
        wait_for_test_id(self.driver, "ESI Level 1 Consideration")
        assert get_text(self.driver, "ESI Level 1 Consideration") == "Highest medical priority"
        assert get_text(self.driver, "Neurosurgery Need Consideration") == "Specialty consultant availability"
        assert get_text(self.driver, "Resource Planning Consideration") == "OR availability for potential surgery"

        # And the queue is updated with specialty considerations:
        #   | Position | Patient Name    | Priority Reason                          |
        #   | 1        | Trauma/Neuro    | ESI 1 + Specialty coordination needed   |
        #   | 2        | Cardiac Patient | ESI 2 + Cardiology available           |
        #   | 3        | General Patient | ESI 3 but no specialty delay           |
        assert get_text(self.driver, "Trauma/Neuro Queue Position") == "1"
        assert get_text(self.driver, "Trauma/Neuro Priority Reason") == "ESI 1 + Specialty coordination needed"
        assert get_text(self.driver, "Cardiac Patient Queue Position") == "2"
        assert get_text(self.driver, "Cardiac Patient Priority Reason") == "ESI 2 + Cardiology available"
        assert get_text(self.driver, "General Patient Queue Position") == "3"
        assert get_text(self.driver, "General Patient Priority Reason") == "ESI 3 but no specialty delay"

        # And specialty teams are notified with urgency levels
        wait_for_test_id(self.driver, "Specialty Team Notification")

    def test_queue_updates_with_time_sensitive_treatment_windows(self):
        # Given several patients with time-sensitive conditions are in queue:
        #   | Patient Name    | Condition           | Treatment Window | Queue Position |
        #   | Stroke Patient  | Suspected stroke    | 4.5 hours       | 4              |
        #   | STEMI Patient   | Heart attack        | 90 minutes      | 6              |

        # When a new ESI level 1 trauma patient arrives
        fill_field(self.driver, "ESI Level", "1")
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-patient-arrival"]').click()

        # Then the system balances acuity with time sensitivity:
        #   | Prioritization Logic | Decision Rationale                        |
        #   | ESI 1 Trauma        | Immediate life threat - top priority      |
        #   | STEMI Patient       | Time-critical (90 min) - position 2      |
        #   | Stroke Patient      | Time-critical (4.5 hr) - position 3      |
        wait_for_test_id(self.driver, "ESI 1 Trauma Decision Rationale")
        assert get_text(self.driver, "ESI 1 Trauma Decision Rationale") == "Immediate life threat - top priority"
        assert get_text(self.driver, "STEMI Patient Decision Rationale") == "Time-critical (90 min) - position 2"
        assert get_text(self.driver, "Stroke Patient Decision Rationale") == "Time-critical (4.5 hr) - position 3"

        # And time-sensitive alerts are maintained:
        #   | Patient Type    | Alert Status                                  |
        #   | Stroke Patient  | 45 minutes remaining in optimal window       |
        #   | STEMI Patient   | 25 minutes remaining for door-to-balloon     |
        assert get_text(self.driver, "Stroke Patient Alert Status") == "45 minutes remaining in optimal window"
        assert get_text(self.driver, "STEMI Patient Alert Status") == "25 minutes remaining for door-to-balloon"

        # And the system tracks treatment deadlines for all time-sensitive cases
        wait_for_test_id(self.driver, "Treatment Deadline Tracker")

    def test_real_time_queue_visualization_updates(self):
        # Given the ED dashboard displays the current patient queue
        # And family members can view estimated wait times on patient portal

        # When queue positions change due to new arrivals
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="simulate-new-arrival"]').click()

        # Then all displays update in real-time:
        #   | Display Location    | Update Type                                 |
        #   | Main ED Dashboard   | Queue positions and wait times refreshed   |
        #   | Patient Portal      | Family notifications of wait time changes  |
        #   | Mobile Apps         | Provider apps show updated patient lists   |
        #   | Waiting Room Display| General wait time estimates updated        |
        wait_for_test_id(self.driver, "Main ED Dashboard Update Type")
        assert get_text(self.driver, "Main ED Dashboard Update Type") == "Queue positions and wait times refreshed"
        assert get_text(self.driver, "Patient Portal Update Type") == "Family notifications of wait time changes"
        assert get_text(self.driver, "Mobile Apps Update Type") == "Provider apps show updated patient lists"
        assert get_text(self.driver, "Waiting Room Display Update Type") == "General wait time estimates updated"

        # And update timestamps are shown on all displays:
        #   | Display Element     | Information Provided                        |
        #   | Last Updated        | "Queue updated at 14:35:22"                |
        #   | Next Update         | "Automatic refresh in 30 seconds"          |
        #   | Manual Refresh      | Button available for immediate update       |
        assert get_text(self.driver, "Last Updated") == "Queue updated at 14:35:22"
        assert get_text(self.driver, "Next Update") == "Automatic refresh in 30 seconds"
        assert get_text(self.driver, "Manual Refresh") == "Button available for immediate update"

        # And change notifications highlight significant updates for staff attention
        wait_for_test_id(self.driver, "Staff Change Notification Highlight")
