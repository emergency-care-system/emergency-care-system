// Selenium WebDriver + libtest-mimic test for
// tests-with-given-when-then-features/08-order-entry.feature
// (equivalent to tests-with-selenium-javascript/08-order-entry.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see tests/support/fields.rs) and the shared
// data-testid contract in tests/support/login.rs (login-identity, login-submit,
// app-root).

#![allow(unused_variables)]

mod support;
use support::*;

async fn background(driver: &WebDriver) -> WebDriverResult<()> {
    // Background:
    //   Given the emergency care system is operational
    //   And I am logged in as "Dr. Smith"
    //   And the electronic order entry module is active
    //   And departmental interfaces (lab, radiology, pharmacy) are connected
    verify_system_is_operational(driver).await?;
    login(driver, "Dr. Smith", false).await?;
    // The electronic order entry module being active and the departmental
    // interfaces being connected are assumed pre-seeded test data /
    // environment configuration.

    let order_entry_nav_link = wait_for_test_id(driver, "Nav Order Entry").await?;
    order_entry_nav_link.click().await?;
    wait_for_test_id(driver, "Order Entry Panel").await?;
    Ok(())
}

fn scenario_01_enter_standard_orders_for_chest_pain_workup(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given I have examined a patient "John Martinez" in bed "ED-5"
        // And the patient presents with "acute chest pain"
        // And the patient's allergies and contraindications have been reviewed
        // (assumed pre-seeded test data)

        // When I open the order entry module for the patient
        driver.find(By::Css("[data-testid=\"open-order-entry-module-button\"]")).await?.click().await?;

        // And I enter the following laboratory orders:
        //   | Order Type     | Test Name        | Priority | Special Instructions |
        //   | Laboratory     | CBC with diff    | Routine  | None                |
        //   | Laboratory     | Troponin I       | STAT     | Serial in 6 hours   |
        //   | Laboratory     | Basic Metabolic  | Routine  | None                |
        driver.find(By::Css("[data-testid=\"enter-order-button\"]")).await?.click().await?;

        // And I enter the following radiology order:
        //   | Order Type     | Study Name       | Priority | Special Instructions |
        //   | Radiology      | Chest X-ray PA/LAT| STAT    | R/O pneumonia       |
        driver.find(By::Css("[data-testid=\"enter-order-button\"]")).await?.click().await?;

        // And I submit all orders with my electronic signature
        driver.find(By::Css("[data-testid=\"submit-orders-button\"]")).await?.click().await?;

        // Then the system sends electronic orders to the laboratory with details:
        //   | Order ID | Test Name     | Patient Info        | Priority | Timestamp |
        //   | LAB-001  | CBC with diff | John Martinez ED-5  | Routine  | Current   |
        //   | LAB-002  | Troponin I    | John Martinez ED-5  | STAT     | Current   |
        //   | LAB-003  | Basic Metabolic| John Martinez ED-5 | Routine  | Current   |
        let lab_orders_sent = driver.find_all(By::Css("[data-testid=\"laboratory-order-sent\"]")).await?;
        assert_eq!(lab_orders_sent.len(), 3);
        let first_lab_order_text = lab_orders_sent[0].text().await?;
        assert_match(&first_lab_order_text, r"LAB-001", false);

        // And the system sends electronic orders to radiology with details:
        //   | Order ID | Study Name    | Patient Info        | Priority | Timestamp |
        //   | RAD-001  | CXR PA/LAT    | John Martinez ED-5  | STAT     | Current   |
        let radiology_orders_sent = driver.find_all(By::Css("[data-testid=\"radiology-order-sent\"]")).await?;
        assert_eq!(radiology_orders_sent.len(), 1);

        // And specimen labels are automatically generated:
        //   | Label Type     | Content                                    |
        //   | Blood Draw     | John Martinez, DOB: 1975-08-15, ED-5     |
        //   | Test Codes     | CBC, Troponin, BMP                        |
        //   | Collection Time| STAT - Collect immediately                |
        //   | Barcode        | Patient and order identifiers             |
        let specimen_labels = driver.find_all(By::Css("[data-testid=\"specimen-label\"]")).await?;
        assert_eq!(specimen_labels.len(), 4);

        // And nursing tasks are added to the workflow:
        //   | Task Type           | Description                    | Priority | Due Time    |
        //   | Blood Collection    | Draw CBC, Troponin, BMP       | STAT     | Immediate   |
        //   | Patient Transport   | Transport to X-ray            | STAT     | After labs  |
        //   | Monitor Results     | Watch for critical values     | High     | Ongoing     |
        let nursing_tasks = driver.find_all(By::Css("[data-testid=\"nursing-task\"]")).await?;
        assert_eq!(nursing_tasks.len(), 3);
        Ok(())
    })
}

