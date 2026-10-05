// Selenium WebDriver + libtest-mimic test for
// tests-with-given-when-then-features/12-allergy-check.feature
// (equivalent to tests-with-selenium-javascript/12-allergy-check.test.js).
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
    //   And I am logged in as "Dr. Smith"
    //   And the allergy checking module is active
    //   And the drug interaction database is up-to-date
    verify_system_is_operational(driver).await?;
    login(driver, "Dr. Smith", false).await?;
    // The allergy checking module and the drug interaction database being
    // up-to-date are assumed to be pre-seeded test environment state.

    let allergy_check_nav_link = wait_for_test_id(driver, "Nav Allergy Check").await?;
    allergy_check_nav_link.click().await?;
    wait_for_test_id(driver, "Allergy Check Panel").await?;
    Ok(())
}

fn scenario_01_prescribe_penicillin_to_patient_with_documented_penicillin_a(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a patient "Maria Rodriguez" is in bed "ED-8"
        // And the patient has documented allergies:
        //   | Allergy        | Reaction Type       | Severity  | Date Documented | Source     |
        //   | Penicillin     | Rash, hives        | Moderate  | 2023-05-15     | Patient    |
        //   | Shellfish      | Anaphylaxis        | Severe    | 2022-08-10     | Patient    |

        // When I enter a medication order for:
        //   | Medication     | Dose      | Route | Frequency | Duration |
        //   | Penicillin VK  | 500mg     | PO    | QID       | 10 days  |
        fill_fields(driver, &vec![
            row([("Field", "Medication"), ("Value", "Penicillin VK")]),
            row([("Field", "Dose"), ("Value", "500mg")]),
            row([("Field", "Route"), ("Value", "PO")]),
            row([("Field", "Frequency"), ("Value", "QID")]),
            row([("Field", "Duration"), ("Value", "10 days")]),
        ]).await?;
        // And I submit the order
        driver.find(By::Css("[data-testid=\"submit-medication-order\"]")).await?.click().await?;

        // Then the system displays an allergy warning:
        //   | Alert Type         | Details                                      |
        //   | DRUG ALLERGY       | ⚠️ ALLERGY ALERT: Patient allergic to Penicillin |
        //   | Severity Level     | Moderate                                     |
        //   | Reaction Type      | Rash, hives                                  |
        //   | Date Documented    | May 15, 2023                                |
        //   | Source             | Patient reported                             |
        wait_for_test_id(driver, "DRUG ALLERGY").await?;
        assert_eq!(get_text(driver, "DRUG ALLERGY").await?, "⚠️ ALLERGY ALERT: Patient allergic to Penicillin");
        assert_eq!(get_text(driver, "Severity Level").await?, "Moderate");
        assert_eq!(get_text(driver, "Reaction Type").await?, "Rash, hives");
        assert_eq!(get_text(driver, "Date Documented").await?, "May 15, 2023");
        assert_eq!(get_text(driver, "Source").await?, "Patient reported");

        // And the system blocks the order submission
        let order_status = get_text(driver, "Order Status").await?;
        assert_match(&order_status, r"blocked", true);

        // And I am presented with options:
        //   | Option             | Description                                  |
        //   | Cancel Order       | Remove penicillin order                      |
        //   | Override with Reason| Document clinical justification            |
        //   | Alternative Drugs  | View suggested alternative antibiotics       |
        wait_for_test_id(driver, "Cancel Order").await?;
        wait_for_test_id(driver, "Override with Reason").await?;
        wait_for_test_id(driver, "Alternative Drugs").await?;

        // And the allergy alert is logged in the audit trail
        let audit_trail_entry = wait_for_test_id(driver, "Audit Trail Entry").await?;
        assert!(audit_trail_entry.is_displayed().await?);
        Ok(())
    })
}

