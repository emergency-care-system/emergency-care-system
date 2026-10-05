// Playwright + JUnit 5 test for
// tests-with-given-when-then-features/13-dynamic-queue-updates.feature
// (equivalent to tests-with-playwright-javascript/13-dynamic-queue-updates.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see support/Fields.java) and the shared
// data-testid contract in support/Login.java (login-identity, login-submit,
// app-root).

package emergencycaresystem;

import static emergencycaresystem.support.Config.BASE_URL;
import static emergencycaresystem.support.Fields.*;
import static emergencycaresystem.support.Login.*;
import static emergencycaresystem.support.Matchers.assertMatches;
import static org.junit.jupiter.api.Assertions.*;

import java.util.List;
import java.util.Map;
import com.microsoft.playwright.Page;
import emergencycaresystem.support.Session;
import org.junit.jupiter.api.AfterAll;
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.MethodOrderer;
import org.junit.jupiter.api.Order;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.TestMethodOrder;

@TestMethodOrder(MethodOrderer.OrderAnnotation.class)
class T13DynamicQueueUpdatesTest {
    private static Session session;
    private static Page page;

    @BeforeAll
    static void setUpClass() {
        session = Session.start();
        page = session.page();
    }

    @AfterAll
    static void tearDownClass() {
        session.close();
    }

    @BeforeEach
    void setUp() {
        // Background:
        //   Given the emergency care system is operational
        //   And the dynamic queue management module is active
        //   And the ESI (Emergency Severity Index) prioritization system is enabled
        //   And 15 patients are currently waiting to be seen
        verifySystemIsOperational(page);
        login(page, "a charge nurse");
        // The dynamic queue management module, the ESI prioritization system,
        // and the 15 already-waiting patients are assumed to be pre-seeded
        // test environment state.

        var dynamicQueueUpdatesNavLink = waitForTestId(page, "Nav Dynamic Queue Updates");
        dynamicQueueUpdatesNavLink.click();
        waitForTestId(page, "Dynamic Queue Updates Panel");
    }

    @Test
    @Order(1)
    @DisplayName("High-priority trauma patient bumps existing queue")
    void highPriorityTraumaPatientBumpsExistingQueue() {
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
        fillFields(page, List.of(
            Map.of("Field", "Patient Name", "Value", "Emergency Trauma"),
            Map.of("Field", "ESI Level", "Value", "1")
        ));
        page.getByTestId("submit-patient-arrival").first().click();
        // And the patient is triaged as "Resuscitation - Life threatening"
        fillField(page, "Triage Level", "Resuscitation - Life threatening");
        page.getByTestId("submit-triage").first().click();

        // Then the system immediately reprioritizes the queue:
        //   | New Position | Patient Name      | ESI Level | Wait Time Impact    |
        //   | 1            | Emergency Trauma  | 1         | Immediate           |
        //   | 2            | Alice Johnson     | 2         | +30 min (50 min)    |
        //   | 3            | Bob Williams      | 2         | +30 min (65 min)    |
        //   | 4            | Carol Davis       | 3         | +30 min (75 min)    |
        //   | 5            | David Brown       | 3         | +30 min (90 min)    |
        waitForTestId(page, "Emergency Trauma Queue Position");
        assertEquals("1", getText(page, "Emergency Trauma Queue Position"));
        assertEquals("Immediate", getText(page, "Emergency Trauma Wait Time Impact"));
        assertEquals("2", getText(page, "Alice Johnson Queue Position"));
        assertEquals("+30 min (50 min)", getText(page, "Alice Johnson Wait Time Impact"));
        assertEquals("3", getText(page, "Bob Williams Queue Position"));
        assertEquals("+30 min (65 min)", getText(page, "Bob Williams Wait Time Impact"));
        assertEquals("4", getText(page, "Carol Davis Queue Position"));
        assertEquals("+30 min (75 min)", getText(page, "Carol Davis Wait Time Impact"));
        assertEquals("5", getText(page, "David Brown Queue Position"));
        assertEquals("+30 min (90 min)", getText(page, "David Brown Wait Time Impact"));

        // And the system updates wait time estimates for all affected patients:
        //   | Patient Name    | Previous Estimate | New Estimate | Change      |
        //   | Alice Johnson   | 20 minutes       | 50 minutes   | +30 minutes |
        //   | Bob Williams    | 35 minutes       | 65 minutes   | +30 minutes |
        //   | Carol Davis     | 45 minutes       | 75 minutes   | +30 minutes |
        //   | All others      | Various          | +30 minutes  | Increased   |
        assertEquals("50 minutes", getText(page, "Alice Johnson Wait Time Estimate"));
        assertEquals("65 minutes", getText(page, "Bob Williams Wait Time Estimate"));
        assertEquals("75 minutes", getText(page, "Carol Davis Wait Time Estimate"));
        var allOthersChange = getText(page, "All Others Change");
        assertMatches(allOthersChange, "increased", true);

        // And notifications are sent to affected patients and families
        waitForTestId(page, "Patient Family Notifications Sent");

        // And the trauma team is immediately alerted for the ESI Level 1 patient
        var traumaTeamAlert = getText(page, "Trauma Team Alert");
        assertMatches(traumaTeamAlert, "ESI Level 1", false);
    }

