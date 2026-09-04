# # Ambulance Arrival Registration
#
# **Key Features:**
#
# - Unknown patient registration workflow for emergency situations
#
# - Temporary record creation with placeholder IDs
#
# - Identity verification flagging system
#
# - EMS information integration capabilities
#
# **Comprehensive Scenarios:**
#
# - Unconscious patient - Complete unknown patient registration
#
# - Partial identification - Handling patients with some identifying information
#
# - Patient consciousness recovery - Converting temporary to verified records
#
# - Mass casualty events - Managing multiple unknown patients
#
# - Insufficient information - Validation and error handling
#
# - Identity verification - Post-stabilization identification process
#
# **Emergency-Specific Features:**
#
# - Placeholder ID generation (UNK prefix)
#
# - EMS information capture
#
# - Mass casualty incident handling
#
# - Social services integration
#
# - Identity verification workflows
#
# - Record merging capabilities

Feature: Ambulance Arrival Registration
  As a registration staff member
  I want to register patients who arrive by ambulance without identification
  So that they can receive immediate medical care while maintaining proper documentation

  Background:
    Given the ED management system is operational
    And I am logged in as registration staff
    And the "unknown patient" registration module is available

  Scenario: Register unconscious patient brought by ambulance
    Given an ambulance arrives with a patient who cannot provide identification
    And the patient is unconscious and has no identification documents
    And EMS provides the following information:
      | Field                 | Value                    |
      | Estimated Age         | 45-50 years              |
      | Gender                | Male                     |
      | Chief Complaint       | Motor vehicle accident   |
      | Vital Signs           | BP: 90/60, HR: 120      |
      | Incident Location     | Highway 55 Mile Marker 12|
      | EMS Unit              | Ambulance 205            |
      | Arrival Time          | 14:30                    |
    When I select "Unknown Patient" registration type
    And I enter the available information from EMS
    And I submit the registration
    Then the system creates a temporary patient record
    And the system assigns a placeholder ID starting with "UNK"
    And the patient record is flagged for "Identity Verification Required"
    And the patient is immediately queued for triage
    And a notification is sent to the charge nurse about the unknown patient
    And the record shows status as "Temporary - Pending Identification"

  Scenario: Register patient with partial identification from personal effects
    Given an ambulance arrives with a patient who cannot provide identification
    And the patient has a wallet with partial information
    And EMS provides the following information:
      | Field                 | Value                    |
      | Estimated Age         | 30-35 years              |
      | Gender                | Female                   |
      | Chief Complaint       | Drug overdose            |
      | Found Name            | Sarah (from credit card) |
      | Partial Phone         | 555-1234 (last 4 digits)|
    When I select "Unknown Patient" registration type
    And I enter the EMS information including partial identity details
    And I mark the identity fields as "Unverified"
    And I submit the registration
    Then the system creates a temporary patient record
    And the system assigns a placeholder ID starting with "UNK"
    And the partial identity information is stored with "Unverified" status
    And the patient record is flagged for "Identity Verification Required"
    And a task is created for social services to assist with identification

  Scenario: Register patient who becomes conscious during registration
    Given an ambulance arrives with a patient who initially cannot provide identification
    And I have started the "Unknown Patient" registration process
    When the patient becomes conscious and provides identification:
      | Field                 | Value                    |
      | Full Name             | Michael Johnson          |
      | Date of Birth         | 1980-12-15               |
      | Phone Number          | 555-876-5432             |
    And I verify the provided identification
    Then the system converts the temporary record to a verified patient record
    And the placeholder ID is replaced with a permanent medical record number
    And the "Identity Verification Required" flag is removed
    And the patient demographic information is updated
    And a note is added documenting the identification process

  Scenario: Handle multiple unknown patients from mass casualty incident
    Given multiple ambulances arrive from a mass casualty incident
    And none of the patients can provide identification
    When I select "Unknown Patient - Mass Casualty" registration type
    And I enter the incident information:
      | Field                 | Value                    |
      | Incident Type         | Multi-vehicle accident   |
      | Incident Location     | Interstate 70 Exit 45    |
      | Total Patients        | 4                        |
    And I register each patient with EMS-provided information
    Then the system creates temporary records for all patients
    And each patient gets a sequential placeholder ID (UNK-001, UNK-002, etc.)
    And all records are linked to the same incident number
    And the mass casualty protocol is activated
    And notifications are sent to administration and social services

  Scenario: Attempt to register unknown patient without EMS information
    Given an ambulance arrives with a patient who cannot provide identification
    And EMS has minimal information available
    When I select "Unknown Patient" registration type
    And I attempt to submit with only basic information:
      | Field                 | Value                    |
      | Gender                | Unknown                  |
      | Estimated Age         | Unknown                  |
      | Chief Complaint       |                          |
    Then the system displays a warning "Insufficient information for registration"
    And the system requires minimum data fields:
      | Required Field        | Requirement              |
      | Estimated Age Range   | Must be provided         |
      | Gender                | Must be Male, Female, or Unknown |
      | Chief Complaint       | Must be provided         |
    And the registration cannot be completed until minimum requirements are met

  Scenario: Identity verification process after patient stabilization
    Given a patient was registered as "Unknown Patient"
    And the patient has now stabilized
    And the patient can provide identification
    When the nurse initiates the identity verification process
    And the patient provides valid identification:
      | Field                 | Value                    |
      | Full Name             | Robert Davis             |
      | Date of Birth         | 1975-08-20               |
      | Social Security       | XXX-XX-1234 (last 4)    |
    And the identification is verified
    Then the system merges the temporary record with verified information
    And the "Identity Verification Required" flag is cleared
    And a permanent medical record number is assigned
    And all clinical documentation is preserved under the new verified record
    And billing information is updated with verified patient details
