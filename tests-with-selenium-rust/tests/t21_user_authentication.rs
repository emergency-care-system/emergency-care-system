// Selenium WebDriver + libtest-mimic test for
// tests-with-given-when-then-features/21-user-authentication.feature
// (equivalent to tests-with-selenium-javascript/21-user-authentication.test.js).
//
// This feature's whole subject is the login/authentication flow itself, so
// most scenarios drive the login page directly with the shared data-testid
// contract (login-identity, login-submit, app-root, see tests/support/login.rs)
// rather than the login() helper, which assumes success. Other fields
// (password, badge number, dates, etc.) are assumed to expose data-testid
// attributes matching their Gherkin field label (kebab-cased, see
// tests/support/fields.rs).

#![allow(unused_variables)]

mod support;
use support::*;

async fn background(driver: &WebDriver) -> WebDriverResult<()> {
    // Background:
    //   Given the emergency care system is operational
    //   And the authentication module is active
    //   And the badge scanning system is functional
    //   And user credentials database is accessible
    //   And audit logging is enabled
    verify_system_is_operational(driver).await?;
    // The remaining Background steps describe pre-seeded system state
    // (authentication module, badge scanning system, credentials database,
    // and audit logging readiness) assumed to already be configured in the
    // test environment.
    Ok(())
}

fn scenario_01_successful_nurse_login_with_username_password_and_badge_scan(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

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
        driver.goto(format!("{}{}", base_url(), "/login")).await?;
        let identity_field = wait_located(driver, By::Css("[data-testid=\"login-identity\"]"), 10000).await?;
        identity_field.send_keys("sjohnson").await?;
        // And I enter my password in the password field
        fill_field(driver, "Password", "CorrectPassword123!").await?;
        // And I scan my badge "BADGE-67890" using the badge reader
        fill_field(driver, "Badge Number", "BADGE-67890").await?;
        // And I click the "Login" button
        driver.find(By::Css("[data-testid=\"login-submit\"]")).await?.click().await?;

        // Then the system authenticates my credentials successfully:
        //   | Authentication Check  | Verification Result                        |
        //   | Username Validation   | Valid - User exists in system             |
        //   | Password Verification | Correct - Password hash matches           |
        //   | Badge Authentication  | Valid - Badge number matches employee     |
        //   | Account Status        | Active - Account is enabled              |
        //   | Role Authorization    | Authorized - RN role has ED access       |
        let authentication_checks = vec![
            row([("label", "Username Validation"), ("value", "Valid - User exists in system")]),
            row([("label", "Password Verification"), ("value", "Correct - Password hash matches")]),
            row([("label", "Badge Authentication"), ("value", "Valid - Badge number matches employee")]),
            row([("label", "Account Status"), ("value", "Active - Account is enabled")]),
            row([("label", "Role Authorization"), ("value", "Authorized - RN role has ED access")]),
        ];
        for row_data in &authentication_checks {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(driver, label).await?, value);
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
        let access_attempt_log = vec![
            row([("label", "User ID"), ("value", "sjohnson (Sarah Johnson)")]),
            row([("label", "Login Timestamp"), ("value", "2025-06-24 08:00:15")]),
            row([("label", "Workstation ID"), ("value", "WS-ED-05")]),
            row([("label", "IP Address"), ("value", "192.168.1.105")]),
            row([("label", "Authentication Method"), ("value", "Username/Password + Badge Scan")]),
            row([("label", "Login Success"), ("value", "True")]),
            row([("label", "Session ID"), ("value", "SES-ABC123DEF456")]),
        ];
        for row_data in &access_attempt_log {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(driver, label).await?, value);
        }

        // And my personalized dashboard is displayed:
        //   | Dashboard Section     | Nurse-Specific Content                     |
        //   | Patient Assignment    | Current patients assigned to Sarah Johnson |
        //   | Task List            | Medication administration, vitals due      |
        //   | Alerts/Notifications | Critical lab values, patient call lights  |
        //   | Quick Access Tools   | Medication lookup, dosage calculator       |
        //   | Shift Information    | Shift start: 07:00, Break schedule        |
        //   | Department Status    | Current census, bed availability          |
        let dashboard_sections = vec![
            row([("label", "Patient Assignment"), ("value", "Current patients assigned to Sarah Johnson")]),
            row([("label", "Task List"), ("value", "Medication administration, vitals due")]),
            row([("label", "Alerts/Notifications"), ("value", "Critical lab values, patient call lights")]),
            row([("label", "Quick Access Tools"), ("value", "Medication lookup, dosage calculator")]),
            row([("label", "Shift Information"), ("value", "Shift start: 07:00, Break schedule")]),
            row([("label", "Department Status"), ("value", "Current census, bed availability")]),
        ];
        for row_data in &dashboard_sections {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(driver, label).await?, value);
        }
        Ok(())
    })
}

