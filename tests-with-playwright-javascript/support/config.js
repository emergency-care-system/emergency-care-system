// Shared configuration for the Playwright test suite.
//
// Override BASE_URL to point at a running instance of the app, e.g.:
//   BASE_URL=http://localhost:5173 npx playwright test
// See ../../playwright.config.js, which also reads this and sets it as
// Playwright's `use.baseURL`.

export const BASE_URL = process.env.BASE_URL || 'http://localhost:5173';
