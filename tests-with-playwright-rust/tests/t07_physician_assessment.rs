// Playwright + libtest-mimic test for
// tests-with-given-when-then-features/07-physician-assessment.feature
// (equivalent to tests-with-playwright-javascript/07-physician-assessment.test.js).
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
    //   And I am logged in as "Dr. Smith" on the mobile app
    //   And the patient chart access module is enabled
    //   And real-time data synchronization is active
    verify_system_is_operational(page).await?;
    login(page, "Dr. Smith", true).await?;
    // The patient chart access module and real-time data synchronization are
    // assumed to be pre-seeded/enabled test data.

    let feature_nav_link = wait_for_test_id(page, "Nav Physician Assessment").await?;
    feature_nav_link.click(None).await?;
    wait_for_test_id(page, "Physician Assessment Panel").await?;
    Ok(())
}

fn scenario_01_access_patient_chart_with_complete_nursing_assessment(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given a patient "Jennifer Martinez" is assigned to bed "ED-8"
        // And the nursing assessment is completed with the following data:
        // (assumed pre-seeded test data)

        // When I open the patient's chart for "Jennifer Martinez" in bed "ED-8"
        page.get_by_test_id("open-patient-chart-button").first().click(None).await?;

        // Then the system displays the patient summary with:
        let patient_summary_fields = vec![
            row([("Section", "Patient Identity"), ("Content", "Jennifer Martinez, DOB: 1975-03-15")]),
            row([("Section", "Bed Assignment"), ("Content", "ED-8")]),
            row([("Section", "Arrival Time"), ("Content", "14:00")]),
            row([("Section", "Triage Notes"), ("Content", "ESI Level 2 - Severe chest pain, onset 2h ago")]),
        ];
        for row_data in &patient_summary_fields {
            let section = row_data["Section"].as_str();
            let content = row_data["Content"].as_str();
            assert_eq!(get_text(page, section).await?, content);
        }

        // And the vital signs section shows:
        let vital_signs = vec![
            row([("Vital Sign", "Blood Pressure"), ("Value", "160/95"), ("Trend", "High")]),
            row([("Vital Sign", "Heart Rate"), ("Value", "110"), ("Trend", "Elevated")]),
            row([("Vital Sign", "Respiratory Rate"), ("Value", "22"), ("Trend", "Elevated")]),
            row([("Vital Sign", "Temperature"), ("Value", "98.6°F"), ("Trend", "Normal")]),
            row([("Vital Sign", "Oxygen Saturation"), ("Value", "94%"), ("Trend", "Low")]),
            row([("Vital Sign", "Pain Score"), ("Value", "8/10"), ("Trend", "Severe")]),
        ];
        for vital in &vital_signs {
            assert_eq!(get_text(page, vital["Vital Sign"].as_str()).await?, vital["Value"].as_str());
            assert_eq!(get_text(page, &format!("{} Trend", vital["Vital Sign"].as_str())).await?, vital["Trend"].as_str());
        }

        // And the allergies section displays:
        let allergies = vec![
            row([("Allergy", "Penicillin"), ("Reaction Type", "Rash"), ("Severity", "Moderate")]),
            row([("Allergy", "Shellfish"), ("Reaction Type", "Unknown"), ("Severity", "Unknown")]),
        ];
        for allergy in &allergies {
            assert_eq!(get_text(page, &format!("{} Reaction", allergy["Allergy"].as_str())).await?, allergy["Reaction Type"].as_str());
            assert_eq!(get_text(page, &format!("{} Severity", allergy["Allergy"].as_str())).await?, allergy["Severity"].as_str());
        }

        // And the current medications section shows:
        let medications = vec![
            row([("Medication", "Metoprolol"), ("Dosage", "50mg"), ("Status", "Active")]),
            row([("Medication", "Aspirin"), ("Dosage", "81mg"), ("Status", "Active")]),
        ];
        for medication in &medications {
            assert_eq!(get_text(page, &format!("{} Dosage", medication["Medication"].as_str())).await?, medication["Dosage"].as_str());
            assert_eq!(get_text(page, &format!("{} Status", medication["Medication"].as_str())).await?, medication["Status"].as_str());
        }
        Ok(())
    })
}

