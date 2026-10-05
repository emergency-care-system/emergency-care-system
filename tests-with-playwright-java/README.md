# tests-with-playwright-java

The 171 scenarios from `tests-with-given-when-then-features/*.feature`, in
Playwright + Java (JUnit 5, Maven). A port of
`tests-with-selenium-javascript/` (the Playwright-specific idioms follow
`tests-with-playwright-python/`); see [`spec/testing.md`](../spec/testing.md)
for the shared contract. All data in the app is synthetic.

```sh
pnpm run dev                                      # in the repo root: app at http://localhost:5173
mvn -f tests-with-playwright-java/pom.xml test    # JDK 21+; or: pnpm run test:playwright-java
```

- Playwright Java downloads its browsers on first use (or reuses the ones
  `pnpm exec playwright install chromium` installed, since both are 1.63).
- `BASE_URL` points at another instance of the app; there is no automatic
  dev-server startup.
- Chromium is headless unless `HEADLESS=0`.
- If `mvn` reports that `JAVA_HOME` is not defined correctly, point it at a
  JDK (e.g. `export JAVA_HOME=$(/usr/libexec/java_home)` on macOS).
- Playwright locators are strict, so the helpers act on `.first()` of a
  `data-testid` match (like Selenium's `findElement`); see
  `spec/testing.md`.
