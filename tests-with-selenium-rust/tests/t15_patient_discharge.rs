// Selenium WebDriver + libtest-mimic test for
// tests-with-given-when-then-features/15-patient-discharge.feature
// (equivalent to tests-with-selenium-javascript/15-patient-discharge.test.js).
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
    //   And I am logged in as "Dr. Johnson"
    //   And the discharge module is active
    //   And billing integration is enabled
    //   And bed management system is connected
    verify_system_is_operational(driver).await?;
    login(driver, "Dr. Johnson", false).await?;
    // The discharge module, billing integration, and the bed management
    // system connection are assumed to be active backend configuration
    // already in place for this environment.

    let patient_discharge_nav_link = wait_for_test_id(driver, "Nav Patient Discharge").await?;
    patient_discharge_nav_link.click().await?;
    wait_for_test_id(driver, "Patient Discharge Panel").await?;
    Ok(())
}

fn scenario_01_complete_standard_patient_discharge_with_instructions(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a patient "Jennifer Martinez" is in bed "ED-8"
        // And the patient has completed treatment for "urinary tract infection"
        // And all diagnostic tests and treatments are finished
        // And the patient is medically stable for discharge
        // (assumed pre-seeded test data)
        // When I enter discharge orders and instructions:
        fill_fields(driver, &vec![
            row([("Field", "Discharge Status"), ("Value", "Home with medications")]),
            row([("Field", "Primary Diagnosis"), ("Value", "Urinary tract infection (N39.0)")]),
            row([("Field", "Medications"), ("Value", "Trimethoprim-Sulfamethoxazole 800mg BID x7d")]),
            row([("Field", "Follow-up Care"), ("Value", "Primary care physician in 3-5 days")]),
            row([("Field", "Activity Level"), ("Value", "Regular activities as tolerated")]),
            row([("Field", "Diet"), ("Value", "Regular diet, increase fluid intake")]),
            row([("Field", "Return Precautions"), ("Value", "Fever >101°F, worsening symptoms, blood in urine")]),
        ]).await?;
        // And I submit the discharge orders
        driver.find(By::Css("[data-testid=\"submit-discharge-form\"]")).await?.click().await?;

        // Then the system generates comprehensive discharge paperwork:
        wait_for_test_id(driver, "Discharge Summary").await?;
        let discharge_paperwork = vec![
            row([("Document", "Discharge Summary"), ("Content", "Treatment summary, diagnosis, medications")]),
            row([("Document", "Medication List"), ("Content", "Prescriptions with dosing instructions")]),
            row([("Document", "Follow-up Instructions"), ("Content", "PCP appointment scheduling information")]),
            row([("Document", "Return Precautions"), ("Content", "When to seek emergency care")]),
            row([("Document", "Patient Education"), ("Content", "UTI prevention and care instructions")]),
        ];
        for row_data in &discharge_paperwork {
            let document = row_data["Document"].as_str();
            let content = row_data["Content"].as_str();
            assert_eq!(get_text(driver, document).await?, content);
        }

        // And the system updates bed availability:
        let bed_availability_updates = vec![
            row([("Change", "Previous Status"), ("Details", "Occupied by Jennifer Martinez")]),
            row([("Change", "New Status"), ("Details", "Needs cleaning")]),
            row([("Change", "Availability"), ("Details", "Removed from available bed count")]),
            row([("Change", "Housekeeping Alert"), ("Details", "Cleaning notification sent")]),
        ];
        for row_data in &bed_availability_updates {
            let change = row_data["Change"].as_str();
            let details = row_data["Details"].as_str();
            assert_eq!(get_text(driver, change).await?, details);
        }

        // And billing processes are automatically triggered:
        let billing_actions = vec![
            row([("Action", "Final Charges"), ("Details", "All services and procedures captured")]),
            row([("Action", "Insurance Billing"), ("Details", "Claims prepared for submission")]),
            row([("Action", "Patient Statement"), ("Details", "Financial responsibility calculated")]),
            row([("Action", "Coding Review"), ("Details", "ICD-10 and CPT codes validated")]),
        ];
        for row_data in &billing_actions {
            let action = row_data["Action"].as_str();
            let details = row_data["Details"].as_str();
            assert_eq!(get_text(driver, action).await?, details);
        }
        Ok(())
    })
}

