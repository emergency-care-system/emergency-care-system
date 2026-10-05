// One dedicated Chromium browser and page per test class. All scenarios in
// a class share one page (like the Selenium suites share one WebDriver per
// file), since later scenarios rely on data earlier scenarios stored in
// the app's localStorage -- so JUnit must run them in order, not in
// parallel (see @TestMethodOrder on each class; Surefire runs classes
// serially by default).
//
// Chromium is headless by default; set HEADLESS=0 to watch it run.

package emergencycaresystem.support;

import com.microsoft.playwright.Browser;
import com.microsoft.playwright.BrowserType;
import com.microsoft.playwright.Page;
import com.microsoft.playwright.Playwright;

public final class Session implements AutoCloseable {
    private final Playwright playwright;
    private final Browser browser;
    private final Page page;

    private Session(Playwright playwright, Browser browser, Page page) {
        this.playwright = playwright;
        this.browser = browser;
        this.page = page;
    }

    public static Session start() {
        Playwright playwright = Playwright.create();
        String headlessEnv = System.getenv("HEADLESS");
        boolean headless = !("0".equals(headlessEnv) || "false".equals(headlessEnv));
        Browser browser = playwright.chromium().launch(new BrowserType.LaunchOptions().setHeadless(headless));
        return new Session(playwright, browser, browser.newPage());
    }

    public Page page() {
        return page;
    }

    @Override
    public void close() {
        browser.close();
        playwright.close();
    }
}
