// Logs in as the identity named in a feature file's Background step, e.g.
//   Given I am logged in as a registration clerk
//   Given I am logged in as "Dr. Smith"
//   Given I am logged in as "Dr. Smith" on the mobile app
//
// `identity` is the exact text that follows "logged in as" (quotes
// stripped). `mobile` mirrors an "on the mobile app" suffix.

use playwright_rs::protocol::WaitForOptions;
use playwright_rs::{Page, Result};

use super::config::base_url;

pub async fn login(page: &Page, identity: &str, mobile: bool) -> Result<()> {
    let url = if mobile {
        format!("{}/login?viewport=mobile", base_url())
    } else {
        format!("{}/login", base_url())
    };
    page.goto(&url, None).await?;

    // Two attempts: SvelteKit server-renders the login form before its client
    // JS finishes hydrating, so a fill()+click() right after the form becomes
    // visible can in principle land before hydration attaches the submit
    // handler, falling back to a plain native form submission (the inputs have
    // no `name`, so it just reloads the still-unhydrated login page with an
    // empty query string) instead of the SPA login. If the first attempt
    // doesn't reach the authenticated shell quickly, retry once now that the
    // page has settled.
    for attempt in 1..=2 {
        page.get_by_test_id("login-identity").fill(identity, None).await?;
        page.get_by_test_id("login-submit").click(None).await?;

        // "app-root" is the root layout wrapper -- it's present on the login
        // page itself too, so waiting for it wouldn't confirm login succeeded.
        // Wait for the sidebar nav instead, which only renders once
        // authenticated.
        let timeout_ms = if attempt == 1 { 4000.0 } else { 10000.0 };
        let options = WaitForOptions::builder().timeout(timeout_ms).build();
        match page.get_by_test_id("feature-nav").wait_for(options).await {
            Ok(()) => return Ok(()),
            Err(err) if attempt == 2 => return Err(err),
            Err(_) => {}
        }
    }
    Ok(())
}

/// Confirms the Background precondition `Given the emergency care system is
/// operational` by loading the app and waiting for its shell to render.
pub async fn verify_system_is_operational(page: &Page) -> Result<()> {
    page.goto(&base_url(), None).await?;
    page.get_by_test_id("app-root").wait_for(None).await
}
