// Shared configuration for the Playwright C# test suite.
//
// Override BASE_URL to point at a running instance of the app, e.g.:
//   BASE_URL=http://localhost:5173 dotnet test
//
// Unlike the JavaScript Playwright suite, this one drives the Playwright
// library directly rather than the `playwright test` CLI runner, so there
// is no automatic dev-server startup -- start `pnpm run dev` yourself first.

namespace EmergencyCareSystem.PlaywrightTests.Support;

public static class Config
{
    public static readonly string BaseUrl =
        Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5173";
}
