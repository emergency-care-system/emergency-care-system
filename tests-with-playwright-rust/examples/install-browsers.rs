// Installs the Chromium build that matches the Playwright driver bundled
// with the playwright-rs crate:
//   cargo run --example install-browsers

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    playwright_rs::install_browsers(Some(&["chromium"])).await?;
    Ok(())
}