fn scenario_02_discharge_patient_with_prescription_medications(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a patient "Robert Chen" is ready for discharge
        // And treatment required multiple medications
        // (assumed pre-seeded test data)
        // When I enter discharge orders including prescriptions:
        fill_fields(driver, &vec![
            row([("Field", "Amoxicillin Dose"), ("Value", "500mg")]),
            row([("Field", "Amoxicillin Frequency"), ("Value", "TID")]),
            row([("Field", "Amoxicillin Duration"), ("Value", "10 days")]),
            row([("Field", "Amoxicillin Special Instructions"), ("Value", "Take with food")]),
            row([("Field", "Ibuprofen Dose"), ("Value", "600mg")]),
            row([("Field", "Ibuprofen Frequency"), ("Value", "Q6H PRN")]),
            row([("Field", "Ibuprofen Duration"), ("Value", "5 days")]),
            row([("Field", "Ibuprofen Special Instructions"), ("Value", "For pain only")]),
            row([("Field", "Omeprazole Dose"), ("Value", "20mg")]),
            row([("Field", "Omeprazole Frequency"), ("Value", "Daily")]),
            row([("Field", "Omeprazole Duration"), ("Value", "14 days")]),
            row([("Field", "Omeprazole Special Instructions"), ("Value", "Take before breakfast")]),
        ]).await?;
        // And I include medication education:
        fill_fields(driver, &vec![
            row([("Field", "Drug Interactions"), ("Value", "Avoid alcohol with antibiotics")]),
            row([("Field", "Side Effects"), ("Value", "Watch for nausea, diarrhea, allergic reactions")]),
            row([("Field", "Compliance"), ("Value", "Complete full antibiotic course")]),
        ]).await?;
        // And I submit the discharge
        driver.find(By::Css("[data-testid=\"submit-discharge-form\"]")).await?.click().await?;

        // Then the system generates medication-specific documentation:
        wait_for_test_id(driver, "Prescription List").await?;
        let medication_documentation = vec![
            row([("Document", "Prescription List"), ("Content", "All medications with complete instructions")]),
            row([("Document", "Drug Information"), ("Content", "Side effects, interactions, precautions")]),
            row([("Document", "Pharmacy List"), ("Content", "Nearby pharmacies with hours")]),
            row([("Document", "Medication Calendar"), ("Content", "Dosing schedule for patient reference")]),
        ];
        for row_data in &medication_documentation {
            let document = row_data["Document"].as_str();
            let content = row_data["Content"].as_str();
            assert_eq!(get_text(driver, document).await?, content);
        }

        // And prescriptions are electronically transmitted to patient's preferred pharmacy
        let prescription_transmission_status = get_text(driver, "Prescription Transmission Status").await?;
        assert_match(&prescription_transmission_status, r"transmitted", true);

        // And medication allergy checking is performed one final time
        let allergy_check_status = get_text(driver, "Medication Allergy Check Status").await?;
        assert_match(&allergy_check_status, r"checked|performed|cleared", true);

        // And patient receives medication counseling checklist
        let counseling_checklist = wait_for_test_id(driver, "Medication Counseling Checklist").await?;
        assert!(counseling_checklist.is_displayed().await?);
        Ok(())
    })
}

