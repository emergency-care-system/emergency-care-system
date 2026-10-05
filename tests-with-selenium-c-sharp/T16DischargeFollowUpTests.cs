// Selenium WebDriver + NUnit test for
// tests-with-given-when-then-features/16-discharge-follow-up.feature
// (equivalent to tests-with-selenium-javascript/16-discharge-follow-up.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see Support/Fields.cs) and the shared
// data-testid contract in Support/Login.cs (login-identity, login-submit,
// app-root).

namespace EmergencyCareSystem.SeleniumTests;

[TestFixture]
[NonParallelizable]
public class T16DischargeFollowUpTests
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
        //   And the patient portal is connected and functional
        //   And the follow-up scheduling module is active
        //   And automated reminder systems are enabled
        //   And patient communication preferences are configured
        VerifySystemIsOperational(driver);
        Login(driver, "a discharge coordinator");
        // The patient portal connection, follow-up scheduling module,
        // automated reminder systems, and communication preference
        // configuration are assumed to be active backend configuration
        // already in place for this environment.

        var dischargeFollowUpNavLink = WaitForTestId(driver, "Nav Discharge Follow-up");
        dischargeFollowUpNavLink.Click();
        WaitForTestId(driver, "Discharge Follow-up Panel");
    }

    [Test, Order(1)]
    [Description("Automatically schedule follow-up reminder for primary care")]
    public void AutomaticallyScheduleFollowUpReminderForPrimaryCare()
    {
        // Given a patient "Jennifer Martinez" has been discharged from bed "ED-8"
        // And the discharge orders include:
        //   | Follow-up Requirement | Details                                    |
        //   | Primary Care Visit    | Schedule within 3-5 days                  |
        //   | Reason for Follow-up  | UTI treatment response, medication review  |
        //   | Urgency Level         | Routine                                    |
        //   | Special Instructions  | Bring discharge paperwork and medication list |
        // And the patient has a registered primary care physician "Dr. Sarah Wilson"
        // (assumed pre-seeded test data)
        // When the discharge process is completed at 14:30
        // (assumed to have already occurred / triggered by the system)

        // Then the system automatically schedules a follow-up reminder:
        WaitForTestId(driver, "Initial Reminder");
        var followUpReminders = new List<Dictionary<string, string>>
        {
            Row(("Type", "Initial Reminder"), ("Details", "Day 2 after discharge (in 48 hours)")),
            Row(("Type", "Follow-up Reminder"), ("Details", "Day 4 after discharge if no appointment")),
            Row(("Type", "Final Reminder"), ("Details", "Day 6 after discharge (urgent)")),
            Row(("Type", "Reminder Methods"), ("Details", "Text, email, phone call")),
        };
        foreach (var rowData in followUpReminders)
        {
            var type = rowData["Type"];
            var details = rowData["Details"];
            Assert.That(GetText(driver, type), Is.EqualTo(details));
        }

        // And discharge instructions are automatically sent to the patient portal:
        var portalContent = new List<Dictionary<string, string>>
        {
            Row(("Section", "Discharge Summary"), ("Content", "Complete treatment summary and diagnosis")),
            Row(("Section", "Medication Instructions"), ("Content", "Prescription details and dosing schedule")),
            Row(("Section", "Follow-up Requirements"), ("Content", "Primary care appointment needed in 3-5 days")),
            Row(("Section", "Return Precautions"), ("Content", "When to seek emergency care")),
            Row(("Section", "Care Instructions"), ("Content", "Home care guidelines and activity restrictions")),
        };
        foreach (var rowData in portalContent)
        {
            var section = rowData["Section"];
            var content = rowData["Content"];
            Assert.That(GetText(driver, section), Is.EqualTo(content));
        }

        // And the patient receives immediate portal notification:
        var portalNotifications = new List<Dictionary<string, string>>
        {
            Row(("Type", "Portal Alert"), ("Content", "📋 New discharge instructions available")),
            Row(("Type", "Text Message"), ("Content", "\"ED discharge complete. Check patient portal for instructions\"")),
            Row(("Type", "Email Notification"), ("Content", "Detailed discharge summary with portal link")),
        };
        foreach (var rowData in portalNotifications)
        {
            var type = rowData["Type"];
            var content = rowData["Content"];
            Assert.That(GetText(driver, type), Is.EqualTo(content));
        }
    }

    [Test, Order(2)]
    [Description("Handle follow-up scheduling with multiple appointment types")]
    public void HandleFollowUpSchedulingWithMultipleAppointmentTypes()
    {
        // Given a patient "Robert Chen" is discharged with complex follow-up needs
        // And the discharge orders specify:
        //   | Follow-up Type        | Timeframe | Provider Type     | Priority  |
        //   | Primary Care         | 3 days    | Family Medicine   | High      |
        //   | Cardiology Consult   | 1 week    | Cardiologist      | Urgent    |
        //   | Lab Work Follow-up   | 5 days    | Lab/Primary Care  | Routine   |
        //   | Physical Therapy     | 2 weeks   | PT Specialist     | Routine   |
        // (assumed pre-seeded test data)
        // When the discharge process is completed
        // (assumed to have already occurred / triggered by the system)

        // Then the system creates multiple follow-up reminders:
        WaitForTestId(driver, "Primary Care Timing");
        var reminderSchedule = new List<Dictionary<string, string>>
        {
            Row(("Type", "Primary Care"), ("Timing", "Schedule within 2 days")),
            Row(("Type", "Cardiology"), ("Timing", "Schedule urgent consult")),
            Row(("Type", "Lab Work"), ("Timing", "Schedule blood draw")),
            Row(("Type", "Physical Therapy"), ("Timing", "Schedule PT evaluation")),
        };
        foreach (var rowData in reminderSchedule)
        {
            var type = rowData["Type"];
            var timing = rowData["Timing"];
            Assert.That(GetText(driver, $"{type} Timing"), Is.EqualTo(timing));
        }

        // And the patient portal receives comprehensive follow-up information:
        var portalFollowUpInfo = new List<Dictionary<string, string>>
        {
            Row(("Section", "Appointment Dashboard"), ("Content", "All required follow-ups with deadlines")),
            Row(("Section", "Provider Contacts"), ("Content", "Phone numbers and scheduling information")),
            Row(("Section", "Priority Indicators"), ("Content", "Urgent vs routine appointment labeling")),
            Row(("Section", "Preparation Instructions"), ("Content", "What to bring to each appointment")),
        };
        foreach (var rowData in portalFollowUpInfo)
        {
            var section = rowData["Section"];
            var content = rowData["Content"];
            Assert.That(GetText(driver, section), Is.EqualTo(content));
        }

        // And automated referrals are generated:
        var automatedReferrals = new List<Dictionary<string, string>>
        {
            Row(("Type", "Electronic Referral"), ("Action", "Sent to cardiology for urgent consult")),
            Row(("Type", "Lab Order"), ("Action", "Standing orders for follow-up labs")),
            Row(("Type", "PT Referral"), ("Action", "Physical therapy evaluation requested")),
        };
        foreach (var rowData in automatedReferrals)
        {
            var type = rowData["Type"];
            var action = rowData["Action"];
            Assert.That(GetText(driver, type), Is.EqualTo(action));
        }
    }

    [Test, Order(3)]
    [Description("Send discharge instructions to patient portal with multimedia content")]
    public void SendDischargeInstructionsToPatientPortalWithMultimediaContent()
    {
        // Given a patient "Maria Santos" was treated for "wound care management"
        // And the patient requires detailed home care instructions
        // (assumed pre-seeded test data)
        // When the discharge process includes educational materials:
        //   | Education Type        | Content Provided                           |
        //   | Wound Care Video      | Step-by-step dressing change demonstration |
        //   | Medication Guide      | Interactive dosing calculator              |
        //   | Warning Signs Chart   | Visual guide for infection symptoms        |
        //   | Activity Guidelines   | Illustrated movement restrictions          |
        // And the discharge is completed
        // (assumed to have already occurred / triggered by the system)

        // Then the patient portal receives multimedia instructions:
        WaitForTestId(driver, "Video Instructions");
        var multimediaInstructions = new List<Dictionary<string, string>>
        {
            Row(("Type", "Video Instructions"), ("Material", "Wound care demonstration (3 minutes)")),
            Row(("Type", "Interactive Tools"), ("Material", "Medication reminder scheduler")),
            Row(("Type", "Visual Guides"), ("Material", "Infection warning signs with photos")),
            Row(("Type", "Progress Tracking"), ("Material", "Healing milestone checklist")),
        };
        foreach (var rowData in multimediaInstructions)
        {
            var type = rowData["Type"];
            var material = rowData["Material"];
            Assert.That(GetText(driver, type), Is.EqualTo(material));
        }

        // And the patient receives learning verification:
        var learningVerification = new List<Dictionary<string, string>>
        {
            Row(("Method", "Video Completion"), ("Requirement", "Must watch wound care video fully")),
            Row(("Method", "Knowledge Check"), ("Requirement", "Brief quiz on warning signs")),
            Row(("Method", "Acknowledgment"), ("Requirement", "Confirm understanding of instructions")),
        };
        foreach (var rowData in learningVerification)
        {
            var method = rowData["Method"];
            var requirement = rowData["Requirement"];
            Assert.That(GetText(driver, method), Is.EqualTo(requirement));
        }

        // And completion tracking is recorded for quality assurance
        var completionTrackingStatus = GetText(driver, "Completion Tracking Status");
        Assert.That(completionTrackingStatus, Does.Match(@"recorded").IgnoreCase);
    }

    [Test, Order(4)]
    [Description("Handle follow-up reminders for patients without primary care physicians")]
    public void HandleFollowUpRemindersForPatientsWithoutPrimaryCarePhysicians()
    {
        // Given a patient "David Kim" is discharged
        // And the patient does not have an established primary care physician
        // And follow-up care is required within 5 days
        // (assumed pre-seeded test data)
        // When the discharge process is completed
        // (assumed to have already occurred / triggered by the system)

        // Then the system provides alternative follow-up options:
        WaitForTestId(driver, "Urgent Care Centers");
        var followUpOptions = new List<Dictionary<string, string>>
        {
            Row(("Option", "Urgent Care Centers"), ("Details", "List of nearby facilities with hours")),
            Row(("Option", "Hospital Clinic"), ("Details", "Available appointment slots")),
            Row(("Option", "Telehealth Options"), ("Details", "Virtual visit scheduling information")),
            Row(("Option", "Community Health Centers"), ("Details", "Low-cost provider options")),
        };
        foreach (var rowData in followUpOptions)
        {
            var option = rowData["Option"];
            var details = rowData["Details"];
            Assert.That(GetText(driver, option), Is.EqualTo(details));
        }

        // And enhanced reminder scheduling is activated:
        var enhancedReminders = new List<Dictionary<string, string>>
        {
            Row(("Type", "Daily Reminders"), ("Frequency", "For first 3 days after discharge")),
            Row(("Type", "Resource Assistance"), ("Frequency", "Links to find primary care providers")),
            Row(("Type", "Financial Counseling"), ("Frequency", "Information about insurance and payment")),
        };
        foreach (var rowData in enhancedReminders)
        {
            var type = rowData["Type"];
            var frequency = rowData["Frequency"];
            Assert.That(GetText(driver, type), Is.EqualTo(frequency));
        }

        // And the patient portal includes provider finding tools:
        var providerFindingTools = new List<Dictionary<string, string>>
        {
            Row(("Tool", "Provider Search"), ("Functionality", "Find doctors accepting new patients")),
            Row(("Tool", "Insurance Verification"), ("Functionality", "Check coverage for potential providers")),
            Row(("Tool", "Appointment Booking"), ("Functionality", "Direct scheduling with available providers")),
        };
        foreach (var rowData in providerFindingTools)
        {
            var tool = rowData["Tool"];
            var functionality = rowData["Functionality"];
            Assert.That(GetText(driver, tool), Is.EqualTo(functionality));
        }
    }

    [Test, Order(5)]
    [Description("Customize follow-up based on patient communication preferences")]
    public void CustomizeFollowUpBasedOnPatientCommunicationPreferences()
    {
        // Given a patient "Lisa Brown" has specified communication preferences:
        //   | Communication Method  | Preference    | Contact Information        |
        //   | Text Messages         | Preferred     | 555-123-4567              |
        //   | Email                 | Secondary     | lisa.brown@email.com      |
        //   | Phone Calls           | Emergency Only| 555-123-4567              |
        //   | Portal Notifications  | Enabled       | Username: lbrown123       |
        // And the patient is discharged with routine follow-up requirements
        // (assumed pre-seeded test data)
        // When the discharge process triggers follow-up communications
        // (assumed to have already occurred / triggered by the system)

        // Then the system respects patient communication preferences:
        WaitForTestId(driver, "Initial Instructions Method");
        var communicationPreferences = new List<Dictionary<string, string>>
        {
            Row(("Type", "Initial Instructions"), ("Method", "Text + Portal"), ("Content", "Brief summary with portal link")),
            Row(("Type", "Follow-up Reminders"), ("Method", "Text Message"), ("Content", "Appointment reminders")),
            Row(("Type", "Urgent Notifications"), ("Method", "Phone Call"), ("Content", "Critical lab results only")),
            Row(("Type", "Educational Content"), ("Method", "Portal Only"), ("Content", "Detailed instructions and videos")),
        };
        foreach (var rowData in communicationPreferences)
        {
            var type = rowData["Type"];
            var method = rowData["Method"];
            var content = rowData["Content"];
            Assert.That(GetText(driver, $"{type} Method"), Is.EqualTo(method));
            Assert.That(GetText(driver, $"{type} Content"), Is.EqualTo(content));
        }

        // And communication tracking records patient engagement:
        var communicationTracking = new List<Dictionary<string, string>>
        {
            Row(("Metric", "Message Delivery"), ("Measurement", "Successful text delivery confirmed")),
            Row(("Metric", "Portal Access"), ("Measurement", "Login timestamps and content viewed")),
            Row(("Metric", "Engagement Level"), ("Measurement", "Time spent reviewing instructions")),
        };
        foreach (var rowData in communicationTracking)
        {
            var metric = rowData["Metric"];
            var measurement = rowData["Measurement"];
            Assert.That(GetText(driver, metric), Is.EqualTo(measurement));
        }
    }

    [Test, Order(6)]
    [Description("Handle follow-up for pediatric patients with parent/guardian coordination")]
    public void HandleFollowUpForPediatricPatientsWithParentGuardianCoordination()
    {
        // Given a pediatric patient "Emma Foster" (age 6) is discharged
        // And the parent "Sarah Foster" is the primary contact
        // And follow-up includes pediatric-specific requirements:
        //   | Follow-up Type        | Pediatric Considerations                   |
        //   | Pediatrician Visit    | Growth and development check               |
        //   | Vaccination Updates   | Catch-up on missed immunizations          |
        //   | School Health Forms   | Medical clearance for return to school     |
        // (assumed pre-seeded test data)
        // When the discharge process is completed
        // (assumed to have already occurred / triggered by the system)

        // Then the system creates parent-focused follow-up communications:
        WaitForTestId(driver, "Parent Portal Account");
        var parentCommunications = new List<Dictionary<string, string>>
        {
            Row(("Target", "Parent Portal Account"), ("Type", "Child's medical summary and instructions")),
            Row(("Target", "School Notifications"), ("Type", "Medical excuse and return guidelines")),
            Row(("Target", "Pediatrician Alert"), ("Type", "ED visit summary and follow-up needs")),
        };
        foreach (var rowData in parentCommunications)
        {
            var target = rowData["Target"];
            var type = rowData["Type"];
            Assert.That(GetText(driver, target), Is.EqualTo(type));
        }

        // And pediatric-specific reminders are scheduled:
        var pediatricReminders = new List<Dictionary<string, string>>
        {
            Row(("Type", "Medication Reminders"), ("Instructions", "Weight-based dosing with schedule")),
            Row(("Type", "Development Milestones"), ("Instructions", "Age-appropriate recovery expectations")),
            Row(("Type", "School Return Criteria"), ("Instructions", "When child can safely return to activities")),
        };
        foreach (var rowData in pediatricReminders)
        {
            var type = rowData["Type"];
            var instructions = rowData["Instructions"];
            Assert.That(GetText(driver, type), Is.EqualTo(instructions));
        }

        // And child safety verification is included:
        var childSafetyChecks = new List<Dictionary<string, string>>
        {
            Row(("Check", "Home Safety Assessment"), ("Requirement", "Childproofing for medication storage")),
            Row(("Check", "Caregiver Instructions"), ("Requirement", "Multiple caregivers receive instructions")),
            Row(("Check", "Emergency Contacts"), ("Requirement", "Updated emergency contact information")),
        };
        foreach (var rowData in childSafetyChecks)
        {
            var check = rowData["Check"];
            var requirement = rowData["Requirement"];
            Assert.That(GetText(driver, check), Is.EqualTo(requirement));
        }
    }

    [Test, Order(7)]
    [Description("Track follow-up compliance and patient outcomes")]
    public void TrackFollowUpComplianceAndPatientOutcomes()
    {
        // Given multiple patients have been discharged with follow-up requirements
        // (assumed pre-seeded test data)
        // When follow-up reminders are sent and appointments are scheduled
        // (assumed to have already occurred / triggered by the system)

        // Then the system tracks compliance metrics:
        WaitForTestId(driver, "Appointment Scheduling");
        var complianceMetrics = new List<Dictionary<string, string>>
        {
            Row(("Metric", "Appointment Scheduling"), ("Measurement", "% of patients who schedule within timeframe")),
            Row(("Metric", "Appointment Attendance"), ("Measurement", "% of scheduled appointments kept")),
            Row(("Metric", "Portal Engagement"), ("Measurement", "% of patients accessing discharge instructions")),
            Row(("Metric", "Medication Compliance"), ("Measurement", "% following prescription instructions")),
        };
        foreach (var rowData in complianceMetrics)
        {
            var metric = rowData["Metric"];
            var measurement = rowData["Measurement"];
            Assert.That(GetText(driver, metric), Is.EqualTo(measurement));
        }

        // And outcome tracking is performed:
        var outcomeTracking = new List<Dictionary<string, string>>
        {
            Row(("Metric", "ED Readmissions"), ("Method", "72-hour and 30-day return rates")),
            Row(("Metric", "Complication Rates"), ("Method", "Follow-up visits for related issues")),
            Row(("Metric", "Patient Satisfaction"), ("Method", "Follow-up surveys about discharge process")),
        };
        foreach (var rowData in outcomeTracking)
        {
            var metric = rowData["Metric"];
            var method = rowData["Method"];
            Assert.That(GetText(driver, metric), Is.EqualTo(method));
        }

        // And quality improvement reports are generated:
        var qualityImprovementReports = new List<Dictionary<string, string>>
        {
            Row(("Report", "Follow-up Effectiveness"), ("Content", "Success rates by discharge diagnosis")),
            Row(("Report", "Communication Analysis"), ("Content", "Best-performing reminder methods")),
            Row(("Report", "Provider Performance"), ("Content", "Follow-up compliance by discharging physician")),
        };
        foreach (var rowData in qualityImprovementReports)
        {
            var report = rowData["Report"];
            var content = rowData["Content"];
            Assert.That(GetText(driver, report), Is.EqualTo(content));
        }
    }

    [Test, Order(8)]
    [Description("Handle follow-up complications and escalation procedures")]
    public void HandleFollowUpComplicationsAndEscalationProcedures()
    {
        // Given a patient "Michael Davis" was discharged 2 days ago
        // And follow-up reminders have been sent
        // (assumed pre-seeded test data)
        // When the patient contacts the ED with worsening symptoms
        // And the patient has not yet scheduled the required follow-up appointment
        // (assumed to have already occurred / triggered by the system)

        // Then the system escalates the follow-up process:
        WaitForTestId(driver, "Urgent Scheduling");
        var escalationActions = new List<Dictionary<string, string>>
        {
            Row(("Action", "Urgent Scheduling"), ("Details", "Same-day appointment coordination")),
            Row(("Action", "Provider Notification"), ("Details", "Original discharging physician alerted")),
            Row(("Action", "Symptom Assessment"), ("Details", "Nurse triage for immediate vs delayed care")),
            Row(("Action", "Documentation Update"), ("Details", "Patient contact and status change recorded")),
        };
        foreach (var rowData in escalationActions)
        {
            var action = rowData["Action"];
            var details = rowData["Details"];
            Assert.That(GetText(driver, action), Is.EqualTo(details));
        }

        // And enhanced monitoring is activated:
        var enhancedMonitoring = new List<Dictionary<string, string>>
        {
            Row(("Type", "Daily Check-ins"), ("Action", "Nurse calls patient for status updates")),
            Row(("Type", "Expedited Referrals"), ("Action", "Fast-track specialist appointments")),
            Row(("Type", "Safety Net Activation"), ("Action", "Ensure patient has immediate care access")),
        };
        foreach (var rowData in enhancedMonitoring)
        {
            var type = rowData["Type"];
            var action = rowData["Action"];
            Assert.That(GetText(driver, type), Is.EqualTo(action));
        }

        // And the care team receives comprehensive updates:
        var careTeamUpdates = new List<Dictionary<string, string>>
        {
            Row(("Member", "Discharging Physician"), ("Information", "Patient contact and current status")),
            Row(("Member", "Primary Care Provider"), ("Information", "Urgent need for appointment")),
            Row(("Member", "Charge Nurse"), ("Information", "Potential readmission risk identified")),
        };
        foreach (var rowData in careTeamUpdates)
        {
            var member = rowData["Member"];
            var information = rowData["Information"];
            Assert.That(GetText(driver, member), Is.EqualTo(information));
        }
    }
}