fn scenario_02_prescribe_medication_with_no_documented_allergies(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a patient "John Taylor" is in bed "ED-12"
        // And the patient has no documented allergies

        // When I enter a medication order for:
        //   | Medication     | Dose      | Route | Frequency |
        //   | Amoxicillin    | 875mg     | PO    | BID       |
        fill_fields(driver, &vec![
            row([("Field", "Medication"), ("Value", "Amoxicillin")]),
            row([("Field", "Dose"), ("Value", "875mg")]),
            row([("Field", "Route"), ("Value", "PO")]),
            row([("Field", "Frequency"), ("Value", "BID")]),
        ]).await?;
        // And I submit the order
        driver.find(By::Css("[data-testid=\"submit-medication-order\"]")).await?.click().await?;

        // Then the system performs allergy checking
        // And no allergy alerts are triggered
        let allergy_alerts = driver.find_all(By::Css("[data-testid=\"allergy-alert\"]")).await?;
        assert_eq!(allergy_alerts.len(), 0);

        // And the order is processed normally
        // And the system displays confirmation:
        //   | Confirmation Type  | Message                                      |
        //   | No Allergies Found | No known allergies to Amoxicillin          |
        //   | Order Status       | Order submitted successfully                 |
        wait_for_test_id(driver, "No Allergies Found").await?;
        assert_eq!(get_text(driver, "No Allergies Found").await?, "No known allergies to Amoxicillin");
        assert_eq!(get_text(driver, "Order Status").await?, "Order submitted successfully");

        // And the medication order is routed to pharmacy
        let pharmacy_routing_status = get_text(driver, "Pharmacy Routing Status").await?;
        assert_match(&pharmacy_routing_status, r"pharmacy", true);
        Ok(())
    })
}

fn scenario_03_prescribe_medication_with_cross_reactive_allergy(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a patient "Sarah Johnson" is in bed "ED-5"
        // And the patient has documented allergies:
        //   | Allergy        | Reaction Type       | Severity  |
        //   | Penicillin     | Respiratory distress| Severe    |

        // When I enter a medication order for:
        //   | Medication     | Dose      | Route | Frequency |
        //   | Amoxicillin    | 500mg     | PO    | TID       |
        fill_fields(driver, &vec![
            row([("Field", "Medication"), ("Value", "Amoxicillin")]),
            row([("Field", "Dose"), ("Value", "500mg")]),
            row([("Field", "Route"), ("Value", "PO")]),
            row([("Field", "Frequency"), ("Value", "TID")]),
        ]).await?;
        // And I submit the order
        driver.find(By::Css("[data-testid=\"submit-medication-order\"]")).await?.click().await?;

        // Then the system displays a cross-reactivity warning:
        //   | Alert Type         | Details                                      |
        //   | CROSS-REACTIVITY   | ⚠️ WARNING: Cross-reactivity with Penicillin|
        //   | Known Allergy      | Patient allergic to Penicillin (Severe)     |
        //   | Cross-Reaction Risk| Amoxicillin is a penicillin derivative     |
        //   | Reaction Type      | Respiratory distress                         |
        //   | Risk Level         | High - Severe reaction possible              |
        wait_for_test_id(driver, "CROSS-REACTIVITY").await?;
        assert_eq!(get_text(driver, "CROSS-REACTIVITY").await?, "⚠️ WARNING: Cross-reactivity with Penicillin");
        assert_eq!(get_text(driver, "Known Allergy").await?, "Patient allergic to Penicillin (Severe)");
        assert_eq!(get_text(driver, "Cross-Reaction Risk").await?, "Amoxicillin is a penicillin derivative");
        assert_eq!(get_text(driver, "Reaction Type").await?, "Respiratory distress");
        assert_eq!(get_text(driver, "Risk Level").await?, "High - Severe reaction possible");

        // And the system provides additional information:
        //   | Information Type   | Content                                      |
        //   | Cross-Reaction Rate| 8-10% cross-reactivity with penicillin     |
        //   | Clinical Guidance  | Consider non-beta-lactam alternatives       |
        //   | Emergency Prep     | Have epinephrine available if administered   |
        assert_eq!(get_text(driver, "Cross-Reaction Rate").await?, "8-10% cross-reactivity with penicillin");
        assert_eq!(get_text(driver, "Clinical Guidance").await?, "Consider non-beta-lactam alternatives");
        assert_eq!(get_text(driver, "Emergency Prep").await?, "Have epinephrine available if administered");

        // And I must acknowledge the cross-reactivity risk before proceeding
        wait_for_test_id(driver, "Acknowledge Cross-Reactivity Risk").await?;
        Ok(())
    })
}

