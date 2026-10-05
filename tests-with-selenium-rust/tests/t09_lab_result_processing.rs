// Selenium WebDriver + libtest-mimic test for
// tests-with-given-when-then-features/09-lab-result-processing.feature
// (equivalent to tests-with-selenium-javascript/09-lab-result-processing.test.js).
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
    //   And the HL7 interface with the laboratory system is active
    //   And critical value alert system is enabled
    //   And physician notification system is functional
    verify_system_is_operational(driver).await?;
    // The HL7 interface, critical value alert system, and physician
    // notification system being active/enabled are assumed pre-seeded
    // test data / environment configuration. This feature has no
    // "logged in as" step in its Background.
    login(driver, "a lab technician", false).await?;

    let lab_result_processing_nav_link = wait_for_test_id(driver, "Nav Lab Result Processing").await?;
    lab_result_processing_nav_link.click().await?;
    wait_for_test_id(driver, "Lab Result Processing Panel").await?;
    Ok(())
}

fn scenario_01_process_normal_lab_results_via_hl7_interface(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a patient "Jennifer Lopez" is in bed "ED-8"
        // And laboratory orders were placed for "CBC, Basic Metabolic Panel"
        // And the attending physician is "Dr. Smith"
        // (assumed pre-seeded test data)

        // When the lab system sends results via HL7 interface:
        //   | Test Name          | Result    | Reference Range | Units  | Status   | Timestamp |
        //   | White Blood Cells  | 7.2       | 4.0-10.0       | K/uL   | Final    | 14:30     |
        //   | Hemoglobin         | 13.5      | 12.0-16.0      | g/dL   | Final    | 14:30     |
        //   | Sodium             | 140       | 136-145        | mmol/L | Final    | 14:30     |
        //   | Potassium          | 4.1       | 3.5-5.0        | mmol/L | Final    | 14:30     |
        //   | Creatinine         | 1.0       | 0.6-1.2        | mg/dL  | Final    | 14:30     |
        driver.find(By::Css("[data-testid=\"receive-lab-results-button\"]")).await?.click().await?;

        // Then the system updates the patient record with all results
        let patient_record_update_status = get_text(driver, "Patient Record Update Status").await?;
        assert_match(&patient_record_update_status, r"updated", true);

        // And the results are marked as "Normal" in the patient chart
        let result_status = get_text(driver, "Result Status").await?;
        assert_eq!(result_status, "Normal");

        // And a standard notification is sent to "Dr. Smith":
        assert_eq!(get_text(driver, "Lab Results").await?, "Normal CBC and BMP available for review");
        assert_eq!(get_text(driver, "Patient").await?, "Jennifer Lopez, Bed ED-8");
        assert_eq!(get_text(driver, "Timestamp").await?, "14:30");
        assert_eq!(get_text(driver, "Priority").await?, "Standard");

        // And the results appear in the patient's timeline with normal value indicators
        let timeline_entries = driver.find_all(By::Css("[data-testid=\"timeline-result-entry\"]")).await?;
        assert!(timeline_entries.len() > 0);

        // And no critical value alerts are generated
        let critical_value_alerts = driver.find_all(By::Css("[data-testid=\"critical-value-alert\"]")).await?;
        assert_eq!(critical_value_alerts.len(), 0);

        // And the nursing staff is notified that results are available for review
        let nursing_results_notification = get_text(driver, "Nursing Results Notification").await?;
        assert!(nursing_results_notification.len() > 0);
        Ok(())
    })
}

