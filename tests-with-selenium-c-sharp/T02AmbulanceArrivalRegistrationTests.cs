// Selenium WebDriver + NUnit test for
// tests-with-given-when-then-features/02-ambulance-arrival-registration.feature
// (equivalent to tests-with-selenium-javascript/02-ambulance-arrival-registration.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see Support/Fields.cs) and the shared
// data-testid contract in Support/Login.cs (login-identity, login-submit,
// app-root).

namespace EmergencyCareSystem.SeleniumTests;

[TestFixture]
[NonParallelizable]
public class T02AmbulanceArrivalRegistrationTests
{
    private IWebDriver driver = null!;

    [OneTimeSetUp]
    public void SetUpClass()
    {
        driver = DriverFactory.Build();
    }

    [OneTimeTearDown]
    public void TearDownClass()
    {
        driver.Quit();
    }

    [SetUp]
    public void SetUp()
    {
        // Background:
        //   Given the emergency care system is operational
        //   And I am logged in as registration staff
        //   And the "unknown patient" registration module is available
        VerifySystemIsOperational(driver);
        Login(driver, "registration staff");
        // The "unknown patient" registration module availability is assumed
        // pre-seeded test data / environment configuration.

        var featureNavLink = WaitForTestId(driver, "Nav Ambulance Arrival Registration");
        featureNavLink.Click();
        WaitForTestId(driver, "Ambulance Arrival Registration Panel");
    }

