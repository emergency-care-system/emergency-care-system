// Playwright + NUnit test for
// tests-with-given-when-then-features/19-mass-casualty-activation.feature
// (equivalent to tests-with-playwright-javascript/19-mass-casualty-activation.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see Support/Fields.cs) and the shared
// data-testid contract in Support/Login.cs (login-identity, login-submit,
// app-root).

namespace EmergencyCareSystem.PlaywrightTests;

[TestFixture]
[NonParallelizable]
public class T19MassCasualtyActivationTests
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
        //   And I am logged in as "Charge Nurse Williams"
        //   And the mass casualty incident (MCI) module is available (assumed pre-seeded test data)
        //   And emergency contact systems are enabled (assumed pre-seeded test data)
        //   And surge capacity protocols are configured (assumed pre-seeded test data)
        await VerifySystemIsOperational(page);
        await Login(page, "Charge Nurse Williams");

        var featureNavLink = await WaitForTestId(page, "Nav Mass Casualty Activation");
        await featureNavLink.ClickAsync();
        await WaitForTestId(page, "Mass Casualty Activation Panel");
    }

    [Test, Order(1)]
    [Description("Activate mass casualty protocol for multi-vehicle accident")]
    public async Task ActivateMassCasualtyProtocolForMultiVehicleAccident()
    {
        // Given it is 16:30 on a Friday afternoon (assumed pre-seeded test data)
        // And normal ED operations are in progress with 12 patients currently in the department (assumed pre-seeded test data)
        // And EMS reports a multi-vehicle accident with 8+ casualties en route (assumed pre-seeded test data)
        // When I receive notification of the mass casualty incident: (assumed simulated by test fixture data)
        // And I activate the disaster protocol in the system
        await page.GetByTestId("activate-disaster-protocol").First.ClickAsync();

        // Then the system immediately switches to surge capacity mode:
        await WaitForTestId(page, "Mode Indicator");
        var systemChangeRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Mode Indicator"), ("value", "\"MASS CASUALTY ACTIVE\" banner displayed")),
            Row(("label", "Interface Switch"), ("value", "MCI-specific workflows activated")),
            Row(("label", "Normal Operations"), ("value", "Routine tasks suspended/deprioritized")),
            Row(("label", "Resource Allocation"), ("value", "Emergency resource management enabled")),
            Row(("label", "Communication Mode"), ("value", "Critical alerts and notifications active")),
        };
        foreach (var row in systemChangeRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And additional staff are automatically alerted:
        var staffAlertRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Off-duty Physicians"), ("value", "SMS, Phone call")),
            Row(("label", "Off-duty Nurses"), ("value", "SMS, Phone call")),
            Row(("label", "Surgical Team"), ("value", "Overhead page, SMS")),
            Row(("label", "Lab/Radiology"), ("value", "System alert, Phone")),
            Row(("label", "Administration"), ("value", "Phone call, SMS")),
            Row(("label", "Security"), ("value", "Radio, Overhead page")),
        };
        foreach (var row in staffAlertRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And rapid registration workflows are created:
        var registrationFeatureRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Patient Identification"), ("value", "Sequential numbering: MCI-001, MCI-002")),
            Row(("label", "Triage Tags"), ("value", "Color-coded electronic tags")),
            Row(("label", "Minimal Data Entry"), ("value", "Name, age, chief complaint only")),
            Row(("label", "Family Notification"), ("value", "Automated family alert system")),
            Row(("label", "Tracking Board"), ("value", "Real-time patient status dashboard")),
        };
        foreach (var row in registrationFeatureRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }
    }

    [Test, Order(2)]
    [Description("Configure surge capacity with bed and resource expansion")]
    public async Task ConfigureSurgeCapacityWithBedAndResourceExpansion()
    {
        // Given the mass casualty protocol has been activated
        // And normal bed capacity is 20 beds
        // When the system enters surge capacity mode

        // Then additional treatment areas are activated:
        await WaitForTestId(page, "Hallway Beds");
        var surgeAreaRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Hallway Beds"), ("value", "+6 treatment spaces")),
            Row(("label", "Procedure Rooms"), ("value", "+3 converted spaces")),
            Row(("label", "Observation Area"), ("value", "+8 holding spaces")),
            Row(("label", "Waiting Room Triage"), ("value", "+4 assessment areas")),
            Row(("label", "Ambulatory Care"), ("value", "+10 walking wounded")),
        };
        foreach (var row in surgeAreaRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And resource allocation is optimized for mass casualty:
        var resourceAllocationRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Trauma Bays"), ("value", "All 4 activated")),
            Row(("label", "Operating Rooms"), ("value", "3 rooms on standby")),
            Row(("label", "Ventilators"), ("value", "8 total (5 from ICU)")),
            Row(("label", "Blood Products"), ("value", "Massive transfusion protocol")),
            Row(("label", "Medication Carts"), ("value", "5 carts deployed")),
        };
        foreach (var row in resourceAllocationRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And staffing ratios are adjusted for emergency operations:
        var staffingRatioRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Physicians"), ("value", "1:12 patients")),
            Row(("label", "Nurses"), ("value", "1:6 patients")),
            Row(("label", "Support Staff"), ("value", "Double coverage")),
        };
        foreach (var row in staffingRatioRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }
    }

    [Test, Order(3)]
    [Description("Implement rapid patient registration and triage workflow")]
    public async Task ImplementRapidPatientRegistrationAndTriageWorkflow()
    {
        // Given mass casualty mode is active
        // And the first ambulance arrives with 3 critical patients
        // When EMS brings patients to the ED

        // Then the rapid registration workflow is initiated:
        await WaitForTestId(page, "Patient Arrival");
        var registrationStepRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Patient Arrival"), ("value", "Immediate tag assignment: MCI-001, 002, 003")),
            Row(("label", "Triage Assessment"), ("value", "START triage protocol applied")),
            Row(("label", "Electronic Tagging"), ("value", "Color-coded digital tags assigned")),
            Row(("label", "Minimal Documentation"), ("value", "Name, estimated age, mechanism of injury")),
            Row(("label", "Bed Assignment"), ("value", "Automatic assignment by acuity")),
        };
        foreach (var row in registrationStepRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And electronic triage tags are applied with color coding:
        var triageColorRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Red (Immediate)"), ("value", "MCI-001 → Trauma Bay 1")),
            Row(("label", "Yellow (Delayed)"), ("value", "MCI-002 → Surge Bed 3")),
            Row(("label", "Green (Minor)"), ("value", "MCI-003 → Ambulatory Area")),
            Row(("label", "Black (Deceased)"), ("value", "Morgue coordination")),
        };
        foreach (var row in triageColorRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And family notification systems are activated:
        var notificationProcessRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Emergency Contacts"), ("value", "Auto-dial from patient personal effects")),
            Row(("label", "Public Information"), ("value", "Hospital hotline number broadcasted")),
            Row(("label", "Media Coordination"), ("value", "Incident command liaison activated")),
            Row(("label", "Social Services"), ("value", "Family support team mobilized")),
        };
        foreach (var row in notificationProcessRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }
    }

    [Test, Order(4)]
    [Description("Coordinate with external emergency services and hospitals")]
    public async Task CoordinateWithExternalEmergencyServicesAndHospitals()
    {
        // Given a major mass casualty incident is in progress
        // And local EMS is overwhelmed with the response
        // When the system activates external coordination protocols

        // Then inter-facility communication is established:
        await WaitForTestId(page, "EMS Command Center");
        var communicationChannelRows = new List<Dictionary<string, string>>
        {
            Row(("label", "EMS Command Center"), ("value", "Patient distribution and transport updates")),
            Row(("label", "Other Area Hospitals"), ("value", "Bed availability and transfer coordination")),
            Row(("label", "Air Medical Services"), ("value", "Helicopter transport for critical patients")),
            Row(("label", "Regional Trauma Centers"), ("value", "Specialty care transfer arrangements")),
        };
        foreach (var row in communicationChannelRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And patient distribution management is activated:
        var distributionStrategyRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Load Balancing"), ("value", "Distribute patients across regional facilities")),
            Row(("label", "Specialty Matching"), ("value", "Route patients to appropriate specialty care")),
            Row(("label", "Capacity Monitoring"), ("value", "Real-time bed availability tracking")),
            Row(("label", "Transport Coordination"), ("value", "Ambulance and helicopter scheduling")),
        };
        foreach (var row in distributionStrategyRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And regional emergency management integration occurs:
        var integrationElementRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Incident Command"), ("value", "Hospital EOC links with regional ICS")),
            Row(("label", "Resource Sharing"), ("value", "Equipment and staff sharing protocols")),
            Row(("label", "Information Sharing"), ("value", "Patient status updates to command center")),
            Row(("label", "Media Management"), ("value", "Coordinated public information releases")),
        };
        foreach (var row in integrationElementRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }
    }

    [Test, Order(5)]
    [Description("Manage family reunification and information center")]
    public async Task ManageFamilyReunificationAndInformationCenter()
    {
        // Given multiple patients from the mass casualty incident are being treated
        // And families are arriving seeking information about their loved ones
        // When the family information center is activated

        // Then patient tracking and family communication systems are deployed:
        await WaitForTestId(page, "Information Hotline");
        var familySupportSystemRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Information Hotline"), ("value", "Dedicated phone line with trained staff")),
            Row(("label", "Family Reunification"), ("value", "Secure area for family waiting and updates")),
            Row(("label", "Patient Tracking"), ("value", "Real-time status board for authorized viewers")),
            Row(("label", "Privacy Protection"), ("value", "HIPAA-compliant information sharing")),
        };
        foreach (var row in familySupportSystemRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And automated family notification processes are initiated:
        var notificationMethodRows = new List<Dictionary<string, string>>
        {
            Row(("label", "SMS Updates"), ("value", "Your family member is being treated safely")),
            Row(("label", "Phone Calls"), ("value", "Personal calls for critical status changes")),
            Row(("label", "Information Boards"), ("value", "General incident updates (no patient names)")),
            Row(("label", "Social Workers"), ("value", "One-on-one family support and counseling")),
        };
        foreach (var row in notificationMethodRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And patient status tracking is maintained:
        var trackingElementRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Current Location"), ("value", "Treatment area, OR, transferred, etc.")),
            Row(("label", "Medical Status"), ("value", "Stable, critical, treated and released")),
            Row(("label", "Next of Kin Contact"), ("value", "Verification and notification status")),
            Row(("label", "Discharge Planning"), ("value", "Expected timeline and care needs")),
        };
        foreach (var row in trackingElementRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }
    }

    [Test, Order(6)]
    [Description("Transition from mass casualty mode back to normal operations")]
    public async Task TransitionFromMassCasualtyModeBackToNormalOperations()
    {
        // Given the mass casualty incident has been resolved
        // And all patients are stabilized or transferred
        // And no additional casualties are expected
        // When I initiate the transition back to normal operations
        await page.GetByTestId("initiate-transition-to-normal").First.ClickAsync();

        // Then the system manages the deactivation process:
        await WaitForTestId(page, "Incident Assessment");
        var deactivationStepRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Incident Assessment"), ("value", "Review of patient outcomes and resources used")),
            Row(("label", "Staff Debriefing"), ("value", "Immediate hot wash and formal debriefing")),
            Row(("label", "Resource Restoration"), ("value", "Return equipment and supplies to normal areas")),
            Row(("label", "Documentation"), ("value", "Complete incident documentation and reports")),
        };
        foreach (var row in deactivationStepRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And normal ED operations are gradually restored:
        var restorationPhaseRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Immediate (0-30 min)"), ("value", "Secure scene")),
            Row(("label", "Short-term (30-60 min)"), ("value", "Resource cleanup")),
            Row(("label", "Medium-term (1-4 hrs)"), ("value", "Staff rotation")),
            Row(("label", "Long-term (4-24 hrs)"), ("value", "Full restoration")),
        };
        foreach (var row in restorationPhaseRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And quality improvement activities are initiated:
        var qiActivityRows = new List<Dictionary<string, string>>
        {
            Row(("label", "After Action Review"), ("value", "Identify strengths and improvement areas")),
            Row(("label", "Performance Metrics"), ("value", "Analyze response times and patient outcomes")),
            Row(("label", "Protocol Updates"), ("value", "Revise procedures based on lessons learned")),
            Row(("label", "Training Needs"), ("value", "Identify staff training and education needs")),
        };
        foreach (var row in qiActivityRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }
    }

    [Test, Order(7)]
    [Description("Handle mass casualty incident during shift change")]
    public async Task HandleMassCasualtyIncidentDuringShiftChange()
    {
        // Given it is 19:00 during evening shift change
        // And day shift staff are preparing to leave
        // And evening shift staff are assuming duties
        // When a mass casualty incident is declared

        // Then the system manages staffing during the transition:
        await WaitForTestId(page, "Shift Hold");
        var staffingStrategyRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Shift Hold"), ("value", "Day shift staff remain for incident response")),
            Row(("label", "Double Coverage"), ("value", "Both shifts work together during surge")),
            Row(("label", "Incident Command Continuity"), ("value", "Clear leadership chain established")),
            Row(("label", "Communication"), ("value", "All staff briefed on roles and responsibilities")),
        };
        foreach (var row in staffingStrategyRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And transition protocols are modified for the emergency:
        var modifiedProtocolRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Handoff Procedures"), ("value", "Suspended until incident resolution")),
            Row(("label", "Staffing Ratios"), ("value", "Enhanced coverage with both shifts")),
            Row(("label", "Leadership Structure"), ("value", "Incident commander takes operational control")),
            Row(("label", "Modified Documentation"), ("value", "Emergency documentation procedures active")),
        };
        foreach (var row in modifiedProtocolRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }
    }

    [Test, Order(8)]
    [Description("Test mass casualty system readiness through drill")]
    public async Task TestMassCasualtySystemReadinessThroughDrill()
    {
        // Given it is a scheduled quarterly mass casualty drill
        // And the drill scenario involves a simulated building collapse with 15 casualties
        // When the drill coordinator activates the test mass casualty protocol
        await page.GetByTestId("activate-test-mci-protocol").First.ClickAsync();

        // Then the system activates in drill mode:
        await WaitForTestId(page, "Test Mode Indicator");
        var drillFeatureRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Test Mode Indicator"), ("value", "\"DRILL - NOT REAL EMERGENCY\" displayed")),
            Row(("label", "Simulated Patients"), ("value", "Test patient records created")),
            Row(("label", "Staff Participation"), ("value", "All roles and responsibilities tested")),
            Row(("label", "Resource Tracking"), ("value", "Equipment and supplies tracked but not used")),
        };
        foreach (var row in drillFeatureRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And drill performance metrics are captured:
        var drillPerformanceMetricRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Activation Time"), ("value", "Time from alert to full surge capacity")),
            Row(("label", "Staff Response Time"), ("value", "Time for staff to report and assume roles")),
            Row(("label", "Communication Speed"), ("value", "Time for all notifications to be completed")),
            Row(("label", "Resource Deployment"), ("value", "Time to set up surge areas and equipment")),
        };
        foreach (var row in drillPerformanceMetricRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }

        // And drill evaluation and improvement planning occurs:
        var evaluationComponentRows = new List<Dictionary<string, string>>
        {
            Row(("label", "Protocol Effectiveness"), ("value", "How well procedures worked in practice")),
            Row(("label", "Staff Preparedness"), ("value", "Knowledge and skill gaps identified")),
            Row(("label", "System Performance"), ("value", "Technology and workflow efficiency")),
            Row(("label", "Improvement Plans"), ("value", "Action items for enhancing response capabilities")),
        };
        foreach (var row in evaluationComponentRows)
        {
            Assert.That((await GetText(page, row["label"])), Is.EqualTo(row["value"]));
        }
    }
}