fn scenario_02_process_critical_lab_results_with_immediate_alerts(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a patient "Michael Davis" is in bed "ED-12"
        // And laboratory orders were placed for "Troponin, BNP, D-Dimer"
        // And the attending physician is "Dr. Johnson"
        // (assumed pre-seeded test data)

        // When the lab system sends critical results via HL7 interface:
        //   | Test Name    | Result | Reference Range | Units  | Status | Critical | Timestamp |
        //   | Troponin I   | 8.5    | 0.0-0.04       | ng/mL  | Final  | Yes      | 15:45     |
        //   | BNP          | 1200   | 0-100          | pg/mL  | Final  | Yes      | 15:45     |
        //   | D-Dimer      | 0.8    | 0.0-0.5        | mg/L   | Final  | No       | 15:45     |
        driver.find(By::Css("[data-testid=\"receive-lab-results-button\"]")).await?.click().await?;

        // Then the system immediately flags critical values:
        //   | Test Name    | Critical Flag | Severity Level |
        //   | Troponin I   | CRITICAL HIGH | Severe         |
        //   | BNP          | CRITICAL HIGH | High           |
        assert_eq!(get_text(driver, "Troponin I Critical Flag").await?, "CRITICAL HIGH");
        assert_eq!(get_text(driver, "Troponin I Severity Level").await?, "Severe");
        assert_eq!(get_text(driver, "BNP Critical Flag").await?, "CRITICAL HIGH");
        assert_eq!(get_text(driver, "BNP Severity Level").await?, "High");

        // And popup notifications are displayed for all logged-in providers:
        assert_eq!(get_text(driver, "Critical Alert").await?, "🔴 CRITICAL: Troponin I = 8.5 ng/mL");
        assert_eq!(get_text(driver, "High Alert").await?, "🟠 HIGH: BNP = 1200 pg/mL");
        assert_eq!(get_text(driver, "Patient Info").await?, "Michael Davis, Bed ED-12");

        // And an immediate notification is sent to "Dr. Johnson":
        assert_eq!(get_text(driver, "Mobile Push").await?, "CRITICAL LAB: Troponin 8.5 - Michael Davis");
        assert_eq!(get_text(driver, "SMS Alert").await?, "ED-12 CRITICAL Troponin I: 8.5 ng/mL");
        assert_eq!(get_text(driver, "In-App Alert").await?, "High priority popup requiring acknowledgment");

        // And the charge nurse receives a critical value notification
        let charge_nurse_notification = get_text(driver, "Charge Nurse Notification").await?;
        assert!(charge_nurse_notification.len() > 0);

        // And the results are highlighted in red on all patient displays
        let critical_result_highlights = driver.find_all(By::Css("[data-testid=\"critical-result-highlight\"]")).await?;
        assert!(critical_result_highlights.len() > 0);

        // And an audit trail is created for the critical value communication
        let audit_trail = driver.find(By::Css("[data-testid=\"critical-value-audit-trail\"]")).await?;
        assert!(audit_trail.is_displayed().await?);
        Ok(())
    })
}

fn scenario_03_handle_lab_results_with_different_statuses_and_corrections(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a patient "Sarah Wilson" is in bed "ED-6"
        // And previous lab results were reported
        // (assumed pre-seeded test data)

        // When the lab system sends updated results via HL7 interface:
        //   | Test Name     | Result | Status     | Previous Result | Correction Reason    | Timestamp |
        //   | Hemoglobin    | 9.2    | Corrected  | 11.2           | Sample hemolysis     | 16:15     |
        //   | Glucose       | 250    | Final      | -              | -                    | 16:15     |
        //   | Pending Test  | -      | Pending    | -              | Sample reprocessing  | 16:15     |
        driver.find(By::Css("[data-testid=\"receive-lab-results-button\"]")).await?.click().await?;

        // Then the system processes different result statuses:
        assert_eq!(get_text(driver, "Corrected").await?, "Replace previous value, maintain history");
        assert_eq!(get_text(driver, "Final").await?, "Add new result to patient record");
        assert_eq!(get_text(driver, "Pending").await?, "Update status, maintain order tracking");

        // And correction notifications are sent:
        assert_eq!(get_text(driver, "Correction Alert").await?, "Lab value corrected: Hgb 11.2 → 9.2 g/dL");
        assert_eq!(get_text(driver, "Reason").await?, "Sample hemolysis detected");
        assert_eq!(get_text(driver, "Clinical Impact").await?, "Anemia now more severe than initially reported");

        // And the attending physician "Dr. Martinez" is notified of the correction
        let physician_correction_notification = get_text(driver, "Physician Correction Notification").await?;
        assert!(physician_correction_notification.len() > 0);

        // And the original result is preserved in the audit trail
        let original_result_audit_entry = driver.find(By::Css("[data-testid=\"original-result-audit-entry\"]")).await?;
        assert!(original_result_audit_entry.is_displayed().await?);

        // And the corrected value triggers anemia protocol alerts
        wait_for_test_id(driver, "Anemia Protocol Alert").await?;
        Ok(())
    })
}

