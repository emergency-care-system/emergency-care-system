// Shared configuration for the Selenium TypeScript test suite.
//
// Override BASE_URL to point at a running instance of the app, e.g.:
//   BASE_URL=http://localhost:5173 pnpm run test:selenium-typescript

export const BASE_URL: string = process.env.BASE_URL || 'http://localhost:5173';
