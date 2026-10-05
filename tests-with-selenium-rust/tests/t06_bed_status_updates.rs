// Selenium WebDriver + libtest-mimic test for
// tests-with-given-when-then-features/06-bed-status-updates.feature
// (equivalent to tests-with-selenium-javascript/06-bed-status-updates.test.js).
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
    //   And I am logged in as a nurse
    //   And the bed management module is active
    //   And housekeeping notification system is enabled
    verify_system_is_operational(driver).await?;
    login(driver, "a nurse", false).await?;
    // The bed management module and housekeeping notification system are
    // assumed to be pre-seeded/enabled test data.

    let feature_nav_link = wait_for_test_id(driver, "Nav Bed Status Updates").await?;
    feature_nav_link.click().await?;
    wait_for_test_id(driver, "Bed Status Updates Panel").await?;
    Ok(())
}

fn scenario_01_mark_bed_as_needs_cleaning_after_patient_discharge(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a patient "John Doe" is currently occupying bed "ED-12"
        // And the bed status is "Occupied"
        // And the available bed count shows 8 out of 20 beds available
        // (assumed pre-seeded test data)

        // When the patient is discharged from bed "ED-12"
        driver.find(By::Css("[data-testid=\"discharge-patient-button\"]")).await?.click().await?;

        // And I mark the bed as "Needs Cleaning"
        fill_field(driver, "New Bed Status", "Needs Cleaning").await?;

        // And I submit the bed status update
        driver.find(By::Css("[data-testid=\"submit-bed-status-update\"]")).await?.click().await?;

        // Then the system updates the bed status to "Dirty"
        wait_for_test_id(driver, "Bed Status").await?;
        let bed_status = get_text(driver, "Bed Status").await?;
        assert_eq!(bed_status, "Dirty");

        // And a notification is sent to housekeeping with details:
        let notification_fields = vec![
            row([("Field", "Room Number"), ("Value", "ED-12")]),
            row([("Field", "Status"), ("Value", "Needs Cleaning")]),
            row([("Field", "Priority"), ("Value", "Standard")]),
            row([("Field", "Patient Type"), ("Value", "Standard discharge")]),
            row([("Field", "Special Requirements"), ("Value", "Standard cleaning protocol")]),
            row([("Field", "Timestamp"), ("Value", "Current time")]),
        ];
        for row_data in &notification_fields {
            let field = row_data["Field"].as_str();
            let value = row_data["Value"].as_str();
            assert_eq!(get_text(driver, field).await?, value);
        }

        // And the bed is removed from the available bed count
        // And the available bed count updates to 7 out of 20 beds available
        let available_bed_count = get_text(driver, "Available Bed Count").await?;
        assert_eq!(available_bed_count, "7 out of 20 beds available");

        // And the bed appears as "Dirty" on the bed management dashboard
        let dashboard_bed_status = get_text(driver, "Dashboard Bed Status").await?;
        assert_eq!(dashboard_bed_status, "Dirty");
        Ok(())
    })
}

fn scenario_02_mark_isolation_bed_for_deep_cleaning_after_infectious_patien(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a patient "Jane Smith" with isolation precautions is in bed "ED-ISO-2"
        // And the bed status is "Occupied - Isolation"
        // And the patient had confirmed MRSA infection
        // (assumed pre-seeded test data)

        // When the patient is discharged from bed "ED-ISO-2"
        driver.find(By::Css("[data-testid=\"discharge-patient-button\"]")).await?.click().await?;

        // And I mark the bed as "Needs Deep Cleaning"
        fill_field(driver, "New Bed Status", "Needs Deep Cleaning").await?;

        // And I specify the isolation type as "Contact Precautions - MRSA"
        fill_field(driver, "Isolation Type", "Contact Precautions - MRSA").await?;

        // And I submit the bed status update
        driver.find(By::Css("[data-testid=\"submit-bed-status-update\"]")).await?.click().await?;

        // Then the system updates the bed status to "Dirty - Isolation"
        wait_for_test_id(driver, "Bed Status").await?;
        let bed_status = get_text(driver, "Bed Status").await?;
        assert_eq!(bed_status, "Dirty - Isolation");

        // And a high-priority notification is sent to housekeeping with details:
        let notification_fields = vec![
            row([("Field", "Room Number"), ("Value", "ED-ISO-2")]),
            row([("Field", "Status"), ("Value", "Needs Deep Cleaning")]),
            row([("Field", "Priority"), ("Value", "High")]),
            row([("Field", "Infection Type"), ("Value", "MRSA - Contact Precautions")]),
            row([("Field", "Special Requirements"), ("Value", "Terminal cleaning required")]),
            row([("Field", "PPE Required"), ("Value", "Gowns, gloves, masks")]),
        ];
        for row_data in &notification_fields {
            let field = row_data["Field"].as_str();
            let value = row_data["Value"].as_str();
            assert_eq!(get_text(driver, field).await?, value);
        }

        // And the bed is flagged as "Out of Service" until deep cleaning completion
        let bed_service_flag = get_text(driver, "Bed Service Flag").await?;
        assert_eq!(bed_service_flag, "Out of Service");

        // And the isolation bed count is reduced by one
        wait_for_test_id(driver, "Isolation Bed Count").await?;

        // And an alert is sent to infection control team
        wait_for_test_id(driver, "Infection Control Alert").await?;
        Ok(())
    })
}

