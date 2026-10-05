// Playwright + libtest-mimic test for
// tests-with-given-when-then-features/14-provider-assignment.feature
// (equivalent to tests-with-playwright-javascript/14-provider-assignment.test.js).
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
    //   And the provider assignment module is active
    //   And provider workload tracking is enabled
    //   And mobile notification system is functional
    verify_system_is_operational(page).await?;
    login(page, "a charge nurse", false).await?;
    // The provider assignment module, provider workload tracking, and the
    // mobile notification system are assumed to be active/enabled backend
    // configuration already in place for this environment.

    let provider_assignment_nav_link = wait_for_test_id(page, "Nav Provider Assignment").await?;
    provider_assignment_nav_link.click(None).await?;
    wait_for_test_id(page, "Provider Assignment Panel").await?;
    Ok(())
}

fn scenario_01_assign_highest_priority_patient_to_newly_available_physician(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given "Dr. Johnson" was seeing a patient in bed "ED-8"
        // And the current patient queue contains:
        //   | Position | Patient Name    | ESI Level | Triage Level   | Wait Time | Bed Ready |
        //   | 1        | Maria Santos    | 2         | High Priority  | 45 min    | Yes       |
        //   | 2        | Robert Kim      | 2         | High Priority  | 60 min    | Yes       |
        //   | 3        | Lisa Chen       | 3         | Urgent         | 90 min    | Yes       |
        //   | 4        | David Brown     | 3         | Urgent         | 105 min   | No        |
        // (assumed pre-seeded test data)
        // When "Dr. Johnson" completes the discharge for the patient in bed "ED-8"
        // And the system detects "Dr. Johnson" is now available
        // (assumed to have already occurred / triggered by the system)

        // Then the system identifies the next patient assignment:
        wait_for_test_id(page, "Highest Priority").await?;
        let next_assignment_criteria = vec![
            row([("Criteria", "Highest Priority"), ("Value", "Maria Santos (ESI Level 2)")]),
            row([("Criteria", "Bed Availability"), ("Value", "Bed ready for immediate assignment")]),
            row([("Criteria", "Provider Match"), ("Value", "Dr. Johnson available and qualified")]),
        ];
        for row_data in &next_assignment_criteria {
            let criteria = row_data["Criteria"].as_str();
            let value = row_data["Value"].as_str();
            assert_eq!(get_text(page, criteria).await?, value);
        }

        // And the system assigns "Maria Santos" to "Dr. Johnson"
        assert_eq!(get_text(page, "Assigned Patient").await?, "Maria Santos");
        assert_eq!(get_text(page, "Assigned Provider").await?, "Dr. Johnson");

        // And a notification is sent to Dr. Johnson's mobile device:
        let mobile_notification = vec![
            row([("Type", "Patient Assignment"), ("Content", "📱 New Patient: Maria Santos, Bed ED-12")]),
            row([("Type", "Priority Level"), ("Content", "ESI Level 2 - High Priority")]),
            row([("Type", "Chief Complaint"), ("Content", "Severe chest pain")]),
            row([("Type", "Wait Time"), ("Content", "Patient waiting 45 minutes")]),
            row([("Type", "Action Required"), ("Content", "Please proceed to ED-12")]),
        ];
        for row_data in &mobile_notification {
            let type_ = row_data["Type"].as_str();
            let content = row_data["Content"].as_str();
            assert_eq!(get_text(page, type_).await?, content);
        }

        // And the patient status is updated to "Assigned to Dr. Johnson"
        assert_eq!(get_text(page, "Patient Status").await?, "Assigned to Dr. Johnson");

        // And the queue position is updated for remaining patients
        let patient_queue = wait_for_test_id(page, "Patient Queue").await?;
        assert!(patient_queue.is_visible().await?);
        Ok(())
    })
}

