// Selenium WebDriver + JUnit 5 test for
// tests-with-given-when-then-features/20-code-blue-response.feature
// (equivalent to tests-with-selenium-javascript/20-code-blue-response.test.js).
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see support/Fields.java) and the shared
// data-testid contract in support/Login.java (login-identity, login-submit,
// app-root).

package emergencycaresystem;

import static emergencycaresystem.support.Config.BASE_URL;
import static emergencycaresystem.support.Fields.*;
import static emergencycaresystem.support.Login.*;
import static emergencycaresystem.support.Matchers.assertMatches;
import static org.junit.jupiter.api.Assertions.*;

import java.util.List;
import java.util.Map;
import emergencycaresystem.support.DriverFactory;
import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.WebElement;
import org.junit.jupiter.api.AfterAll;
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.MethodOrderer;
import org.junit.jupiter.api.Order;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.TestMethodOrder;

@TestMethodOrder(MethodOrderer.OrderAnnotation.class)
class T20CodeBlueResponseTest {
    private static WebDriver driver;

    @BeforeAll
    static void setUpClass() {
        driver = DriverFactory.build();
    }

    @AfterAll
    static void tearDownClass() {
        driver.quit();
    }

    @BeforeEach
    void setUp() {
        // Background:
        //   Given the emergency care system is operational
        //   And the code blue alert system is active
        //   And the resuscitation documentation module is enabled
        //   And all display devices are connected to the alert network
        //   And the code team roster is current and available
        verifySystemIsOperational(driver);
        login(driver, "a code blue team leader");
        // The remaining Background steps describe pre-seeded system state
        // (alert system, documentation module, display devices, and code team
        // roster readiness) assumed to already be configured in the test
        // environment.
        var featureNavLink = waitForTestId(driver, "Nav Code Blue Response");
        featureNavLink.click();
        waitForTestId(driver, "Code Blue Response Panel");
    }

    @Test
    @Order(1)
    @DisplayName("Activate code blue for cardiac arrest in bed 5")
    void activateCodeBlueForCardiacArrestInBed5() {
        // Given a patient "Robert Martinez" is in bed "ED-5"
        // And the patient is being monitored for chest pain
        // And I am "Nurse Johnson" providing direct patient care
        // When the patient suddenly becomes unresponsive and pulseless
        // And I immediately press the code blue button at bedside
        driver.findElement(By.cssSelector("[data-testid=\"code-blue-button\"]")).click();

        // Then the system instantly activates the code blue alert:
        //   | Alert Component       | Activation Details                         |
        //   | Alert Timestamp       | 14:35:22 - Precise time recorded          |
        //   | Location              | ED-5 clearly identified                   |
        //   | Initiating Staff      | Nurse Johnson                             |
        //   | Patient Identity      | Robert Martinez (if available)            |
        //   | Alert Type            | Code Blue - Cardiac Arrest                |
        List<Map<String, String>> alertActivationDetails = List.of(
            Map.of("label", "Alert Timestamp", "value", "14:35:22 - Precise time recorded"),
            Map.of("label", "Location", "value", "ED-5 clearly identified"),
            Map.of("label", "Initiating Staff", "value", "Nurse Johnson"),
            Map.of("label", "Patient Identity", "value", "Robert Martinez (if available)"),
            Map.of("label", "Alert Type", "value", "Code Blue - Cardiac Arrest")
        );
        for (var rowData : alertActivationDetails) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(driver, label));
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
        var codeTeamAlerts = driver.findElements(By.cssSelector("[data-testid=\"code-team-alert-entry\"]"));
        assertEquals(7, codeTeamAlerts.size());