fn scenario_04_override_allergy_alert_with_clinical_justification(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a patient "Michael Chen" is in bed "ED-15"
        // And the patient has a documented penicillin allergy with "mild rash"
        // And the patient has severe sepsis requiring immediate antibiotic treatment

        // When I enter a penicillin order and receive an allergy alert
        fill_field(driver, "Medication", "Penicillin").await?;
        driver.find(By::Css("[data-testid=\"submit-medication-order\"]")).await?.click().await?;
        wait_for_test_id(driver, "DRUG ALLERGY").await?;
        // And I choose to override the allergy warning
        driver.find(By::Css("[data-testid=\"override-allergy-warning\"]")).await?.click().await?;

        // Then the system requires detailed justification:
        //   | Required Field     | Description                                  |
        //   | Clinical Rationale | Why this medication is medically necessary   |
        //   | Risk Assessment    | Evaluation of allergy risk vs benefit       |
        //   | Monitoring Plan    | How allergic reactions will be monitored    |
        //   | Alternative Review | Why alternatives are not suitable            |
        wait_for_test_id(driver, "Clinical Rationale Description").await?;
        assert_eq!(get_text(driver, "Clinical Rationale Description").await?, "Why this medication is medically necessary");
        assert_eq!(get_text(driver, "Risk Assessment Description").await?, "Evaluation of allergy risk vs benefit");
        assert_eq!(get_text(driver, "Monitoring Plan Description").await?, "How allergic reactions will be monitored");
        assert_eq!(get_text(driver, "Alternative Review Description").await?, "Why alternatives are not suitable");

        // And I document the override:
        //   | Field              | Value                                        |
        //   | Clinical Rationale | Life-threatening sepsis, first-line antibiotic needed |
        //   | Risk Assessment    | Mild rash risk acceptable vs sepsis mortality |
        //   | Monitoring Plan    | Continuous monitoring, diphenhydramine available |
        //   | Alternative Review | Other antibiotics inadequate for organism     |
        fill_fields(driver, &vec![
            row([("Field", "Clinical Rationale"), ("Value", "Life-threatening sepsis, first-line antibiotic needed")]),
            row([("Field", "Risk Assessment"), ("Value", "Mild rash risk acceptable vs sepsis mortality")]),
            row([("Field", "Monitoring Plan"), ("Value", "Continuous monitoring, diphenhydramine available")]),
            row([("Field", "Alternative Review"), ("Value", "Other antibiotics inadequate for organism")]),
        ]).await?;
        driver.find(By::Css("[data-testid=\"submit-override-documentation\"]")).await?.click().await?;

        // Then the system accepts the override
        wait_for_test_id(driver, "Override Status").await?;
        let override_status = get_text(driver, "Override Status").await?;
        assert_match(&override_status, r"accepted", true);

        // And logs the override decision with full documentation
        let override_audit_log = wait_for_test_id(driver, "Override Audit Log").await?;
        assert!(override_audit_log.is_displayed().await?);

        // And notifies nursing staff of the allergy override for enhanced monitoring
        let nursing_notification = get_text(driver, "Nursing Notification").await?;
        assert_match(&nursing_notification, r"enhanced monitoring", true);
        Ok(())
    })
}