fn scenario_03_housekeeping_completes_cleaning_and_marks_bed_ready(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given bed "ED-8" has status "Dirty"
        // And housekeeping was notified 30 minutes ago
        // (assumed pre-seeded test data)

        // When the housekeeping staff completes cleaning of bed "ED-8"
        driver.find(By::Css("[data-testid=\"complete-cleaning-button\"]")).await?.click().await?;

        // And the housekeeping supervisor marks the bed as "Clean and Ready"
        fill_field(driver, "New Bed Status", "Clean and Ready").await?;

        // And submits the cleaning completion with details:
        fill_fields(driver, &vec![
            row([("Field", "Cleaning Staff"), ("Value", "Maria Rodriguez")]),
            row([("Field", "Cleaning Start"), ("Value", "14:30")]),
            row([("Field", "Cleaning End"), ("Value", "15:00")]),
            row([("Field", "Cleaning Type"), ("Value", "Standard")]),
            row([("Field", "Supplies Used"), ("Value", "Standard disinfection")]),
        ]).await?;
        driver.find(By::Css("[data-testid=\"submit-cleaning-completion-form\"]")).await?.click().await?;

        // Then the system updates the bed status to "Available"
        wait_for_test_id(driver, "Bed Status").await?;
        let bed_status = get_text(driver, "Bed Status").await?;
        assert_eq!(bed_status, "Available");

        // And the bed is added back to the available bed count
        // And the available bed count increases by one
        wait_for_test_id(driver, "Available Bed Count").await?;

        // And a notification is sent to the charge nurse that bed "ED-8" is ready
        wait_for_test_id(driver, "Charge Nurse Notification").await?;

        // And the bed appears as "Available" on the bed management dashboard
        let dashboard_bed_status = get_text(driver, "Dashboard Bed Status").await?;
        assert_eq!(dashboard_bed_status, "Available");
        Ok(())
    })
}

fn scenario_04_handle_bed_maintenance_request_during_status_update(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a patient is discharged from bed "ED-15"
        // (assumed pre-seeded test data)

        // When I attempt to mark the bed as "Needs Cleaning"
        fill_field(driver, "New Bed Status", "Needs Cleaning").await?;

        // And I notice equipment malfunction in the room
        // (observation, no direct UI action)

        // And I select "Maintenance Required" in addition to cleaning needs
        driver.find(By::Css("[data-testid=\"maintenance-required-checkbox\"]")).await?.click().await?;

        // And I specify the issue as "IV pump not functioning, call light broken"
        fill_field(driver, "Issue Description", "IV pump not functioning, call light broken").await?;

        // And I submit the bed status update
        driver.find(By::Css("[data-testid=\"submit-bed-status-update\"]")).await?.click().await?;

        // Then the system updates the bed status to "Out of Service - Maintenance"
        wait_for_test_id(driver, "Bed Status").await?;
        let bed_status = get_text(driver, "Bed Status").await?;
        assert_eq!(bed_status, "Out of Service - Maintenance");

        // And notifications are sent to both:
        let notifications = vec![
            row([("Department", "Housekeeping"), ("Notification Details", "Hold cleaning until maintenance")]),
            row([("Department", "Maintenance"), ("Notification Details", "IV pump and call light repair")]),
        ];
        for notification in &notifications {
            assert_eq!(get_text(driver, notification["Department"].as_str()).await?, notification["Notification Details"].as_str());
        }

        // And the bed is removed from available count until both issues are resolved
        wait_for_test_id(driver, "Available Bed Count").await?;

        // And a work order is automatically generated for maintenance
        wait_for_test_id(driver, "Maintenance Work Order").await?;

        // And the estimated downtime is calculated and displayed
        let estimated_downtime = get_text(driver, "Estimated Downtime").await?;
        assert!(estimated_downtime.len() > 0);
        Ok(())
    })
}

