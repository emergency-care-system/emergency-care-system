// Playwright + NUnit test for
// tests-with-given-when-then-features/02-ambulance-arrival-registration.feature
// (equivalent to tests-with-playwright-javascript/02-ambulance-arrival-registration.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see Support/Fields.cs) and the shared
// data-testid contract in Support/Login.cs (login-identity, login-submit,
// app-root).

namespace EmergencyCareSystem.PlaywrightTests;

[TestFixture]
[NonParallelizable]
public class T02AmbulanceArrivalRegistrationTests
{
    private Session session = null!;
    private IPage page => session.Page;

    [OneTimeSetUp]
    public async Task SetUpClass()
    {
        session = await Session.StartAsync();
    }

    [OneTimeTearDown]
    public async Task TearDownClass()
    {
        await session.DisposeAsync();
    }

    [SetUp]
    public async Task SetUp()
    {
        // Background:
        //   Given the emergency care system is operational
        //   And I am logged in as registration staff
        //   And the "unknown patient" registration module is available
        await VerifySystemIsOperational(page);
        await Login(page, "registration staff");
        // The "unknown patient" registration module availability is assumed
        // pre-seeded test data / environment configuration.

        var featureNavLink = await WaitForTestId(page, "Nav Ambulance Arrival Registration");
        await featureNavLink.ClickAsync();
        await WaitForTestId(page, "Ambulance Arrival Registration Panel");
    }

    [Test, Order(1)]
    [Description("Register unconscious patient brought by ambulance")]
    public async Task RegisterUnconsciousPatientBroughtByAmbulance()
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
        await FillField(page, "Registration Type", "Unknown Patient");
        // And I enter the available information from EMS
        await FillFields(page, new List<Dictionary<string, string>>
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
        await page.GetByTestId("submit-registration-form").First.ClickAsync();

        // Then the system creates a temporary patient record
        await WaitForTestId(page, "Temporary Patient Record");
        // And the system assigns a placeholder ID starting with "UNK"
        var placeholderId = await GetText(page, "Placeholder ID");
        Assert.That(placeholderId, Does.Match(@"^UNK"));
        // And the patient record is flagged for "Identity Verification Required"
        var identityFlag = await GetText(page, "Identity Verification Flag");
        Assert.That(identityFlag, Is.EqualTo("Identity Verification Required"));
        // And the patient is immediately queued for triage
        var triageQueueStatus = await GetText(page, "Triage Queue Status");
        Assert.That(triageQueueStatus, Does.Match(@"queued for triage").IgnoreCase);
        // And a notification is sent to the charge nurse about the unknown patient
        var chargeNurseNotification = await GetText(page, "Charge Nurse Notification");
        Assert.That(chargeNurseNotification, Does.Match(@"unknown patient").IgnoreCase);
        // And the record shows status as "Temporary - Pending Identification"
        var recordStatus = await GetText(page, "Record Status");
        Assert.That(recordStatus, Is.EqualTo("Temporary - Pending Identification"));
    }

    [Test, Order(2)]
    [Description("Register patient with partial identification from personal effects")]
    public async Task RegisterPatientWithPartialIdentificationFromPersonalEffects()
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
        await FillField(page, "Registration Type", "Unknown Patient");
        // And I enter the EMS information including partial identity details
        await FillFields(page, new List<Dictionary<string, string>>
        {
            Row(("Field", "Estimated Age"), ("Value", "30-35 years")),
            Row(("Field", "Gender"), ("Value", "Female")),
            Row(("Field", "Chief Complaint"), ("Value", "Drug overdose")),
            Row(("Field", "Found Name"), ("Value", "Sarah (from credit card)")),
            Row(("Field", "Partial Phone"), ("Value", "555-1234 (last 4 digits)")),
        });
        // And I mark the identity fields as "Unverified"
        await FillField(page, "Identity Status", "Unverified");
        // And I submit the registration
        await page.GetByTestId("submit-registration-form").First.ClickAsync();