fn scenario_05_check_allergies_for_multiple_medications_simultaneously(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a patient "Lisa Brown" is in bed "ED-7"
        // And the patient has documented allergies:
        //   | Allergy        | Reaction Type       | Severity  |
        //   | Morphine       | Respiratory depression | Severe |
        //   | NSAIDs         | GI bleeding        | Moderate  |

        // When I enter multiple medication orders:
        //   | Medication     | Dose      | Route | Purpose           |
        //   | Fentanyl       | 50mcg     | IV    | Pain control      |
        //   | Ibuprofen      | 600mg     | PO    | Anti-inflammatory |
        //   | Acetaminophen  | 650mg     | PO    | Pain/fever        |
        fill_fields(driver, &vec![
            row([("Field", "Medication 1"), ("Value", "Fentanyl")]),
            row([("Field", "Dose 1"), ("Value", "50mcg")]),
            row([("Field", "Route 1"), ("Value", "IV")]),
            row([("Field", "Purpose 1"), ("Value", "Pain control")]),
            row([("Field", "Medication 2"), ("Value", "Ibuprofen")]),
            row([("Field", "Dose 2"), ("Value", "600mg")]),
            row([("Field", "Route 2"), ("Value", "PO")]),
            row([("Field", "Purpose 2"), ("Value", "Anti-inflammatory")]),
            row([("Field", "Medication 3"), ("Value", "Acetaminophen")]),
            row([("Field", "Dose 3"), ("Value", "650mg")]),
            row([("Field", "Route 3"), ("Value", "PO")]),
            row([("Field", "Purpose 3"), ("Value", "Pain/fever")]),
        ]).await?;
        // And I submit all orders simultaneously
        driver.find(By::Css("[data-testid=\"submit-medication-orders\"]")).await?.click().await?;

        // Then the system checks each medication against documented allergies:
        //   | Medication     | Allergy Status | Alert Level |
        //   | Fentanyl       | No direct allergy | Safe      |
        //   | Ibuprofen      | NSAID allergy  | WARNING   |
        //   | Acetaminophen  | No allergy     | Safe      |
        wait_for_test_id(driver, "Fentanyl Alert Level").await?;
        assert_eq!(get_text(driver, "Fentanyl Allergy Status").await?, "No direct allergy");
        assert_eq!(get_text(driver, "Fentanyl Alert Level").await?, "Safe");
        assert_eq!(get_text(driver, "Ibuprofen Allergy Status").await?, "NSAID allergy");
        assert_eq!(get_text(driver, "Ibuprofen Alert Level").await?, "WARNING");
        assert_eq!(get_text(driver, "Acetaminophen Allergy Status").await?, "No allergy");
        assert_eq!(get_text(driver, "Acetaminophen Alert Level").await?, "Safe");

        // And I receive specific alerts for problematic medications:
        //   | Alert Medication | Warning Message                              |
        //   | Ibuprofen        | Patient allergic to NSAIDs - GI bleeding risk |
        assert_eq!(get_text(driver, "Ibuprofen Warning Message").await?, "Patient allergic to NSAIDs - GI bleeding risk");

        // And safe medications are processed without alerts
        let fentanyl_warnings = driver.find_all(By::Css("[data-testid=\"fentanyl-warning-message\"]")).await?;
        assert_eq!(fentanyl_warnings.len(), 0);

        // And I can review and modify orders before final submission
        wait_for_test_id(driver, "Review and Modify Orders").await?;
        Ok(())
    })
}

fn scenario_06_handle_unknown_or_no_known_allergies_status(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a patient "Robert Davis" is in bed "ED-3"
        // And the patient's allergy status is "Unknown - Unable to assess"

        // When I enter a medication order for:
        //   | Medication     | Dose      | Route |
        //   | Cephalexin     | 500mg     | PO    |
        fill_fields(driver, &vec![
            row([("Field", "Medication"), ("Value", "Cephalexin")]),
            row([("Field", "Dose"), ("Value", "500mg")]),
            row([("Field", "Route"), ("Value", "PO")]),
        ]).await?;
        // And I submit the order
        driver.find(By::Css("[data-testid=\"submit-medication-order\"]")).await?.click().await?;

        // Then the system displays an information alert:
        //   | Alert Type         | Message                                      |
        //   | ALLERGY UNKNOWN    | ⚠️ INFO: Patient allergy status unknown     |
        //   | Risk Consideration | Cannot verify medication allergies          |
        //   | Recommendation     | Consider allergy assessment before administration |
        wait_for_test_id(driver, "ALLERGY UNKNOWN").await?;
        assert_eq!(get_text(driver, "ALLERGY UNKNOWN").await?, "⚠️ INFO: Patient allergy status unknown");
        assert_eq!(get_text(driver, "Risk Consideration").await?, "Cannot verify medication allergies");
        assert_eq!(get_text(driver, "Recommendation").await?, "Consider allergy assessment before administration");

        // And the system provides safety recommendations:
        //   | Recommendation     | Details                                      |
        //   | Allergy Assessment | Attempt to obtain allergy history           |
        //   | Start Monitoring   | Monitor for allergic reactions closely      |
        //   | Have Antidotes Ready| Ensure emergency medications available      |
        assert_eq!(get_text(driver, "Allergy Assessment").await?, "Attempt to obtain allergy history");
        assert_eq!(get_text(driver, "Start Monitoring").await?, "Monitor for allergic reactions closely");
        assert_eq!(get_text(driver, "Have Antidotes Ready").await?, "Ensure emergency medications available");

        // And the order is flagged for enhanced allergy monitoring
        let enhanced_monitoring_flag = get_text(driver, "Enhanced Monitoring Flag").await?;
        assert_match(&enhanced_monitoring_flag, r"enhanced allergy monitoring", true);
        Ok(())
    })
}

