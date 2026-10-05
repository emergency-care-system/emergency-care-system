// Playwright + libtest-mimic test for
// tests-with-given-when-then-features/13-dynamic-queue-updates.feature
// (equivalent to tests-with-playwright-javascript/13-dynamic-queue-updates.test.js).
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
    //   And the dynamic queue management module is active
    //   And the ESI (Emergency Severity Index) prioritization system is enabled
    //   And 15 patients are currently waiting to be seen
    verify_system_is_operational(page).await?;
    login(page, "a charge nurse", false).await?;
    // The dynamic queue management module, the ESI prioritization system,
    // and the 15 already-waiting patients are assumed to be pre-seeded
    // test environment state.

    let dynamic_queue_updates_nav_link = wait_for_test_id(page, "Nav Dynamic Queue Updates").await?;
    dynamic_queue_updates_nav_link.click(None).await?;
    wait_for_test_id(page, "Dynamic Queue Updates Panel").await?;
    Ok(())
}

fn scenario_01_high_priority_trauma_patient_bumps_existing_queue(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given the current patient queue contains:
        //   | Position | Patient Name    | ESI Level | Triage Level   | Current Wait Time |
        //   | 1        | Alice Johnson   | 2         | High Priority  | 20 minutes       |
        //   | 2        | Bob Williams    | 2         | High Priority  | 35 minutes       |
        //   | 3        | Carol Davis     | 3         | Urgent         | 45 minutes       |
        //   | 4        | David Brown     | 3         | Urgent         | 60 minutes       |
        //   | 5        | Emma Wilson     | 3         | Urgent         | 75 minutes       |
        //   | 6-15     | Other patients  | 3-5       | Various        | 90-180 minutes   |
        // And available providers can see 1 patient every 30 minutes

        // When a new trauma patient "Emergency Trauma" arrives with ESI level 1
        fill_fields(page, &vec![
            row([("Field", "Patient Name"), ("Value", "Emergency Trauma")]),
            row([("Field", "ESI Level"), ("Value", "1")]),
        ]).await?;
        page.get_by_test_id("submit-patient-arrival").first().click(None).await?;
        // And the patient is triaged as "Resuscitation - Life threatening"
        fill_field(page, "Triage Level", "Resuscitation - Life threatening").await?;
        page.get_by_test_id("submit-triage").first().click(None).await?;

        // Then the system immediately reprioritizes the queue:
        //   | New Position | Patient Name      | ESI Level | Wait Time Impact    |
        //   | 1            | Emergency Trauma  | 1         | Immediate           |
        //   | 2            | Alice Johnson     | 2         | +30 min (50 min)    |
        //   | 3            | Bob Williams      | 2         | +30 min (65 min)    |
        //   | 4            | Carol Davis       | 3         | +30 min (75 min)    |
        //   | 5            | David Brown       | 3         | +30 min (90 min)    |
        wait_for_test_id(page, "Emergency Trauma Queue Position").await?;
        assert_eq!(get_text(page, "Emergency Trauma Queue Position").await?, "1");
        assert_eq!(get_text(page, "Emergency Trauma Wait Time Impact").await?, "Immediate");
        assert_eq!(get_text(page, "Alice Johnson Queue Position").await?, "2");
        assert_eq!(get_text(page, "Alice Johnson Wait Time Impact").await?, "+30 min (50 min)");
        assert_eq!(get_text(page, "Bob Williams Queue Position").await?, "3");
        assert_eq!(get_text(page, "Bob Williams Wait Time Impact").await?, "+30 min (65 min)");
        assert_eq!(get_text(page, "Carol Davis Queue Position").await?, "4");
        assert_eq!(get_text(page, "Carol Davis Wait Time Impact").await?, "+30 min (75 min)");
        assert_eq!(get_text(page, "David Brown Queue Position").await?, "5");
        assert_eq!(get_text(page, "David Brown Wait Time Impact").await?, "+30 min (90 min)");

        // And the system updates wait time estimates for all affected patients:
        //   | Patient Name    | Previous Estimate | New Estimate | Change      |
        //   | Alice Johnson   | 20 minutes       | 50 minutes   | +30 minutes |
        //   | Bob Williams    | 35 minutes       | 65 minutes   | +30 minutes |
        //   | Carol Davis     | 45 minutes       | 75 minutes   | +30 minutes |
        //   | All others      | Various          | +30 minutes  | Increased   |
        assert_eq!(get_text(page, "Alice Johnson Wait Time Estimate").await?, "50 minutes");
        assert_eq!(get_text(page, "Bob Williams Wait Time Estimate").await?, "65 minutes");
        assert_eq!(get_text(page, "Carol Davis Wait Time Estimate").await?, "75 minutes");
        let all_others_change = get_text(page, "All Others Change").await?;
        assert_match(&all_others_change, r"increased", true);

        // And notifications are sent to affected patients and families
        wait_for_test_id(page, "Patient Family Notifications Sent").await?;

        // And the trauma team is immediately alerted for the ESI Level 1 patient
        let trauma_team_alert = get_text(page, "Trauma Team Alert").await?;
        assert_match(&trauma_team_alert, r"ESI Level 1", false);
        Ok(())
    })
}

