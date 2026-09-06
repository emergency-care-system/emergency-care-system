"""Makes `from support...import ...` resolve regardless of pytest's
import mode (this directory has no __init__.py -- its name has hyphens,
which aren't valid in a Python package name -- so pytest's default
"prepend" import mode already adds this directory to sys.path, but
--import-mode=importlib does not).
"""

import os
import sys

sys.path.insert(0, os.path.dirname(__file__))
