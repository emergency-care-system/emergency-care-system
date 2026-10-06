# Commands

Run from the repository root. Use **pnpm**, not npm or yarn, for every
install/run/build command (see [`spec/index.md`](../spec/index.md)). The
exceptions are the two Python suites (`pip`), the C#/Java/Rust suites
(`dotnet`, `mvn`, `cargo`), and the companion site in
`emergency-care-system.github.io/`, which uses **npm** (see
[`companion-site.md`](companion-site.md)).

## App

```sh
pnpm install
pnpm run dev                        # app at http://localhost:5173
pnpm run check                      # svelte-kit sync + svelte-check -- run after any .svelte/.ts change
pnpm run build
```

## Test suites

Every suite except the two Playwright JS/TS ones needs a dev server
already running (`pnpm run dev` or `BASE_URL`). Details, prerequisites and
the shared contract are in [`spec/testing.md`](../spec/testing.md).

```sh
pnpm run test:selenium              # Mocha (JS)
pnpm run test:playwright            # Playwright (JS)
pnpm run test:selenium-typescript   # Mocha (TS, via tsx)
pnpm run test:playwright-typescript # Playwright (TS)

pip install -r requirements.txt     # once, for the two Python suites
playwright install chromium
pnpm run test:selenium-python       # pytest + Selenium
pnpm run test:playwright-python     # pytest + Playwright

# C#, Java and Rust suites need dotnet, a JDK + Maven, and cargo respectively
pnpm run test:selenium-c-sharp      # NUnit + Selenium
pnpm run test:playwright-c-sharp    # NUnit + Playwright
pnpm run test:selenium-java         # JUnit 5 + Selenium (Maven)
pnpm run test:playwright-java       # JUnit 5 + Playwright (Maven)
pnpm run test:selenium-rust         # thirtyfour (cargo test)
pnpm run test:playwright-rust       # playwright-rs (cargo test)
```

See [`environment.md`](environment.md) for the local-environment problems
that look like test failures but aren't.