fn scenario_02_enter_orders_with_drug_allergy_checking(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given I have examined a patient "Sarah Johnson" in bed "ED-8"
        // And the patient has documented allergies:
        //   | Allergy    | Reaction Type    | Severity |
        //   | Penicillin | Rash, hives      | Moderate |
        //   | Morphine   | Respiratory depression | Severe |
        // (assumed pre-seeded test data)

        // When I attempt to enter a medication order:
        //   | Order Type | Medication  | Dose     | Route | Frequency |
        //   | Medication | Amoxicillin | 500mg    | PO    | TID       |
        driver.find(By::Css("[data-testid=\"enter-order-button\"]")).await?.click().await?;

        // And I submit the order
        driver.find(By::Css("[data-testid=\"submit-orders-button\"]")).await?.click().await?;

        // Then the system displays an allergy alert:
        wait_for_test_id(driver, "Drug Allergy").await?;
        assert_eq!(get_text(driver, "Drug Allergy").await?, "WARNING: Patient allergic to Penicillin");
        assert_eq!(get_text(driver, "Severity").await?, "Moderate - Rash, hives");
        assert_eq!(get_text(driver, "Cross-reaction").await?, "Amoxicillin contains penicillin");
        assert_eq!(get_text(driver, "Recommendation").await?, "Consider alternative antibiotic");

        // And the order is held pending confirmation
        let order_status = get_text(driver, "Order Status").await?;
        assert_match(&order_status, r"pending confirmation", true);

        // And I must either:
        //   | Action Option      | Description                                |
        //   | Override with reason| Document clinical justification          |
        //   | Cancel order       | Remove the problematic medication         |
        //   | Select alternative | Choose non-penicillin antibiotic          |
        let allergy_action_options = driver.find_all(By::Css("[data-testid=\"allergy-action-option\"]")).await?;
        assert_eq!(allergy_action_options.len(), 3);

        // And the allergy alert is logged in the patient record
        let allergy_alert_log_entry = get_text(driver, "Allergy Alert Log Entry").await?;
        assert!(allergy_alert_log_entry.len() > 0);
        Ok(())
    })
}

