// Playwright + JUnit 5 test for
// tests-with-given-when-then-features/21-user-authentication.feature
// (equivalent to tests-with-playwright-javascript/21-user-authentication.test.js).
//
// This feature's whole subject is the login/authentication flow itself, so
// most scenarios drive the login page directly with the shared data-testid
// contract (login-identity, login-submit, app-root, see support/Login.java)
// rather than the login() helper, which assumes success. Other fields
// (password, badge number, dates, etc.) are assumed to expose data-testid
// attributes matching their Gherkin field label (kebab-cased, see
// support/Fields.java).

package emergencycaresystem;

import static emergencycaresystem.support.Config.BASE_URL;
import static emergencycaresystem.support.Fields.*;
import static emergencycaresystem.support.Login.*;
import static emergencycaresystem.support.Matchers.assertMatches;
import static org.junit.jupiter.api.Assertions.*;

import java.util.List;
import java.util.Map;
import com.microsoft.playwright.Page;
import emergencycaresystem.support.Session;
import org.junit.jupiter.api.AfterAll;
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.MethodOrderer;
import org.junit.jupiter.api.Order;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.TestMethodOrder;

@TestMethodOrder(MethodOrderer.OrderAnnotation.class)
class T21UserAuthenticationTest {
    private static Session session;
    private static Page page;

    @BeforeAll
    static void setUpClass() {
        session = Session.start();
        page = session.page();
    }

    @AfterAll
    static void tearDownClass() {
        session.close();
    }

    @BeforeEach
    void setUp() {
        // Background:
        //   Given the emergency care system is operational
        //   And the authentication module is active
        //   And the badge scanning system is functional
        //   And user credentials database is accessible
        //   And audit logging is enabled
        verifySystemIsOperational(page);
        // The remaining Background steps describe pre-seeded system state
        // (authentication module, badge scanning system, credentials database,
        // and audit logging readiness) assumed to already be configured in the
        // test environment.
    }

