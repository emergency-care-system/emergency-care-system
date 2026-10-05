<script lang="ts">
	import Seo from '#lib/components/site/Seo.svelte';
	import { REPO_URL } from '#lib/site.js';

	const suites = [
		{
			dir: 'tests-with-selenium-javascript',
			label: 'Selenium · JavaScript',
			note: 'The original suite — every other suite is a mechanical port of this one.'
		},
		{
			dir: 'tests-with-playwright-javascript',
			label: 'Playwright · JavaScript',
			note: 'Same scenarios, using page.getByTestId(...) instead of a Selenium driver.'
		},
		{
			dir: 'tests-with-selenium-typescript',
			label: 'Selenium · TypeScript',
			note: 'Same structure, with real types (WebDriver, FieldRow[], ...) throughout. Runs under Mocha via tsx.'
		},
		{
			dir: 'tests-with-playwright-typescript',
			label: 'Playwright · TypeScript',
			note: 'Real types (Page, Locator, Browser, ...). Playwright runs .ts files natively.'
		},
		{
			dir: 'tests-with-selenium-python',
			label: 'Selenium · Python',
			note: 'pytest, one Test* class per feature, one test_* method per scenario. Selenium Manager handles the Chrome driver.'
		},
		{
			dir: 'tests-with-playwright-python',
			label: 'Playwright · Python',
			note: "pytest with Playwright's synchronous API and page.get_by_test_id(...)."
		},
		{
			dir: 'tests-with-selenium-c-sharp',
			label: 'Selenium · C#',
			note: 'NUnit through dotnet test, one fixture per feature with its own browser.'
		},
		{
			dir: 'tests-with-playwright-c-sharp',
			label: 'Playwright · C#',
			note: 'NUnit with Microsoft.Playwright and its async API.'
		},
		{
			dir: 'tests-with-selenium-java',
			label: 'Selenium · Java',
			note: 'JUnit 5 through Maven, one class per feature.'
		},
		{
			dir: 'tests-with-playwright-java',
			label: 'Playwright · Java',
			note: 'JUnit 5 and Maven, using the Playwright for Java library.'
		},
		{
			dir: 'tests-with-selenium-rust',
			label: 'Selenium · Rust',
			note: 'The thirtyfour crate; each feature file is its own cargo test binary run by libtest-mimic.'
		},
		{
			dir: 'tests-with-playwright-rust',
			label: 'Playwright · Rust',
			note: 'The community playwright-rs crate (Playwright has no official Rust binding).'
		}
	];
</script>

<Seo
	title="Testing — Emergency Care System"
	description="How the same 171 Gherkin scenarios are ported one-to-one across twelve Selenium/Playwright, JavaScript/TypeScript/Python/C#/Java/Rust test suites."
	path="/testing/"
/>

<section class="page-hero">
	<h1>Twelve test suites, one contract</h1>
	<p>
		Every one of the 171 scenarios across all 22 features is implemented in all twelve suites below,
		driving the same running app through its <code>data-testid</code> attributes. Each test file
		launches its own dedicated browser for its scenarios — a shared browser handling many files'
		logins back-to-back proved unreliable in practice, so every suite runs one worker at a time
		with a fresh browser per file instead.
	</p>
</section>

<div class="card-grid">
	{#each suites as suite (suite.dir)}
		<div class="card">
			<h2>{suite.label}</h2>
			<p>{suite.note}</p>
			<p><a href="{REPO_URL}/tree/main/{suite.dir}">{suite.dir}/</a></p>
		</div>
	{/each}
</div>

<h2>Running them</h2>
<pre><code
		>pnpm install
pnpm run dev                        # app at http://localhost:5173

pnpm run test:selenium              # Mocha (JS)
pnpm run test:selenium-typescript   # Mocha (TS, via tsx)
pnpm run test:playwright            # Playwright (JS)
pnpm run test:playwright-typescript # Playwright (TS)

pip install -r requirements.txt     # once, for the two Python suites
playwright install chromium
pnpm run test:selenium-python       # pytest + Selenium
pnpm run test:playwright-python     # pytest + Playwright

pnpm run test:selenium-c-sharp      # NUnit + Selenium (needs the .NET SDK)
pnpm run test:playwright-c-sharp    # NUnit + Playwright
pnpm run test:selenium-java         # JUnit 5 + Selenium (needs a JDK and Maven)
pnpm run test:playwright-java       # JUnit 5 + Playwright
pnpm run test:selenium-rust         # thirtyfour (needs cargo)
pnpm run test:playwright-rust       # playwright-rs</code
	></pre>

<p>
	See the source repository's <a href="{REPO_URL}#readme">README</a> for full setup and known
	limitations.
</p>