fn scenario_07_check_for_drug_class_allergies(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a patient "Jennifer Wilson" is in bed "ED-11"
        // And the patient has documented allergies:
        //   | Allergy        | Reaction Type       | Severity  | Drug Class |
        //   | Sulfa drugs    | Stevens-Johnson syndrome | Severe | Sulfonamides |

        // When I enter a medication order for:
        //   | Medication           | Dose      | Route | Drug Class    |
        //   | Trimethoprim-Sulfamethoxazole | 800mg | PO | Sulfonamide |
        fill_fields(driver, &vec![
            row([("Field", "Medication"), ("Value", "Trimethoprim-Sulfamethoxazole")]),
            row([("Field", "Dose"), ("Value", "800mg")]),
            row([("Field", "Route"), ("Value", "PO")]),
            row([("Field", "Drug Class"), ("Value", "Sulfonamide")]),
        ]).await?;
        // And I submit the order
        driver.find(By::Css("[data-testid=\"submit-medication-order\"]")).await?.click().await?;

        // Then the system identifies the drug class allergy:
        //   | Alert Type         | Details                                      |
        //   | DRUG CLASS ALLERGY | ⚠️ SEVERE: Patient allergic to Sulfa drugs |
        //   | Specific Drug      | TMP-SMX contains sulfamethoxazole           |
        //   | Reaction History   | Stevens-Johnson syndrome                     |
        //   | Severity           | Severe - Life-threatening reaction possible  |
        wait_for_test_id(driver, "DRUG CLASS ALLERGY").await?;
        assert_eq!(get_text(driver, "DRUG CLASS ALLERGY").await?, "⚠️ SEVERE: Patient allergic to Sulfa drugs");
        assert_eq!(get_text(driver, "Specific Drug").await?, "TMP-SMX contains sulfamethoxazole");
        assert_eq!(get_text(driver, "Reaction History").await?, "Stevens-Johnson syndrome");
        assert_eq!(get_text(driver, "Severity").await?, "Severe - Life-threatening reaction possible");

        // And the system provides drug class education:
        //   | Information        | Content                                      |
        //   | Drug Class         | Sulfonamide antibiotics                     |
        //   | Cross-Reactivity   | All sulfa-containing medications at risk    |
        //   | Alternative Classes| Beta-lactams, fluoroquinolones available   |
        assert_eq!(get_text(driver, "Drug Class").await?, "Sulfonamide antibiotics");
        assert_eq!(get_text(driver, "Cross-Reactivity").await?, "All sulfa-containing medications at risk");
        assert_eq!(get_text(driver, "Alternative Classes").await?, "Beta-lactams, fluoroquinolones available");
        Ok(())
    })
}

fn scenario_08_handle_allergy_information_from_multiple_sources(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a patient "David Kim" is in bed "ED-9"
        // And the patient has allergy information from multiple sources:
        //   | Source             | Allergy    | Reaction        | Reliability |
        //   | Patient Report     | Penicillin | "Bad reaction"  | Unverified  |
        //   | Medical Records    | Penicillin | Urticaria, rash | Verified    |
        //   | Family Member      | Codeine    | Nausea         | Unverified  |

        // When I enter a penicillin order
        fill_field(driver, "Medication", "Penicillin").await?;
        driver.find(By::Css("[data-testid=\"submit-medication-order\"]")).await?.click().await?;

        // Then the system displays comprehensive allergy information:
        //   | Source Type        | Allergy Details                              |
        //   | Verified Record    | Penicillin - Urticaria, rash (Medical Records) |
        //   | Patient Report     | Penicillin - "Bad reaction" (Unverified)    |
        wait_for_test_id(driver, "Verified Record").await?;
        assert_eq!(get_text(driver, "Verified Record").await?, "Penicillin - Urticaria, rash (Medical Records)");
        assert_eq!(get_text(driver, "Patient Report").await?, "Penicillin - \"Bad reaction\" (Unverified)");

        // And the system prioritizes verified information in the alert
        // And provides source credibility indicators:
        //   | Source             | Credibility Level | Clinical Weight      |
        //   | Medical Records    | High reliability  | Primary consideration |
        //   | Patient Report     | Moderate reliability | Secondary consideration |
        assert_eq!(get_text(driver, "Medical Records Credibility Level").await?, "High reliability");
        assert_eq!(get_text(driver, "Medical Records Clinical Weight").await?, "Primary consideration");
        assert_eq!(get_text(driver, "Patient Report Credibility Level").await?, "Moderate reliability");
        assert_eq!(get_text(driver, "Patient Report Clinical Weight").await?, "Secondary consideration");

        // And I can review detailed allergy history before making decisions
        wait_for_test_id(driver, "Detailed Allergy History").await?;
        Ok(())
    })
}

