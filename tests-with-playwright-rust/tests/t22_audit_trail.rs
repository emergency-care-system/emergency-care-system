// Playwright + libtest-mimic test for
// tests-with-given-when-then-features/22-audit-trail.feature
// (equivalent to tests-with-playwright-javascript/22-audit-trail.test.js).
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
    //   And I am logged in as "Compliance Officer Martinez"
    //   And the audit logging system is active and capturing all access events
    //   And patient record access is being monitored in real-time
    //   And audit reports are available for compliance review
    verify_system_is_operational(page).await?;
    login(page, "Compliance Officer Martinez", false).await?;
    // The remaining Background steps describe pre-seeded system state
    // (audit logging capture, real-time access monitoring, and available
    // audit reports) assumed to already be configured in the test
    // environment.
    let feature_nav_link = wait_for_test_id(page, "Nav Audit Trail").await?;
    feature_nav_link.click(None).await?;
    wait_for_test_id(page, "Audit Trail Panel").await?;
    Ok(())
}

fn scenario_01_review_comprehensive_access_log_for_frequently_accessed_pati(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given a patient "Jennifer Rodriguez" was treated in the ED on June 20, 2025
        // And her medical record number is "MRN-789456"
        // And multiple healthcare providers accessed her record during and after her visit
        // When I search for access logs for patient "Jennifer Rodriguez" (MRN-789456)
        fill_field(page, "Patient Search", "Jennifer Rodriguez (MRN-789456)").await?;
        // And I set the date range from June 20-24, 2025
        fill_field(page, "Start Date", "2025-06-20").await?;
        fill_field(page, "End Date", "2025-06-24").await?;
        // And I generate the comprehensive audit report
        page.get_by_test_id("generate-audit-report-button").first().click(None).await?;

        // Then the system displays all users who accessed the record:
        //   | Access # | User Name          | User Role           | Department       | Access Purpose    |
        //   | 1        | Dr. Sarah Kim      | Emergency Physician | Emergency Dept   | Direct patient care|
        //   | 2        | Nurse Johnson      | Registered Nurse    | Emergency Dept   | Direct patient care|
        //   | 3        | Tech Martinez      | Lab Technician      | Laboratory       | Lab result entry  |
        //   | 4        | Dr. Chen           | Radiologist         | Radiology        | Image interpretation|
        //   | 5        | Billing Clerk Adams| Billing Specialist  | Patient Financial| Billing/coding    |
        //   | 6        | Case Mgr Wilson    | Case Manager        | Social Services  | Discharge planning|
        //   | 7        | Dr. Patel          | Cardiologist        | Cardiology       | Consultation      |
        let access_log_entries = page.get_by_test_id("audit-log-entry");
        assert_eq!(access_log_entries.count().await?, 7);

        // And detailed timestamps are shown for each access:
        //   | User Name          | Login Time           | Logout Time          | Session Duration |
        //   | Dr. Sarah Kim      | 2025-06-20 14:15:22 | 2025-06-20 14:45:10 | 29 min 48 sec   |
        //   | Nurse Johnson      | 2025-06-20 14:20:15 | 2025-06-20 16:30:22 | 2 hr 10 min 7 sec|
        //   | Tech Martinez      | 2025-06-20 15:22:45 | 2025-06-20 15:25:12 | 2 min 27 sec    |
        //   | Dr. Chen           | 2025-06-20 16:10:33 | 2025-06-20 16:18:45 | 8 min 12 sec    |
        //   | Billing Clerk Adams| 2025-06-21 09:15:20 | 2025-06-21 09:22:15 | 6 min 55 sec    |
        //   | Case Mgr Wilson    | 2025-06-21 11:30:10 | 2025-06-21 11:45:33 | 15 min 23 sec   |
        //   | Dr. Patel          | 2025-06-22 10:22:18 | 2025-06-22 10:35:45 | 13 min 27 sec   |
        let access_log_timestamps = page.get_by_test_id("audit-log-timestamp-entry");
        assert_eq!(access_log_timestamps.count().await?, 7);

        // And specific data elements accessed are documented:
        //   | User Name          | Data Elements Accessed                           | Actions Performed        |
        //   | Dr. Sarah Kim      | Demographics, Chief complaint, Vital signs, Assessment, Orders | View, Edit, Create    |
        //   | Nurse Johnson      | Vital signs, Medications, Allergies, Care plans | View, Edit, Document   |
        //   | Tech Martinez      | Lab orders, Lab results                         | View, Enter results    |
        //   | Dr. Chen           | Imaging orders, Radiology reports              | View, Create report    |
        //   | Billing Clerk Adams| Diagnosis codes, Procedures, Insurance info     | View only             |
        //   | Case Mgr Wilson    | Discharge plans, Insurance, Social history      | View, Edit            |
        //   | Dr. Patel          | Cardiac tests, Consultation notes              | View, Create notes     |
        let access_log_data_elements = page.get_by_test_id("audit-log-data-element-entry");
        assert_eq!(access_log_data_elements.count().await?, 7);
        Ok(())
    })
}

