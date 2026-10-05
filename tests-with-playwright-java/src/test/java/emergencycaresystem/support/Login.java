// Logs in as the identity named in a feature file's Background step, e.g.
//   Given I am logged in as a registration clerk
//   Given I am logged in as "Dr. Smith"
//   Given I am logged in as "Dr. Smith" on the mobile app
//
// `identity` is the exact text that follows "logged in as" (quotes
// stripped). `mobile` mirrors an "on the mobile app" suffix.

package emergencycaresystem.support;

import static emergencycaresystem.support.Config.BASE_URL;

import com.microsoft.playwright.Locator;
import com.microsoft.playwright.Page;
import com.microsoft.playwright.TimeoutError;

public final class Login {
    private Login() {}

    public static void login(Page page, String identity) {
        login(page, identity, false);
    }

    public static void login(Page page, String identity, boolean mobile) {
        String url = mobile ? BASE_URL + "/login?viewport=mobile" : BASE_URL + "/login";
        page.navigate(url);

        // Two attempts: SvelteKit server-renders the login form before its client
        // JS finishes hydrating, so a fill()+click() right after the form becomes
        // visible can in principle land before hydration attaches the submit
        // handler, falling back to a plain native form submission (the inputs
        // have no `name`, so it just reloads the still-unhydrated login page with
        // an empty query string) instead of the SPA login. If the first attempt
        // doesn't reach the authenticated shell quickly, retry once now that the
        // page has settled.
        for (int attempt = 1; attempt <= 2; attempt++) {
            page.getByTestId("login-identity").fill(identity);
            page.getByTestId("login-submit").click();

            try {
                // "app-root" is the root layout wrapper -- it's present on the
                // login page itself too, so waiting for it wouldn't confirm login
                // succeeded. Wait for the sidebar nav instead, which only renders
                // once authenticated.
                page.getByTestId("feature-nav")
                        .waitFor(new Locator.WaitForOptions().setTimeout(attempt == 1 ? 4000 : 10000));
                return;
            } catch (TimeoutError err) {
                if (attempt == 2) throw err;
            }
        }
    }

    // Confirms the Background precondition `Given the emergency care system is
    // operational` by loading the app and waiting for its shell to render.
    public static void verifySystemIsOperational(Page page) {
        page.navigate(BASE_URL);
        page.getByTestId("app-root").waitFor();
    }
}
