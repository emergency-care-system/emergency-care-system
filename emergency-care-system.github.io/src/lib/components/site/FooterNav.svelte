<script lang="ts">
	import { REPO_URL } from '#lib/site.js';
</script>

<footer class="site-footer">
	<div class="site-shell" style="padding-top: 1.5rem; padding-bottom: 2rem;">
		<p>
			A public, free, open-source demonstration built with synthetic data for educational
			purposes — not a real emergency care system. See <a href={REPO_URL}>the source repository</a>
			for the SvelteKit demo app, its specs, and its twelve parallel test suites.
		</p>
		<p>
			<a href="/features/">Features</a> · <a href="/testing/">Testing</a> ·
			<a href="/about/">About</a> · <a href={REPO_URL}>GitHub</a>
		</p>
	</div>
</footer>
