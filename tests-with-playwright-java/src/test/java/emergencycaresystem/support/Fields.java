// Helpers for interacting with form fields identified by data-testid
// attributes, and for translating Gherkin data-table field labels (e.g.
// "Given Name", "Date of Birth") into the kebab-case testid the app is
// assumed to expose (e.g. "given-name", "date-of-birth").
//
// Playwright's getByTestId locates elements by the `data-testid` attribute
// by default, which is exactly the app's convention, so most of this is a
// thin, label-based wrapper around it.

package emergencycaresystem.support;

import com.microsoft.playwright.Locator;
import com.microsoft.playwright.Page;
import java.util.List;
import java.util.Map;

public final class Fields {
    private Fields() {}

    public static String kebabCase(String label) {
        return label.trim().toLowerCase().replaceAll("[^a-z0-9]+", "-").replaceAll("(^-|-$)", "");
    }

    // A Locator for the element matching a Gherkin label, e.g.
    // locator(page, "Medical Record Number") ->
    // page.getByTestId("medical-record-number")
    public static Locator locator(Page page, String label) {
        return page.getByTestId(kebabCase(label));
    }

    // Like Selenium's findElement, the helpers below act on the first match:
    // Playwright locators are strict and throw when a test id matches more
    // than one element (e.g. one "placeholder-id" per queued patient).

    // Waits until the element exists in the DOM. Playwright's locator actions
    // (fill, click, ...) already auto-wait for the element to exist and be
    // actionable, so this is mainly for the cases the Selenium suite used an
    // explicit wait before reading text or asserting visibility on something
    // that renders after a click, submit, or navigation.
    public static Locator waitLocated(Page page, String testId, int timeoutMs) {
        Locator target = page.getByTestId(testId).first();
        target.waitFor(new Locator.WaitForOptions().setTimeout(timeoutMs));
        return target;
    }

    public static Locator waitForTestId(Page page, String label) {
        return waitLocated(page, kebabCase(label), 10000);
    }

    // Fills a text-like input identified by its Gherkin field label.
    public static void fillField(Page page, String label, String value) {
        locator(page, label).first().fill(value);
    }

    // Fills every "Field" / "Value" row of a Gherkin data table, e.g. the
    // tables under "When I enter the patient's demographic information:".
    public static void fillFields(Page page, List<Map<String, String>> rows) {
        for (Map<String, String> row : rows) {
            fillField(page, row.get("Field"), row.get("Value"));
        }
    }

    // The visible text of a single element, trimmed (like Selenium's getText).
    public static String textOf(Locator element) {
        return element.innerText().trim();
    }

    public static List<String> textsOf(Locator elements) {
        return elements.allInnerTexts().stream().map(String::trim).toList();
    }

    public static String getText(Page page, String label) {
        return textOf(locator(page, label).first());
    }
}