fn scenario_03_discharge_patient_requiring_follow_up_appointments(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a patient "Maria Santos" needs specialized follow-up care
        // And the treatment was for "complex laceration repair"
        // (assumed pre-seeded test data)
        // When I enter discharge orders with follow-up requirements:
        fill_fields(driver, &vec![
            row([("Field", "Wound Check Timeframe"), ("Value", "2-3 days")]),
            row([("Field", "Wound Check Specialist Required"), ("Value", "Primary care")]),
            row([("Field", "Wound Check Special Instructions"), ("Value", "Remove sutures")]),
            row([("Field", "Specialist Consult Timeframe"), ("Value", "1 week")]),
            row([("Field", "Specialist Consult Specialist Required"), ("Value", "Plastic surgeon")]),
            row([("Field", "Specialist Consult Special Instructions"), ("Value", "Scar management")]),
            row([("Field", "Lab Follow-up Timeframe"), ("Value", "5 days")]),
            row([("Field", "Lab Follow-up Specialist Required"), ("Value", "Primary care")]),
            row([("Field", "Lab Follow-up Special Instructions"), ("Value", "Check CBC")]),
        ]).await?;
        // And I specify wound care instructions:
        fill_fields(driver, &vec![
            row([("Field", "Dressing Changes"), ("Value", "Change daily, keep dry for 48 hours")]),
            row([("Field", "Cleaning Protocol"), ("Value", "Gentle soap and water after 48 hours")]),
            row([("Field", "Activity Restrictions"), ("Value", "No heavy lifting >10 lbs for 2 weeks")]),
            row([("Field", "Signs of Infection"), ("Value", "Redness, swelling, pus, fever")]),
        ]).await?;

        // Then the system schedules and documents follow-up care:
        wait_for_test_id(driver, "Appointment Booking").await?;
        let follow_up_scheduling = vec![
            row([("Action", "Appointment Booking"), ("Details", "Attempts to schedule with preferred providers")]),
            row([("Action", "Referral Generation"), ("Details", "Electronic referrals to specialists")]),
            row([("Action", "Reminder Setup"), ("Details", "Patient reminders for appointments")]),
        ];
        for row_data in &follow_up_scheduling {
            let action = row_data["Action"].as_str();
            let details = row_data["Details"].as_str();
            assert_eq!(get_text(driver, action).await?, details);
        }

        // And comprehensive wound care instructions are provided
        let wound_care_instructions = wait_for_test_id(driver, "Wound Care Instructions").await?;
        assert!(wound_care_instructions.is_displayed().await?);

        // And follow-up appointment confirmations are sent to patient
        let appointment_confirmation_status = get_text(driver, "Appointment Confirmation Status").await?;
        assert_match(&appointment_confirmation_status, r"sent|confirmed", true);

        // And referring physician receives notification of specialist referral
        let specialist_referral_notification_status = get_text(driver, "Specialist Referral Notification Status").await?;
        assert_match(&specialist_referral_notification_status, r"sent|notified", true);
        Ok(())
    })
}

fn scenario_04_handle_discharge_with_insurance_authorization_requirements(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a patient "David Kim" requires expensive follow-up imaging
        // And the patient's insurance requires prior authorization
        // (assumed pre-seeded test data)
        // When I enter discharge orders including:
        fill_fields(driver, &vec![
            row([("Field", "Imaging Study"), ("Value", "MRI lumbar spine within 2 weeks")]),
            row([("Field", "Estimated Cost"), ("Value", "$2,400")]),
            row([("Field", "Medical Necessity"), ("Value", "Rule out disc herniation")]),
        ]).await?;
        // And I submit the discharge orders
        driver.find(By::Css("[data-testid=\"submit-discharge-form\"]")).await?.click().await?;

        // Then the system handles insurance requirements:
        wait_for_test_id(driver, "Authorization Check").await?;
        let insurance_requirements = vec![
            row([("Process", "Authorization Check"), ("Action", "Prior auth required for MRI")]),
            row([("Process", "Documentation Prep"), ("Action", "Clinical justification prepared")]),
            row([("Process", "Patient Notification"), ("Action", "Informed of authorization process")]),
            row([("Process", "Alternative Options"), ("Action", "Suggest urgent care MRI if auth denied")]),
        ];
        for row_data in &insurance_requirements {
            let process = row_data["Process"].as_str();
            let action = row_data["Action"].as_str();
            assert_eq!(get_text(driver, process).await?, action);
        }

        // And the patient receives information about:
        let patient_information = vec![
            row([("Type", "Authorization Process"), ("Content", "Timeline and requirements explained")]),
            row([("Type", "Financial Options"), ("Content", "Self-pay rates and payment plans")]),
            row([("Type", "Alternative Providers"), ("Content", "Facilities that may not require pre-auth")]),
        ];
        for row_data in &patient_information {
            let type_ = row_data["Type"].as_str();
            let content = row_data["Content"].as_str();
            assert_eq!(get_text(driver, type_).await?, content);
        }

        // And insurance pre-authorization request is automatically submitted
        let pre_authorization_status = get_text(driver, "Insurance Pre-Authorization Status").await?;
        assert_match(&pre_authorization_status, r"submitted", true);
        Ok(())
    })
}

