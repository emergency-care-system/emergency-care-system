// Builds a Chrome WebDriver instance.
//
// Unlike the JavaScript/Python/Java/C# Selenium bindings, thirtyfour does
// not bundle Selenium Manager, so this module starts a chromedriver server
// itself. It looks for a chromedriver binary, in order:
//   1. the CHROMEDRIVER environment variable (path to the binary);
//   2. Selenium Manager, if SELENIUM_MANAGER points at its binary (it
//      downloads a chromedriver matching the installed Chrome -- the
//      JavaScript suite ships one at
//      node_modules/selenium-webdriver/bin/<platform>/selenium-manager);
//   3. `chromedriver` on the PATH.
//
// Chrome opens a visible window by default (like the other Selenium
// suites); set HEADLESS=1 to run without one.

use std::net::TcpListener;
use std::process::{Child, Command, Stdio};
use std::time::Duration;

use thirtyfour::prelude::*;

/// A running chromedriver server; killed when dropped.
pub struct DriverServer {
    child: Child,
    pub url: String,
}

impl Drop for DriverServer {
    fn drop(&mut self) {
        let _ = self.child.kill();
        let _ = self.child.wait();
    }
}

fn chromedriver_path() -> String {
    if let Ok(path) = std::env::var("CHROMEDRIVER") {
        return path;
    }
    if let Ok(manager) = std::env::var("SELENIUM_MANAGER") {
        let output = Command::new(manager)
            .args(["--browser", "chrome", "--output", "json"])
            .output()
            .expect("failed to run selenium-manager");
        let json: serde_json::Value =
            serde_json::from_slice(&output.stdout).expect("selenium-manager printed invalid JSON");
        if let Some(path) = json["result"]["driver_path"].as_str() {
            return path.to_string();
        }
    }
    "chromedriver".to_string()
}

pub fn start_server() -> DriverServer {
    let port = TcpListener::bind("127.0.0.1:0")
        .expect("no free port")
        .local_addr()
        .unwrap()
        .port();
    let child = Command::new(chromedriver_path())
        .arg(format!("--port={port}"))
        .stdout(Stdio::null())
        .stderr(Stdio::null())
        .spawn()
        .expect("could not start chromedriver (see support/driver.rs for how it is located)");
    DriverServer { child, url: format!("http://127.0.0.1:{port}") }
}

pub async fn build_driver(server: &DriverServer) -> WebDriverResult<WebDriver> {
    let mut caps = DesiredCapabilities::chrome();
    if matches!(std::env::var("HEADLESS").as_deref(), Ok("1") | Ok("true")) {
        caps.add_arg("--headless=new")?;
    }
    // chromedriver needs a moment to start listening.
    let mut last_error = None;
    for _ in 0..50 {
        match WebDriver::new(&server.url, caps.clone()).await {
            Ok(driver) => return Ok(driver),
            Err(err) => {
                last_error = Some(err);
                tokio::time::sleep(Duration::from_millis(200)).await;
            }
        }
    }
    Err(last_error.unwrap())
}
