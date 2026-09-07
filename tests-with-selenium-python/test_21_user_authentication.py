"""Selenium WebDriver + pytest test for tests-with-given-when-then-features/21-user-authentication.feature
(equivalent to tests-with-selenium-javascript/21-user-authentication.test.js).

This feature's whole subject is the login/authentication flow itself, so
most scenarios drive the login page directly with the shared data-testid
contract (login-identity, login-submit, app-root, see support/login.py)
rather than the login() helper, which assumes success. Other fields
(password, badge number, dates, etc.) are assumed to expose data-testid
attributes matching their Gherkin field label (kebab-cased, see
support/fields.py).
"""

from selenium.webdriver.common.by import By
from selenium.webdriver.support import expected_conditions as EC
from selenium.webdriver.support.ui import WebDriverWait

from support.build_driver import build_driver
from support.config import BASE_URL
from support.fields import fill_field, get_text, wait_for_test_id
from support.login import login, verify_system_is_operational


class TestUserAuthentication:
    @classmethod
    def setup_class(cls):
        cls.driver = build_driver()

    @classmethod
    def teardown_class(cls):
        cls.driver.quit()

    def setup_method(self):
        # Background:
        #   Given the emergency care system is operational
        #   And the authentication module is active
        #   And the badge scanning system is functional
        #   And user credentials database is accessible
        #   And audit logging is enabled
        verify_system_is_operational(self.driver)
        # The remaining Background steps describe pre-seeded system state
        # (authentication module, badge scanning system, credentials
        # database, and audit logging readiness) assumed to already be
        # configured in the test environment.

    def test_successful_nurse_login_with_username_password_and_badge_scan(self):
        # Given I am "Nurse Sarah Johnson" with valid system credentials
        # And my user account has the following attributes:
        #   | Account Attribute     | Value                                      |
        #   | Username              | sjohnson                                   |
        #   | Employee ID           | EMP-12345                                 |
        #   | Badge Number          | BADGE-67890                               |
        #   | Role                  | Registered Nurse                          |
        #   | Department            | Emergency Department                       |
        #   | Security Clearance    | Level 2 - Patient Care                   |
        #   | Account Status        | Active                                    |
        # And my badge is properly programmed and functional
        # When I enter my username "sjohnson" in the login field
        self.driver.get(f"{BASE_URL}/login")
        identity_field = WebDriverWait(self.driver, 10).until(
            EC.presence_of_element_located((By.CSS_SELECTOR, '[data-testid="login-identity"]'))
        )
        identity_field.send_keys("sjohnson")
        # And I enter my password in the password field
        fill_field(self.driver, "Password", "CorrectPassword123!")
        # And I scan my badge "BADGE-67890" using the badge reader
        fill_field(self.driver, "Badge Number", "BADGE-67890")
        # And I click the "Login" button
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="login-submit"]').click()

        # Then the system authenticates my credentials successfully:
        #   | Authentication Check  | Verification Result                        |
        #   | Username Validation   | Valid - User exists in system             |
        #   | Password Verification | Correct - Password hash matches           |
        #   | Badge Authentication  | Valid - Badge number matches employee     |
        #   | Account Status        | Active - Account is enabled              |
        #   | Role Authorization    | Authorized - RN role has ED access       |
        authentication_checks = [
            {"label": "Username Validation", "value": "Valid - User exists in system"},
            {"label": "Password Verification", "value": "Correct - Password hash matches"},
            {"label": "Badge Authentication", "value": "Valid - Badge number matches employee"},
            {"label": "Account Status", "value": "Active - Account is enabled"},
            {"label": "Role Authorization", "value": "Authorized - RN role has ED access"},
        ]
        for row in authentication_checks:
            assert get_text(self.driver, row["label"]) == row["value"]

        # And the access attempt is logged with details:
        #   | Audit Log Entry       | Information Recorded                       |
        #   | User ID               | sjohnson (Sarah Johnson)                  |
        #   | Login Timestamp       | 2025-06-24 08:00:15                       |
        #   | Workstation ID        | WS-ED-05                                  |
        #   | IP Address            | 192.168.1.105                            |
        #   | Authentication Method | Username/Password + Badge Scan            |
        #   | Login Success         | True                                      |
        #   | Session ID            | SES-ABC123DEF456                          |
        access_attempt_log = [
            {"label": "User ID", "value": "sjohnson (Sarah Johnson)"},
            {"label": "Login Timestamp", "value": "2025-06-24 08:00:15"},
            {"label": "Workstation ID", "value": "WS-ED-05"},
            {"label": "IP Address", "value": "192.168.1.105"},
            {"label": "Authentication Method", "value": "Username/Password + Badge Scan"},
            {"label": "Login Success", "value": "True"},
            {"label": "Session ID", "value": "SES-ABC123DEF456"},
        ]
        for row in access_attempt_log:
            assert get_text(self.driver, row["label"]) == row["value"]

        # And my personalized dashboard is displayed:
        #   | Dashboard Section     | Nurse-Specific Content                     |
        #   | Patient Assignment    | Current patients assigned to Sarah Johnson |
        #   | Task List            | Medication administration, vitals due      |
        #   | Alerts/Notifications | Critical lab values, patient call lights  |
        #   | Quick Access Tools   | Medication lookup, dosage calculator       |
        #   | Shift Information    | Shift start: 07:00, Break schedule        |
        #   | Department Status    | Current census, bed availability          |
        dashboard_sections = [
            {"label": "Patient Assignment", "value": "Current patients assigned to Sarah Johnson"},
            {"label": "Task List", "value": "Medication administration, vitals due"},
            {"label": "Alerts/Notifications", "value": "Critical lab values, patient call lights"},
            {"label": "Quick Access Tools", "value": "Medication lookup, dosage calculator"},
            {"label": "Shift Information", "value": "Shift start: 07:00, Break schedule"},
            {"label": "Department Status", "value": "Current census, bed availability"},
        ]
        for row in dashboard_sections:
            assert get_text(self.driver, row["label"]) == row["value"]

    def test_failed_login_attempt_with_incorrect_password(self):
        # Given I am attempting to log in as "Nurse Mike Chen"
        # And my username is "mchen"
        # And my correct password is stored in the system
        # When I enter my username "mchen"
        self.driver.get(f"{BASE_URL}/login")
        identity_field = WebDriverWait(self.driver, 10).until(
            EC.presence_of_element_located((By.CSS_SELECTOR, '[data-testid="login-identity"]'))
        )
        identity_field.send_keys("mchen")
        # And I enter an incorrect password "wrongpassword"
        fill_field(self.driver, "Password", "wrongpassword")
        # And I scan my valid badge "BADGE-54321"
        fill_field(self.driver, "Badge Number", "BADGE-54321")
        # And I click the "Login" button
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="login-submit"]').click()

        # Then the system rejects the authentication:
        #   | Authentication Check  | Verification Result                        |
        #   | Username Validation   | Valid - User exists                       |
        #   | Password Verification | Failed - Password does not match          |
        #   | Badge Authentication  | Valid - Badge number correct              |
        #   | Overall Result        | Authentication Failed                     |
        authentication_checks = [
            {"label": "Username Validation", "value": "Valid - User exists"},
            {"label": "Password Verification", "value": "Failed - Password does not match"},
            {"label": "Badge Authentication", "value": "Valid - Badge number correct"},
            {"label": "Overall Result", "value": "Authentication Failed"},
        ]
        for row in authentication_checks:
            assert get_text(self.driver, row["label"]) == row["value"]

        # And a security alert is logged:
        #   | Security Log Entry    | Information Recorded                       |
        #   | User ID               | mchen (Mike Chen)                         |
        #   | Failed Login Time     | 2025-06-24 08:15:32                       |
        #   | Workstation ID        | WS-ED-03                                  |
        #   | IP Address            | 192.168.1.103                            |
        #   | Failure Reason        | Incorrect Password                        |
        #   | Attempt Count         | 1 of 3 allowed attempts                   |
        security_log_entries = [
            {"label": "User ID", "value": "mchen (Mike Chen)"},
            {"label": "Failed Login Time", "value": "2025-06-24 08:15:32"},
            {"label": "Workstation ID", "value": "WS-ED-03"},
            {"label": "IP Address", "value": "192.168.1.103"},
            {"label": "Failure Reason", "value": "Incorrect Password"},
            {"label": "Attempt Count", "value": "1 of 3 allowed attempts"},
        ]
        for row in security_log_entries:
            assert get_text(self.driver, row["label"]) == row["value"]

        # And an error message is displayed:
        #   | Error Message         | "Invalid credentials. Please check your username and password." |
        #   | Security Notice       | "2 attempts remaining before account lockout"                   |
        assert (
            get_text(self.driver, "Error Message")
            == "Invalid credentials. Please check your username and password."
        )
        assert get_text(self.driver, "Security Notice") == "2 attempts remaining before account lockout"

        # And I remain on the login screen to retry authentication
        login_form = wait_for_test_id(self.driver, "Login Form")
        assert login_form.is_displayed()

    def test_account_lockout_after_multiple_failed_login_attempts(self):
        # Given I am "Dr. Amanda Wilson" attempting to log in
        # And my account allows 3 failed login attempts before lockout
        # And I have already made 2 failed login attempts today
        # When I enter my username "awilson"
        self.driver.get(f"{BASE_URL}/login")
        identity_field = WebDriverWait(self.driver, 10).until(
            EC.presence_of_element_located((By.CSS_SELECTOR, '[data-testid="login-identity"]'))
        )
        identity_field.send_keys("awilson")
        # And I enter another incorrect password
        fill_field(self.driver, "Password", "another-wrong-password")
        # And I scan my badge and click login
        fill_field(self.driver, "Badge Number", "BADGE-11111")
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="login-submit"]').click()

        # Then the system locks my account:
        #   | Lockout Response      | Security Action                            |
        #   | Account Status        | Locked - Exceeded maximum failed attempts |
        #   | Lockout Duration      | 30 minutes automatic unlock               |
        #   | Manual Override       | IT Security can unlock immediately        |
        lockout_response = [
            {"label": "Account Status", "value": "Locked - Exceeded maximum failed attempts"},
            {"label": "Lockout Duration", "value": "30 minutes automatic unlock"},
            {"label": "Manual Override", "value": "IT Security can unlock immediately"},
        ]
        for row in lockout_response:
            assert get_text(self.driver, row["label"]) == row["value"]

        # And a security incident is logged:
        #   | Security Incident     | Details                                    |
        #   | Event Type            | Account Lockout - Excessive Failed Attempts|
        #   | User Account          | awilson (Dr. Amanda Wilson)               |
        #   | Lockout Time          | 2025-06-24 14:22:18                       |
        #   | Failed Attempts       | 3 consecutive failures                     |
        #   | Workstation ID        | WS-ED-07                                  |
        security_incident = [
            {"label": "Event Type", "value": "Account Lockout - Excessive Failed Attempts"},
            {"label": "User Account", "value": "awilson (Dr. Amanda Wilson)"},
            {"label": "Lockout Time", "value": "2025-06-24 14:22:18"},
            {"label": "Failed Attempts", "value": "3 consecutive failures"},
            {"label": "Workstation ID", "value": "WS-ED-07"},
        ]
        for row in security_incident:
            assert get_text(self.driver, row["label"]) == row["value"]

        # And security notifications are sent:
        #   | Notification Target   | Alert Content                              |
        #   | IT Security Team      | Account lockout alert for Dr. Wilson      |
        #   | Department Supervisor | Staff member unable to access system      |
        #   | User Email            | Account locked - contact IT for assistance|
        security_notifications = [
            {"label": "IT Security Team", "value": "Account lockout alert for Dr. Wilson"},
            {"label": "Department Supervisor", "value": "Staff member unable to access system"},
            {"label": "User Email", "value": "Account locked - contact IT for assistance"},
        ]
        for row in security_notifications:
            assert get_text(self.driver, row["label"]) == row["value"]

        # And a lockout message is displayed:
        #   | Lockout Message       | "Account temporarily locked due to multiple failed login attempts. Contact IT Security or wait 30 minutes." |
        assert (
            get_text(self.driver, "Lockout Message")
            == "Account temporarily locked due to multiple failed login attempts. "
            "Contact IT Security or wait 30 minutes."
        )

    def test_login_with_expired_password_requiring_password_reset(self):
        # Given I am "Nurse Patricia Martinez" with valid credentials
        # And my password expired 5 days ago according to policy
        # And the system requires password changes every 90 days
        # When I enter my username "pmartinez"
        self.driver.get(f"{BASE_URL}/login")
        identity_field = WebDriverWait(self.driver, 10).until(
            EC.presence_of_element_located((By.CSS_SELECTOR, '[data-testid="login-identity"]'))
        )
        identity_field.send_keys("pmartinez")
        # And I enter my current (expired) password
        fill_field(self.driver, "Password", "ExpiredPassword123!")
        # And I scan my badge successfully
        fill_field(self.driver, "Badge Number", "BADGE-24680")
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="login-submit"]').click()

        # Then the system identifies the expired password:
        #   | Password Check        | Status                                     |
        #   | Password Validity     | Expired - 5 days past expiration         |
        #   | Grace Period          | Exceeded - No grace logins remaining      |
        #   | Password Age          | 95 days old (5 days over 90-day limit)   |
        password_checks = [
            {"label": "Password Validity", "value": "Expired - 5 days past expiration"},
            {"label": "Grace Period", "value": "Exceeded - No grace logins remaining"},
            {"label": "Password Age", "value": "95 days old (5 days over 90-day limit)"},
        ]
        for row in password_checks:
            assert get_text(self.driver, row["label"]) == row["value"]

        # And the password reset workflow is initiated:
        #   | Reset Process         | Required Actions                           |
        #   | Identity Verification | Additional security questions prompted     |
        #   | New Password Entry    | Password must meet complexity requirements |
        #   | Password Confirmation | Confirm new password entry                |
        #   | Security Questions    | Update security questions if needed       |
        reset_process = [
            {"label": "Identity Verification", "value": "Additional security questions prompted"},
            {"label": "New Password Entry", "value": "Password must meet complexity requirements"},
            {"label": "Password Confirmation", "value": "Confirm new password entry"},
            {"label": "Security Questions", "value": "Update security questions if needed"},
        ]
        for row in reset_process:
            assert get_text(self.driver, row["label"]) == row["value"]

        # And password policy requirements are displayed:
        #   | Policy Requirement    | Specification                              |
        #   | Minimum Length        | 12 characters                             |
        #   | Character Types       | Upper, lower, number, special character   |
        #   | Password History      | Cannot reuse last 12 passwords           |
        #   | Common Words          | Cannot use dictionary words or personal info|
        policy_requirements = [
            {"label": "Minimum Length", "value": "12 characters"},
            {"label": "Character Types", "value": "Upper, lower, number, special character"},
            {"label": "Password History", "value": "Cannot reuse last 12 passwords"},
            {"label": "Common Words", "value": "Cannot use dictionary words or personal info"},
        ]
        for row in policy_requirements:
            assert get_text(self.driver, row["label"]) == row["value"]

        # And upon successful password reset, normal login proceeds
        assert "success" in get_text(self.driver, "Login Status").lower()

    def test_role_based_dashboard_customization_after_successful_login(self):
        # Given multiple users with different roles log in successfully
        # When "Dr. Emily Rodriguez" (Emergency Physician) logs in
        login(self.driver, "Dr. Emily Rodriguez")

        # Then her physician dashboard is displayed with:
        #   | Physician Dashboard   | Specialized Content                        |
        #   | Patient Queue         | Patients waiting to be seen by priority    |
        #   | Active Orders         | Lab results, imaging pending review       |
        #   | Critical Alerts       | Abnormal vitals, critical lab values      |
        #   | Decision Support      | Clinical guidelines, drug interactions    |
        #   | Documentation Tools   | Templates for common conditions           |
        physician_dashboard = [
            {"label": "Patient Queue", "value": "Patients waiting to be seen by priority"},
            {"label": "Active Orders", "value": "Lab results, imaging pending review"},
            {"label": "Critical Alerts", "value": "Abnormal vitals, critical lab values"},
            {"label": "Decision Support", "value": "Clinical guidelines, drug interactions"},
            {"label": "Documentation Tools", "value": "Templates for common conditions"},
        ]
        for row in physician_dashboard:
            assert get_text(self.driver, row["label"]) == row["value"]

        # When "Charge Nurse Williams" logs in
        login(self.driver, "Charge Nurse Williams")

        # Then her charge nurse dashboard shows:
        #   | Charge Nurse Dashboard| Management-Focused Content                 |
        #   | Department Overview   | Bed status, staff assignments, census     |
        #   | Resource Management   | Equipment status, supply levels           |
        #   | Staff Coordination    | Break schedules, assignments, coverage    |
        #   | Quality Metrics       | Wait times, patient satisfaction, safety  |
        #   | Administrative Tasks  | Reporting, scheduling, policy updates     |
        charge_nurse_dashboard = [
            {"label": "Department Overview", "value": "Bed status, staff assignments, census"},
            {"label": "Resource Management", "value": "Equipment status, supply levels"},
            {"label": "Staff Coordination", "value": "Break schedules, assignments, coverage"},
            {"label": "Quality Metrics", "value": "Wait times, patient satisfaction, safety"},
            {"label": "Administrative Tasks", "value": "Reporting, scheduling, policy updates"},
        ]
        for row in charge_nurse_dashboard:
            assert get_text(self.driver, row["label"]) == row["value"]

        # When "Tech Support Anderson" logs in
        login(self.driver, "Tech Support Anderson")

        # Then his technical dashboard displays:
        #   | Technical Dashboard   | System Administration Content              |
        #   | System Status         | Server health, network connectivity       |
        #   | User Management       | Account status, permission changes        |
        #   | Audit Logs           | System access, security events            |
        #   | Maintenance Tools     | Backup status, system updates             |
        technical_dashboard = [
            {"label": "System Status", "value": "Server health, network connectivity"},
            {"label": "User Management", "value": "Account status, permission changes"},
            {"label": "Audit Logs", "value": "System access, security events"},
            {"label": "Maintenance Tools", "value": "Backup status, system updates"},
        ]
        for row in technical_dashboard:
            assert get_text(self.driver, row["label"]) == row["value"]

    def test_mobile_device_authentication_with_additional_security(self):
        # Given I am using the mobile ED app on my smartphone
        # And mobile access requires enhanced security measures
        # When I attempt to log in on my mobile device
        self.driver.get(f"{BASE_URL}/login?viewport=mobile")
        WebDriverWait(self.driver, 10).until(
            EC.presence_of_element_located((By.CSS_SELECTOR, '[data-testid="login-identity"]'))
        )

        # Then additional authentication factors are required:
        #   | Mobile Security Factor| Requirement                                |
        #   | Device Registration   | Device must be registered with IT          |
        #   | Biometric Auth        | Fingerprint or face recognition required   |
        #   | Location Verification | GPS confirms user is within hospital grounds|
        #   | Time-based Token      | 6-digit code from authenticator app       |
        mobile_security_factors = [
            {"label": "Device Registration", "value": "Device must be registered with IT"},
            {"label": "Biometric Auth", "value": "Fingerprint or face recognition required"},
            {"label": "Location Verification", "value": "GPS confirms user is within hospital grounds"},
            {"label": "Time-based Token", "value": "6-digit code from authenticator app"},
        ]
        for row in mobile_security_factors:
            assert get_text(self.driver, row["label"]) == row["value"]

        # And I complete multi-factor authentication:
        #   | MFA Step              | Verification Process                       |
        #   | Username/Password     | Standard credential verification           |
        #   | Biometric Scan        | Fingerprint verified against enrolled pattern|
        #   | Location Check        | GPS coordinates within allowed radius      |
        #   | Time Token            | Authenticator app code validated          |
        mfa_steps = [
            {"label": "Username/Password", "value": "Standard credential verification"},
            {"label": "Biometric Scan", "value": "Fingerprint verified against enrolled pattern"},
            {"label": "Location Check", "value": "GPS coordinates within allowed radius"},
            {"label": "Time Token", "value": "Authenticator app code validated"},
        ]
        for row in mfa_steps:
            assert get_text(self.driver, row["label"]) == row["value"]

        # And mobile-specific security controls are applied:
        #   | Mobile Control        | Security Measure                           |
        #   | Session Timeout       | 15-minute inactivity timeout             |
        #   | Screen Lock           | Auto-lock after 2 minutes idle           |
        #   | Data Encryption       | All data encrypted on device             |
        #   | Remote Wipe           | IT can remotely clear data if device lost |
        mobile_controls = [
            {"label": "Session Timeout", "value": "15-minute inactivity timeout"},
            {"label": "Screen Lock", "value": "Auto-lock after 2 minutes idle"},
            {"label": "Data Encryption", "value": "All data encrypted on device"},
            {"label": "Remote Wipe", "value": "IT can remotely clear data if device lost"},
        ]
        for row in mobile_controls:
            assert get_text(self.driver, row["label"]) == row["value"]

    def test_emergency_override_authentication_during_system_issues(self):
        # Given the primary authentication system is experiencing technical difficulties
        # And there is a patient emergency requiring immediate system access
        # And I am "Dr. Lisa Chen" needing urgent access to patient records
        # The emergency override option lives on the sign-in screen.
        self.driver.get(f"{BASE_URL}/login")
        WebDriverWait(self.driver, 10).until(
            EC.presence_of_element_located((By.CSS_SELECTOR, '[data-testid="login-identity"]'))
        )
        # When I request emergency override access
        self.driver.find_element(By.CSS_SELECTOR, '[data-testid="emergency-override-button"]').click()

        # Then the emergency authentication protocol is activated:
        #   | Emergency Protocol    | Override Process                           |
        #   | Identity Verification | Manual verification by IT Security         |
        #   | Supervisor Approval   | Department head authorization required     |
        #   | Time-Limited Access   | 2-hour temporary access granted           |
        #   | Enhanced Monitoring   | All actions logged for later review       |
        emergency_protocol = [
            {"label": "Identity Verification", "value": "Manual verification by IT Security"},
            {"label": "Supervisor Approval", "value": "Department head authorization required"},
            {"label": "Time-Limited Access", "value": "2-hour temporary access granted"},
            {"label": "Enhanced Monitoring", "value": "All actions logged for later review"},
        ]
        for row in emergency_protocol:
            assert get_text(self.driver, row["label"]) == row["value"]

        # And emergency access is granted with restrictions:
        #   | Access Limitation     | Restriction Details                        |
        #   | Time Limit            | Access expires automatically in 2 hours   |
        #   | Function Restrictions | Read-only access to critical patient data |
        #   | Audit Trail          | Enhanced logging of all actions           |
        #   | Supervisor Oversight  | Real-time monitoring of override usage     |
        access_limitations = [
            {"label": "Time Limit", "value": "Access expires automatically in 2 hours"},
            {"label": "Function Restrictions", "value": "Read-only access to critical patient data"},
            {"label": "Audit Trail", "value": "Enhanced logging of all actions"},
            {"label": "Supervisor Oversight", "value": "Real-time monitoring of override usage"},
        ]
        for row in access_limitations:
            assert get_text(self.driver, row["label"]) == row["value"]

        # And emergency access usage is documented:
        #   | Emergency Documentation| Required Information                      |
        #   | Medical Justification | Patient condition requiring urgent access |
        #   | Approving Authority   | Name and title of authorizing supervisor  |
        #   | Access Duration       | Exact start and end times of override use |
        #   | Actions Performed     | Detailed log of all system activities    |
        emergency_documentation = [
            {"label": "Medical Justification", "value": "Patient condition requiring urgent access"},
            {"label": "Approving Authority", "value": "Name and title of authorizing supervisor"},
            {"label": "Access Duration", "value": "Exact start and end times of override use"},
            {"label": "Actions Performed", "value": "Detailed log of all system activities"},
        ]
        for row in emergency_documentation:
            assert get_text(self.driver, row["label"]) == row["value"]