fn scenario_09_real_time_allergy_checking_during_order_modification(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given a patient "Susan Martinez" is in bed "ED-4"
        // And the patient has a penicillin allergy
        // And I have started entering a medication order

        // When I begin typing "Pen" in the medication field
        fill_field(driver, "Medication", "Pen").await?;

        // Then the system provides real-time allergy warnings:
        //   | Alert Type         | Message                                      |
        //   | PREDICTIVE ALERT   | ⚠️ Patient allergic to Penicillin          |
        //   | Medication Match   | "Pen" may be penicillin-related drug       |
        //   | Suggestion         | Consider alternative antibiotics            |
        wait_for_test_id(driver, "PREDICTIVE ALERT").await?;
        assert_eq!(get_text(driver, "PREDICTIVE ALERT").await?, "⚠️ Patient allergic to Penicillin");
        assert_eq!(get_text(driver, "Medication Match").await?, "\"Pen\" may be penicillin-related drug");
        assert_eq!(get_text(driver, "Suggestion").await?, "Consider alternative antibiotics");

        // And the system highlights potential allergy matches as I type
        wait_for_test_id(driver, "Allergy Match Highlight").await?;

        // And provides alternative medication suggestions:
        //   | Alternative        | Drug Class        | Reason               |
        //   | Cephalexin         | Cephalosporin     | Lower cross-reactivity |
        //   | Azithromycin       | Macrolide         | No cross-reactivity   |
        //   | Ciprofloxacin      | Fluoroquinolone   | Different mechanism   |
        assert_eq!(get_text(driver, "Cephalexin Drug Class").await?, "Cephalosporin");
        assert_eq!(get_text(driver, "Cephalexin Reason").await?, "Lower cross-reactivity");
        assert_eq!(get_text(driver, "Azithromycin Drug Class").await?, "Macrolide");
        assert_eq!(get_text(driver, "Azithromycin Reason").await?, "No cross-reactivity");
        assert_eq!(get_text(driver, "Ciprofloxacin Drug Class").await?, "Fluoroquinolone");
        assert_eq!(get_text(driver, "Ciprofloxacin Reason").await?, "Different mechanism");

        // And I can select alternatives directly from the suggestion list
        wait_for_test_id(driver, "Alternative Suggestion List").await?;
        Ok(())
    })
}

fn main() {
    run_feature(&[
        ("Prescribe penicillin to patient with documented penicillin allergy", scenario_01_prescribe_penicillin_to_patient_with_documented_penicillin_a),
        ("Prescribe medication with no documented allergies", scenario_02_prescribe_medication_with_no_documented_allergies),
        ("Prescribe medication with cross-reactive allergy", scenario_03_prescribe_medication_with_cross_reactive_allergy),
        ("Override allergy alert with clinical justification", scenario_04_override_allergy_alert_with_clinical_justification),
        ("Check allergies for multiple medications simultaneously", scenario_05_check_allergies_for_multiple_medications_simultaneously),
        ("Handle unknown or \"No Known Allergies\" status", scenario_06_handle_unknown_or_no_known_allergies_status),
        ("Check for drug class allergies", scenario_07_check_for_drug_class_allergies),
        ("Handle allergy information from multiple sources", scenario_08_handle_allergy_information_from_multiple_sources),
        ("Real-time allergy checking during order modification", scenario_09_real_time_allergy_checking_during_order_modification),
    ]);
}
