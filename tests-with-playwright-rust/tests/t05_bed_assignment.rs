// Playwright + libtest-mimic test for
// tests-with-given-when-then-features/05-bed-assignment.feature
// (equivalent to tests-with-playwright-javascript/05-bed-assignment.test.js).
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
    //   And I am logged in as a charge nurse
    //   And the bed management module is active
    //   And the patient prioritization algorithm is enabled
    verify_system_is_operational(page).await?;
    login(page, "a charge nurse", false).await?;
    // The bed management module and patient prioritization algorithm are
    // assumed to be pre-seeded/enabled test data.

    let feature_nav_link = wait_for_test_id(page, "Nav Bed Assignment").await?;
    feature_nav_link.click(None).await?;
    wait_for_test_id(page, "Bed Assignment Panel").await?;
    Ok(())
}

fn scenario_01_assign_standard_room_to_highest_acuity_patient(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given multiple patients are waiting for beds:
        // And a standard room "ED-12" becomes available
        // (assumed pre-seeded test data)

        // When I request bed assignment recommendations
        page.get_by_test_id("request-recommendations-button").first().click(None).await?;

        // Then the system suggests "Maria Gonzalez" as the top recommendation
        wait_for_test_id(page, "Top Recommendation").await?;
        let top_recommendation = get_text(page, "Top Recommendation").await?;
        assert_eq!(top_recommendation, "Maria Gonzalez");

        // And the recommendation shows:
        let recommendation_fields = vec![
            row([("Field", "Recommended Patient"), ("Value", "Maria Gonzalez")]),
            row([("Field", "ESI Level"), ("Value", "2")]),
            row([("Field", "Wait Time"), ("Value", "45 minutes")]),
            row([("Field", "Room Match"), ("Value", "Standard room suitable")]),
            row([("Field", "Rationale"), ("Value", "Highest acuity patient requiring standard bed")]),
        ];
        for row_data in &recommendation_fields {
            let field = row_data["Field"].as_str();
            let value = row_data["Value"].as_str();
            assert_eq!(get_text(page, field).await?, value);
        }

        // And the system displays updated wait times for remaining patients:
        let wait_time_updates = page.get_by_test_id("wait-time-update-row");
        assert_eq!(wait_time_updates.count().await?, 3);
        Ok(())
    })
}

fn scenario_02_assign_isolation_room_based_on_patient_requirements(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given multiple patients are waiting for beds:
        // And an isolation room "ED-ISO-2" becomes available
        // (assumed pre-seeded test data)

        // When I request bed assignment recommendations
        page.get_by_test_id("request-recommendations-button").first().click(None).await?;

        // Then the system suggests "Alex Johnson" as the top recommendation
        wait_for_test_id(page, "Top Recommendation").await?;
        let top_recommendation = get_text(page, "Top Recommendation").await?;
        assert_eq!(top_recommendation, "Alex Johnson");

        // And the recommendation shows:
        let recommendation_fields = vec![
            row([("Field", "Recommended Patient"), ("Value", "Alex Johnson")]),
            row([("Field", "Room Type"), ("Value", "Isolation room")]),
            row([("Field", "Rationale"), ("Value", "Patient requires isolation precautions")]),
            row([("Field", "Infection Control"), ("Value", "Airborne precautions needed")]),
        ];
        for row_data in &recommendation_fields {
            let field = row_data["Field"].as_str();
            let value = row_data["Value"].as_str();
            assert_eq!(get_text(page, field).await?, value);
        }

        // And the system displays that other patients cannot use this room type
        let room_type_restriction = get_text(page, "Room Type Restriction Notice").await?;
        assert!(room_type_restriction.len() > 0);

        // And the estimated wait times for standard rooms remain unchanged for other patients
        let standard_room_wait_time_change = get_text(page, "Standard Room Wait Time Change").await?;
        assert_eq!(standard_room_wait_time_change, "Unchanged");
        Ok(())
    })
}