        // And patient location is displayed on all devices:
        //   | Display Location      | Information Shown                          |
        //   | ED Dashboard          | 🚨 CODE BLUE - BED ED-5 flashing red     |
        //   | Mobile Devices        | Push notification with location           |
        //   | Overhead Displays     | "CODE BLUE BED ED-5" prominently shown   |
        //   | Pager System          | "CODE BLUE ED-5" message                  |
        //   | Hospital Information  | Alert on all connected terminals          |
        List<Map<String, String>> patientLocationDisplays = List.of(
            Map.of("label", "ED Dashboard", "value", "🚨 CODE BLUE - BED ED-5 flashing red"),
            Map.of("label", "Mobile Devices", "value", "Push notification with location"),
            Map.of("label", "Overhead Displays", "value", "\"CODE BLUE BED ED-5\" prominently shown"),
            Map.of("label", "Pager System", "value", "\"CODE BLUE ED-5\" message"),
            Map.of("label", "Hospital Information", "value", "Alert on all connected terminals")
        );
        for (var rowData : patientLocationDisplays) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(driver, label));
        }

        // And the resuscitation documentation template opens automatically:
        //   | Documentation Section | Template Fields                            |
        //   | Event Details         | Time, location, discoverer, initial rhythm|
        //   | Timeline Tracker      | Medication times, defibrillation, procedures|
        //   | Team Members          | Roles and arrival times                    |
        //   | Vital Signs           | Real-time monitoring integration           |
        //   | Interventions         | CPR quality, airway management, IV access |
        List<Map<String, String>> documentationTemplateSections = List.of(
            Map.of("label", "Event Details", "value", "Time, location, discoverer, initial rhythm"),
            Map.of("label", "Timeline Tracker", "value", "Medication times, defibrillation, procedures"),
            Map.of("label", "Team Members", "value", "Roles and arrival times"),
            Map.of("label", "Vital Signs", "value", "Real-time monitoring integration"),
            Map.of("label", "Interventions", "value", "CPR quality, airway management, IV access")
        );
        for (var rowData : documentationTemplateSections) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(driver, label));
        }
    }

    @Test
    @Order(2)
    @DisplayName("Code blue response with automatic equipment alerts")
    void codeBlueResponseWithAutomaticEquipmentAlerts() {
        // Given a code blue has been activated in bed "ED-5"
        // When the code blue alert is triggered

        // Then emergency equipment alerts are automatically generated:
        //   | Equipment Type        | Alert Message                              |
        //   | Crash Cart            | Crash cart dispatch to ED-5               |
        //   | Defibrillator        | AED/Manual defibrillator to ED-5          |
        //   | Airway Equipment     | Intubation kit and ventilator to ED-5     |
        //   | Emergency Medications | Code blue medication box to ED-5          |
        //   | IV Access Supplies   | Central line kit and fluids to ED-5       |
        List<Map<String, String>> equipmentAlerts = List.of(
            Map.of("label", "Crash Cart", "value", "Crash cart dispatch to ED-5"),
            Map.of("label", "Defibrillator", "value", "AED/Manual defibrillator to ED-5"),
            Map.of("label", "Airway Equipment", "value", "Intubation kit and ventilator to ED-5"),
            Map.of("label", "Emergency Medications", "value", "Code blue medication box to ED-5"),
            Map.of("label", "IV Access Supplies", "value", "Central line kit and fluids to ED-5")
        );
        for (var rowData : equipmentAlerts) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(driver, label));
        }

        // And equipment tracking is initiated:
        //   | Equipment Item        | Status Tracking                            |
        //   | Crash Cart Location   | GPS tracking to bed ED-5                  |
        //   | Defibrillator Readiness| Battery level and functionality check    |
        //   | Medication Expiration | Code blue drugs expiration verification   |
        //   | Equipment Arrival     | Timestamp when equipment reaches bedside  |
        List<Map<String, String>> equipmentTracking = List.of(
            Map.of("label", "Crash Cart Location", "value", "GPS tracking to bed ED-5"),
            Map.of("label", "Defibrillator Readiness", "value", "Battery level and functionality check"),
            Map.of("label", "Medication Expiration", "value", "Code blue drugs expiration verification"),
            Map.of("label", "Equipment Arrival", "value", "Timestamp when equipment reaches bedside")
        );
        for (var rowData : equipmentTracking) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(driver, label));
        }

        // And backup equipment is automatically prepared:
        //   | Backup Equipment      | Preparation Action                         |
        //   | Secondary Crash Cart  | Made ready for potential second code       |
        //   | Additional Ventilator | Checked and moved closer to ED            |
        //   | Blood Bank Alert      | Emergency blood products prepared          |
        //   | OR Notification       | Operating room placed on standby          |
        List<Map<String, String>> backupEquipment = List.of(
            Map.of("label", "Secondary Crash Cart", "value", "Made ready for potential second code"),
            Map.of("label", "Additional Ventilator", "value", "Checked and moved closer to ED"),
            Map.of("label", "Blood Bank Alert", "value", "Emergency blood products prepared"),
            Map.of("label", "OR Notification", "value", "Operating room placed on standby")
        );
        for (var rowData : backupEquipment) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(driver, label));
        }
    }

    @Test
    @Order(3)
    @DisplayName("Real-time code blue documentation during resuscitation")
    void realTimeCodeBlueDocumentationDuringResuscitation() {
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
        List<Map<String, String>> documentedActivities = List.of(
            Map.of("label", "CPR Administration", "value", "Start time, compression quality, provider"),
            Map.of("label", "Medication Given", "value", "Drug name, dose, route, time, provider"),
            Map.of("label", "Defibrillation", "value", "Joules delivered, rhythm before/after"),
            Map.of("label", "Airway Management", "value", "Type of airway, success, provider"),
            Map.of("label", "IV Access", "value", "Location, size, number of attempts")
        );
        for (var rowData : documentedActivities) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(driver, label));
        }

        // And timeline tracking maintains precise chronology:
        //   | Timeline Entry        | Automatic Capture                          |
        //   | Event Start           | 14:35:22 - Code blue activated            |
        //   | CPR Initiated         | 14:35:45 - CPR started by Nurse Johnson   |
        //   | Team Leader Arrival   | 14:36:15 - Dr. Smith assumes leadership   |
        //   | First Medication      | 14:37:30 - Epinephrine 1mg IV push       |
        //   | Defibrillation       | 14:38:45 - 200J biphasic shock delivered  |
        List<Map<String, String>> timelineEntries = List.of(
            Map.of("label", "Event Start", "value", "14:35:22 - Code blue activated"),
            Map.of("label", "CPR Initiated", "value", "14:35:45 - CPR started by Nurse Johnson"),
            Map.of("label", "Team Leader Arrival", "value", "14:36:15 - Dr. Smith assumes leadership"),
            Map.of("label", "First Medication", "value", "14:37:30 - Epinephrine 1mg IV push"),
            Map.of("label", "Defibrillation Timeline", "value", "14:38:45 - 200J biphasic shock delivered")
        );
        for (var rowData : timelineEntries) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(driver, label));
        }

        // And quality metrics are tracked in real-time:
        //   | Quality Metric        | Real-time Monitoring                       |
        //   | Compression Depth     | CPR feedback device integration           |
        //   | Compression Rate      | Metronome guidance and measurement        |
        //   | No-flow Time         | Automatic calculation of interruptions    |
        //   | Medication Timing     | Alert for time-critical drug intervals   |
        List<Map<String, String>> qualityMetrics = List.of(
            Map.of("label", "Compression Depth", "value", "CPR feedback device integration"),
            Map.of("label", "Compression Rate", "value", "Metronome guidance and measurement"),
            Map.of("label", "No-flow Time", "value", "Automatic calculation of interruptions"),
            Map.of("label", "Medication Timing", "value", "Alert for time-critical drug intervals")
        );
        for (var rowData : qualityMetrics) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(driver, label));
        }
    }

    @Test
    @Order(4)
    @DisplayName("Code blue with return of spontaneous circulation (ROSC)")
    void codeBlueWithReturnOfSpontaneousCirculationROSC() {
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
        List<Map<String, String>> codeStatusUpdates = List.of(
            Map.of("label", "ROSC Achievement", "value", "Time: 14:43:15 - ROSC achieved"),
            Map.of("label", "Post-ROSC Vital Signs", "value", "BP: 110/70, HR: 85, documented"),
            Map.of("label", "Rhythm Change", "value", "Normal sinus rhythm confirmed"),
            Map.of("label", "Intervention Pause", "value", "CPR discontinued, monitoring intensified")
        );
        for (var rowData : codeStatusUpdates) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(driver, label));
        }

        // And post-ROSC care protocols are activated:
        //   | Post-ROSC Protocol    | Automated Alerts                           |
        //   | ICU Transfer          | ICU bed request and transport coordination |
        //   | Cardiology Consult    | Urgent cardiology evaluation requested     |
        //   | Temperature Management| Therapeutic hypothermia consideration      |
        //   | Neurological Assessment| Baseline neuro checks ordered             |
        List<Map<String, String>> postRoscProtocols = List.of(
            Map.of("label", "ICU Transfer", "value", "ICU bed request and transport coordination"),
            Map.of("label", "Cardiology Consult", "value", "Urgent cardiology evaluation requested"),
            Map.of("label", "Temperature Management", "value", "Therapeutic hypothermia consideration"),
            Map.of("label", "Neurological Assessment", "value", "Baseline neuro checks ordered")
        );
        for (var rowData : postRoscProtocols) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(driver, label));
        }

        // And family notification procedures are initiated:
        //   | Family Communication  | Process                                    |
        //   | Contact Attempt       | Emergency contact called by social worker  |
        //   | Status Update         | "Patient being treated, stable condition" |
        //   | Visitation Arrangement| Family arrival and bedside visit coordination|
        //   | Chaplain Services     | Spiritual care offered to family          |
        List<Map<String, String>> familyNotificationProcedures = List.of(
            Map.of("label", "Contact Attempt", "value", "Emergency contact called by social worker"),
            Map.of("label", "Status Update", "value", "\"Patient being treated, stable condition\""),
            Map.of("label", "Visitation Arrangement", "value", "Family arrival and bedside visit coordination"),
            Map.of("label", "Chaplain Services", "value", "Spiritual care offered to family")
        );
        for (var rowData : familyNotificationProcedures) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(driver, label));
        }
    }

    @Test
    @Order(5)
    @DisplayName("Unsuccessful code blue with transition to end-of-life care")
    void unsuccessfulCodeBlueWithTransitionToEndOfLifeCare() {
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
        List<Map<String, String>> endOfLifeDocumentation = List.of(
            Map.of("label", "Time of Death", "value", "15:00:15 - Officially recorded"),
            Map.of("label", "Resuscitation Duration", "value", "24 minutes 53 seconds total time"),
            Map.of("label", "Interventions Summary", "value", "Complete list of all attempted treatments"),
            Map.of("label", "Team Members Present", "value", "All providers involved in resuscitation")
        );
        for (var rowData : endOfLifeDocumentation) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(driver, label));
        }

        // And family notification and support procedures are activated:
        //   | Family Support        | Coordinated Response                       |
        //   | Immediate Contact     | Personal notification by physician        |
        //   | Bereavement Support   | Chaplain and social worker assigned       |
        //   | Viewing Arrangement   | Private room prepared for family viewing   |
        //   | Organ Donation        | Coordinator contacted per protocol        |
        List<Map<String, String>> familySupportProcedures = List.of(
            Map.of("label", "Immediate Contact", "value", "Personal notification by physician"),
            Map.of("label", "Bereavement Support", "value", "Chaplain and social worker assigned"),
            Map.of("label", "Viewing Arrangement", "value", "Private room prepared for family viewing"),
            Map.of("label", "Organ Donation", "value", "Coordinator contacted per protocol")
        );
        for (var rowData : familySupportProcedures) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(driver, label));
        }

        // And administrative processes are initiated:
        //   | Administrative Task   | Required Actions                           |
        //   | Medical Examiner      | Contact if death meets criteria           |
        //   | Autopsy Consent       | Family discussion and documentation       |
        //   | Death Certificate     | Physician completion requirements         |
        //   | Quality Review        | Case review scheduled within 24 hours     |
        List<Map<String, String>> administrativeProcesses = List.of(
            Map.of("label", "Medical Examiner", "value", "Contact if death meets criteria"),
            Map.of("label", "Autopsy Consent", "value", "Family discussion and documentation"),
            Map.of("label", "Death Certificate", "value", "Physician completion requirements"),
            Map.of("label", "Quality Review", "value", "Case review scheduled within 24 hours")
        );
        for (var rowData : administrativeProcesses) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(driver, label));
        }
    }

    @Test
    @Order(6)
    @DisplayName("Code blue during visitor hours with family present")
    void codeBlueDuringVisitorHoursWithFamilyPresent() {
        // Given it is 19:30 during evening visitor hours
        // And the patient's family members are at bedside when cardiac arrest occurs
        // When the code blue is activated

        // Then family management protocols are immediately implemented:
        //   | Family Management     | Immediate Actions                          |
        //   | Family Escort         | Security escorts family to private area    |
        //   | Communication         | Social worker provides immediate support   |
        //   | Information Updates   | Regular updates provided during resuscitation|
        //   | Chaplain Services     | Spiritual care offered immediately         |
        List<Map<String, String>> familyManagementProtocols = List.of(
            Map.of("label", "Family Escort", "value", "Security escorts family to private area"),
            Map.of("label", "Communication", "value", "Social worker provides immediate support"),
            Map.of("label", "Information Updates", "value", "Regular updates provided during resuscitation"),
            Map.of("label", "Chaplain Services Immediate Support", "value", "Spiritual care offered immediately")
        );
        for (var rowData : familyManagementProtocols) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(driver, label));
        }

        // And visitor area management is coordinated:
        //   | Visitor Control       | Safety Measures                            |
        //   | Area Clearance        | Non-family visitors moved from immediate area|
        //   | Privacy Protection    | Screens and barriers deployed             |
        //   | Crowd Control         | Security manages visitor flow             |
        //   | Other Patient Care    | Continued care for nearby patients        |
        List<Map<String, String>> visitorAreaManagement = List.of(
            Map.of("label", "Area Clearance", "value", "Non-family visitors moved from immediate area"),
            Map.of("label", "Privacy Protection", "value", "Screens and barriers deployed"),
            Map.of("label", "Crowd Control", "value", "Security manages visitor flow"),
            Map.of("label", "Other Patient Care", "value", "Continued care for nearby patients")
        );
        for (var rowData : visitorAreaManagement) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(driver, label));
        }

        // And family preference accommodation occurs:
        //   | Family Preference     | Options Provided                           |
        //   | Bedside Presence      | Option to remain during resuscitation     |
        //   | Waiting Area          | Comfortable private space with updates    |
        //   | Family Spokesperson   | Designated family member for communication |
        //   | Support Person        | Additional family/friend notification     |
        List<Map<String, String>> familyPreferenceAccommodation = List.of(
            Map.of("label", "Bedside Presence", "value", "Option to remain during resuscitation"),
            Map.of("label", "Waiting Area", "value", "Comfortable private space with updates"),
            Map.of("label", "Family Spokesperson", "value", "Designated family member for communication"),
            Map.of("label", "Support Person", "value", "Additional family/friend notification")
        );
        for (var rowData : familyPreferenceAccommodation) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(driver, label));
        }
    }

    @Test
    @Order(7)
    @DisplayName("Code blue team performance metrics and quality improvement")
    void codeBlueTeamPerformanceMetricsAndQualityImprovement() {
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
        List<Map<String, String>> performanceMetrics = List.of(
            Map.of("label", "Response Time", "value", "1 minute 23 seconds from alert to arrival"),
            Map.of("label", "No-flow Time Performance", "value", "15 seconds total interruption time"),
            Map.of("label", "First Shock Time", "value", "3 minutes 45 seconds from arrest"),
            Map.of("label", "Medication Timing Compliance", "value", "All drugs given within target windows"),
            Map.of("label", "Team Coordination", "value", "Communication effectiveness score")
        );
        for (var rowData : performanceMetrics) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(driver, label));
        }

        // And quality improvement data is captured:
        //   | QI Data Element       | Assessment                                 |
        //   | Protocol Adherence    | 95% compliance with ACLS guidelines       |
        //   | Equipment Function    | All equipment functioned properly         |
        //   | Team Performance      | Effective leadership and role clarity     |
        //   | Communication Quality | Clear, concise, and timely communication  |
        List<Map<String, String>> qiDataElements = List.of(
            Map.of("label", "Protocol Adherence", "value", "95% compliance with ACLS guidelines"),
            Map.of("label", "Equipment Function", "value", "All equipment functioned properly"),
            Map.of("label", "Team Performance", "value", "Effective leadership and role clarity"),
            Map.of("label", "Communication Quality", "value", "Clear, concise, and timely communication")
        );
        for (var rowData : qiDataElements) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(driver, label));
        }

        // And improvement opportunities are identified:
        //   | Improvement Area      | Recommendation                             |
        //   | Response Time         | Consider additional code cart placement    |
        //   | Team Training         | Schedule quarterly simulation training     |
        //   | Equipment Maintenance | Review defibrillator calibration schedule |
        //   | Documentation         | Streamline real-time entry process        |
        List<Map<String, String>> improvementOpportunities = List.of(
            Map.of("label", "Response Time Recommendation", "value", "Consider additional code cart placement"),
            Map.of("label", "Team Training", "value", "Schedule quarterly simulation training"),
            Map.of("label", "Equipment Maintenance", "value", "Review defibrillator calibration schedule"),
            Map.of("label", "Documentation", "value", "Streamline real-time entry process")
        );
        for (var rowData : improvementOpportunities) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(driver, label));
        }
    }

    @Test
    @Order(8)
    @DisplayName("Code blue false alarm with appropriate system response")
    void codeBlueFalseAlarmWithAppropriateSystemResponse() {
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
        List<Map<String, String>> falseAlarmProtocol = List.of(
            Map.of("label", "Alert Cancellation", "value", "\"Code blue canceled - false alarm\" announcement"),
            Map.of("label", "Team Stand-down", "value", "Code team notified to return to normal duties"),
            Map.of("label", "Equipment Check", "value", "Investigate and repair malfunctioning equipment"),
            Map.of("label", "False Alarm Documentation", "value", "Document false alarm and cause")
        );
        for (var rowData : falseAlarmProtocol) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(driver, label));
        }

        // And system improvements are implemented:
        //   | Improvement Action    | Preventive Measures                        |
        //   | Equipment Maintenance | Immediate repair of faulty equipment       |
        //   | Staff Education       | Review proper code blue activation         |
        //   | System Calibration    | Adjust sensitivity to prevent false alarms |
        //   | Audit Trail          | Record incident for system improvement     |
        List<Map<String, String>> systemImprovements = List.of(
            Map.of("label", "Equipment Maintenance Repair", "value", "Immediate repair of faulty equipment"),
            Map.of("label", "Staff Education", "value", "Review proper code blue activation"),
            Map.of("label", "System Calibration", "value", "Adjust sensitivity to prevent false alarms"),
            Map.of("label", "Audit Trail", "value", "Record incident for system improvement")
        );
        for (var rowData : systemImprovements) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(driver, label));
        }

        // And normal operations resume with lessons learned integrated into protocols
        assertMatches(getText(driver, "System Operations Status"), "normal", true);
    }
}
