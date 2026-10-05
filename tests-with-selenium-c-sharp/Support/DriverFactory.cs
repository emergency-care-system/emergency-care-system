// Builds a Chrome WebDriver instance. Selenium Manager (bundled with
// modern Selenium.WebDriver) auto-detects the installed browser and
// downloads a matching driver, so no manual chromedriver setup is needed.
//
// Chrome opens a visible window by default (like the other Selenium
// suites); set HEADLESS=1 to run without one.

using OpenQA.Selenium.Chrome;

namespace EmergencyCareSystem.SeleniumTests.Support;

public static class DriverFactory
{
    public static IWebDriver Build()
    {
        var options = new ChromeOptions();
        var headless = Environment.GetEnvironmentVariable("HEADLESS");
        if (headless is "1" or "true")
        {
            options.AddArgument("--headless=new");
        }
        return new ChromeDriver(options);
    }
}