fn scenario_04_process_pediatric_lab_results_with_age_specific_reference_ra(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a pediatric patient "Emma Foster" (age 6) is in bed "ED-PEDS-2"
        // And laboratory orders were placed for "CBC, CMP"
        // (assumed pre-seeded test data)

        // When the lab system sends pediatric results via HL7 interface:
        //   | Test Name          | Result | Adult Range    | Pediatric Range (Age 6) | Units  | Status |
        //   | White Blood Cells  | 12.5   | 4.0-10.0      | 5.0-14.5               | K/uL   | Final  |
        //   | Hemoglobin         | 11.8   | 12.0-16.0     | 11.5-13.5              | g/dL   | Final  |
        //   | Alkaline Phosphatase| 250   | 44-147        | 156-369                | U/L    | Final  |
        driver.find(By::Css("[data-testid=\"receive-lab-results-button\"]")).await?.click().await?;

        // Then the system applies age-appropriate reference ranges:
        //   | Test Name          | Interpretation      | Flag        |
        //   | White Blood Cells  | Normal for age 6    | Normal      |
        //   | Hemoglobin         | Normal for age 6    | Normal      |
        //   | Alkaline Phosphatase| Normal for age 6   | Normal      |
        assert_eq!(get_text(driver, "White Blood Cells Interpretation").await?, "Normal for age 6");
        assert_eq!(get_text(driver, "White Blood Cells Flag").await?, "Normal");
        assert_eq!(get_text(driver, "Hemoglobin Interpretation").await?, "Normal for age 6");
        assert_eq!(get_text(driver, "Hemoglobin Flag").await?, "Normal");
        assert_eq!(get_text(driver, "Alkaline Phosphatase Interpretation").await?, "Normal for age 6");
        assert_eq!(get_text(driver, "Alkaline Phosphatase Flag").await?, "Normal");

        // And the pediatric attending "Dr. Chen" is notified with age-specific context
        let pediatric_attending_notification = get_text(driver, "Pediatric Attending Notification").await?;
        assert!(pediatric_attending_notification.len() > 0);

        // And the results display shows both adult and pediatric reference ranges
        wait_for_test_id(driver, "Adult Reference Range").await?;
        wait_for_test_id(driver, "Pediatric Reference Range").await?;

        // And no inappropriate critical alerts are generated for age-normal values
        let critical_value_alerts = driver.find_all(By::Css("[data-testid=\"critical-value-alert\"]")).await?;
        assert_eq!(critical_value_alerts.len(), 0);
        Ok(())
    })
}

