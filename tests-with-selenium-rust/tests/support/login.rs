// Logs in as the identity named in a feature file's Background step, e.g.
//   Given I am logged in as a registration clerk
//   Given I am logged in as "Dr. Smith"
//   Given I am logged in as "Dr. Smith" on the mobile app
//
// `identity` is the exact text that follows "logged in as" (quotes
// stripped). `mobile` mirrors an "on the mobile app" suffix.

use thirtyfour::prelude::*;

use super::config::base_url;
use super::fields::wait_located;

pub async fn login(driver: &WebDriver, identity: &str, mobile: bool) -> WebDriverResult<()> {
    let url = if mobile {
        format!("{}/login?viewport=mobile", base_url())
    } else {
        format!("{}/login", base_url())
    };
    driver.goto(url).await?;

    // Two attempts: SvelteKit server-renders the login form before its client
    // JS finishes hydrating, so a very fast send_keys()+click() right after the
    // form becomes locatable can land before hydration attaches the submit
    // handler. When that happens the browser falls back to a plain native
    // form submission (the inputs have no `name`, so it just reloads the
    // still-unhydrated login page with an empty query string) instead of the
    // SPA login. If the first attempt doesn't reach the authenticated shell
    // within a few seconds, retry once now that the page has settled.
    for attempt in 1..=2 {
        let identity_field = wait_located(driver, By::Css("[data-testid=\"login-identity\"]"), 10000).await?;
        identity_field.clear().await?;
        identity_field.send_keys(identity).await?;
        driver.find(By::Css("[data-testid=\"login-submit\"]")).await?.click().await?;

        // "app-root" is the root layout wrapper -- it's present on the login
        // page itself too, so waiting for it wouldn't confirm login succeeded.
        // Wait for the sidebar nav instead, which only renders once
        // authenticated.
        let timeout_ms = if attempt == 1 { 4000 } else { 10000 };
        match wait_located(driver, By::Css("[data-testid=\"feature-nav\"]"), timeout_ms).await {
            Ok(_) => return Ok(()),
            Err(err) if attempt == 2 => return Err(err),
            Err(_) => {}
        }
    }
    Ok(())
}

/// Confirms the Background precondition `Given the emergency care system is
/// operational` by loading the app and waiting for its shell to render.
pub async fn verify_system_is_operational(driver: &WebDriver) -> WebDriverResult<()> {
    driver.goto(base_url()).await?;
    wait_located(driver, By::Css("[data-testid=\"app-root\"]"), 10000).await?;
    Ok(())
}
