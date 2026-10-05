// Playwright + libtest-mimic test for
// tests-with-given-when-then-features/10-critical-lab-alert.feature
// (equivalent to tests-with-playwright-javascript/10-critical-lab-alert.test.js).
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
    //   And the critical value alert system is enabled
    //   And laboratory interfaces are functioning
    //   And all patient displays are connected to the alert system
    verify_system_is_operational(page).await?;
    // The critical value alert system, laboratory interfaces, and patient
    // display connections are assumed pre-seeded test data / environment
    // configuration. This feature has no "logged in as" step in its
    // Background.
    login(page, "a lab technician", false).await?;

    let critical_lab_alert_nav_link = wait_for_test_id(page, "Nav Critical Lab Alert").await?;
    critical_lab_alert_nav_link.click(None).await?;
    wait_for_test_id(page, "Critical Lab Alert Panel").await?;
    Ok(())
}

fn scenario_01_process_critically_high_troponin_result_with_immediate_alert(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given a patient "Robert Martinez" is in bed "ED-7"
        // And the attending physician is "Dr. Johnson"
        // And the charge nurse is "Nurse Williams"
        // And troponin was ordered for "chest pain evaluation"
        // (assumed pre-seeded test data)

        // When the laboratory result is received:
        //   | Test Name       | Result | Reference Range | Units  | Critical Threshold | Status |
        //   | Troponin I      | 5.8    | 0.0-0.04       | ng/mL  | >0.4              | Final  |
        fill_fields(page, &vec![
            row([("Field", "Test Name"), ("Value", "Troponin I")]),
            row([("Field", "Result"), ("Value", "5.8")]),
            row([("Field", "Reference Range"), ("Value", "0.0-0.04")]),
            row([("Field", "Units"), ("Value", "ng/mL")]),
            row([("Field", "Critical Threshold"), ("Value", ">0.4")]),
            row([("Field", "Status"), ("Value", "Final")]),
        ]).await?;
        page.get_by_test_id("receive-lab-result-button").first().click(None).await?;

        // Then the system immediately triggers critical alerts
        let critical_alert_status = get_text(page, "Critical Alert Status").await?;
        assert_match(&critical_alert_status, r"triggered", true);

        // And the attending physician "Dr. Johnson" receives immediate notifications:
        //   | Notification Type | Content                                        | Delivery Method |
        //   | Mobile Push Alert | 🔴 CRITICAL: Troponin I 5.8 ng/mL - ED-7     | Mobile App      |
        //   | SMS Alert        | CRITICAL LAB: R.Martinez ED-7 Troponin 5.8    | Text Message    |
        //   | Popup Alert      | CRITICAL VALUE - Requires Acknowledgment       | Workstation     |
        assert_eq!(get_text(page, "Mobile Push Alert").await?, "🔴 CRITICAL: Troponin I 5.8 ng/mL - ED-7");
        assert_eq!(get_text(page, "SMS Alert").await?, "CRITICAL LAB: R.Martinez ED-7 Troponin 5.8");
        assert_eq!(get_text(page, "Popup Alert").await?, "CRITICAL VALUE - Requires Acknowledgment");

        // And the charge nurse "Nurse Williams" receives critical notifications:
        //   | Notification Type | Content                                        | Delivery Method |
        //   | Desktop Alert    | CRITICAL: Troponin 5.8 - Bed ED-7            | Workstation     |
        //   | Overhead Page    | Critical lab value bed ED-7                   | PA System       |
        //   | Mobile Alert     | Critical troponin result requires attention   | Mobile Device   |
        assert_eq!(get_text(page, "Desktop Alert").await?, "CRITICAL: Troponin 5.8 - Bed ED-7");
        assert_eq!(get_text(page, "Overhead Page").await?, "Critical lab value bed ED-7");
        assert_eq!(get_text(page, "Mobile Alert").await?, "Critical troponin result requires attention");

        // And red flag indicators appear on all patient displays:
        //   | Display Location     | Alert Indicator                              |
        //   | Patient Monitor      | 🔴 CRITICAL LAB flashing red banner         |
        //   | Bedside Workstation  | Red alert icon next to patient name         |
        //   | Main ED Dashboard    | Red flag on bed ED-7 status                 |
        //   | Mobile Devices       | Red notification badge on patient chart     |
        //   | Nursing Station      | Critical value alert on patient board       |
        let red_flag_indicators = page.get_by_test_id("red-flag-indicator");
        assert_eq!(red_flag_indicators.count().await?, 5);
        Ok(())
    })
}

