// Playwright + libtest-mimic test for
// tests-with-given-when-then-features/08-order-entry.feature
// (equivalent to tests-with-playwright-javascript/08-order-entry.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see tests/support/fields.rs) and the shared
// data-testid contract in tests/support/login.rs (login-identity, login-submit,
// app-root).

#![allow(unused_variables)]

mod support;
use support::*;

async fn background(page: &Page) -> Result<()> {
    // Background:
    //   Given the emergency care system is operational
    //   And I am logged in as "Dr. Smith"
    //   And the electronic order entry module is active
    //   And departmental interfaces (lab, radiology, pharmacy) are connected
    verify_system_is_operational(page).await?;
    login(page, "Dr. Smith", false).await?;
    // The electronic order entry module being active and the departmental
    // interfaces being connected are assumed pre-seeded test data /
    // environment configuration.

    let order_entry_nav_link = wait_for_test_id(page, "Nav Order Entry").await?;
    order_entry_nav_link.click(None).await?;
    wait_for_test_id(page, "Order Entry Panel").await?;
    Ok(())
}

fn scenario_01_enter_standard_orders_for_chest_pain_workup(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given I have examined a patient "John Martinez" in bed "ED-5"
        // And the patient presents with "acute chest pain"
        // And the patient's allergies and contraindications have been reviewed
        // (assumed pre-seeded test data)

        // When I open the order entry module for the patient
        page.get_by_test_id("open-order-entry-module-button").first().click(None).await?;

        // And I enter the following laboratory orders:
        //   | Order Type     | Test Name        | Priority | Special Instructions |
        //   | Laboratory     | CBC with diff    | Routine  | None                |
        //   | Laboratory     | Troponin I       | STAT     | Serial in 6 hours   |
        //   | Laboratory     | Basic Metabolic  | Routine  | None                |
        page.get_by_test_id("enter-order-button").first().click(None).await?;

        // And I enter the following radiology order:
        //   | Order Type     | Study Name       | Priority | Special Instructions |
        //   | Radiology      | Chest X-ray PA/LAT| STAT    | R/O pneumonia       |
        page.get_by_test_id("enter-order-button").first().click(None).await?;

        // And I submit all orders with my electronic signature
        page.get_by_test_id("submit-orders-button").first().click(None).await?;

        // Then the system sends electronic orders to the laboratory with details:
        //   | Order ID | Test Name     | Patient Info        | Priority | Timestamp |
        //   | LAB-001  | CBC with diff | John Martinez ED-5  | Routine  | Current   |
        //   | LAB-002  | Troponin I    | John Martinez ED-5  | STAT     | Current   |
        //   | LAB-003  | Basic Metabolic| John Martinez ED-5 | Routine  | Current   |
        let lab_orders_sent = page.get_by_test_id("laboratory-order-sent");
        assert_eq!(lab_orders_sent.count().await?, 3);
        let first_lab_order_text = text_of(&lab_orders_sent.nth(0)).await?;
        assert_match(&first_lab_order_text, r"LAB-001", false);

        // And the system sends electronic orders to radiology with details:
        //   | Order ID | Study Name    | Patient Info        | Priority | Timestamp |
        //   | RAD-001  | CXR PA/LAT    | John Martinez ED-5  | STAT     | Current   |
        let radiology_orders_sent = page.get_by_test_id("radiology-order-sent");
        assert_eq!(radiology_orders_sent.count().await?, 1);

        // And specimen labels are automatically generated:
        //   | Label Type     | Content                                    |
        //   | Blood Draw     | John Martinez, DOB: 1975-08-15, ED-5     |
        //   | Test Codes     | CBC, Troponin, BMP                        |
        //   | Collection Time| STAT - Collect immediately                |
        //   | Barcode        | Patient and order identifiers             |
        let specimen_labels = page.get_by_test_id("specimen-label");
        assert_eq!(specimen_labels.count().await?, 4);

        // And nursing tasks are added to the workflow:
        //   | Task Type           | Description                    | Priority | Due Time    |
        //   | Blood Collection    | Draw CBC, Troponin, BMP       | STAT     | Immediate   |
        //   | Patient Transport   | Transport to X-ray            | STAT     | After labs  |
        //   | Monitor Results     | Watch for critical values     | High     | Ongoing     |
        let nursing_tasks = page.get_by_test_id("nursing-task");
        assert_eq!(nursing_tasks.count().await?, 3);
        Ok(())
    })
}

