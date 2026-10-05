// Playwright + libtest-mimic test for
// tests-with-given-when-then-features/20-code-blue-response.feature
// (equivalent to tests-with-playwright-javascript/20-code-blue-response.test.js).
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
    //   And the code blue alert system is active
    //   And the resuscitation documentation module is enabled
    //   And all display devices are connected to the alert network
    //   And the code team roster is current and available
    verify_system_is_operational(page).await?;
    login(page, "a code blue team leader", false).await?;
    // The remaining Background steps describe pre-seeded system state
    // (alert system, documentation module, display devices, and code team
    // roster readiness) assumed to already be configured in the test
    // environment.
    let feature_nav_link = wait_for_test_id(page, "Nav Code Blue Response").await?;
    feature_nav_link.click(None).await?;
    wait_for_test_id(page, "Code Blue Response Panel").await?;
    Ok(())
}

fn scenario_01_activate_code_blue_for_cardiac_arrest_in_bed_5(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given a patient "Robert Martinez" is in bed "ED-5"
        // And the patient is being monitored for chest pain
        // And I am "Nurse Johnson" providing direct patient care
        // When the patient suddenly becomes unresponsive and pulseless
        // And I immediately press the code blue button at bedside
        page.get_by_test_id("code-blue-button").first().click(None).await?;

        // Then the system instantly activates the code blue alert:
        //   | Alert Component       | Activation Details                         |
        //   | Alert Timestamp       | 14:35:22 - Precise time recorded          |
        //   | Location              | ED-5 clearly identified                   |
        //   | Initiating Staff      | Nurse Johnson                             |
        //   | Patient Identity      | Robert Martinez (if available)            |
        //   | Alert Type            | Code Blue - Cardiac Arrest                |
        let alert_activation_details = vec![
            row([("label", "Alert Timestamp"), ("value", "14:35:22 - Precise time recorded")]),
            row([("label", "Location"), ("value", "ED-5 clearly identified")]),
            row([("label", "Initiating Staff"), ("value", "Nurse Johnson")]),
            row([("label", "Patient Identity"), ("value", "Robert Martinez (if available)")]),
            row([("label", "Alert Type"), ("value", "Code Blue - Cardiac Arrest")]),
        ];
        for row_data in &alert_activation_details {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }

        // And the code team is immediately alerted through multiple channels:
        //   | Team Member           | Alert Method          | Expected Response Time |
        //   | Emergency Physician   | Overhead page, mobile | Immediate             |
        //   | Cardiologist         | Mobile alert, pager   | Within 2 minutes      |
        //   | Anesthesiologist     | Overhead page, mobile | Within 3 minutes      |
        //   | ICU Nurse            | Mobile alert, pager   | Within 2 minutes      |
        //   | Respiratory Therapist | Overhead page, mobile | Within 2 minutes      |
        //   | Pharmacist           | Mobile alert          | Within 3 minutes      |
        //   | Chaplain             | Silent alert          | Within 5 minutes      |
        let code_team_alerts = page.get_by_test_id("code-team-alert-entry");
        assert_eq!(code_team_alerts.count().await?, 7);

        // And patient location is displayed on all devices:
        //   | Display Location      | Information Shown                          |
        //   | ED Dashboard          | 🚨 CODE BLUE - BED ED-5 flashing red     |
        //   | Mobile Devices        | Push notification with location           |
        //   | Overhead Displays     | "CODE BLUE BED ED-5" prominently shown   |
        //   | Pager System          | "CODE BLUE ED-5" message                  |
        //   | Hospital Information  | Alert on all connected terminals          |
        let patient_location_displays = vec![
            row([("label", "ED Dashboard"), ("value", "🚨 CODE BLUE - BED ED-5 flashing red")]),
            row([("label", "Mobile Devices"), ("value", "Push notification with location")]),
            row([("label", "Overhead Displays"), ("value", "\"CODE BLUE BED ED-5\" prominently shown")]),
            row([("label", "Pager System"), ("value", "\"CODE BLUE ED-5\" message")]),
            row([("label", "Hospital Information"), ("value", "Alert on all connected terminals")]),
        ];
        for row_data in &patient_location_displays {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }

        // And the resuscitation documentation template opens automatically:
        //   | Documentation Section | Template Fields                            |
        //   | Event Details         | Time, location, discoverer, initial rhythm|
        //   | Timeline Tracker      | Medication times, defibrillation, procedures|
        //   | Team Members          | Roles and arrival times                    |
        //   | Vital Signs           | Real-time monitoring integration           |
        //   | Interventions         | CPR quality, airway management, IV access |
        let documentation_template_sections = vec![
            row([("label", "Event Details"), ("value", "Time, location, discoverer, initial rhythm")]),
            row([("label", "Timeline Tracker"), ("value", "Medication times, defibrillation, procedures")]),
            row([("label", "Team Members"), ("value", "Roles and arrival times")]),
            row([("label", "Vital Signs"), ("value", "Real-time monitoring integration")]),
            row([("label", "Interventions"), ("value", "CPR quality, airway management, IV access")]),
        ];
        for row_data in &documentation_template_sections {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }
        Ok(())
    })
}

