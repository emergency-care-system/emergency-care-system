// Selenium WebDriver + libtest-mimic test for
// tests-with-given-when-then-features/19-mass-casualty-activation.feature
// (equivalent to tests-with-selenium-javascript/19-mass-casualty-activation.test.js).
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
    //   And I am logged in as "Charge Nurse Williams"
    //   And the mass casualty incident (MCI) module is available (assumed pre-seeded test data)
    //   And emergency contact systems are enabled (assumed pre-seeded test data)
    //   And surge capacity protocols are configured (assumed pre-seeded test data)
    verify_system_is_operational(driver).await?;
    login(driver, "Charge Nurse Williams", false).await?;

    let feature_nav_link = wait_for_test_id(driver, "Nav Mass Casualty Activation").await?;
    feature_nav_link.click().await?;
    wait_for_test_id(driver, "Mass Casualty Activation Panel").await?;
    Ok(())
}

fn scenario_01_activate_mass_casualty_protocol_for_multi_vehicle_accident(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given it is 16:30 on a Friday afternoon (assumed pre-seeded test data)
        // And normal ED operations are in progress with 12 patients currently in the department (assumed pre-seeded test data)
        // And EMS reports a multi-vehicle accident with 8+ casualties en route (assumed pre-seeded test data)
        // When I receive notification of the mass casualty incident: (assumed simulated by test fixture data)
        // And I activate the disaster protocol in the system
        driver.find(By::Css("[data-testid=\"activate-disaster-protocol\"]")).await?.click().await?;

        // Then the system immediately switches to surge capacity mode:
        wait_for_test_id(driver, "Mode Indicator").await?;
        let system_change_rows = vec![
            row([("label", "Mode Indicator"), ("value", "\"MASS CASUALTY ACTIVE\" banner displayed")]),
            row([("label", "Interface Switch"), ("value", "MCI-specific workflows activated")]),
            row([("label", "Normal Operations"), ("value", "Routine tasks suspended/deprioritized")]),
            row([("label", "Resource Allocation"), ("value", "Emergency resource management enabled")]),
            row([("label", "Communication Mode"), ("value", "Critical alerts and notifications active")]),
        ];
        for row in &system_change_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And additional staff are automatically alerted:
        let staff_alert_rows = vec![
            row([("label", "Off-duty Physicians"), ("value", "SMS, Phone call")]),
            row([("label", "Off-duty Nurses"), ("value", "SMS, Phone call")]),
            row([("label", "Surgical Team"), ("value", "Overhead page, SMS")]),
            row([("label", "Lab/Radiology"), ("value", "System alert, Phone")]),
            row([("label", "Administration"), ("value", "Phone call, SMS")]),
            row([("label", "Security"), ("value", "Radio, Overhead page")]),
        ];
        for row in &staff_alert_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And rapid registration workflows are created:
        let registration_feature_rows = vec![
            row([("label", "Patient Identification"), ("value", "Sequential numbering: MCI-001, MCI-002")]),
            row([("label", "Triage Tags"), ("value", "Color-coded electronic tags")]),
            row([("label", "Minimal Data Entry"), ("value", "Name, age, chief complaint only")]),
            row([("label", "Family Notification"), ("value", "Automated family alert system")]),
            row([("label", "Tracking Board"), ("value", "Real-time patient status dashboard")]),
        ];
        for row in &registration_feature_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }
        Ok(())
    })
}

