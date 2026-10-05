// Selenium WebDriver + libtest-mimic test for
// tests-with-given-when-then-features/16-discharge-follow-up.feature
// (equivalent to tests-with-selenium-javascript/16-discharge-follow-up.test.js).
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
    //   And the patient portal is connected and functional
    //   And the follow-up scheduling module is active
    //   And automated reminder systems are enabled
    //   And patient communication preferences are configured
    verify_system_is_operational(driver).await?;
    login(driver, "a discharge coordinator", false).await?;
    // The patient portal connection, follow-up scheduling module,
    // automated reminder systems, and communication preference
    // configuration are assumed to be active backend configuration
    // already in place for this environment.

    let discharge_follow_up_nav_link = wait_for_test_id(driver, "Nav Discharge Follow-up").await?;
    discharge_follow_up_nav_link.click().await?;
    wait_for_test_id(driver, "Discharge Follow-up Panel").await?;
    Ok(())
}

fn scenario_01_automatically_schedule_follow_up_reminder_for_primary_care(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a patient "Jennifer Martinez" has been discharged from bed "ED-8"
        // And the discharge orders include:
        //   | Follow-up Requirement | Details                                    |
        //   | Primary Care Visit    | Schedule within 3-5 days                  |
        //   | Reason for Follow-up  | UTI treatment response, medication review  |
        //   | Urgency Level         | Routine                                    |
        //   | Special Instructions  | Bring discharge paperwork and medication list |
        // And the patient has a registered primary care physician "Dr. Sarah Wilson"
        // (assumed pre-seeded test data)
        // When the discharge process is completed at 14:30
        // (assumed to have already occurred / triggered by the system)

        // Then the system automatically schedules a follow-up reminder:
        wait_for_test_id(driver, "Initial Reminder").await?;
        let follow_up_reminders = vec![
            row([("Type", "Initial Reminder"), ("Details", "Day 2 after discharge (in 48 hours)")]),
            row([("Type", "Follow-up Reminder"), ("Details", "Day 4 after discharge if no appointment")]),
            row([("Type", "Final Reminder"), ("Details", "Day 6 after discharge (urgent)")]),
            row([("Type", "Reminder Methods"), ("Details", "Text, email, phone call")]),
        ];
        for row_data in &follow_up_reminders {
            let type_ = row_data["Type"].as_str();
            let details = row_data["Details"].as_str();
            assert_eq!(get_text(driver, type_).await?, details);
        }

        // And discharge instructions are automatically sent to the patient portal:
        let portal_content = vec![
            row([("Section", "Discharge Summary"), ("Content", "Complete treatment summary and diagnosis")]),
            row([("Section", "Medication Instructions"), ("Content", "Prescription details and dosing schedule")]),
            row([("Section", "Follow-up Requirements"), ("Content", "Primary care appointment needed in 3-5 days")]),
            row([("Section", "Return Precautions"), ("Content", "When to seek emergency care")]),
            row([("Section", "Care Instructions"), ("Content", "Home care guidelines and activity restrictions")]),
        ];
        for row_data in &portal_content {
            let section = row_data["Section"].as_str();
            let content = row_data["Content"].as_str();
            assert_eq!(get_text(driver, section).await?, content);
        }

        // And the patient receives immediate portal notification:
        let portal_notifications = vec![
            row([("Type", "Portal Alert"), ("Content", "📋 New discharge instructions available")]),
            row([("Type", "Text Message"), ("Content", "\"ED discharge complete. Check patient portal for instructions\"")]),
            row([("Type", "Email Notification"), ("Content", "Detailed discharge summary with portal link")]),
        ];
        for row_data in &portal_notifications {
            let type_ = row_data["Type"].as_str();
            let content = row_data["Content"].as_str();
            assert_eq!(get_text(driver, type_).await?, content);
        }
        Ok(())
    })
}

