// Selenium WebDriver + NUnit test for
// tests-with-given-when-then-features/20-code-blue-response.feature
// (equivalent to tests-with-selenium-javascript/20-code-blue-response.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see Support/Fields.cs) and the shared
// data-testid contract in Support/Login.cs (login-identity, login-submit,
// app-root).

namespace EmergencyCareSystem.SeleniumTests;

[TestFixture]
[NonParallelizable]
public class T20CodeBlueResponseTests
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
        //   And the code blue alert system is active
        //   And the resuscitation documentation module is enabled
        //   And all display devices are connected to the alert network
        //   And the code team roster is current and available
        VerifySystemIsOperational(driver);
        Login(driver, "a code blue team leader");
        // The remaining Background steps describe pre-seeded system state
        // (alert system, documentation module, display devices, and code team
        // roster readiness) assumed to already be configured in the test
        // environment.
        var featureNavLink = WaitForTestId(driver, "Nav Code Blue Response");
        featureNavLink.Click();
        WaitForTestId(driver, "Code Blue Response Panel");
    }

    [Test, Order(1)]
    [Description("Activate code blue for cardiac arrest in bed 5")]
    public void ActivateCodeBlueForCardiacArrestInBed5()
    {
        // Given a patient "Robert Martinez" is in bed "ED-5"
        // And the patient is being monitored for chest pain
        // And I am "Nurse Johnson" providing direct patient care
        // When the patient suddenly becomes unresponsive and pulseless
        // And I immediately press the code blue button at bedside
        driver.FindElement(By.CssSelector("[data-testid=\"code-blue-button\"]")).Click();

        // Then the system instantly activates the code blue alert:
        //   | Alert Component       | Activation Details                         |
        //   | Alert Timestamp       | 14:35:22 - Precise time recorded          |
        //   | Location              | ED-5 clearly identified                   |
        //   | Initiating Staff      | Nurse Johnson                             |
        //   | Patient Identity      | Robert Martinez (if available)            |
        //   | Alert Type            | Code Blue - Cardiac Arrest                |
        var alertActivationDetails = new List<Dictionary<string, string>>
        {
            Row(("label", "Alert Timestamp"), ("value", "14:35:22 - Precise time recorded")),
            Row(("label", "Location"), ("value", "ED-5 clearly identified")),
            Row(("label", "Initiating Staff"), ("value", "Nurse Johnson")),
            Row(("label", "Patient Identity"), ("value", "Robert Martinez (if available)")),
            Row(("label", "Alert Type"), ("value", "Code Blue - Cardiac Arrest")),
        };
        foreach (var rowData in alertActivationDetails)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That(GetText(driver, label), Is.EqualTo(value));
        }

        // And the code team is immediately alerted through multiple channels:
        //   | Team Member           | Alert Method          | Expected Response Time |
        //   | Emergency Physician   | Overhead page, mobile | Immediate             |
        //   | Cardiologist         | Mobile alert, pager   | Within 2 minutes      |
        //   | Anesthesiologist     | Overhead page, mobile | Within 3 minutes      |
        //   | ICU Nurse            | Mobile alert, pager   | Within 2 minutes      |
        //   | Respiratory Therapist | Overhead page, mobile | Within 2 minutes      |
        //   | Pharmacist           | Mobile alert          | Within 3 minutes      |
        //   | Chaplain             | Silent alert          | Within 5 minutes      |
        var codeTeamAlerts = driver.FindElements(By.CssSelector("[data-testid=\"code-team-alert-entry\"]"));
        Assert.That(codeTeamAlerts.Count, Is.EqualTo(7));

        // And patient location is displayed on all devices:
        //   | Display Location      | Information Shown                          |
        //   | ED Dashboard          | 🚨 CODE BLUE - BED ED-5 flashing red     |
        //   | Mobile Devices        | Push notification with location           |
        //   | Overhead Displays     | "CODE BLUE BED ED-5" prominently shown   |
        //   | Pager System          | "CODE BLUE ED-5" message                  |
        //   | Hospital Information  | Alert on all connected terminals          |
        var patientLocationDisplays = new List<Dictionary<string, string>>
        {
            Row(("label", "ED Dashboard"), ("value", "🚨 CODE BLUE - BED ED-5 flashing red")),
            Row(("label", "Mobile Devices"), ("value", "Push notification with location")),
            Row(("label", "Overhead Displays"), ("value", "\"CODE BLUE BED ED-5\" prominently shown")),
            Row(("label", "Pager System"), ("value", "\"CODE BLUE ED-5\" message")),
            Row(("label", "Hospital Information"), ("value", "Alert on all connected terminals")),
        };
        foreach (var rowData in patientLocationDisplays)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That(GetText(driver, label), Is.EqualTo(value));
        }

        // And the resuscitation documentation template opens automatically:
        //   | Documentation Section | Template Fields                            |
        //   | Event Details         | Time, location, discoverer, initial rhythm|
        //   | Timeline Tracker      | Medication times, defibrillation, procedures|
        //   | Team Members          | Roles and arrival times                    |
        //   | Vital Signs           | Real-time monitoring integration           |
        //   | Interventions         | CPR quality, airway management, IV access |
        var documentationTemplateSections = new List<Dictionary<string, string>>
        {
            Row(("label", "Event Details"), ("value", "Time, location, discoverer, initial rhythm")),
            Row(("label", "Timeline Tracker"), ("value", "Medication times, defibrillation, procedures")),
            Row(("label", "Team Members"), ("value", "Roles and arrival times")),
            Row(("label", "Vital Signs"), ("value", "Real-time monitoring integration")),
            Row(("label", "Interventions"), ("value", "CPR quality, airway management, IV access")),
        };
        foreach (var rowData in documentationTemplateSections)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That(GetText(driver, label), Is.EqualTo(value));
        }
    }

    [Test, Order(2)]
    [Description("Code blue response with automatic equipment alerts")]
    public void CodeBlueResponseWithAutomaticEquipmentAlerts()
    {
        // Given a code blue has been activated in bed "ED-5"
        // When the code blue alert is triggered

        // Then emergency equipment alerts are automatically generated:
        //   | Equipment Type        | Alert Message                              |
        //   | Crash Cart            | Crash cart dispatch to ED-5               |
        //   | Defibrillator        | AED/Manual defibrillator to ED-5          |
        //   | Airway Equipment     | Intubation kit and ventilator to ED-5     |
        //   | Emergency Medications | Code blue medication box to ED-5          |
        //   | IV Access Supplies   | Central line kit and fluids to ED-5       |
        var equipmentAlerts = new List<Dictionary<string, string>>
        {
            Row(("label", "Crash Cart"), ("value", "Crash cart dispatch to ED-5")),
            Row(("label", "Defibrillator"), ("value", "AED/Manual defibrillator to ED-5")),
            Row(("label", "Airway Equipment"), ("value", "Intubation kit and ventilator to ED-5")),
            Row(("label", "Emergency Medications"), ("value", "Code blue medication box to ED-5")),
            Row(("label", "IV Access Supplies"), ("value", "Central line kit and fluids to ED-5")),
        };
        foreach (var rowData in equipmentAlerts)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That(GetText(driver, label), Is.EqualTo(value));
        }

        // And equipment tracking is initiated:
        //   | Equipment Item        | Status Tracking                            |
        //   | Crash Cart Location   | GPS tracking to bed ED-5                  |
        //   | Defibrillator Readiness| Battery level and functionality check    |
        //   | Medication Expiration | Code blue drugs expiration verification   |
        //   | Equipment Arrival     | Timestamp when equipment reaches bedside  |
        var equipmentTracking = new List<Dictionary<string, string>>
        {
            Row(("label", "Crash Cart Location"), ("value", "GPS tracking to bed ED-5")),
            Row(("label", "Defibrillator Readiness"), ("value", "Battery level and functionality check")),
            Row(("label", "Medication Expiration"), ("value", "Code blue drugs expiration verification")),
            Row(("label", "Equipment Arrival"), ("value", "Timestamp when equipment reaches bedside")),
        };
        foreach (var rowData in equipmentTracking)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That(GetText(driver, label), Is.EqualTo(value));
        }

        // And backup equipment is automatically prepared:
        //   | Backup Equipment      | Preparation Action                         |
        //   | Secondary Crash Cart  | Made ready for potential second code       |
        //   | Additional Ventilator | Checked and moved closer to ED            |
        //   | Blood Bank Alert      | Emergency blood products prepared          |
        //   | OR Notification       | Operating room placed on standby          |
        var backupEquipment = new List<Dictionary<string, string>>
        {
            Row(("label", "Secondary Crash Cart"), ("value", "Made ready for potential second code")),
            Row(("label", "Additional Ventilator"), ("value", "Checked and moved closer to ED")),
            Row(("label", "Blood Bank Alert"), ("value", "Emergency blood products prepared")),
            Row(("label", "OR Notification"), ("value", "Operating room placed on standby")),
        };
        foreach (var rowData in backupEquipment)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That(GetText(driver, label), Is.EqualTo(value));
        }
    }

    [Test, Order(3)]
    [Description("Real-time code blue documentation during resuscitation")]
    public void RealTimeCodeBlueDocumentationDuringResuscitation()
    {
        // Given a code blue is in progress in bed "ED-5"
        // And the resuscitation documentation template is open
        // And "Dr. Smith" is the code team leader
        // When resuscitation interventions are performed

        // Then real-time documentation captures all activities:
        //   | Intervention Type     | Documentation Fields                       |
        //   | CPR Administration    | Start time, compression quality, provider  |
        //   | Medication Given      | Drug name, dose, route, time, provider     |
        //   | Defibrillation       | Joules delivered, rhythm before/after     |
        //   | Airway Management    | Type of airway, success, provider         |
        //   | IV Access            | Location, size, number of attempts        |
        var documentedActivities = new List<Dictionary<string, string>>
        {
            Row(("label", "CPR Administration"), ("value", "Start time, compression quality, provider")),
            Row(("label", "Medication Given"), ("value", "Drug name, dose, route, time, provider")),
            Row(("label", "Defibrillation"), ("value", "Joules delivered, rhythm before/after")),
            Row(("label", "Airway Management"), ("value", "Type of airway, success, provider")),
            Row(("label", "IV Access"), ("value", "Location, size, number of attempts")),
        };
        foreach (var rowData in documentedActivities)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That(GetText(driver, label), Is.EqualTo(value));
        }

        // And timeline tracking maintains precise chronology:
        //   | Timeline Entry        | Automatic Capture                          |
        //   | Event Start           | 14:35:22 - Code blue activated            |
        //   | CPR Initiated         | 14:35:45 - CPR started by Nurse Johnson   |
        //   | Team Leader Arrival   | 14:36:15 - Dr. Smith assumes leadership   |
        //   | First Medication      | 14:37:30 - Epinephrine 1mg IV push       |
        //   | Defibrillation       | 14:38:45 - 200J biphasic shock delivered  |
        var timelineEntries = new List<Dictionary<string, string>>
        {
            Row(("label", "Event Start"), ("value", "14:35:22 - Code blue activated")),
            Row(("label", "CPR Initiated"), ("value", "14:35:45 - CPR started by Nurse Johnson")),
            Row(("label", "Team Leader Arrival"), ("value", "14:36:15 - Dr. Smith assumes leadership")),
            Row(("label", "First Medication"), ("value", "14:37:30 - Epinephrine 1mg IV push")),
            Row(("label", "Defibrillation Timeline"), ("value", "14:38:45 - 200J biphasic shock delivered")),
        };
        foreach (var rowData in timelineEntries)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That(GetText(driver, label), Is.EqualTo(value));
        }

        // And quality metrics are tracked in real-time:
        //   | Quality Metric        | Real-time Monitoring                       |
        //   | Compression Depth     | CPR feedback device integration           |
        //   | Compression Rate      | Metronome guidance and measurement        |
        //   | No-flow Time         | Automatic calculation of interruptions    |
        //   | Medication Timing     | Alert for time-critical drug intervals   |
        var qualityMetrics = new List<Dictionary<string, string>>
        {
            Row(("label", "Compression Depth"), ("value", "CPR feedback device integration")),
            Row(("label", "Compression Rate"), ("value", "Metronome guidance and measurement")),
            Row(("label", "No-flow Time"), ("value", "Automatic calculation of interruptions")),
            Row(("label", "Medication Timing"), ("value", "Alert for time-critical drug intervals")),
        };
        foreach (var rowData in qualityMetrics)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That(GetText(driver, label), Is.EqualTo(value));
        }
    }

    [Test, Order(4)]
    [Description("Code blue with return of spontaneous circulation (ROSC)")]
    public void CodeBlueWithReturnOfSpontaneousCirculationROSC()
    {
        // Given a code blue has been in progress for 8 minutes
        // And resuscitation efforts are ongoing with documentation active
        // When the patient achieves return of spontaneous circulation (ROSC)
        // And "Dr. Smith" confirms pulse and blood pressure of 110/70

        // Then the system updates the code status:
        //   | Status Update         | Documentation Changes                      |
        //   | ROSC Achievement      | Time: 14:43:15 - ROSC achieved            |
        //   | Vital Signs          | BP: 110/70, HR: 85, documented           |
        //   | Rhythm Change        | Normal sinus rhythm confirmed             |
        //   | Intervention Pause   | CPR discontinued, monitoring intensified  |
        var codeStatusUpdates = new List<Dictionary<string, string>>
        {
            Row(("label", "ROSC Achievement"), ("value", "Time: 14:43:15 - ROSC achieved")),
            Row(("label", "Post-ROSC Vital Signs"), ("value", "BP: 110/70, HR: 85, documented")),
            Row(("label", "Rhythm Change"), ("value", "Normal sinus rhythm confirmed")),
            Row(("label", "Intervention Pause"), ("value", "CPR discontinued, monitoring intensified")),
        };
        foreach (var rowData in codeStatusUpdates)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That(GetText(driver, label), Is.EqualTo(value));
        }

        // And post-ROSC care protocols are activated:
        //   | Post-ROSC Protocol    | Automated Alerts                           |
        //   | ICU Transfer          | ICU bed request and transport coordination |
        //   | Cardiology Consult    | Urgent cardiology evaluation requested     |
        //   | Temperature Management| Therapeutic hypothermia consideration      |
        //   | Neurological Assessment| Baseline neuro checks ordered             |
        var postRoscProtocols = new List<Dictionary<string, string>>
        {
            Row(("label", "ICU Transfer"), ("value", "ICU bed request and transport coordination")),
            Row(("label", "Cardiology Consult"), ("value", "Urgent cardiology evaluation requested")),
            Row(("label", "Temperature Management"), ("value", "Therapeutic hypothermia consideration")),
            Row(("label", "Neurological Assessment"), ("value", "Baseline neuro checks ordered")),
        };
        foreach (var rowData in postRoscProtocols)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That(GetText(driver, label), Is.EqualTo(value));
        }

        // And family notification procedures are initiated:
        //   | Family Communication  | Process                                    |
        //   | Contact Attempt       | Emergency contact called by social worker  |
        //   | Status Update         | "Patient being treated, stable condition" |
        //   | Visitation Arrangement| Family arrival and bedside visit coordination|
        //   | Chaplain Services     | Spiritual care offered to family          |
        var familyNotificationProcedures = new List<Dictionary<string, string>>
        {
            Row(("label", "Contact Attempt"), ("value", "Emergency contact called by social worker")),
            Row(("label", "Status Update"), ("value", "\"Patient being treated, stable condition\"")),
            Row(("label", "Visitation Arrangement"), ("value", "Family arrival and bedside visit coordination")),
            Row(("label", "Chaplain Services"), ("value", "Spiritual care offered to family")),
        };
        foreach (var rowData in familyNotificationProcedures)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That(GetText(driver, label), Is.EqualTo(value));
        }
    }

    [Test, Order(5)]
    [Description("Unsuccessful code blue with transition to end-of-life care")]
    public void UnsuccessfulCodeBlueWithTransitionToEndOfLifeCare()
    {
        // Given a code blue has been in progress for 25 minutes
        // And multiple rounds of medications and defibrillation have been attempted
        // And no return of spontaneous circulation has been achieved
        // When "Dr. Smith" as code team leader determines resuscitation efforts should cease
        // And the time of death is called at 15:00:15

        // Then the system handles end-of-life documentation:
        //   | End-of-Life Process   | Documentation Requirements                 |
        //   | Time of Death         | 15:00:15 - Officially recorded           |
        //   | Resuscitation Duration| 24 minutes 53 seconds total time         |
        //   | Interventions Summary | Complete list of all attempted treatments |
        //   | Team Members Present  | All providers involved in resuscitation   |
        var endOfLifeDocumentation = new List<Dictionary<string, string>>
        {
            Row(("label", "Time of Death"), ("value", "15:00:15 - Officially recorded")),
            Row(("label", "Resuscitation Duration"), ("value", "24 minutes 53 seconds total time")),
            Row(("label", "Interventions Summary"), ("value", "Complete list of all attempted treatments")),
            Row(("label", "Team Members Present"), ("value", "All providers involved in resuscitation")),
        };
        foreach (var rowData in endOfLifeDocumentation)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That(GetText(driver, label), Is.EqualTo(value));
        }

        // And family notification and support procedures are activated:
        //   | Family Support        | Coordinated Response                       |
        //   | Immediate Contact     | Personal notification by physician        |
        //   | Bereavement Support   | Chaplain and social worker assigned       |
        //   | Viewing Arrangement   | Private room prepared for family viewing   |
        //   | Organ Donation        | Coordinator contacted per protocol        |
        var familySupportProcedures = new List<Dictionary<string, string>>
        {
            Row(("label", "Immediate Contact"), ("value", "Personal notification by physician")),
            Row(("label", "Bereavement Support"), ("value", "Chaplain and social worker assigned")),
            Row(("label", "Viewing Arrangement"), ("value", "Private room prepared for family viewing")),
            Row(("label", "Organ Donation"), ("value", "Coordinator contacted per protocol")),
        };
        foreach (var rowData in familySupportProcedures)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That(GetText(driver, label), Is.EqualTo(value));
        }

        // And administrative processes are initiated:
        //   | Administrative Task   | Required Actions                           |
        //   | Medical Examiner      | Contact if death meets criteria           |
        //   | Autopsy Consent       | Family discussion and documentation       |
        //   | Death Certificate     | Physician completion requirements         |
        //   | Quality Review        | Case review scheduled within 24 hours     |
        var administrativeProcesses = new List<Dictionary<string, string>>
        {
            Row(("label", "Medical Examiner"), ("value", "Contact if death meets criteria")),
            Row(("label", "Autopsy Consent"), ("value", "Family discussion and documentation")),
            Row(("label", "Death Certificate"), ("value", "Physician completion requirements")),
            Row(("label", "Quality Review"), ("value", "Case review scheduled within 24 hours")),
        };
        foreach (var rowData in administrativeProcesses)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That(GetText(driver, label), Is.EqualTo(value));
        }
    }

    [Test, Order(6)]
    [Description("Code blue during visitor hours with family present")]
    public void CodeBlueDuringVisitorHoursWithFamilyPresent()
    {
        // Given it is 19:30 during evening visitor hours
        // And the patient's family members are at bedside when cardiac arrest occurs
        // When the code blue is activated

        // Then family management protocols are immediately implemented:
        //   | Family Management     | Immediate Actions                          |
        //   | Family Escort         | Security escorts family to private area    |
        //   | Communication         | Social worker provides immediate support   |
        //   | Information Updates   | Regular updates provided during resuscitation|
        //   | Chaplain Services     | Spiritual care offered immediately         |
        var familyManagementProtocols = new List<Dictionary<string, string>>
        {
            Row(("label", "Family Escort"), ("value", "Security escorts family to private area")),
            Row(("label", "Communication"), ("value", "Social worker provides immediate support")),
            Row(("label", "Information Updates"), ("value", "Regular updates provided during resuscitation")),
            Row(("label", "Chaplain Services Immediate Support"), ("value", "Spiritual care offered immediately")),
        };
        foreach (var rowData in familyManagementProtocols)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That(GetText(driver, label), Is.EqualTo(value));
        }

        // And visitor area management is coordinated:
        //   | Visitor Control       | Safety Measures                            |
        //   | Area Clearance        | Non-family visitors moved from immediate area|
        //   | Privacy Protection    | Screens and barriers deployed             |
        //   | Crowd Control         | Security manages visitor flow             |
        //   | Other Patient Care    | Continued care for nearby patients        |
        var visitorAreaManagement = new List<Dictionary<string, string>>
        {
            Row(("label", "Area Clearance"), ("value", "Non-family visitors moved from immediate area")),
            Row(("label", "Privacy Protection"), ("value", "Screens and barriers deployed")),
            Row(("label", "Crowd Control"), ("value", "Security manages visitor flow")),
            Row(("label", "Other Patient Care"), ("value", "Continued care for nearby patients")),
        };
        foreach (var rowData in visitorAreaManagement)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That(GetText(driver, label), Is.EqualTo(value));
        }

        // And family preference accommodation occurs:
        //   | Family Preference     | Options Provided                           |
        //   | Bedside Presence      | Option to remain during resuscitation     |
        //   | Waiting Area          | Comfortable private space with updates    |
        //   | Family Spokesperson   | Designated family member for communication |
        //   | Support Person        | Additional family/friend notification     |
        var familyPreferenceAccommodation = new List<Dictionary<string, string>>
        {
            Row(("label", "Bedside Presence"), ("value", "Option to remain during resuscitation")),
            Row(("label", "Waiting Area"), ("value", "Comfortable private space with updates")),
            Row(("label", "Family Spokesperson"), ("value", "Designated family member for communication")),
            Row(("label", "Support Person"), ("value", "Additional family/friend notification")),
        };
        foreach (var rowData in familyPreferenceAccommodation)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That(GetText(driver, label), Is.EqualTo(value));
        }
    }

    [Test, Order(7)]
    [Description("Code blue team performance metrics and quality improvement")]
    public void CodeBlueTeamPerformanceMetricsAndQualityImprovement()
    {
        // Given a code blue event has been completed
        // And all documentation has been finalized
        // When the quality review process is initiated

        // Then performance metrics are automatically calculated:
        //   | Performance Metric    | Measurement                                |
        //   | Response Time         | 1 minute 23 seconds from alert to arrival |
        //   | No-flow Time          | 15 seconds total interruption time        |
        //   | First Shock Time      | 3 minutes 45 seconds from arrest          |
        //   | Medication Timing     | All drugs given within target windows     |
        //   | Team Coordination     | Communication effectiveness score          |
        var performanceMetrics = new List<Dictionary<string, string>>
        {
            Row(("label", "Response Time"), ("value", "1 minute 23 seconds from alert to arrival")),
            Row(("label", "No-flow Time Performance"), ("value", "15 seconds total interruption time")),
            Row(("label", "First Shock Time"), ("value", "3 minutes 45 seconds from arrest")),
            Row(("label", "Medication Timing Compliance"), ("value", "All drugs given within target windows")),
            Row(("label", "Team Coordination"), ("value", "Communication effectiveness score")),
        };
        foreach (var rowData in performanceMetrics)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That(GetText(driver, label), Is.EqualTo(value));
        }

        // And quality improvement data is captured:
        //   | QI Data Element       | Assessment                                 |
        //   | Protocol Adherence    | 95% compliance with ACLS guidelines       |
        //   | Equipment Function    | All equipment functioned properly         |
        //   | Team Performance      | Effective leadership and role clarity     |
        //   | Communication Quality | Clear, concise, and timely communication  |
        var qiDataElements = new List<Dictionary<string, string>>
        {
            Row(("label", "Protocol Adherence"), ("value", "95% compliance with ACLS guidelines")),
            Row(("label", "Equipment Function"), ("value", "All equipment functioned properly")),
            Row(("label", "Team Performance"), ("value", "Effective leadership and role clarity")),
            Row(("label", "Communication Quality"), ("value", "Clear, concise, and timely communication")),
        };
        foreach (var rowData in qiDataElements)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That(GetText(driver, label), Is.EqualTo(value));
        }

        // And improvement opportunities are identified:
        //   | Improvement Area      | Recommendation                             |
        //   | Response Time         | Consider additional code cart placement    |
        //   | Team Training         | Schedule quarterly simulation training     |
        //   | Equipment Maintenance | Review defibrillator calibration schedule |
        //   | Documentation         | Streamline real-time entry process        |
        var improvementOpportunities = new List<Dictionary<string, string>>
        {
            Row(("label", "Response Time Recommendation"), ("value", "Consider additional code cart placement")),
            Row(("label", "Team Training"), ("value", "Schedule quarterly simulation training")),
            Row(("label", "Equipment Maintenance"), ("value", "Review defibrillator calibration schedule")),
            Row(("label", "Documentation"), ("value", "Streamline real-time entry process")),
        };
        foreach (var rowData in improvementOpportunities)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That(GetText(driver, label), Is.EqualTo(value));
        }
    }

    [Test, Order(8)]
    [Description("Code blue false alarm with appropriate system response")]
    public void CodeBlueFalseAlarmWithAppropriateSystemResponse()
    {
        // Given a code blue alert has been activated in bed "ED-5"
        // And the code team is responding
        // When it is determined that the patient is conscious and stable
        // And the alert was triggered accidentally by equipment malfunction

        // Then the false alarm protocol is activated:
        //   | False Alarm Response  | Actions Taken                              |
        //   | Alert Cancellation    | "Code blue canceled - false alarm" announcement|
        //   | Team Stand-down       | Code team notified to return to normal duties|
        //   | Equipment Check       | Investigate and repair malfunctioning equipment|
        //   | Documentation         | Document false alarm and cause            |
        var falseAlarmProtocol = new List<Dictionary<string, string>>
        {
            Row(("label", "Alert Cancellation"), ("value", "\"Code blue canceled - false alarm\" announcement")),
            Row(("label", "Team Stand-down"), ("value", "Code team notified to return to normal duties")),
            Row(("label", "Equipment Check"), ("value", "Investigate and repair malfunctioning equipment")),
            Row(("label", "False Alarm Documentation"), ("value", "Document false alarm and cause")),
        };
        foreach (var rowData in falseAlarmProtocol)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That(GetText(driver, label), Is.EqualTo(value));
        }

        // And system improvements are implemented:
        //   | Improvement Action    | Preventive Measures                        |
        //   | Equipment Maintenance | Immediate repair of faulty equipment       |
        //   | Staff Education       | Review proper code blue activation         |
        //   | System Calibration    | Adjust sensitivity to prevent false alarms |
        //   | Audit Trail          | Record incident for system improvement     |
        var systemImprovements = new List<Dictionary<string, string>>
        {
            Row(("label", "Equipment Maintenance Repair"), ("value", "Immediate repair of faulty equipment")),
            Row(("label", "Staff Education"), ("value", "Review proper code blue activation")),
            Row(("label", "System Calibration"), ("value", "Adjust sensitivity to prevent false alarms")),
            Row(("label", "Audit Trail"), ("value", "Record incident for system improvement")),
        };
        foreach (var rowData in systemImprovements)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That(GetText(driver, label), Is.EqualTo(value));
        }

        // And normal operations resume with lessons learned integrated into protocols
        Assert.That(GetText(driver, "System Operations Status"), Does.Match(@"normal").IgnoreCase);
    }
}