fn scenario_02_access_patient_chart_during_active_treatment(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given a patient "Michael Chen" is assigned to bed "ED-3"
        // And the patient is currently receiving active treatment
        // And recent assessments include:
        // (assumed pre-seeded test data)

        // When I open the patient's chart for "Michael Chen" in bed "ED-3"
        page.get_by_test_id("open-patient-chart-button").first().click(None).await?;

        // Then the system displays real-time information with:
        let summary_fields = vec![
            row([("Section", "Current Status"), ("Content", "Active treatment in progress")]),
            row([("Section", "Most Recent Vitals"), ("Content", "BP: 130/80, HR: 88, T: 100.2°F (14:15)")]),
            row([("Section", "Active Orders"), ("Content", "Lab work in progress")]),
            row([("Section", "Triage Summary"), ("Content", "ESI 3 - Abd pain, onset 6h ago")]),
        ];
        for row_data in &summary_fields {
            let section = row_data["Section"].as_str();
            let content = row_data["Content"].as_str();
            assert_eq!(get_text(page, section).await?, content);
        }

        // And all data includes timestamps showing data freshness
        wait_for_test_id(page, "Data Freshness Timestamp").await?;

        // And any alerts or critical values are highlighted in red
        let critical_value_element = wait_for_test_id(page, "Critical Value Highlight").await?;
        assert!(critical_value_element.is_visible().await?);

        // And pending lab results show "In Progress" status with expected completion time
        let lab_result_status = get_text(page, "Lab Result Status").await?;
        assert_eq!(lab_result_status, "In Progress");
        Ok(())
    })
}

fn scenario_03_view_patient_chart_with_medication_allergies_and_interaction(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given a patient "Robert Johnson" is assigned to bed "ED-12"
        // And the patient has multiple drug allergies:
        // And current medications include:
        // (assumed pre-seeded test data)

        // When I open the patient's chart for "Robert Johnson"
        page.get_by_test_id("open-patient-chart-button").first().click(None).await?;

        // Then the allergy section prominently displays:
        let allergy_alerts = vec![
            row([("Alert Type", "Critical Alert"), ("Message", "SEVERE ALLERGIES: Morphine, NSAIDs")]),
            row([("Alert Type", "Warning"), ("Message", "Moderate allergy: Codeine")]),
        ];
        for alert in &allergy_alerts {
            assert_eq!(get_text(page, alert["Alert Type"].as_str()).await?, alert["Message"].as_str());
        }

        // And the medication section shows:
        let medications = vec![
            row([("Medication", "Warfarin"), ("Status", "Active"), ("Interaction Alerts", "Monitor for bleeding risk")]),
            row([("Medication", "Metformin"), ("Status", "Active"), ("Interaction Alerts", "No interactions detected")]),
        ];
        for medication in &medications {
            assert_eq!(get_text(page, &format!("{} Status", medication["Medication"].as_str())).await?, medication["Status"].as_str());
            assert_eq!(get_text(page, &format!("{} Interaction Alerts", medication["Medication"].as_str())).await?, medication["Interaction Alerts"].as_str());
        }

        // And any new medication orders will trigger allergy checking
        wait_for_test_id(page, "Allergy Checking Notice").await?;

        // And interaction warnings are displayed for contraindicated drugs
        wait_for_test_id(page, "Interaction Warning").await?;
        Ok(())
    })
}

