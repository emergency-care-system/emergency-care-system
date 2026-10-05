// Helpers for interacting with form fields identified by data-testid
// attributes, and for translating Gherkin data-table field labels (e.g.
// "Given Name", "Date of Birth") into the kebab-case testid the app is
// assumed to expose (e.g. "given-name", "date-of-birth").

using System.Text.RegularExpressions;
using OpenQA.Selenium.Support.UI;

namespace EmergencyCareSystem.SeleniumTests.Support;

public static class Fields
{
    public static string KebabCase(string label) =>
        Regex.Replace(label.Trim().ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');

    public static string TestId(string label) => $"[data-testid=\"{KebabCase(label)}\"]";

    // A By locator for the element matching a Gherkin label, e.g.
    // Locator("Medical Record Number") ->
    // By.CssSelector("[data-testid=\"medical-record-number\"]")
    public static By Locator(string label) => By.CssSelector(TestId(label));

    // Waits until an element matching the locator is present in the DOM.
    public static IWebElement WaitLocated(IWebDriver driver, By by, int timeoutMs = 10000) =>
        new WebDriverWait(driver, TimeSpan.FromMilliseconds(timeoutMs)).Until(d => d.FindElement(by));

    public static IWebElement WaitForTestId(IWebDriver driver, string label, int timeoutMs = 10000) =>
        WaitLocated(driver, Locator(label), timeoutMs);

    // Fills a text-like input identified by its Gherkin field label.
    public static void FillField(IWebDriver driver, string label, string value)
    {
        var element = WaitForTestId(driver, label);
        element.Clear();
        element.SendKeys(value);
    }

    // Fills every "Field" / "Value" row of a Gherkin data table, e.g. the
    // tables under "When I enter the patient's demographic information:".
    public static void FillFields(IWebDriver driver, IEnumerable<Dictionary<string, string>> rows)
    {
        foreach (var row in rows)
        {
            FillField(driver, row["Field"], row["Value"]);
        }
    }

    public static string GetText(IWebDriver driver, string label) =>
        WaitForTestId(driver, label).Text;
}
