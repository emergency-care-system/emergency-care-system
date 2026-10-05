# tests-with-playwright-c-sharp

The 171 scenarios from `tests-with-given-when-then-features/*.feature`, in
Playwright + C# (NUnit). A port of `tests-with-selenium-javascript/` (the
Playwright-specific idioms follow `tests-with-playwright-python/`); see
[`spec/testing.md`](../spec/testing.md) for the shared contract. All data in
the app is synthetic.

```sh
pnpm run dev                                # in the repo root: app at http://localhost:5173
dotnet build tests-with-playwright-c-sharp
pnpm exec playwright install chromium       # once; the same Playwright 1.63 browsers are reused
                                            # (or: pwsh tests-with-playwright-c-sharp/bin/Debug/net10.0/playwright.ps1 install chromium)
dotnet test tests-with-playwright-c-sharp   # or: pnpm run test:playwright-c-sharp
```

- `BASE_URL` points at another instance of the app; there is no automatic
  dev-server startup.
- Chromium is headless unless `HEADLESS=0`.
- Playwright locators are strict, so the helpers act on `.First` of a
  `data-testid` match (like Selenium's `findElement`); see
  `spec/testing.md`.