fn scenario_02_handle_critical_troponin_with_physician_acknowledgment_requi(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given a patient "Maria Santos" is in bed "ED-12"
        // And the attending physician is "Dr. Lee"
        // And a critically high troponin result of "7.2 ng/mL" is received
        // (assumed pre-seeded test data)

        // When the critical alert is triggered
        page.get_by_test_id("trigger-critical-alert-button").first().click(None).await?;

        // Then the system requires physician acknowledgment:
        assert_eq!(get_text(page, "Initial Alert").await?, "Must acknowledge receipt within 15 minutes");
        assert_eq!(get_text(page, "Clinical Review").await?, "Must document result review");
        assert_eq!(get_text(page, "Action Plan").await?, "Must indicate next steps taken");

        // And if "Dr. Lee" does not acknowledge within 15 minutes:
        assert_eq!(get_text(page, "Secondary Alert").await?, "Alert sent to backup physician");
        assert_eq!(get_text(page, "Charge Nurse Alert").await?, "Escalation notice to charge nurse");
        assert_eq!(get_text(page, "Supervisor Alert").await?, "Department supervisor notified");

        // And the acknowledgment status is tracked:
        //   | Status              | Timestamp | Provider    | Action              |
        //   | Alert Sent          | 14:30:15  | System      | Initial notification|
        //   | Acknowledged        | 14:32:45  | Dr. Lee     | Acknowledged receipt|
        //   | Reviewed            | 14:35:20  | Dr. Lee     | Documented review   |
        //   | Action Taken        | 14:40:10  | Dr. Lee     | Treatment initiated |
        let acknowledgment_status_entries = page.get_by_test_id("acknowledgment-status-entry");
        assert_eq!(acknowledgment_status_entries.count().await?, 4);
        Ok(())
    })
}

fn scenario_03_process_multiple_critical_values_simultaneously(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given multiple patients have critical troponin results:
        //   | Patient Name    | Bed   | Troponin Result | Attending     | Severity  |
        //   | John Williams   | ED-3  | 3.2 ng/mL      | Dr. Adams     | High      |
        //   | Lisa Johnson    | ED-8  | 8.9 ng/mL      | Dr. Brown     | Critical  |
        //   | Mike Davis      | ED-15 | 4.1 ng/mL      | Dr. Adams     | High      |
        // (assumed pre-seeded test data)

        // When all critical results are received simultaneously
        page.get_by_test_id("receive-critical-results-button").first().click(None).await?;

        // Then the system prioritizes alerts by severity:
        //   | Priority | Patient      | Alert Level | Notification Urgency    |
        //   | 1        | Lisa Johnson | Critical    | Immediate - All channels|
        //   | 2        | Mike Davis   | High        | Urgent - Standard alerts|
        //   | 3        | John Williams| High        | Urgent - Standard alerts|
        assert_eq!(get_text(page, "Priority 1 Patient").await?, "Lisa Johnson");
        assert_eq!(get_text(page, "Priority 1 Alert Level").await?, "Critical");
        assert_eq!(get_text(page, "Priority 2 Patient").await?, "Mike Davis");
        assert_eq!(get_text(page, "Priority 2 Alert Level").await?, "High");
        assert_eq!(get_text(page, "Priority 3 Patient").await?, "John Williams");
        assert_eq!(get_text(page, "Priority 3 Alert Level").await?, "High");

        // And physicians receive prioritized notifications:
        assert_eq!(get_text(page, "Dr. Brown Alert Summary").await?, "CRITICAL: Lisa Johnson Trop 8.9 - IMMEDIATE");
        assert_eq!(get_text(page, "Dr. Adams Alert Summary").await?, "HIGH: 2 patients with elevated troponin");

        // And the charge nurse receives a summary alert:
        assert_eq!(get_text(page, "Mass Alert").await?, "3 critical troponin results requiring attention");
        assert_eq!(get_text(page, "Priority List").await?, "Lisa Johnson (Critical), others (High)");

        // And all patient displays show appropriately color-coded flags
        let color_coded_flags = page.get_by_test_id("color-coded-flag");
        assert_eq!(color_coded_flags.count().await?, 3);
        Ok(())
    })
}

