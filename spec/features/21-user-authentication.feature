# # User Authentication
#
# **Key Components:**
#
# - Multi-factor authentication combining username/password with badge scanning
#
# - Comprehensive audit logging tracking all access attempts and security events
#
# - Role-based personalized dashboards tailored to user responsibilities
#
# - Security controls including account lockout, password policies, and emergency procedures
#
# **Comprehensive Scenarios:**
#
# - Successful authentication - Standard login with username, password, and badge scan
#
# - Failed login attempt - Incorrect password handling with security logging
#
# - Account lockout - Multiple failed attempts triggering security measures
#
# - Password expiration - Forced password reset with policy enforcement
#
# - Role-based dashboards - Customized interfaces for different user types
#
# - Single Sign-On integration - Seamless authentication with hospital directory
#
# - Mobile device security - Enhanced multi-factor authentication for mobile access
#
# - Emergency override - Emergency access protocols during system issues
#
# **Healthcare-Specific Features:**
#
# - HIPAA-compliant audit logging with detailed access tracking
#
# - Role-based access control (RBAC) for healthcare positions
#
# - Badge/ID card integration for two-factor authentication
#
# - Password policies meeting healthcare security standards
#
# - Emergency override procedures for patient care continuity
#
# - Mobile device management with location verification
#
# - Single Sign-On integration with hospital Active Directory
#
# - Session timeout controls for unattended workstations
#
# - Biometric authentication support for mobile devices
#
# - Enhanced monitoring for privileged accounts
#
# - Security incident logging and notification systems
#
# - Department-specific dashboard customization
#
# - Account lockout procedures with IT Security integration
#
# - Multi-factor authentication for high-risk access scenarios