fn scenario_05_handle_lab_results_during_physician_handoff(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a patient "Robert Kim" is in bed "ED-15"
        // And the day shift physician "Dr. Adams" ordered labs at 18:00
        // And the evening shift physician "Dr. Brown" has taken over at 19:00
        // (assumed pre-seeded test data)

        // When the lab system sends results via HL7 interface at 19:30:
        //   | Test Name     | Result | Reference Range | Status | Critical |
        //   | Lipase        | 350    | 10-140         | Final  | Yes      |
        //   | Amylase       | 180    | 25-125         | Final  | No       |
        driver.find(By::Css("[data-testid=\"receive-lab-results-button\"]")).await?.click().await?;

        // Then the system determines the appropriate physician to notify:
        //   | Notification Target | Rationale                                  |
        //   | Primary: Dr. Brown  | Current attending physician                |
        //   | Secondary: Dr. Adams| Ordered the tests, may need notification   |
        assert_eq!(get_text(driver, "Primary Notification Target").await?, "Dr. Brown");
        assert_eq!(get_text(driver, "Primary Notification Rationale").await?, "Current attending physician");
        assert_eq!(get_text(driver, "Secondary Notification Target").await?, "Dr. Adams");
        assert_eq!(get_text(driver, "Secondary Notification Rationale").await?, "Ordered the tests, may need notification");

        // And both physicians receive notifications with handoff context:
        assert_eq!(get_text(driver, "Dr. Brown Notification").await?, "CRITICAL: Lipase 350 - Patient from Dr. Adams");
        assert_eq!(get_text(driver, "Dr. Adams Notification").await?, "FYI: Your lipase order critical - Now Dr. Brown");

        // And the handoff log is updated with the critical result information
        let handoff_log = driver.find(By::Css("[data-testid=\"handoff-log\"]")).await?;
        assert!(handoff_log.is_displayed().await?);

        // And the charge nurse is notified of the critical value during shift change
        let charge_nurse_shift_change_notification = get_text(driver, "Charge Nurse Shift Change Notification").await?;
        assert!(charge_nurse_shift_change_notification.len() > 0);
        Ok(())
    })
}

fn scenario_06_process_lab_results_with_technical_failures_and_retries(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a patient "Lisa Garcia" is in bed "ED-3"
        // And laboratory results are ready for transmission
        // (assumed pre-seeded test data)

        // When the lab system attempts to send results via HL7 interface
        // And the initial transmission fails due to network connectivity
        // And the lab system retries transmission after 5 minutes
        // (no direct UI action for these narrative steps)

        // And the retry is successful with results:
        //   | Test Name   | Result | Reference Range | Status | Timestamp |
        //   | Troponin    | 0.02   | 0.0-0.04       | Final  | 20:15     |
        driver.find(By::Css("[data-testid=\"receive-lab-results-button\"]")).await?.click().await?;

        // Then the system processes the delayed results
        let delayed_result_processing_status = get_text(driver, "Delayed Result Processing Status").await?;
        assert_match(&delayed_result_processing_status, r"processed", true);

        // And a delay notification is included:
        assert_eq!(get_text(driver, "Delay Notice").await?, "Results delayed due to technical issues");
        assert_eq!(get_text(driver, "Original Time").await?, "Results ready at 20:10");
        assert_eq!(get_text(driver, "Received Time").await?, "Results received at 20:15");

        // And the attending physician is notified of both the results and the delay
        let physician_delay_notification = get_text(driver, "Physician Delay Notification").await?;
        assert!(physician_delay_notification.len() > 0);

        // And system administrators are alerted to the interface failure
        let system_administrator_alert = get_text(driver, "System Administrator Alert").await?;
        assert!(system_administrator_alert.len() > 0);

        // And the delay is documented in the interface audit log
        let interface_audit_log = driver.find(By::Css("[data-testid=\"interface-audit-log\"]")).await?;
        assert!(interface_audit_log.is_displayed().await?);
        Ok(())
    })
}