fn scenario_02_code_blue_response_with_automatic_equipment_alerts(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given a code blue has been activated in bed "ED-5"
        // When the code blue alert is triggered

        // Then emergency equipment alerts are automatically generated:
        //   | Equipment Type        | Alert Message                              |
        //   | Crash Cart            | Crash cart dispatch to ED-5               |
        //   | Defibrillator        | AED/Manual defibrillator to ED-5          |
        //   | Airway Equipment     | Intubation kit and ventilator to ED-5     |
        //   | Emergency Medications | Code blue medication box to ED-5          |
        //   | IV Access Supplies   | Central line kit and fluids to ED-5       |
        let equipment_alerts = vec![
            row([("label", "Crash Cart"), ("value", "Crash cart dispatch to ED-5")]),
            row([("label", "Defibrillator"), ("value", "AED/Manual defibrillator to ED-5")]),
            row([("label", "Airway Equipment"), ("value", "Intubation kit and ventilator to ED-5")]),
            row([("label", "Emergency Medications"), ("value", "Code blue medication box to ED-5")]),
            row([("label", "IV Access Supplies"), ("value", "Central line kit and fluids to ED-5")]),
        ];
        for row_data in &equipment_alerts {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }

        // And equipment tracking is initiated:
        //   | Equipment Item        | Status Tracking                            |
        //   | Crash Cart Location   | GPS tracking to bed ED-5                  |
        //   | Defibrillator Readiness| Battery level and functionality check    |
        //   | Medication Expiration | Code blue drugs expiration verification   |
        //   | Equipment Arrival     | Timestamp when equipment reaches bedside  |
        let equipment_tracking = vec![
            row([("label", "Crash Cart Location"), ("value", "GPS tracking to bed ED-5")]),
            row([("label", "Defibrillator Readiness"), ("value", "Battery level and functionality check")]),
            row([("label", "Medication Expiration"), ("value", "Code blue drugs expiration verification")]),
            row([("label", "Equipment Arrival"), ("value", "Timestamp when equipment reaches bedside")]),
        ];
        for row_data in &equipment_tracking {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }

        // And backup equipment is automatically prepared:
        //   | Backup Equipment      | Preparation Action                         |
        //   | Secondary Crash Cart  | Made ready for potential second code       |
        //   | Additional Ventilator | Checked and moved closer to ED            |
        //   | Blood Bank Alert      | Emergency blood products prepared          |
        //   | OR Notification       | Operating room placed on standby          |
        let backup_equipment = vec![
            row([("label", "Secondary Crash Cart"), ("value", "Made ready for potential second code")]),
            row([("label", "Additional Ventilator"), ("value", "Checked and moved closer to ED")]),
            row([("label", "Blood Bank Alert"), ("value", "Emergency blood products prepared")]),
            row([("label", "OR Notification"), ("value", "Operating room placed on standby")]),
        ];
        for row_data in &backup_equipment {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }
        Ok(())
    })
}