fn scenario_05_discharge_pediatric_patient_with_parent_guardian_instruction(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a pediatric patient "Emma Foster" (age 6) is ready for discharge
        // And the parent "Sarah Foster" is present
        // And treatment was for "febrile seizure"
        // (assumed pre-seeded test data)
        // When I enter pediatric discharge orders:
        fill_fields(driver, &vec![
            row([("Field", "Weight-based Medications"), ("Value", "Acetaminophen 10mg/kg Q6H PRN fever")]),
            row([("Field", "Parent Education"), ("Value", "Fever management, seizure precautions")]),
            row([("Field", "Activity Restrictions"), ("Value", "No swimming for 24 hours")]),
            row([("Field", "School Return"), ("Value", "May return tomorrow if fever-free")]),
        ]).await?;
        // And I provide seizure-specific education:
        fill_fields(driver, &vec![
            row([("Field", "Seizure Precautions"), ("Value", "Keep child safe during future episodes")]),
            row([("Field", "When to Call 911"), ("Value", "Seizure >5 minutes, difficulty breathing")]),
            row([("Field", "Temperature Control"), ("Value", "Aggressive fever reduction strategies")]),
        ]).await?;

        // Then the system generates pediatric-specific discharge materials:
        wait_for_test_id(driver, "Parent Instructions").await?;
        let pediatric_materials = vec![
            row([("Document", "Parent Instructions"), ("Content", "Age-appropriate medication dosing")]),
            row([("Document", "Emergency Signs"), ("Content", "When to bring child back to ED")]),
            row([("Document", "School Note"), ("Content", "Medical excuse and return instructions")]),
            row([("Document", "Developmental Info"), ("Content", "Normal vs concerning behaviors post-seizure")]),
        ];
        for row_data in &pediatric_materials {
            let document = row_data["Document"].as_str();
            let content = row_data["Content"].as_str();
            assert_eq!(get_text(driver, document).await?, content);
        }

        // And parent acknowledgment is electronically captured
        let parent_acknowledgment_status = get_text(driver, "Parent Acknowledgment Status").await?;
        assert_match(&parent_acknowledgment_status, r"captured|recorded", true);

        // And pediatric follow-up with primary care pediatrician is scheduled
        let pediatric_follow_up_status = get_text(driver, "Pediatric Follow-up Status").await?;
        assert_match(&pediatric_follow_up_status, r"scheduled", true);

        // And school nurse receives medical summary if parent consents
        let school_nurse_notification_status = get_text(driver, "School Nurse Notification Status").await?;
        assert_match(&school_nurse_notification_status, r"sent|notified", true);
        Ok(())
    })
}

fn scenario_06_handle_discharge_during_shift_change(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a patient "Lisa Brown" is ready for discharge at 18:45
        // And shift change occurs at 19:00
        // And "Dr. Day" (day shift) is discharging the patient
        // And "Dr. Night" (evening shift) is incoming
        // (assumed pre-seeded test data)
        // When "Dr. Day" enters the discharge orders
        // And the discharge process extends past shift change
        // (assumed to have already occurred / triggered by the system)

        // Then the system manages the transition seamlessly:
        wait_for_test_id(driver, "Discharge Ownership").await?;
        let transition_management = vec![
            row([("Item", "Discharge Ownership"), ("Action", "Dr. Day completes discharge process")]),
            row([("Item", "Documentation"), ("Action", "All discharge notes under Dr. Day's name")]),
            row([("Item", "Follow-up Responsibility"), ("Action", "Any issues route to Dr. Night")]),
            row([("Item", "Billing Attribution"), ("Action", "Dr. Day receives credit for discharge")]),
        ];
        for row_data in &transition_management {
            let item = row_data["Item"].as_str();
            let action = row_data["Action"].as_str();
            assert_eq!(get_text(driver, item).await?, action);
        }

        // And both physicians receive handoff notification:
        let handoff_notifications = vec![
            row([("Physician", "Dr. Day"), ("Content", "Discharge completed for Lisa Brown")]),
            row([("Physician", "Dr. Night"), ("Content", "Lisa Brown discharged - available for questions")]),
        ];
        for row_data in &handoff_notifications {
            let physician = row_data["Physician"].as_str();
            let content = row_data["Content"].as_str();
            assert_eq!(get_text(driver, &format!("{} Notification", physician)).await?, content);
        }

        // And the bed becomes available for evening shift patient flow
        let bed_availability_status = get_text(driver, "Bed Availability Status").await?;
        assert_match(&bed_availability_status, r"available", true);
        Ok(())
    })
}

