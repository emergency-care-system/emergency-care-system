// Selenium WebDriver + libtest-mimic test for
// tests-with-given-when-then-features/03-initial-triage-assessment.feature
// (equivalent to tests-with-selenium-javascript/03-initial-triage-assessment.test.js).
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
    //   And I am logged in as a triage nurse
    //   And the ESI (Emergency Severity Index) scoring module is active
    verify_system_is_operational(driver).await?;
    login(driver, "a triage nurse", false).await?;
    // The ESI scoring module being active is assumed pre-seeded test data /
    // environment configuration.

    let feature_nav_link = wait_for_test_id(driver, "Nav Initial Triage Assessment").await?;
    feature_nav_link.click().await?;
    wait_for_test_id(driver, "Initial Triage Assessment Panel").await?;
    Ok(())
}

fn scenario_01_assess_patient_with_chest_pain_esi_level_2(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a registered patient "John Doe" is waiting for triage
        // And the patient was registered 10 minutes ago
        // (assumed pre-seeded test data)
        // When I select the patient for triage assessment
        driver.find(By::Css("[data-testid=\"select-patient-for-triage-button\"]")).await?.click().await?;

        // And I enter the vital signs:
        fill_fields(driver, &vec![
            row([("Field", "Blood Pressure"), ("Value", "160/95")]),
            row([("Field", "Heart Rate"), ("Value", "110")]),
            row([("Field", "Respiratory Rate"), ("Value", "22")]),
            row([("Field", "Temperature"), ("Value", "98.6°F")]),
            row([("Field", "Oxygen Saturation"), ("Value", "94%")]),
        ]).await?;
        // And I enter the chief complaint as "Chest pain and shortness of breath"
        fill_field(driver, "Chief Complaint", "Chest pain and shortness of breath").await?;
        // And I enter the pain scale as "8/10"
        fill_field(driver, "Pain Scale", "8/10").await?;
        // And I document onset as "Started 2 hours ago"
        fill_field(driver, "Onset", "Started 2 hours ago").await?;
        // And I submit the triage assessment
        driver.find(By::Css("[data-testid=\"submit-triage-assessment-form\"]")).await?.click().await?;

        // Then the system calculates an ESI score of "2"
        let esi_score = get_text(driver, "ESI Score").await?;
        assert_eq!(esi_score, "2");
        // And the system assigns triage level "High Priority"
        let triage_level = get_text(driver, "Triage Level").await?;
        assert_eq!(triage_level, "High Priority");
        // And the patient is positioned at the front of the high priority queue
        let queue_position = get_text(driver, "Queue Position").await?;
        assert_eq!(queue_position, "1");
        // And an alert is sent to the attending physician
        let physician_alert = get_text(driver, "Physician Alert").await?;
        assert_match(&physician_alert, r"attending physician", true);
        // And the estimated wait time is updated to "Immediate"
        let estimated_wait_time = get_text(driver, "Estimated Wait Time").await?;
        assert_eq!(estimated_wait_time, "Immediate");
        Ok(())
    })
}

fn scenario_02_assess_patient_with_minor_injury_esi_level_4(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a registered patient "Jane Smith" is waiting for triage
        // When I select the patient for triage assessment
        driver.find(By::Css("[data-testid=\"select-patient-for-triage-button\"]")).await?.click().await?;

        // And I enter the vital signs:
        fill_fields(driver, &vec![
            row([("Field", "Blood Pressure"), ("Value", "120/80")]),
            row([("Field", "Heart Rate"), ("Value", "75")]),
            row([("Field", "Respiratory Rate"), ("Value", "16")]),
            row([("Field", "Temperature"), ("Value", "98.2°F")]),
            row([("Field", "Oxygen Saturation"), ("Value", "99%")]),
        ]).await?;
        // And I enter the chief complaint as "Sprained ankle from fall"
        fill_field(driver, "Chief Complaint", "Sprained ankle from fall").await?;
        // And I enter the pain scale as "4/10"
        fill_field(driver, "Pain Scale", "4/10").await?;
        // And I document onset as "This morning while jogging"
        fill_field(driver, "Onset", "This morning while jogging").await?;
        // And I submit the triage assessment
        driver.find(By::Css("[data-testid=\"submit-triage-assessment-form\"]")).await?.click().await?;

        // Then the system calculates an ESI score of "4"
        let esi_score = get_text(driver, "ESI Score").await?;
        assert_eq!(esi_score, "4");
        // And the system assigns triage level "Less Urgent"
        let triage_level = get_text(driver, "Triage Level").await?;
        assert_eq!(triage_level, "Less Urgent");
        // And the patient is positioned in the less urgent queue
        let assigned_queue = get_text(driver, "Assigned Queue").await?;
        assert_match(&assigned_queue, r"less urgent", true);
        // And the estimated wait time is updated to "60-90 minutes"
        let estimated_wait_time = get_text(driver, "Estimated Wait Time").await?;
        assert_eq!(estimated_wait_time, "60-90 minutes");
        // And no immediate alerts are generated
        let physician_alerts = driver.find_all(locator("Physician Alert")).await?;
        assert_eq!(physician_alerts.len(), 0);
        Ok(())
    })
}