fn scenario_05_update_bed_status_during_patient_transfer(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a patient "Robert Wilson" is in bed "ED-6"
        // And the patient needs to be transferred to ICU
        // (assumed pre-seeded test data)

        // When the transport team arrives to transfer the patient
        driver.find(By::Css("[data-testid=\"transport-team-arrived-button\"]")).await?.click().await?;

        // And I update the bed status to "Patient in Transit"
        fill_field(driver, "New Bed Status", "Patient in Transit").await?;

        // And I specify the destination as "ICU Room 302"
        fill_field(driver, "Destination", "ICU Room 302").await?;

        // And I submit the status update
        driver.find(By::Css("[data-testid=\"submit-bed-status-update\"]")).await?.click().await?;

        // Then the bed status is temporarily set to "In Transit"
        wait_for_test_id(driver, "Bed Status").await?;
        let bed_status = get_text(driver, "Bed Status").await?;
        assert_eq!(bed_status, "In Transit");

        // And the bed remains unavailable for new assignments
        let bed_availability = get_text(driver, "Bed Availability").await?;
        assert_match(&bed_availability, r"unavailable", true);

        // And a notification is sent to the receiving unit
        wait_for_test_id(driver, "Receiving Unit Notification").await?;

        // And when the transfer is confirmed complete, I can mark the bed as "Needs Cleaning"
        driver.find(By::Css("[data-testid=\"confirm-transfer-complete-button\"]")).await?.click().await?;
        fill_field(driver, "New Bed Status", "Needs Cleaning").await?;

        // And the normal cleaning workflow is initiated
        wait_for_test_id(driver, "Cleaning Workflow Status").await?;
        Ok(())
    })
}

fn scenario_06_handle_multiple_bed_status_updates_simultaneously(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given multiple beds require status updates:
        // (assumed pre-seeded test data)

        // When I perform batch bed status updates:
        fill_fields(driver, &vec![
            row([("Field", "ED-3 New Status"), ("Value", "Needs Cleaning")]),
            row([("Field", "ED-7 New Status"), ("Value", "Patient in Transit")]),
            row([("Field", "ED-11 New Status"), ("Value", "Available")]),
            row([("Field", "ED-14 New Status"), ("Value", "Needs Cleaning")]),
        ]).await?;
        driver.find(By::Css("[data-testid=\"submit-batch-bed-status-update\"]")).await?.click().await?;

        // Then the system processes all updates simultaneously
        wait_for_test_id(driver, "Batch Update Status").await?;

        // And appropriate notifications are sent to all relevant departments
        wait_for_test_id(driver, "Department Notifications Sent").await?;

        // And the bed availability dashboard is updated in real-time
        wait_for_test_id(driver, "Bed Availability Dashboard").await?;

        // And the total available bed count reflects all changes
        wait_for_test_id(driver, "Available Bed Count").await?;
        Ok(())
    })
}

fn scenario_07_handle_urgent_bed_turnover_request(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given the ED is at 95% capacity
        // And there is a trauma patient incoming requiring immediate bed
        // And bed "ED-4" patient is ready for discharge
        // (assumed pre-seeded test data)

        // When I mark the discharge as "Urgent Turnover Required"
        fill_field(driver, "New Bed Status", "Urgent Turnover Required").await?;

        // And I request expedited cleaning for bed "ED-4"
        driver.find(By::Css("[data-testid=\"request-expedited-cleaning-button\"]")).await?.click().await?;

        // And I submit the urgent status update
        driver.find(By::Css("[data-testid=\"submit-bed-status-update\"]")).await?.click().await?;

        // Then the system updates bed status to "Dirty - Urgent"
        wait_for_test_id(driver, "Bed Status").await?;
        let bed_status = get_text(driver, "Bed Status").await?;
        assert_eq!(bed_status, "Dirty - Urgent");

        // And a high-priority notification is sent to housekeeping:
        let notification_fields = vec![
            row([("Field", "Priority Level"), ("Value", "URGENT")]),
            row([("Field", "Room Number"), ("Value", "ED-4")]),
            row([("Field", "Reason"), ("Value", "Incoming trauma patient")]),
            row([("Field", "Target Time"), ("Value", "15 minutes")]),
            row([("Field", "Special Instructions"), ("Value", "Expedited cleaning protocol")]),
        ];
        for row_data in &notification_fields {
            let field = row_data["Field"].as_str();
            let value = row_data["Value"].as_str();
            assert_eq!(get_text(driver, field).await?, value);
        }

        // And the charge nurse is notified of the urgent turnover request
        wait_for_test_id(driver, "Charge Nurse Notification").await?;

        // And a timer is started to track cleaning completion time
        wait_for_test_id(driver, "Cleaning Completion Timer").await?;
        Ok(())
    })
}