fn scenario_02_handle_follow_up_scheduling_with_multiple_appointment_types(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a patient "Robert Chen" is discharged with complex follow-up needs
        // And the discharge orders specify:
        //   | Follow-up Type        | Timeframe | Provider Type     | Priority  |
        //   | Primary Care         | 3 days    | Family Medicine   | High      |
        //   | Cardiology Consult   | 1 week    | Cardiologist      | Urgent    |
        //   | Lab Work Follow-up   | 5 days    | Lab/Primary Care  | Routine   |
        //   | Physical Therapy     | 2 weeks   | PT Specialist     | Routine   |
        // (assumed pre-seeded test data)
        // When the discharge process is completed
        // (assumed to have already occurred / triggered by the system)

        // Then the system creates multiple follow-up reminders:
        wait_for_test_id(driver, "Primary Care Timing").await?;
        let reminder_schedule = vec![
            row([("Type", "Primary Care"), ("Timing", "Schedule within 2 days")]),
            row([("Type", "Cardiology"), ("Timing", "Schedule urgent consult")]),
            row([("Type", "Lab Work"), ("Timing", "Schedule blood draw")]),
            row([("Type", "Physical Therapy"), ("Timing", "Schedule PT evaluation")]),
        ];
        for row_data in &reminder_schedule {
            let type_ = row_data["Type"].as_str();
            let timing = row_data["Timing"].as_str();
            assert_eq!(get_text(driver, &format!("{} Timing", type_)).await?, timing);
        }

        // And the patient portal receives comprehensive follow-up information:
        let portal_follow_up_info = vec![
            row([("Section", "Appointment Dashboard"), ("Content", "All required follow-ups with deadlines")]),
            row([("Section", "Provider Contacts"), ("Content", "Phone numbers and scheduling information")]),
            row([("Section", "Priority Indicators"), ("Content", "Urgent vs routine appointment labeling")]),
            row([("Section", "Preparation Instructions"), ("Content", "What to bring to each appointment")]),
        ];
        for row_data in &portal_follow_up_info {
            let section = row_data["Section"].as_str();
            let content = row_data["Content"].as_str();
            assert_eq!(get_text(driver, section).await?, content);
        }

        // And automated referrals are generated:
        let automated_referrals = vec![
            row([("Type", "Electronic Referral"), ("Action", "Sent to cardiology for urgent consult")]),
            row([("Type", "Lab Order"), ("Action", "Standing orders for follow-up labs")]),
            row([("Type", "PT Referral"), ("Action", "Physical therapy evaluation requested")]),
        ];
        for row_data in &automated_referrals {
            let type_ = row_data["Type"].as_str();
            let action = row_data["Action"].as_str();
            assert_eq!(get_text(driver, type_).await?, action);
        }
        Ok(())
    })
}

fn scenario_03_send_discharge_instructions_to_patient_portal_with_multimedi(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a patient "Maria Santos" was treated for "wound care management"
        // And the patient requires detailed home care instructions
        // (assumed pre-seeded test data)
        // When the discharge process includes educational materials:
        //   | Education Type        | Content Provided                           |
        //   | Wound Care Video      | Step-by-step dressing change demonstration |
        //   | Medication Guide      | Interactive dosing calculator              |
        //   | Warning Signs Chart   | Visual guide for infection symptoms        |
        //   | Activity Guidelines   | Illustrated movement restrictions          |
        // And the discharge is completed
        // (assumed to have already occurred / triggered by the system)

        // Then the patient portal receives multimedia instructions:
        wait_for_test_id(driver, "Video Instructions").await?;
        let multimedia_instructions = vec![
            row([("Type", "Video Instructions"), ("Material", "Wound care demonstration (3 minutes)")]),
            row([("Type", "Interactive Tools"), ("Material", "Medication reminder scheduler")]),
            row([("Type", "Visual Guides"), ("Material", "Infection warning signs with photos")]),
            row([("Type", "Progress Tracking"), ("Material", "Healing milestone checklist")]),
        ];
        for row_data in &multimedia_instructions {
            let type_ = row_data["Type"].as_str();
            let material = row_data["Material"].as_str();
            assert_eq!(get_text(driver, type_).await?, material);
        }

        // And the patient receives learning verification:
        let learning_verification = vec![
            row([("Method", "Video Completion"), ("Requirement", "Must watch wound care video fully")]),
            row([("Method", "Knowledge Check"), ("Requirement", "Brief quiz on warning signs")]),
            row([("Method", "Acknowledgment"), ("Requirement", "Confirm understanding of instructions")]),
        ];
        for row_data in &learning_verification {
            let method = row_data["Method"].as_str();
            let requirement = row_data["Requirement"].as_str();
            assert_eq!(get_text(driver, method).await?, requirement);
        }

        // And completion tracking is recorded for quality assurance
        let completion_tracking_status = get_text(driver, "Completion Tracking Status").await?;
        assert_match(&completion_tracking_status, r"recorded", true);
        Ok(())
    })
}

