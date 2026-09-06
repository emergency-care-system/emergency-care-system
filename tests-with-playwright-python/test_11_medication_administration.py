"""Playwright + pytest test for spec/features/11-medication-administration.feature
(equivalent to tests-with-playwright-javascript/11-medication-administration.test.js).

Assumes the app exposes data-testid attributes matching each Gherkin field
label (kebab-cased, see support/fields.py) and the shared data-testid
contract in support/login.py (login-identity, login-submit, app-root).
"""

from playwright.sync_api import sync_playwright

from support.fields import fill_field, fill_fields, get_text, wait_for_test_id
from support.login import login, verify_system_is_operational


class TestMedicationAdministration:
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
        #   And I am logged in as "Nurse Johnson"
        #   And the medication administration module is active
        #   And the barcode scanning system is functional
        #   And the medication administration record (MAR) is accessible
        verify_system_is_operational(self.page)
        login(self.page, "Nurse Johnson")
        # The medication administration module, barcode scanning system,
        # and MAR accessibility are assumed to be pre-seeded test
        # environment state.

        medication_administration_nav_link = wait_for_test_id(self.page, "Nav Medication Administration")
        medication_administration_nav_link.click()
        wait_for_test_id(self.page, "Medication Administration Panel")

    def test_successfully_administer_scheduled_medication_with_barcode_verification(self):
        # Given a patient "Maria Gonzalez" is in bed "ED-8"
        # And the patient has a prescribed medication:
        #   | Medication    | Dose      | Route | Frequency | Scheduled Time | Status    |
        #   | Metoprolol    | 25mg      | PO    | BID       | 14:00         | Due       |
        # And the patient is wearing a wristband with barcode "PT-12345"
        # And I have the medication with barcode "MED-METOPROLOL-25MG"

        # When I scan the patient's wristband barcode "PT-12345"
        self.page.get_by_test_id("scan-wristband-barcode").click()
        # And I scan the medication barcode "MED-METOPROLOL-25MG"
        self.page.get_by_test_id("scan-medication-barcode").click()

        # Then the system verifies the five rights of medication administration:
        #   | Right          | Verification                                  | Status   |
        #   | Right Patient  | Maria Gonzalez confirmed via wristband       | ✓ Valid  |
        #   | Right Drug     | Metoprolol matches prescribed medication     | ✓ Valid  |
        #   | Right Dose     | 25mg matches prescribed dose                 | ✓ Valid  |
        #   | Right Route    | PO (oral) matches prescribed route           | ✓ Valid  |
        #   | Right Time     | Within acceptable window (13:30-14:30)      | ✓ Valid  |
        wait_for_test_id(self.page, "Right Patient Status")
        assert get_text(self.page, "Right Patient Status") == "✓ Valid"
        assert get_text(self.page, "Right Drug Status") == "✓ Valid"
        assert get_text(self.page, "Right Dose Status") == "✓ Valid"
        assert get_text(self.page, "Right Route Status") == "✓ Valid"
        assert get_text(self.page, "Right Time Status") == "✓ Valid"

        # And the system displays confirmation screen:
        #   | Field             | Value                                        |
        #   | Patient           | Maria Gonzalez, DOB: 1975-08-15            |
        #   | Medication        | Metoprolol 25mg                            |
        #   | Route             | PO (By mouth)                              |
        #   | Administration Time| 14:05                                      |
        assert get_text(self.page, "Patient") == "Maria Gonzalez, DOB: 1975-08-15"
        assert get_text(self.page, "Medication") == "Metoprolol 25mg"
        assert get_text(self.page, "Route") == "PO (By mouth)"
        assert get_text(self.page, "Administration Time") == "14:05"

        # And I confirm the administration
        self.page.get_by_test_id("confirm-administration").click()

        # Then the system records the administration with timestamp:
        #   | Field             | Value                                        |
        #   | Patient ID        | PT-12345                                   |
        #   | Medication        | Metoprolol 25mg PO                         |
        #   | Administered By   | Nurse Johnson                              |
        #   | Administration Time| 2025-06-24 14:05:32                       |
        #   | Verification Method| Barcode scan                              |
        wait_for_test_id(self.page, "Patient ID")
        assert get_text(self.page, "Patient ID") == "PT-12345"
        assert get_text(self.page, "Medication") == "Metoprolol 25mg PO"
        assert get_text(self.page, "Administered By") == "Nurse Johnson"
        assert get_text(self.page, "Administration Time") == "2025-06-24 14:05:32"
        assert get_text(self.page, "Verification Method") == "Barcode scan"

        # And the medication status is updated to "Given"
        assert get_text(self.page, "Medication Status") == "Given"
        # And the next dose is automatically scheduled for "02:00 tomorrow"
        assert get_text(self.page, "Next Dose Scheduled") == "02:00 tomorrow"

    def test_handle_medication_administration_with_allergy_alert(self):
        # Given a patient "Robert Chen" is in bed "ED-12"
        # And the patient has documented allergies:
        #   | Allergy       | Reaction Type      | Severity  |
        #   | Penicillin    | Rash, hives       | Moderate  |
        #   | Morphine      | Respiratory depression | Severe |
        # And there is a prescribed medication:
        #   | Medication    | Dose      | Route | Prescriber |
        #   | Amoxicillin   | 500mg     | PO    | Dr. Smith  |

        # When I scan the patient's wristband barcode
        self.page.get_by_test_id("scan-wristband-barcode").click()
        # And I scan the amoxicillin medication barcode
        self.page.get_by_test_id("scan-medication-barcode").click()

        # Then the system displays an allergy alert:
        #   | Alert Type        | Message                                      |
        #   | DRUG ALLERGY      | ⚠️ WARNING: Patient allergic to Penicillin |
        #   | Cross-Reaction    | Amoxicillin contains penicillin             |
        #   | Severity          | Moderate - Rash, hives                      |
        #   | Recommendation    | Contact physician before administration      |
        wait_for_test_id(self.page, "DRUG ALLERGY")
        assert get_text(self.page, "DRUG ALLERGY") == "⚠️ WARNING: Patient allergic to Penicillin"
        assert get_text(self.page, "Cross-Reaction") == "Amoxicillin contains penicillin"
        assert get_text(self.page, "Severity") == "Moderate - Rash, hives"
        assert get_text(self.page, "Recommendation") == "Contact physician before administration"

        # And the system requires additional verification:
        #   | Verification Step | Requirement                                  |
        #   | Physician Approval| Must have override from prescribing MD      |
        #   | Documentation     | Must document reason for override           |
        #   | Monitoring Plan   | Must specify allergy monitoring protocol    |
        assert get_text(self.page, "Physician Approval") == "Must have override from prescribing MD"
        assert get_text(self.page, "Documentation") == "Must document reason for override"
        assert get_text(self.page, "Monitoring Plan") == "Must specify allergy monitoring protocol"

        # And the medication administration is held pending physician confirmation
        assert "pending physician confirmation" in get_text(self.page, "Administration Status").lower()
        # And an alert is sent to the prescribing physician "Dr. Smith"
        assert "Dr. Smith" in get_text(self.page, "Physician Alert")

    def test_administer_prn_medication_with_clinical_assessment(self):
        # Given a patient "Jennifer Lopez" is in bed "ED-6"
        # And there is a PRN medication order:
        #   | Medication     | Dose    | Route | Indication           | Max Frequency |
        #   | Morphine       | 2mg     | IV    | Pain score ≥7/10     | Q4H PRN       |
        # And the patient's current pain score is "8/10"
        # And the last morphine dose was given 5 hours ago

        # When I scan the patient's wristband barcode
        self.page.get_by_test_id("scan-wristband-barcode").click()
        # And I scan the morphine medication barcode
        self.page.get_by_test_id("scan-medication-barcode").click()

        # Then the system verifies PRN medication criteria:
        #   | Criteria          | Assessment                                   | Status   |
        #   | Clinical Indication| Pain score 8/10 meets threshold ≥7/10      | ✓ Met    |
        #   | Frequency Limit   | Last dose 5h ago, within Q4H limit         | ✓ Valid  |
        #   | Patient Safety    | No respiratory contraindications            | ✓ Safe   |
        wait_for_test_id(self.page, "Clinical Indication Status")
        assert get_text(self.page, "Clinical Indication Status") == "✓ Met"
        assert get_text(self.page, "Frequency Limit Status") == "✓ Valid"
        assert get_text(self.page, "Patient Safety Status") == "✓ Safe"

        # And I document the clinical assessment:
        #   | Assessment Field  | Value                                        |
        #   | Pain Score        | 8/10                                        |
        #   | Pain Location     | Chest                                       |
        #   | Pain Quality      | Sharp, stabbing                             |
        #   | Vital Signs       | BP: 140/85, HR: 95, RR: 18, O2: 96%       |
        fill_fields(
            self.page,
            [
                {"Field": "Pain Score", "Value": "8/10"},
                {"Field": "Pain Location", "Value": "Chest"},
                {"Field": "Pain Quality", "Value": "Sharp, stabbing"},
                {"Field": "Vital Signs", "Value": "BP: 140/85, HR: 95, RR: 18, O2: 96%"},
            ],
        )

        # And I confirm the PRN administration
        self.page.get_by_test_id("confirm-administration").click()

        # Then the system records the administration with clinical justification:
        #   | Documentation     | Details                                      |
        #   | PRN Indication    | Pain score 8/10, patient requested relief   |
        #   | Clinical Assessment| Stable vitals, no respiratory distress     |
        #   | Next Available    | Not before 18:15 (Q4H)                     |
        wait_for_test_id(self.page, "PRN Indication")
        assert get_text(self.page, "PRN Indication") == "Pain score 8/10, patient requested relief"
        assert get_text(self.page, "Clinical Assessment") == "Stable vitals, no respiratory distress"
        assert get_text(self.page, "Next Available") == "Not before 18:15 (Q4H)"

    def test_handle_medication_administration_timing_variances(self):
        # Given a patient "David Kim" is in bed "ED-3"
        # And there is a scheduled medication:
        #   | Medication    | Scheduled Time | Acceptable Window | Current Time |
        #   | Insulin       | 12:00         | ±30 minutes       | 13:15        |

        # When I scan the patient's wristband barcode
        self.page.get_by_test_id("scan-wristband-barcode").click()
        # And I scan the insulin medication barcode
        self.page.get_by_test_id("scan-medication-barcode").click()

        # Then the system detects a timing variance:
        #   | Variance Type     | Details                                      |
        #   | Late Administration| 75 minutes past scheduled time             |
        #   | Outside Window    | Beyond acceptable ±30 minute window         |
        #   | Clinical Risk     | Delayed insulin may affect glucose control  |
        wait_for_test_id(self.page, "Late Administration")
        assert get_text(self.page, "Late Administration") == "75 minutes past scheduled time"
        assert get_text(self.page, "Outside Window") == "Beyond acceptable ±30 minute window"
        assert get_text(self.page, "Clinical Risk") == "Delayed insulin may affect glucose control"

        # And the system requires variance documentation:
        #   | Required Field    | Purpose                                      |
        #   | Delay Reason      | Why medication was not given on time        |
        #   | Patient Status    | Current clinical condition assessment       |
        #   | Physician Notification| Whether MD was contacted about delay    |
        assert get_text(self.page, "Delay Reason Purpose") == "Why medication was not given on time"
        assert get_text(self.page, "Patient Status Purpose") == "Current clinical condition assessment"
        assert get_text(self.page, "Physician Notification Purpose") == "Whether MD was contacted about delay"

        # And I document the variance:
        #   | Field             | Value                                        |
        #   | Delay Reason      | Patient NPO for procedure until 13:00      |
        #   | Patient Assessment| Blood glucose 140 mg/dL, stable            |
        #   | MD Notified       | Dr. Patel aware of delay                    |
        fill_fields(
            self.page,
            [
                {"Field": "Delay Reason", "Value": "Patient NPO for procedure until 13:00"},
                {"Field": "Patient Assessment", "Value": "Blood glucose 140 mg/dL, stable"},
                {"Field": "MD Notified", "Value": "Dr. Patel aware of delay"},
            ],
        )

        # Then the administration is recorded with variance documentation
        wait_for_test_id(self.page, "Variance Documentation Status")
        assert len(get_text(self.page, "Variance Documentation Status")) > 0
        # And the next scheduled dose timing is adjusted accordingly
        assert len(get_text(self.page, "Next Scheduled Dose")) > 0

    def test_administer_high_risk_medication_with_double_verification(self):
        # Given a patient "Susan Williams" is in bed "ED-15"
        # And there is a high-risk medication order:
        #   | Medication    | Dose      | Route | Risk Category        | Special Requirements |
        #   | Heparin       | 5000 units| IV    | High-alert drug      | Double verification  |

        # When I scan the patient's wristband barcode
        self.page.get_by_test_id("scan-wristband-barcode").click()
        # And I scan the heparin medication barcode
        self.page.get_by_test_id("scan-medication-barcode").click()

        # Then the system flags the high-risk medication:
        #   | Alert Type        | Message                                      |
        #   | HIGH-ALERT DRUG   | 🔴 Heparin requires double verification     |
        #   | Risk Factors      | Bleeding risk, dosing errors common         |
        #   | Requirements      | Second nurse must verify before administration|
        wait_for_test_id(self.page, "HIGH-ALERT DRUG")
        assert get_text(self.page, "HIGH-ALERT DRUG") == "🔴 Heparin requires double verification"
        assert get_text(self.page, "Risk Factors") == "Bleeding risk, dosing errors common"
        assert get_text(self.page, "Requirements") == "Second nurse must verify before administration"

        # And the system requires independent double verification:
        #   | Verification Step | Nurse 1 (Primary) | Nurse 2 (Verifying) |
        #   | Patient Identity  | Nurse Johnson      | Pending             |
        #   | Medication        | Verified           | Pending             |
        #   | Dose Calculation  | 5000 units         | Pending             |
        #   | Route/Rate        | IV bolus           | Pending             |
        assert get_text(self.page, "Patient Identity Primary Nurse") == "Nurse Johnson"
        assert get_text(self.page, "Patient Identity Verifying Nurse") == "Pending"
        assert get_text(self.page, "Medication Primary Nurse") == "Verified"
        assert get_text(self.page, "Medication Verifying Nurse") == "Pending"
        assert get_text(self.page, "Dose Calculation Primary Nurse") == "5000 units"
        assert get_text(self.page, "Dose Calculation Verifying Nurse") == "Pending"
        assert get_text(self.page, "Route/Rate Primary Nurse") == "IV bolus"
        assert get_text(self.page, "Route/Rate Verifying Nurse") == "Pending"

        # When "Nurse Martinez" performs the second verification
        fill_field(self.page, "Verifying Nurse", "Nurse Martinez")
        self.page.get_by_test_id("submit-second-verification").click()
        # And both nurses confirm the administration
        self.page.get_by_test_id("confirm-administration").click()

        # Then the system records dual verification:
        #   | Field             | Value                                        |
        #   | Primary Nurse     | Nurse Johnson                               |
        #   | Verifying Nurse   | Nurse Martinez                              |
        #   | Verification Time | 2025-06-24 15:30:15                        |
        #   | Both Signatures   | Electronic signatures captured              |
        wait_for_test_id(self.page, "Primary Nurse")
        assert get_text(self.page, "Primary Nurse") == "Nurse Johnson"
        assert get_text(self.page, "Verifying Nurse") == "Nurse Martinez"
        assert get_text(self.page, "Verification Time") == "2025-06-24 15:30:15"
        assert get_text(self.page, "Both Signatures") == "Electronic signatures captured"

    def test_handle_medication_barcode_scanning_errors(self):
        # Given a patient "Michael Davis" is in bed "ED-11"
        # And I need to administer prescribed medication

        # When I scan the patient's wristband barcode successfully
        self.page.get_by_test_id("scan-wristband-barcode").click()
        # And I attempt to scan a medication barcode that is damaged/unreadable
        self.page.get_by_test_id("scan-medication-barcode").click()

        # Then the system displays a barcode error:
        #   | Error Type        | Message                                      |
        #   | Barcode Unreadable| Unable to scan medication barcode           |
        #   | Manual Options    | Enter medication manually or get replacement|
        #   | Safety Warning    | Manual entry bypasses barcode verification  |
        wait_for_test_id(self.page, "Barcode Unreadable")
        assert get_text(self.page, "Barcode Unreadable") == "Unable to scan medication barcode"
        assert get_text(self.page, "Manual Options") == "Enter medication manually or get replacement"
        assert get_text(self.page, "Safety Warning") == "Manual entry bypasses barcode verification"

        # And I choose to manually enter the medication information:
        #   | Manual Entry Field| Value                                        |
        #   | Medication Name   | Tylenol                                     |
        #   | Strength          | 650mg                                       |
        #   | NDC Number        | 50580-506-02                               |
        #   | Lot Number        | ABC123                                      |
        #   | Expiration Date   | 2026-12-31                                 |
        fill_fields(
            self.page,
            [
                {"Field": "Medication Name", "Value": "Tylenol"},
                {"Field": "Strength", "Value": "650mg"},
                {"Field": "NDC Number", "Value": "50580-506-02"},
                {"Field": "Lot Number", "Value": "ABC123"},
                {"Field": "Expiration Date", "Value": "2026-12-31"},
            ],
        )

        # Then the system validates the manual entry against the order
        manual_entry_validation = wait_for_test_id(self.page, "Manual Entry Validation")
        assert manual_entry_validation.is_visible()
        # And requires supervisor override for manual medication entry
        wait_for_test_id(self.page, "Supervisor Override")
        # And documents the barcode scanning issue for pharmacy review
        pharmacy_review_flag = wait_for_test_id(self.page, "Pharmacy Review Flag")
        assert pharmacy_review_flag.is_visible()

    def test_administer_pediatric_medication_with_weight_based_verification(self):
        # Given a pediatric patient "Emma Foster" (age 5, weight 18kg) is in bed "ED-PEDS-1"
        # And there is a weight-based medication order:
        #   | Medication     | Dose Calculation    | Prescribed Dose | Route |
        #   | Acetaminophen  | 15mg/kg            | 270mg          | PO    |

        # When I scan the patient's wristband barcode
        self.page.get_by_test_id("scan-wristband-barcode").click()
        # And I scan the acetaminophen medication barcode
        self.page.get_by_test_id("scan-medication-barcode").click()

        # Then the system verifies pediatric dosing:
        #   | Verification       | Calculation                                  | Status   |
        #   | Weight Confirmation| Patient weight: 18kg                        | ✓ Valid  |
        #   | Dose Calculation   | 15mg/kg × 18kg = 270mg                     | ✓ Correct|
        #   | Maximum Safe Dose  | 270mg < 400mg max (safe)                    | ✓ Safe   |
        #   | Age Appropriateness| Acetaminophen approved for age 5            | ✓ Valid  |
        wait_for_test_id(self.page, "Weight Confirmation Status")
        assert get_text(self.page, "Weight Confirmation Status") == "✓ Valid"
        assert get_text(self.page, "Dose Calculation Status") == "✓ Correct"
        assert get_text(self.page, "Maximum Safe Dose Status") == "✓ Safe"
        assert get_text(self.page, "Age Appropriateness Status") == "✓ Valid"

        # And the system displays pediatric-specific information:
        #   | Field              | Value                                        |
        #   | Patient Age/Weight | 5 years old, 18kg                          |
        #   | Calculation Shown  | 15mg/kg × 18kg = 270mg                     |
        #   | Liquid Formulation | 160mg/5mL suspension                       |
        #   | Volume to Give     | 8.4mL                                       |
        assert get_text(self.page, "Patient Age/Weight") == "5 years old, 18kg"
        assert get_text(self.page, "Calculation Shown") == "15mg/kg × 18kg = 270mg"
        assert get_text(self.page, "Liquid Formulation") == "160mg/5mL suspension"
        assert get_text(self.page, "Volume to Give") == "8.4mL"

        # And I confirm the pediatric administration
        self.page.get_by_test_id("confirm-administration").click()

        # Then the system records with pediatric-specific documentation
        pediatric_documentation = wait_for_test_id(self.page, "Pediatric Documentation")
        assert pediatric_documentation.is_visible()

    def test_handle_medication_administration_during_code_blue_emergency(self):
        # Given a patient "Crisis Patient" is in bed "ED-TRAUMA-1"
        # And a code blue emergency is in progress
        # And emergency medications are ordered:
        #   | Medication    | Dose      | Route | Urgency    |
        #   | Epinephrine   | 1mg       | IV    | STAT       |
        #   | Atropine      | 0.5mg     | IV    | STAT       |

        # When I scan the patient's wristband during the emergency
        self.page.get_by_test_id("scan-wristband-barcode").click()
        # And I scan the epinephrine medication barcode
        self.page.get_by_test_id("scan-medication-barcode").click()

        # Then the system activates emergency administration mode:
        #   | Emergency Feature | Behavior                                     |
        #   | Rapid Verification| Abbreviated safety checks for life-saving   |
        #   | Time Documentation| Precise timestamp for code blue timeline    |
        #   | Team Notification | Alert code team of medication administration |
        wait_for_test_id(self.page, "Rapid Verification")
        assert get_text(self.page, "Rapid Verification") == "Abbreviated safety checks for life-saving"
        assert get_text(self.page, "Time Documentation") == "Precise timestamp for code blue timeline"
        assert get_text(self.page, "Team Notification") == "Alert code team of medication administration"

        # And the system allows emergency override of timing restrictions
        wait_for_test_id(self.page, "Emergency Override")

        # And records the administration with code blue context:
        #   | Field             | Value                                        |
        #   | Emergency Context | Code Blue - Cardiac arrest                  |
        #   | Rapid Administration| Life-saving intervention                   |
        #   | Code Blue Timeline| 15:45:32 - Epinephrine given               |
        assert get_text(self.page, "Emergency Context") == "Code Blue - Cardiac arrest"
        assert get_text(self.page, "Rapid Administration") == "Life-saving intervention"
        assert get_text(self.page, "Code Blue Timeline") == "15:45:32 - Epinephrine given"

        # And the code blue medication log is automatically updated
        code_blue_medication_log = wait_for_test_id(self.page, "Code Blue Medication Log")
        assert code_blue_medication_log.is_visible()
