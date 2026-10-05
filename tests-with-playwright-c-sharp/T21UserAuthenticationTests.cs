// Playwright + NUnit test for
// tests-with-given-when-then-features/21-user-authentication.feature
// (equivalent to tests-with-playwright-javascript/21-user-authentication.test.js).
//
// This feature's whole subject is the login/authentication flow itself, so
// most scenarios drive the login page directly with the shared data-testid
// contract (login-identity, login-submit, app-root, see Support/Login.cs)
// rather than the login() helper, which assumes success. Other fields
// (password, badge number, dates, etc.) are assumed to expose data-testid
// attributes matching their Gherkin field label (kebab-cased, see
// Support/Fields.cs).

namespace EmergencyCareSystem.PlaywrightTests;

[TestFixture]
[NonParallelizable]
public class T21UserAuthenticationTests
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
        //   And the authentication module is active
        //   And the badge scanning system is functional
        //   And user credentials database is accessible
        //   And audit logging is enabled
        await VerifySystemIsOperational(page);
        // The remaining Background steps describe pre-seeded system state
        // (authentication module, badge scanning system, credentials database,
        // and audit logging readiness) assumed to already be configured in the
        // test environment.
    }

    [Test, Order(1)]
    [Description("Successful nurse login with username, password, and badge scan")]
    public async Task SuccessfulNurseLoginWithUsernamePasswordAndBadgeScan()
    {
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
        await page.GotoAsync(BaseUrl + "/login");
        var identityField = await WaitLocated(page, "login-identity", 10000);
        await identityField.FillAsync("sjohnson");
        // And I enter my password in the password field
        await FillField(page, "Password", "CorrectPassword123!");
        // And I scan my badge "BADGE-67890" using the badge reader
        await FillField(page, "Badge Number", "BADGE-67890");
        // And I click the "Login" button
        await page.GetByTestId("login-submit").First.ClickAsync();

        // Then the system authenticates my credentials successfully:
        //   | Authentication Check  | Verification Result                        |
        //   | Username Validation   | Valid - User exists in system             |
        //   | Password Verification | Correct - Password hash matches           |
        //   | Badge Authentication  | Valid - Badge number matches employee     |
        //   | Account Status        | Active - Account is enabled              |
        //   | Role Authorization    | Authorized - RN role has ED access       |
        var authenticationChecks = new List<Dictionary<string, string>>
        {
            Row(("label", "Username Validation"), ("value", "Valid - User exists in system")),
            Row(("label", "Password Verification"), ("value", "Correct - Password hash matches")),
            Row(("label", "Badge Authentication"), ("value", "Valid - Badge number matches employee")),
            Row(("label", "Account Status"), ("value", "Active - Account is enabled")),
            Row(("label", "Role Authorization"), ("value", "Authorized - RN role has ED access")),
        };
        foreach (var rowData in authenticationChecks)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That((await GetText(page, label)), Is.EqualTo(value));
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
        var accessAttemptLog = new List<Dictionary<string, string>>
        {
            Row(("label", "User ID"), ("value", "sjohnson (Sarah Johnson)")),
            Row(("label", "Login Timestamp"), ("value", "2025-06-24 08:00:15")),
            Row(("label", "Workstation ID"), ("value", "WS-ED-05")),
            Row(("label", "IP Address"), ("value", "192.168.1.105")),
            Row(("label", "Authentication Method"), ("value", "Username/Password + Badge Scan")),
            Row(("label", "Login Success"), ("value", "True")),
            Row(("label", "Session ID"), ("value", "SES-ABC123DEF456")),
        };
        foreach (var rowData in accessAttemptLog)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That((await GetText(page, label)), Is.EqualTo(value));
        }

        // And my personalized dashboard is displayed:
        //   | Dashboard Section     | Nurse-Specific Content                     |
        //   | Patient Assignment    | Current patients assigned to Sarah Johnson |
        //   | Task List            | Medication administration, vitals due      |
        //   | Alerts/Notifications | Critical lab values, patient call lights  |
        //   | Quick Access Tools   | Medication lookup, dosage calculator       |
        //   | Shift Information    | Shift start: 07:00, Break schedule        |
        //   | Department Status    | Current census, bed availability          |
        var dashboardSections = new List<Dictionary<string, string>>
        {
            Row(("label", "Patient Assignment"), ("value", "Current patients assigned to Sarah Johnson")),
            Row(("label", "Task List"), ("value", "Medication administration, vitals due")),
            Row(("label", "Alerts/Notifications"), ("value", "Critical lab values, patient call lights")),
            Row(("label", "Quick Access Tools"), ("value", "Medication lookup, dosage calculator")),
            Row(("label", "Shift Information"), ("value", "Shift start: 07:00, Break schedule")),
            Row(("label", "Department Status"), ("value", "Current census, bed availability")),
        };
        foreach (var rowData in dashboardSections)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That((await GetText(page, label)), Is.EqualTo(value));
        }
    }

    [Test, Order(2)]
    [Description("Failed login attempt with incorrect password")]
    public async Task FailedLoginAttemptWithIncorrectPassword()
    {
        // Given I am attempting to log in as "Nurse Mike Chen"
        // And my username is "mchen"
        // And my correct password is stored in the system
        // When I enter my username "mchen"
        await page.GotoAsync(BaseUrl + "/login");
        var identityField = await WaitLocated(page, "login-identity", 10000);
        await identityField.FillAsync("mchen");
        // And I enter an incorrect password "wrongpassword"
        await FillField(page, "Password", "wrongpassword");
        // And I scan my valid badge "BADGE-54321"
        await FillField(page, "Badge Number", "BADGE-54321");
        // And I click the "Login" button
        await page.GetByTestId("login-submit").First.ClickAsync();

        // Then the system rejects the authentication:
        //   | Authentication Check  | Verification Result                        |
        //   | Username Validation   | Valid - User exists                       |
        //   | Password Verification | Failed - Password does not match          |
        //   | Badge Authentication  | Valid - Badge number correct              |
        //   | Overall Result        | Authentication Failed                     |
        var authenticationChecks = new List<Dictionary<string, string>>
        {
            Row(("label", "Username Validation"), ("value", "Valid - User exists")),
            Row(("label", "Password Verification"), ("value", "Failed - Password does not match")),
            Row(("label", "Badge Authentication"), ("value", "Valid - Badge number correct")),
            Row(("label", "Overall Result"), ("value", "Authentication Failed")),
        };
        foreach (var rowData in authenticationChecks)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That((await GetText(page, label)), Is.EqualTo(value));
        }

        // And a security alert is logged:
        //   | Security Log Entry    | Information Recorded                       |
        //   | User ID               | mchen (Mike Chen)                         |
        //   | Failed Login Time     | 2025-06-24 08:15:32                       |
        //   | Workstation ID        | WS-ED-03                                  |
        //   | IP Address            | 192.168.1.103                            |
        //   | Failure Reason        | Incorrect Password                        |
        //   | Attempt Count         | 1 of 3 allowed attempts                   |
        var securityLogEntries = new List<Dictionary<string, string>>
        {
            Row(("label", "User ID"), ("value", "mchen (Mike Chen)")),
            Row(("label", "Failed Login Time"), ("value", "2025-06-24 08:15:32")),
            Row(("label", "Workstation ID"), ("value", "WS-ED-03")),
            Row(("label", "IP Address"), ("value", "192.168.1.103")),
            Row(("label", "Failure Reason"), ("value", "Incorrect Password")),
            Row(("label", "Attempt Count"), ("value", "1 of 3 allowed attempts")),
        };
        foreach (var rowData in securityLogEntries)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That((await GetText(page, label)), Is.EqualTo(value));
        }

        // And an error message is displayed:
        //   | Error Message         | "Invalid credentials. Please check your username and password." |
        //   | Security Notice       | "2 attempts remaining before account lockout"                   |
        Assert.That((await GetText(page, "Error Message")), Is.EqualTo("Invalid credentials. Please check your username and password."));
        Assert.That((await GetText(page, "Security Notice")), Is.EqualTo("2 attempts remaining before account lockout"));

        // And I remain on the login screen to retry authentication
        var loginForm = await WaitForTestId(page, "Login Form");
        Assert.That((await loginForm.IsVisibleAsync()), Is.True);
    }

    [Test, Order(3)]
    [Description("Account lockout after multiple failed login attempts")]
    public async Task AccountLockoutAfterMultipleFailedLoginAttempts()
    {
        // Given I am "Dr. Amanda Wilson" attempting to log in
        // And my account allows 3 failed login attempts before lockout
        // And I have already made 2 failed login attempts today
        // When I enter my username "awilson"
        await page.GotoAsync(BaseUrl + "/login");
        var identityField = await WaitLocated(page, "login-identity", 10000);
        await identityField.FillAsync("awilson");
        // And I enter another incorrect password
        await FillField(page, "Password", "another-wrong-password");
        // And I scan my badge and click login
        await FillField(page, "Badge Number", "BADGE-11111");
        await page.GetByTestId("login-submit").First.ClickAsync();

        // Then the system locks my account:
        //   | Lockout Response      | Security Action                            |
        //   | Account Status        | Locked - Exceeded maximum failed attempts |
        //   | Lockout Duration      | 30 minutes automatic unlock               |
        //   | Manual Override       | IT Security can unlock immediately        |
        var lockoutResponse = new List<Dictionary<string, string>>
        {
            Row(("label", "Account Status"), ("value", "Locked - Exceeded maximum failed attempts")),
            Row(("label", "Lockout Duration"), ("value", "30 minutes automatic unlock")),
            Row(("label", "Manual Override"), ("value", "IT Security can unlock immediately")),
        };
        foreach (var rowData in lockoutResponse)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That((await GetText(page, label)), Is.EqualTo(value));
        }

        // And a security incident is logged:
        //   | Security Incident     | Details                                    |
        //   | Event Type            | Account Lockout - Excessive Failed Attempts|
        //   | User Account          | awilson (Dr. Amanda Wilson)               |
        //   | Lockout Time          | 2025-06-24 14:22:18                       |
        //   | Failed Attempts       | 3 consecutive failures                     |
        //   | Workstation ID        | WS-ED-07                                  |
        var securityIncident = new List<Dictionary<string, string>>
        {
            Row(("label", "Event Type"), ("value", "Account Lockout - Excessive Failed Attempts")),
            Row(("label", "User Account"), ("value", "awilson (Dr. Amanda Wilson)")),
            Row(("label", "Lockout Time"), ("value", "2025-06-24 14:22:18")),
            Row(("label", "Failed Attempts"), ("value", "3 consecutive failures")),
            Row(("label", "Workstation ID"), ("value", "WS-ED-07")),
        };
        foreach (var rowData in securityIncident)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That((await GetText(page, label)), Is.EqualTo(value));
        }

        // And security notifications are sent:
        //   | Notification Target   | Alert Content                              |
        //   | IT Security Team      | Account lockout alert for Dr. Wilson      |
        //   | Department Supervisor | Staff member unable to access system      |
        //   | User Email            | Account locked - contact IT for assistance|
        var securityNotifications = new List<Dictionary<string, string>>
        {
            Row(("label", "IT Security Team"), ("value", "Account lockout alert for Dr. Wilson")),
            Row(("label", "Department Supervisor"), ("value", "Staff member unable to access system")),
            Row(("label", "User Email"), ("value", "Account locked - contact IT for assistance")),
        };
        foreach (var rowData in securityNotifications)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That((await GetText(page, label)), Is.EqualTo(value));
        }

        // And a lockout message is displayed:
        //   | Lockout Message       | "Account temporarily locked due to multiple failed login attempts. Contact IT Security or wait 30 minutes." |
        Assert.That((await GetText(page, "Lockout Message")), Is.EqualTo("Account temporarily locked due to multiple failed login attempts. Contact IT Security or wait 30 minutes."));
    }

    [Test, Order(4)]
    [Description("Login with expired password requiring password reset")]
    public async Task LoginWithExpiredPasswordRequiringPasswordReset()
    {
        // Given I am "Nurse Patricia Martinez" with valid credentials
        // And my password expired 5 days ago according to policy
        // And the system requires password changes every 90 days
        // When I enter my username "pmartinez"
        await page.GotoAsync(BaseUrl + "/login");
        var identityField = await WaitLocated(page, "login-identity", 10000);
        await identityField.FillAsync("pmartinez");
        // And I enter my current (expired) password
        await FillField(page, "Password", "ExpiredPassword123!");
        // And I scan my badge successfully
        await FillField(page, "Badge Number", "BADGE-24680");
        await page.GetByTestId("login-submit").First.ClickAsync();

        // Then the system identifies the expired password:
        //   | Password Check        | Status                                     |
        //   | Password Validity     | Expired - 5 days past expiration         |
        //   | Grace Period          | Exceeded - No grace logins remaining      |
        //   | Password Age          | 95 days old (5 days over 90-day limit)   |
        var passwordChecks = new List<Dictionary<string, string>>
        {
            Row(("label", "Password Validity"), ("value", "Expired - 5 days past expiration")),
            Row(("label", "Grace Period"), ("value", "Exceeded - No grace logins remaining")),
            Row(("label", "Password Age"), ("value", "95 days old (5 days over 90-day limit)")),
        };
        foreach (var rowData in passwordChecks)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That((await GetText(page, label)), Is.EqualTo(value));
        }

        // And the password reset workflow is initiated:
        //   | Reset Process         | Required Actions                           |
        //   | Identity Verification | Additional security questions prompted     |
        //   | New Password Entry    | Password must meet complexity requirements |
        //   | Password Confirmation | Confirm new password entry                |
        //   | Security Questions    | Update security questions if needed       |
        var resetProcess = new List<Dictionary<string, string>>
        {
            Row(("label", "Identity Verification"), ("value", "Additional security questions prompted")),
            Row(("label", "New Password Entry"), ("value", "Password must meet complexity requirements")),
            Row(("label", "Password Confirmation"), ("value", "Confirm new password entry")),
            Row(("label", "Security Questions"), ("value", "Update security questions if needed")),
        };
        foreach (var rowData in resetProcess)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That((await GetText(page, label)), Is.EqualTo(value));
        }

        // And password policy requirements are displayed:
        //   | Policy Requirement    | Specification                              |
        //   | Minimum Length        | 12 characters                             |
        //   | Character Types       | Upper, lower, number, special character   |
        //   | Password History      | Cannot reuse last 12 passwords           |
        //   | Common Words          | Cannot use dictionary words or personal info|
        var policyRequirements = new List<Dictionary<string, string>>
        {
            Row(("label", "Minimum Length"), ("value", "12 characters")),
            Row(("label", "Character Types"), ("value", "Upper, lower, number, special character")),
            Row(("label", "Password History"), ("value", "Cannot reuse last 12 passwords")),
            Row(("label", "Common Words"), ("value", "Cannot use dictionary words or personal info")),
        };
        foreach (var rowData in policyRequirements)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That((await GetText(page, label)), Is.EqualTo(value));
        }

        // And upon successful password reset, normal login proceeds
        Assert.That((await GetText(page, "Login Status")), Does.Match(@"success").IgnoreCase);
    }

    [Test, Order(5)]
    [Description("Role-based dashboard customization after successful login")]
    public async Task RoleBasedDashboardCustomizationAfterSuccessfulLogin()
    {
        // Given multiple users with different roles log in successfully
        // When "Dr. Emily Rodriguez" (Emergency Physician) logs in
        await Login(page, "Dr. Emily Rodriguez");

        // Then her physician dashboard is displayed with:
        //   | Physician Dashboard   | Specialized Content                        |
        //   | Patient Queue         | Patients waiting to be seen by priority    |
        //   | Active Orders         | Lab results, imaging pending review       |
        //   | Critical Alerts       | Abnormal vitals, critical lab values      |
        //   | Decision Support      | Clinical guidelines, drug interactions    |
        //   | Documentation Tools   | Templates for common conditions           |
        var physicianDashboard = new List<Dictionary<string, string>>
        {
            Row(("label", "Patient Queue"), ("value", "Patients waiting to be seen by priority")),
            Row(("label", "Active Orders"), ("value", "Lab results, imaging pending review")),
            Row(("label", "Critical Alerts"), ("value", "Abnormal vitals, critical lab values")),
            Row(("label", "Decision Support"), ("value", "Clinical guidelines, drug interactions")),
            Row(("label", "Documentation Tools"), ("value", "Templates for common conditions")),
        };
        foreach (var rowData in physicianDashboard)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That((await GetText(page, label)), Is.EqualTo(value));
        }

        // When "Charge Nurse Williams" logs in
        await Login(page, "Charge Nurse Williams");

        // Then her charge nurse dashboard shows:
        //   | Charge Nurse Dashboard| Management-Focused Content                 |
        //   | Department Overview   | Bed status, staff assignments, census     |
        //   | Resource Management   | Equipment status, supply levels           |
        //   | Staff Coordination    | Break schedules, assignments, coverage    |
        //   | Quality Metrics       | Wait times, patient satisfaction, safety  |
        //   | Administrative Tasks  | Reporting, scheduling, policy updates     |
        var chargeNurseDashboard = new List<Dictionary<string, string>>
        {
            Row(("label", "Department Overview"), ("value", "Bed status, staff assignments, census")),
            Row(("label", "Resource Management"), ("value", "Equipment status, supply levels")),
            Row(("label", "Staff Coordination"), ("value", "Break schedules, assignments, coverage")),
            Row(("label", "Quality Metrics"), ("value", "Wait times, patient satisfaction, safety")),
            Row(("label", "Administrative Tasks"), ("value", "Reporting, scheduling, policy updates")),
        };
        foreach (var rowData in chargeNurseDashboard)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That((await GetText(page, label)), Is.EqualTo(value));
        }

        // When "Tech Support Anderson" logs in
        await Login(page, "Tech Support Anderson");

        // Then his technical dashboard displays:
        //   | Technical Dashboard   | System Administration Content              |
        //   | System Status         | Server health, network connectivity       |
        //   | User Management       | Account status, permission changes        |
        //   | Audit Logs           | System access, security events            |
        //   | Maintenance Tools     | Backup status, system updates             |
        var technicalDashboard = new List<Dictionary<string, string>>
        {
            Row(("label", "System Status"), ("value", "Server health, network connectivity")),
            Row(("label", "User Management"), ("value", "Account status, permission changes")),
            Row(("label", "Audit Logs"), ("value", "System access, security events")),
            Row(("label", "Maintenance Tools"), ("value", "Backup status, system updates")),
        };
        foreach (var rowData in technicalDashboard)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That((await GetText(page, label)), Is.EqualTo(value));
        }
    }

    [Test, Order(6)]
    [Description("Mobile device authentication with additional security")]
    public async Task MobileDeviceAuthenticationWithAdditionalSecurity()
    {
        // Given I am using the mobile ED app on my smartphone
        // And mobile access requires enhanced security measures
        // When I attempt to log in on my mobile device
        await page.GotoAsync(BaseUrl + "/login?viewport=mobile");
        await WaitLocated(page, "login-identity", 10000);

        // Then additional authentication factors are required:
        //   | Mobile Security Factor| Requirement                                |
        //   | Device Registration   | Device must be registered with IT          |
        //   | Biometric Auth        | Fingerprint or face recognition required   |
        //   | Location Verification | GPS confirms user is within hospital grounds|
        //   | Time-based Token      | 6-digit code from authenticator app       |
        var mobileSecurityFactors = new List<Dictionary<string, string>>
        {
            Row(("label", "Device Registration"), ("value", "Device must be registered with IT")),
            Row(("label", "Biometric Auth"), ("value", "Fingerprint or face recognition required")),
            Row(("label", "Location Verification"), ("value", "GPS confirms user is within hospital grounds")),
            Row(("label", "Time-based Token"), ("value", "6-digit code from authenticator app")),
        };
        foreach (var rowData in mobileSecurityFactors)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That((await GetText(page, label)), Is.EqualTo(value));
        }

        // And I complete multi-factor authentication:
        //   | MFA Step              | Verification Process                       |
        //   | Username/Password     | Standard credential verification           |
        //   | Biometric Scan        | Fingerprint verified against enrolled pattern|
        //   | Location Check        | GPS coordinates within allowed radius      |
        //   | Time Token            | Authenticator app code validated          |
        var mfaSteps = new List<Dictionary<string, string>>
        {
            Row(("label", "Username/Password"), ("value", "Standard credential verification")),
            Row(("label", "Biometric Scan"), ("value", "Fingerprint verified against enrolled pattern")),
            Row(("label", "Location Check"), ("value", "GPS coordinates within allowed radius")),
            Row(("label", "Time Token"), ("value", "Authenticator app code validated")),
        };
        foreach (var rowData in mfaSteps)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That((await GetText(page, label)), Is.EqualTo(value));
        }

        // And mobile-specific security controls are applied:
        //   | Mobile Control        | Security Measure                           |
        //   | Session Timeout       | 15-minute inactivity timeout             |
        //   | Screen Lock           | Auto-lock after 2 minutes idle           |
        //   | Data Encryption       | All data encrypted on device             |
        //   | Remote Wipe           | IT can remotely clear data if device lost |
        var mobileControls = new List<Dictionary<string, string>>
        {
            Row(("label", "Session Timeout"), ("value", "15-minute inactivity timeout")),
            Row(("label", "Screen Lock"), ("value", "Auto-lock after 2 minutes idle")),
            Row(("label", "Data Encryption"), ("value", "All data encrypted on device")),
            Row(("label", "Remote Wipe"), ("value", "IT can remotely clear data if device lost")),
        };
        foreach (var rowData in mobileControls)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That((await GetText(page, label)), Is.EqualTo(value));
        }
    }

    [Test, Order(7)]
    [Description("Emergency override authentication during system issues")]
    public async Task EmergencyOverrideAuthenticationDuringSystemIssues()
    {
        // Given the primary authentication system is experiencing technical difficulties
        // And there is a patient emergency requiring immediate system access
        // And I am "Dr. Lisa Chen" needing urgent access to patient records
        // The emergency override option lives on the sign-in screen.
        await page.GotoAsync(BaseUrl + "/login");
        await WaitLocated(page, "login-identity", 10000);
        // When I request emergency override access
        await page.GetByTestId("emergency-override-button").First.ClickAsync();

        // Then the emergency authentication protocol is activated:
        //   | Emergency Protocol    | Override Process                           |
        //   | Identity Verification | Manual verification by IT Security         |
        //   | Supervisor Approval   | Department head authorization required     |
        //   | Time-Limited Access   | 2-hour temporary access granted           |
        //   | Enhanced Monitoring   | All actions logged for later review       |
        var emergencyProtocol = new List<Dictionary<string, string>>
        {
            Row(("label", "Identity Verification"), ("value", "Manual verification by IT Security")),
            Row(("label", "Supervisor Approval"), ("value", "Department head authorization required")),
            Row(("label", "Time-Limited Access"), ("value", "2-hour temporary access granted")),
            Row(("label", "Enhanced Monitoring"), ("value", "All actions logged for later review")),
        };
        foreach (var rowData in emergencyProtocol)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That((await GetText(page, label)), Is.EqualTo(value));
        }

        // And emergency access is granted with restrictions:
        //   | Access Limitation     | Restriction Details                        |
        //   | Time Limit            | Access expires automatically in 2 hours   |
        //   | Function Restrictions | Read-only access to critical patient data |
        //   | Audit Trail          | Enhanced logging of all actions           |
        //   | Supervisor Oversight  | Real-time monitoring of override usage     |
        var accessLimitations = new List<Dictionary<string, string>>
        {
            Row(("label", "Time Limit"), ("value", "Access expires automatically in 2 hours")),
            Row(("label", "Function Restrictions"), ("value", "Read-only access to critical patient data")),
            Row(("label", "Audit Trail"), ("value", "Enhanced logging of all actions")),
            Row(("label", "Supervisor Oversight"), ("value", "Real-time monitoring of override usage")),
        };
        foreach (var rowData in accessLimitations)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That((await GetText(page, label)), Is.EqualTo(value));
        }

        // And emergency access usage is documented:
        //   | Emergency Documentation| Required Information                      |
        //   | Medical Justification | Patient condition requiring urgent access |
        //   | Approving Authority   | Name and title of authorizing supervisor  |
        //   | Access Duration       | Exact start and end times of override use |
        //   | Actions Performed     | Detailed log of all system activities    |
        var emergencyDocumentation = new List<Dictionary<string, string>>
        {
            Row(("label", "Medical Justification"), ("value", "Patient condition requiring urgent access")),
            Row(("label", "Approving Authority"), ("value", "Name and title of authorizing supervisor")),
            Row(("label", "Access Duration"), ("value", "Exact start and end times of override use")),
            Row(("label", "Actions Performed"), ("value", "Detailed log of all system activities")),
        };
        foreach (var rowData in emergencyDocumentation)
        {
            var label = rowData["label"];
            var value = rowData["value"];
            Assert.That((await GetText(page, label)), Is.EqualTo(value));
        }
    }
}