fn scenario_03_real_time_code_blue_documentation_during_resuscitation(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given a code blue is in progress in bed "ED-5"
        // And the resuscitation documentation template is open
        // And "Dr. Smith" is the code team leader
        // When resuscitation interventions are performed

        // Then real-time documentation captures all activities:
        //   | Intervention Type     | Documentation Fields                       |
        //   | CPR Administration    | Start time, compression quality, provider  |
        //   | Medication Given      | Drug name, dose, route, time, provider     |
        //   | Defibrillation       | Joules delivered, rhythm before/after     |
        //   | Airway Management    | Type of airway, success, provider         |
        //   | IV Access            | Location, size, number of attempts        |
        let documented_activities = vec![
            row([("label", "CPR Administration"), ("value", "Start time, compression quality, provider")]),
            row([("label", "Medication Given"), ("value", "Drug name, dose, route, time, provider")]),
            row([("label", "Defibrillation"), ("value", "Joules delivered, rhythm before/after")]),
            row([("label", "Airway Management"), ("value", "Type of airway, success, provider")]),
            row([("label", "IV Access"), ("value", "Location, size, number of attempts")]),
        ];
        for row_data in &documented_activities {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }

        // And timeline tracking maintains precise chronology:
        //   | Timeline Entry        | Automatic Capture                          |
        //   | Event Start           | 14:35:22 - Code blue activated            |
        //   | CPR Initiated         | 14:35:45 - CPR started by Nurse Johnson   |
        //   | Team Leader Arrival   | 14:36:15 - Dr. Smith assumes leadership   |
        //   | First Medication      | 14:37:30 - Epinephrine 1mg IV push       |
        //   | Defibrillation       | 14:38:45 - 200J biphasic shock delivered  |
        let timeline_entries = vec![
            row([("label", "Event Start"), ("value", "14:35:22 - Code blue activated")]),
            row([("label", "CPR Initiated"), ("value", "14:35:45 - CPR started by Nurse Johnson")]),
            row([("label", "Team Leader Arrival"), ("value", "14:36:15 - Dr. Smith assumes leadership")]),
            row([("label", "First Medication"), ("value", "14:37:30 - Epinephrine 1mg IV push")]),
            row([("label", "Defibrillation Timeline"), ("value", "14:38:45 - 200J biphasic shock delivered")]),
        ];
        for row_data in &timeline_entries {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }

        // And quality metrics are tracked in real-time:
        //   | Quality Metric        | Real-time Monitoring                       |
        //   | Compression Depth     | CPR feedback device integration           |
        //   | Compression Rate      | Metronome guidance and measurement        |
        //   | No-flow Time         | Automatic calculation of interruptions    |
        //   | Medication Timing     | Alert for time-critical drug intervals   |
        let quality_metrics = vec![
            row([("label", "Compression Depth"), ("value", "CPR feedback device integration")]),
            row([("label", "Compression Rate"), ("value", "Metronome guidance and measurement")]),
            row([("label", "No-flow Time"), ("value", "Automatic calculation of interruptions")]),
            row([("label", "Medication Timing"), ("value", "Alert for time-critical drug intervals")]),
        ];
        for row_data in &quality_metrics {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }
        Ok(())
    })
}

fn scenario_04_code_blue_with_return_of_spontaneous_circulation_rosc(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given a code blue has been in progress for 8 minutes
        // And resuscitation efforts are ongoing with documentation active
        // When the patient achieves return of spontaneous circulation (ROSC)
        // And "Dr. Smith" confirms pulse and blood pressure of 110/70

        // Then the system updates the code status:
        //   | Status Update         | Documentation Changes                      |
        //   | ROSC Achievement      | Time: 14:43:15 - ROSC achieved            |
        //   | Vital Signs          | BP: 110/70, HR: 85, documented           |
        //   | Rhythm Change        | Normal sinus rhythm confirmed             |
        //   | Intervention Pause   | CPR discontinued, monitoring intensified  |
        let code_status_updates = vec![
            row([("label", "ROSC Achievement"), ("value", "Time: 14:43:15 - ROSC achieved")]),
            row([("label", "Post-ROSC Vital Signs"), ("value", "BP: 110/70, HR: 85, documented")]),
            row([("label", "Rhythm Change"), ("value", "Normal sinus rhythm confirmed")]),
            row([("label", "Intervention Pause"), ("value", "CPR discontinued, monitoring intensified")]),
        ];
        for row_data in &code_status_updates {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }

        // And post-ROSC care protocols are activated:
        //   | Post-ROSC Protocol    | Automated Alerts                           |
        //   | ICU Transfer          | ICU bed request and transport coordination |
        //   | Cardiology Consult    | Urgent cardiology evaluation requested     |
        //   | Temperature Management| Therapeutic hypothermia consideration      |
        //   | Neurological Assessment| Baseline neuro checks ordered             |
        let post_rosc_protocols = vec![
            row([("label", "ICU Transfer"), ("value", "ICU bed request and transport coordination")]),
            row([("label", "Cardiology Consult"), ("value", "Urgent cardiology evaluation requested")]),
            row([("label", "Temperature Management"), ("value", "Therapeutic hypothermia consideration")]),
            row([("label", "Neurological Assessment"), ("value", "Baseline neuro checks ordered")]),
        ];
        for row_data in &post_rosc_protocols {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }

        // And family notification procedures are initiated:
        //   | Family Communication  | Process                                    |
        //   | Contact Attempt       | Emergency contact called by social worker  |
        //   | Status Update         | "Patient being treated, stable condition" |
        //   | Visitation Arrangement| Family arrival and bedside visit coordination|
        //   | Chaplain Services     | Spiritual care offered to family          |
        let family_notification_procedures = vec![
            row([("label", "Contact Attempt"), ("value", "Emergency contact called by social worker")]),
            row([("label", "Status Update"), ("value", "\"Patient being treated, stable condition\"")]),
            row([("label", "Visitation Arrangement"), ("value", "Family arrival and bedside visit coordination")]),
            row([("label", "Chaplain Services"), ("value", "Spiritual care offered to family")]),
        ];
        for row_data in &family_notification_procedures {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }
        Ok(())
    })
}

