// Helpers for interacting with form fields identified by data-testid
// attributes, and for translating Gherkin data-table field labels (e.g.
// "Given Name", "Date of Birth") into the kebab-case testid the app is
// assumed to expose (e.g. "given-name", "date-of-birth").

package emergencycaresystem.support;

import java.time.Duration;
import java.util.List;
import java.util.Map;
import org.openqa.selenium.By;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.WebElement;
import org.openqa.selenium.support.ui.ExpectedConditions;
import org.openqa.selenium.support.ui.WebDriverWait;

public final class Fields {
    private Fields() {}

    public static String kebabCase(String label) {
        return label.trim().toLowerCase().replaceAll("[^a-z0-9]+", "-").replaceAll("(^-|-$)", "");
    }

    public static String testId(String label) {
        return "[data-testid=\"" + kebabCase(label) + "\"]";
    }

    // A By locator for the element matching a Gherkin label, e.g.
    // locator("Medical Record Number") ->
    // By.cssSelector("[data-testid=\"medical-record-number\"]")
    public static By locator(String label) {
        return By.cssSelector(testId(label));
    }

    // Waits until an element matching the locator is present in the DOM.
    public static WebElement waitLocated(WebDriver driver, By by, int timeoutMs) {
        return new WebDriverWait(driver, Duration.ofMillis(timeoutMs))
                .until(ExpectedConditions.presenceOfElementLocated(by));
    }

    public static WebElement waitForTestId(WebDriver driver, String label) {
        return waitLocated(driver, locator(label), 10000);
    }

    // Fills a text-like input identified by its Gherkin field label.
    public static void fillField(WebDriver driver, String label, String value) {
        WebElement element = waitForTestId(driver, label);
        element.clear();
        element.sendKeys(value);
    }

    // Fills every "Field" / "Value" row of a Gherkin data table, e.g. the
    // tables under "When I enter the patient's demographic information:".
    public static void fillFields(WebDriver driver, List<Map<String, String>> rows) {
        for (Map<String, String> row : rows) {
            fillField(driver, row.get("Field"), row.get("Value"));
        }
    }

    public static String getText(WebDriver driver, String label) {
        return waitForTestId(driver, label).getText();
    }
}