fn scenario_02_handle_provider_assignment_with_specialty_requirements(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given "Dr. Martinez" (Emergency Medicine) becomes available
        // And "Dr. Patel" (Pediatric Emergency) becomes available
        // And the current queue contains:
        //   | Patient Name     | Age | ESI Level | Specialty Required     | Wait Time |
        //   | Adult Patient    | 45  | 2         | Emergency Medicine     | 30 min    |
        //   | Child Patient    | 8   | 2         | Pediatric Emergency    | 35 min    |
        //   | General Patient  | 30  | 3         | Any                    | 60 min    |
        // (assumed pre-seeded test data)
        // When both providers request their next patient assignment
        // (assumed to have already occurred / triggered by the system)

        // Then the system matches providers to appropriate patients:
        wait_for_test_id(page, "Dr. Patel Assigned Patient").await?;
        let provider_matches = vec![
            row([("Provider", "Dr. Patel"), ("Assigned Patient", "Child Patient")]),
            row([("Provider", "Dr. Martinez"), ("Assigned Patient", "Adult Patient")]),
        ];
        for row in &provider_matches {
            assert_eq!(get_text(page, &format!("{} Assigned Patient", row["Provider"].as_str())).await?, row["Assigned Patient"].as_str());
        }

        // And specialty-specific notifications are sent:
        let specialty_notifications = vec![
            row([("Provider", "Dr. Patel"), ("Content", "👶 Pediatric Patient: Age 8, ESI 2, Fever")]),
            row([("Provider", "Dr. Martinez"), ("Content", "🏥 Adult Patient: Age 45, ESI 2, Chest pain")]),
        ];
        for row_data in &specialty_notifications {
            let provider = row_data["Provider"].as_str();
            let content = row_data["Content"].as_str();
            assert_eq!(get_text(page, &format!("{} Notification", provider)).await?, content);
        }

        // And the general patient remains in queue for the next available provider
        let general_patient_status = get_text(page, "General Patient Queue Status").await?;
        assert_match(&general_patient_status, r"queue", true);
        Ok(())
    })
}

fn scenario_03_prioritize_critical_patient_over_standard_queue_order(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given "Dr. Thompson" becomes available
        // And the queue contains patients in order:
        //   | Position | Patient Name    | ESI Level | Assigned Bed | Special Circumstances |
        //   | 1        | Standard Patient| 3         | ED-5         | None                  |
        //   | 2        | Urgent Patient  | 3         | ED-7         | None                  |
        //   | 3        | Critical Patient| 1         | ED-TRAUMA-1  | Just arrived          |
        // (assumed pre-seeded test data)
        // When the system identifies the next patient for "Dr. Thompson"
        // (assumed to have already occurred / triggered by the system)

        // Then the system prioritizes by acuity over queue position:
        wait_for_test_id(page, "Skip Queue Order").await?;
        let prioritization_logic = vec![
            row([("Logic", "Skip Queue Order"), ("Reasoning", "ESI Level 1 takes priority over Level 3")]),
            row([("Logic", "Critical Priority"), ("Reasoning", "Life-threatening condition requires immediate")]),
            row([("Logic", "Provider Capability"), ("Reasoning", "Dr. Thompson qualified for trauma cases")]),
        ];
        for row_data in &prioritization_logic {
            let logic = row_data["Logic"].as_str();
            let reasoning = row_data["Reasoning"].as_str();
            assert_eq!(get_text(page, logic).await?, reasoning);
        }

        // And "Critical Patient" is assigned to "Dr. Thompson"
        assert_eq!(get_text(page, "Assigned Patient").await?, "Critical Patient");
        assert_eq!(get_text(page, "Assigned Provider").await?, "Dr. Thompson");

        // And the mobile notification includes urgency indicators:
        let urgency_indicators = vec![
            row([("Field", "Priority Alert"), ("Content", "🚨 CRITICAL: ESI Level 1 - Trauma")]),
            row([("Field", "Patient Location"), ("Content", "ED-TRAUMA-1")]),
            row([("Field", "Immediate Action"), ("Content", "Requires immediate assessment")]),
            row([("Field", "Support Teams"), ("Content", "Trauma team standing by")]),
        ];
        for row_data in &urgency_indicators {
            let field = row_data["Field"].as_str();
            let content = row_data["Content"].as_str();
            assert_eq!(get_text(page, field).await?, content);
        }
        Ok(())
    })
}

