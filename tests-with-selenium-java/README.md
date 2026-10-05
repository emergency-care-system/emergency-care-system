# tests-with-selenium-java

The 171 scenarios from `tests-with-given-when-then-features/*.feature`, in
Selenium WebDriver + Java (JUnit 5, Maven). A port of
`tests-with-selenium-javascript/`; see [`spec/testing.md`](../spec/testing.md)
for the shared `data-testid` contract and architecture rules. All data in the
app is synthetic.

```sh
pnpm run dev                                    # in the repo root: app at http://localhost:5173
mvn -f tests-with-selenium-java/pom.xml test    # JDK 21+; or: pnpm run test:selenium-java
```

- `BASE_URL` points at another instance of the app.
- Chrome opens a visible window unless `HEADLESS=1`.
- If `mvn` reports that `JAVA_HOME` is not defined correctly, point it at a
  JDK (e.g. `export JAVA_HOME=$(/usr/libexec/java_home)` on macOS).
- Selenium Manager (bundled with `selenium-java`) fetches the driver. A stale
  `chromedriver` earlier on `PATH` than your Chrome version makes it fail with
  `session not created`; remove it from `PATH` for the run.
- One `T<NN>…Test` class per feature file, each with its own browser
  (`@BeforeAll`); scenarios run in order (`@TestMethodOrder`).
