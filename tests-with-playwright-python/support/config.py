"""Shared configuration for the Playwright Python test suite.

Override BASE_URL to point at a running instance of the app, e.g.:
    BASE_URL=http://localhost:5173 pytest tests-with-playwright-python

Unlike the JavaScript Playwright suite, this one drives the
`playwright.sync_api` directly rather than the `playwright test` CLI
runner, so there is no `playwright.config` and no automatic dev-server
startup -- start `pnpm run dev` yourself first.
"""

import os

BASE_URL: str = os.environ.get("BASE_URL", "http://localhost:5173")
