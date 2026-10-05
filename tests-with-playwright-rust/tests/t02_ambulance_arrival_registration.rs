// Playwright + libtest-mimic test for
// tests-with-given-when-then-features/02-ambulance-arrival-registration.feature
// (equivalent to tests-with-playwright-javascript/02-ambulance-arrival-registration.test.js).
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
    //   And I am logged in as registration staff
    //   And the "unknown patient" registration module is available
    verify_system_is_operational(page).await?;
    login(page, "registration staff", false).await?;
    // The "unknown patient" registration module availability is assumed
    // pre-seeded test data / environment configuration.

    let feature_nav_link = wait_for_test_id(page, "Nav Ambulance Arrival Registration").await?;
    feature_nav_link.click(None).await?;
    wait_for_test_id(page, "Ambulance Arrival Registration Panel").await?;
    Ok(())
}

fn scenario_01_register_unconscious_patient_brought_by_ambulance(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given an ambulance arrives with a patient who cannot provide identification
        // And the patient is unconscious and has no identification documents
        // And EMS provides the following information:
        //   | Field                 | Value                    |
        //   | Estimated Age         | 45-50 years              |
        //   | Gender                | Male                     |
        //   | Chief Complaint       | Motor vehicle accident   |
        //   | Vital Signs           | BP: 90/60, HR: 120       |
        //   | Incident Location     | Highway 55 Mile Marker 12|
        //   | EMS Unit              | Ambulance 205            |
        //   | Arrival Time          | 14:30                    |
        // When I select "Unknown Patient" registration type
        fill_field(page, "Registration Type", "Unknown Patient").await?;
        // And I enter the available information from EMS
        fill_fields(page, &vec![
            row([("Field", "Estimated Age"), ("Value", "45-50 years")]),
            row([("Field", "Gender"), ("Value", "Male")]),
            row([("Field", "Chief Complaint"), ("Value", "Motor vehicle accident")]),
            row([("Field", "Vital Signs"), ("Value", "BP: 90/60, HR: 120")]),
            row([("Field", "Incident Location"), ("Value", "Highway 55 Mile Marker 12")]),
            row([("Field", "EMS Unit"), ("Value", "Ambulance 205")]),
            row([("Field", "Arrival Time"), ("Value", "14:30")]),
        ]).await?;
        // And I submit the registration
        page.get_by_test_id("submit-registration-form").first().click(None).await?;

        // Then the system creates a temporary patient record
        wait_for_test_id(page, "Temporary Patient Record").await?;
        // And the system assigns a placeholder ID starting with "UNK"
        let placeholder_id = get_text(page, "Placeholder ID").await?;
        assert_match(&placeholder_id, r"^UNK", false);
        // And the patient record is flagged for "Identity Verification Required"
        let identity_flag = get_text(page, "Identity Verification Flag").await?;
        assert_eq!(identity_flag, "Identity Verification Required");
        // And the patient is immediately queued for triage
        let triage_queue_status = get_text(page, "Triage Queue Status").await?;
        assert_match(&triage_queue_status, r"queued for triage", true);
        // And a notification is sent to the charge nurse about the unknown patient
        let charge_nurse_notification = get_text(page, "Charge Nurse Notification").await?;
        assert_match(&charge_nurse_notification, r"unknown patient", true);
        // And the record shows status as "Temporary - Pending Identification"
        let record_status = get_text(page, "Record Status").await?;
        assert_eq!(record_status, "Temporary - Pending Identification");
        Ok(())
    })
}