fn scenario_04_handle_provider_assignment_during_high_volume_period(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given the ED is operating at 95% capacity
        // And multiple providers become available simultaneously:
        //   | Provider Name | Specialty          | Last Patient Completed |
        //   | Dr. Adams     | Emergency Medicine | 14:30                 |
        //   | Dr. Brown     | Emergency Medicine | 14:32                 |
        //   | Dr. Wilson    | Emergency Medicine | 14:35                 |
        // And 12 patients are waiting to be seen
        // (assumed pre-seeded test data)
        // When the system processes multiple provider assignments
        // (assumed to have already occurred / triggered by the system)

        // Then the system optimizes assignments across all available providers:
        wait_for_test_id(page, "Dr. Adams Assigned Patient").await?;
        let optimized_assignments = vec![
            row([("Provider", "Dr. Adams"), ("Assigned Patient", "Patient A")]),
            row([("Provider", "Dr. Brown"), ("Assigned Patient", "Patient B")]),
            row([("Provider", "Dr. Wilson"), ("Assigned Patient", "Patient C")]),
        ];
        for row in &optimized_assignments {
            assert_eq!(get_text(page, &format!("{} Assigned Patient", row["Provider"].as_str())).await?, row["Assigned Patient"].as_str());
        }

        // And coordinated notifications are sent to prevent conflicts
        let notification_coordination_status = get_text(page, "Notification Coordination Status").await?;
        assert_match(&notification_coordination_status, r"coordinated", true);

        // And remaining patients receive updated wait time estimates
        let wait_time_update_status = get_text(page, "Wait Time Update Status").await?;
        assert_match(&wait_time_update_status, r"updated", true);

        // And surge capacity protocols are activated if needed
        let surge_capacity_status = get_text(page, "Surge Capacity Protocol Status").await?;
        assert_match(&surge_capacity_status, r"activated|standby", true);
        Ok(())
    })
}

fn scenario_05_provider_assignment_with_workload_balancing(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given provider workload tracking shows:
        //   | Provider Name | Patients Seen Today | Current Workload | Complexity Score |
        //   | Dr. Garcia    | 12                 | Light            | 85              |
        //   | Dr. Lee       | 18                 | Heavy            | 140             |
        //   | Dr. Foster    | 15                 | Moderate         | 110             |
        // And "Dr. Garcia" and "Dr. Foster" both become available
        // And the next patient is "Complex Patient" with multiple comorbidities
        // (assumed pre-seeded test data)
        // When the system determines provider assignment
        // (assumed to have already occurred / triggered by the system)

        // Then the system considers workload balancing:
        wait_for_test_id(page, "Current Workload Decision").await?;
        let workload_balancing = vec![
            row([("Factor", "Current Workload"), ("Decision", "Favors Dr. Garcia")]),
            row([("Factor", "Complexity Fit"), ("Decision", "Both qualified")]),
            row([("Factor", "Fatigue Factor"), ("Decision", "Dr. Garcia preferred")]),
        ];
        for row_data in &workload_balancing {
            let factor = row_data["Factor"].as_str();
            let decision = row_data["Decision"].as_str();
            assert_eq!(get_text(page, &format!("{} Decision", factor)).await?, decision);
        }

        // And "Complex Patient" is assigned to "Dr. Garcia"
        assert_eq!(get_text(page, "Assigned Patient").await?, "Complex Patient");
        assert_eq!(get_text(page, "Assigned Provider").await?, "Dr. Garcia");

        // And workload metrics are updated for both providers
        let dr_garcia_workload = wait_for_test_id(page, "Dr. Garcia Workload").await?;
        assert!(dr_garcia_workload.is_visible().await?);
        let dr_foster_workload = wait_for_test_id(page, "Dr. Foster Workload").await?;
        assert!(dr_foster_workload.is_visible().await?);
        Ok(())
    })
}