fn scenario_04_handle_critical_troponin_during_shift_change(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given a patient "Catherine Brown" is in bed "ED-6"
        // And it is 19:00 during evening shift change
        // And the day shift physician "Dr. Wilson" ordered the troponin
        // And the evening shift physician "Dr. Taylor" has assumed care
        // (assumed pre-seeded test data)

        // When a critically high troponin result of "6.1 ng/mL" is received
        fill_field(page, "Troponin Result", "6.1 ng/mL").await?;
        page.get_by_test_id("receive-lab-result-button").first().click(None).await?;

        // Then both physicians receive critical alerts:
        //   | Physician  | Alert Type    | Content                                |
        //   | Dr. Taylor | Primary Alert | CRITICAL Troponin 6.1 - Your patient  |
        //   | Dr. Wilson | Handoff Alert | FYI: Critical result on your order     |
        assert_eq!(get_text(page, "Dr. Taylor Alert").await?, "CRITICAL Troponin 6.1 - Your patient");
        assert_eq!(get_text(page, "Dr. Wilson Alert").await?, "FYI: Critical result on your order");

        // And the charge nurse receives handoff-specific notification:
        assert_eq!(get_text(page, "Shift Context").await?, "Critical result during physician handoff");
        assert_eq!(get_text(page, "Current MD").await?, "Dr. Taylor (assuming care)");
        assert_eq!(get_text(page, "Ordering MD").await?, "Dr. Wilson (ordered test)");

        // And the handoff documentation is automatically updated
        let handoff_documentation = page.get_by_test_id("handoff-documentation").first();
        assert!(handoff_documentation.is_visible().await?);

        // And red flags appear with shift change context indicators
        let red_flag_indicators = page.get_by_test_id("red-flag-indicator");
        assert!(red_flag_indicators.count().await? > 0);
        Ok(())
    })
}

fn scenario_05_process_critical_troponin_with_additional_cardiac_markers(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given a patient "Steven Kim" is in bed "ED-11"
        // And multiple cardiac markers were ordered
        // (assumed pre-seeded test data)

        // When critical and related results are received:
        //   | Test Name    | Result | Reference Range | Critical | Clinical Significance |
        //   | Troponin I   | 4.7    | 0.0-0.04       | Yes      | Acute MI indicated    |
        //   | CK-MB        | 45     | 0-6.3          | Yes      | Myocardial damage     |
        //   | Myoglobin    | 280    | 25-72          | No       | Elevated but not critical|
        page.get_by_test_id("receive-lab-results-button").first().click(None).await?;

        // Then the system groups related critical values:
        //   | Alert Category  | Content                                      |
        //   | Cardiac Panel   | Multiple critical cardiac markers            |
        //   | Primary Alert   | Troponin I: 4.7 ng/mL (CRITICAL)           |
        //   | Secondary Alert | CK-MB: 45 ng/mL (CRITICAL)                 |
        //   | Supporting Data | Myoglobin: 280 ng/mL (Elevated)            |
        assert_eq!(get_text(page, "Cardiac Panel").await?, "Multiple critical cardiac markers");
        assert_eq!(get_text(page, "Primary Alert").await?, "Troponin I: 4.7 ng/mL (CRITICAL)");
        assert_eq!(get_text(page, "Secondary Alert").await?, "CK-MB: 45 ng/mL (CRITICAL)");
        assert_eq!(get_text(page, "Supporting Data").await?, "Myoglobin: 280 ng/mL (Elevated)");

        // And enhanced clinical context is provided:
        assert_eq!(get_text(page, "Clinical Indication").await?, "Acute myocardial infarction likely");
        assert_eq!(get_text(page, "Recommended Actions").await?, "Cardiology consult, STEMI protocol");
        assert_eq!(get_text(page, "Time Sensitivity").await?, "Treatment within 90 minutes critical");

        // And STEMI protocol alerts are automatically triggered
        wait_for_test_id(page, "STEMI Protocol Alert").await?;
        Ok(())
    })
}

fn scenario_06_handle_false_positive_critical_troponin_alerts(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given a patient "Nancy Rodriguez" is in bed "ED-4"
        // And a troponin result of "5.1 ng/mL" triggers a critical alert
        // (assumed pre-seeded test data)

        // When the laboratory calls to report a sample error
        page.get_by_test_id("report-sample-error-button").first().click(None).await?;

        // And a corrected result shows "0.03 ng/mL" (normal)
        fill_field(page, "Corrected Troponin Result", "0.03 ng/mL").await?;
        page.get_by_test_id("submit-correction-button").first().click(None).await?;

        // Then the system processes the correction:
        assert_eq!(get_text(page, "Cancel Alert").await?, "Original critical alert is cancelled");
        assert_eq!(get_text(page, "Send Correction").await?, "Corrected value sent to all recipients");
        assert_eq!(get_text(page, "Document Error").await?, "Lab error documented in audit trail");

        // And correction notifications are sent:
        assert_eq!(get_text(page, "Alert Cancellation").await?, "CANCELLED: Previous critical troponin alert");
        assert_eq!(get_text(page, "Corrected Value").await?, "Troponin corrected to 0.03 ng/mL (Normal)");
        assert_eq!(get_text(page, "Error Explanation").await?, "Laboratory sample contamination identified");

        // And red flags are removed from all patient displays
        let red_flag_indicators = page.get_by_test_id("red-flag-indicator");
        assert_eq!(red_flag_indicators.count().await?, 0);

        // And the correction is logged for quality assurance review
        let quality_assurance_log = page.get_by_test_id("quality-assurance-log").first();
        assert!(quality_assurance_log.is_visible().await?);
        Ok(())
    })
}

