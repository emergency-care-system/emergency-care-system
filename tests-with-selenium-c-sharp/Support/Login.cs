// Logs in as the identity named in a feature file's Background step, e.g.
//   Given I am logged in as a registration clerk
//   Given I am logged in as "Dr. Smith"
//   Given I am logged in as "Dr. Smith" on the mobile app
//
// `identity` is the exact text that follows "logged in as" (quotes
// stripped). `mobile: true` mirrors an "on the mobile app" suffix.

using static EmergencyCareSystem.SeleniumTests.Support.Config;

namespace EmergencyCareSystem.SeleniumTests.Support;

public static class LoginSupport
{
    public static void Login(IWebDriver driver, string identity, bool mobile = false)
    {
        var url = mobile ? $"{BaseUrl}/login?viewport=mobile" : $"{BaseUrl}/login";
        driver.Navigate().GoToUrl(url);

        // Two attempts: SvelteKit server-renders the login form before its client
        // JS finishes hydrating, so a very fast SendKeys()+Click() right after the
        // form becomes locatable can land before hydration attaches the submit
        // handler. When that happens the browser falls back to a plain native
        // form submission (the inputs have no `name`, so it just reloads the
        // still-unhydrated login page with an empty query string) instead of the
        // SPA login. If the first attempt doesn't reach the authenticated shell
        // within a few seconds, retry once now that the page has settled.
        for (var attempt = 1; attempt <= 2; attempt++)
        {
            var identityField = Fields.WaitLocated(driver, By.CssSelector("[data-testid=\"login-identity\"]"));
            identityField.Clear();
            identityField.SendKeys(identity);
            driver.FindElement(By.CssSelector("[data-testid=\"login-submit\"]")).Click();

            try
            {
                // "app-root" is the root layout wrapper -- it's present on the
                // login page itself too, so waiting for it wouldn't confirm login
                // succeeded. Wait for the sidebar nav instead, which only renders
                // once authenticated.
                Fields.WaitLocated(
                    driver,
                    By.CssSelector("[data-testid=\"feature-nav\"]"),
                    attempt == 1 ? 4000 : 10000);
                return;
            }
            catch (WebDriverTimeoutException)
            {
                if (attempt == 2) throw;
            }
        }
    }

    // Confirms the Background precondition `Given the emergency care system is
    // operational` by loading the app and waiting for its shell to render.
    public static void VerifySystemIsOperational(IWebDriver driver)
    {
        driver.Navigate().GoToUrl(BaseUrl);
        Fields.WaitLocated(driver, By.CssSelector("[data-testid=\"app-root\"]"));
    }
}