fn scenario_02_investigate_suspicious_access_pattern_for_patient_record(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given a patient "Robert Thompson" has a high-profile status
        // And there have been unusual access patterns to his record
        // And the patient was not treated in the hospital during the access period
        // When I generate a detailed audit report for "Robert Thompson" (MRN-456789)
        fill_field(page, "Patient Search", "Robert Thompson (MRN-456789)").await?;
        page.get_by_test_id("generate-audit-report-button").first().click(None).await?;
        // And I focus on the suspicious access period from June 15-18, 2025
        fill_field(page, "Start Date", "2025-06-15").await?;
        fill_field(page, "End Date", "2025-06-18").await?;

        // Then the system identifies potentially inappropriate access:
        //   | Suspicious Activity | Details                                          |
        //   | Unauthorized User   | Dr. Williams (Orthopedics) - No treatment relationship|
        //   | Unusual Timing      | Access at 11:45 PM on June 16 (outside normal hours)|
        //   | Excessive Duration  | 45-minute session for patient not under care    |
        //   | Data Mining Pattern | Accessed multiple unrelated patient records same night|
        let suspicious_activity = vec![
            row([("label", "Unauthorized User"), ("value", "Dr. Williams (Orthopedics) - No treatment relationship")]),
            row([("label", "Unusual Timing"), ("value", "Access at 11:45 PM on June 16 (outside normal hours)")]),
            row([("label", "Excessive Duration"), ("value", "45-minute session for patient not under care")]),
            row([("label", "Data Mining Pattern"), ("value", "Accessed multiple unrelated patient records same night")]),
        ];
        for row_data in &suspicious_activity {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }

        // And detailed forensic information is provided:
        //   | Forensic Data       | Investigation Details                            |
        //   | IP Address          | 192.168.1.205 (Dr. Williams' office computer)  |
        //   | Workstation ID      | WS-ORTHO-03                                     |
        //   | Access Method       | Valid credentials, no badge scan               |
        //   | Previous Pattern    | First time accessing this patient               |
        //   | Concurrent Activity | Accessed 8 other unrelated patients same session|
        let forensic_data = vec![
            row([("label", "IP Address"), ("value", "192.168.1.205 (Dr. Williams' office computer)")]),
            row([("label", "Workstation ID"), ("value", "WS-ORTHO-03")]),
            row([("label", "Access Method"), ("value", "Valid credentials, no badge scan")]),
            row([("label", "Previous Pattern"), ("value", "First time accessing this patient")]),
            row([("label", "Concurrent Activity"), ("value", "Accessed 8 other unrelated patients same session")]),
        ];
        for row_data in &forensic_data {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }

        // And compliance violation indicators are flagged:
        //   | Violation Type      | HIPAA Concern                                   |
        //   | No Treatment Relationship| No medical necessity for access              |
        //   | Excessive Access    | Viewed entire medical history unnecessarily    |
        //   | Pattern of Behavior | Multiple inappropriate accesses detected       |
        //   | Time-based Concern  | Access outside normal work hours              |
        let violation_indicators = vec![
            row([("label", "No Treatment Relationship"), ("value", "No medical necessity for access")]),
            row([("label", "Excessive Access"), ("value", "Viewed entire medical history unnecessarily")]),
            row([("label", "Pattern of Behavior"), ("value", "Multiple inappropriate accesses detected")]),
            row([("label", "Time-based Concern"), ("value", "Access outside normal work hours")]),
        ];
        for row_data in &violation_indicators {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }

        // And automatic security alerts are generated:
        //   | Alert Type          | Notification Details                            |
        //   | Privacy Officer     | Immediate alert sent for investigation          |
        //   | Department Head     | Orthopedics supervisor notified                |
        //   | IT Security         | Account flagged for enhanced monitoring         |
        //   | Risk Management     | Potential HIPAA violation logged              |
        let security_alerts = vec![
            row([("label", "Privacy Officer"), ("value", "Immediate alert sent for investigation")]),
            row([("label", "Department Head"), ("value", "Orthopedics supervisor notified")]),
            row([("label", "IT Security"), ("value", "Account flagged for enhanced monitoring")]),
            row([("label", "Risk Management"), ("value", "Potential HIPAA violation logged")]),
        ];
        for row_data in &security_alerts {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }
        Ok(())
    })
}