fn scenario_02_multiple_high_acuity_patients_arrive_simultaneously(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given the current queue has patients with ESI levels 3-5
        // And the next available appointment slot is in 60 minutes

        // When multiple high-acuity patients arrive within 10 minutes:
        //   | Arrival Time | Patient Name     | ESI Level | Chief Complaint          |
        //   | 14:00        | Critical Patient | 1         | Cardiac arrest           |
        //   | 14:05        | Urgent Patient A | 2         | Severe chest pain        |
        //   | 14:08        | Urgent Patient B | 2         | Difficulty breathing     |
        fill_fields(page, &vec![
            row([("Field", "Patient Name 1"), ("Value", "Critical Patient")]),
            row([("Field", "ESI Level 1"), ("Value", "1")]),
            row([("Field", "Arrival Time 1"), ("Value", "14:00")]),
            row([("Field", "Patient Name 2"), ("Value", "Urgent Patient A")]),
            row([("Field", "ESI Level 2"), ("Value", "2")]),
            row([("Field", "Arrival Time 2"), ("Value", "14:05")]),
            row([("Field", "Patient Name 3"), ("Value", "Urgent Patient B")]),
            row([("Field", "ESI Level 3"), ("Value", "2")]),
            row([("Field", "Arrival Time 3"), ("Value", "14:08")]),
        ]).await?;
        page.get_by_test_id("submit-patient-arrivals").first().click(None).await?;

        // Then the system prioritizes patients by ESI level and arrival time:
        //   | New Position | Patient Name     | ESI Level | Priority Rationale        |
        //   | 1            | Critical Patient | 1         | Highest acuity - immediate |
        //   | 2            | Urgent Patient A | 2         | ESI 2 - arrived first     |
        //   | 3            | Urgent Patient B | 2         | ESI 2 - arrived second    |
        //   | 4-18         | Existing patients| 3-5       | Lower priority            |
        wait_for_test_id(page, "Critical Patient Queue Position").await?;
        assert_eq!(get_text(page, "Critical Patient Queue Position").await?, "1");
        assert_eq!(get_text(page, "Urgent Patient A Queue Position").await?, "2");
        assert_eq!(get_text(page, "Urgent Patient B Queue Position").await?, "3");

        // And the system calculates cascading wait time impacts:
        //   | Patient Category | Wait Time Impact                              |
        //   | ESI Level 3      | +90 minutes (3 new higher priority patients) |
        //   | ESI Level 4      | +90 minutes                                   |
        //   | ESI Level 5      | +90 minutes                                   |
        assert_eq!(get_text(page, "ESI Level 3 Wait Time Impact").await?, "+90 minutes (3 new higher priority patients)");
        assert_eq!(get_text(page, "ESI Level 4 Wait Time Impact").await?, "+90 minutes");
        assert_eq!(get_text(page, "ESI Level 5 Wait Time Impact").await?, "+90 minutes");

        // And multiple department alerts are triggered:
        //   | Department    | Alert Type                                    |
        //   | Trauma Team   | ESI 1 - Immediate response required          |
        //   | Cardiology    | Multiple cardiac-related ESI 2 patients     |
        //   | Administration| Surge capacity - consider additional staff   |
        assert_eq!(get_text(page, "Trauma Team Alert Type").await?, "ESI 1 - Immediate response required");
        assert_eq!(get_text(page, "Cardiology Alert Type").await?, "Multiple cardiac-related ESI 2 patients");
        assert_eq!(get_text(page, "Administration Alert Type").await?, "Surge capacity - consider additional staff");
        Ok(())
    })
}

