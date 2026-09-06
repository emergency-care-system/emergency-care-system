// Selenium WebDriver + Mocha test for
// spec/features/21-user-authentication.feature
//
// This feature's whole subject is the login/authentication flow itself, so
// most scenarios drive the login page directly with the shared data-testid
// contract (login-identity, login-submit, app-root, see support/login.js)
// rather than the login() helper, which assumes success. Other fields
// (password, badge number, dates, etc.) are assumed to expose data-testid
// attributes matching their Gherkin field label (kebab-cased, see
// support/fields.js).

import { strict as assert } from 'assert';
import { By, until, type WebDriver } from 'selenium-webdriver';
import { buildDriver } from './support/build-driver.js';
import { login, verifySystemIsOperational } from './support/login.js';
import { fillField, getText, waitForTestId } from './support/fields.js';
import { BASE_URL } from './support/config.js';

describe('Feature: User Authentication', function () {
  this.timeout(20000);
  let driver: WebDriver;

  before(async () => {
    driver = await buildDriver();
  });

  after(async () => {
    await driver.quit();
  });

  beforeEach(async () => {
    // Background:
    //   Given the emergency care system is operational
    //   And the authentication module is active
    //   And the badge scanning system is functional
    //   And user credentials database is accessible
    //   And audit logging is enabled
    await verifySystemIsOperational(driver);
    // The remaining Background steps describe pre-seeded system state
    // (authentication module, badge scanning system, credentials database,
    // and audit logging readiness) assumed to already be configured in the
    // test environment.
  });

  it('Successful nurse login with username, password, and badge scan', async () => {
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
    await driver.get(`${BASE_URL}/login`);
    const identityField = await driver.wait(
      until.elementLocated(By.css('[data-testid="login-identity"]')),
      10000
    );
    await identityField.sendKeys('sjohnson');
    // And I enter my password in the password field
    await fillField(driver, 'Password', 'CorrectPassword123!');
    // And I scan my badge "BADGE-67890" using the badge reader
    await fillField(driver, 'Badge Number', 'BADGE-67890');
    // And I click the "Login" button
    await driver.findElement(By.css('[data-testid="login-submit"]')).click();

    // Then the system authenticates my credentials successfully:
    //   | Authentication Check  | Verification Result                        |
    //   | Username Validation   | Valid - User exists in system             |
    //   | Password Verification | Correct - Password hash matches           |
    //   | Badge Authentication  | Valid - Badge number matches employee     |
    //   | Account Status        | Active - Account is enabled              |
    //   | Role Authorization    | Authorized - RN role has ED access       |
    const authenticationChecks = [
      { label: 'Username Validation', value: 'Valid - User exists in system' },
      { label: 'Password Verification', value: 'Correct - Password hash matches' },
      { label: 'Badge Authentication', value: 'Valid - Badge number matches employee' },
      { label: 'Account Status', value: 'Active - Account is enabled' },
      { label: 'Role Authorization', value: 'Authorized - RN role has ED access' },
    ];
    for (const { label, value } of authenticationChecks) {
      assert.strictEqual(await getText(driver, label), value);
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
    const accessAttemptLog = [
      { label: 'User ID', value: 'sjohnson (Sarah Johnson)' },
      { label: 'Login Timestamp', value: '2025-06-24 08:00:15' },
      { label: 'Workstation ID', value: 'WS-ED-05' },
      { label: 'IP Address', value: '192.168.1.105' },
      { label: 'Authentication Method', value: 'Username/Password + Badge Scan' },
      { label: 'Login Success', value: 'True' },
      { label: 'Session ID', value: 'SES-ABC123DEF456' },
    ];
    for (const { label, value } of accessAttemptLog) {
      assert.strictEqual(await getText(driver, label), value);
    }

    // And my personalized dashboard is displayed:
    //   | Dashboard Section     | Nurse-Specific Content                     |
    //   | Patient Assignment    | Current patients assigned to Sarah Johnson |
    //   | Task List            | Medication administration, vitals due      |
    //   | Alerts/Notifications | Critical lab values, patient call lights  |
    //   | Quick Access Tools   | Medication lookup, dosage calculator       |
    //   | Shift Information    | Shift start: 07:00, Break schedule        |
    //   | Department Status    | Current census, bed availability          |
    const dashboardSections = [
      { label: 'Patient Assignment', value: 'Current patients assigned to Sarah Johnson' },
      { label: 'Task List', value: 'Medication administration, vitals due' },
      { label: 'Alerts/Notifications', value: 'Critical lab values, patient call lights' },
      { label: 'Quick Access Tools', value: 'Medication lookup, dosage calculator' },
      { label: 'Shift Information', value: 'Shift start: 07:00, Break schedule' },
      { label: 'Department Status', value: 'Current census, bed availability' },
    ];
    for (const { label, value } of dashboardSections) {
      assert.strictEqual(await getText(driver, label), value);
    }
  });

  it('Failed login attempt with incorrect password', async () => {
    // Given I am attempting to log in as "Nurse Mike Chen"
    // And my username is "mchen"
    // And my correct password is stored in the system
    // When I enter my username "mchen"
    await driver.get(`${BASE_URL}/login`);
    const identityField = await driver.wait(
      until.elementLocated(By.css('[data-testid="login-identity"]')),
      10000
    );
    await identityField.sendKeys('mchen');
    // And I enter an incorrect password "wrongpassword"
    await fillField(driver, 'Password', 'wrongpassword');
    // And I scan my valid badge "BADGE-54321"
    await fillField(driver, 'Badge Number', 'BADGE-54321');
    // And I click the "Login" button
    await driver.findElement(By.css('[data-testid="login-submit"]')).click();

    // Then the system rejects the authentication:
    //   | Authentication Check  | Verification Result                        |
    //   | Username Validation   | Valid - User exists                       |
    //   | Password Verification | Failed - Password does not match          |
    //   | Badge Authentication  | Valid - Badge number correct              |
    //   | Overall Result        | Authentication Failed                     |
    const authenticationChecks = [
      { label: 'Username Validation', value: 'Valid - User exists' },
      { label: 'Password Verification', value: 'Failed - Password does not match' },
      { label: 'Badge Authentication', value: 'Valid - Badge number correct' },
      { label: 'Overall Result', value: 'Authentication Failed' },
    ];
    for (const { label, value } of authenticationChecks) {
      assert.strictEqual(await getText(driver, label), value);
    }

    // And a security alert is logged:
    //   | Security Log Entry    | Information Recorded                       |
    //   | User ID               | mchen (Mike Chen)                         |
    //   | Failed Login Time     | 2025-06-24 08:15:32                       |
    //   | Workstation ID        | WS-ED-03                                  |
    //   | IP Address            | 192.168.1.103                            |
    //   | Failure Reason        | Incorrect Password                        |
    //   | Attempt Count         | 1 of 3 allowed attempts                   |
    const securityLogEntries = [
      { label: 'User ID', value: 'mchen (Mike Chen)' },
      { label: 'Failed Login Time', value: '2025-06-24 08:15:32' },
      { label: 'Workstation ID', value: 'WS-ED-03' },
      { label: 'IP Address', value: '192.168.1.103' },
      { label: 'Failure Reason', value: 'Incorrect Password' },
      { label: 'Attempt Count', value: '1 of 3 allowed attempts' },
    ];
    for (const { label, value } of securityLogEntries) {
      assert.strictEqual(await getText(driver, label), value);
    }

    // And an error message is displayed:
    //   | Error Message         | "Invalid credentials. Please check your username and password." |
    //   | Security Notice       | "2 attempts remaining before account lockout"                   |
    assert.strictEqual(
      await getText(driver, 'Error Message'),
      'Invalid credentials. Please check your username and password.'
    );
    assert.strictEqual(await getText(driver, 'Security Notice'), '2 attempts remaining before account lockout');

    // And I remain on the login screen to retry authentication
    const loginForm = await waitForTestId(driver, 'Login Form');
    assert.ok(await loginForm.isDisplayed());
  });

  it('Account lockout after multiple failed login attempts', async () => {
    // Given I am "Dr. Amanda Wilson" attempting to log in
    // And my account allows 3 failed login attempts before lockout
    // And I have already made 2 failed login attempts today
    // When I enter my username "awilson"
    await driver.get(`${BASE_URL}/login`);
    const identityField = await driver.wait(
      until.elementLocated(By.css('[data-testid="login-identity"]')),
      10000
    );
    await identityField.sendKeys('awilson');
    // And I enter another incorrect password
    await fillField(driver, 'Password', 'another-wrong-password');
    // And I scan my badge and click login
    await fillField(driver, 'Badge Number', 'BADGE-11111');
    await driver.findElement(By.css('[data-testid="login-submit"]')).click();

    // Then the system locks my account:
    //   | Lockout Response      | Security Action                            |
    //   | Account Status        | Locked - Exceeded maximum failed attempts |
    //   | Lockout Duration      | 30 minutes automatic unlock               |
    //   | Manual Override       | IT Security can unlock immediately        |
    const lockoutResponse = [
      { label: 'Account Status', value: 'Locked - Exceeded maximum failed attempts' },
      { label: 'Lockout Duration', value: '30 minutes automatic unlock' },
      { label: 'Manual Override', value: 'IT Security can unlock immediately' },
    ];
    for (const { label, value } of lockoutResponse) {
      assert.strictEqual(await getText(driver, label), value);
    }

    // And a security incident is logged:
    //   | Security Incident     | Details                                    |
    //   | Event Type            | Account Lockout - Excessive Failed Attempts|
    //   | User Account          | awilson (Dr. Amanda Wilson)               |
    //   | Lockout Time          | 2025-06-24 14:22:18                       |
    //   | Failed Attempts       | 3 consecutive failures                     |
    //   | Workstation ID        | WS-ED-07                                  |
    const securityIncident = [
      { label: 'Event Type', value: 'Account Lockout - Excessive Failed Attempts' },
      { label: 'User Account', value: 'awilson (Dr. Amanda Wilson)' },
      { label: 'Lockout Time', value: '2025-06-24 14:22:18' },
      { label: 'Failed Attempts', value: '3 consecutive failures' },
      { label: 'Workstation ID', value: 'WS-ED-07' },
    ];
    for (const { label, value } of securityIncident) {
      assert.strictEqual(await getText(driver, label), value);
    }

    // And security notifications are sent:
    //   | Notification Target   | Alert Content                              |
    //   | IT Security Team      | Account lockout alert for Dr. Wilson      |
    //   | Department Supervisor | Staff member unable to access system      |
    //   | User Email            | Account locked - contact IT for assistance|
    const securityNotifications = [
      { label: 'IT Security Team', value: 'Account lockout alert for Dr. Wilson' },
      { label: 'Department Supervisor', value: 'Staff member unable to access system' },
      { label: 'User Email', value: 'Account locked - contact IT for assistance' },
    ];
    for (const { label, value } of securityNotifications) {
      assert.strictEqual(await getText(driver, label), value);
    }

    // And a lockout message is displayed:
    //   | Lockout Message       | "Account temporarily locked due to multiple failed login attempts. Contact IT Security or wait 30 minutes." |
    assert.strictEqual(
      await getText(driver, 'Lockout Message'),
      'Account temporarily locked due to multiple failed login attempts. Contact IT Security or wait 30 minutes.'
    );
  });

  it('Login with expired password requiring password reset', async () => {
    // Given I am "Nurse Patricia Martinez" with valid credentials
    // And my password expired 5 days ago according to policy
    // And the system requires password changes every 90 days
    // When I enter my username "pmartinez"
    await driver.get(`${BASE_URL}/login`);
    const identityField = await driver.wait(
      until.elementLocated(By.css('[data-testid="login-identity"]')),
      10000
    );
    await identityField.sendKeys('pmartinez');
    // And I enter my current (expired) password
    await fillField(driver, 'Password', 'ExpiredPassword123!');
    // And I scan my badge successfully
    await fillField(driver, 'Badge Number', 'BADGE-24680');
    await driver.findElement(By.css('[data-testid="login-submit"]')).click();

    // Then the system identifies the expired password:
    //   | Password Check        | Status                                     |
    //   | Password Validity     | Expired - 5 days past expiration         |
    //   | Grace Period          | Exceeded - No grace logins remaining      |
    //   | Password Age          | 95 days old (5 days over 90-day limit)   |
    const passwordChecks = [
      { label: 'Password Validity', value: 'Expired - 5 days past expiration' },
      { label: 'Grace Period', value: 'Exceeded - No grace logins remaining' },
      { label: 'Password Age', value: '95 days old (5 days over 90-day limit)' },
    ];
    for (const { label, value } of passwordChecks) {
      assert.strictEqual(await getText(driver, label), value);
    }

    // And the password reset workflow is initiated:
    //   | Reset Process         | Required Actions                           |
    //   | Identity Verification | Additional security questions prompted     |
    //   | New Password Entry    | Password must meet complexity requirements |
    //   | Password Confirmation | Confirm new password entry                |
    //   | Security Questions    | Update security questions if needed       |
    const resetProcess = [
      { label: 'Identity Verification', value: 'Additional security questions prompted' },
      { label: 'New Password Entry', value: 'Password must meet complexity requirements' },
      { label: 'Password Confirmation', value: 'Confirm new password entry' },
      { label: 'Security Questions', value: 'Update security questions if needed' },
    ];
    for (const { label, value } of resetProcess) {
      assert.strictEqual(await getText(driver, label), value);
    }

    // And password policy requirements are displayed:
    //   | Policy Requirement    | Specification                              |
    //   | Minimum Length        | 12 characters                             |
    //   | Character Types       | Upper, lower, number, special character   |
    //   | Password History      | Cannot reuse last 12 passwords           |
    //   | Common Words          | Cannot use dictionary words or personal info|
    const policyRequirements = [
      { label: 'Minimum Length', value: '12 characters' },
      { label: 'Character Types', value: 'Upper, lower, number, special character' },
      { label: 'Password History', value: 'Cannot reuse last 12 passwords' },
      { label: 'Common Words', value: 'Cannot use dictionary words or personal info' },
    ];
    for (const { label, value } of policyRequirements) {
      assert.strictEqual(await getText(driver, label), value);
    }

    // And upon successful password reset, normal login proceeds
    assert.match(await getText(driver, 'Login Status'), /success/i);
  });

  it('Role-based dashboard customization after successful login', async () => {
    // Given multiple users with different roles log in successfully
    // When "Dr. Emily Rodriguez" (Emergency Physician) logs in
    await login(driver, 'Dr. Emily Rodriguez');

    // Then her physician dashboard is displayed with:
    //   | Physician Dashboard   | Specialized Content                        |
    //   | Patient Queue         | Patients waiting to be seen by priority    |
    //   | Active Orders         | Lab results, imaging pending review       |
    //   | Critical Alerts       | Abnormal vitals, critical lab values      |
    //   | Decision Support      | Clinical guidelines, drug interactions    |
    //   | Documentation Tools   | Templates for common conditions           |
    const physicianDashboard = [
      { label: 'Patient Queue', value: 'Patients waiting to be seen by priority' },
      { label: 'Active Orders', value: 'Lab results, imaging pending review' },
      { label: 'Critical Alerts', value: 'Abnormal vitals, critical lab values' },
      { label: 'Decision Support', value: 'Clinical guidelines, drug interactions' },
      { label: 'Documentation Tools', value: 'Templates for common conditions' },
    ];
    for (const { label, value } of physicianDashboard) {
      assert.strictEqual(await getText(driver, label), value);
    }

    // When "Charge Nurse Williams" logs in
    await login(driver, 'Charge Nurse Williams');

    // Then her charge nurse dashboard shows:
    //   | Charge Nurse Dashboard| Management-Focused Content                 |
    //   | Department Overview   | Bed status, staff assignments, census     |
    //   | Resource Management   | Equipment status, supply levels           |
    //   | Staff Coordination    | Break schedules, assignments, coverage    |
    //   | Quality Metrics       | Wait times, patient satisfaction, safety  |
    //   | Administrative Tasks  | Reporting, scheduling, policy updates     |
    const chargeNurseDashboard = [
      { label: 'Department Overview', value: 'Bed status, staff assignments, census' },
      { label: 'Resource Management', value: 'Equipment status, supply levels' },
      { label: 'Staff Coordination', value: 'Break schedules, assignments, coverage' },
      { label: 'Quality Metrics', value: 'Wait times, patient satisfaction, safety' },
      { label: 'Administrative Tasks', value: 'Reporting, scheduling, policy updates' },
    ];
    for (const { label, value } of chargeNurseDashboard) {
      assert.strictEqual(await getText(driver, label), value);
    }

    // When "Tech Support Anderson" logs in
    await login(driver, 'Tech Support Anderson');

    // Then his technical dashboard displays:
    //   | Technical Dashboard   | System Administration Content              |
    //   | System Status         | Server health, network connectivity       |
    //   | User Management       | Account status, permission changes        |
    //   | Audit Logs           | System access, security events            |
    //   | Maintenance Tools     | Backup status, system updates             |
    const technicalDashboard = [
      { label: 'System Status', value: 'Server health, network connectivity' },
      { label: 'User Management', value: 'Account status, permission changes' },
      { label: 'Audit Logs', value: 'System access, security events' },
      { label: 'Maintenance Tools', value: 'Backup status, system updates' },
    ];
    for (const { label, value } of technicalDashboard) {
      assert.strictEqual(await getText(driver, label), value);
    }
  });

  it('Mobile device authentication with additional security', async () => {
    // Given I am using the mobile ED app on my smartphone
    // And mobile access requires enhanced security measures
    // When I attempt to log in on my mobile device
    await driver.get(`${BASE_URL}/login?viewport=mobile`);
    await driver.wait(until.elementLocated(By.css('[data-testid="login-identity"]')), 10000);

    // Then additional authentication factors are required:
    //   | Mobile Security Factor| Requirement                                |
    //   | Device Registration   | Device must be registered with IT          |
    //   | Biometric Auth        | Fingerprint or face recognition required   |
    //   | Location Verification | GPS confirms user is within hospital grounds|
    //   | Time-based Token      | 6-digit code from authenticator app       |
    const mobileSecurityFactors = [
      { label: 'Device Registration', value: 'Device must be registered with IT' },
      { label: 'Biometric Auth', value: 'Fingerprint or face recognition required' },
      { label: 'Location Verification', value: 'GPS confirms user is within hospital grounds' },
      { label: 'Time-based Token', value: '6-digit code from authenticator app' },
    ];
    for (const { label, value } of mobileSecurityFactors) {
      assert.strictEqual(await getText(driver, label), value);
    }

    // And I complete multi-factor authentication:
    //   | MFA Step              | Verification Process                       |
    //   | Username/Password     | Standard credential verification           |
    //   | Biometric Scan        | Fingerprint verified against enrolled pattern|
    //   | Location Check        | GPS coordinates within allowed radius      |
    //   | Time Token            | Authenticator app code validated          |
    const mfaSteps = [
      { label: 'Username/Password', value: 'Standard credential verification' },
      { label: 'Biometric Scan', value: 'Fingerprint verified against enrolled pattern' },
      { label: 'Location Check', value: 'GPS coordinates within allowed radius' },
      { label: 'Time Token', value: 'Authenticator app code validated' },
    ];
    for (const { label, value } of mfaSteps) {
      assert.strictEqual(await getText(driver, label), value);
    }

    // And mobile-specific security controls are applied:
    //   | Mobile Control        | Security Measure                           |
    //   | Session Timeout       | 15-minute inactivity timeout             |
    //   | Screen Lock           | Auto-lock after 2 minutes idle           |
    //   | Data Encryption       | All data encrypted on device             |
    //   | Remote Wipe           | IT can remotely clear data if device lost |
    const mobileControls = [
      { label: 'Session Timeout', value: '15-minute inactivity timeout' },
      { label: 'Screen Lock', value: 'Auto-lock after 2 minutes idle' },
      { label: 'Data Encryption', value: 'All data encrypted on device' },
      { label: 'Remote Wipe', value: 'IT can remotely clear data if device lost' },
    ];
    for (const { label, value } of mobileControls) {
      assert.strictEqual(await getText(driver, label), value);
    }
  });

  it('Emergency override authentication during system issues', async () => {
    // Given the primary authentication system is experiencing technical difficulties
    // And there is a patient emergency requiring immediate system access
    // And I am "Dr. Lisa Chen" needing urgent access to patient records
    // The emergency override option lives on the sign-in screen.
    await driver.get(`${BASE_URL}/login`);
    await driver.wait(until.elementLocated(By.css('[data-testid="login-identity"]')), 10000);
    // When I request emergency override access
    await driver.findElement(By.css('[data-testid="emergency-override-button"]')).click();

    // Then the emergency authentication protocol is activated:
    //   | Emergency Protocol    | Override Process                           |
    //   | Identity Verification | Manual verification by IT Security         |
    //   | Supervisor Approval   | Department head authorization required     |
    //   | Time-Limited Access   | 2-hour temporary access granted           |
    //   | Enhanced Monitoring   | All actions logged for later review       |
    const emergencyProtocol = [
      { label: 'Identity Verification', value: 'Manual verification by IT Security' },
      { label: 'Supervisor Approval', value: 'Department head authorization required' },
      { label: 'Time-Limited Access', value: '2-hour temporary access granted' },
      { label: 'Enhanced Monitoring', value: 'All actions logged for later review' },
    ];
    for (const { label, value } of emergencyProtocol) {
      assert.strictEqual(await getText(driver, label), value);
    }

    // And emergency access is granted with restrictions:
    //   | Access Limitation     | Restriction Details                        |
    //   | Time Limit            | Access expires automatically in 2 hours   |
    //   | Function Restrictions | Read-only access to critical patient data |
    //   | Audit Trail          | Enhanced logging of all actions           |
    //   | Supervisor Oversight  | Real-time monitoring of override usage     |
    const accessLimitations = [
      { label: 'Time Limit', value: 'Access expires automatically in 2 hours' },
      { label: 'Function Restrictions', value: 'Read-only access to critical patient data' },
      { label: 'Audit Trail', value: 'Enhanced logging of all actions' },
      { label: 'Supervisor Oversight', value: 'Real-time monitoring of override usage' },
    ];
    for (const { label, value } of accessLimitations) {
      assert.strictEqual(await getText(driver, label), value);
    }

    // And emergency access usage is documented:
    //   | Emergency Documentation| Required Information                      |
    //   | Medical Justification | Patient condition requiring urgent access |
    //   | Approving Authority   | Name and title of authorizing supervisor  |
    //   | Access Duration       | Exact start and end times of override use |
    //   | Actions Performed     | Detailed log of all system activities    |
    const emergencyDocumentation = [
      { label: 'Medical Justification', value: 'Patient condition requiring urgent access' },
      { label: 'Approving Authority', value: 'Name and title of authorizing supervisor' },
      { label: 'Access Duration', value: 'Exact start and end times of override use' },
      { label: 'Actions Performed', value: 'Detailed log of all system activities' },
    ];
    for (const { label, value } of emergencyDocumentation) {
      assert.strictEqual(await getText(driver, label), value);
    }
  });
});
