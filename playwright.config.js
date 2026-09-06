// Playwright config for tests-with-playwright-javascript/.
//
// Override BASE_URL to point at a running instance of the app, e.g.:
//   BASE_URL=http://localhost:5173 npx playwright test
// If BASE_URL isn't set, Playwright starts its own `pnpm run dev` server
// (see webServer below) and points at that instead.
import { defineConfig, devices } from '@playwright/test';

const baseURL = process.env.BASE_URL || 'http://localhost:5173';

export default defineConfig({
  testDir: './tests-with-playwright-javascript',
  timeout: 20000,
  fullyParallel: false,
  // Every test file launches its own dedicated browser process in
  // `beforeAll` (see any test file: `chromium.launch()`, not the shared
  // `browser` fixture) -- one shared per-worker browser handling all 22
  // files' logins/navigations back-to-back against a single dev server
  // was unreliable (a small fraction of logins would silently never
  // complete). Multiple *workers* each launching their own browser and
  // hitting that same dev server concurrently is worse: the single Vite
  // dev server can't keep up with concurrent first-load module transforms,
  // so most tests time out. One worker running files one at a time, each
  // with its own fresh browser, is fast (the whole 171-scenario suite
  // finishes in well under a minute) and has run reliably every time.
  workers: 1,
  reporter: 'list',
  use: {
    baseURL,
    // Off by default -- pass --trace on (or --trace retain-on-failure) on
    // the command line to record a trace for a specific debugging session.
    trace: 'off'
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] }
    }
  ],
  // Only used when BASE_URL isn't already pointing at a server you started
  // yourself -- Playwright reuses an already-running dev server instead of
  // starting a second one.
  webServer: process.env.BASE_URL
    ? undefined
    : {
        command: 'pnpm run dev',
        url: baseURL,
        reuseExistingServer: true,
        timeout: 30000
      }
});
