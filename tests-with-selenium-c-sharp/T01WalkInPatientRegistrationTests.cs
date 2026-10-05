// Selenium WebDriver + NUnit test for
// tests-with-given-when-then-features/01-walk-in-patient-registration.feature
// (equivalent to tests-with-selenium-javascript/01-walk-in-patient-registration.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see Support/Fields.cs) and the shared
// data-testid contract in Support/Login.cs (login-identity, login-submit,
// app-root).

namespace EmergencyCareSystem.SeleniumTests;

[TestFixture]
[NonParallelizable]
public class T01WalkInPatientRegistrationTests
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
        //   And I am logged in as a registration clerk
        VerifySystemIsOperational(driver);
        Login(driver, "a registration clerk");

        // The demo app is a single-page dashboard: after login, select this
        // feature's panel from the sidebar nav (data-testid="nav-<slug>").
        var registrationNavLink = WaitForTestId(driver, "Nav Walk In Patient Registration");
        registrationNavLink.Click();
        WaitForTestId(driver, "Patient Registration Form");
    }

    [Test, Order(1)]
    [Description("Successfully register a new walk-in patient")]
    public void SuccessfullyRegisterANewWalkInPatient()
    {
        // Given a new patient arrives at the ED without prior registration
        // And the patient provides valid identification
        // When I enter the patient's demographic information:
        FillFields(driver, new List<Dictionary<string, string>>
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
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Insurance Type"), ("Value", "Blue Cross")),
            Row(("Field", "Policy Number"), ("Value", "BC123456789")),
            Row(("Field", "Group Number"), ("Value", "GRP001")),
        });
        // And I submit the registration form
        driver.FindElement(By.CssSelector("[data-testid=\"submit-registration-form\"]")).Click();

        // Then the system creates a unique patient record
        WaitForTestId(driver, "Medical Record Number");
        // And the system assigns a medical record number
        var medicalRecordNumber = GetText(driver, "Medical Record Number");
        Assert.That(medicalRecordNumber.Length > 0, Is.True);
        // And the patient is queued for triage
        var triageQueueStatus = GetText(driver, "Triage Queue Status");
        Assert.That(triageQueueStatus, Does.Match(@"queued for triage").IgnoreCase);
        // And I see a confirmation message "Patient successfully registered"
        var confirmationMessage = GetText(driver, "Confirmation Message");
        Assert.That(confirmationMessage, Is.EqualTo("Patient successfully registered"));
        // And the medical record number is displayed
        var medicalRecordNumberElement = driver.FindElement(Locator("Medical Record Number"));
        Assert.That(medicalRecordNumberElement.Displayed, Is.True);
    }

    [Test, Order(2)]
    [Description("Register patient with missing insurance information")]
    public void RegisterPatientWithMissingInsuranceInformation()
    {
        // Given a new patient arrives at the ED without prior registration
        // And the patient does not have insurance information
        // When I enter the patient's demographic information:
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Given Name"), ("Value", "Jane")),
            Row(("Field", "Family Name"), ("Value", "Smith")),
            Row(("Field", "Date of Birth"), ("Value", "1990-03-22")),
            Row(("Field", "Phone Number"), ("Value", "555-987-6543")),
            Row(("Field", "Address"), ("Value", "456 Oak Ave")),
        });
        // And I select "Self-Pay" as the insurance type
        FillField(driver, "Insurance Type", "Self-Pay");
        // And I submit the registration form
        driver.FindElement(By.CssSelector("[data-testid=\"submit-registration-form\"]")).Click();

        // Then the system creates a unique patient record
        WaitForTestId(driver, "Medical Record Number");
        // And the system assigns a medical record number
        var medicalRecordNumber = GetText(driver, "Medical Record Number");
        Assert.That(medicalRecordNumber.Length > 0, Is.True);
        // And the patient is queued for triage
        var triageQueueStatus = GetText(driver, "Triage Queue Status");
        Assert.That(triageQueueStatus, Does.Match(@"queued for triage").IgnoreCase);
        // And the insurance status is marked as "Self-Pay"
        var insuranceStatus = GetText(driver, "Insurance Status");
        Assert.That(insuranceStatus, Is.EqualTo("Self-Pay"));
    }

    [Test, Order(3)]
    [Description("Handle duplicate patient registration attempt")]
    public void HandleDuplicatePatientRegistrationAttempt()
    {
        // Given a patient with the same name and date of birth already exists in the system
        // When I enter the patient's demographic information:
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Given Name"), ("Value", "John")),
            Row(("Field", "Family Name"), ("Value", "Doe")),
            Row(("Field", "Date of Birth"), ("Value", "1985-06-15")),
        });
        // And I submit the registration form
        driver.FindElement(By.CssSelector("[data-testid=\"submit-registration-form\"]")).Click();

        // Then the system displays a warning "Potential duplicate patient found"
        var warningMessage = GetText(driver, "Duplicate Patient Warning");
        Assert.That(warningMessage, Is.EqualTo("Potential duplicate patient found"));
        // And the system shows existing patient records for verification
        var existingRecords = driver.FindElements(By.CssSelector("[data-testid=\"existing-patient-record\"]"));
        Assert.That(existingRecords.Count > 0, Is.True);
        // And I can choose to link to existing record or create new record
        WaitForTestId(driver, "Link to Existing Record");
        WaitForTestId(driver, "Create New Record");
    }

    [Test, Order(4)]
    [Description("Registration with invalid demographic data")]
    public void RegistrationWithInvalidDemographicData()
    {
        // Given a new patient arrives at the ED without prior registration
        // When I enter incomplete demographic information:
        FillFields(driver, new List<Dictionary<string, string>>
        {
            Row(("Field", "Given Name"), ("Value", "John")),
            Row(("Field", "Family Name"), ("Value", "")),
            Row(("Field", "Date of Birth"), ("Value", "invalid-date")),
        });
        // And I submit the registration form
        driver.FindElement(By.CssSelector("[data-testid=\"submit-registration-form\"]")).Click();

        // Then the system displays validation errors:
        var familyNameError = GetText(driver, "Family Name Error");
        Assert.That(familyNameError, Is.EqualTo("Family name is required"));
        var dateOfBirthError = GetText(driver, "Date of Birth Error");
        Assert.That(dateOfBirthError, Is.EqualTo("Invalid date format"));
        // And the patient record is not created
        var medicalRecordNumbers = driver.FindElements(Locator("Medical Record Number"));
        Assert.That(medicalRecordNumbers.Count, Is.EqualTo(0));
        // And the form remains open for correction
        var registrationForm = driver.FindElement(By.CssSelector("[data-testid=\"patient-registration-form\"]"));
        Assert.That(registrationForm.Displayed, Is.True);
    }
}