fn scenario_04_access_chart_for_pediatric_patient_with_age_appropriate_data(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given a pediatric patient "Emma Foster" (age 7) is assigned to bed "ED-PEDS-2"
        // And the nursing assessment includes pediatric-specific data:
        // (assumed pre-seeded test data)

        // When I open the pediatric patient's chart for "Emma Foster"
        page.get_by_test_id("open-patient-chart-button").first().click(None).await?;

        // Then the system displays pediatric-specific information:
        let pediatric_fields = vec![
            row([("Section", "Patient Age/Weight"), ("Content", "7 years old, 22 kg")]),
            row([("Section", "Pediatric Vital Ranges"), ("Content", "All vitals with age-appropriate norms")]),
            row([("Section", "Growth Percentiles"), ("Content", "Weight: 50th percentile")]),
            row([("Section", "Guardian Information"), ("Content", "Sarah Foster (mother) - present")]),
        ];
        for row_data in &pediatric_fields {
            let section = row_data["Section"].as_str();
            let content = row_data["Content"].as_str();
            assert_eq!(get_text(page, section).await?, content);
        }

        // And vital signs are displayed with pediatric normal ranges:
        let vital_signs = vec![
            row([("Vital Sign", "Blood Pressure"), ("Value", "95/60"), ("Status", "Normal")]),
            row([("Vital Sign", "Heart Rate"), ("Value", "110"), ("Status", "Normal")]),
            row([("Vital Sign", "Respiratory"), ("Value", "24"), ("Status", "Normal")]),
            row([("Vital Sign", "Temperature"), ("Value", "102.8°F"), ("Status", "Elevated")]),
        ];
        for vital in &vital_signs {
            assert_eq!(get_text(page, vital["Vital Sign"].as_str()).await?, vital["Value"].as_str());
            assert_eq!(get_text(page, &format!("{} Status", vital["Vital Sign"].as_str())).await?, vital["Status"].as_str());
        }

        // And medication dosing shows weight-based calculations
        wait_for_test_id(page, "Weight-Based Dosing").await?;

        // And parental consent status is clearly indicated
        wait_for_test_id(page, "Parental Consent Status").await?;
        Ok(())
    })
}

fn scenario_05_handle_incomplete_nursing_assessment(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given a patient "David Wilson" is assigned to bed "ED-6"
        // And the nursing assessment is partially completed:
        // (assumed pre-seeded test data)

        // When I open the patient's chart for "David Wilson"
        page.get_by_test_id("open-patient-chart-button").first().click(None).await?;

        // Then the system displays available information clearly marked:
        let completed_data = get_text(page, "Completed Data").await?;
        assert_eq!(completed_data, "Triage notes, initial vitals available");

        let missing_data_items = page.get_by_test_id("missing-data-item");
        assert_eq!(missing_data_items.count().await?, 3);
        let missing_data_texts = texts_of(&missing_data_items).await?;
        assert_eq!(missing_data_texts, vec!["Allergies: Assessment in progress", "Medications: History pending", "Pain scale: Not yet assessed"]);

        // And incomplete sections are highlighted with:
        let visual_indicators = vec![
            row([("Visual Indicator", "Yellow Warning"), ("Description", "Assessment in progress")]),
            row([("Visual Indicator", "Refresh Timer"), ("Description", "Auto-refresh every 30 seconds")]),
            row([("Visual Indicator", "Notification"), ("Description", "\"Assessment updating - refresh for latest\"")]),
        ];
        for indicator in &visual_indicators {
            assert_eq!(get_text(page, indicator["Visual Indicator"].as_str()).await?, indicator["Description"].as_str());
        }

        // And I can request priority completion of missing critical data
        wait_for_test_id(page, "Request Priority Completion Button").await?;
        Ok(())
    })
}

fn scenario_06_access_chart_during_shift_change_with_handoff_notes(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given a patient "Lisa Brown" is assigned to bed "ED-9"
        // And it is during the evening shift change (19:00)
        // And the day shift nurse added handoff notes:
        // (assumed pre-seeded test data)

        // When I open the patient's chart for "Lisa Brown"
        page.get_by_test_id("open-patient-chart-button").first().click(None).await?;

        // Then the system prominently displays shift handoff information:
        let handoff_fields = vec![
            row([("Handoff Section", "Clinical Summary"), ("Content", "Stable condition, pain controlled")]),
            row([("Handoff Section", "Pending Tasks"), ("Content", "Orthopedic consult ordered - pending")]),
            row([("Handoff Section", "Communication Log"), ("Content", "Family contact: Son updated 18:30")]),
            row([("Handoff Section", "Special Needs"), ("Content", "Patient preference: Female staff")]),
        ];
        for field in &handoff_fields {
            assert_eq!(get_text(page, field["Handoff Section"].as_str()).await?, field["Content"].as_str());
        }

        // And the handoff notes are clearly timestamped
        wait_for_test_id(page, "Handoff Notes Timestamp").await?;

        // And I can add my own physician handoff notes
        wait_for_test_id(page, "Add Physician Handoff Notes").await?;

        // And the evening nurse can see both nursing and physician handoff information
        wait_for_test_id(page, "Combined Handoff Information").await?;
        Ok(())
    })
}

