// Playwright + libtest-mimic test for
// tests-with-given-when-then-features/11-medication-administration.feature
// (equivalent to tests-with-playwright-javascript/11-medication-administration.test.js).
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
    //   And I am logged in as "Nurse Johnson"
    //   And the medication administration module is active
    //   And the barcode scanning system is functional
    //   And the medication administration record (MAR) is accessible
    verify_system_is_operational(page).await?;
    login(page, "Nurse Johnson", false).await?;
    // The medication administration module, barcode scanning system, and
    // MAR accessibility are assumed to be pre-seeded test environment state.

    let medication_administration_nav_link = wait_for_test_id(page, "Nav Medication Administration").await?;
    medication_administration_nav_link.click(None).await?;
    wait_for_test_id(page, "Medication Administration Panel").await?;
    Ok(())
}

fn scenario_01_successfully_administer_scheduled_medication_with_barcode_ve(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given a patient "Maria Gonzalez" is in bed "ED-8"
        // And the patient has a prescribed medication:
        //   | Medication    | Dose      | Route | Frequency | Scheduled Time | Status    |
        //   | Metoprolol    | 25mg      | PO    | BID       | 14:00         | Due       |
        // And the patient is wearing a wristband with barcode "PT-12345"
        // And I have the medication with barcode "MED-METOPROLOL-25MG"

        // When I scan the patient's wristband barcode "PT-12345"
        page.get_by_test_id("scan-wristband-barcode").first().click(None).await?;
        // And I scan the medication barcode "MED-METOPROLOL-25MG"
        page.get_by_test_id("scan-medication-barcode").first().click(None).await?;

        // Then the system verifies the five rights of medication administration:
        //   | Right          | Verification                                  | Status   |
        //   | Right Patient  | Maria Gonzalez confirmed via wristband       | ✓ Valid  |
        //   | Right Drug     | Metoprolol matches prescribed medication     | ✓ Valid  |
        //   | Right Dose     | 25mg matches prescribed dose                 | ✓ Valid  |
        //   | Right Route    | PO (oral) matches prescribed route           | ✓ Valid  |
        //   | Right Time     | Within acceptable window (13:30-14:30)      | ✓ Valid  |
        wait_for_test_id(page, "Right Patient Status").await?;
        assert_eq!(get_text(page, "Right Patient Status").await?, "✓ Valid");
        assert_eq!(get_text(page, "Right Drug Status").await?, "✓ Valid");
        assert_eq!(get_text(page, "Right Dose Status").await?, "✓ Valid");
        assert_eq!(get_text(page, "Right Route Status").await?, "✓ Valid");
        assert_eq!(get_text(page, "Right Time Status").await?, "✓ Valid");

        // And the system displays confirmation screen:
        //   | Field             | Value                                        |
        //   | Patient           | Maria Gonzalez, DOB: 1975-08-15            |
        //   | Medication        | Metoprolol 25mg                            |
        //   | Route             | PO (By mouth)                              |
        //   | Administration Time| 14:05                                      |
        assert_eq!(get_text(page, "Patient").await?, "Maria Gonzalez, DOB: 1975-08-15");
        assert_eq!(get_text(page, "Medication").await?, "Metoprolol 25mg");
        assert_eq!(get_text(page, "Route").await?, "PO (By mouth)");
        assert_eq!(get_text(page, "Administration Time").await?, "14:05");

        // And I confirm the administration
        page.get_by_test_id("confirm-administration").first().click(None).await?;

        // Then the system records the administration with timestamp:
        //   | Field             | Value                                        |
        //   | Patient ID        | PT-12345                                   |
        //   | Medication        | Metoprolol 25mg PO                         |
        //   | Administered By   | Nurse Johnson                              |
        //   | Administration Time| 2025-06-24 14:05:32                       |
        //   | Verification Method| Barcode scan                              |
        wait_for_test_id(page, "Patient ID").await?;
        assert_eq!(get_text(page, "Patient ID").await?, "PT-12345");
        assert_eq!(get_text(page, "Medication").await?, "Metoprolol 25mg PO");
        assert_eq!(get_text(page, "Administered By").await?, "Nurse Johnson");
        assert_eq!(get_text(page, "Administration Time").await?, "2025-06-24 14:05:32");
        assert_eq!(get_text(page, "Verification Method").await?, "Barcode scan");

        // And the medication status is updated to "Given"
        assert_eq!(get_text(page, "Medication Status").await?, "Given");
        // And the next dose is automatically scheduled for "02:00 tomorrow"
        assert_eq!(get_text(page, "Next Dose Scheduled").await?, "02:00 tomorrow");
        Ok(())
    })
}