fn scenario_05_unsuccessful_code_blue_with_transition_to_end_of_life_care(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given a code blue has been in progress for 25 minutes
        // And multiple rounds of medications and defibrillation have been attempted
        // And no return of spontaneous circulation has been achieved
        // When "Dr. Smith" as code team leader determines resuscitation efforts should cease
        // And the time of death is called at 15:00:15

        // Then the system handles end-of-life documentation:
        //   | End-of-Life Process   | Documentation Requirements                 |
        //   | Time of Death         | 15:00:15 - Officially recorded           |
        //   | Resuscitation Duration| 24 minutes 53 seconds total time         |
        //   | Interventions Summary | Complete list of all attempted treatments |
        //   | Team Members Present  | All providers involved in resuscitation   |
        let end_of_life_documentation = vec![
            row([("label", "Time of Death"), ("value", "15:00:15 - Officially recorded")]),
            row([("label", "Resuscitation Duration"), ("value", "24 minutes 53 seconds total time")]),
            row([("label", "Interventions Summary"), ("value", "Complete list of all attempted treatments")]),
            row([("label", "Team Members Present"), ("value", "All providers involved in resuscitation")]),
        ];
        for row_data in &end_of_life_documentation {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }

        // And family notification and support procedures are activated:
        //   | Family Support        | Coordinated Response                       |
        //   | Immediate Contact     | Personal notification by physician        |
        //   | Bereavement Support   | Chaplain and social worker assigned       |
        //   | Viewing Arrangement   | Private room prepared for family viewing   |
        //   | Organ Donation        | Coordinator contacted per protocol        |
        let family_support_procedures = vec![
            row([("label", "Immediate Contact"), ("value", "Personal notification by physician")]),
            row([("label", "Bereavement Support"), ("value", "Chaplain and social worker assigned")]),
            row([("label", "Viewing Arrangement"), ("value", "Private room prepared for family viewing")]),
            row([("label", "Organ Donation"), ("value", "Coordinator contacted per protocol")]),
        ];
        for row_data in &family_support_procedures {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }

        // And administrative processes are initiated:
        //   | Administrative Task   | Required Actions                           |
        //   | Medical Examiner      | Contact if death meets criteria           |
        //   | Autopsy Consent       | Family discussion and documentation       |
        //   | Death Certificate     | Physician completion requirements         |
        //   | Quality Review        | Case review scheduled within 24 hours     |
        let administrative_processes = vec![
            row([("label", "Medical Examiner"), ("value", "Contact if death meets criteria")]),
            row([("label", "Autopsy Consent"), ("value", "Family discussion and documentation")]),
            row([("label", "Death Certificate"), ("value", "Physician completion requirements")]),
            row([("label", "Quality Review"), ("value", "Case review scheduled within 24 hours")]),
        ];
        for row_data in &administrative_processes {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }
        Ok(())
    })
}

