// Selenium WebDriver + libtest-mimic test for
// tests-with-given-when-then-features/04-triage-re-assessment.feature
// (equivalent to tests-with-selenium-javascript/04-triage-re-assessment.test.js).
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
    //   And the automatic reassessment alerts are enabled
    verify_system_is_operational(driver).await?;
    login(driver, "a triage nurse", false).await?;
    // The automatic reassessment alerts being enabled is assumed pre-seeded
    // test data / environment configuration.

    let feature_nav_link = wait_for_test_id(driver, "Nav Triage Re-assessment").await?;
    feature_nav_link.click().await?;
    wait_for_test_id(driver, "Triage Re-assessment Panel").await?;
    Ok(())
}

fn scenario_01_reassess_patient_with_worsening_condition_after_2_hours(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a patient "Sarah Johnson" has been waiting in the queue for 2 hours
        // And the patient's initial triage was ESI Level 4 (Less Urgent)
        // And the patient's initial vital signs were:
        //   | Vital Sign          | Initial Value |
        //   | Blood Pressure      | 125/78        |
        //   | Heart Rate          | 82            |
        //   | Respiratory Rate    | 16            |
        //   | Temperature         | 99.1°F        |
        //   | Oxygen Saturation   | 98%           |
        //   | Pain Scale          | 3/10          |
        // (assumed pre-seeded test data)
        // When the system triggers a reassessment alert at the 2-hour mark
        wait_for_test_id(driver, "Reassessment Alert").await?;
        // And I select the patient for reassessment
        driver.find(By::Css("[data-testid=\"select-patient-for-reassessment-button\"]")).await?.click().await?;

        // And I enter the updated vital signs:
        fill_fields(driver, &vec![
            row([("Field", "Blood Pressure"), ("Value", "95/55")]),
            row([("Field", "Heart Rate"), ("Value", "115")]),
            row([("Field", "Respiratory Rate"), ("Value", "24")]),
            row([("Field", "Temperature"), ("Value", "101.8°F")]),
            row([("Field", "Oxygen Saturation"), ("Value", "94%")]),
            row([("Field", "Pain Scale"), ("Value", "8/10")]),
        ]).await?;
        // And I update the chief complaint to "Severe abdominal pain with dizziness"
        fill_field(driver, "Chief Complaint", "Severe abdominal pain with dizziness").await?;
        // And I submit the reassessment
        driver.find(By::Css("[data-testid=\"submit-reassessment-form\"]")).await?.click().await?;

        // Then the system recalculates the ESI score from "4" to "2"
        let esi_score = get_text(driver, "ESI Score").await?;
        assert_eq!(esi_score, "2");
        // And the system updates the triage level from "Less Urgent" to "High Priority"
        let triage_level = get_text(driver, "Triage Level").await?;
        assert_eq!(triage_level, "High Priority");
        // And the patient is moved from position 12 to position 2 in the queue
        let queue_position = get_text(driver, "Queue Position").await?;
        assert_eq!(queue_position, "2");
        // And an escalation alert is sent to the charge nurse
        let escalation_alert = get_text(driver, "Escalation Alert").await?;
        assert_match(&escalation_alert, r"charge nurse", true);
        // And the estimated wait time is updated from "90 minutes" to "15 minutes"
        let estimated_wait_time = get_text(driver, "Estimated Wait Time").await?;
        assert_eq!(estimated_wait_time, "15 minutes");
        // And a reassessment note is automatically added to the patient record
        let reassessment_note = get_text(driver, "Reassessment Note").await?;
        assert!(reassessment_note.len() > 0);
        Ok(())
    })
}

