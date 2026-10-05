// Builds a Chrome WebDriver instance. Selenium Manager (bundled with
// selenium-java) auto-detects the installed browser and downloads a
// matching driver, so no manual chromedriver setup is needed.
//
// Chrome opens a visible window by default (like the other Selenium
// suites); set HEADLESS=1 to run without one.

package emergencycaresystem.support;

import org.openqa.selenium.WebDriver;
import org.openqa.selenium.chrome.ChromeDriver;
import org.openqa.selenium.chrome.ChromeOptions;

public final class DriverFactory {
    private DriverFactory() {}

    public static WebDriver build() {
        ChromeOptions options = new ChromeOptions();
        String headless = System.getenv("HEADLESS");
        if ("1".equals(headless) || "true".equals(headless)) {
            options.addArguments("--headless=new");
        }
        return new ChromeDriver(options);
    }
}