fn scenario_07_critical_troponin_with_patient_transfer_requirements(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given a patient "Timothy Chang" is in bed "ED-9"
        // And a critically high troponin of "9.3 ng/mL" is received
        // And the patient requires immediate transfer to cardiac unit
        // (assumed pre-seeded test data)

        // When the critical alert is processed
        page.get_by_test_id("process-critical-alert-button").first().click(None).await?;

        // Then transfer coordination alerts are included:
        assert_eq!(get_text(page, "Transfer Required").await?, "Patient needs immediate cardiac unit transfer");
        assert_eq!(get_text(page, "Bed Availability").await?, "CCU bed 302 available");
        assert_eq!(get_text(page, "Transport Time").await?, "Transport team ETA 10 minutes");

        // And receiving unit notifications are sent:
        assert_eq!(get_text(page, "CCU Alert").await?, "Incoming transfer - Critical troponin 9.3");
        assert_eq!(get_text(page, "Cardiology Alert").await?, "Urgent consult needed - STEMI protocol");

        // And transfer documentation is automatically initiated
        let transfer_documentation = page.get_by_test_id("transfer-documentation").first();
        assert!(transfer_documentation.is_visible().await?);

        // And critical alerts follow the patient to the receiving unit
        wait_for_test_id(page, "Patient Alert Handoff").await?;
        Ok(())
    })
}

fn scenario_08_validate_critical_troponin_alert_system_functionality(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given the critical alert system is being tested
        // (assumed pre-seeded test data)

        // When a test troponin result of "TEST-5.0 ng/mL" is processed
        fill_field(page, "Test Troponin Result", "TEST-5.0 ng/mL").await?;
        page.get_by_test_id("process-test-result-button").first().click(None).await?;

        // Then the system validates all alert pathways:
        assert_eq!(get_text(page, "Physician Mobile").await?, "Test alert delivered successfully");
        assert_eq!(get_text(page, "Charge Nurse").await?, "Test alert delivered successfully");
        assert_eq!(get_text(page, "Patient Displays").await?, "Red flags displayed correctly");
        assert_eq!(get_text(page, "Audit Trail").await?, "Test alert logged with timestamp");

        // And test alerts are clearly marked as "SYSTEM TEST"
        let test_alert_marking = get_text(page, "Test Alert Marking").await?;
        assert_eq!(test_alert_marking, "SYSTEM TEST");

        // And all test alerts are automatically cleared after validation
        let test_alerts = page.get_by_test_id("test-alert");
        assert_eq!(test_alerts.count().await?, 0);

        // And system performance metrics are recorded:
        //   | Metric           | Measurement                                  |
        //   | Alert Latency    | <30 seconds from result to notification     |
        //   | Delivery Success | 100% successful delivery to all recipients  |
        //   | Display Update   | <5 seconds to update all patient displays   |
        assert_eq!(get_text(page, "Alert Latency").await?, "<30 seconds from result to notification");
        assert_eq!(get_text(page, "Delivery Success").await?, "100% successful delivery to all recipients");
        assert_eq!(get_text(page, "Display Update").await?, "<5 seconds to update all patient displays");
        Ok(())
    })
}

fn main() {
    run_feature(&[
        ("Process critically high troponin result with immediate alerts", scenario_01_process_critically_high_troponin_result_with_immediate_alert),
        ("Handle critical troponin with physician acknowledgment requirements", scenario_02_handle_critical_troponin_with_physician_acknowledgment_requi),
        ("Process multiple critical values simultaneously", scenario_03_process_multiple_critical_values_simultaneously),
        ("Handle critical troponin during shift change", scenario_04_handle_critical_troponin_during_shift_change),
        ("Process critical troponin with additional cardiac markers", scenario_05_process_critical_troponin_with_additional_cardiac_markers),
        ("Handle false positive critical troponin alerts", scenario_06_handle_false_positive_critical_troponin_alerts),
        ("Critical troponin with patient transfer requirements", scenario_07_critical_troponin_with_patient_transfer_requirements),
        ("Validate critical troponin alert system functionality", scenario_08_validate_critical_troponin_alert_system_functionality),
    ]);
}
