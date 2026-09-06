"""Selenium WebDriver + pytest test for spec/features/12-allergy-check.feature
(equivalent to tests-with-selenium-javascript/12-allergy-check.test.js).

Assumes the app exposes data-testid attributes matching each Gherkin field
label (kebab-cased, see support/fields.py) and the shared data-testid
contract in support/login.py (login-identity, login-submit, app-root).
"""

from selenium.webdriver.common.by import By

from support.build_driver import build_driver
from support.fields import fill_field, fill_fields, get_text, wait_for_test_id
from support.login import login, verify_system_is_operational


class TestAllergyCheck:
    @classmethod
    def setup_class(cls):
        cls.driver = build_driver()

    @classmethod
    def teardown_class(cls):
        cls.driver.quit()

    def setup_method(self):
        # Background:
        #   Given the emergency care system is operational
        #   And I am logged in as "Dr. Smith"
        #   And the allergy checking module is active
        #   And the drug interaction database is up-to-date
        verify_system_is_operational(self.driver)
        login(self.driver, "Dr. Smith")
        # The allergy checking module and the drug interaction database being
        # up-to-date are assumed to be pre-seeded test environment state.

        allergy_check_nav_link = wait_for_test_id(self.driver, "Nav Allergy Check")
        allergy_check_nav_link.click()
        wait_for_test_id(self.driver, "Allergy Check Panel")

    def test_prescribe_penicillin_to_patient_with_documented_penicillin_allergy(self):
        # Given a patient "Maria Rodriguez" is in bed "ED-8"
        # And the patient has documented allergies:
        #   | Allergy        | Reaction Type       | Severity  | Date Documented | Source     |
        #   | Penicillin     | Rash, hives        | Moderate  | 2023-05-15     | Patient    |
        #   | Shellfish      | Anaphylaxis        | Severe    | 2022-08-10     | Patient    |

        # When I enter a medication order for:
        #   | Medication     | Dose      | Route | Frequency | Duration |
        #   | Penicillin VK  | 500mg     | PO    | QID       | 10 days  |
        fill_fields(
            self.driver,
            [
                {"Field": "Medication", "Value": "Penicillin VK"},
                {"Field": "Dose", "Value": "500mg"},
                {"Field": "Route", "Value": "PO"},
                {"Field": "Frequency", "Value": "QID"},
                {"Field": "Duration", "Value": "10 days"},
            ],
        )
        # And I submit the order
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-medication-order"]').click()

        # Then the system displays an allergy warning:
        #   | Alert Type         | Details                                      |
        #   | DRUG ALLERGY       | ⚠️ ALLERGY ALERT: Patient allergic to Penicillin |
        #   | Severity Level     | Moderate                                     |
        #   | Reaction Type      | Rash, hives                                  |
        #   | Date Documented    | May 15, 2023                                |
        #   | Source             | Patient reported                             |
        wait_for_test_id(self.driver, "DRUG ALLERGY")
        assert get_text(self.driver, "DRUG ALLERGY") == "⚠️ ALLERGY ALERT: Patient allergic to Penicillin"
        assert get_text(self.driver, "Severity Level") == "Moderate"
        assert get_text(self.driver, "Reaction Type") == "Rash, hives"
        assert get_text(self.driver, "Date Documented") == "May 15, 2023"
        assert get_text(self.driver, "Source") == "Patient reported"

        # And the system blocks the order submission
        order_status = get_text(self.driver, "Order Status")
        assert "blocked" in order_status.lower()

        # And I am presented with options:
        #   | Option             | Description                                  |
        #   | Cancel Order       | Remove penicillin order                      |
        #   | Override with Reason| Document clinical justification            |
        #   | Alternative Drugs  | View suggested alternative antibiotics       |
        wait_for_test_id(self.driver, "Cancel Order")
        wait_for_test_id(self.driver, "Override with Reason")
        wait_for_test_id(self.driver, "Alternative Drugs")

        # And the allergy alert is logged in the audit trail
        audit_trail_entry = wait_for_test_id(self.driver, "Audit Trail Entry")
        assert audit_trail_entry.is_displayed()

    def test_prescribe_medication_with_no_documented_allergies(self):
        # Given a patient "John Taylor" is in bed "ED-12"
        # And the patient has no documented allergies

        # When I enter a medication order for:
        #   | Medication     | Dose      | Route | Frequency |
        #   | Amoxicillin    | 875mg     | PO    | BID       |
        fill_fields(
            self.driver,
            [
                {"Field": "Medication", "Value": "Amoxicillin"},
                {"Field": "Dose", "Value": "875mg"},
                {"Field": "Route", "Value": "PO"},
                {"Field": "Frequency", "Value": "BID"},
            ],
        )
        # And I submit the order
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-medication-order"]').click()

        # Then the system performs allergy checking
        # And no allergy alerts are triggered
        allergy_alerts = self.driver.find_elements(By.CSS_SELECTOR, '[data-testid="allergy-alert"]')
        assert len(allergy_alerts) == 0

        # And the order is processed normally
        # And the system displays confirmation:
        #   | Confirmation Type  | Message                                      |
        #   | No Allergies Found | No known allergies to Amoxicillin          |
        #   | Order Status       | Order submitted successfully                 |
        wait_for_test_id(self.driver, "No Allergies Found")
        assert get_text(self.driver, "No Allergies Found") == "No known allergies to Amoxicillin"
        assert get_text(self.driver, "Order Status") == "Order submitted successfully"

        # And the medication order is routed to pharmacy
        pharmacy_routing_status = get_text(self.driver, "Pharmacy Routing Status")
        assert "pharmacy" in pharmacy_routing_status.lower()

    def test_prescribe_medication_with_cross_reactive_allergy(self):
        # Given a patient "Sarah Johnson" is in bed "ED-5"
        # And the patient has documented allergies:
        #   | Allergy        | Reaction Type       | Severity  |
        #   | Penicillin     | Respiratory distress| Severe    |

        # When I enter a medication order for:
        #   | Medication     | Dose      | Route | Frequency |
        #   | Amoxicillin    | 500mg     | PO    | TID       |
        fill_fields(
            self.driver,
            [
                {"Field": "Medication", "Value": "Amoxicillin"},
                {"Field": "Dose", "Value": "500mg"},
                {"Field": "Route", "Value": "PO"},
                {"Field": "Frequency", "Value": "TID"},
            ],
        )
        # And I submit the order
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-medication-order"]').click()

        # Then the system displays a cross-reactivity warning:
        #   | Alert Type         | Details                                      |
        #   | CROSS-REACTIVITY   | ⚠️ WARNING: Cross-reactivity with Penicillin|
        #   | Known Allergy      | Patient allergic to Penicillin (Severe)     |
        #   | Cross-Reaction Risk| Amoxicillin is a penicillin derivative     |
        #   | Reaction Type      | Respiratory distress                         |
        #   | Risk Level         | High - Severe reaction possible              |
        wait_for_test_id(self.driver, "CROSS-REACTIVITY")
        assert get_text(self.driver, "CROSS-REACTIVITY") == "⚠️ WARNING: Cross-reactivity with Penicillin"
        assert get_text(self.driver, "Known Allergy") == "Patient allergic to Penicillin (Severe)"
        assert get_text(self.driver, "Cross-Reaction Risk") == "Amoxicillin is a penicillin derivative"
        assert get_text(self.driver, "Reaction Type") == "Respiratory distress"
        assert get_text(self.driver, "Risk Level") == "High - Severe reaction possible"

        # And the system provides additional information:
        #   | Information Type   | Content                                      |
        #   | Cross-Reaction Rate| 8-10% cross-reactivity with penicillin     |
        #   | Clinical Guidance  | Consider non-beta-lactam alternatives       |
        #   | Emergency Prep     | Have epinephrine available if administered   |
        assert get_text(self.driver, "Cross-Reaction Rate") == "8-10% cross-reactivity with penicillin"
        assert get_text(self.driver, "Clinical Guidance") == "Consider non-beta-lactam alternatives"
        assert get_text(self.driver, "Emergency Prep") == "Have epinephrine available if administered"

        # And I must acknowledge the cross-reactivity risk before proceeding
        wait_for_test_id(self.driver, "Acknowledge Cross-Reactivity Risk")

    def test_override_allergy_alert_with_clinical_justification(self):
        # Given a patient "Michael Chen" is in bed "ED-15"
        # And the patient has a documented penicillin allergy with "mild rash"
        # And the patient has severe sepsis requiring immediate antibiotic treatment

        # When I enter a penicillin order and receive an allergy alert
        fill_field(self.driver, "Medication", "Penicillin")
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-medication-order"]').click()
        wait_for_test_id(self.driver, "DRUG ALLERGY")
        # And I choose to override the allergy warning
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="override-allergy-warning"]').click()

        # Then the system requires detailed justification:
        #   | Required Field     | Description                                  |
        #   | Clinical Rationale | Why this medication is medically necessary   |
        #   | Risk Assessment    | Evaluation of allergy risk vs benefit       |
        #   | Monitoring Plan    | How allergic reactions will be monitored    |
        #   | Alternative Review | Why alternatives are not suitable            |
        wait_for_test_id(self.driver, "Clinical Rationale Description")
        assert get_text(self.driver, "Clinical Rationale Description") == "Why this medication is medically necessary"
        assert get_text(self.driver, "Risk Assessment Description") == "Evaluation of allergy risk vs benefit"
        assert get_text(self.driver, "Monitoring Plan Description") == "How allergic reactions will be monitored"
        assert get_text(self.driver, "Alternative Review Description") == "Why alternatives are not suitable"

        # And I document the override:
        #   | Field              | Value                                        |
        #   | Clinical Rationale | Life-threatening sepsis, first-line antibiotic needed |
        #   | Risk Assessment    | Mild rash risk acceptable vs sepsis mortality |
        #   | Monitoring Plan    | Continuous monitoring, diphenhydramine available |
        #   | Alternative Review | Other antibiotics inadequate for organism     |
        fill_fields(
            self.driver,
            [
                {"Field": "Clinical Rationale", "Value": "Life-threatening sepsis, first-line antibiotic needed"},
                {"Field": "Risk Assessment", "Value": "Mild rash risk acceptable vs sepsis mortality"},
                {"Field": "Monitoring Plan", "Value": "Continuous monitoring, diphenhydramine available"},
                {"Field": "Alternative Review", "Value": "Other antibiotics inadequate for organism"},
            ],
        )
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-override-documentation"]').click()

        # Then the system accepts the override
        wait_for_test_id(self.driver, "Override Status")
        override_status = get_text(self.driver, "Override Status")
        assert "accepted" in override_status.lower()

        # And logs the override decision with full documentation
        override_audit_log = wait_for_test_id(self.driver, "Override Audit Log")
        assert override_audit_log.is_displayed()

        # And notifies nursing staff of the allergy override for enhanced monitoring
        nursing_notification = get_text(self.driver, "Nursing Notification")
        assert "enhanced monitoring" in nursing_notification.lower()

    def test_check_allergies_for_multiple_medications_simultaneously(self):
        # Given a patient "Lisa Brown" is in bed "ED-7"
        # And the patient has documented allergies:
        #   | Allergy        | Reaction Type       | Severity  |
        #   | Morphine       | Respiratory depression | Severe |
        #   | NSAIDs         | GI bleeding        | Moderate  |

        # When I enter multiple medication orders:
        #   | Medication     | Dose      | Route | Purpose           |
        #   | Fentanyl       | 50mcg     | IV    | Pain control      |
        #   | Ibuprofen      | 600mg     | PO    | Anti-inflammatory |
        #   | Acetaminophen  | 650mg     | PO    | Pain/fever        |
        fill_fields(
            self.driver,
            [
                {"Field": "Medication 1", "Value": "Fentanyl"},
                {"Field": "Dose 1", "Value": "50mcg"},
                {"Field": "Route 1", "Value": "IV"},
                {"Field": "Purpose 1", "Value": "Pain control"},
                {"Field": "Medication 2", "Value": "Ibuprofen"},
                {"Field": "Dose 2", "Value": "600mg"},
                {"Field": "Route 2", "Value": "PO"},
                {"Field": "Purpose 2", "Value": "Anti-inflammatory"},
                {"Field": "Medication 3", "Value": "Acetaminophen"},
                {"Field": "Dose 3", "Value": "650mg"},
                {"Field": "Route 3", "Value": "PO"},
                {"Field": "Purpose 3", "Value": "Pain/fever"},
            ],
        )
        # And I submit all orders simultaneously
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-medication-orders"]').click()

        # Then the system checks each medication against documented allergies:
        #   | Medication     | Allergy Status | Alert Level |
        #   | Fentanyl       | No direct allergy | Safe      |
        #   | Ibuprofen      | NSAID allergy  | WARNING   |
        #   | Acetaminophen  | No allergy     | Safe      |
        wait_for_test_id(self.driver, "Fentanyl Alert Level")
        assert get_text(self.driver, "Fentanyl Allergy Status") == "No direct allergy"
        assert get_text(self.driver, "Fentanyl Alert Level") == "Safe"
        assert get_text(self.driver, "Ibuprofen Allergy Status") == "NSAID allergy"
        assert get_text(self.driver, "Ibuprofen Alert Level") == "WARNING"
        assert get_text(self.driver, "Acetaminophen Allergy Status") == "No allergy"
        assert get_text(self.driver, "Acetaminophen Alert Level") == "Safe"

        # And I receive specific alerts for problematic medications:
        #   | Alert Medication | Warning Message                              |
        #   | Ibuprofen        | Patient allergic to NSAIDs - GI bleeding risk |
        assert get_text(self.driver, "Ibuprofen Warning Message") == "Patient allergic to NSAIDs - GI bleeding risk"

        # And safe medications are processed without alerts
        fentanyl_warnings = self.driver.find_elements(By.CSS_SELECTOR, '[data-testid="fentanyl-warning-message"]')
        assert len(fentanyl_warnings) == 0

        # And I can review and modify orders before final submission
        wait_for_test_id(self.driver, "Review and Modify Orders")

    def test_handle_unknown_or_no_known_allergies_status(self):
        # Given a patient "Robert Davis" is in bed "ED-3"
        # And the patient's allergy status is "Unknown - Unable to assess"

        # When I enter a medication order for:
        #   | Medication     | Dose      | Route |
        #   | Cephalexin     | 500mg     | PO    |
        fill_fields(
            self.driver,
            [
                {"Field": "Medication", "Value": "Cephalexin"},
                {"Field": "Dose", "Value": "500mg"},
                {"Field": "Route", "Value": "PO"},
            ],
        )
        # And I submit the order
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-medication-order"]').click()

        # Then the system displays an information alert:
        #   | Alert Type         | Message                                      |
        #   | ALLERGY UNKNOWN    | ⚠️ INFO: Patient allergy status unknown     |
        #   | Risk Consideration | Cannot verify medication allergies          |
        #   | Recommendation     | Consider allergy assessment before administration |
        wait_for_test_id(self.driver, "ALLERGY UNKNOWN")
        assert get_text(self.driver, "ALLERGY UNKNOWN") == "⚠️ INFO: Patient allergy status unknown"
        assert get_text(self.driver, "Risk Consideration") == "Cannot verify medication allergies"
        assert get_text(self.driver, "Recommendation") == "Consider allergy assessment before administration"

        # And the system provides safety recommendations:
        #   | Recommendation     | Details                                      |
        #   | Allergy Assessment | Attempt to obtain allergy history           |
        #   | Start Monitoring   | Monitor for allergic reactions closely      |
        #   | Have Antidotes Ready| Ensure emergency medications available      |
        assert get_text(self.driver, "Allergy Assessment") == "Attempt to obtain allergy history"
        assert get_text(self.driver, "Start Monitoring") == "Monitor for allergic reactions closely"
        assert get_text(self.driver, "Have Antidotes Ready") == "Ensure emergency medications available"

        # And the order is flagged for enhanced allergy monitoring
        enhanced_monitoring_flag = get_text(self.driver, "Enhanced Monitoring Flag")
        assert "enhanced allergy monitoring" in enhanced_monitoring_flag.lower()

    def test_check_for_drug_class_allergies(self):
        # Given a patient "Jennifer Wilson" is in bed "ED-11"
        # And the patient has documented allergies:
        #   | Allergy        | Reaction Type       | Severity  | Drug Class |
        #   | Sulfa drugs    | Stevens-Johnson syndrome | Severe | Sulfonamides |

        # When I enter a medication order for:
        #   | Medication           | Dose      | Route | Drug Class    |
        #   | Trimethoprim-Sulfamethoxazole | 800mg | PO | Sulfonamide |
        fill_fields(
            self.driver,
            [
                {"Field": "Medication", "Value": "Trimethoprim-Sulfamethoxazole"},
                {"Field": "Dose", "Value": "800mg"},
                {"Field": "Route", "Value": "PO"},
                {"Field": "Drug Class", "Value": "Sulfonamide"},
            ],
        )
        # And I submit the order
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-medication-order"]').click()

        # Then the system identifies the drug class allergy:
        #   | Alert Type         | Details                                      |
        #   | DRUG CLASS ALLERGY | ⚠️ SEVERE: Patient allergic to Sulfa drugs |
        #   | Specific Drug      | TMP-SMX contains sulfamethoxazole           |
        #   | Reaction History   | Stevens-Johnson syndrome                     |
        #   | Severity           | Severe - Life-threatening reaction possible  |
        wait_for_test_id(self.driver, "DRUG CLASS ALLERGY")
        assert get_text(self.driver, "DRUG CLASS ALLERGY") == "⚠️ SEVERE: Patient allergic to Sulfa drugs"
        assert get_text(self.driver, "Specific Drug") == "TMP-SMX contains sulfamethoxazole"
        assert get_text(self.driver, "Reaction History") == "Stevens-Johnson syndrome"
        assert get_text(self.driver, "Severity") == "Severe - Life-threatening reaction possible"

        # And the system provides drug class education:
        #   | Information        | Content                                      |
        #   | Drug Class         | Sulfonamide antibiotics                     |
        #   | Cross-Reactivity   | All sulfa-containing medications at risk    |
        #   | Alternative Classes| Beta-lactams, fluoroquinolones available   |
        assert get_text(self.driver, "Drug Class") == "Sulfonamide antibiotics"
        assert get_text(self.driver, "Cross-Reactivity") == "All sulfa-containing medications at risk"
        assert get_text(self.driver, "Alternative Classes") == "Beta-lactams, fluoroquinolones available"

    def test_handle_allergy_information_from_multiple_sources(self):
        # Given a patient "David Kim" is in bed "ED-9"
        # And the patient has allergy information from multiple sources:
        #   | Source             | Allergy    | Reaction        | Reliability |
        #   | Patient Report     | Penicillin | "Bad reaction"  | Unverified  |
        #   | Medical Records    | Penicillin | Urticaria, rash | Verified    |
        #   | Family Member      | Codeine    | Nausea         | Unverified  |

        # When I enter a penicillin order
        fill_field(self.driver, "Medication", "Penicillin")
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-medication-order"]').click()

        # Then the system displays comprehensive allergy information:
        #   | Source Type        | Allergy Details                              |
        #   | Verified Record    | Penicillin - Urticaria, rash (Medical Records) |
        #   | Patient Report     | Penicillin - "Bad reaction" (Unverified)    |
        wait_for_test_id(self.driver, "Verified Record")
        assert get_text(self.driver, "Verified Record") == "Penicillin - Urticaria, rash (Medical Records)"
        assert get_text(self.driver, "Patient Report") == 'Penicillin - "Bad reaction" (Unverified)'

        # And the system prioritizes verified information in the alert
        # And provides source credibility indicators:
        #   | Source             | Credibility Level | Clinical Weight      |
        #   | Medical Records    | High reliability  | Primary consideration |
        #   | Patient Report     | Moderate reliability | Secondary consideration |
        assert get_text(self.driver, "Medical Records Credibility Level") == "High reliability"
        assert get_text(self.driver, "Medical Records Clinical Weight") == "Primary consideration"
        assert get_text(self.driver, "Patient Report Credibility Level") == "Moderate reliability"
        assert get_text(self.driver, "Patient Report Clinical Weight") == "Secondary consideration"

        # And I can review detailed allergy history before making decisions
        wait_for_test_id(self.driver, "Detailed Allergy History")

    def test_real_time_allergy_checking_during_order_modification(self):
        # Given a patient "Susan Martinez" is in bed "ED-4"
        # And the patient has a penicillin allergy
        # And I have started entering a medication order

        # When I begin typing "Pen" in the medication field
        fill_field(self.driver, "Medication", "Pen")

        # Then the system provides real-time allergy warnings:
        #   | Alert Type         | Message                                      |
        #   | PREDICTIVE ALERT   | ⚠️ Patient allergic to Penicillin          |
        #   | Medication Match   | "Pen" may be penicillin-related drug       |
        #   | Suggestion         | Consider alternative antibiotics            |
        wait_for_test_id(self.driver, "PREDICTIVE ALERT")
        assert get_text(self.driver, "PREDICTIVE ALERT") == "⚠️ Patient allergic to Penicillin"
        assert get_text(self.driver, "Medication Match") == '"Pen" may be penicillin-related drug'
        assert get_text(self.driver, "Suggestion") == "Consider alternative antibiotics"

        # And the system highlights potential allergy matches as I type
        wait_for_test_id(self.driver, "Allergy Match Highlight")

        # And provides alternative medication suggestions:
        #   | Alternative        | Drug Class        | Reason               |
        #   | Cephalexin         | Cephalosporin     | Lower cross-reactivity |
        #   | Azithromycin       | Macrolide         | No cross-reactivity   |
        #   | Ciprofloxacin      | Fluoroquinolone   | Different mechanism   |
        assert get_text(self.driver, "Cephalexin Drug Class") == "Cephalosporin"
        assert get_text(self.driver, "Cephalexin Reason") == "Lower cross-reactivity"
        assert get_text(self.driver, "Azithromycin Drug Class") == "Macrolide"
        assert get_text(self.driver, "Azithromycin Reason") == "No cross-reactivity"
        assert get_text(self.driver, "Ciprofloxacin Drug Class") == "Fluoroquinolone"
        assert get_text(self.driver, "Ciprofloxacin Reason") == "Different mechanism"

        # And I can select alternatives directly from the suggestion list
        wait_for_test_id(self.driver, "Alternative Suggestion List")