fn scenario_03_enter_stat_orders_during_emergency_situation(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

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
        driver.find(By::Css("[data-testid=\"enter-order-button\"]")).await?.click().await?;

        // And I mark all orders as "Emergency - Life threatening"
        fill_field(driver, "Order Marking", "Emergency - Life threatening").await?;

        // And I submit the orders
        driver.find(By::Css("[data-testid=\"submit-orders-button\"]")).await?.click().await?;

        // Then all orders are immediately transmitted with highest priority
        let order_transmission_status = get_text(driver, "Order Transmission Status").await?;
        assert_match(&order_transmission_status, r"highest priority", true);

        // And the laboratory receives orders marked "CRITICAL - TRAUMA"
        let laboratory_order_marking = get_text(driver, "Laboratory Order Marking").await?;
        assert_eq!(laboratory_order_marking, "CRITICAL - TRAUMA");

        // And blood bank is notified to prepare emergency release protocol
        let blood_bank_notification = get_text(driver, "Blood Bank Notification").await?;
        assert_match(&blood_bank_notification, r"emergency release protocol", true);

        // And radiology is alerted for trauma CT protocol
        let radiology_alert = get_text(driver, "Radiology Alert").await?;
        assert_match(&radiology_alert, r"trauma CT protocol", true);

        // And nursing receives immediate action items:
        //   | Task               | Action Required           | Time Limit |
        //   | Blood Draw         | Collect trauma labs       | 5 minutes  |
        //   | IV Access          | Large bore IV x2          | Immediate  |
        //   | Patient Prep       | Prepare for CT transport  | 10 minutes |
        let nursing_action_items = driver.find_all(By::Css("[data-testid=\"nursing-action-item\"]")).await?;
        assert_eq!(nursing_action_items.len(), 3);

        // And all departments receive automatic status updates
        let department_status_update = get_text(driver, "Department Status Update").await?;
        assert!(department_status_update.len() > 0);
        Ok(())
    })
}

fn scenario_04_enter_pediatric_orders_with_weight_based_dosing(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given I have examined a pediatric patient "Tommy Chen" (age 5, weight 18kg) in bed "ED-PEDS-1"
        // And the patient presents with "febrile seizure"
        // (assumed pre-seeded test data)

        // When I enter pediatric medication orders:
        //   | Order Type | Medication | Dose Calculation        | Route | Frequency |
        //   | Medication | Acetaminophen| 15mg/kg (270mg)      | PO    | Q6H PRN   |
        //   | Medication | Lorazepam  | 0.1mg/kg (1.8mg)      | IV    | Once      |
        driver.find(By::Css("[data-testid=\"enter-order-button\"]")).await?.click().await?;

        // And I enter diagnostic orders:
        //   | Order Type | Test/Study    | Pediatric Protocol    | Priority |
        //   | Laboratory | CBC with diff | Pediatric collection  | STAT     |
        //   | Laboratory | Blood glucose | Fingerstick acceptable| STAT     |
        driver.find(By::Css("[data-testid=\"enter-order-button\"]")).await?.click().await?;

        // And I submit the pediatric orders
        driver.find(By::Css("[data-testid=\"submit-orders-button\"]")).await?.click().await?;

        // Then the system validates weight-based dosing calculations
        let dosing_validation_status = get_text(driver, "Dosing Validation Status").await?;
        assert_match(&dosing_validation_status, r"validated", true);

        // And pediatric-specific protocols are applied:
        //   | Protocol Type      | Details                                |
        //   | Collection Volume  | Minimum blood volume for pediatric labs|
        //   | Dosing Alerts      | Maximum safe dose verified             |
        //   | Administration     | Child-friendly instructions           |
        let pediatric_protocols = driver.find_all(By::Css("[data-testid=\"pediatric-protocol\"]")).await?;
        assert_eq!(pediatric_protocols.len(), 3);

        // And nursing receives pediatric-specific tasks:
        //   | Task Type          | Pediatric Instructions                 |
        //   | Medication Admin   | Use pediatric dosing chart            |
        //   | Blood Collection   | Minimize collection volume            |
        //   | Comfort Measures   | Parent/caregiver involvement          |
        let pediatric_nursing_tasks = driver.find_all(By::Css("[data-testid=\"pediatric-nursing-task\"]")).await?;
        assert_eq!(pediatric_nursing_tasks.len(), 3);

        // And pharmacy receives weight-verified dosing information
        let pharmacy_dosing_notification = get_text(driver, "Pharmacy Dosing Notification").await?;
        assert!(pharmacy_dosing_notification.len() > 0);
        Ok(())
    })
}