fn scenario_03_assess_critical_patient_requiring_immediate_attention_esi_le(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a registered patient "Emergency Patient" is waiting for triage
        // When I select the patient for triage assessment
        driver.find(By::Css("[data-testid=\"select-patient-for-triage-button\"]")).await?.click().await?;

        // And I enter the vital signs:
        fill_fields(driver, &vec![
            row([("Field", "Blood Pressure"), ("Value", "70/40")]),
            row([("Field", "Heart Rate"), ("Value", "140")]),
            row([("Field", "Respiratory Rate"), ("Value", "8")]),
            row([("Field", "Temperature"), ("Value", "95.0°F")]),
            row([("Field", "Oxygen Saturation"), ("Value", "85%")]),
        ]).await?;
        // And I enter the chief complaint as "Unresponsive after motor vehicle accident"
        fill_field(driver, "Chief Complaint", "Unresponsive after motor vehicle accident").await?;
        // And I enter the pain scale as "Unable to assess"
        fill_field(driver, "Pain Scale", "Unable to assess").await?;
        // And I mark the patient as "Requires immediate life-saving intervention"
        fill_field(driver, "Intervention Flag", "Requires immediate life-saving intervention").await?;
        // And I submit the triage assessment
        driver.find(By::Css("[data-testid=\"submit-triage-assessment-form\"]")).await?.click().await?;

        // Then the system calculates an ESI score of "1"
        let esi_score = get_text(driver, "ESI Score").await?;
        assert_eq!(esi_score, "1");
        // And the system assigns triage level "Resuscitation"
        let triage_level = get_text(driver, "Triage Level").await?;
        assert_eq!(triage_level, "Resuscitation");
        // And the patient is moved to the top of all queues
        let queue_position = get_text(driver, "Queue Position").await?;
        assert_eq!(queue_position, "1");
        // And a code alert is automatically triggered
        let code_alert = get_text(driver, "Code Alert").await?;
        assert_match(&code_alert, r"triggered", true);
        // And the trauma team is notified immediately
        let trauma_team_notification = get_text(driver, "Trauma Team Notification").await?;
        assert_match(&trauma_team_notification, r"notified", true);
        // And the estimated wait time shows "Immediate - In Progress"
        let estimated_wait_time = get_text(driver, "Estimated Wait Time").await?;
        assert_eq!(estimated_wait_time, "Immediate - In Progress");
        Ok(())
    })
}

