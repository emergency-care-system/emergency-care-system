// Selenium WebDriver + JUnit 5 test for
// tests-with-given-when-then-features/05-bed-assignment.feature
// (equivalent to tests-with-selenium-javascript/05-bed-assignment.test.js).
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
class T05BedAssignmentTest {
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
        //   And I am logged in as a charge nurse
        //   And the bed management module is active
        //   And the patient prioritization algorithm is enabled
        verifySystemIsOperational(driver);
        login(driver, "a charge nurse");
        // The bed management module and patient prioritization algorithm are
        // assumed to be pre-seeded/enabled test data.

        var featureNavLink = waitForTestId(driver, "Nav Bed Assignment");
        featureNavLink.click();
        waitForTestId(driver, "Bed Assignment Panel");
    }

    @Test
    @Order(1)
    @DisplayName("Assign standard room to highest acuity patient")
    void assignStandardRoomToHighestAcuityPatient() {
        // Given multiple patients are waiting for beds:
        // And a standard room "ED-12" becomes available
        // (assumed pre-seeded test data)

        // When I request bed assignment recommendations
        driver.findElement(By.cssSelector("[data-testid=\"request-recommendations-button\"]")).click();

        // Then the system suggests "Maria Gonzalez" as the top recommendation
        waitForTestId(driver, "Top Recommendation");
        var topRecommendation = getText(driver, "Top Recommendation");
        assertEquals("Maria Gonzalez", topRecommendation);

        // And the recommendation shows:
        List<Map<String, String>> recommendationFields = List.of(
            Map.of("Field", "Recommended Patient", "Value", "Maria Gonzalez"),
            Map.of("Field", "ESI Level", "Value", "2"),
            Map.of("Field", "Wait Time", "Value", "45 minutes"),
            Map.of("Field", "Room Match", "Value", "Standard room suitable"),
            Map.of("Field", "Rationale", "Value", "Highest acuity patient requiring standard bed")
        );
        for (var rowData : recommendationFields) {
            var field = rowData.get("Field");
            var value = rowData.get("Value");
            assertEquals(value, getText(driver, field));
        }

        // And the system displays updated wait times for remaining patients:
        var waitTimeUpdates = driver.findElements(By.cssSelector("[data-testid=\"wait-time-update-row\"]"));
        assertEquals(3, waitTimeUpdates.size());
    }

    @Test
    @Order(2)
    @DisplayName("Assign isolation room based on patient requirements")
    void assignIsolationRoomBasedOnPatientRequirements() {
        // Given multiple patients are waiting for beds:
        // And an isolation room "ED-ISO-2" becomes available
        // (assumed pre-seeded test data)

        // When I request bed assignment recommendations
        driver.findElement(By.cssSelector("[data-testid=\"request-recommendations-button\"]")).click();

        // Then the system suggests "Alex Johnson" as the top recommendation
        waitForTestId(driver, "Top Recommendation");
        var topRecommendation = getText(driver, "Top Recommendation");
        assertEquals("Alex Johnson", topRecommendation);

        // And the recommendation shows:
        List<Map<String, String>> recommendationFields = List.of(
            Map.of("Field", "Recommended Patient", "Value", "Alex Johnson"),
            Map.of("Field", "Room Type", "Value", "Isolation room"),
            Map.of("Field", "Rationale", "Value", "Patient requires isolation precautions"),
            Map.of("Field", "Infection Control", "Value", "Airborne precautions needed")
        );
        for (var rowData : recommendationFields) {
            var field = rowData.get("Field");
            var value = rowData.get("Value");
            assertEquals(value, getText(driver, field));
        }

        // And the system displays that other patients cannot use this room type
        var roomTypeRestriction = getText(driver, "Room Type Restriction Notice");
        assertTrue(roomTypeRestriction.length() > 0);

        // And the estimated wait times for standard rooms remain unchanged for other patients
        var standardRoomWaitTimeChange = getText(driver, "Standard Room Wait Time Change");
        assertEquals("Unchanged", standardRoomWaitTimeChange);
    }

    @Test
    @Order(3)
    @DisplayName("Handle pediatric room assignment")
    void handlePediatricRoomAssignment() {
        // Given multiple patients are waiting for beds:
        // And a pediatric room "ED-PEDS-3" becomes available
        // (assumed pre-seeded test data)

        // When I request bed assignment recommendations
        driver.findElement(By.cssSelector("[data-testid=\"request-recommendations-button\"]")).click();

        // Then the system suggests "Sarah Mitchell" as the top recommendation
        waitForTestId(driver, "Top Recommendation");
        var topRecommendation = getText(driver, "Top Recommendation");
        assertEquals("Sarah Mitchell", topRecommendation);

        // And the recommendation shows:
        List<Map<String, String>> recommendationFields = List.of(
            Map.of("Field", "Recommended Patient", "Value", "Sarah Mitchell"),
            Map.of("Field", "Age", "Value", "4 years old"),
            Map.of("Field", "ESI Level", "Value", "2"),
            Map.of("Field", "Room Type", "Value", "Pediatric room"),
            Map.of("Field", "Rationale", "Value", "Highest acuity pediatric patient")
        );
        for (var rowData : recommendationFields) {
            var field = rowData.get("Field");
            var value = rowData.get("Value");
            assertEquals(value, getText(driver, field));
        }

        // And the system displays updated wait times for remaining pediatric patients:
        var pediatricWaitTimeUpdates = driver.findElements(By.cssSelector("[data-testid=\"wait-time-update-row\"]"));
        assertEquals(2, pediatricWaitTimeUpdates.size());

        // And David Chen remains in the adult standard room queue
        var davidChenQueueStatus = getText(driver, "David Chen Queue Status");
        assertMatches(davidChenQueueStatus, "adult standard room queue", true);
    }

    @Test
    @Order(4)
    @DisplayName("No suitable patients for available room type")
    void noSuitablePatientsForAvailableRoomType() {
        // Given multiple patients are waiting for beds:
        // And a trauma room "ED-TRAUMA-1" becomes available
        // (assumed pre-seeded test data)

        // When I request bed assignment recommendations
        driver.findElement(By.cssSelector("[data-testid=\"request-recommendations-button\"]")).click();

        // Then the system displays "No suitable patients for trauma room"
        var noSuitablePatientsMessage = getText(driver, "No Suitable Patients Message");
        assertEquals("No suitable patients for trauma room", noSuitablePatientsMessage);

        // And the system suggests:
        List<Map<String, String>> suggestions = List.of(
            Map.of("Recommendation Type", "Alternative Use", "Details", "Consider using for high acuity standard patients"),
            Map.of("Recommendation Type", "Room Conversion", "Details", "Can be downgraded to standard room if needed"),
            Map.of("Recommendation Type", "Hold for Emergency", "Details", "Keep available for incoming trauma cases")
        );
        for (var suggestion : suggestions) {
            assertEquals(suggestion.get("Details"), getText(driver, suggestion.get("Recommendation Type")));
        }

        // And the system maintains the trauma room as available
        var traumaRoomStatus = getText(driver, "Trauma Room Status");
        assertEquals("Available", traumaRoomStatus);

        // And no patient assignments are automatically made
        var autoAssignments = driver.findElements(By.cssSelector("[data-testid=\"patient-assignment\"]"));
        assertEquals(0, autoAssignments.size());
    }

    @Test
    @Order(5)
    @DisplayName("Handle multiple rooms becoming available simultaneously")
    void handleMultipleRoomsBecomingAvailableSimultaneously() {
        // Given multiple patients are waiting for beds:
        // And multiple rooms become available:
        // (assumed pre-seeded test data)

        // When I request bed assignment recommendations
        driver.findElement(By.cssSelector("[data-testid=\"request-recommendations-button\"]")).click();

        // Then the system provides multiple recommendations:
        var recommendations = driver.findElements(By.cssSelector("[data-testid=\"bed-recommendation-row\"]"));
        assertEquals(3, recommendations.size());

        // And the system updates wait times for all remaining patients
        waitForTestId(driver, "Wait Times Updated Notice");

        // And the recommendations are ranked by patient acuity priority
        var rankingOrder = getText(driver, "Recommendation Ranking Order");
        assertEquals("Ranked by patient acuity priority", rankingOrder);
    }

    @Test
    @Order(6)
    @DisplayName("Consider patient gender for room assignment")
    void considerPatientGenderForRoomAssignment() {
        // Given multiple patients are waiting for beds:
        // And a standard room "ED-8" becomes available
        // And the room currently has a male patient in the adjacent bed
        // (assumed pre-seeded test data)

        // When I request bed assignment recommendations considering privacy preferences
        driver.findElement(By.cssSelector("[data-testid=\"request-recommendations-button\"]")).click();

        // Then the system suggests "Anthony Clark" as the top recommendation
        waitForTestId(driver, "Top Recommendation");
        var topRecommendation = getText(driver, "Top Recommendation");
        assertEquals("Anthony Clark", topRecommendation);

        // And the recommendation includes:
        List<Map<String, String>> recommendationFields = List.of(
            Map.of("Field", "Privacy Consideration", "Value", "Same gender as adjacent patient"),
            Map.of("Field", "Alternative Option", "Value", "Michelle Lee (if privacy not a concern)")
        );
        for (var rowData : recommendationFields) {
            var field = rowData.get("Field");
            var value = rowData.get("Value");
            assertEquals(value, getText(driver, field));
        }

        // And I can override the gender consideration if clinically necessary
        waitForTestId(driver, "Override Gender Consideration");
    }

    @Test
    @Order(7)
    @DisplayName("Handle bed assignment during high volume period")
    void handleBedAssignmentDuringHighVolumePeriod() {
        // Given the ED is operating at 95% capacity
        // And multiple high-acuity patients are waiting:
        // And a standard room "ED-6" becomes available
        // (assumed pre-seeded test data)

        // When I request bed assignment recommendations during crisis mode
        driver.findElement(By.cssSelector("[data-testid=\"request-recommendations-button\"]")).click();

        // Then the system prioritizes "Crisis Patient A" despite room type mismatch
        waitForTestId(driver, "Top Recommendation");
        var topRecommendation = getText(driver, "Top Recommendation");
        assertEquals("Crisis Patient A", topRecommendation);

        // And the system displays:
        List<Map<String, String>> alerts = List.of(
            Map.of("Alert Type", "High Volume Alert", "Message", "ED at capacity - emergency protocols active"),
            Map.of("Alert Type", "Room Flex Option", "Message", "Standard room can accommodate ESI Level 1"),
            Map.of("Alert Type", "Resource Alert", "Message", "Additional equipment may be needed")
        );
        for (var alert : alerts) {
            assertEquals(alert.get("Message"), getText(driver, alert.get("Alert Type")));
        }

        // And the system suggests moving lower acuity patients to make trauma rooms available
        var traumaRoomSuggestion = getText(driver, "Trauma Room Availability Suggestion");
        assertMatches(traumaRoomSuggestion, "moving lower acuity patients", true);
    }

    @Test
    @Order(8)
    @DisplayName("Update wait times after bed assignment")
    void updateWaitTimesAfterBedAssignment() {
        // Given the following patients are waiting:
        // And I assign Patient A to the available room
        // (assumed pre-seeded test data)

        // When the bed assignment is confirmed
        driver.findElement(By.cssSelector("[data-testid=\"confirm-bed-assignment-button\"]")).click();

        // Then the system recalculates wait times for remaining patients:
        var waitTimeUpdates = driver.findElements(By.cssSelector("[data-testid=\"wait-time-update-row\"]"));
        assertEquals(3, waitTimeUpdates.size());

        // And the updated wait times are displayed on the patient tracking board
        waitForTestId(driver, "Patient Tracking Board");

        // And family members are notified of updated estimates via the patient portal
        var familyNotificationStatus = getText(driver, "Family Notification Status");
        assertMatches(familyNotificationStatus, "notified", true);
    }

    @Test
    @Order(9)
    @DisplayName("Handle bed assignment rejection and alternative selection")
    void handleBedAssignmentRejectionAndAlternativeSelection() {
        // Given the system recommends "John Smith" for room "ED-10"
        // And John Smith has ESI Level 2 with chest pain
        // (assumed pre-seeded test data)

        // When I review the recommendation
        waitForTestId(driver, "Top Recommendation");

        // And I determine that John Smith needs cardiac monitoring not available in ED-10
        // (clinical judgement, no direct UI action)

        // And I reject the system recommendation
        driver.findElement(By.cssSelector("[data-testid=\"reject-recommendation-button\"]")).click();

        // Then the system provides alternative recommendations:
        var alternativeRecommendations = driver.findElements(By.cssSelector("[data-testid=\"alternative-recommendation-row\"]"));
        assertEquals(2, alternativeRecommendations.size());

        // And the system suggests alternative rooms for John Smith:
        var alternativeRooms = driver.findElements(By.cssSelector("[data-testid=\"alternative-room-row\"]"));
        assertEquals(2, alternativeRooms.size());

        // And I can select an alternative patient or wait for appropriate room for John Smith
        waitForTestId(driver, "Select Alternative Patient");
    }
}