fn scenario_03_handle_pediatric_room_assignment(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given multiple patients are waiting for beds:
        // And a pediatric room "ED-PEDS-3" becomes available
        // (assumed pre-seeded test data)

        // When I request bed assignment recommendations
        page.get_by_test_id("request-recommendations-button").first().click(None).await?;

        // Then the system suggests "Sarah Mitchell" as the top recommendation
        wait_for_test_id(page, "Top Recommendation").await?;
        let top_recommendation = get_text(page, "Top Recommendation").await?;
        assert_eq!(top_recommendation, "Sarah Mitchell");

        // And the recommendation shows:
        let recommendation_fields = vec![
            row([("Field", "Recommended Patient"), ("Value", "Sarah Mitchell")]),
            row([("Field", "Age"), ("Value", "4 years old")]),
            row([("Field", "ESI Level"), ("Value", "2")]),
            row([("Field", "Room Type"), ("Value", "Pediatric room")]),
            row([("Field", "Rationale"), ("Value", "Highest acuity pediatric patient")]),
        ];
        for row_data in &recommendation_fields {
            let field = row_data["Field"].as_str();
            let value = row_data["Value"].as_str();
            assert_eq!(get_text(page, field).await?, value);
        }

        // And the system displays updated wait times for remaining pediatric patients:
        let pediatric_wait_time_updates = page.get_by_test_id("wait-time-update-row");
        assert_eq!(pediatric_wait_time_updates.count().await?, 2);

        // And David Chen remains in the adult standard room queue
        let david_chen_queue_status = get_text(page, "David Chen Queue Status").await?;
        assert_match(&david_chen_queue_status, r"adult standard room queue", true);
        Ok(())
    })
}

fn scenario_04_no_suitable_patients_for_available_room_type(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given multiple patients are waiting for beds:
        // And a trauma room "ED-TRAUMA-1" becomes available
        // (assumed pre-seeded test data)

        // When I request bed assignment recommendations
        page.get_by_test_id("request-recommendations-button").first().click(None).await?;

        // Then the system displays "No suitable patients for trauma room"
        let no_suitable_patients_message = get_text(page, "No Suitable Patients Message").await?;
        assert_eq!(no_suitable_patients_message, "No suitable patients for trauma room");

        // And the system suggests:
        let suggestions = vec![
            row([("Recommendation Type", "Alternative Use"), ("Details", "Consider using for high acuity standard patients")]),
            row([("Recommendation Type", "Room Conversion"), ("Details", "Can be downgraded to standard room if needed")]),
            row([("Recommendation Type", "Hold for Emergency"), ("Details", "Keep available for incoming trauma cases")]),
        ];
        for suggestion in &suggestions {
            assert_eq!(get_text(page, suggestion["Recommendation Type"].as_str()).await?, suggestion["Details"].as_str());
        }

        // And the system maintains the trauma room as available
        let trauma_room_status = get_text(page, "Trauma Room Status").await?;
        assert_eq!(trauma_room_status, "Available");

        // And no patient assignments are automatically made
        let auto_assignments = page.get_by_test_id("patient-assignment");
        assert_eq!(auto_assignments.count().await?, 0);
        Ok(())
    })
}

fn scenario_05_handle_multiple_rooms_becoming_available_simultaneously(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given multiple patients are waiting for beds:
        // And multiple rooms become available:
        // (assumed pre-seeded test data)

        // When I request bed assignment recommendations
        page.get_by_test_id("request-recommendations-button").first().click(None).await?;

        // Then the system provides multiple recommendations:
        let recommendations = page.get_by_test_id("bed-recommendation-row");
        assert_eq!(recommendations.count().await?, 3);

        // And the system updates wait times for all remaining patients
        wait_for_test_id(page, "Wait Times Updated Notice").await?;

        // And the recommendations are ranked by patient acuity priority
        let ranking_order = get_text(page, "Recommendation Ranking Order").await?;
        assert_eq!(ranking_order, "Ranked by patient acuity priority");
        Ok(())
    })
}

fn scenario_06_consider_patient_gender_for_room_assignment(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given multiple patients are waiting for beds:
        // And a standard room "ED-8" becomes available
        // And the room currently has a male patient in the adjacent bed
        // (assumed pre-seeded test data)

        // When I request bed assignment recommendations considering privacy preferences
        page.get_by_test_id("request-recommendations-button").first().click(None).await?;

        // Then the system suggests "Anthony Clark" as the top recommendation
        wait_for_test_id(page, "Top Recommendation").await?;
        let top_recommendation = get_text(page, "Top Recommendation").await?;
        assert_eq!(top_recommendation, "Anthony Clark");

        // And the recommendation includes:
        let recommendation_fields = vec![
            row([("Field", "Privacy Consideration"), ("Value", "Same gender as adjacent patient")]),
            row([("Field", "Alternative Option"), ("Value", "Michelle Lee (if privacy not a concern)")]),
        ];
        for row_data in &recommendation_fields {
            let field = row_data["Field"].as_str();
            let value = row_data["Value"].as_str();
            assert_eq!(get_text(page, field).await?, value);
        }

        // And I can override the gender consideration if clinically necessary
        wait_for_test_id(page, "Override Gender Consideration").await?;
        Ok(())
    })
}