fn scenario_02_enter_orders_with_drug_allergy_checking(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given I have examined a patient "Sarah Johnson" in bed "ED-8"
        // And the patient has documented allergies:
        //   | Allergy    | Reaction Type    | Severity |
        //   | Penicillin | Rash, hives      | Moderate |
        //   | Morphine   | Respiratory depression | Severe |
        // (assumed pre-seeded test data)

        // When I attempt to enter a medication order:
        //   | Order Type | Medication  | Dose     | Route | Frequency |
        //   | Medication | Amoxicillin | 500mg    | PO    | TID       |
        page.get_by_test_id("enter-order-button").first().click(None).await?;

        // And I submit the order
        page.get_by_test_id("submit-orders-button").first().click(None).await?;

        // Then the system displays an allergy alert:
        wait_for_test_id(page, "Drug Allergy").await?;
        assert_eq!(get_text(page, "Drug Allergy").await?, "WARNING: Patient allergic to Penicillin");
        assert_eq!(get_text(page, "Severity").await?, "Moderate - Rash, hives");
        assert_eq!(get_text(page, "Cross-reaction").await?, "Amoxicillin contains penicillin");
        assert_eq!(get_text(page, "Recommendation").await?, "Consider alternative antibiotic");

        // And the order is held pending confirmation
        let order_status = get_text(page, "Order Status").await?;
        assert_match(&order_status, r"pending confirmation", true);

        // And I must either:
        //   | Action Option      | Description                                |
        //   | Override with reason| Document clinical justification          |
        //   | Cancel order       | Remove the problematic medication         |
        //   | Select alternative | Choose non-penicillin antibiotic          |
        let allergy_action_options = page.get_by_test_id("allergy-action-option");
        assert_eq!(allergy_action_options.count().await?, 3);

        // And the allergy alert is logged in the patient record
        let allergy_alert_log_entry = get_text(page, "Allergy Alert Log Entry").await?;
        assert!(allergy_alert_log_entry.len() > 0);
        Ok(())
    })
}

fn scenario_03_enter_stat_orders_during_emergency_situation(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given I have examined a patient "Emergency Patient" in bed "ED-TRAUMA-1"
        // And the patient is in critical condition with "severe trauma"
        // (assumed pre-seeded test data)

        // When I enter emergency orders:
        //   | Order Type     | Description           | Priority | Special Instructions    |
        //   | Laboratory     | Type and Crossmatch   | STAT     | 6 units PRBC on hold   |
        //   | Laboratory     | PT/INR, PTT          | STAT     | Pre-surgery labs       |
        //   | Radiology      | CT Head without contrast| STAT   | Rule out intracranial bleeding |
        //   | Radiology      | CT Chest/Abd/Pelvis  | STAT     | Trauma protocol        |
        //   | Medication     | Normal Saline        | STAT     | 1L wide open IV        |
        page.get_by_test_id("enter-order-button").first().click(None).await?;

        // And I mark all orders as "Emergency - Life threatening"
        fill_field(page, "Order Marking", "Emergency - Life threatening").await?;

        // And I submit the orders
        page.get_by_test_id("submit-orders-button").first().click(None).await?;

        // Then all orders are immediately transmitted with highest priority
        let order_transmission_status = get_text(page, "Order Transmission Status").await?;
        assert_match(&order_transmission_status, r"highest priority", true);

        // And the laboratory receives orders marked "CRITICAL - TRAUMA"
        let laboratory_order_marking = get_text(page, "Laboratory Order Marking").await?;
        assert_eq!(laboratory_order_marking, "CRITICAL - TRAUMA");

        // And blood bank is notified to prepare emergency release protocol
        let blood_bank_notification = get_text(page, "Blood Bank Notification").await?;
        assert_match(&blood_bank_notification, r"emergency release protocol", true);

        // And radiology is alerted for trauma CT protocol
        let radiology_alert = get_text(page, "Radiology Alert").await?;
        assert_match(&radiology_alert, r"trauma CT protocol", true);

        // And nursing receives immediate action items:
        //   | Task               | Action Required           | Time Limit |
        //   | Blood Draw         | Collect trauma labs       | 5 minutes  |
        //   | IV Access          | Large bore IV x2          | Immediate  |
        //   | Patient Prep       | Prepare for CT transport  | 10 minutes |
        let nursing_action_items = page.get_by_test_id("nursing-action-item");
        assert_eq!(nursing_action_items.count().await?, 3);

        // And all departments receive automatic status updates
        let department_status_update = get_text(page, "Department Status Update").await?;
        assert!(department_status_update.len() > 0);
        Ok(())
    })
}