fn scenario_02_failed_login_attempt_with_incorrect_password(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given I am attempting to log in as "Nurse Mike Chen"
        // And my username is "mchen"
        // And my correct password is stored in the system
        // When I enter my username "mchen"
        driver.goto(format!("{}{}", base_url(), "/login")).await?;
        let identity_field = wait_located(driver, By::Css("[data-testid=\"login-identity\"]"), 10000).await?;
        identity_field.send_keys("mchen").await?;
        // And I enter an incorrect password "wrongpassword"
        fill_field(driver, "Password", "wrongpassword").await?;
        // And I scan my valid badge "BADGE-54321"
        fill_field(driver, "Badge Number", "BADGE-54321").await?;
        // And I click the "Login" button
        driver.find(By::Css("[data-testid=\"login-submit\"]")).await?.click().await?;

        // Then the system rejects the authentication:
        //   | Authentication Check  | Verification Result                        |
        //   | Username Validation   | Valid - User exists                       |
        //   | Password Verification | Failed - Password does not match          |
        //   | Badge Authentication  | Valid - Badge number correct              |
        //   | Overall Result        | Authentication Failed                     |
        let authentication_checks = vec![
            row([("label", "Username Validation"), ("value", "Valid - User exists")]),
            row([("label", "Password Verification"), ("value", "Failed - Password does not match")]),
            row([("label", "Badge Authentication"), ("value", "Valid - Badge number correct")]),
            row([("label", "Overall Result"), ("value", "Authentication Failed")]),
        ];
        for row_data in &authentication_checks {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(driver, label).await?, value);
        }

        // And a security alert is logged:
        //   | Security Log Entry    | Information Recorded                       |
        //   | User ID               | mchen (Mike Chen)                         |
        //   | Failed Login Time     | 2025-06-24 08:15:32                       |
        //   | Workstation ID        | WS-ED-03                                  |
        //   | IP Address            | 192.168.1.103                            |
        //   | Failure Reason        | Incorrect Password                        |
        //   | Attempt Count         | 1 of 3 allowed attempts                   |
        let security_log_entries = vec![
            row([("label", "User ID"), ("value", "mchen (Mike Chen)")]),
            row([("label", "Failed Login Time"), ("value", "2025-06-24 08:15:32")]),
            row([("label", "Workstation ID"), ("value", "WS-ED-03")]),
            row([("label", "IP Address"), ("value", "192.168.1.103")]),
            row([("label", "Failure Reason"), ("value", "Incorrect Password")]),
            row([("label", "Attempt Count"), ("value", "1 of 3 allowed attempts")]),
        ];
        for row_data in &security_log_entries {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(driver, label).await?, value);
        }

        // And an error message is displayed:
        //   | Error Message         | "Invalid credentials. Please check your username and password." |
        //   | Security Notice       | "2 attempts remaining before account lockout"                   |
        assert_eq!(get_text(driver, "Error Message").await?, "Invalid credentials. Please check your username and password.");
        assert_eq!(get_text(driver, "Security Notice").await?, "2 attempts remaining before account lockout");

        // And I remain on the login screen to retry authentication
        let login_form = wait_for_test_id(driver, "Login Form").await?;
        assert!(login_form.is_displayed().await?);
        Ok(())
    })
}