fn scenario_05_handle_order_modifications_and_cancellations(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

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
        fill_fields(driver, &vec![
            row([("Field", "Order ID"), ("Value", "ORD-103")]),
            row([("Field", "Cancellation Reason"), ("Value", "Patient reports morphine allergy")]),
        ]).await?;
        driver.find(By::Css("[data-testid=\"cancel-order-button\"]")).await?.click().await?;

        // And I modify order "ORD-102" to add "portable" due to patient instability
        fill_fields(driver, &vec![
            row([("Field", "Order ID"), ("Value", "ORD-102")]),
            row([("Field", "Modification"), ("Value", "Add portable")]),
        ]).await?;
        driver.find(By::Css("[data-testid=\"modify-order-button\"]")).await?.click().await?;

        // And I add a new order for "Fentanyl 50mcg IV push"
        fill_field(driver, "New Order Description", "Fentanyl 50mcg IV push").await?;
        driver.find(By::Css("[data-testid=\"add-order-button\"]")).await?.click().await?;

        // Then the system processes the order changes:
        //   | Action Type | Order ID | New Status    | Reason/Details              |
        //   | Cancelled   | ORD-103  | Cancelled     | Morphine allergy discovered |
        //   | Modified    | ORD-102  | Updated       | Changed to portable CXR     |
        //   | New Order   | ORD-104  | Pending       | Fentanyl 50mcg IV push     |
        let order_changes = driver.find_all(By::Css("[data-testid=\"order-change-entry\"]")).await?;
        assert_eq!(order_changes.len(), 3);
        let first_order_change_text = order_changes[0].text().await?;
        assert_match(&first_order_change_text, r"ORD-103", false);

        // And notifications are sent to affected departments:
        //   | Department | Notification                               |
        //   | Pharmacy   | Morphine order cancelled - allergy        |
        //   | Radiology  | CXR modified to portable study            |
        //   | Nursing    | New pain medication order available       |
        let department_notifications = driver.find_all(By::Css("[data-testid=\"department-notification\"]")).await?;
        assert_eq!(department_notifications.len(), 3);

        // And an audit trail is maintained for all order changes
        let audit_trail = driver.find(By::Css("[data-testid=\"order-change-audit-trail\"]")).await?;
        assert!(audit_trail.is_displayed().await?);
        Ok(())
    })
}

fn scenario_06_enter_orders_with_insurance_authorization_requirements(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given I have examined a patient "Robert Davis" in bed "ED-6"
        // And the patient has insurance requiring prior authorization for certain studies
        // (assumed pre-seeded test data)

        // When I enter an order for:
        //   | Order Type | Study Name | Estimated Cost | Insurance Notes        |
        //   | Radiology  | CT Abdomen | $1,200        | Requires pre-auth      |
        driver.find(By::Css("[data-testid=\"enter-order-button\"]")).await?.click().await?;

        // And I submit the order
        driver.find(By::Css("[data-testid=\"submit-orders-button\"]")).await?.click().await?;

        // Then the system checks insurance requirements:
        assert_eq!(get_text(driver, "Coverage Verification").await?, "CT covered with prior authorization");
        assert_eq!(get_text(driver, "Authorization Status").await?, "Prior auth required");
        assert_eq!(get_text(driver, "Alternative Options").await?, "Ultrasound covered without pre-auth");

        // And I am presented with options:
        //   | Option             | Description                            |
        //   | Submit for auth    | Send for insurance approval (delay)    |
        //   | Order alternative  | Consider ultrasound instead           |
        //   | Emergency override | Document medical necessity            |
        let insurance_options = driver.find_all(By::Css("[data-testid=\"insurance-option\"]")).await?;
        assert_eq!(insurance_options.len(), 3);

        // And the order status is marked "Pending Authorization"
        let order_status = get_text(driver, "Order Status").await?;
        assert_eq!(order_status, "Pending Authorization");

        // And the patient financial counselor is notified
        let financial_counselor_notification = get_text(driver, "Financial Counselor Notification").await?;
        assert!(financial_counselor_notification.len() > 0);
        Ok(())
    })
}