fn scenario_06_code_blue_during_visitor_hours_with_family_present(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given it is 19:30 during evening visitor hours
        // And the patient's family members are at bedside when cardiac arrest occurs
        // When the code blue is activated

        // Then family management protocols are immediately implemented:
        //   | Family Management     | Immediate Actions                          |
        //   | Family Escort         | Security escorts family to private area    |
        //   | Communication         | Social worker provides immediate support   |
        //   | Information Updates   | Regular updates provided during resuscitation|
        //   | Chaplain Services     | Spiritual care offered immediately         |
        let family_management_protocols = vec![
            row([("label", "Family Escort"), ("value", "Security escorts family to private area")]),
            row([("label", "Communication"), ("value", "Social worker provides immediate support")]),
            row([("label", "Information Updates"), ("value", "Regular updates provided during resuscitation")]),
            row([("label", "Chaplain Services Immediate Support"), ("value", "Spiritual care offered immediately")]),
        ];
        for row_data in &family_management_protocols {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }

        // And visitor area management is coordinated:
        //   | Visitor Control       | Safety Measures                            |
        //   | Area Clearance        | Non-family visitors moved from immediate area|
        //   | Privacy Protection    | Screens and barriers deployed             |
        //   | Crowd Control         | Security manages visitor flow             |
        //   | Other Patient Care    | Continued care for nearby patients        |
        let visitor_area_management = vec![
            row([("label", "Area Clearance"), ("value", "Non-family visitors moved from immediate area")]),
            row([("label", "Privacy Protection"), ("value", "Screens and barriers deployed")]),
            row([("label", "Crowd Control"), ("value", "Security manages visitor flow")]),
            row([("label", "Other Patient Care"), ("value", "Continued care for nearby patients")]),
        ];
        for row_data in &visitor_area_management {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }

        // And family preference accommodation occurs:
        //   | Family Preference     | Options Provided                           |
        //   | Bedside Presence      | Option to remain during resuscitation     |
        //   | Waiting Area          | Comfortable private space with updates    |
        //   | Family Spokesperson   | Designated family member for communication |
        //   | Support Person        | Additional family/friend notification     |
        let family_preference_accommodation = vec![
            row([("label", "Bedside Presence"), ("value", "Option to remain during resuscitation")]),
            row([("label", "Waiting Area"), ("value", "Comfortable private space with updates")]),
            row([("label", "Family Spokesperson"), ("value", "Designated family member for communication")]),
            row([("label", "Support Person"), ("value", "Additional family/friend notification")]),
        ];
        for row_data in &family_preference_accommodation {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }
        Ok(())
    })
}

fn scenario_07_code_blue_team_performance_metrics_and_quality_improvement(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given a code blue event has been completed
        // And all documentation has been finalized
        // When the quality review process is initiated

        // Then performance metrics are automatically calculated:
        //   | Performance Metric    | Measurement                                |
        //   | Response Time         | 1 minute 23 seconds from alert to arrival |
        //   | No-flow Time          | 15 seconds total interruption time        |
        //   | First Shock Time      | 3 minutes 45 seconds from arrest          |
        //   | Medication Timing     | All drugs given within target windows     |
        //   | Team Coordination     | Communication effectiveness score          |
        let performance_metrics = vec![
            row([("label", "Response Time"), ("value", "1 minute 23 seconds from alert to arrival")]),
            row([("label", "No-flow Time Performance"), ("value", "15 seconds total interruption time")]),
            row([("label", "First Shock Time"), ("value", "3 minutes 45 seconds from arrest")]),
            row([("label", "Medication Timing Compliance"), ("value", "All drugs given within target windows")]),
            row([("label", "Team Coordination"), ("value", "Communication effectiveness score")]),
        ];
        for row_data in &performance_metrics {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }

        // And quality improvement data is captured:
        //   | QI Data Element       | Assessment                                 |
        //   | Protocol Adherence    | 95% compliance with ACLS guidelines       |
        //   | Equipment Function    | All equipment functioned properly         |
        //   | Team Performance      | Effective leadership and role clarity     |
        //   | Communication Quality | Clear, concise, and timely communication  |
        let qi_data_elements = vec![
            row([("label", "Protocol Adherence"), ("value", "95% compliance with ACLS guidelines")]),
            row([("label", "Equipment Function"), ("value", "All equipment functioned properly")]),
            row([("label", "Team Performance"), ("value", "Effective leadership and role clarity")]),
            row([("label", "Communication Quality"), ("value", "Clear, concise, and timely communication")]),
        ];
        for row_data in &qi_data_elements {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }

        // And improvement opportunities are identified:
        //   | Improvement Area      | Recommendation                             |
        //   | Response Time         | Consider additional code cart placement    |
        //   | Team Training         | Schedule quarterly simulation training     |
        //   | Equipment Maintenance | Review defibrillator calibration schedule |
        //   | Documentation         | Streamline real-time entry process        |
        let improvement_opportunities = vec![
            row([("label", "Response Time Recommendation"), ("value", "Consider additional code cart placement")]),
            row([("label", "Team Training"), ("value", "Schedule quarterly simulation training")]),
            row([("label", "Equipment Maintenance"), ("value", "Review defibrillator calibration schedule")]),
            row([("label", "Documentation"), ("value", "Streamline real-time entry process")]),
        ];
        for row_data in &improvement_opportunities {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }
        Ok(())
    })
}