fn scenario_03_account_lockout_after_multiple_failed_login_attempts(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given I am "Dr. Amanda Wilson" attempting to log in
        // And my account allows 3 failed login attempts before lockout
        // And I have already made 2 failed login attempts today
        // When I enter my username "awilson"
        driver.goto(format!("{}{}", base_url(), "/login")).await?;
        let identity_field = wait_located(driver, By::Css("[data-testid=\"login-identity\"]"), 10000).await?;
        identity_field.send_keys("awilson").await?;
        // And I enter another incorrect password
        fill_field(driver, "Password", "another-wrong-password").await?;
        // And I scan my badge and click login
        fill_field(driver, "Badge Number", "BADGE-11111").await?;
        driver.find(By::Css("[data-testid=\"login-submit\"]")).await?.click().await?;

        // Then the system locks my account:
        //   | Lockout Response      | Security Action                            |
        //   | Account Status        | Locked - Exceeded maximum failed attempts |
        //   | Lockout Duration      | 30 minutes automatic unlock               |
        //   | Manual Override       | IT Security can unlock immediately        |
        let lockout_response = vec![
            row([("label", "Account Status"), ("value", "Locked - Exceeded maximum failed attempts")]),
            row([("label", "Lockout Duration"), ("value", "30 minutes automatic unlock")]),
            row([("label", "Manual Override"), ("value", "IT Security can unlock immediately")]),
        ];
        for row_data in &lockout_response {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(driver, label).await?, value);
        }

        // And a security incident is logged:
        //   | Security Incident     | Details                                    |
        //   | Event Type            | Account Lockout - Excessive Failed Attempts|
        //   | User Account          | awilson (Dr. Amanda Wilson)               |
        //   | Lockout Time          | 2025-06-24 14:22:18                       |
        //   | Failed Attempts       | 3 consecutive failures                     |
        //   | Workstation ID        | WS-ED-07                                  |
        let security_incident = vec![
            row([("label", "Event Type"), ("value", "Account Lockout - Excessive Failed Attempts")]),
            row([("label", "User Account"), ("value", "awilson (Dr. Amanda Wilson)")]),
            row([("label", "Lockout Time"), ("value", "2025-06-24 14:22:18")]),
            row([("label", "Failed Attempts"), ("value", "3 consecutive failures")]),
            row([("label", "Workstation ID"), ("value", "WS-ED-07")]),
        ];
        for row_data in &security_incident {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(driver, label).await?, value);
        }

        // And security notifications are sent:
        //   | Notification Target   | Alert Content                              |
        //   | IT Security Team      | Account lockout alert for Dr. Wilson      |
        //   | Department Supervisor | Staff member unable to access system      |
        //   | User Email            | Account locked - contact IT for assistance|
        let security_notifications = vec![
            row([("label", "IT Security Team"), ("value", "Account lockout alert for Dr. Wilson")]),
            row([("label", "Department Supervisor"), ("value", "Staff member unable to access system")]),
            row([("label", "User Email"), ("value", "Account locked - contact IT for assistance")]),
        ];
        for row_data in &security_notifications {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(driver, label).await?, value);
        }

        // And a lockout message is displayed:
        //   | Lockout Message       | "Account temporarily locked due to multiple failed login attempts. Contact IT Security or wait 30 minutes." |
        assert_eq!(get_text(driver, "Lockout Message").await?, "Account temporarily locked due to multiple failed login attempts. Contact IT Security or wait 30 minutes.");
        Ok(())
    })
}