fn scenario_03_generate_audit_report_for_break_the_glass_emergency_access(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given a patient "Emergency John Doe" was brought unconscious to the ED
        // And normal consent procedures could not be followed due to patient condition
        // And emergency "break-the-glass" access was used to view records
        // When I review the emergency access audit trail
        page.get_by_test_id("review-emergency-access-button").first().click(None).await?;

        // Then the system documents the break-the-glass access:
        //   | Emergency Access    | Documentation                                   |
        //   | Access Type         | Break-the-glass emergency override             |
        //   | Medical Justification| Patient unconscious, life-threatening condition|
        //   | Authorizing Physician| Dr. Emergency Chief (Emergency Department Head) |
        //   | Access Duration     | 2 hours during critical care period           |
        //   | Override Reason     | Unable to obtain consent, medical emergency    |
        let break_the_glass_documentation = vec![
            row([("label", "Access Type"), ("value", "Break-the-glass emergency override")]),
            row([("label", "Medical Justification"), ("value", "Patient unconscious, life-threatening condition")]),
            row([("label", "Authorizing Physician"), ("value", "Dr. Emergency Chief (Emergency Department Head)")]),
            row([("label", "Access Duration"), ("value", "2 hours during critical care period")]),
            row([("label", "Override Reason"), ("value", "Unable to obtain consent, medical emergency")]),
        ];
        for row_data in &break_the_glass_documentation {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }

        // And all emergency access activities are logged:
        //   | Activity            | Details                                         |
        //   | Records Accessed    | Previous ED visits, medication allergies, medical history|
        //   | Users Involved      | Dr. Sarah Kim, Nurse Johnson, Pharmacist Lee   |
        //   | Data Viewed         | Allergies, medications, past procedures         |
        //   | Clinical Decisions  | Medication choices based on allergy history    |
        //   | Patient Outcome     | Successful treatment, patient stabilized       |
        let emergency_access_activities = vec![
            row([("label", "Records Accessed"), ("value", "Previous ED visits, medication allergies, medical history")]),
            row([("label", "Users Involved"), ("value", "Dr. Sarah Kim, Nurse Johnson, Pharmacist Lee")]),
            row([("label", "Data Viewed"), ("value", "Allergies, medications, past procedures")]),
            row([("label", "Clinical Decisions"), ("value", "Medication choices based on allergy history")]),
            row([("label", "Patient Outcome"), ("value", "Successful treatment, patient stabilized")]),
        ];
        for row_data in &emergency_access_activities {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }

        // And post-emergency review requirements are documented:
        //   | Review Requirement  | Compliance Action                               |
        //   | Medical Necessity   | Clinical justification documented              |
        //   | Minimum Necessary   | Only essential health information accessed     |
        //   | Patient Notification| Patient to be informed of emergency access when able|
        //   | Quality Review      | Emergency access appropriateness reviewed      |
        let review_requirements = vec![
            row([("label", "Medical Necessity"), ("value", "Clinical justification documented")]),
            row([("label", "Minimum Necessary"), ("value", "Only essential health information accessed")]),
            row([("label", "Patient Notification"), ("value", "Patient to be informed of emergency access when able")]),
            row([("label", "Quality Review"), ("value", "Emergency access appropriateness reviewed")]),
        ];
        for row_data in &review_requirements {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }
        Ok(())
    })
}

