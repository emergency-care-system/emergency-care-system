"""Shared configuration for the Selenium Python test suite.

Override BASE_URL to point at a running instance of the app, e.g.:
    BASE_URL=http://localhost:5173 pytest tests-with-selenium-python
"""

import os

BASE_URL: str = os.environ.get("BASE_URL", "http://localhost:5173")