fn scenario_07_discharge_patient_against_medical_advice_ama(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a patient "Michael Davis" wants to leave against medical advice
        // And the patient has been informed of risks
        // And the patient has decision-making capacity
        // (assumed pre-seeded test data)
        // When I process an AMA discharge:
        fill_fields(driver, &vec![
            row([("Field", "Risk Explanation"), ("Value", "Documented that risks were explained")]),
            row([("Field", "Patient Understanding"), ("Value", "Patient verbalized understanding of risks")]),
            row([("Field", "Capacity Assessment"), ("Value", "Patient has decision-making capacity")]),
            row([("Field", "Witness Required"), ("Value", "Nurse witness to AMA conversation")]),
        ]).await?;
        // And I enter minimal safe discharge instructions:
        fill_fields(driver, &vec![
            row([("Field", "Return Immediately"), ("Value", "If symptoms worsen or new symptoms develop")]),
            row([("Field", "Follow-up Care"), ("Value", "Strong recommendation for PCP visit")]),
            row([("Field", "Medication Safety"), ("Value", "Critical medications must be continued")]),
        ]).await?;

        // Then the system generates AMA-specific documentation:
        wait_for_test_id(driver, "AMA Form").await?;
        let ama_documentation = vec![
            row([("Document", "AMA Form"), ("Content", "Legal documentation of patient choice")]),
            row([("Document", "Risk Documentation"), ("Content", "Medical risks of leaving explained")]),
            row([("Document", "Witness Signatures"), ("Content", "Patient, physician, and nurse signatures")]),
            row([("Document", "Limited Liability"), ("Content", "Hospital liability limitations documented")]),
        ];
        for row_data in &ama_documentation {
            let document = row_data["Document"].as_str();
            let content = row_data["Content"].as_str();
            assert_eq!(get_text(driver, document).await?, content);
        }

        // And billing processes reflect AMA status
        let billing_ama_status = get_text(driver, "Billing AMA Status").await?;
        assert_match(&billing_ama_status, r"AMA", true);

        // And legal risk management is notified of AMA discharge
        let legal_risk_notification_status = get_text(driver, "Legal Risk Management Notification Status").await?;
        assert_match(&legal_risk_notification_status, r"notified", true);

        // And patient still receives basic safety instructions
        let basic_safety_instructions = wait_for_test_id(driver, "Basic Safety Instructions").await?;
        assert!(basic_safety_instructions.is_displayed().await?);
        Ok(())
    })
}

fn scenario_08_batch_discharge_processing_during_high_volume(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given multiple patients are ready for simultaneous discharge:
        //   | Patient Name    | Bed    | Diagnosis        | Discharge Type     |
        //   | Patient A       | ED-3   | Minor injury     | Home              |
        //   | Patient B       | ED-7   | Gastroenteritis  | Home with meds    |
        //   | Patient C       | ED-11  | Anxiety          | Home with referral |
        // (assumed pre-seeded test data)
        // When I process multiple discharges efficiently
        // (assumed to have already occurred / triggered by the system)

        // Then the system handles batch processing:
        wait_for_test_id(driver, "Template Usage").await?;
        let batch_processing = vec![
            row([("Feature", "Template Usage"), ("Functionality", "Common discharge templates applied")]),
            row([("Feature", "Automated Documentation"), ("Functionality", "Standard instructions auto-populated")]),
            row([("Feature", "Concurrent Processing"), ("Functionality", "Multiple discharges processed simultaneously")]),
        ];
        for row_data in &batch_processing {
            let feature = row_data["Feature"].as_str();
            let functionality = row_data["Functionality"].as_str();
            assert_eq!(get_text(driver, feature).await?, functionality);
        }

        // And all bed updates occur simultaneously:
        let bed_updates = vec![
            row([("Management", "Status Updates"), ("Action", "All beds marked \"needs cleaning\"")]),
            row([("Management", "Housekeeping Batch"), ("Action", "Single notification for multiple rooms")]),
            row([("Management", "Availability Count"), ("Action", "Bed count updated after all discharges")]),
        ];
        for row_data in &bed_updates {
            let management = row_data["Management"].as_str();
            let action = row_data["Action"].as_str();
            assert_eq!(get_text(driver, management).await?, action);
        }

        // And billing processes are optimized for batch handling
        let batch_billing_status = get_text(driver, "Batch Billing Status").await?;
        assert_match(&batch_billing_status, r"optimized", true);
        Ok(())
    })
}

