"""Helpers for interacting with form fields identified by data-testid
attributes, and for translating Gherkin data-table field labels (e.g.
"Given Name", "Date of Birth") into the kebab-case testid the app is
assumed to expose (e.g. "given-name", "date-of-birth").
"""

import re

from selenium.webdriver.common.by import By
from selenium.webdriver.remote.webdriver import WebDriver
from selenium.webdriver.remote.webelement import WebElement
from selenium.webdriver.support import expected_conditions as EC
from selenium.webdriver.support.ui import WebDriverWait


def kebab_case(label: str) -> str:
    slug = re.sub(r"[^a-z0-9]+", "-", label.strip().lower())
    return slug.strip("-")


def test_id(label: str) -> str:
    return f'[data-testid="{kebab_case(label)}"]'


def locator(label: str) -> tuple[str, str]:
    """A (By, value) locator tuple for the element matching a Gherkin label,
    e.g. locator('Medical Record Number') ->
    (By.CSS_SELECTOR, '[data-testid="medical-record-number"]')
    """
    return (By.CSS_SELECTOR, test_id(label))


def wait_for_test_id(driver: WebDriver, label: str, timeout: float = 10) -> WebElement:
    return WebDriverWait(driver, timeout).until(EC.presence_of_element_located(locator(label)))


def fill_field(driver: WebDriver, label: str, value: str) -> None:
    """Fills a text-like input identified by its Gherkin field label."""
    element = wait_for_test_id(driver, label)
    element.clear()
    element.send_keys(value)


def fill_fields(driver: WebDriver, rows: list[dict[str, str]]) -> None:
    """Fills every "Field" / "Value" row of a Gherkin data table, e.g. the
    tables under "When I enter the patient's demographic information:".
    """
    for row in rows:
        fill_field(driver, row["Field"], row["Value"])


def get_text(driver: WebDriver, label: str) -> str:
    element = wait_for_test_id(driver, label)
    return element.text
