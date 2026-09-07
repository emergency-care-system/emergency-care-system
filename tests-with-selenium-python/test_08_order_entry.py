"""Selenium WebDriver + pytest test for
tests-with-given-when-then-features/08-order-entry.feature
(equivalent to tests-with-selenium-javascript/08-order-entry.test.js).

Assumes the app exposes data-testid attributes matching each Gherkin field
label (kebab-cased, see support/fields.py) and the shared data-testid
contract in support/login.py (login-identity, login-submit, app-root).
"""

from selenium.webdriver.common.by import By

from support.build_driver import build_driver
from support.fields import fill_field, fill_fields, get_text, locator, wait_for_test_id
from support.login import login, verify_system_is_operational


class TestOrderEntry:
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
        #   And the electronic order entry module is active
        #   And departmental interfaces (lab, radiology, pharmacy) are connected
        verify_system_is_operational(self.driver)
        login(self.driver, "Dr. Smith")
        # The electronic order entry module being active and the departmental
        # interfaces being connected are assumed pre-seeded test data /
        # environment configuration.

        order_entry_nav_link = wait_for_test_id(self.driver, "Nav Order Entry")
        order_entry_nav_link.click()
        wait_for_test_id(self.driver, "Order Entry Panel")

    def test_enter_standard_orders_for_chest_pain_workup(self):
        # Given I have examined a patient "John Martinez" in bed "ED-5"
        # And the patient presents with "acute chest pain"
        # And the patient's allergies and contraindications have been reviewed
        # (assumed pre-seeded test data)

        # When I open the order entry module for the patient
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="open-order-entry-module-button"]').click()

        # And I enter the following laboratory orders:
        #   | Order Type     | Test Name        | Priority | Special Instructions |
        #   | Laboratory     | CBC with diff    | Routine  | None                |
        #   | Laboratory     | Troponin I       | STAT     | Serial in 6 hours   |
        #   | Laboratory     | Basic Metabolic  | Routine  | None                |
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="enter-order-button"]').click()

        # And I enter the following radiology order:
        #   | Order Type     | Study Name       | Priority | Special Instructions |
        #   | Radiology      | Chest X-ray PA/LAT| STAT    | R/O pneumonia       |
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="enter-order-button"]').click()

        # And I submit all orders with my electronic signature
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-orders-button"]').click()

        # Then the system sends electronic orders to the laboratory with details:
        #   | Order ID | Test Name     | Patient Info        | Priority | Timestamp |
        #   | LAB-001  | CBC with diff | John Martinez ED-5  | Routine  | Current   |
        #   | LAB-002  | Troponin I    | John Martinez ED-5  | STAT     | Current   |
        #   | LAB-003  | Basic Metabolic| John Martinez ED-5 | Routine  | Current   |
        lab_orders_sent = self.driver.find_elements(By.CSS_SELECTOR, '[data-testid="laboratory-order-sent"]')
        assert len(lab_orders_sent) == 3
        first_lab_order_text = lab_orders_sent[0].text
        assert "LAB-001" in first_lab_order_text

        # And the system sends electronic orders to radiology with details:
        #   | Order ID | Study Name    | Patient Info        | Priority | Timestamp |
        #   | RAD-001  | CXR PA/LAT    | John Martinez ED-5  | STAT     | Current   |
        radiology_orders_sent = self.driver.find_elements(By.CSS_SELECTOR, '[data-testid="radiology-order-sent"]')
        assert len(radiology_orders_sent) == 1

        # And specimen labels are automatically generated:
        #   | Label Type     | Content                                    |
        #   | Blood Draw     | John Martinez, DOB: 1975-08-15, ED-5     |
        #   | Test Codes     | CBC, Troponin, BMP                        |
        #   | Collection Time| STAT - Collect immediately                |
        #   | Barcode        | Patient and order identifiers             |
        specimen_labels = self.driver.find_elements(By.CSS_SELECTOR, '[data-testid="specimen-label"]')
        assert len(specimen_labels) == 4

        # And nursing tasks are added to the workflow:
        #   | Task Type           | Description                    | Priority | Due Time    |
        #   | Blood Collection    | Draw CBC, Troponin, BMP       | STAT     | Immediate   |
        #   | Patient Transport   | Transport to X-ray            | STAT     | After labs  |
        #   | Monitor Results     | Watch for critical values     | High     | Ongoing     |
        nursing_tasks = self.driver.find_elements(By.CSS_SELECTOR, '[data-testid="nursing-task"]')
        assert len(nursing_tasks) == 3

    def test_enter_orders_with_drug_allergy_checking(self):
        # Given I have examined a patient "Sarah Johnson" in bed "ED-8"
        # And the patient has documented allergies:
        #   | Allergy    | Reaction Type    | Severity |
        #   | Penicillin | Rash, hives      | Moderate |
        #   | Morphine   | Respiratory depression | Severe |
        # (assumed pre-seeded test data)

        # When I attempt to enter a medication order:
        #   | Order Type | Medication  | Dose     | Route | Frequency |
        #   | Medication | Amoxicillin | 500mg    | PO    | TID       |
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="enter-order-button"]').click()

        # And I submit the order
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-orders-button"]').click()

        # Then the system displays an allergy alert:
        wait_for_test_id(self.driver, "Drug Allergy")
        assert get_text(self.driver, "Drug Allergy") == "WARNING: Patient allergic to Penicillin"
        assert get_text(self.driver, "Severity") == "Moderate - Rash, hives"
        assert get_text(self.driver, "Cross-reaction") == "Amoxicillin contains penicillin"
        assert get_text(self.driver, "Recommendation") == "Consider alternative antibiotic"

        # And the order is held pending confirmation
        order_status = get_text(self.driver, "Order Status")
        assert "pending confirmation" in order_status.lower()

        # And I must either:
        #   | Action Option      | Description                                |
        #   | Override with reason| Document clinical justification          |
        #   | Cancel order       | Remove the problematic medication         |
        #   | Select alternative | Choose non-penicillin antibiotic          |
        allergy_action_options = self.driver.find_elements(By.CSS_SELECTOR, '[data-testid="allergy-action-option"]')
        assert len(allergy_action_options) == 3

        # And the allergy alert is logged in the patient record
        allergy_alert_log_entry = get_text(self.driver, "Allergy Alert Log Entry")
        assert len(allergy_alert_log_entry) > 0

    def test_enter_stat_orders_during_emergency_situation(self):
        # Given I have examined a patient "Emergency Patient" in bed "ED-TRAUMA-1"
        # And the patient is in critical condition with "severe trauma"
        # (assumed pre-seeded test data)

        # When I enter emergency orders:
        #   | Order Type     | Description           | Priority | Special Instructions    |
        #   | Laboratory     | Type and Crossmatch   | STAT     | 6 units PRBC on hold   |
        #   | Laboratory     | PT/INR, PTT          | STAT     | Pre-surgery labs       |
        #   | Radiology      | CT Head without contrast| STAT   | Rule out intracranial bleeding |
        #   | Radiology      | CT Chest/Abd/Pelvis  | STAT     | Trauma protocol        |
        #   | Medication     | Normal Saline        | STAT     | 1L wide open IV        |
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="enter-order-button"]').click()

        # And I mark all orders as "Emergency - Life threatening"
        fill_field(self.driver, "Order Marking", "Emergency - Life threatening")

        # And I submit the orders
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-orders-button"]').click()

        # Then all orders are immediately transmitted with highest priority
        order_transmission_status = get_text(self.driver, "Order Transmission Status")
        assert "highest priority" in order_transmission_status.lower()

        # And the laboratory receives orders marked "CRITICAL - TRAUMA"
        laboratory_order_marking = get_text(self.driver, "Laboratory Order Marking")
        assert laboratory_order_marking == "CRITICAL - TRAUMA"

        # And blood bank is notified to prepare emergency release protocol
        blood_bank_notification = get_text(self.driver, "Blood Bank Notification")
        assert "emergency release protocol" in blood_bank_notification.lower()

        # And radiology is alerted for trauma CT protocol
        radiology_alert = get_text(self.driver, "Radiology Alert")
        assert "trauma ct protocol" in radiology_alert.lower()

        # And nursing receives immediate action items:
        #   | Task               | Action Required           | Time Limit |
        #   | Blood Draw         | Collect trauma labs       | 5 minutes  |
        #   | IV Access          | Large bore IV x2          | Immediate  |
        #   | Patient Prep       | Prepare for CT transport  | 10 minutes |
        nursing_action_items = self.driver.find_elements(By.CSS_SELECTOR, '[data-testid="nursing-action-item"]')
        assert len(nursing_action_items) == 3

        # And all departments receive automatic status updates
        department_status_update = get_text(self.driver, "Department Status Update")
        assert len(department_status_update) > 0

    def test_enter_pediatric_orders_with_weight_based_dosing(self):
        # Given I have examined a pediatric patient "Tommy Chen" (age 5, weight 18kg) in bed "ED-PEDS-1"
        # And the patient presents with "febrile seizure"
        # (assumed pre-seeded test data)

        # When I enter pediatric medication orders:
        #   | Order Type | Medication | Dose Calculation        | Route | Frequency |
        #   | Medication | Acetaminophen| 15mg/kg (270mg)      | PO    | Q6H PRN   |
        #   | Medication | Lorazepam  | 0.1mg/kg (1.8mg)      | IV    | Once      |
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="enter-order-button"]').click()

        # And I enter diagnostic orders:
        #   | Order Type | Test/Study    | Pediatric Protocol    | Priority |
        #   | Laboratory | CBC with diff | Pediatric collection  | STAT     |
        #   | Laboratory | Blood glucose | Fingerstick acceptable| STAT     |
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="enter-order-button"]').click()

        # And I submit the pediatric orders
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-orders-button"]').click()

        # Then the system validates weight-based dosing calculations
        dosing_validation_status = get_text(self.driver, "Dosing Validation Status")
        assert "validated" in dosing_validation_status.lower()

        # And pediatric-specific protocols are applied:
        #   | Protocol Type      | Details                                |
        #   | Collection Volume  | Minimum blood volume for pediatric labs|
        #   | Dosing Alerts      | Maximum safe dose verified             |
        #   | Administration     | Child-friendly instructions           |
        pediatric_protocols = self.driver.find_elements(By.CSS_SELECTOR, '[data-testid="pediatric-protocol"]')
        assert len(pediatric_protocols) == 3

        # And nursing receives pediatric-specific tasks:
        #   | Task Type          | Pediatric Instructions                 |
        #   | Medication Admin   | Use pediatric dosing chart            |
        #   | Blood Collection   | Minimize collection volume            |
        #   | Comfort Measures   | Parent/caregiver involvement          |
        pediatric_nursing_tasks = self.driver.find_elements(By.CSS_SELECTOR, '[data-testid="pediatric-nursing-task"]')
        assert len(pediatric_nursing_tasks) == 3

        # And pharmacy receives weight-verified dosing information
        pharmacy_dosing_notification = get_text(self.driver, "Pharmacy Dosing Notification")
        assert len(pharmacy_dosing_notification) > 0

    def test_handle_order_modifications_and_cancellations(self):
        # Given I previously entered orders for patient "Maria Rodriguez" in bed "ED-12"
        # And the existing orders include:
        #   | Order ID | Order Type | Description    | Status      | Entered Time |
        #   | ORD-101  | Laboratory | CBC           | In Progress | 10:30        |
        #   | ORD-102  | Radiology  | Chest X-ray   | Pending     | 10:30        |
        #   | ORD-103  | Medication | Morphine 2mg  | Pending     | 10:30        |
        # (assumed pre-seeded test data)

        # When I need to modify the orders based on new clinical information
        # (no direct UI action for this narrative step)

        # And I cancel order "ORD-103" with reason "Patient reports morphine allergy"
        fill_fields(
            self.driver,
            [
                {"Field": "Order ID", "Value": "ORD-103"},
                {"Field": "Cancellation Reason", "Value": "Patient reports morphine allergy"},
            ],
        )
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="cancel-order-button"]').click()

        # And I modify order "ORD-102" to add "portable" due to patient instability
        fill_fields(
            self.driver,
            [
                {"Field": "Order ID", "Value": "ORD-102"},
                {"Field": "Modification", "Value": "Add portable"},
            ],
        )
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="modify-order-button"]').click()

        # And I add a new order for "Fentanyl 50mcg IV push"
        fill_field(self.driver, "New Order Description", "Fentanyl 50mcg IV push")
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="add-order-button"]').click()

        # Then the system processes the order changes:
        #   | Action Type | Order ID | New Status    | Reason/Details              |
        #   | Cancelled   | ORD-103  | Cancelled     | Morphine allergy discovered |
        #   | Modified    | ORD-102  | Updated       | Changed to portable CXR     |
        #   | New Order   | ORD-104  | Pending       | Fentanyl 50mcg IV push     |
        order_changes = self.driver.find_elements(By.CSS_SELECTOR, '[data-testid="order-change-entry"]')
        assert len(order_changes) == 3
        first_order_change_text = order_changes[0].text
        assert "ORD-103" in first_order_change_text

        # And notifications are sent to affected departments:
        #   | Department | Notification                               |
        #   | Pharmacy   | Morphine order cancelled - allergy        |
        #   | Radiology  | CXR modified to portable study            |
        #   | Nursing    | New pain medication order available       |
        department_notifications = self.driver.find_elements(By.CSS_SELECTOR, '[data-testid="department-notification"]')
        assert len(department_notifications) == 3

        # And an audit trail is maintained for all order changes
        audit_trail = self.driver.find_element(By.CSS_SELECTOR, '[data-testid="order-change-audit-trail"]')
        assert audit_trail.is_displayed()

    def test_enter_orders_with_insurance_authorization_requirements(self):
        # Given I have examined a patient "Robert Davis" in bed "ED-6"
        # And the patient has insurance requiring prior authorization for certain studies
        # (assumed pre-seeded test data)

        # When I enter an order for:
        #   | Order Type | Study Name | Estimated Cost | Insurance Notes        |
        #   | Radiology  | CT Abdomen | $1,200        | Requires pre-auth      |
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="enter-order-button"]').click()

        # And I submit the order
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-orders-button"]').click()

        # Then the system checks insurance requirements:
        assert get_text(self.driver, "Coverage Verification") == "CT covered with prior authorization"
        assert get_text(self.driver, "Authorization Status") == "Prior auth required"
        assert get_text(self.driver, "Alternative Options") == "Ultrasound covered without pre-auth"

        # And I am presented with options:
        #   | Option             | Description                            |
        #   | Submit for auth    | Send for insurance approval (delay)    |
        #   | Order alternative  | Consider ultrasound instead           |
        #   | Emergency override | Document medical necessity            |
        insurance_options = self.driver.find_elements(By.CSS_SELECTOR, '[data-testid="insurance-option"]')
        assert len(insurance_options) == 3

        # And the order status is marked "Pending Authorization"
        order_status = get_text(self.driver, "Order Status")
        assert order_status == "Pending Authorization"

        # And the patient financial counselor is notified
        financial_counselor_notification = get_text(self.driver, "Financial Counselor Notification")
        assert len(financial_counselor_notification) > 0

    def test_handle_order_entry_during_system_integration_failures(self):
        # Given I am entering orders for patient "Lisa Wong" in bed "ED-14"
        # And the laboratory information system is temporarily offline
        # (assumed pre-seeded test data)

        # When I enter laboratory orders:
        #   | Order Type | Test Name     | Priority |
        #   | Laboratory | Troponin      | STAT     |
        #   | Laboratory | CBC          | Routine  |
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="enter-order-button"]').click()

        # And I submit the orders
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="submit-orders-button"]').click()

        # Then the system displays a warning: "Lab system offline - orders will be queued"
        warning_message = get_text(self.driver, "Warning Message")
        assert warning_message == "Lab system offline - orders will be queued"

        # And the orders are stored locally with status "Queued for transmission"
        order_status = get_text(self.driver, "Order Status")
        assert order_status == "Queued for transmission"

        # And nursing is notified to manually coordinate with lab
        nursing_coordination_notification = get_text(self.driver, "Nursing Coordination Notification")
        assert len(nursing_coordination_notification) > 0

        # And I receive a notification when lab system connectivity is restored
        connectivity_restored_notification = get_text(self.driver, "Connectivity Restored Notification")
        assert len(connectivity_restored_notification) > 0

        # And queued orders are automatically transmitted when system is available
        queued_order_transmission_status = get_text(self.driver, "Queued Order Transmission Status")
        assert "transmitted" in queued_order_transmission_status.lower()

        # And manual backup procedures are documented for critical orders
        manual_backup_procedure_documentation = get_text(self.driver, "Manual Backup Procedure Documentation")
        assert len(manual_backup_procedure_documentation) > 0

    def test_enter_complex_order_sets_for_specific_protocols(self):
        # Given I have examined a patient "James Thompson" in bed "ED-11"
        # And the patient presents with "suspected stroke"
        # (assumed pre-seeded test data)

        # When I select the "Acute Stroke Protocol" order set
        fill_field(self.driver, "Order Set", "Acute Stroke Protocol")

        # Then the system presents the standardized stroke workup orders:
        #   | Category   | Order Description              | Priority | Default |
        #   | Laboratory | CBC, BMP, PT/INR, PTT         | STAT     | Selected|
        #   | Laboratory | Troponin, Lipid panel         | STAT     | Selected|
        #   | Radiology  | CT Head without contrast      | STAT     | Selected|
        #   | Radiology  | CT Angiogram head/neck        | STAT     | Optional|
        #   | Medication | Aspirin 325mg                 | STAT     | Selected|
        #   | Consults   | Neurology consult             | STAT     | Selected|
        stroke_protocol_orders = self.driver.find_elements(By.CSS_SELECTOR, '[data-testid="stroke-protocol-order"]')
        assert len(stroke_protocol_orders) == 6

        # And I can modify or remove individual orders from the set
        wait_for_test_id(self.driver, "Modify Order")
        wait_for_test_id(self.driver, "Remove Order")

        # And I add stroke-specific timing requirements:
        #   | Order          | Time Requirement                      |
        #   | CT Head        | Within 25 minutes of arrival         |
        #   | Lab results    | Within 45 minutes of arrival         |
        #   | Neurology      | Consult within 15 minutes           |
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="add-timing-requirement-button"]').click()

        # And the system tracks compliance with stroke protocol timing
        protocol_compliance_tracking_status = get_text(self.driver, "Protocol Compliance Tracking Status")
        assert "tracks compliance" in protocol_compliance_tracking_status.lower()

        # And automatic reminders are set for time-sensitive elements
        automatic_reminders = self.driver.find_elements(By.CSS_SELECTOR, '[data-testid="automatic-reminder"]')
        assert len(automatic_reminders) == 3