    @Test
    @Order(1)
    @DisplayName("Successful nurse login with username, password, and badge scan")
    void successfulNurseLoginWithUsernamePasswordAndBadgeScan() {
        // Given I am "Nurse Sarah Johnson" with valid system credentials
        // And my user account has the following attributes:
        //   | Account Attribute     | Value                                      |
        //   | Username              | sjohnson                                   |
        //   | Employee ID           | EMP-12345                                 |
        //   | Badge Number          | BADGE-67890                               |
        //   | Role                  | Registered Nurse                          |
        //   | Department            | Emergency Department                       |
        //   | Security Clearance    | Level 2 - Patient Care                   |
        //   | Account Status        | Active                                    |
        // And my badge is properly programmed and functional
        // When I enter my username "sjohnson" in the login field
        page.navigate(BASE_URL + "/login");
        var identityField = waitLocated(page, "login-identity", 10000);
        identityField.fill("sjohnson");
        // And I enter my password in the password field
        fillField(page, "Password", "CorrectPassword123!");
        // And I scan my badge "BADGE-67890" using the badge reader
        fillField(page, "Badge Number", "BADGE-67890");
        // And I click the "Login" button
        page.getByTestId("login-submit").first().click();

        // Then the system authenticates my credentials successfully:
        //   | Authentication Check  | Verification Result                        |
        //   | Username Validation   | Valid - User exists in system             |
        //   | Password Verification | Correct - Password hash matches           |
        //   | Badge Authentication  | Valid - Badge number matches employee     |
        //   | Account Status        | Active - Account is enabled              |
        //   | Role Authorization    | Authorized - RN role has ED access       |
        List<Map<String, String>> authenticationChecks = List.of(
            Map.of("label", "Username Validation", "value", "Valid - User exists in system"),
            Map.of("label", "Password Verification", "value", "Correct - Password hash matches"),
            Map.of("label", "Badge Authentication", "value", "Valid - Badge number matches employee"),
            Map.of("label", "Account Status", "value", "Active - Account is enabled"),
            Map.of("label", "Role Authorization", "value", "Authorized - RN role has ED access")
        );
        for (var rowData : authenticationChecks) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(page, label));
        }

        // And the access attempt is logged with details:
        //   | Audit Log Entry       | Information Recorded                       |
        //   | User ID               | sjohnson (Sarah Johnson)                  |
        //   | Login Timestamp       | 2025-06-24 08:00:15                       |
        //   | Workstation ID        | WS-ED-05                                  |
        //   | IP Address            | 192.168.1.105                            |
        //   | Authentication Method | Username/Password + Badge Scan            |
        //   | Login Success         | True                                      |
        //   | Session ID            | SES-ABC123DEF456                          |
        List<Map<String, String>> accessAttemptLog = List.of(
            Map.of("label", "User ID", "value", "sjohnson (Sarah Johnson)"),
            Map.of("label", "Login Timestamp", "value", "2025-06-24 08:00:15"),
            Map.of("label", "Workstation ID", "value", "WS-ED-05"),
            Map.of("label", "IP Address", "value", "192.168.1.105"),
            Map.of("label", "Authentication Method", "value", "Username/Password + Badge Scan"),
            Map.of("label", "Login Success", "value", "True"),
            Map.of("label", "Session ID", "value", "SES-ABC123DEF456")
        );
        for (var rowData : accessAttemptLog) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(page, label));
        }

        // And my personalized dashboard is displayed:
        //   | Dashboard Section     | Nurse-Specific Content                     |
        //   | Patient Assignment    | Current patients assigned to Sarah Johnson |
        //   | Task List            | Medication administration, vitals due      |
        //   | Alerts/Notifications | Critical lab values, patient call lights  |
        //   | Quick Access Tools   | Medication lookup, dosage calculator       |
        //   | Shift Information    | Shift start: 07:00, Break schedule        |
        //   | Department Status    | Current census, bed availability          |
        List<Map<String, String>> dashboardSections = List.of(
            Map.of("label", "Patient Assignment", "value", "Current patients assigned to Sarah Johnson"),
            Map.of("label", "Task List", "value", "Medication administration, vitals due"),
            Map.of("label", "Alerts/Notifications", "value", "Critical lab values, patient call lights"),
            Map.of("label", "Quick Access Tools", "value", "Medication lookup, dosage calculator"),
            Map.of("label", "Shift Information", "value", "Shift start: 07:00, Break schedule"),
            Map.of("label", "Department Status", "value", "Current census, bed availability")
        );
        for (var rowData : dashboardSections) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(page, label));
        }
    }

    @Test
    @Order(2)
    @DisplayName("Failed login attempt with incorrect password")
    void failedLoginAttemptWithIncorrectPassword() {
        // Given I am attempting to log in as "Nurse Mike Chen"
        // And my username is "mchen"
        // And my correct password is stored in the system
        // When I enter my username "mchen"
        page.navigate(BASE_URL + "/login");
        var identityField = waitLocated(page, "login-identity", 10000);
        identityField.fill("mchen");
        // And I enter an incorrect password "wrongpassword"
        fillField(page, "Password", "wrongpassword");
        // And I scan my valid badge "BADGE-54321"
        fillField(page, "Badge Number", "BADGE-54321");
        // And I click the "Login" button
        page.getByTestId("login-submit").first().click();

        // Then the system rejects the authentication:
        //   | Authentication Check  | Verification Result                        |
        //   | Username Validation   | Valid - User exists                       |
        //   | Password Verification | Failed - Password does not match          |
        //   | Badge Authentication  | Valid - Badge number correct              |
        //   | Overall Result        | Authentication Failed                     |
        List<Map<String, String>> authenticationChecks = List.of(
            Map.of("label", "Username Validation", "value", "Valid - User exists"),
            Map.of("label", "Password Verification", "value", "Failed - Password does not match"),
            Map.of("label", "Badge Authentication", "value", "Valid - Badge number correct"),
            Map.of("label", "Overall Result", "value", "Authentication Failed")
        );
        for (var rowData : authenticationChecks) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(page, label));
        }

        // And a security alert is logged:
        //   | Security Log Entry    | Information Recorded                       |
        //   | User ID               | mchen (Mike Chen)                         |
        //   | Failed Login Time     | 2025-06-24 08:15:32                       |
        //   | Workstation ID        | WS-ED-03                                  |
        //   | IP Address            | 192.168.1.103                            |
        //   | Failure Reason        | Incorrect Password                        |
        //   | Attempt Count         | 1 of 3 allowed attempts                   |
        List<Map<String, String>> securityLogEntries = List.of(
            Map.of("label", "User ID", "value", "mchen (Mike Chen)"),
            Map.of("label", "Failed Login Time", "value", "2025-06-24 08:15:32"),
            Map.of("label", "Workstation ID", "value", "WS-ED-03"),
            Map.of("label", "IP Address", "value", "192.168.1.103"),
            Map.of("label", "Failure Reason", "value", "Incorrect Password"),
            Map.of("label", "Attempt Count", "value", "1 of 3 allowed attempts")
        );
        for (var rowData : securityLogEntries) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(page, label));
        }

        // And an error message is displayed:
        //   | Error Message         | "Invalid credentials. Please check your username and password." |
        //   | Security Notice       | "2 attempts remaining before account lockout"                   |
        assertEquals("Invalid credentials. Please check your username and password.", getText(page, "Error Message"));
        assertEquals("2 attempts remaining before account lockout", getText(page, "Security Notice"));

        // And I remain on the login screen to retry authentication
        var loginForm = waitForTestId(page, "Login Form");
        assertTrue(loginForm.isVisible());
    }

    @Test
    @Order(3)
    @DisplayName("Account lockout after multiple failed login attempts")
    void accountLockoutAfterMultipleFailedLoginAttempts() {
        // Given I am "Dr. Amanda Wilson" attempting to log in
        // And my account allows 3 failed login attempts before lockout
        // And I have already made 2 failed login attempts today
        // When I enter my username "awilson"
        page.navigate(BASE_URL + "/login");
        var identityField = waitLocated(page, "login-identity", 10000);
        identityField.fill("awilson");
        // And I enter another incorrect password
        fillField(page, "Password", "another-wrong-password");
        // And I scan my badge and click login
        fillField(page, "Badge Number", "BADGE-11111");
        page.getByTestId("login-submit").first().click();

        // Then the system locks my account:
        //   | Lockout Response      | Security Action                            |
        //   | Account Status        | Locked - Exceeded maximum failed attempts |
        //   | Lockout Duration      | 30 minutes automatic unlock               |
        //   | Manual Override       | IT Security can unlock immediately        |
        List<Map<String, String>> lockoutResponse = List.of(
            Map.of("label", "Account Status", "value", "Locked - Exceeded maximum failed attempts"),
            Map.of("label", "Lockout Duration", "value", "30 minutes automatic unlock"),
            Map.of("label", "Manual Override", "value", "IT Security can unlock immediately")
        );
        for (var rowData : lockoutResponse) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(page, label));
        }

        // And a security incident is logged:
        //   | Security Incident     | Details                                    |
        //   | Event Type            | Account Lockout - Excessive Failed Attempts|
        //   | User Account          | awilson (Dr. Amanda Wilson)               |
        //   | Lockout Time          | 2025-06-24 14:22:18                       |
        //   | Failed Attempts       | 3 consecutive failures                     |
        //   | Workstation ID        | WS-ED-07                                  |
        List<Map<String, String>> securityIncident = List.of(
            Map.of("label", "Event Type", "value", "Account Lockout - Excessive Failed Attempts"),
            Map.of("label", "User Account", "value", "awilson (Dr. Amanda Wilson)"),
            Map.of("label", "Lockout Time", "value", "2025-06-24 14:22:18"),
            Map.of("label", "Failed Attempts", "value", "3 consecutive failures"),
            Map.of("label", "Workstation ID", "value", "WS-ED-07")
        );
        for (var rowData : securityIncident) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(page, label));
        }

        // And security notifications are sent:
        //   | Notification Target   | Alert Content                              |
        //   | IT Security Team      | Account lockout alert for Dr. Wilson      |
        //   | Department Supervisor | Staff member unable to access system      |
        //   | User Email            | Account locked - contact IT for assistance|
        List<Map<String, String>> securityNotifications = List.of(
            Map.of("label", "IT Security Team", "value", "Account lockout alert for Dr. Wilson"),
            Map.of("label", "Department Supervisor", "value", "Staff member unable to access system"),
            Map.of("label", "User Email", "value", "Account locked - contact IT for assistance")
        );
        for (var rowData : securityNotifications) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(page, label));
        }

        // And a lockout message is displayed:
        //   | Lockout Message       | "Account temporarily locked due to multiple failed login attempts. Contact IT Security or wait 30 minutes." |
        assertEquals("Account temporarily locked due to multiple failed login attempts. Contact IT Security or wait 30 minutes.", getText(page, "Lockout Message"));
    }

    @Test
    @Order(4)
    @DisplayName("Login with expired password requiring password reset")
    void loginWithExpiredPasswordRequiringPasswordReset() {
        // Given I am "Nurse Patricia Martinez" with valid credentials
        // And my password expired 5 days ago according to policy
        // And the system requires password changes every 90 days
        // When I enter my username "pmartinez"
        page.navigate(BASE_URL + "/login");
        var identityField = waitLocated(page, "login-identity", 10000);
        identityField.fill("pmartinez");
        // And I enter my current (expired) password
        fillField(page, "Password", "ExpiredPassword123!");
        // And I scan my badge successfully
        fillField(page, "Badge Number", "BADGE-24680");
        page.getByTestId("login-submit").first().click();

        // Then the system identifies the expired password:
        //   | Password Check        | Status                                     |
        //   | Password Validity     | Expired - 5 days past expiration         |
        //   | Grace Period          | Exceeded - No grace logins remaining      |
        //   | Password Age          | 95 days old (5 days over 90-day limit)   |
        List<Map<String, String>> passwordChecks = List.of(
            Map.of("label", "Password Validity", "value", "Expired - 5 days past expiration"),
            Map.of("label", "Grace Period", "value", "Exceeded - No grace logins remaining"),
            Map.of("label", "Password Age", "value", "95 days old (5 days over 90-day limit)")
        );
        for (var rowData : passwordChecks) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(page, label));
        }

        // And the password reset workflow is initiated:
        //   | Reset Process         | Required Actions                           |
        //   | Identity Verification | Additional security questions prompted     |
        //   | New Password Entry    | Password must meet complexity requirements |
        //   | Password Confirmation | Confirm new password entry                |
        //   | Security Questions    | Update security questions if needed       |
        List<Map<String, String>> resetProcess = List.of(
            Map.of("label", "Identity Verification", "value", "Additional security questions prompted"),
            Map.of("label", "New Password Entry", "value", "Password must meet complexity requirements"),
            Map.of("label", "Password Confirmation", "value", "Confirm new password entry"),
            Map.of("label", "Security Questions", "value", "Update security questions if needed")
        );
        for (var rowData : resetProcess) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(page, label));
        }

        // And password policy requirements are displayed:
        //   | Policy Requirement    | Specification                              |
        //   | Minimum Length        | 12 characters                             |
        //   | Character Types       | Upper, lower, number, special character   |
        //   | Password History      | Cannot reuse last 12 passwords           |
        //   | Common Words          | Cannot use dictionary words or personal info|
        List<Map<String, String>> policyRequirements = List.of(
            Map.of("label", "Minimum Length", "value", "12 characters"),
            Map.of("label", "Character Types", "value", "Upper, lower, number, special character"),
            Map.of("label", "Password History", "value", "Cannot reuse last 12 passwords"),
            Map.of("label", "Common Words", "value", "Cannot use dictionary words or personal info")
        );
        for (var rowData : policyRequirements) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(page, label));
        }

        // And upon successful password reset, normal login proceeds
        assertMatches(getText(page, "Login Status"), "success", true);
    }

    @Test
    @Order(5)
    @DisplayName("Role-based dashboard customization after successful login")
    void roleBasedDashboardCustomizationAfterSuccessfulLogin() {
        // Given multiple users with different roles log in successfully
        // When "Dr. Emily Rodriguez" (Emergency Physician) logs in
        login(page, "Dr. Emily Rodriguez");

        // Then her physician dashboard is displayed with:
        //   | Physician Dashboard   | Specialized Content                        |
        //   | Patient Queue         | Patients waiting to be seen by priority    |
        //   | Active Orders         | Lab results, imaging pending review       |
        //   | Critical Alerts       | Abnormal vitals, critical lab values      |
        //   | Decision Support      | Clinical guidelines, drug interactions    |
        //   | Documentation Tools   | Templates for common conditions           |
        List<Map<String, String>> physicianDashboard = List.of(
            Map.of("label", "Patient Queue", "value", "Patients waiting to be seen by priority"),
            Map.of("label", "Active Orders", "value", "Lab results, imaging pending review"),
            Map.of("label", "Critical Alerts", "value", "Abnormal vitals, critical lab values"),
            Map.of("label", "Decision Support", "value", "Clinical guidelines, drug interactions"),
            Map.of("label", "Documentation Tools", "value", "Templates for common conditions")
        );
        for (var rowData : physicianDashboard) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(page, label));
        }

        // When "Charge Nurse Williams" logs in
        login(page, "Charge Nurse Williams");

        // Then her charge nurse dashboard shows:
        //   | Charge Nurse Dashboard| Management-Focused Content                 |
        //   | Department Overview   | Bed status, staff assignments, census     |
        //   | Resource Management   | Equipment status, supply levels           |
        //   | Staff Coordination    | Break schedules, assignments, coverage    |
        //   | Quality Metrics       | Wait times, patient satisfaction, safety  |
        //   | Administrative Tasks  | Reporting, scheduling, policy updates     |
        List<Map<String, String>> chargeNurseDashboard = List.of(
            Map.of("label", "Department Overview", "value", "Bed status, staff assignments, census"),
            Map.of("label", "Resource Management", "value", "Equipment status, supply levels"),
            Map.of("label", "Staff Coordination", "value", "Break schedules, assignments, coverage"),
            Map.of("label", "Quality Metrics", "value", "Wait times, patient satisfaction, safety"),
            Map.of("label", "Administrative Tasks", "value", "Reporting, scheduling, policy updates")
        );
        for (var rowData : chargeNurseDashboard) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(page, label));
        }

        // When "Tech Support Anderson" logs in
        login(page, "Tech Support Anderson");

        // Then his technical dashboard displays:
        //   | Technical Dashboard   | System Administration Content              |
        //   | System Status         | Server health, network connectivity       |
        //   | User Management       | Account status, permission changes        |
        //   | Audit Logs           | System access, security events            |
        //   | Maintenance Tools     | Backup status, system updates             |
        List<Map<String, String>> technicalDashboard = List.of(
            Map.of("label", "System Status", "value", "Server health, network connectivity"),
            Map.of("label", "User Management", "value", "Account status, permission changes"),
            Map.of("label", "Audit Logs", "value", "System access, security events"),
            Map.of("label", "Maintenance Tools", "value", "Backup status, system updates")
        );
        for (var rowData : technicalDashboard) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(page, label));
        }
    }

    @Test
    @Order(6)
    @DisplayName("Mobile device authentication with additional security")
    void mobileDeviceAuthenticationWithAdditionalSecurity() {
        // Given I am using the mobile ED app on my smartphone
        // And mobile access requires enhanced security measures
        // When I attempt to log in on my mobile device
        page.navigate(BASE_URL + "/login?viewport=mobile");
        waitLocated(page, "login-identity", 10000);

        // Then additional authentication factors are required:
        //   | Mobile Security Factor| Requirement                                |
        //   | Device Registration   | Device must be registered with IT          |
        //   | Biometric Auth        | Fingerprint or face recognition required   |
        //   | Location Verification | GPS confirms user is within hospital grounds|
        //   | Time-based Token      | 6-digit code from authenticator app       |
        List<Map<String, String>> mobileSecurityFactors = List.of(
            Map.of("label", "Device Registration", "value", "Device must be registered with IT"),
            Map.of("label", "Biometric Auth", "value", "Fingerprint or face recognition required"),
            Map.of("label", "Location Verification", "value", "GPS confirms user is within hospital grounds"),
            Map.of("label", "Time-based Token", "value", "6-digit code from authenticator app")
        );
        for (var rowData : mobileSecurityFactors) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(page, label));
        }

        // And I complete multi-factor authentication:
        //   | MFA Step              | Verification Process                       |
        //   | Username/Password     | Standard credential verification           |
        //   | Biometric Scan        | Fingerprint verified against enrolled pattern|
        //   | Location Check        | GPS coordinates within allowed radius      |
        //   | Time Token            | Authenticator app code validated          |
        List<Map<String, String>> mfaSteps = List.of(
            Map.of("label", "Username/Password", "value", "Standard credential verification"),
            Map.of("label", "Biometric Scan", "value", "Fingerprint verified against enrolled pattern"),
            Map.of("label", "Location Check", "value", "GPS coordinates within allowed radius"),
            Map.of("label", "Time Token", "value", "Authenticator app code validated")
        );
        for (var rowData : mfaSteps) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(page, label));
        }

        // And mobile-specific security controls are applied:
        //   | Mobile Control        | Security Measure                           |
        //   | Session Timeout       | 15-minute inactivity timeout             |
        //   | Screen Lock           | Auto-lock after 2 minutes idle           |
        //   | Data Encryption       | All data encrypted on device             |
        //   | Remote Wipe           | IT can remotely clear data if device lost |
        List<Map<String, String>> mobileControls = List.of(
            Map.of("label", "Session Timeout", "value", "15-minute inactivity timeout"),
            Map.of("label", "Screen Lock", "value", "Auto-lock after 2 minutes idle"),
            Map.of("label", "Data Encryption", "value", "All data encrypted on device"),
            Map.of("label", "Remote Wipe", "value", "IT can remotely clear data if device lost")
        );
        for (var rowData : mobileControls) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(page, label));
        }
    }

    @Test
    @Order(7)
    @DisplayName("Emergency override authentication during system issues")
    void emergencyOverrideAuthenticationDuringSystemIssues() {
        // Given the primary authentication system is experiencing technical difficulties
        // And there is a patient emergency requiring immediate system access
        // And I am "Dr. Lisa Chen" needing urgent access to patient records
        // The emergency override option lives on the sign-in screen.
        page.navigate(BASE_URL + "/login");
        waitLocated(page, "login-identity", 10000);
        // When I request emergency override access
        page.getByTestId("emergency-override-button").first().click();

        // Then the emergency authentication protocol is activated:
        //   | Emergency Protocol    | Override Process                           |
        //   | Identity Verification | Manual verification by IT Security         |
        //   | Supervisor Approval   | Department head authorization required     |
        //   | Time-Limited Access   | 2-hour temporary access granted           |
        //   | Enhanced Monitoring   | All actions logged for later review       |
        List<Map<String, String>> emergencyProtocol = List.of(
            Map.of("label", "Identity Verification", "value", "Manual verification by IT Security"),
            Map.of("label", "Supervisor Approval", "value", "Department head authorization required"),
            Map.of("label", "Time-Limited Access", "value", "2-hour temporary access granted"),
            Map.of("label", "Enhanced Monitoring", "value", "All actions logged for later review")
        );
        for (var rowData : emergencyProtocol) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(page, label));
        }

        // And emergency access is granted with restrictions:
        //   | Access Limitation     | Restriction Details                        |
        //   | Time Limit            | Access expires automatically in 2 hours   |
        //   | Function Restrictions | Read-only access to critical patient data |
        //   | Audit Trail          | Enhanced logging of all actions           |
        //   | Supervisor Oversight  | Real-time monitoring of override usage     |
        List<Map<String, String>> accessLimitations = List.of(
            Map.of("label", "Time Limit", "value", "Access expires automatically in 2 hours"),
            Map.of("label", "Function Restrictions", "value", "Read-only access to critical patient data"),
            Map.of("label", "Audit Trail", "value", "Enhanced logging of all actions"),
            Map.of("label", "Supervisor Oversight", "value", "Real-time monitoring of override usage")
        );
        for (var rowData : accessLimitations) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(page, label));
        }

        // And emergency access usage is documented:
        //   | Emergency Documentation| Required Information                      |
        //   | Medical Justification | Patient condition requiring urgent access |
        //   | Approving Authority   | Name and title of authorizing supervisor  |
        //   | Access Duration       | Exact start and end times of override use |
        //   | Actions Performed     | Detailed log of all system activities    |
        List<Map<String, String>> emergencyDocumentation = List.of(
            Map.of("label", "Medical Justification", "value", "Patient condition requiring urgent access"),
            Map.of("label", "Approving Authority", "value", "Name and title of authorizing supervisor"),
            Map.of("label", "Access Duration", "value", "Exact start and end times of override use"),
            Map.of("label", "Actions Performed", "value", "Detailed log of all system activities")
        );
        for (var rowData : emergencyDocumentation) {
            var label = rowData.get("label");
            var value = rowData.get("value");
            assertEquals(value, getText(page, label));
        }
    }
}
