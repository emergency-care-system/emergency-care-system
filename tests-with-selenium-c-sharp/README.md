# tests-with-selenium-c-sharp

The 171 scenarios from `tests-with-given-when-then-features/*.feature`, in
Selenium WebDriver + C# (NUnit). A port of `tests-with-selenium-javascript/`;
see [`spec/testing.md`](../spec/testing.md) for the shared `data-testid`
contract and architecture rules. All data in the app is synthetic.

```sh
pnpm run dev                          # in the repo root: app at http://localhost:5173
dotnet test tests-with-selenium-c-sharp   # .NET 10 SDK; or: pnpm run test:selenium-c-sharp
```

- `BASE_URL` points at another instance of the app.
- Chrome opens a visible window unless `HEADLESS=1`.
- Selenium Manager (bundled with `Selenium.WebDriver`) fetches the driver. A
  stale `chromedriver` earlier on `PATH` than your Chrome version makes it
  fail with `session not created`; remove it from `PATH` for the run.
- One `T<NN>…Tests` fixture per feature file, each with its own browser
  (`[OneTimeSetUp]`); scenarios run in order (`[Order]`, `[NonParallelizable]`).
