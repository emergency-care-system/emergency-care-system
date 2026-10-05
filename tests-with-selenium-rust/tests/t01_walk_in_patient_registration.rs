// Selenium WebDriver + libtest-mimic test for
// tests-with-given-when-then-features/01-walk-in-patient-registration.feature
// (equivalent to tests-with-selenium-javascript/01-walk-in-patient-registration.test.js).
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
    //   And I am logged in as a registration clerk
    verify_system_is_operational(driver).await?;
    login(driver, "a registration clerk", false).await?;

    // The demo app is a single-page dashboard: after login, select this
    // feature's panel from the sidebar nav (data-testid="nav-<slug>").
    let registration_nav_link = wait_for_test_id(driver, "Nav Walk In Patient Registration").await?;
    registration_nav_link.click().await?;
    wait_for_test_id(driver, "Patient Registration Form").await?;
    Ok(())
}

fn scenario_01_successfully_register_a_new_walk_in_patient(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a new patient arrives at the ED without prior registration
        // And the patient provides valid identification
        // When I enter the patient's demographic information:
        fill_fields(driver, &vec![
            row([("Field", "Given Name"), ("Value", "John")]),
            row([("Field", "Family Name"), ("Value", "Doe")]),
            row([("Field", "Date of Birth"), ("Value", "1985-06-15")]),
            row([("Field", "Phone Number"), ("Value", "555-123-4567")]),
            row([("Field", "Address"), ("Value", "123 Main St")]),
            row([("Field", "City"), ("Value", "Springfield")]),
            row([("Field", "State"), ("Value", "IL")]),
            row([("Field", "Zip Code"), ("Value", "62701")]),
        ]).await?;
        // And I enter the patient's insurance details:
        fill_fields(driver, &vec![
            row([("Field", "Insurance Type"), ("Value", "Blue Cross")]),
            row([("Field", "Policy Number"), ("Value", "BC123456789")]),
            row([("Field", "Group Number"), ("Value", "GRP001")]),
        ]).await?;
        // And I submit the registration form
        driver.find(By::Css("[data-testid=\"submit-registration-form\"]")).await?.click().await?;

        // Then the system creates a unique patient record
        wait_for_test_id(driver, "Medical Record Number").await?;
        // And the system assigns a medical record number
        let medical_record_number = get_text(driver, "Medical Record Number").await?;
        assert!(medical_record_number.len() > 0);
        // And the patient is queued for triage
        let triage_queue_status = get_text(driver, "Triage Queue Status").await?;
        assert_match(&triage_queue_status, r"queued for triage", true);
        // And I see a confirmation message "Patient successfully registered"
        let confirmation_message = get_text(driver, "Confirmation Message").await?;
        assert_eq!(confirmation_message, "Patient successfully registered");
        // And the medical record number is displayed
        let medical_record_number_element = driver.find(locator("Medical Record Number")).await?;
        assert!(medical_record_number_element.is_displayed().await?);
        Ok(())
    })
}

fn scenario_02_register_patient_with_missing_insurance_information(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a new patient arrives at the ED without prior registration
        // And the patient does not have insurance information
        // When I enter the patient's demographic information:
        fill_fields(driver, &vec![
            row([("Field", "Given Name"), ("Value", "Jane")]),
            row([("Field", "Family Name"), ("Value", "Smith")]),
            row([("Field", "Date of Birth"), ("Value", "1990-03-22")]),
            row([("Field", "Phone Number"), ("Value", "555-987-6543")]),
            row([("Field", "Address"), ("Value", "456 Oak Ave")]),
        ]).await?;
        // And I select "Self-Pay" as the insurance type
        fill_field(driver, "Insurance Type", "Self-Pay").await?;
        // And I submit the registration form
        driver.find(By::Css("[data-testid=\"submit-registration-form\"]")).await?.click().await?;

        // Then the system creates a unique patient record
        wait_for_test_id(driver, "Medical Record Number").await?;
        // And the system assigns a medical record number
        let medical_record_number = get_text(driver, "Medical Record Number").await?;
        assert!(medical_record_number.len() > 0);
        // And the patient is queued for triage
        let triage_queue_status = get_text(driver, "Triage Queue Status").await?;
        assert_match(&triage_queue_status, r"queued for triage", true);
        // And the insurance status is marked as "Self-Pay"
        let insurance_status = get_text(driver, "Insurance Status").await?;
        assert_eq!(insurance_status, "Self-Pay");
        Ok(())
    })
}

fn scenario_03_handle_duplicate_patient_registration_attempt(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a patient with the same name and date of birth already exists in the system
        // When I enter the patient's demographic information:
        fill_fields(driver, &vec![
            row([("Field", "Given Name"), ("Value", "John")]),
            row([("Field", "Family Name"), ("Value", "Doe")]),
            row([("Field", "Date of Birth"), ("Value", "1985-06-15")]),
        ]).await?;
        // And I submit the registration form
        driver.find(By::Css("[data-testid=\"submit-registration-form\"]")).await?.click().await?;

        // Then the system displays a warning "Potential duplicate patient found"
        let warning_message = get_text(driver, "Duplicate Patient Warning").await?;
        assert_eq!(warning_message, "Potential duplicate patient found");
        // And the system shows existing patient records for verification
        let existing_records = driver.find_all(By::Css("[data-testid=\"existing-patient-record\"]")).await?;
        assert!(existing_records.len() > 0);
        // And I can choose to link to existing record or create new record
        wait_for_test_id(driver, "Link to Existing Record").await?;
        wait_for_test_id(driver, "Create New Record").await?;
        Ok(())
    })
}

fn scenario_04_registration_with_invalid_demographic_data(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a new patient arrives at the ED without prior registration
        // When I enter incomplete demographic information:
        fill_fields(driver, &vec![
            row([("Field", "Given Name"), ("Value", "John")]),
            row([("Field", "Family Name"), ("Value", "")]),
            row([("Field", "Date of Birth"), ("Value", "invalid-date")]),
        ]).await?;
        // And I submit the registration form
        driver.find(By::Css("[data-testid=\"submit-registration-form\"]")).await?.click().await?;

        // Then the system displays validation errors:
        let family_name_error = get_text(driver, "Family Name Error").await?;
        assert_eq!(family_name_error, "Family name is required");
        let date_of_birth_error = get_text(driver, "Date of Birth Error").await?;
        assert_eq!(date_of_birth_error, "Invalid date format");
        // And the patient record is not created
        let medical_record_numbers = driver.find_all(locator("Medical Record Number")).await?;
        assert_eq!(medical_record_numbers.len(), 0);
        // And the form remains open for correction
        let registration_form = driver.find(By::Css("[data-testid=\"patient-registration-form\"]")).await?;
        assert!(registration_form.is_displayed().await?);
        Ok(())
    })
}

fn main() {
    run_feature(&[
        ("Successfully register a new walk-in patient", scenario_01_successfully_register_a_new_walk_in_patient),
        ("Register patient with missing insurance information", scenario_02_register_patient_with_missing_insurance_information),
        ("Handle duplicate patient registration attempt", scenario_03_handle_duplicate_patient_registration_attempt),
        ("Registration with invalid demographic data", scenario_04_registration_with_invalid_demographic_data),
    ]);
}