fn scenario_07_handle_patient_chart_access_during_network_connectivity_issu(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given a patient "Thomas Anderson" is assigned to bed "ED-4"
        // And the mobile app has intermittent network connectivity
        // (assumed pre-seeded test data)

        // When I attempt to open the patient's chart
        page.get_by_test_id("open-patient-chart-button").first().click(None).await?;

        // And the network connection is temporarily unavailable
        // (simulated network condition, no direct UI action)

        // Then the system displays cached patient data with:
        let cached_data_fields = vec![
            row([("Data Type", "Basic Demographics"), ("Availability", "Available (cached)")]),
            row([("Data Type", "Last Known Vitals"), ("Availability", "Available - last sync 16:45")]),
            row([("Data Type", "Medication Data"), ("Availability", "Available (cached)")]),
            row([("Data Type", "Recent Lab Results"), ("Availability", "May not be current - sync pending")]),
        ];
        for field in &cached_data_fields {
            assert_eq!(get_text(page, field["Data Type"].as_str()).await?, field["Availability"].as_str());
        }

        // And a connectivity warning is displayed: "Limited connectivity - data may not be current"
        let connectivity_warning = get_text(page, "Connectivity Warning").await?;
        assert_eq!(connectivity_warning, "Limited connectivity - data may not be current");

        // And the app attempts automatic sync when connection is restored
        wait_for_test_id(page, "Automatic Sync Status").await?;

        // And critical data is prioritized for sync when connectivity returns
        wait_for_test_id(page, "Sync Priority Notice").await?;

        // And I can manually trigger refresh when connection improves
        wait_for_test_id(page, "Manual Refresh Button").await?;
        Ok(())
    })
}

fn scenario_08_access_chart_with_time_sensitive_alerts_and_notifications(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given a patient "Karen White" is assigned to bed "ED-7"
        // And the patient has time-sensitive clinical alerts:
        // (assumed pre-seeded test data)

        // When I open the patient's chart for "Karen White"
        page.get_by_test_id("open-patient-chart-button").first().click(None).await?;

        // Then the system prominently displays active alerts:
        let active_alerts = vec![
            row([("Alert Priority", "CRITICAL"), ("Alert Details", "🔴 Troponin 0.8 - Possible MI (17:15)")]),
            row([("Alert Priority", "WARNING"), ("Alert Details", "🟡 Medication due - Metoprolol (17:30)")]),
            row([("Alert Priority", "INFO"), ("Alert Details", "🔵 Pain reassessment overdue (17:25)")]),
        ];
        for alert in &active_alerts {
            assert_eq!(get_text(page, alert["Alert Priority"].as_str()).await?, alert["Alert Details"].as_str());
        }

        // And critical alerts require acknowledgment before proceeding
        wait_for_test_id(page, "Alert Acknowledgment").await?;

        // And the timestamp shows how long ago each alert was generated
        wait_for_test_id(page, "Alert Timestamp").await?;

        // And I can take direct action on alerts (order meds, document assessment)
        wait_for_test_id(page, "Alert Action Button").await?;

        // And alert resolution is tracked and timestamped
        wait_for_test_id(page, "Alert Resolution Tracking").await?;
        Ok(())
    })
}

fn main() {
    run_feature(&[
        ("Access patient chart with complete nursing assessment", scenario_01_access_patient_chart_with_complete_nursing_assessment),
        ("Access patient chart during active treatment", scenario_02_access_patient_chart_during_active_treatment),
        ("View patient chart with medication allergies and interactions", scenario_03_view_patient_chart_with_medication_allergies_and_interaction),
        ("Access chart for pediatric patient with age-appropriate data", scenario_04_access_chart_for_pediatric_patient_with_age_appropriate_data),
        ("Handle incomplete nursing assessment", scenario_05_handle_incomplete_nursing_assessment),
        ("Access chart during shift change with handoff notes", scenario_06_access_chart_during_shift_change_with_handoff_notes),
        ("Handle patient chart access during network connectivity issues", scenario_07_handle_patient_chart_access_during_network_connectivity_issu),
        ("Access chart with time-sensitive alerts and notifications", scenario_08_access_chart_with_time_sensitive_alerts_and_notifications),
    ]);
}
