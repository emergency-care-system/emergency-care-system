// Selenium WebDriver + JUnit 5 test for
// tests-with-given-when-then-features/13-dynamic-queue-updates.feature
// (equivalent to tests-with-selenium-javascript/13-dynamic-queue-updates.test.js).
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
import emergencycaresystem.support.DriverFactory;
import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.WebElement;
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
    private static WebDriver driver;

    @BeforeAll
    static void setUpClass() {
        driver = DriverFactory.build();
    }

    @AfterAll
    static void tearDownClass() {
        driver.quit();
    }

    @BeforeEach
    void setUp() {
        // Background:
        //   Given the emergency care system is operational
        //   And the dynamic queue management module is active
        //   And the ESI (Emergency Severity Index) prioritization system is enabled
        //   And 15 patients are currently waiting to be seen
        verifySystemIsOperational(driver);
        login(driver, "a charge nurse");
        // The dynamic queue management module, the ESI prioritization system,
        // and the 15 already-waiting patients are assumed to be pre-seeded
        // test environment state.

        var dynamicQueueUpdatesNavLink = waitForTestId(driver, "Nav Dynamic Queue Updates");
        dynamicQueueUpdatesNavLink.click();
        waitForTestId(driver, "Dynamic Queue Updates Panel");
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
        fillFields(driver, List.of(
            Map.of("Field", "Patient Name", "Value", "Emergency Trauma"),
            Map.of("Field", "ESI Level", "Value", "1")
        ));
        driver.findElement(By.cssSelector("[data-testid=\"submit-patient-arrival\"]")).click();
        // And the patient is triaged as "Resuscitation - Life threatening"
        fillField(driver, "Triage Level", "Resuscitation - Life threatening");
        driver.findElement(By.cssSelector("[data-testid=\"submit-triage\"]")).click();

        // Then the system immediately reprioritizes the queue:
        //   | New Position | Patient Name      | ESI Level | Wait Time Impact    |
        //   | 1            | Emergency Trauma  | 1         | Immediate           |
        //   | 2            | Alice Johnson     | 2         | +30 min (50 min)    |
        //   | 3            | Bob Williams      | 2         | +30 min (65 min)    |
        //   | 4            | Carol Davis       | 3         | +30 min (75 min)    |
        //   | 5            | David Brown       | 3         | +30 min (90 min)    |
        waitForTestId(driver, "Emergency Trauma Queue Position");
        assertEquals("1", getText(driver, "Emergency Trauma Queue Position"));
        assertEquals("Immediate", getText(driver, "Emergency Trauma Wait Time Impact"));
        assertEquals("2", getText(driver, "Alice Johnson Queue Position"));
        assertEquals("+30 min (50 min)", getText(driver, "Alice Johnson Wait Time Impact"));
        assertEquals("3", getText(driver, "Bob Williams Queue Position"));
        assertEquals("+30 min (65 min)", getText(driver, "Bob Williams Wait Time Impact"));
        assertEquals("4", getText(driver, "Carol Davis Queue Position"));
        assertEquals("+30 min (75 min)", getText(driver, "Carol Davis Wait Time Impact"));
        assertEquals("5", getText(driver, "David Brown Queue Position"));
        assertEquals("+30 min (90 min)", getText(driver, "David Brown Wait Time Impact"));

        // And the system updates wait time estimates for all affected patients:
        //   | Patient Name    | Previous Estimate | New Estimate | Change      |
        //   | Alice Johnson   | 20 minutes       | 50 minutes   | +30 minutes |
        //   | Bob Williams    | 35 minutes       | 65 minutes   | +30 minutes |
        //   | Carol Davis     | 45 minutes       | 75 minutes   | +30 minutes |
        //   | All others      | Various          | +30 minutes  | Increased   |
        assertEquals("50 minutes", getText(driver, "Alice Johnson Wait Time Estimate"));
        assertEquals("65 minutes", getText(driver, "Bob Williams Wait Time Estimate"));
        assertEquals("75 minutes", getText(driver, "Carol Davis Wait Time Estimate"));
        var allOthersChange = getText(driver, "All Others Change");
        assertMatches(allOthersChange, "increased", true);

        // And notifications are sent to affected patients and families
        waitForTestId(driver, "Patient Family Notifications Sent");

        // And the trauma team is immediately alerted for the ESI Level 1 patient
        var traumaTeamAlert = getText(driver, "Trauma Team Alert");
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
        fillFields(driver, List.of(
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
        driver.findElement(By.cssSelector("[data-testid=\"submit-patient-arrivals\"]")).click();

        // Then the system prioritizes patients by ESI level and arrival time:
        //   | New Position | Patient Name     | ESI Level | Priority Rationale        |
        //   | 1            | Critical Patient | 1         | Highest acuity - immediate |
        //   | 2            | Urgent Patient A | 2         | ESI 2 - arrived first     |
        //   | 3            | Urgent Patient B | 2         | ESI 2 - arrived second    |
        //   | 4-18         | Existing patients| 3-5       | Lower priority            |
        waitForTestId(driver, "Critical Patient Queue Position");
        assertEquals("1", getText(driver, "Critical Patient Queue Position"));
        assertEquals("2", getText(driver, "Urgent Patient A Queue Position"));
        assertEquals("3", getText(driver, "Urgent Patient B Queue Position"));

        // And the system calculates cascading wait time impacts:
        //   | Patient Category | Wait Time Impact                              |
        //   | ESI Level 3      | +90 minutes (3 new higher priority patients) |
        //   | ESI Level 4      | +90 minutes                                   |
        //   | ESI Level 5      | +90 minutes                                   |
        assertEquals("+90 minutes (3 new higher priority patients)", getText(driver, "ESI Level 3 Wait Time Impact"));
        assertEquals("+90 minutes", getText(driver, "ESI Level 4 Wait Time Impact"));
        assertEquals("+90 minutes", getText(driver, "ESI Level 5 Wait Time Impact"));

        // And multiple department alerts are triggered:
        //   | Department    | Alert Type                                    |
        //   | Trauma Team   | ESI 1 - Immediate response required          |
        //   | Cardiology    | Multiple cardiac-related ESI 2 patients     |
        //   | Administration| Surge capacity - consider additional staff   |
        assertEquals("ESI 1 - Immediate response required", getText(driver, "Trauma Team Alert Type"));
        assertEquals("Multiple cardiac-related ESI 2 patients", getText(driver, "Cardiology Alert Type"));
        assertEquals("Surge capacity - consider additional staff", getText(driver, "Administration Alert Type"));
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
        fillFields(driver, List.of(
            Map.of("Field", "Patient Name", "Value", "Trauma Patient"),
            Map.of("Field", "ESI Level", "Value", "1"),
            Map.of("Field", "Bed Type Required", "Value", "Trauma")
        ));
        driver.findElement(By.cssSelector("[data-testid=\"submit-patient-arrival\"]")).click();

        // Then the system updates the queue considering bed constraints:
        //   | Queue Position | Patient Name    | ESI Level | Bed Assignment Strategy     |
        //   | 1              | Trauma Patient  | 1         | ED-TRAUMA-1 (immediate)     |
        //   | 2              | Alice Johnson   | 2         | ED-5 when available         |
        //   | 3              | Bob Williams    | 2         | Wait for next bed           |
        waitForTestId(driver, "Trauma Patient Queue Position");
        assertEquals("1", getText(driver, "Trauma Patient Queue Position"));
        assertEquals("ED-TRAUMA-1 (immediate)", getText(driver, "Trauma Patient Bed Assignment Strategy"));
        assertEquals("2", getText(driver, "Alice Johnson Queue Position"));
        assertEquals("ED-5 when available", getText(driver, "Alice Johnson Bed Assignment Strategy"));
        assertEquals("3", getText(driver, "Bob Williams Queue Position"));
        assertEquals("Wait for next bed", getText(driver, "Bob Williams Bed Assignment Strategy"));

        // And wait times reflect both queue position and bed availability:
        //   | Patient Name    | Queue Wait | Bed Wait  | Total Estimate |
        //   | Trauma Patient  | 0 minutes  | 0 minutes | Immediate      |
        //   | Alice Johnson   | 0 minutes  | 0 minutes | Immediate      |
        //   | Bob Williams    | 0 minutes  | 45 minutes| 45 minutes     |
        assertEquals("Immediate", getText(driver, "Trauma Patient Total Estimate"));
        assertEquals("Immediate", getText(driver, "Alice Johnson Total Estimate"));
        assertEquals("45 minutes", getText(driver, "Bob Williams Total Estimate"));

        // And the system provides realistic expectations based on resource constraints
        waitForTestId(driver, "Resource Constraint Notice");
    }

    @Test
    @Order(4)
    @DisplayName("Handle queue updates during provider capacity changes")
    void handleQueueUpdatesDuringProviderCapacityChanges() {
        // Given the current provider capacity is 3 physicians seeing patients
        // And average patient encounter time is 30 minutes
        // And 15 patients are in queue with estimated wait times

        // When one physician becomes unavailable due to emergency procedure
        driver.findElement(By.cssSelector("[data-testid=\"mark-physician-unavailable\"]")).click();
        // And a new ESI level 1 patient arrives
        fillField(driver, "ESI Level", "1");
        driver.findElement(By.cssSelector("[data-testid=\"submit-patient-arrival\"]")).click();

        // Then the system recalculates queue times with reduced capacity:
        //   | Capacity Change | Impact                                        |
        //   | 3 → 2 providers | 50% increase in wait times for existing patients |
        //   | ESI 1 arrival  | All patients bumped down one position        |
        waitForTestId(driver, "3 → 2 providers");
        assertEquals("50% increase in wait times for existing patients", getText(driver, "3 → 2 providers"));
        assertEquals("All patients bumped down one position", getText(driver, "ESI 1 arrival"));

        // And updated wait time calculations reflect both changes:
        //   | Patient Category | Original Wait | Capacity Impact | ESI 1 Impact | New Wait   |
        //   | ESI Level 2      | 30 minutes   | +15 minutes     | +30 minutes  | 75 minutes |
        //   | ESI Level 3      | 60 minutes   | +30 minutes     | +30 minutes  | 120 minutes|
        //   | ESI Level 4      | 90 minutes   | +45 minutes     | +30 minutes  | 165 minutes|
        assertEquals("75 minutes", getText(driver, "ESI Level 2 New Wait"));
        assertEquals("120 minutes", getText(driver, "ESI Level 3 New Wait"));
        assertEquals("165 minutes", getText(driver, "ESI Level 4 New Wait"));

        // And the system sends capacity alerts to administration
        waitForTestId(driver, "Capacity Alert");

        // And patients/families are notified of updated wait times
        waitForTestId(driver, "Patient Family Notifications Sent");
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
        fillFields(driver, List.of(
            Map.of("Field", "Pain Level", "Value", "9/10"),
            Map.of("Field", "Vital Signs", "Value", "BP: 85/50, HR: 120"),
            Map.of("Field", "Mental Status", "Value", "Confused"),
            Map.of("Field", "ESI Level", "Value", "2")
        ));
        driver.findElement(By.cssSelector("[data-testid=\"submit-reassessment\"]")).click();

        // Then the system immediately updates her queue position:
        //   | Action Type       | Details                                       |
        //   | Priority Escalation| ESI 3 → ESI 2 due to deterioration          |
        //   | Queue Repositioning| Position 8 → Position 2                     |
        //   | Wait Time Update  | 120 minutes → 15 minutes                     |
        waitForTestId(driver, "Priority Escalation");
        assertEquals("ESI 3 → ESI 2 due to deterioration", getText(driver, "Priority Escalation"));
        assertEquals("Position 8 → Position 2", getText(driver, "Queue Repositioning"));
        assertEquals("120 minutes → 15 minutes", getText(driver, "Wait Time Update"));

        // And escalation notifications are sent:
        //   | Recipient         | Notification Content                          |
        //   | Attending Physician| Patient deterioration - Priority escalated   |
        //   | Charge Nurse      | Sarah Mitchell moved to position 2           |
        //   | Triage Nurse      | Reassessment resulted in ESI upgrade         |
        assertEquals("Patient deterioration - Priority escalated", getText(driver, "Attending Physician Notification"));
        assertEquals("Sarah Mitchell moved to position 2", getText(driver, "Charge Nurse Notification"));
        assertEquals("Reassessment resulted in ESI upgrade", getText(driver, "Triage Nurse Notification"));

        // And all subsequent patients are shifted down in the queue
        waitForTestId(driver, "Queue Shift Notice");

        // And family members are notified of the priority change
        waitForTestId(driver, "Family Priority Change Notification");
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
        fillFields(driver, List.of(
            Map.of("Field", "Patient Name", "Value", "Trauma/Neuro"),
            Map.of("Field", "ESI Level", "Value", "1"),
            Map.of("Field", "Specialty Required", "Value", "Neurosurgery")
        ));
        driver.findElement(By.cssSelector("[data-testid=\"submit-patient-arrival\"]")).click();

        // Then the system considers both acuity and specialty availability:
        //   | Priority Factor   | Consideration                                 |
        //   | ESI Level 1       | Highest medical priority                     |
        //   | Neurosurgery Need | Specialty consultant availability            |
        //   | Resource Planning | OR availability for potential surgery        |
        waitForTestId(driver, "ESI Level 1 Consideration");
        assertEquals("Highest medical priority", getText(driver, "ESI Level 1 Consideration"));
        assertEquals("Specialty consultant availability", getText(driver, "Neurosurgery Need Consideration"));
        assertEquals("OR availability for potential surgery", getText(driver, "Resource Planning Consideration"));

        // And the queue is updated with specialty considerations:
        //   | Position | Patient Name    | Priority Reason                          |
        //   | 1        | Trauma/Neuro    | ESI 1 + Specialty coordination needed   |
        //   | 2        | Cardiac Patient | ESI 2 + Cardiology available           |
        //   | 3        | General Patient | ESI 3 but no specialty delay           |
        assertEquals("1", getText(driver, "Trauma/Neuro Queue Position"));
        assertEquals("ESI 1 + Specialty coordination needed", getText(driver, "Trauma/Neuro Priority Reason"));
        assertEquals("2", getText(driver, "Cardiac Patient Queue Position"));
        assertEquals("ESI 2 + Cardiology available", getText(driver, "Cardiac Patient Priority Reason"));
        assertEquals("3", getText(driver, "General Patient Queue Position"));
        assertEquals("ESI 3 but no specialty delay", getText(driver, "General Patient Priority Reason"));

        // And specialty teams are notified with urgency levels
        waitForTestId(driver, "Specialty Team Notification");
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
        fillField(driver, "ESI Level", "1");
        driver.findElement(By.cssSelector("[data-testid=\"submit-patient-arrival\"]")).click();

        // Then the system balances acuity with time sensitivity:
        //   | Prioritization Logic | Decision Rationale                        |
        //   | ESI 1 Trauma        | Immediate life threat - top priority      |
        //   | STEMI Patient       | Time-critical (90 min) - position 2      |
        //   | Stroke Patient      | Time-critical (4.5 hr) - position 3      |
        waitForTestId(driver, "ESI 1 Trauma Decision Rationale");
        assertEquals("Immediate life threat - top priority", getText(driver, "ESI 1 Trauma Decision Rationale"));
        assertEquals("Time-critical (90 min) - position 2", getText(driver, "STEMI Patient Decision Rationale"));
        assertEquals("Time-critical (4.5 hr) - position 3", getText(driver, "Stroke Patient Decision Rationale"));

        // And time-sensitive alerts are maintained:
        //   | Patient Type    | Alert Status                                  |
        //   | Stroke Patient  | 45 minutes remaining in optimal window       |
        //   | STEMI Patient   | 25 minutes remaining for door-to-balloon     |
        assertEquals("45 minutes remaining in optimal window", getText(driver, "Stroke Patient Alert Status"));
        assertEquals("25 minutes remaining for door-to-balloon", getText(driver, "STEMI Patient Alert Status"));

        // And the system tracks treatment deadlines for all time-sensitive cases
        waitForTestId(driver, "Treatment Deadline Tracker");
    }

    @Test
    @Order(8)
    @DisplayName("Real-time queue visualization updates")
    void realTimeQueueVisualizationUpdates() {
        // Given the ED dashboard displays the current patient queue
        // And family members can view estimated wait times on patient portal

        // When queue positions change due to new arrivals
        driver.findElement(By.cssSelector("[data-testid=\"simulate-new-arrival\"]")).click();

        // Then all displays update in real-time:
        //   | Display Location    | Update Type                                 |
        //   | Main ED Dashboard   | Queue positions and wait times refreshed   |
        //   | Patient Portal      | Family notifications of wait time changes  |
        //   | Mobile Apps         | Provider apps show updated patient lists   |
        //   | Waiting Room Display| General wait time estimates updated        |
        waitForTestId(driver, "Main ED Dashboard Update Type");
        assertEquals("Queue positions and wait times refreshed", getText(driver, "Main ED Dashboard Update Type"));
        assertEquals("Family notifications of wait time changes", getText(driver, "Patient Portal Update Type"));
        assertEquals("Provider apps show updated patient lists", getText(driver, "Mobile Apps Update Type"));
        assertEquals("General wait time estimates updated", getText(driver, "Waiting Room Display Update Type"));

        // And update timestamps are shown on all displays:
        //   | Display Element     | Information Provided                        |
        //   | Last Updated        | "Queue updated at 14:35:22"                |
        //   | Next Update         | "Automatic refresh in 30 seconds"          |
        //   | Manual Refresh      | Button available for immediate update       |
        assertEquals("Queue updated at 14:35:22", getText(driver, "Last Updated"));
        assertEquals("Automatic refresh in 30 seconds", getText(driver, "Next Update"));
        assertEquals("Button available for immediate update", getText(driver, "Manual Refresh"));

        // And change notifications highlight significant updates for staff attention
        waitForTestId(driver, "Staff Change Notification Highlight");
    }
}
