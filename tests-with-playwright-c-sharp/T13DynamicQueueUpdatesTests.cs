// Playwright + NUnit test for
// tests-with-given-when-then-features/13-dynamic-queue-updates.feature
// (equivalent to tests-with-playwright-javascript/13-dynamic-queue-updates.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see Support/Fields.cs) and the shared
// data-testid contract in Support/Login.cs (login-identity, login-submit,
// app-root).

namespace EmergencyCareSystem.PlaywrightTests;

[TestFixture]
[NonParallelizable]
public class T13DynamicQueueUpdatesTests
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
        //   And the dynamic queue management module is active
        //   And the ESI (Emergency Severity Index) prioritization system is enabled
        //   And 15 patients are currently waiting to be seen
        await VerifySystemIsOperational(page);
        await Login(page, "a charge nurse");
        // The dynamic queue management module, the ESI prioritization system,
        // and the 15 already-waiting patients are assumed to be pre-seeded
        // test environment state.

        var dynamicQueueUpdatesNavLink = await WaitForTestId(page, "Nav Dynamic Queue Updates");
        await dynamicQueueUpdatesNavLink.ClickAsync();
        await WaitForTestId(page, "Dynamic Queue Updates Panel");
    }

    [Test, Order(1)]
    [Description("High-priority trauma patient bumps existing queue")]
    public async Task HighPriorityTraumaPatientBumpsExistingQueue()
    {
        // Given the current patient queue contains:
        //   | Position | Patient Name    | ESI Level | Triage Level   | Current Wait Time |
        //   | 1        | Alice Johnson   | 2         | High Priority  | 20 minutes       |
        //   | 2        | Bob Williams    | 2         | High Priority  | 35 minutes       |
        //   | 3        | Carol Davis     | 3         | Urgent         | 45 minutes       |
        //   | 4        | David Brown     | 3         | Urgent         | 60 minutes       |
        //   | 5        | Emma Wilson     | 3         | Urgent         | 75 minutes       |
        //   | 6-15     | Other patients  | 3-5       | Various        | 90-180 minutes   |
        // And available providers can see 1 patient every 30 minutes

        // When a new trauma patient "Emergency Trauma" arrives with ESI level 1
        await FillFields(page, new List<Dictionary<string, string>>
        {
            Row(("Field", "Patient Name"), ("Value", "Emergency Trauma")),
            Row(("Field", "ESI Level"), ("Value", "1")),
        });
        await page.GetByTestId("submit-patient-arrival").First.ClickAsync();
        // And the patient is triaged as "Resuscitation - Life threatening"
        await FillField(page, "Triage Level", "Resuscitation - Life threatening");
        await page.GetByTestId("submit-triage").First.ClickAsync();

        // Then the system immediately reprioritizes the queue:
        //   | New Position | Patient Name      | ESI Level | Wait Time Impact    |
        //   | 1            | Emergency Trauma  | 1         | Immediate           |
        //   | 2            | Alice Johnson     | 2         | +30 min (50 min)    |
        //   | 3            | Bob Williams      | 2         | +30 min (65 min)    |
        //   | 4            | Carol Davis       | 3         | +30 min (75 min)    |
        //   | 5            | David Brown       | 3         | +30 min (90 min)    |
        await WaitForTestId(page, "Emergency Trauma Queue Position");
        Assert.That((await GetText(page, "Emergency Trauma Queue Position")), Is.EqualTo("1"));
        Assert.That((await GetText(page, "Emergency Trauma Wait Time Impact")), Is.EqualTo("Immediate"));
        Assert.That((await GetText(page, "Alice Johnson Queue Position")), Is.EqualTo("2"));
        Assert.That((await GetText(page, "Alice Johnson Wait Time Impact")), Is.EqualTo("+30 min (50 min)"));
        Assert.That((await GetText(page, "Bob Williams Queue Position")), Is.EqualTo("3"));
        Assert.That((await GetText(page, "Bob Williams Wait Time Impact")), Is.EqualTo("+30 min (65 min)"));
        Assert.That((await GetText(page, "Carol Davis Queue Position")), Is.EqualTo("4"));
        Assert.That((await GetText(page, "Carol Davis Wait Time Impact")), Is.EqualTo("+30 min (75 min)"));
        Assert.That((await GetText(page, "David Brown Queue Position")), Is.EqualTo("5"));
        Assert.That((await GetText(page, "David Brown Wait Time Impact")), Is.EqualTo("+30 min (90 min)"));

        // And the system updates wait time estimates for all affected patients:
        //   | Patient Name    | Previous Estimate | New Estimate | Change      |
        //   | Alice Johnson   | 20 minutes       | 50 minutes   | +30 minutes |
        //   | Bob Williams    | 35 minutes       | 65 minutes   | +30 minutes |
        //   | Carol Davis     | 45 minutes       | 75 minutes   | +30 minutes |
        //   | All others      | Various          | +30 minutes  | Increased   |
        Assert.That((await GetText(page, "Alice Johnson Wait Time Estimate")), Is.EqualTo("50 minutes"));
        Assert.That((await GetText(page, "Bob Williams Wait Time Estimate")), Is.EqualTo("65 minutes"));
        Assert.That((await GetText(page, "Carol Davis Wait Time Estimate")), Is.EqualTo("75 minutes"));
        var allOthersChange = await GetText(page, "All Others Change");
        Assert.That(allOthersChange, Does.Match(@"increased").IgnoreCase);

        // And notifications are sent to affected patients and families
        await WaitForTestId(page, "Patient Family Notifications Sent");

        // And the trauma team is immediately alerted for the ESI Level 1 patient
        var traumaTeamAlert = await GetText(page, "Trauma Team Alert");
        Assert.That(traumaTeamAlert, Does.Match(@"ESI Level 1"));
    }

    [Test, Order(2)]
    [Description("Multiple high-acuity patients arrive simultaneously")]
    public async Task MultipleHighAcuityPatientsArriveSimultaneously()
    {
        // Given the current queue has patients with ESI levels 3-5
        // And the next available appointment slot is in 60 minutes

        // When multiple high-acuity patients arrive within 10 minutes:
        //   | Arrival Time | Patient Name     | ESI Level | Chief Complaint          |
        //   | 14:00        | Critical Patient | 1         | Cardiac arrest           |
        //   | 14:05        | Urgent Patient A | 2         | Severe chest pain        |
        //   | 14:08        | Urgent Patient B | 2         | Difficulty breathing     |
        await FillFields(page, new List<Dictionary<string, string>>
        {
            Row(("Field", "Patient Name 1"), ("Value", "Critical Patient")),
            Row(("Field", "ESI Level 1"), ("Value", "1")),
            Row(("Field", "Arrival Time 1"), ("Value", "14:00")),
            Row(("Field", "Patient Name 2"), ("Value", "Urgent Patient A")),
            Row(("Field", "ESI Level 2"), ("Value", "2")),
            Row(("Field", "Arrival Time 2"), ("Value", "14:05")),
            Row(("Field", "Patient Name 3"), ("Value", "Urgent Patient B")),
            Row(("Field", "ESI Level 3"), ("Value", "2")),
            Row(("Field", "Arrival Time 3"), ("Value", "14:08")),
        });
        await page.GetByTestId("submit-patient-arrivals").First.ClickAsync();

        // Then the system prioritizes patients by ESI level and arrival time:
        //   | New Position | Patient Name     | ESI Level | Priority Rationale        |
        //   | 1            | Critical Patient | 1         | Highest acuity - immediate |
        //   | 2            | Urgent Patient A | 2         | ESI 2 - arrived first     |
        //   | 3            | Urgent Patient B | 2         | ESI 2 - arrived second    |
        //   | 4-18         | Existing patients| 3-5       | Lower priority            |
        await WaitForTestId(page, "Critical Patient Queue Position");
        Assert.That((await GetText(page, "Critical Patient Queue Position")), Is.EqualTo("1"));
        Assert.That((await GetText(page, "Urgent Patient A Queue Position")), Is.EqualTo("2"));
        Assert.That((await GetText(page, "Urgent Patient B Queue Position")), Is.EqualTo("3"));

        // And the system calculates cascading wait time impacts:
        //   | Patient Category | Wait Time Impact                              |
        //   | ESI Level 3      | +90 minutes (3 new higher priority patients) |
        //   | ESI Level 4      | +90 minutes                                   |
        //   | ESI Level 5      | +90 minutes                                   |
        Assert.That((await GetText(page, "ESI Level 3 Wait Time Impact")), Is.EqualTo("+90 minutes (3 new higher priority patients)"));
        Assert.That((await GetText(page, "ESI Level 4 Wait Time Impact")), Is.EqualTo("+90 minutes"));
        Assert.That((await GetText(page, "ESI Level 5 Wait Time Impact")), Is.EqualTo("+90 minutes"));

        // And multiple department alerts are triggered:
        //   | Department    | Alert Type                                    |
        //   | Trauma Team   | ESI 1 - Immediate response required          |
        //   | Cardiology    | Multiple cardiac-related ESI 2 patients     |
        //   | Administration| Surge capacity - consider additional staff   |
        Assert.That((await GetText(page, "Trauma Team Alert Type")), Is.EqualTo("ESI 1 - Immediate response required"));
        Assert.That((await GetText(page, "Cardiology Alert Type")), Is.EqualTo("Multiple cardiac-related ESI 2 patients"));
        Assert.That((await GetText(page, "Administration Alert Type")), Is.EqualTo("Surge capacity - consider additional staff"));
    }

    [Test, Order(3)]
    [Description("Queue updates with bed availability constraints")]
    public async Task QueueUpdatesWithBedAvailabilityConstraints()
    {
        // Given 15 patients are waiting and only 2 beds are currently available
        // And the bed types are:
        //   | Bed Number | Bed Type     | Status    |
        //   | ED-5       | Standard     | Available |
        //   | ED-TRAUMA-1| Trauma       | Available |
        //   | ED-8       | Standard     | Occupied  |
        //   | ED-12      | Isolation    | Occupied  |

        // When a trauma patient with ESI level 1 arrives requiring trauma bay
        await FillFields(page, new List<Dictionary<string, string>>
        {
            Row(("Field", "Patient Name"), ("Value", "Trauma Patient")),
            Row(("Field", "ESI Level"), ("Value", "1")),
            Row(("Field", "Bed Type Required"), ("Value", "Trauma")),
        });
        await page.GetByTestId("submit-patient-arrival").First.ClickAsync();

        // Then the system updates the queue considering bed constraints:
        //   | Queue Position | Patient Name    | ESI Level | Bed Assignment Strategy     |
        //   | 1              | Trauma Patient  | 1         | ED-TRAUMA-1 (immediate)     |
        //   | 2              | Alice Johnson   | 2         | ED-5 when available         |
        //   | 3              | Bob Williams    | 2         | Wait for next bed           |
        await WaitForTestId(page, "Trauma Patient Queue Position");
        Assert.That((await GetText(page, "Trauma Patient Queue Position")), Is.EqualTo("1"));
        Assert.That((await GetText(page, "Trauma Patient Bed Assignment Strategy")), Is.EqualTo("ED-TRAUMA-1 (immediate)"));
        Assert.That((await GetText(page, "Alice Johnson Queue Position")), Is.EqualTo("2"));
        Assert.That((await GetText(page, "Alice Johnson Bed Assignment Strategy")), Is.EqualTo("ED-5 when available"));
        Assert.That((await GetText(page, "Bob Williams Queue Position")), Is.EqualTo("3"));
        Assert.That((await GetText(page, "Bob Williams Bed Assignment Strategy")), Is.EqualTo("Wait for next bed"));

        // And wait times reflect both queue position and bed availability:
        //   | Patient Name    | Queue Wait | Bed Wait  | Total Estimate |
        //   | Trauma Patient  | 0 minutes  | 0 minutes | Immediate      |
        //   | Alice Johnson   | 0 minutes  | 0 minutes | Immediate      |
        //   | Bob Williams    | 0 minutes  | 45 minutes| 45 minutes     |
        Assert.That((await GetText(page, "Trauma Patient Total Estimate")), Is.EqualTo("Immediate"));
        Assert.That((await GetText(page, "Alice Johnson Total Estimate")), Is.EqualTo("Immediate"));
        Assert.That((await GetText(page, "Bob Williams Total Estimate")), Is.EqualTo("45 minutes"));

        // And the system provides realistic expectations based on resource constraints
        await WaitForTestId(page, "Resource Constraint Notice");
    }

    [Test, Order(4)]
    [Description("Handle queue updates during provider capacity changes")]
    public async Task HandleQueueUpdatesDuringProviderCapacityChanges()
    {
        // Given the current provider capacity is 3 physicians seeing patients
        // And average patient encounter time is 30 minutes
        // And 15 patients are in queue with estimated wait times

        // When one physician becomes unavailable due to emergency procedure
        await page.GetByTestId("mark-physician-unavailable").First.ClickAsync();
        // And a new ESI level 1 patient arrives
        await FillField(page, "ESI Level", "1");
        await page.GetByTestId("submit-patient-arrival").First.ClickAsync();

        // Then the system recalculates queue times with reduced capacity:
        //   | Capacity Change | Impact                                        |
        //   | 3 → 2 providers | 50% increase in wait times for existing patients |
        //   | ESI 1 arrival  | All patients bumped down one position        |
        await WaitForTestId(page, "3 → 2 providers");
        Assert.That((await GetText(page, "3 → 2 providers")), Is.EqualTo("50% increase in wait times for existing patients"));
        Assert.That((await GetText(page, "ESI 1 arrival")), Is.EqualTo("All patients bumped down one position"));

        // And updated wait time calculations reflect both changes:
        //   | Patient Category | Original Wait | Capacity Impact | ESI 1 Impact | New Wait   |
        //   | ESI Level 2      | 30 minutes   | +15 minutes     | +30 minutes  | 75 minutes |
        //   | ESI Level 3      | 60 minutes   | +30 minutes     | +30 minutes  | 120 minutes|
        //   | ESI Level 4      | 90 minutes   | +45 minutes     | +30 minutes  | 165 minutes|
        Assert.That((await GetText(page, "ESI Level 2 New Wait")), Is.EqualTo("75 minutes"));
        Assert.That((await GetText(page, "ESI Level 3 New Wait")), Is.EqualTo("120 minutes"));
        Assert.That((await GetText(page, "ESI Level 4 New Wait")), Is.EqualTo("165 minutes"));

        // And the system sends capacity alerts to administration
        await WaitForTestId(page, "Capacity Alert");

        // And patients/families are notified of updated wait times
        await WaitForTestId(page, "Patient Family Notifications Sent");
    }

    [Test, Order(5)]
    [Description("Prioritize patient with rapidly deteriorating condition")]
    public async Task PrioritizePatientWithRapidlyDeterioratingCondition()
    {
        // Given a patient "Sarah Mitchell" is currently position 8 in queue with ESI level 3
        // And her initial complaint was "mild abdominal pain"

        // When the patient's condition deteriorates and reassessment shows:
        //   | Assessment Change | New Value                                     |
        //   | Pain Level        | 3/10 → 9/10                                  |
        //   | Vital Signs       | BP: 120/80 → 85/50, HR: 80 → 120            |
        //   | Mental Status     | Alert → Confused                             |
        //   | ESI Level         | 3 → 2                                        |
        await FillFields(page, new List<Dictionary<string, string>>
        {
            Row(("Field", "Pain Level"), ("Value", "9/10")),
            Row(("Field", "Vital Signs"), ("Value", "BP: 85/50, HR: 120")),
            Row(("Field", "Mental Status"), ("Value", "Confused")),
            Row(("Field", "ESI Level"), ("Value", "2")),
        });
        await page.GetByTestId("submit-reassessment").First.ClickAsync();

        // Then the system immediately updates her queue position:
        //   | Action Type       | Details                                       |
        //   | Priority Escalation| ESI 3 → ESI 2 due to deterioration          |
        //   | Queue Repositioning| Position 8 → Position 2                     |
        //   | Wait Time Update  | 120 minutes → 15 minutes                     |
        await WaitForTestId(page, "Priority Escalation");
        Assert.That((await GetText(page, "Priority Escalation")), Is.EqualTo("ESI 3 → ESI 2 due to deterioration"));
        Assert.That((await GetText(page, "Queue Repositioning")), Is.EqualTo("Position 8 → Position 2"));
        Assert.That((await GetText(page, "Wait Time Update")), Is.EqualTo("120 minutes → 15 minutes"));

        // And escalation notifications are sent:
        //   | Recipient         | Notification Content                          |
        //   | Attending Physician| Patient deterioration - Priority escalated   |
        //   | Charge Nurse      | Sarah Mitchell moved to position 2           |
        //   | Triage Nurse      | Reassessment resulted in ESI upgrade         |
        Assert.That((await GetText(page, "Attending Physician Notification")), Is.EqualTo("Patient deterioration - Priority escalated"));
        Assert.That((await GetText(page, "Charge Nurse Notification")), Is.EqualTo("Sarah Mitchell moved to position 2"));
        Assert.That((await GetText(page, "Triage Nurse Notification")), Is.EqualTo("Reassessment resulted in ESI upgrade"));

        // And all subsequent patients are shifted down in the queue
        await WaitForTestId(page, "Queue Shift Notice");

        // And family members are notified of the priority change
        await WaitForTestId(page, "Family Priority Change Notification");
    }

    [Test, Order(6)]
    [Description("Handle specialty service requirements in queue management")]
    public async Task HandleSpecialtyServiceRequirementsInQueueManagement()
    {
        // Given 15 patients are waiting with various specialty needs:
        //   | Patient Name    | ESI Level | Specialty Required  | Current Position |
        //   | General Patient | 3         | None               | 3                |
        //   | Cardiac Patient | 2         | Cardiology         | 5                |
        //   | Peds Patient    | 3         | Pediatrics         | 8                |

        // When a new trauma patient arrives requiring neurosurgery consultation
        // And the patient has ESI level 1
        await FillFields(page, new List<Dictionary<string, string>>
        {
            Row(("Field", "Patient Name"), ("Value", "Trauma/Neuro")),
            Row(("Field", "ESI Level"), ("Value", "1")),
            Row(("Field", "Specialty Required"), ("Value", "Neurosurgery")),
        });
        await page.GetByTestId("submit-patient-arrival").First.ClickAsync();

        // Then the system considers both acuity and specialty availability:
        //   | Priority Factor   | Consideration                                 |
        //   | ESI Level 1       | Highest medical priority                     |
        //   | Neurosurgery Need | Specialty consultant availability            |
        //   | Resource Planning | OR availability for potential surgery        |
        await WaitForTestId(page, "ESI Level 1 Consideration");
        Assert.That((await GetText(page, "ESI Level 1 Consideration")), Is.EqualTo("Highest medical priority"));
        Assert.That((await GetText(page, "Neurosurgery Need Consideration")), Is.EqualTo("Specialty consultant availability"));
        Assert.That((await GetText(page, "Resource Planning Consideration")), Is.EqualTo("OR availability for potential surgery"));

        // And the queue is updated with specialty considerations:
        //   | Position | Patient Name    | Priority Reason                          |
        //   | 1        | Trauma/Neuro    | ESI 1 + Specialty coordination needed   |
        //   | 2        | Cardiac Patient | ESI 2 + Cardiology available           |
        //   | 3        | General Patient | ESI 3 but no specialty delay           |
        Assert.That((await GetText(page, "Trauma/Neuro Queue Position")), Is.EqualTo("1"));
        Assert.That((await GetText(page, "Trauma/Neuro Priority Reason")), Is.EqualTo("ESI 1 + Specialty coordination needed"));
        Assert.That((await GetText(page, "Cardiac Patient Queue Position")), Is.EqualTo("2"));
        Assert.That((await GetText(page, "Cardiac Patient Priority Reason")), Is.EqualTo("ESI 2 + Cardiology available"));
        Assert.That((await GetText(page, "General Patient Queue Position")), Is.EqualTo("3"));
        Assert.That((await GetText(page, "General Patient Priority Reason")), Is.EqualTo("ESI 3 but no specialty delay"));

        // And specialty teams are notified with urgency levels
        await WaitForTestId(page, "Specialty Team Notification");
    }

    [Test, Order(7)]
    [Description("Queue updates with time-sensitive treatment windows")]
    public async Task QueueUpdatesWithTimeSensitiveTreatmentWindows()
    {
        // Given several patients with time-sensitive conditions are in queue:
        //   | Patient Name    | Condition           | Treatment Window | Queue Position |
        //   | Stroke Patient  | Suspected stroke    | 4.5 hours       | 4              |
        //   | STEMI Patient   | Heart attack        | 90 minutes      | 6              |

        // When a new ESI level 1 trauma patient arrives
        await FillField(page, "ESI Level", "1");
        await page.GetByTestId("submit-patient-arrival").First.ClickAsync();

        // Then the system balances acuity with time sensitivity:
        //   | Prioritization Logic | Decision Rationale                        |
        //   | ESI 1 Trauma        | Immediate life threat - top priority      |
        //   | STEMI Patient       | Time-critical (90 min) - position 2      |
        //   | Stroke Patient      | Time-critical (4.5 hr) - position 3      |
        await WaitForTestId(page, "ESI 1 Trauma Decision Rationale");
        Assert.That((await GetText(page, "ESI 1 Trauma Decision Rationale")), Is.EqualTo("Immediate life threat - top priority"));
        Assert.That((await GetText(page, "STEMI Patient Decision Rationale")), Is.EqualTo("Time-critical (90 min) - position 2"));
        Assert.That((await GetText(page, "Stroke Patient Decision Rationale")), Is.EqualTo("Time-critical (4.5 hr) - position 3"));

        // And time-sensitive alerts are maintained:
        //   | Patient Type    | Alert Status                                  |
        //   | Stroke Patient  | 45 minutes remaining in optimal window       |
        //   | STEMI Patient   | 25 minutes remaining for door-to-balloon     |
        Assert.That((await GetText(page, "Stroke Patient Alert Status")), Is.EqualTo("45 minutes remaining in optimal window"));
        Assert.That((await GetText(page, "STEMI Patient Alert Status")), Is.EqualTo("25 minutes remaining for door-to-balloon"));

        // And the system tracks treatment deadlines for all time-sensitive cases
        await WaitForTestId(page, "Treatment Deadline Tracker");
    }

    [Test, Order(8)]
    [Description("Real-time queue visualization updates")]
    public async Task RealTimeQueueVisualizationUpdates()
    {
        // Given the ED dashboard displays the current patient queue
        // And family members can view estimated wait times on patient portal

        // When queue positions change due to new arrivals
        await page.GetByTestId("simulate-new-arrival").First.ClickAsync();

        // Then all displays update in real-time:
        //   | Display Location    | Update Type                                 |
        //   | Main ED Dashboard   | Queue positions and wait times refreshed   |
        //   | Patient Portal      | Family notifications of wait time changes  |
        //   | Mobile Apps         | Provider apps show updated patient lists   |
        //   | Waiting Room Display| General wait time estimates updated        |
        await WaitForTestId(page, "Main ED Dashboard Update Type");
        Assert.That((await GetText(page, "Main ED Dashboard Update Type")), Is.EqualTo("Queue positions and wait times refreshed"));
        Assert.That((await GetText(page, "Patient Portal Update Type")), Is.EqualTo("Family notifications of wait time changes"));
        Assert.That((await GetText(page, "Mobile Apps Update Type")), Is.EqualTo("Provider apps show updated patient lists"));
        Assert.That((await GetText(page, "Waiting Room Display Update Type")), Is.EqualTo("General wait time estimates updated"));

        // And update timestamps are shown on all displays:
        //   | Display Element     | Information Provided                        |
        //   | Last Updated        | "Queue updated at 14:35:22"                |
        //   | Next Update         | "Automatic refresh in 30 seconds"          |
        //   | Manual Refresh      | Button available for immediate update       |
        Assert.That((await GetText(page, "Last Updated")), Is.EqualTo("Queue updated at 14:35:22"));
        Assert.That((await GetText(page, "Next Update")), Is.EqualTo("Automatic refresh in 30 seconds"));
        Assert.That((await GetText(page, "Manual Refresh")), Is.EqualTo("Button available for immediate update"));

        // And change notifications highlight significant updates for staff attention
        await WaitForTestId(page, "Staff Change Notification Highlight");
    }
}
