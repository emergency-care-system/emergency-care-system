// Helpers for interacting with form fields identified by data-testid
// attributes, and for translating Gherkin data-table field labels (e.g.
// "Given Name", "Date of Birth") into the kebab-case testid the app is
// assumed to expose (e.g. "given-name", "date-of-birth").
//
// Playwright's GetByTestId locates elements by the `data-testid` attribute
// by default, which is exactly the app's convention, so most of this is a
// thin, label-based wrapper around it.

using System.Text.RegularExpressions;

namespace EmergencyCareSystem.PlaywrightTests.Support;

public static class Fields
{
    public static string KebabCase(string label) =>
        Regex.Replace(label.Trim().ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');

    // A Locator for the element matching a Gherkin label, e.g.
    // Locator(page, "Medical Record Number") ->
    // page.GetByTestId("medical-record-number")
    public static ILocator Locator(IPage page, string label) => page.GetByTestId(KebabCase(label));

    // Like Selenium's FindElement, the helpers below act on the first match:
    // Playwright locators are strict and throw when a test id matches more
    // than one element (e.g. one "placeholder-id" per queued patient).

    // Waits until the element exists in the DOM. Playwright's locator actions
    // (Fill, Click, ...) already auto-wait for the element to exist and be
    // actionable, so this is mainly for the cases the Selenium suite used an
    // explicit wait before reading text or asserting visibility on something
    // that renders after a click, submit, or navigation.
    public static async Task<ILocator> WaitLocated(IPage page, string testId, float timeoutMs = 10000)
    {
        var target = page.GetByTestId(testId).First;
        await target.WaitForAsync(new() { Timeout = timeoutMs });
        return target;
    }

    public static Task<ILocator> WaitForTestId(IPage page, string label, float timeoutMs = 10000) =>
        WaitLocated(page, KebabCase(label), timeoutMs);

    // Fills a text-like input identified by its Gherkin field label.
    public static Task FillField(IPage page, string label, string value) =>
        Locator(page, label).First.FillAsync(value);

    // Fills every "Field" / "Value" row of a Gherkin data table, e.g. the
    // tables under "When I enter the patient's demographic information:".
    public static async Task FillFields(IPage page, IEnumerable<Dictionary<string, string>> rows)
    {
        foreach (var row in rows)
        {
            await FillField(page, row["Field"], row["Value"]);
        }
    }

    // The visible text of a single element, trimmed (like Selenium's getText).
    public static async Task<string> TextOf(ILocator element) =>
        (await element.InnerTextAsync()).Trim();

    public static async Task<IReadOnlyList<string>> TextsOf(ILocator elements) =>
        (await elements.AllInnerTextsAsync()).Select(t => t.Trim()).ToList();

    public static Task<string> GetText(IPage page, string label) => TextOf(Locator(page, label).First);
}
