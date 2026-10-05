// Playwright + NUnit test for
// tests-with-given-when-then-features/07-physician-assessment.feature
// (equivalent to tests-with-playwright-javascript/07-physician-assessment.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see Support/Fields.cs) and the shared
// data-testid contract in Support/Login.cs (login-identity, login-submit,
// app-root).

namespace EmergencyCareSystem.PlaywrightTests;

[TestFixture]
[NonParallelizable]
public class T07PhysicianAssessmentTests
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
        //   And I am logged in as "Dr. Smith" on the mobile app
        //   And the patient chart access module is enabled
        //   And real-time data synchronization is active
        await VerifySystemIsOperational(page);
        await Login(page, "Dr. Smith", mobile: true);
        // The patient chart access module and real-time data synchronization are
        // assumed to be pre-seeded/enabled test data.

        var featureNavLink = await WaitForTestId(page, "Nav Physician Assessment");
        await featureNavLink.ClickAsync();
        await WaitForTestId(page, "Physician Assessment Panel");
    }

    [Test, Order(1)]
    [Description("Access patient chart with complete nursing assessment")]
    public async Task AccessPatientChartWithCompleteNursingAssessment()
    {
        // Given a patient "Jennifer Martinez" is assigned to bed "ED-8"
        // And the nursing assessment is completed with the following data:
        // (assumed pre-seeded test data)

        // When I open the patient's chart for "Jennifer Martinez" in bed "ED-8"
        await page.GetByTestId("open-patient-chart-button").First.ClickAsync();

        // Then the system displays the patient summary with:
        var patientSummaryFields = new List<Dictionary<string, string>>
        {
            Row(("Section", "Patient Identity"), ("Content", "Jennifer Martinez, DOB: 1975-03-15")),
            Row(("Section", "Bed Assignment"), ("Content", "ED-8")),
            Row(("Section", "Arrival Time"), ("Content", "14:00")),
            Row(("Section", "Triage Notes"), ("Content", "ESI Level 2 - Severe chest pain, onset 2h ago")),
        };
        foreach (var rowData in patientSummaryFields)
        {
            var section = rowData["Section"];
            var content = rowData["Content"];
            Assert.That((await GetText(page, section)), Is.EqualTo(content));
        }

        // And the vital signs section shows:
        var vitalSigns = new List<Dictionary<string, string>>
        {
            Row(("Vital Sign", "Blood Pressure"), ("Value", "160/95"), ("Trend", "High")),
            Row(("Vital Sign", "Heart Rate"), ("Value", "110"), ("Trend", "Elevated")),
            Row(("Vital Sign", "Respiratory Rate"), ("Value", "22"), ("Trend", "Elevated")),
            Row(("Vital Sign", "Temperature"), ("Value", "98.6°F"), ("Trend", "Normal")),
            Row(("Vital Sign", "Oxygen Saturation"), ("Value", "94%"), ("Trend", "Low")),
            Row(("Vital Sign", "Pain Score"), ("Value", "8/10"), ("Trend", "Severe")),
        };
        foreach (var vital in vitalSigns)
        {
            Assert.That((await GetText(page, vital["Vital Sign"])), Is.EqualTo(vital["Value"]));
            Assert.That((await GetText(page, $"{vital["Vital Sign"]} Trend")), Is.EqualTo(vital["Trend"]));
        }

        // And the allergies section displays:
        var allergies = new List<Dictionary<string, string>>
        {
            Row(("Allergy", "Penicillin"), ("Reaction Type", "Rash"), ("Severity", "Moderate")),
            Row(("Allergy", "Shellfish"), ("Reaction Type", "Unknown"), ("Severity", "Unknown")),
        };
        foreach (var allergy in allergies)
        {
            Assert.That((await GetText(page, $"{allergy["Allergy"]} Reaction")), Is.EqualTo(allergy["Reaction Type"]));
            Assert.That((await GetText(page, $"{allergy["Allergy"]} Severity")), Is.EqualTo(allergy["Severity"]));
        }

        // And the current medications section shows:
        var medications = new List<Dictionary<string, string>>
        {
            Row(("Medication", "Metoprolol"), ("Dosage", "50mg"), ("Status", "Active")),
            Row(("Medication", "Aspirin"), ("Dosage", "81mg"), ("Status", "Active")),
        };
        foreach (var medication in medications)
        {
            Assert.That((await GetText(page, $"{medication["Medication"]} Dosage")), Is.EqualTo(medication["Dosage"]));
            Assert.That((await GetText(page, $"{medication["Medication"]} Status")), Is.EqualTo(medication["Status"]));
        }
    }

    [Test, Order(2)]
    [Description("Access patient chart during active treatment")]
    public async Task AccessPatientChartDuringActiveTreatment()
    {
        // Given a patient "Michael Chen" is assigned to bed "ED-3"
        // And the patient is currently receiving active treatment
        // And recent assessments include:
        // (assumed pre-seeded test data)

        // When I open the patient's chart for "Michael Chen" in bed "ED-3"
        await page.GetByTestId("open-patient-chart-button").First.ClickAsync();

        // Then the system displays real-time information with:
        var summaryFields = new List<Dictionary<string, string>>
        {
            Row(("Section", "Current Status"), ("Content", "Active treatment in progress")),
            Row(("Section", "Most Recent Vitals"), ("Content", "BP: 130/80, HR: 88, T: 100.2°F (14:15)")),
            Row(("Section", "Active Orders"), ("Content", "Lab work in progress")),
            Row(("Section", "Triage Summary"), ("Content", "ESI 3 - Abd pain, onset 6h ago")),
        };
        foreach (var rowData in summaryFields)
        {
            var section = rowData["Section"];
            var content = rowData["Content"];
            Assert.That((await GetText(page, section)), Is.EqualTo(content));
        }

        // And all data includes timestamps showing data freshness
        await WaitForTestId(page, "Data Freshness Timestamp");

        // And any alerts or critical values are highlighted in red
        var criticalValueElement = await WaitForTestId(page, "Critical Value Highlight");
        Assert.That((await criticalValueElement.IsVisibleAsync()), Is.True);

        // And pending lab results show "In Progress" status with expected completion time
        var labResultStatus = await GetText(page, "Lab Result Status");
        Assert.That(labResultStatus, Is.EqualTo("In Progress"));
    }

    [Test, Order(3)]
    [Description("View patient chart with medication allergies and interactions")]
    public async Task ViewPatientChartWithMedicationAllergiesAndInteractions()
    {
        // Given a patient "Robert Johnson" is assigned to bed "ED-12"
        // And the patient has multiple drug allergies:
        // And current medications include:
        // (assumed pre-seeded test data)

        // When I open the patient's chart for "Robert Johnson"
        await page.GetByTestId("open-patient-chart-button").First.ClickAsync();

        // Then the allergy section prominently displays:
        var allergyAlerts = new List<Dictionary<string, string>>
        {
            Row(("Alert Type", "Critical Alert"), ("Message", "SEVERE ALLERGIES: Morphine, NSAIDs")),
            Row(("Alert Type", "Warning"), ("Message", "Moderate allergy: Codeine")),
        };
        foreach (var alert in allergyAlerts)
        {
            Assert.That((await GetText(page, alert["Alert Type"])), Is.EqualTo(alert["Message"]));
        }

        // And the medication section shows:
        var medications = new List<Dictionary<string, string>>
        {
            Row(("Medication", "Warfarin"), ("Status", "Active"), ("Interaction Alerts", "Monitor for bleeding risk")),
            Row(("Medication", "Metformin"), ("Status", "Active"), ("Interaction Alerts", "No interactions detected")),
        };
        foreach (var medication in medications)
        {
            Assert.That((await GetText(page, $"{medication["Medication"]} Status")), Is.EqualTo(medication["Status"]));
            Assert.That((await GetText(page, $"{medication["Medication"]} Interaction Alerts")), Is.EqualTo(medication["Interaction Alerts"]));
        }

        // And any new medication orders will trigger allergy checking
        await WaitForTestId(page, "Allergy Checking Notice");

        // And interaction warnings are displayed for contraindicated drugs
        await WaitForTestId(page, "Interaction Warning");
    }

    [Test, Order(4)]
    [Description("Access chart for pediatric patient with age-appropriate data")]
    public async Task AccessChartForPediatricPatientWithAgeAppropriateData()
    {
        // Given a pediatric patient "Emma Foster" (age 7) is assigned to bed "ED-PEDS-2"
        // And the nursing assessment includes pediatric-specific data:
        // (assumed pre-seeded test data)

        // When I open the pediatric patient's chart for "Emma Foster"
        await page.GetByTestId("open-patient-chart-button").First.ClickAsync();

        // Then the system displays pediatric-specific information:
        var pediatricFields = new List<Dictionary<string, string>>
        {
            Row(("Section", "Patient Age/Weight"), ("Content", "7 years old, 22 kg")),
            Row(("Section", "Pediatric Vital Ranges"), ("Content", "All vitals with age-appropriate norms")),
            Row(("Section", "Growth Percentiles"), ("Content", "Weight: 50th percentile")),
            Row(("Section", "Guardian Information"), ("Content", "Sarah Foster (mother) - present")),
        };
        foreach (var rowData in pediatricFields)
        {
            var section = rowData["Section"];
            var content = rowData["Content"];
            Assert.That((await GetText(page, section)), Is.EqualTo(content));
        }

        // And vital signs are displayed with pediatric normal ranges:
        var vitalSigns = new List<Dictionary<string, string>>
        {
            Row(("Vital Sign", "Blood Pressure"), ("Value", "95/60"), ("Status", "Normal")),
            Row(("Vital Sign", "Heart Rate"), ("Value", "110"), ("Status", "Normal")),
            Row(("Vital Sign", "Respiratory"), ("Value", "24"), ("Status", "Normal")),
            Row(("Vital Sign", "Temperature"), ("Value", "102.8°F"), ("Status", "Elevated")),
        };
        foreach (var vital in vitalSigns)
        {
            Assert.That((await GetText(page, vital["Vital Sign"])), Is.EqualTo(vital["Value"]));
            Assert.That((await GetText(page, $"{vital["Vital Sign"]} Status")), Is.EqualTo(vital["Status"]));
        }

        // And medication dosing shows weight-based calculations
        await WaitForTestId(page, "Weight-Based Dosing");

        // And parental consent status is clearly indicated
        await WaitForTestId(page, "Parental Consent Status");
    }

    [Test, Order(5)]
    [Description("Handle incomplete nursing assessment")]
    public async Task HandleIncompleteNursingAssessment()
    {
        // Given a patient "David Wilson" is assigned to bed "ED-6"
        // And the nursing assessment is partially completed:
        // (assumed pre-seeded test data)

        // When I open the patient's chart for "David Wilson"
        await page.GetByTestId("open-patient-chart-button").First.ClickAsync();

        // Then the system displays available information clearly marked:
        var completedData = await GetText(page, "Completed Data");
        Assert.That(completedData, Is.EqualTo("Triage notes, initial vitals available"));

        var missingDataItems = page.GetByTestId("missing-data-item");
        Assert.That(await missingDataItems.CountAsync(), Is.EqualTo(3));
        var missingDataTexts = await TextsOf(missingDataItems);
        Assert.That(missingDataTexts, Is.EqualTo(new List<string> { "Allergies: Assessment in progress", "Medications: History pending", "Pain scale: Not yet assessed" }));

        // And incomplete sections are highlighted with:
        var visualIndicators = new List<Dictionary<string, string>>
        {
            Row(("Visual Indicator", "Yellow Warning"), ("Description", "Assessment in progress")),
            Row(("Visual Indicator", "Refresh Timer"), ("Description", "Auto-refresh every 30 seconds")),
            Row(("Visual Indicator", "Notification"), ("Description", "\"Assessment updating - refresh for latest\"")),
        };
        foreach (var indicator in visualIndicators)
        {
            Assert.That((await GetText(page, indicator["Visual Indicator"])), Is.EqualTo(indicator["Description"]));
        }

        // And I can request priority completion of missing critical data
        await WaitForTestId(page, "Request Priority Completion Button");
    }

    [Test, Order(6)]
    [Description("Access chart during shift change with handoff notes")]
    public async Task AccessChartDuringShiftChangeWithHandoffNotes()
    {
        // Given a patient "Lisa Brown" is assigned to bed "ED-9"
        // And it is during the evening shift change (19:00)
        // And the day shift nurse added handoff notes:
        // (assumed pre-seeded test data)

        // When I open the patient's chart for "Lisa Brown"
        await page.GetByTestId("open-patient-chart-button").First.ClickAsync();

        // Then the system prominently displays shift handoff information:
        var handoffFields = new List<Dictionary<string, string>>
        {
            Row(("Handoff Section", "Clinical Summary"), ("Content", "Stable condition, pain controlled")),
            Row(("Handoff Section", "Pending Tasks"), ("Content", "Orthopedic consult ordered - pending")),
            Row(("Handoff Section", "Communication Log"), ("Content", "Family contact: Son updated 18:30")),
            Row(("Handoff Section", "Special Needs"), ("Content", "Patient preference: Female staff")),
        };
        foreach (var field in handoffFields)
        {
            Assert.That((await GetText(page, field["Handoff Section"])), Is.EqualTo(field["Content"]));
        }

        // And the handoff notes are clearly timestamped
        await WaitForTestId(page, "Handoff Notes Timestamp");

        // And I can add my own physician handoff notes
        await WaitForTestId(page, "Add Physician Handoff Notes");

        // And the evening nurse can see both nursing and physician handoff information
        await WaitForTestId(page, "Combined Handoff Information");
    }

    [Test, Order(7)]
    [Description("Handle patient chart access during network connectivity issues")]
    public async Task HandlePatientChartAccessDuringNetworkConnectivityIssues()
    {
        // Given a patient "Thomas Anderson" is assigned to bed "ED-4"
        // And the mobile app has intermittent network connectivity
        // (assumed pre-seeded test data)

        // When I attempt to open the patient's chart
        await page.GetByTestId("open-patient-chart-button").First.ClickAsync();

        // And the network connection is temporarily unavailable
        // (simulated network condition, no direct UI action)

        // Then the system displays cached patient data with:
        var cachedDataFields = new List<Dictionary<string, string>>
        {
            Row(("Data Type", "Basic Demographics"), ("Availability", "Available (cached)")),
            Row(("Data Type", "Last Known Vitals"), ("Availability", "Available - last sync 16:45")),
            Row(("Data Type", "Medication Data"), ("Availability", "Available (cached)")),
            Row(("Data Type", "Recent Lab Results"), ("Availability", "May not be current - sync pending")),
        };
        foreach (var field in cachedDataFields)
        {
            Assert.That((await GetText(page, field["Data Type"])), Is.EqualTo(field["Availability"]));
        }

        // And a connectivity warning is displayed: "Limited connectivity - data may not be current"
        var connectivityWarning = await GetText(page, "Connectivity Warning");
        Assert.That(connectivityWarning, Is.EqualTo("Limited connectivity - data may not be current"));

        // And the app attempts automatic sync when connection is restored
        await WaitForTestId(page, "Automatic Sync Status");

        // And critical data is prioritized for sync when connectivity returns
        await WaitForTestId(page, "Sync Priority Notice");

        // And I can manually trigger refresh when connection improves
        await WaitForTestId(page, "Manual Refresh Button");
    }

    [Test, Order(8)]
    [Description("Access chart with time-sensitive alerts and notifications")]
    public async Task AccessChartWithTimeSensitiveAlertsAndNotifications()
    {
        // Given a patient "Karen White" is assigned to bed "ED-7"
        // And the patient has time-sensitive clinical alerts:
        // (assumed pre-seeded test data)

        // When I open the patient's chart for "Karen White"
        await page.GetByTestId("open-patient-chart-button").First.ClickAsync();

        // Then the system prominently displays active alerts:
        var activeAlerts = new List<Dictionary<string, string>>
        {
            Row(("Alert Priority", "CRITICAL"), ("Alert Details", "🔴 Troponin 0.8 - Possible MI (17:15)")),
            Row(("Alert Priority", "WARNING"), ("Alert Details", "🟡 Medication due - Metoprolol (17:30)")),
            Row(("Alert Priority", "INFO"), ("Alert Details", "🔵 Pain reassessment overdue (17:25)")),
        };
        foreach (var alert in activeAlerts)
        {
            Assert.That((await GetText(page, alert["Alert Priority"])), Is.EqualTo(alert["Alert Details"]));
        }

        // And critical alerts require acknowledgment before proceeding
        await WaitForTestId(page, "Alert Acknowledgment");

        // And the timestamp shows how long ago each alert was generated
        await WaitForTestId(page, "Alert Timestamp");

        // And I can take direct action on alerts (order meds, document assessment)
        await WaitForTestId(page, "Alert Action Button");

        // And alert resolution is tracked and timestamped
        await WaitForTestId(page, "Alert Resolution Tracking");
    }
}
