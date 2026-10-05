// Shared configuration for the Selenium C# test suite.
//
// Override BASE_URL to point at a running instance of the app, e.g.:
//   BASE_URL=http://localhost:5173 dotnet test

namespace EmergencyCareSystem.SeleniumTests.Support;

public static class Config
{
    public static readonly string BaseUrl =
        Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5173";
}
