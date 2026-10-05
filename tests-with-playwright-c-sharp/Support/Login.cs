// Logs in as the identity named in a feature file's Background step, e.g.
//   Given I am logged in as a registration clerk
//   Given I am logged in as "Dr. Smith"
//   Given I am logged in as "Dr. Smith" on the mobile app
//
// `identity` is the exact text that follows "logged in as" (quotes
// stripped). `mobile: true` mirrors an "on the mobile app" suffix.

using static EmergencyCareSystem.PlaywrightTests.Support.Config;

namespace EmergencyCareSystem.PlaywrightTests.Support;

public static class LoginSupport
{
    public static async Task Login(IPage page, string identity, bool mobile = false)
    {
        var url = mobile ? $"{BaseUrl}/login?viewport=mobile" : $"{BaseUrl}/login";
        await page.GotoAsync(url);

        // Two attempts: SvelteKit server-renders the login form before its client
        // JS finishes hydrating, so a FillAsync()+ClickAsync() right after the form
        // becomes visible can in principle land before hydration attaches the
        // submit handler, falling back to a plain native form submission (the
        // inputs have no `name`, so it just reloads the still-unhydrated login
        // page with an empty query string) instead of the SPA login. If the first
        // attempt doesn't reach the authenticated shell quickly, retry once now
        // that the page has settled.
        for (var attempt = 1; attempt <= 2; attempt++)
        {
            await page.GetByTestId("login-identity").FillAsync(identity);
            await page.GetByTestId("login-submit").ClickAsync();

            try
            {
                // "app-root" is the root layout wrapper -- it's present on the
                // login page itself too, so waiting for it wouldn't confirm login
                // succeeded. Wait for the sidebar nav instead, which only renders
                // once authenticated.
                await page.GetByTestId("feature-nav").WaitForAsync(new() { Timeout = attempt == 1 ? 4000 : 10000 });
                return;
            }
            catch (TimeoutException)
            {
                if (attempt == 2) throw;
            }
        }
    }

    // Confirms the Background precondition `Given the emergency care system is
    // operational` by loading the app and waiting for its shell to render.
    public static async Task VerifySystemIsOperational(IPage page)
    {
        await page.GotoAsync(BaseUrl);
        await page.GetByTestId("app-root").WaitForAsync();
    }
}