fn scenario_08_code_blue_false_alarm_with_appropriate_system_response(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given a code blue alert has been activated in bed "ED-5"
        // And the code team is responding
        // When it is determined that the patient is conscious and stable
        // And the alert was triggered accidentally by equipment malfunction

        // Then the false alarm protocol is activated:
        //   | False Alarm Response  | Actions Taken                              |
        //   | Alert Cancellation    | "Code blue canceled - false alarm" announcement|
        //   | Team Stand-down       | Code team notified to return to normal duties|
        //   | Equipment Check       | Investigate and repair malfunctioning equipment|
        //   | Documentation         | Document false alarm and cause            |
        let false_alarm_protocol = vec![
            row([("label", "Alert Cancellation"), ("value", "\"Code blue canceled - false alarm\" announcement")]),
            row([("label", "Team Stand-down"), ("value", "Code team notified to return to normal duties")]),
            row([("label", "Equipment Check"), ("value", "Investigate and repair malfunctioning equipment")]),
            row([("label", "False Alarm Documentation"), ("value", "Document false alarm and cause")]),
        ];
        for row_data in &false_alarm_protocol {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }

        // And system improvements are implemented:
        //   | Improvement Action    | Preventive Measures                        |
        //   | Equipment Maintenance | Immediate repair of faulty equipment       |
        //   | Staff Education       | Review proper code blue activation         |
        //   | System Calibration    | Adjust sensitivity to prevent false alarms |
        //   | Audit Trail          | Record incident for system improvement     |
        let system_improvements = vec![
            row([("label", "Equipment Maintenance Repair"), ("value", "Immediate repair of faulty equipment")]),
            row([("label", "Staff Education"), ("value", "Review proper code blue activation")]),
            row([("label", "System Calibration"), ("value", "Adjust sensitivity to prevent false alarms")]),
            row([("label", "Audit Trail"), ("value", "Record incident for system improvement")]),
        ];
        for row_data in &system_improvements {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }

        // And normal operations resume with lessons learned integrated into protocols
        assert_match(&get_text(page, "System Operations Status").await?, r"normal", true);
        Ok(())
    })
}

fn main() {
    run_feature(&[
        ("Activate code blue for cardiac arrest in bed 5", scenario_01_activate_code_blue_for_cardiac_arrest_in_bed_5),
        ("Code blue response with automatic equipment alerts", scenario_02_code_blue_response_with_automatic_equipment_alerts),
        ("Real-time code blue documentation during resuscitation", scenario_03_real_time_code_blue_documentation_during_resuscitation),
        ("Code blue with return of spontaneous circulation (ROSC)", scenario_04_code_blue_with_return_of_spontaneous_circulation_rosc),
        ("Unsuccessful code blue with transition to end-of-life care", scenario_05_unsuccessful_code_blue_with_transition_to_end_of_life_care),
        ("Code blue during visitor hours with family present", scenario_06_code_blue_during_visitor_hours_with_family_present),
        ("Code blue team performance metrics and quality improvement", scenario_07_code_blue_team_performance_metrics_and_quality_improvement),
        ("Code blue false alarm with appropriate system response", scenario_08_code_blue_false_alarm_with_appropriate_system_response),
    ]);
}