fn scenario_04_login_with_expired_password_requiring_password_reset(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given I am "Nurse Patricia Martinez" with valid credentials
        // And my password expired 5 days ago according to policy
        // And the system requires password changes every 90 days
        // When I enter my username "pmartinez"
        driver.goto(format!("{}{}", base_url(), "/login")).await?;
        let identity_field = wait_located(driver, By::Css("[data-testid=\"login-identity\"]"), 10000).await?;
        identity_field.send_keys("pmartinez").await?;
        // And I enter my current (expired) password
        fill_field(driver, "Password", "ExpiredPassword123!").await?;
        // And I scan my badge successfully
        fill_field(driver, "Badge Number", "BADGE-24680").await?;
        driver.find(By::Css("[data-testid=\"login-submit\"]")).await?.click().await?;

        // Then the system identifies the expired password:
        //   | Password Check        | Status                                     |
        //   | Password Validity     | Expired - 5 days past expiration         |
        //   | Grace Period          | Exceeded - No grace logins remaining      |
        //   | Password Age          | 95 days old (5 days over 90-day limit)   |
        let password_checks = vec![
            row([("label", "Password Validity"), ("value", "Expired - 5 days past expiration")]),
            row([("label", "Grace Period"), ("value", "Exceeded - No grace logins remaining")]),
            row([("label", "Password Age"), ("value", "95 days old (5 days over 90-day limit)")]),
        ];
        for row_data in &password_checks {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(driver, label).await?, value);
        }

        // And the password reset workflow is initiated:
        //   | Reset Process         | Required Actions                           |
        //   | Identity Verification | Additional security questions prompted     |
        //   | New Password Entry    | Password must meet complexity requirements |
        //   | Password Confirmation | Confirm new password entry                |
        //   | Security Questions    | Update security questions if needed       |
        let reset_process = vec![
            row([("label", "Identity Verification"), ("value", "Additional security questions prompted")]),
            row([("label", "New Password Entry"), ("value", "Password must meet complexity requirements")]),
            row([("label", "Password Confirmation"), ("value", "Confirm new password entry")]),
            row([("label", "Security Questions"), ("value", "Update security questions if needed")]),
        ];
        for row_data in &reset_process {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(driver, label).await?, value);
        }

        // And password policy requirements are displayed:
        //   | Policy Requirement    | Specification                              |
        //   | Minimum Length        | 12 characters                             |
        //   | Character Types       | Upper, lower, number, special character   |
        //   | Password History      | Cannot reuse last 12 passwords           |
        //   | Common Words          | Cannot use dictionary words or personal info|
        let policy_requirements = vec![
            row([("label", "Minimum Length"), ("value", "12 characters")]),
            row([("label", "Character Types"), ("value", "Upper, lower, number, special character")]),
            row([("label", "Password History"), ("value", "Cannot reuse last 12 passwords")]),
            row([("label", "Common Words"), ("value", "Cannot use dictionary words or personal info")]),
        ];
        for row_data in &policy_requirements {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(driver, label).await?, value);
        }

        // And upon successful password reset, normal login proceeds
        assert_match(&get_text(driver, "Login Status").await?, r"success", true);
        Ok(())
    })
}

fn scenario_05_role_based_dashboard_customization_after_successful_login(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given multiple users with different roles log in successfully
        // When "Dr. Emily Rodriguez" (Emergency Physician) logs in
        login(driver, "Dr. Emily Rodriguez", false).await?;

        // Then her physician dashboard is displayed with:
        //   | Physician Dashboard   | Specialized Content                        |
        //   | Patient Queue         | Patients waiting to be seen by priority    |
        //   | Active Orders         | Lab results, imaging pending review       |
        //   | Critical Alerts       | Abnormal vitals, critical lab values      |
        //   | Decision Support      | Clinical guidelines, drug interactions    |
        //   | Documentation Tools   | Templates for common conditions           |
        let physician_dashboard = vec![
            row([("label", "Patient Queue"), ("value", "Patients waiting to be seen by priority")]),
            row([("label", "Active Orders"), ("value", "Lab results, imaging pending review")]),
            row([("label", "Critical Alerts"), ("value", "Abnormal vitals, critical lab values")]),
            row([("label", "Decision Support"), ("value", "Clinical guidelines, drug interactions")]),
            row([("label", "Documentation Tools"), ("value", "Templates for common conditions")]),
        ];
        for row_data in &physician_dashboard {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(driver, label).await?, value);
        }

        // When "Charge Nurse Williams" logs in
        login(driver, "Charge Nurse Williams", false).await?;

        // Then her charge nurse dashboard shows:
        //   | Charge Nurse Dashboard| Management-Focused Content                 |
        //   | Department Overview   | Bed status, staff assignments, census     |
        //   | Resource Management   | Equipment status, supply levels           |
        //   | Staff Coordination    | Break schedules, assignments, coverage    |
        //   | Quality Metrics       | Wait times, patient satisfaction, safety  |
        //   | Administrative Tasks  | Reporting, scheduling, policy updates     |
        let charge_nurse_dashboard = vec![
            row([("label", "Department Overview"), ("value", "Bed status, staff assignments, census")]),
            row([("label", "Resource Management"), ("value", "Equipment status, supply levels")]),
            row([("label", "Staff Coordination"), ("value", "Break schedules, assignments, coverage")]),
            row([("label", "Quality Metrics"), ("value", "Wait times, patient satisfaction, safety")]),
            row([("label", "Administrative Tasks"), ("value", "Reporting, scheduling, policy updates")]),
        ];
        for row_data in &charge_nurse_dashboard {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(driver, label).await?, value);
        }

        // When "Tech Support Anderson" logs in
        login(driver, "Tech Support Anderson", false).await?;

        // Then his technical dashboard displays:
        //   | Technical Dashboard   | System Administration Content              |
        //   | System Status         | Server health, network connectivity       |
        //   | User Management       | Account status, permission changes        |
        //   | Audit Logs           | System access, security events            |
        //   | Maintenance Tools     | Backup status, system updates             |
        let technical_dashboard = vec![
            row([("label", "System Status"), ("value", "Server health, network connectivity")]),
            row([("label", "User Management"), ("value", "Account status, permission changes")]),
            row([("label", "Audit Logs"), ("value", "System access, security events")]),
            row([("label", "Maintenance Tools"), ("value", "Backup status, system updates")]),
        ];
        for row_data in &technical_dashboard {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(driver, label).await?, value);
        }
        Ok(())
    })
}