fn scenario_04_assess_pediatric_patient_with_fever_esi_level_3(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a registered patient "Tommy Jones" (age 5) is waiting for triage
        // When I select the patient for triage assessment
        driver.find(By::Css("[data-testid=\"select-patient-for-triage-button\"]")).await?.click().await?;

        // And I enter the vital signs using pediatric parameters:
        fill_fields(driver, &vec![
            row([("Field", "Blood Pressure"), ("Value", "95/60")]),
            row([("Field", "Heart Rate"), ("Value", "120")]),
            row([("Field", "Respiratory Rate"), ("Value", "24")]),
            row([("Field", "Temperature"), ("Value", "103.2°F")]),
            row([("Field", "Oxygen Saturation"), ("Value", "97%")]),
        ]).await?;
        // And I enter the chief complaint as "High fever and irritability"
        fill_field(driver, "Chief Complaint", "High fever and irritability").await?;
        // And I enter the pain scale as "6/10 (using FACES scale)"
        fill_field(driver, "Pain Scale", "6/10 (using FACES scale)").await?;
        // And I document onset as "Fever started yesterday evening"
        fill_field(driver, "Onset", "Fever started yesterday evening").await?;
        // And I submit the triage assessment
        driver.find(By::Css("[data-testid=\"submit-triage-assessment-form\"]")).await?.click().await?;

        // Then the system calculates an ESI score of "3" using pediatric criteria
        let esi_score = get_text(driver, "ESI Score").await?;
        assert_eq!(esi_score, "3");
        let scoring_criteria = get_text(driver, "Scoring Criteria").await?;
        assert_match(&scoring_criteria, r"pediatric", true);
        // And the system assigns triage level "Urgent"
        let triage_level = get_text(driver, "Triage Level").await?;
        assert_eq!(triage_level, "Urgent");
        // And the patient is positioned in the urgent pediatric queue
        let assigned_queue = get_text(driver, "Assigned Queue").await?;
        assert_match(&assigned_queue, r"urgent pediatric", true);
        // And the pediatric team is notified
        let pediatric_team_notification = get_text(driver, "Pediatric Team Notification").await?;
        assert_match(&pediatric_team_notification, r"notified", true);
        // And the estimated wait time is updated to "30-45 minutes"
        let estimated_wait_time = get_text(driver, "Estimated Wait Time").await?;
        assert_eq!(estimated_wait_time, "30-45 minutes");
        Ok(())
    })
}

fn scenario_05_handle_incomplete_vital_signs_during_triage(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a registered patient "Mary Johnson" is waiting for triage
        // When I select the patient for triage assessment
        driver.find(By::Css("[data-testid=\"select-patient-for-triage-button\"]")).await?.click().await?;

        // And I attempt to enter incomplete vital signs:
        //   | Vital Sign          | Value    |
        //   | Blood Pressure      | 130/85   |
        //   | Heart Rate          |          |
        //   | Respiratory Rate    | 18       |
        //   | Temperature         |          |
        //   | Oxygen Saturation   | 98%      |
        fill_fields(driver, &vec![
            row([("Field", "Blood Pressure"), ("Value", "130/85")]),
            row([("Field", "Heart Rate"), ("Value", "")]),
            row([("Field", "Respiratory Rate"), ("Value", "18")]),
            row([("Field", "Temperature"), ("Value", "")]),
            row([("Field", "Oxygen Saturation"), ("Value", "98%")]),
        ]).await?;
        // And I enter the chief complaint as "Headache"
        fill_field(driver, "Chief Complaint", "Headache").await?;
        // And I submit the triage assessment
        driver.find(By::Css("[data-testid=\"submit-triage-assessment-form\"]")).await?.click().await?;

        // Then the system displays validation errors:
        //   | Missing Field       | Error Message                |
        //   | Heart Rate          | Heart rate is required       |
        //   | Temperature         | Temperature is required      |
        assert_eq!(get_text(driver, "Heart Rate Error").await?, "Heart rate is required");
        assert_eq!(get_text(driver, "Temperature Error").await?, "Temperature is required");
        // And the ESI score cannot be calculated
        let esi_score_elements = driver.find_all(locator("ESI Score")).await?;
        assert_eq!(esi_score_elements.len(), 0);
        // And the assessment remains incomplete
        let assessment_status = get_text(driver, "Assessment Status").await?;
        assert_eq!(assessment_status, "Incomplete");
        // And I must complete all required fields before proceeding
        let triage_form = driver.find(By::Css("[data-testid=\"triage-assessment-form\"]")).await?;
        assert!(triage_form.is_displayed().await?);
        Ok(())
    })
}