fn scenario_04_handle_follow_up_reminders_for_patients_without_primary_care(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a patient "David Kim" is discharged
        // And the patient does not have an established primary care physician
        // And follow-up care is required within 5 days
        // (assumed pre-seeded test data)
        // When the discharge process is completed
        // (assumed to have already occurred / triggered by the system)

        // Then the system provides alternative follow-up options:
        wait_for_test_id(driver, "Urgent Care Centers").await?;
        let follow_up_options = vec![
            row([("Option", "Urgent Care Centers"), ("Details", "List of nearby facilities with hours")]),
            row([("Option", "Hospital Clinic"), ("Details", "Available appointment slots")]),
            row([("Option", "Telehealth Options"), ("Details", "Virtual visit scheduling information")]),
            row([("Option", "Community Health Centers"), ("Details", "Low-cost provider options")]),
        ];
        for row_data in &follow_up_options {
            let option = row_data["Option"].as_str();
            let details = row_data["Details"].as_str();
            assert_eq!(get_text(driver, option).await?, details);
        }

        // And enhanced reminder scheduling is activated:
        let enhanced_reminders = vec![
            row([("Type", "Daily Reminders"), ("Frequency", "For first 3 days after discharge")]),
            row([("Type", "Resource Assistance"), ("Frequency", "Links to find primary care providers")]),
            row([("Type", "Financial Counseling"), ("Frequency", "Information about insurance and payment")]),
        ];
        for row_data in &enhanced_reminders {
            let type_ = row_data["Type"].as_str();
            let frequency = row_data["Frequency"].as_str();
            assert_eq!(get_text(driver, type_).await?, frequency);
        }

        // And the patient portal includes provider finding tools:
        let provider_finding_tools = vec![
            row([("Tool", "Provider Search"), ("Functionality", "Find doctors accepting new patients")]),
            row([("Tool", "Insurance Verification"), ("Functionality", "Check coverage for potential providers")]),
            row([("Tool", "Appointment Booking"), ("Functionality", "Direct scheduling with available providers")]),
        ];
        for row_data in &provider_finding_tools {
            let tool = row_data["Tool"].as_str();
            let functionality = row_data["Functionality"].as_str();
            assert_eq!(get_text(driver, tool).await?, functionality);
        }
        Ok(())
    })
}

fn scenario_05_customize_follow_up_based_on_patient_communication_preferenc(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a patient "Lisa Brown" has specified communication preferences:
        //   | Communication Method  | Preference    | Contact Information        |
        //   | Text Messages         | Preferred     | 555-123-4567              |
        //   | Email                 | Secondary     | lisa.brown@email.com      |
        //   | Phone Calls           | Emergency Only| 555-123-4567              |
        //   | Portal Notifications  | Enabled       | Username: lbrown123       |
        // And the patient is discharged with routine follow-up requirements
        // (assumed pre-seeded test data)
        // When the discharge process triggers follow-up communications
        // (assumed to have already occurred / triggered by the system)

        // Then the system respects patient communication preferences:
        wait_for_test_id(driver, "Initial Instructions Method").await?;
        let communication_preferences = vec![
            row([("Type", "Initial Instructions"), ("Method", "Text + Portal"), ("Content", "Brief summary with portal link")]),
            row([("Type", "Follow-up Reminders"), ("Method", "Text Message"), ("Content", "Appointment reminders")]),
            row([("Type", "Urgent Notifications"), ("Method", "Phone Call"), ("Content", "Critical lab results only")]),
            row([("Type", "Educational Content"), ("Method", "Portal Only"), ("Content", "Detailed instructions and videos")]),
        ];
        for row_data in &communication_preferences {
            let type_ = row_data["Type"].as_str();
            let method = row_data["Method"].as_str();
            let content = row_data["Content"].as_str();
            assert_eq!(get_text(driver, &format!("{} Method", type_)).await?, method);
            assert_eq!(get_text(driver, &format!("{} Content", type_)).await?, content);
        }

        // And communication tracking records patient engagement:
        let communication_tracking = vec![
            row([("Metric", "Message Delivery"), ("Measurement", "Successful text delivery confirmed")]),
            row([("Metric", "Portal Access"), ("Measurement", "Login timestamps and content viewed")]),
            row([("Metric", "Engagement Level"), ("Measurement", "Time spent reviewing instructions")]),
        ];
        for row_data in &communication_tracking {
            let metric = row_data["Metric"].as_str();
            let measurement = row_data["Measurement"].as_str();
            assert_eq!(get_text(driver, metric).await?, measurement);
        }
        Ok(())
    })
}