fn scenario_07_handle_order_entry_during_system_integration_failures(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given I am entering orders for patient "Lisa Wong" in bed "ED-14"
        // And the laboratory information system is temporarily offline
        // (assumed pre-seeded test data)

        // When I enter laboratory orders:
        //   | Order Type | Test Name     | Priority |
        //   | Laboratory | Troponin      | STAT     |
        //   | Laboratory | CBC          | Routine  |
        driver.find(By::Css("[data-testid=\"enter-order-button\"]")).await?.click().await?;

        // And I submit the orders
        driver.find(By::Css("[data-testid=\"submit-orders-button\"]")).await?.click().await?;

        // Then the system displays a warning: "Lab system offline - orders will be queued"
        let warning_message = get_text(driver, "Warning Message").await?;
        assert_eq!(warning_message, "Lab system offline - orders will be queued");

        // And the orders are stored locally with status "Queued for transmission"
        let order_status = get_text(driver, "Order Status").await?;
        assert_eq!(order_status, "Queued for transmission");

        // And nursing is notified to manually coordinate with lab
        let nursing_coordination_notification = get_text(driver, "Nursing Coordination Notification").await?;
        assert!(nursing_coordination_notification.len() > 0);

        // And I receive a notification when lab system connectivity is restored
        let connectivity_restored_notification = get_text(driver, "Connectivity Restored Notification").await?;
        assert!(connectivity_restored_notification.len() > 0);

        // And queued orders are automatically transmitted when system is available
        let queued_order_transmission_status = get_text(driver, "Queued Order Transmission Status").await?;
        assert_match(&queued_order_transmission_status, r"transmitted", true);

        // And manual backup procedures are documented for critical orders
        let manual_backup_procedure_documentation = get_text(driver, "Manual Backup Procedure Documentation").await?;
        assert!(manual_backup_procedure_documentation.len() > 0);
        Ok(())
    })
}

fn scenario_08_enter_complex_order_sets_for_specific_protocols(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given I have examined a patient "James Thompson" in bed "ED-11"
        // And the patient presents with "suspected stroke"
        // (assumed pre-seeded test data)

        // When I select the "Acute Stroke Protocol" order set
        fill_field(driver, "Order Set", "Acute Stroke Protocol").await?;

        // Then the system presents the standardized stroke workup orders:
        //   | Category   | Order Description              | Priority | Default |
        //   | Laboratory | CBC, BMP, PT/INR, PTT         | STAT     | Selected|
        //   | Laboratory | Troponin, Lipid panel         | STAT     | Selected|
        //   | Radiology  | CT Head without contrast      | STAT     | Selected|
        //   | Radiology  | CT Angiogram head/neck        | STAT     | Optional|
        //   | Medication | Aspirin 325mg                 | STAT     | Selected|
        //   | Consults   | Neurology consult             | STAT     | Selected|
        let stroke_protocol_orders = driver.find_all(By::Css("[data-testid=\"stroke-protocol-order\"]")).await?;
        assert_eq!(stroke_protocol_orders.len(), 6);

        // And I can modify or remove individual orders from the set
        wait_for_test_id(driver, "Modify Order").await?;
        wait_for_test_id(driver, "Remove Order").await?;

        // And I add stroke-specific timing requirements:
        //   | Order          | Time Requirement                      |
        //   | CT Head        | Within 25 minutes of arrival         |
        //   | Lab results    | Within 45 minutes of arrival         |
        //   | Neurology      | Consult within 15 minutes           |
        driver.find(By::Css("[data-testid=\"add-timing-requirement-button\"]")).await?.click().await?;

        // And the system tracks compliance with stroke protocol timing
        let protocol_compliance_tracking_status = get_text(driver, "Protocol Compliance Tracking Status").await?;
        assert_match(&protocol_compliance_tracking_status, r"tracks compliance", true);

        // And automatic reminders are set for time-sensitive elements
        let automatic_reminders = driver.find_all(By::Css("[data-testid=\"automatic-reminder\"]")).await?;
        assert_eq!(automatic_reminders.len(), 3);
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