fn scenario_04_enter_pediatric_orders_with_weight_based_dosing(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given I have examined a pediatric patient "Tommy Chen" (age 5, weight 18kg) in bed "ED-PEDS-1"
        // And the patient presents with "febrile seizure"
        // (assumed pre-seeded test data)

        // When I enter pediatric medication orders:
        //   | Order Type | Medication | Dose Calculation        | Route | Frequency |
        //   | Medication | Acetaminophen| 15mg/kg (270mg)      | PO    | Q6H PRN   |
        //   | Medication | Lorazepam  | 0.1mg/kg (1.8mg)      | IV    | Once      |
        page.get_by_test_id("enter-order-button").first().click(None).await?;

        // And I enter diagnostic orders:
        //   | Order Type | Test/Study    | Pediatric Protocol    | Priority |
        //   | Laboratory | CBC with diff | Pediatric collection  | STAT     |
        //   | Laboratory | Blood glucose | Fingerstick acceptable| STAT     |
        page.get_by_test_id("enter-order-button").first().click(None).await?;

        // And I submit the pediatric orders
        page.get_by_test_id("submit-orders-button").first().click(None).await?;

        // Then the system validates weight-based dosing calculations
        let dosing_validation_status = get_text(page, "Dosing Validation Status").await?;
        assert_match(&dosing_validation_status, r"validated", true);

        // And pediatric-specific protocols are applied:
        //   | Protocol Type      | Details                                |
        //   | Collection Volume  | Minimum blood volume for pediatric labs|
        //   | Dosing Alerts      | Maximum safe dose verified             |
        //   | Administration     | Child-friendly instructions           |
        let pediatric_protocols = page.get_by_test_id("pediatric-protocol");
        assert_eq!(pediatric_protocols.count().await?, 3);

        // And nursing receives pediatric-specific tasks:
        //   | Task Type          | Pediatric Instructions                 |
        //   | Medication Admin   | Use pediatric dosing chart            |
        //   | Blood Collection   | Minimize collection volume            |
        //   | Comfort Measures   | Parent/caregiver involvement          |
        let pediatric_nursing_tasks = page.get_by_test_id("pediatric-nursing-task");
        assert_eq!(pediatric_nursing_tasks.count().await?, 3);

        // And pharmacy receives weight-verified dosing information
        let pharmacy_dosing_notification = get_text(page, "Pharmacy Dosing Notification").await?;
        assert!(pharmacy_dosing_notification.len() > 0);
        Ok(())
    })
}