fn scenario_06_handle_follow_up_for_pediatric_patients_with_parent_guardian(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a pediatric patient "Emma Foster" (age 6) is discharged
        // And the parent "Sarah Foster" is the primary contact
        // And follow-up includes pediatric-specific requirements:
        //   | Follow-up Type        | Pediatric Considerations                   |
        //   | Pediatrician Visit    | Growth and development check               |
        //   | Vaccination Updates   | Catch-up on missed immunizations          |
        //   | School Health Forms   | Medical clearance for return to school     |
        // (assumed pre-seeded test data)
        // When the discharge process is completed
        // (assumed to have already occurred / triggered by the system)

        // Then the system creates parent-focused follow-up communications:
        wait_for_test_id(driver, "Parent Portal Account").await?;
        let parent_communications = vec![
            row([("Target", "Parent Portal Account"), ("Type", "Child's medical summary and instructions")]),
            row([("Target", "School Notifications"), ("Type", "Medical excuse and return guidelines")]),
            row([("Target", "Pediatrician Alert"), ("Type", "ED visit summary and follow-up needs")]),
        ];
        for row_data in &parent_communications {
            let target = row_data["Target"].as_str();
            let type_ = row_data["Type"].as_str();
            assert_eq!(get_text(driver, target).await?, type_);
        }

        // And pediatric-specific reminders are scheduled:
        let pediatric_reminders = vec![
            row([("Type", "Medication Reminders"), ("Instructions", "Weight-based dosing with schedule")]),
            row([("Type", "Development Milestones"), ("Instructions", "Age-appropriate recovery expectations")]),
            row([("Type", "School Return Criteria"), ("Instructions", "When child can safely return to activities")]),
        ];
        for row_data in &pediatric_reminders {
            let type_ = row_data["Type"].as_str();
            let instructions = row_data["Instructions"].as_str();
            assert_eq!(get_text(driver, type_).await?, instructions);
        }

        // And child safety verification is included:
        let child_safety_checks = vec![
            row([("Check", "Home Safety Assessment"), ("Requirement", "Childproofing for medication storage")]),
            row([("Check", "Caregiver Instructions"), ("Requirement", "Multiple caregivers receive instructions")]),
            row([("Check", "Emergency Contacts"), ("Requirement", "Updated emergency contact information")]),
        ];
        for row_data in &child_safety_checks {
            let check = row_data["Check"].as_str();
            let requirement = row_data["Requirement"].as_str();
            assert_eq!(get_text(driver, check).await?, requirement);
        }
        Ok(())
    })
}

fn scenario_07_track_follow_up_compliance_and_patient_outcomes(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given multiple patients have been discharged with follow-up requirements
        // (assumed pre-seeded test data)
        // When follow-up reminders are sent and appointments are scheduled
        // (assumed to have already occurred / triggered by the system)

        // Then the system tracks compliance metrics:
        wait_for_test_id(driver, "Appointment Scheduling").await?;
        let compliance_metrics = vec![
            row([("Metric", "Appointment Scheduling"), ("Measurement", "% of patients who schedule within timeframe")]),
            row([("Metric", "Appointment Attendance"), ("Measurement", "% of scheduled appointments kept")]),
            row([("Metric", "Portal Engagement"), ("Measurement", "% of patients accessing discharge instructions")]),
            row([("Metric", "Medication Compliance"), ("Measurement", "% following prescription instructions")]),
        ];
        for row_data in &compliance_metrics {
            let metric = row_data["Metric"].as_str();
            let measurement = row_data["Measurement"].as_str();
            assert_eq!(get_text(driver, metric).await?, measurement);
        }

        // And outcome tracking is performed:
        let outcome_tracking = vec![
            row([("Metric", "ED Readmissions"), ("Method", "72-hour and 30-day return rates")]),
            row([("Metric", "Complication Rates"), ("Method", "Follow-up visits for related issues")]),
            row([("Metric", "Patient Satisfaction"), ("Method", "Follow-up surveys about discharge process")]),
        ];
        for row_data in &outcome_tracking {
            let metric = row_data["Metric"].as_str();
            let method = row_data["Method"].as_str();
            assert_eq!(get_text(driver, metric).await?, method);
        }

        // And quality improvement reports are generated:
        let quality_improvement_reports = vec![
            row([("Report", "Follow-up Effectiveness"), ("Content", "Success rates by discharge diagnosis")]),
            row([("Report", "Communication Analysis"), ("Content", "Best-performing reminder methods")]),
            row([("Report", "Provider Performance"), ("Content", "Follow-up compliance by discharging physician")]),
        ];
        for row_data in &quality_improvement_reports {
            let report = row_data["Report"].as_str();
            let content = row_data["Content"].as_str();
            assert_eq!(get_text(driver, report).await?, content);
        }
        Ok(())
    })
}

