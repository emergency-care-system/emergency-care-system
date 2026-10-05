// Selenium WebDriver + JUnit 5 test for
// tests-with-given-when-then-features/03-initial-triage-assessment.feature
// (equivalent to tests-with-selenium-javascript/03-initial-triage-assessment.test.js).
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
class T03InitialTriageAssessmentTest {
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
        //   And the ESI (Emergency Severity Index) scoring module is active
        verifySystemIsOperational(driver);
        login(driver, "a triage nurse");
        // The ESI scoring module being active is assumed pre-seeded test data /
        // environment configuration.

        var featureNavLink = waitForTestId(driver, "Nav Initial Triage Assessment");
        featureNavLink.click();
        waitForTestId(driver, "Initial Triage Assessment Panel");
    }

    @Test
    @Order(1)
    @DisplayName("Assess patient with chest pain (ESI Level 2)")
    void assessPatientWithChestPainESILevel2() {
        // Given a registered patient "John Doe" is waiting for triage
        // And the patient was registered 10 minutes ago
        // (assumed pre-seeded test data)
        // When I select the patient for triage assessment
        driver.findElement(By.cssSelector("[data-testid=\"select-patient-for-triage-button\"]")).click();

        // And I enter the vital signs:
        fillFields(driver, List.of(
            Map.of("Field", "Blood Pressure", "Value", "160/95"),
            Map.of("Field", "Heart Rate", "Value", "110"),
            Map.of("Field", "Respiratory Rate", "Value", "22"),
            Map.of("Field", "Temperature", "Value", "98.6°F"),
            Map.of("Field", "Oxygen Saturation", "Value", "94%")
        ));
        // And I enter the chief complaint as "Chest pain and shortness of breath"
        fillField(driver, "Chief Complaint", "Chest pain and shortness of breath");
        // And I enter the pain scale as "8/10"
        fillField(driver, "Pain Scale", "8/10");
        // And I document onset as "Started 2 hours ago"
        fillField(driver, "Onset", "Started 2 hours ago");
        // And I submit the triage assessment
        driver.findElement(By.cssSelector("[data-testid=\"submit-triage-assessment-form\"]")).click();

        // Then the system calculates an ESI score of "2"
        var esiScore = getText(driver, "ESI Score");
        assertEquals("2", esiScore);
        // And the system assigns triage level "High Priority"
        var triageLevel = getText(driver, "Triage Level");
        assertEquals("High Priority", triageLevel);
        // And the patient is positioned at the front of the high priority queue
        var queuePosition = getText(driver, "Queue Position");
        assertEquals("1", queuePosition);
        // And an alert is sent to the attending physician
        var physicianAlert = getText(driver, "Physician Alert");
        assertMatches(physicianAlert, "attending physician", true);
        // And the estimated wait time is updated to "Immediate"
        var estimatedWaitTime = getText(driver, "Estimated Wait Time");
        assertEquals("Immediate", estimatedWaitTime);
    }

    @Test
    @Order(2)
    @DisplayName("Assess patient with minor injury (ESI Level 4)")
    void assessPatientWithMinorInjuryESILevel4() {
        // Given a registered patient "Jane Smith" is waiting for triage
        // When I select the patient for triage assessment
        driver.findElement(By.cssSelector("[data-testid=\"select-patient-for-triage-button\"]")).click();

        // And I enter the vital signs:
        fillFields(driver, List.of(
            Map.of("Field", "Blood Pressure", "Value", "120/80"),
            Map.of("Field", "Heart Rate", "Value", "75"),
            Map.of("Field", "Respiratory Rate", "Value", "16"),
            Map.of("Field", "Temperature", "Value", "98.2°F"),
            Map.of("Field", "Oxygen Saturation", "Value", "99%")
        ));
        // And I enter the chief complaint as "Sprained ankle from fall"
        fillField(driver, "Chief Complaint", "Sprained ankle from fall");
        // And I enter the pain scale as "4/10"
        fillField(driver, "Pain Scale", "4/10");
        // And I document onset as "This morning while jogging"
        fillField(driver, "Onset", "This morning while jogging");
        // And I submit the triage assessment
        driver.findElement(By.cssSelector("[data-testid=\"submit-triage-assessment-form\"]")).click();

        // Then the system calculates an ESI score of "4"
        var esiScore = getText(driver, "ESI Score");
        assertEquals("4", esiScore);
        // And the system assigns triage level "Less Urgent"
        var triageLevel = getText(driver, "Triage Level");
        assertEquals("Less Urgent", triageLevel);
        // And the patient is positioned in the less urgent queue
        var assignedQueue = getText(driver, "Assigned Queue");
        assertMatches(assignedQueue, "less urgent", true);
        // And the estimated wait time is updated to "60-90 minutes"
        var estimatedWaitTime = getText(driver, "Estimated Wait Time");
        assertEquals("60-90 minutes", estimatedWaitTime);
        // And no immediate alerts are generated
        var physicianAlerts = driver.findElements(locator("Physician Alert"));
        assertEquals(0, physicianAlerts.size());
    }

    @Test
    @Order(3)
    @DisplayName("Assess critical patient requiring immediate attention (ESI Level 1)")
    void assessCriticalPatientRequiringImmediateAttentionESILevel1() {
        // Given a registered patient "Emergency Patient" is waiting for triage
        // When I select the patient for triage assessment
        driver.findElement(By.cssSelector("[data-testid=\"select-patient-for-triage-button\"]")).click();

        // And I enter the vital signs:
        fillFields(driver, List.of(
            Map.of("Field", "Blood Pressure", "Value", "70/40"),
            Map.of("Field", "Heart Rate", "Value", "140"),
            Map.of("Field", "Respiratory Rate", "Value", "8"),
            Map.of("Field", "Temperature", "Value", "95.0°F"),
            Map.of("Field", "Oxygen Saturation", "Value", "85%")
        ));
        // And I enter the chief complaint as "Unresponsive after motor vehicle accident"
        fillField(driver, "Chief Complaint", "Unresponsive after motor vehicle accident");
        // And I enter the pain scale as "Unable to assess"
        fillField(driver, "Pain Scale", "Unable to assess");
        // And I mark the patient as "Requires immediate life-saving intervention"
        fillField(driver, "Intervention Flag", "Requires immediate life-saving intervention");
        // And I submit the triage assessment
        driver.findElement(By.cssSelector("[data-testid=\"submit-triage-assessment-form\"]")).click();

        // Then the system calculates an ESI score of "1"
        var esiScore = getText(driver, "ESI Score");
        assertEquals("1", esiScore);
        // And the system assigns triage level "Resuscitation"
        var triageLevel = getText(driver, "Triage Level");
        assertEquals("Resuscitation", triageLevel);
        // And the patient is moved to the top of all queues
        var queuePosition = getText(driver, "Queue Position");
        assertEquals("1", queuePosition);
        // And a code alert is automatically triggered
        var codeAlert = getText(driver, "Code Alert");
        assertMatches(codeAlert, "triggered", true);
        // And the trauma team is notified immediately
        var traumaTeamNotification = getText(driver, "Trauma Team Notification");
        assertMatches(traumaTeamNotification, "notified", true);
        // And the estimated wait time shows "Immediate - In Progress"
        var estimatedWaitTime = getText(driver, "Estimated Wait Time");
        assertEquals("Immediate - In Progress", estimatedWaitTime);
    }

    @Test
    @Order(4)
    @DisplayName("Assess pediatric patient with fever (ESI Level 3)")
    void assessPediatricPatientWithFeverESILevel3() {
        // Given a registered patient "Tommy Jones" (age 5) is waiting for triage
        // When I select the patient for triage assessment
        driver.findElement(By.cssSelector("[data-testid=\"select-patient-for-triage-button\"]")).click();

        // And I enter the vital signs using pediatric parameters:
        fillFields(driver, List.of(
            Map.of("Field", "Blood Pressure", "Value", "95/60"),
            Map.of("Field", "Heart Rate", "Value", "120"),
            Map.of("Field", "Respiratory Rate", "Value", "24"),
            Map.of("Field", "Temperature", "Value", "103.2°F"),
            Map.of("Field", "Oxygen Saturation", "Value", "97%")
        ));
        // And I enter the chief complaint as "High fever and irritability"
        fillField(driver, "Chief Complaint", "High fever and irritability");
        // And I enter the pain scale as "6/10 (using FACES scale)"
        fillField(driver, "Pain Scale", "6/10 (using FACES scale)");
        // And I document onset as "Fever started yesterday evening"
        fillField(driver, "Onset", "Fever started yesterday evening");
        // And I submit the triage assessment
        driver.findElement(By.cssSelector("[data-testid=\"submit-triage-assessment-form\"]")).click();

        // Then the system calculates an ESI score of "3" using pediatric criteria
        var esiScore = getText(driver, "ESI Score");
        assertEquals("3", esiScore);
        var scoringCriteria = getText(driver, "Scoring Criteria");
        assertMatches(scoringCriteria, "pediatric", true);
        // And the system assigns triage level "Urgent"
        var triageLevel = getText(driver, "Triage Level");
        assertEquals("Urgent", triageLevel);
        // And the patient is positioned in the urgent pediatric queue
        var assignedQueue = getText(driver, "Assigned Queue");
        assertMatches(assignedQueue, "urgent pediatric", true);
        // And the pediatric team is notified
        var pediatricTeamNotification = getText(driver, "Pediatric Team Notification");
        assertMatches(pediatricTeamNotification, "notified", true);
        // And the estimated wait time is updated to "30-45 minutes"
        var estimatedWaitTime = getText(driver, "Estimated Wait Time");
        assertEquals("30-45 minutes", estimatedWaitTime);
    }

    @Test
    @Order(5)
    @DisplayName("Handle incomplete vital signs during triage")
    void handleIncompleteVitalSignsDuringTriage() {
        // Given a registered patient "Mary Johnson" is waiting for triage
        // When I select the patient for triage assessment
        driver.findElement(By.cssSelector("[data-testid=\"select-patient-for-triage-button\"]")).click();

        // And I attempt to enter incomplete vital signs:
        //   | Vital Sign          | Value    |
        //   | Blood Pressure      | 130/85   |
        //   | Heart Rate          |          |
        //   | Respiratory Rate    | 18       |
        //   | Temperature         |          |
        //   | Oxygen Saturation   | 98%      |
        fillFields(driver, List.of(
            Map.of("Field", "Blood Pressure", "Value", "130/85"),
            Map.of("Field", "Heart Rate", "Value", ""),
            Map.of("Field", "Respiratory Rate", "Value", "18"),
            Map.of("Field", "Temperature", "Value", ""),
            Map.of("Field", "Oxygen Saturation", "Value", "98%")
        ));
        // And I enter the chief complaint as "Headache"
        fillField(driver, "Chief Complaint", "Headache");
        // And I submit the triage assessment
        driver.findElement(By.cssSelector("[data-testid=\"submit-triage-assessment-form\"]")).click();

        // Then the system displays validation errors:
        //   | Missing Field       | Error Message                |
        //   | Heart Rate          | Heart rate is required       |
        //   | Temperature         | Temperature is required      |
        assertEquals("Heart rate is required", getText(driver, "Heart Rate Error"));
        assertEquals("Temperature is required", getText(driver, "Temperature Error"));
        // And the ESI score cannot be calculated
        var esiScoreElements = driver.findElements(locator("ESI Score"));
        assertEquals(0, esiScoreElements.size());
        // And the assessment remains incomplete
        var assessmentStatus = getText(driver, "Assessment Status");
        assertEquals("Incomplete", assessmentStatus);
        // And I must complete all required fields before proceeding
        var triageForm = driver.findElement(By.cssSelector("[data-testid=\"triage-assessment-form\"]"));
        assertTrue(triageForm.isDisplayed());
    }

    @Test
    @Order(6)
    @DisplayName("Reassess patient with worsening condition")
    void reassessPatientWithWorseningCondition() {
        // Given a patient "Robert Davis" has been triaged as ESI Level 4
        // And the patient has been waiting for 90 minutes
        // When I select the patient for reassessment
        driver.findElement(By.cssSelector("[data-testid=\"select-patient-for-reassessment-button\"]")).click();

        // And I enter updated vital signs:
        fillFields(driver, List.of(
            Map.of("Field", "Blood Pressure", "Value", "90/50"),
            Map.of("Field", "Heart Rate", "Value", "120"),
            Map.of("Field", "Respiratory Rate", "Value", "26"),
            Map.of("Field", "Temperature", "Value", "101.5°F"),
            Map.of("Field", "Oxygen Saturation", "Value", "92%")
        ));
        // And I update the chief complaint to "Worsening abdominal pain with nausea"
        fillField(driver, "Chief Complaint", "Worsening abdominal pain with nausea");
        // And I enter the updated pain scale as "9/10"
        fillField(driver, "Pain Scale", "9/10");
        // And I submit the reassessment
        driver.findElement(By.cssSelector("[data-testid=\"submit-reassessment-form\"]")).click();

        // Then the system recalculates the ESI score to "2"
        var esiScore = getText(driver, "ESI Score");
        assertEquals("2", esiScore);
        // And the system updates triage level to "High Priority"
        var triageLevel = getText(driver, "Triage Level");
        assertEquals("High Priority", triageLevel);
        // And the patient is moved to the front of the high priority queue
        var queuePosition = getText(driver, "Queue Position");
        assertEquals("1", queuePosition);
        // And an escalation alert is sent to the charge nurse
        var escalationAlert = getText(driver, "Escalation Alert");
        assertMatches(escalationAlert, "charge nurse", true);
        // And a note is added documenting the condition change
        var conditionChangeNote = getText(driver, "Condition Change Note");
        assertTrue(conditionChangeNote.length() > 0);
    }

    @Test
    @Order(7)
    @DisplayName("Process multiple patients in triage queue")
    void processMultiplePatientsInTriageQueue() {
        // Given multiple patients are waiting for triage:
        //   | Patient Name    | Registration Time | Status        |
        //   | Alice Brown     | 10:00 AM           | Waiting       |
        //   | Bob Wilson      | 10:15 AM           | Waiting       |
        //   | Carol Davis     | 10:30 AM           | Waiting       |
        // (assumed pre-seeded test data)
        // When I complete triage assessments for all patients:
        //   | Patient Name | ESI Score | Triage Level  |
        //   | Alice Brown  | 3         | Urgent        |
        //   | Bob Wilson   | 4         | Less Urgent   |
        //   | Carol Davis  | 2         | High Priority |
        driver.findElement(By.cssSelector("[data-testid=\"complete-all-triage-assessments-button\"]")).click();

        // Then the system positions patients in queue order:
        //   | Queue Position | Patient Name | Triage Level  |
        //   | 1              | Carol Davis  | High Priority |
        //   | 2              | Alice Brown  | Urgent        |
        //   | 3              | Bob Wilson   | Less Urgent   |
        var queueEntries = driver.findElements(By.cssSelector("[data-testid=\"triage-queue-entry\"]"));
        assertEquals(3, queueEntries.size());
        var firstQueueEntryText = queueEntries.get(0).getText();
        assertMatches(firstQueueEntryText, "Carol Davis", false);
        // And wait times are calculated based on queue position and available resources
        var waitTimeCalculationStatus = getText(driver, "Wait Time Calculation Status");
        assertMatches(waitTimeCalculationStatus, "calculated", true);
        // And the triage dashboard is updated with current queue status
        var triageDashboard = waitForTestId(driver, "Triage Dashboard");
        assertTrue(triageDashboard.isDisplayed());
    }
}
