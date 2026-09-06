"""Logs in as the identity named in a feature file's Background step, e.g.
    Given I am logged in as a registration clerk
    Given I am logged in as "Dr. Smith"
    Given I am logged in as "Dr. Smith" on the mobile app

`identity` is the exact text that follows "logged in as" (quotes
stripped). `mobile=True` mirrors an "on the mobile app" suffix.
"""

from playwright.sync_api import Page
from playwright.sync_api import TimeoutError as PlaywrightTimeoutError

from .config import BASE_URL


def login(page: Page, identity: str, mobile: bool = False) -> None:
    url = f"{BASE_URL}/login?viewport=mobile" if mobile else f"{BASE_URL}/login"
    page.goto(url)

    # Two attempts: SvelteKit server-renders the login form before its client
    # JS finishes hydrating, so a fill()+click() right after the form
    # becomes visible can in principle land before hydration attaches the
    # submit handler, falling back to a plain native form submission (the
    # inputs have no `name`, so it just reloads the still-unhydrated login
    # page with an empty query string) instead of the SPA login. If the
    # first attempt doesn't reach the authenticated shell quickly, retry
    # once now that the page has settled.
    for attempt in (1, 2):
        page.get_by_test_id("login-identity").fill(identity)
        page.get_by_test_id("login-submit").click()

        try:
            # "app-root" is the root layout wrapper -- it's present on the
            # login page itself too, so waiting for it wouldn't confirm
            # login succeeded. Wait for the sidebar nav instead, which only
            # renders once authenticated.
            page.get_by_test_id("feature-nav").wait_for(timeout=4000 if attempt == 1 else 10000)
            return
        except PlaywrightTimeoutError:
            if attempt == 2:
                raise


def verify_system_is_operational(page: Page) -> None:
    """Confirms the Background precondition `Given the emergency care system
    is operational` by loading the app and waiting for its shell to render.
    """
    page.goto(BASE_URL)
    page.get_by_test_id("app-root").wait_for()
