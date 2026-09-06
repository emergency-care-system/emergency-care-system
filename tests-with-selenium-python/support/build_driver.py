"""Builds a Chrome WebDriver instance. Selenium Manager (bundled with modern
selenium) auto-detects the installed browser and downloads a matching
driver, so no manual chromedriver setup is needed.
"""

from selenium import webdriver


def build_driver() -> webdriver.Chrome:
    return webdriver.Chrome()
