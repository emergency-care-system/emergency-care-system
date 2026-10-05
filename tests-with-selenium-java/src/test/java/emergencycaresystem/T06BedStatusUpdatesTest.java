// Selenium WebDriver + JUnit 5 test for
// tests-with-given-when-then-features/06-bed-status-updates.feature
// (equivalent to tests-with-selenium-javascript/06-bed-status-updates.test.js).
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
class T06BedStatusUpdatesTest {
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
        //   And I am logged in as a nurse
        //   And the bed management module is active
        //   And housekeeping notification system is enabled
        verifySystemIsOperational(driver);
        login(driver, "a nurse");
        // The bed management module and housekeeping notification system are
        // assumed to be pre-seeded/enabled test data.

        var featureNavLink = waitForTestId(driver, "Nav Bed Status Updates");
        featureNavLink.click();
        waitForTestId(driver, "Bed Status Updates Panel");
    }

    @Test
    @Order(1)
    @DisplayName("Mark bed as needs cleaning after patient discharge")
    void markBedAsNeedsCleaningAfterPatientDischarge() {
        // Given a patient "John Doe" is currently occupying bed "ED-12"
        // And the bed status is "Occupied"
        // And the available bed count shows 8 out of 20 beds available
        // (assumed pre-seeded test data)

        // When the patient is discharged from bed "ED-12"
        driver.findElement(By.cssSelector("[data-testid=\"discharge-patient-button\"]")).click();

        // And I mark the bed as "Needs Cleaning"
        fillField(driver, "New Bed Status", "Needs Cleaning");

        // And I submit the bed status update
        driver.findElement(By.cssSelector("[data-testid=\"submit-bed-status-update\"]")).click();

        // Then the system updates the bed status to "Dirty"
        waitForTestId(driver, "Bed Status");
        var bedStatus = getText(driver, "Bed Status");
        assertEquals("Dirty", bedStatus);

        // And a notification is sent to housekeeping with details:
        List<Map<String, String>> notificationFields = List.of(
            Map.of("Field", "Room Number", "Value", "ED-12"),
            Map.of("Field", "Status", "Value", "Needs Cleaning"),
            Map.of("Field", "Priority", "Value", "Standard"),
            Map.of("Field", "Patient Type", "Value", "Standard discharge"),
            Map.of("Field", "Special Requirements", "Value", "Standard cleaning protocol"),
            Map.of("Field", "Timestamp", "Value", "Current time")
        );
        for (var rowData : notificationFields) {
            var field = rowData.get("Field");
            var value = rowData.get("Value");
            assertEquals(value, getText(driver, field));
        }

        // And the bed is removed from the available bed count
        // And the available bed count updates to 7 out of 20 beds available
        var availableBedCount = getText(driver, "Available Bed Count");
        assertEquals("7 out of 20 beds available", availableBedCount);

        // And the bed appears as "Dirty" on the bed management dashboard
        var dashboardBedStatus = getText(driver, "Dashboard Bed Status");
        assertEquals("Dirty", dashboardBedStatus);
    }

    @Test
    @Order(2)
    @DisplayName("Mark isolation bed for deep cleaning after infectious patient")
    void markIsolationBedForDeepCleaningAfterInfectiousPatient() {
        // Given a patient "Jane Smith" with isolation precautions is in bed "ED-ISO-2"
        // And the bed status is "Occupied - Isolation"
        // And the patient had confirmed MRSA infection
        // (assumed pre-seeded test data)

        // When the patient is discharged from bed "ED-ISO-2"
        driver.findElement(By.cssSelector("[data-testid=\"discharge-patient-button\"]")).click();

        // And I mark the bed as "Needs Deep Cleaning"
        fillField(driver, "New Bed Status", "Needs Deep Cleaning");

        // And I specify the isolation type as "Contact Precautions - MRSA"
        fillField(driver, "Isolation Type", "Contact Precautions - MRSA");

        // And I submit the bed status update
        driver.findElement(By.cssSelector("[data-testid=\"submit-bed-status-update\"]")).click();

        // Then the system updates the bed status to "Dirty - Isolation"
        waitForTestId(driver, "Bed Status");
        var bedStatus = getText(driver, "Bed Status");
        assertEquals("Dirty - Isolation", bedStatus);

        // And a high-priority notification is sent to housekeeping with details:
        List<Map<String, String>> notificationFields = List.of(
            Map.of("Field", "Room Number", "Value", "ED-ISO-2"),
            Map.of("Field", "Status", "Value", "Needs Deep Cleaning"),
            Map.of("Field", "Priority", "Value", "High"),
            Map.of("Field", "Infection Type", "Value", "MRSA - Contact Precautions"),
            Map.of("Field", "Special Requirements", "Value", "Terminal cleaning required"),
            Map.of("Field", "PPE Required", "Value", "Gowns, gloves, masks")
        );
        for (var rowData : notificationFields) {
            var field = rowData.get("Field");
            var value = rowData.get("Value");
            assertEquals(value, getText(driver, field));
        }

        // And the bed is flagged as "Out of Service" until deep cleaning completion
        var bedServiceFlag = getText(driver, "Bed Service Flag");
        assertEquals("Out of Service", bedServiceFlag);

        // And the isolation bed count is reduced by one
        waitForTestId(driver, "Isolation Bed Count");

        // And an alert is sent to infection control team
        waitForTestId(driver, "Infection Control Alert");
    }

    @Test
    @Order(3)
    @DisplayName("Housekeeping completes cleaning and marks bed ready")
    void housekeepingCompletesCleaningAndMarksBedReady() {
        // Given bed "ED-8" has status "Dirty"
        // And housekeeping was notified 30 minutes ago
        // (assumed pre-seeded test data)

        // When the housekeeping staff completes cleaning of bed "ED-8"
        driver.findElement(By.cssSelector("[data-testid=\"complete-cleaning-button\"]")).click();

        // And the housekeeping supervisor marks the bed as "Clean and Ready"
        fillField(driver, "New Bed Status", "Clean and Ready");

        // And submits the cleaning completion with details:
        fillFields(driver, List.of(
            Map.of("Field", "Cleaning Staff", "Value", "Maria Rodriguez"),
            Map.of("Field", "Cleaning Start", "Value", "14:30"),
            Map.of("Field", "Cleaning End", "Value", "15:00"),
            Map.of("Field", "Cleaning Type", "Value", "Standard"),
            Map.of("Field", "Supplies Used", "Value", "Standard disinfection")
        ));
        driver.findElement(By.cssSelector("[data-testid=\"submit-cleaning-completion-form\"]")).click();

        // Then the system updates the bed status to "Available"
        waitForTestId(driver, "Bed Status");
        var bedStatus = getText(driver, "Bed Status");
        assertEquals("Available", bedStatus);

        // And the bed is added back to the available bed count
        // And the available bed count increases by one
        waitForTestId(driver, "Available Bed Count");

        // And a notification is sent to the charge nurse that bed "ED-8" is ready
        waitForTestId(driver, "Charge Nurse Notification");

        // And the bed appears as "Available" on the bed management dashboard
        var dashboardBedStatus = getText(driver, "Dashboard Bed Status");
        assertEquals("Available", dashboardBedStatus);
    }

    @Test
    @Order(4)
    @DisplayName("Handle bed maintenance request during status update")
    void handleBedMaintenanceRequestDuringStatusUpdate() {
        // Given a patient is discharged from bed "ED-15"
        // (assumed pre-seeded test data)

        // When I attempt to mark the bed as "Needs Cleaning"
        fillField(driver, "New Bed Status", "Needs Cleaning");

        // And I notice equipment malfunction in the room
        // (observation, no direct UI action)

        // And I select "Maintenance Required" in addition to cleaning needs
        driver.findElement(By.cssSelector("[data-testid=\"maintenance-required-checkbox\"]")).click();

        // And I specify the issue as "IV pump not functioning, call light broken"
        fillField(driver, "Issue Description", "IV pump not functioning, call light broken");

        // And I submit the bed status update
        driver.findElement(By.cssSelector("[data-testid=\"submit-bed-status-update\"]")).click();

        // Then the system updates the bed status to "Out of Service - Maintenance"
        waitForTestId(driver, "Bed Status");
        var bedStatus = getText(driver, "Bed Status");
        assertEquals("Out of Service - Maintenance", bedStatus);

        // And notifications are sent to both:
        List<Map<String, String>> notifications = List.of(
            Map.of("Department", "Housekeeping", "Notification Details", "Hold cleaning until maintenance"),
            Map.of("Department", "Maintenance", "Notification Details", "IV pump and call light repair")
        );
        for (var notification : notifications) {
            assertEquals(notification.get("Notification Details"), getText(driver, notification.get("Department")));
        }

        // And the bed is removed from available count until both issues are resolved
        waitForTestId(driver, "Available Bed Count");

        // And a work order is automatically generated for maintenance
        waitForTestId(driver, "Maintenance Work Order");

        // And the estimated downtime is calculated and displayed
        var estimatedDowntime = getText(driver, "Estimated Downtime");
        assertTrue(estimatedDowntime.length() > 0);
    }

    @Test
    @Order(5)
    @DisplayName("Update bed status during patient transfer")
    void updateBedStatusDuringPatientTransfer() {
        // Given a patient "Robert Wilson" is in bed "ED-6"
        // And the patient needs to be transferred to ICU
        // (assumed pre-seeded test data)

        // When the transport team arrives to transfer the patient
        driver.findElement(By.cssSelector("[data-testid=\"transport-team-arrived-button\"]")).click();

        // And I update the bed status to "Patient in Transit"
        fillField(driver, "New Bed Status", "Patient in Transit");

        // And I specify the destination as "ICU Room 302"
        fillField(driver, "Destination", "ICU Room 302");

        // And I submit the status update
        driver.findElement(By.cssSelector("[data-testid=\"submit-bed-status-update\"]")).click();

        // Then the bed status is temporarily set to "In Transit"
        waitForTestId(driver, "Bed Status");
        var bedStatus = getText(driver, "Bed Status");
        assertEquals("In Transit", bedStatus);

        // And the bed remains unavailable for new assignments
        var bedAvailability = getText(driver, "Bed Availability");
        assertMatches(bedAvailability, "unavailable", true);

        // And a notification is sent to the receiving unit
        waitForTestId(driver, "Receiving Unit Notification");

        // And when the transfer is confirmed complete, I can mark the bed as "Needs Cleaning"
        driver.findElement(By.cssSelector("[data-testid=\"confirm-transfer-complete-button\"]")).click();
        fillField(driver, "New Bed Status", "Needs Cleaning");

        // And the normal cleaning workflow is initiated
        waitForTestId(driver, "Cleaning Workflow Status");
    }

    @Test
    @Order(6)
    @DisplayName("Handle multiple bed status updates simultaneously")
    void handleMultipleBedStatusUpdatesSimultaneously() {
        // Given multiple beds require status updates:
        // (assumed pre-seeded test data)

        // When I perform batch bed status updates:
        fillFields(driver, List.of(
            Map.of("Field", "ED-3 New Status", "Value", "Needs Cleaning"),
            Map.of("Field", "ED-7 New Status", "Value", "Patient in Transit"),
            Map.of("Field", "ED-11 New Status", "Value", "Available"),
            Map.of("Field", "ED-14 New Status", "Value", "Needs Cleaning")
        ));
        driver.findElement(By.cssSelector("[data-testid=\"submit-batch-bed-status-update\"]")).click();

        // Then the system processes all updates simultaneously
        waitForTestId(driver, "Batch Update Status");

        // And appropriate notifications are sent to all relevant departments
        waitForTestId(driver, "Department Notifications Sent");

        // And the bed availability dashboard is updated in real-time
        waitForTestId(driver, "Bed Availability Dashboard");

        // And the total available bed count reflects all changes
        waitForTestId(driver, "Available Bed Count");
    }

    @Test
    @Order(7)
    @DisplayName("Handle urgent bed turnover request")
    void handleUrgentBedTurnoverRequest() {
        // Given the ED is at 95% capacity
        // And there is a trauma patient incoming requiring immediate bed
        // And bed "ED-4" patient is ready for discharge
        // (assumed pre-seeded test data)

        // When I mark the discharge as "Urgent Turnover Required"
        fillField(driver, "New Bed Status", "Urgent Turnover Required");

        // And I request expedited cleaning for bed "ED-4"
        driver.findElement(By.cssSelector("[data-testid=\"request-expedited-cleaning-button\"]")).click();

        // And I submit the urgent status update
        driver.findElement(By.cssSelector("[data-testid=\"submit-bed-status-update\"]")).click();

        // Then the system updates bed status to "Dirty - Urgent"
        waitForTestId(driver, "Bed Status");
        var bedStatus = getText(driver, "Bed Status");
        assertEquals("Dirty - Urgent", bedStatus);

        // And a high-priority notification is sent to housekeeping:
        List<Map<String, String>> notificationFields = List.of(
            Map.of("Field", "Priority Level", "Value", "URGENT"),
            Map.of("Field", "Room Number", "Value", "ED-4"),
            Map.of("Field", "Reason", "Value", "Incoming trauma patient"),
            Map.of("Field", "Target Time", "Value", "15 minutes"),
            Map.of("Field", "Special Instructions", "Value", "Expedited cleaning protocol")
        );
        for (var rowData : notificationFields) {
            var field = rowData.get("Field");
            var value = rowData.get("Value");
            assertEquals(value, getText(driver, field));
        }

        // And the charge nurse is notified of the urgent turnover request
        waitForTestId(driver, "Charge Nurse Notification");

        // And a timer is started to track cleaning completion time
        waitForTestId(driver, "Cleaning Completion Timer");
    }

    @Test
    @Order(8)
    @DisplayName("Validate bed status change restrictions")
    void validateBedStatusChangeRestrictions() {
        // Given bed "ED-9" currently has status "Occupied"
        // And a patient "Susan Davis" is actively receiving treatment
        // (assumed pre-seeded test data)

        // When I attempt to mark the bed as "Available"
        fillField(driver, "New Bed Status", "Available");

        // And I submit the invalid status change
        driver.findElement(By.cssSelector("[data-testid=\"submit-bed-status-update\"]")).click();

        // Then the system displays a validation error:
        List<Map<String, String>> validationErrors = List.of(
            Map.of("Error Type", "Invalid Transition", "Message", "Cannot mark occupied bed as available"),
            Map.of("Error Type", "Required Action", "Message", "Discharge patient first"),
            Map.of("Error Type", "Current Patient", "Message", "Susan Davis - Active treatment")
        );
        for (var error : validationErrors) {
            assertEquals(error.get("Message"), getText(driver, error.get("Error Type")));
        }

        // And the bed status remains "Occupied"
        var bedStatus = getText(driver, "Bed Status");
        assertEquals("Occupied", bedStatus);

        // And no notifications are sent
        var notifications = driver.findElements(By.cssSelector("[data-testid=\"housekeeping-notification\"]"));
        assertEquals(0, notifications.size());

        // And I am prompted to follow proper discharge workflow
        waitForTestId(driver, "Discharge Workflow Prompt");
    }

    @Test
    @Order(9)
    @DisplayName("Track bed status history and audit trail")
    void trackBedStatusHistoryAndAuditTrail() {
        // Given bed "ED-5" has had multiple status changes today
        // (assumed pre-seeded test data)

        // When I access the bed status history
        driver.findElement(By.cssSelector("[data-testid=\"view-bed-status-history-button\"]")).click();

        // Then the system displays the complete audit trail:
        var auditTrailEntries = driver.findElements(By.cssSelector("[data-testid=\"audit-trail-entry\"]"));
        assertEquals(5, auditTrailEntries.size());

        // And each status change includes timestamp and user identification
        waitForTestId(driver, "Audit Trail");

        // And the audit trail is preserved for compliance reporting
        var complianceStatus = getText(driver, "Compliance Reporting Status");
        assertMatches(complianceStatus, "preserved", true);

        // And I can generate reports on bed utilization patterns
        waitForTestId(driver, "Generate Utilization Report Button");
    }

    @Test
    @Order(10)
    @DisplayName("Handle bed status update during system maintenance")
    void handleBedStatusUpdateDuringSystemMaintenance() {
        // Given the housekeeping notification system is temporarily offline
        // And a patient is discharged from bed "ED-16"
        // (assumed pre-seeded test data)

        // When I mark the bed as "Needs Cleaning"
        fillField(driver, "New Bed Status", "Needs Cleaning");

        // And I submit the status update
        driver.findElement(By.cssSelector("[data-testid=\"submit-bed-status-update\"]")).click();

        // Then the system updates the bed status to "Dirty"
        waitForTestId(driver, "Bed Status");
        var bedStatus = getText(driver, "Bed Status");
        assertEquals("Dirty", bedStatus);

        // And the system queues the housekeeping notification for later delivery
        waitForTestId(driver, "Queued Notification Status");

        // And a warning message is displayed: "Housekeeping system offline - notification queued"
        var warningMessage = getText(driver, "Warning Message");
        assertEquals("Housekeeping system offline - notification queued", warningMessage);

        // And the bed is still removed from available count
        waitForTestId(driver, "Available Bed Count");

        // And when the housekeeping system comes back online, queued notifications are automatically sent
        // (system behavior outside the scope of this interaction)

        // And a log entry is created documenting the delayed notification
        waitForTestId(driver, "Delayed Notification Log Entry");
    }
}