fn scenario_02_configure_surge_capacity_with_bed_and_resource_expansion(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given the mass casualty protocol has been activated
        // And normal bed capacity is 20 beds
        // When the system enters surge capacity mode

        // Then additional treatment areas are activated:
        wait_for_test_id(driver, "Hallway Beds").await?;
        let surge_area_rows = vec![
            row([("label", "Hallway Beds"), ("value", "+6 treatment spaces")]),
            row([("label", "Procedure Rooms"), ("value", "+3 converted spaces")]),
            row([("label", "Observation Area"), ("value", "+8 holding spaces")]),
            row([("label", "Waiting Room Triage"), ("value", "+4 assessment areas")]),
            row([("label", "Ambulatory Care"), ("value", "+10 walking wounded")]),
        ];
        for row in &surge_area_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And resource allocation is optimized for mass casualty:
        let resource_allocation_rows = vec![
            row([("label", "Trauma Bays"), ("value", "All 4 activated")]),
            row([("label", "Operating Rooms"), ("value", "3 rooms on standby")]),
            row([("label", "Ventilators"), ("value", "8 total (5 from ICU)")]),
            row([("label", "Blood Products"), ("value", "Massive transfusion protocol")]),
            row([("label", "Medication Carts"), ("value", "5 carts deployed")]),
        ];
        for row in &resource_allocation_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And staffing ratios are adjusted for emergency operations:
        let staffing_ratio_rows = vec![
            row([("label", "Physicians"), ("value", "1:12 patients")]),
            row([("label", "Nurses"), ("value", "1:6 patients")]),
            row([("label", "Support Staff"), ("value", "Double coverage")]),
        ];
        for row in &staffing_ratio_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }
        Ok(())
    })
}

fn scenario_03_implement_rapid_patient_registration_and_triage_workflow(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given mass casualty mode is active
        // And the first ambulance arrives with 3 critical patients
        // When EMS brings patients to the ED

        // Then the rapid registration workflow is initiated:
        wait_for_test_id(driver, "Patient Arrival").await?;
        let registration_step_rows = vec![
            row([("label", "Patient Arrival"), ("value", "Immediate tag assignment: MCI-001, 002, 003")]),
            row([("label", "Triage Assessment"), ("value", "START triage protocol applied")]),
            row([("label", "Electronic Tagging"), ("value", "Color-coded digital tags assigned")]),
            row([("label", "Minimal Documentation"), ("value", "Name, estimated age, mechanism of injury")]),
            row([("label", "Bed Assignment"), ("value", "Automatic assignment by acuity")]),
        ];
        for row in &registration_step_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And electronic triage tags are applied with color coding:
        let triage_color_rows = vec![
            row([("label", "Red (Immediate)"), ("value", "MCI-001 → Trauma Bay 1")]),
            row([("label", "Yellow (Delayed)"), ("value", "MCI-002 → Surge Bed 3")]),
            row([("label", "Green (Minor)"), ("value", "MCI-003 → Ambulatory Area")]),
            row([("label", "Black (Deceased)"), ("value", "Morgue coordination")]),
        ];
        for row in &triage_color_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And family notification systems are activated:
        let notification_process_rows = vec![
            row([("label", "Emergency Contacts"), ("value", "Auto-dial from patient personal effects")]),
            row([("label", "Public Information"), ("value", "Hospital hotline number broadcasted")]),
            row([("label", "Media Coordination"), ("value", "Incident command liaison activated")]),
            row([("label", "Social Services"), ("value", "Family support team mobilized")]),
        ];
        for row in &notification_process_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }
        Ok(())
    })
}

fn scenario_04_coordinate_with_external_emergency_services_and_hospitals(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a major mass casualty incident is in progress
        // And local EMS is overwhelmed with the response
        // When the system activates external coordination protocols

        // Then inter-facility communication is established:
        wait_for_test_id(driver, "EMS Command Center").await?;
        let communication_channel_rows = vec![
            row([("label", "EMS Command Center"), ("value", "Patient distribution and transport updates")]),
            row([("label", "Other Area Hospitals"), ("value", "Bed availability and transfer coordination")]),
            row([("label", "Air Medical Services"), ("value", "Helicopter transport for critical patients")]),
            row([("label", "Regional Trauma Centers"), ("value", "Specialty care transfer arrangements")]),
        ];
        for row in &communication_channel_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And patient distribution management is activated:
        let distribution_strategy_rows = vec![
            row([("label", "Load Balancing"), ("value", "Distribute patients across regional facilities")]),
            row([("label", "Specialty Matching"), ("value", "Route patients to appropriate specialty care")]),
            row([("label", "Capacity Monitoring"), ("value", "Real-time bed availability tracking")]),
            row([("label", "Transport Coordination"), ("value", "Ambulance and helicopter scheduling")]),
        ];
        for row in &distribution_strategy_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And regional emergency management integration occurs:
        let integration_element_rows = vec![
            row([("label", "Incident Command"), ("value", "Hospital EOC links with regional ICS")]),
            row([("label", "Resource Sharing"), ("value", "Equipment and staff sharing protocols")]),
            row([("label", "Information Sharing"), ("value", "Patient status updates to command center")]),
            row([("label", "Media Management"), ("value", "Coordinated public information releases")]),
        ];
        for row in &integration_element_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }
        Ok(())
    })
}