fn scenario_02_register_patient_with_partial_identification_from_personal_e(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given an ambulance arrives with a patient who cannot provide identification
        // And the patient has a wallet with partial information
        // And EMS provides the following information:
        //   | Field                 | Value                    |
        //   | Estimated Age         | 30-35 years              |
        //   | Gender                | Female                   |
        //   | Chief Complaint       | Drug overdose            |
        //   | Found Name            | Sarah (from credit card) |
        //   | Partial Phone         | 555-1234 (last 4 digits) |
        // When I select "Unknown Patient" registration type
        fill_field(page, "Registration Type", "Unknown Patient").await?;
        // And I enter the EMS information including partial identity details
        fill_fields(page, &vec![
            row([("Field", "Estimated Age"), ("Value", "30-35 years")]),
            row([("Field", "Gender"), ("Value", "Female")]),
            row([("Field", "Chief Complaint"), ("Value", "Drug overdose")]),
            row([("Field", "Found Name"), ("Value", "Sarah (from credit card)")]),
            row([("Field", "Partial Phone"), ("Value", "555-1234 (last 4 digits)")]),
        ]).await?;
        // And I mark the identity fields as "Unverified"
        fill_field(page, "Identity Status", "Unverified").await?;
        // And I submit the registration
        page.get_by_test_id("submit-registration-form").first().click(None).await?;

        // Then the system creates a temporary patient record
        wait_for_test_id(page, "Temporary Patient Record").await?;
        // And the system assigns a placeholder ID starting with "UNK"
        let placeholder_id = get_text(page, "Placeholder ID").await?;
        assert_match(&placeholder_id, r"^UNK", false);
        // And the partial identity information is stored with "Unverified" status
        let identity_status = get_text(page, "Identity Status").await?;
        assert_eq!(identity_status, "Unverified");
        // And the patient record is flagged for "Identity Verification Required"
        let identity_flag = get_text(page, "Identity Verification Flag").await?;
        assert_eq!(identity_flag, "Identity Verification Required");
        // And a task is created for social services to assist with identification
        let social_services_task = get_text(page, "Social Services Task").await?;
        assert_match(&social_services_task, r"identification", true);
        Ok(())
    })
}

fn scenario_03_register_patient_who_becomes_conscious_during_registration(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given an ambulance arrives with a patient who initially cannot provide identification
        // And I have started the "Unknown Patient" registration process

        // When the patient becomes conscious and provides identification:
        fill_fields(page, &vec![
            row([("Field", "Full Name"), ("Value", "Michael Johnson")]),
            row([("Field", "Date of Birth"), ("Value", "1980-12-15")]),
            row([("Field", "Phone Number"), ("Value", "555-876-5432")]),
        ]).await?;
        // And I verify the provided identification
        page.get_by_test_id("verify-identification-button").first().click(None).await?;

        // Then the system converts the temporary record to a verified patient record
        wait_for_test_id(page, "Medical Record Number").await?;
        // And the placeholder ID is replaced with a permanent medical record number
        let medical_record_number = get_text(page, "Medical Record Number").await?;
        assert!(!medical_record_number.starts_with("UNK"));
        // And the "Identity Verification Required" flag is removed
        let identity_flag_elements = locator(page, "Identity Verification Flag");
        assert_eq!(identity_flag_elements.count().await?, 0);
        // And the patient demographic information is updated
        let patient_name = get_text(page, "Patient Name").await?;
        assert_eq!(patient_name, "Michael Johnson");
        // And a note is added documenting the identification process
        let identification_note = get_text(page, "Identification Note").await?;
        assert!(identification_note.len() > 0);
        Ok(())
    })
}

fn scenario_04_handle_multiple_unknown_patients_from_mass_casualty_incident(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given multiple ambulances arrive from a mass casualty incident
        // And none of the patients can provide identification

        // When I select "Unknown Patient - Mass Casualty" registration type
        fill_field(page, "Registration Type", "Unknown Patient - Mass Casualty").await?;
        // And I enter the incident information:
        fill_fields(page, &vec![
            row([("Field", "Incident Type"), ("Value", "Multi-vehicle accident")]),
            row([("Field", "Incident Location"), ("Value", "Interstate 70 Exit 45")]),
            row([("Field", "Total Patients"), ("Value", "4")]),
        ]).await?;
        // And I register each patient with EMS-provided information
        page.get_by_test_id("submit-registration-form").first().click(None).await?;

        // Then the system creates temporary records for all patients
        let temporary_records = page.get_by_test_id("temporary-patient-record");
        assert!(temporary_records.count().await? > 0);
        // And each patient gets a sequential placeholder ID (UNK-001, UNK-002, etc.)
        let placeholder_id = get_text(page, "Placeholder ID").await?;
        assert_match(&placeholder_id, r"^UNK-\d{3}$", false);
        // And all records are linked to the same incident number
        let incident_number = get_text(page, "Incident Number").await?;
        assert!(incident_number.len() > 0);
        // And the mass casualty protocol is activated
        let mass_casualty_protocol_status = get_text(page, "Mass Casualty Protocol Status").await?;
        assert_match(&mass_casualty_protocol_status, r"activated", true);
        // And notifications are sent to administration and social services
        let notification_recipients = get_text(page, "Notification Recipients").await?;
        assert_match(&notification_recipients, r"administration", true);
        Ok(())
    })
}

