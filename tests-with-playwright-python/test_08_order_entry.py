"""Playwright + pytest test for spec/features/08-order-entry.feature
(equivalent to tests-with-playwright-javascript/08-order-entry.test.js).

Assumes the app exposes data-testid attributes matching each Gherkin field
label (kebab-cased, see support/fields.py) and the shared data-testid
contract in support/login.py (login-identity, login-submit, app-root).
"""

from playwright.sync_api import sync_playwright

from support.fields import fill_field, fill_fields, get_text, wait_for_test_id
from support.login import login, verify_system_is_operational


class TestOrderEntry:
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
        #   And I am logged in as "Dr. Smith"
        #   And the electronic order entry module is active
        #   And departmental interfaces (lab, radiology, pharmacy) are connected
        verify_system_is_operational(self.page)
        login(self.page, "Dr. Smith")
        # The electronic order entry module being active and the
        # departmental interfaces being connected are assumed pre-seeded
        # test data / environment configuration.

        order_entry_nav_link = wait_for_test_id(self.page, "Nav Order Entry")
        order_entry_nav_link.click()
        wait_for_test_id(self.page, "Order Entry Panel")

    def test_enter_standard_orders_for_chest_pain_workup(self):
        # Given I have examined a patient "John Martinez" in bed "ED-5"
        # And the patient presents with "acute chest pain"
        # And the patient's allergies and contraindications have been reviewed
        # (assumed pre-seeded test data)

        # When I open the order entry module for the patient
        self.page.get_by_test_id("open-order-entry-module-button").click()

        # And I enter the following laboratory orders:
        #   | Order Type     | Test Name        | Priority | Special Instructions |
        #   | Laboratory     | CBC with diff    | Routine  | None                |
        #   | Laboratory     | Troponin I       | STAT     | Serial in 6 hours   |
        #   | Laboratory     | Basic Metabolic  | Routine  | None                |
        self.page.get_by_test_id("enter-order-button").click()

        # And I enter the following radiology order:
        #   | Order Type     | Study Name       | Priority | Special Instructions |
        #   | Radiology      | Chest X-ray PA/LAT| STAT    | R/O pneumonia       |
        self.page.get_by_test_id("enter-order-button").click()

        # And I submit all orders with my electronic signature
        self.page.get_by_test_id("submit-orders-button").click()

        # Then the system sends electronic orders to the laboratory with details:
        #   | Order ID | Test Name     | Patient Info        | Priority | Timestamp |
        #   | LAB-001  | CBC with diff | John Martinez ED-5  | Routine  | Current   |
        #   | LAB-002  | Troponin I    | John Martinez ED-5  | STAT     | Current   |
        #   | LAB-003  | Basic Metabolic| John Martinez ED-5 | Routine  | Current   |
        lab_orders_sent = self.page.get_by_test_id("laboratory-order-sent").all()
        assert len(lab_orders_sent) == 3
        first_lab_order_text = lab_orders_sent[0].inner_text()
        assert "LAB-001" in first_lab_order_text

        # And the system sends electronic orders to radiology with details:
        #   | Order ID | Study Name    | Patient Info        | Priority | Timestamp |
        #   | RAD-001  | CXR PA/LAT    | John Martinez ED-5  | STAT     | Current   |
        radiology_orders_sent = self.page.get_by_test_id("radiology-order-sent").all()
        assert len(radiology_orders_sent) == 1

        # And specimen labels are automatically generated:
        #   | Label Type     | Content                                    |
        #   | Blood Draw     | John Martinez, DOB: 1975-08-15, ED-5     |
        #   | Test Codes     | CBC, Troponin, BMP                        |
        #   | Collection Time| STAT - Collect immediately                |
        #   | Barcode        | Patient and order identifiers             |
        specimen_labels = self.page.get_by_test_id("specimen-label").all()
        assert len(specimen_labels) == 4

        # And nursing tasks are added to the workflow:
        #   | Task Type           | Description                    | Priority | Due Time    |
        #   | Blood Collection    | Draw CBC, Troponin, BMP       | STAT     | Immediate   |
        #   | Patient Transport   | Transport to X-ray            | STAT     | After labs  |
        #   | Monitor Results     | Watch for critical values     | High     | Ongoing     |
        nursing_tasks = self.page.get_by_test_id("nursing-task").all()
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
        self.page.get_by_test_id("enter-order-button").click()

        # And I submit the order
        self.page.get_by_test_id("submit-orders-button").click()

        # Then the system displays an allergy alert:
        wait_for_test_id(self.page, "Drug Allergy")
        assert get_text(self.page, "Drug Allergy") == "WARNING: Patient allergic to Penicillin"
        assert get_text(self.page, "Severity") == "Moderate - Rash, hives"
        assert get_text(self.page, "Cross-reaction") == "Amoxicillin contains penicillin"
        assert get_text(self.page, "Recommendation") == "Consider alternative antibiotic"

        # And the order is held pending confirmation
        assert "pending confirmation" in get_text(self.page, "Order Status").lower()

        # And I must either:
        #   | Action Option      | Description                                |
        #   | Override with reason| Document clinical justification          |
        #   | Cancel order       | Remove the problematic medication         |
        #   | Select alternative | Choose non-penicillin antibiotic          |
        allergy_action_options = self.page.get_by_test_id("allergy-action-option").all()
        assert len(allergy_action_options) == 3

        # And the allergy alert is logged in the patient record
        assert len(get_text(self.page, "Allergy Alert Log Entry")) > 0

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
        self.page.get_by_test_id("enter-order-button").click()

        # And I mark all orders as "Emergency - Life threatening"
        fill_field(self.page, "Order Marking", "Emergency - Life threatening")

        # And I submit the orders
        self.page.get_by_test_id("submit-orders-button").click()

        # Then all orders are immediately transmitted with highest priority
        assert "highest priority" in get_text(self.page, "Order Transmission Status").lower()

        # And the laboratory receives orders marked "CRITICAL - TRAUMA"
        assert get_text(self.page, "Laboratory Order Marking") == "CRITICAL - TRAUMA"

        # And blood bank is notified to prepare emergency release protocol
        assert "emergency release protocol" in get_text(self.page, "Blood Bank Notification").lower()

        # And radiology is alerted for trauma CT protocol
        assert "trauma ct protocol" in get_text(self.page, "Radiology Alert").lower()

        # And nursing receives immediate action items:
        #   | Task               | Action Required           | Time Limit |
        #   | Blood Draw         | Collect trauma labs       | 5 minutes  |
        #   | IV Access          | Large bore IV x2          | Immediate  |
        #   | Patient Prep       | Prepare for CT transport  | 10 minutes |
        nursing_action_items = self.page.get_by_test_id("nursing-action-item").all()
        assert len(nursing_action_items) == 3

        # And all departments receive automatic status updates
        assert len(get_text(self.page, "Department Status Update")) > 0

    def test_enter_pediatric_orders_with_weight_based_dosing(self):
        # Given I have examined a pediatric patient "Tommy Chen" (age 5, weight 18kg) in bed "ED-PEDS-1"
        # And the patient presents with "febrile seizure"
        # (assumed pre-seeded test data)

        # When I enter pediatric medication orders:
        #   | Order Type | Medication | Dose Calculation        | Route | Frequency |
        #   | Medication | Acetaminophen| 15mg/kg (270mg)      | PO    | Q6H PRN   |
        #   | Medication | Lorazepam  | 0.1mg/kg (1.8mg)      | IV    | Once      |
        self.page.get_by_test_id("enter-order-button").click()

        # And I enter diagnostic orders:
        #   | Order Type | Test/Study    | Pediatric Protocol    | Priority |
        #   | Laboratory | CBC with diff | Pediatric collection  | STAT     |
        #   | Laboratory | Blood glucose | Fingerstick acceptable| STAT     |
        self.page.get_by_test_id("enter-order-button").click()

        # And I submit the pediatric orders
        self.page.get_by_test_id("submit-orders-button").click()

        # Then the system validates weight-based dosing calculations
        assert "validated" in get_text(self.page, "Dosing Validation Status").lower()

        # And pediatric-specific protocols are applied:
        #   | Protocol Type      | Details                                |
        #   | Collection Volume  | Minimum blood volume for pediatric labs|
        #   | Dosing Alerts      | Maximum safe dose verified             |
        #   | Administration     | Child-friendly instructions           |
        pediatric_protocols = self.page.get_by_test_id("pediatric-protocol").all()
        assert len(pediatric_protocols) == 3

        # And nursing receives pediatric-specific tasks:
        #   | Task Type          | Pediatric Instructions                 |
        #   | Medication Admin   | Use pediatric dosing chart            |
        #   | Blood Collection   | Minimize collection volume            |
        #   | Comfort Measures   | Parent/caregiver involvement          |
        pediatric_nursing_tasks = self.page.get_by_test_id("pediatric-nursing-task").all()
        assert len(pediatric_nursing_tasks) == 3

        # And pharmacy receives weight-verified dosing information
        assert len(get_text(self.page, "Pharmacy Dosing Notification")) > 0

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
            self.page,
            [
                {"Field": "Order ID", "Value": "ORD-103"},
                {"Field": "Cancellation Reason", "Value": "Patient reports morphine allergy"},
            ],
        )
        self.page.get_by_test_id("cancel-order-button").click()

        # And I modify order "ORD-102" to add "portable" due to patient instability
        fill_fields(
            self.page,
            [
                {"Field": "Order ID", "Value": "ORD-102"},
                {"Field": "Modification", "Value": "Add portable"},
            ],
        )
        self.page.get_by_test_id("modify-order-button").click()

        # And I add a new order for "Fentanyl 50mcg IV push"
        fill_field(self.page, "New Order Description", "Fentanyl 50mcg IV push")
        self.page.get_by_test_id("add-order-button").click()

        # Then the system processes the order changes:
        #   | Action Type | Order ID | New Status    | Reason/Details              |
        #   | Cancelled   | ORD-103  | Cancelled     | Morphine allergy discovered |
        #   | Modified    | ORD-102  | Updated       | Changed to portable CXR     |
        #   | New Order   | ORD-104  | Pending       | Fentanyl 50mcg IV push     |
        order_changes = self.page.get_by_test_id("order-change-entry").all()
        assert len(order_changes) == 3
        first_order_change_text = order_changes[0].inner_text()
        assert "ORD-103" in first_order_change_text

        # And notifications are sent to affected departments:
        #   | Department | Notification                               |
        #   | Pharmacy   | Morphine order cancelled - allergy        |
        #   | Radiology  | CXR modified to portable study            |
        #   | Nursing    | New pain medication order available       |
        department_notifications = self.page.get_by_test_id("department-notification").all()
        assert len(department_notifications) == 3

        # And an audit trail is maintained for all order changes
        audit_trail = self.page.get_by_test_id("order-change-audit-trail")
        assert audit_trail.is_visible()

    def test_enter_orders_with_insurance_authorization_requirements(self):
        # Given I have examined a patient "Robert Davis" in bed "ED-6"
        # And the patient has insurance requiring prior authorization for certain studies
        # (assumed pre-seeded test data)

        # When I enter an order for:
        #   | Order Type | Study Name | Estimated Cost | Insurance Notes        |
        #   | Radiology  | CT Abdomen | $1,200        | Requires pre-auth      |
        self.page.get_by_test_id("enter-order-button").click()

        # And I submit the order
        self.page.get_by_test_id("submit-orders-button").click()

        # Then the system checks insurance requirements:
        assert get_text(self.page, "Coverage Verification") == "CT covered with prior authorization"
        assert get_text(self.page, "Authorization Status") == "Prior auth required"
        assert get_text(self.page, "Alternative Options") == "Ultrasound covered without pre-auth"

        # And I am presented with options:
        #   | Option             | Description                            |
        #   | Submit for auth    | Send for insurance approval (delay)    |
        #   | Order alternative  | Consider ultrasound instead           |
        #   | Emergency override | Document medical necessity            |
        insurance_options = self.page.get_by_test_id("insurance-option").all()
        assert len(insurance_options) == 3

        # And the order status is marked "Pending Authorization"
        assert get_text(self.page, "Order Status") == "Pending Authorization"

        # And the patient financial counselor is notified
        assert len(get_text(self.page, "Financial Counselor Notification")) > 0

    def test_handle_order_entry_during_system_integration_failures(self):
        # Given I am entering orders for patient "Lisa Wong" in bed "ED-14"
        # And the laboratory information system is temporarily offline
        # (assumed pre-seeded test data)

        # When I enter laboratory orders:
        #   | Order Type | Test Name     | Priority |
        #   | Laboratory | Troponin      | STAT     |
        #   | Laboratory | CBC          | Routine  |
        self.page.get_by_test_id("enter-order-button").click()

        # And I submit the orders
        self.page.get_by_test_id("submit-orders-button").click()

        # Then the system displays a warning: "Lab system offline - orders will be queued"
        assert get_text(self.page, "Warning Message") == "Lab system offline - orders will be queued"

        # And the orders are stored locally with status "Queued for transmission"
        assert get_text(self.page, "Order Status") == "Queued for transmission"

        # And nursing is notified to manually coordinate with lab
        assert len(get_text(self.page, "Nursing Coordination Notification")) > 0

        # And I receive a notification when lab system connectivity is restored
        assert len(get_text(self.page, "Connectivity Restored Notification")) > 0

        # And queued orders are automatically transmitted when system is available
        assert "transmitted" in get_text(self.page, "Queued Order Transmission Status").lower()

        # And manual backup procedures are documented for critical orders
        assert len(get_text(self.page, "Manual Backup Procedure Documentation")) > 0

    def test_enter_complex_order_sets_for_specific_protocols(self):
        # Given I have examined a patient "James Thompson" in bed "ED-11"
        # And the patient presents with "suspected stroke"
        # (assumed pre-seeded test data)

        # When I select the "Acute Stroke Protocol" order set
        fill_field(self.page, "Order Set", "Acute Stroke Protocol")

        # Then the system presents the standardized stroke workup orders:
        #   | Category   | Order Description              | Priority | Default |
        #   | Laboratory | CBC, BMP, PT/INR, PTT         | STAT     | Selected|
        #   | Laboratory | Troponin, Lipid panel         | STAT     | Selected|
        #   | Radiology  | CT Head without contrast      | STAT     | Selected|
        #   | Radiology  | CT Angiogram head/neck        | STAT     | Optional|
        #   | Medication | Aspirin 325mg                 | STAT     | Selected|
        #   | Consults   | Neurology consult             | STAT     | Selected|
        stroke_protocol_orders = self.page.get_by_test_id("stroke-protocol-order").all()
        assert len(stroke_protocol_orders) == 6

        # And I can modify or remove individual orders from the set
        self.page.get_by_test_id("modify-order").first.wait_for()
        self.page.get_by_test_id("remove-order").first.wait_for()

        # And I add stroke-specific timing requirements:
        #   | Order          | Time Requirement                      |
        #   | CT Head        | Within 25 minutes of arrival         |
        #   | Lab results    | Within 45 minutes of arrival         |
        #   | Neurology      | Consult within 15 minutes           |
        self.page.get_by_test_id("add-timing-requirement-button").click()

        # And the system tracks compliance with stroke protocol timing
        assert "tracks compliance" in get_text(self.page, "Protocol Compliance Tracking Status").lower()

        # And automatic reminders are set for time-sensitive elements
        automatic_reminders = self.page.get_by_test_id("automatic-reminder").all()
        assert len(automatic_reminders) == 3