fn scenario_06_mobile_device_authentication_with_additional_security(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given I am using the mobile ED app on my smartphone
        // And mobile access requires enhanced security measures
        // When I attempt to log in on my mobile device
        driver.goto(format!("{}{}", base_url(), "/login?viewport=mobile")).await?;
        wait_located(driver, By::Css("[data-testid=\"login-identity\"]"), 10000).await?;

        // Then additional authentication factors are required:
        //   | Mobile Security Factor| Requirement                                |
        //   | Device Registration   | Device must be registered with IT          |
        //   | Biometric Auth        | Fingerprint or face recognition required   |
        //   | Location Verification | GPS confirms user is within hospital grounds|
        //   | Time-based Token      | 6-digit code from authenticator app       |
        let mobile_security_factors = vec![
            row([("label", "Device Registration"), ("value", "Device must be registered with IT")]),
            row([("label", "Biometric Auth"), ("value", "Fingerprint or face recognition required")]),
            row([("label", "Location Verification"), ("value", "GPS confirms user is within hospital grounds")]),
            row([("label", "Time-based Token"), ("value", "6-digit code from authenticator app")]),
        ];
        for row_data in &mobile_security_factors {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(driver, label).await?, value);
        }

        // And I complete multi-factor authentication:
        //   | MFA Step              | Verification Process                       |
        //   | Username/Password     | Standard credential verification           |
        //   | Biometric Scan        | Fingerprint verified against enrolled pattern|
        //   | Location Check        | GPS coordinates within allowed radius      |
        //   | Time Token            | Authenticator app code validated          |
        let mfa_steps = vec![
            row([("label", "Username/Password"), ("value", "Standard credential verification")]),
            row([("label", "Biometric Scan"), ("value", "Fingerprint verified against enrolled pattern")]),
            row([("label", "Location Check"), ("value", "GPS coordinates within allowed radius")]),
            row([("label", "Time Token"), ("value", "Authenticator app code validated")]),
        ];
        for row_data in &mfa_steps {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(driver, label).await?, value);
        }

        // And mobile-specific security controls are applied:
        //   | Mobile Control        | Security Measure                           |
        //   | Session Timeout       | 15-minute inactivity timeout             |
        //   | Screen Lock           | Auto-lock after 2 minutes idle           |
        //   | Data Encryption       | All data encrypted on device             |
        //   | Remote Wipe           | IT can remotely clear data if device lost |
        let mobile_controls = vec![
            row([("label", "Session Timeout"), ("value", "15-minute inactivity timeout")]),
            row([("label", "Screen Lock"), ("value", "Auto-lock after 2 minutes idle")]),
            row([("label", "Data Encryption"), ("value", "All data encrypted on device")]),
            row([("label", "Remote Wipe"), ("value", "IT can remotely clear data if device lost")]),
        ];
        for row_data in &mobile_controls {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(driver, label).await?, value);
        }
        Ok(())
    })
}