fn scenario_03_queue_updates_with_bed_availability_constraints(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given 15 patients are waiting and only 2 beds are currently available
        // And the bed types are:
        //   | Bed Number | Bed Type     | Status    |
        //   | ED-5       | Standard     | Available |
        //   | ED-TRAUMA-1| Trauma       | Available |
        //   | ED-8       | Standard     | Occupied  |
        //   | ED-12      | Isolation    | Occupied  |

        // When a trauma patient with ESI level 1 arrives requiring trauma bay
        fill_fields(page, &vec![
            row([("Field", "Patient Name"), ("Value", "Trauma Patient")]),
            row([("Field", "ESI Level"), ("Value", "1")]),
            row([("Field", "Bed Type Required"), ("Value", "Trauma")]),
        ]).await?;
        page.get_by_test_id("submit-patient-arrival").first().click(None).await?;

        // Then the system updates the queue considering bed constraints:
        //   | Queue Position | Patient Name    | ESI Level | Bed Assignment Strategy     |
        //   | 1              | Trauma Patient  | 1         | ED-TRAUMA-1 (immediate)     |
        //   | 2              | Alice Johnson   | 2         | ED-5 when available         |
        //   | 3              | Bob Williams    | 2         | Wait for next bed           |
        wait_for_test_id(page, "Trauma Patient Queue Position").await?;
        assert_eq!(get_text(page, "Trauma Patient Queue Position").await?, "1");
        assert_eq!(get_text(page, "Trauma Patient Bed Assignment Strategy").await?, "ED-TRAUMA-1 (immediate)");
        assert_eq!(get_text(page, "Alice Johnson Queue Position").await?, "2");
        assert_eq!(get_text(page, "Alice Johnson Bed Assignment Strategy").await?, "ED-5 when available");
        assert_eq!(get_text(page, "Bob Williams Queue Position").await?, "3");
        assert_eq!(get_text(page, "Bob Williams Bed Assignment Strategy").await?, "Wait for next bed");

        // And wait times reflect both queue position and bed availability:
        //   | Patient Name    | Queue Wait | Bed Wait  | Total Estimate |
        //   | Trauma Patient  | 0 minutes  | 0 minutes | Immediate      |
        //   | Alice Johnson   | 0 minutes  | 0 minutes | Immediate      |
        //   | Bob Williams    | 0 minutes  | 45 minutes| 45 minutes     |
        assert_eq!(get_text(page, "Trauma Patient Total Estimate").await?, "Immediate");
        assert_eq!(get_text(page, "Alice Johnson Total Estimate").await?, "Immediate");
        assert_eq!(get_text(page, "Bob Williams Total Estimate").await?, "45 minutes");

        // And the system provides realistic expectations based on resource constraints
        wait_for_test_id(page, "Resource Constraint Notice").await?;
        Ok(())
    })
}