fn scenario_02_handle_medication_administration_with_allergy_alert(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given a patient "Robert Chen" is in bed "ED-12"
        // And the patient has documented allergies:
        //   | Allergy       | Reaction Type      | Severity  |
        //   | Penicillin    | Rash, hives       | Moderate  |
        //   | Morphine      | Respiratory depression | Severe |
        // And there is a prescribed medication:
        //   | Medication    | Dose      | Route | Prescriber |
        //   | Amoxicillin   | 500mg     | PO    | Dr. Smith  |

        // When I scan the patient's wristband barcode
        page.get_by_test_id("scan-wristband-barcode").first().click(None).await?;
        // And I scan the amoxicillin medication barcode
        page.get_by_test_id("scan-medication-barcode").first().click(None).await?;

        // Then the system displays an allergy alert:
        //   | Alert Type        | Message                                      |
        //   | DRUG ALLERGY      | ⚠️ WARNING: Patient allergic to Penicillin |
        //   | Cross-Reaction    | Amoxicillin contains penicillin             |
        //   | Severity          | Moderate - Rash, hives                      |
        //   | Recommendation    | Contact physician before administration      |
        wait_for_test_id(page, "DRUG ALLERGY").await?;
        assert_eq!(get_text(page, "DRUG ALLERGY").await?, "⚠️ WARNING: Patient allergic to Penicillin");
        assert_eq!(get_text(page, "Cross-Reaction").await?, "Amoxicillin contains penicillin");
        assert_eq!(get_text(page, "Severity").await?, "Moderate - Rash, hives");
        assert_eq!(get_text(page, "Recommendation").await?, "Contact physician before administration");

        // And the system requires additional verification:
        //   | Verification Step | Requirement                                  |
        //   | Physician Approval| Must have override from prescribing MD      |
        //   | Documentation     | Must document reason for override           |
        //   | Monitoring Plan   | Must specify allergy monitoring protocol    |
        assert_eq!(get_text(page, "Physician Approval").await?, "Must have override from prescribing MD");
        assert_eq!(get_text(page, "Documentation").await?, "Must document reason for override");
        assert_eq!(get_text(page, "Monitoring Plan").await?, "Must specify allergy monitoring protocol");

        // And the medication administration is held pending physician confirmation
        let administration_status = get_text(page, "Administration Status").await?;
        assert_match(&administration_status, r"pending physician confirmation", true);
        // And an alert is sent to the prescribing physician "Dr. Smith"
        let physician_alert = get_text(page, "Physician Alert").await?;
        assert_match(&physician_alert, r"Dr\. Smith", false);
        Ok(())
    })
}