    @Test
    @Order(2)
    @DisplayName("Multiple high-acuity patients arrive simultaneously")
    void multipleHighAcuityPatientsArriveSimultaneously() {
        // Given the current queue has patients with ESI levels 3-5
        // And the next available appointment slot is in 60 minutes

        // When multiple high-acuity patients arrive within 10 minutes:
        //   | Arrival Time | Patient Name     | ESI Level | Chief Complaint          |
        //   | 14:00        | Critical Patient | 1         | Cardiac arrest           |
        //   | 14:05        | Urgent Patient A | 2         | Severe chest pain        |
        //   | 14:08        | Urgent Patient B | 2         | Difficulty breathing     |
        fillFields(page, List.of(
            Map.of("Field", "Patient Name 1", "Value", "Critical Patient"),
            Map.of("Field", "ESI Level 1", "Value", "1"),
            Map.of("Field", "Arrival Time 1", "Value", "14:00"),
            Map.of("Field", "Patient Name 2", "Value", "Urgent Patient A"),
            Map.of("Field", "ESI Level 2", "Value", "2"),
            Map.of("Field", "Arrival Time 2", "Value", "14:05"),
            Map.of("Field", "Patient Name 3", "Value", "Urgent Patient B"),
            Map.of("Field", "ESI Level 3", "Value", "2"),
            Map.of("Field", "Arrival Time 3", "Value", "14:08")
        ));
        page.getByTestId("submit-patient-arrivals").first().click();

        // Then the system prioritizes patients by ESI level and arrival time:
        //   | New Position | Patient Name     | ESI Level | Priority Rationale        |
        //   | 1            | Critical Patient | 1         | Highest acuity - immediate |
        //   | 2            | Urgent Patient A | 2         | ESI 2 - arrived first     |
        //   | 3            | Urgent Patient B | 2         | ESI 2 - arrived second    |
        //   | 4-18         | Existing patients| 3-5       | Lower priority            |
        waitForTestId(page, "Critical Patient Queue Position");
        assertEquals("1", getText(page, "Critical Patient Queue Position"));
        assertEquals("2", getText(page, "Urgent Patient A Queue Position"));
        assertEquals("3", getText(page, "Urgent Patient B Queue Position"));

        // And the system calculates cascading wait time impacts:
        //   | Patient Category | Wait Time Impact                              |
        //   | ESI Level 3      | +90 minutes (3 new higher priority patients) |
        //   | ESI Level 4      | +90 minutes                                   |
        //   | ESI Level 5      | +90 minutes                                   |
        assertEquals("+90 minutes (3 new higher priority patients)", getText(page, "ESI Level 3 Wait Time Impact"));
        assertEquals("+90 minutes", getText(page, "ESI Level 4 Wait Time Impact"));
        assertEquals("+90 minutes", getText(page, "ESI Level 5 Wait Time Impact"));

        // And multiple department alerts are triggered:
        //   | Department    | Alert Type                                    |
        //   | Trauma Team   | ESI 1 - Immediate response required          |
        //   | Cardiology    | Multiple cardiac-related ESI 2 patients     |
        //   | Administration| Surge capacity - consider additional staff   |
        assertEquals("ESI 1 - Immediate response required", getText(page, "Trauma Team Alert Type"));
        assertEquals("Multiple cardiac-related ESI 2 patients", getText(page, "Cardiology Alert Type"));
        assertEquals("Surge capacity - consider additional staff", getText(page, "Administration Alert Type"));
    }

