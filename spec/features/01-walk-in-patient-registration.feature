# # Walk-in Patient Registration
#
# **Key Components:**
#
# - Feature description with user story format (As a... I want... So that...)
#
# - Background section for common setup steps
#
# - Multiple scenarios covering different use cases including happy path, edge cases, and error conditions
#
# **Enhanced Test Coverage:**
#
# - Successful registration with complete information
#
# - Registration with missing insurance (self-pay scenario)
#
# - Duplicate patient detection
#
# - Data validation and error handling
#
# **Gherkin Best Practices:**
#
# - Uses data tables for structured input
#
# - Includes specific validation steps
#
# - Covers both positive and negative test cases
#
# - Clear, actionable steps that can be automated
#
# - Descriptive scenario names that explain the business value

Feature: Walk-in Patient Registration
  As a registration clerk
  I want to register new patients who arrive without prior registration
  So that they can be properly identified and queued for triage

  Background:
    Given the ED management system is operational
    And I am logged in as a registration clerk

  Scenario: Successfully register a new walk-in patient
    Given a new patient arrives at the ED without prior registration
    And the patient provides valid identification
    When I enter the patient's demographic information:
      | Field           | Value              |
      | First Name      | John               |
      | Last Name       | Doe                |
      | Date of Birth   | 1985-06-15         |
      | Phone Number    | 555-123-4567       |
      | Address         | 123 Main St        |
      | City            | Springfield        |
      | State           | IL                 |
      | Zip Code        | 62701              |
    And I enter the patient's insurance details:
      | Field           | Value              |
      | Insurance Type  | Blue Cross         |
      | Policy Number   | BC123456789        |
      | Group Number    | GRP001             |
    And I submit the registration form
    Then the system creates a unique patient record
    And the system assigns a medical record number
    And the patient is queued for triage
    And I see a confirmation message "Patient successfully registered"
    And the medical record number is displayed

  Scenario: Register patient with missing insurance information
    Given a new patient arrives at the ED without prior registration
    And the patient does not have insurance information
    When I enter the patient's demographic information:
      | Field           | Value              |
      | First Name      | Jane               |
      | Last Name       | Smith              |
      | Date of Birth   | 1990-03-22         |
      | Phone Number    | 555-987-6543       |
      | Address         | 456 Oak Ave        |
    And I select "Self-Pay" as the insurance type
    And I submit the registration form
    Then the system creates a unique patient record
    And the system assigns a medical record number
    And the patient is queued for triage
    And the insurance status is marked as "Self-Pay"

  Scenario: Handle duplicate patient registration attempt
    Given a patient with the same name and date of birth already exists in the system
    When I enter the patient's demographic information:
      | Field           | Value              |
      | First Name      | John               |
      | Last Name       | Doe                |
      | Date of Birth   | 1985-06-15         |
    And I submit the registration form
    Then the system displays a warning "Potential duplicate patient found"
    And the system shows existing patient records for verification
    And I can choose to link to existing record or create new record

  Scenario: Registration with invalid demographic data
    Given a new patient arrives at the ED without prior registration
    When I enter incomplete demographic information:
      | Field           | Value              |
      | First Name      | John               |
      | Last Name       |                    |
      | Date of Birth   | invalid-date       |
    And I submit the registration form
    Then the system displays validation errors:
      | Field           | Error Message              |
      | Last Name       | Last name is required      |
      | Date of Birth   | Invalid date format        |
    And the patient record is not created
    And the form remains open for correction