fn scenario_03_administer_prn_medication_with_clinical_assessment(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given a patient "Jennifer Lopez" is in bed "ED-6"
        // And there is a PRN medication order:
        //   | Medication     | Dose    | Route | Indication           | Max Frequency |
        //   | Morphine       | 2mg     | IV    | Pain score ≥7/10     | Q4H PRN       |
        // And the patient's current pain score is "8/10"
        // And the last morphine dose was given 5 hours ago

        // When I scan the patient's wristband barcode
        page.get_by_test_id("scan-wristband-barcode").first().click(None).await?;
        // And I scan the morphine medication barcode
        page.get_by_test_id("scan-medication-barcode").first().click(None).await?;

        // Then the system verifies PRN medication criteria:
        //   | Criteria          | Assessment                                   | Status   |
        //   | Clinical Indication| Pain score 8/10 meets threshold ≥7/10      | ✓ Met    |
        //   | Frequency Limit   | Last dose 5h ago, within Q4H limit         | ✓ Valid  |
        //   | Patient Safety    | No respiratory contraindications            | ✓ Safe   |
        wait_for_test_id(page, "Clinical Indication Status").await?;
        assert_eq!(get_text(page, "Clinical Indication Status").await?, "✓ Met");
        assert_eq!(get_text(page, "Frequency Limit Status").await?, "✓ Valid");
        assert_eq!(get_text(page, "Patient Safety Status").await?, "✓ Safe");

        // And I document the clinical assessment:
        //   | Assessment Field  | Value                                        |
        //   | Pain Score        | 8/10                                        |
        //   | Pain Location     | Chest                                       |
        //   | Pain Quality      | Sharp, stabbing                             |
        //   | Vital Signs       | BP: 140/85, HR: 95, RR: 18, O2: 96%       |
        fill_fields(page, &vec![
            row([("Field", "Pain Score"), ("Value", "8/10")]),
            row([("Field", "Pain Location"), ("Value", "Chest")]),
            row([("Field", "Pain Quality"), ("Value", "Sharp, stabbing")]),
            row([("Field", "Vital Signs"), ("Value", "BP: 140/85, HR: 95, RR: 18, O2: 96%")]),
        ]).await?;

        // And I confirm the PRN administration
        page.get_by_test_id("confirm-administration").first().click(None).await?;

        // Then the system records the administration with clinical justification:
        //   | Documentation     | Details                                      |
        //   | PRN Indication    | Pain score 8/10, patient requested relief   |
        //   | Clinical Assessment| Stable vitals, no respiratory distress     |
        //   | Next Available    | Not before 18:15 (Q4H)                     |
        wait_for_test_id(page, "PRN Indication").await?;
        assert_eq!(get_text(page, "PRN Indication").await?, "Pain score 8/10, patient requested relief");
        assert_eq!(get_text(page, "Clinical Assessment").await?, "Stable vitals, no respiratory distress");
        assert_eq!(get_text(page, "Next Available").await?, "Not before 18:15 (Q4H)");
        Ok(())
    })
}

fn scenario_04_handle_medication_administration_timing_variances(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given a patient "David Kim" is in bed "ED-3"
        // And there is a scheduled medication:
        //   | Medication    | Scheduled Time | Acceptable Window | Current Time |
        //   | Insulin       | 12:00         | ±30 minutes       | 13:15        |

        // When I scan the patient's wristband barcode
        page.get_by_test_id("scan-wristband-barcode").first().click(None).await?;
        // And I scan the insulin medication barcode
        page.get_by_test_id("scan-medication-barcode").first().click(None).await?;

        // Then the system detects a timing variance:
        //   | Variance Type     | Details                                      |
        //   | Late Administration| 75 minutes past scheduled time             |
        //   | Outside Window    | Beyond acceptable ±30 minute window         |
        //   | Clinical Risk     | Delayed insulin may affect glucose control  |
        wait_for_test_id(page, "Late Administration").await?;
        assert_eq!(get_text(page, "Late Administration").await?, "75 minutes past scheduled time");
        assert_eq!(get_text(page, "Outside Window").await?, "Beyond acceptable ±30 minute window");
        assert_eq!(get_text(page, "Clinical Risk").await?, "Delayed insulin may affect glucose control");

        // And the system requires variance documentation:
        //   | Required Field    | Purpose                                      |
        //   | Delay Reason      | Why medication was not given on time        |
        //   | Patient Status    | Current clinical condition assessment       |
        //   | Physician Notification| Whether MD was contacted about delay    |
        assert_eq!(get_text(page, "Delay Reason Purpose").await?, "Why medication was not given on time");
        assert_eq!(get_text(page, "Patient Status Purpose").await?, "Current clinical condition assessment");
        assert_eq!(get_text(page, "Physician Notification Purpose").await?, "Whether MD was contacted about delay");

        // And I document the variance:
        //   | Field             | Value                                        |
        //   | Delay Reason      | Patient NPO for procedure until 13:00      |
        //   | Patient Assessment| Blood glucose 140 mg/dL, stable            |
        //   | MD Notified       | Dr. Patel aware of delay                    |
        fill_fields(page, &vec![
            row([("Field", "Delay Reason"), ("Value", "Patient NPO for procedure until 13:00")]),
            row([("Field", "Patient Assessment"), ("Value", "Blood glucose 140 mg/dL, stable")]),
            row([("Field", "MD Notified"), ("Value", "Dr. Patel aware of delay")]),
        ]).await?;

        // Then the administration is recorded with variance documentation
        wait_for_test_id(page, "Variance Documentation Status").await?;
        let variance_documentation_status = get_text(page, "Variance Documentation Status").await?;
        assert!(variance_documentation_status.len() > 0);
        // And the next scheduled dose timing is adjusted accordingly
        let next_scheduled_dose = get_text(page, "Next Scheduled Dose").await?;
        assert!(next_scheduled_dose.len() > 0);
        Ok(())
    })
}

