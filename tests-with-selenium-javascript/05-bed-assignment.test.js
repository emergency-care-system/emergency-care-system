// Selenium WebDriver + Mocha test for
// spec/features/05-bed-assignment.feature
//
// Assumes the app exposes data-testid attributes matching each Gherkin
// field label (kebab-cased, see support/fields.js) and the shared
// data-testid contract in support/login.js (login-identity, login-submit,
// app-root).

import { strict as assert } from 'assert';
import { buildDriver } from './support/build-driver.js';
import { login, verifySystemIsOperational } from './support/login.js';
import { getText, waitForTestId } from './support/fields.js';
import { By } from 'selenium-webdriver';

describe('Feature: Bed Assignment', function () {
  this.timeout(20000);
  let driver;

  before(async () => {
    driver = await buildDriver();
  });

  after(async () => {
    await driver.quit();
  });

  beforeEach(async () => {
    // Background:
    //   Given the ED management system is operational
    //   And I am logged in as a charge nurse
    //   And the bed management module is active
    //   And the patient prioritization algorithm is enabled
    await verifySystemIsOperational(driver);
    await login(driver, 'a charge nurse');
    // The bed management module and patient prioritization algorithm are
    // assumed to be pre-seeded/enabled test data.

    const featureNavLink = await waitForTestId(driver, 'Nav Bed Assignment');
    await featureNavLink.click();
    await waitForTestId(driver, 'Bed Assignment Panel');
  });

  it('Assign standard room to highest acuity patient', async () => {
    // Given multiple patients are waiting for beds:
    // And a standard room "ED-12" becomes available
    // (assumed pre-seeded test data)

    // When I request bed assignment recommendations
    await driver.findElement(By.css('[data-testid="request-recommendations-button"]')).click();

    // Then the system suggests "Maria Gonzalez" as the top recommendation
    await waitForTestId(driver, 'Top Recommendation');
    const topRecommendation = await getText(driver, 'Top Recommendation');
    assert.strictEqual(topRecommendation, 'Maria Gonzalez');

    // And the recommendation shows:
    const recommendationFields = [
      { Field: 'Recommended Patient', Value: 'Maria Gonzalez' },
      { Field: 'ESI Level', Value: '2' },
      { Field: 'Wait Time', Value: '45 minutes' },
      { Field: 'Room Match', Value: 'Standard room suitable' },
      { Field: 'Rationale', Value: 'Highest acuity patient requiring standard bed' },
    ];
    for (const { Field, Value } of recommendationFields) {
      assert.strictEqual(await getText(driver, Field), Value);
    }

    // And the system displays updated wait times for remaining patients:
    const waitTimeUpdates = await driver.findElements(By.css('[data-testid="wait-time-update-row"]'));
    assert.strictEqual(waitTimeUpdates.length, 3);
  });

  it('Assign isolation room based on patient requirements', async () => {
    // Given multiple patients are waiting for beds:
    // And an isolation room "ED-ISO-2" becomes available
    // (assumed pre-seeded test data)

    // When I request bed assignment recommendations
    await driver.findElement(By.css('[data-testid="request-recommendations-button"]')).click();

    // Then the system suggests "Alex Johnson" as the top recommendation
    await waitForTestId(driver, 'Top Recommendation');
    const topRecommendation = await getText(driver, 'Top Recommendation');
    assert.strictEqual(topRecommendation, 'Alex Johnson');

    // And the recommendation shows:
    const recommendationFields = [
      { Field: 'Recommended Patient', Value: 'Alex Johnson' },
      { Field: 'Room Type', Value: 'Isolation room' },
      { Field: 'Rationale', Value: 'Patient requires isolation precautions' },
      { Field: 'Infection Control', Value: 'Airborne precautions needed' },
    ];
    for (const { Field, Value } of recommendationFields) {
      assert.strictEqual(await getText(driver, Field), Value);
    }

    // And the system displays that other patients cannot use this room type
    const roomTypeRestriction = await getText(driver, 'Room Type Restriction Notice');
    assert.ok(roomTypeRestriction.length > 0);

    // And the estimated wait times for standard rooms remain unchanged for other patients
    const standardRoomWaitTimeChange = await getText(driver, 'Standard Room Wait Time Change');
    assert.strictEqual(standardRoomWaitTimeChange, 'Unchanged');
  });

  it('Handle pediatric room assignment', async () => {
    // Given multiple patients are waiting for beds:
    // And a pediatric room "ED-PEDS-3" becomes available
    // (assumed pre-seeded test data)

    // When I request bed assignment recommendations
    await driver.findElement(By.css('[data-testid="request-recommendations-button"]')).click();

    // Then the system suggests "Sarah Mitchell" as the top recommendation
    await waitForTestId(driver, 'Top Recommendation');
    const topRecommendation = await getText(driver, 'Top Recommendation');
    assert.strictEqual(topRecommendation, 'Sarah Mitchell');

    // And the recommendation shows:
    const recommendationFields = [
      { Field: 'Recommended Patient', Value: 'Sarah Mitchell' },
      { Field: 'Age', Value: '4 years old' },
      { Field: 'ESI Level', Value: '2' },
      { Field: 'Room Type', Value: 'Pediatric room' },
      { Field: 'Rationale', Value: 'Highest acuity pediatric patient' },
    ];
    for (const { Field, Value } of recommendationFields) {
      assert.strictEqual(await getText(driver, Field), Value);
    }

    // And the system displays updated wait times for remaining pediatric patients:
    const pediatricWaitTimeUpdates = await driver.findElements(By.css('[data-testid="wait-time-update-row"]'));
    assert.strictEqual(pediatricWaitTimeUpdates.length, 2);

    // And David Chen remains in the adult standard room queue
    const davidChenQueueStatus = await getText(driver, 'David Chen Queue Status');
    assert.match(davidChenQueueStatus, /adult standard room queue/i);
  });

  it('No suitable patients for available room type', async () => {
    // Given multiple patients are waiting for beds:
    // And a trauma room "ED-TRAUMA-1" becomes available
    // (assumed pre-seeded test data)

    // When I request bed assignment recommendations
    await driver.findElement(By.css('[data-testid="request-recommendations-button"]')).click();

    // Then the system displays "No suitable patients for trauma room"
    const noSuitablePatientsMessage = await getText(driver, 'No Suitable Patients Message');
    assert.strictEqual(noSuitablePatientsMessage, 'No suitable patients for trauma room');

    // And the system suggests:
    const suggestions = [
      { 'Recommendation Type': 'Alternative Use', Details: 'Consider using for high acuity standard patients' },
      { 'Recommendation Type': 'Room Conversion', Details: 'Can be downgraded to standard room if needed' },
      { 'Recommendation Type': 'Hold for Emergency', Details: 'Keep available for incoming trauma cases' },
    ];
    for (const suggestion of suggestions) {
      assert.strictEqual(await getText(driver, suggestion['Recommendation Type']), suggestion.Details);
    }

    // And the system maintains the trauma room as available
    const traumaRoomStatus = await getText(driver, 'Trauma Room Status');
    assert.strictEqual(traumaRoomStatus, 'Available');

    // And no patient assignments are automatically made
    const autoAssignments = await driver.findElements(By.css('[data-testid="patient-assignment"]'));
    assert.strictEqual(autoAssignments.length, 0);
  });

  it('Handle multiple rooms becoming available simultaneously', async () => {
    // Given multiple patients are waiting for beds:
    // And multiple rooms become available:
    // (assumed pre-seeded test data)

    // When I request bed assignment recommendations
    await driver.findElement(By.css('[data-testid="request-recommendations-button"]')).click();

    // Then the system provides multiple recommendations:
    const recommendations = await driver.findElements(By.css('[data-testid="bed-recommendation-row"]'));
    assert.strictEqual(recommendations.length, 3);

    // And the system updates wait times for all remaining patients
    await waitForTestId(driver, 'Wait Times Updated Notice');

    // And the recommendations are ranked by patient acuity priority
    const rankingOrder = await getText(driver, 'Recommendation Ranking Order');
    assert.strictEqual(rankingOrder, 'Ranked by patient acuity priority');
  });

  it('Consider patient gender for room assignment', async () => {
    // Given multiple patients are waiting for beds:
    // And a standard room "ED-8" becomes available
    // And the room currently has a male patient in the adjacent bed
    // (assumed pre-seeded test data)

    // When I request bed assignment recommendations considering privacy preferences
    await driver.findElement(By.css('[data-testid="request-recommendations-button"]')).click();

    // Then the system suggests "Anthony Clark" as the top recommendation
    await waitForTestId(driver, 'Top Recommendation');
    const topRecommendation = await getText(driver, 'Top Recommendation');
    assert.strictEqual(topRecommendation, 'Anthony Clark');

    // And the recommendation includes:
    const recommendationFields = [
      { Field: 'Privacy Consideration', Value: 'Same gender as adjacent patient' },
      { Field: 'Alternative Option', Value: 'Michelle Lee (if privacy not a concern)' },
    ];
    for (const { Field, Value } of recommendationFields) {
      assert.strictEqual(await getText(driver, Field), Value);
    }

    // And I can override the gender consideration if clinically necessary
    await waitForTestId(driver, 'Override Gender Consideration');
  });

  it('Handle bed assignment during high volume period', async () => {
    // Given the ED is operating at 95% capacity
    // And multiple high-acuity patients are waiting:
    // And a standard room "ED-6" becomes available
    // (assumed pre-seeded test data)

    // When I request bed assignment recommendations during crisis mode
    await driver.findElement(By.css('[data-testid="request-recommendations-button"]')).click();

    // Then the system prioritizes "Crisis Patient A" despite room type mismatch
    await waitForTestId(driver, 'Top Recommendation');
    const topRecommendation = await getText(driver, 'Top Recommendation');
    assert.strictEqual(topRecommendation, 'Crisis Patient A');

    // And the system displays:
    const alerts = [
      { 'Alert Type': 'High Volume Alert', Message: 'ED at capacity - emergency protocols active' },
      { 'Alert Type': 'Room Flex Option', Message: 'Standard room can accommodate ESI Level 1' },
      { 'Alert Type': 'Resource Alert', Message: 'Additional equipment may be needed' },
    ];
    for (const alert of alerts) {
      assert.strictEqual(await getText(driver, alert['Alert Type']), alert.Message);
    }

    // And the system suggests moving lower acuity patients to make trauma rooms available
    const traumaRoomSuggestion = await getText(driver, 'Trauma Room Availability Suggestion');
    assert.match(traumaRoomSuggestion, /moving lower acuity patients/i);
  });

  it('Update wait times after bed assignment', async () => {
    // Given the following patients are waiting:
    // And I assign Patient A to the available room
    // (assumed pre-seeded test data)

    // When the bed assignment is confirmed
    await driver.findElement(By.css('[data-testid="confirm-bed-assignment-button"]')).click();

    // Then the system recalculates wait times for remaining patients:
    const waitTimeUpdates = await driver.findElements(By.css('[data-testid="wait-time-update-row"]'));
    assert.strictEqual(waitTimeUpdates.length, 3);

    // And the updated wait times are displayed on the patient tracking board
    await waitForTestId(driver, 'Patient Tracking Board');

    // And family members are notified of updated estimates via the patient portal
    const familyNotificationStatus = await getText(driver, 'Family Notification Status');
    assert.match(familyNotificationStatus, /notified/i);
  });

  it('Handle bed assignment rejection and alternative selection', async () => {
    // Given the system recommends "John Smith" for room "ED-10"
    // And John Smith has ESI Level 2 with chest pain
    // (assumed pre-seeded test data)

    // When I review the recommendation
    await waitForTestId(driver, 'Top Recommendation');

    // And I determine that John Smith needs cardiac monitoring not available in ED-10
    // (clinical judgement, no direct UI action)

    // And I reject the system recommendation
    await driver.findElement(By.css('[data-testid="reject-recommendation-button"]')).click();

    // Then the system provides alternative recommendations:
    const alternativeRecommendations = await driver.findElements(By.css('[data-testid="alternative-recommendation-row"]'));
    assert.strictEqual(alternativeRecommendations.length, 2);

    // And the system suggests alternative rooms for John Smith:
    const alternativeRooms = await driver.findElements(By.css('[data-testid="alternative-room-row"]'));
    assert.strictEqual(alternativeRooms.length, 2);

    // And I can select an alternative patient or wait for appropriate room for John Smith
    await waitForTestId(driver, 'Select Alternative Patient');
  });
});
