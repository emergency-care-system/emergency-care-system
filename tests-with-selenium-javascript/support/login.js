// Logs in as the identity named in a feature file's Background step, e.g.
//   Given I am logged in as a registration clerk
//   Given I am logged in as "Dr. Smith"
//   Given I am logged in as "Dr. Smith" on the mobile app
//
// `identity` is the exact text that follows "logged in as" (quotes
// stripped). `options.mobile` mirrors an "on the mobile app" suffix.

import { By, until } from 'selenium-webdriver';
import { BASE_URL } from './config.js';

export async function login(driver, identity, options = {}) {
  const url = options.mobile ? `${BASE_URL}/login?viewport=mobile` : `${BASE_URL}/login`;
  await driver.get(url);

  const identityField = await driver.wait(
    until.elementLocated(By.css('[data-testid="login-identity"]')),
    10000
  );
  await identityField.sendKeys(identity);
  await driver.findElement(By.css('[data-testid="login-submit"]')).click();

  // Wait for the authenticated app shell to confirm the session started.
  await driver.wait(until.elementLocated(By.css('[data-testid="app-root"]')), 10000);
}

// Confirms the Background precondition `Given the ED management system is
// operational` by loading the app and waiting for its shell to render.
export async function verifySystemIsOperational(driver) {
  await driver.get(BASE_URL);
  await driver.wait(until.elementLocated(By.css('[data-testid="app-root"]')), 10000);
}