fn scenario_05_handle_order_modifications_and_cancellations(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given I previously entered orders for patient "Maria Rodriguez" in bed "ED-12"
        // And the existing orders include:
        //   | Order ID | Order Type | Description    | Status      | Entered Time |
        //   | ORD-101  | Laboratory | CBC           | In Progress | 10:30        |
        //   | ORD-102  | Radiology  | Chest X-ray   | Pending     | 10:30        |
        //   | ORD-103  | Medication | Morphine 2mg  | Pending     | 10:30        |
        // (assumed pre-seeded test data)

        // When I need to modify the orders based on new clinical information
        // (no direct UI action for this narrative step)

        // And I cancel order "ORD-103" with reason "Patient reports morphine allergy"
        fill_fields(page, &vec![
            row([("Field", "Order ID"), ("Value", "ORD-103")]),
            row([("Field", "Cancellation Reason"), ("Value", "Patient reports morphine allergy")]),
        ]).await?;
        page.get_by_test_id("cancel-order-button").first().click(None).await?;

        // And I modify order "ORD-102" to add "portable" due to patient instability
        fill_fields(page, &vec![
            row([("Field", "Order ID"), ("Value", "ORD-102")]),
            row([("Field", "Modification"), ("Value", "Add portable")]),
        ]).await?;
        page.get_by_test_id("modify-order-button").first().click(None).await?;

        // And I add a new order for "Fentanyl 50mcg IV push"
        fill_field(page, "New Order Description", "Fentanyl 50mcg IV push").await?;
        page.get_by_test_id("add-order-button").first().click(None).await?;

        // Then the system processes the order changes:
        //   | Action Type | Order ID | New Status    | Reason/Details              |
        //   | Cancelled   | ORD-103  | Cancelled     | Morphine allergy discovered |
        //   | Modified    | ORD-102  | Updated       | Changed to portable CXR     |
        //   | New Order   | ORD-104  | Pending       | Fentanyl 50mcg IV push     |
        let order_changes = page.get_by_test_id("order-change-entry");
        assert_eq!(order_changes.count().await?, 3);
        let first_order_change_text = text_of(&order_changes.nth(0)).await?;
        assert_match(&first_order_change_text, r"ORD-103", false);

        // And notifications are sent to affected departments:
        //   | Department | Notification                               |
        //   | Pharmacy   | Morphine order cancelled - allergy        |
        //   | Radiology  | CXR modified to portable study            |
        //   | Nursing    | New pain medication order available       |
        let department_notifications = page.get_by_test_id("department-notification");
        assert_eq!(department_notifications.count().await?, 3);

        // And an audit trail is maintained for all order changes
        let audit_trail = page.get_by_test_id("order-change-audit-trail").first();
        assert!(audit_trail.is_visible().await?);
        Ok(())
    })
}

fn scenario_06_enter_orders_with_insurance_authorization_requirements(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given I have examined a patient "Robert Davis" in bed "ED-6"
        // And the patient has insurance requiring prior authorization for certain studies
        // (assumed pre-seeded test data)

        // When I enter an order for:
        //   | Order Type | Study Name | Estimated Cost | Insurance Notes        |
        //   | Radiology  | CT Abdomen | $1,200        | Requires pre-auth      |
        page.get_by_test_id("enter-order-button").first().click(None).await?;

        // And I submit the order
        page.get_by_test_id("submit-orders-button").first().click(None).await?;

        // Then the system checks insurance requirements:
        assert_eq!(get_text(page, "Coverage Verification").await?, "CT covered with prior authorization");
        assert_eq!(get_text(page, "Authorization Status").await?, "Prior auth required");
        assert_eq!(get_text(page, "Alternative Options").await?, "Ultrasound covered without pre-auth");

        // And I am presented with options:
        //   | Option             | Description                            |
        //   | Submit for auth    | Send for insurance approval (delay)    |
        //   | Order alternative  | Consider ultrasound instead           |
        //   | Emergency override | Document medical necessity            |
        let insurance_options = page.get_by_test_id("insurance-option");
        assert_eq!(insurance_options.count().await?, 3);

        // And the order status is marked "Pending Authorization"
        let order_status = get_text(page, "Order Status").await?;
        assert_eq!(order_status, "Pending Authorization");

        // And the patient financial counselor is notified
        let financial_counselor_notification = get_text(page, "Financial Counselor Notification").await?;
        assert!(financial_counselor_notification.len() > 0);
        Ok(())
    })
}