fn scenario_04_handle_queue_updates_during_provider_capacity_changes(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given the current provider capacity is 3 physicians seeing patients
        // And average patient encounter time is 30 minutes
        // And 15 patients are in queue with estimated wait times

        // When one physician becomes unavailable due to emergency procedure
        page.get_by_test_id("mark-physician-unavailable").first().click(None).await?;
        // And a new ESI level 1 patient arrives
        fill_field(page, "ESI Level", "1").await?;
        page.get_by_test_id("submit-patient-arrival").first().click(None).await?;

        // Then the system recalculates queue times with reduced capacity:
        //   | Capacity Change | Impact                                        |
        //   | 3 → 2 providers | 50% increase in wait times for existing patients |
        //   | ESI 1 arrival  | All patients bumped down one position        |
        wait_for_test_id(page, "3 → 2 providers").await?;
        assert_eq!(get_text(page, "3 → 2 providers").await?, "50% increase in wait times for existing patients");
        assert_eq!(get_text(page, "ESI 1 arrival").await?, "All patients bumped down one position");

        // And updated wait time calculations reflect both changes:
        //   | Patient Category | Original Wait | Capacity Impact | ESI 1 Impact | New Wait   |
        //   | ESI Level 2      | 30 minutes   | +15 minutes     | +30 minutes  | 75 minutes |
        //   | ESI Level 3      | 60 minutes   | +30 minutes     | +30 minutes  | 120 minutes|
        //   | ESI Level 4      | 90 minutes   | +45 minutes     | +30 minutes  | 165 minutes|
        assert_eq!(get_text(page, "ESI Level 2 New Wait").await?, "75 minutes");
        assert_eq!(get_text(page, "ESI Level 3 New Wait").await?, "120 minutes");
        assert_eq!(get_text(page, "ESI Level 4 New Wait").await?, "165 minutes");

        // And the system sends capacity alerts to administration
        wait_for_test_id(page, "Capacity Alert").await?;

        // And patients/families are notified of updated wait times
        wait_for_test_id(page, "Patient Family Notifications Sent").await?;
        Ok(())
    })
}

fn scenario_05_prioritize_patient_with_rapidly_deteriorating_condition(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given a patient "Sarah Mitchell" is currently position 8 in queue with ESI level 3
        // And her initial complaint was "mild abdominal pain"

        // When the patient's condition deteriorates and reassessment shows:
        //   | Assessment Change | New Value                                     |
        //   | Pain Level        | 3/10 → 9/10                                  |
        //   | Vital Signs       | BP: 120/80 → 85/50, HR: 80 → 120            |
        //   | Mental Status     | Alert → Confused                             |
        //   | ESI Level         | 3 → 2                                        |
        fill_fields(page, &vec![
            row([("Field", "Pain Level"), ("Value", "9/10")]),
            row([("Field", "Vital Signs"), ("Value", "BP: 85/50, HR: 120")]),
            row([("Field", "Mental Status"), ("Value", "Confused")]),
            row([("Field", "ESI Level"), ("Value", "2")]),
        ]).await?;
        page.get_by_test_id("submit-reassessment").first().click(None).await?;

        // Then the system immediately updates her queue position:
        //   | Action Type       | Details                                       |
        //   | Priority Escalation| ESI 3 → ESI 2 due to deterioration          |
        //   | Queue Repositioning| Position 8 → Position 2                     |
        //   | Wait Time Update  | 120 minutes → 15 minutes                     |
        wait_for_test_id(page, "Priority Escalation").await?;
        assert_eq!(get_text(page, "Priority Escalation").await?, "ESI 3 → ESI 2 due to deterioration");
        assert_eq!(get_text(page, "Queue Repositioning").await?, "Position 8 → Position 2");
        assert_eq!(get_text(page, "Wait Time Update").await?, "120 minutes → 15 minutes");

        // And escalation notifications are sent:
        //   | Recipient         | Notification Content                          |
        //   | Attending Physician| Patient deterioration - Priority escalated   |
        //   | Charge Nurse      | Sarah Mitchell moved to position 2           |
        //   | Triage Nurse      | Reassessment resulted in ESI upgrade         |
        assert_eq!(get_text(page, "Attending Physician Notification").await?, "Patient deterioration - Priority escalated");
        assert_eq!(get_text(page, "Charge Nurse Notification").await?, "Sarah Mitchell moved to position 2");
        assert_eq!(get_text(page, "Triage Nurse Notification").await?, "Reassessment resulted in ESI upgrade");

        // And all subsequent patients are shifted down in the queue
        wait_for_test_id(page, "Queue Shift Notice").await?;

        // And family members are notified of the priority change
        wait_for_test_id(page, "Family Priority Change Notification").await?;
        Ok(())
    })
}