fn scenario_05_administer_high_risk_medication_with_double_verification(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given a patient "Susan Williams" is in bed "ED-15"
        // And there is a high-risk medication order:
        //   | Medication    | Dose      | Route | Risk Category        | Special Requirements |
        //   | Heparin       | 5000 units| IV    | High-alert drug      | Double verification  |

        // When I scan the patient's wristband barcode
        page.get_by_test_id("scan-wristband-barcode").first().click(None).await?;
        // And I scan the heparin medication barcode
        page.get_by_test_id("scan-medication-barcode").first().click(None).await?;

        // Then the system flags the high-risk medication:
        //   | Alert Type        | Message                                      |
        //   | HIGH-ALERT DRUG   | 🔴 Heparin requires double verification     |
        //   | Risk Factors      | Bleeding risk, dosing errors common         |
        //   | Requirements      | Second nurse must verify before administration|
        wait_for_test_id(page, "HIGH-ALERT DRUG").await?;
        assert_eq!(get_text(page, "HIGH-ALERT DRUG").await?, "🔴 Heparin requires double verification");
        assert_eq!(get_text(page, "Risk Factors").await?, "Bleeding risk, dosing errors common");
        assert_eq!(get_text(page, "Requirements").await?, "Second nurse must verify before administration");

        // And the system requires independent double verification:
        //   | Verification Step | Nurse 1 (Primary) | Nurse 2 (Verifying) |
        //   | Patient Identity  | Nurse Johnson      | Pending             |
        //   | Medication        | Verified           | Pending             |
        //   | Dose Calculation  | 5000 units         | Pending             |
        //   | Route/Rate        | IV bolus           | Pending             |
        assert_eq!(get_text(page, "Patient Identity Primary Nurse").await?, "Nurse Johnson");
        assert_eq!(get_text(page, "Patient Identity Verifying Nurse").await?, "Pending");
        assert_eq!(get_text(page, "Medication Primary Nurse").await?, "Verified");
        assert_eq!(get_text(page, "Medication Verifying Nurse").await?, "Pending");
        assert_eq!(get_text(page, "Dose Calculation Primary Nurse").await?, "5000 units");
        assert_eq!(get_text(page, "Dose Calculation Verifying Nurse").await?, "Pending");
        assert_eq!(get_text(page, "Route/Rate Primary Nurse").await?, "IV bolus");
        assert_eq!(get_text(page, "Route/Rate Verifying Nurse").await?, "Pending");

        // When "Nurse Martinez" performs the second verification
        fill_field(page, "Verifying Nurse", "Nurse Martinez").await?;
        page.get_by_test_id("submit-second-verification").first().click(None).await?;
        // And both nurses confirm the administration
        page.get_by_test_id("confirm-administration").first().click(None).await?;

        // Then the system records dual verification:
        //   | Field             | Value                                        |
        //   | Primary Nurse     | Nurse Johnson                               |
        //   | Verifying Nurse   | Nurse Martinez                              |
        //   | Verification Time | 2025-06-24 15:30:15                        |
        //   | Both Signatures   | Electronic signatures captured              |
        wait_for_test_id(page, "Primary Nurse").await?;
        assert_eq!(get_text(page, "Primary Nurse").await?, "Nurse Johnson");
        assert_eq!(get_text(page, "Verifying Nurse").await?, "Nurse Martinez");
        assert_eq!(get_text(page, "Verification Time").await?, "2025-06-24 15:30:15");
        assert_eq!(get_text(page, "Both Signatures").await?, "Electronic signatures captured");
        Ok(())
    })
}

