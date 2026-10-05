// Selenium WebDriver + JUnit 5 test for
// tests-with-given-when-then-features/02-ambulance-arrival-registration.feature
// (equivalent to tests-with-selenium-javascript/02-ambulance-arrival-registration.test.js).
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
class T02AmbulanceArrivalRegistrationTest {
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
        //   And I am logged in as registration staff
        //   And the "unknown patient" registration module is available
        verifySystemIsOperational(driver);
        login(driver, "registration staff");
        // The "unknown patient" registration module availability is assumed
        // pre-seeded test data / environment configuration.

        var featureNavLink = waitForTestId(driver, "Nav Ambulance Arrival Registration");
        featureNavLink.click();
        waitForTestId(driver, "Ambulance Arrival Registration Panel");
    }

    @Test
    @Order(1)
    @DisplayName("Register unconscious patient brought by ambulance")
    void registerUnconsciousPatientBroughtByAmbulance() {
        // Given an ambulance arrives with a patient who cannot provide identification
        // And the patient is unconscious and has no identification documents
        // And EMS provides the following information:
        //   | Field                 | Value                    |
        //   | Estimated Age         | 45-50 years              |
        //   | Gender                | Male                     |
        //   | Chief Complaint       | Motor vehicle accident   |
        //   | Vital Signs           | BP: 90/60, HR: 120       |
        //   | Incident Location     | Highway 55 Mile Marker 12|
        //   | EMS Unit              | Ambulance 205            |
        //   | Arrival Time          | 14:30                    |
        // When I select "Unknown Patient" registration type
        fillField(driver, "Registration Type", "Unknown Patient");
        // And I enter the available information from EMS
        fillFields(driver, List.of(
            Map.of("Field", "Estimated Age", "Value", "45-50 years"),
            Map.of("Field", "Gender", "Value", "Male"),
            Map.of("Field", "Chief Complaint", "Value", "Motor vehicle accident"),
            Map.of("Field", "Vital Signs", "Value", "BP: 90/60, HR: 120"),
            Map.of("Field", "Incident Location", "Value", "Highway 55 Mile Marker 12"),
            Map.of("Field", "EMS Unit", "Value", "Ambulance 205"),
            Map.of("Field", "Arrival Time", "Value", "14:30")
        ));
        // And I submit the registration
        driver.findElement(By.cssSelector("[data-testid=\"submit-registration-form\"]")).click();

        // Then the system creates a temporary patient record
        waitForTestId(driver, "Temporary Patient Record");
        // And the system assigns a placeholder ID starting with "UNK"
        var placeholderId = getText(driver, "Placeholder ID");
        assertMatches(placeholderId, "^UNK", false);
        // And the patient record is flagged for "Identity Verification Required"
        var identityFlag = getText(driver, "Identity Verification Flag");
        assertEquals("Identity Verification Required", identityFlag);
        // And the patient is immediately queued for triage
        var triageQueueStatus = getText(driver, "Triage Queue Status");
        assertMatches(triageQueueStatus, "queued for triage", true);
        // And a notification is sent to the charge nurse about the unknown patient
        var chargeNurseNotification = getText(driver, "Charge Nurse Notification");
        assertMatches(chargeNurseNotification, "unknown patient", true);
        // And the record shows status as "Temporary - Pending Identification"
        var recordStatus = getText(driver, "Record Status");
        assertEquals("Temporary - Pending Identification", recordStatus);
    }

    @Test
    @Order(2)
    @DisplayName("Register patient with partial identification from personal effects")
    void registerPatientWithPartialIdentificationFromPersonalEffects() {
        // Given an ambulance arrives with a patient who cannot provide identification
        // And the patient has a wallet with partial information
        // And EMS provides the following information:
        //   | Field                 | Value                    |
        //   | Estimated Age         | 30-35 years              |
        //   | Gender                | Female                   |
        //   | Chief Complaint       | Drug overdose            |
        //   | Found Name            | Sarah (from credit card) |
        //   | Partial Phone         | 555-1234 (last 4 digits) |
        // When I select "Unknown Patient" registration type
        fillField(driver, "Registration Type", "Unknown Patient");
        // And I enter the EMS information including partial identity details
        fillFields(driver, List.of(
            Map.of("Field", "Estimated Age", "Value", "30-35 years"),
            Map.of("Field", "Gender", "Value", "Female"),
            Map.of("Field", "Chief Complaint", "Value", "Drug overdose"),
            Map.of("Field", "Found Name", "Value", "Sarah (from credit card)"),
            Map.of("Field", "Partial Phone", "Value", "555-1234 (last 4 digits)")
        ));
        // And I mark the identity fields as "Unverified"
        fillField(driver, "Identity Status", "Unverified");
        // And I submit the registration
        driver.findElement(By.cssSelector("[data-testid=\"submit-registration-form\"]")).click();

        // Then the system creates a temporary patient record
        waitForTestId(driver, "Temporary Patient Record");
        // And the system assigns a placeholder ID starting with "UNK"
        var placeholderId = getText(driver, "Placeholder ID");
        assertMatches(placeholderId, "^UNK", false);
        // And the partial identity information is stored with "Unverified" status
        var identityStatus = getText(driver, "Identity Status");
        assertEquals("Unverified", identityStatus);
        // And the patient record is flagged for "Identity Verification Required"
        var identityFlag = getText(driver, "Identity Verification Flag");
        assertEquals("Identity Verification Required", identityFlag);
        // And a task is created for social services to assist with identification
        var socialServicesTask = getText(driver, "Social Services Task");
        assertMatches(socialServicesTask, "identification", true);
    }

    @Test
    @Order(3)
    @DisplayName("Register patient who becomes conscious during registration")
    void registerPatientWhoBecomesConsciousDuringRegistration() {
        // Given an ambulance arrives with a patient who initially cannot provide identification
        // And I have started the "Unknown Patient" registration process

        // When the patient becomes conscious and provides identification:
        fillFields(driver, List.of(
            Map.of("Field", "Full Name", "Value", "Michael Johnson"),
            Map.of("Field", "Date of Birth", "Value", "1980-12-15"),
            Map.of("Field", "Phone Number", "Value", "555-876-5432")
        ));
        // And I verify the provided identification
        driver.findElement(By.cssSelector("[data-testid=\"verify-identification-button\"]")).click();

        // Then the system converts the temporary record to a verified patient record
        waitForTestId(driver, "Medical Record Number");
        // And the placeholder ID is replaced with a permanent medical record number
        var medicalRecordNumber = getText(driver, "Medical Record Number");
        assertFalse(medicalRecordNumber.startsWith("UNK"));
        // And the "Identity Verification Required" flag is removed
        var identityFlagElements = driver.findElements(locator("Identity Verification Flag"));
        assertEquals(0, identityFlagElements.size());
        // And the patient demographic information is updated
        var patientName = getText(driver, "Patient Name");
        assertEquals("Michael Johnson", patientName);
        // And a note is added documenting the identification process
        var identificationNote = getText(driver, "Identification Note");
        assertTrue(identificationNote.length() > 0);
    }

    @Test
    @Order(4)
    @DisplayName("Handle multiple unknown patients from mass casualty incident")
    void handleMultipleUnknownPatientsFromMassCasualtyIncident() {
        // Given multiple ambulances arrive from a mass casualty incident
        // And none of the patients can provide identification

        // When I select "Unknown Patient - Mass Casualty" registration type
        fillField(driver, "Registration Type", "Unknown Patient - Mass Casualty");
        // And I enter the incident information:
        fillFields(driver, List.of(
            Map.of("Field", "Incident Type", "Value", "Multi-vehicle accident"),
            Map.of("Field", "Incident Location", "Value", "Interstate 70 Exit 45"),
            Map.of("Field", "Total Patients", "Value", "4")
        ));
        // And I register each patient with EMS-provided information
        driver.findElement(By.cssSelector("[data-testid=\"submit-registration-form\"]")).click();

        // Then the system creates temporary records for all patients
        var temporaryRecords = driver.findElements(By.cssSelector("[data-testid=\"temporary-patient-record\"]"));
        assertTrue(temporaryRecords.size() > 0);
        // And each patient gets a sequential placeholder ID (UNK-001, UNK-002, etc.)
        var placeholderId = getText(driver, "Placeholder ID");
        assertMatches(placeholderId, "^UNK-\\d{3}$", false);
        // And all records are linked to the same incident number
        var incidentNumber = getText(driver, "Incident Number");
        assertTrue(incidentNumber.length() > 0);
        // And the mass casualty protocol is activated
        var massCasualtyProtocolStatus = getText(driver, "Mass Casualty Protocol Status");
        assertMatches(massCasualtyProtocolStatus, "activated", true);
        // And notifications are sent to administration and social services
        var notificationRecipients = getText(driver, "Notification Recipients");
        assertMatches(notificationRecipients, "administration", true);
    }

    @Test
    @Order(5)
    @DisplayName("Attempt to register unknown patient without EMS information")
    void attemptToRegisterUnknownPatientWithoutEMSInformation() {
        // Given an ambulance arrives with a patient who cannot provide identification
        // And EMS has minimal information available

        // When I select "Unknown Patient" registration type
        fillField(driver, "Registration Type", "Unknown Patient");
        // And I attempt to submit with only basic information:
        //   | Field                 | Value                    |
        //   | Gender                | Unknown                  |
        //   | Estimated Age         | Unknown                  |
        //   | Chief Complaint       |                          |
        fillFields(driver, List.of(
            Map.of("Field", "Gender", "Value", "Unknown"),
            Map.of("Field", "Estimated Age", "Value", "Unknown"),
            Map.of("Field", "Chief Complaint", "Value", "")
        ));
        driver.findElement(By.cssSelector("[data-testid=\"submit-registration-form\"]")).click();

        // Then the system displays a warning "Insufficient information for registration"
        var warningMessage = getText(driver, "Warning Message");
        assertEquals("Insufficient information for registration", warningMessage);
        // And the system requires minimum data fields:
        //   | Required Field        | Requirement                       |
        //   | Estimated Age Range   | Must be provided                  |
        //   | Gender                | Must be Male, Female, or Unknown  |
        //   | Chief Complaint       | Must be provided                  |
        assertEquals("Must be provided", getText(driver, "Estimated Age Range Error"));
        assertEquals("Must be Male, Female, or Unknown", getText(driver, "Gender Error"));
        assertEquals("Must be provided", getText(driver, "Chief Complaint Error"));
        // And the registration cannot be completed until minimum requirements are met
        var placeholderIds = driver.findElements(locator("Placeholder ID"));
        assertEquals(0, placeholderIds.size());
    }

    @Test
    @Order(6)
    @DisplayName("Identity verification process after patient stabilization")
    void identityVerificationProcessAfterPatientStabilization() {
        // Given a patient was registered as "Unknown Patient"
        // And the patient has now stabilized
        // And the patient can provide identification

        // When the nurse initiates the identity verification process
        driver.findElement(By.cssSelector("[data-testid=\"initiate-identity-verification-button\"]")).click();
        // And the patient provides valid identification:
        fillFields(driver, List.of(
            Map.of("Field", "Full Name", "Value", "Robert Davis"),
            Map.of("Field", "Date of Birth", "Value", "1975-08-20"),
            Map.of("Field", "Social Security", "Value", "XXX-XX-1234 (last 4)")
        ));
        // And the identification is verified
        driver.findElement(By.cssSelector("[data-testid=\"verify-identification-button\"]")).click();

        // Then the system merges the temporary record with verified information
        waitForTestId(driver, "Medical Record Number");
        var mergeStatus = getText(driver, "Record Merge Status");
        assertMatches(mergeStatus, "merged", true);
        // And the "Identity Verification Required" flag is cleared
        var identityFlagElements = driver.findElements(locator("Identity Verification Flag"));
        assertEquals(0, identityFlagElements.size());
        // And a permanent medical record number is assigned
        var medicalRecordNumber = getText(driver, "Medical Record Number");
        assertTrue(medicalRecordNumber.length() > 0);
        // And all clinical documentation is preserved under the new verified record
        var clinicalDocumentationStatus = getText(driver, "Clinical Documentation Status");
        assertMatches(clinicalDocumentationStatus, "preserved", true);
        // And billing information is updated with verified patient details
        var billingStatus = getText(driver, "Billing Status");
        assertMatches(billingStatus, "updated", true);
    }
}
