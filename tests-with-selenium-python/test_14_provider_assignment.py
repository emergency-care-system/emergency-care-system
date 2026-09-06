"""Selenium WebDriver + pytest test for
spec/features/14-provider-assignment.feature
(equivalent to tests-with-selenium-javascript/14-provider-assignment.test.js).

Assumes the app exposes data-testid attributes matching each Gherkin field
label (kebab-cased, see support/fields.py) and the shared data-testid
contract in support/login.py (login-identity, login-submit, app-root).
"""

import re

from support.build_driver import build_driver
from support.fields import get_text, wait_for_test_id
from support.login import login, verify_system_is_operational


class TestProviderAssignment:
    @classmethod
    def setup_class(cls):
        cls.driver = build_driver()

    @classmethod
    def teardown_class(cls):
        cls.driver.quit()

    def setup_method(self):
        # Background:
        #   Given the emergency care system is operational
        #   And the provider assignment module is active
        #   And provider workload tracking is enabled
        #   And mobile notification system is functional
        verify_system_is_operational(self.driver)
        login(self.driver, "a charge nurse")
        # The provider assignment module, provider workload tracking, and the
        # mobile notification system are assumed to be active/enabled backend
        # configuration already in place for this environment.

        provider_assignment_nav_link = wait_for_test_id(self.driver, "Nav Provider Assignment")
        provider_assignment_nav_link.click()
        wait_for_test_id(self.driver, "Provider Assignment Panel")

    def test_assign_highest_priority_patient_to_newly_available_physician(self):
        # Given "Dr. Johnson" was seeing a patient in bed "ED-8"
        # And the current patient queue contains:
        #   | Position | Patient Name    | ESI Level | Triage Level   | Wait Time | Bed Ready |
        #   | 1        | Maria Santos    | 2         | High Priority  | 45 min    | Yes       |
        #   | 2        | Robert Kim      | 2         | High Priority  | 60 min    | Yes       |
        #   | 3        | Lisa Chen       | 3         | Urgent         | 90 min    | Yes       |
        #   | 4        | David Brown     | 3         | Urgent         | 105 min   | No        |
        # (assumed pre-seeded test data)
        # When "Dr. Johnson" completes the discharge for the patient in bed "ED-8"
        # And the system detects "Dr. Johnson" is now available
        # (assumed to have already occurred / triggered by the system)

        # Then the system identifies the next patient assignment:
        wait_for_test_id(self.driver, "Highest Priority")
        next_assignment_criteria = [
            {"Criteria": "Highest Priority", "Value": "Maria Santos (ESI Level 2)"},
            {"Criteria": "Bed Availability", "Value": "Bed ready for immediate assignment"},
            {"Criteria": "Provider Match", "Value": "Dr. Johnson available and qualified"},
        ]
        for row in next_assignment_criteria:
            assert get_text(self.driver, row["Criteria"]) == row["Value"]

        # And the system assigns "Maria Santos" to "Dr. Johnson"
        assert get_text(self.driver, "Assigned Patient") == "Maria Santos"
        assert get_text(self.driver, "Assigned Provider") == "Dr. Johnson"

        # And a notification is sent to Dr. Johnson's mobile device:
        mobile_notification = [
            {"Type": "Patient Assignment", "Content": "📱 New Patient: Maria Santos, Bed ED-12"},
            {"Type": "Priority Level", "Content": "ESI Level 2 - High Priority"},
            {"Type": "Chief Complaint", "Content": "Severe chest pain"},
            {"Type": "Wait Time", "Content": "Patient waiting 45 minutes"},
            {"Type": "Action Required", "Content": "Please proceed to ED-12"},
        ]
        for row in mobile_notification:
            assert get_text(self.driver, row["Type"]) == row["Content"]

        # And the patient status is updated to "Assigned to Dr. Johnson"
        assert get_text(self.driver, "Patient Status") == "Assigned to Dr. Johnson"

        # And the queue position is updated for remaining patients
        patient_queue = wait_for_test_id(self.driver, "Patient Queue")
        assert patient_queue.is_displayed()

    def test_handle_provider_assignment_with_specialty_requirements(self):
        # Given "Dr. Martinez" (Emergency Medicine) becomes available
        # And "Dr. Patel" (Pediatric Emergency) becomes available
        # And the current queue contains:
        #   | Patient Name     | Age | ESI Level | Specialty Required     | Wait Time |
        #   | Adult Patient    | 45  | 2         | Emergency Medicine     | 30 min    |
        #   | Child Patient    | 8   | 2         | Pediatric Emergency    | 35 min    |
        #   | General Patient  | 30  | 3         | Any                    | 60 min    |
        # (assumed pre-seeded test data)
        # When both providers request their next patient assignment
        # (assumed to have already occurred / triggered by the system)

        # Then the system matches providers to appropriate patients:
        wait_for_test_id(self.driver, "Dr. Patel Assigned Patient")
        provider_matches = [
            {"Provider": "Dr. Patel", "Assigned Patient": "Child Patient"},
            {"Provider": "Dr. Martinez", "Assigned Patient": "Adult Patient"},
        ]
        for row in provider_matches:
            assert get_text(self.driver, f"{row['Provider']} Assigned Patient") == row["Assigned Patient"]

        # And specialty-specific notifications are sent:
        specialty_notifications = [
            {"Provider": "Dr. Patel", "Content": "👶 Pediatric Patient: Age 8, ESI 2, Fever"},
            {"Provider": "Dr. Martinez", "Content": "🏥 Adult Patient: Age 45, ESI 2, Chest pain"},
        ]
        for row in specialty_notifications:
            assert get_text(self.driver, f"{row['Provider']} Notification") == row["Content"]

        # And the general patient remains in queue for the next available provider
        general_patient_status = get_text(self.driver, "General Patient Queue Status")
        assert "queue" in general_patient_status.lower()

    def test_prioritize_critical_patient_over_standard_queue_order(self):
        # Given "Dr. Thompson" becomes available
        # And the queue contains patients in order:
        #   | Position | Patient Name    | ESI Level | Assigned Bed | Special Circumstances |
        #   | 1        | Standard Patient| 3         | ED-5         | None                  |
        #   | 2        | Urgent Patient  | 3         | ED-7         | None                  |
        #   | 3        | Critical Patient| 1         | ED-TRAUMA-1  | Just arrived          |
        # (assumed pre-seeded test data)
        # When the system identifies the next patient for "Dr. Thompson"
        # (assumed to have already occurred / triggered by the system)

        # Then the system prioritizes by acuity over queue position:
        wait_for_test_id(self.driver, "Skip Queue Order")
        prioritization_logic = [
            {"Logic": "Skip Queue Order", "Reasoning": "ESI Level 1 takes priority over Level 3"},
            {"Logic": "Critical Priority", "Reasoning": "Life-threatening condition requires immediate"},
            {"Logic": "Provider Capability", "Reasoning": "Dr. Thompson qualified for trauma cases"},
        ]
        for row in prioritization_logic:
            assert get_text(self.driver, row["Logic"]) == row["Reasoning"]

        # And "Critical Patient" is assigned to "Dr. Thompson"
        assert get_text(self.driver, "Assigned Patient") == "Critical Patient"
        assert get_text(self.driver, "Assigned Provider") == "Dr. Thompson"

        # And the mobile notification includes urgency indicators:
        urgency_indicators = [
            {"Field": "Priority Alert", "Content": "🚨 CRITICAL: ESI Level 1 - Trauma"},
            {"Field": "Patient Location", "Content": "ED-TRAUMA-1"},
            {"Field": "Immediate Action", "Content": "Requires immediate assessment"},
            {"Field": "Support Teams", "Content": "Trauma team standing by"},
        ]
        for row in urgency_indicators:
            assert get_text(self.driver, row["Field"]) == row["Content"]

    def test_handle_provider_assignment_during_high_volume_period(self):
        # Given the ED is operating at 95% capacity
        # And multiple providers become available simultaneously:
        #   | Provider Name | Specialty          | Last Patient Completed |
        #   | Dr. Adams     | Emergency Medicine | 14:30                 |
        #   | Dr. Brown     | Emergency Medicine | 14:32                 |
        #   | Dr. Wilson    | Emergency Medicine | 14:35                 |
        # And 12 patients are waiting to be seen
        # (assumed pre-seeded test data)
        # When the system processes multiple provider assignments
        # (assumed to have already occurred / triggered by the system)

        # Then the system optimizes assignments across all available providers:
        wait_for_test_id(self.driver, "Dr. Adams Assigned Patient")
        optimized_assignments = [
            {"Provider": "Dr. Adams", "Assigned Patient": "Patient A"},
            {"Provider": "Dr. Brown", "Assigned Patient": "Patient B"},
            {"Provider": "Dr. Wilson", "Assigned Patient": "Patient C"},
        ]
        for row in optimized_assignments:
            assert get_text(self.driver, f"{row['Provider']} Assigned Patient") == row["Assigned Patient"]

        # And coordinated notifications are sent to prevent conflicts
        notification_coordination_status = get_text(self.driver, "Notification Coordination Status")
        assert "coordinated" in notification_coordination_status.lower()

        # And remaining patients receive updated wait time estimates
        wait_time_update_status = get_text(self.driver, "Wait Time Update Status")
        assert "updated" in wait_time_update_status.lower()

        # And surge capacity protocols are activated if needed
        surge_capacity_status = get_text(self.driver, "Surge Capacity Protocol Status")
        assert re.search(r"activated|standby", surge_capacity_status, re.IGNORECASE)

    def test_provider_assignment_with_workload_balancing(self):
        # Given provider workload tracking shows:
        #   | Provider Name | Patients Seen Today | Current Workload | Complexity Score |
        #   | Dr. Garcia    | 12                 | Light            | 85              |
        #   | Dr. Lee       | 18                 | Heavy            | 140             |
        #   | Dr. Foster    | 15                 | Moderate         | 110             |
        # And "Dr. Garcia" and "Dr. Foster" both become available
        # And the next patient is "Complex Patient" with multiple comorbidities
        # (assumed pre-seeded test data)
        # When the system determines provider assignment
        # (assumed to have already occurred / triggered by the system)

        # Then the system considers workload balancing:
        wait_for_test_id(self.driver, "Current Workload Decision")
        workload_balancing = [
            {"Factor": "Current Workload", "Decision": "Favors Dr. Garcia"},
            {"Factor": "Complexity Fit", "Decision": "Both qualified"},
            {"Factor": "Fatigue Factor", "Decision": "Dr. Garcia preferred"},
        ]
        for row in workload_balancing:
            assert get_text(self.driver, f"{row['Factor']} Decision") == row["Decision"]

        # And "Complex Patient" is assigned to "Dr. Garcia"
        assert get_text(self.driver, "Assigned Patient") == "Complex Patient"
        assert get_text(self.driver, "Assigned Provider") == "Dr. Garcia"

        # And workload metrics are updated for both providers
        dr_garcia_workload = wait_for_test_id(self.driver, "Dr. Garcia Workload")
        assert dr_garcia_workload.is_displayed()
        dr_foster_workload = wait_for_test_id(self.driver, "Dr. Foster Workload")
        assert dr_foster_workload.is_displayed()

    def test_handle_provider_assignment_with_patient_preferences(self):
        # Given a patient "VIP Patient" has requested "Dr. Johnson" if available
        # And "Dr. Johnson" and "Dr. Smith" both become available
        # And "VIP Patient" is next in the queue with ESI Level 3
        # (assumed pre-seeded test data)
        # When the system processes provider assignment
        # (assumed to have already occurred / triggered by the system)

        # Then the system considers patient preferences:
        wait_for_test_id(self.driver, "Patient Request")
        preference_factors = [
            {"Factor": "Patient Request", "Details": "Specifically requested Dr. Johnson"},
            {"Factor": "Medical Appropriateness", "Details": "Both doctors qualified for ESI Level 3"},
            {"Factor": "Availability", "Details": "Dr. Johnson available and willing"},
        ]
        for row in preference_factors:
            assert get_text(self.driver, row["Factor"]) == row["Details"]

        # And "VIP Patient" is assigned to "Dr. Johnson"
        assert get_text(self.driver, "Assigned Patient") == "VIP Patient"
        assert get_text(self.driver, "Assigned Provider") == "Dr. Johnson"

        # And "Dr. Smith" receives the next patient in queue
        dr_smith_assigned_patient = get_text(self.driver, "Dr. Smith Assigned Patient")
        assert len(dr_smith_assigned_patient) > 0

        # And the assignment includes preference notation:
        preference_notation = [
            {"Field": "Assignment Reason", "Content": "Patient preference request honored"},
            {"Field": "Special Notes", "Content": "VIP status - provide enhanced service"},
        ]
        for row in preference_notation:
            assert get_text(self.driver, row["Field"]) == row["Content"]

    def test_provider_assignment_failure_and_backup_procedures(self):
        # Given "Dr. Williams" becomes available
        # And the highest priority patient is "Emergency Patient" (ESI Level 1)
        # (assumed pre-seeded test data)
        # When the system attempts to send assignment notification to Dr. Williams
        # And the mobile device notification fails to deliver
        # (assumed to have already occurred / triggered by the system)

        # Then the system activates backup notification procedures:
        wait_for_test_id(self.driver, "Overhead Page")
        backup_procedures = [
            {"Method": "Overhead Page", "Action": '"Dr. Williams to ED-TRAUMA-1 immediately"'},
            {"Method": "Desktop Alert", "Action": "Popup on all ED workstations"},
            {"Method": "Charge Nurse Alert", "Action": "Direct notification to charge nurse"},
            {"Method": "Secondary Provider", "Action": "Alert backup doctor if no response in 2 min"},
        ]
        for row in backup_procedures:
            assert get_text(self.driver, row["Method"]) == row["Action"]

        # And the system logs the notification failure for IT review
        notification_failure_log = get_text(self.driver, "Notification Failure Log")
        assert "it review" in notification_failure_log.lower()

        # And continues attempting mobile notification every 30 seconds
        mobile_retry_status = get_text(self.driver, "Mobile Notification Retry Status")
        assert "30 seconds" in mobile_retry_status.lower()

        # And tracks response time for quality metrics
        response_time_tracking_status = get_text(self.driver, "Response Time Tracking Status")
        assert re.search(r"tracking|tracked", response_time_tracking_status, re.IGNORECASE)

    def test_handle_provider_assignment_during_shift_change(self):
        # Given it is 19:00 during evening shift change
        # And "Dr. Day" (day shift) is completing final patients
        # And "Dr. Night" (evening shift) is beginning shift
        # And a critical patient arrives requiring immediate attention
        # (assumed pre-seeded test data)
        # When the system determines provider assignment for the critical patient
        # (assumed to have already occurred / triggered by the system)

        # Then the system considers shift transition factors:
        wait_for_test_id(self.driver, "Shift Status")
        shift_transition_factors = [
            {"Factor": "Shift Status", "Consideration": "Dr. Day finishing, Dr. Night starting"},
            {"Factor": "Continuity", "Consideration": "Assign to Dr. Night for ongoing care"},
            {"Factor": "Availability", "Consideration": "Dr. Night has capacity for complex case"},
        ]
        for row in shift_transition_factors:
            assert get_text(self.driver, row["Factor"]) == row["Consideration"]

        # And the critical patient is assigned to "Dr. Night"
        assert get_text(self.driver, "Assigned Provider") == "Dr. Night"

        # And shift handoff information is included in the notification:
        handoff_information = [
            {"Component": "Shift Context", "Content": "New critical patient - evening shift start"},
            {"Component": "Day Shift Status", "Content": "Dr. Day finishing last 2 patients"},
            {"Component": "Support Available", "Content": "Day shift available for consultation"},
        ]
        for row in handoff_information:
            assert get_text(self.driver, row["Component"]) == row["Content"]

    def test_track_provider_response_times_and_assignment_efficiency(self):
        # Given provider assignment notifications are sent
        # (assumed pre-seeded test data)
        # When providers respond to patient assignments
        # (assumed to have already occurred / triggered by the system)

        # Then the system tracks performance metrics:
        wait_for_test_id(self.driver, "Notification to Response")
        performance_metrics = [
            {"Metric": "Notification to Response", "Measurement": "Time from alert to bedside presence"},
            {"Metric": "Assignment Accuracy", "Measurement": "Correct provider-patient matching"},
            {"Metric": "Queue Optimization", "Measurement": "Wait time reduction effectiveness"},
        ]
        for row in performance_metrics:
            assert get_text(self.driver, row["Metric"]) == row["Measurement"]

        # And generates provider performance reports:
        performance_reports = [
            {
                "Provider": "Dr. Johnson",
                "Avg Response Time": "3.2 minutes",
                "Assignment Accuracy": "98%",
                "Patient Satisfaction": "4.8/5",
            },
            {
                "Provider": "Dr. Smith",
                "Avg Response Time": "4.1 minutes",
                "Assignment Accuracy": "96%",
                "Patient Satisfaction": "4.6/5",
            },
        ]
        for report in performance_reports:
            assert get_text(self.driver, f"{report['Provider']} Avg Response Time") == report["Avg Response Time"]
            assert get_text(self.driver, f"{report['Provider']} Assignment Accuracy") == report["Assignment Accuracy"]
            assert (
                get_text(self.driver, f"{report['Provider']} Patient Satisfaction") == report["Patient Satisfaction"]
            )

        # And identifies optimization opportunities:
        optimization_opportunities = [
            {"Area": "Response Time", "Recommendation": "Target <3 minutes for critical patients"},
            {"Area": "Assignment Matching", "Recommendation": "Consider additional specialty training"},
            {"Area": "Communication", "Recommendation": "Implement two-way acknowledgment system"},
        ]
        for row in optimization_opportunities:
            assert get_text(self.driver, row["Area"]) == row["Recommendation"]