fn scenario_06_handle_provider_assignment_with_patient_preferences(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given a patient "VIP Patient" has requested "Dr. Johnson" if available
        // And "Dr. Johnson" and "Dr. Smith" both become available
        // And "VIP Patient" is next in the queue with ESI Level 3
        // (assumed pre-seeded test data)
        // When the system processes provider assignment
        // (assumed to have already occurred / triggered by the system)

        // Then the system considers patient preferences:
        wait_for_test_id(page, "Patient Request").await?;
        let preference_factors = vec![
            row([("Factor", "Patient Request"), ("Details", "Specifically requested Dr. Johnson")]),
            row([("Factor", "Medical Appropriateness"), ("Details", "Both doctors qualified for ESI Level 3")]),
            row([("Factor", "Availability"), ("Details", "Dr. Johnson available and willing")]),
        ];
        for row_data in &preference_factors {
            let factor = row_data["Factor"].as_str();
            let details = row_data["Details"].as_str();
            assert_eq!(get_text(page, factor).await?, details);
        }

        // And "VIP Patient" is assigned to "Dr. Johnson"
        assert_eq!(get_text(page, "Assigned Patient").await?, "VIP Patient");
        assert_eq!(get_text(page, "Assigned Provider").await?, "Dr. Johnson");

        // And "Dr. Smith" receives the next patient in queue
        let dr_smith_assigned_patient = get_text(page, "Dr. Smith Assigned Patient").await?;
        assert!(dr_smith_assigned_patient.len() > 0);

        // And the assignment includes preference notation:
        let preference_notation = vec![
            row([("Field", "Assignment Reason"), ("Content", "Patient preference request honored")]),
            row([("Field", "Special Notes"), ("Content", "VIP status - provide enhanced service")]),
        ];
        for row_data in &preference_notation {
            let field = row_data["Field"].as_str();
            let content = row_data["Content"].as_str();
            assert_eq!(get_text(page, field).await?, content);
        }
        Ok(())
    })
}

fn scenario_07_provider_assignment_failure_and_backup_procedures(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given "Dr. Williams" becomes available
        // And the highest priority patient is "Emergency Patient" (ESI Level 1)
        // (assumed pre-seeded test data)
        // When the system attempts to send assignment notification to Dr. Williams
        // And the mobile device notification fails to deliver
        // (assumed to have already occurred / triggered by the system)

        // Then the system activates backup notification procedures:
        wait_for_test_id(page, "Overhead Page").await?;
        let backup_procedures = vec![
            row([("Method", "Overhead Page"), ("Action", "\"Dr. Williams to ED-TRAUMA-1 immediately\"")]),
            row([("Method", "Desktop Alert"), ("Action", "Popup on all ED workstations")]),
            row([("Method", "Charge Nurse Alert"), ("Action", "Direct notification to charge nurse")]),
            row([("Method", "Secondary Provider"), ("Action", "Alert backup doctor if no response in 2 min")]),
        ];
        for row_data in &backup_procedures {
            let method = row_data["Method"].as_str();
            let action = row_data["Action"].as_str();
            assert_eq!(get_text(page, method).await?, action);
        }

        // And the system logs the notification failure for IT review
        let notification_failure_log = get_text(page, "Notification Failure Log").await?;
        assert_match(&notification_failure_log, r"IT review", true);

        // And continues attempting mobile notification every 30 seconds
        let mobile_retry_status = get_text(page, "Mobile Notification Retry Status").await?;
        assert_match(&mobile_retry_status, r"30 seconds", true);

        // And tracks response time for quality metrics
        let response_time_tracking_status = get_text(page, "Response Time Tracking Status").await?;
        assert_match(&response_time_tracking_status, r"tracking|tracked", true);
        Ok(())
    })
}

fn scenario_08_handle_provider_assignment_during_shift_change(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given it is 19:00 during evening shift change
        // And "Dr. Day" (day shift) is completing final patients
        // And "Dr. Night" (evening shift) is beginning shift
        // And a critical patient arrives requiring immediate attention
        // (assumed pre-seeded test data)
        // When the system determines provider assignment for the critical patient
        // (assumed to have already occurred / triggered by the system)

        // Then the system considers shift transition factors:
        wait_for_test_id(page, "Shift Status").await?;
        let shift_transition_factors = vec![
            row([("Factor", "Shift Status"), ("Consideration", "Dr. Day finishing, Dr. Night starting")]),
            row([("Factor", "Continuity"), ("Consideration", "Assign to Dr. Night for ongoing care")]),
            row([("Factor", "Availability"), ("Consideration", "Dr. Night has capacity for complex case")]),
        ];
        for row_data in &shift_transition_factors {
            let factor = row_data["Factor"].as_str();
            let consideration = row_data["Consideration"].as_str();
            assert_eq!(get_text(page, factor).await?, consideration);
        }

        // And the critical patient is assigned to "Dr. Night"
        assert_eq!(get_text(page, "Assigned Provider").await?, "Dr. Night");

        // And shift handoff information is included in the notification:
        let handoff_information = vec![
            row([("Component", "Shift Context"), ("Content", "New critical patient - evening shift start")]),
            row([("Component", "Day Shift Status"), ("Content", "Dr. Day finishing last 2 patients")]),
            row([("Component", "Support Available"), ("Content", "Day shift available for consultation")]),
        ];
        for row_data in &handoff_information {
            let component = row_data["Component"].as_str();
            let content = row_data["Content"].as_str();
            assert_eq!(get_text(page, component).await?, content);
        }
        Ok(())
    })
}