fn scenario_02_reassess_patient_with_stable_condition(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a patient "Michael Chen" has been waiting in the queue for 2 hours
        // And the patient's initial triage was ESI Level 3 (Urgent)
        // And the patient's initial vital signs were:
        //   | Vital Sign          | Initial Value |
        //   | Blood Pressure      | 140/90        |
        //   | Heart Rate          | 95            |
        //   | Respiratory Rate    | 20            |
        //   | Temperature         | 100.2°F       |
        //   | Oxygen Saturation   | 96%           |
        //   | Pain Scale          | 6/10          |
        // (assumed pre-seeded test data)
        // When I perform a scheduled reassessment
        driver.find(By::Css("[data-testid=\"select-patient-for-reassessment-button\"]")).await?.click().await?;

        // And I enter the updated vital signs:
        fill_fields(driver, &vec![
            row([("Field", "Blood Pressure"), ("Value", "135/85")]),
            row([("Field", "Heart Rate"), ("Value", "88")]),
            row([("Field", "Respiratory Rate"), ("Value", "18")]),
            row([("Field", "Temperature"), ("Value", "99.8°F")]),
            row([("Field", "Oxygen Saturation"), ("Value", "97%")]),
            row([("Field", "Pain Scale"), ("Value", "5/10")]),
        ]).await?;
        // And I note "Patient reports feeling slightly better"
        fill_field(driver, "Reassessment Note", "Patient reports feeling slightly better").await?;
        // And I submit the reassessment
        driver.find(By::Css("[data-testid=\"submit-reassessment-form\"]")).await?.click().await?;

        // Then the system recalculates and maintains ESI score of "3"
        let esi_score = get_text(driver, "ESI Score").await?;
        assert_eq!(esi_score, "3");
        // And the triage level remains "Urgent"
        let triage_level = get_text(driver, "Triage Level").await?;
        assert_eq!(triage_level, "Urgent");
        // And the patient's queue position remains unchanged
        let queue_position = get_text(driver, "Queue Position").await?;
        assert!(queue_position.len() > 0);
        // And no escalation alerts are generated
        let escalation_alerts = driver.find_all(locator("Escalation Alert")).await?;
        assert_eq!(escalation_alerts.len(), 0);
        // And a reassessment note is added documenting stable condition
        let reassessment_note = get_text(driver, "Reassessment Note").await?;
        assert_eq!(reassessment_note, "Patient reports feeling slightly better");
        // And the next reassessment is scheduled for 1 hour
        let next_reassessment_schedule = get_text(driver, "Next Reassessment Schedule").await?;
        assert_eq!(next_reassessment_schedule, "1 hour");
        Ok(())
    })
}

fn scenario_03_reassess_patient_with_improving_condition(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a patient "Lisa Rodriguez" has been waiting in the queue for 2 hours
        // And the patient's initial triage was ESI Level 2 (High Priority)
        // And the patient's initial vital signs were:
        //   | Vital Sign          | Initial Value |
        //   | Blood Pressure      | 170/100       |
        //   | Heart Rate          | 120           |
        //   | Respiratory Rate    | 28            |
        //   | Temperature         | 98.9°F        |
        //   | Oxygen Saturation   | 92%           |
        //   | Pain Scale          | 9/10          |
        // (assumed pre-seeded test data)
        // When I perform a reassessment
        driver.find(By::Css("[data-testid=\"select-patient-for-reassessment-button\"]")).await?.click().await?;

        // And I enter the updated vital signs:
        fill_fields(driver, &vec![
            row([("Field", "Blood Pressure"), ("Value", "145/85")]),
            row([("Field", "Heart Rate"), ("Value", "95")]),
            row([("Field", "Respiratory Rate"), ("Value", "20")]),
            row([("Field", "Temperature"), ("Value", "98.6°F")]),
            row([("Field", "Oxygen Saturation"), ("Value", "96%")]),
            row([("Field", "Pain Scale"), ("Value", "4/10")]),
        ]).await?;
        // And I note "Patient reports significant improvement after medication"
        fill_field(driver, "Reassessment Note", "Patient reports significant improvement after medication").await?;
        // And I submit the reassessment
        driver.find(By::Css("[data-testid=\"submit-reassessment-form\"]")).await?.click().await?;

        // Then the system recalculates the ESI score from "2" to "3"
        let esi_score = get_text(driver, "ESI Score").await?;
        assert_eq!(esi_score, "3");
        // And the system updates the triage level from "High Priority" to "Urgent"
        let triage_level = get_text(driver, "Triage Level").await?;
        assert_eq!(triage_level, "Urgent");
        // And the patient is moved from position 1 to position 5 in the queue
        let queue_position = get_text(driver, "Queue Position").await?;
        assert_eq!(queue_position, "5");
        // And the charge nurse is notified of the priority change
        let charge_nurse_notification = get_text(driver, "Charge Nurse Notification").await?;
        assert_match(&charge_nurse_notification, r"priority change", true);
        // And the estimated wait time is updated from "Immediate" to "45 minutes"
        let estimated_wait_time = get_text(driver, "Estimated Wait Time").await?;
        assert_eq!(estimated_wait_time, "45 minutes");
        // And higher priority patients are moved up in the queue
        let queue_entries = driver.find_all(By::Css("[data-testid=\"triage-queue-entry\"]")).await?;
        assert!(queue_entries.len() > 0);
        Ok(())
    })
}