    [Test, Order(1)]
    [Description("Register unconscious patient brought by ambulance")]
    public void RegisterUnconsciousPatientBroughtByAmbulance()
    {
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
        FillField(driver, "Registration Type", "Unknown Patient");
        // And I enter the available information from EMS
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Estimated Age"), ("Value", "45-50 years")),
            Row(("Field", "Gender"), ("Value", "Male")),
            Row(("Field", "Chief Complaint"), ("Value", "Motor vehicle accident")),
            Row(("Field", "Vital Signs"), ("Value", "BP: 90/60, HR: 120")),
            Row(("Field", "Incident Location"), ("Value", "Highway 55 Mile Marker 12")),
            Row(("Field", "EMS Unit"), ("Value", "Ambulance 205")),
            Row(("Field", "Arrival Time"), ("Value", "14:30")),
        });
        // And I submit the registration
        driver.FindElement(By.CssSelector("[data-testid=\"submit-registration-form\"]")).Click();

        // Then the system creates a temporary patient record
        WaitForTestId(driver, "Temporary Patient Record");
        // And the system assigns a placeholder ID starting with "UNK"
        var placeholderId = GetText(driver, "Placeholder ID");
        Assert.That(placeholderId, Does.Match(@"^UNK"));
        // And the patient record is flagged for "Identity Verification Required"
        var identityFlag = GetText(driver, "Identity Verification Flag");
        Assert.That(identityFlag, Is.EqualTo("Identity Verification Required"));
        // And the patient is immediately queued for triage
        var triageQueueStatus = GetText(driver, "Triage Queue Status");
        Assert.That(triageQueueStatus, Does.Match(@"queued for triage").IgnoreCase);
        // And a notification is sent to the charge nurse about the unknown patient
        var chargeNurseNotification = GetText(driver, "Charge Nurse Notification");
        Assert.That(chargeNurseNotification, Does.Match(@"unknown patient").IgnoreCase);
        // And the record shows status as "Temporary - Pending Identification"
        var recordStatus = GetText(driver, "Record Status");
        Assert.That(recordStatus, Is.EqualTo("Temporary - Pending Identification"));
    }

    [Test, Order(2)]
    [Description("Register patient with partial identification from personal effects")]
    public void RegisterPatientWithPartialIdentificationFromPersonalEffects()
    {
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
        FillField(driver, "Registration Type", "Unknown Patient");
        // And I enter the EMS information including partial identity details
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Estimated Age"), ("Value", "30-35 years")),
            Row(("Field", "Gender"), ("Value", "Female")),
            Row(("Field", "Chief Complaint"), ("Value", "Drug overdose")),
            Row(("Field", "Found Name"), ("Value", "Sarah (from credit card)")),
            Row(("Field", "Partial Phone"), ("Value", "555-1234 (last 4 digits)")),
        });
        // And I mark the identity fields as "Unverified"
        FillField(driver, "Identity Status", "Unverified");
        // And I submit the registration
        driver.FindElement(By.CssSelector("[data-testid=\"submit-registration-form\"]")).Click();

        // Then the system creates a temporary patient record
        WaitForTestId(driver, "Temporary Patient Record");
        // And the system assigns a placeholder ID starting with "UNK"
        var placeholderId = GetText(driver, "Placeholder ID");
        Assert.That(placeholderId, Does.Match(@"^UNK"));
        // And the partial identity information is stored with "Unverified" status
        var identityStatus = GetText(driver, "Identity Status");
        Assert.That(identityStatus, Is.EqualTo("Unverified"));
        // And the patient record is flagged for "Identity Verification Required"
        var identityFlag = GetText(driver, "Identity Verification Flag");
        Assert.That(identityFlag, Is.EqualTo("Identity Verification Required"));
        // And a task is created for social services to assist with identification
        var socialServicesTask = GetText(driver, "Social Services Task");
        Assert.That(socialServicesTask, Does.Match(@"identification").IgnoreCase);
    }

    [Test, Order(3)]
    [Description("Register patient who becomes conscious during registration")]
    public void RegisterPatientWhoBecomesConsciousDuringRegistration()
    {
        // Given an ambulance arrives with a patient who initially cannot provide identification
        // And I have started the "Unknown Patient" registration process

        // When the patient becomes conscious and provides identification:
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Full Name"), ("Value", "Michael Johnson")),
            Row(("Field", "Date of Birth"), ("Value", "1980-12-15")),
            Row(("Field", "Phone Number"), ("Value", "555-876-5432")),
        });
        // And I verify the provided identification
        driver.FindElement(By.CssSelector("[data-testid=\"verify-identification-button\"]")).Click();

        // Then the system converts the temporary record to a verified patient record
        WaitForTestId(driver, "Medical Record Number");
        // And the placeholder ID is replaced with a permanent medical record number
        var medicalRecordNumber = GetText(driver, "Medical Record Number");
        Assert.That(medicalRecordNumber.StartsWith("UNK", StringComparison.Ordinal), Is.False);
        // And the "Identity Verification Required" flag is removed
        var identityFlagElements = driver.FindElements(Locator("Identity Verification Flag"));
        Assert.That(identityFlagElements.Count, Is.EqualTo(0));
        // And the patient demographic information is updated
        var patientName = GetText(driver, "Patient Name");
        Assert.That(patientName, Is.EqualTo("Michael Johnson"));
        // And a note is added documenting the identification process
        var identificationNote = GetText(driver, "Identification Note");
        Assert.That(identificationNote.Length > 0, Is.True);
    }

    [Test, Order(4)]
    [Description("Handle multiple unknown patients from mass casualty incident")]
    public void HandleMultipleUnknownPatientsFromMassCasualtyIncident()
    {
        // Given multiple ambulances arrive from a mass casualty incident
        // And none of the patients can provide identification

        // When I select "Unknown Patient - Mass Casualty" registration type
        FillField(driver, "Registration Type", "Unknown Patient - Mass Casualty");
        // And I enter the incident information:
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Incident Type"), ("Value", "Multi-vehicle accident")),
            Row(("Field", "Incident Location"), ("Value", "Interstate 70 Exit 45")),
            Row(("Field", "Total Patients"), ("Value", "4")),
        });
        // And I register each patient with EMS-provided information
        driver.FindElement(By.CssSelector("[data-testid=\"submit-registration-form\"]")).Click();

        // Then the system creates temporary records for all patients
        var temporaryRecords = driver.FindElements(By.CssSelector("[data-testid=\"temporary-patient-record\"]"));
        Assert.That(temporaryRecords.Count > 0, Is.True);
        // And each patient gets a sequential placeholder ID (UNK-001, UNK-002, etc.)
        var placeholderId = GetText(driver, "Placeholder ID");
        Assert.That(placeholderId, Does.Match(@"^UNK-\d{3}$"));
        // And all records are linked to the same incident number
        var incidentNumber = GetText(driver, "Incident Number");
        Assert.That(incidentNumber.Length > 0, Is.True);
        // And the mass casualty protocol is activated
        var massCasualtyProtocolStatus = GetText(driver, "Mass Casualty Protocol Status");
        Assert.That(massCasualtyProtocolStatus, Does.Match(@"activated").IgnoreCase);
        // And notifications are sent to administration and social services
        var notificationRecipients = GetText(driver, "Notification Recipients");
        Assert.That(notificationRecipients, Does.Match(@"administration").IgnoreCase);
    }

    [Test, Order(5)]
    [Description("Attempt to register unknown patient without EMS information")]
    public void AttemptToRegisterUnknownPatientWithoutEMSInformation()
    {
        // Given an ambulance arrives with a patient who cannot provide identification
        // And EMS has minimal information available

        // When I select "Unknown Patient" registration type
        FillField(driver, "Registration Type", "Unknown Patient");
        // And I attempt to submit with only basic information:
        //   | Field                 | Value                    |
        //   | Gender                | Unknown                  |
        //   | Estimated Age         | Unknown                  |
        //   | Chief Complaint       |                          |
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Gender"), ("Value", "Unknown")),
            Row(("Field", "Estimated Age"), ("Value", "Unknown")),
            Row(("Field", "Chief Complaint"), ("Value", "")),
        });
        driver.FindElement(By.CssSelector("[data-testid=\"submit-registration-form\"]")).Click();

        // Then the system displays a warning "Insufficient information for registration"
        var warningMessage = GetText(driver, "Warning Message");
        Assert.That(warningMessage, Is.EqualTo("Insufficient information for registration"));
        // And the system requires minimum data fields:
        //   | Required Field        | Requirement                       |
        //   | Estimated Age Range   | Must be provided                  |
        //   | Gender                | Must be Male, Female, or Unknown  |
        //   | Chief Complaint       | Must be provided                  |
        Assert.That(GetText(driver, "Estimated Age Range Error"), Is.EqualTo("Must be provided"));
        Assert.That(GetText(driver, "Gender Error"), Is.EqualTo("Must be Male, Female, or Unknown"));
        Assert.That(GetText(driver, "Chief Complaint Error"), Is.EqualTo("Must be provided"));
        // And the registration cannot be completed until minimum requirements are met
        var placeholderIds = driver.FindElements(Locator("Placeholder ID"));
        Assert.That(placeholderIds.Count, Is.EqualTo(0));
    }

    [Test, Order(6)]
    [Description("Identity verification process after patient stabilization")]
    public void IdentityVerificationProcessAfterPatientStabilization()
    {
        // Given a patient was registered as "Unknown Patient"
        // And the patient has now stabilized
        // And the patient can provide identification

        // When the nurse initiates the identity verification process
        driver.FindElement(By.CssSelector("[data-testid=\"initiate-identity-verification-button\"]")).Click();
        // And the patient provides valid identification:
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Full Name"), ("Value", "Robert Davis")),
            Row(("Field", "Date of Birth"), ("Value", "1975-08-20")),
            Row(("Field", "Social Security"), ("Value", "XXX-XX-1234 (last 4)")),
        });
        // And the identification is verified
        driver.FindElement(By.CssSelector("[data-testid=\"verify-identification-button\"]")).Click();

        // Then the system merges the temporary record with verified information
        WaitForTestId(driver, "Medical Record Number");
        var mergeStatus = GetText(driver, "Record Merge Status");
        Assert.That(mergeStatus, Does.Match(@"merged").IgnoreCase);
        // And the "Identity Verification Required" flag is cleared
        var identityFlagElements = driver.FindElements(Locator("Identity Verification Flag"));
        Assert.That(identityFlagElements.Count, Is.EqualTo(0));
        // And a permanent medical record number is assigned
        var medicalRecordNumber = GetText(driver, "Medical Record Number");
        Assert.That(medicalRecordNumber.Length > 0, Is.True);
        // And all clinical documentation is preserved under the new verified record
        var clinicalDocumentationStatus = GetText(driver, "Clinical Documentation Status");
        Assert.That(clinicalDocumentationStatus, Does.Match(@"preserved").IgnoreCase);
        // And billing information is updated with verified patient details
        var billingStatus = GetText(driver, "Billing Status");
        Assert.That(billingStatus, Does.Match(@"updated").IgnoreCase);
    }
}