fn scenario_07_handle_bed_assignment_during_high_volume_period(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given the ED is operating at 95% capacity
        // And multiple high-acuity patients are waiting:
        // And a standard room "ED-6" becomes available
        // (assumed pre-seeded test data)

        // When I request bed assignment recommendations during crisis mode
        page.get_by_test_id("request-recommendations-button").first().click(None).await?;

        // Then the system prioritizes "Crisis Patient A" despite room type mismatch
        wait_for_test_id(page, "Top Recommendation").await?;
        let top_recommendation = get_text(page, "Top Recommendation").await?;
        assert_eq!(top_recommendation, "Crisis Patient A");

        // And the system displays:
        let alerts = vec![
            row([("Alert Type", "High Volume Alert"), ("Message", "ED at capacity - emergency protocols active")]),
            row([("Alert Type", "Room Flex Option"), ("Message", "Standard room can accommodate ESI Level 1")]),
            row([("Alert Type", "Resource Alert"), ("Message", "Additional equipment may be needed")]),
        ];
        for alert in &alerts {
            assert_eq!(get_text(page, alert["Alert Type"].as_str()).await?, alert["Message"].as_str());
        }

        // And the system suggests moving lower acuity patients to make trauma rooms available
        let trauma_room_suggestion = get_text(page, "Trauma Room Availability Suggestion").await?;
        assert_match(&trauma_room_suggestion, r"moving lower acuity patients", true);
        Ok(())
    })
}

fn scenario_08_update_wait_times_after_bed_assignment(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given the following patients are waiting:
        // And I assign Patient A to the available room
        // (assumed pre-seeded test data)

        // When the bed assignment is confirmed
        page.get_by_test_id("confirm-bed-assignment-button").first().click(None).await?;

        // Then the system recalculates wait times for remaining patients:
        let wait_time_updates = page.get_by_test_id("wait-time-update-row");
        assert_eq!(wait_time_updates.count().await?, 3);

        // And the updated wait times are displayed on the patient tracking board
        wait_for_test_id(page, "Patient Tracking Board").await?;

        // And family members are notified of updated estimates via the patient portal
        let family_notification_status = get_text(page, "Family Notification Status").await?;
        assert_match(&family_notification_status, r"notified", true);
        Ok(())
    })
}

fn scenario_09_handle_bed_assignment_rejection_and_alternative_selection(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given the system recommends "John Smith" for room "ED-10"
        // And John Smith has ESI Level 2 with chest pain
        // (assumed pre-seeded test data)

        // When I review the recommendation
        wait_for_test_id(page, "Top Recommendation").await?;

        // And I determine that John Smith needs cardiac monitoring not available in ED-10
        // (clinical judgement, no direct UI action)

        // And I reject the system recommendation
        page.get_by_test_id("reject-recommendation-button").first().click(None).await?;

        // Then the system provides alternative recommendations:
        let alternative_recommendations = page.get_by_test_id("alternative-recommendation-row");
        assert_eq!(alternative_recommendations.count().await?, 2);

        // And the system suggests alternative rooms for John Smith:
        let alternative_rooms = page.get_by_test_id("alternative-room-row");
        assert_eq!(alternative_rooms.count().await?, 2);

        // And I can select an alternative patient or wait for appropriate room for John Smith
        wait_for_test_id(page, "Select Alternative Patient").await?;
        Ok(())
    })
}

fn main() {
    run_feature(&[
        ("Assign standard room to highest acuity patient", scenario_01_assign_standard_room_to_highest_acuity_patient),
        ("Assign isolation room based on patient requirements", scenario_02_assign_isolation_room_based_on_patient_requirements),
        ("Handle pediatric room assignment", scenario_03_handle_pediatric_room_assignment),
        ("No suitable patients for available room type", scenario_04_no_suitable_patients_for_available_room_type),
        ("Handle multiple rooms becoming available simultaneously", scenario_05_handle_multiple_rooms_becoming_available_simultaneously),
        ("Consider patient gender for room assignment", scenario_06_consider_patient_gender_for_room_assignment),
        ("Handle bed assignment during high volume period", scenario_07_handle_bed_assignment_during_high_volume_period),
        ("Update wait times after bed assignment", scenario_08_update_wait_times_after_bed_assignment),
        ("Handle bed assignment rejection and alternative selection", scenario_09_handle_bed_assignment_rejection_and_alternative_selection),
    ]);
}