fn scenario_05_manage_family_reunification_and_information_center(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given multiple patients from the mass casualty incident are being treated
        // And families are arriving seeking information about their loved ones
        // When the family information center is activated

        // Then patient tracking and family communication systems are deployed:
        wait_for_test_id(driver, "Information Hotline").await?;
        let family_support_system_rows = vec![
            row([("label", "Information Hotline"), ("value", "Dedicated phone line with trained staff")]),
            row([("label", "Family Reunification"), ("value", "Secure area for family waiting and updates")]),
            row([("label", "Patient Tracking"), ("value", "Real-time status board for authorized viewers")]),
            row([("label", "Privacy Protection"), ("value", "HIPAA-compliant information sharing")]),
        ];
        for row in &family_support_system_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And automated family notification processes are initiated:
        let notification_method_rows = vec![
            row([("label", "SMS Updates"), ("value", "Your family member is being treated safely")]),
            row([("label", "Phone Calls"), ("value", "Personal calls for critical status changes")]),
            row([("label", "Information Boards"), ("value", "General incident updates (no patient names)")]),
            row([("label", "Social Workers"), ("value", "One-on-one family support and counseling")]),
        ];
        for row in &notification_method_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And patient status tracking is maintained:
        let tracking_element_rows = vec![
            row([("label", "Current Location"), ("value", "Treatment area, OR, transferred, etc.")]),
            row([("label", "Medical Status"), ("value", "Stable, critical, treated and released")]),
            row([("label", "Next of Kin Contact"), ("value", "Verification and notification status")]),
            row([("label", "Discharge Planning"), ("value", "Expected timeline and care needs")]),
        ];
        for row in &tracking_element_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }
        Ok(())
    })
}

fn scenario_06_transition_from_mass_casualty_mode_back_to_normal_operations(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given the mass casualty incident has been resolved
        // And all patients are stabilized or transferred
        // And no additional casualties are expected
        // When I initiate the transition back to normal operations
        driver.find(By::Css("[data-testid=\"initiate-transition-to-normal\"]")).await?.click().await?;

        // Then the system manages the deactivation process:
        wait_for_test_id(driver, "Incident Assessment").await?;
        let deactivation_step_rows = vec![
            row([("label", "Incident Assessment"), ("value", "Review of patient outcomes and resources used")]),
            row([("label", "Staff Debriefing"), ("value", "Immediate hot wash and formal debriefing")]),
            row([("label", "Resource Restoration"), ("value", "Return equipment and supplies to normal areas")]),
            row([("label", "Documentation"), ("value", "Complete incident documentation and reports")]),
        ];
        for row in &deactivation_step_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And normal ED operations are gradually restored:
        let restoration_phase_rows = vec![
            row([("label", "Immediate (0-30 min)"), ("value", "Secure scene")]),
            row([("label", "Short-term (30-60 min)"), ("value", "Resource cleanup")]),
            row([("label", "Medium-term (1-4 hrs)"), ("value", "Staff rotation")]),
            row([("label", "Long-term (4-24 hrs)"), ("value", "Full restoration")]),
        ];
        for row in &restoration_phase_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And quality improvement activities are initiated:
        let qi_activity_rows = vec![
            row([("label", "After Action Review"), ("value", "Identify strengths and improvement areas")]),
            row([("label", "Performance Metrics"), ("value", "Analyze response times and patient outcomes")]),
            row([("label", "Protocol Updates"), ("value", "Revise procedures based on lessons learned")]),
            row([("label", "Training Needs"), ("value", "Identify staff training and education needs")]),
        ];
        for row in &qi_activity_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }
        Ok(())
    })
}