fn scenario_06_handle_specialty_service_requirements_in_queue_management(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given 15 patients are waiting with various specialty needs:
        //   | Patient Name    | ESI Level | Specialty Required  | Current Position |
        //   | General Patient | 3         | None               | 3                |
        //   | Cardiac Patient | 2         | Cardiology         | 5                |
        //   | Peds Patient    | 3         | Pediatrics         | 8                |

        // When a new trauma patient arrives requiring neurosurgery consultation
        // And the patient has ESI level 1
        fill_fields(page, &vec![
            row([("Field", "Patient Name"), ("Value", "Trauma/Neuro")]),
            row([("Field", "ESI Level"), ("Value", "1")]),
            row([("Field", "Specialty Required"), ("Value", "Neurosurgery")]),
        ]).await?;
        page.get_by_test_id("submit-patient-arrival").first().click(None).await?;

        // Then the system considers both acuity and specialty availability:
        //   | Priority Factor   | Consideration                                 |
        //   | ESI Level 1       | Highest medical priority                     |
        //   | Neurosurgery Need | Specialty consultant availability            |
        //   | Resource Planning | OR availability for potential surgery        |
        wait_for_test_id(page, "ESI Level 1 Consideration").await?;
        assert_eq!(get_text(page, "ESI Level 1 Consideration").await?, "Highest medical priority");
        assert_eq!(get_text(page, "Neurosurgery Need Consideration").await?, "Specialty consultant availability");
        assert_eq!(get_text(page, "Resource Planning Consideration").await?, "OR availability for potential surgery");

        // And the queue is updated with specialty considerations:
        //   | Position | Patient Name    | Priority Reason                          |
        //   | 1        | Trauma/Neuro    | ESI 1 + Specialty coordination needed   |
        //   | 2        | Cardiac Patient | ESI 2 + Cardiology available           |
        //   | 3        | General Patient | ESI 3 but no specialty delay           |
        assert_eq!(get_text(page, "Trauma/Neuro Queue Position").await?, "1");
        assert_eq!(get_text(page, "Trauma/Neuro Priority Reason").await?, "ESI 1 + Specialty coordination needed");
        assert_eq!(get_text(page, "Cardiac Patient Queue Position").await?, "2");
        assert_eq!(get_text(page, "Cardiac Patient Priority Reason").await?, "ESI 2 + Cardiology available");
        assert_eq!(get_text(page, "General Patient Queue Position").await?, "3");
        assert_eq!(get_text(page, "General Patient Priority Reason").await?, "ESI 3 but no specialty delay");

        // And specialty teams are notified with urgency levels
        wait_for_test_id(page, "Specialty Team Notification").await?;
        Ok(())
    })
}

