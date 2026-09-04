# # Order Entry
#
# **Key Components:**
#
# - Electronic order routing to appropriate departments (lab, radiology, pharmacy)
#
# - Automatic specimen label generation with patient identifiers and barcodes
#
# - Nursing workflow integration with task creation and prioritization
#
# - Clinical decision support with allergy checking and drug interactions
#
# **Comprehensive Scenarios:**
#
# - Standard chest pain workup - Basic laboratory and radiology orders with automated routing
#
# - Drug allergy checking - Safety alerts and alternative medication suggestions
#
# - Emergency STAT orders - Critical patient orders with highest priority processing
#
# - Pediatric weight-based dosing - Age and weight-appropriate medication calculations
#
# - Order modifications - Changing, canceling, and updating existing orders
#
# - Insurance authorization - Prior approval requirements and alternative options
#
# - System integration failures - Offline capabilities and manual backup procedures
#
# - Protocol-based order sets - Standardized order sets for specific conditions (stroke protocol)
#
# **Healthcare-Specific Features:**
#
# - STAT vs routine priority handling
#
# - Drug allergy and interaction checking
#
# - Weight-based pediatric dosing calculations
#
# - Specimen collection protocols
#
# - Insurance authorization workflows
#
# - Clinical protocol compliance tracking
#
# - Department-specific order routing
#
# - Critical value monitoring
#
# - Audit trail for order changes
#
# - Time-sensitive protocol requirements
#
# - Emergency override capabilities
#
# - Backup procedures for system failures