fn scenario_06_handle_medication_barcode_scanning_errors(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given a patient "Michael Davis" is in bed "ED-11"
        // And I need to administer prescribed medication

        // When I scan the patient's wristband barcode successfully
        page.get_by_test_id("scan-wristband-barcode").first().click(None).await?;
        // And I attempt to scan a medication barcode that is damaged/unreadable
        page.get_by_test_id("scan-medication-barcode").first().click(None).await?;

        // Then the system displays a barcode error:
        //   | Error Type        | Message                                      |
        //   | Barcode Unreadable| Unable to scan medication barcode           |
        //   | Manual Options    | Enter medication manually or get replacement|
        //   | Safety Warning    | Manual entry bypasses barcode verification  |
        wait_for_test_id(page, "Barcode Unreadable").await?;
        assert_eq!(get_text(page, "Barcode Unreadable").await?, "Unable to scan medication barcode");
        assert_eq!(get_text(page, "Manual Options").await?, "Enter medication manually or get replacement");
        assert_eq!(get_text(page, "Safety Warning").await?, "Manual entry bypasses barcode verification");

        // And I choose to manually enter the medication information:
        //   | Manual Entry Field| Value                                        |
        //   | Medication Name   | Tylenol                                     |
        //   | Strength          | 650mg                                       |
        //   | NDC Number        | 50580-506-02                               |
        //   | Lot Number        | ABC123                                      |
        //   | Expiration Date   | 2026-12-31                                 |
        fill_fields(page, &vec![
            row([("Field", "Medication Name"), ("Value", "Tylenol")]),
            row([("Field", "Strength"), ("Value", "650mg")]),
            row([("Field", "NDC Number"), ("Value", "50580-506-02")]),
            row([("Field", "Lot Number"), ("Value", "ABC123")]),
            row([("Field", "Expiration Date"), ("Value", "2026-12-31")]),
        ]).await?;

        // Then the system validates the manual entry against the order
        let manual_entry_validation = wait_for_test_id(page, "Manual Entry Validation").await?;
        assert!(manual_entry_validation.is_visible().await?);
        // And requires supervisor override for manual medication entry
        wait_for_test_id(page, "Supervisor Override").await?;
        // And documents the barcode scanning issue for pharmacy review
        let pharmacy_review_flag = wait_for_test_id(page, "Pharmacy Review Flag").await?;
        assert!(pharmacy_review_flag.is_visible().await?);
        Ok(())
    })
}

fn scenario_07_administer_pediatric_medication_with_weight_based_verificati(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given a pediatric patient "Emma Foster" (age 5, weight 18kg) is in bed "ED-PEDS-1"
        // And there is a weight-based medication order:
        //   | Medication     | Dose Calculation    | Prescribed Dose | Route |
        //   | Acetaminophen  | 15mg/kg            | 270mg          | PO    |

        // When I scan the patient's wristband barcode
        page.get_by_test_id("scan-wristband-barcode").first().click(None).await?;
        // And I scan the acetaminophen medication barcode
        page.get_by_test_id("scan-medication-barcode").first().click(None).await?;

        // Then the system verifies pediatric dosing:
        //   | Verification       | Calculation                                  | Status   |
        //   | Weight Confirmation| Patient weight: 18kg                        | ✓ Valid  |
        //   | Dose Calculation   | 15mg/kg × 18kg = 270mg                     | ✓ Correct|
        //   | Maximum Safe Dose  | 270mg < 400mg max (safe)                    | ✓ Safe   |
        //   | Age Appropriateness| Acetaminophen approved for age 5            | ✓ Valid  |
        wait_for_test_id(page, "Weight Confirmation Status").await?;
        assert_eq!(get_text(page, "Weight Confirmation Status").await?, "✓ Valid");
        assert_eq!(get_text(page, "Dose Calculation Status").await?, "✓ Correct");
        assert_eq!(get_text(page, "Maximum Safe Dose Status").await?, "✓ Safe");
        assert_eq!(get_text(page, "Age Appropriateness Status").await?, "✓ Valid");

        // And the system displays pediatric-specific information:
        //   | Field              | Value                                        |
        //   | Patient Age/Weight | 5 years old, 18kg                          |
        //   | Calculation Shown  | 15mg/kg × 18kg = 270mg                     |
        //   | Liquid Formulation | 160mg/5mL suspension                       |
        //   | Volume to Give     | 8.4mL                                       |
        assert_eq!(get_text(page, "Patient Age/Weight").await?, "5 years old, 18kg");
        assert_eq!(get_text(page, "Calculation Shown").await?, "15mg/kg × 18kg = 270mg");
        assert_eq!(get_text(page, "Liquid Formulation").await?, "160mg/5mL suspension");
        assert_eq!(get_text(page, "Volume to Give").await?, "8.4mL");

        // And I confirm the pediatric administration
        page.get_by_test_id("confirm-administration").first().click(None).await?;

        // Then the system records with pediatric-specific documentation
        let pediatric_documentation = wait_for_test_id(page, "Pediatric Documentation").await?;
        assert!(pediatric_documentation.is_visible().await?);
        Ok(())
    })
}