fn scenario_07_emergency_override_authentication_during_system_issues(driver: &WebDriver) -> ScenarioFuture<'_> {
    Box::pin(async move {
        background(driver).await?;

        // Given the primary authentication system is experiencing technical difficulties
        // And there is a patient emergency requiring immediate system access
        // And I am "Dr. Lisa Chen" needing urgent access to patient records
        // The emergency override option lives on the sign-in screen.
        driver.goto(format!("{}{}", base_url(), "/login")).await?;
        wait_located(driver, By::Css("[data-testid=\"login-identity\"]"), 10000).await?;
        // When I request emergency override access
        driver.find(By::Css("[data-testid=\"emergency-override-button\"]")).await?.click().await?;

        // Then the emergency authentication protocol is activated:
        //   | Emergency Protocol    | Override Process                           |
        //   | Identity Verification | Manual verification by IT Security         |
        //   | Supervisor Approval   | Department head authorization required     |
        //   | Time-Limited Access   | 2-hour temporary access granted           |
        //   | Enhanced Monitoring   | All actions logged for later review       |
        let emergency_protocol = vec![
            row([("label", "Identity Verification"), ("value", "Manual verification by IT Security")]),
            row([("label", "Supervisor Approval"), ("value", "Department head authorization required")]),
            row([("label", "Time-Limited Access"), ("value", "2-hour temporary access granted")]),
            row([("label", "Enhanced Monitoring"), ("value", "All actions logged for later review")]),
        ];
        for row_data in &emergency_protocol {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(driver, label).await?, value);
        }

        // And emergency access is granted with restrictions:
        //   | Access Limitation     | Restriction Details                        |
        //   | Time Limit            | Access expires automatically in 2 hours   |
        //   | Function Restrictions | Read-only access to critical patient data |
        //   | Audit Trail          | Enhanced logging of all actions           |
        //   | Supervisor Oversight  | Real-time monitoring of override usage     |
        let access_limitations = vec![
            row([("label", "Time Limit"), ("value", "Access expires automatically in 2 hours")]),
            row([("label", "Function Restrictions"), ("value", "Read-only access to critical patient data")]),
            row([("label", "Audit Trail"), ("value", "Enhanced logging of all actions")]),
            row([("label", "Supervisor Oversight"), ("value", "Real-time monitoring of override usage")]),
        ];
        for row_data in &access_limitations {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(driver, label).await?, value);
        }

        // And emergency access usage is documented:
        //   | Emergency Documentation| Required Information                      |
        //   | Medical Justification | Patient condition requiring urgent access |
        //   | Approving Authority   | Name and title of authorizing supervisor  |
        //   | Access Duration       | Exact start and end times of override use |
        //   | Actions Performed     | Detailed log of all system activities    |
        let emergency_documentation = vec![
            row([("label", "Medical Justification"), ("value", "Patient condition requiring urgent access")]),
            row([("label", "Approving Authority"), ("value", "Name and title of authorizing supervisor")]),
            row([("label", "Access Duration"), ("value", "Exact start and end times of override use")]),
            row([("label", "Actions Performed"), ("value", "Detailed log of all system activities")]),
        ];
        for row_data in &emergency_documentation {
            let label = row_data["label"].as_str();
            let value = row_data["value"].as_str();
            assert_eq!(get_text(driver, label).await?, value);
        }
        Ok(())
    })
}

fn main() {
    run_feature(&[
        ("Successful nurse login with username, password, and badge scan", scenario_01_successful_nurse_login_with_username_password_and_badge_scan),
        ("Failed login attempt with incorrect password", scenario_02_failed_login_attempt_with_incorrect_password),
        ("Account lockout after multiple failed login attempts", scenario_03_account_lockout_after_multiple_failed_login_attempts),
        ("Login with expired password requiring password reset", scenario_04_login_with_expired_password_requiring_password_reset),
        ("Role-based dashboard customization after successful login", scenario_05_role_based_dashboard_customization_after_successful_login),
        ("Mobile device authentication with additional security", scenario_06_mobile_device_authentication_with_additional_security),
        ("Emergency override authentication during system issues", scenario_07_emergency_override_authentication_during_system_issues),
    ]);
}
