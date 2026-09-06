// Logs in as the identity named in a feature file's Background step, e.g.
//   Given I am logged in as a registration clerk
//   Given I am logged in as "Dr. Smith"
//   Given I am logged in as "Dr. Smith" on the mobile app
//
// `identity` is the exact text that follows "logged in as" (quotes
// stripped). `options.mobile` mirrors an "on the mobile app" suffix.

import { BASE_URL } from './config.js';

export async function login(page, identity, options = {}) {
  const url = options.mobile ? `${BASE_URL}/login?viewport=mobile` : `${BASE_URL}/login`;
  await page.goto(url);

  await page.getByTestId('login-identity').fill(identity);
  await page.getByTestId('login-submit').click();

  // "app-root" is the root layout wrapper -- it's present on the login page
  // itself too, so waiting for it here would resolve immediately without
  // actually confirming the login succeeded. Wait for the sidebar nav
  // instead, which only renders once authenticated.
  await page.getByTestId('feature-nav').waitFor();
}

// Confirms the Background precondition `Given the ED management system is
// operational` by loading the app and waiting for its shell to render.
export async function verifySystemIsOperational(page) {
  await page.goto(BASE_URL);
  await page.getByTestId('app-root').waitFor();
}
