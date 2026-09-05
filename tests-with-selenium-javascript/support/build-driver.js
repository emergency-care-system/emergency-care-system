// Builds a Chrome WebDriver instance. Selenium Manager (bundled with
// modern selenium-webdriver) auto-detects the installed browser and
// downloads a matching driver, so no manual chromedriver setup is needed.

import { Browser, Builder } from 'selenium-webdriver';

export async function buildDriver() {
  return new Builder().forBrowser(Browser.CHROME).build();
}
