<script lang="ts">
	import { REPO_URL } from '#lib/site.js';
	import SitePickers from './SitePickers.svelte';
</script>

<header class="site-header">
	<nav class="site-nav site-shell" aria-label="Primary">
		<a href="/">🚑 Emergency Care System</a>
		<a href="/features/">Features</a>
		<a href="/testing/">Testing</a>
		<a href="/about/">About</a>
		<a href={REPO_URL}>Source</a>
		<div class="site-nav-controls">
			<SitePickers />
		</div>
	</nav>
</header>
