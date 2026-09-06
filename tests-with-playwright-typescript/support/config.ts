// Shared configuration for the Playwright TypeScript test suite.
//
// Override BASE_URL to point at a running instance of the app, e.g.:
//   BASE_URL=http://localhost:5173 pnpm run test:playwright-typescript
// See ../../playwright.typescript.config.ts, which also reads this and
// sets it as Playwright's `use.baseURL`.

export const BASE_URL: string = process.env.BASE_URL || 'http://localhost:5173';
