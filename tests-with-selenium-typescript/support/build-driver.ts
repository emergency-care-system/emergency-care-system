// Builds a Chrome WebDriver instance. Selenium Manager (bundled with
// modern selenium-webdriver) auto-detects the installed browser and
// downloads a matching driver, so no manual chromedriver setup is needed.

import { Browser, Builder, type WebDriver } from 'selenium-webdriver';

export async function buildDriver(): Promise<WebDriver> {
  return new Builder().forBrowser(Browser.CHROME).build();
}