        // Then the system creates a temporary patient record
        await WaitForTestId(page, "Temporary Patient Record");
        // And the system assigns a placeholder ID starting with "UNK"
        var placeholderId = await GetText(page, "Placeholder ID");
        Assert.That(placeholderId, Does.Match(@"^UNK"));
        // And the partial identity information is stored with "Unverified" status
        var identityStatus = await GetText(page, "Identity Status");
        Assert.That(identityStatus, Is.EqualTo("Unverified"));
        // And the patient record is flagged for "Identity Verification Required"
        var identityFlag = await GetText(page, "Identity Verification Flag");
        Assert.That(identityFlag, Is.EqualTo("Identity Verification Required"));
        // And a task is created for social services to assist with identification
        var socialServicesTask = await GetText(page, "Social Services Task");
        Assert.That(socialServicesTask, Does.Match(@"identification").IgnoreCase);
    }

    [Test, Order(3)]
    [Description("Register patient who becomes conscious during registration")]
    public async Task RegisterPatientWhoBecomesConsciousDuringRegistration()
    {
        // Given an ambulance arrives with a patient who initially cannot provide identification
        // And I have started the "Unknown Patient" registration process

        // When the patient becomes conscious and provides identification:
        await FillFields(page, new List<Dictionary<string, string>>
        {
            Row(("Field", "Full Name"), ("Value", "Michael Johnson")),
            Row(("Field", "Date of Birth"), ("Value", "1980-12-15")),
            Row(("Field", "Phone Number"), ("Value", "555-876-5432")),
        });
        // And I verify the provided identification
        await page.GetByTestId("verify-identification-button").First.ClickAsync();

        // Then the system converts the temporary record to a verified patient record
        await WaitForTestId(page, "Medical Record Number");
        // And the placeholder ID is replaced with a permanent medical record number
        var medicalRecordNumber = await GetText(page, "Medical Record Number");
        Assert.That(medicalRecordNumber.StartsWith("UNK", StringComparison.Ordinal), Is.False);
        // And the "Identity Verification Required" flag is removed
        var identityFlagElements = Locator(page, "Identity Verification Flag");
        Assert.That(await identityFlagElements.CountAsync(), Is.EqualTo(0));
        // And the patient demographic information is updated
        var patientName = await GetText(page, "Patient Name");
        Assert.That(patientName, Is.EqualTo("Michael Johnson"));
        // And a note is added documenting the identification process
        var identificationNote = await GetText(page, "Identification Note");
        Assert.That(identificationNote.Length > 0, Is.True);
    }

    [Test, Order(4)]
    [Description("Handle multiple unknown patients from mass casualty incident")]
    public async Task HandleMultipleUnknownPatientsFromMassCasualtyIncident()
    {
        // Given multiple ambulances arrive from a mass casualty incident
        // And none of the patients can provide identification

        // When I select "Unknown Patient - Mass Casualty" registration type
        await FillField(page, "Registration Type", "Unknown Patient - Mass Casualty");
        // And I enter the incident information:
        await FillFields(page, new List<Dictionary<string, string>>
        {
            Row(("Field", "Incident Type"), ("Value", "Multi-vehicle accident")),
            Row(("Field", "Incident Location"), ("Value", "Interstate 70 Exit 45")),
            Row(("Field", "Total Patients"), ("Value", "4")),
        });
        // And I register each patient with EMS-provided information
        await page.GetByTestId("submit-registration-form").First.ClickAsync();

        // Then the system creates temporary records for all patients
        var temporaryRecords = page.GetByTestId("temporary-patient-record");
        Assert.That(await temporaryRecords.CountAsync() > 0, Is.True);
        // And each patient gets a sequential placeholder ID (UNK-001, UNK-002, etc.)
        var placeholderId = await GetText(page, "Placeholder ID");
        Assert.That(placeholderId, Does.Match(@"^UNK-\d{3}$"));
        // And all records are linked to the same incident number
        var incidentNumber = await GetText(page, "Incident Number");
        Assert.That(incidentNumber.Length > 0, Is.True);
        // And the mass casualty protocol is activated
        var massCasualtyProtocolStatus = await GetText(page, "Mass Casualty Protocol Status");
        Assert.That(massCasualtyProtocolStatus, Does.Match(@"activated").IgnoreCase);
        // And notifications are sent to administration and social services
        var notificationRecipients = await GetText(page, "Notification Recipients");
        Assert.That(notificationRecipients, Does.Match(@"administration").IgnoreCase);
    }

    [Test, Order(5)]
    [Description("Attempt to register unknown patient without EMS information")]
    public async Task AttemptToRegisterUnknownPatientWithoutEMSInformation()
    {
        // Given an ambulance arrives with a patient who cannot provide identification
        // And EMS has minimal information available

        // When I select "Unknown Patient" registration type
        await FillField(page, "Registration Type", "Unknown Patient");
        // And I attempt to submit with only basic information:
        //   | Field                 | Value                    |
        //   | Gender                | Unknown                  |
        //   | Estimated Age         | Unknown                  |
        //   | Chief Complaint       |                          |
        await FillFields(page, new List<Dictionary<string, string>>
        {
            Row(("Field", "Gender"), ("Value", "Unknown")),
            Row(("Field", "Estimated Age"), ("Value", "Unknown")),
            Row(("Field", "Chief Complaint"), ("Value", "")),
        });
        await page.GetByTestId("submit-registration-form").First.ClickAsync();

        // Then the system displays a warning "Insufficient information for registration"
        var warningMessage = await GetText(page, "Warning Message");
        Assert.That(warningMessage, Is.EqualTo("Insufficient information for registration"));
        // And the system requires minimum data fields:
        //   | Required Field        | Requirement                       |
        //   | Estimated Age Range   | Must be provided                  |
        //   | Gender                | Must be Male, Female, or Unknown  |
        //   | Chief Complaint       | Must be provided                  |
        Assert.That((await GetText(page, "Estimated Age Range Error")), Is.EqualTo("Must be provided"));
        Assert.That((await GetText(page, "Gender Error")), Is.EqualTo("Must be Male, Female, or Unknown"));
        Assert.That((await GetText(page, "Chief Complaint Error")), Is.EqualTo("Must be provided"));
        // And the registration cannot be completed until minimum requirements are met
        var placeholderIds = Locator(page, "Placeholder ID");
        Assert.That(await placeholderIds.CountAsync(), Is.EqualTo(0));
    }

    [Test, Order(6)]
    [Description("Identity verification process after patient stabilization")]
    public async Task IdentityVerificationProcessAfterPatientStabilization()
    {
        // Given a patient was registered as "Unknown Patient"
        // And the patient has now stabilized
        // And the patient can provide identification

        // When the nurse initiates the identity verification process
        await page.GetByTestId("initiate-identity-verification-button").First.ClickAsync();
        // And the patient provides valid identification:
        await FillFields(page, new List<Dictionary<string, string>>
        {
            Row(("Field", "Full Name"), ("Value", "Robert Davis")),
            Row(("Field", "Date of Birth"), ("Value", "1975-08-20")),
            Row(("Field", "Social Security"), ("Value", "XXX-XX-1234 (last 4)")),
        });
        // And the identification is verified
        await page.GetByTestId("verify-identification-button").First.ClickAsync();

        // Then the system merges the temporary record with verified information
        await WaitForTestId(page, "Medical Record Number");
        var mergeStatus = await GetText(page, "Record Merge Status");
        Assert.That(mergeStatus, Does.Match(@"merged").IgnoreCase);
        // And the "Identity Verification Required" flag is cleared
        var identityFlagElements = Locator(page, "Identity Verification Flag");
        Assert.That(await identityFlagElements.CountAsync(), Is.EqualTo(0));
        // And a permanent medical record number is assigned
        var medicalRecordNumber = await GetText(page, "Medical Record Number");
        Assert.That(medicalRecordNumber.Length > 0, Is.True);
        // And all clinical documentation is preserved under the new verified record
        var clinicalDocumentationStatus = await GetText(page, "Clinical Documentation Status");
        Assert.That(clinicalDocumentationStatus, Does.Match(@"preserved").IgnoreCase);
        // And billing information is updated with verified patient details
        var billingStatus = await GetText(page, "Billing Status");
        Assert.That(billingStatus, Does.Match(@"updated").IgnoreCase);
    }
}
