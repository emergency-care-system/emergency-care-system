// Logs in as the identity named in a feature file's Background step, e.g.
//   Given I am logged in as a registration clerk
//   Given I am logged in as "Dr. Smith"
//   Given I am logged in as "Dr. Smith" on the mobile app
//
// `identity` is the exact text that follows "logged in as" (quotes
// stripped). `mobile` mirrors an "on the mobile app" suffix.

package emergencycaresystem.support;

import static emergencycaresystem.support.Config.BASE_URL;

import org.openqa.selenium.By;
import org.openqa.selenium.TimeoutException;
import org.openqa.selenium.WebDriver;
import org.openqa.selenium.WebElement;

public final class Login {
    private Login() {}

    public static void login(WebDriver driver, String identity) {
        login(driver, identity, false);
    }

    public static void login(WebDriver driver, String identity, boolean mobile) {
        String url = mobile ? BASE_URL + "/login?viewport=mobile" : BASE_URL + "/login";
        driver.get(url);

        // Two attempts: SvelteKit server-renders the login form before its client
        // JS finishes hydrating, so a very fast sendKeys()+click() right after the
        // form becomes locatable can land before hydration attaches the submit
        // handler. When that happens the browser falls back to a plain native
        // form submission (the inputs have no `name`, so it just reloads the
        // still-unhydrated login page with an empty query string) instead of the
        // SPA login. If the first attempt doesn't reach the authenticated shell
        // within a few seconds, retry once now that the page has settled.
        for (int attempt = 1; attempt <= 2; attempt++) {
            WebElement identityField = Fields.waitLocated(
                    driver, By.cssSelector("[data-testid=\"login-identity\"]"), 10000);
            identityField.clear();
            identityField.sendKeys(identity);
            driver.findElement(By.cssSelector("[data-testid=\"login-submit\"]")).click();

            try {
                // "app-root" is the root layout wrapper -- it's present on the
                // login page itself too, so waiting for it wouldn't confirm login
                // succeeded. Wait for the sidebar nav instead, which only renders
                // once authenticated.
                Fields.waitLocated(
                        driver,
                        By.cssSelector("[data-testid=\"feature-nav\"]"),
                        attempt == 1 ? 4000 : 10000);
                return;
            } catch (TimeoutException err) {
                if (attempt == 2) throw err;
            }
        }
    }

    // Confirms the Background precondition `Given the emergency care system is
    // operational` by loading the app and waiting for its shell to render.
    public static void verifySystemIsOperational(WebDriver driver) {
        driver.get(BASE_URL);
        Fields.waitLocated(driver, By.cssSelector("[data-testid=\"app-root\"]"), 10000);
    }
}
