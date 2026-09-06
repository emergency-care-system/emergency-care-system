"""Playwright + pytest test for spec/features/07-physician-assessment.feature
(equivalent to tests-with-playwright-javascript/07-physician-assessment.test.js).

Assumes the app exposes data-testid attributes matching each Gherkin field
label (kebab-cased, see support/fields.py) and the shared data-testid
contract in support/login.py (login-identity, login-submit, app-root).
"""

from playwright.sync_api import sync_playwright

from support.fields import get_text, wait_for_test_id
from support.login import login, verify_system_is_operational


class TestPhysicianAssessment:
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
        #   And I am logged in as "Dr. Smith" on the mobile app
        #   And the patient chart access module is enabled
        #   And real-time data synchronization is active
        verify_system_is_operational(self.page)
        login(self.page, "Dr. Smith", mobile=True)
        # The patient chart access module and real-time data
        # synchronization are assumed to be pre-seeded/enabled test data.

        feature_nav_link = wait_for_test_id(self.page, "Nav Physician Assessment")
        feature_nav_link.click()
        wait_for_test_id(self.page, "Physician Assessment Panel")

    def test_access_patient_chart_with_complete_nursing_assessment(self):
        # Given a patient "Jennifer Martinez" is assigned to bed "ED-8"
        # And the nursing assessment is completed with the following data:
        # (assumed pre-seeded test data)

        # When I open the patient's chart for "Jennifer Martinez" in bed "ED-8"
        self.page.get_by_test_id("open-patient-chart-button").click()

        # Then the system displays the patient summary with:
        patient_summary_fields = [
            {"Section": "Patient Identity", "Content": "Jennifer Martinez, DOB: 1975-03-15"},
            {"Section": "Bed Assignment", "Content": "ED-8"},
            {"Section": "Arrival Time", "Content": "14:00"},
            {"Section": "Triage Notes", "Content": "ESI Level 2 - Severe chest pain, onset 2h ago"},
        ]
        for row in patient_summary_fields:
            assert get_text(self.page, row["Section"]) == row["Content"]

        # And the vital signs section shows:
        vital_signs = [
            {"Vital Sign": "Blood Pressure", "Value": "160/95", "Trend": "High"},
            {"Vital Sign": "Heart Rate", "Value": "110", "Trend": "Elevated"},
            {"Vital Sign": "Respiratory Rate", "Value": "22", "Trend": "Elevated"},
            {"Vital Sign": "Temperature", "Value": "98.6°F", "Trend": "Normal"},
            {"Vital Sign": "Oxygen Saturation", "Value": "94%", "Trend": "Low"},
            {"Vital Sign": "Pain Score", "Value": "8/10", "Trend": "Severe"},
        ]
        for vital in vital_signs:
            assert get_text(self.page, vital["Vital Sign"]) == vital["Value"]
            assert get_text(self.page, f"{vital['Vital Sign']} Trend") == vital["Trend"]

        # And the allergies section displays:
        allergies = [
            {"Allergy": "Penicillin", "Reaction Type": "Rash", "Severity": "Moderate"},
            {"Allergy": "Shellfish", "Reaction Type": "Unknown", "Severity": "Unknown"},
        ]
        for allergy in allergies:
            assert get_text(self.page, f"{allergy['Allergy']} Reaction") == allergy["Reaction Type"]
            assert get_text(self.page, f"{allergy['Allergy']} Severity") == allergy["Severity"]

        # And the current medications section shows:
        medications = [
            {"Medication": "Metoprolol", "Dosage": "50mg", "Status": "Active"},
            {"Medication": "Aspirin", "Dosage": "81mg", "Status": "Active"},
        ]
        for medication in medications:
            assert get_text(self.page, f"{medication['Medication']} Dosage") == medication["Dosage"]
            assert get_text(self.page, f"{medication['Medication']} Status") == medication["Status"]

    def test_access_patient_chart_during_active_treatment(self):
        # Given a patient "Michael Chen" is assigned to bed "ED-3"
        # And the patient is currently receiving active treatment
        # And recent assessments include:
        # (assumed pre-seeded test data)

        # When I open the patient's chart for "Michael Chen" in bed "ED-3"
        self.page.get_by_test_id("open-patient-chart-button").click()

        # Then the system displays real-time information with:
        summary_fields = [
            {"Section": "Current Status", "Content": "Active treatment in progress"},
            {"Section": "Most Recent Vitals", "Content": "BP: 130/80, HR: 88, T: 100.2°F (14:15)"},
            {"Section": "Active Orders", "Content": "Lab work in progress"},
            {"Section": "Triage Summary", "Content": "ESI 3 - Abd pain, onset 6h ago"},
        ]
        for row in summary_fields:
            assert get_text(self.page, row["Section"]) == row["Content"]

        # And all data includes timestamps showing data freshness
        wait_for_test_id(self.page, "Data Freshness Timestamp")

        # And any alerts or critical values are highlighted in red
        critical_value_element = wait_for_test_id(self.page, "Critical Value Highlight")
        assert critical_value_element.is_visible()

        # And pending lab results show "In Progress" status with expected completion time
        assert get_text(self.page, "Lab Result Status") == "In Progress"

    def test_view_patient_chart_with_medication_allergies_and_interactions(self):
        # Given a patient "Robert Johnson" is assigned to bed "ED-12"
        # And the patient has multiple drug allergies:
        # And current medications include:
        # (assumed pre-seeded test data)

        # When I open the patient's chart for "Robert Johnson"
        self.page.get_by_test_id("open-patient-chart-button").click()

        # Then the allergy section prominently displays:
        allergy_alerts = [
            {"Alert Type": "Critical Alert", "Message": "SEVERE ALLERGIES: Morphine, NSAIDs"},
            {"Alert Type": "Warning", "Message": "Moderate allergy: Codeine"},
        ]
        for alert in allergy_alerts:
            assert get_text(self.page, alert["Alert Type"]) == alert["Message"]

        # And the medication section shows:
        medications = [
            {"Medication": "Warfarin", "Status": "Active", "Interaction Alerts": "Monitor for bleeding risk"},
            {"Medication": "Metformin", "Status": "Active", "Interaction Alerts": "No interactions detected"},
        ]
        for medication in medications:
            assert get_text(self.page, f"{medication['Medication']} Status") == medication["Status"]
            assert (
                get_text(self.page, f"{medication['Medication']} Interaction Alerts")
                == medication["Interaction Alerts"]
            )

        # And any new medication orders will trigger allergy checking
        wait_for_test_id(self.page, "Allergy Checking Notice")

        # And interaction warnings are displayed for contraindicated drugs
        wait_for_test_id(self.page, "Interaction Warning")

    def test_access_chart_for_pediatric_patient_with_age_appropriate_data(self):
        # Given a pediatric patient "Emma Foster" (age 7) is assigned to bed "ED-PEDS-2"
        # And the nursing assessment includes pediatric-specific data:
        # (assumed pre-seeded test data)

        # When I open the pediatric patient's chart for "Emma Foster"
        self.page.get_by_test_id("open-patient-chart-button").click()

        # Then the system displays pediatric-specific information:
        pediatric_fields = [
            {"Section": "Patient Age/Weight", "Content": "7 years old, 22 kg"},
            {"Section": "Pediatric Vital Ranges", "Content": "All vitals with age-appropriate norms"},
            {"Section": "Growth Percentiles", "Content": "Weight: 50th percentile"},
            {"Section": "Guardian Information", "Content": "Sarah Foster (mother) - present"},
        ]
        for row in pediatric_fields:
            assert get_text(self.page, row["Section"]) == row["Content"]

        # And vital signs are displayed with pediatric normal ranges:
        vital_signs = [
            {"Vital Sign": "Blood Pressure", "Value": "95/60", "Status": "Normal"},
            {"Vital Sign": "Heart Rate", "Value": "110", "Status": "Normal"},
            {"Vital Sign": "Respiratory", "Value": "24", "Status": "Normal"},
            {"Vital Sign": "Temperature", "Value": "102.8°F", "Status": "Elevated"},
        ]
        for vital in vital_signs:
            assert get_text(self.page, vital["Vital Sign"]) == vital["Value"]
            assert get_text(self.page, f"{vital['Vital Sign']} Status") == vital["Status"]

        # And medication dosing shows weight-based calculations
        wait_for_test_id(self.page, "Weight-Based Dosing")

        # And parental consent status is clearly indicated
        wait_for_test_id(self.page, "Parental Consent Status")

    def test_handle_incomplete_nursing_assessment(self):
        # Given a patient "David Wilson" is assigned to bed "ED-6"
        # And the nursing assessment is partially completed:
        # (assumed pre-seeded test data)

        # When I open the patient's chart for "David Wilson"
        self.page.get_by_test_id("open-patient-chart-button").click()

        # Then the system displays available information clearly marked:
        assert get_text(self.page, "Completed Data") == "Triage notes, initial vitals available"

        missing_data_items = self.page.get_by_test_id("missing-data-item").all()
        assert len(missing_data_items) == 3
        missing_data_texts = [element.inner_text().strip() for element in missing_data_items]
        assert missing_data_texts == [
            "Allergies: Assessment in progress",
            "Medications: History pending",
            "Pain scale: Not yet assessed",
        ]

        # And incomplete sections are highlighted with:
        visual_indicators = [
            {"Visual Indicator": "Yellow Warning", "Description": "Assessment in progress"},
            {"Visual Indicator": "Refresh Timer", "Description": "Auto-refresh every 30 seconds"},
            {"Visual Indicator": "Notification", "Description": '"Assessment updating - refresh for latest"'},
        ]
        for indicator in visual_indicators:
            assert get_text(self.page, indicator["Visual Indicator"]) == indicator["Description"]

        # And I can request priority completion of missing critical data
        wait_for_test_id(self.page, "Request Priority Completion Button")

    def test_access_chart_during_shift_change_with_handoff_notes(self):
        # Given a patient "Lisa Brown" is assigned to bed "ED-9"
        # And it is during the evening shift change (19:00)
        # And the day shift nurse added handoff notes:
        # (assumed pre-seeded test data)

        # When I open the patient's chart for "Lisa Brown"
        self.page.get_by_test_id("open-patient-chart-button").click()

        # Then the system prominently displays shift handoff information:
        handoff_fields = [
            {"Handoff Section": "Clinical Summary", "Content": "Stable condition, pain controlled"},
            {"Handoff Section": "Pending Tasks", "Content": "Orthopedic consult ordered - pending"},
            {"Handoff Section": "Communication Log", "Content": "Family contact: Son updated 18:30"},
            {"Handoff Section": "Special Needs", "Content": "Patient preference: Female staff"},
        ]
        for field in handoff_fields:
            assert get_text(self.page, field["Handoff Section"]) == field["Content"]

        # And the handoff notes are clearly timestamped
        wait_for_test_id(self.page, "Handoff Notes Timestamp")

        # And I can add my own physician handoff notes
        wait_for_test_id(self.page, "Add Physician Handoff Notes")

        # And the evening nurse can see both nursing and physician handoff information
        wait_for_test_id(self.page, "Combined Handoff Information")

    def test_handle_patient_chart_access_during_network_connectivity_issues(self):
        # Given a patient "Thomas Anderson" is assigned to bed "ED-4"
        # And the mobile app has intermittent network connectivity
        # (assumed pre-seeded test data)

        # When I attempt to open the patient's chart
        self.page.get_by_test_id("open-patient-chart-button").click()

        # And the network connection is temporarily unavailable
        # (simulated network condition, no direct UI action)

        # Then the system displays cached patient data with:
        cached_data_fields = [
            {"Data Type": "Basic Demographics", "Availability": "Available (cached)"},
            {"Data Type": "Last Known Vitals", "Availability": "Available - last sync 16:45"},
            {"Data Type": "Medication Data", "Availability": "Available (cached)"},
            {"Data Type": "Recent Lab Results", "Availability": "May not be current - sync pending"},
        ]
        for field in cached_data_fields:
            assert get_text(self.page, field["Data Type"]) == field["Availability"]

        # And a connectivity warning is displayed: "Limited connectivity - data may not be current"
        assert get_text(self.page, "Connectivity Warning") == "Limited connectivity - data may not be current"

        # And the app attempts automatic sync when connection is restored
        wait_for_test_id(self.page, "Automatic Sync Status")

        # And critical data is prioritized for sync when connectivity returns
        wait_for_test_id(self.page, "Sync Priority Notice")

        # And I can manually trigger refresh when connection improves
        wait_for_test_id(self.page, "Manual Refresh Button")

    def test_access_chart_with_time_sensitive_alerts_and_notifications(self):
        # Given a patient "Karen White" is assigned to bed "ED-7"
        # And the patient has time-sensitive clinical alerts:
        # (assumed pre-seeded test data)

        # When I open the patient's chart for "Karen White"
        self.page.get_by_test_id("open-patient-chart-button").click()

        # Then the system prominently displays active alerts:
        active_alerts = [
            {"Alert Priority": "CRITICAL", "Alert Details": "🔴 Troponin 0.8 - Possible MI (17:15)"},
            {"Alert Priority": "WARNING", "Alert Details": "🟡 Medication due - Metoprolol (17:30)"},
            {"Alert Priority": "INFO", "Alert Details": "🔵 Pain reassessment overdue (17:25)"},
        ]
        for alert in active_alerts:
            assert get_text(self.page, alert["Alert Priority"]) == alert["Alert Details"]

        # And critical alerts require acknowledgment before proceeding
        wait_for_test_id(self.page, "Alert Acknowledgment")

        # And the timestamp shows how long ago each alert was generated
        wait_for_test_id(self.page, "Alert Timestamp")

        # And I can take direct action on alerts (order meds, document assessment)
        wait_for_test_id(self.page, "Alert Action Button")

        # And alert resolution is tracked and timestamped
        wait_for_test_id(self.page, "Alert Resolution Tracking")
