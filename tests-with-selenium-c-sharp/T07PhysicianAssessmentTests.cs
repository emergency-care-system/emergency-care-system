// Selenium WebDriver + NUnit test for
// tests-with-given-when-then-features/07-physician-assessment.feature
// (equivalent to tests-with-selenium-javascript/07-physician-assessment.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see Support/Fields.cs) and the shared
// data-testid contract in Support/Login.cs (login-identity, login-submit,
// app-root).

namespace EmergencyCareSystem.SeleniumTests;

[TestFixture]
[NonParallelizable]
public class T07PhysicianAssessmentTests
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
        //   And I am logged in as "Dr. Smith" on the mobile app
        //   And the patient chart access module is enabled
        //   And real-time data synchronization is active
        VerifySystemIsOperational(driver);
        Login(driver, "Dr. Smith", mobile: true);
        // The patient chart access module and real-time data synchronization are
        // assumed to be pre-seeded/enabled test data.

        var featureNavLink = WaitForTestId(driver, "Nav Physician Assessment");
        featureNavLink.Click();
        WaitForTestId(driver, "Physician Assessment Panel");
    }

    [Test, Order(1)]
    [Description("Access patient chart with complete nursing assessment")]
    public void AccessPatientChartWithCompleteNursingAssessment()
    {
        // Given a patient "Jennifer Martinez" is assigned to bed "ED-8"
        // And the nursing assessment is completed with the following data:
        // (assumed pre-seeded test data)

        // When I open the patient's chart for "Jennifer Martinez" in bed "ED-8"
        driver.FindElement(By.CssSelector("[data-testid=\"open-patient-chart-button\"]")).Click();

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
            Assert.That(GetText(driver, section), Is.EqualTo(content));
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
            Assert.That(GetText(driver, vital["Vital Sign"]), Is.EqualTo(vital["Value"]));
            Assert.That(GetText(driver, $"{vital["Vital Sign"]} Trend"), Is.EqualTo(vital["Trend"]));
        }

        // And the allergies section displays:
        var allergies = new List<Dictionary<string, string>>
        {
            Row(("Allergy", "Penicillin"), ("Reaction Type", "Rash"), ("Severity", "Moderate")),
            Row(("Allergy", "Shellfish"), ("Reaction Type", "Unknown"), ("Severity", "Unknown")),
        };
        foreach (var allergy in allergies)
        {
            Assert.That(GetText(driver, $"{allergy["Allergy"]} Reaction"), Is.EqualTo(allergy["Reaction Type"]));
            Assert.That(GetText(driver, $"{allergy["Allergy"]} Severity"), Is.EqualTo(allergy["Severity"]));
        }

        // And the current medications section shows:
        var medications = new List<Dictionary<string, string>>
        {
            Row(("Medication", "Metoprolol"), ("Dosage", "50mg"), ("Status", "Active")),
            Row(("Medication", "Aspirin"), ("Dosage", "81mg"), ("Status", "Active")),
        };
        foreach (var medication in medications)
        {
            Assert.That(GetText(driver, $"{medication["Medication"]} Dosage"), Is.EqualTo(medication["Dosage"]));
            Assert.That(GetText(driver, $"{medication["Medication"]} Status"), Is.EqualTo(medication["Status"]));
        }
    }

    [Test, Order(2)]
    [Description("Access patient chart during active treatment")]
    public void AccessPatientChartDuringActiveTreatment()
    {
        // Given a patient "Michael Chen" is assigned to bed "ED-3"
        // And the patient is currently receiving active treatment
        // And recent assessments include:
        // (assumed pre-seeded test data)

        // When I open the patient's chart for "Michael Chen" in bed "ED-3"
        driver.FindElement(By.CssSelector("[data-testid=\"open-patient-chart-button\"]")).Click();

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
            Assert.That(GetText(driver, section), Is.EqualTo(content));
        }

        // And all data includes timestamps showing data freshness
        WaitForTestId(driver, "Data Freshness Timestamp");

        // And any alerts or critical values are highlighted in red
        var criticalValueElement = WaitForTestId(driver, "Critical Value Highlight");
        Assert.That(criticalValueElement.Displayed, Is.True);

        // And pending lab results show "In Progress" status with expected completion time
        var labResultStatus = GetText(driver, "Lab Result Status");
        Assert.That(labResultStatus, Is.EqualTo("In Progress"));
    }

    [Test, Order(3)]
    [Description("View patient chart with medication allergies and interactions")]
    public void ViewPatientChartWithMedicationAllergiesAndInteractions()
    {
        // Given a patient "Robert Johnson" is assigned to bed "ED-12"
        // And the patient has multiple drug allergies:
        // And current medications include:
        // (assumed pre-seeded test data)

        // When I open the patient's chart for "Robert Johnson"
        driver.FindElement(By.CssSelector("[data-testid=\"open-patient-chart-button\"]")).Click();

        // Then the allergy section prominently displays:
        var allergyAlerts = new List<Dictionary<string, string>>
        {
            Row(("Alert Type", "Critical Alert"), ("Message", "SEVERE ALLERGIES: Morphine, NSAIDs")),
            Row(("Alert Type", "Warning"), ("Message", "Moderate allergy: Codeine")),
        };
        foreach (var alert in allergyAlerts)
        {
            Assert.That(GetText(driver, alert["Alert Type"]), Is.EqualTo(alert["Message"]));
        }

        // And the medication section shows:
        var medications = new List<Dictionary<string, string>>
        {
            Row(("Medication", "Warfarin"), ("Status", "Active"), ("Interaction Alerts", "Monitor for bleeding risk")),
            Row(("Medication", "Metformin"), ("Status", "Active"), ("Interaction Alerts", "No interactions detected")),
        };
        foreach (var medication in medications)
        {
            Assert.That(GetText(driver, $"{medication["Medication"]} Status"), Is.EqualTo(medication["Status"]));
            Assert.That(GetText(driver, $"{medication["Medication"]} Interaction Alerts"), Is.EqualTo(medication["Interaction Alerts"]));
        }

        // And any new medication orders will trigger allergy checking
        WaitForTestId(driver, "Allergy Checking Notice");

        // And interaction warnings are displayed for contraindicated drugs
        WaitForTestId(driver, "Interaction Warning");
    }

    [Test, Order(4)]
    [Description("Access chart for pediatric patient with age-appropriate data")]
    public void AccessChartForPediatricPatientWithAgeAppropriateData()
    {
        // Given a pediatric patient "Emma Foster" (age 7) is assigned to bed "ED-PEDS-2"
        // And the nursing assessment includes pediatric-specific data:
        // (assumed pre-seeded test data)

        // When I open the pediatric patient's chart for "Emma Foster"
        driver.FindElement(By.CssSelector("[data-testid=\"open-patient-chart-button\"]")).Click();

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
            Assert.That(GetText(driver, section), Is.EqualTo(content));
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
            Assert.That(GetText(driver, vital["Vital Sign"]), Is.EqualTo(vital["Value"]));
            Assert.That(GetText(driver, $"{vital["Vital Sign"]} Status"), Is.EqualTo(vital["Status"]));
        }

        // And medication dosing shows weight-based calculations
        WaitForTestId(driver, "Weight-Based Dosing");

        // And parental consent status is clearly indicated
        WaitForTestId(driver, "Parental Consent Status");
    }

    [Test, Order(5)]
    [Description("Handle incomplete nursing assessment")]
    public void HandleIncompleteNursingAssessment()
    {
        // Given a patient "David Wilson" is assigned to bed "ED-6"
        // And the nursing assessment is partially completed:
        // (assumed pre-seeded test data)

        // When I open the patient's chart for "David Wilson"
        driver.FindElement(By.CssSelector("[data-testid=\"open-patient-chart-button\"]")).Click();

        // Then the system displays available information clearly marked:
        var completedData = GetText(driver, "Completed Data");
        Assert.That(completedData, Is.EqualTo("Triage notes, initial vitals available"));

        var missingDataItems = driver.FindElements(By.CssSelector("[data-testid=\"missing-data-item\"]"));
        Assert.That(missingDataItems.Count, Is.EqualTo(3));
        var missingDataTexts = missingDataItems.Select(x => x.Text).ToList();
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
            Assert.That(GetText(driver, indicator["Visual Indicator"]), Is.EqualTo(indicator["Description"]));
        }

        // And I can request priority completion of missing critical data
        WaitForTestId(driver, "Request Priority Completion Button");
    }

    [Test, Order(6)]
    [Description("Access chart during shift change with handoff notes")]
    public void AccessChartDuringShiftChangeWithHandoffNotes()
    {
        // Given a patient "Lisa Brown" is assigned to bed "ED-9"
        // And it is during the evening shift change (19:00)
        // And the day shift nurse added handoff notes:
        // (assumed pre-seeded test data)

        // When I open the patient's chart for "Lisa Brown"
        driver.FindElement(By.CssSelector("[data-testid=\"open-patient-chart-button\"]")).Click();

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
            Assert.That(GetText(driver, field["Handoff Section"]), Is.EqualTo(field["Content"]));
        }

        // And the handoff notes are clearly timestamped
        WaitForTestId(driver, "Handoff Notes Timestamp");

        // And I can add my own physician handoff notes
        WaitForTestId(driver, "Add Physician Handoff Notes");

        // And the evening nurse can see both nursing and physician handoff information
        WaitForTestId(driver, "Combined Handoff Information");
    }

    [Test, Order(7)]
    [Description("Handle patient chart access during network connectivity issues")]
    public void HandlePatientChartAccessDuringNetworkConnectivityIssues()
    {
        // Given a patient "Thomas Anderson" is assigned to bed "ED-4"
        // And the mobile app has intermittent network connectivity
        // (assumed pre-seeded test data)

        // When I attempt to open the patient's chart
        driver.FindElement(By.CssSelector("[data-testid=\"open-patient-chart-button\"]")).Click();

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
            Assert.That(GetText(driver, field["Data Type"]), Is.EqualTo(field["Availability"]));
        }

        // And a connectivity warning is displayed: "Limited connectivity - data may not be current"
        var connectivityWarning = GetText(driver, "Connectivity Warning");
        Assert.That(connectivityWarning, Is.EqualTo("Limited connectivity - data may not be current"));

        // And the app attempts automatic sync when connection is restored
        WaitForTestId(driver, "Automatic Sync Status");

        // And critical data is prioritized for sync when connectivity returns
        WaitForTestId(driver, "Sync Priority Notice");

        // And I can manually trigger refresh when connection improves
        WaitForTestId(driver, "Manual Refresh Button");
    }

    [Test, Order(8)]
    [Description("Access chart with time-sensitive alerts and notifications")]
    public void AccessChartWithTimeSensitiveAlertsAndNotifications()
    {
        // Given a patient "Karen White" is assigned to bed "ED-7"
        // And the patient has time-sensitive clinical alerts:
        // (assumed pre-seeded test data)

        // When I open the patient's chart for "Karen White"
        driver.FindElement(By.CssSelector("[data-testid=\"open-patient-chart-button\"]")).Click();

        // Then the system prominently displays active alerts:
        var activeAlerts = new List<Dictionary<string, string>>
        {
            Row(("Alert Priority", "CRITICAL"), ("Alert Details", "🔴 Troponin 0.8 - Possible MI (17:15)")),
            Row(("Alert Priority", "WARNING"), ("Alert Details", "🟡 Medication due - Metoprolol (17:30)")),
            Row(("Alert Priority", "INFO"), ("Alert Details", "🔵 Pain reassessment overdue (17:25)")),
        };
        foreach (var alert in activeAlerts)
        {
            Assert.That(GetText(driver, alert["Alert Priority"]), Is.EqualTo(alert["Alert Details"]));
        }

        // And critical alerts require acknowledgment before proceeding
        WaitForTestId(driver, "Alert Acknowledgment");

        // And the timestamp shows how long ago each alert was generated
        WaitForTestId(driver, "Alert Timestamp");

        // And I can take direct action on alerts (order meds, document assessment)
        WaitForTestId(driver, "Alert Action Button");

        // And alert resolution is tracked and timestamped
        WaitForTestId(driver, "Alert Resolution Tracking");
    }
}