fn scenario_07_handle_order_entry_during_system_integration_failures(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given I am entering orders for patient "Lisa Wong" in bed "ED-14"
        // And the laboratory information system is temporarily offline
        // (assumed pre-seeded test data)

        // When I enter laboratory orders:
        //   | Order Type | Test Name     | Priority |
        //   | Laboratory | Troponin      | STAT     |
        //   | Laboratory | CBC          | Routine  |
        page.get_by_test_id("enter-order-button").first().click(None).await?;

        // And I submit the orders
        page.get_by_test_id("submit-orders-button").first().click(None).await?;

        // Then the system displays a warning: "Lab system offline - orders will be queued"
        let warning_message = get_text(page, "Warning Message").await?;
        assert_eq!(warning_message, "Lab system offline - orders will be queued");

        // And the orders are stored locally with status "Queued for transmission"
        let order_status = get_text(page, "Order Status").await?;
        assert_eq!(order_status, "Queued for transmission");

        // And nursing is notified to manually coordinate with lab
        let nursing_coordination_notification = get_text(page, "Nursing Coordination Notification").await?;
        assert!(nursing_coordination_notification.len() > 0);

        // And I receive a notification when lab system connectivity is restored
        let connectivity_restored_notification = get_text(page, "Connectivity Restored Notification").await?;
        assert!(connectivity_restored_notification.len() > 0);

        // And queued orders are automatically transmitted when system is available
        let queued_order_transmission_status = get_text(page, "Queued Order Transmission Status").await?;
        assert_match(&queued_order_transmission_status, r"transmitted", true);

        // And manual backup procedures are documented for critical orders
        let manual_backup_procedure_documentation = get_text(page, "Manual Backup Procedure Documentation").await?;
        assert!(manual_backup_procedure_documentation.len() > 0);
        Ok(())
    })
}

fn scenario_08_enter_complex_order_sets_for_specific_protocols(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given I have examined a patient "James Thompson" in bed "ED-11"
        // And the patient presents with "suspected stroke"
        // (assumed pre-seeded test data)

        // When I select the "Acute Stroke Protocol" order set
        fill_field(page, "Order Set", "Acute Stroke Protocol").await?;

        // Then the system presents the standardized stroke workup orders:
        //   | Category   | Order Description              | Priority | Default |
        //   | Laboratory | CBC, BMP, PT/INR, PTT         | STAT     | Selected|
        //   | Laboratory | Troponin, Lipid panel         | STAT     | Selected|
        //   | Radiology  | CT Head without contrast      | STAT     | Selected|
        //   | Radiology  | CT Angiogram head/neck        | STAT     | Optional|
        //   | Medication | Aspirin 325mg                 | STAT     | Selected|
        //   | Consults   | Neurology consult             | STAT     | Selected|
        let stroke_protocol_orders = page.get_by_test_id("stroke-protocol-order");
        assert_eq!(stroke_protocol_orders.count().await?, 6);

        // And I can modify or remove individual orders from the set
        wait_for_test_id(page, "Modify Order").await?;
        wait_for_test_id(page, "Remove Order").await?;

        // And I add stroke-specific timing requirements:
        //   | Order          | Time Requirement                      |
        //   | CT Head        | Within 25 minutes of arrival         |
        //   | Lab results    | Within 45 minutes of arrival         |
        //   | Neurology      | Consult within 15 minutes           |
        page.get_by_test_id("add-timing-requirement-button").first().click(None).await?;

        // And the system tracks compliance with stroke protocol timing
        let protocol_compliance_tracking_status = get_text(page, "Protocol Compliance Tracking Status").await?;
        assert_match(&protocol_compliance_tracking_status, r"tracks compliance", true);

        // And automatic reminders are set for time-sensitive elements
        let automatic_reminders = page.get_by_test_id("automatic-reminder");
        assert_eq!(automatic_reminders.count().await?, 3);
        Ok(())
    })
}

fn main() {
    run_feature(&[
        ("Enter standard orders for chest pain workup", scenario_01_enter_standard_orders_for_chest_pain_workup),
        ("Enter orders with drug allergy checking", scenario_02_enter_orders_with_drug_allergy_checking),
        ("Enter STAT orders during emergency situation", scenario_03_enter_stat_orders_during_emergency_situation),
        ("Enter pediatric orders with weight-based dosing", scenario_04_enter_pediatric_orders_with_weight_based_dosing),
        ("Handle order modifications and cancellations", scenario_05_handle_order_modifications_and_cancellations),
        ("Enter orders with insurance authorization requirements", scenario_06_enter_orders_with_insurance_authorization_requirements),
        ("Handle order entry during system integration failures", scenario_07_handle_order_entry_during_system_integration_failures),
        ("Enter complex order sets for specific protocols", scenario_08_enter_complex_order_sets_for_specific_protocols),
    ]);
}