fn scenario_07_queue_updates_with_time_sensitive_treatment_windows(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given several patients with time-sensitive conditions are in queue:
        //   | Patient Name    | Condition           | Treatment Window | Queue Position |
        //   | Stroke Patient  | Suspected stroke    | 4.5 hours       | 4              |
        //   | STEMI Patient   | Heart attack        | 90 minutes      | 6              |

        // When a new ESI level 1 trauma patient arrives
        fill_field(page, "ESI Level", "1").await?;
        page.get_by_test_id("submit-patient-arrival").first().click(None).await?;

        // Then the system balances acuity with time sensitivity:
        //   | Prioritization Logic | Decision Rationale                        |
        //   | ESI 1 Trauma        | Immediate life threat - top priority      |
        //   | STEMI Patient       | Time-critical (90 min) - position 2      |
        //   | Stroke Patient      | Time-critical (4.5 hr) - position 3      |
        wait_for_test_id(page, "ESI 1 Trauma Decision Rationale").await?;
        assert_eq!(get_text(page, "ESI 1 Trauma Decision Rationale").await?, "Immediate life threat - top priority");
        assert_eq!(get_text(page, "STEMI Patient Decision Rationale").await?, "Time-critical (90 min) - position 2");
        assert_eq!(get_text(page, "Stroke Patient Decision Rationale").await?, "Time-critical (4.5 hr) - position 3");

        // And time-sensitive alerts are maintained:
        //   | Patient Type    | Alert Status                                  |
        //   | Stroke Patient  | 45 minutes remaining in optimal window       |
        //   | STEMI Patient   | 25 minutes remaining for door-to-balloon     |
        assert_eq!(get_text(page, "Stroke Patient Alert Status").await?, "45 minutes remaining in optimal window");
        assert_eq!(get_text(page, "STEMI Patient Alert Status").await?, "25 minutes remaining for door-to-balloon");

        // And the system tracks treatment deadlines for all time-sensitive cases
        wait_for_test_id(page, "Treatment Deadline Tracker").await?;
        Ok(())
    })
}

fn scenario_08_real_time_queue_visualization_updates(page: &Page) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(page).await?;

        // Given the ED dashboard displays the current patient queue
        // And family members can view estimated wait times on patient portal

        // When queue positions change due to new arrivals
        page.get_by_test_id("simulate-new-arrival").first().click(None).await?;

        // Then all displays update in real-time:
        //   | Display Location    | Update Type                                 |
        //   | Main ED Dashboard   | Queue positions and wait times refreshed   |
        //   | Patient Portal      | Family notifications of wait time changes  |
        //   | Mobile Apps         | Provider apps show updated patient lists   |
        //   | Waiting Room Display| General wait time estimates updated        |
        wait_for_test_id(page, "Main ED Dashboard Update Type").await?;
        assert_eq!(get_text(page, "Main ED Dashboard Update Type").await?, "Queue positions and wait times refreshed");
        assert_eq!(get_text(page, "Patient Portal Update Type").await?, "Family notifications of wait time changes");
        assert_eq!(get_text(page, "Mobile Apps Update Type").await?, "Provider apps show updated patient lists");
        assert_eq!(get_text(page, "Waiting Room Display Update Type").await?, "General wait time estimates updated");

        // And update timestamps are shown on all displays:
        //   | Display Element     | Information Provided                        |
        //   | Last Updated        | "Queue updated at 14:35:22"                |
        //   | Next Update         | "Automatic refresh in 30 seconds"          |
        //   | Manual Refresh      | Button available for immediate update       |
        assert_eq!(get_text(page, "Last Updated").await?, "Queue updated at 14:35:22");
        assert_eq!(get_text(page, "Next Update").await?, "Automatic refresh in 30 seconds");
        assert_eq!(get_text(page, "Manual Refresh").await?, "Button available for immediate update");

        // And change notifications highlight significant updates for staff attention
        wait_for_test_id(page, "Staff Change Notification Highlight").await?;
        Ok(())
    })
}

fn main() {
    run_feature(&[
        ("High-priority trauma patient bumps existing queue", scenario_01_high_priority_trauma_patient_bumps_existing_queue),
        ("Multiple high-acuity patients arrive simultaneously", scenario_02_multiple_high_acuity_patients_arrive_simultaneously),
        ("Queue updates with bed availability constraints", scenario_03_queue_updates_with_bed_availability_constraints),
        ("Handle queue updates during provider capacity changes", scenario_04_handle_queue_updates_during_provider_capacity_changes),
        ("Prioritize patient with rapidly deteriorating condition", scenario_05_prioritize_patient_with_rapidly_deteriorating_condition),
        ("Handle specialty service requirements in queue management", scenario_06_handle_specialty_service_requirements_in_queue_management),
        ("Queue updates with time-sensitive treatment windows", scenario_07_queue_updates_with_time_sensitive_treatment_windows),
        ("Real-time queue visualization updates", scenario_08_real_time_queue_visualization_updates),
    ]);
}