    @Test
    @Order(3)
    @DisplayName("Queue updates with bed availability constraints")
    void queueUpdatesWithBedAvailabilityConstraints() {
        // Given 15 patients are waiting and only 2 beds are currently available
        // And the bed types are:
        //   | Bed Number | Bed Type     | Status    |
        //   | ED-5       | Standard     | Available |
        //   | ED-TRAUMA-1| Trauma       | Available |
        //   | ED-8       | Standard     | Occupied  |
        //   | ED-12      | Isolation    | Occupied  |

        // When a trauma patient with ESI level 1 arrives requiring trauma bay
        fillFields(page, List.of(
            Map.of("Field", "Patient Name", "Value", "Trauma Patient"),
            Map.of("Field", "ESI Level", "Value", "1"),
            Map.of("Field", "Bed Type Required", "Value", "Trauma")
        ));
        page.getByTestId("submit-patient-arrival").first().click();

        // Then the system updates the queue considering bed constraints:
        //   | Queue Position | Patient Name    | ESI Level | Bed Assignment Strategy     |
        //   | 1              | Trauma Patient  | 1         | ED-TRAUMA-1 (immediate)     |
        //   | 2              | Alice Johnson   | 2         | ED-5 when available         |
        //   | 3              | Bob Williams    | 2         | Wait for next bed           |
        waitForTestId(page, "Trauma Patient Queue Position");
        assertEquals("1", getText(page, "Trauma Patient Queue Position"));
        assertEquals("ED-TRAUMA-1 (immediate)", getText(page, "Trauma Patient Bed Assignment Strategy"));
        assertEquals("2", getText(page, "Alice Johnson Queue Position"));
        assertEquals("ED-5 when available", getText(page, "Alice Johnson Bed Assignment Strategy"));
        assertEquals("3", getText(page, "Bob Williams Queue Position"));
        assertEquals("Wait for next bed", getText(page, "Bob Williams Bed Assignment Strategy"));

        // And wait times reflect both queue position and bed availability:
        //   | Patient Name    | Queue Wait | Bed Wait  | Total Estimate |
        //   | Trauma Patient  | 0 minutes  | 0 minutes | Immediate      |
        //   | Alice Johnson   | 0 minutes  | 0 minutes | Immediate      |
        //   | Bob Williams    | 0 minutes  | 45 minutes| 45 minutes     |
        assertEquals("Immediate", getText(page, "Trauma Patient Total Estimate"));
        assertEquals("Immediate", getText(page, "Alice Johnson Total Estimate"));
        assertEquals("45 minutes", getText(page, "Bob Williams Total Estimate"));

        // And the system provides realistic expectations based on resource constraints
        waitForTestId(page, "Resource Constraint Notice");
    }

    @Test
    @Order(4)
    @DisplayName("Handle queue updates during provider capacity changes")
    void handleQueueUpdatesDuringProviderCapacityChanges() {
        // Given the current provider capacity is 3 physicians seeing patients
        // And average patient encounter time is 30 minutes
        // And 15 patients are in queue with estimated wait times

        // When one physician becomes unavailable due to emergency procedure
        page.getByTestId("mark-physician-unavailable").first().click();
        // And a new ESI level 1 patient arrives
        fillField(page, "ESI Level", "1");
        page.getByTestId("submit-patient-arrival").first().click();

        // Then the system recalculates queue times with reduced capacity:
        //   | Capacity Change | Impact                                        |
        //   | 3 → 2 providers | 50% increase in wait times for existing patients |
        //   | ESI 1 arrival  | All patients bumped down one position        |
        waitForTestId(page, "3 → 2 providers");
        assertEquals("50% increase in wait times for existing patients", getText(page, "3 → 2 providers"));
        assertEquals("All patients bumped down one position", getText(page, "ESI 1 arrival"));

        // And updated wait time calculations reflect both changes:
        //   | Patient Category | Original Wait | Capacity Impact | ESI 1 Impact | New Wait   |
        //   | ESI Level 2      | 30 minutes   | +15 minutes     | +30 minutes  | 75 minutes |
        //   | ESI Level 3      | 60 minutes   | +30 minutes     | +30 minutes  | 120 minutes|
        //   | ESI Level 4      | 90 minutes   | +45 minutes     | +30 minutes  | 165 minutes|
        assertEquals("75 minutes", getText(page, "ESI Level 2 New Wait"));
        assertEquals("120 minutes", getText(page, "ESI Level 3 New Wait"));
        assertEquals("165 minutes", getText(page, "ESI Level 4 New Wait"));

        // And the system sends capacity alerts to administration
        waitForTestId(page, "Capacity Alert");

        // And patients/families are notified of updated wait times
        waitForTestId(page, "Patient Family Notifications Sent");
    }

