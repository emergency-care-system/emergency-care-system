// Selenium WebDriver + JUnit 5 test for
// tests-with-given-when-then-features/19-mass-casualty-activation.feature
// (equivalent to tests-with-selenium-javascript/19-mass-casualty-activation.test.js).
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
class T19MassCasualtyActivationTest {
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
        //   And I am logged in as "Charge Nurse Williams"
        //   And the mass casualty incident (MCI) module is available (assumed pre-seeded test data)
        //   And emergency contact systems are enabled (assumed pre-seeded test data)
        //   And surge capacity protocols are configured (assumed pre-seeded test data)
        verifySystemIsOperational(driver);
        login(driver, "Charge Nurse Williams");

        var featureNavLink = waitForTestId(driver, "Nav Mass Casualty Activation");
        featureNavLink.click();
        waitForTestId(driver, "Mass Casualty Activation Panel");
    }

    @Test
    @Order(1)
    @DisplayName("Activate mass casualty protocol for multi-vehicle accident")
    void activateMassCasualtyProtocolForMultiVehicleAccident() {
        // Given it is 16:30 on a Friday afternoon (assumed pre-seeded test data)
        // And normal ED operations are in progress with 12 patients currently in the department (assumed pre-seeded test data)
        // And EMS reports a multi-vehicle accident with 8+ casualties en route (assumed pre-seeded test data)
        // When I receive notification of the mass casualty incident: (assumed simulated by test fixture data)
        // And I activate the disaster protocol in the system
        driver.findElement(By.cssSelector("[data-testid=\"activate-disaster-protocol\"]")).click();

        // Then the system immediately switches to surge capacity mode:
        waitForTestId(driver, "Mode Indicator");
        List<Map<String, String>> systemChangeRows = List.of(
            Map.of("label", "Mode Indicator", "value", "\"MASS CASUALTY ACTIVE\" banner displayed"),
            Map.of("label", "Interface Switch", "value", "MCI-specific workflows activated"),
            Map.of("label", "Normal Operations", "value", "Routine tasks suspended/deprioritized"),
            Map.of("label", "Resource Allocation", "value", "Emergency resource management enabled"),
            Map.of("label", "Communication Mode", "value", "Critical alerts and notifications active")
        );
        for (var row : systemChangeRows) {
            assertEquals(row.get("value"), getText(driver, row.get("label")));
        }

        // And additional staff are automatically alerted:
        List<Map<String, String>> staffAlertRows = List.of(
            Map.of("label", "Off-duty Physicians", "value", "SMS, Phone call"),
            Map.of("label", "Off-duty Nurses", "value", "SMS, Phone call"),
            Map.of("label", "Surgical Team", "value", "Overhead page, SMS"),
            Map.of("label", "Lab/Radiology", "value", "System alert, Phone"),
            Map.of("label", "Administration", "value", "Phone call, SMS"),
            Map.of("label", "Security", "value", "Radio, Overhead page")
        );
        for (var row : staffAlertRows) {
            assertEquals(row.get("value"), getText(driver, row.get("label")));
        }

        // And rapid registration workflows are created:
        List<Map<String, String>> registrationFeatureRows = List.of(
            Map.of("label", "Patient Identification", "value", "Sequential numbering: MCI-001, MCI-002"),
            Map.of("label", "Triage Tags", "value", "Color-coded electronic tags"),
            Map.of("label", "Minimal Data Entry", "value", "Name, age, chief complaint only"),
            Map.of("label", "Family Notification", "value", "Automated family alert system"),
            Map.of("label", "Tracking Board", "value", "Real-time patient status dashboard")
        );
        for (var row : registrationFeatureRows) {
            assertEquals(row.get("value"), getText(driver, row.get("label")));
        }
    }

    @Test
    @Order(2)
    @DisplayName("Configure surge capacity with bed and resource expansion")
    void configureSurgeCapacityWithBedAndResourceExpansion() {
        // Given the mass casualty protocol has been activated
        // And normal bed capacity is 20 beds
        // When the system enters surge capacity mode

        // Then additional treatment areas are activated:
        waitForTestId(driver, "Hallway Beds");
        List<Map<String, String>> surgeAreaRows = List.of(
            Map.of("label", "Hallway Beds", "value", "+6 treatment spaces"),
            Map.of("label", "Procedure Rooms", "value", "+3 converted spaces"),
            Map.of("label", "Observation Area", "value", "+8 holding spaces"),
            Map.of("label", "Waiting Room Triage", "value", "+4 assessment areas"),
            Map.of("label", "Ambulatory Care", "value", "+10 walking wounded")
        );
        for (var row : surgeAreaRows) {
            assertEquals(row.get("value"), getText(driver, row.get("label")));
        }

        // And resource allocation is optimized for mass casualty:
        List<Map<String, String>> resourceAllocationRows = List.of(
            Map.of("label", "Trauma Bays", "value", "All 4 activated"),
            Map.of("label", "Operating Rooms", "value", "3 rooms on standby"),
            Map.of("label", "Ventilators", "value", "8 total (5 from ICU)"),
            Map.of("label", "Blood Products", "value", "Massive transfusion protocol"),
            Map.of("label", "Medication Carts", "value", "5 carts deployed")
        );
        for (var row : resourceAllocationRows) {
            assertEquals(row.get("value"), getText(driver, row.get("label")));
        }

        // And staffing ratios are adjusted for emergency operations:
        List<Map<String, String>> staffingRatioRows = List.of(
            Map.of("label", "Physicians", "value", "1:12 patients"),
            Map.of("label", "Nurses", "value", "1:6 patients"),
            Map.of("label", "Support Staff", "value", "Double coverage")
        );
        for (var row : staffingRatioRows) {
            assertEquals(row.get("value"), getText(driver, row.get("label")));
        }
    }

    @Test
    @Order(3)
    @DisplayName("Implement rapid patient registration and triage workflow")
    void implementRapidPatientRegistrationAndTriageWorkflow() {
        // Given mass casualty mode is active
        // And the first ambulance arrives with 3 critical patients
        // When EMS brings patients to the ED

        // Then the rapid registration workflow is initiated:
        waitForTestId(driver, "Patient Arrival");
        List<Map<String, String>> registrationStepRows = List.of(
            Map.of("label", "Patient Arrival", "value", "Immediate tag assignment: MCI-001, 002, 003"),
            Map.of("label", "Triage Assessment", "value", "START triage protocol applied"),
            Map.of("label", "Electronic Tagging", "value", "Color-coded digital tags assigned"),
            Map.of("label", "Minimal Documentation", "value", "Name, estimated age, mechanism of injury"),
            Map.of("label", "Bed Assignment", "value", "Automatic assignment by acuity")
        );
        for (var row : registrationStepRows) {
            assertEquals(row.get("value"), getText(driver, row.get("label")));
        }

        // And electronic triage tags are applied with color coding:
        List<Map<String, String>> triageColorRows = List.of(
            Map.of("label", "Red (Immediate)", "value", "MCI-001 → Trauma Bay 1"),
            Map.of("label", "Yellow (Delayed)", "value", "MCI-002 → Surge Bed 3"),
            Map.of("label", "Green (Minor)", "value", "MCI-003 → Ambulatory Area"),
            Map.of("label", "Black (Deceased)", "value", "Morgue coordination")
        );
        for (var row : triageColorRows) {
            assertEquals(row.get("value"), getText(driver, row.get("label")));
        }

        // And family notification systems are activated:
        List<Map<String, String>> notificationProcessRows = List.of(
            Map.of("label", "Emergency Contacts", "value", "Auto-dial from patient personal effects"),
            Map.of("label", "Public Information", "value", "Hospital hotline number broadcasted"),
            Map.of("label", "Media Coordination", "value", "Incident command liaison activated"),
            Map.of("label", "Social Services", "value", "Family support team mobilized")
        );
        for (var row : notificationProcessRows) {
            assertEquals(row.get("value"), getText(driver, row.get("label")));
        }
    }

    @Test
    @Order(4)
    @DisplayName("Coordinate with external emergency services and hospitals")
    void coordinateWithExternalEmergencyServicesAndHospitals() {
        // Given a major mass casualty incident is in progress
        // And local EMS is overwhelmed with the response
        // When the system activates external coordination protocols

        // Then inter-facility communication is established:
        waitForTestId(driver, "EMS Command Center");
        List<Map<String, String>> communicationChannelRows = List.of(
            Map.of("label", "EMS Command Center", "value", "Patient distribution and transport updates"),
            Map.of("label", "Other Area Hospitals", "value", "Bed availability and transfer coordination"),
            Map.of("label", "Air Medical Services", "value", "Helicopter transport for critical patients"),
            Map.of("label", "Regional Trauma Centers", "value", "Specialty care transfer arrangements")
        );
        for (var row : communicationChannelRows) {
            assertEquals(row.get("value"), getText(driver, row.get("label")));
        }

        // And patient distribution management is activated:
        List<Map<String, String>> distributionStrategyRows = List.of(
            Map.of("label", "Load Balancing", "value", "Distribute patients across regional facilities"),
            Map.of("label", "Specialty Matching", "value", "Route patients to appropriate specialty care"),
            Map.of("label", "Capacity Monitoring", "value", "Real-time bed availability tracking"),
            Map.of("label", "Transport Coordination", "value", "Ambulance and helicopter scheduling")
        );
        for (var row : distributionStrategyRows) {
            assertEquals(row.get("value"), getText(driver, row.get("label")));
        }

        // And regional emergency management integration occurs:
        List<Map<String, String>> integrationElementRows = List.of(
            Map.of("label", "Incident Command", "value", "Hospital EOC links with regional ICS"),
            Map.of("label", "Resource Sharing", "value", "Equipment and staff sharing protocols"),
            Map.of("label", "Information Sharing", "value", "Patient status updates to command center"),
            Map.of("label", "Media Management", "value", "Coordinated public information releases")
        );
        for (var row : integrationElementRows) {
            assertEquals(row.get("value"), getText(driver, row.get("label")));
        }
    }

    @Test
    @Order(5)
    @DisplayName("Manage family reunification and information center")
    void manageFamilyReunificationAndInformationCenter() {
        // Given multiple patients from the mass casualty incident are being treated
        // And families are arriving seeking information about their loved ones
        // When the family information center is activated

        // Then patient tracking and family communication systems are deployed:
        waitForTestId(driver, "Information Hotline");
        List<Map<String, String>> familySupportSystemRows = List.of(
            Map.of("label", "Information Hotline", "value", "Dedicated phone line with trained staff"),
            Map.of("label", "Family Reunification", "value", "Secure area for family waiting and updates"),
            Map.of("label", "Patient Tracking", "value", "Real-time status board for authorized viewers"),
            Map.of("label", "Privacy Protection", "value", "HIPAA-compliant information sharing")
        );
        for (var row : familySupportSystemRows) {
            assertEquals(row.get("value"), getText(driver, row.get("label")));
        }

        // And automated family notification processes are initiated:
        List<Map<String, String>> notificationMethodRows = List.of(
            Map.of("label", "SMS Updates", "value", "Your family member is being treated safely"),
            Map.of("label", "Phone Calls", "value", "Personal calls for critical status changes"),
            Map.of("label", "Information Boards", "value", "General incident updates (no patient names)"),
            Map.of("label", "Social Workers", "value", "One-on-one family support and counseling")
        );
        for (var row : notificationMethodRows) {
            assertEquals(row.get("value"), getText(driver, row.get("label")));
        }

        // And patient status tracking is maintained:
        List<Map<String, String>> trackingElementRows = List.of(
            Map.of("label", "Current Location", "value", "Treatment area, OR, transferred, etc."),
            Map.of("label", "Medical Status", "value", "Stable, critical, treated and released"),
            Map.of("label", "Next of Kin Contact", "value", "Verification and notification status"),
            Map.of("label", "Discharge Planning", "value", "Expected timeline and care needs")
        );
        for (var row : trackingElementRows) {
            assertEquals(row.get("value"), getText(driver, row.get("label")));
        }
    }

    @Test
    @Order(6)
    @DisplayName("Transition from mass casualty mode back to normal operations")
    void transitionFromMassCasualtyModeBackToNormalOperations() {
        // Given the mass casualty incident has been resolved
        // And all patients are stabilized or transferred
        // And no additional casualties are expected
        // When I initiate the transition back to normal operations
        driver.findElement(By.cssSelector("[data-testid=\"initiate-transition-to-normal\"]")).click();

        // Then the system manages the deactivation process:
        waitForTestId(driver, "Incident Assessment");
        List<Map<String, String>> deactivationStepRows = List.of(
            Map.of("label", "Incident Assessment", "value", "Review of patient outcomes and resources used"),
            Map.of("label", "Staff Debriefing", "value", "Immediate hot wash and formal debriefing"),
            Map.of("label", "Resource Restoration", "value", "Return equipment and supplies to normal areas"),
            Map.of("label", "Documentation", "value", "Complete incident documentation and reports")
        );
        for (var row : deactivationStepRows) {
            assertEquals(row.get("value"), getText(driver, row.get("label")));
        }

        // And normal ED operations are gradually restored:
        List<Map<String, String>> restorationPhaseRows = List.of(
            Map.of("label", "Immediate (0-30 min)", "value", "Secure scene"),
            Map.of("label", "Short-term (30-60 min)", "value", "Resource cleanup"),
            Map.of("label", "Medium-term (1-4 hrs)", "value", "Staff rotation"),
            Map.of("label", "Long-term (4-24 hrs)", "value", "Full restoration")
        );
        for (var row : restorationPhaseRows) {
            assertEquals(row.get("value"), getText(driver, row.get("label")));
        }

        // And quality improvement activities are initiated:
        List<Map<String, String>> qiActivityRows = List.of(
            Map.of("label", "After Action Review", "value", "Identify strengths and improvement areas"),
            Map.of("label", "Performance Metrics", "value", "Analyze response times and patient outcomes"),
            Map.of("label", "Protocol Updates", "value", "Revise procedures based on lessons learned"),
            Map.of("label", "Training Needs", "value", "Identify staff training and education needs")
        );
        for (var row : qiActivityRows) {
            assertEquals(row.get("value"), getText(driver, row.get("label")));
        }
    }

    @Test
    @Order(7)
    @DisplayName("Handle mass casualty incident during shift change")
    void handleMassCasualtyIncidentDuringShiftChange() {
        // Given it is 19:00 during evening shift change
        // And day shift staff are preparing to leave
        // And evening shift staff are assuming duties
        // When a mass casualty incident is declared

        // Then the system manages staffing during the transition:
        waitForTestId(driver, "Shift Hold");
        List<Map<String, String>> staffingStrategyRows = List.of(
            Map.of("label", "Shift Hold", "value", "Day shift staff remain for incident response"),
            Map.of("label", "Double Coverage", "value", "Both shifts work together during surge"),
            Map.of("label", "Incident Command Continuity", "value", "Clear leadership chain established"),
            Map.of("label", "Communication", "value", "All staff briefed on roles and responsibilities")
        );
        for (var row : staffingStrategyRows) {
            assertEquals(row.get("value"), getText(driver, row.get("label")));
        }

        // And transition protocols are modified for the emergency:
        List<Map<String, String>> modifiedProtocolRows = List.of(
            Map.of("label", "Handoff Procedures", "value", "Suspended until incident resolution"),
            Map.of("label", "Staffing Ratios", "value", "Enhanced coverage with both shifts"),
            Map.of("label", "Leadership Structure", "value", "Incident commander takes operational control"),
            Map.of("label", "Modified Documentation", "value", "Emergency documentation procedures active")
        );
        for (var row : modifiedProtocolRows) {
            assertEquals(row.get("value"), getText(driver, row.get("label")));
        }
    }

    @Test
    @Order(8)
    @DisplayName("Test mass casualty system readiness through drill")
    void testMassCasualtySystemReadinessThroughDrill() {
        // Given it is a scheduled quarterly mass casualty drill
        // And the drill scenario involves a simulated building collapse with 15 casualties
        // When the drill coordinator activates the test mass casualty protocol
        driver.findElement(By.cssSelector("[data-testid=\"activate-test-mci-protocol\"]")).click();

        // Then the system activates in drill mode:
        waitForTestId(driver, "Test Mode Indicator");
        List<Map<String, String>> drillFeatureRows = List.of(
            Map.of("label", "Test Mode Indicator", "value", "\"DRILL - NOT REAL EMERGENCY\" displayed"),
            Map.of("label", "Simulated Patients", "value", "Test patient records created"),
            Map.of("label", "Staff Participation", "value", "All roles and responsibilities tested"),
            Map.of("label", "Resource Tracking", "value", "Equipment and supplies tracked but not used")
        );
        for (var row : drillFeatureRows) {
            assertEquals(row.get("value"), getText(driver, row.get("label")));
        }

        // And drill performance metrics are captured:
        List<Map<String, String>> drillPerformanceMetricRows = List.of(
            Map.of("label", "Activation Time", "value", "Time from alert to full surge capacity"),
            Map.of("label", "Staff Response Time", "value", "Time for staff to report and assume roles"),
            Map.of("label", "Communication Speed", "value", "Time for all notifications to be completed"),
            Map.of("label", "Resource Deployment", "value", "Time to set up surge areas and equipment")
        );
        for (var row : drillPerformanceMetricRows) {
            assertEquals(row.get("value"), getText(driver, row.get("label")));
        }

        // And drill evaluation and improvement planning occurs:
        List<Map<String, String>> evaluationComponentRows = List.of(
            Map.of("label", "Protocol Effectiveness", "value", "How well procedures worked in practice"),
            Map.of("label", "Staff Preparedness", "value", "Knowledge and skill gaps identified"),
            Map.of("label", "System Performance", "value", "Technology and workflow efficiency"),
            Map.of("label", "Improvement Plans", "value", "Action items for enhancing response capabilities")
        );
        for (var row : evaluationComponentRows) {
            assertEquals(row.get("value"), getText(driver, row.get("label")));
        }
    }
}
