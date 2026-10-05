// Selenium WebDriver + JUnit 5 test for
// tests-with-given-when-then-features/04-triage-re-assessment.feature
// (equivalent to tests-with-selenium-javascript/04-triage-re-assessment.test.js).
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
class T04TriageReAssessmentTest {
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
        //   And I am logged in as a triage nurse
        //   And the automatic reassessment alerts are enabled
        verifySystemIsOperational(driver);
        login(driver, "a triage nurse");
        // The automatic reassessment alerts being enabled is assumed pre-seeded
        // test data / environment configuration.

        var featureNavLink = waitForTestId(driver, "Nav Triage Re-assessment");
        featureNavLink.click();
        waitForTestId(driver, "Triage Re-assessment Panel");
    }

    @Test
    @Order(1)
    @DisplayName("Reassess patient with worsening condition after 2 hours")
    void reassessPatientWithWorseningConditionAfter2Hours() {
        // Given a patient "Sarah Johnson" has been waiting in the queue for 2 hours
        // And the patient's initial triage was ESI Level 4 (Less Urgent)
        // And the patient's initial vital signs were:
        //   | Vital Sign          | Initial Value |
        //   | Blood Pressure      | 125/78        |
        //   | Heart Rate          | 82            |
        //   | Respiratory Rate    | 16            |
        //   | Temperature         | 99.1°F        |
        //   | Oxygen Saturation   | 98%           |
        //   | Pain Scale          | 3/10          |
        // (assumed pre-seeded test data)
        // When the system triggers a reassessment alert at the 2-hour mark
        waitForTestId(driver, "Reassessment Alert");
        // And I select the patient for reassessment
        driver.findElement(By.cssSelector("[data-testid=\"select-patient-for-reassessment-button\"]")).click();

        // And I enter the updated vital signs:
        fillFields(driver, List.of(
            Map.of("Field", "Blood Pressure", "Value", "95/55"),
            Map.of("Field", "Heart Rate", "Value", "115"),
            Map.of("Field", "Respiratory Rate", "Value", "24"),
            Map.of("Field", "Temperature", "Value", "101.8°F"),
            Map.of("Field", "Oxygen Saturation", "Value", "94%"),
            Map.of("Field", "Pain Scale", "Value", "8/10")
        ));
        // And I update the chief complaint to "Severe abdominal pain with dizziness"
        fillField(driver, "Chief Complaint", "Severe abdominal pain with dizziness");
        // And I submit the reassessment
        driver.findElement(By.cssSelector("[data-testid=\"submit-reassessment-form\"]")).click();

        // Then the system recalculates the ESI score from "4" to "2"
        var esiScore = getText(driver, "ESI Score");
        assertEquals("2", esiScore);
        // And the system updates the triage level from "Less Urgent" to "High Priority"
        var triageLevel = getText(driver, "Triage Level");
        assertEquals("High Priority", triageLevel);
        // And the patient is moved from position 12 to position 2 in the queue
        var queuePosition = getText(driver, "Queue Position");
        assertEquals("2", queuePosition);
        // And an escalation alert is sent to the charge nurse
        var escalationAlert = getText(driver, "Escalation Alert");
        assertMatches(escalationAlert, "charge nurse", true);
        // And the estimated wait time is updated from "90 minutes" to "15 minutes"
        var estimatedWaitTime = getText(driver, "Estimated Wait Time");
        assertEquals("15 minutes", estimatedWaitTime);
        // And a reassessment note is automatically added to the patient record
        var reassessmentNote = getText(driver, "Reassessment Note");
        assertTrue(reassessmentNote.length() > 0);
    }

    @Test
    @Order(2)
    @DisplayName("Reassess patient with stable condition")
    void reassessPatientWithStableCondition() {
        // Given a patient "Michael Chen" has been waiting in the queue for 2 hours
        // And the patient's initial triage was ESI Level 3 (Urgent)
        // And the patient's initial vital signs were:
        //   | Vital Sign          | Initial Value |
        //   | Blood Pressure      | 140/90        |
        //   | Heart Rate          | 95            |
        //   | Respiratory Rate    | 20            |
        //   | Temperature         | 100.2°F       |
        //   | Oxygen Saturation   | 96%           |
        //   | Pain Scale          | 6/10          |
        // (assumed pre-seeded test data)
        // When I perform a scheduled reassessment
        driver.findElement(By.cssSelector("[data-testid=\"select-patient-for-reassessment-button\"]")).click();

        // And I enter the updated vital signs:
        fillFields(driver, List.of(
            Map.of("Field", "Blood Pressure", "Value", "135/85"),
            Map.of("Field", "Heart Rate", "Value", "88"),
            Map.of("Field", "Respiratory Rate", "Value", "18"),
            Map.of("Field", "Temperature", "Value", "99.8°F"),
            Map.of("Field", "Oxygen Saturation", "Value", "97%"),
            Map.of("Field", "Pain Scale", "Value", "5/10")
        ));
        // And I note "Patient reports feeling slightly better"
        fillField(driver, "Reassessment Note", "Patient reports feeling slightly better");
        // And I submit the reassessment
        driver.findElement(By.cssSelector("[data-testid=\"submit-reassessment-form\"]")).click();

        // Then the system recalculates and maintains ESI score of "3"
        var esiScore = getText(driver, "ESI Score");
        assertEquals("3", esiScore);
        // And the triage level remains "Urgent"
        var triageLevel = getText(driver, "Triage Level");
        assertEquals("Urgent", triageLevel);
        // And the patient's queue position remains unchanged
        var queuePosition = getText(driver, "Queue Position");
        assertTrue(queuePosition.length() > 0);
        // And no escalation alerts are generated
        var escalationAlerts = driver.findElements(locator("Escalation Alert"));
        assertEquals(0, escalationAlerts.size());
        // And a reassessment note is added documenting stable condition
        var reassessmentNote = getText(driver, "Reassessment Note");
        assertEquals("Patient reports feeling slightly better", reassessmentNote);
        // And the next reassessment is scheduled for 1 hour
        var nextReassessmentSchedule = getText(driver, "Next Reassessment Schedule");
        assertEquals("1 hour", nextReassessmentSchedule);
    }

    @Test
    @Order(3)
    @DisplayName("Reassess patient with improving condition")
    void reassessPatientWithImprovingCondition() {
        // Given a patient "Lisa Rodriguez" has been waiting in the queue for 2 hours
        // And the patient's initial triage was ESI Level 2 (High Priority)
        // And the patient's initial vital signs were:
        //   | Vital Sign          | Initial Value |
        //   | Blood Pressure      | 170/100       |
        //   | Heart Rate          | 120           |
        //   | Respiratory Rate    | 28            |
        //   | Temperature         | 98.9°F        |
        //   | Oxygen Saturation   | 92%           |
        //   | Pain Scale          | 9/10          |
        // (assumed pre-seeded test data)
        // When I perform a reassessment
        driver.findElement(By.cssSelector("[data-testid=\"select-patient-for-reassessment-button\"]")).click();

        // And I enter the updated vital signs:
        fillFields(driver, List.of(
            Map.of("Field", "Blood Pressure", "Value", "145/85"),
            Map.of("Field", "Heart Rate", "Value", "95"),
            Map.of("Field", "Respiratory Rate", "Value", "20"),
            Map.of("Field", "Temperature", "Value", "98.6°F"),
            Map.of("Field", "Oxygen Saturation", "Value", "96%"),
            Map.of("Field", "Pain Scale", "Value", "4/10")
        ));
        // And I note "Patient reports significant improvement after medication"
        fillField(driver, "Reassessment Note", "Patient reports significant improvement after medication");
        // And I submit the reassessment
        driver.findElement(By.cssSelector("[data-testid=\"submit-reassessment-form\"]")).click();

        // Then the system recalculates the ESI score from "2" to "3"
        var esiScore = getText(driver, "ESI Score");
        assertEquals("3", esiScore);
        // And the system updates the triage level from "High Priority" to "Urgent"
        var triageLevel = getText(driver, "Triage Level");
        assertEquals("Urgent", triageLevel);
        // And the patient is moved from position 1 to position 5 in the queue
        var queuePosition = getText(driver, "Queue Position");
        assertEquals("5", queuePosition);
        // And the charge nurse is notified of the priority change
        var chargeNurseNotification = getText(driver, "Charge Nurse Notification");
        assertMatches(chargeNurseNotification, "priority change", true);
        // And the estimated wait time is updated from "Immediate" to "45 minutes"
        var estimatedWaitTime = getText(driver, "Estimated Wait Time");
        assertEquals("45 minutes", estimatedWaitTime);
        // And higher priority patients are moved up in the queue
        var queueEntries = driver.findElements(By.cssSelector("[data-testid=\"triage-queue-entry\"]"));
        assertTrue(queueEntries.size() > 0);
    }

    @Test
    @Order(4)
    @DisplayName("Automatic reassessment alert triggers")
    void automaticReassessmentAlertTriggers() {
        // Given multiple patients have been waiting for extended periods:
        //   | Patient Name     | Wait Time | Current ESI | Due for Reassessment |
        //   | John Williams    | 2 hours   | 4           | Yes                  |
        //   | Emma Thompson    | 1.5 hours | 3           | No                   |
        //   | David Kim        | 3 hours   | 3           | Yes                  |
        // (assumed pre-seeded test data)
        // When the system performs its hourly reassessment check
        driver.findElement(By.cssSelector("[data-testid=\"trigger-hourly-reassessment-check-button\"]")).click();

        // Then reassessment alerts are generated for:
        //   | Patient Name  | Alert Type           | Reason                    |
        //   | John Williams | Standard Reassess    | 2 hours ESI Level 4       |
        //   | David Kim     | Urgent Reassess      | 3 hours ESI Level 3       |
        var reassessmentAlerts = driver.findElements(By.cssSelector("[data-testid=\"reassessment-alert\"]"));
        assertEquals(2, reassessmentAlerts.size());
        // And the alerts appear on the triage nurse dashboard
        var triageNurseDashboard = waitForTestId(driver, "Triage Nurse Dashboard");
        assertTrue(triageNurseDashboard.isDisplayed());
        // And the patients are flagged with "Reassessment Due" status
        var reassessmentDueFlags = driver.findElements(By.cssSelector("[data-testid=\"reassessment-due-flag\"]"));
        assertEquals(2, reassessmentDueFlags.size());
        // And Emma Thompson does not receive an alert
        var alertTexts = reassessmentAlerts.stream().map(WebElement::getText).toList();
        assertFalse(alertTexts.stream().anyMatch(text -> text.contains("Emma Thompson")));
    }

    @Test
    @Order(5)
    @DisplayName("Handle patient who becomes critical during reassessment")
    void handlePatientWhoBecomesCriticalDuringReassessment() {
        // Given a patient "Robert Martinez" has been waiting in the queue for 2 hours
        // And the patient's initial triage was ESI Level 3 (Urgent)
        // When I begin the reassessment process
        driver.findElement(By.cssSelector("[data-testid=\"select-patient-for-reassessment-button\"]")).click();
        // And I observe the patient is now unresponsive

        // And I enter critical vital signs:
        fillFields(driver, List.of(
            Map.of("Field", "Blood Pressure", "Value", "60/30"),
            Map.of("Field", "Heart Rate", "Value", "150"),
            Map.of("Field", "Respiratory Rate", "Value", "6"),
            Map.of("Field", "Temperature", "Value", "96.2°F"),
            Map.of("Field", "Oxygen Saturation", "Value", "80%"),
            Map.of("Field", "Consciousness", "Value", "Unresponsive")
        ));
        // And I submit the emergency reassessment
        driver.findElement(By.cssSelector("[data-testid=\"submit-reassessment-form\"]")).click();

        // Then the system immediately calculates ESI score as "1"
        var esiScore = getText(driver, "ESI Score");
        assertEquals("1", esiScore);
        // And the system updates triage level to "Resuscitation"
        var triageLevel = getText(driver, "Triage Level");
        assertEquals("Resuscitation", triageLevel);
        // And the patient is moved to the top of all queues
        var queuePosition = getText(driver, "Queue Position");
        assertEquals("1", queuePosition);
        // And a code blue alert is automatically triggered
        var codeBlueAlert = getText(driver, "Code Blue Alert");
        assertMatches(codeBlueAlert, "triggered", true);
        // And the rapid response team is notified immediately
        var rapidResponseNotification = getText(driver, "Rapid Response Notification");
        assertMatches(rapidResponseNotification, "notified", true);
        // And the patient is flagged for immediate intervention
        var interventionFlag = getText(driver, "Intervention Flag");
        assertMatches(interventionFlag, "immediate intervention", true);
        // And I am prompted to initiate emergency protocols
        waitForTestId(driver, "Emergency Protocol Prompt");
    }

    @Test
    @Order(6)
    @DisplayName("Reassess pediatric patient with different parameters")
    void reassessPediatricPatientWithDifferentParameters() {
        // Given a pediatric patient "Amy Foster" (age 8) has been waiting for 2 hours
        // And the patient's initial triage was ESI Level 3 (Urgent)
        // When I perform a pediatric reassessment
        driver.findElement(By.cssSelector("[data-testid=\"select-patient-for-reassessment-button\"]")).click();

        // And I enter updated vital signs using age-appropriate parameters:
        fillFields(driver, List.of(
            Map.of("Field", "Blood Pressure", "Value", "85/50"),
            Map.of("Field", "Heart Rate", "Value", "140"),
            Map.of("Field", "Respiratory Rate", "Value", "32"),
            Map.of("Field", "Temperature", "Value", "103.8°F"),
            Map.of("Field", "Oxygen Saturation", "Value", "93%"),
            Map.of("Field", "Pain Scale (FACES)", "Value", "8/10")
        ));
        // And I note "Child appears more lethargic than initial assessment"
        fillField(driver, "Reassessment Note", "Child appears more lethargic than initial assessment");
        // And I submit the pediatric reassessment
        driver.findElement(By.cssSelector("[data-testid=\"submit-reassessment-form\"]")).click();

        // Then the system recalculates using pediatric ESI criteria
        var scoringCriteria = getText(driver, "Scoring Criteria");
        assertMatches(scoringCriteria, "pediatric", true);
        // And the ESI score is updated from "3" to "2"
        var esiScore = getText(driver, "ESI Score");
        assertEquals("2", esiScore);
        // And the triage level is updated to "High Priority"
        var triageLevel = getText(driver, "Triage Level");
        assertEquals("High Priority", triageLevel);
        // And the pediatric emergency team is notified
        var pediatricTeamNotification = getText(driver, "Pediatric Team Notification");
        assertMatches(pediatricTeamNotification, "notified", true);
        // And the patient is moved to the pediatric high priority queue
        var assignedQueue = getText(driver, "Assigned Queue");
        assertMatches(assignedQueue, "pediatric high priority", true);
        // And parent/guardian notification protocols are initiated
        var guardianNotification = getText(driver, "Guardian Notification");
        assertMatches(guardianNotification, "initiated", true);
    }

    @Test
    @Order(7)
    @DisplayName("Document reassessment with no vital sign changes")
    void documentReassessmentWithNoVitalSignChanges() {
        // Given a patient "Catherine Lee" has been waiting for 2 hours
        // And a reassessment is due
        // When I perform the reassessment
        driver.findElement(By.cssSelector("[data-testid=\"select-patient-for-reassessment-button\"]")).click();
        // And the vital signs remain identical to the initial assessment

        // But I note "Patient reports increased anxiety about wait time"
        fillField(driver, "Reassessment Note", "Patient reports increased anxiety about wait time");
        // And I provide reassurance and update on expected wait time
        // And I submit the reassessment
        driver.findElement(By.cssSelector("[data-testid=\"submit-reassessment-form\"]")).click();

        // Then the ESI score and triage level remain unchanged
        var esiScore = getText(driver, "ESI Score");
        assertTrue(esiScore.length() > 0);
        var triageLevel = getText(driver, "Triage Level");
        assertTrue(triageLevel.length() > 0);
        // And the queue position is maintained
        var queuePosition = getText(driver, "Queue Position");
        assertTrue(queuePosition.length() > 0);
        // And a documentation note is added about patient anxiety
        var reassessmentNote = getText(driver, "Reassessment Note");
        assertMatches(reassessmentNote, "anxiety", true);
        // And comfort measures are suggested in the patient instructions
        var patientInstructions = getText(driver, "Patient Instructions");
        assertMatches(patientInstructions, "comfort", true);
        // And the next reassessment interval is maintained
        var nextReassessmentSchedule = getText(driver, "Next Reassessment Schedule");
        assertTrue(nextReassessmentSchedule.length() > 0);
    }

    @Test
    @Order(8)
    @DisplayName("Handle reassessment during shift change")
    void handleReassessmentDuringShiftChange() {
        // Given a patient "Thomas Wilson" is due for reassessment
        // And the day shift triage nurse is preparing to leave
        // And the night shift triage nurse is arriving
        // When the day shift nurse initiates the reassessment handoff
        driver.findElement(By.cssSelector("[data-testid=\"initiate-reassessment-handoff-button\"]")).click();
        // And transfers the patient assessment to the night shift nurse
        driver.findElement(By.cssSelector("[data-testid=\"transfer-assessment-button\"]")).click();

        // Then the reassessment timing is preserved
        var reassessmentTiming = getText(driver, "Reassessment Timing");
        assertTrue(reassessmentTiming.length() > 0);
        // And all previous assessment data remains accessible
        var previousAssessmentData = waitForTestId(driver, "Previous Assessment Data");
        assertTrue(previousAssessmentData.isDisplayed());
        // And the night shift nurse can complete the reassessment
        var reassessmentForm = driver.findElement(By.cssSelector("[data-testid=\"reassessment-form\"]"));
        assertTrue(reassessmentForm.isDisplayed());
        // And continuity of care documentation is maintained
        var continuityDocumentation = getText(driver, "Continuity Documentation");
        assertTrue(continuityDocumentation.length() > 0);
        // And the handoff is logged in the system audit trail
        var auditTrailEntries = driver.findElements(By.cssSelector("[data-testid=\"audit-trail-entry\"]"));
        assertTrue(auditTrailEntries.size() > 0);
    }
}
