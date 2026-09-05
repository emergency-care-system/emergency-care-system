// Shared configuration for the Selenium test suite.
//
// Override BASE_URL to point at a running instance of the app, e.g.:
//   BASE_URL=http://localhost:5173 npx mocha

export const BASE_URL = process.env.BASE_URL || 'http://localhost:5173';