fn scenario_06_reassess_patient_with_worsening_condition(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a patient "Robert Davis" has been triaged as ESI Level 4
        // And the patient has been waiting for 90 minutes
        // When I select the patient for reassessment
        driver.find(By::Css("[data-testid=\"select-patient-for-reassessment-button\"]")).await?.click().await?;

        // And I enter updated vital signs:
        fill_fields(driver, &vec![
            row([("Field", "Blood Pressure"), ("Value", "90/50")]),
            row([("Field", "Heart Rate"), ("Value", "120")]),
            row([("Field", "Respiratory Rate"), ("Value", "26")]),
            row([("Field", "Temperature"), ("Value", "101.5°F")]),
            row([("Field", "Oxygen Saturation"), ("Value", "92%")]),
        ]).await?;
        // And I update the chief complaint to "Worsening abdominal pain with nausea"
        fill_field(driver, "Chief Complaint", "Worsening abdominal pain with nausea").await?;
        // And I enter the updated pain scale as "9/10"
        fill_field(driver, "Pain Scale", "9/10").await?;
        // And I submit the reassessment
        driver.find(By::Css("[data-testid=\"submit-reassessment-form\"]")).await?.click().await?;

        // Then the system recalculates the ESI score to "2"
        let esi_score = get_text(driver, "ESI Score").await?;
        assert_eq!(esi_score, "2");
        // And the system updates triage level to "High Priority"
        let triage_level = get_text(driver, "Triage Level").await?;
        assert_eq!(triage_level, "High Priority");
        // And the patient is moved to the front of the high priority queue
        let queue_position = get_text(driver, "Queue Position").await?;
        assert_eq!(queue_position, "1");
        // And an escalation alert is sent to the charge nurse
        let escalation_alert = get_text(driver, "Escalation Alert").await?;
        assert_match(&escalation_alert, r"charge nurse", true);
        // And a note is added documenting the condition change
        let condition_change_note = get_text(driver, "Condition Change Note").await?;
        assert!(condition_change_note.len() > 0);
        Ok(())
    })
}

fn scenario_07_process_multiple_patients_in_triage_queue(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given multiple patients are waiting for triage:
        //   | Patient Name    | Registration Time | Status        |
        //   | Alice Brown     | 10:00 AM           | Waiting       |
        //   | Bob Wilson      | 10:15 AM           | Waiting       |
        //   | Carol Davis     | 10:30 AM           | Waiting       |
        // (assumed pre-seeded test data)
        // When I complete triage assessments for all patients:
        //   | Patient Name | ESI Score | Triage Level  |
        //   | Alice Brown  | 3         | Urgent        |
        //   | Bob Wilson   | 4         | Less Urgent   |
        //   | Carol Davis  | 2         | High Priority |
        driver.find(By::Css("[data-testid=\"complete-all-triage-assessments-button\"]")).await?.click().await?;

        // Then the system positions patients in queue order:
        //   | Queue Position | Patient Name | Triage Level  |
        //   | 1              | Carol Davis  | High Priority |
        //   | 2              | Alice Brown  | Urgent        |
        //   | 3              | Bob Wilson   | Less Urgent   |
        let queue_entries = driver.find_all(By::Css("[data-testid=\"triage-queue-entry\"]")).await?;
        assert_eq!(queue_entries.len(), 3);
        let first_queue_entry_text = queue_entries[0].text().await?;
        assert_match(&first_queue_entry_text, r"Carol Davis", false);
        // And wait times are calculated based on queue position and available resources
        let wait_time_calculation_status = get_text(driver, "Wait Time Calculation Status").await?;
        assert_match(&wait_time_calculation_status, r"calculated", true);
        // And the triage dashboard is updated with current queue status
        let triage_dashboard = wait_for_test_id(driver, "Triage Dashboard").await?;
        assert!(triage_dashboard.is_displayed().await?);
        Ok(())
    })
}

fn main() {
    run_feature(&[
        ("Assess patient with chest pain (ESI Level 2)", scenario_01_assess_patient_with_chest_pain_esi_level_2),
        ("Assess patient with minor injury (ESI Level 4)", scenario_02_assess_patient_with_minor_injury_esi_level_4),
        ("Assess critical patient requiring immediate attention (ESI Level 1)", scenario_03_assess_critical_patient_requiring_immediate_attention_esi_le),
        ("Assess pediatric patient with fever (ESI Level 3)", scenario_04_assess_pediatric_patient_with_fever_esi_level_3),
        ("Handle incomplete vital signs during triage", scenario_05_handle_incomplete_vital_signs_during_triage),
        ("Reassess patient with worsening condition", scenario_06_reassess_patient_with_worsening_condition),
        ("Process multiple patients in triage queue", scenario_07_process_multiple_patients_in_triage_queue),
    ]);
}