    @Test
    @Order(5)
    @DisplayName("Prioritize patient with rapidly deteriorating condition")
    void prioritizePatientWithRapidlyDeterioratingCondition() {
        // Given a patient "Sarah Mitchell" is currently position 8 in queue with ESI level 3
        // And her initial complaint was "mild abdominal pain"

        // When the patient's condition deteriorates and reassessment shows:
        //   | Assessment Change | New Value                                     |
        //   | Pain Level        | 3/10 → 9/10                                  |
        //   | Vital Signs       | BP: 120/80 → 85/50, HR: 80 → 120            |
        //   | Mental Status     | Alert → Confused                             |
        //   | ESI Level         | 3 → 2                                        |
        fillFields(page, List.of(
            Map.of("Field", "Pain Level", "Value", "9/10"),
            Map.of("Field", "Vital Signs", "Value", "BP: 85/50, HR: 120"),
            Map.of("Field", "Mental Status", "Value", "Confused"),
            Map.of("Field", "ESI Level", "Value", "2")
        ));
        page.getByTestId("submit-reassessment").first().click();

        // Then the system immediately updates her queue position:
        //   | Action Type       | Details                                       |
        //   | Priority Escalation| ESI 3 → ESI 2 due to deterioration          |
        //   | Queue Repositioning| Position 8 → Position 2                     |
        //   | Wait Time Update  | 120 minutes → 15 minutes                     |
        waitForTestId(page, "Priority Escalation");
        assertEquals("ESI 3 → ESI 2 due to deterioration", getText(page, "Priority Escalation"));
        assertEquals("Position 8 → Position 2", getText(page, "Queue Repositioning"));
        assertEquals("120 minutes → 15 minutes", getText(page, "Wait Time Update"));

        // And escalation notifications are sent:
        //   | Recipient         | Notification Content                          |
        //   | Attending Physician| Patient deterioration - Priority escalated   |
        //   | Charge Nurse      | Sarah Mitchell moved to position 2           |
        //   | Triage Nurse      | Reassessment resulted in ESI upgrade         |
        assertEquals("Patient deterioration - Priority escalated", getText(page, "Attending Physician Notification"));
        assertEquals("Sarah Mitchell moved to position 2", getText(page, "Charge Nurse Notification"));
        assertEquals("Reassessment resulted in ESI upgrade", getText(page, "Triage Nurse Notification"));

        // And all subsequent patients are shifted down in the queue
        waitForTestId(page, "Queue Shift Notice");

        // And family members are notified of the priority change
        waitForTestId(page, "Family Priority Change Notification");
    }

    @Test
    @Order(6)
    @DisplayName("Handle specialty service requirements in queue management")
    void handleSpecialtyServiceRequirementsInQueueManagement() {
        // Given 15 patients are waiting with various specialty needs:
        //   | Patient Name    | ESI Level | Specialty Required  | Current Position |
        //   | General Patient | 3         | None               | 3                |
        //   | Cardiac Patient | 2         | Cardiology         | 5                |
        //   | Peds Patient    | 3         | Pediatrics         | 8                |

        // When a new trauma patient arrives requiring neurosurgery consultation
        // And the patient has ESI level 1
        fillFields(page, List.of(
            Map.of("Field", "Patient Name", "Value", "Trauma/Neuro"),
            Map.of("Field", "ESI Level", "Value", "1"),
            Map.of("Field", "Specialty Required", "Value", "Neurosurgery")
        ));
        page.getByTestId("submit-patient-arrival").first().click();

        // Then the system considers both acuity and specialty availability:
        //   | Priority Factor   | Consideration                                 |
        //   | ESI Level 1       | Highest medical priority                     |
        //   | Neurosurgery Need | Specialty consultant availability            |
        //   | Resource Planning | OR availability for potential surgery        |
        waitForTestId(page, "ESI Level 1 Consideration");
        assertEquals("Highest medical priority", getText(page, "ESI Level 1 Consideration"));
        assertEquals("Specialty consultant availability", getText(page, "Neurosurgery Need Consideration"));
        assertEquals("OR availability for potential surgery", getText(page, "Resource Planning Consideration"));

        // And the queue is updated with specialty considerations:
        //   | Position | Patient Name    | Priority Reason                          |
        //   | 1        | Trauma/Neuro    | ESI 1 + Specialty coordination needed   |
        //   | 2        | Cardiac Patient | ESI 2 + Cardiology available           |
        //   | 3        | General Patient | ESI 3 but no specialty delay           |
        assertEquals("1", getText(page, "Trauma/Neuro Queue Position"));
        assertEquals("ESI 1 + Specialty coordination needed", getText(page, "Trauma/Neuro Priority Reason"));
        assertEquals("2", getText(page, "Cardiac Patient Queue Position"));
        assertEquals("ESI 2 + Cardiology available", getText(page, "Cardiac Patient Priority Reason"));
        assertEquals("3", getText(page, "General Patient Queue Position"));
        assertEquals("ESI 3 but no specialty delay", getText(page, "General Patient Priority Reason"));

        // And specialty teams are notified with urgency levels
        waitForTestId(page, "Specialty Team Notification");
    }