fn scenario_04_audit_trail_for_patient_who_requested_access_log_of_their_ow(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given a patient "Maria Santos" has requested a copy of her access log
        // And this is her legal right under HIPAA
        // And she was treated on multiple occasions in the past year
        // When I generate a patient-facing access report for "Maria Santos" (MRN-321654)
        fill_field(page, "Patient Search", "Maria Santos (MRN-321654)").await?;
        page.get_by_test_id("generate-patient-access-report-button").first().click(None).await?;
        // And I include the past 12 months of access activity
        fill_field(page, "Reporting Period", "12 months").await?;

        // Then the system creates a patient-appropriate access summary:
        //   | Access Summary      | Patient-Friendly Information                    |
        //   | Healthcare Providers| Names and roles of providers who accessed record|
        //   | Treatment Dates     | Dates when records were accessed for care      |
        //   | Purpose Categories  | Treatment, payment, healthcare operations       |
        //   | Administrative Access| Billing, quality assurance, regulatory compliance|
        let access_summary = vec![
            row([("label", "Healthcare Providers"), ("value", "Names and roles of providers who accessed record")]),
            row([("label", "Treatment Dates"), ("value", "Dates when records were accessed for care")]),
            row([("label", "Purpose Categories"), ("value", "Treatment, payment, healthcare operations")]),
            row([("label", "Administrative Access"), ("value", "Billing, quality assurance, regulatory compliance")]),
        ];
        for row_data in &access_summary {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }

        // And sensitive details are appropriately filtered:
        //   | Information Included| Patient Access Report Content                  |
        //   | Provider Names      | Dr. Sarah Kim (Emergency Medicine)             |
        //   | Access Dates        | June 20, 2025 for emergency treatment         |
        //   | General Purpose     | Direct patient care and treatment             |
        //   | Department          | Emergency Department                           |
        let information_included = vec![
            row([("label", "Provider Names"), ("value", "Dr. Sarah Kim (Emergency Medicine)")]),
            row([("label", "Access Dates"), ("value", "June 20, 2025 for emergency treatment")]),
            row([("label", "General Purpose"), ("value", "Direct patient care and treatment")]),
            row([("label", "Department"), ("value", "Emergency Department")]),
        ];
        for row_data in &information_included {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }

        // And technical details are excluded:
        //   | Information Excluded| Reason for Exclusion                          |
        //   | IP Addresses        | Technical data not relevant to patient        |
        //   | Workstation IDs     | Internal system identifiers                   |
        //   | Session Details     | Technical access information                  |
        //   | Investigation Data  | Law enforcement sensitive information          |
        let information_excluded = vec![
            row([("label", "IP Addresses"), ("value", "Technical data not relevant to patient")]),
            row([("label", "Workstation IDs"), ("value", "Internal system identifiers")]),
            row([("label", "Session Details"), ("value", "Technical access information")]),
            row([("label", "Investigation Data"), ("value", "Law enforcement sensitive information")]),
        ];
        for row_data in &information_excluded {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }

        // And the report includes patient rights information:
        //   | Patient Rights      | Information Provided                           |
        //   | Right to Restrict   | How to request access restrictions            |
        //   | Right to Complain   | How to file privacy complaints                |
        //   | Contact Information | Privacy officer contact details               |
        let patient_rights = vec![
            row([("label", "Right to Restrict"), ("value", "How to request access restrictions")]),
            row([("label", "Right to Complain"), ("value", "How to file privacy complaints")]),
            row([("label", "Contact Information"), ("value", "Privacy officer contact details")]),
        ];
        for row_data in &patient_rights {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }
        Ok(())
    })
}

