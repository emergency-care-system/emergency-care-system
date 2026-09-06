"""Helpers for interacting with form fields identified by data-testid
attributes, and for translating Gherkin data-table field labels (e.g.
"Given Name", "Date of Birth") into the kebab-case testid the app is
assumed to expose (e.g. "given-name", "date-of-birth").

Playwright's `get_by_test_id` locates elements by the `data-testid`
attribute by default, which is exactly the app's convention, so most of
this is a thin, label-based wrapper around it.
"""

import re

from playwright.sync_api import Locator, Page


def kebab_case(label: str) -> str:
    slug = re.sub(r"[^a-z0-9]+", "-", label.strip().lower())
    return slug.strip("-")


def locator(page: Page, label: str) -> Locator:
    """A Locator for the element matching a Gherkin label, e.g.
    locator(page, 'Medical Record Number') ->
    page.get_by_test_id('medical-record-number')
    """
    return page.get_by_test_id(kebab_case(label))


def wait_for_test_id(page: Page, label: str, timeout: float = 10000) -> Locator:
    """Playwright's locator actions (fill, click, ...) already auto-wait for
    the element to exist and be actionable, so this is mainly for the cases
    the Selenium suite used an explicit wait before reading text or
    asserting visibility on something that renders after a click, submit,
    or navigation.
    """
    target = locator(page, label)
    target.wait_for(timeout=timeout)
    return target


def fill_field(page: Page, label: str, value: str) -> None:
    """Fills a text-like input identified by its Gherkin field label."""
    locator(page, label).fill(value)


def fill_fields(page: Page, rows: list[dict[str, str]]) -> None:
    """Fills every "Field" / "Value" row of a Gherkin data table, e.g. the
    tables under "When I enter the patient's demographic information:".
    """
    for row in rows:
        fill_field(page, row["Field"], row["Value"])


def get_text(page: Page, label: str) -> str:
    text = locator(page, label).inner_text()
    return text.strip()
