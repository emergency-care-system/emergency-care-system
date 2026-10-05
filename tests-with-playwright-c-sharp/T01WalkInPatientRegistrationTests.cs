// Playwright + NUnit test for
// tests-with-given-when-then-features/01-walk-in-patient-registration.feature
// (equivalent to tests-with-playwright-javascript/01-walk-in-patient-registration.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see Support/Fields.cs) and the shared
// data-testid contract in Support/Login.cs (login-identity, login-submit,
// app-root).

namespace EmergencyCareSystem.PlaywrightTests;

[TestFixture]
[NonParallelizable]
public class T01WalkInPatientRegistrationTests
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
        //   And I am logged in as a registration clerk
        await VerifySystemIsOperational(page);
        await Login(page, "a registration clerk");

        // The demo app is a single-page dashboard: after login, select this
        // feature's panel from the sidebar nav (data-testid="nav-<slug>").
        var registrationNavLink = await WaitForTestId(page, "Nav Walk In Patient Registration");
        await registrationNavLink.ClickAsync();
        await WaitForTestId(page, "Patient Registration Form");
    }

    [Test, Order(1)]
    [Description("Successfully register a new walk-in patient")]
    public async Task SuccessfullyRegisterANewWalkInPatient()
    {
        // Given a new patient arrives at the ED without prior registration
        // And the patient provides valid identification
        // When I enter the patient's demographic information:
        await FillFields(page, new List<Dictionary<string, string>>
        {
            Row(("Field", "Given Name"), ("Value", "John")),
            Row(("Field", "Family Name"), ("Value", "Doe")),
            Row(("Field", "Date of Birth"), ("Value", "1985-06-15")),
            Row(("Field", "Phone Number"), ("Value", "555-123-4567")),
            Row(("Field", "Address"), ("Value", "123 Main St")),
            Row(("Field", "City"), ("Value", "Springfield")),
            Row(("Field", "State"), ("Value", "IL")),
            Row(("Field", "Zip Code"), ("Value", "62701")),
        });
        // And I enter the patient's insurance details:
        await FillFields(page, new List<Dictionary<string, string>>
        {
            Row(("Field", "Insurance Type"), ("Value", "Blue Cross")),
            Row(("Field", "Policy Number"), ("Value", "BC123456789")),
            Row(("Field", "Group Number"), ("Value", "GRP001")),
        });
        // And I submit the registration form
        await page.GetByTestId("submit-registration-form").First.ClickAsync();

        // Then the system creates a unique patient record
        await WaitForTestId(page, "Medical Record Number");
        // And the system assigns a medical record number
        var medicalRecordNumber = await GetText(page, "Medical Record Number");
        Assert.That(medicalRecordNumber.Length > 0, Is.True);
        // And the patient is queued for triage
        var triageQueueStatus = await GetText(page, "Triage Queue Status");
        Assert.That(triageQueueStatus, Does.Match(@"queued for triage").IgnoreCase);
        // And I see a confirmation message "Patient successfully registered"
        var confirmationMessage = await GetText(page, "Confirmation Message");
        Assert.That(confirmationMessage, Is.EqualTo("Patient successfully registered"));
        // And the medical record number is displayed
        var medicalRecordNumberElement = Locator(page, "Medical Record Number").First;
        Assert.That((await medicalRecordNumberElement.IsVisibleAsync()), Is.True);
    }

    [Test, Order(2)]
    [Description("Register patient with missing insurance information")]
    public async Task RegisterPatientWithMissingInsuranceInformation()
    {
        // Given a new patient arrives at the ED without prior registration
        // And the patient does not have insurance information
        // When I enter the patient's demographic information:
        await FillFields(page, new List<Dictionary<string, string>>
        {
            Row(("Field", "Given Name"), ("Value", "Jane")),
            Row(("Field", "Family Name"), ("Value", "Smith")),
            Row(("Field", "Date of Birth"), ("Value", "1990-03-22")),
            Row(("Field", "Phone Number"), ("Value", "555-987-6543")),
            Row(("Field", "Address"), ("Value", "456 Oak Ave")),
        });
        // And I select "Self-Pay" as the insurance type
        await FillField(page, "Insurance Type", "Self-Pay");
        // And I submit the registration form
        await page.GetByTestId("submit-registration-form").First.ClickAsync();

        // Then the system creates a unique patient record
        await WaitForTestId(page, "Medical Record Number");
        // And the system assigns a medical record number
        var medicalRecordNumber = await GetText(page, "Medical Record Number");
        Assert.That(medicalRecordNumber.Length > 0, Is.True);
        // And the patient is queued for triage
        var triageQueueStatus = await GetText(page, "Triage Queue Status");
        Assert.That(triageQueueStatus, Does.Match(@"queued for triage").IgnoreCase);
        // And the insurance status is marked as "Self-Pay"
        var insuranceStatus = await GetText(page, "Insurance Status");
        Assert.That(insuranceStatus, Is.EqualTo("Self-Pay"));
    }

    [Test, Order(3)]
    [Description("Handle duplicate patient registration attempt")]
    public async Task HandleDuplicatePatientRegistrationAttempt()
    {
        // Given a patient with the same name and date of birth already exists in the system
        // When I enter the patient's demographic information:
        await FillFields(page, new List<Dictionary<string, string>>
        {
            Row(("Field", "Given Name"), ("Value", "John")),
            Row(("Field", "Family Name"), ("Value", "Doe")),
            Row(("Field", "Date of Birth"), ("Value", "1985-06-15")),
        });
        // And I submit the registration form
        await page.GetByTestId("submit-registration-form").First.ClickAsync();

        // Then the system displays a warning "Potential duplicate patient found"
        var warningMessage = await GetText(page, "Duplicate Patient Warning");
        Assert.That(warningMessage, Is.EqualTo("Potential duplicate patient found"));
        // And the system shows existing patient records for verification
        var existingRecords = page.GetByTestId("existing-patient-record");
        Assert.That(await existingRecords.CountAsync() > 0, Is.True);
        // And I can choose to link to existing record or create new record
        await WaitForTestId(page, "Link to Existing Record");
        await WaitForTestId(page, "Create New Record");
    }

    [Test, Order(4)]
    [Description("Registration with invalid demographic data")]
    public async Task RegistrationWithInvalidDemographicData()
    {
        // Given a new patient arrives at the ED without prior registration
        // When I enter incomplete demographic information:
        await FillFields(page, new List<Dictionary<string, string>>
        {
            Row(("Field", "Given Name"), ("Value", "John")),
            Row(("Field", "Family Name"), ("Value", "")),
            Row(("Field", "Date of Birth"), ("Value", "invalid-date")),
        });
        // And I submit the registration form
        await page.GetByTestId("submit-registration-form").First.ClickAsync();

        // Then the system displays validation errors:
        var familyNameError = await GetText(page, "Family Name Error");
        Assert.That(familyNameError, Is.EqualTo("Family name is required"));
        var dateOfBirthError = await GetText(page, "Date of Birth Error");
        Assert.That(dateOfBirthError, Is.EqualTo("Invalid date format"));
        // And the patient record is not created
        var medicalRecordNumbers = Locator(page, "Medical Record Number");
        Assert.That(await medicalRecordNumbers.CountAsync(), Is.EqualTo(0));
        // And the form remains open for correction
        var registrationForm = page.GetByTestId("patient-registration-form").First;
        Assert.That((await registrationForm.IsVisibleAsync()), Is.True);
    }
}