fn scenario_04_automatic_reassessment_alert_triggers(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given multiple patients have been waiting for extended periods:
        //   | Patient Name     | Wait Time | Current ESI | Due for Reassessment |
        //   | John Williams    | 2 hours   | 4           | Yes                  |
        //   | Emma Thompson    | 1.5 hours | 3           | No                   |
        //   | David Kim        | 3 hours   | 3           | Yes                  |
        // (assumed pre-seeded test data)
        // When the system performs its hourly reassessment check
        driver.find(By::Css("[data-testid=\"trigger-hourly-reassessment-check-button\"]")).await?.click().await?;

        // Then reassessment alerts are generated for:
        //   | Patient Name  | Alert Type           | Reason                    |
        //   | John Williams | Standard Reassess    | 2 hours ESI Level 4       |
        //   | David Kim     | Urgent Reassess      | 3 hours ESI Level 3       |
        let reassessment_alerts = driver.find_all(By::Css("[data-testid=\"reassessment-alert\"]")).await?;
        assert_eq!(reassessment_alerts.len(), 2);
        // And the alerts appear on the triage nurse dashboard
        let triage_nurse_dashboard = wait_for_test_id(driver, "Triage Nurse Dashboard").await?;
        assert!(triage_nurse_dashboard.is_displayed().await?);
        // And the patients are flagged with "Reassessment Due" status
        let reassessment_due_flags = driver.find_all(By::Css("[data-testid=\"reassessment-due-flag\"]")).await?;
        assert_eq!(reassessment_due_flags.len(), 2);
        // And Emma Thompson does not receive an alert
        let alert_texts = texts_of(&reassessment_alerts).await?;
        assert!(!alert_texts.iter().any(|text| text.contains("Emma Thompson")));
        Ok(())
    })
}

