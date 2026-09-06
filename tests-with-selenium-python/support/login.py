"""Logs in as the identity named in a feature file's Background step, e.g.
    Given I am logged in as a registration clerk
    Given I am logged in as "Dr. Smith"
    Given I am logged in as "Dr. Smith" on the mobile app

`identity` is the exact text that follows "logged in as" (quotes
stripped). `mobile=True` mirrors an "on the mobile app" suffix.
"""

from selenium.common.exceptions import TimeoutException
from selenium.webdriver.common.by import By
from selenium.webdriver.remote.webdriver import WebDriver
from selenium.webdriver.support import expected_conditions as EC
from selenium.webdriver.support.ui import WebDriverWait

from .config import BASE_URL


def login(driver: WebDriver, identity: str, mobile: bool = False) -> None:
    url = f"{BASE_URL}/login?viewport=mobile" if mobile else f"{BASE_URL}/login"
    driver.get(url)

    # Two attempts: SvelteKit server-renders the login form before its client
    # JS finishes hydrating, so a very fast send_keys()+click() right after
    # the form becomes locatable can land before hydration attaches the
    # submit handler. When that happens the browser falls back to a plain
    # native form submission (the inputs have no `name`, so it just reloads
    # the still-unhydrated login page with an empty query string) instead of
    # the SPA login. If the first attempt doesn't reach the authenticated
    # shell within a few seconds, retry once now that the page has settled.
    for attempt in (1, 2):
        identity_field = WebDriverWait(driver, 10).until(
            EC.presence_of_element_located((By.CSS_SELECTOR, '[data-testid="login-identity"]'))
        )
        identity_field.clear()
        identity_field.send_keys(identity)
        driver.find_element(By.CSS_SELECTOR, '[data-testid="login-submit"]').click()

        try:
            # "app-root" is the root layout wrapper -- it's present on the
            # login page itself too, so waiting for it wouldn't confirm login
            # succeeded. Wait for the sidebar nav instead, which only
            # renders once authenticated.
            WebDriverWait(driver, 4 if attempt == 1 else 10).until(
                EC.presence_of_element_located((By.CSS_SELECTOR, '[data-testid="feature-nav"]'))
            )
            return
        except TimeoutException:
            if attempt == 2:
                raise


def verify_system_is_operational(driver: WebDriver) -> None:
    """Confirms the Background precondition `Given the emergency care system
    is operational` by loading the app and waiting for its shell to render.
    """
    driver.get(BASE_URL)
    WebDriverWait(driver, 10).until(
        EC.presence_of_element_located((By.CSS_SELECTOR, '[data-testid="app-root"]'))
    )