fn scenario_07_handle_batch_lab_results_processing(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given multiple patients have pending lab results:
        //   | Patient Name    | Bed    | Attending     | Tests Ordered        |
        //   | Alice Johnson   | ED-4   | Dr. Smith     | CBC, BMP             |
        //   | Bob Thompson    | ED-7   | Dr. Smith     | Liver function tests |
        //   | Carol Martinez  | ED-11  | Dr. Brown     | Cardiac enzymes      |
        // (assumed pre-seeded test data)

        // When the lab system sends batch results via HL7 interface:
        //   | Patient       | Test Results                              | Critical Values |
        //   | Alice Johnson | All normal values                         | None           |
        //   | Bob Thompson  | ALT: 150 (High), AST: 120 (High)        | None           |
        //   | Carol Martinez| Troponin: 2.1 (Critical)                | Troponin       |
        driver.find(By::Css("[data-testid=\"receive-lab-results-button\"]")).await?.click().await?;

        // Then the system processes all results simultaneously
        let batch_processing_status = get_text(driver, "Batch Processing Status").await?;
        assert_match(&batch_processing_status, r"simultaneously", true);

        // And notifications are prioritized by criticality:
        //   | Priority | Patient        | Notification Type    |
        //   | 1        | Carol Martinez | Critical value alert |
        //   | 2        | Bob Thompson   | Abnormal value alert |
        //   | 3        | Alice Johnson  | Normal results       |
        assert_eq!(get_text(driver, "Priority 1 Patient").await?, "Carol Martinez");
        assert_eq!(get_text(driver, "Priority 1 Notification Type").await?, "Critical value alert");
        assert_eq!(get_text(driver, "Priority 2 Patient").await?, "Bob Thompson");
        assert_eq!(get_text(driver, "Priority 2 Notification Type").await?, "Abnormal value alert");
        assert_eq!(get_text(driver, "Priority 3 Patient").await?, "Alice Johnson");
        assert_eq!(get_text(driver, "Priority 3 Notification Type").await?, "Normal results");

        // And physicians receive consolidated notifications when appropriate
        let consolidated_notification = get_text(driver, "Consolidated Notification").await?;
        assert!(consolidated_notification.len() > 0);

        // And system performance metrics are maintained during batch processing
        wait_for_test_id(driver, "System Performance Metrics").await?;
        Ok(())
    })
}

fn scenario_08_process_lab_results_with_interpretation_comments(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a patient "David Lee" is in bed "ED-9"
        // And complex laboratory tests were ordered
        // (assumed pre-seeded test data)

        // When the lab system sends results with pathologist interpretation:
        //   | Test Name        | Result | Reference | Interpretation                    | Timestamp |
        //   | Blood Smear      | -      | -         | Moderate anisocytosis noted      | 21:00     |
        //   | Hemoglobin A1C   | 9.2%   | <5.7%     | Consistent with poor DM control  | 21:00     |
        //   | Thyroid Function | -      | -         | Pattern suggests hyperthyroidism | 21:00     |
        driver.find(By::Css("[data-testid=\"receive-lab-results-button\"]")).await?.click().await?;

        // Then the system includes interpretation comments in the patient record
        let interpretation_comments = driver.find_all(By::Css("[data-testid=\"interpretation-comment\"]")).await?;
        assert_eq!(interpretation_comments.len(), 3);

        // And the attending physician receives enhanced notifications:
        assert_eq!(get_text(driver, "Raw Results").await?, "Numeric values and reference ranges");
        assert_eq!(get_text(driver, "Interpretation").await?, "Pathologist comments and clinical significance");
        assert_eq!(get_text(driver, "Recommendations").await?, "Suggested follow-up or additional testing");

        // And interpretation comments are highlighted in the patient chart
        let highlighted_interpretation_comments = driver.find_all(By::Css("[data-testid=\"highlighted-interpretation-comment\"]")).await?;
        assert!(highlighted_interpretation_comments.len() > 0);

        // And complex results are flagged for physician review and acknowledgment
        wait_for_test_id(driver, "Physician Review Flag").await?;
        Ok(())
    })
}

fn main() {
    run_feature(&[
        ("Process normal lab results via HL7 interface", scenario_01_process_normal_lab_results_via_hl7_interface),
        ("Process critical lab results with immediate alerts", scenario_02_process_critical_lab_results_with_immediate_alerts),
        ("Handle lab results with different statuses and corrections", scenario_03_handle_lab_results_with_different_statuses_and_corrections),
        ("Process pediatric lab results with age-specific reference ranges", scenario_04_process_pediatric_lab_results_with_age_specific_reference_ra),
        ("Handle lab results during physician handoff", scenario_05_handle_lab_results_during_physician_handoff),
        ("Process lab results with technical failures and retries", scenario_06_process_lab_results_with_technical_failures_and_retries),
        ("Handle batch lab results processing", scenario_07_handle_batch_lab_results_processing),
        ("Process lab results with interpretation comments", scenario_08_process_lab_results_with_interpretation_comments),
    ]);
}