Feature: User Authentication
  As a healthcare provider
  I want to securely log into the ED management system
  So that I can access patient information and perform my clinical duties with proper authorization

  Background:
    Given the ED management system is operational
    And the authentication module is active
    And the badge scanning system is functional
    And user credentials database is accessible
    And audit logging is enabled

  Scenario: Successful nurse login with username, password, and badge scan
    Given I am "Nurse Sarah Johnson" with valid system credentials
    And my user account has the following attributes:
      | Account Attribute     | Value                                      |
      | Username              | sjohnson                                   |
      | Employee ID           | EMP-12345                                 |
      | Badge Number          | BADGE-67890                               |
      | Role                  | Registered Nurse                          |
      | Department            | Emergency Department                       |
      | Security Clearance    | Level 2 - Patient Care                   |
      | Account Status        | Active                                    |
    And my badge is properly programmed and functional
    When I enter my username "sjohnson" in the login field
    And I enter my password in the password field
    And I scan my badge "BADGE-67890" using the badge reader
    And I click the "Login" button
    Then the system authenticates my credentials successfully:
      | Authentication Check  | Verification Result                        |
      | Username Validation   | Valid - User exists in system             |
      | Password Verification | Correct - Password hash matches           |
      | Badge Authentication  | Valid - Badge number matches employee     |
      | Account Status        | Active - Account is enabled              |
      | Role Authorization    | Authorized - RN role has ED access       |
    And the access attempt is logged with details:
      | Audit Log Entry       | Information Recorded                       |
      | User ID               | sjohnson (Sarah Johnson)                  |
      | Login Timestamp       | 2025-06-24 08:00:15                       |
      | Workstation ID        | WS-ED-05                                  |
      | IP Address            | 192.168.1.105                            |
      | Authentication Method | Username/Password + Badge Scan            |
      | Login Success         | True                                      |
      | Session ID            | SES-ABC123DEF456                          |
    And my personalized dashboard is displayed:
      | Dashboard Section     | Nurse-Specific Content                     |
      | Patient Assignment    | Current patients assigned to Sarah Johnson |
      | Task List            | Medication administration, vitals due      |
      | Alerts/Notifications | Critical lab values, patient call lights  |
      | Quick Access Tools   | Medication lookup, dosage calculator       |
      | Shift Information    | Shift start: 07:00, Break schedule        |
      | Department Status    | Current census, bed availability          |

  Scenario: Failed login attempt with incorrect password
    Given I am attempting to log in as "Nurse Mike Chen"
    And my username is "mchen" 
    And my correct password is stored in the system
    When I enter my username "mchen"
    And I enter an incorrect password "wrongpassword"
    And I scan my valid badge "BADGE-54321"
    And I click the "Login" button
    Then the system rejects the authentication:
      | Authentication Check  | Verification Result                        |
      | Username Validation   | Valid - User exists                       |
      | Password Verification | Failed - Password does not match          |
      | Badge Authentication  | Valid - Badge number correct              |
      | Overall Result        | Authentication Failed                     |
    And a security alert is logged:
      | Security Log Entry    | Information Recorded                       |
      | User ID               | mchen (Mike Chen)                         |
      | Failed Login Time     | 2025-06-24 08:15:32                       |
      | Workstation ID        | WS-ED-03                                  |
      | IP Address            | 192.168.1.103                            |
      | Failure Reason        | Incorrect Password                        |
      | Attempt Count         | 1 of 3 allowed attempts                   |
    And an error message is displayed:
      | Error Message         | "Invalid credentials. Please check your username and password." |
      | Security Notice       | "2 attempts remaining before account lockout"                   |
    And I remain on the login screen to retry authentication

  Scenario: Account lockout after multiple failed login attempts
    Given I am "Dr. Amanda Wilson" attempting to log in
    And my account allows 3 failed login attempts before lockout
    And I have already made 2 failed login attempts today
    When I enter my username "awilson"
    And I enter another incorrect password
    And I scan my badge and click login
    Then the system locks my account:
      | Lockout Response      | Security Action                            |
      | Account Status        | Locked - Exceeded maximum failed attempts |
      | Lockout Duration      | 30 minutes automatic unlock               |
      | Manual Override       | IT Security can unlock immediately        |
    And a security incident is logged:
      | Security Incident     | Details                                    |
      | Event Type            | Account Lockout - Excessive Failed Attempts|
      | User Account          | awilson (Dr. Amanda Wilson)               |
      | Lockout Time          | 2025-06-24 14:22:18                       |
      | Failed Attempts       | 3 consecutive failures                     |
      | Workstation ID        | WS-ED-07                                  |
    And security notifications are sent:
      | Notification Target   | Alert Content                              |
      | IT Security Team      | Account lockout alert for Dr. Wilson      |
      | Department Supervisor | Staff member unable to access system      |
      | User Email            | Account locked - contact IT for assistance|
    And a lockout message is displayed:
      | Lockout Message       | "Account temporarily locked due to multiple failed login attempts. Contact IT Security or wait 30 minutes." |

  Scenario: Login with expired password requiring password reset
    Given I am "Nurse Patricia Martinez" with valid credentials
    And my password expired 5 days ago according to policy
    And the system requires password changes every 90 days
    When I enter my username "pmartinez"
    And I enter my current (expired) password
    And I scan my badge successfully
    Then the system identifies the expired password:
      | Password Check        | Status                                     |
      | Password Validity     | Expired - 5 days past expiration         |
      | Grace Period          | Exceeded - No grace logins remaining      |
      | Password Age          | 95 days old (5 days over 90-day limit)   |
    And the password reset workflow is initiated:
      | Reset Process         | Required Actions                           |
      | Identity Verification | Additional security questions prompted     |
      | New Password Entry    | Password must meet complexity requirements |
      | Password Confirmation | Confirm new password entry                |
      | Security Questions    | Update security questions if needed       |
    And password policy requirements are displayed:
      | Policy Requirement    | Specification                              |
      | Minimum Length        | 12 characters                             |
      | Character Types       | Upper, lower, number, special character   |
      | Password History      | Cannot reuse last 12 passwords           |
      | Common Words          | Cannot use dictionary words or personal info|
    And upon successful password reset, normal login proceeds

  Scenario: Role-based dashboard customization after successful login
    Given multiple users with different roles log in successfully
    When "Dr. Emily Rodriguez" (Emergency Physician) logs in
    Then her physician dashboard is displayed with:
      | Physician Dashboard   | Specialized Content                        |
      | Patient Queue         | Patients waiting to be seen by priority    |
      | Active Orders         | Lab results, imaging pending review       |
      | Critical Alerts       | Abnormal vitals, critical lab values      |
      | Decision Support      | Clinical guidelines, drug interactions    |
      | Documentation Tools   | Templates for common conditions           |
    When "Charge Nurse Williams" logs in
    Then her charge nurse dashboard shows:
      | Charge Nurse Dashboard| Management-Focused Content                 |
      | Department Overview   | Bed status, staff assignments, census     |
      | Resource Management   | Equipment status, supply levels           |
      | Staff Coordination    | Break schedules, assignments, coverage    |
      | Quality Metrics       | Wait times, patient satisfaction, safety  |
      | Administrative Tasks  | Reporting, scheduling, policy updates     |
    When "Tech Support Anderson" logs in
    Then his technical dashboard displays:
      | Technical Dashboard   | System Administration Content              |
      | System Status         | Server health, network connectivity       |
      | User Management       | Account status, permission changes        |
      | Audit Logs           | System access, security events            |
      | Maintenance Tools     | Backup status, system updates             |

  Scenario: Single Sign-On (SSO) integration with hospital directory
    Given the hospital uses Active Directory for centralized authentication
    And SSO is configured for the ED management system
    When I am "Dr. Robert Kim" already logged into the hospital network
    And I access the ED management system from my workstation
    Then the system recognizes my existing authentication:
      | SSO Authentication    | Process                                    |
      | Network Credential    | Windows authentication token validated    |
      | Directory Lookup      | User details retrieved from Active Directory|
      | Role Mapping          | Hospital role mapped to ED system permissions|
      | Session Creation      | Automatic login without credential prompt  |
    And I am automatically logged in with appropriate permissions
    And the SSO login is logged for audit purposes:
      | SSO Audit Entry       | Information                                |
      | Authentication Method | Single Sign-On via Active Directory       |
      | Network Username      | HOSPITAL\\rkim                            |
      | Automatic Login Time  | 2025-06-24 09:30:45                       |
      | Workstation Domain    | Verified hospital domain computer          |

  Scenario: Mobile device authentication with additional security
    Given I am using the mobile ED app on my smartphone
    And mobile access requires enhanced security measures
    When I attempt to log in on my mobile device
    Then additional authentication factors are required:
      | Mobile Security Factor| Requirement                                |
      | Device Registration   | Device must be registered with IT          |
      | Biometric Auth        | Fingerprint or face recognition required   |
      | Location Verification | GPS confirms user is within hospital grounds|
      | Time-based Token      | 6-digit code from authenticator app       |
    And I complete multi-factor authentication:
      | MFA Step              | Verification Process                       |
      | Username/Password     | Standard credential verification           |
      | Biometric Scan        | Fingerprint verified against enrolled pattern|
      | Location Check        | GPS coordinates within allowed radius      |
      | Time Token            | Authenticator app code validated          |
    And mobile-specific security controls are applied:
      | Mobile Control        | Security Measure                           |
      | Session Timeout       | 15-minute inactivity timeout             |
      | Screen Lock           | Auto-lock after 2 minutes idle           |
      | Data Encryption       | All data encrypted on device             |
      | Remote Wipe           | IT can remotely clear data if device lost |

  Scenario: Emergency override authentication during system issues
    Given the primary authentication system is experiencing technical difficulties
    And there is a patient emergency requiring immediate system access
    And I am "Dr. Lisa Chen" needing urgent access to patient records
    When I request emergency override access
    Then the emergency authentication protocol is activated:
      | Emergency Protocol    | Override Process                           |
      | Identity Verification | Manual verification by IT Security         |
      | Supervisor Approval   | Department head authorization required     |
      | Time-Limited Access   | 2-hour temporary access granted           |
      | Enhanced Monitoring   | All actions logged for later review       |
    And emergency access is granted with restrictions:
      | Access Limitation     | Restriction Details                        |
      | Time Limit            | Access expires automatically in 2 hours   |
      | Function Restrictions | Read-only access to critical patient data |
      | Audit Trail          | Enhanced logging of all actions           |
      | Supervisor Oversight  | Real-time monitoring of override usage     |
    And emergency access usage is documented:
      | Emergency Documentation| Required Information                      |
      | Medical Justification | Patient condition requiring urgent access |
      | Approving Authority   | Name and title of authorizing supervisor  |
      | Access Duration       | Exact start and end times of override use |
      | Actions Performed     | Detailed log of all system activities    |