fn scenario_05_attempt_to_register_unknown_patient_without_ems_information(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given an ambulance arrives with a patient who cannot provide identification
        // And EMS has minimal information available

        // When I select "Unknown Patient" registration type
        fill_field(page, "Registration Type", "Unknown Patient").await?;
        // And I attempt to submit with only basic information:
        //   | Field                 | Value                    |
        //   | Gender                | Unknown                  |
        //   | Estimated Age         | Unknown                  |
        //   | Chief Complaint       |                          |
        fill_fields(page, &vec![
            row([("Field", "Gender"), ("Value", "Unknown")]),
            row([("Field", "Estimated Age"), ("Value", "Unknown")]),
            row([("Field", "Chief Complaint"), ("Value", "")]),
        ]).await?;
        page.get_by_test_id("submit-registration-form").first().click(None).await?;

        // Then the system displays a warning "Insufficient information for registration"
        let warning_message = get_text(page, "Warning Message").await?;
        assert_eq!(warning_message, "Insufficient information for registration");
        // And the system requires minimum data fields:
        //   | Required Field        | Requirement                       |
        //   | Estimated Age Range   | Must be provided                  |
        //   | Gender                | Must be Male, Female, or Unknown  |
        //   | Chief Complaint       | Must be provided                  |
        assert_eq!(get_text(page, "Estimated Age Range Error").await?, "Must be provided");
        assert_eq!(get_text(page, "Gender Error").await?, "Must be Male, Female, or Unknown");
        assert_eq!(get_text(page, "Chief Complaint Error").await?, "Must be provided");
        // And the registration cannot be completed until minimum requirements are met
        let placeholder_ids = locator(page, "Placeholder ID");
        assert_eq!(placeholder_ids.count().await?, 0);
        Ok(())
    })
}

fn scenario_06_identity_verification_process_after_patient_stabilization(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given a patient was registered as "Unknown Patient"
        // And the patient has now stabilized
        // And the patient can provide identification

        // When the nurse initiates the identity verification process
        page.get_by_test_id("initiate-identity-verification-button").first().click(None).await?;
        // And the patient provides valid identification:
        fill_fields(page, &vec![
            row([("Field", "Full Name"), ("Value", "Robert Davis")]),
            row([("Field", "Date of Birth"), ("Value", "1975-08-20")]),
            row([("Field", "Social Security"), ("Value", "XXX-XX-1234 (last 4)")]),
        ]).await?;
        // And the identification is verified
        page.get_by_test_id("verify-identification-button").first().click(None).await?;

        // Then the system merges the temporary record with verified information
        wait_for_test_id(page, "Medical Record Number").await?;
        let merge_status = get_text(page, "Record Merge Status").await?;
        assert_match(&merge_status, r"merged", true);
        // And the "Identity Verification Required" flag is cleared
        let identity_flag_elements = locator(page, "Identity Verification Flag");
        assert_eq!(identity_flag_elements.count().await?, 0);
        // And a permanent medical record number is assigned
        let medical_record_number = get_text(page, "Medical Record Number").await?;
        assert!(medical_record_number.len() > 0);
        // And all clinical documentation is preserved under the new verified record
        let clinical_documentation_status = get_text(page, "Clinical Documentation Status").await?;
        assert_match(&clinical_documentation_status, r"preserved", true);
        // And billing information is updated with verified patient details
        let billing_status = get_text(page, "Billing Status").await?;
        assert_match(&billing_status, r"updated", true);
        Ok(())
    })
}

fn main() {
    run_feature(&[
        ("Register unconscious patient brought by ambulance", scenario_01_register_unconscious_patient_brought_by_ambulance),
        ("Register patient with partial identification from personal effects", scenario_02_register_patient_with_partial_identification_from_personal_e),
        ("Register patient who becomes conscious during registration", scenario_03_register_patient_who_becomes_conscious_during_registration),
        ("Handle multiple unknown patients from mass casualty incident", scenario_04_handle_multiple_unknown_patients_from_mass_casualty_incident),
        ("Attempt to register unknown patient without EMS information", scenario_05_attempt_to_register_unknown_patient_without_ems_information),
        ("Identity verification process after patient stabilization", scenario_06_identity_verification_process_after_patient_stabilization),
    ]);
}