fn scenario_05_monthly_compliance_audit_report_for_department_oversight(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given it is the first week of July 2025
        // And I need to generate the monthly compliance report for June
        // When I run the comprehensive monthly audit analysis
        page.get_by_test_id("run-monthly-audit-analysis-button").first().click(None).await?;

        // Then the system provides departmental access statistics:
        //   | Access Metric       | June 2025 Statistics                           |
        //   | Total Record Access | 15,847 patient record accesses                |
        //   | Unique Users        | 156 healthcare providers                       |
        //   | Average Session     | 12 minutes 34 seconds                         |
        //   | After-hours Access  | 892 accesses (5.6% of total)                 |
        //   | Emergency Override  | 12 break-the-glass accesses                  |
        let access_statistics = vec![
            row([("label", "Total Record Access"), ("value", "15,847 patient record accesses")]),
            row([("label", "Unique Users"), ("value", "156 healthcare providers")]),
            row([("label", "Average Session"), ("value", "12 minutes 34 seconds")]),
            row([("label", "After-hours Access"), ("value", "892 accesses (5.6% of total)")]),
            row([("label", "Emergency Override"), ("value", "12 break-the-glass accesses")]),
        ];
        for row_data in &access_statistics {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }

        // And compliance indicators are summarized:
        //   | Compliance Area     | Status                                         |
        //   | Appropriate Access  | 99.2% of accesses had documented treatment relationship|
        //   | Minimum Necessary   | 98.7% accessed only required data elements    |
        //   | Timely Documentation| 99.8% of access properly documented within 24 hours|
        //   | Unauthorized Access | 0.3% flagged for investigation (47 instances) |
        let compliance_indicators = vec![
            row([("label", "Appropriate Access"), ("value", "99.2% of accesses had documented treatment relationship")]),
            row([("label", "Minimum Necessary Compliance"), ("value", "98.7% accessed only required data elements")]),
            row([("label", "Timely Documentation"), ("value", "99.8% of access properly documented within 24 hours")]),
            row([("label", "Unauthorized Access"), ("value", "0.3% flagged for investigation (47 instances)")]),
        ];
        for row_data in &compliance_indicators {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }

        // And trends and patterns are identified:
        //   | Trend Analysis      | Findings                                       |
        //   | Access Volume       | 15% increase from May (normal seasonal pattern)|
        //   | User Compliance     | 2 users require additional HIPAA training     |
        //   | System Performance  | No audit logging failures detected            |
        //   | Policy Adherence    | 99.1% compliance with access policies         |
        let trends_and_patterns = vec![
            row([("label", "Access Volume"), ("value", "15% increase from May (normal seasonal pattern)")]),
            row([("label", "User Compliance"), ("value", "2 users require additional HIPAA training")]),
            row([("label", "System Performance"), ("value", "No audit logging failures detected")]),
            row([("label", "Policy Adherence"), ("value", "99.1% compliance with access policies")]),
        ];
        for row_data in &trends_and_patterns {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }

        // And recommendations for improvement are provided:
        //   | Improvement Area    | Recommendation                                 |
        //   | User Training       | Schedule refresher training for 2 staff members|
        //   | Policy Updates      | Review after-hours access procedures          |
        //   | System Enhancement  | Consider additional automated monitoring       |
        //   | Process Improvement | Streamline emergency access documentation     |
        let improvement_recommendations = vec![
            row([("label", "User Training"), ("value", "Schedule refresher training for 2 staff members")]),
            row([("label", "Policy Updates"), ("value", "Review after-hours access procedures")]),
            row([("label", "System Enhancement"), ("value", "Consider additional automated monitoring")]),
            row([("label", "Process Improvement"), ("value", "Streamline emergency access documentation")]),
        ];
        for row_data in &improvement_recommendations {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }
        Ok(())
    })
}