fn scenario_05_handle_patient_who_becomes_critical_during_reassessment(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a patient "Robert Martinez" has been waiting in the queue for 2 hours
        // And the patient's initial triage was ESI Level 3 (Urgent)
        // When I begin the reassessment process
        driver.find(By::Css("[data-testid=\"select-patient-for-reassessment-button\"]")).await?.click().await?;
        // And I observe the patient is now unresponsive

        // And I enter critical vital signs:
        fill_fields(driver, &vec![
            row([("Field", "Blood Pressure"), ("Value", "60/30")]),
            row([("Field", "Heart Rate"), ("Value", "150")]),
            row([("Field", "Respiratory Rate"), ("Value", "6")]),
            row([("Field", "Temperature"), ("Value", "96.2°F")]),
            row([("Field", "Oxygen Saturation"), ("Value", "80%")]),
            row([("Field", "Consciousness"), ("Value", "Unresponsive")]),
        ]).await?;
        // And I submit the emergency reassessment
        driver.find(By::Css("[data-testid=\"submit-reassessment-form\"]")).await?.click().await?;

        // Then the system immediately calculates ESI score as "1"
        let esi_score = get_text(driver, "ESI Score").await?;
        assert_eq!(esi_score, "1");
        // And the system updates triage level to "Resuscitation"
        let triage_level = get_text(driver, "Triage Level").await?;
        assert_eq!(triage_level, "Resuscitation");
        // And the patient is moved to the top of all queues
        let queue_position = get_text(driver, "Queue Position").await?;
        assert_eq!(queue_position, "1");
        // And a code blue alert is automatically triggered
        let code_blue_alert = get_text(driver, "Code Blue Alert").await?;
        assert_match(&code_blue_alert, r"triggered", true);
        // And the rapid response team is notified immediately
        let rapid_response_notification = get_text(driver, "Rapid Response Notification").await?;
        assert_match(&rapid_response_notification, r"notified", true);
        // And the patient is flagged for immediate intervention
        let intervention_flag = get_text(driver, "Intervention Flag").await?;
        assert_match(&intervention_flag, r"immediate intervention", true);
        // And I am prompted to initiate emergency protocols
        wait_for_test_id(driver, "Emergency Protocol Prompt").await?;
        Ok(())
    })
}

fn scenario_06_reassess_pediatric_patient_with_different_parameters(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a pediatric patient "Amy Foster" (age 8) has been waiting for 2 hours
        // And the patient's initial triage was ESI Level 3 (Urgent)
        // When I perform a pediatric reassessment
        driver.find(By::Css("[data-testid=\"select-patient-for-reassessment-button\"]")).await?.click().await?;

        // And I enter updated vital signs using age-appropriate parameters:
        fill_fields(driver, &vec![
            row([("Field", "Blood Pressure"), ("Value", "85/50")]),
            row([("Field", "Heart Rate"), ("Value", "140")]),
            row([("Field", "Respiratory Rate"), ("Value", "32")]),
            row([("Field", "Temperature"), ("Value", "103.8°F")]),
            row([("Field", "Oxygen Saturation"), ("Value", "93%")]),
            row([("Field", "Pain Scale (FACES)"), ("Value", "8/10")]),
        ]).await?;
        // And I note "Child appears more lethargic than initial assessment"
        fill_field(driver, "Reassessment Note", "Child appears more lethargic than initial assessment").await?;
        // And I submit the pediatric reassessment
        driver.find(By::Css("[data-testid=\"submit-reassessment-form\"]")).await?.click().await?;

        // Then the system recalculates using pediatric ESI criteria
        let scoring_criteria = get_text(driver, "Scoring Criteria").await?;
        assert_match(&scoring_criteria, r"pediatric", true);
        // And the ESI score is updated from "3" to "2"
        let esi_score = get_text(driver, "ESI Score").await?;
        assert_eq!(esi_score, "2");
        // And the triage level is updated to "High Priority"
        let triage_level = get_text(driver, "Triage Level").await?;
        assert_eq!(triage_level, "High Priority");
        // And the pediatric emergency team is notified
        let pediatric_team_notification = get_text(driver, "Pediatric Team Notification").await?;
        assert_match(&pediatric_team_notification, r"notified", true);
        // And the patient is moved to the pediatric high priority queue
        let assigned_queue = get_text(driver, "Assigned Queue").await?;
        assert_match(&assigned_queue, r"pediatric high priority", true);
        // And parent/guardian notification protocols are initiated
        let guardian_notification = get_text(driver, "Guardian Notification").await?;
        assert_match(&guardian_notification, r"initiated", true);
        Ok(())
    })
}

