// Selenium WebDriver + JUnit 5 test for
// tests-with-given-when-then-features/01-walk-in-patient-registration.feature
// (equivalent to tests-with-selenium-javascript/01-walk-in-patient-registration.test.js).
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
class T01WalkInPatientRegistrationTest {
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
        //   And I am logged in as a registration clerk
        verifySystemIsOperational(driver);
        login(driver, "a registration clerk");

        // The demo app is a single-page dashboard: after login, select this
        // feature's panel from the sidebar nav (data-testid="nav-<slug>").
        var registrationNavLink = waitForTestId(driver, "Nav Walk In Patient Registration");
        registrationNavLink.click();
        waitForTestId(driver, "Patient Registration Form");
    }

    @Test
    @Order(1)
    @DisplayName("Successfully register a new walk-in patient")
    void successfullyRegisterANewWalkInPatient() {
        // Given a new patient arrives at the ED without prior registration
        // And the patient provides valid identification
        // When I enter the patient's demographic information:
        fillFields(driver, List.of(
            Map.of("Field", "Given Name", "Value", "John"),
            Map.of("Field", "Family Name", "Value", "Doe"),
            Map.of("Field", "Date of Birth", "Value", "1985-06-15"),
            Map.of("Field", "Phone Number", "Value", "555-123-4567"),
            Map.of("Field", "Address", "Value", "123 Main St"),
            Map.of("Field", "City", "Value", "Springfield"),
            Map.of("Field", "State", "Value", "IL"),
            Map.of("Field", "Zip Code", "Value", "62701")
        ));
        // And I enter the patient's insurance details:
        fillFields(driver, List.of(
            Map.of("Field", "Insurance Type", "Value", "Blue Cross"),
            Map.of("Field", "Policy Number", "Value", "BC123456789"),
            Map.of("Field", "Group Number", "Value", "GRP001")
        ));
        // And I submit the registration form
        driver.findElement(By.cssSelector("[data-testid=\"submit-registration-form\"]")).click();

        // Then the system creates a unique patient record
        waitForTestId(driver, "Medical Record Number");
        // And the system assigns a medical record number
        var medicalRecordNumber = getText(driver, "Medical Record Number");
        assertTrue(medicalRecordNumber.length() > 0);
        // And the patient is queued for triage
        var triageQueueStatus = getText(driver, "Triage Queue Status");
        assertMatches(triageQueueStatus, "queued for triage", true);
        // And I see a confirmation message "Patient successfully registered"
        var confirmationMessage = getText(driver, "Confirmation Message");
        assertEquals("Patient successfully registered", confirmationMessage);
        // And the medical record number is displayed
        var medicalRecordNumberElement = driver.findElement(locator("Medical Record Number"));
        assertTrue(medicalRecordNumberElement.isDisplayed());
    }

    @Test
    @Order(2)
    @DisplayName("Register patient with missing insurance information")
    void registerPatientWithMissingInsuranceInformation() {
        // Given a new patient arrives at the ED without prior registration
        // And the patient does not have insurance information
        // When I enter the patient's demographic information:
        fillFields(driver, List.of(
            Map.of("Field", "Given Name", "Value", "Jane"),
            Map.of("Field", "Family Name", "Value", "Smith"),
            Map.of("Field", "Date of Birth", "Value", "1990-03-22"),
            Map.of("Field", "Phone Number", "Value", "555-987-6543"),
            Map.of("Field", "Address", "Value", "456 Oak Ave")
        ));
        // And I select "Self-Pay" as the insurance type
        fillField(driver, "Insurance Type", "Self-Pay");
        // And I submit the registration form
        driver.findElement(By.cssSelector("[data-testid=\"submit-registration-form\"]")).click();

        // Then the system creates a unique patient record
        waitForTestId(driver, "Medical Record Number");
        // And the system assigns a medical record number
        var medicalRecordNumber = getText(driver, "Medical Record Number");
        assertTrue(medicalRecordNumber.length() > 0);
        // And the patient is queued for triage
        var triageQueueStatus = getText(driver, "Triage Queue Status");
        assertMatches(triageQueueStatus, "queued for triage", true);
        // And the insurance status is marked as "Self-Pay"
        var insuranceStatus = getText(driver, "Insurance Status");
        assertEquals("Self-Pay", insuranceStatus);
    }

    @Test
    @Order(3)
    @DisplayName("Handle duplicate patient registration attempt")
    void handleDuplicatePatientRegistrationAttempt() {
        // Given a patient with the same name and date of birth already exists in the system
        // When I enter the patient's demographic information:
        fillFields(driver, List.of(
            Map.of("Field", "Given Name", "Value", "John"),
            Map.of("Field", "Family Name", "Value", "Doe"),
            Map.of("Field", "Date of Birth", "Value", "1985-06-15")
        ));
        // And I submit the registration form
        driver.findElement(By.cssSelector("[data-testid=\"submit-registration-form\"]")).click();

        // Then the system displays a warning "Potential duplicate patient found"
        var warningMessage = getText(driver, "Duplicate Patient Warning");
        assertEquals("Potential duplicate patient found", warningMessage);
        // And the system shows existing patient records for verification
        var existingRecords = driver.findElements(By.cssSelector("[data-testid=\"existing-patient-record\"]"));
        assertTrue(existingRecords.size() > 0);
        // And I can choose to link to existing record or create new record
        waitForTestId(driver, "Link to Existing Record");
        waitForTestId(driver, "Create New Record");
    }

    @Test
    @Order(4)
    @DisplayName("Registration with invalid demographic data")
    void registrationWithInvalidDemographicData() {
        // Given a new patient arrives at the ED without prior registration
        // When I enter incomplete demographic information:
        fillFields(driver, List.of(
            Map.of("Field", "Given Name", "Value", "John"),
            Map.of("Field", "Family Name", "Value", ""),
            Map.of("Field", "Date of Birth", "Value", "invalid-date")
        ));
        // And I submit the registration form
        driver.findElement(By.cssSelector("[data-testid=\"submit-registration-form\"]")).click();

        // Then the system displays validation errors:
        var familyNameError = getText(driver, "Family Name Error");
        assertEquals("Family name is required", familyNameError);
        var dateOfBirthError = getText(driver, "Date of Birth Error");
        assertEquals("Invalid date format", dateOfBirthError);
        // And the patient record is not created
        var medicalRecordNumbers = driver.findElements(locator("Medical Record Number"));
        assertEquals(0, medicalRecordNumbers.size());
        // And the form remains open for correction
        var registrationForm = driver.findElement(By.cssSelector("[data-testid=\"patient-registration-form\"]"));
        assertTrue(registrationForm.isDisplayed());
    }
}