fn scenario_09_track_provider_response_times_and_assignment_efficiency(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given provider assignment notifications are sent
        // (assumed pre-seeded test data)
        // When providers respond to patient assignments
        // (assumed to have already occurred / triggered by the system)

        // Then the system tracks performance metrics:
        wait_for_test_id(page, "Notification to Response").await?;
        let performance_metrics = vec![
            row([("Metric", "Notification to Response"), ("Measurement", "Time from alert to bedside presence")]),
            row([("Metric", "Assignment Accuracy"), ("Measurement", "Correct provider-patient matching")]),
            row([("Metric", "Queue Optimization"), ("Measurement", "Wait time reduction effectiveness")]),
        ];
        for row_data in &performance_metrics {
            let metric = row_data["Metric"].as_str();
            let measurement = row_data["Measurement"].as_str();
            assert_eq!(get_text(page, metric).await?, measurement);
        }

        // And generates provider performance reports:
        let performance_reports = vec![
            row([("Provider", "Dr. Johnson"), ("Avg Response Time", "3.2 minutes"), ("Assignment Accuracy", "98%"), ("Patient Satisfaction", "4.8/5")]),
            row([("Provider", "Dr. Smith"), ("Avg Response Time", "4.1 minutes"), ("Assignment Accuracy", "96%"), ("Patient Satisfaction", "4.6/5")]),
        ];
        for report in &performance_reports {
            assert_eq!(get_text(page, &format!("{} Avg Response Time", report["Provider"].as_str())).await?, report["Avg Response Time"].as_str());
            assert_eq!(get_text(page, &format!("{} Assignment Accuracy", report["Provider"].as_str())).await?, report["Assignment Accuracy"].as_str());
            assert_eq!(get_text(page, &format!("{} Patient Satisfaction", report["Provider"].as_str())).await?, report["Patient Satisfaction"].as_str());
        }

        // And identifies optimization opportunities:
        let optimization_opportunities = vec![
            row([("Area", "Response Time"), ("Recommendation", "Target <3 minutes for critical patients")]),
            row([("Area", "Assignment Matching"), ("Recommendation", "Consider additional specialty training")]),
            row([("Area", "Communication"), ("Recommendation", "Implement two-way acknowledgment system")]),
        ];
        for row_data in &optimization_opportunities {
            let area = row_data["Area"].as_str();
            let recommendation = row_data["Recommendation"].as_str();
            assert_eq!(get_text(page, area).await?, recommendation);
        }
        Ok(())
    })
}

fn main() {
    run_feature(&[
        ("Assign highest priority patient to newly available physician", scenario_01_assign_highest_priority_patient_to_newly_available_physician),
        ("Handle provider assignment with specialty requirements", scenario_02_handle_provider_assignment_with_specialty_requirements),
        ("Prioritize critical patient over standard queue order", scenario_03_prioritize_critical_patient_over_standard_queue_order),
        ("Handle provider assignment during high volume period", scenario_04_handle_provider_assignment_during_high_volume_period),
        ("Provider assignment with workload balancing", scenario_05_provider_assignment_with_workload_balancing),
        ("Handle provider assignment with patient preferences", scenario_06_handle_provider_assignment_with_patient_preferences),
        ("Provider assignment failure and backup procedures", scenario_07_provider_assignment_failure_and_backup_procedures),
        ("Handle provider assignment during shift change", scenario_08_handle_provider_assignment_during_shift_change),
        ("Track provider response times and assignment efficiency", scenario_09_track_provider_response_times_and_assignment_efficiency),
    ]);
}