fn scenario_08_validate_bed_status_change_restrictions(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given bed "ED-9" currently has status "Occupied"
        // And a patient "Susan Davis" is actively receiving treatment
        // (assumed pre-seeded test data)

        // When I attempt to mark the bed as "Available"
        fill_field(driver, "New Bed Status", "Available").await?;

        // And I submit the invalid status change
        driver.find(By::Css("[data-testid=\"submit-bed-status-update\"]")).await?.click().await?;

        // Then the system displays a validation error:
        let validation_errors = vec![
            row([("Error Type", "Invalid Transition"), ("Message", "Cannot mark occupied bed as available")]),
            row([("Error Type", "Required Action"), ("Message", "Discharge patient first")]),
            row([("Error Type", "Current Patient"), ("Message", "Susan Davis - Active treatment")]),
        ];
        for error in &validation_errors {
            assert_eq!(get_text(driver, error["Error Type"].as_str()).await?, error["Message"].as_str());
        }

        // And the bed status remains "Occupied"
        let bed_status = get_text(driver, "Bed Status").await?;
        assert_eq!(bed_status, "Occupied");

        // And no notifications are sent
        let notifications = driver.find_all(By::Css("[data-testid=\"housekeeping-notification\"]")).await?;
        assert_eq!(notifications.len(), 0);

        // And I am prompted to follow proper discharge workflow
        wait_for_test_id(driver, "Discharge Workflow Prompt").await?;
        Ok(())
    })
}

fn scenario_09_track_bed_status_history_and_audit_trail(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given bed "ED-5" has had multiple status changes today
        // (assumed pre-seeded test data)

        // When I access the bed status history
        driver.find(By::Css("[data-testid=\"view-bed-status-history-button\"]")).await?.click().await?;

        // Then the system displays the complete audit trail:
        let audit_trail_entries = driver.find_all(By::Css("[data-testid=\"audit-trail-entry\"]")).await?;
        assert_eq!(audit_trail_entries.len(), 5);

        // And each status change includes timestamp and user identification
        wait_for_test_id(driver, "Audit Trail").await?;

        // And the audit trail is preserved for compliance reporting
        let compliance_status = get_text(driver, "Compliance Reporting Status").await?;
        assert_match(&compliance_status, r"preserved", true);

        // And I can generate reports on bed utilization patterns
        wait_for_test_id(driver, "Generate Utilization Report Button").await?;
        Ok(())
    })
}

fn scenario_10_handle_bed_status_update_during_system_maintenance(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given the housekeeping notification system is temporarily offline
        // And a patient is discharged from bed "ED-16"
        // (assumed pre-seeded test data)

        // When I mark the bed as "Needs Cleaning"
        fill_field(driver, "New Bed Status", "Needs Cleaning").await?;

        // And I submit the status update
        driver.find(By::Css("[data-testid=\"submit-bed-status-update\"]")).await?.click().await?;

        // Then the system updates the bed status to "Dirty"
        wait_for_test_id(driver, "Bed Status").await?;
        let bed_status = get_text(driver, "Bed Status").await?;
        assert_eq!(bed_status, "Dirty");

        // And the system queues the housekeeping notification for later delivery
        wait_for_test_id(driver, "Queued Notification Status").await?;

        // And a warning message is displayed: "Housekeeping system offline - notification queued"
        let warning_message = get_text(driver, "Warning Message").await?;
        assert_eq!(warning_message, "Housekeeping system offline - notification queued");

        // And the bed is still removed from available count
        wait_for_test_id(driver, "Available Bed Count").await?;

        // And when the housekeeping system comes back online, queued notifications are automatically sent
        // (system behavior outside the scope of this interaction)

        // And a log entry is created documenting the delayed notification
        wait_for_test_id(driver, "Delayed Notification Log Entry").await?;
        Ok(())
    })
}

fn main() {
    run_feature(&[
        ("Mark bed as needs cleaning after patient discharge", scenario_01_mark_bed_as_needs_cleaning_after_patient_discharge),
        ("Mark isolation bed for deep cleaning after infectious patient", scenario_02_mark_isolation_bed_for_deep_cleaning_after_infectious_patien),
        ("Housekeeping completes cleaning and marks bed ready", scenario_03_housekeeping_completes_cleaning_and_marks_bed_ready),
        ("Handle bed maintenance request during status update", scenario_04_handle_bed_maintenance_request_during_status_update),
        ("Update bed status during patient transfer", scenario_05_update_bed_status_during_patient_transfer),
        ("Handle multiple bed status updates simultaneously", scenario_06_handle_multiple_bed_status_updates_simultaneously),
        ("Handle urgent bed turnover request", scenario_07_handle_urgent_bed_turnover_request),
        ("Validate bed status change restrictions", scenario_08_validate_bed_status_change_restrictions),
        ("Track bed status history and audit trail", scenario_09_track_bed_status_history_and_audit_trail),
        ("Handle bed status update during system maintenance", scenario_10_handle_bed_status_update_during_system_maintenance),
    ]);
}