fn scenario_08_handle_medication_administration_during_code_blue_emergency(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given a patient "Crisis Patient" is in bed "ED-TRAUMA-1"
        // And a code blue emergency is in progress
        // And emergency medications are ordered:
        //   | Medication    | Dose      | Route | Urgency    |
        //   | Epinephrine   | 1mg       | IV    | STAT       |
        //   | Atropine      | 0.5mg     | IV    | STAT       |

        // When I scan the patient's wristband during the emergency
        page.get_by_test_id("scan-wristband-barcode").first().click(None).await?;
        // And I scan the epinephrine medication barcode
        page.get_by_test_id("scan-medication-barcode").first().click(None).await?;

        // Then the system activates emergency administration mode:
        //   | Emergency Feature | Behavior                                     |
        //   | Rapid Verification| Abbreviated safety checks for life-saving   |
        //   | Time Documentation| Precise timestamp for code blue timeline    |
        //   | Team Notification | Alert code team of medication administration |
        wait_for_test_id(page, "Rapid Verification").await?;
        assert_eq!(get_text(page, "Rapid Verification").await?, "Abbreviated safety checks for life-saving");
        assert_eq!(get_text(page, "Time Documentation").await?, "Precise timestamp for code blue timeline");
        assert_eq!(get_text(page, "Team Notification").await?, "Alert code team of medication administration");

        // And the system allows emergency override of timing restrictions
        wait_for_test_id(page, "Emergency Override").await?;

        // And records the administration with code blue context:
        //   | Field             | Value                                        |
        //   | Emergency Context | Code Blue - Cardiac arrest                  |
        //   | Rapid Administration| Life-saving intervention                   |
        //   | Code Blue Timeline| 15:45:32 - Epinephrine given               |
        assert_eq!(get_text(page, "Emergency Context").await?, "Code Blue - Cardiac arrest");
        assert_eq!(get_text(page, "Rapid Administration").await?, "Life-saving intervention");
        assert_eq!(get_text(page, "Code Blue Timeline").await?, "15:45:32 - Epinephrine given");

        // And the code blue medication log is automatically updated
        let code_blue_medication_log = wait_for_test_id(page, "Code Blue Medication Log").await?;
        assert!(code_blue_medication_log.is_visible().await?);
        Ok(())
    })
}

fn main() {
    run_feature(&[
        ("Successfully administer scheduled medication with barcode verification", scenario_01_successfully_administer_scheduled_medication_with_barcode_ve),
        ("Handle medication administration with allergy alert", scenario_02_handle_medication_administration_with_allergy_alert),
        ("Administer PRN medication with clinical assessment", scenario_03_administer_prn_medication_with_clinical_assessment),
        ("Handle medication administration timing variances", scenario_04_handle_medication_administration_timing_variances),
        ("Administer high-risk medication with double verification", scenario_05_administer_high_risk_medication_with_double_verification),
        ("Handle medication barcode scanning errors", scenario_06_handle_medication_barcode_scanning_errors),
        ("Administer pediatric medication with weight-based verification", scenario_07_administer_pediatric_medication_with_weight_based_verificati),
        ("Handle medication administration during code blue emergency", scenario_08_handle_medication_administration_during_code_blue_emergency),
    ]);
}