Feature: Order Entry
  As a physician
  I want to enter diagnostic and treatment orders electronically
  So that they are automatically routed to appropriate departments and nursing workflow

  Background:
    Given the ED management system is operational
    And I am logged in as "Dr. Smith"
    And the electronic order entry module is active
    And departmental interfaces (lab, radiology, pharmacy) are connected

  Scenario: Enter standard orders for chest pain workup
    Given I have examined a patient "John Martinez" in bed "ED-5"
    And the patient presents with "acute chest pain"
    And the patient's allergies and contraindications have been reviewed
    When I open the order entry module for the patient
    And I enter the following laboratory orders:
      | Order Type     | Test Name        | Priority | Special Instructions |
      | Laboratory     | CBC with diff    | Routine  | None                |
      | Laboratory     | Troponin I       | STAT     | Serial in 6 hours   |
      | Laboratory     | Basic Metabolic  | Routine  | None                |
    And I enter the following radiology order:
      | Order Type     | Study Name       | Priority | Special Instructions |
      | Radiology      | Chest X-ray PA/LAT| STAT    | R/O pneumonia       |
    And I submit all orders with my electronic signature
    Then the system sends electronic orders to the laboratory with details:
      | Order ID | Test Name     | Patient Info        | Priority | Timestamp |
      | LAB-001  | CBC with diff | John Martinez ED-5  | Routine  | Current   |
      | LAB-002  | Troponin I    | John Martinez ED-5  | STAT     | Current   |
      | LAB-003  | Basic Metabolic| John Martinez ED-5 | Routine  | Current   |
    And the system sends electronic orders to radiology with details:
      | Order ID | Study Name    | Patient Info        | Priority | Timestamp |
      | RAD-001  | CXR PA/LAT    | John Martinez ED-5  | STAT     | Current   |
    And specimen labels are automatically generated:
      | Label Type     | Content                                    |
      | Blood Draw     | John Martinez, DOB: 1975-08-15, ED-5     |
      | Test Codes     | CBC, Troponin, BMP                        |
      | Collection Time| STAT - Collect immediately                |
      | Barcode        | Patient and order identifiers             |
    And nursing tasks are added to the workflow:
      | Task Type           | Description                    | Priority | Due Time    |
      | Blood Collection    | Draw CBC, Troponin, BMP       | STAT     | Immediate   |
      | Patient Transport   | Transport to X-ray            | STAT     | After labs  |
      | Monitor Results     | Watch for critical values     | High     | Ongoing     |

  Scenario: Enter orders with drug allergy checking
    Given I have examined a patient "Sarah Johnson" in bed "ED-8"
    And the patient has documented allergies:
      | Allergy    | Reaction Type    | Severity |
      | Penicillin | Rash, hives      | Moderate |
      | Morphine   | Respiratory depression | Severe |
    When I attempt to enter a medication order:
      | Order Type | Medication  | Dose     | Route | Frequency |
      | Medication | Amoxicillin | 500mg    | PO    | TID       |
    And I submit the order
    Then the system displays an allergy alert:
      | Alert Type     | Message                                    |
      | Drug Allergy   | WARNING: Patient allergic to Penicillin   |
      | Severity       | Moderate - Rash, hives                    |
      | Cross-reaction | Amoxicillin contains penicillin           |
      | Recommendation | Consider alternative antibiotic            |
    And the order is held pending confirmation
    And I must either:
      | Action Option      | Description                                |
      | Override with reason| Document clinical justification          |
      | Cancel order       | Remove the problematic medication         |
      | Select alternative | Choose non-penicillin antibiotic          |
    And the allergy alert is logged in the patient record

  Scenario: Enter STAT orders during emergency situation
    Given I have examined a patient "Emergency Patient" in bed "ED-TRAUMA-1"
    And the patient is in critical condition with "severe trauma"
    When I enter emergency orders:
      | Order Type     | Description           | Priority | Special Instructions    |
      | Laboratory     | Type and Crossmatch   | STAT     | 6 units PRBC on hold   |
      | Laboratory     | PT/INR, PTT          | STAT     | Pre-surgery labs       |
      | Radiology      | CT Head without contrast| STAT   | Rule out intracranial bleeding |
      | Radiology      | CT Chest/Abd/Pelvis  | STAT     | Trauma protocol        |
      | Medication     | Normal Saline        | STAT     | 1L wide open IV        |
    And I mark all orders as "Emergency - Life threatening"
    And I submit the orders
    Then all orders are immediately transmitted with highest priority
    And the laboratory receives orders marked "CRITICAL - TRAUMA"
    And blood bank is notified to prepare emergency release protocol
    And radiology is alerted for trauma CT protocol
    And nursing receives immediate action items:
      | Task               | Action Required           | Time Limit |
      | Blood Draw         | Collect trauma labs       | 5 minutes  |
      | IV Access          | Large bore IV x2          | Immediate  |
      | Patient Prep       | Prepare for CT transport  | 10 minutes |
    And all departments receive automatic status updates

  Scenario: Enter pediatric orders with weight-based dosing
    Given I have examined a pediatric patient "Tommy Chen" (age 5, weight 18kg) in bed "ED-PEDS-1"
    And the patient presents with "febrile seizure"
    When I enter pediatric medication orders:
      | Order Type | Medication | Dose Calculation        | Route | Frequency |
      | Medication | Acetaminophen| 15mg/kg (270mg)      | PO    | Q6H PRN   |
      | Medication | Lorazepam  | 0.1mg/kg (1.8mg)      | IV    | Once      |
    And I enter diagnostic orders:
      | Order Type | Test/Study    | Pediatric Protocol    | Priority |
      | Laboratory | CBC with diff | Pediatric collection  | STAT     |
      | Laboratory | Blood glucose | Fingerstick acceptable| STAT     |
    And I submit the pediatric orders
    Then the system validates weight-based dosing calculations
    And pediatric-specific protocols are applied:
      | Protocol Type      | Details                                |
      | Collection Volume  | Minimum blood volume for pediatric labs|
      | Dosing Alerts      | Maximum safe dose verified             |
      | Administration     | Child-friendly instructions           |
    And nursing receives pediatric-specific tasks:
      | Task Type          | Pediatric Instructions                 |
      | Medication Admin   | Use pediatric dosing chart            |
      | Blood Collection   | Minimize collection volume            |
      | Comfort Measures   | Parent/caregiver involvement          |
    And pharmacy receives weight-verified dosing information

  Scenario: Handle order modifications and cancellations
    Given I previously entered orders for patient "Maria Rodriguez" in bed "ED-12"
    And the existing orders include:
      | Order ID | Order Type | Description    | Status      | Entered Time |
      | ORD-101  | Laboratory | CBC           | In Progress | 10:30        |
      | ORD-102  | Radiology  | Chest X-ray   | Pending     | 10:30        |
      | ORD-103  | Medication | Morphine 2mg  | Pending     | 10:30        |
    When I need to modify the orders based on new clinical information
    And I cancel order "ORD-103" with reason "Patient reports morphine allergy"
    And I modify order "ORD-102" to add "portable" due to patient instability
    And I add a new order for "Fentanyl 50mcg IV push"
    Then the system processes the order changes:
      | Action Type | Order ID | New Status    | Reason/Details              |
      | Cancelled   | ORD-103  | Cancelled     | Morphine allergy discovered |
      | Modified    | ORD-102  | Updated       | Changed to portable CXR     |
      | New Order   | ORD-104  | Pending       | Fentanyl 50mcg IV push     |
    And notifications are sent to affected departments:
      | Department | Notification                               |
      | Pharmacy   | Morphine order cancelled - allergy        |
      | Radiology  | CXR modified to portable study            |
      | Nursing    | New pain medication order available       |
    And an audit trail is maintained for all order changes

  Scenario: Enter orders with insurance authorization requirements
    Given I have examined a patient "Robert Davis" in bed "ED-6"
    And the patient has insurance requiring prior authorization for certain studies
    When I enter an order for:
      | Order Type | Study Name | Estimated Cost | Insurance Notes        |
      | Radiology  | CT Abdomen | $1,200        | Requires pre-auth      |
    And I submit the order
    Then the system checks insurance requirements:
      | Check Type         | Result                                 |
      | Coverage Verification| CT covered with prior authorization   |
      | Authorization Status | Prior auth required                   |
      | Alternative Options | Ultrasound covered without pre-auth   |
    And I am presented with options:
      | Option             | Description                            |
      | Submit for auth    | Send for insurance approval (delay)    |
      | Order alternative  | Consider ultrasound instead           |
      | Emergency override | Document medical necessity            |
    And the order status is marked "Pending Authorization"
    And the patient financial counselor is notified

  Scenario: Handle order entry during system integration failures
    Given I am entering orders for patient "Lisa Wong" in bed "ED-14"
    And the laboratory information system is temporarily offline
    When I enter laboratory orders:
      | Order Type | Test Name     | Priority |
      | Laboratory | Troponin      | STAT     |
      | Laboratory | CBC          | Routine  |
    And I submit the orders
    Then the system displays a warning: "Lab system offline - orders will be queued"
    And the orders are stored locally with status "Queued for transmission"
    And nursing is notified to manually coordinate with lab
    And I receive a notification when lab system connectivity is restored
    And queued orders are automatically transmitted when system is available
    And manual backup procedures are documented for critical orders

  Scenario: Enter complex order sets for specific protocols
    Given I have examined a patient "James Thompson" in bed "ED-11"
    And the patient presents with "suspected stroke"
    When I select the "Acute Stroke Protocol" order set
    Then the system presents the standardized stroke workup orders:
      | Category   | Order Description              | Priority | Default |
      | Laboratory | CBC, BMP, PT/INR, PTT         | STAT     | Selected|
      | Laboratory | Troponin, Lipid panel         | STAT     | Selected|
      | Radiology  | CT Head without contrast      | STAT     | Selected|
      | Radiology  | CT Angiogram head/neck        | STAT     | Optional|
      | Medication | Aspirin 325mg                 | STAT     | Selected|
      | Consults   | Neurology consult             | STAT     | Selected|
    And I can modify or remove individual orders from the set
    And I add stroke-specific timing requirements:
      | Order          | Time Requirement                      |
      | CT Head        | Within 25 minutes of arrival         |
      | Lab results    | Within 45 minutes of arrival         |
      | Neurology      | Consult within 15 minutes           |
    And the system tracks compliance with stroke protocol timing
    And automatic reminders are set for time-sensitive elements