    @Test
    @Order(7)
    @DisplayName("Queue updates with time-sensitive treatment windows")
    void queueUpdatesWithTimeSensitiveTreatmentWindows() {
        // Given several patients with time-sensitive conditions are in queue:
        //   | Patient Name    | Condition           | Treatment Window | Queue Position |
        //   | Stroke Patient  | Suspected stroke    | 4.5 hours       | 4              |
        //   | STEMI Patient   | Heart attack        | 90 minutes      | 6              |

        // When a new ESI level 1 trauma patient arrives
        fillField(page, "ESI Level", "1");
        page.getByTestId("submit-patient-arrival").first().click();

        // Then the system balances acuity with time sensitivity:
        //   | Prioritization Logic | Decision Rationale                        |
        //   | ESI 1 Trauma        | Immediate life threat - top priority      |
        //   | STEMI Patient       | Time-critical (90 min) - position 2      |
        //   | Stroke Patient      | Time-critical (4.5 hr) - position 3      |
        waitForTestId(page, "ESI 1 Trauma Decision Rationale");
        assertEquals("Immediate life threat - top priority", getText(page, "ESI 1 Trauma Decision Rationale"));
        assertEquals("Time-critical (90 min) - position 2", getText(page, "STEMI Patient Decision Rationale"));
        assertEquals("Time-critical (4.5 hr) - position 3", getText(page, "Stroke Patient Decision Rationale"));

        // And time-sensitive alerts are maintained:
        //   | Patient Type    | Alert Status                                  |
        //   | Stroke Patient  | 45 minutes remaining in optimal window       |
        //   | STEMI Patient   | 25 minutes remaining for door-to-balloon     |
        assertEquals("45 minutes remaining in optimal window", getText(page, "Stroke Patient Alert Status"));
        assertEquals("25 minutes remaining for door-to-balloon", getText(page, "STEMI Patient Alert Status"));

        // And the system tracks treatment deadlines for all time-sensitive cases
        waitForTestId(page, "Treatment Deadline Tracker");
    }

    @Test
    @Order(8)
    @DisplayName("Real-time queue visualization updates")
    void realTimeQueueVisualizationUpdates() {
        // Given the ED dashboard displays the current patient queue
        // And family members can view estimated wait times on patient portal

        // When queue positions change due to new arrivals
        page.getByTestId("simulate-new-arrival").first().click();

        // Then all displays update in real-time:
        //   | Display Location    | Update Type                                 |
        //   | Main ED Dashboard   | Queue positions and wait times refreshed   |
        //   | Patient Portal      | Family notifications of wait time changes  |
        //   | Mobile Apps         | Provider apps show updated patient lists   |
        //   | Waiting Room Display| General wait time estimates updated        |
        waitForTestId(page, "Main ED Dashboard Update Type");
        assertEquals("Queue positions and wait times refreshed", getText(page, "Main ED Dashboard Update Type"));
        assertEquals("Family notifications of wait time changes", getText(page, "Patient Portal Update Type"));
        assertEquals("Provider apps show updated patient lists", getText(page, "Mobile Apps Update Type"));
        assertEquals("General wait time estimates updated", getText(page, "Waiting Room Display Update Type"));

        // And update timestamps are shown on all displays:
        //   | Display Element     | Information Provided                        |
        //   | Last Updated        | "Queue updated at 14:35:22"                |
        //   | Next Update         | "Automatic refresh in 30 seconds"          |
        //   | Manual Refresh      | Button available for immediate update       |
        assertEquals("Queue updated at 14:35:22", getText(page, "Last Updated"));
        assertEquals("Automatic refresh in 30 seconds", getText(page, "Next Update"));
        assertEquals("Button available for immediate update", getText(page, "Manual Refresh"));

        // And change notifications highlight significant updates for staff attention
        waitForTestId(page, "Staff Change Notification Highlight");
    }
}