fn scenario_06_investigate_potential_data_breach_with_forensic_audit_trail(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given there are concerns about a potential data security incident
        // And multiple patient records may have been inappropriately accessed
        // And law enforcement has requested detailed audit information
        // When I conduct a forensic audit investigation
        page.get_by_test_id("conduct-forensic-audit-button").first().click(None).await?;
        // And I analyze access patterns from June 1-30, 2025
        fill_field(page, "Start Date", "2025-06-01").await?;
        fill_field(page, "End Date", "2025-06-30").await?;

        // Then the system provides comprehensive forensic data:
        //   | Forensic Element    | Investigation Data                             |
        //   | User Activity       | Detailed timeline of all user actions         |
        //   | Data Accessed       | Specific patient information viewed/modified   |
        //   | System Interactions | Every click, search, and data retrieval       |
        //   | Network Activity    | IP addresses, network connections, file transfers|
        //   | Concurrent Sessions | Multiple simultaneous access attempts         |
        let forensic_elements = vec![
            row([("label", "User Activity"), ("value", "Detailed timeline of all user actions")]),
            row([("label", "Data Accessed"), ("value", "Specific patient information viewed/modified")]),
            row([("label", "System Interactions"), ("value", "Every click, search, and data retrieval")]),
            row([("label", "Network Activity"), ("value", "IP addresses, network connections, file transfers")]),
            row([("label", "Concurrent Sessions"), ("value", "Multiple simultaneous access attempts")]),
        ];
        for row_data in &forensic_elements {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }

        // And security indicators are analyzed:
        //   | Security Indicator  | Analysis Results                               |
        //   | Unusual Patterns    | 15 users accessed >100 records in one day    |
        //   | Off-site Access     | 23 connections from non-hospital IP addresses |
        //   | Data Export Activity| 5 instances of bulk data downloads            |
        //   | Failed Login Attempts| 247 failed logins from external IPs          |
        let security_indicators = vec![
            row([("label", "Unusual Patterns"), ("value", "15 users accessed >100 records in one day")]),
            row([("label", "Off-site Access"), ("value", "23 connections from non-hospital IP addresses")]),
            row([("label", "Data Export Activity"), ("value", "5 instances of bulk data downloads")]),
            row([("label", "Failed Login Attempts"), ("value", "247 failed logins from external IPs")]),
        ];
        for row_data in &security_indicators {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }

        // And evidence preservation procedures are documented:
        //   | Evidence Type       | Preservation Method                            |
        //   | Audit Logs          | Tamper-proof digital signature applied        |
        //   | System Snapshots    | Full system state captured and archived       |
        //   | User Account Data   | Complete account history preserved            |
        //   | Network Logs        | Network traffic logs secured for analysis     |
        let evidence_preservation = vec![
            row([("label", "Audit Logs"), ("value", "Tamper-proof digital signature applied")]),
            row([("label", "System Snapshots"), ("value", "Full system state captured and archived")]),
            row([("label", "User Account Data"), ("value", "Complete account history preserved")]),
            row([("label", "Network Logs"), ("value", "Network traffic logs secured for analysis")]),
        ];
        for row_data in &evidence_preservation {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }

        // And legal compliance requirements are met:
        //   | Legal Requirement   | Compliance Action                              |
        //   | Chain of Custody    | Documented evidence handling procedures        |
        //   | Data Integrity      | Cryptographic verification of audit data      |
        //   | Discovery Response  | Legal hold procedures activated               |
        //   | Regulatory Reporting| Breach notification procedures initiated      |
        let legal_compliance = vec![
            row([("label", "Chain of Custody"), ("value", "Documented evidence handling procedures")]),
            row([("label", "Data Integrity"), ("value", "Cryptographic verification of audit data")]),
            row([("label", "Discovery Response"), ("value", "Legal hold procedures activated")]),
            row([("label", "Regulatory Reporting"), ("value", "Breach notification procedures initiated")]),
        ];
        for row_data in &legal_compliance {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(page, label).await?, value);
        }
        Ok(())
    })
}

fn main() {
    run_feature(&[
        ("Review comprehensive access log for frequently accessed patient record", scenario_01_review_comprehensive_access_log_for_frequently_accessed_pati),
        ("Investigate suspicious access pattern for patient record", scenario_02_investigate_suspicious_access_pattern_for_patient_record),
        ("Generate audit report for break-the-glass emergency access", scenario_03_generate_audit_report_for_break_the_glass_emergency_access),
        ("Audit trail for patient who requested access log of their own record", scenario_04_audit_trail_for_patient_who_requested_access_log_of_their_ow),
        ("Monthly compliance audit report for department oversight", scenario_05_monthly_compliance_audit_report_for_department_oversight),
        ("Investigate potential data breach with forensic audit trail", scenario_06_investigate_potential_data_breach_with_forensic_audit_trail),
    ]);
}