fn scenario_07_handle_mass_casualty_incident_during_shift_change(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given it is 19:00 during evening shift change
        // And day shift staff are preparing to leave
        // And evening shift staff are assuming duties
        // When a mass casualty incident is declared

        // Then the system manages staffing during the transition:
        wait_for_test_id(driver, "Shift Hold").await?;
        let staffing_strategy_rows = vec![
            row([("label", "Shift Hold"), ("value", "Day shift staff remain for incident response")]),
            row([("label", "Double Coverage"), ("value", "Both shifts work together during surge")]),
            row([("label", "Incident Command Continuity"), ("value", "Clear leadership chain established")]),
            row([("label", "Communication"), ("value", "All staff briefed on roles and responsibilities")]),
        ];
        for row in &staffing_strategy_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And transition protocols are modified for the emergency:
        let modified_protocol_rows = vec![
            row([("label", "Handoff Procedures"), ("value", "Suspended until incident resolution")]),
            row([("label", "Staffing Ratios"), ("value", "Enhanced coverage with both shifts")]),
            row([("label", "Leadership Structure"), ("value", "Incident commander takes operational control")]),
            row([("label", "Modified Documentation"), ("value", "Emergency documentation procedures active")]),
        ];
        for row in &modified_protocol_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }
        Ok(())
    })
}

fn scenario_08_test_mass_casualty_system_readiness_through_drill(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given it is a scheduled quarterly mass casualty drill
        // And the drill scenario involves a simulated building collapse with 15 casualties
        // When the drill coordinator activates the test mass casualty protocol
        driver.find(By::Css("[data-testid=\"activate-test-mci-protocol\"]")).await?.click().await?;

        // Then the system activates in drill mode:
        wait_for_test_id(driver, "Test Mode Indicator").await?;
        let drill_feature_rows = vec![
            row([("label", "Test Mode Indicator"), ("value", "\"DRILL - NOT REAL EMERGENCY\" displayed")]),
            row([("label", "Simulated Patients"), ("value", "Test patient records created")]),
            row([("label", "Staff Participation"), ("value", "All roles and responsibilities tested")]),
            row([("label", "Resource Tracking"), ("value", "Equipment and supplies tracked but not used")]),
        ];
        for row in &drill_feature_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And drill performance metrics are captured:
        let drill_performance_metric_rows = vec![
            row([("label", "Activation Time"), ("value", "Time from alert to full surge capacity")]),
            row([("label", "Staff Response Time"), ("value", "Time for staff to report and assume roles")]),
            row([("label", "Communication Speed"), ("value", "Time for all notifications to be completed")]),
            row([("label", "Resource Deployment"), ("value", "Time to set up surge areas and equipment")]),
        ];
        for row in &drill_performance_metric_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }

        // And drill evaluation and improvement planning occurs:
        let evaluation_component_rows = vec![
            row([("label", "Protocol Effectiveness"), ("value", "How well procedures worked in practice")]),
            row([("label", "Staff Preparedness"), ("value", "Knowledge and skill gaps identified")]),
            row([("label", "System Performance"), ("value", "Technology and workflow efficiency")]),
            row([("label", "Improvement Plans"), ("value", "Action items for enhancing response capabilities")]),
        ];
        for row in &evaluation_component_rows {
            assert_eq!(get_text(driver, row["label"].as_str()).await?, row["value"].as_str());
        }
        Ok(())
    })
}

fn main() {
    run_feature(&[
        ("Activate mass casualty protocol for multi-vehicle accident", scenario_01_activate_mass_casualty_protocol_for_multi_vehicle_accident),
        ("Configure surge capacity with bed and resource expansion", scenario_02_configure_surge_capacity_with_bed_and_resource_expansion),
        ("Implement rapid patient registration and triage workflow", scenario_03_implement_rapid_patient_registration_and_triage_workflow),
        ("Coordinate with external emergency services and hospitals", scenario_04_coordinate_with_external_emergency_services_and_hospitals),
        ("Manage family reunification and information center", scenario_05_manage_family_reunification_and_information_center),
        ("Transition from mass casualty mode back to normal operations", scenario_06_transition_from_mass_casualty_mode_back_to_normal_operations),
        ("Handle mass casualty incident during shift change", scenario_07_handle_mass_casualty_incident_during_shift_change),
        ("Test mass casualty system readiness through drill", scenario_08_test_mass_casualty_system_readiness_through_drill),
    ]);
}
