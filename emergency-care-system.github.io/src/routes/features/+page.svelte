<script lang="ts">
	import Seo from '#lib/components/site/Seo.svelte';
	import { FEATURES } from '#lib/data/features.js';
	import { REPO_URL } from '#lib/site.js';
</script>

<Seo
	title="Features — Emergency Care System"
	description="All 22 Gherkin features behind the Emergency Care System demo, from walk-in registration through audit trails."
	path="/features/"
/>

<section class="page-hero">
	<h1>22 features</h1>
	<p>
		Each feature below started as a Gherkin <code>.feature</code> file — a Background plus several
		scenarios — in <code>tests-with-given-when-then-features/</code>, the source repository's single source of truth.
		Every scenario is implemented once in the demo app and ported, one-to-one, across all twelve test
		suites.
	</p>
</section>

<ol class="link-list">
	{#each FEATURES as feature (feature.slug)}
		<li class="card">
			<h2>{feature.title}</h2>
			<p>
				As {feature.asA}, I want {feature.iWant}, so that {feature.soThat}.
			</p>
			<p>
				<a href="{REPO_URL}/blob/main/tests-with-given-when-then-features/{feature.slug}.feature"
					>View {feature.slug}.feature on GitHub →</a
				>
			</p>
		</li>
	{/each}
</ol>