fn scenario_09_track_discharge_metrics_and_quality_indicators(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given patient discharges are being processed
        // (assumed pre-seeded test data)
        // When discharge orders are completed
        // (assumed to have already occurred / triggered by the system)

        // Then the system tracks key performance indicators:
        wait_for_test_id(driver, "Discharge Time").await?;
        let performance_indicators = vec![
            row([("Metric", "Discharge Time"), ("Measurement", "Order entry to patient departure")]),
            row([("Metric", "Readmission Rate"), ("Measurement", "72-hour return rate tracking")]),
            row([("Metric", "Instruction Quality"), ("Measurement", "Patient understanding verification")]),
            row([("Metric", "Follow-up Compliance"), ("Measurement", "Scheduled appointment attendance")]),
        ];
        for row_data in &performance_indicators {
            let metric = row_data["Metric"].as_str();
            let measurement = row_data["Measurement"].as_str();
            assert_eq!(get_text(driver, metric).await?, measurement);
        }

        // And generates quality reports:
        let quality_reports = vec![
            row([("Report", "Provider Performance"), ("Content", "Discharge efficiency by physician")]),
            row([("Report", "Patient Satisfaction"), ("Content", "Discharge process satisfaction scores")]),
            row([("Report", "Readmission Analysis"), ("Content", "Patterns in early returns")]),
        ];
        for row_data in &quality_reports {
            let report = row_data["Report"].as_str();
            let content = row_data["Content"].as_str();
            assert_eq!(get_text(driver, report).await?, content);
        }

        // And identifies improvement opportunities:
        let improvement_opportunities = vec![
            row([("Area", "Process Efficiency"), ("Recommendation", "Streamline documentation workflows")]),
            row([("Area", "Patient Education"), ("Recommendation", "Enhance instruction clarity")]),
            row([("Area", "Follow-up Coordination"), ("Recommendation", "Improve appointment scheduling system")]),
        ];
        for row_data in &improvement_opportunities {
            let area = row_data["Area"].as_str();
            let recommendation = row_data["Recommendation"].as_str();
            assert_eq!(get_text(driver, area).await?, recommendation);
        }
        Ok(())
    })
}

fn main() {
    run_feature(&[
        ("Complete standard patient discharge with instructions", scenario_01_complete_standard_patient_discharge_with_instructions),
        ("Discharge patient with prescription medications", scenario_02_discharge_patient_with_prescription_medications),
        ("Discharge patient requiring follow-up appointments", scenario_03_discharge_patient_requiring_follow_up_appointments),
        ("Handle discharge with insurance authorization requirements", scenario_04_handle_discharge_with_insurance_authorization_requirements),
        ("Discharge pediatric patient with parent/guardian instructions", scenario_05_discharge_pediatric_patient_with_parent_guardian_instruction),
        ("Handle discharge during shift change", scenario_06_handle_discharge_during_shift_change),
        ("Discharge patient against medical advice (AMA)", scenario_07_discharge_patient_against_medical_advice_ama),
        ("Batch discharge processing during high volume", scenario_08_batch_discharge_processing_during_high_volume),
        ("Track discharge metrics and quality indicators", scenario_09_track_discharge_metrics_and_quality_indicators),
    ]);
}
