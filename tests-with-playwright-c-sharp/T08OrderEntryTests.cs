// Playwright + NUnit test for
// tests-with-given-when-then-features/08-order-entry.feature
// (equivalent to tests-with-playwright-javascript/08-order-entry.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see Support/Fields.cs) and the shared
// data-testid contract in Support/Login.cs (login-identity, login-submit,
// app-root).

namespace EmergencyCareSystem.PlaywrightTests;

[TestFixture]
[NonParallelizable]
public class T08OrderEntryTests
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
        //   And I am logged in as "Dr. Smith"
        //   And the electronic order entry module is active
        //   And departmental interfaces (lab, radiology, pharmacy) are connected
        await VerifySystemIsOperational(page);
        await Login(page, "Dr. Smith");
        // The electronic order entry module being active and the departmental
        // interfaces being connected are assumed pre-seeded test data /
        // environment configuration.

        var orderEntryNavLink = await WaitForTestId(page, "Nav Order Entry");
        await orderEntryNavLink.ClickAsync();
        await WaitForTestId(page, "Order Entry Panel");
    }

    [Test, Order(1)]
    [Description("Enter standard orders for chest pain workup")]
    public async Task EnterStandardOrdersForChestPainWorkup()
    {
        // Given I have examined a patient "John Martinez" in bed "ED-5"
        // And the patient presents with "acute chest pain"
        // And the patient's allergies and contraindications have been reviewed
        // (assumed pre-seeded test data)

        // When I open the order entry module for the patient
        await page.GetByTestId("open-order-entry-module-button").First.ClickAsync();

        // And I enter the following laboratory orders:
        //   | Order Type     | Test Name        | Priority | Special Instructions |
        //   | Laboratory     | CBC with diff    | Routine  | None                |
        //   | Laboratory     | Troponin I       | STAT     | Serial in 6 hours   |
        //   | Laboratory     | Basic Metabolic  | Routine  | None                |
        await page.GetByTestId("enter-order-button").First.ClickAsync();

        // And I enter the following radiology order:
        //   | Order Type     | Study Name       | Priority | Special Instructions |
        //   | Radiology      | Chest X-ray PA/LAT| STAT    | R/O pneumonia       |
        await page.GetByTestId("enter-order-button").First.ClickAsync();

        // And I submit all orders with my electronic signature
        await page.GetByTestId("submit-orders-button").First.ClickAsync();

        // Then the system sends electronic orders to the laboratory with details:
        //   | Order ID | Test Name     | Patient Info        | Priority | Timestamp |
        //   | LAB-001  | CBC with diff | John Martinez ED-5  | Routine  | Current   |
        //   | LAB-002  | Troponin I    | John Martinez ED-5  | STAT     | Current   |
        //   | LAB-003  | Basic Metabolic| John Martinez ED-5 | Routine  | Current   |
        var labOrdersSent = page.GetByTestId("laboratory-order-sent");
        Assert.That(await labOrdersSent.CountAsync(), Is.EqualTo(3));
        var firstLabOrderText = await TextOf(labOrdersSent.Nth(0));
        Assert.That(firstLabOrderText, Does.Match(@"LAB-001"));

        // And the system sends electronic orders to radiology with details:
        //   | Order ID | Study Name    | Patient Info        | Priority | Timestamp |
        //   | RAD-001  | CXR PA/LAT    | John Martinez ED-5  | STAT     | Current   |
        var radiologyOrdersSent = page.GetByTestId("radiology-order-sent");
        Assert.That(await radiologyOrdersSent.CountAsync(), Is.EqualTo(1));

        // And specimen labels are automatically generated:
        //   | Label Type     | Content                                    |
        //   | Blood Draw     | John Martinez, DOB: 1975-08-15, ED-5     |
        //   | Test Codes     | CBC, Troponin, BMP                        |
        //   | Collection Time| STAT - Collect immediately                |
        //   | Barcode        | Patient and order identifiers             |
        var specimenLabels = page.GetByTestId("specimen-label");
        Assert.That(await specimenLabels.CountAsync(), Is.EqualTo(4));

        // And nursing tasks are added to the workflow:
        //   | Task Type           | Description                    | Priority | Due Time    |
        //   | Blood Collection    | Draw CBC, Troponin, BMP       | STAT     | Immediate   |
        //   | Patient Transport   | Transport to X-ray            | STAT     | After labs  |
        //   | Monitor Results     | Watch for critical values     | High     | Ongoing     |
        var nursingTasks = page.GetByTestId("nursing-task");
        Assert.That(await nursingTasks.CountAsync(), Is.EqualTo(3));
    }

    [Test, Order(2)]
    [Description("Enter orders with drug allergy checking")]
    public async Task EnterOrdersWithDrugAllergyChecking()
    {
        // Given I have examined a patient "Sarah Johnson" in bed "ED-8"
        // And the patient has documented allergies:
        //   | Allergy    | Reaction Type    | Severity |
        //   | Penicillin | Rash, hives      | Moderate |
        //   | Morphine   | Respiratory depression | Severe |
        // (assumed pre-seeded test data)

        // When I attempt to enter a medication order:
        //   | Order Type | Medication  | Dose     | Route | Frequency |
        //   | Medication | Amoxicillin | 500mg    | PO    | TID       |
        await page.GetByTestId("enter-order-button").First.ClickAsync();

        // And I submit the order
        await page.GetByTestId("submit-orders-button").First.ClickAsync();

        // Then the system displays an allergy alert:
        await WaitForTestId(page, "Drug Allergy");
        Assert.That((await GetText(page, "Drug Allergy")), Is.EqualTo("WARNING: Patient allergic to Penicillin"));
        Assert.That((await GetText(page, "Severity")), Is.EqualTo("Moderate - Rash, hives"));
        Assert.That((await GetText(page, "Cross-reaction")), Is.EqualTo("Amoxicillin contains penicillin"));
        Assert.That((await GetText(page, "Recommendation")), Is.EqualTo("Consider alternative antibiotic"));

        // And the order is held pending confirmation
        var orderStatus = await GetText(page, "Order Status");
        Assert.That(orderStatus, Does.Match(@"pending confirmation").IgnoreCase);

        // And I must either:
        //   | Action Option      | Description                                |
        //   | Override with reason| Document clinical justification          |
        //   | Cancel order       | Remove the problematic medication         |
        //   | Select alternative | Choose non-penicillin antibiotic          |
        var allergyActionOptions = page.GetByTestId("allergy-action-option");
        Assert.That(await allergyActionOptions.CountAsync(), Is.EqualTo(3));

        // And the allergy alert is logged in the patient record
        var allergyAlertLogEntry = await GetText(page, "Allergy Alert Log Entry");
        Assert.That(allergyAlertLogEntry.Length > 0, Is.True);
    }

    [Test, Order(3)]
    [Description("Enter STAT orders during emergency situation")]
    public async Task EnterSTATOrdersDuringEmergencySituation()
    {
        // Given I have examined a patient "Emergency Patient" in bed "ED-TRAUMA-1"
        // And the patient is in critical condition with "severe trauma"
        // (assumed pre-seeded test data)

        // When I enter emergency orders:
        //   | Order Type     | Description           | Priority | Special Instructions    |
        //   | Laboratory     | Type and Crossmatch   | STAT     | 6 units PRBC on hold   |
        //   | Laboratory     | PT/INR, PTT          | STAT     | Pre-surgery labs       |
        //   | Radiology      | CT Head without contrast| STAT   | Rule out intracranial bleeding |
        //   | Radiology      | CT Chest/Abd/Pelvis  | STAT     | Trauma protocol        |
        //   | Medication     | Normal Saline        | STAT     | 1L wide open IV        |
        await page.GetByTestId("enter-order-button").First.ClickAsync();

        // And I mark all orders as "Emergency - Life threatening"
        await FillField(page, "Order Marking", "Emergency - Life threatening");

        // And I submit the orders
        await page.GetByTestId("submit-orders-button").First.ClickAsync();

        // Then all orders are immediately transmitted with highest priority
        var orderTransmissionStatus = await GetText(page, "Order Transmission Status");
        Assert.That(orderTransmissionStatus, Does.Match(@"highest priority").IgnoreCase);

        // And the laboratory receives orders marked "CRITICAL - TRAUMA"
        var laboratoryOrderMarking = await GetText(page, "Laboratory Order Marking");
        Assert.That(laboratoryOrderMarking, Is.EqualTo("CRITICAL - TRAUMA"));

        // And blood bank is notified to prepare emergency release protocol
        var bloodBankNotification = await GetText(page, "Blood Bank Notification");
        Assert.That(bloodBankNotification, Does.Match(@"emergency release protocol").IgnoreCase);

        // And radiology is alerted for trauma CT protocol
        var radiologyAlert = await GetText(page, "Radiology Alert");
        Assert.That(radiologyAlert, Does.Match(@"trauma CT protocol").IgnoreCase);

        // And nursing receives immediate action items:
        //   | Task               | Action Required           | Time Limit |
        //   | Blood Draw         | Collect trauma labs       | 5 minutes  |
        //   | IV Access          | Large bore IV x2          | Immediate  |
        //   | Patient Prep       | Prepare for CT transport  | 10 minutes |
        var nursingActionItems = page.GetByTestId("nursing-action-item");
        Assert.That(await nursingActionItems.CountAsync(), Is.EqualTo(3));

        // And all departments receive automatic status updates
        var departmentStatusUpdate = await GetText(page, "Department Status Update");
        Assert.That(departmentStatusUpdate.Length > 0, Is.True);
    }

    [Test, Order(4)]
    [Description("Enter pediatric orders with weight-based dosing")]
    public async Task EnterPediatricOrdersWithWeightBasedDosing()
    {
        // Given I have examined a pediatric patient "Tommy Chen" (age 5, weight 18kg) in bed "ED-PEDS-1"
        // And the patient presents with "febrile seizure"
        // (assumed pre-seeded test data)

        // When I enter pediatric medication orders:
        //   | Order Type | Medication | Dose Calculation        | Route | Frequency |
        //   | Medication | Acetaminophen| 15mg/kg (270mg)      | PO    | Q6H PRN   |
        //   | Medication | Lorazepam  | 0.1mg/kg (1.8mg)      | IV    | Once      |
        await page.GetByTestId("enter-order-button").First.ClickAsync();

        // And I enter diagnostic orders:
        //   | Order Type | Test/Study    | Pediatric Protocol    | Priority |
        //   | Laboratory | CBC with diff | Pediatric collection  | STAT     |
        //   | Laboratory | Blood glucose | Fingerstick acceptable| STAT     |
        await page.GetByTestId("enter-order-button").First.ClickAsync();

        // And I submit the pediatric orders
        await page.GetByTestId("submit-orders-button").First.ClickAsync();

        // Then the system validates weight-based dosing calculations
        var dosingValidationStatus = await GetText(page, "Dosing Validation Status");
        Assert.That(dosingValidationStatus, Does.Match(@"validated").IgnoreCase);

        // And pediatric-specific protocols are applied:
        //   | Protocol Type      | Details                                |
        //   | Collection Volume  | Minimum blood volume for pediatric labs|
        //   | Dosing Alerts      | Maximum safe dose verified             |
        //   | Administration     | Child-friendly instructions           |
        var pediatricProtocols = page.GetByTestId("pediatric-protocol");
        Assert.That(await pediatricProtocols.CountAsync(), Is.EqualTo(3));

        // And nursing receives pediatric-specific tasks:
        //   | Task Type          | Pediatric Instructions                 |
        //   | Medication Admin   | Use pediatric dosing chart            |
        //   | Blood Collection   | Minimize collection volume            |
        //   | Comfort Measures   | Parent/caregiver involvement          |
        var pediatricNursingTasks = page.GetByTestId("pediatric-nursing-task");
        Assert.That(await pediatricNursingTasks.CountAsync(), Is.EqualTo(3));

        // And pharmacy receives weight-verified dosing information
        var pharmacyDosingNotification = await GetText(page, "Pharmacy Dosing Notification");
        Assert.That(pharmacyDosingNotification.Length > 0, Is.True);
    }

    [Test, Order(5)]
    [Description("Handle order modifications and cancellations")]
    public async Task HandleOrderModificationsAndCancellations()
    {
        // Given I previously entered orders for patient "Maria Rodriguez" in bed "ED-12"
        // And the existing orders include:
        //   | Order ID | Order Type | Description    | Status      | Entered Time |
        //   | ORD-101  | Laboratory | CBC           | In Progress | 10:30        |
        //   | ORD-102  | Radiology  | Chest X-ray   | Pending     | 10:30        |
        //   | ORD-103  | Medication | Morphine 2mg  | Pending     | 10:30        |
        // (assumed pre-seeded test data)

        // When I need to modify the orders based on new clinical information
        // (no direct UI action for this narrative step)

        // And I cancel order "ORD-103" with reason "Patient reports morphine allergy"
        await FillFields(page, new List<Dictionary<string, string>>
        {
            Row(("Field", "Order ID"), ("Value", "ORD-103")),
            Row(("Field", "Cancellation Reason"), ("Value", "Patient reports morphine allergy")),
        });
        await page.GetByTestId("cancel-order-button").First.ClickAsync();

        // And I modify order "ORD-102" to add "portable" due to patient instability
        await FillFields(page, new List<Dictionary<string, string>>
        {
            Row(("Field", "Order ID"), ("Value", "ORD-102")),
            Row(("Field", "Modification"), ("Value", "Add portable")),
        });
        await page.GetByTestId("modify-order-button").First.ClickAsync();

        // And I add a new order for "Fentanyl 50mcg IV push"
        await FillField(page, "New Order Description", "Fentanyl 50mcg IV push");
        await page.GetByTestId("add-order-button").First.ClickAsync();

        // Then the system processes the order changes:
        //   | Action Type | Order ID | New Status    | Reason/Details              |
        //   | Cancelled   | ORD-103  | Cancelled     | Morphine allergy discovered |
        //   | Modified    | ORD-102  | Updated       | Changed to portable CXR     |
        //   | New Order   | ORD-104  | Pending       | Fentanyl 50mcg IV push     |
        var orderChanges = page.GetByTestId("order-change-entry");
        Assert.That(await orderChanges.CountAsync(), Is.EqualTo(3));
        var firstOrderChangeText = await TextOf(orderChanges.Nth(0));
        Assert.That(firstOrderChangeText, Does.Match(@"ORD-103"));

        // And notifications are sent to affected departments:
        //   | Department | Notification                               |
        //   | Pharmacy   | Morphine order cancelled - allergy        |
        //   | Radiology  | CXR modified to portable study            |
        //   | Nursing    | New pain medication order available       |
        var departmentNotifications = page.GetByTestId("department-notification");
        Assert.That(await departmentNotifications.CountAsync(), Is.EqualTo(3));

        // And an audit trail is maintained for all order changes
        var auditTrail = page.GetByTestId("order-change-audit-trail").First;
        Assert.That((await auditTrail.IsVisibleAsync()), Is.True);
    }

    [Test, Order(6)]
    [Description("Enter orders with insurance authorization requirements")]
    public async Task EnterOrdersWithInsuranceAuthorizationRequirements()
    {
        // Given I have examined a patient "Robert Davis" in bed "ED-6"
        // And the patient has insurance requiring prior authorization for certain studies
        // (assumed pre-seeded test data)

        // When I enter an order for:
        //   | Order Type | Study Name | Estimated Cost | Insurance Notes        |
        //   | Radiology  | CT Abdomen | $1,200        | Requires pre-auth      |
        await page.GetByTestId("enter-order-button").First.ClickAsync();

        // And I submit the order
        await page.GetByTestId("submit-orders-button").First.ClickAsync();

        // Then the system checks insurance requirements:
        Assert.That((await GetText(page, "Coverage Verification")), Is.EqualTo("CT covered with prior authorization"));
        Assert.That((await GetText(page, "Authorization Status")), Is.EqualTo("Prior auth required"));
        Assert.That((await GetText(page, "Alternative Options")), Is.EqualTo("Ultrasound covered without pre-auth"));

        // And I am presented with options:
        //   | Option             | Description                            |
        //   | Submit for auth    | Send for insurance approval (delay)    |
        //   | Order alternative  | Consider ultrasound instead           |
        //   | Emergency override | Document medical necessity            |
        var insuranceOptions = page.GetByTestId("insurance-option");
        Assert.That(await insuranceOptions.CountAsync(), Is.EqualTo(3));

        // And the order status is marked "Pending Authorization"
        var orderStatus = await GetText(page, "Order Status");
        Assert.That(orderStatus, Is.EqualTo("Pending Authorization"));

        // And the patient financial counselor is notified
        var financialCounselorNotification = await GetText(page, "Financial Counselor Notification");
        Assert.That(financialCounselorNotification.Length > 0, Is.True);
    }

    [Test, Order(7)]
    [Description("Handle order entry during system integration failures")]
    public async Task HandleOrderEntryDuringSystemIntegrationFailures()
    {
        // Given I am entering orders for patient "Lisa Wong" in bed "ED-14"
        // And the laboratory information system is temporarily offline
        // (assumed pre-seeded test data)

        // When I enter laboratory orders:
        //   | Order Type | Test Name     | Priority |
        //   | Laboratory | Troponin      | STAT     |
        //   | Laboratory | CBC          | Routine  |
        await page.GetByTestId("enter-order-button").First.ClickAsync();

        // And I submit the orders
        await page.GetByTestId("submit-orders-button").First.ClickAsync();

        // Then the system displays a warning: "Lab system offline - orders will be queued"
        var warningMessage = await GetText(page, "Warning Message");
        Assert.That(warningMessage, Is.EqualTo("Lab system offline - orders will be queued"));

        // And the orders are stored locally with status "Queued for transmission"
        var orderStatus = await GetText(page, "Order Status");
        Assert.That(orderStatus, Is.EqualTo("Queued for transmission"));

        // And nursing is notified to manually coordinate with lab
        var nursingCoordinationNotification = await GetText(page, "Nursing Coordination Notification");
        Assert.That(nursingCoordinationNotification.Length > 0, Is.True);

        // And I receive a notification when lab system connectivity is restored
        var connectivityRestoredNotification = await GetText(page, "Connectivity Restored Notification");
        Assert.That(connectivityRestoredNotification.Length > 0, Is.True);

        // And queued orders are automatically transmitted when system is available
        var queuedOrderTransmissionStatus = await GetText(page, "Queued Order Transmission Status");
        Assert.That(queuedOrderTransmissionStatus, Does.Match(@"transmitted").IgnoreCase);

        // And manual backup procedures are documented for critical orders
        var manualBackupProcedureDocumentation = await GetText(page, "Manual Backup Procedure Documentation");
        Assert.That(manualBackupProcedureDocumentation.Length > 0, Is.True);
    }

    [Test, Order(8)]
    [Description("Enter complex order sets for specific protocols")]
    public async Task EnterComplexOrderSetsForSpecificProtocols()
    {
        // Given I have examined a patient "James Thompson" in bed "ED-11"
        // And the patient presents with "suspected stroke"
        // (assumed pre-seeded test data)

        // When I select the "Acute Stroke Protocol" order set
        await FillField(page, "Order Set", "Acute Stroke Protocol");

        // Then the system presents the standardized stroke workup orders:
        //   | Category   | Order Description              | Priority | Default |
        //   | Laboratory | CBC, BMP, PT/INR, PTT         | STAT     | Selected|
        //   | Laboratory | Troponin, Lipid panel         | STAT     | Selected|
        //   | Radiology  | CT Head without contrast      | STAT     | Selected|
        //   | Radiology  | CT Angiogram head/neck        | STAT     | Optional|
        //   | Medication | Aspirin 325mg                 | STAT     | Selected|
        //   | Consults   | Neurology consult             | STAT     | Selected|
        var strokeProtocolOrders = page.GetByTestId("stroke-protocol-order");
        Assert.That(await strokeProtocolOrders.CountAsync(), Is.EqualTo(6));

        // And I can modify or remove individual orders from the set
        await WaitForTestId(page, "Modify Order");
        await WaitForTestId(page, "Remove Order");

        // And I add stroke-specific timing requirements:
        //   | Order          | Time Requirement                      |
        //   | CT Head        | Within 25 minutes of arrival         |
        //   | Lab results    | Within 45 minutes of arrival         |
        //   | Neurology      | Consult within 15 minutes           |
        await page.GetByTestId("add-timing-requirement-button").First.ClickAsync();

        // And the system tracks compliance with stroke protocol timing
        var protocolComplianceTrackingStatus = await GetText(page, "Protocol Compliance Tracking Status");
        Assert.That(protocolComplianceTrackingStatus, Does.Match(@"tracks compliance").IgnoreCase);

        // And automatic reminders are set for time-sensitive elements
        var automaticReminders = page.GetByTestId("automatic-reminder");
        Assert.That(await automaticReminders.CountAsync(), Is.EqualTo(3));
    }
}
