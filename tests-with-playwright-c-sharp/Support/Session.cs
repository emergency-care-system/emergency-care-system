// One dedicated Chromium browser and page per test class. All scenarios in
// a class share one page (like the Selenium suites share one WebDriver per
// file), since later scenarios rely on data earlier scenarios stored in
// the app's localStorage -- so NUnit must run them in order, not in
// parallel (see [Order] and [NonParallelizable] on each fixture).
//
// Chromium is headless by default; set HEADLESS=0 to watch it run.

namespace EmergencyCareSystem.PlaywrightTests.Support;

public sealed class Session : IAsyncDisposable
{
    private readonly IPlaywright _playwright;
    private readonly IBrowser _browser;

    public IPage Page { get; }

    private Session(IPlaywright playwright, IBrowser browser, IPage page)
    {
        _playwright = playwright;
        _browser = browser;
        Page = page;
    }

    public static async Task<Session> StartAsync()
    {
        var playwright = await Playwright.CreateAsync();
        var headless = Environment.GetEnvironmentVariable("HEADLESS") is not ("0" or "false");
        var browser = await playwright.Chromium.LaunchAsync(new() { Headless = headless });
        var page = await browser.NewPageAsync();
        return new Session(playwright, browser, page);
    }

    public async ValueTask DisposeAsync()
    {
        await _browser.CloseAsync();
        _playwright.Dispose();
    }
}