fn scenario_08_handle_follow_up_complications_and_escalation_procedures(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a patient "Michael Davis" was discharged 2 days ago
        // And follow-up reminders have been sent
        // (assumed pre-seeded test data)
        // When the patient contacts the ED with worsening symptoms
        // And the patient has not yet scheduled the required follow-up appointment
        // (assumed to have already occurred / triggered by the system)

        // Then the system escalates the follow-up process:
        wait_for_test_id(driver, "Urgent Scheduling").await?;
        let escalation_actions = vec![
            row([("Action", "Urgent Scheduling"), ("Details", "Same-day appointment coordination")]),
            row([("Action", "Provider Notification"), ("Details", "Original discharging physician alerted")]),
            row([("Action", "Symptom Assessment"), ("Details", "Nurse triage for immediate vs delayed care")]),
            row([("Action", "Documentation Update"), ("Details", "Patient contact and status change recorded")]),
        ];
        for row_data in &escalation_actions {
            let action = row_data["Action"].as_str();
            let details = row_data["Details"].as_str();
            assert_eq!(get_text(driver, action).await?, details);
        }

        // And enhanced monitoring is activated:
        let enhanced_monitoring = vec![
            row([("Type", "Daily Check-ins"), ("Action", "Nurse calls patient for status updates")]),
            row([("Type", "Expedited Referrals"), ("Action", "Fast-track specialist appointments")]),
            row([("Type", "Safety Net Activation"), ("Action", "Ensure patient has immediate care access")]),
        ];
        for row_data in &enhanced_monitoring {
            let type_ = row_data["Type"].as_str();
            let action = row_data["Action"].as_str();
            assert_eq!(get_text(driver, type_).await?, action);
        }

        // And the care team receives comprehensive updates:
        let care_team_updates = vec![
            row([("Member", "Discharging Physician"), ("Information", "Patient contact and current status")]),
            row([("Member", "Primary Care Provider"), ("Information", "Urgent need for appointment")]),
            row([("Member", "Charge Nurse"), ("Information", "Potential readmission risk identified")]),
        ];
        for row_data in &care_team_updates {
            let member = row_data["Member"].as_str();
            let information = row_data["Information"].as_str();
            assert_eq!(get_text(driver, member).await?, information);
        }
        Ok(())
    })
}

fn main() {
    run_feature(&[
        ("Automatically schedule follow-up reminder for primary care", scenario_01_automatically_schedule_follow_up_reminder_for_primary_care),
        ("Handle follow-up scheduling with multiple appointment types", scenario_02_handle_follow_up_scheduling_with_multiple_appointment_types),
        ("Send discharge instructions to patient portal with multimedia content", scenario_03_send_discharge_instructions_to_patient_portal_with_multimedi),
        ("Handle follow-up reminders for patients without primary care physicians", scenario_04_handle_follow_up_reminders_for_patients_without_primary_care),
        ("Customize follow-up based on patient communication preferences", scenario_05_customize_follow_up_based_on_patient_communication_preferenc),
        ("Handle follow-up for pediatric patients with parent/guardian coordination", scenario_06_handle_follow_up_for_pediatric_patients_with_parent_guardian),
        ("Track follow-up compliance and patient outcomes", scenario_07_track_follow_up_compliance_and_patient_outcomes),
        ("Handle follow-up complications and escalation procedures", scenario_08_handle_follow_up_complications_and_escalation_procedures),
    ]);
}