fn scenario_07_document_reassessment_with_no_vital_sign_changes(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a patient "Catherine Lee" has been waiting for 2 hours
        // And a reassessment is due
        // When I perform the reassessment
        driver.find(By::Css("[data-testid=\"select-patient-for-reassessment-button\"]")).await?.click().await?;
        // And the vital signs remain identical to the initial assessment

        // But I note "Patient reports increased anxiety about wait time"
        fill_field(driver, "Reassessment Note", "Patient reports increased anxiety about wait time").await?;
        // And I provide reassurance and update on expected wait time
        // And I submit the reassessment
        driver.find(By::Css("[data-testid=\"submit-reassessment-form\"]")).await?.click().await?;

        // Then the ESI score and triage level remain unchanged
        let esi_score = get_text(driver, "ESI Score").await?;
        assert!(esi_score.len() > 0);
        let triage_level = get_text(driver, "Triage Level").await?;
        assert!(triage_level.len() > 0);
        // And the queue position is maintained
        let queue_position = get_text(driver, "Queue Position").await?;
        assert!(queue_position.len() > 0);
        // And a documentation note is added about patient anxiety
        let reassessment_note = get_text(driver, "Reassessment Note").await?;
        assert_match(&reassessment_note, r"anxiety", true);
        // And comfort measures are suggested in the patient instructions
        let patient_instructions = get_text(driver, "Patient Instructions").await?;
        assert_match(&patient_instructions, r"comfort", true);
        // And the next reassessment interval is maintained
        let next_reassessment_schedule = get_text(driver, "Next Reassessment Schedule").await?;
        assert!(next_reassessment_schedule.len() > 0);
        Ok(())
    })
}

fn scenario_08_handle_reassessment_during_shift_change(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a patient "Thomas Wilson" is due for reassessment
        // And the day shift triage nurse is preparing to leave
        // And the night shift triage nurse is arriving
        // When the day shift nurse initiates the reassessment handoff
        driver.find(By::Css("[data-testid=\"initiate-reassessment-handoff-button\"]")).await?.click().await?;
        // And transfers the patient assessment to the night shift nurse
        driver.find(By::Css("[data-testid=\"transfer-assessment-button\"]")).await?.click().await?;

        // Then the reassessment timing is preserved
        let reassessment_timing = get_text(driver, "Reassessment Timing").await?;
        assert!(reassessment_timing.len() > 0);
        // And all previous assessment data remains accessible
        let previous_assessment_data = wait_for_test_id(driver, "Previous Assessment Data").await?;
        assert!(previous_assessment_data.is_displayed().await?);
        // And the night shift nurse can complete the reassessment
        let reassessment_form = driver.find(By::Css("[data-testid=\"reassessment-form\"]")).await?;
        assert!(reassessment_form.is_displayed().await?);
        // And continuity of care documentation is maintained
        let continuity_documentation = get_text(driver, "Continuity Documentation").await?;
        assert!(continuity_documentation.len() > 0);
        // And the handoff is logged in the system audit trail
        let audit_trail_entries = driver.find_all(By::Css("[data-testid=\"audit-trail-entry\"]")).await?;
        assert!(audit_trail_entries.len() > 0);
        Ok(())
    })
}

fn main() {
    run_feature(&[
        ("Reassess patient with worsening condition after 2 hours", scenario_01_reassess_patient_with_worsening_condition_after_2_hours),
        ("Reassess patient with stable condition", scenario_02_reassess_patient_with_stable_condition),
        ("Reassess patient with improving condition", scenario_03_reassess_patient_with_improving_condition),
        ("Automatic reassessment alert triggers", scenario_04_automatic_reassessment_alert_triggers),
        ("Handle patient who becomes critical during reassessment", scenario_05_handle_patient_who_becomes_critical_during_reassessment),
        ("Reassess pediatric patient with different parameters", scenario_06_reassess_pediatric_patient_with_different_parameters),
        ("Document reassessment with no vital sign changes", scenario_07_document_reassessment_with_no_vital_sign_changes),
        ("Handle reassessment during shift change", scenario_08_handle_reassessment_during_shift_change),
    ]);
}
